using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using PetPing.Windows.Models;
using Forms = System.Windows.Forms;

namespace PetPing.Windows.Views;

public partial class PetWindow : Window
{
    private readonly Reminder _reminder;
    private readonly DispatcherTimer _autoHide = new() { Interval = TimeSpan.FromSeconds(45) };
    private bool _closing;

    public Guid ReminderId => _reminder.Id;
    public event Action<Reminder>? Completed;
    public event Action<Reminder>? Snoozed;

    public PetWindow(Reminder reminder)
    {
        InitializeComponent();
        _reminder = reminder;
        ReminderTitle.Text = reminder.Title;
        ReminderTitle.ToolTip = reminder.Title;
        AccessibilityHelper();
        SourceInitialized += (_, _) => PositionForCurrentActivity();
        Loaded += OnPetLoaded;
        Closed += (_, _) => _autoHide.Stop();
        _autoHide.Tick += (_, _) => AnimateAway();
    }

    private void AccessibilityHelper()
    {
        System.Windows.Automation.AutomationProperties.SetName(
            ReminderTitle, "Reminder: " + _reminder.Title);
        System.Windows.Automation.AutomationProperties.SetHelpText(
            HideButton, "Closes this popup without completing the reminder.");
    }

    private void PositionForCurrentActivity()
    {
        var work = SystemParameters.WorkArea;
        // While a window fills the screen, keep the pet near the top, clear of
        // playback timelines/subtitles and common video controls at the bottom.
        var nearFullScreen = ForegroundOccupiesScreen();
        Left = Math.Max(work.Left + 8, work.Right - Width - 16);
        Top = nearFullScreen
            ? work.Top + 52
            : Math.Max(work.Top + 18, work.Bottom - Height - 86);
    }

    private static bool ForegroundOccupiesScreen()
    {
        try
        {
            var foreground = GetForegroundWindow();
            if (foreground == IntPtr.Zero || !GetWindowRect(foreground, out var bounds))
                return false;
            var screen = Forms.Screen.FromHandle(foreground).WorkingArea;
            return bounds.Left <= screen.Left + 25
                && bounds.Top <= screen.Top + 25
                && bounds.Right >= screen.Right - 25
                && bounds.Bottom >= screen.Bottom - 25;
        }
        catch { return false; }
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
        Opacity = 0;
        EntranceSlide.X = 80;

        var slide = new DoubleAnimation(80, 0, TimeSpan.FromMilliseconds(420))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        EntranceSlide.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slide);

        BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(330))
        {
            FillBehavior = FillBehavior.HoldEnd
        });

        var floatMotion = new DoubleAnimation(0, -7, TimeSpan.FromSeconds(1.15))
        {
            AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        PetFloat.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, floatMotion);

        var tailMotion = new DoubleAnimation(-6, 10, TimeSpan.FromSeconds(0.9))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        TailWag.BeginAnimation(RotateTransform.AngleProperty, tailMotion);
        _autoHide.Start();
    }

    private void OnPetMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _autoHide.Stop();
    }

    private void OnPetMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_closing)
        {
            _autoHide.Stop();
            _autoHide.Start();
        }
    }

    private void AnimateAway()
    {
        if (_closing) return;
        _closing = true;
        _autoHide.Stop();
        var exit = new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(230));
        exit.Completed += (_, _) => Close();
        BeginAnimation(OpacityProperty, exit);
        EntranceSlide.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty,
            new DoubleAnimation(0, 45, TimeSpan.FromMilliseconds(230)));
    }

    private void OnHide(object sender, RoutedEventArgs e) => AnimateAway();

    private void OnDone(object sender, RoutedEventArgs e)
    {
        if (_closing) return;
        Completed?.Invoke(_reminder);
        AnimateAway();
    }

    private void OnSnooze(object sender, RoutedEventArgs e)
    {
        if (_closing) return;
        Snoozed?.Invoke(_reminder);
        AnimateAway();
    }
}
