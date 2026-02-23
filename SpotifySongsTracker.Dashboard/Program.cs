using SpotifySongsTracker.Dashboard.Hubs;
using SpotifySongsTracker.Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<SpotifyStatsService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<StatsHub>("/statsHub");

app.Run();
