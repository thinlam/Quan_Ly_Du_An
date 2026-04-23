# Twin Repository Implementation Guide
**From**: QLDA Main Repository  
**To**: Twin Repository (Parallel Project Management System)  
**Document Date**: April 23, 2026  
**Purpose**: Complete implementation blueprint for replicating features in twin repo

---

## 📋 Executive Summary

This guide summarizes **12 fully-implemented modules** from QLDA project that can be migrated to the twin repository. Total effort: **~240+ hours** across **6 months** of development.

---

## 🏗️ High-Level Architecture

```
Twin Repo Structure (Based on QLDA Implementation):
├── Domain Layer (Entities + Value Objects)
├── Application Layer (Commands, Queries, DTOs, Validators)
├── Infrastructure Layer (Repositories, EF Configurations)
├── Persistence Layer (Database Migrations, Seeds)
├── WebApi Layer (Controllers, Middleware)
└── Tests (Unit, Integration, API tests)
```

---

## 📦 Module Implementation Summary

### Module 1: Project Management (DuAn) - 80 Hours
**Key Deliverables**:
- Core entity with hierarchical structure (ParentId)
- CRUD operations with soft delete
- Budget reporting with aggregations
- Advanced filtering & pagination

**Files to Reference**:
- Domain: `QLDA.Domain/Entities/DuAn.cs`
- Application: `QLDA.Application/DuAns/Commands/DuAnCreateCommand.cs`, `DuAnUpdateCommand.cs`
- Queries: `QLDA.Application/DuAns/Queries/DuAnGetByIdQuery.cs`
- DTOs: `QLDA.Application/DuAns/DTOs/DuAnDto.cs`, `DuAnMappings.cs`
- Controller: `QLDA.WebApi/Controllers/DuAnsController.cs`

**Critical Implementation Notes**:
- ✅ Use soft delete (IsDeleted, DeletedAt fields)
- ✅ Implement hierarchical parent-child relationships
- ✅ Add audit fields (CreatedBy, LastModifiedBy, timestamps)
- ✅ Use CQRS pattern (separate Commands and Queries)
- ⚠️ Handle collection synchronization properly (see SyncHelper pattern below)

**Estimated Effort for Twin Repo**: 40-50 hours

---

### Module 2: Project Steps (DuAnBuoc) - 40 Hours
**Key Deliverables**:
- Step/task management for projects
- State management
- Document attachments support

**Files to Reference**:
- Entity: `QLDA.Domain/Entities/DuAnBuoc.cs`
- Commands: `QLDA.Application/DuAnBuocs/Commands/`
- Queries: `QLDA.Application/DuAnBuocs/Queries/`
- Controller: `QLDA.WebApi/Controllers/DuAnBuocsController.cs`

**Estimated Effort for Twin Repo**: 20-25 hours

---

### Module 3: Bid Package Management (GoiThau) - 50 Hours
**Key Deliverables**:
- Tender/package creation and management
- Supplier relationship
- State transitions

**Files to Reference**:
- Entity: `QLDA.Domain/Entities/GoiThau.cs`
- Commands/Queries: `QLDA.Application/GoiThaus/`
- Controller: `QLDA.WebApi/Controllers/GoiThausController.cs`

**Estimated Effort for Twin Repo**: 25-30 hours

---

### Module 4: Contract Management (HopDong) - 60 Hours
**Key Deliverables**:
- Contract creation and tracking
- Amendment management (PhuLucHopDong)
- Multi-level validation

**Files to Reference**:
- Entities: `QLDA.Domain/Entities/HopDong.cs`, `PhuLucHopDong.cs`
- Commands: `QLDA.Application/HopDongs/Commands/`
- Controller: `QLDA.WebApi/Controllers/HopDongsController.cs`

**Estimated Effort for Twin Repo**: 30-35 hours

---

### Module 5: Financial Management (ThanhToan & TamUng) - 55 Hours
**Key Deliverables**:
- Payment tracking and approval workflow
- Advance fund management
- Financial validation

**Files to Reference**:
- Entities: `QLDA.Domain/Entities/ThanhToan.cs`, `TamUng.cs`
- Commands: `QLDA.Application/ThanhToans/`, `QLDA.Application/TamUngs/`
- Controllers: `QLDA.WebApi/Controllers/ThanhToansController.cs`, `TamUngsController.cs`

**Critical Notes**:
- Implement approval workflow with state transitions
- Add financial validation rules
- Consider audit trail for sensitive operations

**Estimated Effort for Twin Repo**: 30-35 hours

---

### Module 6: Acceptance & Validation (NghiemThu) - 35 Hours
**Key Deliverables**:
- Acceptance testing workflow
- Validation records
- Result tracking

**Files to Reference**:
- Entity: `QLDA.Domain/Entities/NghiemThu.cs`
- Commands/Queries: `QLDA.Application/NghiemThus/`
- Controller: `QLDA.WebApi/Controllers/NghiemThusController.cs`

**Estimated Effort for Twin Repo**: 18-20 hours

---

### Module 7: Budget Management (DuToan) - 40+ Hours
**Key Deliverables**:
- Budget estimates and tracking
- Multiple budget versions support
- Budget aggregations and reports

**Files to Reference**:
- Entity: `QLDA.Domain/Entities/DuToan.cs`
- Commands/Queries: `QLDA.Application/DuToans/`
- Controller: `QLDA.WebApi/Controllers/DuToansController.cs`

**Estimated Effort for Twin Repo**: 20-25 hours

---

### Module 8: Reporting & Analytics (BaoCao) - 30+ Hours
**Key Deliverables**:
- Aggregated reporting
- Multi-dimensional filtering
- Export to Excel/PDF

**Files to Reference**:
- Queries: `QLDA.Application/Reports/Queries/`
- Service: `QLDA.Infrastructure/Services/ExcelExportService.cs`

**Estimated Effort for Twin Repo**: 15-20 hours

---

### Module 9: Master Data Management (DanhMuc*) - 60 Hours
**Key Deliverables**:
- Catalog entities (Categories, Enumerations)
- Hierarchical categories
- Reference data management

**Entities to Implement**:
```
DanhMucLoaiDuAn
DanhMucHinhThucDauTu
DanhMucHinhThucQuanLyDuAn
DanhMucNguonVon
DanhMucTrangThai
DanhMucLinhVuc
DanhMucNhomDuAn
... (See QLDA.Domain/Entities for full list)
```

**Files to Reference**:
- All entities: `QLDA.Domain/Entities/DanhMuc*.cs`
- CRUD implementations: `QLDA.Application/DanhMucs/`

**Pattern**: Generic CRUD controller handling all DanhMuc entities

**Estimated Effort for Twin Repo**: 30-40 hours

---

### Module 10: Authentication & Security - 40+ Hours
**Key Deliverables**:
- JWT authentication
- Role-based authorization
- Policy-based access control

**Files to Reference**:
- Auth service: `QLDA.Infrastructure/Services/AuthService.cs`
- Middleware: `QLDA.WebApi/Middleware/AuthenticationMiddleware.cs`
- Policies: `QLDA.WebApi/AuthorizationPolicies/`

**Estimated Effort for Twin Repo**: 20-25 hours

---

### Module 11: Common Infrastructure - 40+ Hours
**Key Deliverables**:
- Unit of Work pattern
- Repository pattern
- Dependency injection setup
- Shared DTOs and mappers

**Files to Reference**:
- UnitOfWork: `BuildingBlocks/src/BuildingBlocks.Persistence/UnitOfWork.cs`
- Repository: `BuildingBlocks/src/BuildingBlocks.Persistence/Repository.cs`
- DI Setup: `QLDA.WebApi/Program.cs`

**Estimated Effort for Twin Repo**: 25-30 hours

---

### Module 12: Data Migration & Seeding - 30+ Hours
**Key Deliverables**:
- EF Core migrations for all entities
- Initial data seeding
- Migration management scripts

**Files to Reference**:
- Migrations folder: `QLDA.Persistence/Migrations/`
- Seed data: `QLDA.Persistence/Seeds/`

**Estimated Effort for Twin Repo**: 15-20 hours

---

## 🔑 Key Implementation Patterns Used

### Pattern 1: CQRS with MediatR
**Command Example**:
```csharp
// File: DuAnCreateCommand.cs
public record DuAnCreateCommand(DuAnCreateDto Model) : IRequest<DuAn>;

internal class DuAnCreateCommandHandler : IRequestHandler<DuAnCreateCommand, DuAn>
{
    public async Task<DuAn> Handle(DuAnCreateCommand request, CancellationToken cancellationToken)
    {
        // Validation → Create → Save → Return
    }
}
```

### Pattern 2: Collection Synchronization (SyncHelper)
**Problem**: Updating parent with multiple child collections while handling deletes

**Solution**:
```csharp
await SyncHelper.SyncCollection(
    repository, 
    entity.ChildCollection,
    requestModel.ChildDtos,
    (existing, request) => {
        existing.Field1 = request.Field1;
        existing.Field2 = request.Field2;
    },
    cancellationToken
);
```

**Files to Reference**:
- `BuildingBlocks/src/BuildingBlocks.Application/SyncHelper.cs`
- Usage example: `DuAnUpdateCommand.cs` (lines 47-65)

### Pattern 3: Hierarchical Entity with Materialized Path
**For**: Parent-child relationships with DuAn

**Implementation**:
- Use `ParentId` for FK relationship
- Use `MoveNodeAsync()` method to update materialized path
- File: `QLDA.Domain/Entities/DuAn.cs`

### Pattern 4: Soft Delete Pattern
**All entities extend**:
```csharp
public abstract class Entity<TKey> : IAuditableEntity
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
```

**In queries**: Always filter `where !e.IsDeleted`

### Pattern 5: Unit of Work with Transaction Management
**Implementation**:
```csharp
if (_unitOfWork.HasTransaction) {
    await UpdateAsync(entity, cancellationToken);
} else {
    using var tx = await _unitOfWork.BeginTransactionAsync(
        IsolationLevel.ReadCommitted, 
        cancellationToken
    );
    await UpdateAsync(entity, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    await _unitOfWork.CommitTransactionAsync(cancellationToken);
}
```

---

## 🛠️ Implementation Roadmap for Twin Repo

### Phase 1: Setup & Infrastructure (Week 1-2)
- [ ] Clone repository structure
- [ ] Setup Domain layer with base classes
- [ ] Configure EF Core and migrations
- [ ] Setup DI container (Program.cs)
- **Effort**: 20-25 hours

### Phase 2: Core Modules (Week 3-6)
- [ ] Module 1: Project Management (DuAn)
- [ ] Module 2: Project Steps (DuAnBuoc)
- [ ] Module 3: Bid Packages (GoiThau)
- [ ] Module 4: Contract Management (HopDong)
- **Effort**: 100-120 hours

### Phase 3: Financial & Workflow (Week 7-9)
- [ ] Module 5: Financial Management (ThanhToan, TamUng)
- [ ] Module 6: Acceptance (NghiemThu)
- [ ] Module 7: Budget (DuToan)
- **Effort**: 85-95 hours

### Phase 4: Master Data & Common (Week 10-11)
- [ ] Module 9: Master Data (DanhMuc*)
- [ ] Module 11: Common Infrastructure enhancements
- **Effort**: 60-70 hours

### Phase 5: Reporting & Security (Week 12-13)
- [ ] Module 8: Reporting
- [ ] Module 10: Authentication & Security
- [ ] Module 12: Migrations & Seeding
- **Effort**: 60-70 hours

### Phase 6: Testing & Deployment (Week 14-15)
- [ ] Unit tests for all modules
- [ ] Integration tests
- [ ] API endpoint tests
- [ ] Performance testing
- **Effort**: 30-40 hours

---

## 📚 Code References & Files to Copy

### Base Classes & Interfaces
```
BuildingBlocks/src/BuildingBlocks.Domain/
  ├── Entity.cs (base class)
  ├── ValueObject.cs
  └── IAggregateRoot.cs

BuildingBlocks/src/BuildingBlocks.Application/
  ├── SyncHelper.cs (critical!)
  ├── IRepository.cs
  └── IUnitOfWork.cs
```

### DTO & Mapping Patterns
```
QLDA.Application/[ModuleName]/DTOs/
  ├── [Entity]Dto.cs
  ├── [Entity]CreateDto.cs
  ├── [Entity]UpdateDto.cs
  ├── [Entity]SearchDto.cs
  └── [Entity]Mappings.cs
```

### Validator Pattern
```csharp
public class DuAnCreateValidator : AbstractValidator<DuAnCreateDto>
{
    public DuAnCreateValidator()
    {
        RuleFor(x => x.TenDuAn)
            .NotEmpty()
            .MaximumLength(500);
        
        RuleFor(x => x.ThoiGianKhoiCong)
            .InclusiveBetween(2020, 2100);
    }
}
```

### Controller Pattern
```csharp
[ApiController]
[Route("api/[controller]")]
public class DuAnsController : BaseController
{
    private readonly IMediator _mediator;

    [HttpPost("them-moi")]
    public async Task<ActionResult<DuAnDto>> Create([FromBody] DuAnCreateDto request)
    {
        var command = new DuAnCreateCommand(request);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}/chi-tiet")]
    public async Task<ActionResult<DuAnDto>> GetById([FromRoute] Guid id)
    {
        var query = new DuAnGetByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

---

## ⚠️ Common Pitfalls to Avoid

1. **Not implementing SyncHelper correctly** → Data inconsistency with child collections
2. **Missing soft delete checks in queries** → Returning deleted data
3. **Not using Unit of Work for transactions** → Race conditions
4. **Improper audit field tracking** → Cannot trace changes
5. **Missing validators** → Invalid data in database
6. **Not filtering IsDeleted in all queries** → Data integrity issues

---

## 📊 Effort Summary

| Phase | Duration | Hours | Status |
|-------|----------|-------|--------|
| Setup & Infrastructure | Week 1-2 | 20-25 | Planned |
| Core Modules | Week 3-6 | 100-120 | Planned |
| Financial & Workflow | Week 7-9 | 85-95 | Planned |
| Master Data & Common | Week 10-11 | 60-70 | Planned |
| Reporting & Security | Week 12-13 | 60-70 | Planned |
| Testing & Deployment | Week 14-15 | 30-40 | Planned |
| **TOTAL** | **15 weeks** | **~355-420 hours** | **-** |

---

## 📞 Support & Questions

When implementing features in twin repo:
1. Reference the exact file paths provided in each module section
2. Follow the code patterns shown in "Key Implementation Patterns"
3. Refer to the QLDA repository as the source of truth
4. Document any deviations or customizations made for the twin repo
5. Keep this document updated with progress

---

## ✅ Verification Checklist for Each Module

- [ ] Entity created with all properties and relationships
- [ ] EF Core Configuration created
- [ ] Migration generated
- [ ] Repository methods created
- [ ] DTOs (Create, Update, Read, Search) created
- [ ] Mappings created
- [ ] Validators created
- [ ] Commands implemented
- [ ] Queries implemented
- [ ] Controller endpoints implemented
- [ ] Unit tests created (minimum 80% coverage)
- [ ] Integration tests created
- [ ] Documentation updated

