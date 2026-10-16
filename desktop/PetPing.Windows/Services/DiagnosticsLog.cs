namespace PetPing.Windows.Services;

/// <summary>
/// Privacy-conscious, bounded, local event log. Never send titles or free-form
/// exception messages to this log. Diagnostics remain on the user's device.
/// </summary>
public sealed class DiagnosticsLog
{
    private readonly object _sync = new();
    private readonly string _path;

    public string DirectoryPath { get; }

    public DiagnosticsLog(string? folder = null)
    {
        DirectoryPath = folder ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PetPing");
        _path = Path.Combine(DirectoryPath, "diagnostics.log");
    }

    public void Record(string code, Exception? error = null)
    {
        // Only constant app-defined event names; no reminder titles, text,
        // exception messages, stack traces or personal identifiers.
        if (code.Length is 0 or > 80 || code.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '_')))
            throw new ArgumentException("Diagnostic event codes must be fixed identifiers.", nameof(code));
        try
        {
            lock (_sync)
            {
                Directory.CreateDirectory(DirectoryPath);
                if (File.Exists(_path) && new FileInfo(_path).Length > 256 * 1024)
                    File.Move(_path, _path + ".previous", overwrite: true);
                var kind = error?.GetType().Name ?? "none";
                File.AppendAllText(_path, $"{DateTimeOffset.UtcNow:O} {code} {kind}{Environment.NewLine}");
            }
        }
        catch (IOException) { /* diagnostics must never stop a reminder */ }
        catch (UnauthorizedAccessException) { /* diagnostics must never stop a reminder */ }
    }
}
