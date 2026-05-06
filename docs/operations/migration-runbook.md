# Migration Runbook

> `WMS.Migrate` CLI tool — tenant database provisioning ve migration management.

## Kurulum

```bash
cd src/backend/WMS.Migrate
dotnet run -- --help
```

## Komutlar

### `wms-migrate apply` — Tüm tenant'larda migration apply et

```bash
dotnet run apply --all [--parallel N]
```

- Tüm tenant'ların DB'sinde migration'ları apply eder
- `--parallel N`: aynı anda N tenant'da çalıştır (varsayılan: 1)
- Her tenant'ın connection string'i Catalog DB'den okunur
- Hata: tenant'ı atla, devam et, summary rapor ver

### `wms-migrate create` — Yeni migration oluştur

```bash
dotnet run create --name AddInventoryAdjustments --project backend
```

- Migration klasörüne yeni dosya ekler (`001_AddInventoryAdjustments.cs`)
- `Up()` ve `Down()` method'ları otomatik oluşturulur

### `wms-migrate seed` — Tenant DB'sine seed data ekle

```bash
dotnet run seed --tenant-id {guid}
```

- Varsayılan kategoriler, birimler, örnek ürünler ekle
- Admin kullanıcı oluştur (tenant yöneticisi)

## Deploy Sonrası Adımlar

### Pre-flight Check

1. Catalog DB erişilebilir mi?
   ```bash
   pg_isready -h <catalog-host> -p 5432
   ```

2. Tenant connection string'leri valid mi?
   - Catalog DB'den `TenantDatabases` tablosunu sorgula
   - Her tenant için `PasswordEnc` çözümlenebilen formatta mı?

3. Migration script'leri uyumlu mu?
   - `__EFMigrationsHistory` tablosu — son migration ID
   - Yeni migration'lar eksik mi?

### Execution

```bash
# Production deploy sonrası
cd src/backend/WMS.Migrate
dotnet run apply --all --parallel 2
```

### Hata Durumunda

1. **Migration timeout:** `--timeout 300` (5 dk) ile tekrar çalıştır
2. **Connection refused:** Catalog DB health check — ardından retry
3. **Data conflict:** Mevcut data ile migration uyumsuz — rollback düşün
4. **Partial success:** Hangi tenant'lar başarılı, hangileri başarısız — summary'den kontrol et

### Migration Sonrası Doğrulama

```bash
# Her tenant'da tablo var mı?
dotnet run validate --tenant-id {guid} --tables Products,Movements,Balances,Lots,Recipes,FifoLayers

# Stok tutarlılık kontrolü
dotnet run validate --tenant-id {guid} --check-stock-consistency
```

## Manuel Tenant Migration

Tek bir tenant için:

```bash
dotnet run apply --tenant-id {guid} --force
```

- `--force`: migration'ı yeniden çalıştır (idempotent migration'lar için)
- Riskli: mevcut data üzerine yazabilir — dikkatli kullan

## Rollback

Migration'lar genellikle idempotent — ters çalıştırmaya gerek yok. Ancak:

```bash
# Specific migration'ı geri al (Down())
dotnet run rollback --tenant-id {guid} --migration-id {migration-guid}
```

## Monitoring

- Migration süresi per-tenant (log'lanır)
- Hata oranı per-tenant
- Toplam migration süresi
- Timeout olan tenant'lar
