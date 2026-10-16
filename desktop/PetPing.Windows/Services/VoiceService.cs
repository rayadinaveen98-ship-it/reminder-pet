using System.Speech.Synthesis;

namespace PetPing.Windows.Services;

/// <summary>Optional local Windows TTS. Failure must not affect visual reminder delivery.</summary>
public sealed class VoiceService : IDisposable
{
    private readonly SpeechSynthesizer? _speaker;
    public bool Enabled { get; set; } = true;
    public bool IsAvailable => _speaker is not null;
    public event Action<Exception>? SpeechFailed;
    public event Action<bool>? SpeakingChanged;

    public VoiceService()
    {
        try
        {
            _speaker = new SpeechSynthesizer();
            _speaker.SetOutputToDefaultAudioDevice();
            _speaker.SpeakStarted += (_, _) =>
            {
                try { SpeakingChanged?.Invoke(true); } catch { }
            };
            _speaker.SpeakCompleted += (_, args) =>
            {
                try { SpeakingChanged?.Invoke(false); } catch { }
                if (args.Error is { } error)
                    try { SpeechFailed?.Invoke(error); } catch { }
            };

        }
        catch { _speaker = null; }
    }

    public bool SpeakReminder(string title)
    {
        if (!Enabled || _speaker is null) return false;
        try
        {
            _speaker.SpeakAsyncCancelAll();
            _speaker.SpeakAsync("Hey! It's reminder time. " + title);
            return true;
        }
        catch (Exception error)
        {
            try { SpeechFailed?.Invoke(error); } catch { }
            return false;
        }
    }

    public void Stop()
    {
        try { _speaker?.SpeakAsyncCancelAll(); }
        catch (Exception error)
        {
            try { SpeechFailed?.Invoke(error); } catch { }
        }
    }

    public void Dispose() => _speaker?.Dispose();
}
