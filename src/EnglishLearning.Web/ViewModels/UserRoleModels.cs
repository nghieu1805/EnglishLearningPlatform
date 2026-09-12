using System.ComponentModel.DataAnnotations;

namespace EnglishLearning.Web.ViewModels;

public class UserRoleItemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int? CurrentGrade { get; set; }

    public List<string> Roles { get; set; } = [];
}

public class UserListViewModel
{
    public List<UserRoleItemViewModel> Items { get; set; } = [];

    public string Search { get; set; } = string.Empty;

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalItems { get; set; }
}

public class ChangeUserRoleModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "Student|Staff",
        ErrorMessage = "Role chỉ có thể là Student hoặc Staff.")]
    public string Role { get; set; } = "Student";

    public string Search { get; set; } = string.Empty;

    public int Page { get; set; } = 1;
}