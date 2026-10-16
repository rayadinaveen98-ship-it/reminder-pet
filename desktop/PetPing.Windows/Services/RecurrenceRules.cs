using PetPing.Windows.Models;

namespace PetPing.Windows.Services;

/// <summary>
/// The next *future* occurrence for a daily / same-weekday series. Time is
/// interpreted in the timezone chosen when the series was created, not an
/// assumed fixed 24/168-hour offset. After long downtime, skip intermediate
/// dates instead of generating an unbounded backlog.
/// </summary>
public static class RecurrenceRules
{
    public static DateTimeOffset NextAfter(Reminder current, DateTimeOffset now)
    {
        if (current.Repeat == RepeatPattern.None)
            throw new InvalidOperationException("A one-time reminder has no future occurrence.");
        if (current.Repeat is not (RepeatPattern.Daily or RepeatPattern.Weekly))
            throw new InvalidDataException("Unsupported recurrence pattern.");
        if (string.IsNullOrWhiteSpace(current.RepeatTimeZoneId))
            throw new InvalidDataException("Recurring reminder has no time zone.");

        var tz = TimeZoneInfo.FindSystemTimeZoneById(current.RepeatTimeZoneId);
        var sourceLocal = TimeZoneInfo.ConvertTime(current.OriginalDueAt, tz).DateTime;
        var wallClock = current.RepeatLocalTime ?? sourceLocal.TimeOfDay;
        if (wallClock < TimeSpan.Zero || wallClock >= TimeSpan.FromDays(1))
            throw new InvalidDataException("Recurring time of day is invalid.");

        var stepDays = current.Repeat == RepeatPattern.Daily ? 1 : 7;
        var candidate = DateTime.SpecifyKind(sourceLocal.Date.AddDays(stepDays).Add(wallClock),
            DateTimeKind.Unspecified);

        // Jump over long offline intervals without iterating every missed date.
        var localNow = TimeZoneInfo.ConvertTime(now, tz).DateTime;
        if (candidate.Date < localNow.Date)
        {
            var daysBehind = (localNow.Date - candidate.Date).Days;
            candidate = candidate.AddDays((daysBehind / stepDays) * stepDays);
        }

        for (int i = 0; i < 5; i++)
        {
            var due = ResolveWallTime(candidate, tz);
            if (due > now) return due;
            candidate = candidate.AddDays(stepDays);
        }
        throw new InvalidOperationException("Could not find next recurring reminder.");
    }

    public static DateTimeOffset ResolveWallTime(DateTime local, TimeZoneInfo tz)
    {
        local = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        // Spring DST gap: move forward to the first valid minute on this date.
        // Preserve the requested wall clock separately so the *following*
        // day/week is at the user's original chosen time.
        for (int i = 0; tz.IsInvalidTime(local) && i < 180; i++)
            local = local.AddMinutes(1);
        if (tz.IsInvalidTime(local))
            throw new InvalidDataException("No valid local time near recurrence.");
        var offset = tz.IsAmbiguousTime(local)
            ? tz.GetAmbiguousTimeOffsets(local).Max()
            : tz.GetUtcOffset(local);
        return new DateTimeOffset(local, offset);
    }
}
