using System.Globalization;
using System.Windows;
using PetPing.Windows.Models;
using PetPing.Windows.Services;

namespace PetPing.Windows;

public partial class MainWindow : Window
{
    private readonly ReminderStore _store;
    private readonly VoiceService _voice;
    private readonly Action _testPet;
    private bool _reallyQuit;

    public MainWindow(ReminderStore store, VoiceService voice, Action testPet)
    {
        InitializeComponent();
        _store = store;
        _voice = voice;
        _testPet = testPet;
        ReminderDate.SelectedDate = DateTime.Today;
        ReminderTime.Text = DateTime.Now.AddMinutes(5).ToString("HH:mm");
        RefreshList();
        Closing += (_, e) => { if (!_reallyQuit) { e.Cancel = true; Hide(); } };
    }

    public void RefreshList()
    {
        ReminderList.ItemsSource = null;
        ReminderList.ItemsSource = _store.Reminders.OrderBy(r => r.DueAt).ToList();
        ReminderCount.Text = $"{_store.Reminders.Count(r => r.Status == ReminderStatus.Pending)} upcoming";
    }

    public void OpenDashboard()
    {
        Show(); WindowState = WindowState.Normal; Activate();
    }

    public void AllowQuit() => _reallyQuit = true;

    private void OnAddReminder(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ReminderDate.SelectedDate is not DateTime date) throw new ArgumentException("Please choose a date.");
            if (!TimeSpan.TryParseExact(ReminderTime.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var time)
                || time.TotalHours >= 24)
                throw new ArgumentException("Use a time like 09:30 or 19:00.");
            var local = DateTime.SpecifyKind(date.Date.Add(time), DateTimeKind.Unspecified);
            if (TimeZoneInfo.Local.IsInvalidTime(local))
                throw new ArgumentException("This local time doesn't exist because the clocks change. Choose another time.");
            if (TimeZoneInfo.Local.IsAmbiguousTime(local))
                throw new ArgumentException("This time happens twice because the clocks change. Choose a different time.");
            var due = new DateTimeOffset(local, TimeZoneInfo.Local.GetUtcOffset(local));
            AddReminder(ReminderName.Text, due);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException)
        { ShowError(ex.Message); }
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
    private void OnVoiceToggled(object sender, RoutedEventArgs e) => _voice.Enabled = VoiceToggle.IsChecked == true;
}
