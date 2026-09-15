using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Web.Services;

namespace EnglishLearning.Web.ViewModels;

public class DictionaryPage
{
    public string Query { get; set; } = string.Empty;

    public bool HasSearched { get; set; }

    public string? ErrorMessage { get; set; }

    public List<Vocabulary> Results { get; set; } = [];

    public IReadOnlyDictionary<int, LearningStatus> Statuses
    { get; set; } =
        new Dictionary<int, LearningStatus>();

    public DictionaryApiLookupResult? ExternalResult
    { get; set; }

    public bool HasInternalResults =>
        Results.Count > 0;

    public bool HasExternalResults =>
        ExternalResult?.Found == true;
}