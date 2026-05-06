# Depo Yönetim Sistemi — Tasarım Dokümanı

**Tarih:** 2026-05-06
**Durum:** Taslak (kullanıcı incelemesi bekleniyor)
**Yazar:** Brainstorming oturumu — Alisettar (alisettar@dolusoft.com) ile

---

## 1. Genel Bakış

Karışık imalat sektörüne yönelik, çok kiracılı (multi-tenant) bulut SaaS depo yönetim sistemi. Stok hareketleri, depo türleri (fiziksel, sanal, makine), lot/parti bazlı izlenebilirlik ve çok seviyeli reçete (BOM) yönetimi sağlar. Sistemin merkezi sabit düzenli bir dashboard'tur; kritik stok, depo doluluk, son hareketler ve lot arama widget'ları zorunlu olarak yer alır.

**Hedef ölçek:** 500+ tenant (şirket), aktif eşzamanlı ~250 tenant. Tenant başına 5–100 kullanıcı, 1–10 depo, 500–20.000 SKU, günlük 100–5.000 stok hareketi.

**Teknoloji:** .NET 9 ASP.NET Core Web API + Vue.js 3 SPA + PostgreSQL + Entity Framework Core (Code-First).

---

## 2. Sözlük (Glossary)

| Terim | Tanım |
|---|---|
| **Tenant** | Sistemi kullanan bağımsız şirket. Kendi DB'si ve veri izolasyonu vardır. |
| **Catalog DB** | Tüm tenant'ların kataloğu, kullanıcı eşleşmesi ve sistem yönetimi için kullanılan tek paylaşımlı PostgreSQL veritabanı. |
| **Tenant DB** | Bir tenant'a ait tüm domain verisini tutan ayrı PostgreSQL veritabanı. Şema tüm tenant'larda aynıdır. |
| **Lot / Çeki / Parti** | Tek kavram — üretim/sevk partisi numarası. Bu doküman boyunca **lot** terimi kullanılır. |
| **Fiziksel depo** | Gerçek alan/bina; alt konum hiyerarşisi (koridor / raf / göz) içerir. |
| **Sanal depo** | Mantıksal grup (karantina, sevk hazırlık, müşteri rezervi). Fiziksel envanteri olmaz. |
| **Makine deposu** | Bir makinenin "üzerindeki" yük. Sadece bağlı makine tarafından kullanılır. Müşteriye doğrudan sevk yapılamaz. |
| **FIFO katmanı (FifoLayer)** | Bir mal kabulü/üretim çıkışı sonucunda doğan, FIFO çıkışlarda sırasıyla tüketilen stok dilimi. |
| **Snapshot (StockBalance)** | Depo + ürün + lot kırılımında anlık stok bakiyesi. Hareketlerden türetilir; performans için tutulur. |
| **Reçete (BOM)** | Bir mamulün üretiminde hangi malzemelerin hangi miktarlarda kullanıldığını tanımlayan yapı. Çok seviyeli, versiyonlu, alternatif malzemeli. |
| **Üretim emri (UE)** | Belirli miktarda mamul üretim talimatı. Bağlı bir makinede çalışır; başlatıldığında makine deposundan otomatik tüketim olur (varsayım). |

---

## 3. Kapsam

### 3.1 Faz 1 (bu spec'in kapsamı)

- Multi-tenant altyapı (catalog DB + DB-per-tenant + manuel migration runner)
- Auth: e-mail/parola, JWT (RS256), opsiyonel TOTP 2FA
- Ürün, kategori, ürün-bazlı birim/dönüşüm yönetimi
- Depo (fiziksel + alt konum, sanal, makine) yönetimi
- Stok hareketleri: mal kabul, sevk, transfer, sayım/fire/iade, makine yükle/indir, üretim sarfiyatı, üretim çıkışı
- Append-only event log + snapshot + FIFO katman motoru
- Lot yönetimi (mal kabul + üretimde doğar; ürün bazlı `LotRequired`)
- Reçete: çok seviyeli sınırsız, versiyonlu, alternatif malzemeli, fire (oransal + miktarsal)
- Üretim emri (skeleton): aç/başlat/çıkış/tamamla/iptal — operasyon ve rota yok
- Stok hatası düzeltme: ters hareket, sayım farkı, void, rebuild (4 seviyeli)
- Dashboard zorunlu widget'lar: kritik stok, depo doluluk, son hareketler, lot arama
- Lot izlenebilirlik: **geriye doğru** (bu lot hangi hammadde lotlarından üretildi)
- Standart roller (TenantAdmin, WarehouseManager, Operator, Viewer); her rol için permission seti **sabit gelir**. Tenant admin kullanıcıları rollere atar (custom rol/permission düzenleme **faz 2**).
- Test: unit (Domain) + integration (shared test PostgreSQL)

### 3.2 Faz 2 (kapsam dışı — sonraya bırakılmıştır)

- Reçete operasyonları, rota (routing), iş istasyonu, makine saat ücreti
- Üretim emri detayları (kısmi teslim, takvimleme)
- ERP entegrasyonu, barkod/RFID/mobil uygulama
- E-mail / SMS / push bildirim sistemi
- Public API gateway, 3. parti entegrasyon
- Dashboard'un faz 2 widget'ları: toplam stok değeri, makine yük durumu, bekleyen UE, yaklaşan SKT, hareketsiz stok, günlük grafik
- Lot izlenebilirlik **ileriye doğru** (bu lot hangi mamullerin üretiminde kullanıldı)
- Cloud secret manager (KMS) — faz 1'de catalog DB şifreli kolon yeterli
- E2E testler (Cypress/Playwright)
- OpenTelemetry, Prometheus, dağıtık izleme
- Kubernetes manifest'leri

### 3.3 Açıkça yapılmayacaklar

- Üretim çıkış maliyetinde işçilik/makine saat ücreti (sadece tüketilen malzemelerin ağırlıklı toplamı)
- Geçmişe dönük (`occurredAt < now`) hareket girişi (FIFO sırasını korumak için yasak)
- Mikroservis mimarisi (Yaklaşım B reddedildi — modüler monolith disiplini ile gidiyoruz)
- Ayrı message broker (RabbitMQ/Kafka). Faz 2'de Worker ihtiyacı çıkarsa Hangfire + PostgreSQL üzerinden ele alınır.

---

## 4. Yüksek Seviye Mimari (Yaklaşım A — Modüler Monolith)

### 4.1 Topoloji

```
                  ┌─────────────────────┐
                  │   Vue.js 3 SPA      │  (statik host: nginx / Azure SWA)
                  └──────────┬──────────┘
                             │ HTTPS / JWT (RS256)
                             ▼
                  ┌─────────────────────┐
                  │   WMS.Api           │  ASP.NET Core 9 Web API
                  │  (modüler monolith) │  → yatay ölçek (N replica, stateless)
                  └──┬───────────────┬──┘
                     │               │
           ┌─────────▼─────┐   ┌─────▼──────────────────┐
           │ Catalog DB    │   │ Tenant DBs (N adet)    │
           │ (PostgreSQL)  │   │ wms_tenant_001, ...    │
           │ — tenants     │   │ (her biri aynı şema)   │
           │ — user_lookup │   └────────────────────────┘
           │ — super_admin │
           └───────────────┘
```

API stateless. Tenant kimliği her HTTP isteğinin JWT claim'i ile gelir. Middleware bunu çözüp request scope'una `ITenantContext` enjekte eder. `AppDbContext` o tenant'ın connection string'iyle açılır.

### 4.2 Solution yapısı (klasör/namespace ile modül ayrımı)

```
WMS.sln
├── src/
│   ├── WMS.Api/                       (composition root, endpoints, middleware)
│   ├── WMS.Application/               (use-case'ler, MediatR handler'lar, DTO, validator)
│   ├── WMS.Domain/                    (entity, value object, domain event, kural)
│   ├── WMS.Infrastructure/            (tenant DB context, repository, dış servisler)
│   ├── WMS.Infrastructure.Catalog/    (catalog DB context)
│   ├── WMS.Migrate/                   (CLI tool: migration ve tenant yönetimi)
│   └── WMS.Shared/                    (cross-cutting: Result<T>, exception, util)
└── tests/
    ├── WMS.Domain.UnitTests/
    ├── WMS.Application.UnitTests/
    └── WMS.Api.IntegrationTests/      (shared test PostgreSQL)
```

`WMS.Domain` ve `WMS.Application` içinde modüller **klasör/namespace** ile ayrılır:
`Tenancy / Identity / Catalog / Warehousing / Inventory / Recipes / Production / Reporting`.

### 4.3 Mimari prensipler

1. **Clean Architecture katmanları:** `Domain` ← `Application` ← `Infrastructure` / `Api` (içeri bağımlılık)
2. **CQRS-lite (MediatR):** Command ve Query ayrı handler'lar, ama tek DB
3. **Result pattern:** Validation/business hataları `Result<T>`; altyapı hataları exception
4. **Tenant izolasyonu DB seviyesinde:** Hiçbir entity'de `tenant_id` kolonu yok; yanlış DB = boş DB
5. **Audit:** Stok hareketi append-only — silinmez, güncellenmez. Ters hareket veya void ile düzeltme
6. **Stateless API:** Yatay ölçek; sticky session yok

---

## 5. Multi-tenant Modeli

### 5.1 Catalog DB şeması

```sql
super_admins (
  id              uuid PK,
  email           citext UNIQUE NOT NULL,
  password_hash   text NOT NULL,         -- Argon2id
  mfa_secret      text NULL,
  is_locked       boolean,
  created_at      timestamptz
)

tenants (
  id              uuid PK,
  code            text UNIQUE NOT NULL,
  name            text NOT NULL,
  status          text  -- Active | Suspended | Failed | Deleted
  plan            text,
  created_at      timestamptz
)

tenant_databases (
  tenant_id       uuid FK -> tenants(id) PK,
  host            text,
  port            int,
  db_name         text,
  username        text,
  password_enc    bytea,                  -- AES-GCM (master key env)
  region          text
)

user_lookup (
  id              uuid PK,
  email           citext UNIQUE NOT NULL, -- GLOBAL UNIQUE — bir e-mail tek tenant'a aittir
  password_hash   text NOT NULL,           -- Argon2id
  tenant_id       uuid FK -> tenants(id),
  mfa_secret      text NULL,
  failed_attempts int,
  is_locked       boolean,
  last_login_at   timestamptz,
  created_at      timestamptz
)

audit_global (
  id, occurred_at, actor_id, actor_type (super_admin | system),
  action, target_type, target_id, ip
)
```

### 5.2 Tenant DB şeması (her tenant için aynı)

```
warehouses, warehouse_locations, machine_warehouses
products, product_units, units, categories
lots
stock_movements    (append-only event log)
stock_balances     (snapshot — depo bazlı)
fifo_layers        (FIFO katmanları, lot bazlı)
recipes, recipe_versions, recipe_items, alternative_materials
production_orders  (skeleton)
production_outputs
users              (parola YOK; sadece detay: full_name, role_id, status)
roles, permissions, role_permissions
tenant_settings    (allow_negative_stock vs.)
audit_tenant       (kim/ne/ne zaman)
```

### 5.3 İstek akışı (request lifecycle)

```
1. Vue → POST /api/v1/auth/login { email, password }    ← tenant kodu YOK
2. WMS.Api → Catalog DB: user_lookup'tan email bul → password doğrula → tenant_id al
3. Tenant DB'sine bağlan, users tablosunda email ile detay (rol, permission) bul
4. JWT üret: { sub: tenantUserId, tenant: tenantId, role, perms, email }
5. SPA → her sonraki istekte Authorization: Bearer <jwt>
6. TenantResolutionMiddleware → JWT'den tenant_id okur, ITenantContext'e koyar
7. Endpoint MediatR handler çağırır
8. Handler IDbContextFactory<AppDbContext> üzerinden:
   - ITenantContext.TenantId al
   - ICatalogDb'den tenant'ın connection string'ini cache'ten oku (IMemoryCache, TTL 10 dk)
   - AppDbContext'i o connection string ile aç
9. Sorgu çalıştır → response
```

### 5.4 Kullanıcı oluşturma (atomicity — iki DB)

Tenant admin yeni kullanıcı eklediğinde:
1. **Önce catalog'a yaz** (`user_lookup` — unique violation = e-mail başka tenant'ta var → reddet)
2. **Sonra tenant DB'ye yaz** (`users`)
3. Tenant DB yazma başarısız olursa → catalog'da kaydı sil (compensating transaction)
4. (Faz 2 outbox pattern ile failure-safe hale getirilebilir)

### 5.5 Tenant yaşam döngüsü

| Olay | Akış |
|---|---|
| Yeni tenant | Super-admin → catalog satırı + `wms-migrate create-tenant <code>` CLI çalıştırır → DB create + migration + seed (default warehouse, admin user) → catalog `status=Active` |
| Migration | Deploy sonrası **elle** `wms-migrate apply --all --parallel 4` çalıştırılır (otomatik catch-up YOK) |
| Suspend | catalog `status=Suspended` → middleware 402 döner |
| Delete | DB dump alınır, soft-delete catalog'da, X gün sonra fiziksel silme |

### 5.6 Performans (~250 eşzamanlı tenant)

- **Npgsql connection pool** her tenant için ayrı pool tutar
- Production'da **PgBouncer transaction-pool mode** önerilir
- API'de `IDbContextFactory<AppDbContext>` kısa ömürlü context oluşturur
- Tenant connection string'leri **catalog'dan okunur, IMemoryCache 10 dk TTL**
- Tenant DB parolaları catalog'da **AES-GCM ile şifreli** (master key env'de — KMS faz 2)

### 5.7 CLI tool — `WMS.Migrate`

```
wms-migrate apply --tenant-id <id>      → tek tenant
wms-migrate apply --all --parallel 4    → tüm tenant'lar paralel
wms-migrate create-tenant <code> <name> → yeni tenant DB + migrate + seed
wms-migrate rebuild-balances --tenant <id>  → snapshot ve FIFO katmanları rebuild
wms-migrate status                      → her tenant'ın migration durumu
```

---

## 6. Domain Modeli

### 6.1 Aggregate'ler

| Aggregate Root | İçerdikleri |
|---|---|
| **Product** | ProductUnit'lar, kategori ref, `LotRequired` |
| **Warehouse** | WarehouseLocation'lar (tree) |
| **MachineWarehouse** | (ayrı root) |
| **RecipeVersion** | RecipeItem'lar, AlternativeMaterial'lar |
| **StockMovement** | (root, append-only) |
| **ProductionOrder** | (skeleton) |
| **Lot** | (root) |

### 6.2 Ana entity alanları

#### `Product`
```
Id (Guid), Code (string, unique), Name
PrimaryUnitId (Guid)              -- baz ölçü birimi
LotRequired (bool)                -- ürün bazlı flag
ShelfLifeDays (int?)              -- null = SKT yok
MinStock (decimal?)               -- kritik seviye widget için
MaxStock (decimal?)
Status (Active | Passive)
CreatedAt, UpdatedAt
```

#### `ProductUnit` (ürün-bazlı birim ve dönüşüm)
```
Id, ProductId, UnitId, ConversionToPrimary (decimal)
-- örn: vida → primary "adet"; ProductUnit "kutu" = 100 adet
```

#### `Warehouse`
```
Id, Code, Name
Type (Physical | Virtual)
ParentWarehouseId (Guid?)         -- depo grubu için (opsiyonel)
Status
```

#### `WarehouseLocation` (sadece Physical için)
```
Id, WarehouseId, Code, Name
ParentLocationId (Guid?)           -- self-ref tree → koridor > raf > göz
Capacity (decimal?)                -- doluluk widget'ı için
LocationPath (ltree)               -- PostgreSQL ltree, hızlı tree sorgu
```

#### `MachineWarehouse`
```
Id, Code, Name, MachineCode
Status (Idle | Loaded | Running)
```

#### `Lot`
```
Id, ProductId, LotNumber (tenant içinde unique per product)
ProductionDate, ExpiryDate (date?)
Source (Receipt | Production)
SourceReferenceId (Guid?)            -- mal kabul belgesi / üretim emri ref
QualityStatus (OK | Quarantine | Rejected)
```

#### `StockMovement` — sistemin kalbi (append-only)
```
Id, OccurredAt
Type (GoodsReceipt | Shipment | TransferIn | TransferOut |
      MachineLoad | MachineUnload | ProductionConsumption | ProductionOutput |
      InventoryAdjustmentIn | InventoryAdjustmentOut | Scrap |
      CustomerReturn | SupplierReturn | Reversal)

ProductId
LotId (Guid?)                       -- null → lotsuz
WarehouseId | MachineWarehouseId
LocationId (Guid?)                   -- sadece Physical depo
Quantity (decimal)                   -- +giriş / -çıkış (signed)
UnitId
UnitCost (decimal?)                  -- FIFO maliyetlendirme
ReferenceType, ReferenceId           -- belge ref (mal kabul, sevk irsaliyesi, UE)
ReversalOfId (Guid?)                 -- ters hareket için kaynak movement ref
IsVoided (bool)                      -- Seviye 3 void
VoidedAt, VoidedByUserId, VoidReason
SourceLotIds (jsonb?)                -- üretim sarfında: kaynak lotlar (geriye trace için)
Note
CreatedByUserId
RowVersion (xmin)
```
**Bu tablo SİLİNMEZ, GÜNCELLENMEZ.** Tek istisna: `IsVoided` flag (Seviye 3 — admin yetkisi).

#### `StockBalance` (snapshot — depo bazlı)
```
ProductId, LotId (nullable), WarehouseId | MachineWarehouseId
Quantity (decimal), UpdatedAt, RowVersion
PRIMARY KEY (ProductId, LotId, WarehouseId)
-- nullable kolonlar için PostgreSQL "NULLS NOT DISTINCT" (PG 15+)
-- LocationId snapshot'ta YOK; raf bakiyesi anlık stock_movements agregasyonu ile
```
Hareket eklendiğinde aynı transaction içinde UPSERT (`ON CONFLICT DO UPDATE`).

#### `FifoLayer`
```
Id, ProductId, WarehouseId | MachineWarehouseId
LocationId (nullable)
LotId (nullable)
ReceiptDate                          -- FIFO sırası bu alana göre
RemainingQuantity                    -- tüketildikçe azalır, 0 olunca kapalı (soft)
UnitCost
SourceMovementId
```

#### `Recipe` ve `RecipeVersion`
```
Recipe:        Id, ProductId, Name, Status
RecipeVersion: Id, RecipeId, VersionNo, ValidFrom, ValidUntil, IsActive,
               OutputQuantity, OutputUnitId
RecipeItem:    Id, RecipeVersionId, ProductId,    -- alt mamul de olabilir → çok seviyeli
               Quantity, UnitId,
               WastePercent (decimal?),            -- oransal fire
               WasteFixed (decimal?),              -- miktarsal fire
               SortOrder
AlternativeMaterial: Id, RecipeItemId, ProductId, Priority, Quantity, UnitId
```
Çok seviyeli sınırsız: `RecipeItem.ProductId` başka bir ürünün reçetesine işaret edebilir → recursive expansion runtime'da CTE ile.

### 6.3 Domain invariant'ları

1. `LotRequired = true` ürünün **lotsuz** stok hareketi reddedilir.
2. `LotRequired = false` ürün hem lotlu hem lotsuz girilebilir → snapshot'ta iki ayrı satır.
3. Makine deposundan müşteriye sevk **fiziksel olarak engellidir** (movement type whitelist).
4. Sanal depoya/-dan transfer her zaman serbest, ama fiziksel envanter değişmez.
5. Negatif stok yasak — varsayılan; `tenant_settings.allow_negative_stock` ile gevşetilebilir.
6. Çıkış hareketi her zaman **FIFO katman seçimi** ile yapılır; manuel lot override opsiyonel.
7. RecipeVersion için `ValidFrom..ValidUntil` aralıkları örtüşemez (DB EXCLUDE constraint, `tstzrange`).
8. RecipeItem'ın ürünü kendi reçetesine dolaylı dönmemeli (cycle detection — application seviyesinde).
9. `OccurredAt < now()` hareket girişi yasak (geçmişe dönük yok).
10. Movement silme yasak; sadece Reversal veya Void.

### 6.4 Stok hatası düzeltme stratejisi (4 seviye)

| Seviye | Senaryo | Mekanizma |
|---|---|---|
| **1. Reversal** | Yanlış miktar/lot girilmiş | Yeni `Reversal` movement (kaynak hareketin negatifi + `ReversalOfId`). Snapshot otomatik düzelir, FIFO katmanı geri açılır. Eski kayıt silinmez. |
| **2. Adjustment** | Sayım/fiziksel fark | `InventoryAdjustmentIn/Out` hareket tipi, neden zorunlu (sayım, fire, bulundu) |
| **3. Void** | Tamamen iptal (operatör hatası) | Super-admin yetkisi → hareketin `IsVoided=true` + `VoidReason` flag'lenir, snapshot etkisi geri alınır |
| **4. Rebuild** | Snapshot/FIFO bozulmuş | `wms-migrate rebuild-balances --tenant <id>` CLI: advisory lock al → yeni snapshot tablosuna sıfırdan üret → atomik rename → audit kaydı |

Rebuild **veri kaybetmez**, sadece türetilmiş veriyi yeniden hesaplar.

---

## 7. Veri Akışları

### 7.1 Mal kabul (Goods Receipt) — lotlu ürün

```
POST /api/v1/inventory/goods-receipts
        ↓
GoodsReceiptCommand → GoodsReceiptHandler
        ↓
DB transaction (ReadCommitted) BEGIN
  for each kalem:
    - Product yükle, LotRequired kontrol → uyumsuzluk = Result.Fail
    - Lot UPSERT (LotNo + ProductId unique) → mevcutsa kullan, yoksa yarat
    - StockMovement ekle (Type=GoodsReceipt, Qty=+, lotId, locationId)
    - StockBalance UPSERT (ON CONFLICT DO UPDATE)
    - FifoLayer ekle (ReceiptDate=now, RemainingQty=qty, UnitCost)
    - audit_tenant satırı
COMMIT
        ↓
DomainEvent: GoodsReceived → in-process handler (dashboard cache invalidate)
```

### 7.2 Sevk — FIFO çıkış

```
POST /api/v1/inventory/shipments
        ↓
ShipmentHandler
        ↓
TX BEGIN (Serializable izolasyon)
  for each kalem:
    - StockBalance kontrol → yetersizse:
        if !tenant.allow_negative_stock → Result.Fail("yetersiz stok")
    - FifoEngine.Consume(productId, qty, warehouseId):
        SELECT ... FROM fifo_layers
          WHERE remaining > 0 AND product = ... AND warehouse = ...
          ORDER BY receipt_date ASC, id ASC
          FOR UPDATE SKIP LOCKED
        loop until qty consumed:
          take min(layer.remaining, qty)
          update layer.remaining -= taken
          insert StockMovement (Type=Shipment, Qty=-taken, layer.lotId, unitCost=layer.cost)
        StockBalance UPDATE (-qty)
COMMIT
```

`FOR UPDATE SKIP LOCKED` ile aynı anda iki sevk farklı FIFO katmanlarından paralel çalışabilir; deadlock yok.

### 7.3 Transfer (depolar arası)

```
TransferHandler
TX BEGIN
  - kaynak: FIFO çıkış (Type=TransferOut)
  - hedef: yeni FifoLayer (kaynak layer'ın ReceiptDate ve UnitCost'unu KORUR)
           StockMovement (Type=TransferIn)
COMMIT
```

**Karar:** Transfer FIFO sırasını bozmaz — tarih taşınır (muhasebe açısından doğru).

### 7.4 Üretim akışı

```
A) Yük ekleme (operatör)
   POST /api/v1/machines/{id}/load
   → Transfer akışı (fiziksel depo → makine deposu, Type=MachineLoad)

B) Üretim emri başladı
   ProductionOrder.Start()
   → reçete açılır (recursive CTE)
   → her kalem için makine deposundan ÇIKIŞ (Type=ProductionConsumption)
        - SourceLotIds movement'a kaydedilir (geriye trace için)
        - yetersizse → emir BAŞLATILMAZ (Result.Fail), eksik liste
   → fire (oransal + miktarsal) ayrı Scrap movement olarak yazılır

C) Üretim çıkışı
   POST /api/v1/production/{id}/output { qty, lotNo }
   → mamul lotu yarat (Source=Production, SourceReferenceId=UE)
   → fiziksel mamul deposuna giriş (Type=ProductionOutput)
   → yeni FifoLayer (cost = tüketilen malzemelerin ağırlıklı maliyeti — faz 1)

D) Yük indirme
   POST /api/v1/machines/{id}/unload
   → makine deposu → fiziksel depo (Type=MachineUnload)
```

### 7.5 Dashboard

```
GET /api/v1/dashboard      ← tek istek, paralel sorgular Task.WhenAll

  CriticalStock:    products WHERE total_qty < min_stock ORDER BY oran ASC LIMIT 50
  WarehouseFill:    location bazlı capacity + filled agregasyon
  RecentMovements:  son 20 stok hareketi
  LotSearch:        boş — frontend search input, ayrı endpoint /api/v1/inventory/lots?q=

Cache: IMemoryCache, TTL 60sn, key="tenant:{id}:dashboard:{widget}"
Invalidate: GoodsReceived, Shipped, Adjusted event'lerinde
```

### 7.6 Yeni tenant oluşturma

```
POST /api/v1/admin/tenants  { code, name, plan, adminEmail, adminPassword }
        ↓ (super-admin yetkisi)
[Catalog DB tx]
  - tenants satırı + tenant_databases (encrypted_password)
  - user_lookup (adminEmail, hashedPassword, tenant_id)
COMMIT
        ↓
[CLI: wms-migrate create-tenant — async kullanıcı tarafından çalıştırılır]
  - PostgreSQL'de DB CREATE: wms_tenant_<id>
  - EF migrations apply
  - seed: default warehouse "ANA-DEPO", admin user (rol=TenantAdmin)
  - catalog: tenants.status = "Active"
```

### 7.7 Concurrency stratejisi

| Senaryo | Yaklaşım |
|---|---|
| Aynı ürüne aynı anda iki giriş | append-only log; UPSERT snapshot çakışmasız |
| Aynı stoktan iki paralel sevk | `FOR UPDATE SKIP LOCKED` FIFO katmanı |
| Snapshot UPSERT yarışı | PostgreSQL `ON CONFLICT DO UPDATE` atomik |
| Aynı belge çift submit | `Idempotency-Key` header zorunlu (uuid, 24sa cache) |
| Reçete versiyon tarih çakışması | DB EXCLUDE constraint (`tstzrange`) |

---

## 8. API Yüzeyi

### 8.1 Genel kurallar

- **REST + JSON**, sürümleme `/api/v1/...`
- **Authentication:** Bearer JWT (RS256, access 15dk + refresh 7gün, rotasyonlu)
- **Multi-tenant:** tenant otomatik JWT'den (URL'de tenant id YOK)
- **Pagination:** `?page=1&pageSize=50`
- **Filtreleme:** `?filter[status]=active&filter[product.code]=*VID*`
- **Sıralama:** `?sort=-occurredAt,code`
- **Idempotency-Key:** belge oluşturan POST'larda **zorunlu**, eksikse 400
- **Tarih formatı:** ISO 8601 UTC her yerde; DB `timestamptz`
- **HTTP statüleri:** 200/201/204, 400 (validation), 401/403, 404, 409 (conflict), 422 (business), 5xx
- **Response envelope (basit):**

```json
// başarı
{ "success": true, "data": { ... } }

// hata
{
  "success": false,
  "message": "Yetersiz stok",
  "errors": [
    { "field": "items[0].quantity", "code": "INSUFFICIENT_STOCK", "message": "..." }
  ],
  "correlationId": "..."
}
```

### 8.2 Endpoint kümeleri

#### Auth
```
POST   /api/v1/auth/login              { email, password } → { token, refreshToken, user }
POST   /api/v1/auth/refresh            { refreshToken }
POST   /api/v1/auth/logout
POST   /api/v1/auth/change-password
GET    /api/v1/auth/me
```

#### Super-admin (catalog scope)
```
GET    /api/v1/admin/tenants
POST   /api/v1/admin/tenants
PATCH  /api/v1/admin/tenants/{id}/status
GET    /api/v1/admin/tenants/{id}/health
POST   /api/v1/admin/tenants/{id}/rebuild-balances
```

#### Tenant yönetimi
```
GET    /api/v1/tenant/settings
PATCH  /api/v1/tenant/settings
GET/POST/PATCH /api/v1/tenant/users
GET/POST/PATCH /api/v1/tenant/roles
```

#### Catalog
```
GET/POST/PATCH/DELETE /api/v1/products
GET                   /api/v1/products/{id}
GET/POST/PATCH        /api/v1/products/{id}/units
GET/POST/PATCH        /api/v1/categories
GET/POST              /api/v1/units
```

#### Warehousing
```
GET/POST/PATCH/DELETE /api/v1/warehouses
GET                   /api/v1/warehouses/{id}
GET/POST/PATCH        /api/v1/warehouses/{id}/locations
GET/POST/PATCH        /api/v1/machine-warehouses
```

#### Inventory (sistemin kalbi)
```
POST   /api/v1/inventory/goods-receipts            [Idempotency-Key]
POST   /api/v1/inventory/shipments                 [Idempotency-Key]
POST   /api/v1/inventory/transfers                 [Idempotency-Key]
POST   /api/v1/inventory/adjustments               [Idempotency-Key]
POST   /api/v1/inventory/movements/{id}/reverse    (Seviye 1)
POST   /api/v1/inventory/movements/{id}/void       (Seviye 3 — yetki: Admin)

GET    /api/v1/inventory/movements                 (filtrelenebilir liste)
GET    /api/v1/inventory/movements/{id}
GET    /api/v1/inventory/balances                  (anlık bakiye)
GET    /api/v1/inventory/lots                      (lot arama)
GET    /api/v1/inventory/lots/{lotNo}/trace        (geriye doğru izlenebilirlik)
```

#### Recipes
```
GET/POST/PATCH/DELETE /api/v1/recipes
GET                   /api/v1/recipes/{id}/versions
POST                  /api/v1/recipes/{id}/versions
PATCH                 /api/v1/recipes/{id}/versions/{vid}
POST                  /api/v1/recipes/{id}/versions/{vid}/activate
POST                  /api/v1/recipes/{id}/versions/{vid}/explode
GET/POST/PATCH        /api/v1/recipes/{id}/versions/{vid}/items
GET/POST              /api/v1/recipes/items/{itemId}/alternatives
```

#### Production (skeleton)
```
GET/POST/PATCH        /api/v1/production-orders
POST                  /api/v1/production-orders/{id}/start
POST                  /api/v1/production-orders/{id}/output
POST                  /api/v1/production-orders/{id}/complete
POST                  /api/v1/production-orders/{id}/cancel
```

#### Machine ops
```
POST   /api/v1/machines/{warehouseId}/load
POST   /api/v1/machines/{warehouseId}/unload
GET    /api/v1/machines/{warehouseId}/stock
```

#### Dashboard
```
GET    /api/v1/dashboard
GET    /api/v1/dashboard/critical-stock
GET    /api/v1/dashboard/warehouse-fill
GET    /api/v1/dashboard/recent-movements
GET    /api/v1/dashboard/lot-search?q=...
```

### 8.3 Yetkilendirme

JWT içinde `perms[]` claim listesi (örn: `inventory.write`, `recipe.write`, `admin.tenant.create`).
Endpoint üzerinde `[Authorize(Policy = "inventory.write")]`.
Permission seti rol tanımına bağlı (faz 1'de sabit). Tenant admin kullanıcıları rollere atar; özel rol oluşturma faz 2.

### 8.4 Standart roller (faz 1)

| Rol | İzinler |
|---|---|
| TenantAdmin | tüm tenant izinleri (kullanıcı, rol, ayar, tüm modüller) |
| WarehouseManager | inventory.*, warehouses.*, products.read, recipes.read |
| Operator | machines.load/unload, production-orders.start/output, balances.read |
| Viewer | tüm read izinleri, write yok |
| SuperAdmin | catalog DB üzerinde, sadece super_admins tablosundan |

---

## 9. Hata Yönetimi ve Güvenlik

### 9.1 Hata sınıflandırması

| Tip | Örnek | Yanıt | Loglama |
|---|---|---|---|
| **Validation** (input formatı) | "quantity negatif" | 400 + `errors[]` | Information |
| **Business rule** | "yetersiz stok" | 422 + `errors[]` | Information |
| **Infrastructure** | DB down, deadlock, null ref | 500 + correlation ID | Error + alarm |

`ExceptionHandlingMiddleware` + `ResultEnvelopeFilter` ile merkezi dönüşüm.

### 9.2 Validation iki seviye

1. **Application** — FluentValidation (Command/Query başına `IValidator<T>`): format, zorunluluk, regex, çapraz alan
2. **Domain** — invariant entity metodları içinde; ihlal → `DomainException` → MediatR pipeline `Result.Fail`'e çevirir

### 9.3 Güvenlik

| Konu | Karar |
|---|---|
| Auth | JWT RS256 (asimetrik), access 15dk + refresh 7gün, refresh DB'de rotasyonlu |
| 2FA | TOTP, opsiyonel (kullanıcı kendi açar); kritik işlemlerde (rebuild, void) zorunlu |
| Parola | Argon2id (Konscious.Security.Cryptography.Argon2) |
| Rate limit | Anonim 10 r/s, auth 100 r/s, /auth/login IP+email başına 5/dk |
| CORS | sadece SaaS domain whitelist |
| Headers | HSTS, CSP, X-Content-Type-Options, X-Frame-Options=DENY |
| SQL injection | EF parametrized; raw SQL sadece admin işlerde, parametreli |
| Hassas log | Parola/JWT/MFA loglanmaz (Serilog masking) |
| Audit | Her yazma işlemi `audit_tenant`'a (kim, ne, ne zaman — JSON snapshot YOK) |
| Tenant izolasyonu | DB-per-tenant + JWT.tenantId; JWT'siz endpoint hiçbir tenant DB'sine dokunamaz |
| Backup | pg_dump günlük, tenant başına ayrı dosya, 30 gün retention; PITR (WAL) faz 2 |
| Şifreli alan | Catalog `tenant_databases.password_enc` AES-GCM (master key env'de) |

### 9.4 Loglama

- **Serilog** + structured logging → file (rolling daily); Seq/Loki faz 2
- **Correlation ID** her request'e middleware tarafından eklenir, response header + tüm log satırlarına yazılır
- **Health check:** `/health` (catalog DB ping), `/health/ready` (tenant DB sample ping)

---

## 10. Test Stratejisi

### 10.1 Test piramidi

```
              ┌─────────────────┐
              │  E2E (faz 2)    │  ← Cypress/Playwright
              ├─────────────────┤
         ┌───┤ Integration tests├───┐
         │   │ shared test PG    │  ← DB-touching handlers, gerçek PostgreSQL
         │   │                   │
       ┌─┴───┴─────────────────┴─┴─┐
       │     Unit tests             │  ← Domain invariants, FIFO engine, validators
       │     (xUnit + FluentAssertions)│
       └────────────────────────────┘
```

### 10.2 Birim testler (Domain ağırlıklı)

- Domain entity invariant'ları her kural bir test
- **FIFO Engine** saf in-memory test (DB'siz): tek katman, çoklu katman, tam tüketim, kalan, alternatif lot override
- Validator'lar: her FluentValidation kuralı için happy + sad path

### 10.3 Entegrasyon testleri (kritik akışlar)

**Shared test PostgreSQL** (CI'da kalıcı bir test DB host'u). Her test ayrı tenant DB yaratır, migration uygular, senaryoyu çalıştırır, sonra temizler. Mock yok — gerçek DB.

**Kapsam:**
- Mal kabul → bakiye doğrulama
- Sevk → FIFO sırası, snapshot azalma, hareket loglandı
- Concurrent sevk (paralel 2 task) → toplam tüketim doğru, deadlock yok
- Transfer → ReceiptDate korunma
- Üretim emri → reçete patlatma → sarf doğru
- Reverse / Void / Rebuild → idempotent ve doğru sonuç
- Tenant izolasyonu → tenant A'nın JWT'siyle tenant B'ye erişim yok
- Rebuild → 1000 hareketten snapshot doğru üretiliyor mu (golden output)

### 10.4 Test verisi

- Builder pattern: `ProductBuilder.WithLotRequired().Build()`
- Database seed helpers: temiz tenant + 5 ürün + 2 depo

### 10.5 CI

1. `dotnet build`
2. `dotnet test --filter Category=Unit` (hızlı, paralel)
3. `dotnet test --filter Category=Integration` (shared test PostgreSQL'a bağlanır)
4. SonarCloud / quality gate (faz 2)

---

## 11. Klasör Yapısı (Repo)

```
depo-yonetim/
├── README.md
├── .gitignore
├── .editorconfig
├── docs/
│   ├── superpowers/specs/
│   │   └── 2026-05-06-depo-yonetim-design.md     ← bu spec
│   ├── architecture/
│   │   ├── 01-overview.md
│   │   ├── 02-multi-tenant.md
│   │   ├── 03-domain-model.md
│   │   ├── 04-data-flows.md
│   │   ├── 05-api-surface.md
│   │   ├── 06-error-security.md
│   │   └── 07-testing.md
│   ├── domain/
│   │   ├── glossary.md
│   │   └── invariants.md
│   └── operations/
│       ├── tenant-lifecycle.md
│       ├── migration-runbook.md
│       └── rebuild-balances.md
│
├── src/
│   ├── backend/
│   │   ├── WMS.sln
│   │   ├── Directory.Build.props
│   │   ├── Directory.Packages.props
│   │   ├── WMS.Api/
│   │   │   ├── Endpoints/{Auth,Inventory,Warehousing,Recipes,Production,Dashboard,Admin}/
│   │   │   ├── Middleware/
│   │   │   ├── Auth/
│   │   │   └── Filters/
│   │   ├── WMS.Application/
│   │   │   ├── Common/{Behaviors,Result,Interfaces}/
│   │   │   ├── Identity/
│   │   │   ├── Catalog/
│   │   │   ├── Warehousing/
│   │   │   ├── Inventory/{GoodsReceipts,Shipments,Transfers,Adjustments,Reversals,Voids,Queries}/
│   │   │   ├── Recipes/
│   │   │   ├── Production/
│   │   │   ├── Dashboard/
│   │   │   └── Admin/
│   │   ├── WMS.Domain/
│   │   │   ├── Common/
│   │   │   ├── Tenancy/
│   │   │   ├── Identity/
│   │   │   ├── Catalog/
│   │   │   ├── Warehousing/
│   │   │   ├── Inventory/{StockMovement,StockBalance,FifoLayer,Lot,Services/FifoEngine}
│   │   │   ├── Recipes/
│   │   │   ├── Production/
│   │   │   └── Exceptions/
│   │   ├── WMS.Infrastructure/
│   │   │   ├── Persistence/{AppDbContext,DbContextFactory,Configurations,Migrations,Repositories}/
│   │   │   └── Services/{TenantConnectionResolver,PasswordHasher,JwtTokenService,EncryptionService}/
│   │   ├── WMS.Infrastructure.Catalog/
│   │   ├── WMS.Shared/
│   │   ├── WMS.Migrate/
│   │   └── tests/
│   │       ├── WMS.Domain.UnitTests/
│   │       ├── WMS.Application.UnitTests/
│   │       └── WMS.Api.IntegrationTests/
│   │           ├── Fixtures/{SharedDatabaseFixture,TenantFactory}
│   │           └── Scenarios/
│   │
│   └── frontend/
│       ├── package.json
│       ├── vite.config.ts
│       ├── src/
│       │   ├── main.ts
│       │   ├── App.vue
│       │   ├── router/
│       │   ├── stores/                (Pinia)
│       │   ├── api/                   (typed client)
│       │   ├── views/{auth,dashboard,inventory,warehouses,products,recipes,admin}/
│       │   ├── components/{common,inventory,dashboard}/
│       │   ├── composables/
│       │   ├── types/
│       │   ├── utils/
│       │   └── assets/
│       └── public/
│
├── deploy/
│   ├── docker/
│   │   ├── api.Dockerfile
│   │   ├── frontend.Dockerfile
│   │   └── docker-compose.dev.yml
│   ├── k8s/                            (faz 2)
│   └── scripts/
│       ├── new-tenant.ps1
│       └── rebuild-balances.ps1
│
├── .github/workflows/
│   ├── backend-ci.yml
│   ├── frontend-ci.yml
│   └── deploy.yml
│
└── tools/seed-data/
```

---

## 12. Varsayımlar ve Açık Noktalar

### 12.1 Varsayımlar (kullanıcı detay istemediği için kabul edilen)

1. **Üretim emri makineye bağlıdır** — UE açılırken bir makine seçilir
2. **Otomatik tüketim makine deposundan yapılır** (UE başlatıldığında reçete makine deposundaki stoktan tüketir)
3. **Üretim çıkış maliyeti = tüketilen malzemelerin ağırlıklı toplamı** (faz 1; faz 2'de işçilik/makine saat ücreti)
4. **Üretim çıkış lotu manuel girilir** (kullanıcı `lotNo` belirtir; auto-üretim faz 2)
5. **Standart roller:** TenantAdmin, WarehouseManager, Operator, Viewer + SuperAdmin (faz 1'de sabit, custom rol/permission **faz 2**)
6. **Kimlik doğrulama:** JWT + opsiyonel TOTP 2FA (SSO faz 2)
7. **Bildirim sistemi yok faz 1** (e-posta/SMS faz 2)
8. **Public API gateway yok faz 1** (3. parti entegrasyonu faz 2)
9. **Üretim fire'ı ayrı `Scrap` movement** (raporlanabilirlik için)
10. **Reçete operasyonları ve rota faz 2**

### 12.2 Açık noktalar (uygulama sırasında netleştirilecek)

- Üretim emri kısmi teslim/kısmi çıkış senaryosu (faz 2 detayda)
- Üretim çıkışında çoklu mamul (tek UE → birden fazla mamul + yan ürün) faz 2
- Lot quality status değişim akışı (Quarantine ↔ OK transition'ı kim yapar) faz 2
- Stok rezervasyonu (sevk hazırlığı için tutma) faz 2
- Tenant başına özel kod alan/etiket sistemi faz 2

---

## 13. Faz 1 Çıktı Listesi

Bu spec implementasyon planına dönüştürüldüğünde, faz 1 sonunda şunların çalışıyor olması beklenir:

- [ ] Catalog DB + en az 2 örnek tenant DB
- [ ] `WMS.Migrate` CLI tool (apply, create-tenant, rebuild-balances, status)
- [ ] Auth (login, refresh, logout, me, 2FA opsiyonel)
- [ ] Super-admin tenant yönetimi
- [ ] Ürün, kategori, birim CRUD
- [ ] Depo (fiziksel + tree konum, sanal, makine) CRUD
- [ ] Mal kabul, sevk (FIFO), transfer, sayım/fire/iade hareket akışları
- [ ] Reçete CRUD, çok seviyeli, versiyonlu, alternatif, fire
- [ ] Üretim emri skeleton + makine yükle/indir + üretim sarf+çıkış
- [ ] 4 seviyeli stok hatası düzeltme (Reverse, Adjustment, Void, Rebuild)
- [ ] Dashboard 4 zorunlu widget (kritik stok, depo doluluk, son hareketler, lot arama)
- [ ] Lot izlenebilirlik geriye doğru (`/lots/{lotNo}/trace`)
- [ ] Test: unit (Domain) + integration (shared test PostgreSQL)
- [ ] Vue.js SPA: auth, dashboard, inventory hareketleri, ürün/depo/reçete CRUD ekranları
- [ ] CI/CD pipeline (build + test)
- [ ] docker-compose.dev.yml ile lokal geliştirme
