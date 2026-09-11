using Microsoft.AspNetCore.Identity;
namespace EnglishLearning.Infrastructure.Identity;
public class ApplicationUser:IdentityUser
{
 public string DisplayName { get; set; } = "";
 public int? CurrentGrade { get; set; }
}
