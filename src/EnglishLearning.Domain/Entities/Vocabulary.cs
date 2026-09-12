using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class Vocabulary
{
    public int Id { get; set; }

    public int? TopicId { get; set; }

    public Topic? Topic { get; set; }

    public int? TopicSourceId { get; set; }

    public TopicSource? TopicSource { get; set; }

    [Required]
    [MaxLength(100)]
    public string Word { get; set; } =
        string.Empty;

    [Required]
    [MaxLength(500)]
    public string MeaningVi { get; set; } =
        string.Empty;

    [MaxLength(50)]
    public string PartOfSpeech { get; set; } =
        string.Empty;

    [MaxLength(100)]
    public string UkPhonetic { get; set; } =
        string.Empty;

    [MaxLength(100)]
    public string UsPhonetic { get; set; } =
        string.Empty;

    [MaxLength(10)]
    public string Cefr { get; set; } =
        string.Empty;

    [MaxLength(500)]
    public string RelatedWords { get; set; } =
        string.Empty;

    [MaxLength(500)]
    public string WordFamily { get; set; } =
        string.Empty;

    public VocabularyLevel Level { get; set; } =
        VocabularyLevel.Core;

    public List<VocabularyExample> Examples { get; set; } =
        [];

    public List<VocabularyAudio> Audios { get; set; } =
        [];
}