using Cansat_Dashboard.ApiService.Hubs;
using Cansat_Dashboard.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR and our telemetry service
builder.Services.AddSignalR();
builder.Services.AddHostedService<TelemetryService>();

// *** ADD THIS CORS POLICY ***
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// *** ENABLE THE CORS POLICY ***
app.UseCors();

app.UseHttpsRedirection();

// Map the DashboardHub so the frontend can connect to it
app.MapHub<DashboardHub>("/dashboardHub");

app.MapDefaultEndpoints();

app.Run();

