using ChallengeBank.API.Api;
using ChallengeBank.BuildingBlocks.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeBank.API.Extensions;

public static class ApiResponseExtensions
{
    public static IActionResult ToApiResult<T>(
        this ControllerBase controller,
        Result<T> result,
        string successMessage,
        int successStatusCode = StatusCodes.Status200OK) =>
        result.IsSuccess
            ? controller.ToJsonResponse(successStatusCode, successMessage, result.Value)
            : MapError(controller, result.Error);

    public static IActionResult ToCreatedApiResult<T>(
        this ControllerBase controller,
        Result<T> result,
        string actionName,
        Func<T, object> routeValuesFactory,
        string successMessage,
        Func<T, object?> responseDataFactory) =>
        result.IsSuccess
            ? controller.CreatedAtAction(
                actionName,
                routeValuesFactory(result.Value),
                ApiEnvelope.Create(
                    controller.HttpContext,
                    StatusCodes.Status201Created,
                    successMessage,
                    responseDataFactory(result.Value)))
            : MapError(controller, result.Error);

    public static IActionResult ToJsonResponse(
        this ControllerBase controller,
        int statusCode,
        string message,
        object? data = null) =>
        new ObjectResult(ApiEnvelope.Create(controller.HttpContext, statusCode, message, data))
        {
            StatusCode = statusCode,
            ContentTypes = { "application/json" }
        };

    private static IActionResult MapError(ControllerBase controller, Error error)
    {
        var message = ApiMessages.FromError(error);

        var statusCode = error.Code switch
        {
            var code when code.StartsWith("Client.NotFound", StringComparison.Ordinal)
                || code.StartsWith("Transfer.NotFound", StringComparison.Ordinal)
                || code.StartsWith("Transfer.SenderNotFound", StringComparison.Ordinal)
                || code.StartsWith("Transfer.ReceiverNotFound", StringComparison.Ordinal)
                || code.StartsWith("Transfer.UserNotFound", StringComparison.Ordinal) => StatusCodes.Status404NotFound,

            var code when code.StartsWith("Transfer.SenderRequired", StringComparison.Ordinal)
                || code.StartsWith("Transfer.ReceiverRequired", StringComparison.Ordinal)
                || code.StartsWith("Transfer.UserRequired", StringComparison.Ordinal)
                || code.StartsWith("Transfer.Invalid", StringComparison.Ordinal) => StatusCodes.Status400BadRequest,

            var code when code.StartsWith("Client.DocumentExists", StringComparison.Ordinal) => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status400BadRequest
        };

        return controller.ToJsonResponse(statusCode, message);
    }
}
