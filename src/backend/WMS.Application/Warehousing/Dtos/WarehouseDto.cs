using WMS.Domain.Warehousing;

namespace WMS.Application.Warehousing.Dtos;

public record WarehouseLocationDto(
    Guid Id,
    string Code,
    string Name,
    decimal? Capacity,
    bool IsActive)
{
    public static WarehouseLocationDto FromEntity(WarehouseLocation l) =>
        new(l.Id, l.Code, l.Name, l.Capacity, l.IsActive);
}

public record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    string Type,
    string Status,
    string? Address,
    Guid? ParentWarehouseId,
    DateTime CreatedAt,
    IReadOnlyCollection<WarehouseLocationDto> Locations,
    string? MachineCode = null,
    string? MachineStatus = null)
{
    public static WarehouseDto FromEntity(Warehouse w)
    {
        string? machineCode = null;
        string? machineStatus = null;
        if (w is MachineWarehouse mw)
        {
            machineCode = mw.MachineCode;
            machineStatus = mw.MachineStatus.ToString();
        }

        return new WarehouseDto(
            w.Id, w.Code, w.Name,
            w.Type.ToString(), w.Status.ToString(),
            w.Address, w.ParentWarehouseId, w.CreatedAt,
            w.Locations.Select(WarehouseLocationDto.FromEntity).ToList(),
            machineCode, machineStatus);
    }
}
