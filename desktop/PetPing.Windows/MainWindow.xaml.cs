using System.Globalization;
using System.Windows;
using PetPing.Windows.Models;
using PetPing.Windows.Services;

namespace PetPing.Windows;

public partial class MainWindow : Window
{
    private readonly ReminderStore _store;
    private readonly VoiceService _voice;
    private readonly SpeechSettings _speechSettings;
    private readonly Action _testPet;
    private readonly Action<Reminder> _reminderChanged;
    private bool _reallyQuit;

    public MainWindow(ReminderStore store, VoiceService voice, SpeechSettings speechSettings,
        Action testPet, Action<Reminder> reminderChanged)
    {
        InitializeComponent();
        _store = store;
        _voice = voice;
        _speechSettings = speechSettings;
        _testPet = testPet;
        _reminderChanged = reminderChanged;
        ReminderDate.SelectedDate = DateTime.Today;
        ReminderTime.Text = DateTime.Now.AddMinutes(5).ToString("HH:mm");
        VoiceToggle.IsChecked = _speechSettings.Enabled;
        StartupToggle.IsChecked = StartupRegistrar.IsEnabled();
        RefreshList();
        Closing += (_, e) => { if (!_reallyQuit) { e.Cancel = true; Hide(); } };
    }

    public void RefreshList()
    {
        ReminderList.ItemsSource = null;
        ReminderList.ItemsSource = _store.Reminders
            .OrderBy(r => r.Status is ReminderStatus.Delivered or ReminderStatus.Missed ? 0 : 1)
            .ThenBy(r => r.DueAt).ToList();
        var upcoming = _store.Reminders.Count(r => r.Status == ReminderStatus.Pending);
        var attention = _store.Reminders.Count(ReminderRules.NeedsAttention);
        ReminderCount.Text = attention > 0
            ? $"{attention} need attention   •   {upcoming} upcoming"
            : $"{upcoming} upcoming";
        AttentionHint.Text = attention > 0
            ? "Unanswered and missed reminders stay here until you complete or snooze them."
            : "Your pet will appear when a reminder is due.";
    }

    public void OpenDashboard() { Show(); WindowState = WindowState.Normal; Activate(); }
    public void AllowQuit() => _reallyQuit = true;

    private void OnAddReminder(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ReminderDate.SelectedDate is not DateTime date)
                throw new ArgumentException("Please choose a date.");
            if (!TimeSpan.TryParseExact(ReminderTime.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var time)
                || time.TotalHours >= 24)
                throw new ArgumentException("Use a time like 09:30 or 19:00.");
            var local = DateTime.SpecifyKind(date.Date.Add(time), DateTimeKind.Unspecified);
            if (TimeZoneInfo.Local.IsInvalidTime(local))
                throw new ArgumentException("That local time does not exist because the clocks change. Choose another time.");
            if (TimeZoneInfo.Local.IsAmbiguousTime(local))
                throw new ArgumentException("That time happens twice because the clocks change. Choose another time.");
            AddReminder(ReminderName.Text, new DateTimeOffset(local, TimeZoneInfo.Local.GetUtcOffset(local)));
        }
        catch (Exception ex) when (ex is ArgumentException or IOException) { ShowError(ex.Message); }
    }

    private void OnQuickAdd(object sender, RoutedEventArgs e)
    {
        try { AddReminder(ReminderName.Text, DateTimeOffset.Now.AddMinutes(1)); }
        catch (Exception ex) when (ex is ArgumentException or IOException) { ShowError(ex.Message); }
    }

    private void AddReminder(string title, DateTimeOffset due)
    {
        _store.Add(title, due);
        ReminderName.Clear();
        ValidationMessage.Visibility = Visibility.Collapsed;
        RefreshList();
    }

    private void ShowError(string message)
    {
        ValidationMessage.Text = message;
        ValidationMessage.Visibility = Visibility.Visible;
    }

    private Reminder? SelectedOrWarn()
    {
        if (ReminderList.SelectedItem is Reminder reminder) return reminder;
        ShowError("Select a reminder first.");
        return null;
    }

    private void OnSelectedDone(object sender, RoutedEventArgs e)
    {
        var reminder = SelectedOrWarn();
        if (reminder is null) return;
        if (reminder.Status == ReminderStatus.Completed)
        {
            ShowError("This reminder is already done.");
            return;
        }
        _store.Mark(reminder, ReminderStatus.Completed);
        _reminderChanged(reminder);
        ValidationMessage.Visibility = Visibility.Collapsed;
        RefreshList();
    }

    private void OnSelectedSnooze(object sender, RoutedEventArgs e)
    {
        var reminder = SelectedOrWarn();
        if (reminder is null) return;
        if (!ReminderRules.CanSnooze(reminder))
        {
            ShowError("Select a missed or unanswered reminder to snooze.");
            return;
        }
        try
        {
            _store.Snooze(reminder, TimeSpan.FromMinutes(10));
            _reminderChanged(reminder);
            ValidationMessage.Visibility = Visibility.Collapsed;
            RefreshList();
        }
        catch (IOException ex) { ShowError("Couldn't save snooze: " + ex.Message); }
    }

    private void OnDelete(object sender, RoutedEventArgs e)
    {
        var reminder = SelectedOrWarn();
        if (reminder is null) return;
        _store.Reminders.Remove(reminder);
        _store.Save();
        _reminderChanged(reminder);
        ValidationMessage.Visibility = Visibility.Collapsed;
        RefreshList();
    }

    private void OnTestPet(object sender, RoutedEventArgs e) => _testPet();

    private void OnVoiceToggled(object sender, RoutedEventArgs e)
    {
        try
        {
            var enabled = VoiceToggle.IsChecked == true;
            _speechSettings.SetEnabled(enabled);
            _voice.Enabled = enabled;
            ValidationMessage.Visibility = Visibility.Collapsed;
        }
        catch (IOException ex)
        {
            VoiceToggle.IsChecked = _voice.Enabled;
            ShowError("Could not save sound settings: " + ex.Message);
        }
    }

    private void OnStartupToggled(object sender, RoutedEventArgs e)
    {
        try
        {
            StartupRegistrar.SetEnabled(StartupToggle.IsChecked == true);
            ValidationMessage.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            StartupToggle.IsChecked = StartupRegistrar.IsEnabled();
            ShowError("Could not change Windows startup setting: " + ex.Message);
        }
    }
}
