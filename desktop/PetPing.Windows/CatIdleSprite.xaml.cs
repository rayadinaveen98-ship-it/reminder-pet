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
public partial class CatIdleSprite : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _clock = new();
    private readonly Dictionary<string, BitmapImage> _frames = [];
    private PetMotion _motion = PetMotion.Full;
    private bool _speaking;
    private bool _done;
    private bool _snooze;
    private string _current = "";
    public bool Available => _frames.Count == 4;

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
            Path.Combine(PetPingDataRoot.DefaultDirectory, "CatIdle"),
            Path.Combine(AppContext.BaseDirectory, "Assets", "CatIdle")
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

    public void SetMotion(PetMotion motion)
    {
        _motion = motion;
        if (motion == PetMotion.Off) Pause();
        else if (IsVisible) Start();
        Tick();
    }

    public void SetSpeaking(bool speaking)
    {
        _speaking = speaking;
        Tick();
    }

    public void SetResponse(bool completed)
    {
        _done = completed;
        _snooze = !completed;
        Pause();
        Tick();
    }

    public void SetIdle()
    {
        _speaking = _done = _snooze = false;
        _current = "";
        IdleShift.Y = 0;
        BreathingScale.ScaleX = BreathingScale.ScaleY = 1;
        if (IsVisible && _motion != PetMotion.Off) Start();
        Tick();
    }

    public void Start()
    {
        if (!Available || _motion == PetMotion.Off || _timer.IsEnabled ||
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

    private void Tick()
    {
        if (!Available) return;
        var elapsed = _clock.ElapsedMilliseconds;
        UpdateImage(CatIdlePlaybackRules.FrameAt(elapsed, _motion, _speaking, _done, _snooze));
        var breathe = _speaking || _done || _snooze
            ? 1.0 : CatIdlePlaybackRules.BreathingScale(elapsed, _motion);
        BreathingScale.ScaleX = BreathingScale.ScaleY = breathe;
        IdleShift.Y = _motion == PetMotion.Full && !_speaking && !_done && !_snooze
            ? Math.Sin(elapsed / 3200.0 * 2 * Math.PI) * 1.5 : 0;
    }

    private void UpdateImage(string key)
    {
        if (_current == key) return;
        if (_frames.TryGetValue(key, out var image))
        {
            CatFrame.Source = image;
            _current = key;
        }
    }
}
