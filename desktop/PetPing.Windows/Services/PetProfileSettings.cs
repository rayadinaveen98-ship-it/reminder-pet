using System.Text.Json;
using System.Text.Json.Serialization;

namespace PetPing.Windows.Services;

public enum PetSpecies { Cat, Dog, Bunny }
public enum PetPalette { Apricot, Cocoa, Snow, Lavender }
public enum PetAccessory { None, Bow, Scarf }
public enum PetSize { Small, Medium, Large }
public enum PetPersonality { Playful, Calm, Curious }
public enum PetDoneAction { Hop, Wave }
public enum PetSnoozeAction { Yawn, Stretch }

/// <summary>
/// User-owned, versioned profile; unrelated to reminder persistence. New pet
/// species and their appearance are realized by the same actor as desktop alerts.
/// </summary>
public sealed record PetProfile
{
    public int SchemaVersion { get; init; } = 1;
    public PetSpecies Species { get; init; } = PetSpecies.Cat;
    public PetPalette Palette { get; init; } = PetPalette.Apricot;
    public PetAccessory Accessory { get; init; } = PetAccessory.None;
    public PetSize Size { get; init; } = PetSize.Medium;
    public PetPersonality Personality { get; init; } = PetPersonality.Playful;
    public PetDoneAction DoneAction { get; init; } = PetDoneAction.Hop;
    public PetSnoozeAction SnoozeAction { get; init; } = PetSnoozeAction.Yawn;
    public int RepeatSpeechSeconds { get; init; } = 60;
}

public static class PetProfileCatalog
{
    public static string Name(PetSpecies species) => species switch
    {
        PetSpecies.Cat => "Milo the Cat",
        PetSpecies.Dog => "Buddy the Dog",
        PetSpecies.Bunny => "Luna the Bunny",
        _ => "Companion"
    };

    public static void Validate(PetProfile? profile)
    {
        if (profile is null || profile.SchemaVersion != 1 ||
            !Enum.IsDefined(profile.Species) || !Enum.IsDefined(profile.Palette) ||
            !Enum.IsDefined(profile.Accessory) || !Enum.IsDefined(profile.Size) ||
            !Enum.IsDefined(profile.Personality) || !Enum.IsDefined(profile.DoneAction) ||
            !Enum.IsDefined(profile.SnoozeAction) ||
            profile.RepeatSpeechSeconds is not (30 or 60 or 120))
            throw new ArgumentException("Unsupported pet profile or speech interval.");
    }
}

/// <summary>
/// Atomic profile store with safe fallback. Damaged JSON is preserved for the
/// user to inspect; saving writes a new profile independently from reminders.
/// </summary>
public sealed class PetProfileSettings
{
    private readonly string _path;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true, Converters = { new JsonStringEnumConverter() }
    };

    public PetProfile Current { get; private set; } = new();

    public PetProfileSettings(string? directory = null)
    {
        var folder = directory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PetPing");
        Directory.CreateDirectory(folder);
        _path = Path.Combine(folder, "pet-profile.json");
        if (!File.Exists(_path)) return;
        try
        {
            var loaded = JsonSerializer.Deserialize<PetProfile>(File.ReadAllText(_path), JsonOptions);
            PetProfileCatalog.Validate(loaded);
            Current = loaded!;
        }
        catch (Exception error) when (error is JsonException or IOException
            or UnauthorizedAccessException or ArgumentException)
        {
            // Do not replace or silently erase malformed user configuration.
            Current = new PetProfile();
        }
    }

    public void Update(PetProfile next)
    {
        PetProfileCatalog.Validate(next);
        var temporary = _path + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(next, JsonOptions));
            File.Move(temporary, _path, overwrite: true);
        }
        catch
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (IOException) { }
            throw;
        }
        Current = next;
    }
}
