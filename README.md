# Share2GoogleDrive

A Windows desktop application that allows you to quickly upload files to Google Drive via the context menu or a keyboard shortcut. Features a fun Super Mario Bros theme!

## Features

- **Context Menu Integration**: Right-click any file and select "Mario Delivery!" to upload
- **Keyboard Shortcut**: Use `Ctrl+G` (configurable) to upload the selected file
- **Google OAuth**: Secure authentication with your Google account
- **Target Folder Selection**: Choose which Google Drive folder to upload to
- **Auto-Open**: Optionally open uploaded files in browser automatically
- **File Conflict Handling**: Choose to replace, keep both, or cancel when a file exists
- **System Tray**: Runs quietly in the background with animated Mario icon
- **Mario Theme**: Fun Nintendo-inspired UI throughout the app

## Requirements

- Windows 10/11
- Google account

## Installation

### Download Installer (Recommended)

Download the latest release from the [Releases page](https://github.com/tjanssens/share2GoogleDrive/releases):

| Installer | Size | Requirements |
|-----------|------|--------------|
| `Share2GoogleDrive-Setup-x.x.x.exe` | ~15 MB | [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |
| `Share2GoogleDrive-Setup-x.x.x-Standalone.exe` | ~100 MB | None (includes .NET runtime) |

**Which one should I download?**
- If you already have .NET 8.0 installed (or don't mind installing it), use the smaller installer
- If you want a hassle-free installation with no dependencies, use the Standalone version

### Installation Steps

1. Download the installer
2. Run the setup wizard (no admin rights required!)
3. Follow the Mario-themed installation steps
4. Launch Share2GoogleDrive from the Start Menu or Desktop

### From Source

1. Clone the repository
2. Open `Share2GoogleDrive.sln` in Visual Studio 2022
3. Build and run

```bash
cd src/Share2GoogleDrive
dotnet build
dotnet run
```

## Setup

### Google API Credentials

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project
3. Enable the Google Drive API
4. Create OAuth 2.0 credentials (Desktop application)
5. Download the credentials and save as `credentials.json`
6. Place it in `%APPDATA%\Share2GoogleDrive\`

### First Run

1. Launch the application
2. Click "Sign In" to authenticate with Google
3. Select your target folder
4. Click "Register" to add the context menu entry
5. Optionally configure the keyboard shortcut

## Usage

### Via Context Menu

1. Right-click on any file in Windows Explorer
2. Click "Send 2 Drive"
3. The file will be uploaded to your configured folder

### Via Keyboard Shortcut

1. Select a file in Windows Explorer
2. Press `Ctrl+G` (or your configured shortcut)
3. The file will be uploaded

## Settings

- **Google Account**: Sign in/out of your Google account
- **Target Folder**: Select which folder to upload files to
- **Open in Browser**: Automatically open files after upload
- **Keyboard Shortcut**: Configure the upload hotkey
- **Context Menu**: Register/remove the right-click menu entry
- **Autostart**: Start with Windows

## File Structure

```
%APPDATA%\Share2GoogleDrive\
├── settings.json       # Application settings
├── credentials.json    # Google API credentials (you provide)
├── token/              # OAuth tokens (auto-generated)
└── logs/               # Application logs
```

## Troubleshooting

### "credentials.json not found"

Download your OAuth credentials from Google Cloud Console and place the file in `%APPDATA%\Share2GoogleDrive\`.

### Hotkey not working

- Make sure another application isn't using the same hotkey
- Try a different key combination in settings

### Context menu not appearing

- Run the application and click "Register" in settings
- If it still doesn't work, try running as administrator

## License

MIT License

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
