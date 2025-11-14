#define MyAppName "SmapIt"
#define MyAppVersion "0.0.0"
#define MyAppExeName "SmapIt Manager.exe"

#include ".\build\dependency_installer.iss"

[Setup]
AppId={{7CFA1A64-BC94-4794-8CB8-0EE3788ABC31}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
MinVersion=10.0.14393

DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes

LicenseFile=TermsOfUse.txt
OutputDir=output\Installer
OutputBaseFilename=SmapIt-v{#MyAppVersion}
SetupIconFile=./Main/SmapIt.ico

PrivilegesRequired=lowest
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "output\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "build\dependencies\*"; DestDir: "{tmp}"; Flags: dontcopy

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    if FrameworkIsNotInstalled then
      InstallFramework;
  end;
end;

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
