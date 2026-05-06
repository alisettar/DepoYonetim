# Yeni Konuşma için Hazır Prompt

> Tüm Faz 1 implementasyonları tamamlandı. Yeni bir Claude Code oturumu için devam edilecek işler.

## Mevcut Durum

Tüm 5 milestone tamamlandı:
- **M1** ✅ — Altyapı + Catalog DB + Auth + WMS.Migrate CLI
- **M2** ✅ — Catalog modülü (ürün/kategori/birim) + Depo iskeleti
- **M3** ✅ — Stok hareket motoru (FIFO + snapshot + tüm hareket türleri + 4 seviyeli düzeltme)
- **M4** ✅ — Reçete + Üretim skeleton + lot izlenebilirlik
- **M5** ✅ — Vue.js SPA + Dashboard + CI/CD

## Kalan İşler

| Öncelik | İş | Özet |
|---------|-----|------|
| HIGH | Void endpoint | Hareket iptali (Reversal değil, flag-based) |
| HIGH | Rebuild-balances CLI | Stok bakiyesi sıfırlama aracı |
| HIGH | Unit/Integration testleri | Test kate henüz yok |
| MEDIUM | Production Order skeleton | Sipariş yönetimi iskeleti |
| MEDIUM | CI/CD pipeline | GitHub Actions / GitLab CI |
| LOW | UI sayfaları | Recipe editor, lot trace UI, dashboard detay |
| LOW | Deployment | Docker compose, staging/production config |

## Yeni Konuşma Prompt'u

```
depo-yonetim projesinde kaldığım yerden devam ediyorum. Tüm Faz 1 milestone'ları tamamlandı.

Mevcut durum:
- Tüm backend modüller çalışıyor (Auth, Warehouse, Unit, Category, Product, Inventory/FIFO, Recipes, Dashboard)
- Vue.js frontend tamamlandı (Dashboard, Inventory, Warehouse yönetimi)
- Multi-tenant architecture: Catalog DB + per-tenant DB'ler, JWT RS256
- Dokümanlar güncellendi (README.md, docs/architecture/*, docs/operations/*)

Kalan işler (yüksekten düşüğe):
1. Void endpoint — hareket iptali
2. Rebuild-balances CLI — stok sıfırlama
3. Unit/Integration testleri
4. Production Order skeleton
5. CI/CD pipeline
6. Deployment

Şimdi [Void endpoint / Rebuild CLI / Test / CI / Deployment] üzerinde çalışmak istiyorum.

İlk olarak ilgili endpoint'leri/handler'ları kodla, ardından testlerini yaz.
Türkçe iletişim.
```

## Kod Dosyaları

| Dosya | Amaç |
|-------|------|
| `src/backend/WMS.Api/Endpoints/InventoryEndpoints.cs` | Hareket endpoint'leri |
| `src/backend/WMS.Application/Inventory/` | CQRS handler'lar |
| `src/backend/WMS.Domain/Inventory/Services/FifoEngine.cs` | FIFO motoru |
| `src/backend/WMS.Migrate/` | CLI tool |
| `src/backend/WMS.Api/Middleware/TenantResolutionMiddleware.cs` | Tenant resolution |
| `src/frontend/src/views/` | Vue sayfaları |
| `docs/architecture/` | Mimari dokümanlar |
| `docs/operations/` | Operasyon kılavuzları |
