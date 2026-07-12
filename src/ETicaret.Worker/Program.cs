using MassTransit;
using ETicaret.Worker.Consumers;
using ETicaret.Business;
using ETicaret.Data;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=ETicaret;Username=ETicaret_user;Password=dev_password";

ETicaret.Data.ServiceRegistration.AddDataLayer(builder.Services, connectionString);
ETicaret.Business.DependencyInjection.AddBusinessServices(builder.Services);

builder.Services.AddSingleton<ETicaret.Worker.Services.OllamaService>();

// Firebase başlatma
var firebaseCredPath = builder.Configuration["Firebase:CredentialPath"];
if (!string.IsNullOrEmpty(firebaseCredPath) && File.Exists(firebaseCredPath))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebaseCredPath)
    });
}

// MassTransit v8 — RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedConsumer>();
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<OrderShippedConsumer>();
    x.AddConsumer<RefundApprovedConsumer>();
    x.AddConsumer<SendNotificationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var mqHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var mqUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var mqPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(mqHost, "/", h =>
        {
            h.Username(mqUser);
            h.Password(mqPass);
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHostedService<ETicaret.Worker.Services.NotificationCleanupWorker>();

var host = builder.Build();
host.Run();

