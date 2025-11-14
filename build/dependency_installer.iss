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
      if (Copy(SubKey, 1, 2) = '8.') and (CompareStr(SubKey, '8.0.22') >= 0) then
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
