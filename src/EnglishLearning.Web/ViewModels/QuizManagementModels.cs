namespace EnglishLearning.Web.ViewModels;

public class QuizListItemViewModel
{
    public int Id { get; set; }

    public int TopicId { get; set; }

    public string Title { get; set; } =
        string.Empty;

    public string GradeName { get; set; } =
        string.Empty;

    public string TopicName { get; set; } =
        string.Empty;

    public int QuestionCount { get; set; }

    public int AnswerCount { get; set; }

    public int InvalidQuestionCount { get; set; }

    public bool IsReady =>
        QuestionCount > 0 &&
        InvalidQuestionCount == 0;
}

public class QuizListViewModel
{
    public List<QuizListItemViewModel> Items { get; set; } =
        [];

    public string Search { get; set; } =
        string.Empty;

    public int? GradeId { get; set; }

    public int? TopicId { get; set; }

    public string Status { get; set; } =
        string.Empty;

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalItems { get; set; }
}

public class QuizDetailViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } =
        string.Empty;

    public string GradeName { get; set; } =
        string.Empty;

    public string TopicName { get; set; } =
        string.Empty;

    public List<QuizQuestionItemViewModel> Questions { get; set; } =
        [];
}

public class QuizQuestionItemViewModel
{
    public int Id { get; set; }

    public string Prompt { get; set; } =
        string.Empty;

    public List<QuizAnswerItemViewModel> Answers { get; set; } =
        [];

    public bool IsValid =>
        Answers.Count >= 2 &&
        Answers.Count(answer => answer.IsCorrect) == 1;
}

public class QuizAnswerItemViewModel
{
    public int Id { get; set; }

    public string Text { get; set; } =
        string.Empty;

    public bool IsCorrect { get; set; }
}