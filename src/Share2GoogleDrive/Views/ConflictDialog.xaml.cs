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
