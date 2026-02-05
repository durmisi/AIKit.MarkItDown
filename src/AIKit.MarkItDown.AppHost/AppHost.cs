var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AIKit_MarkItDown_Test>("aikit-markitdown-test");


var fastApiApp = builder.AddUvicornApp("MarkItDown", "./AIKit.MarkItDown.Api", "main:app")
    .WithUv()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");


builder.Build().Run();
