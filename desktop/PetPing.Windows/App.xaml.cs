using System.Drawing;
using System.Windows;
using Microsoft.Win32;
using PetPing.Windows.Models;
using PetPing.Windows.Services;
using PetPing.Windows.Views;
using Forms = System.Windows.Forms;

namespace PetPing.Windows;

public partial class App : System.Windows.Application
{
    private Mutex? _singleInstance;
    private ReminderStore? _store;
    private ReminderScheduler? _scheduler;
    private VoiceService? _voice;
    private SpeechSettings? _speechSettings;
    private PetAppearanceSettings? _appearance;
    private DiagnosticsLog? _diagnostics;
    private bool _faultNoticeShown;
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
            System.Windows.MessageBox.Show("PetPing is already running. Open it from the system tray.", "PetPing");
            Shutdown();
            return;
        }

        try
        {
            _diagnostics = new DiagnosticsLog();
            _diagnostics.Record("app_started");
            _store = new ReminderStore();
            _speechSettings = new SpeechSettings();
            _appearance = new PetAppearanceSettings();
            _voice = new VoiceService { Enabled = _speechSettings.Enabled };
            _voice.SpeechFailed += error => _diagnostics?.Record("voice_failed", error);
            _voice.SpeakingChanged += speaking =>
                Dispatcher.BeginInvoke(new Action(() => _petWindow?.SetSpeaking(speaking)));
            _dashboard = new MainWindow(_store, _voice, _speechSettings, _appearance, ShowTestPet, OnReminderChangedFromDashboard);
            MainWindow = _dashboard;
            CreateTray();
            _scheduler = new ReminderScheduler(_store);
            _scheduler.ReminderDue += QueueReminder;
            _scheduler.RemindersMissed += OnRemindersMissed;
            _scheduler.SchedulerFault += OnSchedulerFault;
            _scheduler.Changed += () =>
            {
                _dashboard.RefreshList();
                UpdateTrayStatus();
            };
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
            _diagnostics?.Record("startup_failed", ex);
            System.Windows.MessageBox.Show("PetPing couldn't start. Your reminders were not erased.\n\n" + ex.Message,
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
        UpdateTrayStatus();
    }

    private void QueueReminder(Reminder reminder)
    {
        if (reminder.Status != ReminderStatus.Delivered) return;
        if (_petWindow?.ReminderId == reminder.Id) return;
        if (_petQueue.Any(r => r.Id == reminder.Id)) return;
        _petQueue.Enqueue(reminder);
        ShowNextPet();
    }

    private void UpdateTrayStatus()
    {
        if (_tray is null || _store is null) return;
        var needsAttention = _store.Reminders.Count(ReminderRules.NeedsAttention);
        _tray.Text = needsAttention > 0
            ? $"PetPing - {needsAttention} need attention"
            : "PetPing - reminders running";
    }

    private void OnRemindersMissed(int count)
    {
        UpdateTrayStatus();
        if (_tray is null) return;
        var subject = count == 1 ? "1 reminder was missed" : $"{count} reminders were missed";
        _tray.ShowBalloonTip(6000, "PetPing — check reminders",
            $"{subject} while your computer was unavailable. Open PetPing to snooze or complete.",
            Forms.ToolTipIcon.Info);
    }

    private void OnReminderChangedFromDashboard(Reminder reminder)
    {
        // If the dashboard completes, snoozes or removes a reminder, do not
        // leave a now-stale pet popup claiming it still needs action.
        if (_petWindow?.ReminderId == reminder.Id)
            _petWindow.Close();
        UpdateTrayStatus();
    }

    private void ShowNextPet()
    {
        if (_locked || _petWindow is not null || _quitting) return;
        while (_petQueue.Count > 0)
        {
            var reminder = _petQueue.Dequeue();
            if (reminder.Status != ReminderStatus.Delivered) continue;
            PetWindow? popup = null;
            try
            {
                popup = new PetWindow(reminder, _appearance?.Placement ?? PetPlacement.Auto,
                    _appearance?.Motion ?? PetMotion.Full);
                _petWindow = popup;
                popup.Completed += r =>
                {
                    try
                    {
                        _store!.Mark(r, ReminderStatus.Completed);
                        _dashboard?.RefreshList();
                        UpdateTrayStatus();
                    }
                    catch (Exception error) { OnPresentationFault("completion_save_failed", error); }
                };
                popup.Snoozed += r =>
                {
                    try
                    {
                        _store!.Snooze(r, TimeSpan.FromMinutes(10));
                        _dashboard?.RefreshList();
                        UpdateTrayStatus();
                    }
                    catch (Exception error) { OnPresentationFault("snooze_save_failed", error); }
                };
                popup.Closed += (_, _) =>
                {
                    if (ReferenceEquals(_petWindow, popup)) _petWindow = null;
                    ShowNextPet();
                };
                popup.Show();
                _diagnostics?.Record("pet_shown");
                if (_voice is not null && _voice.Enabled && !_voice.SpeakReminder(reminder.Title))
                    _diagnostics?.Record("voice_unavailable");
                return;
            }
            catch (Exception error)
            {
                _diagnostics?.Record("pet_presentation_failed", error);
                if (ReferenceEquals(_petWindow, popup)) _petWindow = null;
                try { popup?.Close(); } catch { /* ignore failed cleanup */ }
                NotifyGeneric("Reminder needs attention",
                    "A reminder is due. Open PetPing to view it.");
                // The reminder is still Delivered and remains in Needs Attention.
                // Continue trying other queued reminders instead of terminating the scheduler.
            }
        }
    }

    private void OnSchedulerFault(string code, Exception error)
    {
        _diagnostics?.Record(code, error);
        if (!_faultNoticeShown)
        {
            _faultNoticeShown = true;
            NotifyGeneric("Reminder service needs attention",
                "An error occurred while saving or delivering a reminder. Please open PetPing.");
        }
    }

    private void OnPresentationFault(string code, Exception error)
    {
        _diagnostics?.Record(code, error);
        NotifyGeneric("Reminder not updated",
            "Your action could not be saved. Please reopen PetPing and try again.");
    }

    private void NotifyGeneric(string title, string body)
    {
        try
        {
            _tray?.ShowBalloonTip(5500, title, body, Forms.ToolTipIcon.Warning);
        }
        catch (Exception error) { _diagnostics?.Record("tray_notice_failed", error); }
    }

    private void ShowTestPet()
    {
        if (_petWindow is not null) return;
        var demo = new Reminder { Title = "This is how I'll remind you!" };
        var popup = new PetWindow(demo, _appearance?.Placement ?? PetPlacement.Auto,
            _appearance?.Motion ?? PetMotion.Full);
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
        if (e.Mode == PowerModes.Suspend)
        {
            _diagnostics?.Record("power_suspended");
            _voice?.Stop();
        }
        if (e.Mode == PowerModes.Resume)
            Dispatcher.BeginInvoke(() =>
            {
                _diagnostics?.Record("power_resumed");
                _scheduler?.CheckNow();
            });
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                _locked = true;
                _diagnostics?.Record("session_locked");
                _voice?.Stop();
                // On lock, hide any existing pet and requeue its unanswered reminder
                // for the next unlock. Do not try to draw above Windows' secure screen.
                if (_petWindow is not null)
                {
                    var active = _store?.Reminders.FirstOrDefault(r => r.Id == _petWindow.ReminderId);
                    if (active is not null && active.Status == ReminderStatus.Delivered)
                    {
                        if (!_petQueue.Any(r => r.Id == active.Id))
                            _petQueue.Enqueue(active);
                    }
                    _petWindow.Close();
                }
            }
            if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                _locked = false;
                _diagnostics?.Record("session_unlocked");
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
        _diagnostics?.Record("app_exited");
        if (_singleInstance is not null)
        {
            try { _singleInstance.ReleaseMutex(); } catch (ApplicationException) { }
            _singleInstance.Dispose();
        }
        base.OnExit(e);
    }
}
