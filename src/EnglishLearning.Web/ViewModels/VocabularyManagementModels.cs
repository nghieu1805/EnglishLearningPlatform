using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Web.ViewModels;

public class VocabularyListItemViewModel
{
    public int Id { get; set; }

    public int? TopicId { get; set; }

    public string GradeName { get; set; } =
        string.Empty;

    public string TopicName { get; set; } =
        string.Empty;

    public string SourceName { get; set; } =
        string.Empty;

    public string Unit { get; set; } =
        string.Empty;

    public string Word { get; set; } =
        string.Empty;

    public string MeaningVi { get; set; } =
        string.Empty;

    public string PartOfSpeech { get; set; } =
        string.Empty;

    public string Cefr { get; set; } =
        string.Empty;

    public VocabularyLevel Level { get; set; } =
        VocabularyLevel.Core;

    public int ExampleCount { get; set; }

    public int AudioCount { get; set; }
}

public class VocabularyListViewModel
{
    public List<VocabularyListItemViewModel> Items { get; set; } =
        [];

    public string Search { get; set; } =
        string.Empty;

    public int? GradeId { get; set; }

    public int? TopicId { get; set; }

    public string Cefr { get; set; } =
        string.Empty;

    public string Level { get; set; } =
        string.Empty;

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalItems { get; set; }
}