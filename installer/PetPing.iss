; PetPing v0.12 Windows per-user installer (unsigned alpha)
; Compile after publishing the win-x64 self-contained application.
; Installer binaries are created by the Windows GitHub Actions runner.
#define AppName "PetPing"
#define AppVersion "0.12.0"
#define ExeName "PetPing.Windows.exe"

[Setup]
AppId={{8D6C3B50-3E9C-4E2C-92DF-5751A9B257C0}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Reminder Pet Project
DefaultDirName={localappdata}\Programs\PetPing
DefaultGroupName=PetPing
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
WizardStyle=modern
Compression=lzma
SolidCompression=yes
OutputDir=..\artifacts\PetPing-Installer-v0.12
OutputBaseFilename=PetPing-Setup-v0.12-unsigned
UninstallDisplayName=PetPing
CloseApplications=yes
CloseApplicationsFilter=PetPing.Windows.exe
RestartApplications=no
UsePreviousAppDir=yes

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
Source: "..\artifacts\PetPing-Windows-v0.12\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\PetPing"; Filename: "{app}\{#ExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\PetPing"; Filename: "{app}\{#ExeName}"; Tasks: desktopicon; WorkingDir: "{app}"

[Run]
Filename: "{app}\{#ExeName}"; Description: "Launch PetPing"; Flags: nowait postinstall skipifsilent

[Code]
// The in-app Start with Windows option records the executable's absolute path.
// Preserve that user choice when migrating from the earlier portable build.
procedure CurStepChanged(CurStep: TSetupStep);
var
  ExistingCommand: String;
begin
  if CurStep = ssPostInstall then
  begin
    if RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run',
      'PetPingReminder', ExistingCommand) then
    begin
      if Pos('PetPing.Windows.exe', ExistingCommand) > 0 then
        RegWriteStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run',
          'PetPingReminder',
          '"' + ExpandConstant('{app}\PetPing.Windows.exe') + '" --background');
    end;
  end;
end;

// Only remove the start-on-login entry if it belongs to this exact installed EXE.
// Never remove %LOCALAPPDATA%\PetPing: it holds reminders and pet preferences.
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  CurrentCommand: String;
  InstalledCommand: String;
begin
  if CurUninstallStep = usUninstall then
  begin
    InstalledCommand := '"' + ExpandConstant('{app}\PetPing.Windows.exe') + '" --background';
    if RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run',
      'PetPingReminder', CurrentCommand) then
    begin
      if CompareText(CurrentCommand, InstalledCommand) = 0 then
        RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run',
          'PetPingReminder');
    end;
  end;
end;
