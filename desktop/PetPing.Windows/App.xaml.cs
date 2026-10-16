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
    private SpeechSettings? _speechSettings;
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
            _speechSettings = new SpeechSettings();
            _voice = new VoiceService { Enabled = _speechSettings.Enabled };
            _dashboard = new MainWindow(_store, _voice, _speechSettings, ShowTestPet);
            MainWindow = _dashboard;
            CreateTray();
            _scheduler = new ReminderScheduler(_store);
            _scheduler.ReminderDue += QueueReminder;
            _scheduler.Changed += () => _dashboard.RefreshList();
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.SessionSwitch += OnSessionSwitch;

            if (!e.Args.Contains("--background", StringComparer.OrdinalIgnoreCase))
                _dashboard.Show();

            // Restore a recent, unresolved alert after an unexpected process stop.
            // This is intentionally done BEFORE checking newly due Pending reminders.
            foreach (var reminder in _store.Reminders
                         .Where(r => ReminderRules.ShouldRestoreAlert(r, DateTimeOffset.UtcNow))
                         .OrderBy(r => r.DueAt))
                QueueReminder(reminder);

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
        open.Click += (_, _) => Dispatcher.BeginInvoke(() => _dashboard?.OpenDashboard());
        var quit = new Forms.ToolStripMenuItem("Quit (stop reminders)");
        quit.Click += (_, _) => Dispatcher.BeginInvoke(new Action(Quit));
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
        _tray.DoubleClick += (_, _) => Dispatcher.BeginInvoke(() => _dashboard?.OpenDashboard());
    }

    private void QueueReminder(Reminder reminder)
    {
        if (reminder.Status != ReminderStatus.Delivered) return;
        _petQueue.Enqueue(reminder);
        ShowNextPet();
    }

    private void ShowNextPet()
    {
        if (_locked || _petWindow is not null || _quitting) return;
        while (_petQueue.Count > 0)
        {
            var reminder = _petQueue.Dequeue();
            if (reminder.Status != ReminderStatus.Delivered) continue;
            var popup = new PetWindow(reminder);
            _petWindow = popup;
            popup.Completed += r => { _store!.Mark(r, ReminderStatus.Completed); _dashboard?.RefreshList(); };
            popup.Snoozed += r => { _store!.Snooze(r, TimeSpan.FromMinutes(10)); _dashboard?.RefreshList(); };
            popup.Closed += (_, _) =>
            {
                if (ReferenceEquals(_petWindow, popup)) _petWindow = null;
                ShowNextPet();
            };
            popup.Show();
            _voice?.SpeakReminder(reminder.Title);
            return;
        }
    }

    private void ShowTestPet()
    {
        if (_petWindow is not null) return;
        var demo = new Reminder { Title = "This is how I'll remind you!" };
        var popup = new PetWindow(demo);
        _petWindow = popup;
        popup.Closed += (_, _) =>
        {
            if (ReferenceEquals(_petWindow, popup)) _petWindow = null;
            ShowNextPet();
        };
        popup.Show();
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
        _dashboard?.AllowQuit();
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
