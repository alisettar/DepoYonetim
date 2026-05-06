using System.CommandLine;

var rootCommand = new RootCommand("WMS Migrate CLI - Database migration and tenant management tool")
{
    new Option<string>(new[] { "--tenant-id", "-t" }),
    new Option<string>(new[] { "--all", "-a" }),
    new Option<string>(new[] { "--parallel", "-p" })
};

rootCommand.SetHandler(() =>
{
    Console.WriteLine("WMS Migrate CLI");
    Console.WriteLine("Usage: wms-migrate <command> [options]");
});

return await rootCommand.InvokeAsync(args);
