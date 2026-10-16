namespace PetPing.Windows.Services;

/// <summary>
/// Screen-coordinate independent motion planner. A pet-sized WPF layered window
/// starts by either edge and ends with its center in the active monitor's middle zone.
/// Values are WPF device-independent pixels, not a raw physical-pixel screen size.
/// </summary>
public static class PetWalkRules
{
    public const int DefaultRepeatSeconds = 60;
    public static (double Start, double End) Plan(
        double workLeft, double workWidth, double actorWidth, bool fromLeft)
    {
        if (!double.IsFinite(workLeft) || !double.IsFinite(workWidth) ||
            !double.IsFinite(actorWidth) || workWidth <= 0 || actorWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(workWidth), "Screen and actor must have finite positive sizes.");

        var maximum = workLeft + Math.Max(0, workWidth - actorWidth);
        var end = Math.Clamp(workLeft + (workWidth - actorWidth) / 2.0,
            workLeft, maximum);
        var start = fromLeft ? workLeft : maximum;
        return (start, end);
    }

    public static bool IsVoiceRepeatAllowed(
        bool enabled, bool muted, bool locked, bool visible, bool awaitingResponse) =>
        enabled && !muted && !locked && visible && awaitingResponse;

    public static DateTimeOffset NextSpeechAt(DateTimeOffset speechFinishedAt, int repeatSeconds)
    {
        if (repeatSeconds < 15 || repeatSeconds > 3600)
            throw new ArgumentOutOfRangeException(nameof(repeatSeconds));
        return speechFinishedAt.AddSeconds(repeatSeconds);
    }
}
