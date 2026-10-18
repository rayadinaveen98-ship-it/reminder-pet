using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PetPing.Windows.Services;

namespace PetPing.Windows;

/// <summary>
/// Optional cat-only artwork renderer. Loads user-approved PNGs from the
/// separate experimental profile (or a portable sidecar folder); never from
/// v0.17's profile. If missing/corrupt, the legacy WPF cat still renders.
/// Does not load GIF/WebP codecs or allocate bitmaps on animation ticks.
/// </summary>
public partial class CatIdleSprite : System.Windows.Controls.UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _clock = new();
    private readonly Dictionary<string, BitmapImage> _frames = [];
    private readonly Dictionary<int, BitmapImage> _walkFrames = [];
    private BitmapImage? _talkRestFrame;
    private bool _walking;
    private CatStage _stage = CatStage.Stopped;
    public CatStage Stage => _stage;
    private PetMotion _motion = PetMotion.Full;
    private bool _speaking;
    private long _lastSpokenWordAt = -1;
    private int _spokenWords;
    private bool _done;
    private bool _snooze;
    private string _current = "";
    public bool Available => _frames.Count == 4;
    public bool WalkAvailable => _walkFrames.Count == CatWalkPlaybackRules.FrameCount;
    public bool TalkAvailable => _talkRestFrame is not null;

    public CatIdleSprite()
    {
        InitializeComponent();
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(83)
        };
        _timer.Tick += (_, _) => Tick();
        IsVisibleChanged += (_, _) => { if (IsVisible) Start(); else Pause(); };
        Unloaded += (_, _) => Pause();
    }

    public bool TryLoad()
    {
        if (Available) return true;
        // An explicit installed sidecar folder is safer than packaging
        // experimental PNGs inside the signed/stable WPF asset bundle.
        var roots = new[]
        {
            // Prefer the complete release-matched art beside the executable;
            // an older 4-image profile sidecar must not mask new Walking art.
            Path.Combine(AppContext.BaseDirectory, "Assets", "CatIdle"),
            Path.Combine(PetPingDataRoot.DefaultDirectory, "CatIdle")
        };
        foreach (var folder in roots)
        {
            if (!Directory.Exists(folder)) continue;
            try
            {
                var loaded = new Dictionary<string, BitmapImage>();
                foreach (var key in new[] { "open", "half", "closed", "happy" })
                {
                    var file = Path.Combine(folder, $"Cat_Idle_{key}_164x218.png");
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(file, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    if (bitmap.PixelWidth != 164 || bitmap.PixelHeight != 218)
                        throw new InvalidDataException("Cat idle sprite dimensions do not match.");
                    bitmap.Freeze();
                    loaded.Add(key, bitmap);
                }
                _frames.Clear();
                foreach (var (key, frame) in loaded) _frames.Add(key, frame);
                // Walking is disabled for the final static pet experience.
                // Do not load old walking sprite files left in prior profiles.
                LoadTalkingFrame(folder);
                UpdateImage("open");
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                or NotSupportedException or InvalidOperationException
                or System.Runtime.InteropServices.COMException or ArgumentException
                or FormatException)
            {
                _frames.Clear();
                _current = "";
            }
        }
        return false;
    }

    private void LoadTalkingFrame(string folder)
    {
        try
        {
            var file = Path.Combine(folder, "Cat_Talk_rest_164x218.png");
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(file, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            if (bitmap.PixelWidth != 164 || bitmap.PixelHeight != 218)
                throw new InvalidDataException("Talking sprite dimensions do not match.");
            bitmap.Freeze();
            _talkRestFrame = bitmap;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException
            or NotSupportedException or InvalidOperationException
            or System.Runtime.InteropServices.COMException or ArgumentException
            or FormatException)
        {
            _talkRestFrame = null; // older 8-sprite packs remain functional
        }
    }

    private void LoadWalkingFrames(string folder)
    {
        // Walk art is optional: full idle quality and old vector walking still
        // work when a user has only the four original idle PNGs.
        var frames = new Dictionary<int, BitmapImage>();
        try
        {
            for (var i = 0; i < CatWalkPlaybackRules.FrameCount; i++)
            {
                var path = Path.Combine(folder, $"Cat_Walk_{i}_164x218.png");
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                if (bitmap.PixelWidth != 164 || bitmap.PixelHeight != 218)
                    throw new InvalidDataException("Walking sprite dimensions do not match.");
                bitmap.Freeze();
                frames.Add(i, bitmap);
            }
            _walkFrames.Clear();
            foreach (var (key, image) in frames) _walkFrames.Add(key, image);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException
            or NotSupportedException or InvalidOperationException
            or System.Runtime.InteropServices.COMException or ArgumentException
            or FormatException)
        {
            _walkFrames.Clear();
        }
    }

    public bool SetWalking(bool walking)
    {
        if (walking && !WalkAvailable) return false;
        var next = CatAnimationFlowRules.Next(_stage,
            walking ? CatStageEvent.BeginWalk : CatStageEvent.Arrive);
        if (walking && next != CatStage.Walking) return false;
        if (!walking && next != CatStage.Idle) return false;
        _stage = next;
        _walking = next == CatStage.Walking;
        _done = _snooze = _speaking = false;
        _spokenWords = 0;
        _lastSpokenWordAt = -1;
        _clock.Restart();
        if (IsVisible && _motion != PetMotion.Off) _timer.Start();
        Tick();
        return true;
    }

    public void SetMotion(PetMotion motion)
    {
        _motion = motion;
        if (motion == PetMotion.Off) Pause();
        else if (IsVisible) Start();
        Tick();
    }

    public void SetSpeaking(bool speaking)
    {
        var next = CatAnimationFlowRules.Next(_stage,
            speaking ? CatStageEvent.SpeechStarted : CatStageEvent.SpeechStopped);
        if (next == _stage) return;
        _stage = next;
        _speaking = next == CatStage.Talking;
        _spokenWords = 0;
        _lastSpokenWordAt = -1;
        if (_speaking && !_clock.IsRunning) _clock.Start();
        Tick();
    }

    /// <summary>
    /// Called on the dispatcher when the active Windows TTS voice starts a
    /// word. This is not a phoneme / viseme callback.
    /// </summary>
    public void OnSpeechWord()
    {
        if (!Available || !CatAnimationFlowRules.CanAnimateSpeech(_stage)) return;
        _lastSpokenWordAt = _clock.ElapsedMilliseconds;
        _spokenWords++;
        Tick();
    }

    public void SetResponse(bool completed)
    {
        var next = CatAnimationFlowRules.Next(_stage,
            completed ? CatStageEvent.Done : CatStageEvent.Snooze);
        if (next == _stage) return;
        _stage = next;
        _walking = _speaking = false;
        _spokenWords = 0;
        _lastSpokenWordAt = -1;
        _done = next == CatStage.Done;
        _snooze = next == CatStage.Snoozed;
        Pause();
        Tick();
    }

    public void SetIdle()
    {
        _stage = CatAnimationFlowRules.Next(_stage, CatStageEvent.Reset);
        _walking = false;
        _spokenWords = 0;
        _lastSpokenWordAt = -1;
        _speaking = _done = _snooze = false;
        _current = "";
        IdleShift.Y = 0;
        BreathingScale.ScaleX = BreathingScale.ScaleY = 1;
        if (IsVisible && _motion != PetMotion.Off) Start();
        Tick();
    }

    public void Start()
    {
        if (!Available || CatAnimationFlowRules.IsTerminal(_stage) ||
            _motion == PetMotion.Off || _timer.IsEnabled ||
            !IsVisible || _done || _snooze) return;
        _clock.Restart();
        _timer.Start();
        Tick();
    }

    public void Pause()
    {
        _timer.Stop();
        _clock.Stop();
    }

    public void StopAnimation()
    {
        _stage = CatAnimationFlowRules.Next(_stage, CatStageEvent.Stop);
        _walking = _speaking = _done = _snooze = false;
        _lastSpokenWordAt = -1;
        _spokenWords = 0;
        Pause();
    }

    private void Tick()
    {
        if (!Available) return;
        var elapsed = _clock.ElapsedMilliseconds;
        if (_walking && WalkAvailable)
            UpdateWalkImage(CatWalkPlaybackRules.FrameAt(elapsed, _motion));
        else if (_speaking)
            UpdateImage(CatTalkPlaybackRules.FrameAt(elapsed, _lastSpokenWordAt,
                _spokenWords, _motion));
        else
            UpdateImage(CatIdlePlaybackRules.FrameAt(elapsed, _motion, false, _done, _snooze));
        var breathe = _walking || _speaking || _done || _snooze
            ? 1.0 : CatIdlePlaybackRules.BreathingScale(elapsed, _motion);
        BreathingScale.ScaleX = BreathingScale.ScaleY = breathe;
        IdleShift.Y = _motion == PetMotion.Full && !_walking && !_speaking && !_done && !_snooze
            ? Math.Sin(elapsed / 3200.0 * 2 * Math.PI) * 1.5 : 0;
    }

    private void UpdateWalkImage(int index)
    {
        var key = "walk" + index;
        if (_current == key) return;
        if (_walkFrames.TryGetValue(index, out var image))
        {
            CatFrame.Source = image;
            _current = key;
        }
    }

    private void UpdateImage(string key)
    {
        if (_current == key) return;
        if (key == "rest")
        {
            CatFrame.Source = _talkRestFrame ?? _frames.GetValueOrDefault("half");
            _current = key;
        }
        else if (_frames.TryGetValue(key, out var image))
        {
            CatFrame.Source = image;
            _current = key;
        }
    }
}
