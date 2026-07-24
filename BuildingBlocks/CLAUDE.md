# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## ⚠️ MUST DO
- **Must create plans in project directories**: Plans must be created in `{PROJECT_ROOT}/plans/` NOT in `~/.claude/plans/`
- **For web searches**: Use MCP providers (`mcp__web-search-prime__web_search_prime` or `mcp__zread__search_doc`) instead of `WebSearch` tool
- **For reading web content**: Use `mcp__web-reader__webReader` instead of `WebFetch` tool
- **For code knowledge graph queries**: Use `mcp__gitnexus__query` for execution flows and `mcp__gitnexus__context` for symbol context
- **Reason**: MCP providers are faster and more reliable than built-in tools
- **When adding files/folders**: Add them to the solution file (.sln) for visibility in Visual Studio
- **Solution folder pattern**: Use solution folders (src, tests, docs, plans, modules) for organization
- **New projects**: Reference BuildingBlocks layers appropriately based on module layer
- **Solution organization**: Modify solution folders (.sln) only - DO NOT move physical folders
- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `gitnexus_impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `gitnexus_detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- **MUST sync validators** when modifying Domain entity properties (adding/removing `[Required]`, `[MaxLength]`, etc.). See "Validator Synchronization" rule.
- When exploring unfamiliar code, use `gitnexus_query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `gitnexus_context({name: "symbolName"})`.

## 🚫 NEVER DO
- **Never write to global Claude config**: Do NOT modify files in `~/.claude/` except via user's explicit request
- **Never create memory files outside project**: Memory files belong in `{PROJECT_ROOT}/.claude/memory/` if needed
- **Never touch global settings**: Do NOT modify `~/.claude/settings.json` or `~/.claude/settings.local.json`
- **Never use global hooks for project-specific tasks**: Project hooks belong in `{PROJECT_ROOT}/.claude/`
- **All outputs stay in project**: Reports, plans, docs → `{PROJECT_ROOT}/plans/`, `{PROJECT_ROOT}/docs/`
- **NEVER** edit a function, class, or method without first running `gitnexus_impact` on it.
- **NEVER** ignore HIGH or CRITICAL risk warnings from impact analysis.
- **NEVER** rename symbols with find-and-replace — use `gitnexus_rename` which understands the call graph.
- **NEVER** commit changes without running `gitnexus_detect_changes()` to check affected scope.

**Project root**: `~\DesktopModules\MVC\BuildingBlocks`

## Build Commands

```bash
# Build solution
dotnet build BuildingBlocks.sln

# Build in Release mode
dotnet build BuildingBlocks.sln -c Release

# Run tests
dotnet test tests/BuildingBlocks.Tests/SharedKernel.Tests.csproj

# Run single test class
dotnet test tests/BuildingBlocks.Tests/SharedKernel.Tests.csproj --filter "FullyQualifiedName~YourTestClass"

# Pack NuGet packages
dotnet pack BuildingBlocks.sln -c Release
```

## Architecture

BuildingBlocks is a shared library for modular monolith DNN DesktopModules (DVDC, QLDA, QLHD, NVTT).

### Layer Dependencies

```
CrossCutting (base, no deps)
    ↓
Domain → CrossCutting, SequentialGuid
    ↓
Persistence → Domain, CrossCutting (EF Core, Dapper)
    ↓
Application → Domain, Persistence, CrossCutting (MediatR, FluentValidation)
    ↓
Infrastructure → CrossCutting (Aspose.Cells)
```

### Project Purposes

| Project                         | Purpose                                             |
| ------------------------------- | --------------------------------------------------- |
| `BuildingBlocks.Domain`         | Entities, interfaces, constants, enums              |
| `BuildingBlocks.Application`    | CQRS (MediatR), services, DTOs, pipeline behaviors  |
| `BuildingBlocks.Persistence`    | Repositories, EF Core configs, interceptors, Dapper |
| `BuildingBlocks.Infrastructure` | DateTime provider, Excel helpers (Aspose)           |
| `BuildingBlocks.CrossCutting`   | Extensions, exceptions, utilities                   |

## Key Patterns

- **CQRS**: MediatR Commands/Queries/Handlers in Application layer
- **Repository**: `IRepository<TEntity, TKey>` with bulk operations
- **Unit of Work**: `IUnitOfWork` with transaction management
- **Pipeline Behaviors**: Validation, Logging, Performance (>500ms warning), Exception
- **Audit Trail**: `AuditInterceptor` + `IAuditable` interface
- **Materialized Path**: Hierarchical entities via `MaterializedPathEntity<TKey>`

## Entity Design

Base entities inherit from:

```csharp
// Standard entity with audit fields
public class MyEntity : Entity<Guid>, IAuditable

// Catalog/master data
public class MyDanhMuc : DanhMuc  // Ma, Ten, MoTa, Used, OrderIndex

// Hierarchical entity
public class MyHierarchy : MaterializedPathEntity<Guid>  // ParentId, Path, Level
```

## Vietnamese Domain Terms

| Term       | English             |
| ---------- | ------------------- |
| DanhMuc    | Catalog/Master Data |
| Attachment | File Attachment (runtime entity; DB table may still be `TepDinhKem`) |
| DonVi      | Organization Unit   |
| TrangThai  | Status              |
| MoTa       | Description         |

## Git Commit Format

**Format:** `{ModuleShorthand}: {message}`

**Rule:** Commits MUST include module shorthand prefix.

```
# Examples:
BB: Domain - Add new base entity
DVDC: Application - Add query handler
QLDA: Persistence - Update entity configuration
NVTT: Domain - Add new entity
QLHD: Infrastructure - Update helper
```

Module shorthands: `BB` (BuildingBlocks), `DVDC`, `QLDA`, `QLHD`, `NVTT`

## Module Organization

Each module (DVDC, QLDA, QLHD, NVTT) maintains its own:
- `CLAUDE.md` - Module-specific instructions
- `docs/` - Module documentation
- `plans/` - Module plans and reports

| Shorthand | Module             | Path                   |
| --------- | ------------------ | ---------------------- |
| BB        | BuildingBlocks     | `MVC/BuildingBlocks/`  |
| DVDC      | Dịch vụ dùng chung | `MVC/DichVuDungChung/` |
| QLDA      | Quản lý dự án      | `MVC/QuanLyDuAn/`      |
| QLHD      | Quản lý hợp đồng   | `MVC/QuanLyHopDong/`   |
| NVTT      | Nhiệm vụ trọng tâm | `MVC/NhiemVuTrongTam/` |

## Plans Structure

Plans are organized by module with each module having its own reports folder:

```
plans/
├── BB/                              # BuildingBlocks core
│   ├── {YYMMDD}-{HHMM}-{slug}/     # Plan folders
│   │   ├── plan.md
│   │   └── phase-*.md
│   └── reports/                     # BB-specific reports
├── QLHD/                            # Quản lý hợp đồng
│   ├── {YYMMDD}-{HHMM}-{slug}/
│   └── reports/
├── DVDC/                            # Dịch vụ dùng chung
├── QLDA/                            # Quản lý dự án
├── NVTT/                            # Nhiệm vụ trọng tâm
└── reports/                         # Cross-module reports (rare)
```

### Module Placement Guide

| Plan Keywords                                                                  | Module | Path          |
| ------------------------------------------------------------------------------ | ------ | ------------- |
| `qlhd`, `hopdong`, `khachhang`, `doanhnghiep`, `duan`, `thutien`, `tiendo`     | QLHD   | `plans/QLHD/` |
| `junction`, `validator`, `danhmuc-*` (generic), `monthyear`, `repository` (BB) | BB     | `plans/BB/`   |
| `dichvu`, `dvdc`                                                               | DVDC   | `plans/DVDC/` |
| `quanlyduan`, `qlda`, `duan` (project-specific)                                | QLDA   | `plans/QLDA/` |
| `nhiemvu`, `nvtt`                                                              | NVTT   | `plans/NVTT/` |

## File Size Limits

- **Code files**: Max 200 lines - split if exceeded
- **Docs**: Max 800 lines
- **Known issue**: `ExcelHelper.cs` (517 lines) needs refactoring

## Dependency Injection

Each layer has `DependencyInjection.cs` with extension methods:

```csharp
services.AddDomain();
services.AddApplication(Assembly.GetExecutingAssembly());
services.AddPersistence(configuration);
services.AddInfrastructure();
```

---

# Rules for Modules

> **IMPORTANT:** These rules apply to ALL modules (DVDC, QLDA, QLHD, NVTT). Each module's `CLAUDE.md` should reference this section for shared rules.

## GetQueryableSet() Default Filters

**Rule:** `GetQueryableSet()` already filters `IsDeleted = false` AND `Used = true` by default. Do NOT add redundant `!e.IsDeleted` or `e.Used` checks.

**Default Behavior:**
```csharp
// Parameterless version applies these filters automatically:
GetQueryableSet() ≡ GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByDescIndex: true)
```

**Pattern:**
```csharp
// ✅ Correct - GetQueryableSet() already filters IsDeleted and Used
var entity = await _repository.GetQueryableSet()
    .Include(t => t.TrangThai)
    .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

// ❌ Wrong - redundant IsDeleted check
var entity = await _repository.GetQueryableSet()
    .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

// ❌ Wrong - redundant Used check
var query = _repository.GetQueryableSet()
    .Where(e => !e.IsDeleted && e.Used);

// ✅ Override defaults when needed
var allRecords = _repository.GetQueryableSet(OnlyUsed: false, OnlyNotDeleted: false);
var onlyDeleted = _repository.GetQueryableSet(OnlyUsed: false, OnlyNotDeleted: false)
    .Where(e => e.IsDeleted);
```

**Why:** Redundant filters clutter queries and mislead readers into thinking additional filtering is happening. The repository contract guarantees filtering.

**Alternative Methods:**
- `GetOriginalSet()` - Raw DbSet, no filters
- `GetOrderedSet()` - Ordered but no soft-delete/used filters

## Index Configuration for DanhMuc Entities

**Rule:** When configuring indexes for `DanhMuc` entities in modules, indexes MUST allow duplicates when filtering by `Used` and `IsDeleted`.

**Why:** DanhMuc entities support soft-delete (`IsDeleted`) and usage filtering (`Used`). Unique indexes must include these columns to allow:
- Recreating deleted records with same `Ma`
- Multiple records with same `Ma` but different `Used`/`IsDeleted` states

**Implementation in EF Core Configuration:**

```csharp
// ✓ Correct - allows duplicate Ma with different Used/IsDeleted states
builder.HasIndex(e => new { e.Ma, e.IsDeleted })
    .IsUnique()
    .HasFilter("[Ma] IS NOT NULL AND [Ma] <> ''");  // Active records only

// ❌ Wrong - will fail when trying to recreate deleted record
builder.HasIndex(e => e.Ma).IsUnique();
```

**Filter Logic:**
- `HasFilter("[Used] = 1 AND [IsDeleted] = 0")` - Enforces uniqueness only for active records
- Allows same `Ma` to exist multiple times if only one is active (`Used=1, IsDeleted=0`)

## DanhMuc Entity Naming Conventions

All catalog/master data entities MUST follow the naming pattern `DanhMuc{Ten}`:

| Pattern        | Status    | Example                                          |
| -------------- | --------- | ------------------------------------------------ |
| `DanhMuc{Ten}` | ✅ Correct | `DanhMucLoaiHopDong`, `DanhMucHinhThucThanhToan` |
| `Dm{Ten}`      | ❌ Wrong   | `DmLoaiHopDong`, `DmHinhThucThanhToan`           |

**Exceptions (legacy tables - keep original names):**
- ✅ `DmDonVi` - Legacy organization unit table (`DM_DONVI`)
- ✅ `UserMaster` - Legacy user table (`USER_MASTER`)

**Applies to:** Entity classes, Configuration classes, Database table names, File names

**Rationale:** Full "DanhMuc" prefix improves readability, makes entity purpose immediately clear, maintains consistency, and helps LLM tools understand code structure.

## Legacy Tables (DmDonVi, USER_MASTER)

**Rule:** Do NOT create navigation properties (FK) to `DmDonVi` or `USER_MASTER` tables.

**Why:** These are legacy tables managed outside EF Core migrations. Direct navigation properties can cause migration conflicts and schema ownership issues.

### Pattern: GetById Query with Join (Recommended)

Use `.Join()` for legacy tables, navigation properties directly in Select for non-legacy entities, and `.ToDto()` for child collections:

```csharp
// Requires: BuildingBlocks.Application.Attachments.Common, BuildingBlocks.Domain.Constants
internal class HopDongGetByIdQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<HopDongGetByIdQuery, HopDongDto>
{
    private readonly IRepository<HopDong, Guid> _repository = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
    private readonly IRepository<DmDonVi, long> _dmDonViRepository = serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    private readonly IRepository<Attachment, Guid> _attachmentRepository = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();

    public async Task<HopDongDto> Handle(HopDongGetByIdQuery request, CancellationToken cancellationToken)
    {
        var attachmentTypes = AttachmentSubquery.ExpandGroupTypes(
            includeSigned: true,
            nameof(EGroupType.HopDong));

        var dto = await _repository.GetQueryableSet()
            .Where(e => e.Id == request.Id)
            .Join(_dmDonViRepository.GetQueryableSet(),
                hd => hd.PhongBanPhuTrachChinhId,
                pbptc => pbptc.Id,
                (hd, pbptc) => new { hd, pbptc })
            .Select(joinedData => new HopDongDto
            {
                Id = joinedData.hd.Id,
                Ten = joinedData.hd.Ten,
                // Legacy table via Join
                TenPhongBan = joinedData.pbptc.TenDonVi,
                // Navigation properties for non-legacy entities (no Include needed)
                TenKhachHang = joinedData.hd.KhachHang!.Ten,
                TenDuAn = joinedData.hd.DuAn!.Ten,
                // Child collections via .ToDto()
                PhongBanPhoiHopIds = joinedData.hd.PhongBanPhoiHops!.Select(p => p.RightId).ToList(),
                ThuTienThucTes = joinedData.hd.ThuTienThucTes!.Select(t => t.ToDto()).ToList(),
                // Attachment uses GroupId + GroupType pattern (subquery)
                TepDinhKems = _attachmentRepository.GetQueryableSet()
                    .Where(f => f.GroupId == joinedData.hd.Id.ToString()
                        && attachmentTypes.Contains(f.GroupType))
                    .Select(e => e.ToDto()).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIfNull(dto, "Không tìm thấy bản ghi");
        return dto;
    }
}
```

### Key Points

| Pattern                       | Usage                                                                             |
| ----------------------------- | --------------------------------------------------------------------------------- |
| `.Join()`                     | Legacy tables (DmDonVi, USER_MASTER)                                              |
| `entity.Navigation!.Property` | Non-legacy navigation properties (no Include needed in Select)                    |
| `.Select(e => e.ToDto())`     | Child collections with ToDto mapping                                              |
| Subquery with `GroupId`       | `Attachment` (uses GroupId instead of FK) + `GroupType` via `AttachmentSubquery` / `SignedGroupTypeHelper` |

### Anti-Pattern: Navigation Properties to Legacy Tables

```csharp
// ❌ Don't do this
public class MyEntity
{
    public long PhongBanId { get; set; }
    public DmDonVi? PhongBan { get; set; }  // Avoid navigation to legacy table
}
```

## Migration Rules for DanhMuc Tables

### Column Order Standard

When creating migrations with `CreateTable` for DanhMuc entities, columns MUST be ordered:

| Order | Column      | Description                |
| ----- | ----------- | -------------------------- |
| 0     | `Id`        | Primary key                |
| 1     | `Ma`        | Code/identifier            |
| 2     | `Ten`       | Name                       |
| 3     | `MoTa`      | Description                |
| 4     | `Used`      | Usage flag                 |
| 5-19  | `{custom}`  | Entity-specific properties |
| 20    | `CreatedBy` | Audit: creator             |
| 21    | `CreatedAt` | Audit: creation time       |
| 22    | `UpdatedBy` | Audit: updater             |
| 23    | `UpdatedAt` | Audit: update time         |
| 24    | `IsDeleted` | Soft delete flag           |
| 25    | `Index`     | Ordering index             |

**Automatic Enforcement:** Column order is automatically enforced via `HasColumnOrder()` in `ConfigureForDanhMuc()` extension method (`src/BuildingBlocks.Persistence/Configurations/ConfigurationExtension.cs`).

**For new DanhMuc entities with custom properties:**

```csharp
public class DanhMucCustomConfiguration : AggregateRootConfiguration<DanhMucCustom>
{
    public override void Configure(EntityTypeBuilder<DanhMucCustom> builder)
    {
        builder.ToTable("DanhMucCustom");
        builder.ConfigureForDanhMuc();

        // Entity-specific property (order 5+)
        builder.Property(e => e.CustomField)
            .HasColumnOrder(5)
            .HasMaxLength(100);
    }
}
```

## Decimal Property Configuration

**Rule:** All `decimal` properties in entities MUST have explicit precision configured via `HasPrecision()`.

**Why:** Without explicit precision, EF Core defaults to (18,2) but warns about potential truncation. Explicit configuration ensures:
- Consistent column types across migrations
- No silent data truncation
- Clear intent for currency/percentage fields

**Precision Guidelines:**

| Property Type          | Precision | Scale | Example               |
| ---------------------- | --------- | ----- | --------------------- |
| Currency (VND amounts) | 18        | 2     | `GiaTri`, `TienThue`  |
| Percentage             | 5         | 2     | `PhanTram` (0-100.00) |

**Implementation:**

```csharp
public class MyEntityConfiguration : AggregateRootConfiguration<MyEntity>
{
    public override void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        // Currency values - 18 digits total, 2 after decimal
        builder.Property(e => e.GiaTri).HasPrecision(18, 2);

        // Percentages - 5 digits total, 2 after decimal (supports 0.00 to 999.99)
        builder.Property(e => e.PhanTram).HasPrecision(5, 2);
    }
}
```

## Validator Synchronization

**Rule:** When modifying entity properties in Domain layer, MUST update corresponding FluentValidation validators.

**Triggered when:**
- Adding/removing `[Required]` attribute from property
- Adding/removing `[MaxLength]` or `[StringLength]` attributes
- Changing property type (e.g., `int` to `int?`)
- Adding new required properties to existing entities

**Implementation:**

| Entity Change                     | Validator Action                                |
| --------------------------------- | ----------------------------------------------- |
| Add `[Required]` to property      | Add `RuleFor(x => x.Model.Property).NotEmpty()` |
| Remove `[Required]` from property | Remove or make validation conditional           |
| Add `[MaxLength(n)]`              | Add `.MaximumLength(n)` to rule                 |
| New required property             | Add validation rule to Insert/Update validators |

**Pattern:**

```csharp
// Domain entity
public class HopDong : Entity<Guid>
{
    [Required] public string Ten { get; set; } = string.Empty;
    [Required] public int LoaiHopDongId { get; set; }
}

// Validator (in Application layer)
public class HopDongInsertCommandValidator : AbstractValidator<HopDongInsertCommand>
{
    public HopDongInsertCommandValidator()
    {
        RuleFor(x => x.Model.Ten)
            .NotEmpty().WithMessage("Tên hợp đồng là bắt buộc")
            .MaximumLength(500).WithMessage("Tên hợp đồng không được vượt quá 500 ký tự");

        RuleFor(x => x.Model.LoaiHopDongId)
            .NotEmpty().WithMessage("Loại hợp đồng là bắt buộc");
    }
}
```

**Files to check:**
- `{Module}.Domain/Entities/{Entity}.cs` - Entity with validation attributes
- `{Module}.Application/{Entities}/Validators/{Entity}InsertCommandValidator.cs`
- `{Module}.Application/{Entities}/Validators/{Entity}UpdateCommandValidator.cs`

## Migration Workflow for Modules

**Rule:** When running EF Core migrations for any module, always use the `{ModuleName}.Migrator` project as both the startup project and migrations output.

**Command Pattern:**

```bash
# From the module's Persistence or Migrator directory
cd modules/{ModuleName}/{ModuleName}.Migrator

# Create new migration
dotnet ef migrations add {MigrationName} --startup-project . --project ./{ModuleName}.Migrator

# Or from Persistence directory
cd modules/{ModuleName}/{ModuleName}.Persistence
dotnet ef migrations add {MigrationName} --startup-project ../{ModuleName}.Migrator --project ../{ModuleName}.Migrator
```

**Why:** The Migrator project is configured as the migrations assembly in DbContext options. Using it ensures migrations are generated in the correct location.

**Example for QLHD module:**

```bash
cd modules/QLHD/QLHD.Persistence
dotnet ef migrations add AddKhachHangSeedData --startup-project ../QLHD.Migrator --project ../QLHD.Migrator
```

## Seed Data Rules for DanhMuc Entities

### CreatedAt Must Use UTC

**Rule:** All seed data MUST set `CreatedAt` with UTC timezone (`TimeSpan.Zero`).

**Why:** EF Core generates migrations with local timezone (+7:00) if not explicitly set, causing inconsistent datetime values.

**Implementation:**

```csharp
public class DanhMucCustomConfiguration : AggregateRootConfiguration<DanhMucCustom>
{
    // UTC timestamp for all seed data
    private static readonly DateTimeOffset SeedCreatedAt = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public override void Configure(EntityTypeBuilder<DanhMucCustom> builder)
    {
        builder.HasData(
            new DanhMucCustom
            {
                Id = 1,
                Ma = "C0001",
                Ten = "Sample",
                CreatedAt = SeedCreatedAt  // ✅ Explicit UTC timestamp
            }
        );
    }
}
```

### IsDefault Rule

**Rule:** Every seed data set MUST have exactly one `IsDefault = true` record per group.

**Scenarios:**

| Entity Type                    | Grouping             | Requirement                       |
| ------------------------------ | -------------------- | --------------------------------- |
| Simple DanhMuc                 | None                 | Exactly 1 default in entire table |
| DanhMuc with `LoaiTrangThaiId` | By `LoaiTrangThaiId` | Exactly 1 default per group       |

**Examples:**

```csharp
// ✅ Simple DanhMuc - one default for entire table
builder.HasData(
    new DanhMucLoaiHopDong { Id = 1, ..., IsDefault = false },
    new DanhMucLoaiHopDong { Id = 2, ..., IsDefault = false },
    new DanhMucLoaiHopDong { Id = 7, ..., IsDefault = true }  // Exactly one default
);

// ✅ Grouped DanhMuc - one default per group (LoaiTrangThaiId)
builder.HasData(
    // Group: LoaiTrangThaiId = 1 (Hợp đồng)
    new DanhMucTrangThai { Id = 1, LoaiTrangThaiId = 1, IsDefault = true },  // Default for group 1
    new DanhMucTrangThai { Id = 2, LoaiTrangThaiId = 1, IsDefault = false },
    // Group: LoaiTrangThaiId = 2 (Kế hoạch)
    new DanhMucTrangThai { Id = 5, LoaiTrangThaiId = 2, IsDefault = true },  // Default for group 2
    new DanhMucTrangThai { Id = 6, LoaiTrangThaiId = 2, IsDefault = false },
    // Group: LoaiTrangThaiId = 3 (Cuộc họp)
    new DanhMucTrangThai { Id = 15, LoaiTrangThaiId = 3, IsDefault = true }  // Default for group 3
);
```

**Unique Index Enforcement:**

```csharp
// Simple DanhMuc - unique IsDefault across table
builder.HasIndex(e => e.IsDefault)
    .HasFilter("[IsDefault] = 1")
    .IsUnique();

// Grouped DanhMuc - unique IsDefault per group
builder.HasIndex(e => new { e.LoaiTrangThaiId, e.IsDefault })
    .HasFilter("[IsDefault] = 1")
    .IsUnique();
```

---

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **BuildingBlocks** (5495 symbols, 13508 relationships, 94 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> If any GitNexus tool warns the index is stale, run `npx gitnexus analyze` in terminal first.

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `gitnexus_impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `gitnexus_detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `gitnexus_query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `gitnexus_context({name: "symbolName"})`.

## When Debugging

1. `gitnexus_query({query: "<error or symptom>"})` — find execution flows related to the issue
2. `gitnexus_context({name: "<suspect function>"})` — see all callers, callees, and process participation
3. `READ gitnexus://repo/BuildingBlocks/process/{processName}` — trace the full execution flow step by step
4. For regressions: `gitnexus_detect_changes({scope: "compare", base_ref: "main"})` — see what your branch changed

## When Refactoring

- **Renaming**: MUST use `gitnexus_rename({symbol_name: "old", new_name: "new", dry_run: true})` first. Review the preview — graph edits are safe, text_search edits need manual review. Then run with `dry_run: false`.
- **Extracting/Splitting**: MUST run `gitnexus_context({name: "target"})` to see all incoming/outgoing refs, then `gitnexus_impact({target: "target", direction: "upstream"})` to find all external callers before moving code.
- After any refactor: run `gitnexus_detect_changes({scope: "all"})` to verify only expected files changed.

## Never Do

- NEVER edit a function, class, or method without first running `gitnexus_impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `gitnexus_rename` which understands the call graph.
- NEVER commit changes without running `gitnexus_detect_changes()` to check affected scope.

## Tools Quick Reference

| Tool | When to use | Command |
|------|-------------|---------|
| `query` | Find code by concept | `gitnexus_query({query: "auth validation"})` |
| `context` | 360-degree view of one symbol | `gitnexus_context({name: "validateUser"})` |
| `impact` | Blast radius before editing | `gitnexus_impact({target: "X", direction: "upstream"})` |
| `detect_changes` | Pre-commit scope check | `gitnexus_detect_changes({scope: "staged"})` |
| `rename` | Safe multi-file rename | `gitnexus_rename({symbol_name: "old", new_name: "new", dry_run: true})` |
| `cypher` | Custom graph queries | `gitnexus_cypher({query: "MATCH ..."})` |

## Impact Risk Levels

| Depth | Meaning | Action |
|-------|---------|--------|
| d=1 | WILL BREAK — direct callers/importers | MUST update these |
| d=2 | LIKELY AFFECTED — indirect deps | Should test |
| d=3 | MAY NEED TESTING — transitive | Test if critical path |

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/BuildingBlocks/context` | Codebase overview, check index freshness |
| `gitnexus://repo/BuildingBlocks/clusters` | All functional areas |
| `gitnexus://repo/BuildingBlocks/processes` | All execution flows |
| `gitnexus://repo/BuildingBlocks/process/{name}` | Step-by-step execution trace |

## Self-Check Before Finishing

Before completing any code modification task, verify:
1. `gitnexus_impact` was run for all modified symbols
2. No HIGH/CRITICAL risk warnings were ignored
3. `gitnexus_detect_changes()` confirms changes match expected scope
4. All d=1 (WILL BREAK) dependents were updated

## Keeping the Index Fresh

After committing code changes, the GitNexus index becomes stale. Re-run analyze to update it:

```bash
npx gitnexus analyze
```

If the index previously included embeddings, preserve them by adding `--embeddings`:

```bash
npx gitnexus analyze --embeddings
```

To check whether embeddings exist, inspect `.gitnexus/meta.json` — the `stats.embeddings` field shows the count (0 means no embeddings). **Running analyze without `--embeddings` will delete any previously generated embeddings.**

> Claude Code users: A PostToolUse hook handles this automatically after `git commit` and `git merge`.

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->
