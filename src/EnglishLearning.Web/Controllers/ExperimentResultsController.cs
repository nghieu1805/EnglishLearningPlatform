using System.Globalization;
using System.Text;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Web.Controllers;

[Authorize(Roles = "Admin")]
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public class ExperimentResultsController(
    ExperimentResultService resultService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        ExperimentMode? mode,
        CancellationToken cancellationToken)
    {
        if (!IsValidMode(mode))
        {
            return BadRequest("Cấu hình trải nghiệm không hợp lệ.");
        }

        var results = await resultService.GetAsync(
            mode,
            cancellationToken);

        ViewData["SelectedMode"] = mode?.ToString();

        return View(results);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(
        ExperimentMode? mode,
        CancellationToken cancellationToken)
    {
        if (!IsValidMode(mode))
        {
            return BadRequest("Cấu hình trải nghiệm không hợp lệ.");
        }

        var results = await resultService.GetAsync(
            mode,
            cancellationToken);

        var csv = new StringBuilder();

        AppendRow(
            csv,
            "ParticipantCode",
            "SessionId",
            "AssignedMode",
            "CriticalFunctionStability",
            "ApprovalRequired",
            "Phase",
            "AdaptationApplied",
            "AdaptationAccepted",
            "SessionCreatedAtUtc",
            "SessionCompletedAtUtc",
            "MeasurementTaskId",
            "ExerciseId",
            "SubmitMoved",
            "TaskStartedAtUtc",
            "TaskCompletedAtUtc",
            "MeasurementCompleted",
            "DurationMilliseconds",
            "TimeToSubmitMilliseconds",
            "OldSubmitLocationClickCount",
            "IncompleteSubmitCount",
            "PredictabilityRating",
            "SubmitFindabilityRating",
            "LayoutSatisfactionRating",
            "OverallSatisfactionRating",
            "Comment",
            "SurveySubmittedAtUtc",
            "SurveySubmitted");

        foreach (var item in results)
        {
            AppendRow(
                csv,
                item.ParticipantCode,
                item.SessionId,
                item.Mode.ToString(),
                item.StabilityText,
                item.RequiresApproval,
                item.Phase.ToString(),
                item.AdaptationApplied,
                item.AdaptationAccepted,
                item.CreatedAtUtc,
                item.CompletedAtUtc,
                item.MeasurementTaskId,
                item.ExerciseId,
                item.SubmitMoved,
                item.TaskStartedAtUtc,
                item.TaskCompletedAtUtc,
                item.MeasurementCompleted,
                item.DurationMilliseconds,
                item.TimeToSubmitMilliseconds,
                item.OldSubmitLocationClickCount,
                item.IncompleteSubmitCount,
                item.PredictabilityRating,
                item.SubmitFindabilityRating,
                item.LayoutSatisfactionRating,
                item.OverallSatisfactionRating,
                item.Comment,
                item.SurveySubmittedAtUtc,
                item.SurveySubmitted);
        }

        // UTF-8 BOM giúp Excel nhận diện tiếng Việt.
        var encoding = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: true);

        byte[] preamble = encoding.GetPreamble();
        byte[] content = encoding.GetBytes(csv.ToString());
        byte[] bytes = new byte[preamble.Length + content.Length];

        Buffer.BlockCopy(
            preamble, 0, bytes, 0, preamble.Length);

        Buffer.BlockCopy(
            content, 0, bytes, preamble.Length, content.Length);

        string group = mode?.ToString() ?? "All";
        string fileName =
            $"experiment-results-{group}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(
            bytes,
            "text/csv; charset=utf-8",
            fileName);
    }

    private bool IsValidMode(ExperimentMode? mode)
    {
        return ModelState.IsValid &&
            (!mode.HasValue ||
             Enum.IsDefined(typeof(ExperimentMode), mode.Value));
    }

    private static void AppendRow(
        StringBuilder csv,
        params object?[] values)
    {
        csv.Append(string.Join(",", values.Select(ToCsvCell)));
        csv.Append("\r\n");
    }

    private static string ToCsvCell(object? value)
    {
        string text = value switch
        {
            null => string.Empty,
            bool flag => flag ? "true" : "false",
            DateTime date => date.ToString(
                "yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'",
                CultureInfo.InvariantCulture),
            IFormattable number => number.ToString(
                null,
                CultureInfo.InvariantCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };

        // Ngăn nội dung văn bản được Excel hiểu thành công thức.
        if (value is string)
        {
            string trimmed = text.TrimStart();

            if (trimmed.Length > 0 &&
                "=+-@".Contains(trimmed[0]))
            {
                text = "'" + text;
            }
        }

        // Bao mọi ô trong dấu ngoặc kép và escape dấu kép.
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}