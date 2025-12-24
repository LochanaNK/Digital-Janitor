using DigitalJanitor.BackgroundServices;
using DigitalJanitor.Interfaces;


var builder = Host.CreateApplicationBuilder(args);

// 1. Register the real FileSystem for production use
builder.Services.AddSingleton<IFileSystem, PhysicalFileSystem>();

// 2. Register the Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();