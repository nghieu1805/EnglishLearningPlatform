using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class UserVocabularyProgress
{
public int Id { get; set; }
[MaxLength(255)] public string UserId { get; set; } = "";
public int VocabularyId { get; set; }
public Vocabulary? Vocabulary { get; set; }
public LearningStatus Status { get; set; }
public DateTime UpdatedAtUtc { get; set; }
}
