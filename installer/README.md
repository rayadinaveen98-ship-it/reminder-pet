# PetPing Windows installer — current developer preview

The packaged Windows app is **version 0.15** while the v0.17 Companion Beacon redesign passes quality gates. Do not describe it as a fully released v0.17.

## Build (Windows)
- Requirements: free .NET 10 SDK, Windows PowerShell (STA), Inno Setup 6 for the installer.
- Run `desktop/BUILD_WINDOWS.cmd`, or `dotnet publish desktop/PetPing.Windows/PetPing.Windows.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false`.
- The app project runs `tools/Build-BrandIcons.ps1` automatically before compilation. The script loads selected original `BrandMark.xaml` and generates 7 ICO sizes inside `desktop/PetPing.Windows/Assets/PetPing.ico`.
- Build Inno Setup using `installer/PetPing.iss` after the app publishes to `artifacts/PetPing-Windows-v0.15/`. Setup icon and executable/shortcuts use the same approved logo.
- GitHub Windows Actions builds and uploads an unsigned portable package and per-user installer. It validates icon frames, publishes WPF, and runs data-preserving upgrade/install/uninstall smokes.

## Data/privacy and upgrade
- Installed for current user under `%LOCALAPPDATA%\Programs\PetPing`; requires no administrator privileges.
- Reminder/profile JSON is under `%LOCALAPPDATA%\PetPing`, outside the app folder. Installer upgrades and uninstall **must not delete or overwrite** it.
- App's existing Start with Windows setting remains opt-in; installer remaps existing entry only if it belongs to PetPing.
- This is unsigned preview software. Windows SmartScreen may show a warning. No auto-updater, cloud sync, or telemetry is added.

## Still requires physical acceptance
- Inspect 16px/24px taskbar, dark taskbar, Start menu, installer icon and Windows titlebar on actual hardware.
- Verify 810x640 layouts, 100–200% DPI and text scaling, keyboard/high contrast, switch Cat/Dog/Bunny, voice, quit and sleep/wake behavior.
- CI builds and synthetic data-preservation tests do **not** prove physical alert delivery or usability.
