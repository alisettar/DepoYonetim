# Depo Yönetim Sistemi

Karışık imalat (discrete + process) sektörü için multi-tenant SaaS depo yönetim ve lot izlenebilirlik sistemi.

## Hızlı bakış

- **Stack:** .NET 10 Web API + Vue.js 3 (TypeScript) + PostgreSQL + Entity Framework Core
- **Mimari:** Modüler monolith, database-per-tenant multi-tenant SaaS
- **Durum:** Çekirdek tüm modüller tamamlandı

## Tamamlanmış modüller

| Modül | Durum | Özet |
|-------|-------|------|
| **Auth** | ✅ | JWT RS256, login, refresh token, tenant bazlı yetkilendirme |
| **Warehouse** | ✅ | Depo, konum yönetimi (Physical / Virtual / Machine tipleri) |
| **Unit / Category** | ✅ | Birim ve kategori yönetimi (CRUD) |
| **Product** | ✅ | Ürün tanımlama, çoklu birim desteği, lotRequired, shelfLife |
| **Inventory (FIFO)** | ✅ | StockMovements (immutable), FifoLayer'lar, StockBalances, lot izlenebilirlik |
| **Mal Kabul / Sevkiyat** | ✅ | GoodsReceipt / Shipment endpoint'leri, lot + quantity + cost |
| **Transfer** | ✅ | TransferOut + TransferIn ile depo-iler işlemler |
| **Recipes / BOM** | ✅ | Üretim tarifleri, versiyonlama, malzeme listesi, BOM explode |
| **Dashboard** | ✅ | Kritik stok, depo doluluk oranı, son hareketler, lot arama |
| **Lot Traceability** | ✅ | Lot detay, hareket zinciri, üretim-iade izleme |
| **Multi-tenant** | ✅ | Catalog DB + tenant DB'leri, şifreli connstr, cached resolver |

## Kalan işler

- **Void endpoint** — iptal hareketi oluşturma
- **Rebuild-balances CLI** — stok dengesizliklerini sıfırlama aracı
- **Unit/Integration testleri** — test kate henüz yok
- **CI/CD** — pipeline konfigürasyonu
- **Production Order** — sipariş yönetimi iskeleti
- **UI sayfaları** — recipe editor, lot trace UI, dashboard widget detayları

## Dokümantasyon

- [Tasarım Spec'i (2026-05-06)](docs/superpowers/specs/2026-05-06-depo-yonetim-design.md)
- [Mimari dokümanlar](docs/architecture/)
- [Domain sözlüğü](docs/domain/glossary.md)
- [Operasyon kılavuzları](docs/operations/)

## Klasör yapısı

```
docs/        → tasarım, mimari, domain, operasyon dokümanları
src/backend/ → .NET 10 solution (WMS.Api, WMS.Application, WMS.Domain, WMS.Infrastructure, WMS.Infrastructure.Catalog, WMS.Shared.Common, WMS.Migrate, tests/)
src/frontend/→ Vue.js 3 + TypeScript SPA
docs/        → mimari, domain, operasyon dokümanları
deploy/      → Docker, K8s, scripts
tools/       → seed data ve yardımcı araçlar
```

Detaylar için [tasarım dokümanına](docs/superpowers/specs/2026-05-06-depo-yonetim-design.md) bakınız.
