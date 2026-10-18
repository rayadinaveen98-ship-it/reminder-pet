namespace PetPing.Windows.Services;

/// <summary>
/// Word-boundary cues from local Windows speech (System.Speech SpeakProgress)
/// drive approved cat facial frames. The renderer never claims phoneme-level
/// lip synchronization: Windows TTS doesn't supply our rig with visemes.
/// An animation-clock fallback still moves the mouth if progress events are
/// unsupported by an installed speech voice.
/// </summary>
public static class CatTalkPlaybackRules
{
    public const int WordCueWindowMs = 480;
    public const int FallbackCycleMs = 420;

    public static string FrameAt(long elapsedMs, long lastWordAtMs,
        int wordCounter, PetMotion motion)
    {
        if (!Enum.IsDefined(motion)) throw new ArgumentOutOfRangeException(nameof(motion));
        if (motion == PetMotion.Off) return "open";
        if (elapsedMs < 0) elapsedMs = 0;

        // Cue pacing only follows real word starts; if no cues arrive,
        // gracefully fall back to a low-frequency mouth cycle while speaking.
        if (lastWordAtMs >= 0 && lastWordAtMs <= elapsedMs &&
            elapsedMs - lastWordAtMs < WordCueWindowMs)
        {
            var dt = elapsedMs - lastWordAtMs;
            if (dt < 135) return "open";
            if (dt < 255) return "half";
            if (motion == PetMotion.Full && dt < 355 && wordCounter % 3 == 0)
                return "happy";
            if (dt < 355) return "open";
            return "half";
        }

        var phase = (elapsedMs % FallbackCycleMs + FallbackCycleMs) % FallbackCycleMs;
        if (motion == PetMotion.Gentle) return phase < 245 ? "open" : "half";
        return phase < 175 ? "open" : phase < 320 ? "half" : "open";
    }
}
