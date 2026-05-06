# Migration Runbook

> Tasarım spec'inin [§5.5 ve §5.7](../superpowers/specs/2026-05-06-depo-yonetim-design.md#57-cli-tool--wmsmigrate) detay runbook'u.
>
> `wms-migrate apply --all --parallel 4` deploy sonrası elle çalıştırılır. Bu doküman:
> - Pre-flight check (DB sağlık, replica gecikmesi)
> - Paralel/sıralı çalıştırma kararları
> - Hata durumunda rollback adımları
> - Migration sonrası doğrulama scripti
>
> İlk implementasyondan sonra doldurulacak.
