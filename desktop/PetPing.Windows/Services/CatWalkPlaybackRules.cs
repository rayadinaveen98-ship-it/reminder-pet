namespace PetPing.Windows.Services;

/// <summary>
/// A four-frame 2.5D walk cycle made from a single approved walking pose.
/// Uses deterministic keyframes, no WPF dependency and no animation allocation.
/// Full motion: 600ms 4-frame cycle. Gentle: 1200ms slower 4-frame cycle.
/// Off: never animates (first frame). No independently rigged paws yet.
/// </summary>
public static class CatWalkPlaybackRules
{
    public const int FrameCount = 4;
    public const int FullFrameMilliseconds = 150;
    public const int GentleFrameMilliseconds = 300;

    public static int FrameAt(long elapsedMilliseconds, PetMotion motion)
    {
        if (!Enum.IsDefined(motion))
            throw new ArgumentOutOfRangeException(nameof(motion));
        if (motion == PetMotion.Off) return 0;
        var period = motion == PetMotion.Gentle
            ? GentleFrameMilliseconds : FullFrameMilliseconds;
        var step = elapsedMilliseconds / period;
        return (int)((step % FrameCount + FrameCount) % FrameCount);
    }
}
