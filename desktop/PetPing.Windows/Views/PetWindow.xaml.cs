using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using PetPing.Windows.Models;
using PetPing.Windows.Services;
using Forms = System.Windows.Forms;

namespace PetPing.Windows.Views;

public partial class PetWindow : Window
{
    private readonly Reminder _reminder;
    private readonly PetPlacement _placement;
    private readonly PetMotion _motion;
    private readonly DispatcherTimer _autoHide = new() { Interval = TimeSpan.FromSeconds(45) };
    private readonly DispatcherTimer _blink = new() { Interval = TimeSpan.FromSeconds(4.5) };
    private bool _closing;
    private bool _reacting;

    public Guid ReminderId => _reminder.Id;
    // Acknowledgment callbacks return false on persistence failure, so the pet
    // stays visible and the user can retry rather than believing the action saved.
    public event Func<Reminder, bool>? Completed;
    public event Func<Reminder, bool>? Snoozed;

    public PetWindow(Reminder reminder,
        PetPlacement placement = PetPlacement.Auto,
        PetMotion motion = PetMotion.Full)
    {
        InitializeComponent();
        _reminder = reminder;
        _placement = placement;
        _motion = motion;
        ReminderTitle.Text = reminder.Title;
        ReminderTitle.ToolTip = reminder.Title;
        SetAccessibilityNames();
        SourceInitialized += (_, _) => PositionForCurrentActivity();
        Loaded += OnPetLoaded;
        Closed += (_, _) =>
        {
            _autoHide.Stop();
            _blink.Stop();
        };
        _autoHide.Tick += (_, _) => AnimateAway();
        _blink.Tick += (_, _) => Blink();
    }

    private void SetAccessibilityNames()
    {
        System.Windows.Automation.AutomationProperties.SetName(
            ReminderTitle, "Reminder: " + _reminder.Title);
        System.Windows.Automation.AutomationProperties.SetHelpText(
            HideButton, "Closes this popup without completing the reminder.");
    }

    /// <summary>
    /// Works in WPF DIPs. WinForms screen working areas are physical pixels, so
    /// first convert the target screen boundaries using WPF's device transform.
    /// </summary>
    private void PositionForCurrentActivity()
    {
        var foreground = GetForegroundWindow();
        var screen = foreground == IntPtr.Zero
            ? Forms.Screen.PrimaryScreen
            : Forms.Screen.FromHandle(foreground);
        if (screen is null)
        {
            var fallback = SystemParameters.WorkArea;
            Left = fallback.Right - Width - 16;
            Top = fallback.Bottom - Height - 82;
            return;
        }
        var pixelArea = screen.WorkingArea;
        var transform = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice
            ?? Matrix.Identity;
        var topLeft = transform.Transform(new System.Windows.Point(pixelArea.Left, pixelArea.Top));
        var bottomRight = transform.Transform(new System.Windows.Point(pixelArea.Right, pixelArea.Bottom));
        var area = new Rect(topLeft, bottomRight);

        var full = IsLargeForegroundWindow(foreground, pixelArea);
        var choice = _placement == PetPlacement.Auto
            ? (full ? PetPlacement.TopRight : PetPlacement.BottomRight)
            : _placement;
        var leftSide = choice is PetPlacement.TopLeft or PetPlacement.BottomLeft;
        var topSide = choice is PetPlacement.TopRight or PetPlacement.TopLeft;

        Left = leftSide ? area.Left + 16 : area.Right - Width - 16;
        Top = topSide ? area.Top + 40 : area.Bottom - Height - 78;
        Left = Math.Clamp(Left, area.Left, Math.Max(area.Left, area.Right - Width));
        Top = Math.Clamp(Top, area.Top, Math.Max(area.Top, area.Bottom - Height));
    }

    private static bool IsLargeForegroundWindow(IntPtr hwnd, System.Drawing.Rectangle area)
    {
        if (hwnd == IntPtr.Zero || !GetWindowRect(hwnd, out var rect)) return false;
        return rect.Left <= area.Left + 25 &&
               rect.Top <= area.Top + 25 &&
               rect.Right >= area.Right - 25 &&
               rect.Bottom >= area.Bottom - 25;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left, Top, Right, Bottom;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hwnd, out NativeRect rect);

    private void OnPetLoaded(object sender, RoutedEventArgs e)
    {
        if (_motion == PetMotion.Off)
        {
            Opacity = 1;
            EntranceSlide.X = 0;
        }
        else
        {
            Opacity = 0;
            EntranceSlide.X = _motion == PetMotion.Full ? 80 : 0;
            if (_motion == PetMotion.Full)
            {
                var slide = new DoubleAnimation(80, 0, TimeSpan.FromMilliseconds(420))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                EntranceSlide.BeginAnimation(TranslateTransform.XProperty, slide);
            }
            BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(
                    _motion == PetMotion.Gentle ? 180 : 330)));

            if (_motion == PetMotion.Full)
            {
                PetFloat.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(0, -7, TimeSpan.FromSeconds(1.15))
                    {
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    });
                TailWag.BeginAnimation(RotateTransform.AngleProperty,
                    new DoubleAnimation(-6, 10, TimeSpan.FromSeconds(0.9))
                    {
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    });
            }
            _blink.Start();
        }
        _autoHide.Start();
    }

    private void Blink()
    {
        if (_motion == PetMotion.Off || _closing) return;
        var blink = new DoubleAnimation(1, 0.12, TimeSpan.FromMilliseconds(100))
        {
            AutoReverse = true
        };
        LeftBlink.BeginAnimation(ScaleTransform.ScaleYProperty, blink);
        RightBlink.BeginAnimation(ScaleTransform.ScaleYProperty, blink);
    }

    /// <summary>
    /// Called by the Windows speech engine's started/completed events. Speech
    /// is not guessed by a timer, and no microphone or cloud service is used.
    /// </summary>
    public void SetSpeaking(bool speaking)
    {
        if (_closing || _reacting || !IsLoaded) return;
        MouthSmile.Visibility = speaking ? Visibility.Collapsed : Visibility.Visible;
        MouthSpeak.Visibility = speaking ? Visibility.Visible : Visibility.Collapsed;
        MouthSpeak.BeginAnimation(OpacityProperty, null);
        if (speaking && _motion == PetMotion.Full)
        {
            MouthSpeak.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0.45,
                TimeSpan.FromMilliseconds(180))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            });
        }
    }

    private void OnPetMouseEnter(object sender, System.Windows.Input.MouseEventArgs e) =>
        _autoHide.Stop();

    private void OnPetMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_closing) return;
        _autoHide.Stop();
        _autoHide.Start();
    }

    private void AnimateAway()
    {
        if (_closing) return;
        _closing = true;
        _autoHide.Stop();
        _blink.Stop();
        if (_motion == PetMotion.Off)
        {
            Close();
            return;
        }
        var exit = new DoubleAnimation(Opacity, 0,
            TimeSpan.FromMilliseconds(_motion == PetMotion.Gentle ? 150 : 230));
        exit.Completed += (_, _) => Close();
        BeginAnimation(OpacityProperty, exit);
        if (_motion == PetMotion.Full)
            EntranceSlide.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(0, 45, TimeSpan.FromMilliseconds(230)));
    }

    private void OnHide(object sender, RoutedEventArgs e) => AnimateAway();

    private void ReactAndLeave(string response)
    {
        _reacting = true;
        _autoHide.Stop();
        _blink.Stop();
        ReminderTitle.Text = response;
        ReminderTitle.ToolTip = response;
        MouthSpeak.BeginAnimation(OpacityProperty, null);
        MouthSpeak.Visibility = Visibility.Collapsed;
        MouthSmile.Visibility = Visibility.Visible;
        var responseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(650) };
        responseTimer.Tick += (_, _) =>
        {
            responseTimer.Stop();
            AnimateAway();
        };
        responseTimer.Start();
    }

    private void OnDone(object sender, RoutedEventArgs e)
    {
        if (_closing || _reacting) return;
        if (Completed?.Invoke(_reminder) != false)
            ReactAndLeave("Nice work! ✓");
    }

    private void OnSnooze(object sender, RoutedEventArgs e)
    {
        if (_closing || _reacting) return;
        if (Snoozed?.Invoke(_reminder) != false)
            ReactAndLeave("Okay! I'll remind you again.");
    }
}
