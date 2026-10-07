using System.Security.Claims;
using EnglishLearning.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace EnglishLearning.Web.Controllers;

[Authorize]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class ExperimentController(
    ExperimentService experimentService,
    IWebHostEnvironment environment) : Controller
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

        return View(session);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        await experimentService.GetOrCreateAsync(
            userId,
            cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TriggerAdaptation(
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var session = await experimentService.GetAsync(
            userId,
            cancellationToken);

        if (session is null)
        {
            return RedirectToAction(nameof(Index));
        }

        await experimentService.TriggerAdaptationAsync(
            userId,
            cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DecideAdaptation(
        bool? accepted,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        if (!ModelState.IsValid || !accepted.HasValue)
        {
            return BadRequest("Quyết định không hợp lệ.");
        }

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        await experimentService.DecideAdaptationAsync(
            userId,
            accepted.Value,
            cancellationToken);

        return RedirectToAction(nameof(Index));
    }
}