using System.ComponentModel.DataAnnotations;

namespace EnglishLearning.Web.ViewModels;

public class ExperimentSurveyViewModel
{
    [Required(ErrorMessage = "Vui lòng trả lời câu hỏi 1.")]
    [Range(1, 5, ErrorMessage = "Điểm phải từ 1 đến 5.")]
    [Display(
        Name = "Tôi dễ dự đoán vị trí của các chức năng khi sử dụng giao diện.")]
    public int? PredictabilityRating { get; set; }

    [Required(ErrorMessage = "Vui lòng trả lời câu hỏi 2.")]
    [Range(1, 5, ErrorMessage = "Điểm phải từ 1 đến 5.")]
    [Display(
        Name = "Tôi dễ tìm thấy nút nộp bài khi cần.")]
    public int? SubmitFindabilityRating { get; set; }

    [Required(ErrorMessage = "Vui lòng trả lời câu hỏi 3.")]
    [Range(1, 5, ErrorMessage = "Điểm phải từ 1 đến 5.")]
    [Display(
        Name = "Tôi hài lòng với cách bố trí giao diện trong phiên trải nghiệm.")]
    public int? LayoutSatisfactionRating { get; set; }

    [Required(ErrorMessage = "Vui lòng trả lời câu hỏi 4.")]
    [Range(1, 5, ErrorMessage = "Điểm phải từ 1 đến 5.")]
    [Display(
        Name = "Tôi hài lòng với trải nghiệm thực hiện và nộp bài.")]
    public int? OverallSatisfactionRating { get; set; }

    [StringLength(
        2000,
        ErrorMessage = "Góp ý không được vượt quá 2.000 ký tự.")]
    [Display(Name = "Góp ý thêm (không bắt buộc)")]
    public string? Comment { get; set; }
}