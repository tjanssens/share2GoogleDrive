; Share2GoogleDrive Inno Setup Script
; Mario Bros themed installer for Windows

#ifndef PublishType
  #define PublishType "Framework"
#endif

#define MyAppName "Share2GoogleDrive"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Share2GoogleDrive"
#define MyAppURL "https://github.com/tjanssens/share2GoogleDrive"
#define MyAppExeName "Share2GoogleDrive.exe"

#if PublishType == "Standalone"
  #define OutputSuffix "-Standalone"
  #define SourcePath "..\publish\standalone"
#else
  #define OutputSuffix ""
  #define SourcePath "..\publish\framework"
#endif

[Setup]
; App identity
AppId={{8F4E2A3B-1D5C-4E6F-9A8B-7C2D3E4F5A6B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases

; Installation settings - Per-user install (no admin required)
DefaultDirName={localappdata}\{#MyAppName}
DefaultGroupName={#MyAppName}
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; Output settings
OutputDir=..\dist
OutputBaseFilename={#MyAppName}-Setup-{#MyAppVersion}{#OutputSuffix}
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes

; Visual settings - Mario themed
SetupIconFile=..\src\Share2GoogleDrive\Resources\Icons\app.ico
WizardStyle=modern
WizardSizePercent=100
WizardImageFile=setup-wizard.bmp
WizardSmallImageFile=setup-header.bmp

; Uninstaller
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

; Misc
DisableProgramGroupPage=yes
DisableWelcomePage=no
LicenseFile=license.txt
InfoBeforeFile=
InfoAfterFile=

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
; Mario-themed welcome messages
english.WelcomeLabel1=Welcome to Mario's Cloud Castle!
english.WelcomeLabel2=This will install [name/ver] on your computer.%n%nIt's-a me, the installer! Let's-a go!%n%nClick Next to continue, or Cancel to exit the castle.
english.FinishedHeadingLabel=Yahoo! Installation Complete!
english.FinishedLabel=Setup has finished installing [name] on your computer.%n%nThe application may be launched by selecting the installed shortcuts.%n%nPlayer 1, you're ready to upload to your cloud castle!

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "launchapp"; Description: "Launch Share2GoogleDrive after installation"; GroupDescription: "After installation:"

[Files]
; Main application files
Source: "{#SourcePath}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "Mario's Cloud Castle - Upload files to Google Drive"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"; Comment: "Exit the castle"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "Mario's Cloud Castle"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Mario's Cloud Castle"; Flags: nowait postinstall skipifsilent; Tasks: launchapp

[UninstallDelete]
; Clean up any additional files created during runtime (but NOT user settings)
Type: filesandordirs; Name: "{app}\logs"

[Registry]
; Clean up context menu on uninstall
Root: HKCU; Subkey: "Software\Classes\*\shell\Send2Drive"; Flags: uninsdeletekey dontcreatekey
; Clean up autostart on uninstall
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "Share2GoogleDrive"; Flags: uninsdeletevalue dontcreatekey

[Code]
// Check for .NET 8.0 runtime (only for framework-dependent version)
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;
  #if PublishType == "Framework"
  // Try to run dotnet --list-runtimes to check for .NET 8.0
  if not Exec('dotnet', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Result := False;
  end
  else if ResultCode <> 0 then
  begin
    Result := False;
  end;
  #endif
end;

function CheckDotNet8Runtime(): Boolean;
var
  ResultCode: Integer;
  TempFile: String;
  Lines: TArrayOfString;
  I: Integer;
begin
  Result := False;
  #if PublishType == "Framework"
  TempFile := ExpandConstant('{tmp}\dotnet_check.txt');

  // Run dotnet --list-runtimes and save output
  if Exec('cmd', '/c dotnet --list-runtimes > "' + TempFile + '" 2>&1', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if LoadStringsFromFile(TempFile, Lines) then
    begin
      for I := 0 to GetArrayLength(Lines) - 1 do
      begin
        // Look for Microsoft.WindowsDesktop.App 8.x
        if Pos('Microsoft.WindowsDesktop.App 8.', Lines[I]) > 0 then
        begin
          Result := True;
          Break;
        end;
      end;
    end;
    DeleteFile(TempFile);
  end;
  #else
  // Standalone version doesn't need .NET check
  Result := True;
  #endif
end;

function InitializeSetup(): Boolean;
var
  ErrorMsg: String;
begin
  Result := True;

  #if PublishType == "Framework"
  if not CheckDotNet8Runtime() then
  begin
    ErrorMsg := 'Mamma Mia! .NET 8.0 Desktop Runtime is required but not installed.' + #13#10 + #13#10 +
                'Please download and install .NET 8.0 Desktop Runtime from:' + #13#10 +
                'https://dotnet.microsoft.com/download/dotnet/8.0' + #13#10 + #13#10 +
                'After installing .NET, run this installer again.';
    MsgBox(ErrorMsg, mbError, MB_OK);
    Result := False;
  end;
  #endif
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  // Note: User settings in %APPDATA%\Share2GoogleDrive are intentionally preserved
  // This allows users to keep their settings if they reinstall
end;
