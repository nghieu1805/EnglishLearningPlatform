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

        // Giữ nguyên nhóm và tiến trình nếu đã có phiên.
        var existing = await GetAsync(
            userId,
            cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        ExperimentMode[] modes =
        [
            ExperimentMode.A,
            ExperimentMode.B,
            ExperimentMode.C,
            ExperimentMode.D
        ];

        var mode = modes[
            RandomNumberGenerator.GetInt32(modes.Length)];

        return await CreateSessionAsync(
            userId,
            mode,
            cancellationToken);
    }

    public async Task<ExperimentSession> CreateTestSessionAsync(
        string userId,
        ExperimentMode mode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        if (!Enum.IsDefined(typeof(ExperimentMode), mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        // Không thay đổi nhóm của tài khoản đã có phiên.
        var existing = await GetAsync(
            userId,
            cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        return await CreateSessionAsync(
            userId,
            mode,
            cancellationToken);
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

        var session = await GetAsync(
            userId,
            cancellationToken);

        if (session is null)
        {
            throw new InvalidOperationException(
                "Bạn chưa tạo phiên trải nghiệm.");
        }

        if (session.Phase != ExperimentPhase.Familiarisation)
        {
            return;
        }

        var needsApproval = RequiresApproval(session.Mode);
        var now = DateTime.UtcNow;

        // Dùng cho nút kích hoạt thử trong Development.
        // Chỉ chuyển trạng thái nếu phiên vẫn đang làm quen.
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
                        needsApproval
                            ? (DateTime?)null
                            : now),
                cancellationToken);
    }

    public async Task DecideAdaptationAsync(
        string userId,
        bool accepted,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var now = DateTime.UtcNow;

        // Chỉ nhóm B/D đang chờ quyết định được Accept/Reject.
        // Giữ nguyên quyết định đầu tiên nếu gửi lại form.
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
                        accepted
                            ? (DateTime?)now
                            : null)
                    .SetProperty(
                        item => item.Phase,
                        ExperimentPhase.Measurement),
                cancellationToken);
    }

    private async Task<ExperimentSession> CreateSessionAsync(
        string userId,
        ExperimentMode mode,
        CancellationToken cancellationToken)
    {
        var userExists = await db.Users.AnyAsync(
            user => user.Id == userId,
            cancellationToken);

        if (!userExists)
        {
            throw new InvalidOperationException(
                "Không tìm thấy tài khoản người dùng.");
        }

        var session = new ExperimentSession
        {
            UserId = userId,
            Mode = mode,
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
            // Hai yêu cầu đồng thời có thể cùng tạo phiên.
            db.Entry(session).State = EntityState.Detached;

            var concurrentSession = await GetAsync(
                userId,
                cancellationToken);

            if (concurrentSession is not null)
            {
                return concurrentSession;
            }

            // Giữ nguyên lỗi nếu không có phiên được tạo đồng thời.
            throw;
        }
    }
}