# Rebuild Balances Runbook

> Seviye 4 — Stok hatası düzeltme stratejisi. `StockBalance` tablosu `StockMovement` + `FifoLayer`'lardan yeniden hesaplanır.

## Ne Zaman Çalıştırılır

- Stok bakiyeleri hareketlerle tutarsız olduğunda
- Migration sonrası bakiye sıfırlama gerektiğinde
- Manuel sayım sonrası sistem verisini sıfırlamak için
- Production'da "stok eksi" durumu tespit edildiğinde

## Nasıl Çalıştırılır

```bash
# Tüm tenant'lar için rebuild
dotnet run rebuild-balances --all

# Tek bir tenant için
dotnet run rebuild-balances --tenant-id {guid}

# Specific product için
dotnet run rebuild-balances --tenant-id {guid} --product-id {guid}
```

## Adımlar

### 1. Ön Doğrulama

```bash
# Movement quantity vs. snapshot quantity karşılaştırması
dotnet run validate --tenant-id {guid} --check-stock-consistency

# Output örneği:
# Total movements: 15,234
# Total balances: 15,230
# Mismatch count: 4
# Affected products: SKU-001, SKU-003, SKU-007, SKU-012
```

### 2. Advisory Lock

- PostgreSQL advisory lock ile rebuild process tek seferde bir tenant'da çalışır
- Canlı tenant'ı kısa süre kilitler (~5-30 dakika, tenant büyüklüğüne göre)
- Aynı anda başka rebuild başlamaz
- `dotnet run rebuild-balances --tenant-id {guid} --force` ile lock bypass (riskli)

### 3. Rebuild

```bash
dotnet run rebuild-balances --tenant-id {guid}
```

**İşlem:**
1. Tüm `StockMovements` sırayla okunur (ORDER BY `OccurredAt`)
2. Her hareket için:
   - GoodsReceipt → `quantity += input`
   - Shipment → `quantity -= consumed` (FIFO)
   - TransferIn/Out → `warehouseId` değiştirm
   - Adjustment → doğrudan `quantity +=/- value`
3. `StockBalance` tablosu tamamen silinir ve yeniden oluşturulur
4. `FifoLayer` kayıtları ile doğrulama

### 4. Sonradan Doğrulama

```bash
# Rebuild sonrası tutarlılık raporu
dotnet run validate --tenant-id {guid} --check-stock-consistency

# Output örneği:
# Total movements: 15,234
# Total balances: 15,234
# Mismatch count: 0
# Status: PASS
```

### 5. Audit Kaydı

- Rebuild işlemi `audit_tenant` tablosuna kaydedilir
- Kim çalıştırdı (timestamp)
- Ne kadar süre sürdü
- Kaç product rebuild edildi
- Önceki/sonraki karşılaştırma

## Güvenlik Kontrolleri

- **Read-only mod:** Rebuild sırasında yeni hareket yazılamaz (tablo kilidi)
- **Optimistic concurrency:** Rebuild sırasında gelen hareketleri geri al
- **Rollback:** Rebuild tamamlanmadan kesilirse, transaction rollback

## Monitoring

- Rebuild süresi per-tenant (log'lanır)
- Hata oranı per-tenant
- Concurrent rebuild sayısı (max 1 per tenant)
