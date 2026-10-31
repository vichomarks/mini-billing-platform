using MiniBilling.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.MapGet("/health", () => "OK");

app.Run();