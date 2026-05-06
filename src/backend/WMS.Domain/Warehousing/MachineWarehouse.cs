namespace WMS.Domain.Warehousing;

public class MachineWarehouse : Warehouse
{
    public string MachineCode { get; private set; } = string.Empty;
    public MachineWarehouseStatus MachineStatus { get; private set; }

    private MachineWarehouse() { }

    public static MachineWarehouse Create(
        Guid id,
        string code,
        string name,
        string machineCode,
        string? address = null,
        Guid? parentWarehouseId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Depo kodu zorunludur.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Depo adı zorunludur.", nameof(name));
        if (string.IsNullOrWhiteSpace(machineCode))
            throw new ArgumentException("Makine kodu zorunludur.", nameof(machineCode));
        if (code.Length > 20)
            throw new ArgumentException("Depo kodu en fazla 20 karakter olabilir.", nameof(code));
        if (name.Length > 200)
            throw new ArgumentException("Depo adı en fazla 200 karakter olabilir.", nameof(name));
        if (machineCode.Length > 50)
            throw new ArgumentException("Makine kodu en fazla 50 karakter olabilir.", nameof(machineCode));

        return new MachineWarehouse
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Type = WarehouseType.Machine,
            Status = WarehouseStatus.Active,
            Address = address?.Trim(),
            ParentWarehouseId = parentWarehouseId,
            CreatedAt = DateTime.UtcNow,
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            MachineStatus = MachineWarehouseStatus.Idle
        };
    }

    public void SetMachineStatus(MachineWarehouseStatus status) => MachineStatus = status;
}
