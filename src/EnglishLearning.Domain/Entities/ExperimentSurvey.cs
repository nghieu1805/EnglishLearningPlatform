using System.ComponentModel.DataAnnotations;

namespace EnglishLearning.Domain.Entities;

public class ExperimentSurvey
{
    public int Id { get; set; }

    public int ExperimentSessionId { get; set; }

    public ExperimentSession ExperimentSession { get; set; } = null!;

    [Range(1, 5)]
    public int PredictabilityRating { get; set; }

    [Range(1, 5)]
    public int SubmitFindabilityRating { get; set; }

    [Range(1, 5)]
    public int LayoutSatisfactionRating { get; set; }

    [Range(1, 5)]
    public int OverallSatisfactionRating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public DateTime SubmittedAtUtc { get; set; }
}