using System.Security.Cryptography;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Web.Services;

public class ExperimentService(ApplicationDbContext db)
{
    public async Task<ExperimentSession?> GetAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return await db.ExperimentSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                session => session.UserId == userId,
                cancellationToken);
    }

    public async Task<ExperimentSession> GetOrCreateAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        // Nếu đã có phiên thì giữ nguyên nhóm và tiến trình.
        var existing = await GetAsync(
            userId,
            cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var userExists = await db.Users
            .AnyAsync(
                user => user.Id == userId,
                cancellationToken);

        if (!userExists)
        {
            throw new InvalidOperationException(
                "Không tìm thấy tài khoản người dùng.");
        }

        ExperimentMode[] modes =
        [
            ExperimentMode.A,
            ExperimentMode.B,
            ExperimentMode.C,
            ExperimentMode.D
        ];

        var session = new ExperimentSession
        {
            UserId = userId,
            Mode = modes[
                RandomNumberGenerator.GetInt32(modes.Length)],
            Phase = ExperimentPhase.Familiarisation,
            AdaptationApplied = false,
            AdaptationAccepted = null,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.ExperimentSessions.Add(session);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return session;
        }
        catch (DbUpdateException)
        {
            // Xử lý trường hợp hai yêu cầu cùng tạo phiên
            // cho một tài khoản tại cùng thời điểm.
            db.Entry(session).State = EntityState.Detached;

            var createdByAnotherRequest = await GetAsync(
                userId,
                cancellationToken);

            if (createdByAnotherRequest is not null)
            {
                return createdByAnotherRequest;
            }

            // Nếu không phải trùng phiên, giữ nguyên lỗi database.
            throw;
        }
    }

    public static bool RequiresApproval(
        ExperimentMode mode)
    {
        return mode is
            ExperimentMode.B or ExperimentMode.D;
    }

    public static bool IsSubmitFixed(
        ExperimentMode mode)
    {
        return mode is
            ExperimentMode.C or ExperimentMode.D;
    }
    public async Task TriggerAdaptationAsync(
    string userId,
    CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var session = await GetAsync(userId, cancellationToken);

        if (session is null)
        {
            throw new InvalidOperationException(
                "Bạn chưa tạo phiên trải nghiệm.");
        }

        if (session.Phase != ExperimentPhase.Familiarisation)
        {
            return;
        }

        bool needsApproval = RequiresApproval(session.Mode);
        var now = DateTime.UtcNow;

        // Chỉ chuyển trạng thái nếu phiên vẫn đang ở giai đoạn làm quen.
        await db.ExperimentSessions
            .Where(item =>
                item.UserId == userId &&
                item.Phase == ExperimentPhase.Familiarisation)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        item => item.Phase,
                        needsApproval
                            ? ExperimentPhase.Adaptation
                            : ExperimentPhase.Measurement)
                    .SetProperty(
                        item => item.AdaptationApplied,
                        !needsApproval)
                    .SetProperty(
                        item => item.AdaptationAppliedAtUtc,
                        needsApproval ? (DateTime?)null : now),
                cancellationToken);
    }

    public async Task DecideAdaptationAsync(
        string userId,
        bool accepted,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var now = DateTime.UtcNow;

        // Chỉ B/D đang chờ quyết định mới được Accept/Reject.
        // Quyết định đầu tiên được giữ nguyên nếu gửi lại form.
        await db.ExperimentSessions
            .Where(item =>
                item.UserId == userId &&
                item.Phase == ExperimentPhase.Adaptation &&
                item.AdaptationAccepted == null &&
                (item.Mode == ExperimentMode.B ||
                 item.Mode == ExperimentMode.D))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        item => item.AdaptationAccepted,
                        (bool?)accepted)
                    .SetProperty(
                        item => item.AdaptationApplied,
                        accepted)
                    .SetProperty(
                        item => item.AdaptationAppliedAtUtc,
                        accepted ? (DateTime?)now : null)
                    .SetProperty(
                        item => item.Phase,
                        ExperimentPhase.Measurement),
                cancellationToken);
    }
}