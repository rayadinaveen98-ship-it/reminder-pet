using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using PetPing.Windows.Models;
using PetPing.Windows.Services;

namespace PetPing.Windows;

public partial class MainWindow : Window
{
    private readonly ReminderStore _store;
    private readonly VoiceService _voice;
    private readonly SpeechSettings _speechSettings;
    private readonly PetAppearanceSettings _appearance;
    private bool _loadingAppearance;
    private readonly Action _testPet;
    private readonly Action<Reminder> _reminderChanged;
    private bool _reallyQuit;
    private Reminder? _editing;
    private DashboardPage _currentPage = DashboardPage.Home;

    private enum DashboardPage { Home, Reminders, Pets, Studio, Settings }

    public MainWindow(ReminderStore store, VoiceService voice, SpeechSettings speechSettings,
        PetAppearanceSettings appearance, Action testPet, Action<Reminder> reminderChanged)
    {
        InitializeComponent();
        _store = store;
        _voice = voice;
        _speechSettings = speechSettings;
        _appearance = appearance;
        _testPet = testPet;
        ReminderDate.SelectedDate = DateTime.Today;
        ReminderTime.Text = DateTime.Now.AddMinutes(5).ToString("HH:mm");
        VoiceToggle.IsChecked = _speechSettings.Enabled;
        StartupToggle.IsChecked = StartupRegistrar.IsEnabled();
        _loadingAppearance = true;
        PlacementPicker.SelectedIndex = (int)_appearance.Placement;
        MotionPicker.SelectedIndex = (int)_appearance.Motion;
        _loadingAppearance = false;
        InboxPicker.SelectedIndex = 0;
        RefreshList();
        NavigateTo(DashboardPage.Home);
        Closing += (_, e) => { if (!_reallyQuit) { e.Cancel = true; Hide(); } };
    }

    public void RefreshList()
    {
        var previousId = (ReminderList.SelectedItem as Reminder)?.Id;
        var filter = CurrentFilter();
        var shown = ReminderInboxRules.Select(_store.Reminders, filter, DateTime.Today);
        ReminderList.ItemsSource = null;
        ReminderList.ItemsSource = shown;
        if (previousId is { } id)
            ReminderList.SelectedItem = shown.FirstOrDefault(r => r.Id == id);

        var upcoming = _store.Reminders.Count(r => r.Status == ReminderStatus.Pending);
        var attention = _store.Reminders.Count(ReminderRules.NeedsAttention);
        ReminderCount.Text = $"{shown.Count} shown  •  {upcoming} upcoming" +
            (attention > 0 ? $"  •  {attention} attention" : "");
        EmptyInbox.Visibility = shown.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        EmptyInboxHint.Text = filter switch
        {
            InboxFilter.Today => "No reminders scheduled for today.",
            InboxFilter.Upcoming => "Nothing upcoming. Add your next reminder above.",
            InboxFilter.NeedsAttention => "Nothing needs attention. You're all caught up.",
            InboxFilter.History => "Finished reminders will appear here.",
            _ => "Create your first reminder using the form above."
        };
        AttentionHint.Text = filter switch
        {
            InboxFilter.Today => "Reminders scheduled for today.",
            InboxFilter.Upcoming => "Your future reminders, including snoozed ones.",
            InboxFilter.NeedsAttention => "Unanswered or missed reminders. Complete or snooze them.",
            InboxFilter.History => "Completed reminders are kept here for reference.",
            _ => "Select a reminder to edit, complete, snooze or delete."
        };

        // Dashboard summary is derived from the same local reminder store.
        var snapshot = DashboardSummaryRules.Build(_store.Reminders, DateTime.Today);
        HomeTodayCount.Text = snapshot.TodayCount.ToString();
        HomeUpcomingCount.Text = snapshot.UpcomingCount.ToString();
        HomeAttentionCount.Text = snapshot.AttentionCount.ToString();
        HomeNextList.ItemsSource = snapshot.NextReminders;
        HomeNextList.Visibility = snapshot.NextReminders.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        HomeEmptyNext.Visibility = snapshot.NextReminders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void NavigateTo(DashboardPage page)
    {
        _currentPage = page;
        HomePage.Visibility = page == DashboardPage.Home ? Visibility.Visible : Visibility.Collapsed;
        RemindersPage.Visibility = page == DashboardPage.Reminders ? Visibility.Visible : Visibility.Collapsed;
        PetsPage.Visibility = page == DashboardPage.Pets ? Visibility.Visible : Visibility.Collapsed;
        StudioPage.Visibility = page == DashboardPage.Studio ? Visibility.Visible : Visibility.Collapsed;
        SettingsPage.Visibility = page == DashboardPage.Settings ? Visibility.Visible : Visibility.Collapsed;

        (PageHeading.Text, PageDescription.Text) = page switch
        {
            DashboardPage.Home => ("Home", "Your companion and today's plans."),
            DashboardPage.Reminders => ("Reminders", "Make space for what matters."),
            DashboardPage.Pets => ("My Pets", "Meet your little desktop companions."),
            DashboardPage.Studio => ("Pet Studio", "Make Milo feel more like your own."),
            _ => ("Settings", "PetPing, just the way you like it.")
        };

        var controls = new[] { NavHome, NavReminders, NavPets, NavStudio, NavSettings };
        for (var i = 0; i < controls.Length; i++)
        {
            var active = i == (int)page;
            controls[i].Background = new SolidColorBrush(
                active ? System.Windows.Media.Color.FromRgb(221, 211, 249) : System.Windows.Media.Colors.Transparent);
            controls[i].Foreground = new SolidColorBrush(
                active ? System.Windows.Media.Color.FromRgb(65, 48, 133) : System.Windows.Media.Color.FromRgb(36, 39, 46));
            controls[i].FontWeight = active ? FontWeights.Bold : FontWeights.SemiBold;
        }
        ErrorToast.Visibility = Visibility.Collapsed;
        if (page is DashboardPage.Home or DashboardPage.Reminders)
            RefreshList();
    }

    private void OnNavigateHome(object sender, RoutedEventArgs e) => NavigateTo(DashboardPage.Home);
    private void OnNavigateReminders(object sender, RoutedEventArgs e) => NavigateTo(DashboardPage.Reminders);
    private void OnNavigatePets(object sender, RoutedEventArgs e) => NavigateTo(DashboardPage.Pets);
    private void OnNavigateStudio(object sender, RoutedEventArgs e) => NavigateTo(DashboardPage.Studio);
    private void OnNavigateSettings(object sender, RoutedEventArgs e) => NavigateTo(DashboardPage.Settings);

    private void OnHomeAdd(object sender, RoutedEventArgs e)
    {
        NavigateTo(DashboardPage.Reminders);
        ReminderName.Focus();
    }

    private void OnDismissError(object sender, RoutedEventArgs e) =>
        ErrorToast.Visibility = Visibility.Collapsed;

    private InboxFilter CurrentFilter() => InboxPicker?.SelectedIndex switch
    {
        1 => InboxFilter.Today,
        2 => InboxFilter.Upcoming,
        3 => InboxFilter.NeedsAttention,
        4 => InboxFilter.History,
        _ => InboxFilter.All
    };

    private void OnInboxChanged(object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_store is not null && ReminderList is not null)
            RefreshList();
    }

    private void OnDashboardKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.N)
        {
            if (_editing is not null) ResetForm();
            NavigateTo(DashboardPage.Reminders);
            ReminderName.Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.F5)
        {
            RefreshList();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _editing is not null)
        {
            ResetForm();
            ReminderName.Focus();
            e.Handled = true;
        }
    }

    public void OpenDashboard() { Show(); WindowState = WindowState.Normal; Activate(); }
    public void AllowQuit() => _reallyQuit = true;

    private static RepeatPattern PickerPattern(int index) => index switch
    {
        1 => RepeatPattern.Daily,
        2 => RepeatPattern.Weekly,
        _ => RepeatPattern.None
    };

    private DateTimeOffset ReadDueAt()
    {
        if (ReminderDate.SelectedDate is not DateTime date)
            throw new ArgumentException("Please choose a date.");
        if (!TimeSpan.TryParseExact(ReminderTime.Text.Trim(), @"hh\:mm",
                CultureInfo.InvariantCulture, out var time) || time.TotalHours >= 24)
            throw new ArgumentException("Use a time like 09:30 or 19:00.");

        var local = DateTime.SpecifyKind(date.Date.Add(time), DateTimeKind.Unspecified);
        if (TimeZoneInfo.Local.IsInvalidTime(local))
            throw new ArgumentException("That time does not exist because the clocks change. Choose another.");
        if (TimeZoneInfo.Local.IsAmbiguousTime(local))
            throw new ArgumentException("That time happens twice because the clocks change. Choose another.");
        return new DateTimeOffset(local, TimeZoneInfo.Local.GetUtcOffset(local));
    }

    private void OnAddReminder(object sender, RoutedEventArgs e)
    {
        try
        {
            var due = ReadDueAt();
            var mode = PickerPattern(RepeatPicker.SelectedIndex);
            if (_editing is { } previous)
            {
                _store.Edit(previous, ReminderName.Text, due, mode);
                _reminderChanged(previous);
            }
            else
                _store.Add(ReminderName.Text, due, mode);
            ResetForm();
            RefreshList();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException
            or IOException or TimeZoneNotFoundException or InvalidTimeZoneException)
        { ShowError(ex.Message); }
    }

    private void OnQuickAdd(object sender, RoutedEventArgs e)
    {
        if (_editing is not null) { ShowError("Finish or cancel editing first."); return; }
        try
        {
            _store.Add(ReminderName.Text, DateTimeOffset.Now.AddMinutes(1),
                PickerPattern(RepeatPicker.SelectedIndex));
            ResetForm();
            RefreshList();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException)
        { ShowError(ex.Message); }
    }

    private void OnSelectedEdit(object sender, RoutedEventArgs e)
    {
        var reminder = SelectedOrWarn();
        if (reminder is null) return;
        if (reminder.Status != ReminderStatus.Pending || reminder.RepeatContinuationCreated)
        {
            ShowError("Only upcoming reminders that haven't already repeated can be edited.");
            return;
        }
        _editing = reminder;
        ReminderName.Text = reminder.Title;
        var local = reminder.DueAt.ToLocalTime();
        ReminderDate.SelectedDate = local.Date;
        ReminderTime.Text = local.ToString("HH:mm", CultureInfo.InvariantCulture);
        RepeatPicker.SelectedIndex = reminder.Repeat switch
        {
            RepeatPattern.Daily => 1,
            RepeatPattern.Weekly => 2,
            _ => 0
        };
        QuickAddButton.Visibility = Visibility.Collapsed;
        CancelEditButton.Visibility = Visibility.Visible;
        AddButton.Content = "Save changes";
        ErrorToast.Visibility = Visibility.Collapsed;
        ReminderName.Focus();
    }

    private void OnCancelEdit(object sender, RoutedEventArgs e) => ResetForm();

    private void ResetForm()
    {
        _editing = null;
        ReminderName.Clear();
        ReminderDate.SelectedDate = DateTime.Today;
        ReminderTime.Text = DateTime.Now.AddMinutes(5).ToString("HH:mm");
        RepeatPicker.SelectedIndex = 0;
        QuickAddButton.Visibility = Visibility.Visible;
        CancelEditButton.Visibility = Visibility.Collapsed;
        AddButton.Content = "Add reminder";
        ErrorToast.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string message)
    {
        ValidationMessage.Text = message;
        ErrorToast.Visibility = Visibility.Visible;
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
        try
        {
            _store.Mark(reminder, ReminderStatus.Completed);
            if (_editing?.Id == reminder.Id) ResetForm();
            _reminderChanged(reminder);
            ErrorToast.Visibility = Visibility.Collapsed;
            RefreshList();
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        { ShowError("Could not mark reminder done: " + ex.Message); }
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
            ErrorToast.Visibility = Visibility.Collapsed;
            RefreshList();
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        { ShowError("Couldn't save snooze: " + ex.Message); }
    }

    private void OnDelete(object sender, RoutedEventArgs e)
    {
        var reminder = SelectedOrWarn();
        if (reminder is null) return;
        var question = reminder.Repeat != RepeatPattern.None && reminder.Status == ReminderStatus.Pending
            ? "Delete this upcoming reminder? This also stops its future repeated occurrences."
            : "Delete this reminder? This cannot be undone.";
        if (System.Windows.MessageBox.Show(question, "Delete reminder",
            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        try
        {
            _store.Delete(reminder);
            if (_editing?.Id == reminder.Id) ResetForm();
            _reminderChanged(reminder);
            ErrorToast.Visibility = Visibility.Collapsed;
            RefreshList();
        }
        catch (IOException ex) { ShowError("Could not delete reminder: " + ex.Message); }
    }

    private void OnOpenDataFolder(object sender, RoutedEventArgs e)
    {
        try
        {
            var directory = ReminderStore.DefaultDataDirectory;
            Directory.CreateDirectory(directory);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = directory,
                UseShellExecute = true
            });
            ErrorToast.Visibility = Visibility.Collapsed;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException
            or System.ComponentModel.Win32Exception)
        {
            ShowError("Could not open the local data folder: " + error.Message);
        }
    }

    private void OnTestPet(object sender, RoutedEventArgs e) => _testPet();

    private void OnTestVoice(object sender, RoutedEventArgs e)
    {
        if (!_voice.Enabled)
        {
            ShowError("Turn on Speak reminders before testing voice.");
            return;
        }
        if (!_voice.IsAvailable || !_voice.SpeakReminder("This is a PetPing voice test."))
        {
            ShowError("Windows speech is unavailable. Visual reminders still work.");
            return;
        }
        ErrorToast.Visibility = Visibility.Collapsed;
    }

    private void OnAppearanceChanged(object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_loadingAppearance || _appearance is null ||
            PlacementPicker.SelectedIndex < 0 || MotionPicker.SelectedIndex < 0)
            return;
        try
        {
            _appearance.Update((PetPlacement)PlacementPicker.SelectedIndex,
                               (PetMotion)MotionPicker.SelectedIndex);
            ErrorToast.Visibility = Visibility.Collapsed;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException
            or ArgumentException)
        {
            _loadingAppearance = true;
            PlacementPicker.SelectedIndex = (int)_appearance.Placement;
            MotionPicker.SelectedIndex = (int)_appearance.Motion;
            _loadingAppearance = false;
            ShowError("Could not save pet preferences: " + error.Message);
        }
    }

    private void OnVoiceToggled(object sender, RoutedEventArgs e)
    {
        try
        {
            var enabled = VoiceToggle.IsChecked == true;
            _speechSettings.SetEnabled(enabled);
            _voice.Enabled = enabled;
            ErrorToast.Visibility = Visibility.Collapsed;
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
            ErrorToast.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            StartupToggle.IsChecked = StartupRegistrar.IsEnabled();
            ShowError("Could not change Windows startup setting: " + ex.Message);
        }
    }
}
