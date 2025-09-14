using Cansat_Dashboard.Web.Components;
using Cansat_Dashboard.Web.Services;
using Cansat_Dashboard.Web.Hubs;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults - if you're using .NET Aspire
// builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR
builder.Services.AddSignalR();

// Add DashboardState service
builder.Services.AddScoped<DashboardState>();

// Add TelemetryService
builder.Services.AddHostedService<TelemetryService>();

// Add Response Compression for SignalR
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

var app = builder.Build();

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

// Map SignalR Hub
app.MapHub<DashboardHub>("/dashboardHub");

// Map default endpoints if using Aspire
// app.MapDefaultEndpoints();

app.Run();