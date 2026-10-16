using System.Globalization;
using PetPing.Windows.Models;

namespace PetPing.Windows.Services;

/// <summary>
/// Shared reminder-editor rules. Validate the complete value before writing to
/// the local store. No UI dependency and no persisted schema changes.
/// </summary>
public static class ReminderEditorRules
{
    public const int MaxTitleLength = 120;
    public static readonly TimeSpan DefaultLeadTime = TimeSpan.FromMinutes(5);

    public static string ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Give your reminder a short name.");
        var trimmed = title.Trim();
        if (trimmed.Length > MaxTitleLength)
            throw new ArgumentException("Keep the reminder name within 120 characters.");
        return trimmed;
    }

    public static RepeatPattern SelectRepeat(int index) => index switch
    {
        0 => RepeatPattern.None,
        1 => RepeatPattern.Daily,
        2 => RepeatPattern.Weekly,
        _ => throw new ArgumentException("Choose a valid repeat option.")
    };

    public static DateTimeOffset DefaultLocalDue(DateTimeOffset now, TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        // Adding before converting is essential near midnight and DST changes.
        return TimeZoneInfo.ConvertTime(now.Add(DefaultLeadTime), timeZone);
    }

    public static DateTimeOffset ReadLocalDue(DateTime? date, string? clock,
        TimeZoneInfo timeZone, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        if (date is null)
            throw new ArgumentException("Choose a reminder date.");

        if (!TimeSpan.TryParseExact(clock?.Trim(), @"hh\:mm",
                CultureInfo.InvariantCulture, out var time) ||
            time < TimeSpan.Zero || time >= TimeSpan.FromDays(1))
            throw new ArgumentException("Use a 24-hour time like 09:30 or 19:00.");

        DateTime local;
        try
        {
            local = DateTime.SpecifyKind(date.Value.Date.Add(time), DateTimeKind.Unspecified);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new ArgumentException("Choose a valid reminder date and time.");
        }

        if (timeZone.IsInvalidTime(local))
            throw new ArgumentException("That time does not exist when the clocks change. Choose another.");
        if (timeZone.IsAmbiguousTime(local))
            throw new ArgumentException("That time occurs twice when the clocks change. Choose another.");

        var result = new DateTimeOffset(local, timeZone.GetUtcOffset(local));
        if (result <= now)
            throw new ArgumentException("Choose a future date and time.");

        return result;
    }
}
