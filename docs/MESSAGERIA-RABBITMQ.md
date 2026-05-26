# Mensageria — atualização de `bankingDetails` (RabbitMQ)

Requisitos do desafio: **F1–F4**, **F10–F11** (broker, fluxo real, caso `bankingDetails`, justificativa assíncrona, contrato abstrato e stub).

## Fluxo escolhido

Quando o **Admin** altera dados bancários no `PATCH /api/clients/{id}`, a API de Clientes persiste no SQL e publica um evento. Um **worker** consome a fila e dispara notificação (hoje: log via stub).

```
PATCH /api/clients/{id}  (Clients API)
        │
        ├─► SQL Server (schema clients) — fonte da verdade
        ├─► Redis — invalida cache do GET
        └─► RabbitMQ exchange challengebank.events
                    routing key: clients.bankingdetails.updated
                        │
                        ▼
            fila notifications.clients.bankingdetails.updated
                        │
                        ▼
        ChallengeBank.Notifications.Worker
                        │
                        └─► INotificationService (LogNotificationService — stub)
```

## Por que esse endpoint é assíncrono (F4)

O `PATCH` de cliente precisa responder rápido com o estado atualizado (**síncrono** no HTTP). Já as **ações colaterais de notificação** (e-mail, SMS, push, integração com antifraude, auditoria externa) são:

| Motivo | Explicação |
|--------|------------|
| **Latência e disponibilidade** | Enviar e-mail ou chamar APIs de terceiros pode levar segundos ou falhar. Não deve bloquear nem falhar o PATCH que já gravou o dado no banco. |
| **Desacoplamento** | Clientes não conhece o canal de notificação. O worker pode evoluir (trocar stub por SendGrid, Service Bus, etc.) sem redeploy da API. |
| **Escalabilidade** | Picos de PATCH não derrubam a API; filas absorvem carga e permitem consumidores independentes. |
| **Consistência eventual** | O cliente vê o `bankingDetails` atualizado na resposta do PATCH/GET; a notificação chega em segundos — aceitável para aviso de alteração cadastral. |

**O que permanece síncrono:** validação, persistência, invalidação de cache e resposta HTTP com o DTO atualizado.

**O que é assíncrono:** apenas o disparo de notificação após detectar mudança real em `agency` / `accountNumber`.

## Broker (F1)

**RabbitMQ** — adequado para desenvolvimento local e Docker; no compose sobe com UI em `http://localhost:15672` (guest/guest).

Alternativa citada no desafio: Azure Service Bus (mesmo padrão publish/subscribe, troca de implementação em `IBankingDetailsEventsPublisher`).

## Contratos (F10–F11)

| Artefato | Projeto | Papel |
|----------|---------|--------|
| `ClientBankingDetailsUpdatedEvent` | `ChallengeBank.Contracts` | Payload do evento |
| `IBankingDetailsEventsPublisher` | Clients.Application | Abstração da publicação |
| `INotificationService` | `ChallengeBank.Contracts` | Abstração do envio (e-mail/SMS/etc.) |
| `RabbitMqBankingDetailsEventsPublisher` | Clients.Infrastructure | Implementação RabbitMQ |
| `LogNotificationService` | Notifications.Worker | Stub que registra no log |

Publicação ocorre em `UpdateClientCommandHandler` somente se `bankingDetails` foi enviado no PATCH **e** agency/conta mudaram em relação ao valor anterior.

## Configuração

### Clients API (`appsettings.json` / Docker)

```json
"RabbitMq": {
  "HostName": "localhost",
  "Port": 5672,
  "UserName": "guest",
  "Password": "guest",
  "Exchange": "challengebank.events",
  "RoutingKeyBankingDetailsUpdated": "clients.bankingdetails.updated"
}
```

No Docker (`challengerbank/docker-compose.yml`), `clients-api` usa `RabbitMq__HostName=rabbitmq`.

### Worker

Fila: `notifications.clients.bankingdetails.updated` (bind no mesmo exchange e routing key).

Variáveis no serviço `notifications-worker` do compose — ver `docker-compose.yml`.

## Validar manualmente (Docker)

```bash
cd challengerbank
docker compose up -d --build
```

1. Login Admin: `POST https://localhost:7101/api/auth/login` — `admin` / `Admin@123`
2. Criar cliente com `bankingDetails` ou PATCH em cliente existente alterando `agency` / `accountNumber`
3. Logs do worker:

```bash
docker logs challengerbank-notifications-worker --tail 50
```

Esperado: log do stub com `ClientId`, agência e conta.

## Testes automatizados

`dotnet test` — integração usa ambiente `Testing` **sem** RabbitMQ obrigatório (`IBankingDetailsEventsPublisher` opcional no handler). O fluxo de mensageria é validado manualmente ou em ambiente com broker (acima).

## Referências no código

- Publicação: `UpdateClientCommandHandler`, `RabbitMqBankingDetailsEventsPublisher`
- Consumo: `ChallengeBank.Notifications.Worker/Worker.cs`
- Evento: `ChallengeBank.Contracts/Events/ClientBankingDetailsUpdatedEvent.cs`
