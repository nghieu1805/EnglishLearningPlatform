using EnglishLearning.Application.Interfaces;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Domain.Entities;
namespace EnglishLearning.Application.Services;
public class QuizService(ILearningRepository repository)
{
 public async Task<QuizDto?> GetAsync(int id)
 {
  var e=await repository.ExerciseAsync(id);
  return e is null || !IsValid(e) ? null : new(e.Id,e.Title,e.Questions.Select(q=>new QuestionDto(q.Id,q.Prompt,q.Answers.Select(a=>new ChoiceDto(a.Id,a.Text)).ToList())).ToList());
 }
 public static bool IsValid(Exercise e)=>e.Questions.Count>0 && e.Questions.All(q=>q.Answers.Count>=2 && q.Answers.Count(a=>a.IsCorrect)==1);
 public static QuizAttempt Score(Exercise e,string userId,IReadOnlyDictionary<int,int> selections)
 {
  if(!IsValid(e)) throw new ArgumentException("Quiz cần ít nhất một câu hỏi, mỗi câu có ít nhất 2 đáp án và đúng 1 đáp án đúng.");
  if(selections.Keys.Any(id=>e.Questions.All(q=>q.Id!=id))) throw new ArgumentException("Câu hỏi không thuộc quiz.");
  var attempt=new QuizAttempt{UserId=userId,ExerciseId=e.Id,Total=e.Questions.Count,SubmittedAtUtc=DateTime.UtcNow};
  foreach(var q in e.Questions)
  {
   var selected=selections.TryGetValue(q.Id,out var aId)?q.Answers.SingleOrDefault(a=>a.Id==aId):null;
   if(selections.ContainsKey(q.Id) && selected is null) throw new ArgumentException("Đáp án không thuộc câu hỏi.");
   attempt.Answers.Add(new QuizAnswer{QuestionId=q.Id,SelectedAnswerId=selected?.Id,IsCorrect=selected?.IsCorrect==true,PromptSnapshot=q.Prompt,SelectedSnapshot=selected?.Text??"Chưa trả lời",CorrectSnapshot=q.Answers.Single(a=>a.IsCorrect).Text});
  }
  attempt.Correct=attempt.Answers.Count(a=>a.IsCorrect);return attempt;
 }
 public async Task<int?> SubmitAsync(int id,string userId,Dictionary<int,int> selections)
 {
  var e=await repository.ExerciseAsync(id);if(e is null)return null;
  return await repository.SaveAttemptAsync(Score(e,userId,selections));
 }
}
