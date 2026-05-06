namespace WMS.Domain.Recipes;

public class AlternativeMaterial
{
    public Guid Id { get; private set; }
    public Guid RecipeItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Priority { get; private set; }
    public decimal Quantity { get; private set; }
    public Guid UnitId { get; private set; }

    private AlternativeMaterial() { }

    public static AlternativeMaterial Create(
        Guid recipeItemId,
        Guid productId,
        int priority,
        decimal quantity,
        Guid unitId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Ürün zorunludur.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));
        if (unitId == Guid.Empty)
            throw new ArgumentException("Birim zorunludur.", nameof(unitId));

        return new AlternativeMaterial
        {
            Id = Guid.NewGuid(),
            RecipeItemId = recipeItemId,
            ProductId = productId,
            Priority = priority,
            Quantity = quantity,
            UnitId = unitId
        };
    }

    public void Update(int priority, decimal quantity, Guid unitId)
    {
        if (quantity <= 0)
            throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));
        if (unitId == Guid.Empty)
            throw new ArgumentException("Birim zorunludur.", nameof(unitId));

        Priority = priority;
        Quantity = quantity;
        UnitId = unitId;
    }
}
