using System.ComponentModel.DataAnnotations;

namespace EnglishLearning.Web.ViewModels;

public class RegisterModel
{
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [MinLength(
        8,
        ErrorMessage = "Mật khẩu cần ít nhất 8 ký tự.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(
        ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
    [Compare(
        nameof(Password),
        ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } =
        string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [MaxLength(
        100,
        ErrorMessage = "Họ tên không quá 100 ký tự.")]
    public string DisplayName { get; set; } =
        string.Empty;

    [Range(
        1,
        12,
        ErrorMessage = "Lớp phải từ 1 đến 12.")]
    public int CurrentGrade { get; set; } = 6;
}

public class LoginModel
{
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class UpdateProfileModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [MaxLength(
        100,
        ErrorMessage = "Họ tên không quá 100 ký tự.")]
    public string DisplayName { get; set; } =
        string.Empty;

    [Range(
        1,
        12,
        ErrorMessage = "Lớp phải từ 1 đến 12.")]
    public int CurrentGrade { get; set; } = 6;
}

public class ChangePasswordModel
{
    [Required(
        ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [MinLength(
        8,
        ErrorMessage = "Mật khẩu mới cần ít nhất 8 ký tự.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
    [Compare(
        nameof(NewPassword),
        ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } =
        string.Empty;
}

public class ProfilePageModel
{
    public string Email { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];

    public UpdateProfileModel Profile { get; set; } =
        new();

    public ChangePasswordModel Password { get; set; } =
        new();

    public int LearnedWords { get; set; }

    public int LearningWords { get; set; }

    public int NeedsReviewWords { get; set; }

    public int QuizAttempts { get; set; }

    public int QuizAccuracy { get; set; }
}