using System.Drawing;
using System.Windows;
using Microsoft.Win32;
using PetPing.Windows.Models;
using PetPing.Windows.Services;
using PetPing.Windows.Views;
using Forms = System.Windows.Forms;

namespace PetPing.Windows;

public partial class App : Application
{
    private Mutex? _singleInstance;
    private ReminderStore? _store;
    private ReminderScheduler? _scheduler;
    private VoiceService? _voice;
    private Forms.NotifyIcon? _tray;
    private MainWindow? _dashboard;
    private PetWindow? _petWindow;
    private readonly Queue<Reminder> _petQueue = new();
    private bool _locked;
    private bool _quitting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _singleInstance = new Mutex(true, @"Local\PetPing.Windows.Instance", out bool firstInstance);
        if (!firstInstance)
        {
            MessageBox.Show("PetPing is already running. Open it from the system tray.", "PetPing");
            Shutdown();
            return;
        }
        try
        {
            _store = new ReminderStore();
            _voice = new VoiceService();
            _dashboard = new MainWindow(_store, _voice, ShowTestPet);
            MainWindow = _dashboard;
            CreateTray();
            _scheduler = new ReminderScheduler(_store);
            _scheduler.ReminderDue += QueueReminder;
            _scheduler.Changed += () => _dashboard.RefreshList();
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.SessionSwitch += OnSessionSwitch;
            _dashboard.Show();
            _scheduler.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show("PetPing couldn't start. Your reminders were not erased.\n\n" + ex.Message,
                "Startup error", MessageBoxButton.OK, MessageBoxImage.Error);
            Quit();
        }
    }

    private void CreateTray()
    {
        var open = new Forms.ToolStripMenuItem("Open PetPing");
        open.Click += (_, _) => _dashboard?.OpenDashboard();
        var quit = new Forms.ToolStripMenuItem("Quit (stop reminders)");
        quit.Click += (_, _) => Quit();
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add(open);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(quit);
        _tray = new Forms.NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "PetPing - reminders running",
            Visible = true,
            ContextMenuStrip = menu
        };
        _tray.DoubleClick += (_, _) => _dashboard?.OpenDashboard();
    }

    private void QueueReminder(Reminder reminder)
    {
        _petQueue.Enqueue(reminder);
        ShowNextPet();
    }

    private void ShowNextPet()
    {
        if (_locked || _petWindow is not null || _petQueue.Count == 0 || _quitting) return;
        var reminder = _petQueue.Dequeue();
        if (reminder.Status is ReminderStatus.Completed or ReminderStatus.Dismissed) { ShowNextPet(); return; }
        _petWindow = new PetWindow(reminder);
        _petWindow.Completed += r => { _store!.Mark(r, ReminderStatus.Completed); _dashboard?.RefreshList(); };
        _petWindow.Snoozed += r => { _store!.Snooze(r, TimeSpan.FromMinutes(10)); _dashboard?.RefreshList(); };
        _petWindow.Closed += (_, _) => { _petWindow = null; ShowNextPet(); };
        _petWindow.Show();
        _voice?.SpeakReminder(reminder.Title);
    }

    private void ShowTestPet()
    {
        // Preview doesn't save fake reminders or change actual reminder state.
        if (_petWindow is not null) return;
        var demo = new Reminder { Title = "This is how I'll remind you!" };
        var window = new PetWindow(demo);
        window.Closed += (_, _) => _petWindow = null;
        _petWindow = window;
        window.Show();
        _voice?.SpeakReminder(demo.Title);
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume)
            Dispatcher.BeginInvoke(() => _scheduler?.CheckNow());
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (e.Reason == SessionSwitchReason.SessionLock) _locked = true;
            if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                _locked = false;
                _scheduler?.CheckNow();
                ShowNextPet();
            }
        });
    }

    private void Quit()
    {
        if (_quitting) return;
        _quitting = true;
        if (_dashboard is not null) _dashboard.AllowQuit();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _scheduler?.Stop();
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        _petWindow?.Close();
        if (_tray is not null) { _tray.Visible = false; _tray.Dispose(); }
        _voice?.Dispose();
        if (_singleInstance is not null)
        {
            try { _singleInstance.ReleaseMutex(); } catch (ApplicationException) { }
            _singleInstance.Dispose();
        }
        base.OnExit(e);
    }
}
