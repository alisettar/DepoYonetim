# 01 — Genel Bakış

> Bu dosya, [tasarım spec'inin](../superpowers/specs/2026-05-06-depo-yonetim-design.md) Bölüm 1 ve 4'ünün yaşayan referansıdır.
> Kod ilerledikçe güncellenir.

## Solution yapısı

```
depo-yonetim/
├── src/backend/
│   ├── WMS.Api/                    → ASP.NET 10 Web API (Minimal APIs, Swagger, health-check)
│   ├── WMS.Application/            → CQRS handlers, DTOs, interfaces (MediatR)
│   ├── WMS.Domain/                 → Aggregate root'lar, domain events, value objects
│   ├── WMS.Infrastructure/         → EF Core context, repositories, stock mgmt services
│   ├── WMS.Infrastructure.Catalog/ → Catalog DB (shared) entity modelleri
│   ├── WMS.Shared.Common/          → Ortak cryptography, result<T>, exception handling
│   ├── WMS.Migrate/                → CLI: tenant database provisioning
│   └── tests/                      → Unit + integration test projeleri
├── src/frontend/                   → Vue.js 3 + TypeScript + Vite SPA
└── docs/                           → mimari, domain, operasyon dokümanları
```

## Katmanlı mimari

```
HTTP Request
    → TenantResolutionMiddleware (tenant_id çıkar, ITenantContext set)
    → Minimal API Endpoint (WMS.Api/Endpoints/)
    → MediatR → CQRS Command/Query Handler (WMS.Application)
    → Repository (WMS.Infrastructure)
    → EF Core → PostgreSQL
    → Domain invariants (aggregate validation in constructor)
```

- **Endpoints:** Minimal API pattern ile route tanımlanır. `MapXEndpoints()` extension metodu ile kayıt yapılır.
- **CQRS:** `WMS.Application` katmanında Command ve Query sınıfları, `IRequest<TResponse>` implement eder. MediatR otomatik olarak handler'ları keşfeder.
- **Domain:** Aggregate'ler constructor'da invariant doğrular. `BusinessException` ile domain kuralları kırılır.
- **Infrastructure:** `AppDbContext` (tenant DB) ve `CatalogDbContext` (shared) EF Core ile yönetilir.
- **Shared:** `Result<T>` pattern, `AesGcmService` (şifreleme), `JwtTokenService` (RS256).

## Domain aggregate'leri

| Aggregate | Açıklama |
|-----------|---------|
| `Product` (Catalog) | Ürün kodu, adı, birim, kategori, lotRequired, shelfLife, min/maxStock |
| `WarehouseLocation` | Konum kodu, adı, depo, kapasite, aktiflik durumu |
| `StockMovement` | Her hareket immutable — tip (enum), ürün, lot, miktar, fiyat, tarih |
| `FifoLayer` | Hareketlere bağlanan FIFO katmanları — stok konsolidasyonu |
| `StockBalance` | Ürün × Lot × Depo = bir satır — anlık bakiye |
| `Recipe` | Üretim tarif, versiyonlama, malzeme listesi, BOM |
| `Lot` | Lot numarası, üretim/son kullanma tarihi, kalite durumu (OK/Quarantine/Rejected) |

## CQRS pattern

Command ve Query'ler `MediatR` ile işlenir:

- **Command:** `CreateGoodsReceiptCommand`, `CreateShipmentCommand`, `ProcessTransferOutCommand` — yanıt yok veya `Result<Success>`
- **Query:** `GetProductsQuery`, `GetMovementsQuery`, `GetCriticalStockQuery` — yanıt DTO koleksiyonu
- **Handler:** `IRequestHandler<TRequest, TResponse>` implement eder, repository'leri inject alır

## Result<T> pattern

Domain hataları `Result<T>` ile dönülür:

- `Result<T>.Success(value)` — başarılı
- `Result<T>.Failure(errorCode, message)` — hata
- `Result<T>.NotFound()` — bulunamadı
- Handler'larda `.EnsureSuccess()` ile assertion yapılır

## Multi-tenant mimari

- **Catalog DB** (shared): Tenant metadata, kullanıcı auth, sistem konfigurasyon
- **Tenant DB'leri**: Her tenant için bağımsız PostgreSQL veritabanı
- **TenantConnectionResolver**: Catalog DB'den tenant record'ını okur, şifreli connstr'ı çözer
- **CachedTenantConnectionFactory**: IMemoryCache ile 10dk TTL
- **TenantResolutionMiddleware**: JWT'den tenant_id çıkarır, doğru connstr'e yönlendirir
