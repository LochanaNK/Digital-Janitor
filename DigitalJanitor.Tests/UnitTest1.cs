using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using DigitalJanitor.Interfaces;
using DigitalJanitor.BackgroundServices;
using DigitalJanitor.Models;

namespace DigitalJanitor.Tests;

public class UnitTest1
{
    [Fact]
    public void OrganizeFile_MovesPdfToDocumentsFolder()
    {
        // 1. ARRANGE
        var mockFS = new Mock<IFileSystem>();
        var mockLogger = new Mock<ILogger<Worker>>();
        var mockOptions = new Mock<IOptions<JanitorSettings>>();

        // Create the settings the test will use
        var settings = new JanitorSettings
        {
            WatchPath = "/home/lnk/Downloads",
            TargetBase = "/home/lnk/Organized",
            Extensions = new Dictionary<string, string> { { ".pdf", "Documents" } }
        };

        // Tell the mock to return these settings when the Worker asks for them
        mockOptions.Setup(o => o.Value).Returns(settings);

        string testFile = "/home/lnk/Downloads/test.pdf";
        
        mockFS.Setup(f => f.IsFileReady(testFile)).Returns(true);
        mockFS.Setup(f => f.GetCreationTime(testFile)).Returns(new DateTime(2025, 12, 1));
        
        // 2. ACT - Now passing THREE arguments
        var worker = new Worker(mockLogger.Object, mockFS.Object, mockOptions.Object);
        worker.OrganizeFile(testFile);

        // 3. ASSERT
        string expectedPath = Path.Combine("/home/lnk/Organized", "Documents", "2025", "December", "test.pdf");
        mockFS.Verify(f => f.Move(testFile, expectedPath, false), Times.Once);
    }
}