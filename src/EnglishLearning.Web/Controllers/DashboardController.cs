using EnglishLearning.Application.Interfaces;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EnglishLearning.Web.Controllers;

[Authorize]
public class DashboardController(
    ILearningRepository repository,
    ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        LearningStatus? status)
    {
        if (status.HasValue &&
            !Enum.IsDefined(
                typeof(LearningStatus),
                status.Value))
        {
            return BadRequest(
                "Trạng thái học không hợp lệ.");
        }

        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var progress =
            await repository.ProgressAsync(userId);

        var history =
            await repository.HistoryAsync(userId);

        var topicData =
            await db.Topics
                .AsNoTracking()
                .Where(topic => topic.IsPublished)
                .OrderBy(topic =>
                    topic.Grade!.Number)
                .ThenBy(topic => topic.SortOrder)
                .ThenBy(topic => topic.Name)
                .Select(topic => new
                {
                    topic.Id,

                    GradeName =
                        topic.Grade != null
                            ? topic.Grade.Name
                            : string.Empty,

                    topic.Name,

                    Total =
                        topic.Vocabulary.Count,

                    CoreTotal =
                        topic.Vocabulary.Count(
                            vocabulary =>
                                vocabulary.Level ==
                                VocabularyLevel.Core),

                    AdvancedTotal =
                        topic.Vocabulary.Count(
                            vocabulary =>
                                vocabulary.Level ==
                                VocabularyLevel.Advanced)
                })
                .Where(topic => topic.Total > 0)
                .ToListAsync();

        var summaries =
            topicData.Select(topic =>
            {
                var topicProgress =
                    progress
                        .Where(item =>
                            item.Vocabulary?.TopicId ==
                            topic.Id)
                        .ToList();

                return new TopicSummary(
                    topic.Id,
                    topic.GradeName,
                    topic.Name,
                    topic.Total,
                    topic.CoreTotal,
                    topic.AdvancedTotal,
                    topicProgress.Count(item =>
                        item.Status ==
                        LearningStatus.New),
                    topicProgress.Count(item =>
                        item.Status ==
                        LearningStatus.Learning),
                    topicProgress.Count(item =>
                        item.Status ==
                        LearningStatus.Learned),
                    topicProgress.Count(item =>
                        item.Status ==
                        LearningStatus.NeedsReview));
            })
            .ToList();

        var filteredProgress =
            status.HasValue
                ? progress
                    .Where(item =>
                        item.Status == status.Value)
                    .ToList()
                : progress;

        var model = new DashboardPage(
            progress,
            filteredProgress,
            history,
            summaries,
            status);

        return View(model);
    }
}