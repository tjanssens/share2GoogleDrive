# Share2GoogleDrive

A Windows desktop application that allows you to quickly upload files to Google Drive via the context menu or a keyboard shortcut.

## Features

- **Context Menu Integration**: Right-click any file and select "Send 2 Drive" to upload
- **Keyboard Shortcut**: Use `Ctrl+G` (configurable) to upload the selected file
- **Google OAuth**: Secure authentication with your Google account
- **Target Folder Selection**: Choose which Google Drive folder to upload to
- **Auto-Open**: Optionally open uploaded files in browser automatically
- **File Conflict Handling**: Choose to replace, keep both, or cancel when a file exists
- **System Tray**: Runs quietly in the background

## Requirements

- Windows 10/11
- .NET 8.0 Runtime
- Google account

## Installation

### From Release

1. Download the latest release from the Releases page
2. Run the installer or extract the portable version
3. Launch Share2GoogleDrive

### From Source

1. Clone the repository
2. Open `Share2GoogleDrive.sln` in Visual Studio 2022
3. Build and run

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
