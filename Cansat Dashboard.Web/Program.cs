// File: Cansat Dashboard.Web/Program.cs

// Add these using statements at the top
using Cansat_Dashboard.Web.Components;
using Cansat_Dashboard.Web.Hubs;
using Cansat_Dashboard.Web.Services;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("--- 1. WebApplication.CreateBuilder finished. ---");


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
Console.WriteLine("--- 2. AddRazorComponents finished. ---");


// Add SignalR Service
builder.Services.AddSignalR();
Console.WriteLine("--- 3. AddSignalR finished. ---");

builder.Services.AddScoped<DashboardState>();



// Add our background telemetry simulator
builder.Services.AddHostedService<TelemetryService>();
Console.WriteLine("--- 4. AddHostedService<TelemetryService> finished. ---");


// Add Response Compression for SignalR
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
Console.WriteLine("--- 5. AddResponseCompression finished. ---");


var app = builder.Build();
Console.WriteLine("--- 6. builder.Build() finished. ---");


app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map the SignalR Hub endpoint
app.MapHub<DashboardHub>("/dashboardHub");

app.Run();