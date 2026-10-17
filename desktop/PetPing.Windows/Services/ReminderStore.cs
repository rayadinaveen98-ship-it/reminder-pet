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

    public static string DefaultDataDirectory => PetPingDataRoot.DefaultDirectory;

    private readonly string _dataPath;
    public List<Reminder> Reminders { get; private set; } = [];

    public ReminderStore(string? dataDirectory = null)
    {
        var folder = dataDirectory ?? DefaultDataDirectory;
        Directory.CreateDirectory(folder);
        _dataPath = Path.Combine(folder, "reminders.json");
        Load();
    }

    private static List<Reminder> ReadValidated(string path)
    {
        var restored = JsonSerializer.Deserialize<List<Reminder>>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException("Reminder data is empty or invalid.");
        if (restored.Any(r => r.Id == Guid.Empty || string.IsNullOrWhiteSpace(r.Title)))
            throw new InvalidDataException("Reminder data contains invalid entries.");
        if (restored.Select(r => r.Id).Distinct().Count() != restored.Count)
            throw new InvalidDataException("Reminder data contains duplicate IDs.");
        if (restored.Any(r => !Enum.IsDefined(r.Repeat) || !Enum.IsDefined(r.Status)))
            throw new InvalidDataException("Reminder data contains an unsupported state.");
        foreach (var reminder in restored)
        {
            if (reminder.Repeat == RepeatPattern.Custom)
            {
                if (reminder.CustomSchedule is null ||
                    string.IsNullOrWhiteSpace(reminder.RepeatTimeZoneId) ||
                    reminder.RepeatLocalTime is not { } time ||
                    time < TimeSpan.Zero || time >= TimeSpan.FromDays(1) ||
                    reminder.SeriesId is null)
                    throw new InvalidDataException("Custom repeat record is incomplete.");
                reminder.CustomSchedule.Validate();
                try { _ = TimeZoneInfo.FindSystemTimeZoneById(reminder.RepeatTimeZoneId); }
                catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
                { throw new InvalidDataException("Custom repeat has an invalid time zone.", ex); }
            }
            else if (reminder.CustomSchedule is not null)
                throw new InvalidDataException("A legacy reminder has unexpected custom settings.");
        }
        return restored;
    }

    public void Load()
    {
        if (!File.Exists(_dataPath)) { Reminders = []; return; }
        // Never overwrite unreadable data. The user controls whether a backup
        // is restored through an explicit Windows confirmation prompt.
        Reminders = ReadValidated(_dataPath);
    }

    public static bool HasValidBackup(string? dataDirectory = null)
    {
        var backup = Path.Combine(dataDirectory ?? DefaultDataDirectory, "reminders.json.bak");
        if (!File.Exists(backup)) return false;
        try
        {
            _ = ReadValidated(backup);
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException
            or System.Text.Json.JsonException or InvalidDataException)
        { return false; }
    }

    /// <summary>
    /// Called ONLY after the user explicitly chooses to restore. The backup is
    /// validated before writing. The unreadable primary is preserved separately;
    /// neither it nor the original backup is silently discarded.
    /// </summary>
    public static string RestoreBackupAfterConfirmation(string? dataDirectory = null)
    {
        var folder = dataDirectory ?? DefaultDataDirectory;
        var primary = Path.Combine(folder, "reminders.json");
        var backup = primary + ".bak";
        if (!HasValidBackup(folder))
            throw new InvalidDataException("There is no valid saved backup to restore.");

        // Unique filename avoids overwriting an earlier recovery attempt.
        var savedOriginal = Path.Combine(folder,
            "reminders.unreadable-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") +
            "-" + Guid.NewGuid().ToString("N")[..8] + ".json");
        if (File.Exists(primary)) File.Copy(primary, savedOriginal);

        var temp = Path.Combine(folder, "reminders.restore-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            File.Copy(backup, temp);
            _ = ReadValidated(temp);
            File.Move(temp, primary, overwrite: true);
        }
        finally
        {
            try { if (File.Exists(temp)) File.Delete(temp); }
            catch (IOException) { /* do not mask primary result */ }
        }
        return File.Exists(savedOriginal) ? savedOriginal : "";
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
        _ = ReminderEditorRules.ValidateTitle(title);
        if (dueAt <= DateTimeOffset.UtcNow) throw new ArgumentException("Choose a future time.");
    }

    private static string? ValidateRecurrence(RepeatPattern repeat, string? zoneId)
    {
        if (!Enum.IsDefined(repeat) || repeat == RepeatPattern.Custom)
            throw new ArgumentException("Use AddCustom for custom schedules.");
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

    /// <summary>
    /// Creates one future custom occurrence using a local anchor/date and
    /// intended wall clock, without rewriting any old reminder records.
    /// This is a domain-only entry point until a gated v0.18 editor exists.
    /// </summary>
    public Reminder AddCustom(string title, TimeSpan localTime, string timeZoneId,
        CustomRepeatOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        if (string.IsNullOrWhiteSpace(timeZoneId))
            throw new ArgumentException("Choose a time zone.", nameof(timeZoneId));
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        if (localTime < TimeSpan.Zero || localTime >= TimeSpan.FromDays(1))
            throw new ArgumentOutOfRangeException(nameof(localTime));
        var now = DateTimeOffset.UtcNow;
        var nextDue = NextCustomDue(options, localTime, tz, now);
        ValidateTitleAndTime(title, nextDue);
        EnsurePreCustomBackup();
        var reminder = new Reminder
        {
            Title = title.Trim(),
            DueAt = nextDue,
            OriginalDueAt = nextDue,
            Repeat = RepeatPattern.Custom,
            SeriesId = Guid.NewGuid(),
            RepeatTimeZoneId = timeZoneId,
            RepeatLocalTime = localTime,
            CustomSchedule = options
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
        if (reminder.Repeat == RepeatPattern.Custom || repeat == RepeatPattern.Custom)
            throw new InvalidOperationException("Custom schedules need their dedicated editor.");
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

    /// <summary>
    /// Edits the next unsent custom occurrence only. The edited occurrence
    /// becomes the source of later scheduling. Earlier history is immutable.
    /// </summary>
    public void EditCustom(Reminder reminder, string title, TimeSpan localTime,
        string timeZoneId, CustomRepeatOptions options)
    {
        ArgumentNullException.ThrowIfNull(reminder);
        ArgumentNullException.ThrowIfNull(options);
        if (reminder.Repeat != RepeatPattern.Custom || reminder.Status != ReminderStatus.Pending ||
            reminder.RepeatContinuationCreated || !Reminders.Contains(reminder))
            throw new InvalidOperationException("Only an upcoming unsent custom reminder can be edited.");
        options.Validate();
        if (string.IsNullOrWhiteSpace(timeZoneId))
            throw new ArgumentException("Choose a time zone.", nameof(timeZoneId));
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        if (localTime < TimeSpan.Zero || localTime >= TimeSpan.FromDays(1))
            throw new ArgumentOutOfRangeException(nameof(localTime));
        var next = NextCustomDue(options, localTime, tz, DateTimeOffset.UtcNow);
        ValidateTitleAndTime(title, next);
        Change(reminder, () =>
        {
            reminder.Title = title.Trim();
            reminder.DueAt = next;
            reminder.OriginalDueAt = next;
            reminder.RepeatTimeZoneId = timeZoneId;
            reminder.RepeatLocalTime = localTime;
            reminder.CustomSchedule = options;
        });
    }

    /// <summary>One calculator shared by the custom editor and store.</summary>
    private static DateTimeOffset NextCustomDue(CustomRepeatOptions options,
        TimeSpan localTime, TimeZoneInfo zone, DateTimeOffset now) =>
        options.Kind switch
        {
            CustomRepeatKind.Weekdays => AdvancedRepeatRules.NextWeekdayOccurrence(
                options.AnchorLocalDate, localTime, options.SelectedWeekdays(),
                options.Interval, zone, now),
            CustomRepeatKind.EveryNDays => AdvancedRepeatRules.NextDayIntervalOccurrence(
                options.AnchorLocalDate, localTime, options.Interval, zone, now),
            _ => throw new InvalidDataException("Unsupported custom recurrence kind.")
        };

    /// <summary>
    /// Preserve a pre-custom file users can explicitly restore if a future
    /// v0.17 downgrade refuses to read Custom records. Never overwrite it.
    /// </summary>
    private void EnsurePreCustomBackup()
    {
        var backup = Path.Combine(Path.GetDirectoryName(_dataPath)!,
            "reminders.pre-custom-v017.json");
        if (File.Exists(backup))
        {
            var saved = ReadValidated(backup);
            if (saved.Any(r => r.Repeat == RepeatPattern.Custom))
                throw new InvalidDataException("The pre-custom backup is not a legacy snapshot.");
            return;
        }
        if (Reminders.Any(r => r.Repeat == RepeatPattern.Custom))
            throw new InvalidOperationException("Legacy backup missing. Export a safe backup before adding custom reminders.");
        if (File.Exists(_dataPath))
        {
            _ = ReadValidated(_dataPath);
            File.Copy(_dataPath, backup, overwrite: false);
        }
        else
        {
            using var stream = new FileStream(backup, FileMode.CreateNew,
                FileAccess.Write, FileShare.None);
            JsonSerializer.Serialize(stream, Array.Empty<Reminder>(), JsonOptions);
            stream.Flush(flushToDisk: true);
        }
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
                    RepeatLocalTime = reminder.RepeatLocalTime,
                    CustomSchedule = reminder.CustomSchedule
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

    public void Mark(Reminder reminder, ReminderStatus status,
        DateTimeOffset? occurredAt = null)
    {
        if (!Enum.IsDefined(status))
            throw new ArgumentException("Unknown reminder status.");
        var eventTime = occurredAt ?? DateTimeOffset.UtcNow;
        var wasPending = reminder.Status == ReminderStatus.Pending;
        Change(reminder, () =>
        {
            reminder.Status = status;
            reminder.FinishedAt = eventTime;
        }, advanceSeries: wasPending && (status is ReminderStatus.Missed or ReminderStatus.Completed),
            now: eventTime);
    }

    public void Snooze(Reminder reminder, TimeSpan duration)
    {
        if (!ReminderRules.CanSnooze(reminder))
            throw new InvalidOperationException("Only reminders that need attention can be snoozed.");
        SnoozePolicy.Validate(duration);
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
        private readonly CustomRepeatOptions? _custom = reminder.CustomSchedule;

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
            r.CustomSchedule = _custom;
        }
    }
}
