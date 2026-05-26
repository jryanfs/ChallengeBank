namespace ChallengeBank.Transactions.Application.Abstractions;

public interface ITransferDuplicateGuard
{
    /// <summary>
    /// Reserva a combinação remetente/destinatário/valor na janela configurada (ex.: 5 min).
    /// Retorna <c>true</c> se a transferência pode prosseguir; <c>false</c> se já existe registro recente.
    /// </summary>
    Task<bool> TryRegisterAsync(
        Guid senderUserId,
        Guid receiverUserId,
        decimal amount,
        CancellationToken cancellationToken);

    /// <summary>
    /// Libera a reserva (ex.: falha ao persistir no banco).
    /// </summary>
    Task ReleaseAsync(
        Guid senderUserId,
        Guid receiverUserId,
        decimal amount,
        CancellationToken cancellationToken);
}
