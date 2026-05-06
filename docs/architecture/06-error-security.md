# 06 — Hata Yönetimi ve Güvenlik

> Validation, security, JWT, parola hashing, audit politikaları burada güncellenir.

## Hata model — Result<T>

Tüm business logic `Result<T>` pattern ile çalışır:

```csharp
public class Result
{
    bool IsSuccess { get; }
    bool IsFailure => !IsSuccess;
    string? Message { get; }
    string? ErrorCode { get; }
    
    // Factory
    static Result Success()
    static Result Failure(string message)
    static Result Failure(string errorCode, string message)
    
    static Result<T> Success<T>(T value)
    static Result<T> Failure<T>(string message)
    static Result<T> Failure<T>(string errorCode, string message)
    static Result<T> From<T>(Func<T> factory)  // try-catch wrapper
}

public class Result<T> : Result
{
    T? Value { get; }
    void Match(Action<T> onSuccess, Action onError)
    T ValueOr(T fallback)
    Result<TOut> Map<TOut>(Func<T, TOut> mapper)
}
```

### BusinessException

Domain kurallarının kırılması `BusinessException` ile fırlatılır:

```csharp
throw new BusinessException("INSUFFICIENT_STOCK", "Yetersiz stok.");
throw new BusinessException("UNIT_IS_PRIMARY", "Birincil birim ek birim olarak eklenemez.");
```

- `ErrorCode`: mekanik hata kodu (test/monitoring için)
- `Message`: insa okunabilir mesaj

### Validation

- **Domain constructor validation:** Aggregate constructor'larında invariant kontrolü (null check, length, range, relationship)
- **DTO validation:** Endpoint layer'da request model validation (data annotations veya FluentValidation)
- **Stock validation:** FIFO engine — tala edilen miktar mevcut stoktan büyükse `BusinessException`

## JWT Authentication (RS256)

### Token oluşturma

`JwtTokenService` — `WMS.Shared.Common.Cryptography`

- **Algorithm:** `RS256` (RSA SHA-256)
- **Key:** PKCS8 private key PEM dosyası
- **Claims:**
  - `sub` — userId (GUID)
  - `email` — kullanıcı email
  - `tenant_id` — tenant GUID
  - `tenant_code` — tenant code (string)
  - `actor_type` — "tenant_user" / "admin"
  - `iss` — issuer
  - `iat`, `nbf`, `exp`, `jti` — standart claim'ler
  - `perms[]` — permission string'leri (çoklu)
- **Access token yaşam:** 15 dakika
- **Refresh token:** 64 byte random hex (DB'de salt'lanarak saklanır)

### Token doğrulama

```csharp
var handler = new JwtSecurityTokenHandler();
var validation = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = issuer,
    ValidateAudience = false,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new RsaSecurityKey(publicKeyPem),
    ClockSkew = TimeSpan.FromSeconds(0)  // strict validation
};
```

### Mevcut durum (dev-bypass)

`TenantResolutionMiddleware.ValidateJwtToken()` şu anda gerçek JWT doğrulaması yapmıyor. Token'ın 3 parça (`.` ile ayrılmış) olup olmadığını kontrol edip boş bir `ClaimsPrincipal` döndürüyor. Production'da `JwtTokenService.ValidateToken()` ile değiştirilmesi gerekiyor.

## Parola Hashing

`PasswordHasher` — PBKDF2-SHA256:

- **Salt:** 16 byte random
- **Iterations:** 10,000
- **Hash length:** 32 byte
- **Format:** `pbkdf2$sha256${iterations}${saltHex}${hashHex}`
- **Verification:** HMAC'den bağımsız, sabit-zamanlı karşılaştırma (`SpanEquals`)

```csharp
// Hash
var hash = PasswordHasher.Hash("şifre123");
// pbkdf2$sha256$10000$...

// Verify
bool ok = PasswordHasher.Verify("şifre123", hash);  // true
```

## Tenant İzolasyon

### Connection String Şifreleme

- **Algoritma:** AES-GCM
- **Servis:** `AesGcmService`
- **Şifreli veri:** Catalog DB'de `TenantDatabase.PasswordEnc` (byte array)
- **Çözme:** `TenantConnectionResolver.BuildConnectionString()` içinde decrypt edilir

```
tenant password → AES-GCM encrypt → Catalog DB'ye kaydet
Catalog DB'den oku → AES-GCM decrypt → PostgreSQL connection string → AppDbContext
```

### Request-level isolation

Her HTTP istek için:
1. JWT'den `tenant_id` çıkarılır
2. `TenantConnectionResolver.GetConnectionString(tenantId)` ile tenant'ın kendi DB connection string'i alınır
3. `AppDbContext` bu connection string ile oluşturulur
4. Tüm EF sorguları bu tenant'ın verisine erişir

Başka bir tenant'ın verisine erişmek için:
- JWT'de sahtecilik (token'da farklı tenant_id) → JWT imza doğrulaması bu önler
- Manuel query injection → her DB bağımsız, farklı schema yok
- Hardcoded connstr kullanımı → production'da dev-bypass kaldırıldığında imkansız

## Audit trail

### Immutable hareketler

`StockMovement` tablosu immutable — bir hareket silinmez:

- Hareket tipi enum (14 tip): `GoodsReceipt`, `Shipment`, `TransferIn`, `TransferOut`, `MachineLoad`, `MachineUnload`, `ProductionConsumption`, `ProductionOutput`, `InventoryAdjustmentIn`, `InventoryAdjustmentOut`, `Scrap`, `CustomerReturn`, `SupplierReturn`, `Reversal`
- Her hareket `OccurredAt` (timestamp), `Note`, `CreatedByUserId` ile tam kaydedilir
- Hareketler silinmez — sadece `Reversal` tipi ile karşı hareket açılır

### FIFO layer yapısı

`FifoLayer` — her StockMovement için bir katman:
- Hareket silindiğinde ilgili FifoLayer'lar silinir (cascade)
- Hareket tipi değiştirilemez
- Stok bakiyesi (`StockBalance`) hareketlerden türetilir — her zaman hareketlerle tutarlı

## Security Headers (API Gateway)

Gateway katmanında ek güvenlik:
- Rate limiting
- CORS policy (SPA origin'i ile sınırlı)
- Request size limits
- TLS termination (production)

## Error Response Format

Başarısız istekler:

```json
{
  "message": "İşlem başarısız: detaylı mesaj",
  "errorCode": "ERROR_CODE"
}
```

HTTP status kodları:
- `400 Bad Request` — validation hatası
- `401 Unauthorized` — token geçersiz
- `402 PaymentRequired` — tenant aktif değil
- `404 Not Found` — kaynak bulunamadı
- `409 Conflict` — duplicate (örn: aynı kodda ürün)
- `500 Internal Server Error` — beklenmeyen hata
