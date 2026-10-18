namespace PetPing.Windows.Services;

/// <summary>
/// Pure keyframe selection for the approved six-second cat idle prototype.
/// No WPF dependency, no random timers and no image allocations.
/// </summary>
public static class CatIdlePlaybackRules
{
    public const int LoopMilliseconds = 6000;
    public static string FrameAt(long elapsedMilliseconds, PetMotion motion,
        bool speaking = false, bool respondingDone = false, bool respondingSnooze = false)
    {
        if (respondingDone) return "happy";
        if (respondingSnooze) return "closed";
        if (speaking) return motion == PetMotion.Full &&
                elapsedMilliseconds / 190 % 2 == 0 ? "happy" : "open";
        if (motion == PetMotion.Off) return "open";
        var t = (int)((elapsedMilliseconds % LoopMilliseconds + LoopMilliseconds) %
            LoopMilliseconds);
        return In(t,1850,2020) || In(t,2180,2340) ||
               In(t,4750,4920) || In(t,5080,5230) ? "half"
            : In(t,2020,2180) || In(t,4920,5080) ? "closed"
            : motion == PetMotion.Full && In(t,5450,5740) ? "happy"
            : "open";
    }

    public static double BreathingScale(long elapsedMilliseconds, PetMotion motion)
    {
        if (motion != PetMotion.Full) return 1.0;
        var phase = Math.PI * 2 * (elapsedMilliseconds % 3200) / 3200;
        return 1.0 + 0.003 * (1 - Math.Cos(phase));
    }

    private static bool In(int value, int min, int max) =>
        value >= min && value < max;
}
