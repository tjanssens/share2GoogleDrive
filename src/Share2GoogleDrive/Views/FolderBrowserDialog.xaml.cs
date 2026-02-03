using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Share2GoogleDrive.Models;
using Share2GoogleDrive.Services;
using Share2GoogleDrive.ViewModels;
using Serilog;

namespace Share2GoogleDrive.Views;

public partial class FolderBrowserDialog : Window
{
    private readonly IGoogleDriveService _driveService;
    private FolderTreeItem? _selectedItem;

    public DriveFolder? SelectedFolder { get; private set; }

    public FolderBrowserDialog(MainViewModel viewModel)
    {
        InitializeComponent();

        // Get drive service from the app
        _driveService = ((App)Application.Current).DriveService;

        Loaded += FolderBrowserDialog_Loaded;
    }

    private async void FolderBrowserDialog_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadFoldersAsync();
    }

    private async Task LoadFoldersAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;

        try
        {
            var folders = await _driveService.GetFoldersAsync(null);

            var rootItems = new ObservableCollection<FolderTreeItem>
            {
                new FolderTreeItem
                {
                    Id = null,
                    Name = "🌍 Mario's World",
                    HasChildren = true,
                    IsExpanded = true,
                    Children = new ObservableCollection<FolderTreeItem>(
                        folders.Select(f => new FolderTreeItem
                        {
                            Id = f.Id,
                            Name = f.Name,
                            HasChildren = f.HasChildren
                        }))
                }
            };

            FolderTree.ItemsSource = rootItems;

            // Set up lazy loading for child folders
            foreach (var item in rootItems)
            {
                SetupLazyLoading(item);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load folders");
            MessageBox.Show($"Mamma Mia! {ex.Message}", "💀 Game Over",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void SetupLazyLoading(FolderTreeItem item)
    {
        if (item.HasChildren && item.Children.Count == 0)
        {
            // Add placeholder
            item.Children.Add(new FolderTreeItem { Name = "🔍 Exploring...", IsPlaceholder = true });
        }

        item.PropertyChanged += async (s, e) =>
        {
            if (e.PropertyName == nameof(FolderTreeItem.IsExpanded) &&
                item.IsExpanded &&
                item.Children.Count == 1 &&
                item.Children[0].IsPlaceholder)
            {
                await LoadChildFoldersAsync(item);
            }
        };

        foreach (var child in item.Children.Where(c => !c.IsPlaceholder))
        {
            SetupLazyLoading(child);
        }
    }

    private async Task LoadChildFoldersAsync(FolderTreeItem parent)
    {
        try
        {
            var folders = await _driveService.GetFoldersAsync(parent.Id);

            parent.Children.Clear();
            foreach (var folder in folders)
            {
                var child = new FolderTreeItem
                {
                    Id = folder.Id,
                    Name = folder.Name,
                    HasChildren = folder.HasChildren
                };
                SetupLazyLoading(child);
                parent.Children.Add(child);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load child folders for {ParentId}", parent.Id);
            parent.Children.Clear();
            parent.Children.Add(new FolderTreeItem { Name = "💀 Oops! Try again", IsPlaceholder = true });
        }
    }

    private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _selectedItem = e.NewValue as FolderTreeItem;
    }

    private async void CreateFolder_Click(object sender, RoutedEventArgs e)
    {
        var folderName = NewFolderNameTextBox.Text.Trim();
        if (string.IsNullOrEmpty(folderName))
        {
            MessageBox.Show("Hey! You need to name your castle first!", "🏗️ Build Castle",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        LoadingOverlay.Visibility = Visibility.Visible;

        try
        {
            var parentId = _selectedItem?.Id;
            var newFolder = await _driveService.CreateFolderAsync(folderName, parentId);

            // Add to tree
            if (_selectedItem != null)
            {
                _selectedItem.Children.Add(new FolderTreeItem
                {
                    Id = newFolder.Id,
                    Name = newFolder.Name,
                    HasChildren = false
                });
                _selectedItem.IsExpanded = true;
            }

            NewFolderNameTextBox.Clear();
            MessageBox.Show($"Yahoo! Castle '{folderName}' has been built!", "🎉 New Castle!",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create folder");
            MessageBox.Show($"Mamma Mia! Castle construction failed: {ex.Message}", "💀 Build Failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedItem == null || _selectedItem.IsPlaceholder)
        {
            MessageBox.Show("Hey! Pick a castle to enter first!", "🏰 Select Castle",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedFolder = new DriveFolder
        {
            Id = _selectedItem.Id ?? string.Empty,
            Name = _selectedItem.Name
        };

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

public class FolderTreeItem : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool HasChildren { get; set; }
    public bool IsPlaceholder { get; set; }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public ObservableCollection<FolderTreeItem> Children { get; set; } = new();
}
