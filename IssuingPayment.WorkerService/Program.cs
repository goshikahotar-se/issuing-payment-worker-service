using IssuingPayment.WorkerService;
using Amazon.Runtime;
using Amazon.SQS;
using IssuingPayment.WorkerService.Application.Authorizations.ConsumeAuthorizationEvents;
using IssuingPayment.WorkerService.Infrastructure;
using IssuingPayment.WorkerService.Infrastructure.Authorizations;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IAmazonSQS>(_ =>
    {
        var serverUrl = builder.Configuration["LocalStack:ServerUrl"];
        if (string.IsNullOrEmpty(serverUrl))
            throw new InvalidOperationException("LocalStack:ServerUrl is not set");
        
        var amazonSqsConfig = new AmazonSQSConfig
        {
            ServiceURL = serverUrl,
            AuthenticationRegion = builder.Configuration["LocalStack:Region"] ?? "eu-west-1"
        };

        var credentials = new BasicAWSCredentials(
            "test",
            "test");
        
        return new AmazonSQSClient(credentials, amazonSqsConfig);
    }
);

builder.Services.AddSingleton<IAuthorizationEventHandler, AuthorizationEventHandler>();
builder.Services.AddHostedService<AuthorizationEventSqsListener>();

var host = builder.Build();
host.Run();