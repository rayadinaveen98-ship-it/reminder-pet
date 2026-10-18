using System.Speech.Synthesis;

namespace PetPing.Windows.Services;

/// <summary>
/// Local Windows narrator. Tagged speech sessions prevent cancelled or stale
/// speech completions from starting another reminder's repeating voice loop.
/// </summary>
public sealed class VoiceService : IDisposable
{
    private readonly SpeechSynthesizer? _speaker;
    private readonly object _sync = new();
    private Prompt? _activePrompt;
    private Guid _activeSession;

    public bool Enabled { get; set; } = true;
    public bool IsAvailable => _speaker is not null;
    public event Action<Exception>? SpeechFailed;
    public event Action<bool>? SpeakingChanged;
    public event Action<Guid, bool>? SessionSpeakingChanged;
    // Pure timing cue: do not expose spoken reminder text in UI events.
    public event Action<Guid>? SessionWordStarted;
    public event Action<Guid>? SessionSpeechFinished;

    public VoiceService()
    {
        try
        {
            _speaker = new SpeechSynthesizer();
            _speaker.SetOutputToDefaultAudioDevice();
            _speaker.SpeakStarted += (_, args) =>
            {
                var session = CurrentSessionFor(args.Prompt);
                if (session is { } id)
                {
                    try { SpeakingChanged?.Invoke(true); } catch { }
                    try { SessionSpeakingChanged?.Invoke(id, true); } catch { }
                }
            };
            // SpeakProgress is emitted by the real Windows voice as each
            // word begins. Matching the currently active session excludes
            // cancelled / previous reminders and protects user privacy.
            _speaker.SpeakProgress += (_, args) =>
            {
                var session = CurrentSessionFor(args.Prompt);
                if (session is { } id)
                {
                    try { SessionWordStarted?.Invoke(id); } catch { }
                }
            };
            _speaker.SpeakCompleted += (_, args) =>
            {
                var session = CurrentSessionFor(args.Prompt);
                if (session is not { } id) return; // a cancelled earlier voice
                lock (_sync)
                {
                    if (ReferenceEquals(_activePrompt, args.Prompt))
                    {
                        _activePrompt = null;
                        _activeSession = Guid.Empty;
                    }
                }
                try { SpeakingChanged?.Invoke(false); } catch { }
                try { SessionSpeakingChanged?.Invoke(id, false); } catch { }
                if (args.Error is { } error)
                {
                    try { SpeechFailed?.Invoke(error); } catch { }
                }
                if (!args.Cancelled)
                {
                    try { SessionSpeechFinished?.Invoke(id); } catch { }
                }
            };
        }
        catch { _speaker = null; }
    }

    private Guid? CurrentSessionFor(Prompt? prompt)
    {
        lock (_sync)
            return prompt is not null && ReferenceEquals(prompt, _activePrompt) &&
                _activeSession != Guid.Empty ? _activeSession : null;
    }

    // Backwards-compatible preview/voice test call. UI may pass its pet session
    // ID explicitly for cancellation-safe repeat scheduling.
    public bool SpeakReminder(string title) => SpeakReminder(title, Guid.NewGuid());

    public bool SpeakReminder(string title, Guid sessionId)
    {
        if (!Enabled || _speaker is null || sessionId == Guid.Empty) return false;
        try
        {
            Stop();
            var prompt = new Prompt("Hey! It's reminder time. " + title);
            lock (_sync)
            {
                _activePrompt = prompt;
                _activeSession = sessionId;
            }
            _speaker.SpeakAsync(prompt);
            return true;
        }
        catch (Exception error)
        {
            Stop();
            try { SpeechFailed?.Invoke(error); } catch { }
            return false;
        }
    }

    public void Stop()
    {
        var previousSession = Guid.Empty;
        lock (_sync)
        {
            previousSession = _activeSession;
            _activeSession = Guid.Empty;
            _activePrompt = null;
        }

        try { _speaker?.SpeakAsyncCancelAll(); }
        catch (Exception error)
        {
            try { SpeechFailed?.Invoke(error); } catch { }
        }
        if (previousSession != Guid.Empty)
        {
            try { SpeakingChanged?.Invoke(false); } catch { }
            try { SessionSpeakingChanged?.Invoke(previousSession, false); } catch { }
        }
    }

    public void Dispose()
    {
        Stop();
        _speaker?.Dispose();
    }
}
