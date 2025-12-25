using DigitalJanitor.BackgroundServices;
using DigitalJanitor.Interfaces;
using DigitalJanitor.Models;


var builder = Host.CreateApplicationBuilder(args);

//binding JSON section to the janitor settings class
builder.Services.Configure<JanitorSettings>(
    builder.Configuration.GetSection("JanitorSettings")
);

// 1. Register the real FileSystem for production use
builder.Services.AddSingleton<IFileSystem, PhysicalFileSystem>();

// 2. Register the Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();