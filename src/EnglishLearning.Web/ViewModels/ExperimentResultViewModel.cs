using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Web.ViewModels;

public class ExperimentResultViewModel
{
    public int SessionId { get; set; }

    // Dùng mã phiên làm mã người tham gia trong bảng kết quả.
    public string ParticipantCode => $"P{SessionId:D4}";

    public ExperimentMode Mode { get; set; }

    public ExperimentPhase Phase { get; set; }

    public bool AdaptationApplied { get; set; }

    public bool? AdaptationAccepted { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    // Dữ liệu của quiz đo lường, không cộng hai quiz làm quen.
    public int? MeasurementTaskId { get; set; }

    public int? ExerciseId { get; set; }

    public bool? SubmitMoved { get; set; }

    public DateTime? TaskStartedAtUtc { get; set; }

    public DateTime? TaskCompletedAtUtc { get; set; }

    public long? DurationMilliseconds { get; set; }

    public long? TimeToSubmitMilliseconds { get; set; }

    public int? OldSubmitLocationClickCount { get; set; }

    public int? IncompleteSubmitCount { get; set; }

    // Dữ liệu khảo sát.
    public int? PredictabilityRating { get; set; }

    public int? SubmitFindabilityRating { get; set; }

    public int? LayoutSatisfactionRating { get; set; }

    public int? OverallSatisfactionRating { get; set; }

    public string? Comment { get; set; }

    public DateTime? SurveySubmittedAtUtc { get; set; }

    public bool MeasurementCompleted =>
        TaskCompletedAtUtc.HasValue;

    public bool SurveySubmitted =>
        SurveySubmittedAtUtc.HasValue;

    public double? DurationSeconds =>
        DurationMilliseconds.HasValue
            ? DurationMilliseconds.Value / 1000.0
            : null;

    public double? TimeToSubmitSeconds =>
        TimeToSubmitMilliseconds.HasValue
            ? TimeToSubmitMilliseconds.Value / 1000.0
            : null;

    public string StabilityText =>
        Mode is ExperimentMode.C or ExperimentMode.D
            ? "Fixed"
            : "Movable";

    public bool RequiresApproval =>
        Mode is ExperimentMode.B or ExperimentMode.D;

    public string PhaseText => Phase switch
    {
        ExperimentPhase.Familiarisation => "Làm quen",
        ExperimentPhase.Adaptation => "Chờ quyết định",
        ExperimentPhase.Measurement => "Thực hiện tác vụ",
        ExperimentPhase.Completed => "Đã hoàn thành",
        _ => "Không xác định"
    };

    public string ApprovalText =>
        AdaptationAccepted.HasValue
            ? (AdaptationAccepted.Value ? "Accept" : "Reject")
            : (RequiresApproval
                ? "Chưa quyết định"
                : "Không cần phê duyệt");
}