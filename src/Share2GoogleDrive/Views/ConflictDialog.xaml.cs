using System.Windows;
using Share2GoogleDrive.Models;

namespace Share2GoogleDrive.Views;

public partial class ConflictDialog : Window
{
    public ConflictResolution Result { get; private set; } = ConflictResolution.Cancel;

    public ConflictDialog(string fileName)
    {
        InitializeComponent();
        FileNameText.Text = fileName;

        // Bring window to front when shown
        Loaded += (s, e) =>
        {
            Topmost = true;
            Activate();
            Focus();
            // Reset topmost after a moment so other windows can go on top if needed
            Dispatcher.BeginInvoke(new Action(() => Topmost = false),
                System.Windows.Threading.DispatcherPriority.Background);
        };
    }

    private void Replace_Click(object sender, RoutedEventArgs e)
    {
        Result = ConflictResolution.Replace;
        DialogResult = true;
        Close();
    }

    private void KeepBoth_Click(object sender, RoutedEventArgs e)
    {
        Result = ConflictResolution.KeepBoth;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Result = ConflictResolution.Cancel;
        DialogResult = false;
        Close();
    }
}
