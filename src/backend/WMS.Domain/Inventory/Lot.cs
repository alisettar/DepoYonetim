using WMS.Domain.Inventory.Enums;

namespace WMS.Domain.Inventory;

public class Lot
{
    public Guid Id { get; protected set; }
    public Guid ProductId { get; protected set; }
    public string LotNumber { get; protected set; } = string.Empty;
    public DateTime ProductionDate { get; protected set; }
    public DateTime? ExpiryDate { get; protected set; }
    public LotSource Source { get; protected set; }
    public Guid? SourceReferenceId { get; protected set; }
    public QualityStatus QualityStatus { get; protected set; }
    public DateTime CreatedAt { get; protected set; }

    protected Lot() { }

    public static Lot Create(
        Guid id,
        Guid productId,
        string lotNumber,
        LotSource source,
        DateTime? productionDate = null,
        DateTime? expiryDate = null,
        Guid? sourceReferenceId = null)
    {
        if (string.IsNullOrEmpty(lotNumber))
            throw new ArgumentException("Lot numarası zorunludur.", nameof(lotNumber));
        if (lotNumber.Length > 50)
            throw new ArgumentException("Lot numarası en fazla 50 karakter olabilir.", nameof(lotNumber));

        return new Lot
        {
            Id = id,
            ProductId = productId,
            LotNumber = lotNumber,
            Source = source,
            ProductionDate = productionDate ?? DateTime.UtcNow,
            ExpiryDate = expiryDate,
            SourceReferenceId = sourceReferenceId,
            QualityStatus = QualityStatus.OK,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateQualityStatus(QualityStatus newStatus)
    {
        QualityStatus = newStatus;
    }
}
