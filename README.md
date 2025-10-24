
# 123Vendas • Sales API (v8)

Arquitetura **Clean + DDD + Ports & Adapters**. Agora com **Infra única** (`Sales123.Sales.Infra`) contendo:
- `Persistence/Mongo` (repositório)
- `Persistence/InMemory` (dev/tests)
- `Messaging/MassTransit` (publisher + consumers)

Recursos:
- CRUD Vendas + mutações (add item, replace qty, cancel sale/item).
- Domínio com políticas de desconto e eventos.
- Serilog (JSON), middleware de request/response, correlação e métricas (OpenTelemetry).
- MassTransit + RabbitMQ (publish/consume).
- Swagger com documentação clara e cabeçalho `x-correlation-id`.

## Como rodar

1) Suba Mongo e RabbitMQ:
```bash
docker compose up -d
```

2) Rode a API:
```bash
dotnet build
dotnet run --project src/Sales123.Sales.WebApi
```
Swagger: http://localhost:5187/swagger  
RabbitMQ UI: http://localhost:15672 (guest/guest)


## Modo sem Docker (InMemory)
Sem Mongo/Rabbit instalados? Rode em modo **InMemory** (padrão no `appsettings.json`):
```bash
dotnet build
dotnet run --project src/Sales123.Sales.WebApi
```
- Repositório: InMemory
- Transporte de eventos: MassTransit InMemory (consumidores ainda logam os eventos)
- Swagger: http://localhost:5187/swagger

Para usar Mongo/Rabbit depois, altere `UseInMemory` para `false` em `appsettings.json`.
