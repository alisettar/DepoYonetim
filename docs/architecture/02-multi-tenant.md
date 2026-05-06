# 02 — Multi-tenant Modeli

> Catalog DB şeması, request lifecycle, tenant lifecycle, `WMS.Migrate` CLI burada uygulama detaylarıyla zenginleştirilir.

## Veritabanı mimarisi

```
┌─────────────────────┐
│  Catalog DB (shared) │ ← tüm tenant metadata, auth, kullanıcı
│                     │ ← TenantDatabases (connstr'lar şifreli)
│  Tenants            │
│  TenantDatabases    │
│  Users              │
└────────┬────────────┘
         │ her tenant'ın kendi PostgreSQL DB'si
         ▼
┌────────────────┐  ┌────────────────┐  ┌────────────────┐
│  Tenant A DB   │  │  Tenant B DB   │  │  Tenant N DB   │
│                │  │                │  │                │
│  Products      │  │  Products      │  │  Products      │
│  Movements     │  │  Movements     │  │  Movements     │
│  Balances      │  │  Balances      │  │  Balances      │
│  Lots          │  │  Lots          │  │  Lots          │
│  Recipes       │  │  Recipes       │  │  Recipes       │
│  FifoLayers    │  │  FifoLayers    │  │  FifoLayers    │
└────────────────┘  └────────────────┘  └────────────────┘
```

## Tenant resolution — request lifecycle

1. HTTP istek gelir
2. `TenantResolutionMiddleware` devreye girer
3. Auth header'dan Bearer token çıkarılır
4. JWT doğrulanır → `tenant_id`, `tenant_code`, `userId`, `email` claim'lerinden çekilir
5. `CachedTenantConnectionFactory.GetConnectionString(tenantId)` çağrılır
6. Cache miss ise `TenantConnectionResolver` Catalog DB'den tenant record'ını çeker
7. Şifreli password `AesGcmService.Decrypt()` ile çözülür
8. `AppDbContext` bu connection string ile kurulur
9. Request business logic işler
10. Response dönülür

## Bileşenler

### TenantResolutionMiddleware

`WMS.Api/Middleware/TenantResolutionMiddleware.cs`

- `/health`, `/api/v1/auth`, `/swagger`, `/favicon` yollarını atlar
- Auth header yoksa → `DefaultTenantId`'ye bağlanır (dev-bypass, bak: altta)
- Auth header varsa → JWT'den tenant_id çıkarır
- `tenant_id` claim yoksa veya invalid token → 401 Unauthorized
- Tenant connstr bulunamazsa → 402 PaymentRequired (tenant aktif değil)
- `context.Items["TenantContext"]` set edilir → downstream tüm kod bu kullanır

### CachedTenantConnectionFactory

`WMS.Infrastructure/Services/CachedTenantConnectionFactory.cs`

- `IMemoryCache` ile 10 dakika TTL
- `GetConnectionString(Guid)` — senkron (cache için gerekli)
- `GetConnectionStringAsync(Guid, CancellationToken)` — asenkron

### TenantConnectionResolver

`WMS.Infrastructure/Services/TenantConnectionResolver.cs`

- `CatalogDbContext` ile shared DB'ye bağlanır
- `TenantDatabases` → `Tenants` join ile tam record alır
- Status kontrolü: tenant "Active" olmalı
- Password şifresi AES-GCM ile çözülür
- Connection string formatı: `Host=...;Port=...;Database=...;Username=...;Password=...`

### TenantContext

`WMS.Shared/Common/TenantContext.cs`

- `ITenantContext` implement eder
- `tenantId`, `tenantCode`, `userId`, `email`, `actorType`
- `FromClaims()` static factory — middleware'den çağrılır
- `IHttpContextAccessor` üzerinden downstream'e inject edilir

### WMS.Migrate CLI

Tenant DB'sini provision eder:
- Migration'ları apply eder
- Seed data oluşturur (varsayılan kategori, birimler, örnek ürünler)
- Tenant oluşturulduğunda otomatik veya manuel trigger ile çalışır

## Dev-bypass durumu

**Şu anki durum:** Middleware'in `ValidateJwtToken()` metodu JWT parçalarını string olarak splitter (`token.Split('.')`), 3 parça varsa boş bir `ClaimsPrincipal` döndürür. Gerçek JWT doğrulama (`JwtTokenService.ValidateToken()`) henüz çağrılmıyor.

**Sonuç:**
- Auth header yoksa → `DefaultTenantId` ile tek bir dev database'e bağlanır
- Auth header varsa ama invalid → 401 (token kontrolü yapılıyor)
- Auth header varsa ama empty ClaimsPrincipal → `tenant_id = Guid.Empty` → dev database'e bağlanır

**Production hedefi:**
- `ValidateJwtToken()` → `JwtTokenService.ValidateToken()` ile değiştirilecek
- JWT RS256 ile imzalanmış, 15 dakika geçerli
- `tenant_id`, `tenant_code`, `actor_type` claim'leri JWT'den okunacak
- Dev-bypass tamamen kaldırılacak veya `ASPNETCORE_ENVIRONMENT=Development` ile aktif hale getirilecek

**Geçici dev konfigürasyonu:**
- `appsettings.json` veya env var: `DefaultTenantId`
- `DevTenantConnectionFactory` → hardcoded connstr, tenant filter atlar
- `Program.cs`'de tek bir `AppDbContext` instance'ı, tüm istekler aynı DB'ye gider

## Güvenlik

- Tenant connstr'ları `AesGcmService` ile AES-GCM şifreli olarak Catalog DB'de saklanır
- JWT RS256 (RSA public/private key pair) ile imzalanır
- Her tenant'ın veritabanı izole — bir tenant'ın DbContext'i başka tenant'ın verisine erişemez
- Tenant ID, request header veya URL path'ten manuel okunamaz — sadece JWT'den gelir
