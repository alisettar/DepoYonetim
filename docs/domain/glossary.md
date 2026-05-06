# Domain Sözlüğü

| Terim | Tanım |
|---|---|
| **Tenant** | Sistemi kullanan bağımsız şirket. Kendi DB'si ve veri izolasyonu vardır. |
| **Catalog DB** | Tüm tenant'ların kataloğu, kullanıcı eşleşmesi ve sistem yönetimi için kullanılan tek paylaşımlı PostgreSQL veritabanı. |
| **Tenant DB** | Bir tenant'a ait tüm domain verisini tutan ayrı PostgreSQL veritabanı. Şema tüm tenant'larda aynıdır. |
| **Lot / Çeki / Parti** | Tek kavram — üretim/sevk partisi numarası. Doküman boyunca **lot** terimi kullanılır. |
| **Fiziksel depo** | Gerçek alan/bina; alt konum hiyerarşisi (koridor / raf / göz) içerir. |
| **Sanal depo** | Mantıksal grup (karantina, sevk hazırlık, müşteri rezervi). Fiziksel envanteri olmaz. |
| **Makine deposu** | Bir makinenin "üzerindeki" yük. Sadece bağlı makine tarafından kullanılır. Müşteriye doğrudan sevk yapılamaz. |
| **FIFO katmanı (FifoLayer)** | Bir mal kabulü/üretim çıkışı sonucunda doğan, FIFO çıkışlarda sırasıyla tüketilen stok dilimi. |
| **Snapshot (StockBalance)** | Depo + ürün + lot kırılımında anlık stok bakiyesi. Hareketlerden türetilir; performans için tutulur. |
| **Reçete (BOM)** | Bir mamulün üretiminde hangi malzemelerin hangi miktarlarda kullanıldığını tanımlayan yapı. Çok seviyeli, versiyonlu, alternatif malzemeli. |
| **Üretim emri (UE)** | Belirli miktarda mamul üretim talimatı. Bağlı bir makinede çalışır; başlatıldığında makine deposundan otomatik tüketim olur. |
| **Reversal** | Bir stok hareketinin tam negatifi olan yeni bir hareket — düzeltme aracı (Seviye 1). |
| **Void** | Hareketin `IsVoided=true` flag'lenmesi (Seviye 3). Super-admin yetkisi gerekir. |
| **Rebuild** | Snapshot ve FIFO katmanlarının `stock_movements`'tan sıfırdan yeniden hesaplanması (Seviye 4). Veri kaybetmez. |
