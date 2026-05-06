# 03 — Domain Modeli

> Aggregate'ler, entity alanları, FIFO motoru, stok düzeltme stratejisi burada güncellenir.

## Aggregate Root'lar

### Product (Catalog)

`WMS.Domain/Catalog/Product.cs`

| Alan | Tip | Açıklama |
|------|-----|---------|
| `Id` | `Guid` | Primary key |
| `Code` | `string` | Ürün kodu (unique, max 50, upper case) |
| `Name` | `string` | Ürün adı (max 200) |
| `CategoryId` | `Guid` | Kategori referansı |
| `PrimaryUnitId` | `Guid` | Birincil birim |
| `LotRequired` | `bool` | Lot numarası zorunlu mu? |
| `ShelfLifeDays` | `int?` | Raf ömrü (gün) |
| `MinStock` | `decimal?` | Minimum stok threshold |
| `MaxStock` | `decimal?` | Maksimum stok threshold |
| `Status` | `ProductStatus` | `Active` / `Passive` |
| `Units` | `ProductUnit[]` | Ek birimler (conversion listesi) |

### WarehouseLocation

| Alan | Tip | Açıklama |
|------|-----|---------|
| `Id` | `Guid` | Primary key |
| `Code` | `string` | Konum kodu (örn: A-01-03) |
| `Name` | `string` | Konum adı |
| `WarehouseId` | `Guid` | Bağlı depo |
| `Capacity` | `decimal?` | Kapasite (null = sınırsız) |
| `IsActive` | `bool` | Aktif/deaktif |

### StockMovement

| Alan | Tip | Açıklama |
|------|-----|---------|
| `Id` | `Guid` | Primary key |
| `OccurredAt` | `DateTime` | Hareket zamanı (UTC) |
| `Type` | `MovementType` | 14 farklı tip |
| `ProductId` | `Guid` | Ürün referansı |
| `LotId` | `Guid?` | Lot referansı (nullable) |
| `WarehouseId` | `Guid?` | Depo referansı |
| `MachineWarehouseId` | `Guid?` | Makine depo referansı |
| `LocationId` | `Guid?` | Konum referansı |
| `Quantity` | `decimal` | Miktar |
| `UnitId` | `Guid` | Birim |
| `UnitCost` | `decimal?` | Birim maliyet |
| `Note` | `string?` | Not |
| `CreatedByUserId` | `Guid` | Oluşturan kullanıcı |

### Lot

| Alan | Tip | Açıklama |
|------|-----|---------|
| `Id` | `Guid` | Primary key |
| `ProductId` | `Guid` | Ürün referansı |
| `LotNumber` | `string` | Lot numarası |
| `ProductionDate` | `DateTime` | Üretim tarihi |
| `ExpiryDate` | `DateTime?` | Son kullanma tarihi |
| `Source` | `LotSource` | `Receipt` / `Production` |
| `QualityStatus` | `QualityStatus` | `OK` / `Quarantine` / `Rejected` |

### StockBalance

| Alan | Tip | Açıklama |
|------|-----|---------|
| `Id` | `Guid` | Primary key |
| `ProductId` | `Guid` | Ürün referansı |
| `LotId` | `Guid?` | Lot referansı |
| `WarehouseId` | `Guid?` | Depo referansı |
| `MachineWarehouseId` | `Guid?` | Makine depo referansı |
| `LocationId` | `Guid?` | Konum referansı |
| `Quantity` | `decimal` | Anlık bakiye |
| `UpdatedAt` | `DateTime` | Son güncelleme |

## Recipe / BOM

### Recipe (Aggregate root)

| Alan | Tip | Açıklama |
|------|-----|---------|
| `Id` | `Guid` | Primary key |
| `ProductId` | `Guid` | Ürün referansı |
| `Name` | `string` | Tarif adı |
| `Status` | `RecipeStatus` | `Draft` / `Active` / `Archived` |
| `Versions` | `RecipeVersion[]` | Versiyon listesi |

### RecipeVersion

| Alan | Tip | Açıklama |
|------|-----|---------|
| `Id` | `Guid` | Primary key |
| `RecipeId` | `Guid` | Tarif referansı |
| `VersionNo` | `int` | Versiyon numarası (artan) |
| `ValidFrom` | `DateTime` | Geçerli başlangıç |
| `ValidUntil` | `DateTime?` | Geçerlilik bitişi |
| `IsActive` | `bool` | Aktif versiyon mu? |
| `OutputQuantity` | `decimal` | Üretim miktarı |
| `OutputUnitId` | `Guid` | Üretim birimi |
| `Items` | `RecipeItem[]` | Malzeme listesi |

### RecipeItem

| Alan | Tip | Açıklama |
|------|-----|---------|
| `Id` | `Guid` | Primary key |
| `ProductId` | `Guid` | Hammadde referansı |
| `Quantity` | `decimal` | Miktar |
| `UnitId` | `Guid` | Birim |
| `WastePercent` | `decimal?` | Fire yüzdesi |
| `WasteFixed` | `decimal?` | Sabit fire miktarı |
| `SortOrder` | `int` | Sıralama |
| `Alternatives` | `AlternativeMaterial[]` | Alternatif hammaddeler |

## FIFO Motoru

`WMS.Domain/Inventory/Services/FifoEngine.cs`

### FifoLayer

Her `StockMovement` için bir `FifoLayer` oluşturulur:

| Alan | Tip | Açıklama |
|------|-----|---------|
| `Id` | `Guid` | Primary key |
| `MovementId` | `Guid` | Hareket referansı |
| `ProductId` | `Guid` | Ürün |
| `LotId` | `Guid?` | Lot |
| `WarehouseId` | `Guid?` | Depo |
| `Quantity` | `decimal` | Başlangıç miktarı |
| `RemainingQuantity` | `decimal` | Kalan miktar (harcanan düşülür) |
| `UnitCost` | `decimal` | Birim maliyet |
| `ReceiptDate` | `DateTime` | Giriş tarihi |
| `IsConsumed` | `bool` | Tamamen tüketildi mi? |

### Consume Akışı

```
FifoEngine.Consume(layers, requestedQuantity)
→ FIFO sırası ile eski katmanlardan başla
→ Her katmandan: Math.Min(layer.RemainingQuantity, remaining)
→ remaining = remaining - take
→ remaining > 0 kalırsa: BusinessException("INSUFFICIENT_STOCK")
→ Geriye: IReadOnlyList<FifoConsumption> { layerId, lotId, quantity, unitCost, receiptDate }
```

### Stok Düzeltme (4 Seviye)

1. **Sayım (Inventory Adjustment):** Manuel gir — `InventoryAdjustmentIn` / `InventoryAdjustmentOut`
2. **Rebuild-balances CLI:** Tüm hareketlerden hesapla — `StockBalance` sıfırdan oluşturulur
3. **Reversal:** Yanlış hareketi tersine çevir — `Reversal` tipi ile
4. **Manual fix:** Admin panel'den doğrudan bakiye düzenleme (audit log ile)

## Domain Events

- `ProductCreated` — yeni ürün eklendi
- `ProductDeactivated` — ürün pasife alındı
- `StockMovementRecorded` — hareket kaydedildi
- `RecipeActivated` — tarif versiyonu aktif edildi
- `TenantCreated` — yeni tenant provision edildi
