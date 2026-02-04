# Share2GoogleDrive Installer

This directory contains the Inno Setup script and resources for building the Windows installer.

## Prerequisites

- [Inno Setup 6.x](https://jrsoftware.org/isdl.php) installed
- .NET 8.0 SDK for building the application

## Building the Installer

### 1. Publish the Application

First, publish the application using the appropriate profile:

```bash
# Framework-dependent (requires .NET 8.0 runtime on target machine, ~15MB)
dotnet publish ../src/Share2GoogleDrive -c Release -r win-x64 --self-contained false -o ../publish/framework

# Self-contained (no dependencies, ~100MB)
dotnet publish ../src/Share2GoogleDrive -c Release -r win-x64 --self-contained true -o ../publish/standalone
```

### 2. Build the Installer

Using Inno Setup Compiler (ISCC):

```bash
# Framework-dependent installer
iscc share2googledrive.iss /DPublishType=Framework

# Self-contained installer
iscc share2googledrive.iss /DPublishType=Standalone
```

Or open `share2googledrive.iss` in Inno Setup and compile from the GUI.

### 3. Output

Installers will be created in the `../dist/` directory:
- `Share2GoogleDrive-Setup-1.0.0.exe` - Framework-dependent
- `Share2GoogleDrive-Setup-1.0.0-Standalone.exe` - Self-contained

## Installer Graphics

The installer uses Mario-themed graphics:

- **setup-wizard.bmp**: Left panel image (164x314 pixels)
  - Sky blue background (#5C94FC)
  - Mario character and clouds

- **setup-header.bmp**: Header image (150x57 pixels)
  - Small Mario icon with app name

### Creating Custom Graphics

If you need to recreate the graphics:

1. Use the Mario theme colors from `App.xaml`:
   - Sky Blue: #5C94FC (background)
   - Mario Red: #E52521
   - Mario Blue: #049CD8
   - Mario Yellow: #FBD000
   - Mario Green: #43B047
   - Block Brown: #8B4513
   - Cloud White: #F8E8D0

2. Size requirements:
   - Wizard image: 164x314 pixels (BMP format)
   - Header image: 150x57 pixels (BMP format)

## Installer Features

- **Per-user installation**: No admin rights required
- **Start Menu shortcuts**: Created automatically
- **Desktop shortcut**: Optional
- **Launch after install**: Optional
- **Mario-themed UI**: Custom welcome messages and colors
- **Clean uninstall**: Removes registry entries for context menu and autostart
- **Preserves settings**: User settings in %APPDATA% are kept after uninstall

## Registry Entries Managed

The installer handles cleanup of these registry entries on uninstall:

- Context menu: `HKCU\Software\Classes\*\shell\Send2Drive`
- Autostart: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\Share2GoogleDrive`

## Framework-Dependent vs Self-Contained

| Feature | Framework-Dependent | Self-Contained |
|---------|---------------------|----------------|
| Size | ~15 MB | ~100 MB |
| .NET Required | Yes (.NET 8.0 Desktop Runtime) | No |
| Updates | App updates smaller | Full app in each update |
| Best for | Users with .NET installed | Maximum compatibility |

The installer automatically checks for .NET 8.0 when using the framework-dependent version and shows a friendly error message if it's not installed.
