# M1 — Altyapı + Catalog DB + Auth + WMS.Migrate CLI

**Tarih:** 2026-05-06
**Durum:** Plan
**Spec referansı:** `docs/superpowers/specs/2026-05-06-depo-yonetim-design.md`
**Çıktı:** `WMS.sln` + tüm 6 projenin csproj'leri, Catalog DB şeması + migration, CLI (`apply`, `create-tenant`), JWT auth (login/refresh/me), super-admin tenant CRUD + ilk integration test.

---

## Karar Özeti

| Karar | Seçim | Neden |
|---|---|---|
| A. Test PostgreSQL | **A2** — Yerel PG + Respawn | Docker zorunluluğu yok, lokal hızlı feedback. CI'da `services: postgres`. |
| B. Super-admin login | **B1** — Ayrı endpoint `POST /api/v1/admin/auth/login` | Temiz ayrım, claim'de `actor_type: super_admin`. |
| C. JWT anahtar | **C1** — `secrets/jwt-private.pem` / `jwt-public.pem` (gitignore) | Deterministik, restart'a dayanır. |
| D. Logging | **Serilog + Console** | Faz 1 için yeterli. |
| E. 2FA | **M1'de yok** | MFA scaffolding atlanıyor. |

---

## Görevler (TDD — her görevde önce failing test → implement → commit)

### T1 — Solution + csproj iskeleti

**Hedef:** `WMS.sln` oluşturulur, tüm projeler compile olur (boş ama geçerli).

**Dosyalar:**
- `src/backend/WMS.sln`
- `src/backend/Directory.Build.props` — `<LangVersion>12</LangVersion>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<Nullable>enable</Nullable>, NoWarn>NET9008</NoWarn>`
- `src/backend/Directory.Packages.props` — Central package management, `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`, `<GlobalPropertiesRemovePackageVersionAnnotation>true</GlobalPropertiesRemovePackageVersionAnnotation>`
- `src/backend/WMS.Shared/WMS.Shared.csproj`
- `src/backend/WMS.Infrastructure.Catalog/WMS.Infrastructure.Catalog.csproj`
- `src/backend/WMS.Infrastructure/WMS.Infrastructure.csproj`
- `src/backend/WMS.Application/WMS.Application.csproj`
- `src/backend/WMS.Api/WMS.Api.csproj`
- `src/backend/WMS.Migrate/WMS.Migrate.csproj` (exe)
- `src/backend/tests/WMS.Domain.UnitTests/WMS.Domain.UnitTests.csproj`
- `src/backend/tests/WMS.Application.UnitTests/WMS.Application.UnitTests.csproj`
- `src/backend/tests/WMS.Api.IntegrationTests/WMS.Api.IntegrationTests.csproj`

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

**Testler:** Derleme testi yok (basit). `dotnet build` başarılı olmalı.

**İlk commit** — iskelet + .gitignore güncelleme:
```
git add . && git commit -m "Initial skeleton: solution + csproj + Directory.props"
```

---

### T2 — Test altyapısı + ilk dummy test

**Hedef:** xUnit runner çalışır, Respawn ile DB temizleme var, dummy test geçiyor.

**NuGet paketleri:**
- `xunit` `2.9.2`
- `xunit.runner.visualstudio` `2.8.2`
- `Npgsql` `8.0.6`
- `Respawn` `5.0.6`
- `Testcontainers.PostgreSql` **YOK** (A2 kararı)

**Dosyalar:**
- `tests/WMS.Api.IntegrationTests/Fixtures/SharedPostgreSqlFixture.cs` — local PG'ye bağlanır, testler arasında Respawn ile içerik temizler
- `tests/WMS.Api.IntegrationTests/Scenarios/SmokeTest.cs` — "PG bağlantısı çalışıyor" assertion'u

**Testler:**
1. `SmokeTest_PgConnect_ShouldReturnTrue`

---

### T3 — WMS.Shared: Result<T>, hata kodları, exception'lar

**Hedef:** Cross-cutting hatalar tanımlı.

**Dosyalar:**
- `WMS.Shared/Result/TResult.cs` — `Result<T>` static factory: `Success<T>`, `Failure<T>(message)`, `Failure<T>(code, message)`, pattern matching extension'lar
- `WMS.Shared/Result/Result.cs` — `Result` (void), `Success()`, `Failure(message)`, `Failure(code, message)`
- `WMS.Shared/Result/ValidationProblem.cs` — validation hataları için (`errors[]` koleksiyonu)
- `WMS.Shared/Exceptions/DomainException.cs`
- `WMS.Shared/Exceptions/NotFoundException.cs`
- `WMS.Shared/Exceptions/InvalidOperationException.cs`
- `WMS.Shared/Common/Cryptography/PasswordHasher.cs` — Argon2id wrapper (Konscious.Security.Cryptography)
- `WMS.Shared/Common/Cryptography/AesGcmService.cs` — AES-GCM şifreleme/çözümleme

**NuGet:** Konscious.Security.Cryptography `1.3.0`

**Testler:**
- `PasswordHasher_ShouldHashAndVerify`
- `PasswordHasher_ShouldNotVerifyWrongPassword`
- `AesGcmService_ShouldEncryptAndDecrypt`
- `Result_ShouldSupportSuccessFailurePatternMatch`
- `Result_ShouldReturnCorrectTypes`

---

### T4 — Catalog DB entity'leri + EF config + ilk migration

**Hedef:** Catalog DB şeması EF Code-First ile tanımlı, migration oluşturuldu.

**Dosyalar:**
- `WMS.Infrastructure.Catalog/Entities/SuperAdmin.cs`
- `WMS.Infrastructure.Catalog/Entities/Tenant.cs`
- `WMS.Infrastructure.Catalog/Entities/TenantDatabase.cs`
- `WMS.Infrastructure.Catalog/Entities/UserLookup.cs`
- `WMS.Infrastructure.Catalog/Entities/AuditGlobal.cs`
- `WMS.Infrastructure.Catalog/Entities/Enums/TenantStatus.cs`
- `WMS.Infrastructure.Catalog/Entities/Enums/AuditActorType.cs`
- `WMS.Infrastructure.Catalog/CatalogDbContext.cs` — `DbSet<SuperAdmin>`, `DbSet<Tenant>`, vb.
- `WMS.Infrastructure.Catalog/Configurations/SuperAdminConfiguration.cs`
- `WMS.Infrastructure.Catalog/Configurations/TenantConfiguration.cs`
- `WMS.Infrastructure.Catalog/Configurations/TenantDatabaseConfiguration.cs`
- `WMS.Infrastructure.Catalog/Configurations/UserLookupConfiguration.cs`
- `WMS.Infrastructure.Catalog/Configurations/AuditGlobalConfiguration.cs`
- `WMS.Infrastructure.Catalog/Migrations/InitialCatalog.cs`

**Testler:**
- `MigrationSnapshot_ShouldMatchEntities` — migration'ın entity'leri yansıttığını doğrula (reflection + snapshot karşılaştırması)

---

### T5 — TenantConnectionResolver + IMemoryCache

**Hedef:** `TenantConnectionResolver` catalog'dan tenant'ın connection string'ini çözer, IMemoryCache ile 10dk TTL.

**Dosyalar:**
- `WMS.Infrastructure/Services/TenantConnectionResolver.cs`
- `WMS.Shared/Common/ITenantContext.cs` — `TenantId`, `TenantCode`, `UserId`, `ActorType`
- `WMS.Infrastructure/Services/CachedTenantConnectionFactory.cs` — resolver + cache wrapper

**Testler:**
- `TenantConnectionResolver_ShouldReturnConnectionStringForActiveTenant`
- `TenantConnectionResolver_ShouldReturnNullForDeletedTenant`
- `CachedConnectionFactory_ShouldCacheFor10Minutes`
- `CachedConnectionFactory_ShouldInvalidOnTenantStatusChange`

---

### T6 — WMS.Migrate CLI: apply + create-tenant

**Hedef:** `wms-migrate` CLI — `apply --tenant-id <id>` ve `create-tenant <code> <name>` komutları çalışır.

**Dosyalar:**
- `WMS.Migrate/Program.cs` — command line parsing (Microsoft.Extensions.Hosting Console app)
- `WMS.Migrate/Commands/ApplyCommand.cs` — tek tenant veya --all
- `WMS.Migrate/Commands/CreateTenantCommand.cs` — DB create + apply migrations + seed
- `WMS.Migrate/Seeders/DefaultWarehouseSeeder.cs` — "ANA-DEPO" default warehouse

**Testler:**
- `CreateTenant_ShouldCreateDbAndApplyMigrations` — integration test (SharedPostgreSqlFixture ile)

---

### T7 — JWT Token Service (RS256)

**Hedef:** RS256 JWT üretimi, access 15dk + refresh 7gün, rotasyonlu refresh token.

**Dosyalar:**
- `WMS.Shared/Common/Cryptography/JwtTokenService.cs` — RS256, key pem'den okuma
- `WMS.Shared/Common/Models/JwtClaims.cs` — claim modelleri
- `WMS.Shared/Common/Models/RefreshToken.cs` — DB model

**NuGet:** `System.IdentityModel.Tokens.Jwt` `8.0.1`, `Microsoft.IdentityModel.Tokens` `8.0.1`

**Testler:**
- `JwtService_ShouldGenerateAccessAndRefreshToken`
- `JwtService_ShouldValidateAccessTokens`
- `JwtService_ShouldRejectExpiredToken`
- `JwtService_ShouldHaveCorrectClaimValues`

---

### T8 — TenantResolutionMiddleware + ITenantContext

**Hedef:** Middleware JWT'den tenantId okur, request scope'a ITenantContext enjekte eder.

**Dosyas:**
- `WMS.Api/Middleware/TenantResolutionMiddleware.cs`
- `WMS.Api/Middleware/TenantResolutionMiddlewareExtensions.cs`
- `WMS.Api/Services/CurrentTenantContext.cs` — implementation

**Testler:**
- `Middleware_ShouldSetTenantContextFromJwt`
- `Middleware_ShouldReturn401WhenNoJwt`
- `Middleware_ShouldReturn402WhenTenantSuspended`

---

### T9 — Auth endpoint'leri

**Hedef:** `POST /api/v1/auth/login`, `/refresh`, `/me`. Admin login: `/api/v1/admin/auth/login`.

**Dosyalar:**
- `WMS.Api/Endpoints/Auth/LoginEndpoint.cs` — endpoint definition
- `WMS.Application/Identity/Commands/LoginCommand.cs` — command + handler
- `WMS.Application/Identity/Commands/RefreshTokenCommand.cs`
- `WMS.Application/Identity/Queries/GetMeQuery.cs`
- `WMS.Api/Endpoints/Admin/AdminLoginEndpoint.cs`
- `WMS.Application/Identity/Commands/AdminLoginCommand.cs`
- DTO'lar: `LoginRequest`, `LoginResponse`, `MeResponse`

**Testler:**
- `Login_ValidCredentials_ShouldReturnToken` — integration test
- `Login_WrongPassword_ShouldReturn401`
- `Login_Admin_ShouldReturnActorTypeSuperAdmin`
- `Me_ShouldReturnUserDetails`

---

### T10 — Super-admin tenant CRUD

**Hedef:** `POST /api/v1/admin/tenants`, `GET /api/v1/admin/tenants`, `PATCH /tenants/{id}/status`, `GET /tenants/{id}/health`.

**Dosyalar:**
- `WMS.Api/Endpoints/Admin/TenantEndpoints.cs`
- `WMS.Application/Admin/Commands/CreateTenantCommand.cs`
- `WMS.Application/Admin/Queries/ListTenantsQuery.cs`
- `WMS.Application/Admin/Commands/UpdateTenantStatusCommand.cs`
- `WMS.Application/Admin/Queries/TenantHealthQuery.cs`
- DTO'lar: `CreateTenantRequest`, `TenantDto`, `TenantHealthDto`
- `WMS.Api/Filters/AdminAuthFilter.cs` — super-admin middleware pipeline

**Testler:**
- `CreateTenant_ShouldCreateCatalogEntryAndReturn201` — integration test
- `ListTenants_ShouldReturnAllActive`
- `UpdateTenantStatus_Suspend_ShouldBlockRequests`
- `TenantHealth_ShouldReturn200ForActiveTenant`

---

### T11 — İlk full integration test (end-to-end flow)

**Hedef:** Super-admin login → tenant oluştur → CLI ile create-tenant (simüle) → tenant login → health check 200.

**Dosyalar:**
- `tests/WMS.Api.IntegrationTests/Scenarios/FullTenantOnboardingFlow.cs`

**Testler:**
- `FullTenantOnboardingFlow_ShouldWorkEndToEnd`

---

## parallelization stratejisi

Bağımsız gruplar:

| Grup | Görevler | Bağımlılık |
|---|---|---|
| **P0** | T1, T2 | Hiçbiri |
| **P1** | T3 | T1 |
| **P2** | T4, T5 | T3 |
| **P3** | T6, T7, T8 | T4, T5, T3 |
| **P4** | T9 | T7, T8 |
| **P5** | T10 | T8 |
| **P6** | T11 | T9, T10 |

Ardışık: P0 → P1 → P2/P3 → P4/P5 → P6

---

## Git stratejisi

1. `git add .` → `git commit -m "Initial: skeleton, solution, directories"` (T1 sonrası)
2. Tüm task'lar tek branch'de, commit'lar atomic:
   - `chore: add solution + csproj files`
   - `feat: add Result<T> pattern and exceptions`
   - `feat: add catalog entities + initial migration`
   - `feat: add tenant connection resolver with cache`
   - `feat: add CLI migrate tool`
   - `feat: add JWT token service`
   - `feat: add tenant resolution middleware`
   - `feat: add auth endpoints`
   - `feat: add super-admin tenant CRUD`
   - `test: add full onboarding flow integration test`
3. Push: `git push -u origin main` (remote henüz push edilmemiş, ilk push)

---

## Gözden kaçırılmamalı

- `Directory.Packages.props`'da `<GlobalPropertiesRemovePackageVersionAnnotation>` → `Directory.Build.props`'a da eklemeliyiz, VS'nin "All Projects" görünümü için
- `.gitignore`'a `secrets/`, `*.pem`, `bin/`, `obj/`, `.vs/`, `node_modules/` eklenmeli
- `WMS.Migrate` projesi `OutputType=Exe` olmalı
- Tüm test projeleri `IsTestProject=true` işaretli olmalı (CI için)
