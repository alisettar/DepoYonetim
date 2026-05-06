namespace WMS.Domain.Catalog;

public class Category
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private Category() { }

    public static Category Create(Guid id, string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Kategori kodu zorunludur.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kategori adı zorunludur.", nameof(name));
        if (code.Length > 20)
            throw new ArgumentException("Kategori kodu en fazla 20 karakter olabilir.", nameof(code));
        if (name.Length > 100)
            throw new ArgumentException("Kategori adı en fazla 100 karakter olabilir.", nameof(name));

        return new Category
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim()
        };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kategori adı zorunludur.", nameof(name));
        if (name.Length > 100)
            throw new ArgumentException("Kategori adı en fazla 100 karakter olabilir.", nameof(name));
        Name = name.Trim();
    }
}
