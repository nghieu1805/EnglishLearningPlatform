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
    List<QuizAttempt> Attempts,
    List<TopicSummary> Topics);

public record TopicSummary(
    string Name,
    int Total,
    int Learned);