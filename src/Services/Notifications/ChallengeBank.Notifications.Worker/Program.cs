using ChallengeBank.Contracts.Notifications;
using ChallengeBank.Notifications.Worker;
using ChallengeBank.Notifications.Worker.Messaging;
using ChallengeBank.Notifications.Worker.Notifications;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<INotificationService, LogNotificationService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
