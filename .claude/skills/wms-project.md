---
name: wms-project
description: WMS Depo Yönetim projesinin tüm mimari kararları, kod standartları, proje yapısı ve M1-M2 tamamlanmış görevleri hakkında context sağlar
type: reference
---

# WMS Depo Yönetim Projesi — Skill Dosyası

## Proje Özeti

Multi-tenant warehouse management system (SaaS). Monolithic architecture, modular monolith pattern.

**Teknoloji yığını:**
- Backend: .NET 9+ (C# 12), ASP.NET Core Minimal API, Entity Framework Core Code-First
- Database: PostgreSQL (tenant-per-db isolation), Catalog DB (single shared)
- Auth: JWT RS256, PBKDF2-SHA256 password hashing, AES-GCM encryption
- Messaging: MediatR (CQRS-lite), Results pattern
- Logging: Serilog + Console Sink
- Test: xUnit + Respawn (local DB)

**Mimari katmanlar:**
```
WMS.Shared       → Temel utilities, Result<T>, exception'lar, crypto
WMS.Domain       → Domain entity'leri, aggregate root'lar, domain kuralları
WMS.Application  → Command/Query handler'ları, DTO'lar, business logic
WMS.Infrastructure → EF Core DbContext'ler, repository'ler, services
WMS.Infrastructure.Catalog → Catalog DB (shared) entity'leri + mapping
WMS.Api          → Minimal API endpoint'leri, middleware, configuration
WMS.Migrate      → CLI tool (wms-migrate), apply + create-tenant
```

**Bağımlılık zinciri:**
```
WMS.Shared (temel)
  ↑
WMS.Domain (WMS.Shared)
  ↑
WMS.Application (WMS.Domain + WMS.Shared)
  ↑
WMS.Infrastructure (WMS.Application + WMS.Shared)
  ↑
WMS.Infrastructure.Catalog (WMS.Shared)
  ↑
WMS.Api (WMS.Application + WMS.Infrastructure + WMS.Infrastructure.Catalog)
WMS.Migrate (WMS.Infrastructure.Catalog)
```

---

## M1 — TAMAMLANDI (2026-05-06)

### Tamamlanan Görevler

**T1 — Solution + csproj iskeleti**
- `WMS.sln` + 10 proje (6 ana + 3 test + 1 migrate)
- `Directory.Build.props` — LangVersion 12, ImplicitUsings enable, Nullable enable, NoWarn NET9008
- `Directory.Packages.props` — Central Package Management (CPM) enabled
- `.gitignore` — secrets/, bin/, obj/, .vs/, *.pem

**T2 — Test altyapısı**
- xUnit 2.9.2 + FluentAssertions 8.2.0 + NSubstitute 5.3.0
- Respawn ile local PostgreSQL temizleme
- Integration test projesi: `WMS.Api.IntegrationTests`
- `SharedPostgreSqlFixture` — testler arasında DB içerik temizleme

**T3 — WMS.Shared: Result<T> + Exceptions + Crypto**
- `Result<T>` — static factory: `MakeSuccess(T)`, `MakeFailure(string errorCode, string message)`
- `Result` (void) — pattern matching: `isSuccess`, `isFailure`
- `ValidationProblem` — validation hataları için `errors[]` koleksiyonu
- Domain exception'lar: `DomainException`, `NotFoundException`, `InvalidOperationException`
- `PasswordHasher` — PBKDF2-SHA256 (Consistent.Security.Cryptography 1.3.0)
- `AesGcmService` — AES-GCM şifreleme/çözümleme
- `JwtTokenService` — RS256, pem key'den okuma, access 15dk + refresh 7gün
- `JwtClaims` — claim modelleri
- `RefreshToken` — DB model

**T4 — Catalog DB entity'leri + EF config**
- `SuperAdmin` — super admin kullanıcı
- `Tenant` — tenant (code, name, plan, status, createdAt)
- `TenantDatabase` — tenant'ın PostgreSQL database bilgileri
- `TenantUser` — tenant'a özel kullanıcılar
- `SuperAdminLogin` — super admin giriş kayıtları
- `CatalogDbContext` — Fluent API konfigürasyon
- Migration: `InitialCatalog`

**T5 — TenantConnectionResolver + IMemoryCache**
- `TenantConnectionResolver` — catalog'dan tenant'ın connection string'ini çözer
- `CachedTenantConnectionFactory` — IMemoryCache ile 10dk TTL
- `ITenantContext` — request-scoped: `TenantId`, `TenantCode`, `UserId`, `ActorType`

**T6 — WMS.Migrate CLI**
- `wms-migrate apply --tenant-id <id>` — tek tenant migration apply
- `wms-migrate apply --all` — tüm tenant'ların migration'larını apply
- `wms-migrate create-tenant <code> <name>` — DB create + migration + seed
- Microsoft.Extensions.Hosting Console app

**T7 — JWT Token Service (RS256)**
- `JwtTokenService` — RS256 JWT üretimi
- Access token: 15dk, refresh token: 7 gün
- Pem key'den okuma (secrets/jwt-private.pem, jwt-public.pem)
- `JwtClaims` — claim modelleri

**T8 — TenantResolutionMiddleware**
- `TenantResolutionMiddleware` — JWT'den tenantId okur, request scope'a ITenantContext enjekte eder
- `CurrentTenantContext` — implementation
- Middleware pipeline'a entegre

**T9 — Auth endpoint'leri**
- `POST /api/v1/auth/login` — tenant kullanıcı login
- `POST /api/v1/admin/auth/login` — super-admin login
- `LoginRequest`, `LoginResponse`, `MeResponse` DTO'ları
- `LoginHandler`, `AdminLoginHandler` — MediatR handler'ları
- Result pattern ile error handling

**T10 — Super-admin tenant CRUD**
- `POST /api/v1/admin/tenants` — tenant oluştur
- `GET /api/v1/admin/tenants` — liste (status, page, pageSize)
- `PATCH /api/v1/admin/tenants/{id}/status` — durum güncelle
- `GET /api/v1/admin/tenants/{id}/health` — sağlık kontrolü
- `CreateTenantCommand`, `ListTenantsQuery`, `UpdateTenantStatusCommand`, `TenantHealthQuery`
- DTO'lar: `CreateTenantRequest`, `TenantDto`, `TenantHealthDto`

**T11 — İlk full integration test**
- `FullTenantOnboardingFlow` — super-admin login → tenant oluştur → tenant login → health check

### Kod Standardı

1. **Primary Constructor:** Tüm handler'lar, DTO'lar, entity'ler primary constructor ile
   ```csharp
   public class CreateProductHandler(
       CatalogDbContext catalogDbContext,
       ITenantContext tenantContext)
       : IRequestHandler<CreateProductCommand, Result<Guid>>
   ```

2. **Minimal API — GetRequiredService<ISender>()**
   ```csharp
   private static async Task HandleCreate(HttpContext context)
   {
       var sender = context.RequestServices.GetRequiredService<ISender>();
       var command = new CreateProductCommand(request.Sku, request.Name);
       var result = await sender.Send(command, context.RequestAborted);
   }
   ```

3. **Result Pattern**
   ```csharp
   if (result.IsSuccess) { /* success */ }
   else if (result.IsFailure) { /* error */ }
   ```

4. **Record Types:** DTO'lar `record` olarak tanımlanır
   ```csharp
   public record CreateProductRequest(string Sku, string Name, Guid CategoryId);
   ```

5. **MediatR — ISender:** Endpoint layer'da `ISender` kullan (IMediator değil, leaner)

6. **using Statement:** MediatR için `using MediatR;`

7. **GlobalUsings:** `WMS.Api/GlobalUsings.cs` — `Microsoft.AspNetCore.Http` + `Microsoft.AspNetCore.Mvc`

---

## M2 — PLANLANDI (Hala başlanmadı)

M2 planı: `docs/superpowers/plans/2026-05-06-m2-warehouse-stock.md`

**Kalan karar:** Entity'ler `WMS.Domain`'e mi taşınacak, yoksa `WMS.Infrastructure.Catalog`'ta mı kalacak?

**M2 Görevleri:**
- T1: Domain entity'leri (Warehouse, Location, Product, StockMovement)
- T2: Catalog DB config + migration
- T3: Product CRUD (CreateProduct, UpdateProduct, ListProducts, GetProductBySku)
- T4: Stock movement (Receive, Ship, Adjust, Reserve, Release, Transfer) + computed balance
- T5: API endpoints (Product + Stock endpoints)
- T6: TenantMiddleware (path-based tenant extraction)
- T7: Integration test (full flow)

---

## Karar Özeti

| Karar | Seçim | Neden |
|---|---|---|
| A. Test PostgreSQL | Yerel PG + Respawn | Docker zorunluluğu yok, lokal hızlı feedback |
| B. Super-admin login | Ayrı endpoint `POST /api/v1/admin/auth/login` | Temiz ayrım, claim'de `actor_type: super_admin` |
| C. JWT anahtar | `secrets/jwt-private.pem` / `jwt-public.pem` | Deterministik, restart'a dayanır |
| D. Logging | Serilog + Console | Faz 1 için yeterli |
| E. 2FA | M1'de yok | MFA scaffolding atlanıyor |
| F. Balance hesabı | Gerçek zamanlı computed (stored değil) | Tek bilgi kaynağı movements; stale balance yok |
| G. Migration | Code-First, EF Core | WMS.Migrate CLI ile yönetilir |
| H. DI pattern | `GetRequiredService<ISender>()` | Minimal API'de DI, leaner abstraction |

---

## Dosya Yapısı

```
src/backend/
├── Directory.Build.props          # C# 12, implicit usings, nullable, CPM
├── Directory.Packages.props       # Central package management
├── WMS.Shared/                    # Result<T>, exceptions, crypto, jwt
│   ├── Result/
│   │   ├── Result.cs
│   │   ├── ResultOfT.cs
│   │   └── ValidationProblem.cs
│   ├── Common/
│   │   ├── ITenantContext.cs
│   │   └── Cryptography/
│   │       ├── PasswordHasher.cs
│   │       ├── AesGcmService.cs
│   │       └── JwtTokenService.cs
│   └── Exceptions/
│       ├── DomainException.cs
│       ├── NotFoundException.cs
│       └── InvalidOperationException.cs
├── WMS.Domain/                    # Domain entity'leri, aggregate root'lar
├── WMS.Application/               # Command/Query handler'ları, DTO'lar
│   └── Identity/Commands/
│   │   ├── LoginCommand.cs
│   │   └── AdminLoginCommand.cs
│   └── Admin/Commands/
│   │   ├── CreateTenantCommand.cs
│   │   └── UpdateTenantStatusCommand.cs
│   └── Admin/Queries/
│       ├── ListTenantsQuery.cs
│       └── TenantHealthQuery.cs
├── WMS.Infrastructure/            # EF Core DbContext'ler, services
│   └── Services/
│   │   ├── CachedTenantConnectionFactory.cs
│   │   ├── AuthService.cs
│   │   └── CatalogRepository.cs
│   └── Persistence/
│       └── AppDbContext.cs
├── WMS.Infrastructure.Catalog/    # Catalog DB entity'leri + mapping
│   ├── Entities/
│   ├── Configurations/
│   └── CatalogDbContext.cs
├── WMS.Api/                       # Minimal API endpoint'leri
│   ├── Program.cs
│   ├── GlobalUsings.cs
│   ├── Endpoints/
│   │   ├── Auth/
│   │   │   ├── LoginEndpoint.cs
│   │   │   └── AdminLoginEndpoint.cs
│   │   └── Admin/
│   │       └── TenantEndpoints.cs
│   └── Middleware/
│       └── TenantResolutionMiddleware.cs
├── WMS.Migrate/                   # CLI tool
│   └── Program.cs
└── tests/
    ├── WMS.Domain.UnitTests/
    ├── WMS.Application.UnitTests/
    └── WMS.Api.IntegrationTests/
        └── Fixtures/
            └── SharedPostgreSqlFixture.cs
```

---

## Ortak Kalıplar

### Handler Pattern
```csharp
public record MyCommand(string Data) : IRequest<Result<Guid>>;

public class MyHandler(
    MyDbContext dbContext,
    ITenantContext tenantContext)
    : IRequestHandler<MyCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(MyCommand request, CancellationToken ct)
    {
        // validate
        var entity = new MyEntity { ... };
        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(ct);
        return Ok(entity.Id);
    }
}
```

### Endpoint Pattern
```csharp
public static class MyEndpoint
{
    public static IEndpointRouteBuilder MapMyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/my-endpoint", HandleCreate);
        return app;
    }

    private static async Task HandleCreate(HttpContext context)
    {
        var sender = context.RequestServices.GetRequiredService<ISender>();
        var req = await context.Request.ReadFromJsonAsync<MyRequest>(...);
        var command = new MyCommand(req.Data);
        var result = await sender.Send(command, context.RequestAborted);

        if (result.IsSuccess)
        {
            context.Response.StatusCode = 201;
            await context.Response.WriteAsJsonAsync(result.Value, ...);
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = result.Message }, ...);
        }
    }
}
```

### Result Pattern Kullanım
```csharp
private static Result<string> Fail(string errorCode, string message)
    => Result<string>.MakeFailure(errorCode, message);

private static Result<string> Ok(string value)
    => Result<string>.MakeSuccess(value);
```

---

## Notlar

- Tüm proje Türkçe dokümantasyon ve isimlendirmelerle ilerliyor
- Git commit mesajları İngilizce, ama codebase'de Türkçe isimler kullanılabilir
- Docker kullanımı optional — local PostgreSQL yeterli
- CPM (Central Package Management) zorunlu — `Directory.Packages.props` üzerinden tüm paket versiyonları yönetilir
- .NET 10 preview kullanılıyor (preview warnings: NETSDK1057)
