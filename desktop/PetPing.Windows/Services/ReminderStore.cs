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
        // Invalid JSON must not cause the app to silently overwrite saved reminders.
        var restored = JsonSerializer.Deserialize<List<Reminder>>(File.ReadAllText(_dataPath), JsonOptions)
            ?? throw new InvalidDataException("Your reminder file is invalid; it has not been erased.");
        if (restored.Any(r => r.Id == Guid.Empty || string.IsNullOrWhiteSpace(r.Title)))
            throw new InvalidDataException("Your reminder file has invalid entries; it has not been erased.");
        if (restored.Select(r => r.Id).Distinct().Count() != restored.Count)
            throw new InvalidDataException("Your reminder file contains duplicate IDs; it has not been erased.");
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

    public Reminder Add(string title, DateTimeOffset dueAt)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("A reminder needs a name.");
        if (title.Length > 120) throw new ArgumentException("Keep the name within 120 characters.");
        if (dueAt <= DateTimeOffset.UtcNow) throw new ArgumentException("Choose a future time.");
        var reminder = new Reminder { Title = title.Trim(), DueAt = dueAt, OriginalDueAt = dueAt };
        Reminders.Add(reminder);
        try { Save(); }
        catch { Reminders.Remove(reminder); throw; }
        return reminder;
    }

    public void Deliver(Reminder reminder, DateTimeOffset deliveredAt)
    {
        if (reminder.Status != ReminderStatus.Pending)
            throw new InvalidOperationException("Only pending reminders can be delivered.");
        var beforeStatus = reminder.Status;
        var beforeDelivered = reminder.DeliveredAt;
        var beforeFinished = reminder.FinishedAt;
        reminder.Status = ReminderStatus.Delivered;
        reminder.DeliveredAt = deliveredAt;
        reminder.FinishedAt = null;
        try { Save(); }
        catch
        {
            reminder.Status = beforeStatus;
            reminder.DeliveredAt = beforeDelivered;
            reminder.FinishedAt = beforeFinished;
            throw;
        }
    }

    public void Mark(Reminder reminder, ReminderStatus status)
    {
        var beforeStatus = reminder.Status;
        var beforeFinished = reminder.FinishedAt;
        reminder.Status = status;
        reminder.FinishedAt = DateTimeOffset.UtcNow;
        try { Save(); }
        catch
        {
            reminder.Status = beforeStatus;
            reminder.FinishedAt = beforeFinished;
            throw;
        }
    }

    public void Snooze(Reminder reminder, TimeSpan duration)
    {
        if (!ReminderRules.CanSnooze(reminder))
            throw new InvalidOperationException("Only reminders that need attention can be snoozed.");
        if (duration <= TimeSpan.Zero) throw new ArgumentException("Snooze must be positive.");
        var beforeDue = reminder.DueAt;
        var beforeStatus = reminder.Status;
        var beforeDelivered = reminder.DeliveredAt;
        var beforeFinished = reminder.FinishedAt;
        var beforeCount = reminder.SnoozeCount;
        reminder.DueAt = DateTimeOffset.UtcNow.Add(duration);
        reminder.Status = ReminderStatus.Pending;
        reminder.DeliveredAt = null;
        reminder.FinishedAt = null;
        reminder.SnoozeCount++;
        try { Save(); }
        catch
        {
            reminder.DueAt = beforeDue;
            reminder.Status = beforeStatus;
            reminder.DeliveredAt = beforeDelivered;
            reminder.FinishedAt = beforeFinished;
            reminder.SnoozeCount = beforeCount;
            throw;
        }
    }
}
