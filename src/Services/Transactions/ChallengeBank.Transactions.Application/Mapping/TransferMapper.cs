using ChallengeBank.Transactions.Application.DTOs;
using ChallengeBank.Transactions.Domain.Entities;

namespace ChallengeBank.Transactions.Application.Mapping;

public static class TransferMapper
{
    public static TransferDto ToDto(Transfer transfer) =>
        new(
            transfer.Id,
            transfer.SenderUserId,
            transfer.ReceiverUserId,
            transfer.Amount,
            transfer.Description,
            transfer.Status,
            transfer.CreatedAtUtc,
            transfer.UpdatedAtUtc);
}
