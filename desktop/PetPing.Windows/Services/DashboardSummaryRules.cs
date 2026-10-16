using PetPing.Windows.Models;

namespace PetPing.Windows.Services;

public sealed record DashboardSnapshot(
    int TodayCount, int UpcomingCount, int AttentionCount,
    IReadOnlyList<Reminder> NextReminders);

/// <summary>
/// Read-only, deterministic UI projection. The redesigned Home never makes up
/// task totals or changes persisted reminder state.
/// </summary>
public static class DashboardSummaryRules
{
    public static DashboardSnapshot Build(IEnumerable<Reminder> reminders, DateTime today)
    {
        var all = reminders.ToArray();
        var next = all
            .Where(r => r.Status == ReminderStatus.Pending)
            .OrderBy(r => r.DueAt)
            .Take(3)
            .ToArray();
        return new DashboardSnapshot(
            ReminderInboxRules.Count(all, InboxFilter.Today, today),
            all.Count(r => r.Status == ReminderStatus.Pending),
            all.Count(ReminderRules.NeedsAttention),
            next);
    }
}
