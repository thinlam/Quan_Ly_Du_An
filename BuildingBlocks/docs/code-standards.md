# Code Standards

## File Organization

### Size Limits
- **Maximum 200 lines per code file** - split into smaller modules if exceeded
- **Maximum 800 lines per documentation file** - use subdirectories for large topics
- Use kebab-case for file names with descriptive names

### Project Structure

```
src/
├── BuildingBlocks.Domain/        # 22 files
├── BuildingBlocks.Application/   # 45+ files
├── BuildingBlocks.Persistence/   # 16 files
├── BuildingBlocks.Infrastructure/ # 6 files
└── BuildingBlocks.CrossCutting/   # 17 files

tests/
└── BuildingBlocks.Tests/         # xUnit project
```

## Naming Conventions

### Projects & Namespaces

| Layer | Pattern | Example |
|-------|---------|---------|
| BuildingBlocks | `BuildingBlocks.{Layer}` | `BuildingBlocks.Domain` |
| Subfolder | `BuildingBlocks.{Layer}.{Subfolder}` | `BuildingBlocks.Domain.Entities` |
| Module | `{Module}.{Layer}` | `DVDC.Domain`, `QLDA.Application` |

### Classes

| Type | Convention | Example |
|------|------------|---------|
| Entity | PascalCase | `DanhMuc`, `UserMaster`, `DmDonVi` |
| DTO | PascalCase + Dto suffix | `DanhMucDto`, `ComboBoxDto`, `PagedResultDto` |
| Command | PascalCase + Command suffix | `CreateDanhMucCommand`, `UpdateEntityCommand` |
| Query | PascalCase + Query suffix | `GetDanhMucByIdQuery`, `GetPagedListQuery` |
| Handler | Entity + Action + Handler | `CreateDanhMucCommandHandler` |
| Interface | I + PascalCase | `IRepository`, `IUnitOfWork`, `IDapperRepository` |
| Service | PascalCase + Service suffix | `CrudService`, `UserProvider`, `HistoryQueryService` |
| Constants | PascalCase + Constants suffix | `RoleConstants`, `PermissionConstants` |
| Exception | PascalCase + Exception suffix | `ManagedException`, `NotFoundException` |

### Vietnamese Domain Terms

| Code Term | English Equivalent | Usage |
|-----------|-------------------|-------|
| `DanhMuc` | Catalog/Master Data | Base class for catalog entities |
| `Attachment` | File Attachment | Runtime entity; DB table may be `TepDinhKem` per module config |
| `DonVi` | Organization Unit | Organizational hierarchy |
| `TrangThai` | Status | Status enumeration |
| `MoTa` | Description | Description fields |
| `NgayTao` | Created Date | Audit field |
| `NguoiTao` | Created By | Audit field |

### Database Table Names

Defined in `DatabaseTableNames` constants:
- `TrangThaiYeuCau` - Request status
- `LoaiDuLieu` - Data type
- `UserMaster` - User master table
- `DmDonVi` - Organization unit catalog

## Code Patterns

### Global Usings

Each project should have `GlobalUsing.cs`:

```csharp
// BuildingBlocks.Domain/GlobalUsing.cs
global using System;
global using System.Collections.Generic;
global using BuildingBlocks.Domain.Entities;
global using BuildingBlocks.Domain.Interfaces;
global using BuildingBlocks.CrossCutting.Exceptions;
```

### Dependency Injection

Register services in `DependencyInjection.cs`:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDomain();
        services.AddApplication();
        services.AddPersistence(configuration);
        services.AddInfrastructure();
        services.AddCrossCutting();
        return services;
    }
}
```

### Entity Design

Base entity pattern:

```csharp
public class MyEntity : Entity<Guid>, IAuditable, ISoftDeletable
{
    public string Name { get; set; }
    public string? Description { get; set; }

    // IAuditable
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
```

### DanhMuc Base Class

For catalog/master data entities:

```csharp
public class MyDanhMuc : DanhMuc
{
    // Inherits: Ma, Ten, MoTa, Used, OrderIndex
    // Add custom properties
    public string? AdditionalField { get; set; }
}
```

### CQRS Pattern

#### Command
```csharp
public record CreateMyEntityCommand(string Name, string? Description)
    : IRequest<Guid>;

public class CreateMyEntityCommandValidator
    : AbstractValidator<CreateMyEntityCommand>
{
    public CreateMyEntityCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
```

#### Handler
```csharp
public class CreateMyEntityCommandHandler
    : IRequestHandler<CreateMyEntityCommand, Guid>
{
    private readonly IRepository<MyEntity, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProvider _userProvider;

    public async Task<Guid> Handle(
        CreateMyEntityCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new MyEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _userProvider.UserId
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
```

#### Query
```csharp
public record GetMyEntityByIdQuery(Guid Id)
    : IRequest<MyEntityDto>;

public class GetMyEntityByIdQueryHandler
    : IRequestHandler<GetMyEntityByIdQuery, MyEntityDto>
{
    private readonly IDapperRepository<MyEntity, Guid> _repository;

    public async Task<MyEntityDto> Handle(
        GetMyEntityByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return entity.MapToDto();
    }
}
```

### Repository Pattern

#### GetQueryableSet() Default Filters

**Rule:** `GetQueryableSet()` already filters `IsDeleted = false` AND `Used = true` by default. Do NOT add redundant `!e.IsDeleted` or `e.Used` checks.

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

**Why:** Redundant filters clutter queries and mislead readers. The repository contract guarantees filtering.

**Alternative Methods:**
- `GetOriginalSet()` - Raw DbSet, no filters
- `GetOrderedSet()` - Ordered but no soft-delete/used filters

#### Interface (Domain)
```csharp
public interface IRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);
    Task DeleteAsync(TKey id, CancellationToken cancellationToken);

    // Bulk operations
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task DeleteRangeAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken);
}
```

#### Junction Repository (for entities without IAggregateRoot)
```csharp
// For junction tables (e.g., HopDongPhongBanPhoiHop) that implement IMayHaveDelete
public interface IJunctionRepository<TJunction>
    where TJunction : class, IMayHaveDelete
{
    Task<int> SoftDeleteAsync(
        Expression<Func<TJunction, bool>> predicate,
        CancellationToken cancellationToken = default);
}
```

### Error Handling

Use `ManagedException` for business errors:

```csharp
if (entity == null)
    throw new NotFoundException($"Entity with id {id} not found");

if (entity.Used)
    throw new ManagedException("Cannot delete entity that is in use");

if (!IsValid(input))
    throw new ValidationException("Invalid input data");
```

### Constants Pattern

Group related constants in static classes:

```csharp
public static class RoleConstants
{
    public const string DVDC_TiepNhan = "DVDC_TiepNhan";
    public const string DVDC_XuLy = "DVDC_XuLy";
    public const string QLDA_Admin = "QLDA_Admin";
    public const string QLHD_Manager = "QLHD_Manager";
}

public static class ErrorMessageConstants
{
    public const string NotFound = "Không tìm thấy dữ liệu";
    public const string DuplicateCode = "Mã đã tồn tại trong hệ thống";
    public const string InvalidInput = "Dữ liệu đầu vào không hợp lệ";
}
```

### Namespace Aliases for Disambiguation

When importing multiple namespaces that contain types with the same name (e.g., `RoleConstants` exists in both `BuildingBlocks.Domain.Constants` and module-specific `Domain.Constants`), use namespace aliases to disambiguate:

```csharp
// ❌ Ambiguous reference - causes CS0104 error
using Module.Domain.Constants;
using BuildingBlocks.Domain.Constants;

var roles = new[] { RoleConstants.ModuleSpecificRole, RoleConstants.RegisteredUsers }; // Error!

// ✅ Proper disambiguation with alias
using Module.Domain.Constants;
using BuildingBlocksConstants = BuildingBlocks.Domain.Constants;

var roles = new[] { RoleConstants.ModuleSpecificRole, BuildingBlocksConstants.RoleConstants.RegisteredUsers }; // OK
```

This pattern is required when both module-specific and BuildingBlocks-level constants need to be accessed in the same file.
```

## Best Practices

### Controllers
- Keep controllers thin - delegate to MediatR handlers
- Use `[ApiController]` and `[Route]` attributes
- Return `IActionResult` or `Result<T>` types

### Validation
- Use FluentValidation at command/query level
- Validate business rules in handlers
- Never trust user input

### Logging
- Use Serilog with structured logging
- Log important operations (create, update, delete)
- Include correlation IDs for tracing

### Performance
- Use Dapper for read-heavy queries
- Use EF Core for complex writes
- Implement pagination for list endpoints (MAX_PAGE_SIZE = 100)
- Monitor with `PerformanceBehaviour` (>500ms warning)

### Security
- Never commit secrets to repository
- Use `UserProvider` for user context
- Validate user permissions in handlers
- Use parameterized queries (EF Core/Dapper)

## Module Organization

Each module (DVDC, QLDA, QLHD, NVTT) maintains its own documentation and configuration:

```
{Module}/
├── CLAUDE.md              # Module-specific instructions for Claude
├── docs/                  # Module documentation
│   ├── code-standards.md
│   ├── system-architecture.md
│   └── ...
└── plans/                 # Module plans and reports
    └── reports/
```

### Module Shorthands
| Shorthand | Module | Path |
|-----------|--------|------|
| BB | BuildingBlocks | `MVC/BuildingBlocks/` |
| DVDC | Dịch vụ dùng chung | `MVC/DichVuDungChung/` |
| QLDA | Quản lý dự án | `MVC/QuanLyDuAn/` |
| QLHD | Quản lý hợp đồng | `MVC/QuanLyHopDong/` |
| NVTT | Nhiệm vụ trọng tâm | `MVC/NhiemVuTrongTam/` |

## Git Commit Format

**Format:** `{ModuleShorthand}: {message}`

**Rule:** Commits from any module MUST include the module shorthand prefix.

### Examples
```
BB: Domain - Add new base entity
BB: Application - Add validation behavior
DVDC: Persistence - Update entity configuration
QLDA: Application - Add new query handler
NVTT: Domain - Add new entity
QLHD: Infrastructure - Update Excel helper
```

### Module Shorthands
| Shorthand | Module |
|-----------|--------|
| BB | BuildingBlocks |
| DVDC | Dịch vụ dùng chung |
| QLDA | Quản lý dự án |
| QLHD | Quản lý hợp đồng |
| NVTT | Nhiệm vụ trọng tâm |

## Known Issues

### Files Exceeding Size Limit
| File | Lines | Action Needed |
|------|-------|----------------|
| `ExcelHelper.cs` | 517 | Consider splitting |

### Naming Inconsistencies
| Issue | Location | Resolution |
|-------|----------|------------|
| `SharedKernel.Tests.csproj` naming | Tests project | Rename to `BuildingBlocks.Tests.csproj` |

### Technical Debt
- No unit tests implemented
- CryptographyHelper uses deprecated TripleDES ECB mode
- Potential duplicate logic in AuditInterceptor vs AuditableEntityInterceptor

---

**Last Updated:** 2026-03-27