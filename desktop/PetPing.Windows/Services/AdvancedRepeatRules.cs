namespace PetPing.Windows.Services;

/// <summary>
/// v0.18 R2-B pure preview calculations for custom recurring schedules.
/// Does not mutate Reminder, its persisted enum, or any occurrence history.
/// </summary>
public static class AdvancedRepeatRules
{
    public const int MaxDayInterval = 365;
    public const int MaxWeekInterval = 52;

    /// <summary>
    /// Select a day or several days each Nth week, with the week anchored
    /// on Monday of firstLocalDate. The chosen local wall time is retained
    /// across DST transitions, even if an individual date has a missing hour.
    /// </summary>
    public static DateTimeOffset NextWeekdayOccurrence(
        DateTime firstLocalDate, TimeSpan localTime,
        IEnumerable<DayOfWeek> weekdays, int everyNWeeks,
        TimeZoneInfo timeZone, DateTimeOffset after)
    {
        ArgumentNullException.ThrowIfNull(weekdays);
        Validate(timeZone, localTime);
        if (everyNWeeks < 1 || everyNWeeks > MaxWeekInterval)
            throw new ArgumentOutOfRangeException(nameof(everyNWeeks),
                "Choose a week interval between 1 and 52.");

        var offsets = weekdays.Distinct()
            .Select(day =>
            {
                if ((int)day is < 0 or > 6)
                    throw new ArgumentException("Choose valid weekdays.", nameof(weekdays));
                return MondayOffset(day);
            })
            .OrderBy(value => value).ToArray();
        if (offsets.Length == 0)
            throw new ArgumentException("Select at least one weekday.", nameof(weekdays));

        var anchor = DateTime.SpecifyKind(firstLocalDate.Date, DateTimeKind.Unspecified);
        var anchorWeek = anchor.AddDays(-MondayOffset(anchor.DayOfWeek));
        var today = TimeZoneInfo.ConvertTime(after, timeZone).Date;
        var elapsedWeeks = Math.Max(0, (today - anchorWeek).Days / 7);
        var eligibleBlock = (elapsedWeeks / everyNWeeks) * everyNWeeks;

        // At most one current interval-week and two future interval-weeks.
        // No looping over years of past missed reminders.
        for (int block = 0; block < 3; block++)
        {
            var weekStart = anchorWeek.AddDays(((long)eligibleBlock +
                ((long)block * everyNWeeks)) * 7);
            foreach (int dayOffset in offsets)
            {
                var date = weekStart.AddDays(dayOffset);
                if (date < anchor) continue;
                var due = ResolveDateAndTime(date, localTime, timeZone);
                if (due > after) return due;
            }
        }
        throw new InvalidOperationException("No future weekday occurrence could be calculated.");
    }

    /// <summary>Choose every N days from a fixed local anchor date.</summary>
    public static DateTimeOffset NextDayIntervalOccurrence(
        DateTime firstLocalDate, TimeSpan localTime,
        int everyNDays, TimeZoneInfo timeZone, DateTimeOffset after)
    {
        Validate(timeZone, localTime);
        if (everyNDays < 1 || everyNDays > MaxDayInterval)
            throw new ArgumentOutOfRangeException(nameof(everyNDays),
                "Choose a day interval between 1 and 365.");

        var anchor = DateTime.SpecifyKind(firstLocalDate.Date, DateTimeKind.Unspecified);
        var localToday = TimeZoneInfo.ConvertTime(after, timeZone).Date;
        var elapsed = Math.Max(0, (localToday - anchor).Days);
        var firstIndex = elapsed / everyNDays;

        for (int i = 0; i < 3; i++)
        {
            var date = anchor.AddDays(((long)firstIndex + i) * everyNDays);
            var due = ResolveDateAndTime(date, localTime, timeZone);
            if (due > after) return due;
        }
        throw new InvalidOperationException("No future interval occurrence could be calculated.");
    }

    private static void Validate(TimeZoneInfo timeZone, TimeSpan localTime)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        if (localTime < TimeSpan.Zero || localTime >= TimeSpan.FromDays(1))
            throw new ArgumentOutOfRangeException(nameof(localTime),
                "Time must be between 00:00 and 23:59.");
    }

    private static int MondayOffset(DayOfWeek day) => ((int)day + 6) % 7;

    private static DateTimeOffset ResolveDateAndTime(
        DateTime date, TimeSpan localTime, TimeZoneInfo timeZone)
    {
        var local = DateTime.SpecifyKind(date.Date.Add(localTime), DateTimeKind.Unspecified);
        // Reuses v0.17 daily/weekly DST policy: spring gaps slide to first
        // valid minute; fall overlap chooses the earlier instant.
        return RecurrenceRules.ResolveWallTime(local, timeZone);
    }
}
