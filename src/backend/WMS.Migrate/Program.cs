var command = args.Length > 0 ? args[0] : string.Empty;

return command switch
{
    "apply"            => Run("apply", args),
    "create-tenant"    => Run("create-tenant", args),
    "rebuild-balances" => Run("rebuild-balances", args),
    "status"           => Run("status", args),
    _                  => Usage()
};

static int Run(string cmd, string[] args)
{
    Console.WriteLine($"TODO: {cmd}");
    return 0;
}

static int Usage()
{
    Console.WriteLine("WMS Migrate CLI");
    Console.WriteLine();
    Console.WriteLine("Komutlar:");
    Console.WriteLine("  apply [--tenant-id <id>] [--all] [--parallel <n>]");
    Console.WriteLine("  create-tenant <code> <name>");
    Console.WriteLine("  rebuild-balances --tenant-id <id>");
    Console.WriteLine("  status");
    return 0;
}
