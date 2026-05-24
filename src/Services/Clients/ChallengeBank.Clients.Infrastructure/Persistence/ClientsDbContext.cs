using ChallengeBank.BuildingBlocks.Application.Interfaces;
using ChallengeBank.Clients.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBank.Clients.Infrastructure.Persistence;

public sealed class ClientsDbContext(DbContextOptions<ClientsDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Client> Clients => Set<Client>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("clients");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClientsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
