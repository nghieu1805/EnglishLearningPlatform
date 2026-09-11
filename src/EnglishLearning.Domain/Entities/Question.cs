using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class Question
{
public int Id { get; set; }
public int ExerciseId { get; set; }
public Exercise? Exercise { get; set; }
[Required,MaxLength(1000)] public string Prompt { get; set; } = "";
public List<Answer> Answers { get; set; } = [];
}
