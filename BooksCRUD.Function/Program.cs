using Azure.Storage.Blobs;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults() // sets up Azure Functions runtime
    .ConfigureServices(services =>
    {
        // Register BlobServiceClient so it can be injected into your Function
        services.AddSingleton(_ =>
            new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage")));
    })
    .Build();

host.Run();
