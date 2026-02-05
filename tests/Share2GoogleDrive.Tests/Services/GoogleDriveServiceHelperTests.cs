using System.Reflection;
using Share2GoogleDrive.Services;
using Xunit;

namespace Share2GoogleDrive.Tests.Services;

/// <summary>
/// Tests for pure helper methods in GoogleDriveService.
/// These methods are private static, so we use reflection to test them.
/// </summary>
public class GoogleDriveServiceHelperTests
{
    private static string GetMimeType(string filePath)
    {
        var method = typeof(GoogleDriveService)
            .GetMethod("GetMimeType", BindingFlags.NonPublic | BindingFlags.Static);

        return (string)method!.Invoke(null, new object[] { filePath })!;
    }

    private static string EscapeQuery(string value)
    {
        var method = typeof(GoogleDriveService)
            .GetMethod("EscapeQuery", BindingFlags.NonPublic | BindingFlags.Static);

        return (string)method!.Invoke(null, new object[] { value })!;
    }

    #region GetMimeType Tests - Documents

    [Fact]
    public void GetMimeType_Pdf_ReturnsCorrectMime()
    {
        Assert.Equal("application/pdf", GetMimeType("document.pdf"));
    }

    [Fact]
    public void GetMimeType_Doc_ReturnsCorrectMime()
    {
        Assert.Equal("application/msword", GetMimeType("document.doc"));
    }

    [Fact]
    public void GetMimeType_Docx_ReturnsCorrectMime()
    {
        Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            GetMimeType("document.docx"));
    }

    [Fact]
    public void GetMimeType_Xls_ReturnsCorrectMime()
    {
        Assert.Equal("application/vnd.ms-excel", GetMimeType("spreadsheet.xls"));
    }

    [Fact]
    public void GetMimeType_Xlsx_ReturnsCorrectMime()
    {
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            GetMimeType("spreadsheet.xlsx"));
    }

    [Fact]
    public void GetMimeType_Ppt_ReturnsCorrectMime()
    {
        Assert.Equal("application/vnd.ms-powerpoint", GetMimeType("presentation.ppt"));
    }

    [Fact]
    public void GetMimeType_Pptx_ReturnsCorrectMime()
    {
        Assert.Equal("application/vnd.openxmlformats-officedocument.presentationml.presentation",
            GetMimeType("presentation.pptx"));
    }

    #endregion

    #region GetMimeType Tests - Text Files

    [Fact]
    public void GetMimeType_Txt_ReturnsCorrectMime()
    {
        Assert.Equal("text/plain", GetMimeType("readme.txt"));
    }

    [Fact]
    public void GetMimeType_Csv_ReturnsCorrectMime()
    {
        Assert.Equal("text/csv", GetMimeType("data.csv"));
    }

    [Fact]
    public void GetMimeType_Html_ReturnsCorrectMime()
    {
        Assert.Equal("text/html", GetMimeType("page.html"));
    }

    [Fact]
    public void GetMimeType_Htm_ReturnsCorrectMime()
    {
        Assert.Equal("text/html", GetMimeType("page.htm"));
    }

    #endregion

    #region GetMimeType Tests - Data Files

    [Fact]
    public void GetMimeType_Xml_ReturnsCorrectMime()
    {
        Assert.Equal("application/xml", GetMimeType("config.xml"));
    }

    [Fact]
    public void GetMimeType_Json_ReturnsCorrectMime()
    {
        Assert.Equal("application/json", GetMimeType("data.json"));
    }

    #endregion

    #region GetMimeType Tests - Archives

    [Fact]
    public void GetMimeType_Zip_ReturnsCorrectMime()
    {
        Assert.Equal("application/zip", GetMimeType("archive.zip"));
    }

    [Fact]
    public void GetMimeType_Rar_ReturnsCorrectMime()
    {
        Assert.Equal("application/x-rar-compressed", GetMimeType("archive.rar"));
    }

    [Fact]
    public void GetMimeType_7z_ReturnsCorrectMime()
    {
        Assert.Equal("application/x-7z-compressed", GetMimeType("archive.7z"));
    }

    #endregion

    #region GetMimeType Tests - Images

    [Fact]
    public void GetMimeType_Png_ReturnsCorrectMime()
    {
        Assert.Equal("image/png", GetMimeType("image.png"));
    }

    [Fact]
    public void GetMimeType_Jpg_ReturnsCorrectMime()
    {
        Assert.Equal("image/jpeg", GetMimeType("image.jpg"));
    }

    [Fact]
    public void GetMimeType_Jpeg_ReturnsCorrectMime()
    {
        Assert.Equal("image/jpeg", GetMimeType("image.jpeg"));
    }

    [Fact]
    public void GetMimeType_Gif_ReturnsCorrectMime()
    {
        Assert.Equal("image/gif", GetMimeType("animation.gif"));
    }

    [Fact]
    public void GetMimeType_Bmp_ReturnsCorrectMime()
    {
        Assert.Equal("image/bmp", GetMimeType("image.bmp"));
    }

    [Fact]
    public void GetMimeType_Svg_ReturnsCorrectMime()
    {
        Assert.Equal("image/svg+xml", GetMimeType("vector.svg"));
    }

    #endregion

    #region GetMimeType Tests - Audio/Video

    [Fact]
    public void GetMimeType_Mp3_ReturnsCorrectMime()
    {
        Assert.Equal("audio/mpeg", GetMimeType("song.mp3"));
    }

    [Fact]
    public void GetMimeType_Mp4_ReturnsCorrectMime()
    {
        Assert.Equal("video/mp4", GetMimeType("video.mp4"));
    }

    [Fact]
    public void GetMimeType_Avi_ReturnsCorrectMime()
    {
        Assert.Equal("video/x-msvideo", GetMimeType("video.avi"));
    }

    [Fact]
    public void GetMimeType_Mov_ReturnsCorrectMime()
    {
        Assert.Equal("video/quicktime", GetMimeType("video.mov"));
    }

    #endregion

    #region GetMimeType Tests - Unknown/Edge Cases

    [Fact]
    public void GetMimeType_Unknown_ReturnsOctetStream()
    {
        Assert.Equal("application/octet-stream", GetMimeType("file.unknown"));
    }

    [Fact]
    public void GetMimeType_NoExtension_ReturnsOctetStream()
    {
        Assert.Equal("application/octet-stream", GetMimeType("filename"));
    }

    [Fact]
    public void GetMimeType_UpperCaseExtension_ReturnsCorrectMime()
    {
        // Should handle case-insensitively
        Assert.Equal("application/pdf", GetMimeType("DOCUMENT.PDF"));
    }

    [Fact]
    public void GetMimeType_MixedCaseExtension_ReturnsCorrectMime()
    {
        Assert.Equal("application/pdf", GetMimeType("Document.Pdf"));
    }

    [Fact]
    public void GetMimeType_FullPath_ReturnsCorrectMime()
    {
        Assert.Equal("application/pdf", GetMimeType(@"C:\Users\Test\Documents\file.pdf"));
    }

    [Fact]
    public void GetMimeType_MultipleExtensions_ReturnsCorrectMime()
    {
        // Should use the last extension
        Assert.Equal("application/zip", GetMimeType("file.tar.zip"));
    }

    #endregion

    #region EscapeQuery Tests

    [Fact]
    public void EscapeQuery_SingleQuote_Escaped()
    {
        // Arrange
        var input = "test'file.txt";

        // Act
        var result = EscapeQuery(input);

        // Assert
        Assert.Equal(@"test\'file.txt", result);
    }

    [Fact]
    public void EscapeQuery_Backslash_Escaped()
    {
        // Arrange
        var input = @"test\file.txt";

        // Act
        var result = EscapeQuery(input);

        // Assert
        Assert.Equal(@"test\\file.txt", result);
    }

    [Fact]
    public void EscapeQuery_Combined_AllEscaped()
    {
        // Arrange
        var input = @"test's\file.txt";

        // Act
        var result = EscapeQuery(input);

        // Assert
        Assert.Equal(@"test\'s\\file.txt", result);
    }

    [Fact]
    public void EscapeQuery_NoSpecialCharacters_Unchanged()
    {
        // Arrange
        var input = "normalfilename.txt";

        // Act
        var result = EscapeQuery(input);

        // Assert
        Assert.Equal("normalfilename.txt", result);
    }

    [Fact]
    public void EscapeQuery_MultipleQuotes_AllEscaped()
    {
        // Arrange
        var input = "file'with'many'quotes.txt";

        // Act
        var result = EscapeQuery(input);

        // Assert
        Assert.Equal(@"file\'with\'many\'quotes.txt", result);
    }

    [Fact]
    public void EscapeQuery_MultipleBackslashes_AllEscaped()
    {
        // Arrange
        var input = @"path\to\file.txt";

        // Act
        var result = EscapeQuery(input);

        // Assert
        Assert.Equal(@"path\\to\\file.txt", result);
    }

    [Fact]
    public void EscapeQuery_EmptyString_ReturnsEmpty()
    {
        // Act
        var result = EscapeQuery(string.Empty);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void EscapeQuery_OnlyQuote_Escaped()
    {
        // Arrange
        var input = "'";

        // Act
        var result = EscapeQuery(input);

        // Assert
        Assert.Equal(@"\'", result);
    }

    [Fact]
    public void EscapeQuery_OnlyBackslash_Escaped()
    {
        // Arrange
        var input = @"\";

        // Act
        var result = EscapeQuery(input);

        // Assert
        Assert.Equal(@"\\", result);
    }

    [Fact]
    public void EscapeQuery_BackslashBeforeQuote_BothEscaped()
    {
        // Arrange
        var input = @"\'";

        // Act
        var result = EscapeQuery(input);

        // Assert - backslash becomes \\, quote becomes \', result is \\\'
        Assert.Equal(@"\\\'", result);
    }

    #endregion

    #region GetMimeType - All Extensions Coverage

    [Theory]
    [InlineData(".pdf", "application/pdf")]
    [InlineData(".doc", "application/msword")]
    [InlineData(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData(".xls", "application/vnd.ms-excel")]
    [InlineData(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData(".ppt", "application/vnd.ms-powerpoint")]
    [InlineData(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    [InlineData(".txt", "text/plain")]
    [InlineData(".csv", "text/csv")]
    [InlineData(".html", "text/html")]
    [InlineData(".htm", "text/html")]
    [InlineData(".xml", "application/xml")]
    [InlineData(".json", "application/json")]
    [InlineData(".zip", "application/zip")]
    [InlineData(".rar", "application/x-rar-compressed")]
    [InlineData(".7z", "application/x-7z-compressed")]
    [InlineData(".png", "image/png")]
    [InlineData(".jpg", "image/jpeg")]
    [InlineData(".jpeg", "image/jpeg")]
    [InlineData(".gif", "image/gif")]
    [InlineData(".bmp", "image/bmp")]
    [InlineData(".svg", "image/svg+xml")]
    [InlineData(".mp3", "audio/mpeg")]
    [InlineData(".mp4", "video/mp4")]
    [InlineData(".avi", "video/x-msvideo")]
    [InlineData(".mov", "video/quicktime")]
    public void GetMimeType_AllExtensions_Covered(string extension, string expectedMime)
    {
        // Act
        var result = GetMimeType($"testfile{extension}");

        // Assert
        Assert.Equal(expectedMime, result);
    }

    #endregion
}
