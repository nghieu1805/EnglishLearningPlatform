using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Web.Services;

public class ExperimentResultService(ApplicationDbContext db)
{
    public async Task<List<ExperimentResultViewModel>> GetAsync(
        ExperimentMode? mode = null,
        CancellationToken cancellationToken = default)
    {
        if (mode.HasValue &&
            !Enum.IsDefined(typeof(ExperimentMode), mode.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        var sessions = db.ExperimentSessions
            .AsNoTracking()
            .AsQueryable();

        if (mode.HasValue)
        {
            sessions = sessions.Where(
                session => session.Mode == mode.Value);
        }

        var measurementTasks = db.ExperimentTasks
            .AsNoTracking()
            .Where(task =>
                task.Phase == ExperimentPhase.Measurement &&
                task.TaskNumber == 1);

        var surveys = db.ExperimentSurveys
            .AsNoTracking();

        var query =
            from session in sessions

            join task in measurementTasks
                on session.Id equals task.ExperimentSessionId
                into taskGroup

            from task in taskGroup.DefaultIfEmpty()

            join survey in surveys
                on session.Id equals survey.ExperimentSessionId
                into surveyGroup

            from survey in surveyGroup.DefaultIfEmpty()

            orderby session.Id descending

            select new ExperimentResultViewModel
            {
                SessionId = session.Id,
                Mode = session.Mode,
                Phase = session.Phase,
                AdaptationApplied = session.AdaptationApplied,
                AdaptationAccepted = session.AdaptationAccepted,
                CreatedAtUtc = session.CreatedAtUtc,
                CompletedAtUtc = session.CompletedAtUtc,

                MeasurementTaskId = task == null
                    ? (int?)null
                    : task.Id,

                ExerciseId = task == null
                    ? (int?)null
                    : task.ExerciseId,

                SubmitMoved = task == null
                    ? (bool?)null
                    : task.SubmitMoved,

                TaskStartedAtUtc = task == null
                    ? (DateTime?)null
                    : task.StartedAtUtc,

                TaskCompletedAtUtc = task == null
                    ? (DateTime?)null
                    : task.CompletedAtUtc,

                DurationMilliseconds = task == null
                    ? (long?)null
                    : task.DurationMilliseconds,

                TimeToSubmitMilliseconds = task == null
                    ? (long?)null
                    : task.TimeToSubmitMilliseconds,

                OldSubmitLocationClickCount = task == null
                    ? (int?)null
                    : task.OldSubmitLocationClickCount,

                IncompleteSubmitCount = task == null
                    ? (int?)null
                    : task.IncompleteSubmitCount,

                PredictabilityRating = survey == null
                    ? (int?)null
                    : survey.PredictabilityRating,

                SubmitFindabilityRating = survey == null
                    ? (int?)null
                    : survey.SubmitFindabilityRating,

                LayoutSatisfactionRating = survey == null
                    ? (int?)null
                    : survey.LayoutSatisfactionRating,

                OverallSatisfactionRating = survey == null
                    ? (int?)null
                    : survey.OverallSatisfactionRating,

                Comment = survey == null
                    ? null
                    : survey.Comment,

                SurveySubmittedAtUtc = survey == null
                    ? (DateTime?)null
                    : survey.SubmittedAtUtc
            };

        return await query.ToListAsync(cancellationToken);
    }
}