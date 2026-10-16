using System.Windows;
using System.Windows.Threading;
using PetPing.Windows.Models;

namespace PetPing.Windows.Views;

public partial class PetWindow : Window
{
    private readonly Reminder _reminder;
    private readonly DispatcherTimer _autoHide = new() { Interval = TimeSpan.FromSeconds(45) };
    public event Action<Reminder>? Completed;
    public event Action<Reminder>? Snoozed;

    public PetWindow(Reminder reminder)
    {
        InitializeComponent();
        _reminder = reminder;
        ReminderTitle.Text = reminder.Title;
        var work = SystemParameters.WorkArea;
        Left = work.Right - Width - 18;
        Top = work.Bottom - Height - 18;
        // Do not leave an always-on-top window blocking the screen indefinitely.
        // Timeout does not silently mark the reminder done.
        _autoHide.Tick += (_, _) => Close();
        Loaded += (_, _) => _autoHide.Start();
        Closed += (_, _) => _autoHide.Stop();
    }

    private void OnDone(object sender, RoutedEventArgs e)
    {
        Completed?.Invoke(_reminder);
        Close();
    }

    private void OnSnooze(object sender, RoutedEventArgs e)
    {
        Snoozed?.Invoke(_reminder);
        Close();
    }
}
