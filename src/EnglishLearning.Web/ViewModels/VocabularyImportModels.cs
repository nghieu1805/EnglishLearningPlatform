using Microsoft.AspNetCore.Http;

namespace EnglishLearning.Web.ViewModels;

public class VocabularyImportUploadViewModel
{
    public IFormFile? File { get; set; }

    public string Mode { get; set; } =
        "preview";
}

public class VocabularyImportCsvRow
{
    public string GradeNumber { get; set; } =
        string.Empty;

    public string TopicName { get; set; } =
        string.Empty;

    public string Textbook { get; set; } =
        string.Empty;

    public string Unit { get; set; } =
        string.Empty;

    public string Level { get; set; } =
        string.Empty;

    public string Word { get; set; } =
        string.Empty;

    public string MeaningVi { get; set; } =
        string.Empty;

    public string PartOfSpeech { get; set; } =
        string.Empty;

    public string UkPhonetic { get; set; } =
        string.Empty;

    public string UsPhonetic { get; set; } =
        string.Empty;

    public string Cefr { get; set; } =
        string.Empty;

    public string RelatedWords { get; set; } =
        string.Empty;

    public string WordFamily { get; set; } =
        string.Empty;

    public string Example { get; set; } =
        string.Empty;

    public string TranslationVi { get; set; } =
        string.Empty;

    public string UkAudioUrl { get; set; } =
        string.Empty;

    public string UsAudioUrl { get; set; } =
        string.Empty;
}

public class VocabularyImportRowResult
{
    public int RowNumber { get; set; }

    public VocabularyImportCsvRow Data { get; set; } =
        new();

    public List<string> Errors { get; set; } =
        [];

    public bool IsValid =>
        Errors.Count == 0;
}

public class VocabularyImportResultViewModel
{
    public string FileName { get; set; } =
        string.Empty;

    public int TotalRows { get; set; }

    public int ValidRows { get; set; }

    public int ErrorRows { get; set; }

    public int ImportedRows { get; set; }

    public bool WasImported { get; set; }

    public List<VocabularyImportRowResult> Rows { get; set; } =
        [];
}