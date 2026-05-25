# ChallengerBank — SQL Server (container)

Banco único **`ChallengeBank`** em um container SQL Server 2022.

## Subir o banco

```bash
cd challengerbank
docker compose up -d
```

Aguarde **30–60 segundos** na primeira execução.

| Item | Valor |
|------|-------|
| Host (na máquina) | `localhost,14333` |
| Usuário | `sa` |
| Senha | `ChallengeBank@123` |
| Database | `ChallengeBank` (criado pelas migrations da API) |

## Rodar a API

**Visual Studio:** perfil **ChallengerBank** em `ChallengeBank.API`

**Terminal:**

```bash
cd challengerbank && docker compose up -d
cd ..
dotnet run --project src/Host/ChallengeBank.API --launch-profile ChallengerBank
```

Connection string em `appsettings.ChallengerBank.json` (ambiente `ChallengerBank`).

## Parar o container

```bash
docker compose down
```

Para remover os dados: `docker compose down -v`

## DBeaver

- **Host:** `localhost` · **Port:** `14333`
- **User:** `sa` / **Password:** `ChallengeBank@123`
- **Database:** `ChallengeBank`
- Marque **Trust server certificate**

## LocalDB vs ChallengerBank

| Ambiente | Quando usar |
|----------|-------------|
| `Development` (padrão) | LocalDB — `(localdb)\MSSQLLocalDB` |
| `ChallengerBank` | Container neste compose |
