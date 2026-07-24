# Phase 5 — EF Core Mapping & Cleanup `TepDinhKem`

> **Trạng thái:** Hoàn thành  
> **Branch:** `151-refactor-consolidate-attachment-files`  
> **Ngày:** 2026-07-20 (cập nhật mapping 2026-07-21 sau merge `main` / `3e70b87`)  
> **Phạm vi:** Chỉ Phase 5 (không Phase 6, không commit tự động)

---

## Summary

Phase 5 consolidate runtime entity về `BuildingBlocks.Domain.Entities.Attachment`, map EF vào bảng DB `TepDinhKem`, và xóa toàn bộ implementation duplicate (`TepDinhKem` entity, command/query/handler cũ, helper duplicate). DTO/API compatibility (`TepDinhKemDto`, `DanhSachTepDinhKem`, …) được giữ nguyên.

**EF mapping (sau merge `main`):** không còn `QLDA.Persistence/Configurations/AttachmentConfiguration.cs`. Owner runtime table name + `ConfigureForBase()` là **`AppDbContext.OnModelCreating`** (force `ToTable("TepDinhKem")`), đồng bộ với fix `3e70b87` — tránh race BB `Attachments` vs QLDA `TepDinhKem`.

---

## Bước 0 — Baseline (trước khi sửa)

### Lệnh đã chạy

```powershell
Set-Location e:\SER
git status
git branch --show-current
dotnet build SER.sln -c Release --nologo
```

### Kết quả baseline

| Kiểm tra | Kết quả |
|----------|---------|
| Branch | `151-refactor-consolidate-attachment-files` |
| Build Release | **0 errors, 0 warnings** |
| Entity runtime | Còn song song `Attachment` + `TepDinhKem` (BB Domain) |
| QLDA đã dùng `IRepository<Attachment>` | ~99% handlers — chỉ còn 1 file dùng entity cũ |
| Migration/snapshot | Không sửa |

### Inventory trước Phase 5

| Nhóm | Phát hiện | Hành động Phase 5 |
|------|-----------|-------------------|
| `BuildingBlocks.Domain.Entities.TepDinhKem` | Entity duplicate | **Xóa** |
| `BuildingBlocks.Persistence.TepDinhKemConfiguration` | Map `TepDinhKems` | **Xóa** |
| `BuildingBlocks.Application/TepDinhKems/` | Command/query/DTO cũ | **Xóa toàn folder** |
| `QLDA.Application/TepDinhKems/Commands/*` | Bulk command cũ (0 caller WebApi) | **Xóa** |
| `QLDA.Application/TepDinhKems/Queries/GetDanhSachTepDinhKemQuery` | Compatibility query | **Xóa** |
| `QLDA.Application/Common/SignedHelper.cs` | Duplicate ký số | **Xóa** → dùng `SignedGroupTypeHelper` |
| `QLDA.Application/Common/SyncHelper.cs` | Duplicate sync | **Xóa** → dùng BB `SyncHelper` |
| `QLDA.Application/TepDinhKems/DTOs/*` | API contract | **Giữ** |
| `ToTrinhPheDuyetGetExportQuery` | Inject `IRepository<TepDinhKem>` không dùng | **Xóa dead code** |

---

## Bước 1 — EF Core mapping consolidate

### 1.1 BuildingBlocks `AttachmentConfiguration`

**File:** `BuildingBlocks/src/BuildingBlocks.Persistence/Configurations/AttachmentConfiguration.cs`

**Thay đổi:** Thêm `ExcludeFromMigrations()` — BB không sở hữu DDL QLDA. File vẫn tồn tại cho shared module / module khác; **QLDA không apply** class này (xem §1.3).

```csharp
builder.ToTable("Attachments", t => t.ExcludeFromMigrations());
builder.ConfigureForBase();
```

### 1.2 QLDA `AttachmentConfiguration` — **Deleted** (theo `3e70b87`)

**File:** `QLDA.Persistence/Configurations/AttachmentConfiguration.cs` → **không còn trong repo**

**Lý do (commit `3e70b87` trên `main`):** cả BB (`ToTable("Attachments")`) và QLDA (`ToTable("TepDinhKem")`) đều bị `ApplyConfigurationsFromAssembly` pick up. Thứ tự `AppDomain.GetAssemblies()` không deterministic → config chạy sau thắng. Nếu BB thắng → runtime query bảng `Attachments` (không tồn tại) → `Invalid object name 'Attachments'`.

Fix: **xóa** QLDA config (redundant) + **force** `ToTable("TepDinhKem")` trong `AppDbContext` sau mọi `ApplyConfigurationsFromAssembly`.

> Bản Phase 5 ban đầu (2026-07-20) từng giữ QLDA `AttachmentConfiguration` làm owner. Sau merge `main` → cập nhật theo `3e70b87`: owner = `AppDbContext`.

### 1.3 `AppDbContext` — owner bảng `TepDinhKem` + exclude BB config

**File:** `QLDA.Persistence/AppDbContext.cs`

1. **Exclude** `BuildingBlocks.Persistence.Configurations.AttachmentConfiguration` khỏi cả hai `ApplyConfigurationsFromAssembly` (AggregateRoot + `IEntityTypeConfiguration`) — tránh map nhầm `"Attachments"`.
2. **Xóa** block `Entity<TepDinhKem>` (entity đã xóa).
3. **Force** map `Attachment` → `TepDinhKem` + `ConfigureForBase()` (vì không còn `IEntityTypeConfiguration<Attachment>` nào được apply cho QLDA).

```csharp
// Trong filter ApplyConfigurationsFromAssembly:
t.FullName != "BuildingBlocks.Persistence.Configurations.AttachmentConfiguration"

// Sau vòng ApplyConfigurations:
modelBuilder.Entity<BuildingBlocks.Domain.Entities.Attachment>(e =>
{
    e.ToTable("TepDinhKem", t => t.ExcludeFromMigrations());
    e.ConfigureForBase();
});
```

> `ExcludeFromMigrations` giữ snapshot Migrator ổn định. `ConfigureForBase()` cung cấp key/audit defaults (trước đây nằm trong QLDA `AttachmentConfiguration`).

### 1.4 Xóa configuration entity cũ

| File | Hành động |
|------|-----------|
| `BuildingBlocks.Persistence/Configurations/TepDinhKemConfiguration.cs` | **Deleted** |
| `QLDA.Persistence/Configurations/AttachmentConfiguration.cs` | **Deleted** (theo `3e70b87` / merge `main`) |

---

## Bước 2 — Sửa caller còn lại trước khi xóa entity

### 2.1 `ToTrinhPheDuyetGetExportQuery`

**File:** `QLDA.Application/ToTrinhPheDuyet/Queries/ToTrinhPheDuyetGetExportQuery.cs`

- Xóa field `IRepository<TepDinhKem>` — inject nhưng không dùng trong handler.

### 2.2 Chuyển `SignedHelper` → `SignedGroupTypeHelper`

| File | Thay đổi |
|------|----------|
| `QLDA.Application/BanGiaoHoSos/DTOs/BanGiaoHoSoMappings.cs` | `using BuildingBlocks.Application.Attachments.Common` |
| `QLDA.WebApi/Models/BanGiaoHoSos/BanGiaoHoSoMappingConfiguration.cs` | idem |
| `QLDA.WebApi/Models/HoSoDeXuatCapDoCntts/HoSoDeXuatCapDoCnttMappingConfiguration.cs` | idem |

Extension `ResolveSignedGroupType` giữ nguyên API — chỉ đổi namespace sang `SignedGroupTypeHelper`.

---

## Bước 3 — SyncHelper consolidation

### 3.1 BuildingBlocks `SyncHelper`

**File:** `BuildingBlocks.Application/Common/SyncHelper.cs`

- Xóa overload `SetDeleteWithRelatedFiles(IRepository<TepDinhKem, …>)`
- Thêm overload 3 tham số `(IRepository<Attachment>, groupIds, cancellationToken)` để tương thích caller QLDA delete commands

### 3.2 QLDA global using

**File:** `QLDA.Application/GlobalUsing.cs`

```csharp
global using BuildingBlocks.Application.Common;
```

→ Tất cả handler dùng `using QLDA.Application.Common` cho `SyncHelper` tự resolve sang BB version.

### 3.3 Xóa duplicate

| File | Hành động |
|------|-----------|
| `QLDA.Application/Common/SyncHelper.cs` | **Deleted** |
| `QLDA.Application/Common/SignedHelper.cs` | **Deleted** |

---

## Bước 4 — Xóa entity và implementation cũ

### 4.1 Entity

| File | Hành động |
|------|-----------|
| `BuildingBlocks.Domain/Entities/TepDinhKem.cs` | **Deleted** |

> Không có `QLDA.Domain/Entities/TepDinhKem.cs` trong repo.

### 4.2 BuildingBlocks.Application/TepDinhKems/ (toàn folder)

| File | Hành động |
|------|-----------|
| `Commands/TepDinhKemBulkInsertOrUpdateCommand.cs` | **Deleted** |
| `Commands/TepDinhKemInsertCommand.cs` | **Deleted** |
| `Queries/GetDanhSachTepDinhKemQuery.cs` | **Deleted** |
| `DTOs/TepDinhKemDto.cs` | **Deleted** |
| `DTOs/TepDinhKemInsertModel.cs` | **Deleted** |
| `DTOs/TepDinhKemInsertOrUpdateModel.cs` | **Deleted** |
| `DTOs/TepDinhKemMapping.cs` | **Deleted** |

### 4.3 BuildingBlocks interface cũ

| File | Hành động |
|------|-----------|
| `BuildingBlocks.Application/Common/Interfaces/IMayHaveTepDinhKem.cs` | **Deleted** (QLDA có bản riêng dùng `TepDinhKemDto`) |

### 4.4 QLDA command/query cũ

| File | Hành động |
|------|-----------|
| `QLDA.Application/TepDinhKems/Commands/TepDinhKemBulkInsertOrUpdateCommand.cs` | **Deleted** |
| `QLDA.Application/TepDinhKems/Commands/TepDinhKemBulkDeleteByGroupCommand.cs` | **Deleted** (0 caller) |
| `QLDA.Application/TepDinhKems/Queries/GetDanhSachTepDinhKemQuery.cs` | **Deleted** |

### 4.5 WebApi cleanup nhỏ

| File | Thay đổi |
|------|----------|
| `QLDA.WebApi/Controllers/DuAnController.cs` | Xóa `using QLDA.Application.TepDinhKems.Queries` (unused) |

---

## Final architecture

```text
Runtime entity:          BuildingBlocks.Domain.Entities.Attachment
Database table:          TepDinhKem
Owning mapping (QLDA):   QLDA.Persistence.AppDbContext
                         → ToTable("TepDinhKem", ExcludeFromMigrations) + ConfigureForBase()
BB AttachmentConfiguration:
                         Exists (ToTable "Attachments", ExcludeFromMigrations)
                         nhưng bị EXCLUDE trong AppDbContext.ApplyConfigurationsFromAssembly
QLDA AttachmentConfiguration:  Deleted (3e70b87)
Shared application module: BuildingBlocks.Application.Attachments
Signed logic (single):   BuildingBlocks.Application.Attachments.Common.SignedGroupTypeHelper
Sync logic (single):     BuildingBlocks.Application.Common.SyncHelper
```

---

## Files changed

### Added

_(Không có file mới — chỉ doc này)_

### Modified

| File | Mô tả |
|------|-------|
| `BuildingBlocks.Persistence/Configurations/AttachmentConfiguration.cs` | `ExcludeFromMigrations` (không apply vào QLDA DbContext) |
| `BuildingBlocks.Application/Common/SyncHelper.cs` | Xóa TepDinhKem overload, thêm 3-param Attachment overload |
| `QLDA.Persistence/AppDbContext.cs` | Exclude BB AttachmentConfiguration; force `ToTable("TepDinhKem")` + `ConfigureForBase()`; xóa `Entity<TepDinhKem>` |
| `QLDA.Application/GlobalUsing.cs` | `global using BuildingBlocks.Application.Common` |
| `QLDA.Application/BanGiaoHoSos/DTOs/BanGiaoHoSoMappings.cs` | SignedGroupTypeHelper |
| `QLDA.Application/ToTrinhPheDuyet/Queries/ToTrinhPheDuyetGetExportQuery.cs` | Xóa dead TepDinhKem repo |
| `QLDA.WebApi/Controllers/DuAnController.cs` | Xóa unused using |
| `QLDA.WebApi/Models/BanGiaoHoSos/BanGiaoHoSoMappingConfiguration.cs` | SignedGroupTypeHelper |
| `QLDA.WebApi/Models/HoSoDeXuatCapDoCntts/HoSoDeXuatCapDoCnttMappingConfiguration.cs` | SignedGroupTypeHelper |

### Deleted

**Entity & EF:**
- `BuildingBlocks.Domain/Entities/TepDinhKem.cs`
- `BuildingBlocks.Persistence/Configurations/TepDinhKemConfiguration.cs`
- `QLDA.Persistence/Configurations/AttachmentConfiguration.cs` (redundant với force map trong `AppDbContext` — `3e70b87`)

**BuildingBlocks.Application/TepDinhKems/ (7 files):**
- Commands, Queries, DTOs, Mapping

**QLDA legacy:**
- `QLDA.Application/TepDinhKems/Commands/TepDinhKemBulkInsertOrUpdateCommand.cs`
- `QLDA.Application/TepDinhKems/Commands/TepDinhKemBulkDeleteByGroupCommand.cs`
- `QLDA.Application/TepDinhKems/Queries/GetDanhSachTepDinhKemQuery.cs`
- `QLDA.Application/Common/SignedHelper.cs`
- `QLDA.Application/Common/SyncHelper.cs`
- `BuildingBlocks.Application/Common/Interfaces/IMayHaveTepDinhKem.cs`

### Kept for compatibility

```text
QLDA.Application/TepDinhKems/DTOs/
├── TepDinhKemDto.cs
├── TepDinhKemInsertDto.cs
├── TepDinhKemInsertOrUpdateDto.cs
└── TepDinhKemMappingConfiguration.cs

QLDA.Application/Common/Interfaces/IMayHaveTepDinhKemDto.cs
QLDA.WebApi/Models/TepDinhKems/TepDinhKemModel.cs
QLDA.WebApi/Models/Common/Interfaces/IMayHaveTepDinhKemModel.cs
QLDA.Application/DuAns/Queries/DuAnGetDanhSachTepDinhKemQuery.cs  (query riêng theo DuAnId)
```

---

## EF mapping verification

```text
Attachment mapped table (runtime):  TepDinhKem
Migration / table owner (QLDA):     AppDbContext.OnModelCreating
                                    → ToTable("TepDinhKem", ExcludeFromMigrations)
                                    → ConfigureForBase()
ExcludeFromMigrations locations:
  - BuildingBlocks.Persistence.AttachmentConfiguration → ToTable("Attachments", ExcludeFromMigrations)
    (file còn; KHÔNG apply vào QLDA AppDbContext)
  - QLDA.Persistence.AppDbContext → Entity<Attachment>().ToTable("TepDinhKem", ExcludeFromMigrations)
QLDA.AttachmentConfiguration:       Deleted (3e70b87)
BB AttachmentConfiguration applied: No (filtered out by FullName)
Duplicate TepDinhKemConfiguration:  0 (deleted)
Migration files modified:           0
ModelSnapshot modified:             0
```

---

## Remaining `TepDinhKem` references (phân loại)

| Loại | Ví dụ | OK? |
|------|-------|-----|
| DTO compatibility | `TepDinhKemDto`, `DanhSachTepDinhKem` | ✅ Giữ |
| Variable name (repo Attachment) | `private IRepository<Attachment> TepDinhKem` | ✅ OK — chỉ tên biến |
| Query riêng DuAn | `DuAnGetDanhSachTepDinhKemQuery` | ✅ Giữ |
| Comment/dead code | `TheoDoiDeXuatNhuCauKinhPhiQuery` commented line | ✅ Không runtime |
| Migration history | `AppDbContextModelSnapshot` → `TepDinhKem` entity | ✅ Không sửa |
| Documentation | `plan.md`, phase 1–4 docs | ✅ Phase 6 |
| **Runtime entity** | `IRepository<TepDinhKem>`, `new TepDinhKem` | ❌ **0** |

---

## Validation

### Runtime reference checks

```text
IRepository<TepDinhKem> in BuildingBlocks/QLDA.Application:  0
new TepDinhKem (entity) in BuildingBlocks/QLDA.Application:  0
List<TepDinhKem>/IEnumerable<TepDinhKem> entity:               0
IEntityTypeConfiguration<TepDinhKem>:                          0
modelBuilder.Entity<TepDinhKem>:                               0
```

### Helper duplicate checks

```text
class SignedHelper:                    0 (deleted)
class SignedGroupTypeHelper:           1 (BuildingBlocks.Application.Attachments.Common)
QLDA.Application.Common.SyncHelper:    0 (deleted)
BuildingBlocks.Application.SyncHelper: 1 (canonical)
```

### Build & test

```text
Build command:  dotnet build SER.sln -c Release --nologo
Build result:   SUCCESS — 0 errors, 0 warnings
                (re-verify 2026-07-21 sau merge + AppDbContext ConfigureForBase)

Test command:
  dotnet test QLDA.Tests -c Release --filter "FullyQualifiedName~Phase3|FullyQualifiedName~Phase4"
  dotnet test BuildingBlocks.Tests -c Release --filter "FullyQualifiedName~Attachments|Phase3|Phase4"
QLDA.Tests:     Passed: 14, Failed: 0
BB.Tests:       Passed: 23, Failed: 0
```

### Migration safety

```text
git diff --name-only *Migration* *ModelSnapshot*:  (empty — không đụng migration)
```

---

## Smoke test (runtime)

| # | Kiểm tra | Trạng thái |
|---|----------|------------|
| 1–10 | API upload/get/download với DB thật | **Chưa chạy** — không có DB/token trong session |
| Mapping source | `Attachment` → table `TepDinhKem` via `AppDbContext` | ✅ Verified by code |
| Không sinh table `Attachments` trên QLDA | BB config excluded + ExcludeFromMigrations | ✅ Verified |
| API contract `DanhSachTepDinhKem` | DTO giữ nguyên | ✅ Verified |

---

## Deferred

- **Phase 6** — đã hoàn thành (xem `phase-6-implementation.md`); lưu ý `docs/code-standards.md` §14 vẫn có thể ghi `QLDA.AttachmentConfiguration` — nên đồng bộ sang `AppDbContext` nếu chỉnh docs
- **Commit/push** — user tự commit khi sẵn sàng (`AppDbContext` có thay đổi uncommitted nếu chưa commit merge fix)
- **Runtime API smoke test** — cần môi trường DB

---

## Điều kiện hoàn thành Phase 5 — Checklist

### Entity và architecture
- [x] Chỉ còn runtime entity `Attachment`
- [x] Không còn runtime entity `TepDinhKem`
- [x] QLDA dùng `IRepository<Attachment, Guid>`
- [x] BuildingBlocks Application dùng module `Attachments`
- [x] Không còn implementation duplicate

### Persistence
- [x] `AppDbContext` force map `Attachment` → bảng `TepDinhKem` (+ `ConfigureForBase`)
- [x] `QLDA.AttachmentConfiguration` đã xóa (tránh race với BB — `3e70b87`)
- [x] BB `AttachmentConfiguration` bị exclude khỏi `ApplyConfigurationsFromAssembly` của QLDA
- [x] Không tạo bảng `Attachments` trên QLDA (ExcludeFromMigrations + không apply BB config)
- [x] Shared context không generate DDL trùng
- [x] Không còn configuration reference entity đã xóa

### Cleanup
- [x] Entity `TepDinhKem` duplicate đã xóa
- [x] Command/query/handler cũ không còn caller đã xóa
- [x] Configuration cũ đã xóa
- [x] `SignedHelper.cs` đã xóa
- [x] `SyncHelper.cs` (QLDA) đã xóa
- [x] DTO compatibility vẫn được giữ

### Safety
- [x] Không sửa business entity
- [x] Không đổi API contract
- [x] Không tạo migration
- [x] Không sửa migration cũ / ModelSnapshot

### Build
- [x] `dotnet build SER.sln -c Release --nologo` — 0 error
