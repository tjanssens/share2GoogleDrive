using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Serilog;

namespace Share2GoogleDrive.Services;

/// <summary>
/// Service for registering and handling global hotkeys.
/// </summary>
public interface IHotkeyService : IDisposable
{
    event EventHandler? HotkeyPressed;
    bool Register(ModifierKeys modifiers, Key key);
    void Unregister();
    bool IsRegistered { get; }
}

public class HotkeyService : IHotkeyService
{
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID = 9000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private HwndSource? _source;
    private IntPtr _windowHandle;
    private bool _isRegistered;

    public event EventHandler? HotkeyPressed;
    public bool IsRegistered => _isRegistered;

    public bool Register(ModifierKeys modifiers, Key key)
    {
        try
        {
            Unregister();

            // Create a hidden window for receiving hotkey messages
            var window = Application.Current?.MainWindow;
            if (window == null)
            {
                // Create a helper window if main window doesn't exist
                window = new Window
                {
                    Width = 0,
                    Height = 0,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false
                };
                window.Show();
                window.Hide();
            }

            var helper = new WindowInteropHelper(window);
            _windowHandle = helper.EnsureHandle();

            _source = HwndSource.FromHwnd(_windowHandle);
            _source?.AddHook(HwndHook);

            uint mod = 0;
            if (modifiers.HasFlag(ModifierKeys.Alt)) mod |= 0x0001;
            if (modifiers.HasFlag(ModifierKeys.Control)) mod |= 0x0002;
            if (modifiers.HasFlag(ModifierKeys.Shift)) mod |= 0x0004;
            if (modifiers.HasFlag(ModifierKeys.Windows)) mod |= 0x0008;

            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

            _isRegistered = RegisterHotKey(_windowHandle, HOTKEY_ID, mod, vk);

            if (_isRegistered)
            {
                Log.Information("Hotkey registered: {Modifiers}+{Key}", modifiers, key);
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                Log.Warning("Failed to register hotkey, error code: {Error}", error);
            }

            return _isRegistered;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering hotkey");
            return false;
        }
    }

    public void Unregister()
    {
        if (_isRegistered && _windowHandle != IntPtr.Zero)
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            _isRegistered = false;
            Log.Information("Hotkey unregistered");
        }

        if (_source != null)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
        }
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            Log.Debug("Hotkey pressed");
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
    }
}
