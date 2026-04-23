# KeHoachVon (Budget Planning) Implementation Guide

**Document Date**: April 23, 2026  
**Status**: FULLY IMPLEMENTED & TESTED  
**Effort Invested**: ~20 hours  
**Last Updated**: April 23, 2026  

---

## 📋 Quick Facts

| Property | Value |
|----------|-------|
| **Entity Name** | KeHoachVon |
| **Meaning** | Budget Planning / Budget Allocation |
| **Parent Entity** | DuAn (Project) |
| **Relationship** | One-to-Many (DuAn → Multiple KeHoachVons) |
| **Soft Delete** | Yes (IsDeleted, DeletedAt) |
| **Audit Fields** | CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy |
| **Key Challenge** | Collection synchronization with proper FK management |

---

## 🏗️ Architecture Overview

```
DuAn (Project)
├── ID: Guid
├── TenDuAn: string
├── ... (other properties)
└── KeHoachVons: ICollection<KeHoachVon>  ← One-to-Many relationship

KeHoachVon (Budget Plan)
├── ID: Guid (PK)
├── DuAnId: Guid (FK) ← CRITICAL: Must always be set!
├── NguonVonId: Guid? (FK to funding source)
├── Nam: int (Year)
├── SoVon: decimal (Amount)
├── SoVonDieuChinh: decimal? (Adjusted amount)
├── SoQuyetDinh: string? (Decision number)
├── NgayKy: DateTimeOffset? (Sign date)
├── GhiChu: string? (Notes)
└── IsDeleted: bool (Soft delete)
```

---

## 🗂️ Files Involved

### Domain Layer
- **Entity**: `QLDA.Domain/Entities/KeHoachVon.cs`
- **Configuration**: `QLDA.Persistence/Configurations/KeHoachVonConfiguration.cs`
- **Related**: `QLDA.Domain/Entities/DuAn.cs` (has collection property)

### Application Layer
- **DTOs**: `QLDA.Application/KeHoachVons/DTOs/`
  - `KeHoachVon.cs` (read DTO)
  - `KeHoachVonInsertDto.cs` (create input)
  - `KeHoachVonUpdateDto.cs` (update input)
  - `KeHoachVonMapping.cs` (mappers)

- **Queries**: `QLDA.Application/KeHoachVons/Queries/`
- **Commands**: `QLDA.Application/KeHoachVons/Commands/` (if any)

### Integration with DuAn
- **Commands**: `QLDA.Application/DuAns/Commands/DuAnUpdateCommand.cs`
- **Mappings**: `QLDA.Application/DuAns/DTOs/DuAnMappings.cs`
- **DTOs**: `QLDA.Application/DuAns/DTOs/DuAnUpdateModel.cs`

### Persistence
- **Migration**: `QLDA.Persistence/Migrations/` (look for KeHoachVon table creation)

---

## 📊 Entity Details

### KeHoachVon.cs
```csharp
public class KeHoachVon : Entity<Guid>, IAggregateRoot {
    
    public Guid DuAnId { get; set; }                    // ← Foreign Key (CRITICAL!)
    public Guid? NguonVonId { get; set; }               // Optional: Funding source
    public int Nam { get; set; }                         // Required: Year
    public decimal SoVon { get; set; }                   // Required: Budget amount
    public decimal? SoVonDieuChinh { get; set; }        // Optional: Adjusted amount
    public string? SoQuyetDinh { get; set; }            // Optional: Decision number
    public DateTimeOffset? NgayKy { get; set; }         // Optional: Sign date
    public string? GhiChu { get; set; }                 // Optional: Notes
    
    // Navigation
    public DuAn? DuAn { get; set; }
}
```

### EF Core Configuration
**File**: `KeHoachVonConfiguration.cs`

```csharp
public override void Configure(EntityTypeBuilder<KeHoachVon> builder) {
    builder.ToTable(nameof(KeHoachVon));
    builder.ConfigureForBase();

    builder.Property(e => e.DuAnId).IsRequired();       // FK is required
    builder.Property(e => e.Nam).IsRequired();
    builder.Property(e => e.SoVon)
        .HasColumnType("decimal(18,2)")
        .IsRequired();
    builder.Property(e => e.SoVonDieuChinh)
        .HasColumnType("decimal(18,2)");
    builder.Property(e => e.SoQuyetDinh).HasMaxLength(100);
    builder.Property(e => e.GhiChu).HasColumnType("nvarchar(max)");
    
    // DateTimeOffset handling
    builder.Property(e => e.NgayKy)
        .HasConversion(
            toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
            fromDb => fromDb
        );

    // Relationship configuration
    builder.HasOne(e => e.DuAn)
        .WithMany(e => e.KeHoachVons)
        .HasForeignKey(e => e.DuAnId)
        .OnDelete(DeleteBehavior.Cascade);  // Delete KeHoachVons when DuAn deleted
}
```

---

## 📝 DTOs & Mappings

### 1. KeHoachVonDto (Read)
```csharp
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

### 2. KeHoachVonInsertDto (Create)
```csharp
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

### 3. KeHoachVonUpdateDto (Update)
```csharp
public class KeHoachVonUpdateDto : IHasKey<Guid> {
    public Guid Id { get; set; }  // ← Required to identify which record to update
    public Guid? NguonVonId { get; set; }
    public int Nam { get; set; }
    public decimal SoVon { get; set; }
    public decimal? SoVonDieuChinh { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? GhiChu { get; set; }
}
```

### 4. KeHoachVonMapping (Mappers)
**File**: `QLDA.Application/KeHoachVons/DTOs/KeHoachVonMapping.cs`

```csharp
public class KeHoachVonMapping {
    
    // ✅ Maps update DTO to entity WITHOUT DuAnId
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

    // ✅ CRITICAL: Maps update DTO to entity WITH DuAnId
    // This overload is essential for proper FK mapping!
    public static KeHoachVon ToEntity(this KeHoachVonUpdateDto dto, Guid duAnId) {
        var entity = dto.ToEntity();
        entity.DuAnId = duAnId;  // ← DuAnId is set here
        return entity;
    }

    // Maps insert DTO to entity WITHOUT DuAnId
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

    // ✅ Maps insert DTO to entity WITH DuAnId
    public static KeHoachVon ToEntity(this KeHoachVonInsertDto dto, Guid duAnId) {
        var entity = dto.ToEntity();
        entity.DuAnId = duAnId;
        return entity;
    }

    // Maps entity to read DTO
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

    // Updates entity fields from DTO (preserves DuAnId!)
    public static void Update(this KeHoachVon entity, KeHoachVonUpdateDto dto) {
        entity.NguonVonId = dto.NguonVonId;
        entity.Nam = dto.Nam;
        entity.SoVon = dto.SoVon;
        entity.SoVonDieuChinh = dto.SoVonDieuChinh;
        entity.SoQuyetDinh = dto.SoQuyetDinh;
        entity.NgayKy = dto.NgayKy;
        entity.GhiChu = dto.GhiChu;
        // NOTE: DuAnId is NOT updated here (it should never change!)
    }
}
```

---

## 🔄 Integration with DuAn Update Flow

### The Problem We Fixed

**Before**: When updating a DuAn with KeHoachVons, the code did this:

```csharp
// ❌ WRONG - in DuAnMappings.Update()
entity.KeHoachVons = [.. dto.KeHoachVons?.Select(e => e.ToEntity()) ?? []];
//                                                         ↑ No DuAnId passed!
// This creates entities with DuAnId = null → FK constraint violation!
```

**After**: The flow is now correct:

```
DuAn Update Request
    ↓
DuAnUpdateCommand.Handle()
    ├─ 1. Include(e => e.KeHoachVons) from DB
    ├─ 2. entity.Update(request.Model) [updates scalar properties ONLY]
    ├─ 3. SyncKeHoachVonsAsync() [handles collection changes]
    │       ├─ For updates: existingKHV.Update(khv)  [preserves DuAnId]
    │       ├─ For inserts: khv.ToEntity(entity.Id)  [sets DuAnId correctly]
    │       └─ For deletes: Delete old records
    └─ 4. Save changes to DB
```

### DuAnUpdateCommand.cs Implementation

**File**: `QLDA.Application/DuAns/Commands/DuAnUpdateCommand.cs`

```csharp
public async Task<DuAn> Handle(DuAnUpdateCommand request, CancellationToken cancellationToken) {
    await ValidateAsync(request, cancellationToken);

    var entity = await DuAn.GetQueryableSet()
        .Include(e => e.KeHoachVons)  // ← Load existing budget plans
        .FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);
    
    ManagedException.ThrowIfNull(entity);

    // Store original ParentId for hierarchical path updates
    var originalParentId = entity.ParentId;
    
    // ✅ Update properties but NOT collections
    // Collections are handled by Sync methods below
    entity.Update(request.Model);

    // Handle parent change (hierarchical materialized path)
    if (originalParentId != entity.ParentId) {
        DuAn? newParent = null;
        if (entity.ParentId.HasValue) {
            newParent = await DuAn.GetQueryableSet()
                .FirstOrDefaultAsync(e => e.Id == entity.ParentId.Value, cancellationToken);
        }
        await DuAn.MoveNodeAsync(entity, newParent, cancellationToken);
    }

    // ✅ Sync DuToan collection
    await SyncHelper.SyncCollection(DuToan, entity.DuToans, [...], ...);
    
    // ✅ Sync KeHoachVon collection (handles FK properly)
    await SyncKeHoachVonsAsync(entity, request.Model.KeHoachVons, cancellationToken);

    // Save with transaction management
    if (_unitOfWork.HasTransaction) {
        await UpdateAsync(entity, cancellationToken);
    } else {
        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }

    return entity;
}

// ✅ CRITICAL: Synchronization method
private async Task SyncKeHoachVonsAsync(
    DuAn entity, 
    List<KeHoachVonUpdateDto>? keHoachVons, 
    CancellationToken cancellationToken) {
    
    var existingKeHoachVons = entity.KeHoachVons ?? [];
    
    // Find which ones to update, insert, delete
    var keHoachVonsToUpdate = keHoachVons?
        .Where(khv => existingKeHoachVons.Any(e => e.Id == khv.Id))
        .ToList() ?? [];
    var keHoachVonsToInsert = keHoachVons?
        .Where(khv => !existingKeHoachVons.Any(e => e.Id == khv.Id))
        .ToList() ?? [];
    var keHoachVonsToDelete = existingKeHoachVons
        .Where(e => keHoachVons == null || !keHoachVons.Any(khv => khv.Id == e.Id))
        .ToList();

    // UPDATE existing records
    foreach (var khv in keHoachVonsToUpdate) {
        var existingKHV = existingKeHoachVons.First(e => e.Id == khv.Id);
        existingKHV.Update(khv);  // ✅ Preserves DuAnId
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // INSERT new records
    foreach (var khv in keHoachVonsToInsert) {
        var newKHV = khv.ToEntity(entity.Id);  // ✅ Sets DuAnId correctly!
        await _unitOfWork.GetRepository<KeHoachVon, Guid>()
            .AddAsync(newKHV, cancellationToken);
    }

    // DELETE removed records
    foreach (var khv in keHoachVonsToDelete) {
        await _unitOfWork.GetRepository<KeHoachVon, Guid>()
            .DeleteAsync(khv, cancellationToken);
    }
}
```

### DuAnMappings.cs (Correct Implementation)

**File**: `QLDA.Application/DuAns/DTOs/DuAnMappings.cs`

```csharp
public static void Update(this DuAn entity, DuAnUpdateModel dto) {
    // Update all scalar properties
    entity.Id = dto.Id;
    entity.TenDuAn = dto.TenDuAn;
    entity.QuyTrinhId = dto.QuyTrinhId;
    // ... other properties ...
    
    // Update DuAnNguonVons (source of funding)
    entity.DuAnNguonVons = [.. dto.DanhSachNguonVon?
        .Select(nguonVonId => new DuAnNguonVon {
            DuAnId = dto.Id,
            NguonVonId = nguonVonId
        }) ?? []];
    
    // Update DuAnChiuTrachNhiemXuLys (responsible units)
    entity.DuAnChiuTrachNhiemXuLys = [..dto.DonViPhoiHopIds?
        .Select(phoiHopId => new DuAnChiuTrachNhiemXuLy {
            DuAnId = dto.Id,
            ChiuTrachNhiemXuLyId = phoiHopId,
            Loai = EChiuTrachNhiemXuLy.DonViPhoiHop
        }) ?? []];
    
    // ✅ IMPORTANT: KeHoachVons is NOT updated here!
    // Note: KeHoachVons NOT updated here - handled by SyncKeHoachVonsAsync in DuAnUpdateCommand
    // This prevents loss of data and ensures proper FK management
}
```

---

## 🧪 Testing KeHoachVon

### Unit Tests: Mapping Logic
```csharp
[Fact]
public void ToEntity_WithDuAnId_ShouldSetDuAnIdCorrectly() {
    // Arrange
    var dto = new KeHoachVonUpdateDto {
        Id = Guid.NewGuid(),
        Nam = 2024,
        SoVon = 1000000
    };
    var duAnId = Guid.NewGuid();
    
    // Act
    var entity = dto.ToEntity(duAnId);
    
    // Assert
    Assert.Equal(duAnId, entity.DuAnId);  // ← DuAnId must be set!
}

[Fact]
public void Update_ShouldPreserveDuAnId() {
    // Arrange
    var entity = new KeHoachVon { DuAnId = Guid.NewGuid() };
    var originalDuAnId = entity.DuAnId;
    var dto = new KeHoachVonUpdateDto { Nam = 2025 };
    
    // Act
    entity.Update(dto);
    
    // Assert
    Assert.Equal(originalDuAnId, entity.DuAnId);  // ← Never changes!
}
```

### Integration Tests: DuAn Update with KeHoachVons
```csharp
[Fact]
public async Task UpdateDuAn_WithNewKeHoachVon_ShouldSetForeignKey() {
    // Arrange
    var duAnId = Guid.NewGuid();
    var duAn = new DuAn { Id = duAnId, TenDuAn = "Test" };
    
    var updateModel = new DuAnUpdateModel {
        Id = duAnId,
        KeHoachVons = new List<KeHoachVonUpdateDto> {
            new() { Nam = 2024, SoVon = 1000000 }  // New record
        }
    };
    
    // Act
    var result = await mediator.Send(new DuAnUpdateCommand(updateModel));
    
    // Assert
    var savedKeHoachVon = result.KeHoachVons.First();
    Assert.Equal(duAnId, savedKeHoachVon.DuAnId);  // ← Must equal!
}
```

---

## ⚠️ Critical Implementation Rules

### Rule 1: Always Pass DuAnId When Creating
```csharp
// ❌ WRONG
var entity = khv.ToEntity();  // DuAnId is null!

// ✅ CORRECT
var entity = khv.ToEntity(duAnId);  // DuAnId is set!
```

### Rule 2: Never Replace Collection in Mapper
```csharp
// ❌ WRONG - in DuAnMappings.Update()
entity.KeHoachVons = [.. dto.KeHoachVons?.Select(e => e.ToEntity()) ?? []];

// ✅ CORRECT - handle in dedicated sync method
// Note: KeHoachVons NOT updated here - handled by SyncKeHoachVonsAsync
```

### Rule 3: Update Method Never Changes FK
```csharp
public static void Update(this KeHoachVon entity, KeHoachVonUpdateDto dto) {
    entity.Nam = dto.Nam;
    entity.SoVon = dto.SoVon;
    // ... other fields ...
    
    // ❌ NEVER do this:
    // entity.DuAnId = something;  // FK should NEVER change!
}
```

### Rule 4: Always Load Collections Before Update
```csharp
// ✅ CRITICAL
var entity = await DuAn.GetQueryableSet()
    .Include(e => e.KeHoachVons)  // ← Must include!
    .FirstOrDefaultAsync(e => e.Id == duAnId);
```

### Rule 5: Use SyncHelper for Collections
```csharp
// ✅ Use dedicated sync method
await SyncKeHoachVonsAsync(entity, request.Model.KeHoachVons, cancellationToken);
```

---

## 🔍 Database Schema

### KeHoachVon Table
```sql
CREATE TABLE [KeHoachVon] (
    [Id] [uniqueidentifier] PRIMARY KEY DEFAULT NEWID(),
    [DuAnId] [uniqueidentifier] NOT NULL,              -- FK
    [NguonVonId] [uniqueidentifier] NULL,              -- Optional FK
    [Nam] [int] NOT NULL,
    [SoVon] [decimal](18,2) NOT NULL,
    [SoVonDieuChinh] [decimal](18,2) NULL,
    [SoQuyetDinh] [nvarchar](100) NULL,
    [NgayKy] [datetimeoffset] NULL,
    [GhiChu] [nvarchar](max) NULL,
    
    -- Audit fields
    [CreatedAt] [datetimeoffset] NOT NULL,
    [CreatedBy] [nvarchar](255) NULL,
    [LastModifiedAt] [datetimeoffset] NULL,
    [LastModifiedBy] [nvarchar](255) NULL,
    [IsDeleted] [bit] DEFAULT 0,
    [DeletedAt] [datetimeoffset] NULL,
    
    CONSTRAINT [FK_KeHoachVon_DuAn] FOREIGN KEY ([DuAnId])
        REFERENCES [DuAn]([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_KeHoachVon_DuAnId] ON [KeHoachVon]([DuAnId]);
CREATE INDEX [IX_KeHoachVon_IsDeleted] ON [KeHoachVon]([IsDeleted]);
```

---

## 🚀 How to Use in Twin Repository

### Step 1: Copy Domain Entity
```
From: QLDA.Domain/Entities/KeHoachVon.cs
To: [TwinRepo].Domain/Entities/KeHoachVon.cs
```

### Step 2: Copy Configuration
```
From: QLDA.Persistence/Configurations/KeHoachVonConfiguration.cs
To: [TwinRepo].Persistence/Configurations/KeHoachVonConfiguration.cs
```

### Step 3: Copy DTOs & Mappings
```
From: QLDA.Application/KeHoachVons/DTOs/
To: [TwinRepo].Application/KeHoachVons/DTOs/
```

### Step 4: Create Migration
```powershell
# In Package Manager Console for [TwinRepo].Persistence
Add-Migration AddKeHoachVonTable
Update-Database
```

### Step 5: Update DuAn Entity
```csharp
// In [TwinRepo].Domain/Entities/DuAn.cs
public ICollection<KeHoachVon>? KeHoachVons { get; set; } = [];
```

### Step 6: Integrate into DuAn Update
```csharp
// Copy SyncKeHoachVonsAsync() method to [TwinRepo] DuAnUpdateCommand
// Follow exact same pattern as QLDA implementation
```

---

## ✅ Verification Checklist

Before deploying KeHoachVon to production:

- [ ] Entity compiles without errors
- [ ] EF Core configuration is correct
- [ ] Migration creates table properly
- [ ] DuAnId is NOT nullable in schema
- [ ] Foreign key constraint is set with ON DELETE CASCADE
- [ ] KeHoachVonMapping has both overloads of ToEntity()
- [ ] DuAnMappings.Update() does NOT replace KeHoachVons collection
- [ ] DuAnUpdateCommand.SyncKeHoachVonsAsync() handles insert/update/delete
- [ ] Unit tests pass (mapping, FK preservation)
- [ ] Integration tests pass (full DuAn update with KeHoachVons)
- [ ] No null DuAnId values in database
- [ ] Soft deletes work correctly

---

## 🔗 Related Documentation

- [FEATURE_IMPLEMENTATION_INVENTORY.md](../feature/id/FEATURE_IMPLEMENTATION_INVENTORY.md) - Master feature list
- [TWIN_REPO_IMPLEMENTATION_GUIDE.md](../TWIN_REPO_IMPLEMENTATION_GUIDE.md) - Twin repo roadmap
- [MULTI_REPO_MANAGEMENT_RULES.md](../MULTI_REPO_MANAGEMENT_RULES.md) - Multi-repo rules

---

## 📞 Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| "DuAnId is null" FK violation | ToEntity() called without duAnId parameter | Use overload with duAnId: `khv.ToEntity(duAnId)` |
| Data lost on DuAn update | Collection replaced in Update() method | Remove replacement, use SyncKeHoachVonsAsync() only |
| DuAnId changes unexpectedly | Update() method modifying FK | Never update DuAnId in Update() method |
| Collections not loaded | Missing Include() in query | Add `.Include(e => e.KeHoachVons)` |
| Orphaned KeHoachVon records | No cascade delete configured | Set `OnDelete(DeleteBehavior.Cascade)` in config |

---

## 📈 Performance Considerations

### Query Optimization
```csharp
// ✅ Good: Single query with include
var duAn = await _context.DuAns
    .Include(e => e.KeHoachVons)
    .FirstOrDefaultAsync(e => e.Id == duAnId);

// ❌ Bad: N+1 query problem
var duAn = await _context.DuAns.FirstOrDefaultAsync(e => e.Id == duAnId);
var keHoachVons = await _context.KeHoachVons  // Separate query!
    .Where(e => e.DuAnId == duAnId)
    .ToListAsync();
```

### Bulk Updates
```csharp
// For large updates, consider batch operations
// instead of individual SaveChanges() calls
await _unitOfWork.SaveChangesAsync(cancellationToken);  // Flush at end
```

---

## 🎓 Learning Outcomes

After implementing KeHoachVon, you understand:

✅ One-to-Many relationships in EF Core  
✅ Collection synchronization patterns  
✅ Foreign key management in domain models  
✅ CQRS pattern with complex commands  
✅ Soft delete implementation  
✅ DTOs and mapping patterns  
✅ Transaction management  
✅ Unit of Work pattern  

This knowledge applies to ALL similar features in the system!

