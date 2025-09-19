using Cansat_Dashboard.Web.Components;
using Cansat_Dashboard.Web.Hubs;
using Cansat_Dashboard.Web.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);

WebApplicationBuilder builder1 = builder;
// builder.AddServiceDefaults();

builder1.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder1.Services.AddSignalR();
builder1.Services.AddScoped<DashboardState>();
builder1.Services.AddHostedService<TelemetryService>();
builder1.Services.AddResponseCompression(opts =>
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