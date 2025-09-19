using Cansat_Dashboard.ApiService.Hubs;
using Cansat_Dashboard.ApiService.Services;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR and our telemetry service
builder.Services.AddSignalR();
builder.Services.AddHostedService<TelemetryService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors();

app.UseHttpsRedirection();

// Map the DashboardHub so the frontend can connect to it
app.MapHub<DashboardHub>("/dashboardHub");

// Map default endpoints
app.MapDefaultEndpoints();

app.Run();