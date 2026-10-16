using PetPing.Windows.Models;

namespace PetPing.Windows.Services;

public enum ReminderDeliveryDecision { None, Deliver, Missed }

public static class ReminderRules
{
    public static readonly TimeSpan CatchUpWindow = TimeSpan.FromMinutes(15);

    public static ReminderDeliveryDecision Evaluate(Reminder reminder, DateTimeOffset now)
    {
        if (reminder.Status != ReminderStatus.Pending || reminder.DueAt > now)
            return ReminderDeliveryDecision.None;
        return now - reminder.DueAt <= CatchUpWindow
            ? ReminderDeliveryDecision.Deliver
            : ReminderDeliveryDecision.Missed;
    }

    public static bool NeedsAttention(Reminder reminder) =>
        reminder.Status is ReminderStatus.Delivered or ReminderStatus.Missed;

    public static bool CanSnooze(Reminder reminder) => NeedsAttention(reminder);

    public static bool ShouldRestoreAlert(Reminder reminder, DateTimeOffset now) =>
        reminder.Status == ReminderStatus.Delivered &&
        reminder.DeliveredAt is { } delivered &&
        delivered <= now &&
        now - delivered <= CatchUpWindow &&
        reminder.FinishedAt is null;
}
