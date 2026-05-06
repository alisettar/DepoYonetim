# Yeni Konuşma için Hazır Prompt

> Faz 1 implementasyonuna başlamak için yeni bir Claude Code oturumunda olduğu gibi gönderilebilir.

---

## Faz 1 Milestone Özeti

| # | Milestone | Çıktı | Süre tahmini |
|---|---|---|---|
| **M1** | **Altyapı + Catalog + Auth** | `WMS.sln` + tüm projeler, Catalog DB şeması ve EF migration, `WMS.Migrate` CLI (`apply`, `create-tenant`), `TenantConnectionResolver`, JWT auth (login/refresh/me), super-admin tenant CRUD endpoint'leri, ilk integration test (shared test PG bağlantısı) | ~1-2 hafta |
| **M2** | **Catalog modülü + Depo iskeleti** | Tenant DB şeması, ürün/kategori/birim CRUD, depo (fiziksel+tree konum, sanal, makine) CRUD, ProductUnit dönüşüm, tenant_settings | ~1 hafta |
| **M3** | **Stok hareket motoru (sistemin kalbi)** | StockMovement append-only, StockBalance snapshot, FifoEngine, mal kabul/sevk/transfer/sayım/yükle-indir akışları, idempotency, Reversal/Void, rebuild-balances CLI komutu, kapsamlı integration test | ~2-3 hafta |
| **M4** | **Reçete + Üretim skeleton** | Recipe/RecipeVersion/RecipeItem/Alternative CRUD, çok seviyeli explode (recursive CTE), ProductionOrder skeleton, üretim sarf+çıkış akışı, fire (Scrap movement), lot izlenebilirlik (geriye) | ~1-2 hafta |
| **M5** | **Vue.js SPA + Dashboard** | Vue 3 proje setup'ı, auth ekranları, dashboard 4 zorunlu widget, ürün/depo/reçete CRUD ekranları, hareket form'ları, docker-compose.dev.yml, CI/CD pipeline | ~2-3 hafta |

---

## Prompt (kopyala-yapıştır)

```
depo-yonetim projesinin Faz 1 implementasyonuna başlıyorum. Tasarım dokümanı
docs/superpowers/specs/2026-05-06-depo-yonetim-design.md dosyasında onaylı
ve tam halde mevcut. Klasör iskeleti ve doküman yapısı kuruldu; .gitkeep
dışında henüz hiçbir kod, csproj veya package.json yok.

Faz 1'i 5 milestone'a böldük (önceki oturumda kararlaştırıldı):
  M1 — Altyapı + Catalog DB + Auth + WMS.Migrate CLI
  M2 — Catalog modülü (ürün/kategori/birim) + Depo iskeleti
  M3 — Stok hareket motoru (FIFO + snapshot + tüm hareket türleri + 4 seviyeli düzeltme)
  M4 — Reçete + Üretim skeleton + lot izlenebilirlik
  M5 — Vue.js SPA + Dashboard + CI/CD

İlk olarak M1 için ayrıntılı implementation planını çıkarmak istiyorum.

Lütfen:
1) Spec dosyasını oku — özellikle §4 (mimari), §5 (multi-tenant), §6
   (domain modeli), §11 (klasör yapısı) ve §13 (Faz 1 çıktı listesi)
2) writing-plans skill'ini kullanarak M1 için adım adım plan çıkar
3) TDD prensibiyle ilerle — her adımda önce test, sonra implementation
4) Plan onayımdan sonra executing-plans skill'i ile uygulamaya geç
5) Bölüm bölüm onay alarak ilerle, dosya yazmadan önce sor

Türkçe iletişim. Önemli mimari kararlar için spec'e referans ver,
yeni karar gerekirse önce bana sor.
```

---

## Sonraki milestone'lara geçerken

M1 tamamlandığında, aynı şablonu M2 için uyarlayarak yeni bir konuşma başlatabilirsin. Sadece prompt'taki **"İlk olarak M1 için..."** satırını **"M2 için..."** olarak değiştir ve M1'in tamamlandığını belirt.

Örnek M2 başlangıç prompt'u:

```
depo-yonetim Faz 1 / M2'ye geçiyorum. M1 (altyapı + catalog + auth + migrate CLI)
tamamlandı ve testleri geçiyor. Şimdi M2 için planlama:

  M2 kapsamı: Tenant DB şeması, ürün/kategori/birim CRUD, depo
  (fiziksel+tree konum, sanal, makine) CRUD, ProductUnit dönüşüm, tenant_settings.

Spec referansı: docs/superpowers/specs/2026-05-06-depo-yonetim-design.md
özellikle §6.2 entity alanları ve §11 klasör yapısı.

writing-plans ile M2 detay planını çıkar. TDD prensibi, bölüm bölüm onay,
Türkçe.
```
