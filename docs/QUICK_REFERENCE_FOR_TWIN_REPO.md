# Quick Reference - QLDA Twin Repository Implementation
## At-a-Glance Summary

**Document Purpose**: Quick lookup reference for implementing features in twin repository  
**Last Updated**: April 22, 2026

---

## 🎯 How to Use This Document

1. **Need to implement a new feature?** → Go to "Feature Template"
2. **Want to understand a pattern?** → Go to "Common Patterns"
3. **Looking for file structure?** → Go to "File Organization"
4. **Need code example?** → Go to "Code Snippets"
5. **Debugging an issue?** → Go to "Troubleshooting"

---

## 📁 File Organization Quick Map

### Domain Layer Structure
```
QLDA.Domain/
├── Entities/
│   └── [EntityName].cs          (extends BaseEntity)
├── Enums/
│   └── [EnumName]Enum.cs
├── Interfaces/
│   └── IRepository.cs, IUnitOfWork.cs
└── Exceptions/
    └── [CustomException].cs
```

### Application Layer Structure
```
QLDA.Application/
├── {Feature}/                    (Feature folder)
│   ├── Commands/
│   │   └── {Operation}/
│   │       ├── {Operation}Command.cs
│   │       └── {Operation}CommandHandler.cs
│   ├── Queries/
│   │   └── {Operation}/
│   │       ├── {Operation}Query.cs
│   │       └── {Operation}QueryHandler.cs
│   ├── DTOs/
│   │   ├── {Feature}Dto.cs
│   │   ├── Create{Feature}Request.cs
│   │   ├── Update{Feature}Request.cs
│   │   └── {Feature}SearchDto.cs
│   └── Validators/
│       └── {Feature}Validator.cs
└── Common/
    ├── Behaviors/              (MediatR cross-cutting)
    ├── DTOs/
    ├── Mappings/               (AutoMapper)
    ├── Interfaces/
    └── Extensions/             (WhereIf, ToPaginatedList)
```

### Persistence Layer Structure
```
QLDA.Persistence/
├── Context/
│   └── ApplicationDbContext.cs
├── Repositories/
│   ├── GenericRepository.cs
│   └── UnitOfWork.cs
├── Configurations/
│   └── {Entity}Configuration.cs
└── Migrations/
    └── [timestamp]_{MigrationName}.cs
```

### WebApi Layer Structure
```
QLDA.WebApi/
├── Controllers/
│   ├── {Feature}Controller.cs
│   └── AuthController.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── JwtMiddleware.cs
├── Program.cs
└── appsettings.json
```

---

## 🔄 Common Patterns

### Pattern: CRUD Feature (Most Common)
**Files Required**: 12-14 files
```
Command Files (3):     CreateXxx, UpdateXxx, DeleteXxx
Query Files (2):       GetXxxById, GetXxxList
DTO Files (4):         XxxDto, CreateRequest, UpdateRequest, SearchDto
Validator Files (2):   CreateValidator, UpdateValidator
Handler Files (5):     All corresponding handlers
Total Handlers:        5 (3 command + 2 query)
```

**Implementation Time**: 2-4 hours

**Key Files Template**:
1. `{Feature}Dto.cs` - Read model
2. `Create{Feature}Request.cs` - Write request
3. `Update{Feature}Request.cs` - Update request
4. `{Feature}SearchDto.cs` - Filter/search
5. `Create{Feature}Command.cs` - CQRS command
6. `Create{Feature}CommandHandler.cs` - Handles creation
7. `Create{Feature}Validator.cs` - Input validation
8. `Update{Feature}Command.cs` - CQRS command
9. `Update{Feature}CommandHandler.cs` - Handles update
10. `Delete{Feature}Command.cs` - CQRS command
11. `Delete{Feature}CommandHandler.cs` - Handles deletion
12. `Get{Feature}ByIdQuery.cs` - CQRS query
13. `Get{Feature}ListQuery.cs` - CQRS query
14. `{Feature}Configuration.cs` - EF Core config

### Pattern: Reporting/Analytics Feature
**Files Required**: 4-5 files
```
Query File (1):       BaoCao{Feature}Query.cs
Handler File (1):     BaoCao{Feature}QueryHandler.cs
DTO Files (2):        BaoCao{Feature}SearchDto.cs, BaoCao{Feature}Dto.cs
No Commands:          Reports are read-only
```

**Implementation Time**: 1-2 hours

---

## 💡 Code Snippets - Copy & Paste

### Create a DTO
```csharp
public class MyFeatureDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Create a Search DTO
```csharp
public class MyFeatureSearchDto : CommonSearchDto
{
    public string Name { get; set; }
    public int? Status { get; set; }
}
```

### Create Entity Config
```csharp
public class MyFeatureConfiguration : IEntityTypeConfiguration<MyFeature>
{
    public void Configure(EntityTypeBuilder<MyFeature> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(500);
        
        // Add indexes
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.IsDeleted);
    }
}
```

### Create Command
```csharp
public class CreateMyFeatureCommand : IRequest<Guid>
{
    public string Name { get; set; }
}
```

### Create Command Handler
```csharp
public class CreateMyFeatureCommandHandler : IRequestHandler<CreateMyFeatureCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateMyFeatureCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Guid> Handle(CreateMyFeatureCommand request, CancellationToken cancellationToken)
    {
        var entity = new MyFeature { Id = Guid.NewGuid(), Name = request.Name };
        await _unitOfWork.MyFeatureRepository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Id;
    }
}
```

### Create Query Handler with Pagination
```csharp
public class GetMyFeatureListQuery : IRequest<PaginatedList<MyFeatureDto>>
{
    public MyFeatureSearchDto SearchDto { get; set; }
}

public class GetMyFeatureListQueryHandler : IRequestHandler<GetMyFeatureListQuery, PaginatedList<MyFeatureDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public GetMyFeatureListQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<PaginatedList<MyFeatureDto>> Handle(GetMyFeatureListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.MyFeatureRepository.GetQueryable()
            .AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(request.SearchDto.Name),
                x => x.Name.Contains(request.SearchDto.Name))
            .WhereIf(request.SearchDto.Status.HasValue,
                x => x.Status == request.SearchDto.Status);
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip(request.SearchDto.PageIndex * request.SearchDto.PageSize)
            .Take(request.SearchDto.PageSize)
            .Select(x => _mapper.Map<MyFeatureDto>(x))
            .ToListAsync(cancellationToken);
        
        return new PaginatedList<MyFeatureDto>(items, totalCount, request.SearchDto.PageIndex, request.SearchDto.PageSize);
    }
}
```

### Create Validator
```csharp
public class CreateMyFeatureValidator : AbstractValidator<CreateMyFeatureCommand>
{
    public CreateMyFeatureValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(500).WithMessage("Name cannot exceed 500 characters");
    }
}
```

### Create Controller Endpoint
```csharp
[ApiController]
[Route("api/my-feature")]
[Authorize]
public class MyFeatureController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public MyFeatureController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("danh-sach")]
    public async Task<ResultApi<PaginatedList<MyFeatureDto>>> GetList([FromQuery] MyFeatureSearchDto searchDto)
    {
        var result = await _mediator.Send(new GetMyFeatureListQuery { SearchDto = searchDto });
        return ResultApi<PaginatedList<MyFeatureDto>>.Success(result);
    }
    
    [HttpPost("them-moi")]
    public async Task<ResultApi<Guid>> Create([FromBody] CreateMyFeatureRequest request)
    {
        var command = new CreateMyFeatureCommand { Name = request.Name };
        var result = await _mediator.Send(command);
        return ResultApi<Guid>.SuccessCreated(result);
    }
}
```

---

## 🚀 Step-by-Step: Implementing a New Feature

### Step 1: Create Entity (Domain)
```bash
📁 QLDA.Domain/Entities/
   └── MyFeature.cs
```

### Step 2: Create DbSet & Config
```bash
📁 QLDA.Persistence/
   ├── Context/ → Add DbSet<MyFeature>
   └── Configurations/
       └── MyFeatureConfiguration.cs
```

### Step 3: Create Migration
```bash
$ dotnet ef migrations add AddMyFeature --project QLDA.Persistence
```

### Step 4: Create DTOs
```bash
📁 QLDA.Application/MyFeatures/DTOs/
   ├── MyFeatureDto.cs
   ├── CreateMyFeatureRequest.cs
   ├── UpdateMyFeatureRequest.cs
   └── MyFeatureSearchDto.cs
```

### Step 5: Create Commands/Queries
```bash
📁 QLDA.Application/MyFeatures/
   ├── Commands/
   │   ├── Create/
   │   │   ├── CreateMyFeatureCommand.cs
   │   │   └── CreateMyFeatureCommandHandler.cs
   │   └── Update/...
   └── Queries/
       ├── GetById/...
       └── GetList/...
```

### Step 6: Create Validators
```bash
📁 QLDA.Application/MyFeatures/Validators/
   ├── CreateMyFeatureValidator.cs
   └── UpdateMyFeatureValidator.cs
```

### Step 7: Register MediatR
```csharp
// In DependencyInjection.cs
services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssemblyContaining<CreateMyFeatureCommand>();
});
```

### Step 8: Create Controller
```bash
📁 QLDA.WebApi/Controllers/
   └── MyFeatureController.cs
```

### Step 9: Test Everything
```bash
✅ Unit tests for validators
✅ Unit tests for handlers
✅ Integration tests for API
✅ Manual testing with Postman/Swagger
```

---

## 🎯 Naming Conventions Quick Guide

| Item | Convention | Example |
|------|-----------|---------|
| **Entity Classes** | PascalCase | `DuAn`, `GoiThau`, `HopDong` |
| **Properties** | PascalCase (Vietnamese OK) | `TenDuAn`, `ThoiGianKhoiCong` |
| **DTO Classes** | `{Feature}Dto` | `DuAnDto` |
| **Request DTOs** | `Create{Feature}Request` | `CreateDuAnRequest` |
| **Search DTOs** | `{Feature}SearchDto` | `DuAnSearchDto` |
| **Commands** | `{Action}{Feature}Command` | `CreateDuAnCommand` |
| **Queries** | `Get{Feature}{Operation}Query` | `GetDuAnByIdQuery` |
| **Handlers** | `{Command/Query}Handler` | `CreateDuAnCommandHandler` |
| **Validators** | `{Feature}Validator` | `CreateDuAnValidator` |
| **Controllers** | `{Feature}Controller` | `DuAnController` |
| **API Routes** | kebab-case | `/api/du-an/danh-sach` |
| **Database Tables** | PascalCase | `DuAn`, `GoiThau` |

---

## ⚡ Performance Tips

1. **Always paginate lists**: Use `PaginatedList` with `pageSize`
2. **Load related data upfront**: Use `.Include()` for page results only
3. **Use `.AsNoTracking()`** for read-only queries
4. **Add indexes** on filtered/searched columns
5. **Use Dapper** for complex analytical queries
6. **Implement caching** for frequently accessed master data
7. **Monitor N+1 queries**: Enable EF Core SQL logging

---

## 🐛 Troubleshooting Matrix

| Issue | Cause | Solution |
|-------|-------|----------|
| "No DbSet for MyFeature" | Missing DbSet in DbContext | Add `public DbSet<MyFeature> MyFeature { get; set; }` |
| "Handler not found" | Not registered in MediatR | Add to DependencyInjection.cs |
| "Mapper not found" | AutoMapper profile missing | Create mapping in MappingProfile.cs |
| "Query returns deleted items" | Missing soft delete filter | Add `.HasQueryFilter()` in OnModelCreating |
| "401 Unauthorized" | Missing [Authorize] or invalid token | Check JWT settings and token validity |
| "Validation fails silently" | Validator not registered | Use FluentValidation MediatR behavior |
| "Changes not saved" | Forgot SaveChangesAsync() | Always call `await _unitOfWork.SaveChangesAsync()` |
| "Slow list query" | N+1 problem or missing index | Check includes and add database index |

---

## 📋 Minimum Viable Feature Checklist

- [ ] Entity created in Domain
- [ ] DbSet added to DbContext
- [ ] Entity configuration created
- [ ] Migration created and tested
- [ ] DTOs created (Dto, Request, SearchDto)
- [ ] Commands and handlers created
- [ ] Queries and handlers created
- [ ] Validators created
- [ ] Handlers registered in DependencyInjection
- [ ] Controller created with endpoints
- [ ] AutoMapper mapping configured
- [ ] Unit tests written
- [ ] Integration tests written
- [ ] Tested in Swagger UI

---

## 🔐 Security Checklist

- [ ] All endpoints have `[Authorize]` attribute
- [ ] Never return entities directly (use DTOs)
- [ ] Validate all user input with FluentValidation
- [ ] Sanitize string inputs
- [ ] Check user permissions before operations
- [ ] Encrypt sensitive data
- [ ] Use parameterized queries (EF Core does this)
- [ ] No hardcoded secrets in code
- [ ] Log security events
- [ ] Regular security audits

---

## 📊 API Response Format (Standard)

**Success Response**:
```json
{
  "isSuccess": true,
  "message": "Operation successful",
  "data": {
    "id": "guid",
    "name": "value"
  },
  "errors": null
}
```

**Paginated Response**:
```json
{
  "isSuccess": true,
  "message": "Retrieved successfully",
  "data": {
    "items": [{...}, {...}],
    "pageIndex": 0,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10
  },
  "errors": null
}
```

**Error Response**:
```json
{
  "isSuccess": false,
  "message": "Operation failed",
  "data": null,
  "errors": ["Error message 1", "Error message 2"]
}
```

---

## 🔗 Related Documentation

- **Full Implementation Guide**: `docs/IMPLEMENTATION_GUIDE_FOR_TWIN_REPO.md`
- **Project Overview**: `docs/project-overview-pdr.md`
- **Codebase Summary**: `docs/codebase-summary.md`
- **Code Standards**: `docs/code-standards.md`
- **Features & Use Cases**: `docs/features.md`
- **API Documentation**: Auto-generated at `/swagger`

---

## 📞 Common Commands

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Create migration
dotnet ef migrations add MigrationName --project QLDA.Persistence

# Update database
dotnet run --project QLDA.Migrator

# Run API
dotnet run --project QLDA.WebApi

# Run with watch (development)
dotnet watch run --project QLDA.WebApi
```

---

**Quick Reference Version 1.0**  
**For**: Twin Repository Team  
**Updated**: April 22, 2026
