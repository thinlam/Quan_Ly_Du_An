using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QLDA.FakeDataTool.Commands;
using QLDA.FakeDataTool.Services;
using QLDA.Persistence;

namespace QLDA.FakeDataTool;

[Command(Name = "fake", Description = "QLDA Fake Data Generator — always inserts directly")]
[Subcommand(typeof(ClearCommand))]
[HelpOption]
public class Program
{
    [Argument(0, Description = "Entity alias: da/duan, gt/goithau, hd/hopdong, all")]
    public string Entity { get; set; } = "all";

    [Argument(1, Description = "Number of entities (default: 10)")]
    public int Count { get; set; } = 10;

    [Option("-o|--output", Description = "SQLite file path (default: dev-data.db). Only used when --schema is not set.")]
    public string Output { get; set; } = "dev-data.db";

    [Option("--schema", Description = "SQL Server schema (dbo, dev). Uses SqlServer connection from appsettings.json.")]
    public string? Schema { get; set; }

    [Option("--seed", Description = "Random seed for deterministic generation (default: 12345)")]
    public int Seed { get; set; } = 12345;

    private CommandLineApplication? _app;
    private static IConfiguration? _configuration;

    public static async Task<int> Main(string[] args)
    {
        var exePath = AppContext.BaseDirectory;
        _configuration = new ConfigurationBuilder()
            .SetBasePath(exePath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        return await CommandLineApplication.ExecuteAsync<Program>(args);
    }

    public async Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken ct)
    {
        _app = app;

        if (Count < 1)
        {
            Console.Error.WriteLine("Error: Count must be at least 1.");
            return 1;
        }

        var entityType = ResolveEntityType();

        try
        {
            var (db, target) = CreateDbContext();
            var inserted = await InsertDirectAsync(entityType, Count, db, ct);
            Console.WriteLine($"Inserted {inserted} record(s) to {target}");
            await db.DisposeAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.Error.WriteLine($"Inner: {ex.InnerException.Message}");
            return 1;
        }
    }

    private (AppDbContext db, string target) CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        if (!string.IsNullOrEmpty(Schema))
        {
            var connStr = _configuration!["ConnectionStrings:SqlServer"]
                ?? throw new InvalidOperationException("Missing ConnectionStrings:SqlServer in appsettings.json");

            optionsBuilder.UseSqlServer(connStr);
            var db = new SchemaAppDbContext(optionsBuilder.Options, Schema);
            return (db, $"SQL Server (schema: {Schema})");
        }

        var sqliteConn = _configuration!["ConnectionStrings:Default"] ?? $"DataSource={Output}";
        if (!sqliteConn.StartsWith("DataSource", StringComparison.OrdinalIgnoreCase))
            sqliteConn = $"DataSource={Output}";

        optionsBuilder.UseSqlite(sqliteConn);
        var sqliteDb = new SqliteAppDbContext(optionsBuilder.Options);
        return (sqliteDb, $"SQLite: {Output}");
    }

    private EntityType ResolveEntityType()
    {
        return Entity.ToLowerInvariant() switch
        {
            "da" or "duan" => EntityType.DuAn,
            "gt" or "goithau" => EntityType.GoiThau,
            "hd" or "hopdong" => EntityType.HopDong,
            "all" => EntityType.All,
            _ => throw new CommandParsingException(_app!, $"Unknown entity alias: {Entity}. Use: da, gt, hd, all")
        };
    }

    private async Task<int> InsertDirectAsync(EntityType entityType, int count, AppDbContext db, CancellationToken ct)
    {
        if (db.Database.IsSqlite())
            await db.Database.EnsureCreatedAsync(ct);

        var service = new FakeDataService(db);
        return entityType switch
        {
            EntityType.DuAn => await service.SeedDuAnAsync(count, ct),
            EntityType.GoiThau => await service.SeedGoiThauAsync(count, ct),
            EntityType.HopDong => await service.SeedHopDongAsync(count, ct),
            EntityType.All => await service.SeedAllAsync(count, ct),
            _ => 0
        };
    }
}

public enum EntityType
{
    DuAn,
    GoiThau,
    HopDong,
    All
}
