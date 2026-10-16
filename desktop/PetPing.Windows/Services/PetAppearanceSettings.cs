using System.Text.Json;
using System.Text.Json.Serialization;

namespace PetPing.Windows.Services;

public enum PetPlacement { Auto, TopRight, BottomRight, TopLeft, BottomLeft }
public enum PetMotion { Full, Gentle, Off }

/// <summary>
/// Local user preferences, intentionally independent of reminder storage.
/// Invalid/corrupt settings never erase reminders or prevent reminders from running.
/// </summary>
public sealed class PetAppearanceSettings
{
    private readonly string _path;
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public PetPlacement Placement { get; private set; } = PetPlacement.Auto;
    public PetMotion Motion { get; private set; } = PetMotion.Full;

    public PetAppearanceSettings(string? directory = null)
    {
        var folder = directory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PetPing");
        Directory.CreateDirectory(folder);
        _path = Path.Combine(folder, "pet-appearance.json");
        if (!File.Exists(_path)) return;
        try
        {
            var saved = JsonSerializer.Deserialize<AppearanceData>(File.ReadAllText(_path), Options);
            if (saved is null || !Enum.IsDefined(saved.Placement) || !Enum.IsDefined(saved.Motion))
                return;
            Placement = saved.Placement;
            Motion = saved.Motion;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            // Keep original file untouched for diagnosis; fall back to safe defaults.
        }
    }

    public void Update(PetPlacement placement, PetMotion motion)
    {
        if (!Enum.IsDefined(placement) || !Enum.IsDefined(motion))
            throw new ArgumentException("Unknown appearance setting.");

        var temp = _path + ".tmp";
        var next = new AppearanceData { Placement = placement, Motion = motion };
        try
        {
            File.WriteAllText(temp, JsonSerializer.Serialize(next, Options));
            File.Move(temp, _path, overwrite: true);
        }
        catch
        {
            // The previous settings remain in memory and the old file untouched.
            try { if (File.Exists(temp)) File.Delete(temp); } catch { }
            throw;
        }

        Placement = placement;
        Motion = motion;
    }

    private sealed class AppearanceData
    {
        public PetPlacement Placement { get; set; } = PetPlacement.Auto;
        public PetMotion Motion { get; set; } = PetMotion.Full;
    }
}
