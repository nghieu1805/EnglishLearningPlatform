using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class ExperimentTask
{
    public int Id { get; set; }

    public int ExperimentSessionId { get; set; }

    public ExperimentSession ExperimentSession { get; set; } = null!;

    public int ExerciseId { get; set; }

    public Exercise Exercise { get; set; } = null!;

    // Giai đoạn tại thời điểm bắt đầu tác vụ.
    public ExperimentPhase Phase { get; set; }

    // Thứ tự tác vụ trong từng giai đoạn:
    // Làm quen: 1 và 2.
    // Đo lường: 1.
    public int TaskNumber { get; set; }

    // Nhận diện lượt làm, phục vụ chống ghi nhận trùng.
    public Guid AttemptToken { get; set; } = Guid.NewGuid();

    // Bố cục thực tế khi người dùng bắt đầu tác vụ.
    public bool AdaptationApplied { get; set; }

    public bool SubmitMoved { get; set; }

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    // Thời điểm đầu tiên đã chọn đáp án cho tất cả câu hỏi.
    public DateTime? AllQuestionsAnsweredAtUtc { get; set; }

    // Chỉ ghi khi bài đã được nộp và lưu thành công.
    public DateTime? CompletedAtUtc { get; set; }

    // Thời gian từ bắt đầu đến nộp thành công.
    public long? DurationMilliseconds { get; set; }

    // Thời gian từ trả lời đủ câu hỏi đến nộp thành công.
    public long? TimeToSubmitMilliseconds { get; set; }

    // Số lần nhấn vào vị trí Submit cũ khi nút đã di chuyển.
    public int OldSubmitLocationClickCount { get; set; }

    // Số lần thử nộp khi chưa trả lời đủ câu hỏi.
    // Đây là lỗi thao tác, khác với trả lời sai tiếng Anh.
    public int IncompleteSubmitCount { get; set; }

    public int? QuizAttemptId { get; set; }

    public QuizAttempt? QuizAttempt { get; set; }
}