namespace PetPing.Windows.Models;

/// <summary>
/// Additive v1 metadata for future custom recurring reminders only.
/// Never reinterpret a legacy Daily / Weekly reminder with these settings.
/// An unknown schema is rejected on load rather than silently discarded.
/// </summary>
public enum CustomRepeatKind { Weekdays, EveryNDays }

public sealed record CustomRepeatOptions
{
    public int SchemaVersion { get; init; } = 1;
    public CustomRepeatKind Kind { get; init; }
    // Unspecified local calendar date; time zone is on the Reminder.
    public DateTime AnchorLocalDate { get; init; }
    public int Interval { get; init; } = 1;
    // Monday bit 0, Tuesday bit 1 ... Sunday bit 6.
    public byte WeekdayMask { get; init; }

    public void Validate()
    {
        if (SchemaVersion != 1)
            throw new InvalidDataException("Custom repeat has an unsupported schema version.");
        if (!Enum.IsDefined(Kind))
            throw new InvalidDataException("Custom repeat has an unknown kind.");
        if (AnchorLocalDate == default || AnchorLocalDate.Kind != DateTimeKind.Unspecified ||
            AnchorLocalDate.TimeOfDay != TimeSpan.Zero)
            throw new InvalidDataException("Custom repeat needs a valid local anchor date.");
        if (Kind == CustomRepeatKind.Weekdays)
        {
            if (Interval is < 1 or > 52 || (WeekdayMask & 0x7f) == 0 ||
                (WeekdayMask & 0x80) != 0)
                throw new InvalidDataException("Choose 1–52 weeks and at least one valid weekday.");
        }
        else if (Interval is < 1 or > 365 || WeekdayMask != 0)
            throw new InvalidDataException("Choose a day interval from 1 to 365.");
    }

    public DayOfWeek[] SelectedWeekdays()
    {
        Validate();
        if (Kind != CustomRepeatKind.Weekdays) return [];
        var result = new List<DayOfWeek>(7);
        for (int mondayBit = 0; mondayBit < 7; mondayBit++)
            if ((WeekdayMask & (1 << mondayBit)) != 0)
                result.Add((DayOfWeek)((mondayBit + 1) % 7));
        return [.. result];
    }
}
