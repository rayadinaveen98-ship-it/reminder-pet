namespace PetPing.Windows.Services;

/// <summary>
/// Deliberately separate experimental reminder/profile data from v0.17.
/// Only builds with PETPING_EXPERIMENTAL use the isolated folder.
/// </summary>
public static class PetPingDataRoot
{
    public static string DefaultDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
#if PETPING_EXPERIMENTAL
        "PetPing-Experimental"
#else
        "PetPing"
#endif
    );
}
