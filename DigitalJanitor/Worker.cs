using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DigitalJanitor.Interfaces;
using DigitalJanitor.Models;

namespace DigitalJanitor.BackgroundServices;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly JanitorSettings _settings;

    public Worker(ILogger<Worker> logger, IFileSystem fileSystem, IOptions<JanitorSettings> options)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _settings = options.Value;
    }

    // Helper to resolve Linux home directory (~)
    private string ResolvePath(string path) => 
        path.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string watchPath = ResolvePath(_settings.WatchPath);

        if (!_fileSystem.Exists(watchPath))
        {
            _fileSystem.CreateDirectory(watchPath);
        }

        using FileSystemWatcher watcher = new FileSystemWatcher(watchPath);
        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

        watcher.Created += (sender, eventArgs) => OrganizeFile(eventArgs.FullPath);
        watcher.Renamed += (sender, eventArgs) => OrganizeFile(eventArgs.FullPath);

        watcher.EnableRaisingEvents = true;

        _logger.LogInformation("Janitor started. Watching: {Path}", watchPath);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public void OrganizeFile(string filePath)
    {
        try
        {
            string ext = Path.GetExtension(filePath).ToLower();

            // filter out temp files
            if (ext == ".tmp" || ext == ".crdownload") return;

            // checking if the the file is ready
            if (!_fileSystem.IsFileReady(filePath)) return;

            // get the category from appsettings json or defaults to others
            string category = _settings.Extensions.GetValueOrDefault(ext, "Others");

            // set date
            DateTime creationTime = _fileSystem.GetCreationTime(filePath);
            string year = creationTime.Year.ToString();
            string month = creationTime.ToString("MMMM");

            // settings destination directory
            string targetBase = ResolvePath(_settings.TargetBase);
            string destinationDir = Path.Combine(targetBase, category, year, month);
            _fileSystem.CreateDirectory(destinationDir);

            // handling duplicate files
            string fileNameOnly = Path.GetFileNameWithoutExtension(filePath);
            string destPath = Path.Combine(destinationDir, Path.GetFileName(filePath));
            int count = 1;

            while (_fileSystem.Exists(destPath))
            {
                string newFileName = $"{fileNameOnly} ({count}){ext}";
                destPath = Path.Combine(destinationDir, newFileName);
                count++;
            }

            // moving files
            _fileSystem.Move(filePath, destPath, false);
            _logger.LogInformation("Successfully organized: {File} into {Category}", Path.GetFileName(destPath), category);
        }
        catch (Exception exception)
        {
            _logger.LogError("Critical Error processing {File}: {Message}", filePath, exception.Message);
        }
    }
}