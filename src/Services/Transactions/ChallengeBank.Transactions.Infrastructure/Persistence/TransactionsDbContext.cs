using ChallengeBank.BuildingBlocks.Application.Interfaces;
using ChallengeBank.Transactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBank.Transactions.Infrastructure.Persistence;

public sealed class TransactionsDbContext(DbContextOptions<TransactionsDbContext> options)
    : DbContext(options), ITransactionsUnitOfWork
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("transactions");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
