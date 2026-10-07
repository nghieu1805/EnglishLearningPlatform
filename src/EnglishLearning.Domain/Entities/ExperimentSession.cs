using System.ComponentModel.DataAnnotations;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class ExperimentSession
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string UserId { get; set; } = string.Empty;

    public ExperimentMode Mode { get; set; }

    public ExperimentPhase Phase { get; set; }
        = ExperimentPhase.Familiarisation;

    public bool AdaptationApplied { get; set; }

    // null: chưa quyết định hoặc không cần hỏi.
    // true: Accept; false: Reject.
    public bool? AdaptationAccepted { get; set; }

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;

    public DateTime? AdaptationAppliedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }
}