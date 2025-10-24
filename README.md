# 123Vendas • Sales API (In-Memory)

API de Vendas em **.NET 8** com camadas **Infra / Domain / Application / WebApi**.  
**Persistência:** totalmente **in-memory** (os dados existem apenas enquanto a aplicação está em execução).  
**Swagger** com exemplos dinâmicos, **logs** (request/response/tempo/status) e **correlação** via `x-correlation-id`.

---

## Sumário
- [Requisitos](#requisitos)
- [Como rodar](#como-rodar)
- [Como testar](#como-testar)
- [Persistência (in-memory)](#persistência-in-memory)
- [Logging & correlação](#logging--correlação)
- [Contratos REST (visão rápida)](#contratos-rest-visão-rápida)
- [Envelope de resposta](#envelope-de-resposta)
- [Erros & status codes](#erros--status-codes)
- [Estrutura do projeto](#estrutura-do-projeto)
---

## Requisitos
- **.NET 8 SDK** instalado.

---

## Como rodar

```bash
# na raiz do repositório
dotnet build

# subir a API
dotnet run --project src/Sales123.Sales.WebApi
```

- A **UI do Swagger** está na **raiz** (`/`).


---

## Como testar

```bash
dotnet test
```

> Os testes cobrem o domínio (entidades/VOs/políticas) e a camada Application (serviço de vendas).

---

## Persistência (in-memory)

- Repositório **thread-safe** em memória.
- Índices:
  - **Primário** por `Id`.
  - **Secundário** por `(Number, BranchId)`.
- **Importante:** todos os dados são **voláteis** — ao reiniciar a aplicação, tudo é perdido.  

---

## Logging & correlação

- **Request/Response** logados:
  - Método, **URL completa**, corpo para `POST/PUT/PATCH/DELETE`, **status code** e **tempo** em ms.
  - **GET** também loga URL completa e tempo.
- **Correlação**:
  - Aceita `x-correlation-id`. Se ausente, o middleware **gera** um.
  - O `traceId` retorna no envelope da resposta.

**Erro 500**:
- Resposta JSON **padronizada** (ver envelope abaixo).
- Emite **evento de erro** com classe raiz, linha e *stacktrace* amigável (consumidor registra/loga).

---

## Contratos REST (visão rápida)


- `POST /api/sales` — cria venda
- `GET /api/sales` — lista paginada
- `GET /api/sales/{id}` — busca por id
- `DELETE /api/sales/{id}` — exclui

Itens:
- `POST /api/sales/{id}/items` — adiciona item
- `PUT /api/sales/{id}/items/{itemId}/quantity` — altera quantidade
- `POST /api/sales/{id}/items/{itemId}/cancel` — cancela item

Venda:
- `POST /api/sales/{id}/cancel` — cancela a venda

### Exemplo (curl) – criar venda

```bash
curl -X POST http://localhost:5000/api/sales   -H "Content-Type: application/json"   -H "x-correlation-id: demo-123"   -d '{
    "number": "20251023-0001",
    "saleDate": "2025-10-23T12:00:00Z",
    "customerId": "C1",
    "customerName": "Cliente A",
    "branchId": "BR-01",
    "branchName": "Filial 01",
    "items": [
      { "productId": "P1", "productName": "Produto X", "sku": "SKU-1", "quantity": 5, "unitPrice": 10.0 }
    ]
  }'
```

---

## resposta

Formato unificado:

```json
{
  "success": true,
  "data": { /* ... */ },
  "traceId": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "notifications": [
    { "code": "sale.already_exists", "message": "Venda já existe...", "path": "number", "severity": 2 }
  ]
}
```

- `success`: `true` ou `false`
- `data`: objeto de retorno (ou `null`)
- `traceId`: mesmo valor do `x-correlation-id` (se enviado/gerado)
- `notifications`: erros/alertas (ex.: validação, conflito)

---

## Erros & status codes

- **409 Conflict** — venda já existe (`POST /api/sales`).  
  Notificação: `sale.already_exists`.
- **422 Unprocessable Entity** — violações de regra de negócio/validação.
- **404 Not Found** — venda/item não encontrados.
- **500 Internal Server Error** — exceção não tratada:
  - resposta segue o **envelope** (`success=false`, `data=null`, `traceId`, `notifications`).
  - evento é **emitido** com tipo raiz, linha e *stacktrace* amigável.

---

## Estrutura do projeto

```
src/
  Sales123.Sales.Domain/        # Entidades, VOs, políticas, eventos, Result/Notifications
  Sales123.Sales.Application/   # Serviços (SaleService), DTOs/Commands, abstrações
  Sales123.Sales.Infra.*        # Repositório in-memory, publishers in-memory
  Sales123.Sales.WebApi/        # Controllers, Middlewares, Swagger examples, Program

tests/
  Sales123.Sales.Test/          # xUnit
```

---

