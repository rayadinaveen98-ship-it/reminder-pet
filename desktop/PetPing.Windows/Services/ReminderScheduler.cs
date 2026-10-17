using System.Windows.Threading;
using PetPing.Windows.Models;

namespace PetPing.Windows.Services;

/// <summary>
/// Windows dispatcher adapter for the deterministic ReminderDueProcessor.
/// Timer ticks are best effort. Resume / unlock / startup also call CheckNow;
/// no exact-time promise is made while Windows is asleep or the app is closed.
/// </summary>
public sealed class ReminderScheduler
{
    private readonly ReminderDueProcessor _processor;
    private readonly DispatcherTimer _tick;

    public DateTimeOffset? LastCheckedAt => _processor.LastCheckedAt;

    public event Action<Reminder>? ReminderDue;
    public event Action<int>? RemindersMissed;
    public event Action<string, Exception>? SchedulerFault;
    public event Action? Changed;

    public ReminderScheduler(ReminderStore store)
    {
        _processor = new ReminderDueProcessor(store);
        _processor.ReminderDue += r => ReminderDue?.Invoke(r);
        _processor.RemindersMissed += count => RemindersMissed?.Invoke(count);
        _processor.SchedulerFault += (code, error) => SchedulerFault?.Invoke(code, error);
        _processor.Changed += () => Changed?.Invoke();

        _tick = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _tick.Tick += (_, _) => CheckNow();
    }

    public void Start() { CheckNow(); _tick.Start(); }
    public void Stop() => _tick.Stop();
    public void CheckNow() => _processor.CheckNow(DateTimeOffset.UtcNow);
}
