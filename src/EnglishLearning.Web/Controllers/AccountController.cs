using EnglishLearning.Infrastructure.Identity;
using EnglishLearning.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Controller
{
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.DisplayName,
            CurrentGrade = model.CurrentGrade
        };

        var createResult = await userManager.CreateAsync(
            user,
            model.Password);

        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);

            return View(model);
        }

        var roleResult = await userManager.AddToRoleAsync(
            user,
            "Student");

        if (!roleResult.Succeeded)
        {
            // Không giữ lại tài khoản nếu quá trình gán role thất bại.
            await userManager.DeleteAsync(user);

            AddIdentityErrors(roleResult);

            return View(model);
        }

        await signInManager.SignInAsync(
            user,
            isPersistent: false);

        return RedirectToAction(
            "Index",
            "Dashboard");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        return View(new LoginModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl!);
            }

            return RedirectToAction(
                "Index",
                "Dashboard");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Tài khoản tạm khoá 15 phút do đăng nhập sai nhiều lần.");
        }
        else
        {
            ModelState.AddModelError(
                string.Empty,
                "Email hoặc mật khẩu không đúng.");
        }

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var model = new ProfileModel
        {
            DisplayName = user.DisplayName,
            CurrentGrade = user.CurrentGrade ?? 6
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Profile(ProfileModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        user.DisplayName = model.DisplayName;
        user.CurrentGrade = model.CurrentGrade;

        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["Message"] = "Đã lưu hồ sơ.";

            return RedirectToAction(nameof(Profile));
        }

        AddIdentityErrors(result);

        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();

        return RedirectToAction(
            "Index",
            "Home");
    }

    public IActionResult Denied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;

        return View();
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                error.Description);
        }
    }
}