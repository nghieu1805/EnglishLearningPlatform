using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class QuizAttempt
{
public int Id { get; set; }
[MaxLength(255)] public string UserId { get; set; } = "";
public int ExerciseId { get; set; }
public Exercise? Exercise { get; set; }
public int Total { get; set; }
public int Correct { get; set; }
public DateTime SubmittedAtUtc { get; set; }
public List<QuizAnswer> Answers { get; set; } = [];
}
