using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Share2GoogleDrive.Helpers;
using Xunit;

namespace Share2GoogleDrive.Tests.Helpers;

public class ConvertersTests
{
    #region BoolToVisibilityConverter Tests

    [Fact]
    public void BoolToVisibilityConverter_True_ReturnsVisible()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void BoolToVisibilityConverter_False_ReturnsCollapsed()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void BoolToVisibilityConverter_Null_ReturnsCollapsed()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_Visible_ReturnsTrue()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.ConvertBack(Visibility.Visible, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(true, result);
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_Collapsed_ReturnsFalse()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(false, result);
    }

    #endregion

    #region InverseBoolToVisibilityConverter Tests

    [Fact]
    public void InverseBoolToVisibilityConverter_True_ReturnsCollapsed()
    {
        // Arrange
        var converter = new InverseBoolToVisibilityConverter();

        // Act
        var result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void InverseBoolToVisibilityConverter_False_ReturnsVisible()
    {
        // Arrange
        var converter = new InverseBoolToVisibilityConverter();

        // Act
        var result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void InverseBoolToVisibilityConverter_Null_ReturnsVisible()
    {
        // Arrange
        var converter = new InverseBoolToVisibilityConverter();

        // Act
        var result = converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void InverseBoolToVisibilityConverter_ConvertBack_Collapsed_ReturnsTrue()
    {
        // Arrange
        var converter = new InverseBoolToVisibilityConverter();

        // Act
        var result = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(true, result);
    }

    [Fact]
    public void InverseBoolToVisibilityConverter_ConvertBack_Visible_ReturnsFalse()
    {
        // Arrange
        var converter = new InverseBoolToVisibilityConverter();

        // Act
        var result = converter.ConvertBack(Visibility.Visible, typeof(bool), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(false, result);
    }

    #endregion

    #region BoolToStatusConverter Tests

    [Fact]
    public void BoolToStatusConverter_True_ReturnsPlayerReady()
    {
        // Arrange
        var converter = new BoolToStatusConverter();

        // Act
        var result = converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("🌟 Player 1 Ready!", result);
    }

    [Fact]
    public void BoolToStatusConverter_False_ReturnsNoPlayer()
    {
        // Arrange
        var converter = new BoolToStatusConverter();

        // Act
        var result = converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("👻 No Player", result);
    }

    [Fact]
    public void BoolToStatusConverter_Null_ReturnsNoPlayer()
    {
        // Arrange
        var converter = new BoolToStatusConverter();

        // Act
        var result = converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("👻 No Player", result);
    }

    [Fact]
    public void BoolToStatusConverter_ConvertBack_ThrowsNotImplementedException()
    {
        // Arrange
        var converter = new BoolToStatusConverter();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack("🌟 Player 1 Ready!", typeof(bool), null!, CultureInfo.InvariantCulture));
    }

    #endregion

    #region BoolToRegisteredConverter Tests

    [Fact]
    public void BoolToRegisteredConverter_True_ReturnsPipeInstalled()
    {
        // Arrange
        var converter = new BoolToRegisteredConverter();

        // Act
        var result = converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("🟢 Pipe Installed!", result);
    }

    [Fact]
    public void BoolToRegisteredConverter_False_ReturnsNoPipe()
    {
        // Arrange
        var converter = new BoolToRegisteredConverter();

        // Act
        var result = converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("❌ No Pipe", result);
    }

    [Fact]
    public void BoolToRegisteredConverter_Null_ReturnsNoPipe()
    {
        // Arrange
        var converter = new BoolToRegisteredConverter();

        // Act
        var result = converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("❌ No Pipe", result);
    }

    [Fact]
    public void BoolToRegisteredConverter_ConvertBack_ThrowsNotImplementedException()
    {
        // Arrange
        var converter = new BoolToRegisteredConverter();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack("🟢 Pipe Installed!", typeof(bool), null!, CultureInfo.InvariantCulture));
    }

    #endregion

    #region BoolToColorConverter Tests

    [Fact]
    public void BoolToColorConverter_True_ReturnsGreen()
    {
        // Arrange
        var converter = new BoolToColorConverter();

        // Act
        var result = converter.Convert(true, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Brushes.Green, result);
    }

    [Fact]
    public void BoolToColorConverter_False_ReturnsGray()
    {
        // Arrange
        var converter = new BoolToColorConverter();

        // Act
        var result = converter.Convert(false, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Brushes.Gray, result);
    }

    [Fact]
    public void BoolToColorConverter_Null_ReturnsGray()
    {
        // Arrange
        var converter = new BoolToColorConverter();

        // Act
        var result = converter.Convert(null!, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Brushes.Gray, result);
    }

    [Fact]
    public void BoolToColorConverter_ConvertBack_ThrowsNotImplementedException()
    {
        // Arrange
        var converter = new BoolToColorConverter();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack(Brushes.Green, typeof(bool), null!, CultureInfo.InvariantCulture));
    }

    #endregion
}
