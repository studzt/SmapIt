#define MyAppName "SmapIt"
#define MyAppVersion "0.0.0"
#define MyAppExeName "SmapIt Manager.exe"
#include "CodeDependencies.iss"

[Setup]
AppId={{7CFA1A64-BC94-4794-8CB8-0EE3788ABC31}
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
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "output\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup: Boolean;
begin
  Dependency_AddDotNet80;
end;