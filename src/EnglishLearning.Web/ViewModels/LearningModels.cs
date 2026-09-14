using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Web.ViewModels;

public record TopicPage(
    Topic Topic,
    List<Exercise> Exercises,
    IReadOnlyDictionary<int, LearningStatus> Statuses)
{
    public List<Vocabulary> CoreVocabulary =>
        Topic.Vocabulary
            .Where(vocabulary =>
                vocabulary.Level == VocabularyLevel.Core)
            .OrderBy(vocabulary => vocabulary.Word)
            .ToList();

    public List<Vocabulary> AdvancedVocabulary =>
        Topic.Vocabulary
            .Where(vocabulary =>
                vocabulary.Level == VocabularyLevel.Advanced)
            .OrderBy(vocabulary => vocabulary.Word)
            .ToList();

    public LearningStatus GetStatus(int vocabularyId)
    {
        return Statuses.TryGetValue(
            vocabularyId,
            out var status)
                ? status
                : LearningStatus.New;
    }

    public int LearnedCount(
        IEnumerable<Vocabulary> vocabulary)
    {
        return vocabulary.Count(item =>
            GetStatus(item.Id) == LearningStatus.Learned);
    }
}

public record DashboardPage(
    List<UserVocabularyProgress> Progress,
    List<UserVocabularyProgress> FilteredProgress,
    List<QuizAttempt> Attempts,
    List<TopicSummary> Topics,
    LearningStatus? SelectedStatus)
{
    public int LearningCount =>
        Progress.Count(item =>
            item.Status == LearningStatus.Learning);

    public int LearnedCount =>
        Progress.Count(item =>
            item.Status == LearningStatus.Learned);

    public int NeedsReviewCount =>
        Progress.Count(item =>
            item.Status == LearningStatus.NeedsReview);

    public int CompletedTopicCount =>
        Topics.Count(topic =>
            topic.Total > 0 &&
            topic.Learned == topic.Total);

    public int TotalQuestions =>
        Attempts.Sum(attempt => attempt.Total);

    public int Accuracy =>
        TotalQuestions == 0
            ? 0
            : (int)Math.Round(
                100.0 *
                Attempts.Sum(attempt => attempt.Correct) /
                TotalQuestions);
}

public record TopicSummary(
    int Id,
    string GradeName,
    string Name,
    int Total,
    int CoreTotal,
    int AdvancedTotal,
    int New,
    int Learning,
    int Learned,
    int NeedsReview)
{
    public int CompletionPercentage =>
        Total == 0
            ? 0
            : (int)Math.Round(
                100.0 * Learned / Total);
}