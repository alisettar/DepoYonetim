using WMS.Shared.Exceptions;

namespace WMS.Domain.Recipes;

public class Recipe
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public RecipeStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<RecipeVersion> _versions = [];
    public IReadOnlyCollection<RecipeVersion> Versions => _versions.AsReadOnly();

    private Recipe() { }

    public static Recipe Create(Guid id, Guid productId, string name)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Ürün zorunludur.", nameof(productId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Reçete adı zorunludur.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Reçete adı en fazla 200 karakter olabilir.", nameof(name));

        return new Recipe
        {
            Id = id,
            ProductId = productId,
            Name = name.Trim(),
            Status = RecipeStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Reçete adı zorunludur.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Reçete adı en fazla 200 karakter olabilir.", nameof(name));

        Name = name.Trim();
    }

    public void Archive()
    {
        if (Status == RecipeStatus.Archived)
            throw new BusinessException("RECIPE_ALREADY_ARCHIVED", "Reçete zaten arşivlenmiş.");

        foreach (var v in _versions.Where(v => v.IsActive))
            v.Deactivate();

        Status = RecipeStatus.Archived;
    }

    public RecipeVersion AddVersion(
        DateTime validFrom,
        DateTime? validUntil,
        decimal outputQuantity,
        Guid outputUnitId)
    {
        if (Status == RecipeStatus.Archived)
            throw new BusinessException("RECIPE_ARCHIVED", "Arşivlenmiş reçeteye versiyon eklenemez.");

        int nextVersionNo = _versions.Count == 0 ? 1 : _versions.Max(v => v.VersionNo) + 1;
        var version = RecipeVersion.Create(Id, nextVersionNo, validFrom, validUntil, outputQuantity, outputUnitId);
        _versions.Add(version);
        return version;
    }

    public void ActivateVersion(Guid versionId)
    {
        var target = _versions.FirstOrDefault(v => v.Id == versionId)
            ?? throw new BusinessException("VERSION_NOT_FOUND", "Versiyon bulunamadı.");

        foreach (var v in _versions.Where(v => v.IsActive))
            v.Deactivate();

        target.Activate();
        Status = RecipeStatus.Active;
    }

    public void SetActiveStatus() => Status = RecipeStatus.Active;

    public RecipeVersion GetVersion(Guid versionId) =>
        _versions.FirstOrDefault(v => v.Id == versionId)
        ?? throw new BusinessException("VERSION_NOT_FOUND", "Versiyon bulunamadı.");
}
