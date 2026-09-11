using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class StudyHistory
{
public int Id { get; set; }
[MaxLength(255)] public string UserId { get; set; } = "";
[MaxLength(100)] public string Activity { get; set; } = "";
public DateTime OccurredAtUtc { get; set; }
}
