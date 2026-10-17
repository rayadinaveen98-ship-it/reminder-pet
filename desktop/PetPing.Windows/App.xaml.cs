using System.Drawing;
using System.Windows;
using System.Windows.Threading;
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
    private PetProfileSettings? _petProfiles;
    private DiagnosticsLog? _diagnostics;
    private bool _faultNoticeShown;
    private Forms.NotifyIcon? _tray;
    private System.Drawing.Icon? _brandTrayIcon;
    private MainWindow? _dashboard;
    private PetWindow? _petWindow;
    private readonly Queue<Reminder> _petQueue = new();
    private bool _locked;
    private bool _suspended;
    private bool _quitting;
    private readonly DispatcherTimer _voiceRepeat = new() { Interval = TimeSpan.FromSeconds(PetWalkRules.DefaultRepeatSeconds) };

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _singleInstance = new Mutex(true, @"Local\PetPing.Experimental.Windows.Instance", out bool firstInstance);
        if (!firstInstance)
        {
            System.Windows.MessageBox.Show("PetPing Experimental is already running. Open it from the system tray.", "PetPing");
            Shutdown();
            return;
        }

        try
        {
            _diagnostics = new DiagnosticsLog();
            _diagnostics.Record("app_started");
            _store = OpenStoreWithOptionalRecovery();
            _speechSettings = new SpeechSettings();
            _appearance = new PetAppearanceSettings();
            _petProfiles = new PetProfileSettings();
            _voice = new VoiceService { Enabled = _speechSettings.Enabled };
            _voice.SpeechFailed += error => _diagnostics?.Record("voice_failed", error);
            _voice.SessionSpeakingChanged += (session, speaking) =>
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_petWindow?.SpeechSessionId == session)
                        _petWindow.SetSpeaking(speaking);
                }));
            _voice.SessionSpeechFinished += session =>
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_petWindow?.SpeechSessionId == session && CanRepeatSpeech(_petWindow))
                    {
                        _voiceRepeat.Stop();
                        _voiceRepeat.Interval = TimeSpan.FromSeconds(
                            _petProfiles?.Current.RepeatSpeechSeconds ?? PetWalkRules.DefaultRepeatSeconds);
                        _voiceRepeat.Start();
                    }
                }));
            _voiceRepeat.Tick += (_, _) =>
            {
                _voiceRepeat.Stop();
                if (_petWindow is { } active && CanRepeatSpeech(active))
                    SpeakFor(active);
            };
            _dashboard = new MainWindow(_store, _voice, _speechSettings, _appearance,
                _petProfiles, ShowTestPet, StopNarration, OnReminderChangedFromDashboard);
            MainWindow = _dashboard;
            try { _dashboard.Icon = BrandIconFactory.RenderBitmap(32); }
            catch (Exception error) { _diagnostics?.Record("window_icon_failed", error); }
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

    private ReminderStore OpenStoreWithOptionalRecovery()
    {
        try { return new ReminderStore(); }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException)
        {
            _diagnostics?.Record("reminder_file_invalid", error);
            if (!ReminderStore.HasValidBackup())
                throw new InvalidDataException(
                    "The reminder file cannot be read and no usable backup was found. " +
                    "The original file has NOT been erased. Open " +
                    ReminderStore.DefaultDataDirectory +
                    " to make a safe copy before attempting repairs.", error);

            var result = System.Windows.MessageBox.Show(
                "PetPing cannot read the saved reminder file.\n\n" +
                "A valid backup from BEFORE your most recent save is available. " +
                "Restoring it may remove newer reminder changes.\n\n" +
                "The damaged original will be kept separately for recovery.\n\n" +
                "Restore the older backup and continue?",
                "Restore previous reminder backup?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
                throw new InvalidDataException(
                    "Recovery cancelled. Your saved reminder file and backup were left unchanged.", error);

            var retainedPath = ReminderStore.RestoreBackupAfterConfirmation();
            _diagnostics?.Record("reminder_backup_restored");
            var restored = new ReminderStore();
            System.Windows.MessageBox.Show(
                "The backup was restored. Some recent changes may be missing.\n\n" +
                (string.IsNullOrEmpty(retainedPath)
                    ? "The previous backup remains in your PetPing data folder."
                    : "The unreadable file has been kept separately in your PetPing data folder."),
                "Reminders restored",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return restored;
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
        try { _brandTrayIcon = BrandIconFactory.CreateTrayIcon(); }
        catch (Exception error) { _diagnostics?.Record("tray_icon_failed", error); }
        _tray = new Forms.NotifyIcon
        {
            Icon = _brandTrayIcon ?? SystemIcons.Application,
            Text = "PetPing Exp - reminders running",
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
            ? $"PetPing Exp - {needsAttention} need attention"
            : "PetPing Exp - reminders running";
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
        // Dashboard actions cannot leave an unanswered voice repeating.
        if (_petWindow?.ReminderId == reminder.Id)
        {
            StopNarration();
            _petWindow.Close();
        }
        UpdateTrayStatus();
    }

    private bool CanRepeatSpeech(PetWindow popup) =>
        PetWalkRules.IsVoiceRepeatAllowed(_voice?.Enabled == true,
            popup.IsMuted, _locked || _suspended,
            popup.IsVisible, popup.AwaitingResponse)
        && _store?.Reminders.Any(r => r.Id == popup.ReminderId &&
                                   r.Status == ReminderStatus.Delivered) == true;

    private void SpeakFor(PetWindow popup)
    {
        if (!CanRepeatSpeech(popup)) return;
        if (_voice?.SpeakReminder(
            _store!.Reminders.First(r => r.Id == popup.ReminderId).Title,
            popup.SpeechSessionId) != true)
            _diagnostics?.Record("voice_unavailable");
    }

    private void StopNarration()
    {
        _voiceRepeat.Stop();
        _voice?.Stop();
    }

    private void RefreshAfterSavedAction()
    {
        // UI refresh is best-effort. A successfully persisted Done/Snooze must
        // NEVER be reported as failed merely because a dashboard view threw.
        try { _dashboard?.RefreshList(); }
        catch (Exception error) { _diagnostics?.Record("dashboard_refresh_failed", error); }
        try { UpdateTrayStatus(); }
        catch (Exception error) { _diagnostics?.Record("tray_refresh_failed", error); }
    }

    private void RequeueAndCloseActivePet()
    {
        if (_petWindow is not { } popup) return;
        var reminder = _store?.Reminders.FirstOrDefault(r => r.Id == popup.ReminderId);
        if (reminder?.Status == ReminderStatus.Delivered &&
            !_petQueue.Any(r => r.Id == reminder.Id))
            _petQueue.Enqueue(reminder);
        StopNarration();
        popup.Close();
    }

    private void ShowNextPet()
    {
        if (_locked || _suspended || _petWindow is not null || _quitting) return;
        while (_petQueue.Count > 0)
        {
            var reminder = _petQueue.Dequeue();
            if (reminder.Status != ReminderStatus.Delivered ||
                _store?.Reminders.Any(r => r.Id == reminder.Id) != true) continue;
            PetWindow? popup = null;
            try
            {
                popup = new PetWindow(reminder, _appearance?.Placement ?? PetPlacement.Auto,
                    _appearance?.Motion ?? PetMotion.Full, _petProfiles?.Current);
                _petWindow = popup;
                popup.Completed += r =>
                {
                    try
                    {
                        _store!.Mark(r, ReminderStatus.Completed);
                        RefreshAfterSavedAction();
                        return true;
                    }
                    catch (Exception error)
                    {
                        OnPresentationFault("completion_save_failed", error);
                        return false;
                    }
                };
                popup.Snoozed += (r, duration) =>
                {
                    try
                    {
                        _store!.Snooze(r, duration);
                        RefreshAfterSavedAction();
                        return true;
                    }
                    catch (Exception error)
                    {
                        OnPresentationFault("snooze_save_failed", error);
                        return false;
                    }
                };
                popup.Arrived += () => SpeakFor(popup);
                popup.InteractionStopped += StopNarration;
                popup.Closed += (_, _) =>
                {
                    StopNarration();
                    if (ReferenceEquals(_petWindow, popup)) _petWindow = null;
                    ShowNextPet();
                };
                popup.Show();
                _diagnostics?.Record("pet_shown");
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
            _appearance?.Motion ?? PetMotion.Full, _petProfiles?.Current);
        _petWindow = popup;
        popup.Arrived += () => _voice?.SpeakReminder(demo.Title, popup.SpeechSessionId);
        popup.InteractionStopped += StopNarration;
        popup.Closed += (_, _) =>
        {
            StopNarration();
            if (ReferenceEquals(_petWindow, popup)) _petWindow = null;
            ShowNextPet();
        };
        popup.Show();
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Suspend)
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _suspended = true;
                _diagnostics?.Record("power_suspended");
                RequeueAndCloseActivePet();
                StopNarration();
            }));
        if (e.Mode == PowerModes.Resume)
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _suspended = false;
                _diagnostics?.Record("power_resumed");
                _scheduler?.CheckNow();
                ShowNextPet();
            }));
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                _locked = true;
                _diagnostics?.Record("session_locked");
                // Session lock must stop speech and postpone the active actor.
                RequeueAndCloseActivePet();
                StopNarration();
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
        StopNarration();
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        _petWindow?.Close();
        if (_tray is not null) { _tray.Visible = false; _tray.Dispose(); }
        _brandTrayIcon?.Dispose();
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
