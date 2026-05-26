using ChallengeBank.Notifications.Worker;
using ChallengeBank.Notifications.Worker.Extensions;
using ChallengeBank.Notifications.Worker.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddNotificationServices(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.LogNotificationConfiguration();
host.Run();
