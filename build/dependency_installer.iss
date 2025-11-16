; https://stackoverflow.com/questions/20752882/how-can-i-install-net-framework-as-a-prerequisite-using-inno-setup

[Code]
var CancelWithoutPrompt: boolean;

function InitializeSetup(): Boolean;
begin
  CancelWithoutPrompt := false;
  result := true;
end;

procedure CancelButtonClick(CurPageID: Integer; var Cancel, Confirm: Boolean);
begin
  if CurPageID=wpInstalling then
    Confirm := not CancelWithoutPrompt;
end;

[Code]
function PackVersionString(VersionString: String): Int64;
var
  Major, Minor, Revision, Build: Cardinal;
  P: Integer;
  S: String;
begin
  // Definir valores padrão
  Major := 0;
  Minor := 0;
  Revision := 0;
  Build := 0;

  S := VersionString;

  P := Pos('.', S);
  if P > 0 then
  begin
    try
      Major := StrToInt(Copy(S, 1, P - 1));
    except
      Major := 0; // Se não for número, assume 0
    end;
    Delete(S, 1, P); // Remove o "Major."
  end
  else if Length(S) > 0 then
  begin
    try
      Major := StrToInt(S); // String só tinha o Major (ex: "9")
    except
      Major := 0;
    end;
    S := ''; // Limpa a string
  end;

  if Length(S) > 0 then
  begin
    P := Pos('.', S);
    if P > 0 then
    begin
      try
        Minor := StrToInt(Copy(S, 1, P - 1));
      except
        Minor := 0;
      end;
      Delete(S, 1, P); // Remove o "Minor."
    end
    else
    begin
      try
        Minor := StrToInt(S); // String tinha Major.Minor (ex: "9.5")
      except
        Minor := 0;
      end;
      S := '';
    end;
  end;

  if Length(S) > 0 then
  begin
    P := Pos('.', S);
    if P > 0 then
    begin
      try
        Revision := StrToInt(Copy(S, 1, P - 1));
      except
        Revision := 0;
      end;
      Delete(S, 1, P); // Remove o "Revision."
    end
    else
    begin
      try
        Revision := StrToInt(S); // String tinha Major.Minor.Revision (ex: "9.5.29")
      except
        Revision := 0;
      end;
      S := '';
    end;
  end;

  if Length(S) > 0 then
  begin
    try
      Build := StrToInt(S); // String tinha 4 partes
    except
      Build := 0;
    end;
  end;

  Result := PackVersionComponents(Major, Minor, Revision, Build);
end;

function FrameworkIsNotInstalled: Boolean;
var
  KeyPath, SubKey: string;
  Versions: TArrayOfString;
  I: Integer;
begin
  Result := True;
  KeyPath := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';

  if RegGetSubkeyNames(HKLM, KeyPath, Versions) then
  begin
    for I := 0 to GetArrayLength(Versions) - 1 do
    begin
      SubKey := Versions[I];
      if ComparePackedVersion(PackVersionString(SubKey), PackVersionString('8.0.22')) > 0 then
      begin
        Result := False;
        exit;
      end;
    end;
  end;
end;

procedure InstallFramework;
var
  StatusText: string;
  ResultCode: Integer;
begin
  ExtractTemporaryFile('dotnet-runtime-8.0.22-win-x64.exe');

  StatusText := WizardForm.StatusLabel.Caption;
  WizardForm.StatusLabel.Caption := 'Installing .NET Desktop Runtime 8...';
  WizardForm.ProgressGauge.Style := npbstMarquee;
  try
      if not Exec(ExpandConstant('{tmp}\dotnet-runtime-8.0.22-win-x64.exe'), '/q /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('.NET installation failed with code: ' + IntToStr(ResultCode) + '.',
      mbError, MB_OK);
    CancelWithoutPrompt := true;
    WizardForm.Close;       
  end;
  finally
    WizardForm.StatusLabel.Caption := StatusText;
    WizardForm.ProgressGauge.Style := npbstNormal;
  end;
end;
