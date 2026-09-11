using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;
namespace EnglishLearning.Domain.Entities;
public class VocabularyExample
{
public int Id { get; set; }
public int VocabularyId { get; set; }
public Vocabulary? Vocabulary { get; set; }
[Required,MaxLength(1000)] public string Sentence { get; set; } = "";
[MaxLength(1000)] public string TranslationVi { get; set; } = "";
}
