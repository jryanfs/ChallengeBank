# Resiliência — chamada HTTP entre microsserviços

Fluxo no sistema: [ARQUITETURA-FLUXO.md](./ARQUITETURA-FLUXO.md) (§5).

## Cenário escolhido (requisito F5–F9 do desafio)

Ao criar uma transferência, o microsserviço **Transferências** valida se remetente e destinatário existem consultando o microsserviço **Clientes**:

```
POST /api/transfers  (Transactions API :5102)
        │
        ▼
   GET /api/clients/{userId}  (Clients API :5101)   ← Polly aqui
```

Implementação: `HttpClientClientExistenceChecker` em `ChallengeBank.Transactions.API`.

## Políticas (Microsoft.Extensions.Http.Resilience)

| Política | Configuração padrão (`appsettings.json`) | Justificativa |
|----------|------------------------------------------|---------------|
| **Timeout** | 2 s (`TotalRequestTimeout` + `AttemptTimeout`) | Evita que a API de transferências fique bloqueada se Clientes estiver lento |
| **Retry** | 3 tentativas, delay 200 ms com jitter | Falhas transitórias de rede ou 5xx momentâneo |
| **Circuit Breaker** | 5 falhas / janela 30 s, ratio 50% | Se Clientes estiver fora, falha rápido e não sobrecarrega o serviço |

Configuração em `ClientsService` no `appsettings.json` da API de Transferências.

## Comportamento na API

- Cliente **não encontrado** (404 do Clients) → transferência retorna **404** com mensagem em português
- Clientes **indisponível** (rede, circuit aberto, timeout) → transferência retorna **503** (`Transfer.ClientsServiceUnavailable`)

## JWT entre serviços

O token `Authorization: Bearer` recebido na API de Transferências é **repassado** na chamada HTTP ao Clients, para respeitar o mesmo RBAC.

## Executar localmente

Terminal 1:

```bash
dotnet run --project src/Services/Clients/ChallengeBank.Clients.API
```

Terminal 2 (Clientes deve estar no ar em `https://localhost:7101`):

```bash
dotnet run --project src/Services/Transactions/ChallengeBank.Transactions.API --launch-profile https
```

**Docker:** `cd challengerbank && docker compose up -d --build` — Postman em `https://localhost:7101` e `https://localhost:7102`.

Login e token em **qualquer** das duas APIs (mesmo `Jwt:Audience` = `ChallengeBank`).
