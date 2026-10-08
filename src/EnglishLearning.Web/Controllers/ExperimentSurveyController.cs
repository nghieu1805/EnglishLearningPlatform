using System.Security.Claims;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Web.Services;
using EnglishLearning.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Web.Controllers;

[Authorize]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class ExperimentSurveyController(
    ExperimentService experimentService,
    ExperimentSurveyService surveyService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var session = await experimentService.GetAsync(
            userId,
            cancellationToken);

        if (session is null ||
            session.Phase != ExperimentPhase.Completed)
        {
            TempData["Message"] =
                "Bạn cần hoàn thành phiên trải nghiệm trước khi làm khảo sát.";

            return RedirectToAction("Index", "Experiment");
        }

        var existing = await surveyService.GetAsync(
            userId,
            cancellationToken);

        if (existing is not null)
        {
            return RedirectToAction(nameof(ThankYou));
        }

        return View(new ExperimentSurveyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        ExperimentSurveyViewModel model,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var session = await experimentService.GetAsync(
            userId,
            cancellationToken);

        if (session is null ||
            session.Phase != ExperimentPhase.Completed)
        {
            TempData["Message"] =
                "Bạn cần hoàn thành phiên trải nghiệm trước khi làm khảo sát.";

            return RedirectToAction("Index", "Experiment");
        }

        var existing = await surveyService.GetAsync(
            userId,
            cancellationToken);

        if (existing is not null)
        {
            return RedirectToAction(nameof(ThankYou));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await surveyService.SubmitAsync(
                userId,
                model,
                cancellationToken);

            return RedirectToAction(nameof(ThankYou));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            return View(model);
        }
        catch (InvalidOperationException exception)
        {
            TempData["Message"] = exception.Message;

            return RedirectToAction("Index", "Experiment");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ThankYou(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var survey = await surveyService.GetAsync(
            userId,
            cancellationToken);

        if (survey is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View();
    }
}