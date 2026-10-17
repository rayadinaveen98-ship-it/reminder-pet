using PetPing.Windows.Models;

namespace PetPing.Windows.Services;

/// <summary>
/// Deterministic and UI-independent reminder due reconciliation.
/// Windows can delay timer ticks during sleep, lock, workload and process
/// startup. Evaluate once against a supplied clock and commit to disk BEFORE
/// attempting presentation. A failed store update is retried after a bounded
/// cooldown, not silently acknowledged.
/// </summary>
public sealed class ReminderDueProcessor
{
    private readonly ReminderStore _store;
    private bool _checking;
    private DateTimeOffset _retryAfter = DateTimeOffset.MinValue;

    public static readonly TimeSpan StorageRetryDelay = TimeSpan.FromSeconds(30);
    public DateTimeOffset? LastCheckedAt { get; private set; }
    public DateTimeOffset RetryAfter => _retryAfter;

    public event Action<Reminder>? ReminderDue;
    public event Action<int>? RemindersMissed;
    public event Action<string, Exception>? SchedulerFault;
    public event Action? Changed;

    public ReminderDueProcessor(ReminderStore store) =>
        _store = store ?? throw new ArgumentNullException(nameof(store));

    public void CheckNow(DateTimeOffset now)
    {
        // Do not re-enter during callbacks, even if the user triggers another
        // dashboard refresh / manual reconciliation in a notification callback.
        if (_checking) return;
        LastCheckedAt = now;
        if (now < _retryAfter) return;

        _checking = true;
        try
        {
            var newlyMissed = 0;
            // Snapshot avoids collection-modified errors if a callback
            // completes/deletes another pending reminder while we run.
            foreach (var reminder in _store.Reminders
                         .Where(r => r.Status == ReminderStatus.Pending)
                         .OrderBy(r => r.DueAt).ToArray())
            {
                if (!_store.Reminders.Contains(reminder) ||
                    reminder.Status != ReminderStatus.Pending) continue;

                var decision = ReminderRules.Evaluate(reminder, now);
                if (decision == ReminderDeliveryDecision.None) continue;
                try
                {
                    if (decision == ReminderDeliveryDecision.Missed)
                    {
                        _store.Mark(reminder, ReminderStatus.Missed, now);
                        newlyMissed++;
                    }
                    else
                        _store.Deliver(reminder, now);
                }
                catch (Exception error)
                {
                    // A failing write leaves Pending in memory and on disk.
                    // Stop this cycle and retry later; do not notify Done.
                    _retryAfter = now.Add(StorageRetryDelay);
                    Report("store_update_failed", error);
                    break;
                }

                if (decision == ReminderDeliveryDecision.Deliver)
                    TryNotify(() => ReminderDue?.Invoke(reminder), "presentation_failed");
                TryNotify(() => Changed?.Invoke(), "dashboard_refresh_failed");
            }

            if (newlyMissed > 0)
                TryNotify(() => RemindersMissed?.Invoke(newlyMissed),
                    "missed_notice_failed");
        }
        finally
        {
            _checking = false;
        }
    }

    private void TryNotify(Action callback, string errorCode)
    {
        try { callback(); }
        catch (Exception error) { Report(errorCode, error); }
    }

    private void Report(string errorCode, Exception error)
    {
        try { SchedulerFault?.Invoke(errorCode, error); }
        catch { /* Fault observers must not take down reminder reconciliation. */ }
    }
}
