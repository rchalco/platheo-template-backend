using VectorStinger.Infrastructure.DataAccess.Manager;
using Xunit;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using VectorStinger.Infrastructure.DataAccess.Interface;

public class ImageManagerTests
{
    [Fact]
    public void SaveImagePath_And_GetImageBase64WithPrefix_Works()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "TestImages");
        var loggerMock = new Mock<ILogger<IImageManager>>();
        var manager = new ImageManager(tempDir, loggerMock.Object);
        var fileName = "test.jpg";
        var folder = "TestFolder";
        var imageBytes = new byte[] { 255, 216, 255, 224 }; // JPEG header

        // Act
        manager.SaveImagePath(imageBytes, fileName, folder);
        var base64 = manager.GetImageBase64WithPrefix(fileName, folder);

        // Assert
        Assert.StartsWith("data:image/jpeg;base64,", base64);

        // Cleanup
        Directory.Delete(Path.Combine(tempDir, folder), true);
    }
}