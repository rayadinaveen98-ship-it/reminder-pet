using System.Text.Json;
using System.Text.Json.Serialization;

namespace PetPing.Windows.Services;

public enum PetSpecies { Cat, Dog, Bunny }
public enum PetPalette { Apricot, Cocoa, Snow, Lavender }
public enum PetAccessory { None, Bow, Scarf }
public enum PetSize { Small, Medium, Large }
public enum PetPersonality { Playful, Calm, Curious }
public enum PetEffects { Off, Gentle, Playful }
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
    public PetEffects Effects { get; init; } = PetEffects.Playful;
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
            !Enum.IsDefined(profile.Personality) || !Enum.IsDefined(profile.Effects) ||
            !Enum.IsDefined(profile.DoneAction) ||
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
    private Dictionary<PetSpecies, PetProfile> _profiles = Defaults();
    public PetSpecies SelectedSpecies { get; private set; } = PetSpecies.Cat;
    public PetProfile Current => _profiles[SelectedSpecies];

    public PetProfile Get(PetSpecies species) =>
        _profiles.TryGetValue(species, out var profile)
            ? profile : throw new ArgumentException("Unknown pet species.");

    private static Dictionary<PetSpecies, PetProfile> Defaults() => new()
    {
        [PetSpecies.Cat] = new PetProfile(),
        [PetSpecies.Dog] = new PetProfile
        {
            Species = PetSpecies.Dog, Palette = PetPalette.Cocoa
        },
        [PetSpecies.Bunny] = new PetProfile
        {
            Species = PetSpecies.Bunny, Palette = PetPalette.Snow
        }
    };

    public PetProfileSettings(string? directory = null)
    {
        var folder = directory ?? PetPingDataRoot.DefaultDirectory;
        Directory.CreateDirectory(folder);
        _path = Path.Combine(folder, "pet-profile.json");
        if (!File.Exists(_path)) return;
        try
        {
            var loaded = JsonSerializer.Deserialize<PetRoster>(File.ReadAllText(_path), JsonOptions);
            ValidateRoster(loaded);
            _profiles = loaded!.Profiles.ToDictionary(p => p.Species);
            SelectedSpecies = loaded.SelectedSpecies;
        }
        catch (Exception error) when (error is JsonException or IOException
            or UnauthorizedAccessException or ArgumentException)
        {
            // Corrupt profile never overwrites the original or reminder data.
            _profiles = Defaults();
            SelectedSpecies = PetSpecies.Cat;
        }
    }

    private static void ValidateRoster(PetRoster? roster)
    {
        if (roster is null || roster.SchemaVersion != 1 ||
            !Enum.IsDefined(roster.SelectedSpecies) || roster.Profiles is null ||
            roster.Profiles.Count != Enum.GetValues<PetSpecies>().Length)
            throw new ArgumentException("Unsupported pet roster.");
        foreach (var profile in roster.Profiles)
            PetProfileCatalog.Validate(profile);
        if (roster.Profiles.Select(p => p.Species).Distinct().Count() != roster.Profiles.Count ||
            !roster.Profiles.Any(p => p.Species == roster.SelectedSpecies))
            throw new ArgumentException("Invalid or duplicate pet entries.");
    }

    private void Save(Dictionary<PetSpecies, PetProfile> next, PetSpecies selection)
    {
        var roster = new PetRoster
        {
            SelectedSpecies = selection,
            Profiles = next.Values.OrderBy(p => p.Species).ToList()
        };
        ValidateRoster(roster);
        var temporary = _path + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(roster, JsonOptions));
            File.Move(temporary, _path, overwrite: true);
        }
        catch
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (IOException) { }
            throw;
        }
        _profiles = next;
        SelectedSpecies = selection;
    }

    public void Update(PetProfile next)
    {
        PetProfileCatalog.Validate(next);
        var updated = new Dictionary<PetSpecies, PetProfile>(_profiles)
        {
            [next.Species] = next
        };
        Save(updated, next.Species);
    }

    public void Select(PetSpecies species)
    {
        if (!Enum.IsDefined(species)) throw new ArgumentException("Unknown pet species.");
        if (species == SelectedSpecies) return;
        Save(new Dictionary<PetSpecies, PetProfile>(_profiles), species);
    }

    private sealed class PetRoster
    {
        public int SchemaVersion { get; set; } = 1;
        public PetSpecies SelectedSpecies { get; set; } = PetSpecies.Cat;
        public List<PetProfile> Profiles { get; set; } = [];
    }
}
