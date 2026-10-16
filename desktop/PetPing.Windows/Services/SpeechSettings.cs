using System.Text.Json;

namespace PetPing.Windows.Services;

public sealed class SpeechSettings
{
    private readonly string _filePath;
    public bool Enabled { get; private set; } = true;

    public SpeechSettings()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PetPing");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "settings.json");
        if (!File.Exists(_filePath)) return;
        try
        {
            using var json = JsonDocument.Parse(File.ReadAllText(_filePath));
            if (json.RootElement.TryGetProperty("speakReminders", out var enabled)
                && (enabled.ValueKind == JsonValueKind.True || enabled.ValueKind == JsonValueKind.False))
                Enabled = enabled.GetBoolean();
        }
        catch (JsonException)
        {
            // Settings must not prevent a user's reminders from starting.
            // Keep the original file untouched until a setting is explicitly changed.
            Enabled = true;
        }
    }

    public void SetEnabled(bool enabled)
    {
        var temp = _filePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(new { speakReminders = enabled }));
        File.Move(temp, _filePath, overwrite: true);
        Enabled = enabled;
    }
}
