using PetPing.Windows.Models;

namespace PetPing.Windows.Services;

/// <summary>
/// UI-only reminder collection filters. Never changes scheduling or stored states.
/// Calling code supplies the local current date to keep tests deterministic.
/// </summary>
public enum InboxFilter { Today, Upcoming, NeedsAttention, History, All }

public static class ReminderInboxRules
{
    public static IReadOnlyList<Reminder> Select(
        IEnumerable<Reminder> reminders, InboxFilter filter, DateTime today)
    {
        var day = today.Date;
        var selected = reminders.Where(r => filter switch
        {
            InboxFilter.Today =>
                r.Status is ReminderStatus.Pending or ReminderStatus.Delivered or ReminderStatus.Missed &&
                r.DueAt.ToLocalTime().Date == day,
            InboxFilter.Upcoming => r.Status == ReminderStatus.Pending,
            InboxFilter.NeedsAttention => ReminderRules.NeedsAttention(r),
            InboxFilter.History => r.Status is ReminderStatus.Completed or ReminderStatus.Dismissed,
            _ => true
        });

        return selected
            .OrderBy(r => filter is InboxFilter.All
                ? (ReminderRules.NeedsAttention(r) ? 0 :
                   r.Status == ReminderStatus.Pending ? 1 : 2)
                : 0)
            .ThenBy(r => filter == InboxFilter.History ? -(r.FinishedAt ?? r.DueAt).UtcTicks
                : r.DueAt.UtcTicks)
            .ToArray();
    }

    public static int Count(IEnumerable<Reminder> reminders,
        InboxFilter filter, DateTime today) => Select(reminders, filter, today).Count;
}
