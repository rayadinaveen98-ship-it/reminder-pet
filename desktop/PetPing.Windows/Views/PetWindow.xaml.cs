using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using PetPing.Windows.Models;
using PetPing.Windows.Services;
using Forms = System.Windows.Forms;

namespace PetPing.Windows.Views;

/// <summary>
/// Small transparent walking actor. We animate Window.Left rather than a
/// full-screen transparent surface: other desktop controls remain accessible.
/// </summary>
public partial class PetWindow : Window
{
    private readonly Reminder _reminder;
    private readonly PetPlacement _placement;
    private readonly PetMotion _motion;
    private readonly DispatcherTimer _blink = new() { Interval = TimeSpan.FromSeconds(4.3) };
    private bool _closing;
    private bool _reacting;
    private bool _arrived;
    private bool _muted;
    private double _destinationLeft;

    public Guid ReminderId => _reminder.Id;
    public Guid SpeechSessionId { get; } = Guid.NewGuid();
    public bool IsMuted => _muted;
    public bool AwaitingResponse => _arrived && !_closing && !_reacting;
    public event Action? Arrived;
    public event Action? InteractionStopped;
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
        System.Windows.Automation.AutomationProperties.SetName(
            ReminderTitle, "Reminder: " + reminder.Title);
        System.Windows.Automation.AutomationProperties.SetHelpText(
            HideButton, "Dismisses this pet, but leaves reminder in Needs Attention.");
        ReminderTitle.Visibility = Visibility.Collapsed;
        ActionChips.Visibility = Visibility.Collapsed;
        SourceInitialized += (_, _) => SetInitialPosition();
        Loaded += OnPetLoaded;
        _blink.Tick += (_, _) => Blink();
        Closed += (_, _) =>
        {
            _closing = true;
            _blink.Stop();
            StopWalkAnimation();
            InteractionStopped?.Invoke();
        };
    }

    private void SetInitialPosition()
    {
        var foreground = GetForegroundWindow();
        var screen = foreground == IntPtr.Zero
            ? Forms.Screen.PrimaryScreen
            : Forms.Screen.FromHandle(foreground);

        var working = screen?.WorkingArea ??
            new System.Drawing.Rectangle(0, 0, (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight);
        var transform = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice
            ?? Matrix.Identity;
        var tl = transform.Transform(new System.Windows.Point(working.Left, working.Top));
        var br = transform.Transform(new System.Windows.Point(working.Right, working.Bottom));
        var area = new Rect(tl, br);

        // Manual Left/Right choice is respected. Auto chooses random side on
        // every reminder, rather than always sticking to one edge.
        var fromLeft = _placement switch
        {
            PetPlacement.TopLeft or PetPlacement.BottomLeft => true,
            PetPlacement.TopRight or PetPlacement.BottomRight => false,
            _ => Random.Shared.Next(2) == 0
        };

        var path = PetWalkRules.Plan(area.Left, area.Width, Width, fromLeft);
        _destinationLeft = path.End;
        Left = _motion == PetMotion.Off ? path.End : path.Start;
        var topSide = _placement is PetPlacement.TopLeft or PetPlacement.TopRight;
        Top = topSide ? area.Top + 12 : area.Bottom - Height - 15;
        Top = Math.Clamp(Top, area.Top, Math.Max(area.Top, area.Bottom - Height));

        // Mirror the face/body so the pet points toward the destination.
        PetFacing.ScaleX = fromLeft ? 1 : -1;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private void OnPetLoaded(object sender, RoutedEventArgs e)
    {
        Opacity = 1;
        if (_motion == PetMotion.Off || Math.Abs(Left - _destinationLeft) < 1)
        {
            Arrive();
            return;
        }

        var duration = _motion == PetMotion.Gentle
            ? TimeSpan.FromSeconds(1.15)
            : TimeSpan.FromSeconds(2.8);
        var walk = new DoubleAnimation(Left, _destinationLeft, duration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        walk.Completed += (_, _) =>
        {
            if (_closing) return;
            BeginAnimation(LeftProperty, null);
            Left = _destinationLeft;
            StopWalkAnimation();
            Arrive();
        };
        BeginAnimation(LeftProperty, walk);
        if (_motion == PetMotion.Full)
        {
            LeftFootShift.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(0, -10, TimeSpan.FromMilliseconds(210))
                { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
            RightFootShift.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(-10, 0, TimeSpan.FromMilliseconds(210))
                { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
            TailWag.BeginAnimation(RotateTransform.AngleProperty,
                new DoubleAnimation(-8, 12, TimeSpan.FromMilliseconds(430))
                { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
        }
    }

    private void StopWalkAnimation()
    {
        LeftFootShift.BeginAnimation(TranslateTransform.YProperty, null);
        RightFootShift.BeginAnimation(TranslateTransform.YProperty, null);
        LeftFootShift.Y = 0;
        RightFootShift.Y = 0;
    }

    private void Arrive()
    {
        if (_closing || _arrived) return;
        _arrived = true;
        ReminderTitle.Visibility = Visibility.Visible;
        ActionChips.Visibility = Visibility.Visible;
        if (_motion != PetMotion.Off)
        {
            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(240));
            ReminderTitle.BeginAnimation(OpacityProperty, fade);
            ActionChips.BeginAnimation(OpacityProperty, fade);
            _blink.Start();
            if (_motion == PetMotion.Full)
                PetFloat.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(0, -6, TimeSpan.FromSeconds(1.25))
                    {
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    });
        }
        Arrived?.Invoke();
    }

    private void Blink()
    {
        if (_closing || _motion == PetMotion.Off) return;
        var blink = new DoubleAnimation(1, .1, TimeSpan.FromMilliseconds(95))
            { AutoReverse = true };
        LeftBlink.BeginAnimation(ScaleTransform.ScaleYProperty, blink);
        RightBlink.BeginAnimation(ScaleTransform.ScaleYProperty, blink);
    }

    public void SetSpeaking(bool speaking)
    {
        if (!_arrived || _closing || _reacting) return;
        MouthSmile.Visibility = speaking ? Visibility.Collapsed : Visibility.Visible;
        MouthSpeak.Visibility = speaking ? Visibility.Visible : Visibility.Collapsed;
        MouthSpeak.BeginAnimation(OpacityProperty, null);
        if (speaking && _motion == PetMotion.Full)
            MouthSpeak.BeginAnimation(OpacityProperty,
                new DoubleAnimation(1, .5, TimeSpan.FromMilliseconds(190))
                {
                    AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever
                });
    }

    private void OnMute(object sender, RoutedEventArgs e)
    {
        if (_closing || _reacting || _muted) return;
        _muted = true;
        MuteButton.Content = "Muted";
        MuteButton.IsEnabled = false;
        SetSpeaking(false);
        InteractionStopped?.Invoke();
    }

    private void OnHide(object sender, RoutedEventArgs e)
    {
        if (_closing) return;
        InteractionStopped?.Invoke();
        AnimateAway();
    }

    private void ReactAndLeave(string response)
    {
        _reacting = true;
        _blink.Stop();
        InteractionStopped?.Invoke();
        ReminderTitle.Text = response;
        ReminderTitle.ToolTip = response;
        ActionChips.Visibility = Visibility.Collapsed;
        SetSpeaking(false);
        MouthSpeak.BeginAnimation(OpacityProperty, null);
        MouthSpeak.Visibility = Visibility.Collapsed;
        MouthSmile.Visibility = Visibility.Visible;

        if (_motion == PetMotion.Full)
            PetFloat.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(0, -24, TimeSpan.FromMilliseconds(260))
                { AutoReverse = true });

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(730) };
        timer.Tick += (_, _) => { timer.Stop(); AnimateAway(); };
        timer.Start();
    }

    private void OnDone(object sender, RoutedEventArgs e)
    {
        if (!AwaitingResponse) return;
        if (Completed?.Invoke(_reminder) != false)
            ReactAndLeave("Nice work! ✓");
    }

    private void OnSnooze(object sender, RoutedEventArgs e)
    {
        if (!AwaitingResponse) return;
        if (Snoozed?.Invoke(_reminder) != false)
            ReactAndLeave("Okay, I'll remind you later.");
    }

    private void AnimateAway()
    {
        if (_closing) return;
        _closing = true;
        _blink.Stop();
        InteractionStopped?.Invoke();
        BeginAnimation(LeftProperty, null);
        if (_motion == PetMotion.Off)
        {
            Close();
            return;
        }
        var animation = new DoubleAnimation(Opacity, 0,
            TimeSpan.FromMilliseconds(_motion == PetMotion.Gentle ? 150 : 260));
        animation.Completed += (_, _) => Close();
        BeginAnimation(OpacityProperty, animation);
    }
}
