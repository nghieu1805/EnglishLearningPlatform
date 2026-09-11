using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class VocabularyAudio
{
public int Id { get; set; }
public int VocabularyId { get; set; }
public Vocabulary? Vocabulary { get; set; }
[RegularExpression("UK|US")] public string Accent { get; set; } = "UK";
[Required,MaxLength(2000)] public string Url { get; set; } = "";
}
