using System.Text.Json.Serialization;

namespace EnglishLearning.Web.Services;

public sealed class DictionaryApiEntry
{
    [JsonPropertyName("word")]
    public string Word { get; set; } = string.Empty;

    [JsonPropertyName("phonetic")]
    public string Phonetic { get; set; } = string.Empty;

    [JsonPropertyName("phonetics")]
    public List<DictionaryApiPhonetic> Phonetics
    { get; set; } = [];

    [JsonPropertyName("meanings")]
    public List<DictionaryApiMeaning> Meanings
    { get; set; } = [];

    [JsonPropertyName("sourceUrls")]
    public List<string> SourceUrls
    { get; set; } = [];

    [JsonPropertyName("license")]
    public DictionaryApiLicense? License { get; set; }
}

public sealed class DictionaryApiPhonetic
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("audio")]
    public string Audio { get; set; } = string.Empty;

    [JsonPropertyName("sourceUrl")]
    public string SourceUrl { get; set; } = string.Empty;
}

public sealed class DictionaryApiMeaning
{
    [JsonPropertyName("partOfSpeech")]
    public string PartOfSpeech { get; set; } =
        string.Empty;

    [JsonPropertyName("definitions")]
    public List<DictionaryApiDefinition> Definitions
    { get; set; } = [];

    [JsonPropertyName("synonyms")]
    public List<string> Synonyms { get; set; } = [];

    [JsonPropertyName("antonyms")]
    public List<string> Antonyms { get; set; } = [];
}

public sealed class DictionaryApiDefinition
{
    [JsonPropertyName("definition")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("example")]
    public string Example { get; set; } = string.Empty;

    [JsonPropertyName("synonyms")]
    public List<string> Synonyms { get; set; } = [];

    [JsonPropertyName("antonyms")]
    public List<string> Antonyms { get; set; } = [];
}

public sealed class DictionaryApiLicense
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

public sealed class DictionaryApiLookupResult
{
    public bool IsAvailable { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public List<DictionaryApiEntry> Entries
    { get; set; } = [];

    public bool Found => Entries.Count > 0;
}