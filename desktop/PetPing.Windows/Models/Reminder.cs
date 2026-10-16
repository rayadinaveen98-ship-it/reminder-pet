namespace PetPing.Windows.Models;

public enum ReminderStatus { Pending, Delivered, Completed, Dismissed, Missed }
public enum RepeatPattern { None, Daily, Weekly, Custom }

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

    // Additive schema: old v0.5 JSON without these fields loads as one-off reminders.
    public RepeatPattern Repeat { get; set; } = RepeatPattern.None;
    public Guid? SeriesId { get; set; }
    public string? RepeatTimeZoneId { get; set; }
    public TimeSpan? RepeatLocalTime { get; set; }
    // When an occurrence is snoozed, it must NOT generate the next series
    // occurrence a second time. Once true, it is persisted with the event.
    public bool RepeatContinuationCreated { get; set; }

    // Optional only for RepeatPattern.Custom; never inferred for old schedules.
    public CustomRepeatOptions? CustomSchedule { get; set; }

    public string DueLabel => DueAt.ToLocalTime().ToString("ddd, dd MMM yyyy • hh:mm tt");
    public string RepeatLabel => Repeat switch
    {
        RepeatPattern.Daily => "Every day",
        RepeatPattern.Weekly => "Every week",
        RepeatPattern.Custom when CustomSchedule?.Kind == CustomRepeatKind.Weekdays =>
            "Selected weekdays",
        RepeatPattern.Custom when CustomSchedule?.Kind == CustomRepeatKind.EveryNDays =>
            "Every N days",
        RepeatPattern.Custom => "Custom repeat",
        _ => "One time"
    };
    public string StatusLabel => Status switch
    {
        ReminderStatus.Pending => SnoozeCount > 0 ? "Snoozed" : "Upcoming",
        ReminderStatus.Delivered => "Needs your attention",
        ReminderStatus.Completed => "Done",
        ReminderStatus.Dismissed => "Dismissed",
        ReminderStatus.Missed => "Missed while app was unavailable",
        _ => "Unknown"
    };
}
