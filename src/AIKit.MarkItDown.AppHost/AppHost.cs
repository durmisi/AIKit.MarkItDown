var builder = DistributedApplication.CreateBuilder(args);

var fastApiApp = builder.AddUvicornApp("MarkItDown", "../AIKit.MarkItDown.Server", "main:app")
    .WithHttpEndpoint(name: "api", port: 8000, env: "PORT")
    .WithHttpHealthCheck("/health");

builder.Build().Run();