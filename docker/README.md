# Docker (opcional)

O projeto usa por padrão **SQL Server LocalDB** com banco único `ChallengeBank`.

Se preferir containers, ajuste as connection strings em `ChallengeBank.API` para apontar aos serviços Docker e use um único banco ou dois conforme o `docker-compose.yml`.

```bash
docker compose up -d
```
