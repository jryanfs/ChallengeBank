# ChallengeBank — Microsserviços Bancários



Solução em **C# 12** / **.NET 8** com **Clean Architecture**, **SOLID** e separação por bounded contexts (Clientes e Transações), com **API unificada** e **banco de dados único**.



## Estrutura da solução



```

ChallengeBank/

├── src/

│   ├── BuildingBlocks/              # Abstrações compartilhadas (DDD + CQRS)

│   ├── Host/

│   │   └── ChallengeBank.API        # API única + Swagger unificado

│   └── Services/

│       ├── Clients/                 # Bounded context Clientes

│       │   ├── Domain / Application / Infrastructure

│       └── Transactions/          # Bounded context Transações

│           ├── Domain / Application / Infrastructure

├── tests/

└── challengerbank/                  # SQL Server em container (opcional)

```



## Banco de dados



Um único banco **`ChallengeBank`** no SQL Server, com schemas separados:



| Schema | Tabelas |

|--------|---------|

| `clients` | `Clients` |

| `transactions` | `Transactions` |



Connection string: `ChallengeBankDb` em `src/Host/ChallengeBank.API/appsettings.json`



## Camadas (Clean Architecture)



| Camada | Responsabilidade |

|--------|------------------|

| **Domain** | Entidades, enums, regras de negócio, interfaces de repositório |

| **Application** | Commands/Queries (CQRS), handlers, DTOs, `Result` pattern |

| **Infrastructure** | EF Core, `DbContext`, repositórios, migrations |

| **API (Host)** | Controllers, DI, Swagger unificado, health checks |



## Pré-requisitos



- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

- **SQL Server LocalDB** (padrão) ou **ChallengerBank** — ver `challengerbank/README.md`



## Como executar



### Opção A — LocalDB (padrão)



```bash

sqllocaldb start MSSQLLocalDB

dotnet run --project src/Host/ChallengeBank.API

```



### Opção B — SQL Server (ChallengerBank)



```bash

cd challengerbank && docker compose up -d

dotnet run --project src/Host/ChallengeBank.API --launch-profile ChallengerBank

```



No Visual Studio: perfil **ChallengerBank** em `ChallengeBank.API`. Detalhes em `challengerbank/README.md`.



Em **Development** e **ChallengerBank**, as migrations são aplicadas automaticamente ao iniciar a API.



**Instância SQL Server nativa:** copie `appsettings.Local.example.json` → `appsettings.Local.json` na pasta `ChallengeBank.API`.



### Migrations (manual, se necessário)



```bash

dotnet ef database update -p src/Services/Clients/ChallengeBank.Clients.Infrastructure -s src/Host/ChallengeBank.API

dotnet ef database update -p src/Services/Transactions/ChallengeBank.Transactions.Infrastructure -s src/Host/ChallengeBank.API

```



> `dotnet tool install --global dotnet-ef`



### 3. Rodar a API (Visual Studio 2022)



**Projeto de inicialização:** `ChallengeBank.API` (`src/Host/ChallengeBank.API`)



```bash

dotnet run --project src/Host/ChallengeBank.API

```



| Recurso | URL |

|---------|-----|

| Swagger (tudo em um) | http://localhost:5000/swagger |

| Health check | http://localhost:5000/health |



### 4. Testes



```bash

dotnet test

```



## Postman



Importe `docs/postman/ChallengeBank.postman_collection.json` — base URL única: `http://localhost:5000`



## Endpoints



- `POST /api/clients` — cadastrar cliente

- `GET /api/clients/{id}` — consultar cliente

- `POST /api/transactions` — registrar transação

- `GET /api/transactions/{id}` — consultar transação

- `GET /health` — health check (ambos os DbContexts)



## Stack



| Item | Tecnologia |

|------|------------|

| Linguagem | C# 12 |

| Framework | .NET 8 |

| Banco | SQL Server (banco único `ChallengeBank`) |

| ORM | Entity Framework Core 8 |


