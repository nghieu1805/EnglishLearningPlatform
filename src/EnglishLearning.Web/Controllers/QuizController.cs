using System.Security.Claims;
using EnglishLearning.Application.Interfaces;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Web.Controllers;

[Authorize]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class QuizController(
    QuizService quizService,
    ILearningRepository repository,
    ExperimentService experimentService,
    ExperimentTaskService experimentTaskService,
    ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Take(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest("Mã Quiz không hợp lệ.");
        }

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var model = await quizService.GetAsync(id);

        if (model is null)
        {
            return NotFound(
                "Quiz chưa sẵn sàng hoặc chủ đề chưa được xuất bản.");
        }

        try
        {
            var task = await experimentTaskService.StartOrResumeAsync(
                userId,
                id,
                cancellationToken);

            SetExperimentViewData(task);

            return View(model);
        }
        catch (InvalidOperationException exception)
        {
            TempData["Message"] = exception.Message;

            return RedirectToAction(
                "Index",
                "Experiment");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Take(
        int id,
        Dictionary<int, int>? selections,
        Guid? attemptToken,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest("Mã Quiz không hợp lệ.");
        }

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        ExperimentTask? task = null;

        var session = await experimentService.GetAsync(
            userId,
            cancellationToken);

        if (attemptToken.HasValue)
        {
            task = await experimentTaskService.GetOwnedAsync(
                userId,
                attemptToken.Value,
                cancellationToken);

            if (task is null || task.ExerciseId != id)
            {
                return BadRequest(
                    "Tác vụ không hợp lệ hoặc không thuộc bài kiểm tra này.");
            }

            // Gửi lại form đã hoàn thành: mở kết quả đã lưu.
            if (task.CompletedAtUtc.HasValue &&
                task.QuizAttemptId.HasValue)
            {
                return RedirectToAction(
                    nameof(Result),
                    new { id = task.QuizAttemptId.Value });
            }

            if (session is null || session.Phase != task.Phase)
            {
                TempData["Message"] =
                    "Giai đoạn đã thay đổi. Hãy tiếp tục từ phiên trải nghiệm.";

                return RedirectToAction(
                    "Index",
                    "Experiment");
            }
        }
        else if (session is not null)
        {
            TempData["Message"] =
                "Bài chưa có mã tác vụ. Hãy mở lại quiz từ phiên trải nghiệm.";

            return RedirectToAction(
                "Index",
                "Experiment");
        }

        if (!ModelState.IsValid)
        {
            return await ShowQuizWithErrorAsync(
                id,
                "Dữ liệu câu trả lời không hợp lệ.",
                task);
        }

        selections ??= new Dictionary<int, int>();

        if (selections.Count == 0)
        {
            return await ShowQuizWithErrorAsync(
                id,
                "Bạn cần chọn đáp án trước khi nộp bài.",
                task);
        }

        try
        {
            int? resultId;

            if (task is null)
            {
                // Luồng học thông thường, chưa có phiên trải nghiệm.
                resultId = await quizService.SubmitAsync(
                    id,
                    userId,
                    selections);
            }
            else
            {
                resultId = await SubmitExperimentQuizAsync(
                    id,
                    userId,
                    selections,
                    task.AttemptToken,
                    cancellationToken);
            }

            if (!resultId.HasValue)
            {
                return NotFound(
                    "Quiz không tồn tại hoặc chưa sẵn sàng.");
            }

            return RedirectToAction(
                nameof(Result),
                new { id = resultId.Value });
        }
        catch (ArgumentException exception)
        {
            return await ShowQuizWithErrorAsync(
                id,
                exception.Message,
                task);
        }
        catch (InvalidOperationException exception)
        {
            return await ShowQuizWithErrorAsync(
                id,
                exception.Message,
                task);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Result(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Mã kết quả không hợp lệ.");
        }

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var attempt = await repository.AttemptAsync(
            id,
            userId);

        if (attempt is null)
        {
            return NotFound("Không tìm thấy kết quả Quiz.");
        }

        return View(attempt);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMetrics(
    Guid attemptToken,
    DateTimeOffset? allQuestionsAnsweredAtUtc,
    int oldSubmitLocationClickCount,
    int incompleteSubmitCount,
    CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid ||
            attemptToken == Guid.Empty ||
            oldSubmitLocationClickCount < 0 ||
            incompleteSubmitCount < 0 ||
            oldSubmitLocationClickCount > 100000 ||
            incompleteSubmitCount > 100000)
        {
            return BadRequest("Dữ liệu thao tác không hợp lệ.");
        }

        var task = await experimentTaskService.GetOwnedAsync(
            userId,
            attemptToken,
            cancellationToken);

        if (task is null)
        {
            return NotFound();
        }

        if (task.CompletedAtUtc.HasValue)
        {
            return NoContent();
        }

        var answeredAt = allQuestionsAnsweredAtUtc?.UtcDateTime;
        var now = DateTime.UtcNow;

        if (answeredAt.HasValue &&
            (answeredAt.Value < task.StartedAtUtc ||
             answeredAt.Value > now))
        {
            return BadRequest(
                "Thời điểm thao tác không hợp lệ. Kiểm tra đồng hồ thiết bị.");
        }

        var oldClicks = task.SubmitMoved
            ? oldSubmitLocationClickCount
            : 0;

        await db.ExperimentTasks
            .Where(item =>
                item.Id == task.Id &&
                item.CompletedAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        item => item.AllQuestionsAnsweredAtUtc,
                        item => item.AllQuestionsAnsweredAtUtc ?? answeredAt)
                    .SetProperty(
                        item => item.OldSubmitLocationClickCount,
                        item => item.OldSubmitLocationClickCount > oldClicks
                            ? item.OldSubmitLocationClickCount
                            : oldClicks)
                    .SetProperty(
                        item => item.IncompleteSubmitCount,
                        item => item.IncompleteSubmitCount > incompleteSubmitCount
                            ? item.IncompleteSubmitCount
                            : incompleteSubmitCount),
                cancellationToken);

        return NoContent();
    }
    private async Task<int?> SubmitExperimentQuizAsync(
        int quizId,
        string userId,
        Dictionary<int, int> selections,
        Guid attemptToken,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync(cancellationToken);

        // Khóa dòng tác vụ đến khi transaction kết thúc.
        // Yêu cầu nộp đồng thời phải chờ yêu cầu trước xử lý xong.
        await db.ExperimentTasks
            .Where(task =>
                task.AttemptToken == attemptToken &&
                task.ExperimentSession.UserId == userId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    task => task.StartedAtUtc,
                    task => task.StartedAtUtc),
                cancellationToken);

        var task = await experimentTaskService.GetOwnedAsync(
            userId,
            attemptToken,
            cancellationToken);

        if (task is null || task.ExerciseId != quizId)
        {
            throw new InvalidOperationException(
                "Không tìm thấy tác vụ hợp lệ.");
        }

        // Kiểm tra lại sau khi lấy khóa.
        if (task.CompletedAtUtc.HasValue)
        {
            if (!task.QuizAttemptId.HasValue)
            {
                throw new InvalidOperationException(
                    "Tác vụ đã hoàn thành nhưng chưa có kết quả.");
            }

            await transaction.CommitAsync(cancellationToken);

            return task.QuizAttemptId.Value;
        }

        var session = await experimentService.GetAsync(
            userId,
            cancellationToken);

        if (session is null || session.Phase != task.Phase)
        {
            throw new InvalidOperationException(
                "Giai đoạn đã thay đổi. Hãy quay lại phiên trải nghiệm.");
        }

        var resultId = await quizService.SubmitAsync(
            quizId,
            userId,
            selections);

        if (!resultId.HasValue)
        {
            return null;
        }

        await experimentTaskService.CompleteAsync(
            userId,
            attemptToken,
            resultId.Value,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return resultId.Value;
    }

    private async Task<IActionResult> ShowQuizWithErrorAsync(
        int quizId,
        string message,
        ExperimentTask? task)
    {
        var model = await quizService.GetAsync(quizId);

        if (model is null)
        {
            return NotFound(
                "Quiz không tồn tại hoặc chưa sẵn sàng.");
        }

        ModelState.AddModelError(
            string.Empty,
            message);

        SetExperimentViewData(task);

        return View("Take", model);
    }

    private void SetExperimentViewData(
        ExperimentTask? task)
    {
        ViewData["MoveSubmit"] = task?.SubmitMoved ?? false;
        ViewData["AttemptToken"] = task?.AttemptToken;
        ViewData["ExperimentTaskNumber"] = task?.TaskNumber;
        ViewData["ExperimentTaskPhase"] = task?.Phase;
    }
}