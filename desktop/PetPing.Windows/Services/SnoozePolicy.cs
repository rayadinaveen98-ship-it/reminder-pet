namespace PetPing.Windows.Services;

/// <summary>
/// Bounded local snooze scheduling. One minute through 24 hours; no timezone
/// or recurrence-series mutation. The user-selected delay starts at action time.
/// </summary>
public static class SnoozePolicy
{
    public const int MinMinutes = 1;
    public const int MaxMinutes = 24 * 60;
    public const int DefaultMinutes = 10;
    public static readonly int[] Presets = [5, 10, 15, 30, 60];

    public static TimeSpan FromMinutes(int minutes)
    {
        if (minutes < MinMinutes || minutes > MaxMinutes)
            throw new ArgumentOutOfRangeException(nameof(minutes),
                "Choose between 1 minute and 24 hours.");
        return TimeSpan.FromMinutes(minutes);
    }

    public static TimeSpan Validate(TimeSpan duration)
    {
        if (duration < TimeSpan.FromMinutes(MinMinutes) ||
            duration > TimeSpan.FromMinutes(MaxMinutes))
            throw new ArgumentOutOfRangeException(nameof(duration),
                "Snooze must be between 1 minute and 24 hours.");
        return duration;
    }
}
