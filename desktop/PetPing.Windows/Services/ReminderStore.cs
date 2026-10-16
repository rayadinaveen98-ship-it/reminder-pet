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

    public ReminderStore()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PetPing");
        Directory.CreateDirectory(folder);
        _dataPath = Path.Combine(folder, "reminders.json");
        Load();
    }

    public void Load()
    {
        if (!File.Exists(_dataPath)) { Reminders = []; return; }
        // Do not overwrite corrupt user data with a blank list.
        Reminders = JsonSerializer.Deserialize<List<Reminder>>(File.ReadAllText(_dataPath), JsonOptions)
            ?? throw new InvalidDataException("Reminder database is empty or invalid.");
    }

    public void Save()
    {
        var temp = _dataPath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(Reminders, JsonOptions));
        File.Move(temp, _dataPath, overwrite: true);
    }

    public Reminder Add(string title, DateTimeOffset dueAt)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("A reminder needs a name.");
        if (dueAt <= DateTimeOffset.Now) throw new ArgumentException("Choose a future time.");
        var reminder = new Reminder { Title = title.Trim(), DueAt = dueAt, OriginalDueAt = dueAt };
        Reminders.Add(reminder);
        Save();
        return reminder;
    }

    public void Mark(Reminder reminder, ReminderStatus status)
    {
        reminder.Status = status;
        reminder.FinishedAt = DateTimeOffset.UtcNow;
        Save();
    }

    public void Snooze(Reminder reminder, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero) throw new ArgumentException("Snooze must be positive.");
        reminder.DueAt = DateTimeOffset.UtcNow.Add(duration);
        reminder.Status = ReminderStatus.Pending;
        reminder.DeliveredAt = null;
        reminder.FinishedAt = null;
        reminder.SnoozeCount++;
        Save();
    }
}
