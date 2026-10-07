using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Web.Services;

public class ExperimentTaskService(ApplicationDbContext db)
{
    public async Task<ExperimentTask?> StartOrResumeAsync(
        string userId,
        int exerciseId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var session = await db.ExperimentSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.UserId == userId,
                cancellationToken);

        // Người chưa tham gia trải nghiệm vẫn có thể làm quiz.
        if (session is null)
        {
            return null;
        }

        if (session.Phase is ExperimentPhase.Adaptation
            or ExperimentPhase.Completed)
        {
            throw new InvalidOperationException(
                "Phiên hiện tại chưa cho phép bắt đầu bài kiểm tra.");
        }

        var phase = session.Phase;

        var tasks = await db.ExperimentTasks
            .AsNoTracking()
            .Where(task =>
                task.ExperimentSessionId == session.Id &&
                task.Phase == phase)
            .OrderBy(task => task.TaskNumber)
            .ToListAsync(cancellationToken);

        var unfinishedTask = tasks.FirstOrDefault(
            task => task.CompletedAtUtc is null);

        if (unfinishedTask is not null)
        {
            if (unfinishedTask.ExerciseId != exerciseId)
            {
                throw new InvalidOperationException(
                    "Hãy hoàn thành bài đang làm trước khi mở bài khác.");
            }

            return unfinishedTask;
        }

        var taskLimit =
            phase == ExperimentPhase.Familiarisation ? 2 : 1;

        var completedCount = tasks.Count(
            task => task.CompletedAtUtc.HasValue);

        if (completedCount >= taskLimit)
        {
            throw new InvalidOperationException(
                "Các tác vụ của giai đoạn này đã hoàn thành.");
        }

        var exerciseExists = await db.Exercises.AnyAsync(
            exercise => exercise.Id == exerciseId,
            cancellationToken);

        if (!exerciseExists)
        {
            throw new InvalidOperationException(
                "Không tìm thấy bài kiểm tra.");
        }

        var taskNumber = completedCount + 1;

        var newTask = new ExperimentTask
        {
            ExperimentSessionId = session.Id,
            ExerciseId = exerciseId,
            Phase = phase,
            TaskNumber = taskNumber,
            AttemptToken = Guid.NewGuid(),
            AdaptationApplied = session.AdaptationApplied,
            SubmitMoved =
                session.AdaptationApplied &&
                !ExperimentService.IsSubmitFixed(session.Mode),
            StartedAtUtc = DateTime.UtcNow
        };

        db.ExperimentTasks.Add(newTask);

        try
        {
            await db.SaveChangesAsync(cancellationToken);

            return newTask;
        }
        catch (DbUpdateException)
        {
            // Hai yêu cầu mở trang có thể cùng tạo một tác vụ.
            db.Entry(newTask).State = EntityState.Detached;

            var existingTask = await db.ExperimentTasks
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    task =>
                        task.ExperimentSessionId == session.Id &&
                        task.Phase == phase &&
                        task.TaskNumber == taskNumber,
                    cancellationToken);

            if (existingTask is null)
            {
                throw;
            }

            if (existingTask.ExerciseId != exerciseId)
            {
                throw new InvalidOperationException(
                    "Hãy hoàn thành bài đang làm trước khi mở bài khác.");
            }

            if (existingTask.CompletedAtUtc.HasValue)
            {
                throw new InvalidOperationException(
                    "Tác vụ này đã hoàn thành. " +
                    "Hãy quay lại phiên trải nghiệm.");
            }

            return existingTask;
        }
    }

    public async Task<ExperimentTask?> GetOwnedAsync(
        string userId,
        Guid attemptToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return await db.ExperimentTasks
            .AsNoTracking()
            .SingleOrDefaultAsync(
                task =>
                    task.AttemptToken == attemptToken &&
                    task.ExperimentSession.UserId == userId,
                cancellationToken);
    }

    public async Task CompleteAsync(
        string userId,
        Guid attemptToken,
        int quizAttemptId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        // Nếu controller đã mở transaction thì dùng transaction đó.
        await using var transaction =
            db.Database.CurrentTransaction is null
                ? await db.Database.BeginTransactionAsync(
                    cancellationToken)
                : null;

        var task = await GetOwnedAsync(
            userId,
            attemptToken,
            cancellationToken);

        if (task is null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy tác vụ của tài khoản này.");
        }

        if (task.CompletedAtUtc.HasValue)
        {
            if (task.QuizAttemptId != quizAttemptId)
            {
                throw new InvalidOperationException(
                    "Tác vụ đã được liên kết với một kết quả khác.");
            }

            // Vẫn kiểm tra chuyển giai đoạn khi gửi lại yêu cầu.
        }
        else
        {
            var validQuizAttempt = await db.QuizAttempts.AnyAsync(
                attempt =>
                    attempt.Id == quizAttemptId &&
                    attempt.UserId == userId &&
                    attempt.ExerciseId == task.ExerciseId,
                cancellationToken);

            if (!validQuizAttempt)
            {
                throw new InvalidOperationException(
                    "Kết quả quiz không thuộc tài khoản " +
                    "hoặc bài đang làm.");
            }

            var completedAt = DateTime.UtcNow;

            // Tổng thời gian từ khi bắt đầu tác vụ đến khi hoàn thành.
            var duration = Math.Max(
                0L,
                (long)(completedAt - task.StartedAtUtc)
                    .TotalMilliseconds);

            // Thời gian từ lần đầu chọn đủ đáp án đến khi hoàn thành.
            // Để null nếu chưa nhận được mốc thời gian từ trình duyệt.
            long? timeToSubmit =
                task.AllQuestionsAnsweredAtUtc.HasValue
                    ? Math.Max(
                        0L,
                        (long)(
                            completedAt -
                            task.AllQuestionsAnsweredAtUtc.Value)
                        .TotalMilliseconds)
                    : null;

            var updatedRows = await db.ExperimentTasks
                .Where(item =>
                    item.Id == task.Id &&
                    item.CompletedAtUtc == null &&
                    item.ExperimentSession.Phase == task.Phase)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            item => item.QuizAttemptId,
                            (int?)quizAttemptId)
                        .SetProperty(
                            item => item.CompletedAtUtc,
                            (DateTime?)completedAt)
                        .SetProperty(
                            item => item.DurationMilliseconds,
                            (long?)duration)
                        .SetProperty(
                            item => item.TimeToSubmitMilliseconds,
                            timeToSubmit),
                    cancellationToken);

            if (updatedRows == 0)
            {
                var latestTask = await GetOwnedAsync(
                    userId,
                    attemptToken,
                    cancellationToken);

                if (latestTask is null ||
                    !latestTask.CompletedAtUtc.HasValue ||
                    latestTask.QuizAttemptId != quizAttemptId)
                {
                    throw new InvalidOperationException(
                        "Giai đoạn đã thay đổi " +
                        "hoặc tác vụ đã được xử lý.");
                }
            }
        }

        var now = DateTime.UtcNow;

        if (task.Phase == ExperimentPhase.Familiarisation)
        {
            var completedCount = await db.ExperimentTasks
                .CountAsync(
                    item =>
                        item.ExperimentSessionId ==
                            task.ExperimentSessionId &&
                        item.Phase ==
                            ExperimentPhase.Familiarisation &&
                        item.CompletedAtUtc != null &&
                        item.QuizAttemptId != null,
                    cancellationToken);

            if (completedCount >= 2)
            {
                var session = await db.ExperimentSessions
                    .AsNoTracking()
                    .SingleAsync(
                        item =>
                            item.Id == task.ExperimentSessionId,
                        cancellationToken);

                var requiresApproval =
                    ExperimentService.RequiresApproval(session.Mode);

                await db.ExperimentSessions
                    .Where(item =>
                        item.Id == session.Id &&
                        item.Phase ==
                            ExperimentPhase.Familiarisation)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(
                                item => item.Phase,
                                requiresApproval
                                    ? ExperimentPhase.Adaptation
                                    : ExperimentPhase.Measurement)
                            .SetProperty(
                                item => item.AdaptationApplied,
                                !requiresApproval)
                            .SetProperty(
                                item => item.AdaptationAppliedAtUtc,
                                requiresApproval
                                    ? (DateTime?)null
                                    : now),
                        cancellationToken);
            }
        }
        else if (task.Phase == ExperimentPhase.Measurement)
        {
            await db.ExperimentSessions
                .Where(item =>
                    item.Id == task.ExperimentSessionId &&
                    item.Phase == ExperimentPhase.Measurement)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            item => item.Phase,
                            ExperimentPhase.Completed)
                        .SetProperty(
                            item => item.CompletedAtUtc,
                            (DateTime?)now),
                    cancellationToken);
        }

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }
}