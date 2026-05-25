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

| `transactions` | `Transfers` |



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



Inclui testes de **domínio** e **integração** (`tests/Integration/`) — fluxo cliente e fluxo transferência com JWT.




## Formato de resposta (envelope)

Todas as rotas da API (exceto `/health`) retornam:

```json
{
  "Status": 200,
  "Message": "Cliente consultado com sucesso.",
  "Trace": "0HN7...",
  "Data": { }
}
```

Propriedades em inglês (`Status`, `Message`, `Trace`, `Data`); textos de `Message` em português. Erros **401**, **403**, **404**, **409** e **400** usam o mesmo envelope (ex.: transferência inexistente informa o id).

## Autenticação (JWT + RBAC)



| Usuário | Senha | Role |
|---------|-------|------|
| `user` | `User@123` | User |
| `admin` | `Admin@123` | Admin |



1. `POST /api/auth/login` com `{ "username", "password" }` → retorna `accessToken`
2. Envie `Authorization: Bearer {accessToken}` nas demais rotas (exceto `/health` e login)



**RBAC nas rotas existentes:**

| Rota | User | Admin |
|------|------|-------|
| POST/GET `/api/clients` | Sim | Sim |
| PATCH `/api/clients/{id}` | Não (403) | Sim |
| POST/GET `/api/transfers` | Sim | Sim |
| GET `/api/transfers/user/{userId}` | Não (403) | Sim |
| GET `/health` | Anônimo | Anônimo |



## Postman



Importe `docs/postman/ChallengeBank.postman_collection.json` e o environment `ChallengeBank.Local.postman_environment.json` — execute **Login (admin)** antes do fluxo.



## Endpoints



- `POST /api/auth/login` — obter JWT

- `POST /api/clients` — cadastrar cliente (opcional: `address`, `bankingDetails`) — requer JWT

- `GET /api/clients/{id}` — consultar cliente (com dados bancários)

- `PATCH /api/clients/{id}` — atualização parcial (`name`, `email`, `address`, `bankingDetails`)

- `POST /api/transfers` — criar transferência (`senderUserId`, `receiverUserId`, `amount`, `description`) → retorna `{ id, status }`

- `GET /api/transfers/{id}` — detalhe da transferência

- `GET /api/transfers/user/{userId}` — lista de transferências do usuário

- `GET /health` — health check (ambos os DbContexts)



## Stack



| Item | Tecnologia |

|------|------------|

| Linguagem | C# 12 |

| Framework | .NET 8 |

| Banco | SQL Server (banco único `ChallengeBank`) |

| ORM | Entity Framework Core 8 |


