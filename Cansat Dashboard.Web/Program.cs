using Cansat_Dashboard.Web.Components;
using Cansat_Dashboard.ApiService;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models; // for Swagger




var builder = WebApplication.CreateBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DashboardState service
builder.Services.AddScoped<DashboardState>();

// Configure HttpClient for backend API
builder.Services.AddHttpClient("apiservice", client =>
{
    // This will be resolved by service discovery
    client.BaseAddress = new Uri("https+http://apiservice");
});

var app = builder.Build();

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

// Map default endpoints
app.MapDefaultEndpoints();

app.Run();