var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Cansat_Dashboard_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Cansat_Dashboard_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
