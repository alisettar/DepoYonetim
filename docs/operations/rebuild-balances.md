# Rebuild Balances Runbook

> Tasarım spec'inin [§6.4 — Seviye 4](../superpowers/specs/2026-05-06-depo-yonetim-design.md#64-stok-hatası-düzeltme-stratejisi-4-seviye) detay runbook'u.
>
> `wms-migrate rebuild-balances --tenant <id>` ne zaman, kim tarafından, hangi kontrolden sonra çalıştırılır:
> - Ön doğrulama: total movement quantity vs. total snapshot quantity karşılaştırması
> - Advisory lock alma süresi tahmini (canlı tenant'ı kısa süre kilitler)
> - Sonradan doğrulama: rebuild öncesi/sonrası agregasyon raporu
> - audit_tenant kaydı
>
> İlk implementasyondan sonra doldurulacak.
