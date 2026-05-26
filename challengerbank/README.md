# ChallengerBank — Docker

Stack completa: **SQL Server**, **Redis**, **RabbitMQ**, **Clients API**, **Transactions API** e **Notifications Worker**.

Fluxogramas: [../docs/ARQUITETURA-FLUXO.md](../docs/ARQUITETURA-FLUXO.md).

## Subir tudo

```bash
cd challengerbank
docker compose up -d --build
```

Aguarde **1–2 minutos** na primeira execução (build + SQL + migrations).

| Serviço | HTTPS (Postman / browser) | HTTP (redirect interno) |
|---------|---------------------------|------------------------|
| **Clientes** | https://localhost:7101/swagger | http://localhost:5101 |
| **Transferências** | https://localhost:7102/swagger | http://localhost:5102 |
| **SQL Server** | — | localhost:14333 |
| **Redis** | — | localhost:6379 |
| **RabbitMQ (UI)** | — | http://localhost:15672 |

Entre containers, Transferências chama Clientes em **HTTP** (`http://clients-api:8080`) — sem TLS na rede interna.

## Postman

1. Importe `docs/postman/ChallengeBank.postman_collection.json`
2. Environment **ChallengeBank - Docker (HTTPS)** (`ChallengeBank.Docker.postman_environment.json`)
3. Postman → **Settings** → desative *SSL certificate verification* (certificado autoassinado do container)
4. Execute **Login (admin)** → fluxo **Runner**

## Redis

**Clientes:** cache do `GET /api/clients/{id}` (TTL) e invalidação no `PATCH`.

**Transferências:** anti-duplicata no `POST /api/transfers` — mesma combinação remetente/destinatário/valor bloqueada por 5 minutos (testável no Postman: *Create Transfer - duplicata Redis (409)*).

## RabbitMQ (mensageria)

Ao atualizar `bankingDetails` no `PATCH /api/clients/{id}`, a API publica um evento no exchange `challengebank.events`.

O `notifications-worker` consome esse evento e envia **e-mail via SendGrid** (configure `challengerbank/.env` — ver [../docs/SENDGRID.md](../docs/SENDGRID.md)).

## Parar

```bash
docker compose down
```

Dados do SQL: `docker compose down -v`

## Apenas SQL (dev local dotnet)

```bash
docker compose up -d sqlserver
```

Depois rode as APIs com perfil `https` ou `ChallengerBank` no Visual Studio / terminal.

## Credenciais SQL

| Campo | Valor |
|-------|-------|
| Host | `localhost,14333` |
| User | `sa` |
| Password | `ChallengeBank@123` |
| Database | `ChallengeBank` |
