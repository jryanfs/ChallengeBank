using ChallengeBank.API.Api;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeBank.API.Extensions;

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
                        string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? $"O campo '{x.Key}' é inválido."
                            : e.ErrorMessage))
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
