namespace WMS.Domain.Catalog;

public class ProductUnit
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid UnitId { get; private set; }
    public decimal ConversionToPrimary { get; private set; }

    private ProductUnit() { }

    internal static ProductUnit Create(Guid productId, Guid unitId, decimal conversionToPrimary)
    {
        if (conversionToPrimary <= 0)
            throw new ArgumentException("Dönüşüm katsayısı sıfırdan büyük olmalıdır.", nameof(conversionToPrimary));

        return new ProductUnit
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UnitId = unitId,
            ConversionToPrimary = conversionToPrimary
        };
    }
}
