# Testing Guide

## Quick Start

```bash
# Run all tests
dotnet test QLDA.Tests/QLDA.Tests.csproj

# Run specific test class
dotnet test QLDA.Tests --filter "FullyQualifiedName~DuAnHandlerTests"

# Generate fake data (FakeDataTool — always inserts directly)
fake.bat da 10                        # SQLite
fake.bat all 50 --schema dev          # SQL Server dev schema
```

## Architecture

```
QLDA.Tests/           # xUnit test project
├── Fakers/           # Bogus fake data generators
│   ├── EntityFakerBase.cs       # Abstract base with deterministic seed
│   ├── DuAnFaker.cs             # DuAn entity faker
│   ├── GoiThauFaker.cs          # GoiThau entity faker
│   ├── HopDongFaker.cs          # HopDong entity faker
│   ├── CatalogSeeder.cs         # Master data seeding
│   └── BusinessDataSeeder.cs    # Linked data (DuAn→GoiThau→HopDong)
├── Fixtures/
│   ├── SharedSqliteFixture.cs   # In-memory SQLite (shared collection)
│   └── IsolatedSqliteFixture.cs # File-based SQLite (per-test DB)
├── Repositories/     # Repository-level tests
├── Handlers/         # MediatR handler tests
└── Tests/            # Infrastructure/fixture tests

QLDA.FakeDataTool/    # CLI tool for fake data generation
├── Program.cs                   # CLI entry point (McMaster CLI)
├── SqliteAppDbContext.cs        # SQLite-compatible DbContext
├── SchemaAppDbContext.cs        # SQL Server schema DbContext (dbo/dev)
├── Fakers/
│   ├── EntityFakerBase.cs       # Abstract base
│   ├── FakerSeedManager.cs      # Static seed manager
│   ├── DuAnFaker.cs             # DuAn faker
│   ├── GoiThauFaker.cs          # GoiThau faker
│   ├── HopDongFaker.cs          # HopDong faker
│   └── FKReferenceData.cs       # Static FK reference IDs
├── Services/
│   └── FakeDataService.cs       # Auto-seed FK chain logic
├── Commands/
│   └── ClearCommand.cs          # Clear seeded data
└── appsettings.json             # Connection strings

fake.bat              # CLI wrapper (project root)
```

## Test Fixtures

| Fixture | Type | Use Case |
|---------|------|----------|
| `SharedSqliteFixture` | In-memory | Fast unit tests (~5-10ms/test), shared DB |
| `IsolatedSqliteFixture` | File-based | Integration tests needing clean state |

Both fixtures use `TestAppDbContext` which clears SQL Server-specific defaults (NEWSEQUENTIALID, nvarchar(max), bracket filters) for SQLite compatibility.

## Writing Tests

### Repository Test Pattern

```csharp
[Collection("SharedSqlite")]
public class MyRepositoryTests(SharedSqliteFixture fixture)
{
    [Fact]
    public async Task ShouldPersistEntity()
    {
        var entity = new MyFaker().Generate();
        fixture.DbContext.Set<MyEntity>().Add(entity);
        await fixture.DbContext.SaveChangesAsync();

        var result = await fixture.DbContext.Set<MyEntity>().FindAsync(entity.Id);
        result.Should().NotBeNull();
    }
}
```

### Handler Test Pattern

```csharp
[Collection("SharedSqlite")]
public class MyHandlerTests(SharedSqliteFixture fixture)
{
    private IMediator CreateMediator()
    {
        var services = new ServiceCollection();
        services.AddScoped<DbContext>(_ => fixture.DbContext);
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped(typeof(IUnitOfWork), sp => sp.GetRequiredService<DbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            typeof(DuAnInsertCommand).Assembly));
        var provider = services.BuildServiceProvider();
        return provider.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
    }
}
```

### Bogus Faker Pattern

```csharp
public class MyEntityFaker : EntityFakerBase<MyEntity>
{
    public MyEntityFaker() : base()
    {
        RuleFor(e => e.Name, f => f.Company.CompanyName());
        RuleFor(e => e.CreatedAt, DateTimeOffset.UtcNow);
        RuleFor(e => e.IsDeleted, false);
    }

    public MyEntityFaker WithCustomField(int value)
    {
        RuleFor(e => e.CustomField, value);
        return this;
    }
}
```

## FakeDataTool CLI

### Usage

```
fake.bat <entity> [count] [options]
fake.bat clear [options]
```

### Entity Aliases

| Alias | Entity | Auto-seed FK |
|-------|--------|-------------|
| `da`, `duan` | DuAn (Dự án) | DanhMucLoaiDuAn |
| `gt`, `goithau` | GoiThau (Gói thầu) | DuAn |
| `hd`, `hopdong` | HopDong (Hợp đồng) | DuAn + GoiThau (1:1) |
| `all` | All entities | Full chain |

### Options

| Option | Description | Default |
|--------|-------------|---------|
| `-o`, `--output` | SQLite file path | `dev-data.db` |
| `--schema` | SQL Server schema (dbo/dev) | (SQLite mode) |
| `--seed` | Random seed | 12345 |

### Target Selection

| Mode | Command | Where data goes |
|------|---------|----------------|
| **SQLite** | `fake.bat da 10` | `dev-data.db` file |
| **SQLite custom** | `fake.bat da 10 -o my.db` | Custom file |
| **SQL Server dbo** | `fake.bat da 10 --schema dbo` | SQL Server (dbo schema) |
| **SQL Server dev** | `fake.bat da 10 --schema dev` | SQL Server (dev schema) |

### Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "Default": "DataSource=dev-data.db",
    "SqlServer": "Server=.;Database=QLDA;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

- No `--schema` → uses `ConnectionStrings:Default` (SQLite)
- With `--schema` → uses `ConnectionStrings:SqlServer` (SQL Server)

### Examples

```bash
# === SQLite (default) ===
fake.bat da 10                    # Insert 10 DuAn → dev-data.db
fake.bat gt 5                     # Auto-seed DuAn, insert 5 GoiThau
fake.bat hd 3                     # Auto-seed DuAn+GoiThau, insert 3 HopDong
fake.bat all 20                   # Full chain: 20 each (60 total)

# === Custom SQLite file ===
fake.bat da 10 -o test-data.db

# === SQL Server schema ===
fake.bat da 10 --schema dev       # Insert to dev schema
fake.bat all 50 --schema dbo      # Insert to dbo schema

# === Clear data ===
fake.bat clear                    # Clear dev-data.db
fake.bat clear -o test-data.db    # Clear specific file
fake.bat clear --schema dev       # Clear SQL Server dev schema

# === Deterministic data ===
fake.bat da 10 --seed 99999       # Different seed = different data
```

### Auto-seed FK Chain

FakeDataTool automatically seeds parent entities when needed:

```
Seed DuAn → auto-seeds DanhMucLoaiDuAn (if not exists)
Seed GoiThau → auto-seeds DuAn (if not enough)
Seed HopDong → auto-seeds GoiThau (if not enough, respects 1:1)
Seed all → creates linked chain: DuAn → GoiThau → HopDong
```

### GoiThau-HopDong 1:1 Relationship

GoiThau and HopDong have a one-to-one relationship. FakeDataTool handles this:

- `fake.bat gt 5` — Creates 5 GoiThau (no HopDong)
- `fake.bat hd 3` — Finds GoiThau without HopDong, creates 3 HopDong (auto-seeds more GoiThau if needed)
- `fake.bat all 10` — Creates 10 DuAn, 10 GoiThau, 10 HopDong (linked 1:1)

### Architecture: How Target Selection Works

```
┌─────────────────────────────────────┐
│ fake.bat da 10                      │
│              ↓                      │
│ Has --schema flag?                  │
│   NO → SQLite mode                  │
│     ├── SqliteAppDbContext          │
│     ├── Clears SQL Server defaults  │
│     └── EnsureCreatedAsync()        │
│   YES → SQL Server mode             │
│     ├── SchemaAppDbContext          │
│     ├── HasDefaultSchema("dev")     │
│     └── DB must already exist       │
└─────────────────────────────────────┘
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| `SQLite Error: 'near "max": syntax error'` | SqliteAppDbContext clears nvarchar(max) → TEXT |
| `not constant default value` | SqliteAppDbContext clears DefaultValueSql (NEWSEQUENTIALID etc.) |
| `relationship has been severed` | GoiThau-HopDong is 1:1, ensure unique GoiThauId per HopDong |
| Tests share data | SharedSqliteFixture uses shared DB; use IsolatedSqliteFixture for clean state |
| `Database not found` | FakeDataTool clear on nonexistent file — harmless |
| `Missing ConnectionStrings:SqlServer` | Must configure appsettings.json before using `--schema` |
| `no such table: HopDong` | FakeDataTool handles gracefully — run seed first |

## Test Count

| Category | Tests | Status |
|----------|-------|--------|
| Infrastructure | 6 | Passing |
| DuAn Repository | 6 | Passing |
| GoiThau Repository | 3 | Passing |
| HopDong Repository | 4 | Passing |
| DuAn Handler | 3 | Passing |
| HopDong Handler | 3 | Passing |
| **Total** | **25** | **All passing** |
