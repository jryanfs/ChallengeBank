using ChallengeBank.Api.Shared.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ChallengeBank.Api.Shared.Extensions;

public static class MvcExtensions
{
    public static IServiceCollection AddApiResponseFormat(this IServiceCollection services)
    {
        services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
            options.JsonSerializerOptions.DictionaryKeyPolicy = null;
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e =>
                        ModelStateErrorTranslator.Translate(x.Key, e.ErrorMessage)))
                    .Distinct()
                    .ToList();

                var message = ApiMessages.ValidationErrors(errors);
                var envelope = ApiEnvelope.Create(
                    context.HttpContext,
                    StatusCodes.Status400BadRequest,
                    message,
                    errors);

                return new BadRequestObjectResult(envelope);
            };
        });

        return services;
    }
}
