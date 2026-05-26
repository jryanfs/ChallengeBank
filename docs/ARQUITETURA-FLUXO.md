# Arquitetura e fluxos — ChallengeBank

Visão do sistema **como está na `main`**: dois microsserviços HTTP, worker de notificações, SQL Server, Redis, RabbitMQ e SendGrid.

---

## 1. Visão de componentes (infraestrutura)

```mermaid
flowchart TB
    subgraph Cliente["Cliente / Postman"]
        U[Usuário ou Admin]
    end

    subgraph APIs["Microsserviços .NET 8"]
        CA[Clients API<br/>:7101 HTTPS]
        TA[Transactions API<br/>:7102 HTTPS]
        NW[Notifications Worker<br/>background]
    end

    subgraph Dados["Persistência e cache"]
        SQL[(SQL Server<br/>DB ChallengeBank)]
        R[("Redis")]
    end

    subgraph Mensageria["Mensageria"]
        RMQ>RabbitMQ<br/>exchange challengebank.events]
    end

    subgraph Externo["Externo"]
        SG[SendGrid API<br/>e-mail]
    end

    U -->|JWT Bearer| CA
    U -->|JWT Bearer| TA
    CA -->|schema clients| SQL
    TA -->|schema transactions| SQL
    CA <-->|cache GET / invalida PATCH| R
    TA <-->|anti-duplicata POST transfer| R
    CA -->|publish bankingDetails| RMQ
    RMQ -->|consume fila| NW
    NW --> SG
    TA -->|GET /api/clients/id<br/>Polly Retry/CB/Timeout| CA
```

| Componente | Porta / acesso (Docker) | Papel |
|------------|---------------------------|--------|
| **Clients API** | https://localhost:7101 | Cadastro, consulta, PATCH cliente |
| **Transactions API** | https://localhost:7102 | Transferências P2P |
| **Notifications Worker** | — (sem HTTP público) | Consome RabbitMQ, envia e-mail |
| **SQL Server** | localhost:14333 | Banco único, schemas `clients` e `transactions` |
| **Redis** | localhost:6379 | Cache cliente + deduplicação de transferência |
| **RabbitMQ** | :5672 / UI :15672 | Eventos assíncronos |
| **SendGrid** | API HTTPS | Entrega de e-mail |

---

## 2. Autenticação (JWT compartilhado)

As duas APIs usam o mesmo `Jwt:Audience` = `ChallengeBank`. Login em qualquer uma gera token válido nas duas.

```mermaid
sequenceDiagram
    actor User as Cliente Postman
    participant CA as Clients API
    participant TA as Transactions API

    User->>CA: POST /api/auth/login user/password
    CA-->>User: 200 envelope + accessToken
    Note over User: Authorization Bearer em todas as rotas

    User->>TA: POST /api/transfers + Bearer
    Note over TA: Repassa Bearer ao chamar Clients
    TA->>CA: GET /api/clients/{id} + Bearer
    CA-->>TA: 200 ou 404
```

| Usuário | Senha | Role | PATCH cliente | Lista transfers/user |
|---------|-------|------|---------------|----------------------|
| `user` | `User@123` | User | 403 | 403 |
| `admin` | `Admin@123` | Admin | 200 | 200 |

---

## 3. Clientes — GET com cache Redis

```mermaid
sequenceDiagram
    participant API as Clients API
    participant R as Redis
    participant SQL as SQL Server

  API->>R: GET cache clients:{id}
    alt cache hit TTL 120s
        R-->>API: ClientDto
    else cache miss
        API->>SQL: SELECT Clients
        SQL-->>API: entidade
        API->>R: SET com TTL
    end
    API-->>API: envelope 200
```

**Invalidação:** no `PATCH /api/clients/{id}` a chave do cliente é removida do Redis.

---

## 4. Clientes — PATCH bankingDetails (síncrono + assíncrono)

```mermaid
sequenceDiagram
    actor Admin
    participant API as Clients API
    participant SQL as SQL Server
    participant R as Redis
    participant RMQ as RabbitMQ
    participant W as Notifications Worker
    participant SG as SendGrid

    Admin->>API: PATCH /api/clients/{id} bankingDetails
    API->>SQL: UPDATE Clients
    API->>R: DELETE cache
    alt agency ou conta mudou
        API->>RMQ: ClientBankingDetailsUpdatedEvent
        Note over API: HTTP 200 antes do e-mail
        RMQ->>W: mensagem na fila
        W->>SG: SendBankingDetailsUpdatedAsync
        SG-->>Admin: e-mail caixa entrada ou spam
    end
    API-->>Admin: 200 envelope + ClientDto
```

| Etapa | Síncrono? | Detalhe |
|-------|-----------|---------|
| Validação + SQL + resposta PATCH | Sim | Usuário recebe DTO atualizado |
| Publicação RabbitMQ | Sim na API, rápida | Não espera o e-mail |
| E-mail SendGrid | Assíncrono | Worker separado — ver [MESSAGERIA-RABBITMQ.md](./MESSAGERIA-RABBITMQ.md) |

**Exchange:** `challengebank.events` · **Routing key:** `clients.bankingdetails.updated` · **Fila:** `notifications.clients.bankingdetails.updated`

---

## 5. Transferências — POST com Polly e anti-duplicata Redis

```mermaid
sequenceDiagram
    actor User
    participant TA as Transactions API
    participant R as Redis
    participant CA as Clients API
    participant SQL as SQL Server

    User->>TA: POST /api/transfers Bearer
    TA->>R: SET NX dedup sender+receiver+amount 5min
    alt chave já existe
        R-->>TA: duplicata
        TA-->>User: 409 Conflict
    else registrado
        TA->>CA: GET clients sender Polly
        TA->>CA: GET clients receiver Polly
        alt Clients indisponível
            CA-->>TA: timeout ou 5xx
            TA-->>User: 503
        else ok
            TA->>SQL: INSERT Transfer
            TA-->>User: 201 Created
        end
    end
```

Políticas HTTP (Transfers → Clients): **Timeout**, **Retry**, **Circuit Breaker** — [RESILIENCIA-POLLY.md](./RESILIENCIA-POLLY.md).

---

## 6. Camadas por microsserviço (Clean Architecture)

```mermaid
flowchart LR
    subgraph Clients["Clients"]
        C_API[API]
        C_APP[Application CQRS]
        C_DOM[Domain]
        C_INF[Infrastructure EF Redis RabbitMQ]
        C_API --> C_APP --> C_DOM
        C_APP --> C_INF
    end

    subgraph Transactions["Transactions"]
        T_API[API]
        T_APP[Application CQRS]
        T_DOM[Domain]
        T_INF[Infrastructure EF Redis HttpClient]
        T_API --> T_APP --> T_DOM
        T_APP --> T_INF
    end

    subgraph Shared["Compartilhado"]
        BB[BuildingBlocks]
        AS[Api.Shared envelope JWT]
        CT[Contracts eventos INotificationService]
    end

    Clients --> Shared
    Transactions --> Shared
```

---

## 7. Controle de versão (Gitflow simplificado)

```mermaid
gitGraph
    commit id: "main estável"
    branch feature/polly
    checkout feature/polly
    commit id: "PR #3 microsserviços Polly"
    checkout main
    merge feature/polly id: "merge PR3"
    branch feature/redis-mq
    checkout feature/redis-mq
    commit id: "PR #4 Redis RabbitMQ"
    checkout main
    merge feature/redis-mq id: "merge PR4"
    branch feature/sendgrid
    checkout feature/sendgrid
    commit id: "PR #5 SendGrid"
    checkout main
    merge feature/sendgrid id: "merge PR5"
    branch documentacao
    checkout documentacao
    commit id: "PR docs fluxograma"
```

| PR | Tema |
|----|------|
| #3 | Desvinculação APIs + Polly |
| #4 | Redis cache/dedup + RabbitMQ + worker |
| #5 | SendGrid e-mail |
| docs | Fluxograma, evidências README |

Fluxo: **branch de feature → Pull Request → merge na `main`**.

---

## 8. Mapa rápido endpoint → sistema

| Endpoint | API | SQL | Redis | RabbitMQ | SendGrid |
|----------|-----|-----|-------|----------|----------|
| POST /api/auth/login | ambas | — | — | — | — |
| POST/GET /api/clients | Clients | ✓ | GET cache | — | — |
| PATCH /api/clients/{id} | Clients | ✓ | invalida | ✓ se banking mudou | via worker |
| POST /api/transfers | Transfers | ✓ | dedup | — | — |
| GET /api/transfers/* | Transfers | ✓ | — | — | — |
| GET /health | ambas | ✓ check | — | — | — |

---

## 9. Documentação relacionada

| Documento | Conteúdo |
|-----------|----------|
| [README.md](../README.md) | Execução, endpoints, credenciais |
| [challengerbank/README.md](../challengerbank/README.md) | Docker Compose |
| [MESSAGERIA-RABBITMQ.md](./MESSAGERIA-RABBITMQ.md) | Por que assíncrono |
| [SENDGRID.md](./SENDGRID.md) | Configuração e-mail |
| [RESILIENCIA-POLLY.md](./RESILIENCIA-POLLY.md) | Polly |
| [postman/](./postman/) | Collection + environments Local/Docker |

---

## 10. Cobertura da documentação vs. sistema (auditoria)

| Tópico do desafio | Documentado? | Onde |
|-------------------|:------------:|------|
| Dois microsserviços + Clean Architecture | Sim | README, §6 deste doc |
| Clientes POST/GET/PATCH + bankingDetails | Sim | README endpoints, §3–4 |
| Transferências P2P POST/GET/lista | Sim | README, §5 |
| JWT + RBAC | Sim | README, §2 |
| Envelope + erros PT (400/401/403/404/409/503) | Sim | README |
| Redis cache GET cliente | Sim | README, §3 |
| Redis anti-duplicata transferência 5 min | Sim | README, §5, challengerbank README |
| RabbitMQ + evento bankingDetails | Sim | MESSAGERIA, §4 |
| SendGrid + INotificationService | Sim | SENDGRID, README evidência |
| Polly Retry/CB/Timeout | Sim | RESILIENCIA-POLLY, §5 |
| Docker Compose stack completa | Sim | challengerbank/README |
| Postman E2E | Sim | README Postman, collection |
| Testes `dotnet test` | Sim | README (22 testes) |
| Fluxograma arquitetura | Sim | Este documento |
| Gitflow / PRs | Sim | README, §7 |
| **Foto perfil Azure Blob** | Não | Fora do escopo implementado |
| **CI GitHub Actions** | Não | Não implementado |
| **Deploy Azure / API Gateway** | Não | Não implementado |
| **Serilog estruturado** | Não | Logging padrão ASP.NET |
| **Board de tarefas / e-mail Loomi** | Não | Processo externo ao repo |

**Conclusão:** a documentação do repositório cobre **100% do que foi implementado** no código. Itens não cobertos são **funcionalidades não entregues** (P2) ou **entrega processual** (board, e-mail avaliador), não lacunas de doc do que existe.
