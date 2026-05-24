# ChallengeBank — Microsserviços Bancários

Solução em **C# 12** / **.NET 8** com **Clean Architecture**, **SOLID** e separação por bounded contexts para o desafio técnico de ambiente bancário (Clientes e Transações).

## Estrutura da solução

```
ChallengeBank/
├── src/
│   ├── BuildingBlocks/              # Abstrações compartilhadas (DDD + CQRS)
│   │   ├── ChallengeBank.BuildingBlocks.Domain
│   │   └── ChallengeBank.BuildingBlocks.Application
│   └── Services/
│       ├── Clients/                 # Microsserviço de Clientes
│       │   ├── ChallengeBank.Clients.Domain
│       │   ├── ChallengeBank.Clients.Application
│       │   ├── ChallengeBank.Clients.Infrastructure   # EF Core + SQL Server
│       │   └── ChallengeBank.Clients.API
│       └── Transactions/            # Microsserviço de Transações
│           ├── ChallengeBank.Transactions.Domain
│           ├── ChallengeBank.Transactions.Application
│           ├── ChallengeBank.Transactions.Infrastructure
│           └── ChallengeBank.Transactions.API
├── tests/
│   ├── Clients/
│   └── Transactions/
└── docker/
    └── docker-compose.yml           # SQL Server (um banco por serviço)
```

## Camadas (Clean Architecture)

| Camada | Responsabilidade |
|--------|------------------|
| **Domain** | Entidades, enums, regras de negócio, interfaces de repositório |
| **Application** | Commands/Queries (CQRS), handlers, DTOs, `Result` pattern |
| **Infrastructure** | EF Core, `DbContext`, repositórios, migrations |
| **API** | Controllers finos, DI, Swagger, health checks |

### Padrões aplicados

- **DDD**: `AggregateRoot`, `Entity`, `ValueObject`, `DomainException`
- **CQRS**: `ICommand` / `IQuery` com handlers dedicados
- **Repository**: abstração no Domain, implementação na Infrastructure
- **Unit of Work**: `IUnitOfWork` via `DbContext`
- **Result**: respostas de aplicação sem exceções para fluxo de negócio esperado

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (o SDK 9 também compila projetos `net8.0`)
- Docker (opcional, para SQL Server local)

## Como executar

### 1. Subir os bancos (Docker)

```bash
cd docker
docker compose up -d
```

- **Clientes**: `localhost:1433` → database `ChallengeBank_Clients`
- **Transações**: `localhost:1434` → database `ChallengeBank_Transactions`

### 2. Aplicar migrations

```bash
dotnet ef migrations add InitialCreate -p src/Services/Clients/ChallengeBank.Clients.Infrastructure -s src/Services/Clients/ChallengeBank.Clients.API
dotnet ef database update -p src/Services/Clients/ChallengeBank.Clients.Infrastructure -s src/Services/Clients/ChallengeBank.Clients.API

dotnet ef migrations add InitialCreate -p src/Services/Transactions/ChallengeBank.Transactions.Infrastructure -s src/Services/Transactions/ChallengeBank.Transactions.API
dotnet ef database update -p src/Services/Transactions/ChallengeBank.Transactions.Infrastructure -s src/Services/Transactions/ChallengeBank.Transactions.API
```

> Instale a ferramenta global se necessário: `dotnet tool install --global dotnet-ef`

### 3. Rodar as APIs

```bash
dotnet run --project src/Services/Clients/ChallengeBank.Clients.API
dotnet run --project src/Services/Transactions/ChallengeBank.Transactions.API
```

| Serviço | HTTP | Swagger |
|---------|------|---------|
| Clientes | http://localhost:5001 | /swagger |
| Transações | http://localhost:5002 | /swagger |

### 4. Testes

```bash
dotnet test
```

## Endpoints iniciais

**Clientes**

- `POST /api/clients` — cadastrar cliente
- `GET /api/clients/{id}` — consultar cliente
- `GET /health` — health check

**Transações**

- `POST /api/transactions` — registrar transação
- `GET /api/transactions/{id}` — consultar transação
- `GET /health` — health check

## Próximos passos sugeridos (desafio)

- [ ] Validação com FluentValidation
- [ ] Comunicação assíncrona entre serviços (mensageria / integration events)
- [ ] API Gateway ou BFF
- [ ] Autenticação/autorização (JWT)
- [ ] Observabilidade (OpenTelemetry, logs estruturados)
- [ ] Testes de integração e arquitetura
- [ ] CI/CD e containers por microsserviço

## Stack

| Item | Tecnologia |
|------|------------|
| Linguagem | C# 12 |
| Framework | .NET 8 |
| Banco | SQL Server |
| ORM | Entity Framework Core 8 |
