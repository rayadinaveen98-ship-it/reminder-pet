using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;
using PetPing.Windows.Models;
using PetPing.Windows.Services;

namespace PetPing.Windows;

public partial class MainWindow : Window
{
    private readonly ReminderStore _store;
    private readonly VoiceService _voice;
    private readonly SpeechSettings _speechSettings;
    private readonly PetAppearanceSettings _appearance;
    private readonly PetProfileSettings _petProfiles;
    private bool _loadingProfile;
    private readonly DispatcherTimer _studioReactionReset = new();
    private bool _loadingAppearance;
    private readonly Action _testPet;
    private readonly Action _stopNarration;
    private readonly Action<Reminder> _reminderChanged;
    private bool _reallyQuit;
    private bool _savingReminder;
    private Reminder? _editing;
    private DashboardPage _currentPage = DashboardPage.Home;

    private enum DashboardPage { Home, Reminders, Pets, Studio, Settings }

    public MainWindow(ReminderStore store, VoiceService voice, SpeechSettings speechSettings,
        PetAppearanceSettings appearance, PetProfileSettings petProfiles,
        Action testPet, Action stopNarration, Action<Reminder> reminderChanged)
    {
        InitializeComponent();
        _store = store;
        _voice = voice;
        _speechSettings = speechSettings;
        _appearance = appearance;
        _petProfiles = petProfiles;
        _studioReactionReset.Tick += (_, _) =>
        {
            _studioReactionReset.Stop();
            StudioCharacter.Stop();
        };
        _testPet = testPet;
        _stopNarration = stopNarration;
        _reminderChanged = reminderChanged;
        SetDefaultWhen();
        VoiceToggle.IsChecked = _speechSettings.Enabled;
        StartupToggle.IsChecked = StartupRegistrar.IsEnabled();
        _loadingAppearance = true;
        PlacementPicker.SelectedIndex = (int)_appearance.Placement;
        MotionPicker.SelectedIndex = (int)_appearance.Motion;
        _loadingAppearance = false;
        PopulateProfilePickers();
        RefreshPetUI();
        InboxPicker.SelectedIndex = 0;
        RefreshList();
        NavigateTo(DashboardPage.Home);
        Loaded += (_, _) => ApplyResponsiveLayout(ActualWidth);
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

    // Keep date/time inputs, pet choices and advanced Studio controls readable.
    private void OnWindowResized(object sender, SizeChangedEventArgs e) =>
        ApplyResponsiveLayout(e.NewSize.Width);

    private void ApplyResponsiveLayout(double width)
    {
        if (ReminderFieldsGrid is null || HomeMetricsGrid is null ||
            ReminderDateGroup is null || ReminderTimeGroup is null ||
            ReminderRepeatGroup is null || ReminderFieldsActions is null ||
            PetsChoiceGrid is null || StudioAppearanceGrid is null ||
            StudioReactionsGrid is null || StudioMovementGrid is null)
            return;

        var compact = width < 1010;
        HomeMetricsGrid.Columns = compact ? 1 : 3;
        PetsChoiceGrid.Columns = compact ? 1 : 3;

        // Wrap three form fields rather than narrowing each into a tiny input.
        var columns = ReminderFieldsGrid.ColumnDefinitions;
        for (var i = 0; i < columns.Count; i++)
            columns[i].Width = new GridLength(compact && i > 0 ? 0 : 1,
                GridUnitType.Star);
        Grid.SetRow(ReminderDateGroup, 0);
        Grid.SetRow(ReminderTimeGroup, compact ? 1 : 0);
        Grid.SetRow(ReminderRepeatGroup, compact ? 2 : 0);
        Grid.SetColumn(ReminderDateGroup, 0);
        Grid.SetColumn(ReminderTimeGroup, compact ? 0 : 1);
        Grid.SetColumn(ReminderRepeatGroup, compact ? 0 : 2);
        ReminderDateGroup.Margin = compact
            ? new Thickness(0, 0, 0, 12) : new Thickness(0, 0, 12, 0);
        ReminderTimeGroup.Margin = compact
            ? new Thickness(0, 0, 0, 12) : new Thickness(0, 0, 12, 0);
        Grid.SetRow(ReminderFieldsActions, compact ? 3 : 1);
        Grid.SetColumnSpan(ReminderFieldsActions, compact ? 1 : 3);

        ConfigureStudioGrid(StudioAppearanceGrid, StudioAppearanceLeft,
            StudioAppearanceRight, compact);
        ConfigureStudioGrid(StudioReactionsGrid, StudioReactionsLeft,
            StudioReactionsRight, compact);
        ConfigureStudioGrid(StudioMovementGrid, StudioMovementLeft,
            StudioMovementRight, compact);

        // At narrow widths each pet card uses the full readable row.
        PetCatCard.Margin = compact ? new Thickness(0, 0, 0, 12)
            : new Thickness(0, 0, 7, 0);
        PetDogCard.Margin = compact ? new Thickness(0, 0, 0, 12)
            : new Thickness(7, 0, 7, 0);
        PetBunnyCard.Margin = compact ? new Thickness(0)
            : new Thickness(7, 0, 0, 0);
    }

    private static void ConfigureStudioGrid(Grid grid, FrameworkElement left,
        FrameworkElement right, bool compact)
    {
        grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
        grid.ColumnDefinitions[1].Width = compact
            ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
        Grid.SetRow(left, 0);
        Grid.SetRow(right, compact ? 1 : 0);
        Grid.SetColumn(left, 0);
        Grid.SetColumn(right, compact ? 0 : 1);
        left.Margin = compact ? new Thickness(0, 0, 0, 14)
            : new Thickness(0, 0, 12, 0);
        right.Margin = compact ? new Thickness(0) : new Thickness(12, 0, 0, 0);
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
            DashboardPage.Studio => ("Pet Studio", "Customize your companion."),
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

    private void PopulateProfilePickers()
    {
        _loadingProfile = true;
        var current = _petProfiles.Current;
        PetPalettePicker.SelectedIndex = (int)current.Palette;
        PetAccessoryPicker.SelectedIndex = (int)current.Accessory;
        PetSizePicker.SelectedIndex = (int)current.Size;
        PetPersonalityPicker.SelectedIndex = (int)current.Personality;
        PetDonePicker.SelectedIndex = (int)current.DoneAction;
        PetSnoozePicker.SelectedIndex = (int)current.SnoozeAction;
        PetEffectsPicker.SelectedIndex = (int)current.Effects;
        PetRepeatPicker.SelectedIndex = current.RepeatSpeechSeconds switch
        {
            30 => 0, 120 => 2, _ => 1
        };
        _loadingProfile = false;
    }

    private void RefreshPetUI()
    {
        _studioReactionReset.Stop();
        StudioCharacter.Stop();
        var p = _petProfiles.Current;
        var label = PetProfileCatalog.Name(p.Species);
        StudioTitle.Text = "Make " + label.Split(' ')[0] + " your own";
        ActivePetDescription.Text = label + " is your active companion. " +
            "The next reminder will use your saved appearance and actions.";
        HomeHelloButton.Content = "Say hello to " + label.Split(' ')[0];
        StudioCharacter.Apply(p);
        HomeCharacter.Apply(p);
        PetsCatCharacter.Apply(_petProfiles.Get(PetSpecies.Cat));
        PetsDogCharacter.Apply(_petProfiles.Get(PetSpecies.Dog));
        PetsBunnyCharacter.Apply(_petProfiles.Get(PetSpecies.Bunny));
        CatChoiceStatus.Text = p.Species == PetSpecies.Cat ? "Active" : "Available";
        DogChoiceStatus.Text = p.Species == PetSpecies.Dog ? "Active" : "Available";
        BunnyChoiceStatus.Text = p.Species == PetSpecies.Bunny ? "Active" : "Available";
        CatSelectButton.Content = p.Species == PetSpecies.Cat ? "✓ Selected" : "Choose Milo";
        DogSelectButton.Content = p.Species == PetSpecies.Dog ? "✓ Selected" : "Choose Buddy";
        BunnySelectButton.Content = p.Species == PetSpecies.Bunny ? "✓ Selected" : "Choose Luna";
        CatSelectButton.IsEnabled = p.Species != PetSpecies.Cat;
        DogSelectButton.IsEnabled = p.Species != PetSpecies.Dog;
        BunnySelectButton.IsEnabled = p.Species != PetSpecies.Bunny;
        SetPetCardSelection(PetCatCard, p.Species == PetSpecies.Cat);
        SetPetCardSelection(PetDogCard, p.Species == PetSpecies.Dog);
        SetPetCardSelection(PetBunnyCard, p.Species == PetSpecies.Bunny);
    }

    private static void SetPetCardSelection(Border card, bool selected)
    {
        card.Background = new SolidColorBrush(selected
            ? System.Windows.Media.Color.FromRgb(242, 238, 255)
            : System.Windows.Media.Colors.White);
        card.BorderBrush = new SolidColorBrush(selected
            ? System.Windows.Media.Color.FromRgb(117, 98, 212)
            : System.Windows.Media.Color.FromRgb(222, 220, 228));
        card.BorderThickness = new Thickness(selected ? 2 : 1);
    }

    private void OnToggleSettingsHelp(object sender, RoutedEventArgs e)
    {
        var open = SettingsHelpPanel.Visibility != Visibility.Visible;
        SettingsHelpPanel.Visibility = open ? Visibility.Visible : Visibility.Collapsed;
        SettingsHelpToggle.Content = open ? "Hide quick help" : "Show quick help";
        System.Windows.Automation.AutomationProperties.SetName(
            SettingsHelpToggle, open ? "Hide PetPing help" : "Show PetPing help");
    }

    private void SelectPet(PetSpecies species)
    {
        if (_petProfiles.Current.Species == species) return;
        try
        {
            _petProfiles.Select(species);
            PopulateProfilePickers();
            RefreshPetUI();
            ErrorToast.Visibility = Visibility.Collapsed;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException
            or ArgumentException)
        {
            ShowError("Could not save your companion: " + error.Message);
        }
    }

    private void PreviewStudioReaction(bool done)
    {
        if (_petProfiles is null || StudioCharacter is null) return;
        _studioReactionReset.Stop();
        StudioCharacter.Apply(_petProfiles.Current);
        var milliseconds = StudioCharacter.React(done, _appearance.Motion);
        _studioReactionReset.Interval = TimeSpan.FromMilliseconds(milliseconds);
        _studioReactionReset.Start();
    }

    private void OnPreviewDone(object sender, RoutedEventArgs e) =>
        PreviewStudioReaction(true);

    private void OnPreviewSnooze(object sender, RoutedEventArgs e) =>
        PreviewStudioReaction(false);

    private void OnSelectCat(object sender, RoutedEventArgs e) => SelectPet(PetSpecies.Cat);
    private void OnSelectDog(object sender, RoutedEventArgs e) => SelectPet(PetSpecies.Dog);
    private void OnSelectBunny(object sender, RoutedEventArgs e) => SelectPet(PetSpecies.Bunny);

    private void OnPetProfileChanged(object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_loadingProfile || _petProfiles is null || PetPalettePicker is null ||
            PetAccessoryPicker is null || PetSizePicker is null ||
            PetPersonalityPicker is null || PetDonePicker is null ||
            PetSnoozePicker is null || PetRepeatPicker is null || PetEffectsPicker is null ||
            PetPalettePicker.SelectedIndex < 0 || PetAccessoryPicker.SelectedIndex < 0 ||
            PetSizePicker.SelectedIndex < 0 || PetPersonalityPicker.SelectedIndex < 0 ||
            PetDonePicker.SelectedIndex < 0 || PetSnoozePicker.SelectedIndex < 0 ||
            PetRepeatPicker.SelectedIndex < 0 || PetEffectsPicker.SelectedIndex < 0)
            return;

        var next = _petProfiles.Current with
        {
            Palette = (PetPalette)PetPalettePicker.SelectedIndex,
            Accessory = (PetAccessory)PetAccessoryPicker.SelectedIndex,
            Size = (PetSize)PetSizePicker.SelectedIndex,
            Personality = (PetPersonality)PetPersonalityPicker.SelectedIndex,
            DoneAction = (PetDoneAction)PetDonePicker.SelectedIndex,
            SnoozeAction = (PetSnoozeAction)PetSnoozePicker.SelectedIndex,
            Effects = (PetEffects)PetEffectsPicker.SelectedIndex,
            RepeatSpeechSeconds = PetRepeatPicker.SelectedIndex switch
            {
                0 => 30, 2 => 120, _ => 60
            }
        };

        try
        {
            _petProfiles.Update(next);
            RefreshPetUI();
            ErrorToast.Visibility = Visibility.Collapsed;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException
            or ArgumentException)
        {
            PopulateProfilePickers();
            ShowError("Could not save pet customization: " + error.Message);
        }
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

    private static RepeatPattern PickerPattern(int index) =>
        index == 3 ? RepeatPattern.Custom : ReminderEditorRules.SelectRepeat(index);

    private void SetDefaultWhen()
    {
        var local = ReminderEditorRules.DefaultLocalDue(DateTimeOffset.Now, TimeZoneInfo.Local);
        ReminderDate.SelectedDate = local.Date;
        ReminderTime.Text = local.ToString("HH:mm", CultureInfo.InvariantCulture);
    }

    private TimeSpan ReadClockTime()
    {
        if (!TimeSpan.TryParseExact(ReminderTime.Text.Trim(), @"hh\:mm",
                CultureInfo.InvariantCulture, out var time) ||
            time < TimeSpan.Zero || time >= TimeSpan.FromDays(1))
            throw new ArgumentException("Use a time like 09:30 or 19:00.");
        return time;
    }

    private DateTimeOffset ReadDueAt() =>
        ReminderEditorRules.ReadLocalDue(ReminderDate.SelectedDate, ReminderTime.Text,
            TimeZoneInfo.Local, DateTimeOffset.Now);

    private System.Windows.Controls.CheckBox[] WeekdayCheckBoxes() =>
        [RepeatMon, RepeatTue, RepeatWed, RepeatThu, RepeatFri, RepeatSat, RepeatSun];

    private (TimeSpan Time, CustomRepeatOptions Options) ReadCustomSchedule()
    {
        if (ReminderDate.SelectedDate is not DateTime firstDate)
            throw new ArgumentException("Choose the first date for your schedule.");
        var clock = ReadClockTime();
        if (!int.TryParse(CustomInterval.Text.Trim(), System.Globalization.NumberStyles.None,
                CultureInfo.InvariantCulture, out var interval))
            throw new ArgumentException("Enter a whole number for the repeat interval.");

        byte mask = 0;
        var kind = CustomRepeatKindPicker.SelectedIndex switch
        {
            0 => CustomRepeatKind.Weekdays,
            1 => CustomRepeatKind.EveryNDays,
            _ => throw new ArgumentException("Choose a custom repeat type.")
        };
        if (kind == CustomRepeatKind.Weekdays)
        {
            var checks = WeekdayCheckBoxes();
            for (int i = 0; i < checks.Length; i++)
                if (checks[i].IsChecked == true)
                    mask |= (byte)(1 << i);
        }
        var options = new CustomRepeatOptions
        {
            Kind = kind,
            AnchorLocalDate = DateTime.SpecifyKind(firstDate.Date, DateTimeKind.Unspecified),
            Interval = interval,
            WeekdayMask = mask
        };
        options.Validate();
        return (clock, options);
    }

    private static DateTimeOffset PreviewNextCustom(
        TimeSpan clock, CustomRepeatOptions options, DateTimeOffset now)
    {
        var zone = TimeZoneInfo.Local;
        return options.Kind switch
        {
            CustomRepeatKind.Weekdays => AdvancedRepeatRules.NextWeekdayOccurrence(
                options.AnchorLocalDate, clock, options.SelectedWeekdays(),
                options.Interval, zone, now),
            CustomRepeatKind.EveryNDays => AdvancedRepeatRules.NextDayIntervalOccurrence(
                options.AnchorLocalDate, clock, options.Interval, zone, now),
            _ => throw new InvalidDataException("Custom repeat type is not supported.")
        };
    }

    private void OnRepeatPickerChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomRepeatPanel is null || QuickAddButton is null ||
            ReminderDateLabel is null) return;
        var custom = RepeatPicker.SelectedIndex == 3;
        CustomRepeatPanel.Visibility = custom ? Visibility.Visible : Visibility.Collapsed;
        ReminderDateLabel.Text = custom ? "Start date" : "Date";
        QuickAddButton.IsEnabled = !custom && !_savingReminder;
        RefreshRepeatPreview();
    }

    private void OnCustomRepeatChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomWeekdays is null || CustomIntervalUnit is null ||
            CustomRepeatKindPicker is null) return;
        var weekdays = CustomRepeatKindPicker.SelectedIndex == 0;
        CustomWeekdays.Visibility = weekdays ? Visibility.Visible : Visibility.Collapsed;
        CustomIntervalUnit.Text = weekdays ? "week(s)" : "day(s)";
        RefreshRepeatPreview();
    }

    private void OnRepeatEditorChanged(object sender, RoutedEventArgs e) =>
        RefreshRepeatPreview();

    private void RefreshRepeatPreview()
    {
        if (RepeatSchedulePreview is null || ReminderDate is null ||
            ReminderTime is null || RepeatPicker is null ||
            CustomInterval is null || CustomRepeatKindPicker is null ||
            RepeatMon is null || _store is null)
            return;

        try
        {
            var repeat = PickerPattern(RepeatPicker.SelectedIndex);
            if (_editing is { } selected &&
                (selected.Repeat == RepeatPattern.Custom) != (repeat == RepeatPattern.Custom))
            {
                RepeatSchedulePreview.Text =
                    "To change between custom and regular repeats, create a new reminder.";
                return;
            }
            DateTimeOffset next;
            string label;
            if (repeat == RepeatPattern.Custom)
            {
                var (clock, options) = ReadCustomSchedule();
                next = PreviewNextCustom(clock, options, DateTimeOffset.UtcNow);
                label = options.Kind == CustomRepeatKind.Weekdays
                    ? $"Every {options.Interval} week(s) on selected days"
                    : $"Every {options.Interval} day(s)";
            }
            else
            {
                next = ReadDueAt();
                label = repeat switch
                {
                    RepeatPattern.Daily => "Every day",
                    RepeatPattern.Weekly => "Every week",
                    _ => "One time"
                };
            }
            RepeatSchedulePreview.Text =
                $"Next: {next.ToLocalTime():ddd, dd MMM yyyy • HH:mm}  |  {label}";
        }
        catch (Exception error) when (error is ArgumentException or
            InvalidDataException or InvalidOperationException)
        { RepeatSchedulePreview.Text = error.Message; }
    }

    private void OnAddReminder(object sender, RoutedEventArgs e)
    {
        if (_savingReminder) return;
        _savingReminder = true;
        AddButton.IsEnabled = false;
        QuickAddButton.IsEnabled = false;
        try
        {
            var title = ReminderEditorRules.ValidateTitle(ReminderName.Text);
            var repeat = PickerPattern(RepeatPicker.SelectedIndex);
            if (_editing is { } existing &&
                (existing.Repeat == RepeatPattern.Custom) != (repeat == RepeatPattern.Custom))
                throw new InvalidOperationException(
                    "To change between custom and regular repeats, create a new reminder.");

            if (repeat == RepeatPattern.Custom)
            {
                var (clock, options) = ReadCustomSchedule();
                if (_editing is { } customReminder)
                {
                    _store.EditCustom(customReminder, title,
                        clock, TimeZoneInfo.Local.Id, options);
                    _reminderChanged(customReminder);
                }
                else
                {
                    // Experimental data is isolated; first use still needs consent.
                    if (!_store.Reminders.Any(r => r.Repeat == RepeatPattern.Custom))
                    {
                        var response = System.Windows.MessageBox.Show(
                            "Experimental custom scheduling uses a separate PetPing-Experimental " +
                            "data folder. A pre-custom backup will also be created. " +
                            "Create this schedule?",
                            "PetPing Experimental — custom schedule",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning,
                            MessageBoxResult.No);
                        if (response != MessageBoxResult.Yes) return;
                    }
                    _store.AddCustom(title, clock, TimeZoneInfo.Local.Id, options);
                }
            }
            else
            {
                var due = ReadDueAt();
                if (_editing is { } regularReminder)
                {
                    _store.Edit(regularReminder, title, due, repeat);
                    _reminderChanged(regularReminder);
                }
                else
                    _store.Add(title, due, repeat);
            }
            ResetForm();
            RefreshList();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException
            or IOException or UnauthorizedAccessException or TimeZoneNotFoundException
            or InvalidTimeZoneException)
        { ShowError(ex.Message); }
        finally
        {
            _savingReminder = false;
            AddButton.IsEnabled = true;
            QuickAddButton.IsEnabled = RepeatPicker.SelectedIndex != 3 &&
                _editing is null;
        }
    }

    private void OnQuickAdd(object sender, RoutedEventArgs e)
    {
        if (_savingReminder) return;
        if (_editing is not null) { ShowError("Finish or cancel editing first."); return; }
        if (RepeatPicker.SelectedIndex == 3)
        {
            ShowError("Use Add reminder to save a custom repeat schedule.");
            return;
        }
        _savingReminder = true;
        AddButton.IsEnabled = false;
        QuickAddButton.IsEnabled = false;
        try
        {
            _store.Add(ReminderEditorRules.ValidateTitle(ReminderName.Text), DateTimeOffset.Now.AddMinutes(1),
                PickerPattern(RepeatPicker.SelectedIndex));
            ResetForm();
            RefreshList();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException
            or IOException or UnauthorizedAccessException)
        { ShowError(ex.Message); }
        finally
        {
            _savingReminder = false;
            AddButton.IsEnabled = true;
            QuickAddButton.IsEnabled = RepeatPicker.SelectedIndex != 3;
        }
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
        if (reminder.Repeat == RepeatPattern.Custom)
        {
            if (reminder.CustomSchedule is not { } options ||
                reminder.RepeatLocalTime is not { } clock)
            {
                _editing = null;
                ShowError("This custom schedule is incomplete and cannot be edited safely.");
                return;
            }
            ReminderDate.SelectedDate = options.AnchorLocalDate;
            ReminderTime.Text = clock.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
            RepeatPicker.SelectedIndex = 3;
            CustomRepeatKindPicker.SelectedIndex =
                options.Kind == CustomRepeatKind.Weekdays ? 0 : 1;
            CustomInterval.Text = options.Interval.ToString(CultureInfo.InvariantCulture);
            var checks = WeekdayCheckBoxes();
            for (int i = 0; i < checks.Length; i++)
                checks[i].IsChecked = (options.WeekdayMask & (1 << i)) != 0;
        }
        else
        {
            var local = reminder.DueAt.ToLocalTime();
            ReminderDate.SelectedDate = local.Date;
            ReminderTime.Text = local.ToString("HH:mm", CultureInfo.InvariantCulture);
            RepeatPicker.SelectedIndex = reminder.Repeat switch
            {
                RepeatPattern.Daily => 1,
                RepeatPattern.Weekly => 2,
                _ => 0
            };
        }
        QuickAddButton.Visibility = Visibility.Collapsed;
        CancelEditButton.Visibility = Visibility.Visible;
        AddButton.Content = "Save changes";
        ErrorToast.Visibility = Visibility.Collapsed;
        RefreshRepeatPreview();
        ReminderName.Focus();
    }

    private void OnCancelEdit(object sender, RoutedEventArgs e) => ResetForm();

    private void ResetForm()
    {
        _editing = null;
        ReminderName.Clear();
        SetDefaultWhen();
        RepeatPicker.SelectedIndex = 0;
        CustomRepeatKindPicker.SelectedIndex = 0;
        CustomInterval.Text = "1";
        foreach (var check in WeekdayCheckBoxes()) check.IsChecked = false;
        QuickAddButton.Visibility = Visibility.Visible;
        CancelEditButton.Visibility = Visibility.Collapsed;
        AddButton.Content = "Add reminder";
        ErrorToast.Visibility = Visibility.Collapsed;
        RefreshRepeatPreview();
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

    private void OnSnoozeDurationChanged(object sender,
        SelectionChangedEventArgs e)
    {
        if (SnoozeDurationPicker is null || CustomSnoozeMinutes is null) return;
        var isCustom = (SnoozeDurationPicker.SelectedItem as ComboBoxItem)?.Tag as string
            == "custom";
        CustomSnoozeMinutes.Visibility = isCustom
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private TimeSpan ReadSnoozeDuration()
    {
        var selected = (SnoozeDurationPicker.SelectedItem as ComboBoxItem)?.Tag as string;
        if (selected == "custom")
        {
            var value = CustomSnoozeMinutes.Text.Trim();
            if (!int.TryParse(value, System.Globalization.NumberStyles.None,
                CultureInfo.InvariantCulture, out var customMinutes))
                throw new ArgumentException("Enter a whole number of minutes (1–1440).");
            return SnoozePolicy.FromMinutes(customMinutes);
        }
        if (!int.TryParse(selected, System.Globalization.NumberStyles.None,
            CultureInfo.InvariantCulture, out var minutes) ||
            !SnoozePolicy.Presets.Contains(minutes))
            throw new ArgumentException("Choose a valid snooze duration.");
        return SnoozePolicy.FromMinutes(minutes);
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
            var duration = ReadSnoozeDuration();
            _store.Snooze(reminder, duration);
            _reminderChanged(reminder);
            ErrorToast.Visibility = Visibility.Collapsed;
            RefreshList();
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException
            or ArgumentException)
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

    private void OnExportReminderBackup(object sender, RoutedEventArgs e)
    {
        try
        {
            using var picker = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Choose a separate folder or external drive for a reminder backup",
                ShowNewFolderButton = true,
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (picker.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var backup = _store.ExportVerifiedBackup(picker.SelectedPath);
            ErrorToast.Visibility = Visibility.Collapsed;
            System.Windows.MessageBox.Show(
                "Your reminder backup was verified and saved here:\n\n" + backup +
                "\n\nThis is a copy. Your active reminders were not changed.",
                "Reminder backup saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or
            ArgumentException or InvalidOperationException or System.Text.Json.JsonException)
        {
            ShowError("Couldn't export reminders: " + error.Message);
        }
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
            // A disabled voice must stop the current utterance immediately,
            // not only prevent the next scheduled repetition.
            if (!enabled) _stopNarration();
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
