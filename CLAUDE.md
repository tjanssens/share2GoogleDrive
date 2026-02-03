# Share2GoogleDrive - Development Guidelines

## Project Overview
WPF desktop application (.NET 8.0) for uploading files to Google Drive via hotkey, context menu, or drag-and-drop. The entire UI follows a **Super Mario Bros Nintendo theme**.

## Build & Run
```bash
cd src/Share2GoogleDrive
dotnet build
dotnet run
```

## Mario Theme Design Guidelines

### Color Palette (defined in App.xaml)
| Name | Hex | Usage |
|------|-----|-------|
| MarioRed | #E52521 | Headers, important text, danger accents |
| MarioBlue | #049CD8 | Links, secondary actions |
| MarioYellow | #FBD000 | Highlights, loading overlays, notifications |
| MarioGreen | #43B047 | Success states, primary action buttons (pipes) |
| BlockBrown | #8B4513 | Borders, ground elements, footer |
| SkyBlue | #5C94FC | Window backgrounds (like Mario sky) |
| CloudWhite | #FFFEF8E8 | Content area backgrounds |

### Button Styles
- **MarioButton**: Standard brown button with yellow text
- **PipeButton**: Green "pipe" style for primary/positive actions (Sign In, Save, Select)
- **DangerButton**: Red button for destructive actions (Sign Out, Remove)

### UI Components
- **GroupBox**: Brown border with rounded corners, cream background
- **TextBox**: Brown border, white background
- **CheckBox**: Custom Mario-style with green checkmark

### Text & Messaging Style

#### Window Titles
Use castle/world metaphors with emojis:
- Main window: "Mario's Cloud Castle - Settings"
- Folder browser: "Select Your Castle"
- Conflict dialog: "Item Already in Castle!"

#### Labels & UI Text
Use game metaphors:
- Google account → "Player Account"
- Connected → "Player 1 Ready!"
- Not connected → "No Player"
- Sign In → "Enter Game"
- Sign Out → "Exit Game"
- Upload settings → "Delivery Settings"
- Target folder → "Target Castle"
- Create folder → "Build Castle"
- Hotkey → "Power-Up Shortcut"
- Context menu → "Right-Click Power"
- Notifications → "Power-up notifications"
- Save → "Save Game"
- Loading → "Exploring world..."

#### Error Messages
Always start with "Mamma Mia!" for errors:
- "Mamma Mia! {error details}"
- Title: "Game Over" or "Build Failed"

#### Success Messages
Use celebratory Mario expressions:
- "Yahoo! {success details}"
- "Let's-a go!"

#### Warning/Prompt Messages
Use friendly Mario-style prompts:
- "Hey! {instruction}"
- "Pick a castle to enter first!"

#### Emoji Usage
Use emojis consistently in UI:
- Folders/locations: castle
- World/root: globe
- Loading/search: magnifying glass
- Success: star, sparkles
- Error: skull
- Settings: gear
- Player: game controller
- Notifications: mushroom (power-up)

### Icons & Animation
- **App icon**: Custom Mario-themed icon (mario_icon.ico)
- **Tray icon**: Animated during uploads using sprite frames
- **Animation frames**: Located in Resources/mario_frame_*.png

### Window Behavior
- Main window: 520x900, non-resizable unless screen is smaller
- Dialogs: Center on screen, bring to front on show
- Close button hides to tray instead of closing app

### Registry/System Integration
- Context menu text: "Mario Delivery!" (no emoji - registry doesn't support it)
- Tray tooltip: "Mario's Cloud Castle"

## Project Structure
```
src/Share2GoogleDrive/
├── App.xaml              # Styles, colors, converters
├── Views/
│   ├── MainWindow.xaml   # Settings screen
│   ├── FolderBrowserDialog.xaml  # Folder picker
│   └── ConflictDialog.xaml       # File conflict resolution
├── ViewModels/
│   └── MainViewModel.cs  # Main app logic
├── Services/
│   ├── GoogleDriveService.cs
│   ├── NotificationService.cs
│   ├── TrayIconService.cs
│   └── UploadService.cs
├── Resources/
│   ├── mario_icon.ico
│   └── mario_frame_*.png (animation)
└── Helpers/
    └── Converters.cs     # Bool to status/color converters
```

## Code Conventions
- Use MVVM pattern with CommunityToolkit.Mvvm
- Status messages in ViewModel should follow Mario text style
- All user-facing strings should use Mario theme vocabulary
- Toast notifications use Microsoft.Toolkit.Uwp.Notifications
