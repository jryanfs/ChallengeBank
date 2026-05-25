using System.Reflection;
using ChallengeBank.API.Extensions;
using ChallengeBank.API.Integration;
using ChallengeBank.API.Middleware;
using ChallengeBank.Clients.Infrastructure;
using ChallengeBank.Clients.Infrastructure.Persistence;
using ChallengeBank.Transactions.Application.Abstractions;
using ChallengeBank.Transactions.Infrastructure;
using ChallengeBank.Transactions.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddApiResponseFormat();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationResultHandler>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ChallengeBank API",
        Version = "v1",
        Description = "Unified API — standard envelope: Status, Message, Trace and Data (when applicable)."
    });

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT: Bearer {seu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
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

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

ChallengeBank.Clients.Application.DependencyInjection.AddApplication(builder.Services);
ChallengeBank.Transactions.Application.DependencyInjection.AddApplication(builder.Services);
ChallengeBank.Clients.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);
ChallengeBank.Transactions.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);

builder.Services.AddScoped<IClientExistenceChecker, ClientExistenceChecker>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ClientsDbContext>("clients-db")
    .AddDbContextCheck<TransactionsDbContext>("transactions-db");

var app = builder.Build();

await app.ApplyMigrationsAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiStatusCodeMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "ChallengerBank")
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ChallengeBank API v1");
        options.DocumentTitle = "ChallengeBank — Clientes & Transferências";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
