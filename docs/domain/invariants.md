# Domain Invariant'ları

Sistemin bozulmaması gereken kuralları. Test edilebilir, kod ile doğrulanır.

| # | Invariant | Nerede uygulanır |
|---|---|---|
| 1 | `LotRequired = true` ürünün lotsuz hareketi reddedilir | Application validator + Domain |
| 2 | `LotRequired = false` ürün hem lotlu hem lotsuz girilebilir | Snapshot iki ayrı satır kabul eder |
| 3 | Makine deposundan müşteriye sevk yasak | Movement type whitelist |
| 4 | Sanal depoya/-dan transfer fiziksel envanteri değiştirmez | Movement handler |
| 5 | Negatif stok varsayılan olarak yasak; `tenant_settings.allow_negative_stock` ile gevşetilebilir | Application |
| 6 | Çıkış hareketi her zaman FIFO katman seçimi ile (manuel lot override opsiyonel) | FifoEngine |
| 7 | RecipeVersion `ValidFrom..ValidUntil` aralıkları örtüşemez | DB EXCLUDE constraint (`tstzrange`) |
| 8 | RecipeItem ürünü kendi reçetesine dolaylı dönmemeli | Application cycle detection |
| 9 | `OccurredAt < now()` hareket girişi yasak | Application validator |
| 10 | StockMovement silme yasak; sadece Reversal veya Void | DB seviyesinde DELETE policy |
| 11 | Bir e-mail catalog'da global UNIQUE — bir tenant'a aittir | DB constraint |
| 12 | Tenant DB `users` tablosunda parola tutulmaz (auth tek noktadan: catalog `user_lookup`) | Şema |

> Her invariant için `WMS.Domain.UnitTests` veya `WMS.Api.IntegrationTests` altında en az bir test olacak.
