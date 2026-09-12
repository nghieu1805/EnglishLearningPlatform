using EnglishLearning.Application.Interfaces;
using EnglishLearning.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnglishLearning.Web.Controllers;

[Authorize]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class QuizController(
    QuizService quizService,
    ILearningRepository repository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Take(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Mã Quiz không hợp lệ.");
        }

        var model = await quizService.GetAsync(id);

        if (model is null)
        {
            return NotFound(
                "Quiz chưa sẵn sàng hoặc chủ đề chưa được xuất bản.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Take(
        int id,
        Dictionary<int, int>? selections)
    {
        if (id <= 0)
        {
            return BadRequest("Mã Quiz không hợp lệ.");
        }

        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return await ShowQuizWithError(
                id,
                "Dữ liệu câu trả lời không hợp lệ.");
        }

        selections ??=
            new Dictionary<int, int>();

        if (selections.Count == 0)
        {
            return await ShowQuizWithError(
                id,
                "Bạn cần chọn đáp án trước khi nộp bài.");
        }

        try
        {
            int? attemptId =
                await quizService.SubmitAsync(
                    id,
                    userId,
                    selections);

            if (!attemptId.HasValue)
            {
                return NotFound(
                    "Quiz không tồn tại hoặc chưa sẵn sàng.");
            }

            return RedirectToAction(
                nameof(Result),
                new
                {
                    id = attemptId.Value
                });
        }
        catch (ArgumentException exception)
        {
            return await ShowQuizWithError(
                id,
                exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return await ShowQuizWithError(
                id,
                exception.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Result(int id)
    {
        if (id <= 0)
        {
            return BadRequest(
                "Mã kết quả không hợp lệ.");
        }

        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var attempt =
            await repository.AttemptAsync(
                id,
                userId);

        if (attempt is null)
        {
            return NotFound(
                "Không tìm thấy kết quả Quiz.");
        }

        return View(attempt);
    }

    private async Task<IActionResult> ShowQuizWithError(
        int quizId,
        string message)
    {
        var model =
            await quizService.GetAsync(quizId);

        if (model is null)
        {
            return NotFound(
                "Quiz không tồn tại hoặc chưa sẵn sàng.");
        }

        ModelState.AddModelError(
            string.Empty,
            message);

        return View("Take", model);
    }
}