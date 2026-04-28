# Testing Guide — QLDA

> Hướng dẫn đầy đủ về hệ thống testing: kiến trúc, cách hoạt động từng bước, cách viết test mới, và cách chạy.

---

## Developer Workflows

### Workflow 1: SQLite Local (không cần SQL Server)

Dành cho dev phát triển cục bộ. Không cần hạ tầng gì, chỉ cần .NET 8 SDK.

```bash
# 1. Clone
git clone <repository-url> && cd SER

# 2. Tạo database SQLite + schema
ef.bat update --sqlite           # → dev-data.db

# 3. Seed fake data
fake.bat all 20                  # → 60 records (DuAn + GoiThau + HopDong)

# 4. Chạy tests (SQLite in-memory, độc lập với dev-data.db)
test.bat

# 5. Tạo migration nếu schema thay đổi
ef.bat add DescribeChange        # → tạo migration file
ef.bat update --sqlite           # → tạo lại SQLite với schema mới
fake.bat clear && fake.bat all 20

# 6. Sẵn sàng review? Push branch, CI chạy cùng tests trên server
```

**Dùng khi:** Phát triển feature, viết tests, iterate schema. Không cần SQL Server.

### Workflow 2: SQL Server Dev Schema

Dành cho môi trường phát triển chung. Dùng `--schema dev` để cách ly data.

```bash
# 1. Clone + cấu hình SQL Server connection trong appsettings.json

# 2. Apply migrations vào dev schema
ef.bat update                    # → SQL Server (default connection)

# 3. Seed data vào dev schema
fake.bat all 50 --schema dev     # → cách ly trong schema [dev]

# 4. Chạy WebApi trỏ vào dev schema
dotnet run --project QLDA.WebApi

# 5. Verify qua Swagger với SQL Server thật

# 6. Chạy tests trước khi push
test.bat
```

**Dùng khi:** Test với SQL Server thật, môi trường team dùng chung, validate SQL-specific features (Dapper queries, stored procedures).

### Workflow 3: Staging/Production Deployment

Triển khai lên môi trường thật. Migrations + build đã verify chỉ.

```bash
# 1. CI pipeline chạy (tự động)
dotnet build SER.sln
dotnet test QLDA.Tests           # Tất cả tests phải pass

# 2. Apply migrations vào database target
ef.bat update                    # → Production SQL Server

# 3. Deploy WebApi
dotnet publish QLDA.WebApi -c Release -o ./publish

# 4. (Optional) Seed staging data cho QA
fake.bat all 100 --schema staging
```

**Dùng khi:** Merge vào main, deploy lên staging/prod. CI gate đảm bảo tests pass trước khi migration chạy.

### So sánh 3 Workflows

| Aspect | SQLite Local | SQL Server Dev | Staging/Prod |
|--------|-------------|----------------|--------------|
| Hạ tầng | Không cần | SQL Server | SQL Server |
| Database | `dev-data.db` (file) | Schema `[dev]` | Schema `[dbo]` |
| Schema method | `EnsureCreated()` | `Database.Migrate()` | `Database.Migrate()` |
| Seed data | `fake.bat all 20` | `fake.bat all 50 --schema dev` | `fake.bat all 100 --schema staging` |
| Tests | `test.bat` | `test.bat` | CI pipeline |
| Xác minh | Tests pass | Swagger + SQL | CI + QA |
| Rủi ro | Zero | Thấp | Có kiểm soát |

---

## Quick Start

```bash
# Build solution
dotnet build SER.sln

# Run all tests
test.bat

# Run only integration (HTTP endpoint) tests
test.bat int

# Run by controller
test.bat duan       # DuAn tests
test.bat goithau    # GoiThau tests
test.bat hopdong    # HopDong tests

# Generate fake data
fake.bat da 10                        # SQLite
fake.bat all 50 --schema dev          # SQL Server dev schema
```

---

## Database Migrations

### Architecture

QLDA.Migrator supports both SQL Server and SQLite via `--provider` parameter:

```
dotnet run --project QLDA.Migrator                              # SQL Server (default)
dotnet run --project QLDA.Migrator -- --provider sqlite         # SQLite
```

**SQL Server mode:**
```
QLDA.Migrator/Program.cs --provider sqlserver
  → AddPersistence(connectionStrings, migratorAssemblyName)
    → services.AddDbContext<AppDbContext>(UseSqlServer)
  → app.MigrateAppDb()
    → AppDbContext.Database.Migrate()    ← Applies migration files with retry policy
```

**SQLite mode:**
```
QLDA.Migrator/Program.cs --provider sqlite
  → AddPersistenceSqlite(sqliteConnectionString)
    → services.AddDbContext<AppDbContext>(UseSqlite) + factory override
    → Resolves SqliteAppDbContext (clears SQL Server defaults)
  → app.EnsureCreatedAppDb()
    → SqliteAppDbContext.Database.EnsureCreated()    ← Creates schema from EF model
```

### Apply Migrations

**SQL Server (default):**

Configure in `QLDA.Migrator/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QLDA;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

```bash
dotnet run --project QLDA.Migrator
# or explicitly:
dotnet run --project QLDA.Migrator -- --provider sqlserver
```

Verifies via `__EFMigrationsHistory` table — lists all applied migrations.

**SQLite:**

```json
{
  "ConnectionStrings": {
    "SqliteConnection": "DataSource=dev-data.db"
  }
}
```

```bash
dotnet run --project QLDA.Migrator -- --provider sqlite
```

Creates database file with all tables from the EF Core model.

### Create New Migration

When adding/modifying entities in `QLDA.Domain` or configurations in `QLDA.Persistence`:

```bash
# Generate migration
dotnet ef migrations add <MigrationName> --project QLDA.Persistence --startup-project QLDA.Migrator

# Example: adding a new column
dotnet ef migrations add AddNewFieldToHopDong --project QLDA.Persistence --startup-project QLDA.Migrator

# Apply to SQL Server
dotnet run --project QLDA.Migrator

# Apply to SQLite
dotnet run --project QLDA.Migrator -- --provider sqlite
```

**Migration file location:** `QLDA.Migrator/Migrations/`

Each migration generates 3 files:
- `<timestamp>_<name>.cs` — Up/Down migration logic
- `<timestamp>_<name>.Designer.cs` — Migration snapshot
- `AppDbContextModelSnapshot.cs` — Current model snapshot (updated on each migration)

### SQLite vs SQL Server

| Aspect | SQL Server | SQLite |
|--------|-----------|--------|
| Method | `Database.Migrate()` | `Database.EnsureCreated()` |
| Source | Migration files in `QLDA.Migrator/Migrations/` | EF Core model (`OnModelCreating`) |
| Schema | Full SQL Server features | Subset (no NEWSEQUENTIALID, etc.) |
| DbContext | `AppDbContext` | `SqliteAppDbContext` (shared, in Persistence) |
| Tool | `QLDA.Migrator --provider sqlserver` | `QLDA.Migrator --provider sqlite` |

**Why SQLite doesn't use migration files:** Existing migrations contain SQL Server-specific SQL (`SYSDATETIMEOFFSET()`, `DATEDIFF()`, `SqlServer:Identity`). `EnsureCreated()` creates schema directly from the shared EF Core model instead. `SqliteAppDbContext` automatically replaces SQL Server defaults with SQLite equivalents (STRFTIME, JULIANDAY).

### Shared SqliteAppDbContext

Located at `QLDA.Persistence/SqliteAppDbContext.cs` — shared by:
- **QLDA.Migrator** (SQLite mode) — via `AddPersistenceSqlite()` DI registration
- **QLDA.FakeDataTool** — direct instantiation in `Program.cs`
- **QLDA.Tests** — `SqliteTestDbContext` inherits from it

### Migration Workflow

```
1. Modify entity in QLDA.Domain/
2. Add EF config in QLDA.Persistence/ (if needed)
3. dotnet ef migrations add <Name> --project QLDA.Persistence --startup-project QLDA.Migrator
4. Review generated migration files
5. dotnet run --project QLDA.Migrator                              ← Apply to SQL Server
6. dotnet run --project QLDA.Migrator -- --provider sqlite         ← Apply to SQLite
7. Run tests to verify: test.bat
```

---

## Project Structure

```
QLDA.Tests/                          # xUnit test project
├── Fakers/                          # Bogus fake data generators
│   ├── EntityFakerBase.cs           # Abstract base with deterministic seed
│   ├── DuAnFaker.cs                 # DuAn entity faker
│   ├── GoiThauFaker.cs              # GoiThau entity faker
│   ├── HopDongFaker.cs              # HopDong entity faker
│   ├── CatalogSeeder.cs             # Master data seeding
│   ├── BusinessDataSeeder.cs        # Linked data (DuAn→GoiThau→HopDong)
│   └── TestDtoFaker.cs              # DTO fakers for integration tests
├── Fixtures/
│   ├── SharedSqliteFixture.cs       # In-memory SQLite (unit tests)
│   ├── IsolatedSqliteFixture.cs     # File-based SQLite (per-test DB)
│   ├── SqliteTestDbContext.cs       # Inherits shared SqliteAppDbContext from Persistence
│   └── WebApiFixture.cs             # WebApplicationFactory + JWT bypass
├── Integration/                     # HTTP endpoint integration tests
│   ├── DuAnControllerTests.cs       # 4 tests: GetChiTiet, Create, Update, SoftDelete
│   ├── GoiThauControllerTests.cs    # 4 tests: GetChiTiet, Create, Update, Delete
│   └── HopDongControllerTests.cs    # 4 tests: GetChiTiet, Create, Update, Delete
├── Repositories/                    # Repository-level tests
├── Handlers/                        # MediatR handler tests
└── Tests/                           # Infrastructure/fixture tests

QLDA.FakeDataTool/                   # CLI tool for fake data generation
├── Program.cs                       # CLI entry point (McMaster CLI)
├── SchemaAppDbContext.cs            # SQL Server schema DbContext (dbo/dev)
├── Fakers/
│   ├── EntityFakerBase.cs           # Abstract base
│   ├── FakerSeedManager.cs          # Static seed manager
│   ├── DuAnFaker.cs                 # DuAn faker
│   ├── GoiThauFaker.cs              # GoiThau faker
│   ├── HopDongFaker.cs              # HopDong faker
│   └── FKReferenceData.cs           # Static FK reference IDs
├── Services/
│   └── FakeDataService.cs           # Auto-seed FK chain logic
├── Commands/
│   └── ClearCommand.cs              # Clear seeded data
└── appsettings.json                 # Connection strings

test.bat                              # Test runner wrapper
fake.bat                              # Fake data CLI wrapper
```

---

## Test Types

| Type | Fixture | Speed | What it tests |
|------|---------|-------|---------------|
| Unit (Repository) | SharedSqliteFixture | ~5-10ms | EF Core queries directly |
| Unit (Handler) | SharedSqliteFixture + MediatR | ~10-20ms | Command/Query handlers |
| Integration (HTTP) | WebApiFixture | ~100-500ms | Full ASP.NET pipeline end-to-end |

---

## HTTP Integration Tests — Deep Dive

### Tổng quan kiến trúc

Integration tests chạy **thực sự** trên ASP.NET pipeline — không mock, không giả lập. Server được khởi tạo bởi `WebApplicationFactory<Program>`, request đi qua routing → auth → controller → MediatR handler → EF Core → database giống y hệt production, chỉ thay SQL Server bằng SQLite in-memory.

```
┌──────────────────────────────────────────────────────┐
│  Test Class: DuAnControllerTests                     │
│    │                                                 │
│    │  HttpClient.PostAsync("/api/du-an/them-moi")    │
│    ▼                                                 │
│  ┌─────────────────────────────────────────────────┐ │
│  │  WebApiFixture (WebApplicationFactory<Program>)  │ │
│  │                                                   │ │
│  │  ASP.NET Pipeline (REAL, not mocked):            │ │
│  │  ┌─────────────┐                                 │ │
│  │  │ Routing     │ → DuAnController.Create()       │ │
│  │  │ Auth (JWT)  │ → Validate test token           │ │
│  │  │ MediatR     │ → DuAnInsertCommand.Handler     │ │
│  │  │ EF Core     │ → SqliteTestDbContext           │ │
│  │  │ SQLite      │ → INSERT INTO "DuAn" ...        │ │
│  │  └─────────────┘                                 │ │
│  │                                                   │ │
│  │  Response: { "result": true, "dataResult": ... } │ │
│  └─────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────┘
```

### Tại sao cần `public partial class Program`

WebApi dùng **top-level statements** trong `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
// ...
app.Run();
```

C# compiler tự tạo class `Program` nhưng mặc định là `internal`. `WebApplicationFactory<Program>` cần `Program` là `public` để tạo test server.

**Fix:** Thêm 1 dòng cuối `Program.cs`:

```csharp
app.Run();

public partial class Program { }    // ← Expose Program for WebApplicationFactory
```

### Tại sao cần InternalsVisibleTo

`WebApplicationFactory<Program>` hoạt động trong `QLDA.Tests` project nhưng cần truy cập `Program` từ `QLDA.WebApi`. Dù `Program` giờ đã public, nhiều DI services trong WebApi vẫn là internal.

**File:** `QLDA.WebApi/Properties/AssemblyInfo.cs`

```csharp
[assembly: InternalsVisibleTo("QLDA.Tests")]
```

### SqliteTestDbContext — Tại sao cần override AppDbContext

`AppDbContext.OnModelCreating` cấu hình SQL Server-specific defaults:

```csharp
// Trong ConfigurationExtension.cs — SQL Server ONLY
builder.Property<DateTimeOffset>("CreatedAt")
    .HasDefaultValueSql("SYSDATETIMEOFFSET()");        // ← SQLite không hiểu

builder.Property<Guid>("Id")
    .HasDefaultValueSql("NEWSEQUENTIALID()");           // ← SQLite không hiểu

builder.Property<long>("Index")
    .HasDefaultValueSql("DATEDIFF(SECOND, '19700101', GETUTCDATE())");  // ← SQLite không hiểu
```

Nếu dùng `AppDbContext` trực tiếp với SQLite → `NOT NULL constraint failed` vì:
1. EF thấy `HasDefaultValueSql` → không gửi giá trị trong INSERT (tin DB sẽ tạo)
2. SQLite không hiểu `SYSDATETIMEOFFSET()` → cột NULL → vi phạm NOT NULL

**Solution:** `SqliteTestDbContext` override `OnModelCreating`, duyệt tất cả properties và thay thế:

| SQL Server function | SQLite replacement |
|---------------------|--------------------|
| `SYSDATETIMEOFFSET()` | `STRFTIME('%Y-%m-%dT%H:%M:%fZ', 'NOW')` |
| `DATEDIFF(SECOND, ...)` | `CAST((JULIANDAY('NOW') - 2440587.5) * 86400 AS INTEGER)` |
| `NEWSEQUENTIALID()` | `null` (EF tự tạo `Guid.NewGuid()` client-side) |
| `nvarchar(max)` | `TEXT` |
| `HasFilter("[...]")` | `HasFilter(null)` (xóa SQL Server bracket syntax) |

```csharp
public class SqliteTestDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ClearSqlServerDefaults(modelBuilder);
    }

    private static void ClearSqlServerDefaults(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties().ToList())
            {
                var defaultValueSql = property.GetDefaultValueSql();
                if (string.IsNullOrEmpty(defaultValueSql)) continue;

                // Replace SQL Server functions with SQLite equivalents
                if (defaultValueSql.Contains("SYSDATETIMEOFFSET") || ...)
                    propBuilder.HasDefaultValueSql("STRFTIME(...)");
                // ... similar for other functions
            }
        }
    }
}
```

### WebApiFixture — Cách hoạt động từng bước

```csharp
public class WebApiFixture : WebApplicationFactory<Program>, IAsyncLifetime, IWebApiFixture
```

**Bước 1: `ConfigureWebHost` — Override DI registrations**

```csharp
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.UseEnvironment("Testing");
    builder.ConfigureServices(services =>
    {
        // 1. Remove existing AppDbContext registrations
        // 2. Open SQLite in-memory connection
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // 3. Disable FK enforcement (DanhMuc not fully seeded)
        //    PRAGMA foreign_keys only works BEFORE any transaction
        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "PRAGMA foreign_keys = OFF;";
            cmd.ExecuteNonQuery();
        }

        // 4. Register AppDbContext → factory creates SqliteTestDbContext
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

        // Override to create SqliteTestDbContext instead of AppDbContext
        services.AddScoped<AppDbContext>(sp =>
        {
            var options = sp.GetRequiredService<DbContextOptions<AppDbContext>>();
            return new SqliteTestDbContext(options);  // ← Clears SQL Server defaults
        });

        // 5. Override JWT to accept test tokens
        services.PostConfigure<JwtBearerOptions>("Bearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("12345678901234567890123456789012")),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
            };
        });
    });
}
```

**Tại sao dùng factory thay `AddDbContext<AppDbContext, SqliteTestDbContext>`?**

`AddDbContext<AppDbContext, SqliteTestDbContext>` đăng ký `DbContextOptions<SqliteTestDbContext>`, nhưng handlers resolve `DbContextOptions<AppDbContext>` → DI error `Unable to resolve service`. Factory pattern giải quyết: DI resolves `DbContextOptions<AppDbContext>` + tạo `SqliteTestDbContext` manually.

**Bước 2: `InitializeAsync` — Seed data**

```csharp
public async Task InitializeAsync()
{
    Client = CreateClient();   // Build test server, tạo HttpClient

    // Tạo SqliteTestDbContext riêng cho seeding
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(_connection).Options;
    _seedDb = new SqliteTestDbContext(options);

    await _seedDb.Database.EnsureCreatedAsync();  // Tạo tất cả bảng từ EF model
    await SeedReferenceDataAsync();                 // Seed DuAn + GoiThau + HopDong
}
```

**Seed data:**

```csharp
private async Task SeedReferenceDataAsync()
{
    var duAn = new DuAn { TenDuAn = "Test Dự án", LoaiDuAnId = 1, TrangThaiDuAnId = 1, ... };
    _seedDb.Set<DuAn>().Add(duAn);
    await _seedDb.SaveChangesAsync();

    var goiThau = new GoiThau { DuAnId = duAn.Id, Ten = "Test Gói thầu", ... };
    _seedDb.Set<GoiThau>().Add(goiThau);
    await _seedDb.SaveChangesAsync();

    var hopDong = new HopDong { DuAnId = duAn.Id, GoiThauId = goiThau.Id, ... };
    _seedDb.Set<HopDong>().Add(hopDong);
    await _seedDb.SaveChangesAsync();
}
```

**Bước 3: JWT Token generation**

```csharp
public string GenerateTestToken()
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
    var claims = new[]
    {
        new Claim(ClaimConstants.Roles, RoleConstants.QLDA_QuanTri),
        new Claim(ClaimConstants.Roles, RoleConstants.QLDA_TatCa),
    };
    var token = new JwtSecurityToken(claims: claims, signingCredentials: ...);
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

Key `"12345678901234567890123456789012"` phải khớp với key trong `appsettings.json` của WebApi. `PostConfigure` override `TokenValidationParameters` để test server chấp nhận token này.

**Bước 4: Test class pattern**

```csharp
[Collection("WebApi")]                           // ← Shared fixture (chạy tuần tự)
public class DuAnControllerTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task Create_ValidDto_ReturnsOk() { ... }
}
```

- `[Collection("WebApi")]` → xUnit inject `WebApiFixture` (shared, khởi tạo 1 lần)
- Tests trong cùng collection chạy **tuần tự** (không song song) — tránh xung đột DB
- `CreateAuthenticatedClient()` tạo **HttpClient mới** mỗi lần (stateless)

---

## Test Patterns — Các mẫu test

### GET endpoint (chi tiết)

```csharp
[Fact]
public async Task GetChiTiet_ExistingId_ReturnsOk()
{
    var response = await AuthedClient.GetAsync($"/api/du-an/{fixture.SeededDuAnId}/chi-tiet");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ResultApi>();
    result.Should().NotBeNull();
    result!.Result.Should().BeTrue();
}
```

Flow: `HttpClient → ASP.NET routing → JWT auth → DuAnController.GetChiTiet() → MediatR → SQLite SELECT → JSON response`

### POST endpoint (create)

```csharp
[Fact]
public async Task Create_ValidDto_ReturnsOk()
{
    var dto = new DuAnInsertDtoFaker().Generate();     // Bogus tạo DTO giả
    var response = await AuthedClient.PostAsJsonAsync("/api/du-an/them-moi", dto);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ResultApi>();
    result!.Result.Should().BeTrue();
    result!.DataResult.Should().NotBeNull();
}
```

### DELETE endpoint (create-then-delete)

```csharp
[Fact]
public async Task SoftDelete_ExistingDuAn_ReturnsOk()
{
    // Bước 1: Tạo entity mới (không xóa shared seed data)
    var createDto = new DuAnInsertDtoFaker().Generate();
    var createResponse = await AuthedClient.PostAsJsonAsync("/api/du-an/them-moi", createDto);
    var createResult = await createResponse.Content.ReadFromJsonAsync<ResultApi>();

    // Bước 2: Extract Guid từ response
    // Lưu ý: ASP.NET dùng camelCase → "id" chứ không phải "Id"
    var idToDelete = createResult!.DataResult switch
    {
        System.Text.Json.JsonElement el => el.GetProperty("id").GetGuid(),
        Guid g => g,
        _ => throw new InvalidOperationException(...)
    };

    // Bước 3: Delete
    var response = await AuthedClient.DeleteAsync($"/api/du-an/{idToDelete}/xoa-tam");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### GoiThau-HopDong 1:1 pattern

GoiThau và HopDong có quan hệ 1:1 (unique index trên GoiThauId). Seeded GoiThau **đã có** HopDong → phải tạo GoiThau mới trước khi tạo HopDong:

```csharp
[Fact]
public async Task Create_ValidDto_ReturnsOk()
{
    // Bước 1: Tạo GoiThau mới (chưa có HopDong)
    var goiThauDto = new GoiThauInsertDtoFaker(fixture.SeededDuAnId).Generate();
    var goiThauResp = await AuthedClient.PostAsJsonAsync("/api/goi-thau/them-moi", goiThauDto);
    var goiThauResult = await goiThauResp.Content.ReadFromJsonAsync<ResultApi>();
    var newGoiThauId = extractGuid(goiThauResult!);

    // Bước 2: Tạo HopDong với GoiThauId mới
    var dto = new HopDongInsertDtoFaker(fixture.SeededDuAnId, newGoiThauId).Generate();
    var response = await AuthedClient.PostAsJsonAsync("/api/hop-dong/them-moi", dto);
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

---

## Bogus DTO Fakers

Fakers tạo dữ liệu giả cho POST/PUT request payloads:

```csharp
// QLDA.Tests/Fakers/TestDtoFaker.cs
public class DuAnInsertDtoFaker : Faker<DuAnInsertDto>
{
    public DuAnInsertDtoFaker()
    {
        RuleFor(x => x.TenDuAn, f => f.Company.CompanyName());
        RuleFor(x => x.MaDuAn, f => f.Random.AlphaNumeric(8).ToUpper());
        RuleFor(x => x.LoaiDuAnId, 1);              // DanhMucLoaiDuAn Id
        RuleFor(x => x.TrangThaiDuAnId, 1);          // DanhMucTrangThaiDuAn Id
        RuleFor(x => x.TongMucDauTu, f => f.Random.Long(1_000_000_000, 100_000_000_000));
    }
}
```

| Faker | Parameters | Purpose |
|-------|------------|---------|
| `DuAnInsertDtoFaker()` | none | Tạo DuAn mới |
| `DuAnUpdateModelFaker(id)` | DuAn Id | Cập nhật DuAn hiện có |
| `GoiThauInsertDtoFaker(duAnId)` | DuAn Id | Tạo GoiThau thuộc DuAn |
| `GoiThauUpdateDtoFaker(id)` | GoiThau Id | Cập nhật GoiThau |
| `HopDongInsertDtoFaker(duAnId, goiThauId)` | DuAn + GoiThau Id | Tạo HopDong |
| `HopDongUpdateDtoFaker(id, goiThauId)` | HopDong + GoiThau Id | Cập nhật HopDong |

---

## Unit Tests (Repository & Handler)

### Test Fixtures

| Fixture | Type | Use Case |
|---------|------|----------|
| `SharedSqliteFixture` | In-memory | Fast unit tests (~5-10ms/test), shared DB |
| `IsolatedSqliteFixture` | File-based | Integration tests needing clean state |

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

---

## CLI — test.bat

```bash
test.bat                # Run all tests
test.bat build          # Build only (no tests)
test.bat int            # Integration tests only
test.bat duan           # DuAn tests
test.bat goithau        # GoiThau tests
test.bat hopdong        # HopDong tests
test.bat detailed       # All tests with detailed output
test.bat <filter>       # Custom xUnit filter
```

Các lệnh tương đương `dotnet test`:

| test.bat | dotnet test equivalent |
|----------|------------------------|
| `test.bat` | `dotnet test QLDA.Tests/QLDA.Tests.csproj` |
| `test.bat int` | `dotnet test QLDA.Tests --filter "FullyQualifiedName~Integration" --logger "console;verbosity=detailed"` |
| `test.bat duan` | `dotnet test QLDA.Tests --filter "DuAnControllerTests" --logger "console;verbosity=detailed"` |

---

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

### Examples

```bash
# SQLite (default)
fake.bat da 10                    # Insert 10 DuAn → dev-data.db
fake.bat gt 5                     # Auto-seed DuAn, insert 5 GoiThau
fake.bat hd 3                     # Auto-seed DuAn+GoiThau, insert 3 HopDong
fake.bat all 20                   # Full chain: 20 each (60 total)

# Custom SQLite file
fake.bat da 10 -o test-data.db

# SQL Server schema
fake.bat da 10 --schema dev       # Insert to dev schema
fake.bat all 50 --schema dbo      # Insert to dbo schema

# Clear data
fake.bat clear                    # Clear dev-data.db
fake.bat clear -o test-data.db    # Clear specific file
fake.bat clear --schema dev       # Clear SQL Server dev schema

# Deterministic data
fake.bat da 10 --seed 99999       # Different seed = different data
```

### Auto-seed FK Chain

```
Seed DuAn → auto-seeds DanhMucLoaiDuAn (if not exists)
Seed GoiThau → auto-seeds DuAn (if not enough)
Seed HopDong → auto-seeds GoiThau (if not enough, respects 1:1)
Seed all → creates linked chain: DuAn → GoiThau → HopDong
```

### Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "Default": "DataSource=dev-data.db",
    "SqlServer": "Server=.;Database=QLDA;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

- No `--schema` → `ConnectionStrings:Default` (SQLite)
- With `--schema` → `ConnectionStrings:SqlServer` (SQL Server)

---

## Adding New Integration Tests

### Bước 1: Tạo DTO faker (nếu chưa có)

```csharp
// QLDA.Tests/Fakers/TestDtoFaker.cs
public class MyEntityInsertDtoFaker : Faker<MyEntityInsertDto>
{
    public MyEntityInsertDtoFaker()
    {
        RuleFor(x => x.Name, f => f.Company.CompanyName());
        // ... set required fields
    }
}
```

### Bước 2: Tạo test class

```csharp
// QLDA.Tests/Integration/MyEntityControllerTests.cs
[Collection("WebApi")]
public class MyEntityControllerTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task GetChiTiet_ExistingId_ReturnsOk()
    {
        // Seed data hoặc dùng fixture.SeededXxxId
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsOk()
    {
        var dto = new MyEntityInsertDtoFaker().Generate();
        var response = await AuthedClient.PostAsJsonAsync("/api/my-entity/them-moi", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Bước 3: Thêm alias vào test.bat (optional)

```bat
:myentity
dotnet test %PROJECT% --filter "MyEntityControllerTests" --logger "console;verbosity=detailed"
goto end
```

### Bước 4: Chạy test

```bash
test.bat myentity
```

---

## Troubleshooting

| Issue | Root Cause | Solution |
|-------|------------|----------|
| `CS0122: Program is inaccessible` | `Program` class is internal | Thêm `public partial class Program { }` cuối `Program.cs` |
| `CS9338: Inconsistent accessibility` | Public class inherits internal type | Đảm bảo `Program` là public |
| `NOT NULL constraint failed: CreatedAt` | SQL Server defaults in SQLite | `SqliteTestDbContext` phải replace `SYSDATETIMEOFFSET()` → `STRFTIME(...)` |
| `FOREIGN KEY constraint failed` | DanhMuc not seeded in test DB | `PRAGMA foreign_keys = OFF` khi mở connection |
| `Unable to resolve DbContextOptions<AppDbContext>` | Wrong DI registration | Dùng factory pattern, KHÔNG dùng `AddDbContext<A, B>` |
| `Nullable object must have a value` | DanhMuc navigation property null | `GetDanhSach` cần full DanhMuc — skip hoặc seed thêm |
| `KeyNotFoundException: "Id"` | camelCase serialization | Dùng `el.GetProperty("id")` (lowercase), không phải `"Id"` |
| `Gói thầu đã có hợp đồng` | GoiThau-HopDong 1:1 violation | Tạo GoiThau mới trước khi tạo HopDong |
| `xUnit1000: Test classes must be public` | xUnit analyzer rule | Đảm bảo test class là `public` + `Program` là public |
| `SQLite Error: 'near "max": syntax error'` | nvarchar(max) in SQLite | `SqliteTestDbContext` clears → TEXT |
| `not constant default value` | DefaultValueSql in SQLite | `SqliteTestDbContext` clears SQL Server defaults |
| `relationship has been severed` | GoiThau-HopDong 1:1 | Ensure unique GoiThauId per HopDong |

---

## Test Count

| Category | Tests | Status |
|----------|-------|--------|
| Infrastructure | 6 | Passing |
| DuAn Repository | 6 | Passing |
| GoiThau Repository | 3 | Passing |
| HopDong Repository | 4 | Passing |
| DuAn Handler | 3 | Passing |
| HopDong Handler | 3 | Passing |
| Integration (HTTP) | 12 | All passing |
| **Total** | **37** | **All passing** |

### Integration Test Detail

| Controller | GET chi-tiet | POST create | PUT update | DELETE | Total |
|------------|:---:|:---:|:---:|:---:|:---:|
| DuAn | 1 | 1 | 1 | 1 (soft) | 4 |
| GoiThau | 1 | 1 | 1 | 1 (hard) | 4 |
| HopDong | 1 | 1 | 1 | 1 (soft) | 4 |
| **Total** | **3** | **3** | **3** | **3** | **12** |
