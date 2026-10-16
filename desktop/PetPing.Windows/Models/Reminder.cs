namespace PetPing.Windows.Models;

public enum ReminderStatus { Pending, Delivered, Completed, Dismissed, Missed }

public sealed class Reminder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public DateTimeOffset DueAt { get; set; }
    public DateTimeOffset OriginalDueAt { get; set; }
    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public int SnoozeCount { get; set; }

    public string DueLabel => DueAt.ToLocalTime().ToString("ddd, dd MMM yyyy • hh:mm tt");
    public string StatusLabel => Status switch
    {
        ReminderStatus.Pending => "Upcoming",
        ReminderStatus.Delivered => "Needs your attention",
        ReminderStatus.Completed => "Done",
        ReminderStatus.Dismissed => "Dismissed",
        ReminderStatus.Missed => "Missed while app was unavailable",
        _ => "Unknown"
    };
}
