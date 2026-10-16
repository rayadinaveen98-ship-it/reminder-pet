using System.Windows.Threading;
using PetPing.Windows.Models;

namespace PetPing.Windows.Services;

public sealed class ReminderScheduler
{
    // Reminders up to 15 minutes late are shown at resume/start. Older ones become missed.
    public static readonly TimeSpan CatchUpWindow = TimeSpan.FromMinutes(15);
    private readonly ReminderStore _store;
    private readonly DispatcherTimer _tick;
    public event Action<Reminder>? ReminderDue;
    public event Action? Changed;

    public ReminderScheduler(ReminderStore store)
    {
        _store = store;
        _tick = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromSeconds(1) };
        _tick.Tick += (_, _) => CheckNow();
    }

    public void Start() { CheckNow(); _tick.Start(); }
    public void Stop() => _tick.Stop();

    public void CheckNow()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var reminder in _store.Reminders.Where(r => r.Status == ReminderStatus.Pending).OrderBy(r => r.DueAt).ToList())
        {
            if (reminder.DueAt > now) continue;
            if (now - reminder.DueAt > CatchUpWindow)
            {
                _store.Mark(reminder, ReminderStatus.Missed);
            }
            else
            {
                // Persist BEFORE raising event: a second tick cannot re-deliver this occurrence.
                reminder.Status = ReminderStatus.Delivered;
                reminder.DeliveredAt = now;
                _store.Save();
                ReminderDue?.Invoke(reminder);
            }
            Changed?.Invoke();
        }
    }
}
