using EnglishLearning.Domain.Entities;
namespace EnglishLearning.Web.ViewModels;
public record TopicPage(Topic Topic,List<Exercise> Exercises);
public record DashboardPage(List<UserVocabularyProgress> Progress,List<QuizAttempt> Attempts,List<TopicSummary> Topics);
public record TopicSummary(string Name,int Total,int Learned);
