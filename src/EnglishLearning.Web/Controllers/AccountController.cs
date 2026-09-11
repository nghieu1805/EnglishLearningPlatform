using EnglishLearning.Web.ViewModels;
using EnglishLearning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EnglishLearning.Web.Controllers;
public class AccountController(UserManager<ApplicationUser> users,SignInManager<ApplicationUser> signIn):Controller
{
 [HttpGet] public IActionResult Register()=>View(new RegisterModel());
 [HttpPost] public async Task<IActionResult> Register(RegisterModel model){
  if(!ModelState.IsValid)return View(model);
  var user=new ApplicationUser{UserName=model.Email,Email=model.Email,DisplayName=model.DisplayName,CurrentGrade=model.CurrentGrade};
  var result=await users.CreateAsync(user,model.Password);
  if(result.Succeeded){await signIn.SignInAsync(user,false);return RedirectToAction("Index","Dashboard");}
  foreach(var error in result.Errors)ModelState.AddModelError("",error.Description);return View(model);
 }
 [HttpGet] public IActionResult Login(string? returnUrl)=>View(new LoginModel{ReturnUrl=returnUrl});
 [HttpPost] public async Task<IActionResult> Login(LoginModel model){
  if(!ModelState.IsValid)return View(model);
  var result=await signIn.PasswordSignInAsync(model.Email,model.Password,model.RememberMe,true);
  if(result.Succeeded)return Url.IsLocalUrl(model.ReturnUrl)?LocalRedirect(model.ReturnUrl!):RedirectToAction("Index","Dashboard");
  ModelState.AddModelError("",result.IsLockedOut?"Tài khoản tạm khoá 15 phút sau nhiều lần đăng nhập sai.":"Email hoặc mật khẩu không đúng.");return View(model);
 }
 [Authorize,HttpPost] public async Task<IActionResult> Logout(){await signIn.SignOutAsync();return RedirectToAction("Index","Home");}
 [Authorize,HttpGet] public async Task<IActionResult> Profile(){var u=await users.GetUserAsync(User);if(u is null)return Challenge();return View(new ProfileModel{DisplayName=u.DisplayName,CurrentGrade=u.CurrentGrade??6});}
 [Authorize,HttpPost] public async Task<IActionResult> Profile(ProfileModel model){
  if(!ModelState.IsValid)return View(model);var u=await users.GetUserAsync(User);if(u is null)return Challenge();
  u.DisplayName=model.DisplayName;u.CurrentGrade=model.CurrentGrade;var r=await users.UpdateAsync(u);
  if(r.Succeeded)TempData["Message"]="Đã lưu hồ sơ.";else foreach(var e in r.Errors)ModelState.AddModelError("",e.Description);return View(model);
 }
 public IActionResult Denied(){Response.StatusCode=403;return View();}
}
