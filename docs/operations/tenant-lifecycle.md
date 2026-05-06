# Tenant Yaşam Döngüsü

> Tenant oluşturma, suspend, delete, restore adımları burada belgelenir.

## Yeni Tenant Açma

### Admin üzerinden (API)

```bash
POST /api/v1/admin/tenants
{
  "code": "tenant-a",
  "name": "Tenant A Ltd.",
  "dbHost": "db.example.com",
  "dbPort": 5432,
  "dbUsername": "tenant_a_user",
  "passwordEnc": "aes_gcm_encrypted_password",
  "databaseName": "tenant_a_wms"
}
```

**Response:** 201 Created — tenant ID döner

### Adımlar

1. **Catalog DB kaydı:** `TenantDatabases` + `Tenants` tablosuna kaydedilir
2. **Tenant DB provision:** PostgreSQL'de boş database oluşturulur
3. **Migration apply:** `WMS.Migrate apply --tenant-id {guid}`
4. **Seed data:** Varsayılan kategori, birimler, admin kullanıcı
5. **Admin oluşturma:** Tenant yöneticisi kullanıcı oluşturulur
6. **Response:** Tenant artık JWT ile erişilebilir

### CLI ile tenant oluşturma

```bash
cd src/backend/WMS.Migrate
dotnet run create-tenant --code "tenant-a" --name "Tenant A" \
  --db-host "localhost" --db-port 5432 --db-name "tenant_a_wms" \
  --db-username "postgres" --db-password "postgres" \
  --password-enc "<aes_gcm_encrypted>"
```

## Tenant Suspend (Devre Dışı)

```bash
PATCH /api/v1/admin/tenants/{id}/status
{ "status": "Suspended" }
```

**Sonuç:**
- Tenant JWT'si geçersiz olur (middleware 402 döner)
- Mevcut hareketler korunur
- Yeni hareket girilemez

## Tenant Restore

```bash
PATCH /api/v1/admin/tenants/{id}/status
{ "status": "Active" }
```

**Sonuç:**
- Tenant JWT'si tekrar geçerli
- Tüm hareketler erişilebilir

## Tenant Silme (Delete)

⚠️ **Kalıcı işlem** — geri alma yok

```bash
# Soft delete — veri silinmez, sadece status değişir
DELETE /api/v1/admin/tenants/{id}
```

**Adımlar:**
1. Tenant status → `Deleted`
2. Tüm tenant kullanıcıları pasife alınır
3. Tenant DB'si backup alınır
4. (Opsiyonel) Tenant DB drop edilir
5. Catalog DB'de `TenantDatabases` record'ı silinir

## Migration Check

Herhangi bir operasyon öncesi:

```bash
# Tenant DB durumu kontrol et
dotnet run check --tenant-id {guid}

# Tüm tenant'ları listele
dotnet run list-tenants

# Tenant health check
curl https://api.example.com/api/v1/admin/tenants/{id}/health
```

## Disaster Recovery

- Her tenant DB'si günlük backup
- Catalog DB backup: tüm tenant metadata + şifreli connstr'lar
- Restore sırası: Catalog → Tenant DB'leri → Migration apply
- Toplam RTO: ~30 dakika (tüm tenant'lar)
