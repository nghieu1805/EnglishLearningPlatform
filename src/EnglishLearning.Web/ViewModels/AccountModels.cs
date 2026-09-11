using System.ComponentModel.DataAnnotations;
namespace EnglishLearning.Web.ViewModels;
public class RegisterModel {
 [Required,EmailAddress] public string Email {get;set;}="";
 [Required,MinLength(8),DataType(DataType.Password)] public string Password {get;set;}="";
 [Compare(nameof(Password)),DataType(DataType.Password)] public string ConfirmPassword {get;set;}="";
 [Required,MaxLength(100)] public string DisplayName {get;set;}="";
 [Range(1,12)] public int CurrentGrade {get;set;}=6;
}
public class LoginModel {
 [Required,EmailAddress] public string Email {get;set;}="";
 [Required,DataType(DataType.Password)] public string Password {get;set;}="";
 public bool RememberMe {get;set;}
 public string? ReturnUrl {get;set;}
}
public class ProfileModel {
 [Required,MaxLength(100)] public string DisplayName {get;set;}="";
 [Range(1,12)] public int CurrentGrade {get;set;}=6;
}
