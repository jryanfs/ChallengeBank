# E-mail — SendGrid (worker de notificações)

Quando `bankingDetails` é alterado, o `ChallengeBank.Notifications.Worker` consome o evento RabbitMQ e envia e-mail via **SendGrid** (`INotificationService` → `SendGridNotificationService`).

Fluxo no sistema: [ARQUITETURA-FLUXO.md](./ARQUITETURA-FLUXO.md) (§4).

## Configuração

| Chave | Obrigatório | Descrição |
|-------|-------------|-----------|
| `SendGrid:ApiKey` | Sim (para enviar) | API Key do painel SendGrid |
| `SendGrid:FromEmail` | Sim | Remetente **verificado** no SendGrid |
| `SendGrid:FromName` | Não | Nome exibido (padrão: ChallengeBank) |
| `SendGrid:OverrideRecipientEmail` | Não | Em dev, envia tudo para este e-mail |

Sem `ApiKey` / `FromEmail`, o worker **não falha**: registra aviso no log (modo fallback).

## Desenvolvimento local

1. Crie API Key em [SendGrid](https://app.sendgrid.com/settings/api_keys)
2. Verifique o remetente (Single Sender ou domínio)
3. Copie o exemplo:

```bash
cp src/Services/Notifications/ChallengeBank.Notifications.Worker/appsettings.Local.example.json \
   src/Services/Notifications/ChallengeBank.Notifications.Worker/appsettings.Local.json
```

4. Preencha `ApiKey`, `FromEmail` e opcionalmente `OverrideRecipientEmail`
5. Rode o worker:

```bash
dotnet run --project src/Services/Notifications/ChallengeBank.Notifications.Worker
```

Ou use User Secrets:

```bash
dotnet user-secrets set "SendGrid:ApiKey" "SG...." \
  --project src/Services/Notifications/ChallengeBank.Notifications.Worker
```

## Docker

```bash
cd challengerbank
cp .env.example .env
# edite .env com suas chaves
docker compose up -d --build notifications-worker
```

Variáveis lidas pelo compose: `SENDGRID_API_KEY`, `SENDGRID_FROM_EMAIL`, `SENDGRID_FROM_NAME`, `SENDGRID_OVERRIDE_TO`.

## Validar

1. RabbitMQ + worker + clients-api no ar
2. Postman: pasta **Mensageria RabbitMQ (3 passos)** → PATCH com novo `bankingDetails`
3. Log do worker:

```bash
docker logs challengerbank-notifications-worker --tail 20
```

Esperado com SendGrid ativo:

```text
Notificações por e-mail: SendGrid ATIVO. From=...
E-mail SendGrid enviado. To=... ClientId=...
```

## Evento

O payload inclui `ClientEmail` e `ClientFullName` (preenchidos na publicação pelo `UpdateClientCommandHandler`).
