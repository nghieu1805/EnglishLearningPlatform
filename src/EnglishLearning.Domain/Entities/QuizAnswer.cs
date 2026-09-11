using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class QuizAnswer
{
public int Id { get; set; }
public int QuizAttemptId { get; set; }
public QuizAttempt? QuizAttempt { get; set; }
public int QuestionId { get; set; }
public int? SelectedAnswerId { get; set; }
public bool IsCorrect { get; set; }
public string PromptSnapshot { get; set; } = "";
public string SelectedSnapshot { get; set; } = "";
public string CorrectSnapshot { get; set; } = "";
}
