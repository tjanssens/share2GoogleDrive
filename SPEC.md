# Share2GoogleDrive - Specification

## Overview

Share2GoogleDrive is a Windows desktop application that allows users to quickly upload files to Google Drive via the Windows context menu (right-click) or a keyboard shortcut.

## Functional Requirements

### 1. Context Menu Integration

- **Feature**: Right-clicking on a file shows a menu item "Send 2 Drive"
- **Behavior**:
  - Clicking "Send 2 Drive" starts the upload to the configured Google Drive folder
  - Single file selection only (V1)
  - Shows progress indication during upload
  - Displays notification on successful upload or error

### 2. Keyboard Shortcut Support

- **Default shortcut**: `Ctrl + G`
- **Behavior**:
  - Works when a file is selected in Windows Explorer
  - Same functionality as context menu option
  - Shortcut is configurable via settings

### 3. Settings

#### 3.1 Google Account (OAuth)
- OAuth 2.0 authentication with Google
- Ability to sign in/sign out
- Display of connected account (email)
- Secure token storage (Windows Credential Manager)

#### 3.2 Target Folder on Google Drive
- Folder picker/browser for Google Drive folders
- Ability to set default target folder
- Option to use "My Drive" root as default
- Ability to create new folder

#### 3.3 Keyboard Shortcut Configuration
- Customizable keyboard shortcut combination
- Validation to prevent conflicts with system shortcuts
- Reset to default option

#### 3.4 Auto-Open After Upload
- Option to automatically open the uploaded file in Google Drive (browser)
- Configurable: enabled/disabled
- Default: enabled

### 4. File Conflict Handling

- When a file with the same name already exists in the target folder:
  - Show dialog asking user what to do:
    - **Replace**: Overwrite the existing file
    - **Keep both**: Rename the new file (add number suffix)
    - **Cancel**: Abort the upload

### 5. User Interface

#### 5.1 System Tray Icon
- Application runs in system tray
- Right-click menu with:
  - Open Settings
  - Exit

#### 5.2 Settings Window
- Tabs for different setting categories:
  - Account
  - Upload Settings
  - Keyboard Shortcut
  - General (autostart, notifications, etc.)

#### 5.3 Notifications
- Windows toast notifications for:
  - Upload started
  - Upload completed (with clickable link to file)
  - Upload failed (with error message)

## Technical Specifications

### Platform
- **Target**: Windows 10/11
- **Framework**: .NET 8.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Packaging**: MSIX installer and portable EXE

### Google Drive API
- Google Drive API v3
- OAuth 2.0 for authentication
- Scopes:
  - `https://www.googleapis.com/auth/drive.file`
  - `https://www.googleapis.com/auth/drive.metadata.readonly`

### Context Menu Registration
- Windows Registry modification for shell integration
- Registration under `HKEY_CLASSES_ROOT\*\shell\Send2Drive`
- Uses shell extension or command-line invocation

### Keyboard Shortcut Implementation
- Global hotkey registration via Windows API (RegisterHotKey)
- Background service for hotkey listening

### Data Storage
- Configuration: JSON file (System.Text.Json)
- Tokens: Windows Credential Manager (CredentialManager NuGet)
- Location: `%APPDATA%\Share2GoogleDrive\`

## Non-Functional Requirements

### Performance
- Upload starts within 2 seconds of action
- Minimal CPU/memory usage when idle
- Support for files up to 5GB

### Security
- No storage of passwords
- Encrypted token storage via Windows Credential Manager
- Secure OAuth flow (system browser, not embedded)

### Reliability
- Automatic retry on network errors (max 3 attempts)
- Resumable uploads for large files
- Logging for troubleshooting (Serilog)

### User Experience
- User-level installation (no admin required)
- Autostart option
- Minimal configuration required for first use

## Architecture

```
Share2GoogleDrive/
├── src/
│   ├── Share2GoogleDrive/                    # Main WPF Application
│   │   ├── App.xaml                          # Application entry point
│   │   ├── App.xaml.cs
│   │   ├── Views/
│   │   │   ├── MainWindow.xaml               # Settings window
│   │   │   ├── FolderBrowserDialog.xaml      # Google Drive folder picker
│   │   │   └── ConflictDialog.xaml           # File conflict resolution
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs
│   │   │   ├── SettingsViewModel.cs
│   │   │   └── FolderBrowserViewModel.cs
│   │   ├── Models/
│   │   │   ├── AppSettings.cs
│   │   │   └── UploadResult.cs
│   │   ├── Services/
│   │   │   ├── GoogleAuthService.cs          # OAuth implementation
│   │   │   ├── GoogleDriveService.cs         # Drive API wrapper
│   │   │   ├── SettingsService.cs            # Settings management
│   │   │   ├── HotkeyService.cs              # Global hotkey handling
│   │   │   ├── NotificationService.cs        # Toast notifications
│   │   │   └── TrayIconService.cs            # System tray management
│   │   └── Helpers/
│   │       ├── RegistryHelper.cs             # Context menu registration
│   │       └── CredentialHelper.cs           # Windows Credential Manager
│   │
│   └── Share2GoogleDrive.ShellExtension/     # Optional: Shell extension for context menu
│       └── ...
│
├── tests/
│   └── Share2GoogleDrive.Tests/
│       └── ...
│
├── installer/
│   └── ...                                   # MSIX/WiX installer
│
├── resources/
│   ├── icons/                                # Application icons
│   └── credentials.json                      # Google API credentials (template)
│
├── Share2GoogleDrive.sln
└── README.md
```

## User Stories

### US-01: Upload file via context menu
**As a** user
**I want to** upload a file to Google Drive via right-click
**So that** I can quickly share files without opening the browser

**Acceptance Criteria:**
- [ ] "Send 2 Drive" appears in context menu when right-clicking a file
- [ ] Clicking starts upload to configured folder
- [ ] Progress is shown
- [ ] Notification on completion
- [ ] File opens in browser if setting is enabled

### US-02: Upload file via keyboard shortcut
**As a** user
**I want to** upload a selected file with a keyboard shortcut
**So that** I can upload even faster without clicking

**Acceptance Criteria:**
- [ ] Ctrl+G (or configured key) starts upload
- [ ] Works when file is selected in Explorer
- [ ] Same feedback as context menu upload

### US-03: Connect Google Account
**As a** user
**I want to** connect my Google account via OAuth
**So that** the application has access to my Google Drive

**Acceptance Criteria:**
- [ ] OAuth flow opens in default browser
- [ ] After authorization, account is connected
- [ ] Account info is shown in settings
- [ ] Ability to disconnect

### US-04: Set target folder
**As a** user
**I want to** choose which folder files are uploaded to
**So that** my files stay organized

**Acceptance Criteria:**
- [ ] Folder browser shows Google Drive structure
- [ ] Selected folder is saved
- [ ] New folder can be created

### US-05: Customize keyboard shortcut
**As a** user
**I want to** customize the keyboard shortcut
**So that** I can choose a combination that works for me

**Acceptance Criteria:**
- [ ] Shortcut field in settings
- [ ] Pressing key combination registers new shortcut
- [ ] Warning for conflicting combinations
- [ ] Reset to default available

### US-06: Handle file conflicts
**As a** user
**I want to** be asked what to do when a file already exists
**So that** I don't accidentally overwrite important files

**Acceptance Criteria:**
- [ ] Dialog appears when file with same name exists
- [ ] Options: Replace, Keep both, Cancel
- [ ] Selected action is executed

### US-07: Auto-open after upload
**As a** user
**I want to** have the option to automatically open uploaded files in browser
**So that** I can immediately share or view the file

**Acceptance Criteria:**
- [ ] Setting to enable/disable auto-open
- [ ] When enabled, browser opens to file after successful upload
- [ ] Default is enabled

## Roadmap

### V1 (MVP)
- Context menu integration
- Single file upload
- OAuth authentication
- System tray application
- Keyboard shortcut support
- Settings window
- Target folder selection
- Notifications
- File conflict handling (ask user)
- Auto-open after upload

### V2 (Future)
- Multiple file selection upload
- Folder upload support
- Multiple Google accounts
- Drag & drop to tray icon
- Auto-update functionality

## Technology Stack

### .NET 8.0 with WPF
**Advantages:**
- Native Windows integration
- Excellent performance
- Strong typing and IDE support
- Good Google API library support
- Modern C# features (records, pattern matching, etc.)
- Single-file deployment option

### NuGet Packages

```xml
<PackageReference Include="Google.Apis.Drive.v3" Version="1.68.0" />
<PackageReference Include="Google.Apis.Auth" Version="1.68.0" />
<PackageReference Include="Microsoft.Toolkit.Uwp.Notifications" Version="7.1.3" />
<PackageReference Include="Hardcodet.NotifyIcon.Wpf" Version="1.1.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="CredentialManagement" Version="1.0.2" />
```

## Configuration File Example

```json
{
  "version": "1.0.0",
  "account": {
    "email": "user@gmail.com",
    "connected": true
  },
  "upload": {
    "defaultFolderId": "1ABC123xyz",
    "defaultFolderName": "Uploads",
    "showProgress": true,
    "notifyOnComplete": true,
    "openInBrowserAfterUpload": true
  },
  "hotkey": {
    "enabled": true,
    "modifiers": "Control",
    "key": "G"
  },
  "general": {
    "autostart": true,
    "minimizeToTray": true
  }
}
```

## Context Menu Registry Entry

```
[HKEY_CLASSES_ROOT\*\shell\Send2Drive]
@="Send 2 Drive"
"Icon"="%APPDATA%\\Share2GoogleDrive\\icon.ico"

[HKEY_CLASSES_ROOT\*\shell\Send2Drive\command]
@="\"%APPDATA%\\Share2GoogleDrive\\Share2GoogleDrive.exe\" \"%1\""
```

## Google OAuth Setup

1. Create project in Google Cloud Console
2. Enable Google Drive API
3. Create OAuth 2.0 credentials (Desktop application)
4. Download `credentials.json`
5. Configure redirect URI: `http://localhost:{port}/authorize/`

## Error Handling

| Error | User Message | Action |
|-------|--------------|--------|
| No internet | "No internet connection. Please check your network." | Retry button |
| Auth expired | "Session expired. Please sign in again." | Open auth flow |
| File too large | "File exceeds 5GB limit." | Cancel upload |
| Upload failed | "Upload failed. Retrying..." | Auto-retry (3x) |
| Conflict | "File already exists. What would you like to do?" | Show conflict dialog |
