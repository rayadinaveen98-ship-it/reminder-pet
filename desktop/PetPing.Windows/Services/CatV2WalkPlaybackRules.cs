namespace PetPing.Windows.Services;

/// <summary>
/// Playback index for the user-visible Cat V2 32-frame articulated gait STUDY.
/// This selects existing authored PNGs; the separate CatRiggedGaitRules owns
/// research-only IK calculations and is not a substitute for real 3D artwork.
/// </summary>
public static class CatV2WalkPlaybackRules
{
    public const int FrameCount = 32;
    public const int FullCycleMilliseconds = 1000;
    public const int GentleCycleMilliseconds = 1400;
    public const int SourceWidth = 640;
    public const int SourceHeight = 850;
    public const int PreviewDecodeHeight = 218;

    public static int FrameAt(long elapsedMilliseconds, PetMotion motion)
    {
        if (!Enum.IsDefined(motion)) throw new ArgumentOutOfRangeException(nameof(motion));
        if (motion == PetMotion.Off) return 0;
        int cycle = motion == PetMotion.Gentle
            ? GentleCycleMilliseconds : FullCycleMilliseconds;
        // Normalize before multiplying to avoid overflow on long-running app.
        long phase = ((elapsedMilliseconds % cycle) + cycle) % cycle;
        return (int)(phase * FrameCount / cycle);
    }
}
