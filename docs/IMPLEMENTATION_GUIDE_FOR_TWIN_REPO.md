# Implementation Guide for Twin Repository
## QLDA Project Management System - Feature Replication

**Document Version**: 1.0  
**Generated**: April 22, 2026  
**Purpose**: Comprehensive guide for replicating QLDA implementations in another repository  
**Status**: ✅ Ready for Twin Repo Implementation

---

## 📋 Table of Contents

1. [Quick Reference - What's Been Implemented](#quick-reference)
2. [Architecture Patterns Used](#architecture-patterns)
3. [Layer-by-Layer Implementation](#layer-by-layer)
4. [Feature Implementation Patterns](#feature-patterns)
5. [Database Schema](#database-schema)
6. [Code Examples & Templates](#code-templates)
7. [Step-by-Step Implementation Checklist](#implementation-checklist)
8. [Testing Strategy](#testing-strategy)
9. [Common Pitfalls & Solutions](#pitfalls)
10. [Deployment Considerations](#deployment)

---

## 🎯 Quick Reference - What's Been Implemented {#quick-reference}

### Fully Implemented Features

| Feature | Module | Status | Key Components |
|---------|--------|--------|-----------------|
| **Project Management** | DuAn | ✅ Complete | CRUD, hierarchical structure, status tracking |
| **Bid Package Management** | GoiThau | ✅ Complete | Lifecycle management, tender documents |
| **Contract Management** | HopDong | ✅ Complete | CRUD, amendments, compliance tracking |
| **Project Steps** | DuAnBuoc | ✅ Complete | Task tracking, progress monitoring |
| **Budget Reporting** | BaoCao | ✅ Complete | Report API with filtering, pagination, aggregation |
| **Financial Management** | ThanhToan/TamUng | ✅ Complete | Payments, advances, settlements |
| **Master Data** | DanhMuc* | ✅ Complete | Category management, enumerations |
| **Authentication** | Auth | ✅ Complete | JWT Bearer, login, refresh, logout |
| **User Management** | Users | ✅ Complete | RBAC, department assignment |

### Technology Stack Implemented

```
Backend:        .NET 8.0, ASP.NET Core Web API
Database:       SQL Server (EF Core Code-First + Dapper)
CQRS Pattern:   MediatR with Command/Query separation
Validation:     FluentValidation on all inputs
Authentication: JWT Bearer Token
ORM:            Entity Framework Core 8.0
Mapping:        AutoMapper
Logging:        Serilog/ILogger
Testing:        xUnit ready (can add tests)
Documentation:  Swagger/OpenAPI auto-generated
```

---

## 🏗️ Architecture Patterns Used {#architecture-patterns}

### 1. Clean Architecture Principle

The solution follows a **strict dependency inward rule**:

```
┌─────────────────────────────────────────────────────┐
│                  WebApi Layer                        │
│  (Controllers, Middleware, Configuration, Filters)  │
└────────────────────┬────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────┐
│              Application Layer                       │
│  (CQRS: Commands/Queries, Handlers, DTOs, Validators)
└────────────────┬──────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────────┐
│              Domain Layer (Core)                      │
│   (Entities, Value Objects, Interfaces, Events)    │
└─────────────────────────────────────────────────────┘
                 ↑                    ↑
                 │                    │
    ┌────────────┴─────────┐    ┌─────┴──────────┐
    │                      │    │                │
┌───────────┐      ┌──────────────┐     ┌───────────────┐
│Infrastructure│  │ Persistence     │     │  Tests        │
│(Services)  │  │ (DB, Repos)    │     │  (xUnit)      │
└────────────┘    └───────────────┘     └───────────────┘
```

**Dependency Flow**:
- `WebApi` → depends on `Application`
- `Application` → depends on `Domain`
- `Infrastructure` → depends on `Application` & `Domain`
- `Persistence` → depends on `Domain`
- `Tests` → depend on inner layers

### 2. CQRS with MediatR

Every business operation follows either Command (write) or Query (read) pattern:

**Commands** (State Modification):
```csharp
public class CreateDuAnCommand : IRequest<Guid>
{
    public string TenDuAn { get; set; }
    public int ThoiGianKhoiCong { get; set; }
    // ... more properties
}

public class CreateDuAnCommandHandler : IRequestHandler<CreateDuAnCommand, Guid>
{
    public async Task<Guid> Handle(CreateDuAnCommand request, CancellationToken cancellationToken)
    {
        // Business logic here
    }
}
```

**Queries** (Data Retrieval):
```csharp
public class GetDuAnByIdQuery : IRequest<DuAnDto>
{
    public Guid Id { get; set; }
}

public class GetDuAnByIdQueryHandler : IRequestHandler<GetDuAnByIdQuery, DuAnDto>
{
    public async Task<DuAnDto> Handle(GetDuAnByIdQuery request, CancellationToken cancellationToken)
    {
        // Query logic here
    }
}
```

### 3. Repository Pattern + Unit of Work

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    IQueryable<T> GetQueryable();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IUnitOfWork
{
    IRepository<DuAn> DuAnRepository { get; }
    IRepository<GoiThau> GoiThauRepository { get; }
    Task<int> SaveChangesAsync();
}
```

### 4. Soft Delete Pattern

All entities inherit from `BaseEntity` with soft delete fields:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string LastModifiedBy { get; set; }
    public DateTime? DeletedAt { get; set; }  // ← Soft delete
    public bool IsDeleted { get; set; }       // ← Soft delete flag
}
```

**In DbContext**:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Global query filter: exclude soft-deleted records by default
    modelBuilder.Entity<DuAn>()
        .HasQueryFilter(x => !x.IsDeleted);
    
    modelBuilder.Entity<GoiThau>()
        .HasQueryFilter(x => !x.IsDeleted);
    // ... etc
}
```

### 5. MediatR Pipeline Behaviors

Cross-cutting concerns handled transparently:

```csharp
// Logging Behavior
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing {CommandName}", typeof(TRequest).Name);
        var result = await next();
        _logger.LogInformation("Completed {CommandName}", typeof(TRequest).Name);
        return result;
    }
}

// Validation Behavior
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = await _validators.ValidateAsync(context, cancellationToken);
        
        if (failures.Any())
            throw new ValidationException(failures);
        
        return await next();
    }
}
```

---

## 🔧 Layer-by-Layer Implementation {#layer-by-layer}

### Domain Layer (`QLDA.Domain`)

**Purpose**: Pure business logic with zero external dependencies

**Key Files**:
```
QLDA.Domain/
├── Entities/
│   ├── BaseEntity.cs              # Base class for all entities
│   ├── DuAn.cs                    # Project entity
│   ├── GoiThau.cs                 # Bid package entity
│   ├── HopDong.cs                 # Contract entity
│   ├── DuAnBuoc.cs                # Project steps
│   ├── DuToan.cs                  # Budget entity
│   ├── NghiemThu.cs               # Acceptance entity
│   └── DanhMuc*.cs                # Master data entities
├── Enums/
│   ├── TrangThaiDuAnEnum.cs       # Project status
│   └── ...other enums
├── Interfaces/
│   ├── IRepository.cs             # Repository interface
│   ├── IUnitOfWork.cs             # UoW interface
│   └── Domain service interfaces
├── Services/
│   └── Domain business logic
└── Events/
    └── Domain events
```

**Sample Entity Implementation**:
```csharp
public class DuAn : BaseEntity, IAggregateRoot
{
    public string TenDuAn { get; set; }
    public string MaDuAn { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public int? ThoiGianHoanThanh { get; set; }
    public decimal? KhaiToanKinhPhi { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public int? LoaiDuAnId { get; set; }
    public int? HinhThucDauTuId { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public Guid? ParentId { get; set; }  // For hierarchical structure
    
    // Navigation properties
    public virtual ICollection<DuAnBuoc> BuocHienTai { get; set; }
    public virtual ICollection<DuToan> DuToans { get; set; }
    public virtual ICollection<NghiemThu> NghiemThus { get; set; }
}
```

### Application Layer (`QLDA.Application`)

**Purpose**: Orchestrate domain logic to fulfill use cases

**Feature Folder Structure**:
```
QLDA.Application/
├── DuAns/
│   ├── Commands/
│   │   ├── CreateDuAn/
│   │   │   ├── CreateDuAnCommand.cs
│   │   │   └── CreateDuAnCommandHandler.cs
│   │   ├── UpdateDuAn/
│   │   │   ├── UpdateDuAnCommand.cs
│   │   │   └── UpdateDuAnCommandHandler.cs
│   │   └── DeleteDuAn/
│   │       ├── DeleteDuAnCommand.cs
│   │       └── DeleteDuAnCommandHandler.cs
│   ├── Queries/
│   │   ├── GetDuAnById/
│   │   │   ├── GetDuAnByIdQuery.cs
│   │   │   └── GetDuAnByIdQueryHandler.cs
│   │   ├── GetDuAnList/
│   │   │   ├── GetDuAnListQuery.cs
│   │   │   └── GetDuAnListQueryHandler.cs
│   │   └── BaoCaoDuAn/
│   │       ├── BaoCaoDuAnGetDanhSachQuery.cs
│   │       └── BaoCaoDuAnGetDanhSachQueryHandler.cs
│   ├── DTOs/
│   │   ├── DuAnDto.cs
│   │   ├── CreateDuAnRequest.cs
│   │   ├── UpdateDuAnRequest.cs
│   │   ├── BaoCaoDuAnSearchDto.cs
│   │   └── BaoCaoDuAnDto.cs
│   └── Validators/
│       ├── CreateDuAnValidator.cs
│       ├── UpdateDuAnValidator.cs
│       └── BaoCaoDuAnSearchValidator.cs
├── GoiThaus/         # Similar structure
├── HopDongs/         # Similar structure
├── Common/
│   ├── Behaviors/
│   │   ├── LoggingBehavior.cs
│   │   ├── ValidationBehavior.cs
│   │   ├── ExceptionHandlingBehavior.cs
│   │   └── PerformanceBehavior.cs
│   ├── DTOs/
│   │   ├── ResultApi.cs           # Wrapper for all responses
│   │   ├── PaginatedList.cs       # Pagination
│   │   └── CommonSearchDto.cs     # Base search DTO
│   ├── Interfaces/
│   │   ├── IUserProvider.cs
│   │   └── ICrudService.cs
│   ├── Mappings/
│   │   ├── MappingProfile.cs
│   │   └── AutoMapperConfig.cs
│   ├── Extensions/
│   │   ├── QueryableExtensions.cs # WhereIf, ToPaginatedList
│   │   └── EnumExtensions.cs
│   └── Constants/
│       └── ErrorMessages.cs
└── DependencyInjection.cs         # Register all MediatR handlers
```

**Key Files to Create for Each Feature**:

1. **DTO Files** (`CreateXxxRequest.cs`, `UpdateXxxRequest.cs`, `XxxDto.cs`, `XxxSearchDto.cs`):
```csharp
public class CreateDuAnRequest
{
    public string TenDuAn { get; set; }
    public string MaDuAn { get; set; }
    [Range(2020, 2100)]
    public int? ThoiGianKhoiCong { get; set; }
    public decimal? KhaiToanKinhPhi { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
}

public class DuAnSearchDto : CommonSearchDto
{
    public string TenDuAn { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public int? ThoiGianHoanThanh { get; set; }
}

public class DuAnDto
{
    public Guid Id { get; set; }
    public string TenDuAn { get; set; }
    public string MaDuAn { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

2. **Command Handler**:
```csharp
public class CreateDuAnCommand : IRequest<Guid>
{
    public string TenDuAn { get; set; }
    public string MaDuAn { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
}

public class CreateDuAnCommandHandler : IRequestHandler<CreateDuAnCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserProvider _userProvider;
    
    public CreateDuAnCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IUserProvider userProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userProvider = userProvider;
    }
    
    public async Task<Guid> Handle(CreateDuAnCommand request, CancellationToken cancellationToken)
    {
        var entity = new DuAn
        {
            Id = Guid.NewGuid(),
            TenDuAn = request.TenDuAn,
            MaDuAn = request.MaDuAn,
            ThoiGianKhoiCong = request.ThoiGianKhoiCong,
            DonViPhuTrachChinhId = request.DonViPhuTrachChinhId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _userProvider.GetUserId(),
        };
        
        await _unitOfWork.DuAnRepository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        
        return entity.Id;
    }
}
```

3. **Query Handler**:
```csharp
public class GetDuAnListQuery : IRequest<PaginatedList<DuAnDto>>
{
    public DuAnSearchDto SearchDto { get; set; }
}

public class GetDuAnListQueryHandler : IRequestHandler<GetDuAnListQuery, PaginatedList<DuAnDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public GetDuAnListQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<PaginatedList<DuAnDto>> Handle(
        GetDuAnListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _unitOfWork.DuAnRepository.GetQueryable()
            .AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(request.SearchDto.TenDuAn),
                x => x.TenDuAn.Contains(request.SearchDto.TenDuAn))
            .WhereIf(request.SearchDto.ThoiGianKhoiCong.HasValue,
                x => x.ThoiGianKhoiCong == request.SearchDto.ThoiGianKhoiCong);
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip(request.SearchDto.PageIndex * request.SearchDto.PageSize)
            .Take(request.SearchDto.PageSize)
            .Select(x => _mapper.Map<DuAnDto>(x))
            .ToListAsync(cancellationToken);
        
        return new PaginatedList<DuAnDto>(
            items,
            totalCount,
            request.SearchDto.PageIndex,
            request.SearchDto.PageSize);
    }
}
```

4. **Validator**:
```csharp
public class CreateDuAnValidator : AbstractValidator<CreateDuAnCommand>
{
    public CreateDuAnValidator()
    {
        RuleFor(x => x.TenDuAn)
            .NotEmpty().WithMessage("Tên dự án không được để trống")
            .MaximumLength(500).WithMessage("Tên dự án không vượt quá 500 ký tự");
        
        RuleFor(x => x.ThoiGianKhoiCong)
            .GreaterThanOrEqualTo(2020).WithMessage("Năm khởi công không được nhỏ hơn 2020")
            .LessThanOrEqualTo(2100).WithMessage("Năm khởi công không được lớn hơn 2100");
        
        RuleFor(x => x.KhaiToanKinhPhi)
            .GreaterThan(0).WithMessage("Kinh phí phải lớn hơn 0")
            .When(x => x.KhaiToanKinhPhi.HasValue);
    }
}
```

### Persistence Layer (`QLDA.Persistence`)

**Purpose**: Data access, EF Core configuration, migrations

**Key Files**:
```
QLDA.Persistence/
├── Context/
│   └── ApplicationDbContext.cs      # Main DbContext
├── Repositories/
│   ├── GenericRepository.cs         # Generic CRUD implementation
│   ├── DuAnRepository.cs            # Feature-specific if needed
│   └── UnitOfWork.cs                # Coordinates repositories
├── Configurations/
│   ├── DuAnConfiguration.cs
│   ├── GoiThauConfiguration.cs
│   └── ...other entity configs
├── Migrations/
│   ├── 20260101000001_InitialCreate.cs
│   ├── 20260115000002_AddDuAnBuoc.cs
│   └── ...incremental migrations
├── SeedData/
│   └── InitialDataSeeder.cs
└── DependencyInjection.cs           # Register DbContext, Repositories
```

**DbContext Implementation**:
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    
    public DbSet<DuAn> DuAn { get; set; }
    public DbSet<GoiThau> GoiThau { get; set; }
    public DbSet<HopDong> HopDong { get; set; }
    public DbSet<DuAnBuoc> DuAnBuoc { get; set; }
    public DbSet<DuToan> DuToan { get; set; }
    public DbSet<NghiemThu> NghiemThu { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Soft delete filters
        modelBuilder.Entity<DuAn>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<GoiThau>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<HopDong>().HasQueryFilter(x => !x.IsDeleted);
        
        // Entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set audit fields before saving
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = GetCurrentUserId();
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = GetCurrentUserId();
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private string GetCurrentUserId() => _userProvider?.GetUserId() ?? "system";
}
```

**Entity Configuration Example**:
```csharp
public class DuAnConfiguration : IEntityTypeConfiguration<DuAn>
{
    public void Configure(EntityTypeBuilder<DuAn> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.TenDuAn)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(x => x.KhaiToanKinhPhi)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
        
        // Relationships
        builder.HasMany(x => x.BuocHienTai)
            .WithOne(x => x.DuAn)
            .HasForeignKey(x => x.DuAnId);
        
        builder.HasMany(x => x.DuToans)
            .WithOne(x => x.DuAn)
            .HasForeignKey(x => x.DuAnId);
        
        // Indexes
        builder.HasIndex(x => x.TenDuAn);
        builder.HasIndex(x => x.MaDuAn).IsUnique();
        builder.HasIndex(x => x.DonViPhuTrachChinhId);
    }
}
```

**Repository Implementation**:
```csharp
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual IQueryable<T> GetQueryable() => _dbSet.AsQueryable();
    
    public async Task<T> GetByIdAsync(Guid id)
        => await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    
    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.ToListAsync();
    
    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);
    
    public void Update(T entity)
        => _dbSet.Update(entity);
    
    public void Delete(T entity)
        => _dbSet.Remove(entity);
}

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly ApplicationDbContext _context;
    private GenericRepository<DuAn> _duAnRepository;
    private GenericRepository<GoiThau> _goiThauRepository;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public IRepository<DuAn> DuAnRepository
        => _duAnRepository ??= new GenericRepository<DuAn>(_context);
    
    public IRepository<GoiThau> GoiThauRepository
        => _goiThauRepository ??= new GenericRepository<GoiThau>(_context);
    
    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
    
    public async ValueTask DisposeAsync()
        => await _context.DisposeAsync();
}
```

### WebApi Layer (`QLDA.WebApi`)

**Purpose**: HTTP endpoints, authentication, middleware, configuration

**Key Files**:
```
QLDA.WebApi/
├── Controllers/
│   ├── DuAnController.cs
│   ├── GoiThauController.cs
│   ├── HopDongController.cs
│   ├── AuthController.cs
│   └── HealthController.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   ├── JwtMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Filters/
│   ├── ExceptionFilter.cs
│   └── AuthorizationFilter.cs
├── ConfigurationOptions/
│   ├── JwtSettings.cs
│   ├── ApiSettings.cs
│   └── DatabaseSettings.cs
├── Program.cs                     # Startup configuration
├── appsettings.json
├── appsettings.Development.json
└── appsettings.Production.json
```

**Controller Implementation**:
```csharp
[ApiController]
[Route("api/du-an")]
[Authorize]  // Require authentication
public class DuAnController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public DuAnController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Get paginated list of projects
    /// </summary>
    [HttpGet("danh-sach")]
    public async Task<ResultApi<PaginatedList<DuAnDto>>> GetList(
        [FromQuery] DuAnSearchDto searchDto,
        CancellationToken cancellationToken)
    {
        var query = new GetDuAnListQuery { SearchDto = searchDto };
        var result = await _mediator.Send(query, cancellationToken);
        return ResultApi<PaginatedList<DuAnDto>>.Success(result);
    }
    
    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id:guid}/chi-tiet")]
    public async Task<ResultApi<DuAnDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetDuAnByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return ResultApi<DuAnDto>.Success(result);
    }
    
    /// <summary>
    /// Create new project
    /// </summary>
    [HttpPost("them-moi")]
    public async Task<ResultApi<Guid>> Create(
        [FromBody] CreateDuAnRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDuAnCommand { /* map from request */ };
        var result = await _mediator.Send(command, cancellationToken);
        return ResultApi<Guid>.SuccessCreated(result);
    }
    
    /// <summary>
    /// Update existing project
    /// </summary>
    [HttpPut("cap-nhat")]
    public async Task<ResultApi> Update(
        [FromBody] UpdateDuAnRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDuAnCommand { /* map from request */ };
        await _mediator.Send(command, cancellationToken);
        return ResultApi.Success();
    }
    
    /// <summary>
    /// Budget report with filtering
    /// </summary>
    [HttpGet("bao-cao-du-toan")]
    public async Task<ResultApi<PaginatedList<BaoCaoDuAnDto>>> GetBaoCaoDuToan(
        [FromQuery] BaoCaoDuAnSearchDto searchDto,
        CancellationToken cancellationToken)
    {
        var query = new BaoCaoDuAnGetDanhSachQuery { SearchDto = searchDto };
        var result = await _mediator.Send(query, cancellationToken);
        return ResultApi<PaginatedList<BaoCaoDuAnDto>>.Success(result);
    }
}
```

**Program.cs Configuration**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
var services = builder.Services;
var config = builder.Configuration;

// Database
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// MediatR
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(CreateDuAnCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// AutoMapper
services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Validation
services.AddFluentValidation(x =>
    x.RegisterValidatorsFromAssemblyContaining<CreateDuAnValidator>());

// JWT Authentication
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"])),
            ValidateIssuer = true,
            ValidIssuer = config["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = config["JwtSettings:Audience"],
        };
    });

// Controllers
services.AddControllers();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "QLDA API", Version = "v1" });
    
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "JWT Bearer Token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    };
    
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

// Middleware
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "QLDA API v1"));
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 📋 Feature Implementation Patterns {#feature-patterns}

### Pattern 1: Simple CRUD Operations

**Use Case**: Create, Read, Update, Delete for a single entity

**Files to Create**: 5-6 files per feature
```
Feature/
├── Commands/
│   ├── Create{Feature}/
│   │   ├── Create{Feature}Command.cs
│   │   └── Create{Feature}CommandHandler.cs
│   ├── Update{Feature}/
│   │   ├── Update{Feature}Command.cs
│   │   └── Update{Feature}CommandHandler.cs
│   └── Delete{Feature}/
│       ├── Delete{Feature}Command.cs
│       └── Delete{Feature}CommandHandler.cs
├── Queries/
│   ├── Get{Feature}ById/
│   │   ├── Get{Feature}ByIdQuery.cs
│   │   └── Get{Feature}ByIdQueryHandler.cs
│   └── Get{Feature}List/
│       ├── Get{Feature}ListQuery.cs
│       └── Get{Feature}ListQueryHandler.cs
├── DTOs/
│   ├── {Feature}Dto.cs
│   ├── Create{Feature}Request.cs
│   ├── Update{Feature}Request.cs
│   └── {Feature}SearchDto.cs
└── Validators/
    ├── Create{Feature}Validator.cs
    └── Update{Feature}Validator.cs
```

### Pattern 2: Complex Reporting/Analytics

**Use Case**: Aggregated data with multiple filtering options and joins

**Files to Create**: 3-4 files
```
Feature/
├── Queries/
│   └── BaoCao{Feature}/
│       ├── BaoCao{Feature}GetDanhSachQuery.cs
│       └── BaoCao{Feature}GetDanhSachQueryHandler.cs
├── DTOs/
│   ├── BaoCao{Feature}SearchDto.cs
│   └── BaoCao{Feature}Dto.cs
└── (No Commands or Validators for read-only reports)
```

**Key Implementation Details**:
- Fetch paginated main entity
- Load ALL related data for page results (prevents N+1)
- Aggregate/calculate derived fields in-memory
- Return paginated with calculated fields

```csharp
// ✅ DO: Load all related data at once
var pageResults = await mainEntity
    .Skip(skip).Take(pageSize)
    .Include(x => x.RelatedData1)
    .Include(x => x.RelatedData2)
    .ToListAsync();

var relatedData3 = await context.RelatedData3
    .Where(x => pageResults.Select(r => r.Id).Contains(x.MainId))
    .ToListAsync();

// Then aggregate/calculate in-memory
var dtos = pageResults.Select(x => new ReportDto
{
    Id = x.Id,
    CalculatedField = relatedData3.Where(r => r.MainId == x.Id).Sum(r => r.Value)
}).ToList();

// ❌ DON'T: Query inside loop (N+1 problem)
foreach (var item in mainData)
{
    item.RelatedCount = await context.Related.CountAsync(x => x.MainId == item.Id);
}
```

### Pattern 3: Hierarchical Data (Tree Structure)

**Use Case**: Parent-child relationships with multiple levels

**Implementation**: Materialized Path Pattern
```csharp
public class TreeEntity : BaseEntity
{
    public Guid? ParentId { get; set; }
    public string Path { get; set; }  // e.g., "1/2/3/4"
    public int Level { get; set; }     // Depth in tree
    
    public virtual TreeEntity Parent { get; set; }
    public virtual ICollection<TreeEntity> Children { get; set; }
}

// When creating child node:
if (parentId.HasValue)
{
    var parent = await _repository.GetByIdAsync(parentId.Value);
    child.Path = $"{parent.Path}/{newId}";
    child.Level = parent.Level + 1;
}
else
{
    child.Path = newId.ToString();
    child.Level = 0;
}

// Query all descendants efficiently:
var descendants = await _context.TreeEntity
    .Where(x => x.Path.StartsWith(parentPath))
    .ToListAsync();
```

---

## 🗄️ Database Schema {#database-schema}

### Core Tables

All tables follow this structure:

```sql
CREATE TABLE [DuAn] (
    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
    [TenDuAn] nvarchar(500) NOT NULL,
    [MaDuAn] nvarchar(100) UNIQUE,
    [ThoiGianKhoiCong] int,
    [ThoiGianHoanThanh] int,
    [KhaiToanKinhPhi] decimal(18,2),
    [LoaiDuAnTheoNamId] int,
    [LoaiDuAnId] int,
    [HinhThucDauTuId] int,
    [DonViPhuTrachChinhId] bigint,
    [ParentId] uniqueidentifier,
    
    -- Soft Delete Fields
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [DeletedAt] datetime2,
    
    -- Audit Fields
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] nvarchar(256),
    [LastModifiedAt] datetime2,
    [LastModifiedBy] nvarchar(256),
    
    -- Indexes
    INDEX [IX_DuAn_TenDuAn] ([TenDuAn]),
    INDEX [IX_DuAn_MaDuAn] ([MaDuAn]),
    INDEX [IX_DuAn_IsDeleted] ([IsDeleted]),
);
```

### Relationships

```
DuAn (Parent)
├── DuAnBuoc (Project Steps)
├── DuToan (Budget)
├── NghiemThu (Acceptance)
├── GoiThau (Bid Packages)
├── HopDong (Contracts)
└── BaoCao (Reports)

GoiThau
└── HopDong (linked contract)

HopDong
├── PhuLucHopDong (amendments)
└── ThanhToan (payments)
```

### Key Indices to Add

```csharp
builder.HasIndex(x => x.IsDeleted);
builder.HasIndex(x => x.TenDuAn);
builder.HasIndex(x => x.MaDuAn).IsUnique();
builder.HasIndex(x => new { x.DuAnId, x.IsDeleted });
builder.HasIndex(x => new { x.DonViPhuTrachChinhId, x.IsDeleted });
builder.HasIndex(x => x.CreatedAt);
```

---

## 💻 Code Examples & Templates {#code-templates}

### Template 1: Simple Feature CRUD

**Step 1: Domain Entity** (`QLDA.Domain/Entities/MyEntity.cs`)
```csharp
public class MyEntity : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Status { get; set; }
    
    public virtual ICollection<RelatedEntity> RelatedEntities { get; set; }
}
```

**Step 2: DTOs** (`QLDA.Application/MyEntities/DTOs/`)
```csharp
// MyEntityDto.cs
public class MyEntityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

// CreateMyEntityRequest.cs
public class CreateMyEntityRequest
{
    [Required(ErrorMessage = "Tên không được để trống")]
    [StringLength(500)]
    public string Name { get; set; }
    
    public string Description { get; set; }
}

// MyEntitySearchDto.cs
public class MyEntitySearchDto : CommonSearchDto
{
    public string Name { get; set; }
    public int? Status { get; set; }
}
```

**Step 3: Commands & Handlers**
```csharp
// CreateMyEntityCommand.cs
public class CreateMyEntityCommand : IRequest<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
}

// CreateMyEntityCommandHandler.cs
public class CreateMyEntityCommandHandler : IRequestHandler<CreateMyEntityCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Guid> Handle(CreateMyEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = new MyEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
        };
        
        await _unitOfWork.MyEntityRepository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Id;
    }
}
```

**Step 4: Validators**
```csharp
public class CreateMyEntityValidator : AbstractValidator<CreateMyEntityCommand>
{
    public CreateMyEntityValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống")
            .MaximumLength(500).WithMessage("Tên không vượt quá 500 ký tự");
    }
}
```

**Step 5: Query & Handler**
```csharp
public class GetMyEntityListQuery : IRequest<PaginatedList<MyEntityDto>>
{
    public MyEntitySearchDto SearchDto { get; set; }
}

public class GetMyEntityListQueryHandler : IRequestHandler<GetMyEntityListQuery, PaginatedList<MyEntityDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public async Task<PaginatedList<MyEntityDto>> Handle(GetMyEntityListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.MyEntityRepository.GetQueryable()
            .WhereIf(!string.IsNullOrEmpty(request.SearchDto.Name), x => x.Name.Contains(request.SearchDto.Name))
            .WhereIf(request.SearchDto.Status.HasValue, x => x.Status == request.SearchDto.Status);
        
        return await query.ToPaginatedListAsync(request.SearchDto.PageIndex, request.SearchDto.PageSize, _mapper.Map<MyEntityDto>, cancellationToken);
    }
}
```

**Step 6: Controller**
```csharp
[ApiController]
[Route("api/my-entity")]
[Authorize]
public class MyEntityController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpGet("danh-sach")]
    public async Task<ResultApi<PaginatedList<MyEntityDto>>> GetList([FromQuery] MyEntitySearchDto searchDto)
    {
        var result = await _mediator.Send(new GetMyEntityListQuery { SearchDto = searchDto });
        return ResultApi<PaginatedList<MyEntityDto>>.Success(result);
    }
    
    [HttpPost("them-moi")]
    public async Task<ResultApi<Guid>> Create([FromBody] CreateMyEntityRequest request)
    {
        var command = new CreateMyEntityCommand { Name = request.Name, Description = request.Description };
        var result = await _mediator.Send(command);
        return ResultApi<Guid>.SuccessCreated(result);
    }
}
```

---

## ✅ Step-by-Step Implementation Checklist {#implementation-checklist}

Use this checklist when implementing a new feature in your twin repository:

### Phase 1: Planning
- [ ] Define entity structure (properties, relationships)
- [ ] Define DTOs (request, response, search)
- [ ] Plan validation rules
- [ ] Identify related entities for data loading

### Phase 2: Domain Layer
- [ ] Create entity class inheriting `BaseEntity`
- [ ] Add navigation properties
- [ ] Implement `IEntity` if needed

### Phase 3: Persistence Layer
- [ ] Create entity configuration in `Configurations/`
- [ ] Add DbSet to `ApplicationDbContext`
- [ ] Create repository interface if specialized
- [ ] Create migration
- [ ] Update seed data if needed

### Phase 4: Application Layer - DTOs & Validators
- [ ] Create `{Feature}Dto.cs`
- [ ] Create `Create{Feature}Request.cs`
- [ ] Create `Update{Feature}Request.cs`
- [ ] Create `{Feature}SearchDto.cs` (extends `CommonSearchDto`)
- [ ] Create validators for all commands
- [ ] Add mapping configuration in AutoMapper

### Phase 5: Application Layer - CQRS
- [ ] Create `Create{Feature}Command` & handler
- [ ] Create `Update{Feature}Command` & handler
- [ ] Create `Delete{Feature}Command` & handler
- [ ] Create `Get{Feature}ByIdQuery` & handler
- [ ] Create `Get{Feature}ListQuery` & handler
- [ ] Register all in MediatR

### Phase 6: WebApi Layer
- [ ] Create `{Feature}Controller`
- [ ] Implement `GET danh-sach` endpoint
- [ ] Implement `GET {id}/chi-tiet` endpoint
- [ ] Implement `POST them-moi` endpoint
- [ ] Implement `PUT cap-nhat` endpoint
- [ ] Implement `DELETE {id}/xoa-tam` endpoint (soft delete)
- [ ] Add XML documentation

### Phase 7: Testing
- [ ] Write unit tests for validators
- [ ] Write unit tests for command handlers
- [ ] Write integration tests for API endpoints
- [ ] Write query handler tests

### Phase 8: Documentation
- [ ] Document API endpoints in Swagger
- [ ] Update solution documentation
- [ ] Document any custom business logic
- [ ] Update ERD if relationships changed

---

## 🧪 Testing Strategy {#testing-strategy}

### Unit Test Example

```csharp
[TestClass]
public class CreateDuAnCommandHandlerTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private CreateDuAnCommandHandler _handler;
    
    [TestInitialize]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateDuAnCommandHandler(_unitOfWorkMock.Object, /* other mocks */);
    }
    
    [TestMethod]
    public async Task Handle_ValidCommand_CreatesEntity()
    {
        // Arrange
        var command = new CreateDuAnCommand { TenDuAn = "Test Project" };
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.IsNotNull(result);
        _unitOfWorkMock.Verify(x => x.DuAnRepository.AddAsync(It.IsAny<DuAn>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task Handle_InvalidCommand_ThrowsException()
    {
        var command = new CreateDuAnCommand { TenDuAn = "" };
        await _handler.Handle(command, CancellationToken.None);
    }
}
```

### Integration Test Example

```csharp
[TestClass]
public class DuAnControllerIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _httpClient;
    private ApplicationDbContext _dbContext;
    
    public DuAnControllerIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>();
    }
    
    public async Task InitializeAsync()
    {
        _httpClient = _factory.CreateClient();
        _dbContext = _factory.Services.GetService<ApplicationDbContext>();
        await _dbContext.Database.EnsureCreatedAsync();
    }
    
    [TestMethod]
    public async Task GetList_ReturnsOkWithData()
    {
        // Arrange - seed data
        var duAn = new DuAn { Id = Guid.NewGuid(), TenDuAn = "Test" };
        _dbContext.DuAn.Add(duAn);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var response = await _httpClient.GetAsync("/api/du-an/danh-sach");
        
        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("Test"));
    }
    
    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        _httpClient?.Dispose();
        _factory?.Dispose();
    }
}
```

---

## ⚠️ Common Pitfalls & Solutions {#pitfalls}

| Issue | Problem | Solution |
|-------|---------|----------|
| **N+1 Queries** | Query inside loop causes performance degradation | Use `.Include()` or load related data upfront for page results |
| **Expose Entities to API** | Directly returning entities breaks abstraction | Always use DTOs for request/response |
| **Forgot Soft Delete Filter** | Old data appears in queries | Always enable global query filters for `IsDeleted` |
| **No Audit Fields** | Can't track who modified what | Set `CreatedBy`, `LastModifiedBy` in `SaveChangesAsync()` |
| **Validation Only in Controller** | Duplicate validation logic | Put FluentValidation in Application layer, use MediatR behavior |
| **SQL Injection** | Hardcoded SQL queries are vulnerable | Always use parameterized queries, use EF Core or Dapper safely |
| **Missing Pagination** | Loading all records causes OOM | Always implement `PaginatedList` for list queries |
| **Forgot to Index** | Queries slow down with large data | Add indexes on frequently searched columns |
| **Hard-coded Strings** | Difficult to maintain, change in multiple places | Use constants in `ErrorMessages.cs` |
| **No Logging** | Can't debug production issues | Add logging in behaviors and exception handlers |

---

## 🚀 Deployment Considerations {#deployment}

### Pre-Deployment Checklist

- [ ] All tests passing (unit + integration)
- [ ] No compiler warnings
- [ ] Database migrations tested in staging
- [ ] Swagger documentation complete
- [ ] Security: No hardcoded secrets (use environment variables)
- [ ] Performance: Load test endpoints with expected volume
- [ ] Backup: Database backup plan in place
- [ ] Monitoring: Logging and monitoring configured
- [ ] Documentation: Updated all relevant docs
- [ ] Rollback: Have rollback plan (migration + code)

### Database Migration Strategy

**Always follow this approach**:
```bash
# 1. Create migration (generates files)
dotnet ef migrations add MyMigrationName --project QLDA.Persistence

# 2. Review generated migration
# 3. Test migration in local environment
# 4. Commit to repository
# 5. In production, apply via:
dotnet run --project QLDA.Migrator
```

**Never**:
- ❌ Manually modify migration files after running
- ❌ Delete database to test migrations
- ❌ Use `database update` in production without backup
- ❌ Add breaking changes without migration path

### Environment-Specific Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server={server};Database={db};..."
  },
  "JwtSettings": {
    "SecretKey": "{use Azure Key Vault or environment variable}",
    "Issuer": "https://yourdomain.com",
    "Audience": "https://yourdomain.com"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

---

## 📚 Additional Resources

- Clean Architecture: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- CQRS Pattern: https://martinfowler.com/bliki/CQRS.html
- MediatR: https://github.com/jbogard/MediatR
- Entity Framework Core: https://learn.microsoft.com/en-us/ef/core/
- FluentValidation: https://docs.fluentvalidation.net/

---

## 📞 Support Notes

This guide covers the most common patterns implemented in the SER/QLDA repository. For features specific to your implementation:

1. Check existing feature implementation in this repo
2. Follow the same pattern for consistency
3. Refer back to this guide for code samples
4. Update this document when discovering new patterns

---

**End of Implementation Guide**  
**Version**: 1.0 | **Last Updated**: April 22, 2026  
**For**: Twin Repository Team
