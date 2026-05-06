# 04 — Veri Akışları

> Mal kabul, sevk (FIFO), transfer, üretim, dashboard, tenant oluşturma akışlarının detayları burada güncellenir.

## Mal Kabul (Goods Receipt)

```
POST /api/v1/inventory/goods-receipts
{
  "items": [
    { "productId": "...", "lotNumber": "L-001", "quantity": 100, "unitId": "...", "warehouseId": "...", "unitCost": 10.50 }
  ]
}
```

### İşlem akışı

1. **Validasyon:** Ürün aktif mi? Depo var mı? Miktar > 0?
2. **Lot oluşturma:** `lotNumber` varsa ve ürün `lotRequired` ise → `Lot` aggregate oluştur
3. **FIFO layer ekle:** `FifoLayer.Create(movementId, productId, lotId, warehouseId, quantity, unitCost)` — immutable
4. **StockMovement kaydet:** `OccurredAt = now`, `Type = GoodsReceipt`
5. **Stok bakiyesi:** `StockBalance` güncellenir (productId + lotId + warehouseId → quantity += input)
6. **Transaction commit:** Tüm operasyonlar tek DB transaction'ında

### Etkilenen tablo'lar

| Tablo | Eylem | Açıklama |
|-------|-------|---------|
| `StockMovements` | INSERT | Immutable kayıt |
| `FifoLayers` | INSERT | Yeni stok katmanı |
| `StockBalances` | UPSERT | Anlık bakiye güncellenir |
| `Lots` | INSERT | Lot numarası ilk kezse |

## Sevkiyat (Shipment)

```
POST /api/v1/inventory/shipments
{
  "items": [
    { "productId": "...", "quantity": 50, "unitId": "...", "warehouseId": "..." }
  ]
}
```

### İşlem akışı

1. **Validasyon:** Ürün aktif mi? Miktar > 0?
2. **FIFO consumption:** `FifoEngine.Consume(layers, quantity)` — FIFO sırası ile eski stoktan düşer
3. **Stok kontrolü:** `FifoEngine` yetersiz stokta `BusinessException` fırlatır — transaction rollback
4. **StockMovement kaydet:** `Type = Shipment`
5. **FIFO layer güncelle:** Her consumed layer'ın `RemainingQuantity` azalır. quantity = 0 olan layer'lar silinir
6. **Stok bakiyesi:** `StockBalance` güncellenir (quantity -= output)

### Concurrency

- FIFO consume + update tek transaction içinde
- EF Core `RowVersion` ile optimistic concurrency (eşzamanlı değişikliklerde conflict)

## Transfer (TransferOut → TransferIn)

```
POST /api/v1/inventory/transfers
{
  "items": [
    { "productId": "...", "quantity": 20, "unitId": "...", 
      "sourceWarehouseId": "...", "targetWarehouseId": "..." }
  ]
}
```

### İşlem akışı

1. **TransferOut** (anlık):
   - FIFO consumption (şevkiyat gibi)
   - `StockMovement` type: `TransferOut`
   - Stok bakiyesi azalır (source warehouse)

2. **TransferIn** (anlık):
   - FIFO layer ekle (yeni bir katman)
   - `StockMovement` type: `TransferIn`
   - Stok bakiyesi artar (target warehouse)

3. Her iki hareket tek transaction'da **veya** bağımsız transaction'lar (sistem tasarımına göre).

## Üretim (Recipe Consumption + Output)

```
POST /api/v1/inventory/production-consumption
POST /api/v1/inventory/production-output
```

### İşlem akışı

1. **Tarif seçimi:** `Recipe` + `RecipeVersion` (aktif)
2. **BOM explode:** `explodeRecipeBom(recipeId, versionId)` — tüm malzemelerin miktarlarını hesapla
3. **Hammadde consumption:** `FifoEngine.Consume()` ile FIFO'dan hammadde düş
4. **Malzeme hareketi:** `Type = ProductionConsumption`
5. **Yarı mamul/son üre** output:** `StockMovement` type: `ProductionOutput`
6. **Lot oluşturma:** Yarı mamul için yeni `Lot` oluştur (üretim tarihi, son kullanma = productionDate + shelfLifeDays)

## Dashboard — Kritik Stok

```
GET /api/v1/dashboard/critical-stock
```

### Akış

1. `Products` tablosundan `Status = Active AND MinStock IS NOT NULL` filtresi
2. `StockBalances` ile her ürün için toplam stok (grup: productId)
3. `Units` ile birim kodu
4. `currentQty < minStock` olanlar filtrelenir
5. `fillRatio = currentQty / minStock` ile sıralama (en düşük doluluk önce)

## Tenant Oluşturma

```
POST /api/v1/admin/tenants
{
  "code": "tenant-a",
  "name": "Tenant A Ltd.",
  "dbHost": "db.example.com",
  "dbPort": 5432,
  "dbUsername": "tenant_a_user",
  "passwordEnc": "aes_gcm_encrypted",
  "databaseName": "tenant_a_wms"
}
```

### Akış

1. **Validasyon:** Code unique mi? Password valid mi?
2. **Catalog DB'ye yaz:** `TenantDatabases` record oluştur (şifreli password)
3. **Tenant DB oluştur:** `WMS.Migrate` CLI çalıştır — schema migration'ları apply et
4. **Seed data:** Varsayılan kategoriler, birimler, örnek ürünler ekle
5. **Response:** Tenant oluşturuldu — artık JWT ile erişilebilir

## Concurrency Model

- **Optimistic concurrency:** `RowVersion` (timestamp) her aggregate'de
- **Locking:** Database-level row-level locking (PostgreSQL MVCC)
- **FIFO consumption:** Tek transaction içinde oku → hesapla → yaz
- **Dashboard:** Read-only, snapshot query — concurrency concern yok