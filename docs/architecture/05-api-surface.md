# 05 — API Yüzeyi

> Gerçek endpoint'ler `WMS.Api/Endpoints/` klasöründeki dosyalardan üretilir.
> OpenAPI/Swagger endpoint'leri çalıştırmada otomatik üretilir.

## Tüm endpoint'ler

### Auth

| Yöntem | Endpoint | Açıklama |
|--------|----------|---------|
| `POST` | `/api/v1/auth/login` | Kullanıcı girişi — JWT döner |
| `POST` | `/api/v1/admin/auth/login` | Admin girişi |

### Warehouses

| Yöntem | Endpoint | Açıklama |
|--------|----------|---------|
| `GET` | `/api/v1/warehouses` | Tüm depo listesi (`?type=Physical` ile filter) |
| `GET` | `/api/v1/warehouses/{id}` | Tekil depo |
| `POST` | `/api/v1/warehouses` | Yeni depo (code, name, type: Physical/Virtual/Machine) |
| `PUT` | `/api/v1/warehouses/{id}` | Depo güncelleme (name, address) |
| `DELETE` | `/api/v1/warehouses/{id}` | Depo silme |

### Units

| Yöntem | Endpoint | Açıklama |
|--------|----------|---------|
| `GET` | `/api/v1/units` | Tüm birimler |
| `GET` | `/api/v1/units/{id}` | Tekil birim |
| `POST` | `/api/v1/units` | Yeni birim (code, name) |
| `PUT` | `/api/v1/units/{id}` | Birim güncelleme (name) |
| `DELETE` | `/api/v1/units/{id}` | Birim silme |

### Categories

| Yöntem | Endpoint | Açıklama |
|--------|----------|---------|
| `GET` | `/api/v1/categories` | Tüm kategoriler |
| `GET` | `/api/v1/categories/{id}` | Tekil kategori |
| `POST` | `/api/v1/categories` | Yeni kategori (code, name) |
| `PUT` | `/api/v1/categories/{id}` | Kategori güncelleme (name) |
| `DELETE` | `/api/v1/categories/{id}` | Kategori silme |

### Products

| Yöntem | Endpoint | Açıklama |
|--------|----------|---------|
| `GET` | `/api/v1/products` | Ürün listesi (`?status=Active`) |
| `GET` | `/api/v1/products/{id}` | Tekil ürün |
| `POST` | `/api/v1/products` | Yeni ürün (code, name, categoryId, primaryUnitId, ...) |
| `PUT` | `/api/v1/products/{id}` | Ürün güncelleme |
| `DELETE` | `/api/v1/products/{id}` | Ürün silme (soft) |
| `POST` | `/api/v1/products/{id}/units` | Üneke ek birim ekle (unitId, conversion) |
| `DELETE` | `/api/v1/products/{id}/units/{unitId}` | Üneke ek birim kaldır |

### Inventory

| Yöntem | Endpoint | Açıklama |
|--------|----------|---------|
| `POST` | `/api/v1/inventory/goods-receipts` | Mal kabul (multi-item, lot + quantity + cost) |
| `POST` | `/api/v1/inventory/shipments` | Sevkiyat (multi-item, FIFO'dan düşer) |
| `POST` | `/api/v1/inventory/transfers` | Transfer (depo → deponya) |
| `POST` | `/api/v1/inventory/adjustments` | Sayım düzeltmesi (artı/eksi) |
| `GET` | `/api/v1/inventory/movements` | Hareket listesi (`?type=GoodsReceipt&productId=...&warehouseId=...`) |
| `GET` | `/api/v1/inventory/balances` | Stok bakiyeleri (`?productId=...&warehouseId=...`) |
| `GET` | `/api/v1/inventory/lots` | Lot listesi (paginated, `?page=&pageSize=&qualityStatus=&lotNumberFilter=`) |
| `GET` | `/api/v1/inventory/lots/{lotId}/movements` | Lot detay — tüm hareketleri |
| `GET` | `/api/v1/inventory/lots/{lotId}/trace` | Lot izleme — üretim → tüketim zinciri |
| `PATCH` | `/api/v1/inventory/lots/{lotId}/quality-status` | Lot kalite durumu (OK/Quarantine/Rejected) |

### Recipes / BOM

| Yöntem | Endpoint | Açıklama |
|--------|----------|---------|
| `GET` | `/api/v1/recipes` | Tarif listesi (`?productId=...`) |
| `GET` | `/api/v1/recipes/{id}` | Tekil tarif (versiyonlar + items dahil) |
| `POST` | `/api/v1/recipes` | Yeni tarif (productId, name) |
| `PATCH` | `/api/v1/recipes/{id}` | Tarif güncelleme (name) |
| `DELETE` | `/api/v1/recipes/{id}` | Tarif arşivleme (soft delete) |
| `GET` | `/api/v1/recipes/{id}/versions` | Tarif versiyonları |
| `POST` | `/api/v1/recipes/{id}/versions` | Yeni versiyon (validFrom, validUntil, outputQuantity, outputUnitId) |
| `PATCH` | `/api/v1/recipes/{id}/versions/{vid}` | Versiyon düzenle (outputQuantity, outputUnitId) |
| `POST` | `/api/v1/recipes/{id}/versions/{vid}/activate` | Versiyonu aktif yap |
| `POST` | `/api/v1/recipes/{id}/versions/{vid}/explode` | BOM explode — tüm bileşenlerin maliyetini hesapla |
| `POST` | `/api/v1/recipes/{id}/versions/{vid}/items` | Versiyona malzeme ekle |
| `PATCH` | `/api/v1/recipes/{id}/versions/{vid}/items/{iid}` | Malzeme güncelle (quantity, unit, waste) |
| `DELETE` | `/api/v1/recipes/{id}/versions/{vid}/items/{iid}` | Malzeme sil |
| `POST` | `/api/v1/recipes/{id}/versions/{vid}/items/{iid}/alternatives` | Alternatif malzeme ekle |
| `DELETE` | `/api/v1/recipes/{id}/versions/{vid}/items/{iid}/alternatives/{aid}` | Alternatif sil |

### Dashboard

| Yöntem | Endpoint | Açıklama |
|--------|----------|---------|
| `GET` | `/api/v1/dashboard/critical-stock` | Kritik stok altındaki ürünler |
| `GET` | `/api/v1/dashboard/warehouse-fill` | Depo doluluk oranları |
| `GET` | `/api/v1/dashboard/recent-movements` | Son hareketler (`?count=20`) |
| `GET` | `/api/v1/dashboard/lot-search` | Lot numarası ile arama (`?q=LOT-001&productId=...`) |

### Admin (Tenant Yönetimi)

| Yöntem | Endpoint | Açıklama |
|--------|----------|---------|
| `POST` | `/api/v1/admin/tenants` | Yeni tenant oluştur |
| `GET` | `/api/v1/admin/tenants` | Tüm tenant'lar |
| `PATCH` | `/api/v1/admin/tenants/{id}/status` | Tenant durumu değiştir (aktif/pasif) |
| `GET` | `/api/v1/admin/tenants/{id}/health` | Tenant sağlık kontrolü |

### Health

| Yöntem | Endpoint | Açıklama |
|--------|----------|---------|
| `GET` | `/` | API bilinfo |
| `GET` | `/health` | Liveness probe |
| `GET` | `/health/ready` | Readiness probe |

## Notlar

- Tüm endpoint'ler `/api/v1` prefix'i ile başlar
- Swagger UI: `http://localhost:5000/swagger` (Development mode)
- Response envelope: doğrudan DTO veya DTO listesi — hata durumunda `ProblemDetails` veya `{ message, errorCode }` yapısı
- Tüm auth-required endpoint'ler Bearer token ile yetkilendirilir
- Bulk request'ler (goods-receipts, shipments) array payload kabul eder
