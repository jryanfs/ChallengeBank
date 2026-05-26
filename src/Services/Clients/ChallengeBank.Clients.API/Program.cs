using System.Reflection;
using ChallengeBank.Api.Shared.Extensions;
using ChallengeBank.Api.Shared.Middleware;
using ChallengeBank.Clients.API.Extensions;
using ChallengeBank.Clients.Infrastructure;
using ChallengeBank.Clients.Infrastructure.Persistence;
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
        Title = "ChallengeBank — Clientes API",
        Version = "v1",
        Description = "Microsserviço de clientes. Envelope: Status, Message, Trace, Data."
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

ChallengeBank.Clients.Application.DependencyInjection.AddApplication(builder.Services);
ChallengeBank.Clients.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ClientsDbContext>("clients-db");

var app = builder.Build();

await app.ApplyClientsMigrationsAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiStatusCodeMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "ChallengerBank")
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Clientes API v1");
        options.DocumentTitle = "ChallengeBank — Clientes";
    });
}

if (!app.Environment.IsEnvironment("Testing") && !app.Environment.IsEnvironment("Docker"))
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
