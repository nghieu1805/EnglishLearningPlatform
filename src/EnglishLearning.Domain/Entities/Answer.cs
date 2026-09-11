using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class Answer
{
public int Id { get; set; }
public int QuestionId { get; set; }
public Question? Question { get; set; }
[Required,MaxLength(500)] public string Text { get; set; } = "";
public bool IsCorrect { get; set; }
}
