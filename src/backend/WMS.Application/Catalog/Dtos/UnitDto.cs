using WMS.Domain.Catalog;

namespace WMS.Application.Catalog.Dtos;

public record UnitDto(Guid Id, string Code, string Name)
{
    public static UnitDto FromEntity(Unit u) => new(u.Id, u.Code, u.Name);
}
