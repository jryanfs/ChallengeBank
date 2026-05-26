using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChallengeBank.Transactions.API.Json;

/// <summary>
/// Trata string vazia ou ausente como null para Guid?, permitindo validação em português via FluentValidation.
/// </summary>
public sealed class NullableGuidJsonConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => ParseString(reader.GetString()),
            _ => throw new JsonException("Formato de identificador inválido.")
        };
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value.ToString());
    }

    private static Guid? ParseString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Guid.TryParse(value, out var guid) ? guid : throw new JsonException("Formato de identificador inválido.");
    }
}
