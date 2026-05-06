# 07 — Test Stratejisi

> Unit + Integration kapsamı, shared test PostgreSQL kurulumu, CI pipeline yapılandırması burada netleşir.

## Mevcut Durum

**Test projeleri mevcut ancak test content henüz yazılmadı:**

| Proje | Konum | Durum |
|-------|-------|-------|
| `WMS.Domain.UnitTests` | `src/backend/tests/` | Proje var, test yok |
| `WMS.Application.UnitTests` | `src/backend/tests/` | Proje var, test yok |
| `WMS.Api.IntegrationTests` | `src/backend/tests/` | Proje var, test yok |

## Planlanan Test Kapsamı

### Unit Tests — Domain

- `Product.Create()` — invariant validation (code/name length, categoryId, primaryUnitId)
- `Product.AddUnit()` — duplicate check, primary unit guard
- `Product.Deactivate()` — status transition
- `Warehouse.Create()` — code uniqueness, type validation
- `Lot.Create()` — productionDate, expiryDate validation
- `FifoEngine.Consume()` — FIFO order, insufficient stock exception

### Unit Tests — Application

- `GoodsReceiptCommand` — multi-item processing, lot creation
- `ShipmentCommand` — FIFO consumption, stock validation
- `TransferCommand` — TransferOut + TransferIn atomic processing
- `RecipeVersion.Create()` — validFrom < validUntil, outputQuantity validation
- `Dashboard queries` — critical stock, warehouse fill calculations

### Unit Tests — Shared

- `PasswordHasher.Hash()` + `Verify()` — format, collision resistance
- `AesGcmService.Encrypt()` + `Decrypt()` — round-trip, IV uniqueness
- `JwtTokenService.GenerateToken()` + `ValidateToken()` — claim preservation, expiry
- `Result<T>` — success/failure/match behavior

### Integration Tests — API

- `POST /api/v1/auth/login` → JWT döner
- `POST /api/v1/inventory/goods-receipts` → 201, StockMovement/FifoLayer/StockBalance eklenir
- `POST /api/v1/inventory/shipments` → FIFO consumption, insufficient stock → 400
- `POST /api/v1/inventory/transfers` → TransferOut + TransferIn
- `POST /api/v1/recipes` + `versions` + `items` — full recipe lifecycle
- Dashboard endpoints — sample data ile query doğrulama

### Integration Tests — Tenant Isolation

- İki farklı tenant için request → her biri kendi DB'sine erişir
- Cross-tenant data leak yok (A tenant'ın verisine B tenant erişemez)
- Tenant inactive → 402 PaymentRequired

## Test Veritabanı

- **PostgreSQL** — Docker container ile local integration test
- **EF Core InMemory vs Npgsql:** Integration testleri gerçek Npgsql provider ile çalışır
- **Test database naming:** `wms_test_{tenant}_{random}` — her test run'da temizlenir

## CI Pipeline Test Planı

- `dotnet test` — tüm test projeleri
- `dotnet build` — build failure yok
- Docker image build → health check endpoint doğrulama
