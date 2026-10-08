using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Web.Services;

public class ExperimentSurveyService(ApplicationDbContext db)
{
    public async Task<ExperimentSurvey?> GetAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return await db.ExperimentSurveys
            .AsNoTracking()
            .SingleOrDefaultAsync(
                survey =>
                    survey.ExperimentSession.UserId == userId,
                cancellationToken);
    }

    public async Task<ExperimentSurvey> SubmitAsync(
        string userId,
        ExperimentSurveyViewModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(model);

        var session = await db.ExperimentSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.UserId == userId,
                cancellationToken);

        if (session is null)
        {
            throw new InvalidOperationException(
                "Bạn chưa tạo phiên trải nghiệm.");
        }

        if (session.Phase != ExperimentPhase.Completed)
        {
            throw new InvalidOperationException(
                "Bạn cần hoàn thành phiên trải nghiệm trước khi gửi khảo sát.");
        }

        var existing = await GetAsync(
            userId,
            cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        int predictability = RequireRating(
            model.PredictabilityRating,
            "câu hỏi 1");

        int submitFindability = RequireRating(
            model.SubmitFindabilityRating,
            "câu hỏi 2");

        int layoutSatisfaction = RequireRating(
            model.LayoutSatisfactionRating,
            "câu hỏi 3");

        int overallSatisfaction = RequireRating(
            model.OverallSatisfactionRating,
            "câu hỏi 4");

        if (model.Comment is { Length: > 2000 })
        {
            throw new ArgumentException(
                "Góp ý không được vượt quá 2.000 ký tự.");
        }

        string? comment = string.IsNullOrWhiteSpace(model.Comment)
            ? null
            : model.Comment.Trim();

        var survey = new ExperimentSurvey
        {
            ExperimentSessionId = session.Id,
            PredictabilityRating = predictability,
            SubmitFindabilityRating = submitFindability,
            LayoutSatisfactionRating = layoutSatisfaction,
            OverallSatisfactionRating = overallSatisfaction,
            Comment = comment,
            SubmittedAtUtc = DateTime.UtcNow
        };

        db.ExperimentSurveys.Add(survey);

        try
        {
            await db.SaveChangesAsync(cancellationToken);

            return survey;
        }
        catch (DbUpdateException)
        {
            // Hai yêu cầu có thể cùng gửi khảo sát cho một phiên.
            // Index duy nhất giữ lại một bản trả lời.
            db.Entry(survey).State = EntityState.Detached;

            var savedByAnotherRequest = await GetAsync(
                userId,
                cancellationToken);

            if (savedByAnotherRequest is not null)
            {
                return savedByAnotherRequest;
            }

            // Nếu chưa có bản khảo sát, giữ nguyên lỗi database.
            throw;
        }
    }

    private static int RequireRating(
        int? rating,
        string question)
    {
        if (rating is not int value || value < 1 || value > 5)
        {
            throw new ArgumentException(
                $"Vui lòng chọn điểm từ 1 đến 5 cho {question}.");
        }

        return value;
    }
}