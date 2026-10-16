# Windows v0.8 installer — packaging notes

## Goal
Provide an optional per-user desktop installer (Inno Setup 6, no license fee) alongside the portable ZIP. No Windows Store/MSIX registration or code-signing certificate is involved.

## Build
Windows GitHub Actions:
1. Publish Windows x64 self-contained .NET WPF files into artifacts/PetPing-Windows-v0.8/.
2. Use Chocolatey to install the free Inno Setup 6 compiler on the ephemeral Windows runner.
3. Compile installer/PetPing.iss to artifacts/PetPing-Installer-v0.8/PetPing-Setup-v0.8-unsigned.exe.
4. Run an automated **silent install -> check executable -> silent uninstall** sequence with a sentinel file inside LocalAppData/PetPing, verifying that user reminder data survives.
5. Upload both portable ZIP and installer EXE as time-limited workflow artifacts.

## Scope/safety
- Installs into current user's %LOCALAPPDATA%\Programs\PetPing, with a Start Menu shortcut and optional desktop shortcut.
- No admin privileges requested.
- Installer asks Windows to close affected running apps to replace executables when possible. Best practice: explicitly Quit the tray app before running setup.
- Upgrades preserve reminders/preferences/diagnostics because they reside outside the install directory.
- When a prior Start with Windows entry for PetPing exists, installation rewrites it to the new installed EXE; uninstall removes that registry value only if it points to the exact installed EXE.
- Uninstallation never deletes %LOCALAPPDATA%\PetPing. If the user wants to delete personal data, that must be a separate explicit step.
- No background auto-updater, cloud networking or telemetry added.
- **Unsigned**: Windows SmartScreen may display a warning. Signing/reputation and a trusted installer UX are separate beta distribution gates.

## Manual QA pending
Test installation on a normal Windows account, upgrade from a portable build, Start with Windows persistence, Windows uninstall UI, high-DPI dashboard and actual lock/wake behavior. CI proves packaging and simple lifecycle only, not user-facing visual perfection.
