using Log.Api.Consumers;
using MassTransit;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(), "logs/log.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductAddedEvent>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitmq = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
        cfg.ReceiveEndpoint("log-product-events-queue", e =>
        {
            e.ConfigureConsumer<ProductAddedEvent>(context);
        });
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();