using System.Windows.Threading;
using PetPing.Windows.Models;

namespace PetPing.Windows.Services;

public sealed class ReminderScheduler
{
    private readonly ReminderStore _store;
    private readonly DispatcherTimer _tick;
    public event Action<Reminder>? ReminderDue;
    public event Action<int>? RemindersMissed;
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
        var newlyMissed = 0;
        foreach (var reminder in _store.Reminders
                     .Where(r => r.Status == ReminderStatus.Pending)
                     .OrderBy(r => r.DueAt).ToList())
        {
            switch (ReminderRules.Evaluate(reminder, now))
            {
                case ReminderDeliveryDecision.None:
                    break;
                case ReminderDeliveryDecision.Missed:
                    _store.Mark(reminder, ReminderStatus.Missed);
                    newlyMissed++;
                    Changed?.Invoke();
                    break;
                case ReminderDeliveryDecision.Deliver:
                    reminder.Status = ReminderStatus.Delivered;
                    reminder.DeliveredAt = now;
                    _store.Save();
                    ReminderDue?.Invoke(reminder);
                    Changed?.Invoke();
                    break;
            }
        }

        // One summary for a batch, not one intrusive alert per missed reminder.
        if (newlyMissed > 0)
            RemindersMissed?.Invoke(newlyMissed);
    }
}
