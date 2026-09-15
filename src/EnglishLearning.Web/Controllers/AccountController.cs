using EnglishLearning.Application.Interfaces;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Identity;
using EnglishLearning.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILearningRepository repository) : Controller
{
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Email = model.Email.Trim();
        model.DisplayName = model.DisplayName.Trim();

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.DisplayName,
            CurrentGrade = model.CurrentGrade
        };

        var createResult =
            await userManager.CreateAsync(
                user,
                model.Password);

        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);

            return View(model);
        }

        var roleResult =
            await userManager.AddToRoleAsync(
                user,
                "Student");

        if (!roleResult.Succeeded)
        {
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
        return View(
            new LoginModel
            {
                ReturnUrl = returnUrl
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Email = model.Email.Trim();

        var result =
            await signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(
                    model.ReturnUrl!);
            }

            return RedirectToAction(
                "Index",
                "Dashboard");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Tài khoản tạm khoá 15 phút do " +
                "đăng nhập sai nhiều lần.");
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
        var user =
            await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var model =
            await BuildProfileModelAsync(user);

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        [Bind(Prefix = "Profile")]
        UpdateProfileModel model)
    {
        var user =
            await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            var pageModel =
                await BuildProfileModelAsync(
                    user,
                    model);

            return View(
                nameof(Profile),
                pageModel);
        }

        user.DisplayName =
            model.DisplayName.Trim();

        user.CurrentGrade =
            model.CurrentGrade;

        var result =
            await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);

            var pageModel =
                await BuildProfileModelAsync(
                    user,
                    model);

            return View(
                nameof(Profile),
                pageModel);
        }

        await signInManager.RefreshSignInAsync(user);

        TempData["Message"] =
            "Đã cập nhật hồ sơ.";

        return RedirectToAction(
            nameof(Profile));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        [Bind(Prefix = "Password")]
        ChangePasswordModel model)
    {
        var user =
            await userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            ClearPasswordValuesFromModelState();

            var pageModel =
                await BuildProfileModelAsync(user);

            return View(
                nameof(Profile),
                pageModel);
        }

        var result =
            await userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            ClearPasswordValuesFromModelState();

            var pageModel =
                await BuildProfileModelAsync(user);

            return View(
                nameof(Profile),
                pageModel);
        }

        await signInManager.RefreshSignInAsync(user);

        TempData["Message"] =
            "Đã đổi mật khẩu thành công.";

        return RedirectToAction(
            nameof(Profile));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();

        return RedirectToAction(
            "Index",
            "Home");
    }

    [HttpGet]
    public IActionResult Denied()
    {
        Response.StatusCode =
            StatusCodes.Status403Forbidden;

        return View();
    }

    private async Task<ProfilePageModel>
        BuildProfileModelAsync(
            ApplicationUser user,
            UpdateProfileModel? profile = null)
    {
        var roles =
            await userManager.GetRolesAsync(user);

        var progress =
            await repository.ProgressAsync(user.Id);

        var attempts =
            await repository.HistoryAsync(user.Id);

        int totalQuestions =
            attempts.Sum(attempt =>
                attempt.Total);

        int accuracy =
            totalQuestions == 0
                ? 0
                : (int)Math.Round(
                    100.0 *
                    attempts.Sum(attempt =>
                        attempt.Correct) /
                    totalQuestions);

        return new ProfilePageModel
        {
            Email =
                user.Email ?? string.Empty,

            Roles =
                roles.ToList(),

            Profile =
                profile ??
                new UpdateProfileModel
                {
                    DisplayName =
                        user.DisplayName,

                    CurrentGrade =
                        user.CurrentGrade ?? 6
                },

            LearnedWords =
                progress.Count(item =>
                    item.Status ==
                    LearningStatus.Learned),

            LearningWords =
                progress.Count(item =>
                    item.Status ==
                    LearningStatus.Learning),

            NeedsReviewWords =
                progress.Count(item =>
                    item.Status ==
                    LearningStatus.NeedsReview),

            QuizAttempts =
                attempts.Count,

            QuizAccuracy =
                accuracy
        };
    }

    private void AddIdentityErrors(
        IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                error.Description);
        }
    }

    private void ClearPasswordValuesFromModelState()
    {
        ModelState.Remove(
            "Password.CurrentPassword");

        ModelState.Remove(
            "Password.NewPassword");

        ModelState.Remove(
            "Password.ConfirmNewPassword");
    }
}