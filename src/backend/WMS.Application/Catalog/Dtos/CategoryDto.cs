using WMS.Domain.Catalog;

namespace WMS.Application.Catalog.Dtos;

public record CategoryDto(Guid Id, string Code, string Name)
{
    public static CategoryDto FromEntity(Category c) => new(c.Id, c.Code, c.Name);
}
