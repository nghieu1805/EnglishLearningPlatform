using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Application.Interfaces;
public interface ILearningRepository
{
 Task<List<Grade>> GradesAsync();
 Task<List<Topic>> TopicsAsync(int gradeId);
 Task<Topic?> TopicAsync(int id);
 Task<List<Vocabulary>> SearchAsync(string term);
 Task SaveProgressAsync(string userId,int vocabularyId,LearningStatus status);
 Task<List<UserVocabularyProgress>> ProgressAsync(string userId);
 Task<List<QuizAttempt>> HistoryAsync(string userId);
 Task<Exercise?> ExerciseAsync(int id);
 Task<int> SaveAttemptAsync(QuizAttempt attempt);
 Task<QuizAttempt?> AttemptAsync(int id,string userId);
}
