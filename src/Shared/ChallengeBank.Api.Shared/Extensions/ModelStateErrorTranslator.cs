namespace ChallengeBank.Api.Shared.Extensions;

internal static class ModelStateErrorTranslator
{
    public static string Translate(string fieldKey, string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return $"O campo '{SimplifyFieldName(fieldKey)}' é inválido.";

        if (errorMessage.Contains("could not be converted", StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains("Formato de identificador inválido", StringComparison.OrdinalIgnoreCase))
        {
            return TranslateGuidField(fieldKey);
        }

        if (fieldKey.Equals("request", StringComparison.OrdinalIgnoreCase)
            && errorMessage.Contains("required", StringComparison.OrdinalIgnoreCase))
        {
            return "O corpo da requisição é inválido. Verifique remetente, destinatário e valor.";
        }

        if (errorMessage.Contains("required", StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains("é obrigatório", StringComparison.OrdinalIgnoreCase))
        {
            return TranslateGuidField(fieldKey);
        }

        return errorMessage;
    }

    private static string TranslateGuidField(string fieldKey)
    {
        var field = SimplifyFieldName(fieldKey);

        if (field.Contains("receiverUserId", StringComparison.OrdinalIgnoreCase)
            || field.Contains("ReceiverUserId", StringComparison.Ordinal))
            return "O identificador do destinatário é obrigatório.";

        if (field.Contains("senderUserId", StringComparison.OrdinalIgnoreCase)
            || field.Contains("SenderUserId", StringComparison.Ordinal))
            return "O identificador do remetente é obrigatório.";

        return $"O campo '{field}' é inválido.";
    }

    private static string SimplifyFieldName(string fieldKey)
    {
        if (fieldKey.StartsWith("$.", StringComparison.Ordinal))
            return fieldKey[2..];

        if (fieldKey.StartsWith("request.", StringComparison.OrdinalIgnoreCase))
            return fieldKey["request.".Length..];

        return fieldKey;
    }
}
