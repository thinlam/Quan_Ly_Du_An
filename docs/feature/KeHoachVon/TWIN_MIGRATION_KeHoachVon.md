# Twin Repository Migration: KeHoachVon (Budget Planning)

**From**: QLDA Main Repository  
**Feature**: Budget Planning (KeHoachVon) Management  
**Status**: Ready for Migration  
**Last Updated**: April 23, 2026  
**Estimated Effort**: 12-15 hours  

---

## 📋 Executive Summary

**What is KeHoachVon?**
Budget planning records tied to a project (DuAn). Each project can have multiple budget plans for different years and funding sources.

**Key Challenge**: Collection synchronization while maintaining proper Foreign Key (FK) relationships.

**Implementation Complexity**: Medium  
**Dependency**: Requires DuAn module to be implemented first

---

## 🎯 Feature Scope

### What's Included
- ✅ KeHoachVon entity with all properties
- ✅ Budget record creation, update, deletion
- ✅ Soft delete support
- ✅ Audit fields (CreatedBy, timestamps)
- ✅ Collection synchronization with DuAn updates
- ✅ EF Core migration & database schema

### What's NOT Included
- ❌ Budget approval workflow (implement separately if needed)
- ❌ Budget vs actual reporting (separate feature)
- ❌ Historical budget tracking (implement separately)
- ❌ Budget consolidation/aggregation

---

## 📂 Source Files Location in QLDA

```
QLDA.Domain/
└── Entities/
    └── KeHoachVon.cs                    [Entity definition]

QLDA.Persistence/
└── Configurations/
    └── KeHoachVonConfiguration.cs       [EF Core config]

QLDA.Application/
└── KeHoachVons/
    └── DTOs/
        ├── KeHoachVon.cs               [Read DTO]
        ├── KeHoachVonInsertDto.cs      [Create DTO]
        ├── KeHoachVonUpdateDto.cs      [Update DTO]
        └── KeHoachVonMapping.cs        [Mappers & logic]

QLDA.Application/
└── DuAns/
    ├── Commands/
    │   └── DuAnUpdateCommand.cs        [Uses SyncKeHoachVonsAsync]
    └── DTOs/
        ├── DuAnMappings.cs             [Integrates KeHoachVons]
        └── DuAnUpdateModel.cs          [References KeHoachVonUpdateDto]
```

---

## 🚀 Step-by-Step Implementation

### Phase 1: Domain Layer (1-2 hours)

#### 1.1 Create KeHoachVon Entity
**Copy from**: `QLDA.Domain/Entities/KeHoachVon.cs`

```csharp
// File: [YourRepo].Domain/Entities/KeHoachVon.cs

using [YourRepo].Domain.Common;

namespace [YourRepo].Domain.Entities;

public class KeHoachVon : Entity<Guid>, IAggregateRoot {
    
    public Guid DuAnId { get; set; }                    // FK to Project
    public Guid? NguonVonId { get; set; }               // FK to Funding Source (optional)
    
    public int Nam { get; set; }                         // Year
    public decimal SoVon { get; set; }                   // Budget Amount (18,2)
    public decimal? SoVonDieuChinh { get; set; }        // Adjusted Amount (18,2)
    public string? SoQuyetDinh { get; set; }            // Decision Number (max 100)
    public DateTimeOffset? NgayKy { get; set; }         // Sign Date
    public string? GhiChu { get; set; }                 // Notes
    
    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    #endregion
}
```

**Critical Points**:
- ✅ `DuAnId` is required (not nullable)
- ✅ Inherit from `Entity<Guid>` and `IAggregateRoot`
- ✅ Decimal fields use (18,2) precision

#### 1.2 Update DuAn Entity
**Modify**: `[YourRepo].Domain/Entities/DuAn.cs`

```csharp
// Add this navigation property to DuAn class:
public ICollection<KeHoachVon>? KeHoachVons { get; set; } = [];
```

**Verify**:
- [ ] DuAn has KeHoachVons collection property

---

### Phase 2: Persistence Layer (2-3 hours)

#### 2.1 Create EF Core Configuration
**Copy from**: `QLDA.Persistence/Configurations/KeHoachVonConfiguration.cs`

```csharp
// File: [YourRepo].Persistence/Configurations/KeHoachVonConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using [YourRepo].Domain.Entities;

namespace [YourRepo].Persistence.Configurations;

public class KeHoachVonConfiguration : AggregateRootConfiguration<KeHoachVon> {
    
    public override void Configure(EntityTypeBuilder<KeHoachVon> builder) {
        builder.ToTable(nameof(KeHoachVon));
        builder.ConfigureForBase();

        // Required properties
        builder.Property(e => e.DuAnId).IsRequired();
        builder.Property(e => e.Nam).IsRequired();
        builder.Property(e => e.SoVon)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Optional properties
        builder.Property(e => e.NguonVonId);
        builder.Property(e => e.SoVonDieuChinh)
            .HasColumnType("decimal(18,2)");
        builder.Property(e => e.SoQuyetDinh)
            .HasMaxLength(100);
        builder.Property(e => e.GhiChu)
            .HasColumnType("nvarchar(max)");

        // DateTimeOffset conversion for UTC consistency
        builder.Property(e => e.NgayKy)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        // Relationships
        builder.HasOne(e => e.DuAn)
            .WithMany(e => e.KeHoachVons)
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Cascade);  // ← Important!
    }
}
```

**Critical Points**:
- ✅ Decimal precision is (18,2)
- ✅ Cascade delete when DuAn is deleted
- ✅ DateTimeOffset UTC conversion

#### 2.2 Register Configuration
**Modify**: Your `DbContext.OnModelCreating()` or configuration registration

```csharp
// Add to your DbContext configuration:
modelBuilder.ApplyConfiguration(new KeHoachVonConfiguration());
```

#### 2.3 Create Migration
```powershell
# In Package Manager Console for [YourRepo].Persistence

Add-Migration AddKeHoachVonTable
Update-Database
```

**Verify in Database**:
```sql
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'KeHoachVon'
```

**Verify Checklist**:
- [ ] Table created with correct columns
- [ ] DuAnId is NOT NULL
- [ ] Decimal columns are (18,2)
- [ ] Foreign key constraint exists
- [ ] Indexes created on DuAnId and IsDeleted

---

### Phase 3: Application Layer (3-4 hours)

#### 3.1 Create DTOs
**Copy from**: `QLDA.Application/KeHoachVons/DTOs/`

**File 1**: KeHoachVon.cs (Read DTO)
```csharp
// File: [YourRepo].Application/KeHoachVons/DTOs/KeHoachVon.cs

using [YourRepo].Application.Common.Interfaces;

namespace [YourRepo].Application.KeHoachVons.DTOs;

public class KeHoachVon : IHasKey<Guid> {
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public Guid? NguonVonId { get; set; }
    public int Nam { get; set; }
    public decimal SoVon { get; set; }
    public decimal? SoVonDieuChinh { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? GhiChu { get; set; }
}
```

**File 2**: KeHoachVonInsertDto.cs (Create Input)
```csharp
// File: [YourRepo].Application/KeHoachVons/DTOs/KeHoachVonInsertDto.cs

namespace [YourRepo].Application.KeHoachVons.DTOs;

public class KeHoachVonInsertDto {
    public Guid? NguonVonId { get; set; }
    public int Nam { get; set; }
    public decimal SoVon { get; set; }
    public decimal? SoVonDieuChinh { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? GhiChu { get; set; }
}
```

**File 3**: KeHoachVonUpdateDto.cs (Update Input)
```csharp
// File: [YourRepo].Application/KeHoachVons/DTOs/KeHoachVonUpdateDto.cs

using [YourRepo].Application.Common.Interfaces;

namespace [YourRepo].Application.KeHoachVons.DTOs;

public class KeHoachVonUpdateDto : IHasKey<Guid> {
    public Guid Id { get; set; }  // ← Required to identify which record
    public Guid? NguonVonId { get; set; }
    public int Nam { get; set; }
    public decimal SoVon { get; set; }
    public decimal? SoVonDieuChinh { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? GhiChu { get; set; }
}
```

#### 3.2 Create Mappings
**Copy from**: `QLDA.Application/KeHoachVons/DTOs/KeHoachVonMapping.cs`

```csharp
// File: [YourRepo].Application/KeHoachVons/DTOs/KeHoachVonMapping.cs

using [YourRepo].Domain.Entities;

namespace [YourRepo].Application.KeHoachVons.DTOs;

public class KeHoachVonMapping {
    
    // ✅ WITHOUT DuAnId - used internally
    public static KeHoachVon ToEntity(this KeHoachVonUpdateDto dto) {
        return new KeHoachVon {
            Id = dto.Id,
            NguonVonId = dto.NguonVonId,
            Nam = dto.Nam,
            SoVon = dto.SoVon,
            SoVonDieuChinh = dto.SoVonDieuChinh,
            SoQuyetDinh = dto.SoQuyetDinh,
            NgayKy = dto.NgayKy,
            GhiChu = dto.GhiChu
        };
    }

    // ✅ WITH DuAnId - CRITICAL OVERLOAD!
    public static KeHoachVon ToEntity(this KeHoachVonUpdateDto dto, Guid duAnId) {
        var entity = dto.ToEntity();
        entity.DuAnId = duAnId;  // ← FK is set here!
        return entity;
    }

    public static KeHoachVon ToEntity(this KeHoachVonInsertDto dto) {
        return new KeHoachVon {
            NguonVonId = dto.NguonVonId,
            Nam = dto.Nam,
            SoVon = dto.SoVon,
            SoVonDieuChinh = dto.SoVonDieuChinh,
            SoQuyetDinh = dto.SoQuyetDinh,
            NgayKy = dto.NgayKy,
            GhiChu = dto.GhiChu
        };
    }

    // ✅ WITH DuAnId for inserts
    public static KeHoachVon ToEntity(this KeHoachVonInsertDto dto, Guid duAnId) {
        var entity = dto.ToEntity();
        entity.DuAnId = duAnId;
        return entity;
    }

    public static KeHoachVonDto ToDto(this KeHoachVon entity) {
        return new KeHoachVonDto {
            Id = entity.Id,
            DuAnId = entity.DuAnId,
            NguonVonId = entity.NguonVonId,
            Nam = entity.Nam,
            SoVon = entity.SoVon,
            SoVonDieuChinh = entity.SoVonDieuChinh,
            SoQuyetDinh = entity.SoQuyetDinh,
            NgayKy = entity.NgayKy,
            GhiChu = entity.GhiChu
        };
    }

    // ✅ Update preserves DuAnId (never changes!)
    public static void Update(this KeHoachVon entity, KeHoachVonUpdateDto dto) {
        entity.NguonVonId = dto.NguonVonId;
        entity.Nam = dto.Nam;
        entity.SoVon = dto.SoVon;
        entity.SoVonDieuChinh = dto.SoVonDieuChinh;
        entity.SoQuyetDinh = dto.SoQuyetDinh;
        entity.NgayKy = dto.NgayKy;
        entity.GhiChu = dto.GhiChu;
        // NOTE: DuAnId is NEVER updated!
    }
}
```

**Critical Points**:
- ✅ Two overloads of `ToEntity()` - with and without DuAnId
- ✅ `Update()` method never changes DuAnId
- ✅ Both Insert and Update DTOs map to Entity correctly

---

### Phase 4: Integration with DuAn (3-4 hours)

#### 4.1 Update DuAnUpdateModel
**Modify**: `[YourRepo].Application/DuAns/DTOs/DuAnUpdateModel.cs`

```csharp
// Add this property to DuAnUpdateModel class:
public List<KeHoachVonUpdateDto>? KeHoachVons { get; set; }
```

#### 4.2 Update DuAnMappings
**Modify**: `[YourRepo].Application/DuAns/DTOs/DuAnMappings.cs`

**In ToEntity() method** (for Create):
```csharp
public static DuAn ToEntity(this DuAnInsertDto dto) {
    var id = GuidExtensions.GetSequentialGuidId();
    var entity = new DuAn {
        // ... other properties ...
        
        // ✅ Map KeHoachVons correctly with DuAnId
        KeHoachVons = [.. dto.KeHoachVons?.Select(e => e.ToEntity(id)) ?? []]
    };
    return entity;
}
```

**In Update() method** (for Update):
```csharp
public static void Update(this DuAn entity, DuAnUpdateModel dto) {
    // Update scalar properties...
    entity.Id = dto.Id;
    // ... other properties ...
    
    // ✅ IMPORTANT: KeHoachVons NOT updated here!
    // Note: KeHoachVons NOT updated here - handled by SyncKeHoachVonsAsync in DuAnUpdateCommand
}
```

#### 4.3 Update DuAnUpdateCommand
**Modify**: `[YourRepo].Application/DuAns/Commands/DuAnUpdateCommand.cs`

**In Handle() method** - add Include:
```csharp
var entity = await DuAn.GetQueryableSet()
    .Include(e => e.KeHoachVons)  // ← Add this line
    .Include(e => e.DuToans)
    .Include(e => e.DuAnNguonVons)
    .FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);
```

**Call sync method** after entity.Update():
```csharp
// After entity.Update(request.Model):
await SyncKeHoachVonsAsync(entity, request.Model.KeHoachVons, cancellationToken);
```

**Add sync method** at end of class:
```csharp
private async Task SyncKeHoachVonsAsync(
    DuAn entity,
    List<KeHoachVonUpdateDto>? keHoachVons,
    CancellationToken cancellationToken) {
    
    var existingKeHoachVons = entity.KeHoachVons ?? [];
    var keHoachVonsToUpdate = keHoachVons?
        .Where(khv => existingKeHoachVons.Any(e => e.Id == khv.Id)).ToList() ?? [];
    var keHoachVonsToInsert = keHoachVons?
        .Where(khv => !existingKeHoachVons.Any(e => e.Id == khv.Id)).ToList() ?? [];
    var keHoachVonsToDelete = existingKeHoachVons
        .Where(e => keHoachVons == null || !keHoachVons.Any(khv => khv.Id == e.Id)).ToList();

    // UPDATE
    foreach (var khv in keHoachVonsToUpdate) {
        var existingKHV = existingKeHoachVons.First(e => e.Id == khv.Id);
        existingKHV.Update(khv);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // INSERT
    foreach (var khv in keHoachVonsToInsert) {
        var newKHV = khv.ToEntity(entity.Id);  // ← DuAnId set here!
        await _unitOfWork.GetRepository<KeHoachVon, Guid>()
            .AddAsync(newKHV, cancellationToken);
    }

    // DELETE
    foreach (var khv in keHoachVonsToDelete) {
        await _unitOfWork.GetRepository<KeHoachVon, Guid>()
            .DeleteAsync(khv, cancellationToken);
    }
}
```

---

### Phase 5: Testing (2-3 hours)

#### 5.1 Unit Tests
```csharp
// File: [YourRepo].Tests/Application/KeHoachVons/KeHoachVonMappingTests.cs

[TestClass]
public class KeHoachVonMappingTests {
    
    [TestMethod]
    public void ToEntity_WithDuAnId_ShouldSetForeignKey() {
        var dto = new KeHoachVonUpdateDto { Id = Guid.NewGuid(), Nam = 2024 };
        var duAnId = Guid.NewGuid();
        
        var entity = dto.ToEntity(duAnId);
        
        Assert.AreEqual(duAnId, entity.DuAnId);
    }
    
    [TestMethod]
    public void Update_ShouldPreserveDuAnId() {
        var entity = new KeHoachVon { DuAnId = Guid.NewGuid() };
        var originalDuAnId = entity.DuAnId;
        var dto = new KeHoachVonUpdateDto { Nam = 2025 };
        
        entity.Update(dto);
        
        Assert.AreEqual(originalDuAnId, entity.DuAnId);
    }
}
```

#### 5.2 Integration Tests
```csharp
// Test DuAn update with new KeHoachVon
// Verify DuAnId is set correctly
// Verify soft delete works
// Verify cascade delete works
```

---

## ✅ Validation Checklist

**Before deploying to production**:

### Code Quality
- [ ] Entity compiles
- [ ] DTOs compiles
- [ ] Mappings compile
- [ ] No compiler warnings
- [ ] All unit tests pass

### Database
- [ ] Migration runs without errors
- [ ] Table created with correct schema
- [ ] Foreign key constraint exists
- [ ] Indexes created

### Integration
- [ ] DuAn.KeHoachVons property works
- [ ] SyncKeHoachVonsAsync handles insert/update/delete
- [ ] No null DuAnId values in DB
- [ ] Soft deletes work

### Testing
- [ ] Mapping unit tests pass
- [ ] DuAn update with KeHoachVons integration test passes
- [ ] No orphaned records on delete

---

## 📊 Effort Breakdown

| Phase | Task | Hours | Status |
|-------|------|-------|--------|
| 1 | Create entity & config | 1.5 | |
| 2 | Create migration | 1.5 | |
| 3 | Create DTOs & mappings | 2 | |
| 4 | Integrate with DuAn | 2.5 | |
| 5 | Testing & validation | 2.5 | |
| **Total** | | **10-15** | |

---

## 🚀 Next Steps After KeHoachVon

Once KeHoachVon is implemented, you can implement similar collection-based features:
- HopDongNhanXuTri (Contract handlers)
- DuAnHopLyTacVu (Project coordination units)
- Any other One-to-Many collections

The pattern is identical!

