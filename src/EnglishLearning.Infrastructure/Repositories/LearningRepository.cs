using EnglishLearning.Application.Interfaces;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Repositories;

public class LearningRepository(
    ApplicationDbContext db) : ILearningRepository
{
    public Task<List<Grade>> GradesAsync()
    {
        return db.Grades
            .AsNoTracking()
            .OrderBy(grade => grade.Number)
            .ToListAsync();
    }

    public Task<List<Topic>> TopicsAsync(int gradeId)
    {
        return db.Topics
            .AsNoTracking()
            .Include(topic => topic.Sources)
            .Where(topic =>
                topic.GradeId == gradeId &&
                topic.IsPublished)
            .OrderBy(topic => topic.SortOrder)
            .ThenBy(topic => topic.Id)
            .ToListAsync();
    }

    public Task<Topic?> TopicAsync(int id)
    {
        return db.Topics
            .AsNoTracking()
            .Include(topic => topic.Grade)
            .Include(topic => topic.Sources)
            .Include(topic => topic.Vocabulary)
                .ThenInclude(vocabulary =>
                    vocabulary.Examples)
            .Include(topic => topic.Vocabulary)
                .ThenInclude(vocabulary =>
                    vocabulary.Audios)
            .Include(topic => topic.Vocabulary)
                .ThenInclude(vocabulary =>
                    vocabulary.TopicSource)
            .AsSplitQuery()
            .SingleOrDefaultAsync(topic =>
                topic.Id == id &&
                topic.IsPublished);
    }

    public Task<List<Vocabulary>> SearchAsync(
        string term)
    {
        string query = term.Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(
                new List<Vocabulary>());
        }

        return db.Vocabularies
            .AsNoTracking()
            .Include(vocabulary =>
                vocabulary.Topic)
                .ThenInclude(topic =>
                    topic!.Grade)
            .Include(vocabulary =>
                vocabulary.TopicSource)
            .Include(vocabulary =>
                vocabulary.Examples)
            .Include(vocabulary =>
                vocabulary.Audios)
            .AsSplitQuery()
            .Where(vocabulary =>
                (vocabulary.TopicId == null ||
                 vocabulary.Topic!.IsPublished) &&
                vocabulary.Word.Contains(query))
            .OrderBy(vocabulary =>
                vocabulary.Word == query
                    ? 0
                    : vocabulary.Word.StartsWith(query)
                        ? 1
                        : 2)
            .ThenBy(vocabulary =>
                vocabulary.Word)
            .Take(50)
            .ToListAsync();
    }

    public async Task SaveProgressAsync(
        string userId,
        int vocabularyId,
        LearningStatus status)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "Người dùng không hợp lệ.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentException(
                "Trạng thái không hợp lệ.");
        }

        bool vocabularyExists =
            await db.Vocabularies.AnyAsync(
                vocabulary =>
                    vocabulary.Id == vocabularyId &&
                    (vocabulary.TopicId == null ||
                     vocabulary.Topic!.IsPublished));

        if (!vocabularyExists)
        {
            throw new ArgumentException(
                "Không tìm thấy từ.");
        }

        var progress =
            await db.UserVocabularyProgresses
                .SingleOrDefaultAsync(item =>
                    item.UserId == userId &&
                    item.VocabularyId ==
                    vocabularyId);

        if (progress is null)
        {
            progress =
                new UserVocabularyProgress
                {
                    UserId = userId,
                    VocabularyId = vocabularyId
                };

            db.UserVocabularyProgresses.Add(
                progress);
        }

        progress.Status = status;
        progress.UpdatedAtUtc =
            DateTime.UtcNow;

        db.StudyHistories.Add(
            new StudyHistory
            {
                UserId = userId,
                Activity = "Vocabulary",
                OccurredAtUtc = DateTime.UtcNow
            });

        await db.SaveChangesAsync();
    }

    public Task<List<UserVocabularyProgress>>
        ProgressAsync(string userId)
    {
        return db.UserVocabularyProgresses
            .AsNoTracking()
            .Include(progress =>
                progress.Vocabulary)
            .Where(progress =>
                progress.UserId == userId)
            .OrderByDescending(progress =>
                progress.UpdatedAtUtc)
            .ToListAsync();
    }

    public Task<List<QuizAttempt>>
        HistoryAsync(string userId)
    {
        return db.QuizAttempts
            .AsNoTracking()
            .Include(attempt =>
                attempt.Exercise)
            .Where(attempt =>
                attempt.UserId == userId)
            .OrderByDescending(attempt =>
                attempt.SubmittedAtUtc)
            .Take(100)
            .ToListAsync();
    }

    public Task<Exercise?> ExerciseAsync(int id)
    {
        return db.Exercises
            .AsNoTracking()
            .Include(exercise =>
                exercise.Questions)
                .ThenInclude(question =>
                    question.Answers)
            .AsSplitQuery()
            .SingleOrDefaultAsync(exercise =>
                exercise.Id == id &&
                exercise.Topic != null &&
                exercise.Topic.IsPublished);
    }

    public async Task<int> SaveAttemptAsync(
        QuizAttempt attempt)
    {
        if (string.IsNullOrWhiteSpace(
            attempt.UserId))
        {
            throw new ArgumentException(
                "Người dùng không hợp lệ.");
        }

        db.QuizAttempts.Add(attempt);

        db.StudyHistories.Add(
            new StudyHistory
            {
                UserId = attempt.UserId,
                Activity = "Quiz",
                OccurredAtUtc = DateTime.UtcNow
            });

        await db.SaveChangesAsync();

        return attempt.Id;
    }

    public Task<QuizAttempt?> AttemptAsync(
        int id,
        string userId)
    {
        return db.QuizAttempts
            .AsNoTracking()
            .Include(attempt =>
                attempt.Answers)
            .Include(attempt =>
                attempt.Exercise)
            .AsSplitQuery()
            .SingleOrDefaultAsync(attempt =>
                attempt.Id == id &&
                attempt.UserId == userId);
    }
}