# Depo Yönetim Sistemi

Karışık imalat sektörü için multi-tenant SaaS depo yönetim sistemi.

## Hızlı bakış

- **Stack:** .NET 9 Web API + Vue.js 3 + PostgreSQL + Entity Framework Core
- **Mimari:** Modüler monolith, database-per-tenant
- **Durum:** Tasarım aşaması (kod henüz yok)

## Dokümantasyon

- [Tasarım Spec'i (2026-05-06)](docs/superpowers/specs/2026-05-06-depo-yonetim-design.md)
- [Mimari dokümanlar](docs/architecture/)
- [Domain sözlüğü](docs/domain/glossary.md)
- [Operasyon kılavuzları](docs/operations/)

## Klasör yapısı

```
docs/        → tasarım, mimari, domain, operasyon dokümanları
src/backend/ → .NET Web API solution (WMS.Api, WMS.Application, WMS.Domain, ...)
src/frontend/→ Vue.js SPA
deploy/      → Docker, K8s, scripts
tools/       → seed data ve yardımcı araçlar
```

Detaylar için [tasarım dokümanına](docs/superpowers/specs/2026-05-06-depo-yonetim-design.md) bakınız.
