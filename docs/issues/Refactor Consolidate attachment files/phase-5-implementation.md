# Phase 5 — EF Core Mapping & Cleanup `TepDinhKem`

> **Trạng thái:** Hoàn thành  
> **Branch:** `151-refactor-consolidate-attachment-files`  
> **Ngày:** 2026-07-20  
> **Phạm vi:** Chỉ Phase 5 (không Phase 6, không commit tự động)

---

## Summary

Phase 5 consolidate runtime entity về `BuildingBlocks.Domain.Entities.Attachment`, map EF vào bảng DB `TepDinhKem`, và xóa toàn bộ implementation duplicate (`TepDinhKem` entity, command/query/handler cũ, helper duplicate). DTO/API compatibility (`TepDinhKemDto`, `DanhSachTepDinhKem`, …) được giữ nguyên.

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

**Thay đổi:** Thêm `ExcludeFromMigrations()` — BB không sở hữu DDL.

```csharp
builder.ToTable("Attachments", t => t.ExcludeFromMigrations());
builder.ConfigureForBase();
```

### 1.2 QLDA `AttachmentConfiguration` (giữ nguyên — owner bảng thật)

**File:** `QLDA.Persistence/Configurations/AttachmentConfiguration.cs`

```csharp
public class AttachmentConfiguration : BuildingBlocks.Persistence.Configurations.AttachmentConfiguration {
    public override void Configure(EntityTypeBuilder<Attachment> builder) {
        builder.ToTable("TepDinhKem");  // ← nơi DUY NHẤT map bảng thật
        builder.ConfigureForBase();
    }
}
```

### 1.3 `AppDbContext` — loại bỏ entity cũ

**File:** `QLDA.Persistence/AppDbContext.cs`

**Trước:**
```csharp
modelBuilder.Entity<TepDinhKem>(e => e.ToTable(t => t.ExcludeFromMigrations()));
modelBuilder.Entity<Attachment>(e => e.ToTable(t => t.ExcludeFromMigrations()));
```

**Sau:**
```csharp
modelBuilder.Entity<Attachment>(e => e.ToTable(t => t.ExcludeFromMigrations()));
```

> `ExcludeFromMigrations` trên `Attachment` trong `AppDbContext` giữ snapshot migration ổn định. Runtime table name vẫn là `TepDinhKem` nhờ QLDA `AttachmentConfiguration` override.

### 1.4 Xóa configuration entity cũ

| File | Hành động |
|------|-----------|
| `BuildingBlocks.Persistence/Configurations/TepDinhKemConfiguration.cs` | **Deleted** |

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
Runtime entity:     BuildingBlocks.Domain.Entities.Attachment
Database table:     TepDinhKem
Owning configuration: QLDA.Persistence.Configurations.AttachmentConfiguration
Shared application module: BuildingBlocks.Application.Attachments
Signed logic (single): BuildingBlocks.Application.Attachments.Common.SignedGroupTypeHelper
Sync logic (single): BuildingBlocks.Application.Common.SyncHelper
```

---

## Files changed

### Added

_(Không có file mới — chỉ doc này)_

### Modified

| File | Mô tả |
|------|-------|
| `BuildingBlocks.Persistence/Configurations/AttachmentConfiguration.cs` | `ExcludeFromMigrations` |
| `BuildingBlocks.Application/Common/SyncHelper.cs` | Xóa TepDinhKem overload, thêm 3-param Attachment overload |
| `QLDA.Persistence/AppDbContext.cs` | Xóa block `Entity<TepDinhKem>` |
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
Migration owner:                    QLDA.Persistence (AttachmentConfiguration)
ExcludeFromMigrations locations:
  - BuildingBlocks.Persistence.AttachmentConfiguration → ToTable("Attachments", ExcludeFromMigrations)
  - QLDA.Persistence.AppDbContext → Entity<Attachment>().ToTable(ExcludeFromMigrations)
Duplicate configurations remaining: 0 (TepDinhKemConfiguration deleted)
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
Build command:  dotnet clean SER.sln -c Release && dotnet build SER.sln -c Release --nologo
Build result:   SUCCESS — 0 errors, 0 warnings

Test command:   dotnet test SER.sln -c Release --no-build --filter "FullyQualifiedName~Phase3|FullyQualifiedName~Phase4"
QLDA.Tests:     Passed: 14, Failed: 0
BB.Tests:       Passed: 13, Failed: 0
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
| Mapping source | `Attachment` → table `TepDinhKem` | ✅ Verified by code |
| Không sinh table `Attachment` | BB ExcludeFromMigrations | ✅ Verified |
| API contract `DanhSachTepDinhKem` | DTO giữ nguyên | ✅ Verified |

---

## Deferred

- **Phase 6** — cập nhật `docs/code-standards.md`, `BuildingBlocks/CLAUDE.md`, `QLDA/CLAUDE.md`
- **Commit/push** — user tự commit khi sẵn sàng
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
- [x] QLDA `AttachmentConfiguration` map bảng `TepDinhKem`
- [x] Không tạo bảng `Attachment` (ExcludeFromMigrations ở BB)
- [x] Shared context không generate DDL trùng
- [x] `ExcludeFromMigrations()` đúng DbContext
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
