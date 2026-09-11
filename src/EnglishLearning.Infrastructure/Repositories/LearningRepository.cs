using EnglishLearning.Application.Interfaces;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace EnglishLearning.Infrastructure.Repositories;
public class LearningRepository(ApplicationDbContext db):ILearningRepository
{
 public Task<List<Grade>> GradesAsync()=>db.Grades.AsNoTracking().OrderBy(x=>x.Number).ToListAsync();
 public Task<List<Topic>> TopicsAsync(int gradeId)=>db.Topics.AsNoTracking().Include(x=>x.Sources).Where(x=>x.GradeId==gradeId&&x.IsPublished).OrderBy(x=>x.SortOrder).ThenBy(x=>x.Id).ToListAsync();
 public Task<Topic?> TopicAsync(int id)=>db.Topics.AsNoTracking().Include(x=>x.Grade).Include(x=>x.Sources).Include(x=>x.Vocabulary).ThenInclude(x=>x.Examples).Include(x=>x.Vocabulary).ThenInclude(x=>x.Audios).AsSplitQuery().SingleOrDefaultAsync(x=>x.Id==id&&x.IsPublished);
 public Task<List<Vocabulary>> SearchAsync(string term)=>db.Vocabularies.AsNoTracking().Include(x=>x.Examples).Include(x=>x.Audios).Where(x=>(x.TopicId==null||x.Topic!.IsPublished)&&x.Word.Contains(term)).OrderBy(x=>x.Word).Take(50).ToListAsync();
 public async Task SaveProgressAsync(string userId,int vocabularyId,LearningStatus status)
 {
  if(!Enum.IsDefined(status))throw new ArgumentException("Trạng thái không hợp lệ.");
  if(!await db.Vocabularies.AnyAsync(v=>v.Id==vocabularyId&&(v.TopicId==null||v.Topic!.IsPublished)))throw new ArgumentException("Không tìm thấy từ.");
  var p=await db.UserVocabularyProgresses.SingleOrDefaultAsync(x=>x.UserId==userId&&x.VocabularyId==vocabularyId);
  if(p is null){p=new(){UserId=userId,VocabularyId=vocabularyId};db.UserVocabularyProgresses.Add(p);}
  p.Status=status;p.UpdatedAtUtc=DateTime.UtcNow;
  db.StudyHistories.Add(new(){UserId=userId,Activity="Vocabulary",OccurredAtUtc=DateTime.UtcNow});
  await db.SaveChangesAsync();
 }
 public Task<List<UserVocabularyProgress>> ProgressAsync(string userId)=>db.UserVocabularyProgresses.AsNoTracking().Include(x=>x.Vocabulary).Where(x=>x.UserId==userId).OrderByDescending(x=>x.UpdatedAtUtc).ToListAsync();
 public Task<List<QuizAttempt>> HistoryAsync(string userId)=>db.QuizAttempts.AsNoTracking().Include(x=>x.Exercise).Where(x=>x.UserId==userId).OrderByDescending(x=>x.SubmittedAtUtc).Take(100).ToListAsync();
 public Task<Exercise?> ExerciseAsync(int id)=>db.Exercises.AsNoTracking().Include(x=>x.Questions).ThenInclude(x=>x.Answers).SingleOrDefaultAsync(x=>x.Id==id&&x.Topic!.IsPublished);
 public async Task<int> SaveAttemptAsync(QuizAttempt attempt){db.QuizAttempts.Add(attempt);db.StudyHistories.Add(new(){UserId=attempt.UserId,Activity="Quiz",OccurredAtUtc=DateTime.UtcNow});await db.SaveChangesAsync();return attempt.Id;}
 public Task<QuizAttempt?> AttemptAsync(int id,string userId)=>db.QuizAttempts.AsNoTracking().Include(x=>x.Answers).Include(x=>x.Exercise).SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==userId);
}
