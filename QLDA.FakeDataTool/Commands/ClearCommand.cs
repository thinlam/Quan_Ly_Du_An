using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QLDA.Domain.Entities;
using QLDA.Persistence;
using QLDA.FakeDataTool;

namespace QLDA.FakeDataTool.Commands;

/// <summary>
/// Clear seeded data command.
/// Deletes in reverse dependency order: HopDong → GoiThau → DuAn.
/// Supports both SQLite and SQL Server schema.
/// </summary>
[Command("clear", Description = "Clear seeded data from database")]
public class ClearCommand
{
    [Option("-o|--output", Description = "SQLite file path (default: dev-data.db). Only used when --schema is not set.")]
    public string Output { get; set; } = "dev-data.db";

    [Option("--schema", Description = "SQL Server schema (dbo, dev). Uses SqlServer connection from appsettings.json.")]
    public string? Schema { get; set; }

    public async Task<int> OnExecuteAsync(CancellationToken ct)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            AppDbContext db;

            if (!string.IsNullOrEmpty(Schema))
            {
                // SQL Server mode
                var exePath = AppContext.BaseDirectory;
                var config = new ConfigurationBuilder()
                    .SetBasePath(exePath)
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                var connStr = config["ConnectionStrings:SqlServer"]
                    ?? throw new InvalidOperationException("Missing ConnectionStrings:SqlServer in appsettings.json");

                optionsBuilder.UseSqlServer(connStr);
                db = new SchemaAppDbContext(optionsBuilder.Options, Schema);
            }
            else
            {
                // SQLite mode
                if (!File.Exists(Output))
                {
                    Console.WriteLine($"Database not found: {Output}");
                    return 0;
                }
                optionsBuilder.UseSqlite($"DataSource={Output}");
                db = new SqliteAppDbContext(optionsBuilder.Options);
            }

            await using (db)
            {
                // Delete in reverse dependency order
                int hdCount = 0, gtCount = 0, daCount = 0;

                try { hdCount = await db.Set<HopDong>().ExecuteDeleteAsync(ct); }
                catch (Exception) { }

                try { gtCount = await db.Set<GoiThau>().ExecuteDeleteAsync(ct); }
                catch (Exception) { }

                try { daCount = await db.Set<DuAn>().ExecuteDeleteAsync(ct); }
                catch (Exception) { }

                var target = !string.IsNullOrEmpty(Schema) ? $"SQL Server (schema: {Schema})" : $"SQLite: {Output}";
                Console.WriteLine($"Deleted: {hdCount} HopDong, {gtCount} GoiThau, {daCount} DuAn");
                Console.WriteLine($"Database cleared: {target}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}