using System.Reflection;
using ChallengeBank.Api.Shared.Extensions;
using ChallengeBank.Api.Shared.Middleware;
using ChallengeBank.Transactions.API.Extensions;
using ChallengeBank.Transactions.API.Hosting;
using ChallengeBank.Transactions.API.Json;
using ChallengeBank.Transactions.Infrastructure;
using ChallengeBank.Transactions.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new NullableGuidJsonConverter());
    });
builder.Services.AddApiResponseFormat();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationResultHandler>();
if (!builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddClientsServiceHttpClient(builder.Configuration, builder.Environment);
else
    builder.Services.AddHttpContextAccessor();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ChallengeBank — Transferências API",
        Version = "v1",
        Description = "Microsserviço de transferências. Consulta Clientes via HTTP com Polly (Retry, Circuit Breaker, Timeout)."
    });

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
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
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

ChallengeBank.Transactions.Application.DependencyInjection.AddApplication(builder.Services);
ChallengeBank.Transactions.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<TransactionsDbContext>("transactions-db");

var app = builder.Build();

await app.ApplyTransactionsMigrationsAsync();

TransferDuplicateGuardDiagnostics.LogRegisteredGuard(app);

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiStatusCodeMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "ChallengerBank")
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Transferências API v1");
        options.DocumentTitle = "ChallengeBank — Transferências";
    });
}

if (!app.Environment.IsEnvironment("Testing") && !app.Environment.IsEnvironment("Docker"))
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
