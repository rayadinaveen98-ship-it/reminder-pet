using System.Speech.Synthesis;

namespace PetPing.Windows.Services;

public sealed class VoiceService : IDisposable
{
    private readonly SpeechSynthesizer? _speaker;
    public bool Enabled { get; set; } = true;

    public VoiceService()
    {
        try { _speaker = new SpeechSynthesizer(); _speaker.SetOutputToDefaultAudioDevice(); }
        catch { _speaker = null; }
    }

    public void SpeakReminder(string title)
    {
        if (!Enabled || _speaker is null) return;
        try
        {
            _speaker.SpeakAsyncCancelAll();
            _speaker.SpeakAsync("Hey! It's reminder time. " + title);
        }
        catch { /* Keep visible popup even if speech engine unavailable. */ }
    }

    public void Dispose() => _speaker?.Dispose();
}
