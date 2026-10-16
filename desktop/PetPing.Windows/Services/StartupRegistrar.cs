using Microsoft.Win32;

namespace PetPing.Windows.Services;

public static class StartupRegistrar
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string EntryName = "PetPingReminder";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey);
        var command = key?.GetValue(EntryName) as string;
        return command is not null
            && string.Equals(command, QuotedExecutable(), StringComparison.OrdinalIgnoreCase);
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true)
            ?? throw new IOException("Windows startup settings could not be opened.");
        if (enabled)
            key.SetValue(EntryName, QuotedExecutable(), RegistryValueKind.String);
        else
            key.DeleteValue(EntryName, throwOnMissingValue: false);
    }

    private static string QuotedExecutable()
    {
        var path = Environment.ProcessPath ??
            throw new InvalidOperationException("Cannot determine the location of PetPing.");
        return "\"" + path + "\" --background";
    }
}
