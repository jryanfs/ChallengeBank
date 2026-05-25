using ChallengeBank.BuildingBlocks.Application.Common;

namespace ChallengeBank.API.Api;

public static class ApiMessages
{
    public const string LoginSuccess = "Autenticação realizada com sucesso.";
    public const string LoginInvalid = "Usuário ou senha inválidos.";
    public const string Unauthorized = "Token ausente ou inválido. Realize o login para obter um novo token.";
    public const string Forbidden = "Você não tem permissão para executar esta operação.";
    public const string ValidationFailed = "Os dados enviados são inválidos.";
    public const string InternalError = "Ocorreu um erro interno. Tente novamente mais tarde.";
    public const string EndpointNotFound = "O endpoint solicitado não foi encontrado.";
    public const string MethodNotAllowed = "O método HTTP não é permitido para este endpoint.";

    public const string ClientCreated = "Cliente cadastrado com sucesso.";
    public const string ClientRetrieved = "Cliente consultado com sucesso.";
    public const string ClientUpdated = "Cliente atualizado com sucesso.";
    public const string ClientNotFound = "Cliente não encontrado.";
    public const string TransferCreated = "Transferência realizada com sucesso.";
    public const string TransferRetrieved = "Transferência consultada com sucesso.";
    public const string TransferNotFound = "Transferência não encontrada.";
    public const string TransfersListed = "Transferências listadas com sucesso.";

    public const string TransferSenderRequired = "O identificador do remetente é obrigatório.";
    public const string TransferReceiverRequired = "O identificador do destinatário é obrigatório.";
    public const string TransferUserRequired = "O identificador do usuário é obrigatório.";

    public static string FromError(Error error) =>
        error.Code switch
        {
            "Client.NotFound" => string.IsNullOrWhiteSpace(error.Message) ? ClientNotFound : error.Message,
            "Client.DocumentExists" => "Já existe um cliente cadastrado com este documento.",
            "Transfer.NotFound" => string.IsNullOrWhiteSpace(error.Message) ? TransferNotFound : error.Message,
            "Transfer.SenderNotFound" => string.IsNullOrWhiteSpace(error.Message)
                ? "Cliente remetente não encontrado."
                : error.Message,
            "Transfer.ReceiverNotFound" => string.IsNullOrWhiteSpace(error.Message)
                ? "Cliente destinatário não encontrado."
                : error.Message,
            "Transfer.UserNotFound" => string.IsNullOrWhiteSpace(error.Message)
                ? "Usuário não encontrado."
                : error.Message,
            "Transfer.SenderRequired" => TransferSenderRequired,
            "Transfer.ReceiverRequired" => TransferReceiverRequired,
            "Transfer.UserRequired" => TransferUserRequired,
            "Transfer.Invalid" => string.IsNullOrWhiteSpace(error.Message)
                ? "Transferência inválida. Verifique os dados informados."
                : error.Message,
            _ when error.Code.StartsWith("Client.", StringComparison.Ordinal) => error.Message,
            _ when error.Code.StartsWith("Transfer.", StringComparison.Ordinal) => error.Message,
            _ => string.IsNullOrWhiteSpace(error.Message) ? ValidationFailed : error.Message
        };

    public static string ValidationErrors(IEnumerable<string> errors) =>
        errors.Any()
            ? $"Validação falhou: {string.Join(" ", errors)}"
            : ValidationFailed;
}
