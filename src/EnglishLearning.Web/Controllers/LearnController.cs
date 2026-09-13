using EnglishLearning.Application.Interfaces;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EnglishLearning.Web.Controllers;

public class LearnController(
    ILearningRepository repository,
    ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Grade(int id)
    {
        var grade = await db.Grades.FindAsync(id);

        if (grade is null)
        {
            return NotFound();
        }

        ViewBag.Grade = grade.Name;

        var topics =
            await repository.TopicsAsync(id);

        return View(topics);
    }

    [HttpGet]
    public async Task<IActionResult> Topic(int id)
    {
        var topic =
            await repository.TopicAsync(id);

        if (topic is null)
        {
            return NotFound();
        }

        var exercises =
            await db.Exercises
                .AsNoTracking()
                .Where(exercise =>
                    exercise.TopicId == id)
                .OrderBy(exercise => exercise.Id)
                .ToListAsync();

        IReadOnlyDictionary<int, LearningStatus> statuses =
            new Dictionary<int, LearningStatus>();

        if (User.Identity?.IsAuthenticated == true)
        {
            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var progress =
                    await repository.ProgressAsync(userId);

                statuses = progress.ToDictionary(
                    item => item.VocabularyId,
                    item => item.Status);
            }
        }

        var model = new TopicPage(
            topic,
            exercises,
            statuses);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Practice(int id)
    {
        var topic =
            await repository.TopicAsync(id);

        return topic is null
            ? NotFound()
            : View(topic);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Progress(
        int vocabularyId,
        LearningStatus status,
        string? returnUrl)
    {
        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        try
        {
            await repository.SaveProgressAsync(
                userId,
                vocabularyId,
                status);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }

        TempData["Message"] =
            "Đã cập nhật trạng thái học.";

        if (Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl!);
        }

        return RedirectToAction(
            "Index",
            "Dashboard");
    }
}