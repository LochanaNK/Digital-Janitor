using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DigitalJanitor.Interfaces;
using DigitalJanitor.BackgroundServices;

namespace DigitalJanitor.Tests;

public class UnitTest1
{
    [Fact]
    public void OrganizeFile_MovesPdfToDocumentsFolder()
    {
        // 1. ARRANGE
        var mockFS = new Mock<IFileSystem>();
        var mockLogger = new Mock<ILogger<Worker>>();
        
        string testFile = "/home/lnk/Downloads/test.pdf";
        mockFS.Setup(f => f.IsFileReady(testFile)).Returns(true);
        
        // Setup the mock to return a specific date when called
        mockFS.Setup(f => f.GetCreationTime(testFile))
              .Returns(new DateTime(2025, 12, 1));
        
        // Passing TWO arguments to the constructor now
        var worker = new Worker(mockLogger.Object, mockFS.Object);

        // 2. ACT
        worker.OrganizeFile(testFile); // Method must be PUBLIC in Worker.cs

        // 3. ASSERT
        // Path.Combine is safer than hardcoding strings for cross-platform tests
        string expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            "Organized", "Documents", "2025", "December", "test.pdf");

        mockFS.Verify(f => f.Move(testFile, expectedPath, false), Times.Once);
    }
}