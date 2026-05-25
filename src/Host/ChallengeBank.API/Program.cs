using System.Reflection;
using ChallengeBank.API.Extensions;
using ChallengeBank.Clients.Application;
using ChallengeBank.Clients.Infrastructure;
using ChallengeBank.Clients.Infrastructure.Persistence;
using ChallengeBank.Transactions.Application;
using ChallengeBank.Transactions.Infrastructure;
using ChallengeBank.Transactions.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ChallengeBank API",
        Version = "v1",
        Description = "API unificada do desafio bancário — módulos de **Clientes** e **Transações** em um único host e banco de dados."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.TagActionsBy(api =>
    {
        var tag = api.ActionDescriptor.EndpointMetadata
            .OfType<TagsAttribute>()
            .SelectMany(t => t.Tags)
            .FirstOrDefault();

        return tag is null ? [] : [tag];
    });

    options.DocInclusionPredicate((_, _) => true);
});

ChallengeBank.Clients.Application.DependencyInjection.AddApplication(builder.Services);
ChallengeBank.Transactions.Application.DependencyInjection.AddApplication(builder.Services);
ChallengeBank.Clients.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);
ChallengeBank.Transactions.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ClientsDbContext>("clients-db")
    .AddDbContextCheck<TransactionsDbContext>("transactions-db");

var app = builder.Build();

await app.ApplyMigrationsAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ChallengeBank API v1");
        options.DocumentTitle = "ChallengeBank — Clientes & Transações";
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
