using System.Windows.Threading;
using PetPing.Windows.Models;

namespace PetPing.Windows.Services;

/// <summary>
/// Evaluates due reminders while the tray process is running. Timer callbacks
/// are not real-time alarms; sleep and shutdown are handled by catch-up on resume.
/// Storage failures are reported without terminating the dispatcher.
/// </summary>
public sealed class ReminderScheduler
{
    private readonly ReminderStore _store;
    private readonly DispatcherTimer _tick;
    private DateTimeOffset _retryAfter = DateTimeOffset.MinValue;
    public DateTimeOffset? LastCheckedAt { get; private set; }

    public event Action<Reminder>? ReminderDue;
    public event Action<int>? RemindersMissed;
    public event Action<string, Exception>? SchedulerFault;
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
        LastCheckedAt = now;
        if (now < _retryAfter) return;

        var newlyMissed = 0;
        foreach (var reminder in _store.Reminders
                     .Where(r => r.Status == ReminderStatus.Pending)
                     .OrderBy(r => r.DueAt).ToList())
        {
            var decision = ReminderRules.Evaluate(reminder, now);
            if (decision == ReminderDeliveryDecision.None) continue;
            try
            {
                if (decision == ReminderDeliveryDecision.Missed)
                {
                    _store.Mark(reminder, ReminderStatus.Missed);
                    newlyMissed++;
                }
                else
                {
                    // Commit state before invoking any presentation code.
                    _store.Deliver(reminder, now);
                    try { ReminderDue?.Invoke(reminder); }
                    catch (Exception error)
                    {
                        // A UI/notification failure must not kill the resident scheduler.
                        Report("presentation_failed", error);
                    }
                }
                try { Changed?.Invoke(); }
                catch (Exception error) { Report("dashboard_refresh_failed", error); }
            }
            catch (Exception error)
            {
                // Suppress repeated writes/reports while storage is unavailable;
                // the item stays Pending in memory and is tried again later.
                _retryAfter = DateTimeOffset.UtcNow.AddSeconds(30);
                Report("store_update_failed", error);
                break;
            }
        }
        if (newlyMissed > 0)
        {
            try { RemindersMissed?.Invoke(newlyMissed); }
            catch (Exception error) { Report("missed_notice_failed", error); }
        }
    }

    private void Report(string code, Exception error)
    {
        try { SchedulerFault?.Invoke(code, error); }
        catch { /* even the fault observer may fail: do not crash the timer */ }
    }
}
