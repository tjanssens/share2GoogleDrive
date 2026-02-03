using System.Windows;
using System.Windows.Input;
using Share2GoogleDrive.ViewModels;

namespace Share2GoogleDrive.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private bool _isCapturingHotkey;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += MainWindow_Loaded;
        UpdateHotkeyDisplay();

        // Check if screen is smaller than window, if so make resizable
        AdjustForScreenSize();
    }

    private void AdjustForScreenSize()
    {
        var screenWidth = SystemParameters.WorkArea.Width;
        var screenHeight = SystemParameters.WorkArea.Height;

        if (screenWidth < Width || screenHeight < Height)
        {
            // Screen is smaller than window - allow resizing
            ResizeMode = ResizeMode.CanResizeWithGrip;
            MinWidth = 480;
            MinHeight = 600;

            // Fit to screen with some margin
            if (Width > screenWidth)
                Width = screenWidth - 40;
            if (Height > screenHeight)
                Height = screenHeight - 40;
        }
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
        UpdateHotkeyDisplay();
    }

    private void UpdateHotkeyDisplay()
    {
        var modifiers = _viewModel.HotkeyModifiers;
        var key = _viewModel.HotkeyKey;

        var parts = new List<string>();
        if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
        parts.Add(key.ToString());

        HotkeyTextBox.Text = string.Join(" + ", parts);
    }

    private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!_isCapturingHotkey) return;

        e.Handled = true;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Ignore modifier-only keys
        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftAlt || key == Key.RightAlt ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LWin || key == Key.RWin)
        {
            return;
        }

        var modifiers = Keyboard.Modifiers;

        // Require at least one modifier
        if (modifiers == ModifierKeys.None)
        {
            HotkeyTextBox.Text = "Press a key with Ctrl, Alt, or Shift";
            return;
        }

        _viewModel.HotkeyModifiers = modifiers;
        _viewModel.HotkeyKey = key;

        UpdateHotkeyDisplay();
        _isCapturingHotkey = false;
        HotkeyTextBox.Background = System.Windows.Media.Brushes.White;
    }

    private void HotkeyTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        _isCapturingHotkey = true;
        HotkeyTextBox.Text = "Press your key combination...";
        HotkeyTextBox.Background = System.Windows.Media.Brushes.LightYellow;
    }

    private void HotkeyTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        _isCapturingHotkey = false;
        HotkeyTextBox.Background = System.Windows.Media.Brushes.White;
        UpdateHotkeyDisplay();
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new FolderBrowserDialog(_viewModel);
        if (dialog.ShowDialog() == true && dialog.SelectedFolder != null)
        {
            _viewModel.SetTargetFolder(dialog.SelectedFolder.Id, dialog.SelectedFolder.Name);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Hide instead of close to keep app running in tray
        e.Cancel = true;
        Hide();
    }
}
