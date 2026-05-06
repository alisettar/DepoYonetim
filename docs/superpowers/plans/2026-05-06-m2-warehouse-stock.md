# M2 — Warehouse Domain: Stok + İşlem Hareketleri

**Tarih:** 2026-05-06
**Durum:** Plan
**Spec referansı:** `docs/superpowers/specs/2026-05-06-depo-yonetim-design.md`
**Önceki:** M1 tamamlandı (altyapı + catalog + auth + tenant CRUD)

---

## M1 Özeti

M1'de başarıyla tamamlandı:
- 10 proje solution (6 ana + 3 test + 1 migrate)
- Catalog DB şeması (SuperAdmin, Tenant, TenantDatabase, TenantUser)
- JWT RS256 auth, PBKDF2 password hashing, AES-GCM encryption
- TenantConnectionResolver + IMemoryCache (10dk TTL)
- WMS.Migrate CLI (apply + create-tenant)
- TenantResolutionMiddleware
- Auth endpoint'leri (login, admin-login)
- Tenant CRUD (create, list, status, health)
- Kod standardı: minimal API, `GetRequiredService<ISender>()`, Result pattern, primary constructor, C# 12
- Build: 0 hata, 0 uyarı

---

## M1 Sonrası Durum

```
WMS.Shared       → Result<T>, Result, exceptions, PasswordHasher, AesGcmService, JwtTokenService
WMS.Domain       → Entity'ler: SuperAdmin, SuperAdminLogin, TenantUser
WMS.Application  → Handlers: LoginCommand, AdminLoginCommand, CreateTenantCommand, ListTenantsQuery, UpdateTenantStatusCommand, TenantHealthQuery
WMS.Infrastructure → DbContext'ler: CatalogDbContext, AppDbContext
WMS.Infrastructure.Catalog → Entities: SuperAdmin, Tenant, TenantDatabase, TenantUser, SuperAdminLogin
WMS.Api          → Endpoints: /api/v1/auth/login, /api/v1/admin/auth/login, /api/v1/admin/tenants/*
WMS.Migrate      → CLI: apply, create-tenant
```

---

## M2 Karar Özeti

| Karar | Seçim | Neden |
|---|---|---|
| A. İşlem kaydı | **A1** — Her movement bir `StockMovement` entity | Audit trail, raporlama, balance hesaplama |
| B. Balance hesabı | **B1** — Gerçek zamanlı hesaplanan (computed, not stored) | Tek bilgi kaynağı movements; stale balance yok |
| C. Location modeli | **C1** — Simple: zone → aisle → rack → bin | Basit, çoğu WMS için yeterli |
| D. Transaction | **D1** — Her movement atomic transaction | Data integrity, rollback on failure |
| E. RBAC M2'de | **E1** — Admin/Operator role'leri ekleniyor | Super-admin dışında operator yetkilendirme |

---

## M2 Görevleri

### T1 — Domain entity'leri: Warehouse + Location + Product

**Hedef:** Warehouse, Location, Product domain entity'leri + enum'lar.

**Dosyalar:**
- `WMS.Domain/Catalog/Entities/Warehouse.cs` — `Warehouse` aggregate root
  - `Id`, `Code`, `Name`, `TenantId`, `Address`, `IsActive`
  - `CreateLocation()` domain method
  - `Deactivate()` domain method
- `WMS.Domain/Catalog/Entities/WarehouseLocation.cs` — physical location
  - `Id`, `WarehouseId`, `Zone`, `Aisle`, `Section`, `Bin`
  - `FullName` computed property: `A1-B2-C3`
  - `Activate()` / `Deactivate()`
- `WMS.Domain/Catalog/Entities/Product.cs` — product master data
  - `Id`, `Sku`, `Name`, `CategoryId`, `UnitOfMeasure`, `IsActive`
  - `Sku` unique per tenant
- `WMS.Domain/Catalog/Entities/StockMovement.cs` — movement records
  - `Id`, `WarehouseId`, `ProductId`, `LocationId`
  - `Type` (Inbound, Outbound, Transfer, Adjustment, Reserve, Release)
  - `Quantity`, `ReferenceType` (PO, SO, Internal), `ReferenceId`
  - `PerformedBy`, `PerformedAt`
  - `Note`
- `WMS.Domain/Catalog/Entities/ProductCategory.cs` — product categories
- `WMS.Domain/Catalog/Enums/StockMovementType.cs` — Inbound, Outbound, Transfer, Adjustment, Reserve, Release
- `WMS.Domain/Catalog/Enums/UnitOfMeasure.cs` — Piece, Kg, Liter, Box, Pallet

**Domain kuralları:**
- Product Sku unique per tenant
- Quantity > 0 olmalı
- Warehouse inactive olamaz (movement yaparken)
- Location warehouse'a ait olmalı
- Transfer: source ve destination warehouse farklı olmalı

**Testler:**
- `Warehouse_ShouldCreateLocation`
- `Warehouse_ShouldFailWhenInactive`
- `Product_ShouldEnforceUniqueSkuPerTenant`
- `StockMovement_ShouldRejectNegativeQuantity`
- `StockMovement_ShouldValidateTransfer`

---

### T2 — Catalog DB: Warehouse + Product + Location + Movement

**Hedef:** Yeni entity'ler EF Core ile CatalogDbContext'e eklenir, migration oluşturulur.

**Dosyalar:**
- `WMS.Infrastructure.Catalog/Configurations/WarehouseConfiguration.cs`
- `WMS.Infrastructure.Catalog/Configurations/WarehouseLocationConfiguration.cs`
- `WMS.Infrastructure.Catalog/Configurations/ProductConfiguration.cs`
- `WMS.Infrastructure.Catalog/Configurations/StockMovementConfiguration.cs`
- `WMS.Infrastructure.Catalog/Configurations/ProductCategoryConfiguration.cs`
- `WMS.Migrate/Seeders/DefaultProductSeeder.cs` — default categories + UOM enum'lar

**Değişiklikler:**
- `CatalogDbContext` — yeni `DbSet`'ler eklendi
- Migration: `Add-Migration M2_warehouse_domain` (simüle, actual migration tool ile değil, schema definition)

**Testler:**
- `MigrationSnapshot_ShouldMatchEntities` — yeni entity'leri yansıttı mı?

---

### T3 — Application: Product CRUD

**Hedef:** Product oluştur, listele, güncelle.

**Dosyalar:**
- `WMS.Application/Catalog/Commands/CreateProductCommand.cs` — `CreateProductCommand(Sku, Name, CategoryId, UnitOfMeasure) → Result<Guid>`
- `WMS.Application/Catalog/Commands/UpdateProductCommand.cs` — `UpdateProductCommand(Id, Name?, IsActive?) → Result`
- `WMS.Application/Catalog/Queries/ListProductsQuery.cs` — `ListProductsQuery(int page, int pageSize) → Result<PagedResponse<ProductDto>>`
- `WMS.Application/Catalog/Queries/GetProductBySkuQuery.cs` — `GetProductBySkuQuery(string Sku) → Result<ProductDto?>`
- `WMS.Application/Catalog/Dtos/ProductDto.cs` — `ProductDto(Sku, Name, CategoryId, UnitOfMeasure, IsActive)`
- `WMS.Application/Common/Dtos/PagedResponse.cs` — `PagedResponse<T>(T[] Items, int Total, int Page, int PageSize)`

**Handler'lar primary constructor ile:**
```csharp
public class CreateProductHandler(
    CatalogDbContext catalogDbContext,
    ITenantContext tenantContext)
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    // handler logic
}
```

**Testler:**
- `CreateProduct_ShouldSucceed` — unique SKU
- `CreateProduct_ShouldFailDuplicateSku` — aynı tenant'da duplicate
- `ListProducts_ShouldReturnPaged`
- `GetProductBySku_ShouldReturnProduct`
- `GetProductBySku_ShouldReturnNull` — bulunamazsa

---

### T4 — Application: Stock Movement (Inbound/Outbound)

**Hedef:** Stok giriş/çıkış hareketleri, balance hesaplama.

**Dosyalar:**
- `WMS.Application/Stock/Commands/ReceiveCommand.cs` — Inbound
- `WMS.Application/Stock/Commands/ShipCommand.cs` — Outbound
- `WMS.Application/Stock/Commands/AdjustCommand.cs` — Adjustment
- `WMS.Application/Stock/Commands/TransferCommand.cs` — Transfer
- `WMS.Application/Stock/Commands/ReserveCommand.cs` — Reserve
- `WMS.Application/Stock/Commands/ReleaseCommand.cs` — Release
- `WMS.Application/Stock/Queries/BalanceQuery.cs` — `BalanceQuery(ProductId, WarehouseId?) → Result<BalanceDto>`
- `WMS.Application/Stock/Queries/MovementLogQuery.cs` — `MovementLogQuery(ProductId, Page, PageSize) → Result<PagedResponse<MovementLogDto>>`
- `WMS.Application/Stock/Dtos/BalanceDto.cs` — `BalanceDto(ProductId, WarehouseId?, ProductName, QuantityOnHand)`
- `WMS.Application/Stock/Dtos/MovementLogDto.cs`

**Balance hesaplama (computed):**
```csharp
// Gerçek zamanlı hesaplanan, stored değil
var balance = await _dbContext.StockMovements
    .Where(m => m.ProductId == productId 
             && m.Type != StockMovementType.Reserve
             && m.Type != StockMovementType.Release)
    .SumAsync(m => m.Type == StockMovementType.Inbound || m.Type == StockMovementType.Adjustment
                    ? m.Quantity
                    : -m.Quantity);
```

**Domain kuralları:**
- Outbound: `quantityOnHand >= requestQuantity` değilse hata
- Transfer: source warehouse'da yeterli stok olmalı
- Balance computed — stored balance yok

**Testler:**
- `Receive_ShouldCreateMovementAndIncreaseBalance`
- `Ship_ShouldFailInsufficientStock`
- `Ship_ShouldSucceedAndDecreaseBalance`
- `BalanceComputed_ShouldReflectAllMovements`
- `Transfer_ShouldFailInsufficientSourceBalance`
- `Adjust_ShouldAllowNegativeBalance` — adjustment ile negative olabilir mi? (karar verilmesi gerek)

---

### T5 — API: Product + Stock Endpoints

**Hedef:** Product CRUD ve stock movement endpoint'leri.

**Dosyalar:**
- `WMS.Api/Endpoints/Catalog/ProductEndpoints.cs`
  - `POST /api/v1/{tenant}/products` — product oluştur
  - `GET /api/v1/{tenant}/products` — liste (page, pageSize, search)
  - `GET /api/v1/{tenant}/products/{sku}` — tekil sorgu
  - `PATCH /api/v1/{tenant}/products/{id}` — güncelle
- `WMS.Api/Endpoints/Stock/MovementEndpoints.cs`
  - `POST /api/v1/{warehouse}/movements/inbound` — receive
  - `POST /api/v1/{warehouse}/movements/outbound` — ship
  - `POST /api/v1/{warehouse}/movements/adjust` — adjustment
  - `POST /api/v1/{warehouse}/movements/reserve` — reserve
  - `POST /api/v1/{warehouse}/movements/release` — release
  - `GET /api/v1/{warehouse}/movements` — movement log
  - `GET /api/v1/{warehouse}/stock/balance` — balance

**Path parameter:** `{tenant}` veya `{warehouse}` — hangisi daha uygun?
- **Karar:** `{tenant}` path'te olmalı, `{warehouse}` header'da. Ama tenant-specific endpoint'ler zaten tenant context'ten geliyor. **Karar:** `/api/v1/{tenant}/products` ve `/api/v1/{warehouse}/movements` — warehouse ID path'te.

**Endpoint yapısı (minimal API):**
```csharp
public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
{
    app.MapPost("/api/v1/{tenantId}/products", HandleCreate);
    app.MapGet("/api/v1/{tenantId}/products", HandleList);
    app.MapGet("/api/v1/{tenantId}/products/{sku}", HandleGetBySku);
    app.MapPatch("/api/v1/{tenantId}/products/{id}", HandleUpdate);
    return app;
}

private static async Task HandleCreate(HttpContext context, string tenantId, [FromBody] CreateProductRequest request)
{
    var sender = context.RequestServices.GetRequiredService<ISender>();
    var command = new CreateProductCommand(request.Sku, request.Name, request.CategoryId, request.UnitOfMeasure);
    var result = await sender.Send(command, context.RequestAborted);
    // ... response handling
}
```

**Testler:**
- `CreateProduct_ShouldReturn201` — valid request
- `CreateProduct_ShouldReturn409` — duplicate SKU
- `ShipStock_ShouldReturn400` — insufficient balance
- `ReceiveStock_ShouldReturn201`

---

### T6 — TenantMiddleware + path validation

**Hedef:** `/api/v1/{tenantId}/` path prefix ile tenant doğrulama, existing endpoint'leri tenant-aware yap.

**Dosyalar:**
- `WMS.Api/Middleware/TenantPathMiddleware.cs` — path'ten tenantId çıkarma
- `WMS.Api/Middleware/TenantPathMiddlewareExtensions.cs` — extension method

**Değişiklikler:**
- Mevcut admin endpoint'leri (/api/v1/admin/*) path validation'dan muaf
- Tenant endpoint'leri (/api/v1/{tenantId}/*) path'ten tenantId çıkarıp ITenantContext'e ekler

**Testler:**
- `Middleware_ShouldExtractTenantIdFromPath`
- `Middleware_ShouldAllowAdminEndpoints`
- `Middleware_ShouldReturn400ForUnknownTenant`

---

### T7 — Integration test: Full flow (product + stock)

**Hedef:** Super-admin login → tenant oluştur → product oluştur → receive → ship → balance check → adjustment.

**Dosyalar:**
- `tests/WMS.Api.IntegrationTests/Scenarios/StockFlowTest.cs`

**Testler:**
- `FullStockFlow_ShouldWorkEndToEnd`

---

## M2 Kod Standardı

- **Primary constructor** — tüm handler'lar ve DTO'lar
- **GetRequiredService\<ISender\>()** — minimal API endpoint'lerinde DI
- **Result pattern** — error handling
- **Domain methods** — entity'lerde business logic (`CreateLocation()`, `Deactivate()`)
- **Computed balance** — stored değil, query'de hesaplanan
- **Transaction** — her movement atomic
- **C# 12** — record types, collection expressions, using declarations

---

## Bağımlılık Grafiği

```
M1 tamamlandı (T1-T10)
  ↓
M2-T1: Domain entity'leri (Warehouse, Location, Product, StockMovement)
  ↓
M2-T2: Catalog DB config + migration
M2-T3: Application — Product CRUD
  ↓
M2-T4: Application — Stock Movement + Balance
M2-T5: API — Product + Stock endpoints
  ↓
M2-T6: TenantMiddleware + path validation
  ↓
M2-T7: Integration test
```

## Gözden kaçırılmamalı

- `WMS.Domain` → `WMS.Infrastructure.Catalog` proje referansı eklenmeli (domain entity'leri catalog'da)
- **YAPILMADI:** M1'de entity'ler `WMS.Domain` yerine `WMS.Infrastructure.Catalog` içinde. M2'de domain entity'leri de `WMS.Domain`'e taşımak mı? Yoksa `WMS.Infrastructure.Catalog`'ı domain + persistence olarak mı bırakmak?
- **Karar verilecek:** Entity'ler nerede olmalı?
  - **Seçenek 1:** `WMS.Domain`'de aggregate root'lar + `WMS.Infrastructure.Catalog`'ta EF config
  - **Seçenek 2:** Hepsi `WMS.Infrastructure.Catalog`'ta (monolith, simpler)
- `PagedResponse<T>` shared utility olarak `WMS.Shared`'a mı yoksa `WMS.Application`'a mı?
- **Karar:** `WMS.Shared`'a ekle — `WMS.Shared/Common/Dtos/PagedResponse.cs`
