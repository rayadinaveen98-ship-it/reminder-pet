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
    private bool _reallyQuit;

    public MainWindow(ReminderStore store, VoiceService voice, SpeechSettings speechSettings, Action testPet)
    {
        InitializeComponent();
        _store = store;
        _voice = voice;
        _speechSettings = speechSettings;
        _testPet = testPet;
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
        ReminderList.ItemsSource = _store.Reminders.OrderBy(r => r.DueAt).ToList();
        ReminderCount.Text = $"{_store.Reminders.Count(r => r.Status == ReminderStatus.Pending)} upcoming";
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

    private void OnSelectedDone(object sender, RoutedEventArgs e)
    {
        if (ReminderList.SelectedItem is not Reminder reminder) return;
        _store.Mark(reminder, ReminderStatus.Completed);
        RefreshList();
    }

    private void OnDelete(object sender, RoutedEventArgs e)
    {
        if (ReminderList.SelectedItem is not Reminder reminder) return;
        _store.Reminders.Remove(reminder);
        _store.Save();
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
