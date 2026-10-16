using System.Text.Json;
using System.Text.Json.Serialization;
using PetPing.Windows.Models;

namespace PetPing.Windows.Services;

public sealed class ReminderStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _dataPath;
    public List<Reminder> Reminders { get; private set; } = [];

    public ReminderStore(string? dataDirectory = null)
    {
        var folder = dataDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PetPing");
        Directory.CreateDirectory(folder);
        _dataPath = Path.Combine(folder, "reminders.json");
        Load();
    }

    public void Load()
    {
        if (!File.Exists(_dataPath)) { Reminders = []; return; }
        var restored = JsonSerializer.Deserialize<List<Reminder>>(File.ReadAllText(_dataPath), JsonOptions)
            ?? throw new InvalidDataException("Your reminder file is invalid; it has not been erased.");
        if (restored.Any(r => r.Id == Guid.Empty || string.IsNullOrWhiteSpace(r.Title)))
            throw new InvalidDataException("Your reminder file has invalid entries; it has not been erased.");
        if (restored.Select(r => r.Id).Distinct().Count() != restored.Count)
            throw new InvalidDataException("Your reminder file contains duplicate IDs; it has not been erased.");
        if (restored.Any(r => !Enum.IsDefined(r.Repeat)))
            throw new InvalidDataException("Reminder data contains an unsupported repetition mode.");
        Reminders = restored;
    }

    public void Save()
    {
        var temp = _dataPath + ".tmp";
        var backup = _dataPath + ".bak";
        using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            JsonSerializer.Serialize(stream, Reminders, JsonOptions);
            stream.Flush(flushToDisk: true);
        }
        if (File.Exists(_dataPath))
            File.Replace(temp, _dataPath, backup);
        else
            File.Move(temp, _dataPath);
    }

    private static void ValidateTitleAndTime(string title, DateTimeOffset dueAt)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("A reminder needs a name.");
        if (title.Trim().Length > 120) throw new ArgumentException("Keep the name within 120 characters.");
        if (dueAt <= DateTimeOffset.UtcNow) throw new ArgumentException("Choose a future time.");
    }

    private static string? ValidateRecurrence(RepeatPattern repeat, string? zoneId)
    {
        if (!Enum.IsDefined(repeat)) throw new ArgumentException("Unsupported repeat setting.");
        if (repeat == RepeatPattern.None) return null;
        zoneId ??= TimeZoneInfo.Local.Id;
        _ = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        return zoneId;
    }

    public Reminder Add(string title, DateTimeOffset dueAt,
        RepeatPattern repeat = RepeatPattern.None, string? timeZoneId = null)
    {
        ValidateTitleAndTime(title, dueAt);
        var tz = ValidateRecurrence(repeat, timeZoneId);
        var reminder = new Reminder
        {
            Title = title.Trim(),
            DueAt = dueAt,
            OriginalDueAt = dueAt,
            Repeat = repeat,
            SeriesId = repeat == RepeatPattern.None ? null : Guid.NewGuid(),
            RepeatTimeZoneId = tz,
            RepeatLocalTime = tz is null ? null
                : TimeZoneInfo.ConvertTime(dueAt, TimeZoneInfo.FindSystemTimeZoneById(tz)).TimeOfDay
        };
        Reminders.Add(reminder);
        try { Save(); }
        catch { Reminders.Remove(reminder); throw; }
        return reminder;
    }

    /// <summary>Edits only the next ordinary pending occurrence. Historical events
    /// and already-snoozed recurring occurrences are intentionally immutable.</summary>
    public void Edit(Reminder reminder, string title, DateTimeOffset dueAt,
        RepeatPattern repeat, string? timeZoneId = null)
    {
        if (reminder.Status != ReminderStatus.Pending || reminder.RepeatContinuationCreated)
            throw new InvalidOperationException("Only upcoming, unsent reminders can be edited.");
        ValidateTitleAndTime(title, dueAt);
        var tz = ValidateRecurrence(repeat, timeZoneId);
        Change(reminder, () =>
        {
            reminder.Title = title.Trim();
            reminder.DueAt = dueAt;
            reminder.OriginalDueAt = dueAt;
            reminder.Repeat = repeat;
            reminder.SeriesId = repeat == RepeatPattern.None ? null : reminder.SeriesId ?? Guid.NewGuid();
            reminder.RepeatTimeZoneId = tz;
            reminder.RepeatLocalTime = tz is null ? null
                : TimeZoneInfo.ConvertTime(dueAt, TimeZoneInfo.FindSystemTimeZoneById(tz)).TimeOfDay;
        });
    }

    public void Delete(Reminder reminder)
    {
        var index = Reminders.IndexOf(reminder);
        if (index < 0) return;
        Reminders.RemoveAt(index);
        try { Save(); }
        catch { Reminders.Insert(index, reminder); throw; }
    }

    // Atomic state transition + series advancement. If disk update fails,
    // restore original in-memory values AND discard the new occurrence.
    private void Change(Reminder reminder, Action update,
        bool advanceSeries = false, DateTimeOffset? now = null)
    {
        var snapshot = new StateSnapshot(reminder);
        Reminder? added = null;
        try
        {
            update();
            if (advanceSeries && reminder.Repeat != RepeatPattern.None &&
                !reminder.RepeatContinuationCreated)
            {
                var reference = now ?? DateTimeOffset.UtcNow;
                var nextDue = RecurrenceRules.NextAfter(reminder, reference);
                added = new Reminder
                {
                    Id = Guid.NewGuid(),
                    Title = reminder.Title,
                    DueAt = nextDue,
                    OriginalDueAt = nextDue,
                    Status = ReminderStatus.Pending,
                    Repeat = reminder.Repeat,
                    SeriesId = reminder.SeriesId ?? Guid.NewGuid(),
                    RepeatTimeZoneId = reminder.RepeatTimeZoneId,
                    RepeatLocalTime = reminder.RepeatLocalTime
                };
                reminder.RepeatContinuationCreated = true;
                // Defensive guard against duplicated future series occurrences.
                if (!Reminders.Any(r => r.Id != reminder.Id &&
                    r.SeriesId == added.SeriesId && r.Repeat == added.Repeat &&
                    r.OriginalDueAt >= added.OriginalDueAt))
                    Reminders.Add(added);
                else
                    added = null;
            }
            Save();
        }
        catch
        {
            snapshot.Restore(reminder);
            if (added is not null) Reminders.Remove(added);
            throw;
        }
    }

    public void Deliver(Reminder reminder, DateTimeOffset deliveredAt)
    {
        if (reminder.Status != ReminderStatus.Pending)
            throw new InvalidOperationException("Only pending reminders can be delivered.");
        Change(reminder, () =>
        {
            reminder.Status = ReminderStatus.Delivered;
            reminder.DeliveredAt = deliveredAt;
            reminder.FinishedAt = null;
        }, advanceSeries: true, now: deliveredAt);
    }

    public void Mark(Reminder reminder, ReminderStatus status)
    {
        if (!Enum.IsDefined(status))
            throw new ArgumentException("Unknown reminder status.");
        var wasPending = reminder.Status == ReminderStatus.Pending;
        Change(reminder, () =>
        {
            reminder.Status = status;
            reminder.FinishedAt = DateTimeOffset.UtcNow;
        }, advanceSeries: wasPending && (status is ReminderStatus.Missed or ReminderStatus.Completed));
    }

    public void Snooze(Reminder reminder, TimeSpan duration)
    {
        if (!ReminderRules.CanSnooze(reminder))
            throw new InvalidOperationException("Only reminders that need attention can be snoozed.");
        if (duration <= TimeSpan.Zero) throw new ArgumentException("Snooze must be positive.");
        Change(reminder, () =>
        {
            reminder.DueAt = DateTimeOffset.UtcNow.Add(duration);
            reminder.Status = ReminderStatus.Pending;
            reminder.DeliveredAt = null;
            reminder.FinishedAt = null;
            reminder.SnoozeCount++;
            // Keep RepeatContinuationCreated: a snoozed old occurrence
            // must never generate tomorrow's / next week's instance twice.
        });
    }

    private sealed class StateSnapshot(Reminder reminder)
    {
        private readonly string _title = reminder.Title;
        private readonly DateTimeOffset _due = reminder.DueAt;
        private readonly DateTimeOffset _original = reminder.OriginalDueAt;
        private readonly ReminderStatus _status = reminder.Status;
        private readonly DateTimeOffset? _delivered = reminder.DeliveredAt;
        private readonly DateTimeOffset? _finished = reminder.FinishedAt;
        private readonly int _snooze = reminder.SnoozeCount;
        private readonly RepeatPattern _repeat = reminder.Repeat;
        private readonly Guid? _series = reminder.SeriesId;
        private readonly string? _zone = reminder.RepeatTimeZoneId;
        private readonly TimeSpan? _localTime = reminder.RepeatLocalTime;
        private readonly bool _continued = reminder.RepeatContinuationCreated;

        public void Restore(Reminder r)
        {
            r.Title = _title;
            r.DueAt = _due;
            r.OriginalDueAt = _original;
            r.Status = _status;
            r.DeliveredAt = _delivered;
            r.FinishedAt = _finished;
            r.SnoozeCount = _snooze;
            r.Repeat = _repeat;
            r.SeriesId = _series;
            r.RepeatTimeZoneId = _zone;
            r.RepeatLocalTime = _localTime;
            r.RepeatContinuationCreated = _continued;
        }
    }
}
