using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DigitalJanitor.Interfaces;

namespace DigitalJanitor.BackgroundServices;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IFileSystem _fileSystem;

    private readonly string _watchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    private readonly string _targetBase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Organized");

    private readonly Dictionary<string, string> _extensions = new()
    {
        {".pdf", "Documents"},
        {".docx", "Documents"},
        {".xlsx", "Documents"},
        {".txt", "Documents"},
        {".html", "WebFiles"},
        {".jpg", "Images"},
        {".jpeg", "Images"},
        {".jfif", "Images"},
        {".pjpeg", "Images"},
        {".pjp", "Images"},
        {".png", "Images"},
        {".apng", "Images"},
        {".svg", "Images"},
        {".webp", "Images"},
        {".avif", "Images"},
        {".gif", "Images"},
        {".mp4", "Videos"},
        {".mp3", "Audio"},
        {".zip", "Archives"},
        {".rar", "Archives"},
        {".7z", "Archives"},
        {".tar.gz", "Archives"},
        {".gz", "Archives"},
        {".bz2", "Archives"},
        {".exe", "Applications"},
        {".apk", "Applications"},
    };


    public Worker(ILogger<Worker> logger, IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
    }    

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_fileSystem.Exists(_watchPath))
        {
            _fileSystem.CreateDirectory(_watchPath);
        }
        using FileSystemWatcher watcher = new FileSystemWatcher(_watchPath);

        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

        watcher.Created += (sender, eventArgs) => OrganizeFile(eventArgs.FullPath);
        watcher.Renamed += (sender, eventArgs) => OrganizeFile(eventArgs.FullPath);

        watcher.EnableRaisingEvents = true;

        _logger.LogInformation("Janitor started. Watching: {Path}", _watchPath);

        //keeping the service until stopped
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public void OrganizeFile(string filePath)
    {
        try
        {
            string ext = Path.GetExtension(filePath).ToLower();

            if (ext == ".tmp" || ext == ".crdownload")
            {
                return;
            }

            try
            {
                if (!_fileSystem.IsFileReady(filePath))
                {
                    return;
                }
                string category = _extensions.GetValueOrDefault(ext, "Others");

                DateTime creationTime = _fileSystem.GetCreationTime(filePath);
                string year = creationTime.Year.ToString();
                string month = creationTime.ToString("MMMM");

                string destinationDir = Path.Combine(_targetBase, category, year, month);
                _fileSystem.CreateDirectory(destinationDir);

                string destPath = Path.Combine(destinationDir, Path.GetFileName(filePath));

                _fileSystem.Move(filePath, destPath, false);
                _logger.LogInformation("Moved file: {File} to {Dest}", Path.GetFileName(filePath), category);
            }
            catch (Exception exception)
            {
                _logger.LogError("Could not move {File}: {Message}", Path.GetFileName(filePath), exception.Message);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("Error {Message}", exception.Message);
        }
    }

    private bool WaitForFile(string path)
    {
        int retires = 5;
        while (retires > 0)
        {
            try
            {
                using FileStream fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            catch
            {
                retires--;
                Thread.Sleep(1000);
            }
        }
        return false;
    }
}