namespace EnglishLearning.Web.ViewModels;

public class TopicListItemViewModel
{
    public int Id { get; set; }

    public int GradeId { get; set; }

    public string GradeName { get; set; } =
        string.Empty;

    public string Name { get; set; } =
        string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsPublished { get; set; }

    public bool IsDemo { get; set; }

    public int SourceCount { get; set; }

    public int VocabularyCount { get; set; }
}

public class TopicListViewModel
{
    public List<TopicListItemViewModel> Items { get; set; } =
        [];

    public string Search { get; set; } =
        string.Empty;

    public int? GradeId { get; set; }

    public string Status { get; set; } =
        string.Empty;

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalItems { get; set; }
}