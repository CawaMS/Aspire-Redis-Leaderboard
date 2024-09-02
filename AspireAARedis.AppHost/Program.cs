var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
                   //.PublishAsConnectionString();

var apiService = builder.AddProject<Projects.AspireAARedis_ApiService>("apiservice")
                        .WithReference(cache);

builder.AddProject<Projects.AspireAARedis_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WithReference(apiService);

builder.Build().Run();
