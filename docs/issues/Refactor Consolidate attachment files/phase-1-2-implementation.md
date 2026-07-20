# Phase 1–2 Implementation Log

> **Phạm vi đợt này:** chỉ Phase 1 (khảo sát + baseline) và Phase 2 (BuildingBlocks.Application.Attachments).
> **Không commit** trong đợt này — chờ user tự commit.
> **Không** thực hiện Phase 3–6.

Ngày: 2026-07-20

---

## Phase 1 — Kết quả khảo sát

### 1.1 Baseline build

```bash
dotnet build SER.sln -c Release --nologo
```

| Mục | Kết quả |
|---|---|
| Build result | **Succeeded** |
| Errors | 0 |
| Warnings | 0 |
| Thời gian | ~2.6s |

### 1.2 Entity & Persistence hiện có

| Layer | File | Ghi chú |
|---|---|---|
| Domain | `BuildingBlocks.Domain/Entities/Attachment.cs` | Entity mới (POCO), schema: ParentId, GroupId, GroupType, Type, FileName, OriginalName, Path, Size |
| Domain | `BuildingBlocks.Domain/Entities/TepDinhKem.cs` | Entity cũ, cùng schema — vẫn tồn tại song song |
| Persistence BB | `BuildingBlocks.Persistence/Configurations/AttachmentConfiguration.cs` | `ToTable("Attachments")` |
| Persistence QLDA | `QLDA.Persistence/Configurations/AttachmentConfiguration.cs` | Override `ToTable("TepDinhKem")` — DB giữ tên cũ |
| Persistence BB | `BuildingBlocks.Persistence/Configurations/TepDinhKemConfiguration.cs` | Config entity cũ |

### 1.3 Application layer hiện có (trước Phase 2)

**BuildingBlocks.Application.Attachments/** (stub, chưa đủ):

| File | Trạng thái trước Phase 2 |
|---|---|
| `DTOs/AttachmentDto.cs` | Có 9 fields; `Id` là `Guid` (non-null); JsonIgnore trên ParentId/GroupId/GroupType |
| `DTOs/AttachmentMapping.cs` | ToDto / ToEntities — **không** resolve ký số theo ParentId |
| `DTOs/AttachmentInsertModel.cs` | Có sẵn |
| `DTOs/AttachmentInsertOrUpdateModel.cs` | Có sẵn |
| `Commands/AttachmentInsertCommand.cs` | Insert only + HasTransaction |
| `Commands/AttachmentBulkInsertOrUpdateCommand.cs` | Stub: sync theo **GroupId only**, thiếu GroupTypes, thiếu AutoDeleteMissing |
| `Queries/GetAttachmentsQuery.cs` | Filter `GroupId` + optional `GroupTypes` exact — chưa có IncludeSigned |

**BuildingBlocks.Application.TepDinhKems/** — giữ nguyên (Phase 2 không xóa):

- Commands / Queries / DTOs cũ vẫn build được.

**QLDA.Application.TepDinhKems/** — giữ nguyên:

- `TepDinhKemDto` (có TenNguoiTao + audit fields riêng QLDA)
- `TepDinhKemBulkInsertOrUpdateCommand` (đã dùng `IRepository<Attachment, Guid>` + `ScopeGroupTypes` optional)
- Mapping qua `SignedHelper.ResolveSignedGroupType`

### 1.4 Module / reference bị ảnh hưởng (Phase 3+ — chưa đụng)

| Khu vực | Ước lượng | Pattern |
|---|---|---|
| Controllers QLDA dùng bulk insert file | ~40–50 | `TepDinhKemBulkInsertOrUpdateCommand` |
| Query handlers GetDanhSach + subquery file | ~40+ | `IRepository<TepDinhKem\|Attachment>` + `SignedHelper.Prefix` |
| WebApi Models MappingConfiguration | ~30 | `GetDanhSachTepDinhKem(model, groupId)` per-entity |
| Application feature Mappings | ~15 | `ToEntities` + `ResolveSignedGroupType` |

### 1.5 Logic ký số & GroupType hiện tại

- Helper: `QLDA.Application/Common/SignedHelper.cs`
  - `Prefix = "KySo_"`
  - `ResolveSignedGroupType(base, isChild)` — tránh double prefix
  - `ToBaseGroupType`, `IsSignedVariant`, `WithSignedVariant` (trả `string[]`), `WhereSignedScope`
- Convention thực tế trong mapping QLDA:
  - `ParentId == null` → `GroupType = base`
  - `ParentId != null` → `GroupType = KySo_ + base`
- Cùng `GroupId` có thể có nhiều `GroupType` (vd BanGiaoHoSo + BienBanBanGiao)

### 1.6 Transaction pattern hiện tại

- Controllers / handlers: kiểm tra `_unitOfWork.HasTransaction`
  - **Có tx** → chỉ thao tác, không commit
  - **Không tx** → `BeginTransaction` → work → `SaveChanges` → `Commit`
- Sync file: `BuildingBlocks.Application.Common.SyncHelper.SyncCollection` (soft-delete mặc định)
- QLDA cũng có bản `QLDA.Application/Common/SyncHelper.cs` (duplicate — xóa ở Phase sau)

### 1.7 Rủi ro phát hiện (chuẩn bị Phase 3+)

| # | Rủi ro | Mức | Ghi chú |
|---|---|---|---|
| 1 | Sync theo GroupId không filter GroupType → xóa nhầm loại file khác | **HIGH** | Stub AttachmentBulk cũ có bug này; Phase 2 đã siết scope |
| 2 | `AutoDeleteMissing` mặc định nếu = true sẽ xóa file khi UI gửi list thiếu | **HIGH** | Phase 2: default = **false** |
| 3 | Double prefix `KySo_KySo_` nếu resolve 2 lần | MEDIUM | `ResolveSignedGroupType` chặn nếu đã có Prefix |
| 4 | Entity `TepDinhKem` + `Attachment` song song → confuse DI/EF | MEDIUM | Cleanup ở Phase 5 |
| 5 | Spill field QLDA (`TenNguoiTao`) vào DTO chung | MEDIUM | Phase 2: `IAttachmentDto` chỉ 9 core fields |
| 6 | Subquery trong `.Select()` không gọi Mediator | MEDIUM | Phase 2 thêm `AttachmentSubquery` (chưa refactor callers) |

---

## Phase 2 — Những phần đã triển khai

### 2.1 Mục tiêu

Hoàn thiện **BuildingBlocks.Application.Attachments** theo hướng **backward-compatible**:

- QLDA code cũ vẫn build / chạy như trước
- **Không** đổi API request/response
- **Không** rename `DanhSachTepDinhKem` / xóa `TepDinhKemDto`
- **Không** xóa `SignedHelper.cs` / `SyncHelper.cs` / entity cũ
- **Không** migration / snapshot

### 2.2 File thêm mới

| File | Vai trò |
|---|---|
| `Attachments/Common/IAttachmentDto.cs` | Interface 9 core fields |
| `Attachments/Common/SignedGroupTypeHelper.cs` | Copy logic từ QLDA SignedHelper (BB bản riêng; QLDA SignedHelper **giữ nguyên**) |
| `Attachments/Common/AttachmentCollectionExtensions.cs` | `ToEntities`, `SyncAttachmentsAsync`, `BaseGroupType()` |
| `Attachments/Common/AttachmentSubquery.cs` | `ForGroupTypes` / `ForExactGroupType` / `OriginalOnly` / `SignedOnly` / `ForGroupId` |
| `Attachments/Validators/AttachmentBulkInsertOrUpdateCommandValidator.cs` | FluentValidation: GroupId + GroupTypes bắt buộc |

### 2.3 File đã chỉnh sửa

| File | Thay đổi chính |
|---|---|
| `Attachments/DTOs/AttachmentDto.cs` | Implement `IAttachmentDto`; `Id` → `Guid?`; bỏ JsonIgnore |
| `Attachments/DTOs/AttachmentMapping.cs` | `ToDto<T>()` generic; `ToEntities(..., baseGroupType)` **bắt buộc** + resolve ký số |
| `Attachments/Commands/AttachmentBulkInsertOrUpdateCommand.cs` | Viết lại: `GroupTypes` bắt buộc, `Entities`, `AutoDeleteMissing=false`, HasTransaction |
| `Attachments/Queries/GetAttachmentsQuery.cs` | `GroupIds` + `BaseGroupTypes` + `IncludeSigned=true` |

### 2.4 Cơ chế bắt buộc GroupType / GroupTypes

```csharp
public required List<string> GroupTypes { get; set; }  // trên command

// Handler Validate:
if (GroupTypes null/empty) throw ManagedException("GroupTypes là bắt buộc...");

// Validator:
RuleFor(x => x.GroupTypes).NotEmpty()...
```

Sync chỉ load/xử lý trong tập:

```text
∀ gt ∈ GroupTypes: { gt, KySo_gt }
```

File thuộc GroupType khác cùng GroupId → **không đụng**; nếu lọt vào existing/submitted ngoài scope → **throw**.

### 2.5 Cơ chế AutoDeleteMissing

| Giá trị | Hành vi |
|---|---|
| `false` (default) | Chỉ insert + update. Existing trong scope nhưng không có trong request → **giữ nguyên** (thu hẹp `existingForSync` về intersection Ids trước khi gọi SyncCollection) |
| `true` | Soft-delete các file trong scope không có trong request (qua SyncHelper) |

### 2.6 Scope của SyncAttachmentsAsync

```csharp
await repository.SyncAttachmentsAsync(
    existing: null,                 // null = tự load theo GroupId + scope
    submitted: entities,
    baseGroupTypes: [...],          // bắt buộc
    autoDeleteMissing: false,
    groupId: request.GroupId,       // cần khi submitted rỗng + AutoDeleteMissing
    cancellationToken);
```

Luồng:

1. Build `allGroupTypes = base ∪ KySo_base`
2. Load existing (nếu null) bằng `GetOriginalSet().Where(GroupId + GroupType ∈ scope)`
3. Validate submitted/existing cùng GroupId và trong scope
4. SyncCollection với updateAction (copy fields + re-derive GroupType theo ParentId)

### 2.7 Transaction trong handler

```csharp
if (_unitOfWork.HasTransaction)
{
    // Tham gia tx ngoài — KHÔNG SaveChanges/Commit
    await InsertOrUpdateAsync(...);
}
else
{
    using var tx = await _unitOfWork.BeginTransactionAsync(ReadCommitted, ...);
    await InsertOrUpdateAsync(...);
    await _unitOfWork.SaveChangesAsync(...);
    await _unitOfWork.CommitTransactionAsync(...);
}
```

### 2.8 Convention ký số (SignedGroupTypeHelper)

```text
ParentId == null  →  baseGroupType
ParentId != null  →  KySo_<baseGroupType>
Đã có prefix KySo_ → không thêm lần nữa (tránh KySo_KySo_)
```

API chính:

| Method | Ý nghĩa |
|---|---|
| `ResolveSignedGroupType(base, isChild)` | Resolve write-side |
| `ToBaseGroupType()` | Strip prefix |
| `IsSignedVariant()` | Check prefix |
| `WithSignedVariant(base)` | Trả **string** `KySo_base` (khác QLDA SignedHelper trả `string[]`) |
| `ExpandWithSignedVariant(base)` | Trả `[base, KySo_base]` |
| `WhereSignedScope(q, groupId, base)` | Filter IQueryable |

> **Lưu ý tương thích:** QLDA vẫn dùng `QLDA.Application.Common.SignedHelper`. BB có bản riêng để Phase 3+ chuyển dần — **không** xóa SignedHelper trong đợt này.

### 2.9 GetAttachmentsQuery (read helper)

```csharp
new GetAttachmentsQuery(
    GroupIds: [id.ToString()],
    BaseGroupTypes: ["BanGiaoHoSo"],  // optional
    IncludeSigned: true               // default: tự thêm KySo_ variant
);
```

`IncludeSigned=false` → exact match BaseGroupTypes (không auto-add KySo_).

### 2.10 Các bước code đã làm (checklist)

- [x] **Bước 1** — Baseline build Release (Phase 1)
- [x] **Bước 2** — Survey entity / DTO / command / SignedHelper / SyncHelper / refs
- [x] **Bước 3** — Tạo `IAttachmentDto`
- [x] **Bước 4** — Tạo `SignedGroupTypeHelper` (BB)
- [x] **Bước 5** — Tạo `AttachmentCollectionExtensions` (+ SyncAttachmentsAsync)
- [x] **Bước 6** — Tạo `AttachmentSubquery`
- [x] **Bước 7** — Cập nhật `AttachmentDto` + `AttachmentMapping`
- [x] **Bước 8** — Viết lại `AttachmentBulkInsertOrUpdateCommand` + Handler
- [x] **Bước 9** — Thêm Validator
- [x] **Bước 10** — Cập nhật `GetAttachmentsQuery`
- [x] **Bước 11** — `dotnet clean` + `dotnet build` SER.sln Release
- [x] **Bước 12** — `git status` / `diff --stat` / `diff --check`
- [x] **Bước 13** — Viết docs này
- [ ] **DỪNG** — không sang Phase 3

---

## Validation (sau Phase 2)

```text
Build command:
  dotnet clean SER.sln -c Release
  dotnet build SER.sln -c Release --nologo

Build result:
  Succeeded

Errors:
  0

Warnings:
  0
```

### Git (không commit)

```text
Modified:
  BuildingBlocks/.../AttachmentBulkInsertOrUpdateCommand.cs
  BuildingBlocks/.../AttachmentDto.cs
  BuildingBlocks/.../AttachmentMapping.cs
  BuildingBlocks/.../GetAttachmentsQuery.cs

Untracked:
  BuildingBlocks/.../Attachments/Common/
  BuildingBlocks/.../Attachments/Validators/
  docs/issues/Refactor Consolidate attachment files/  (plan + docs này)

git diff --check: OK (no whitespace errors)
```

---

## Chưa thực hiện

- **Phase 3** — chưa: không chuyển controller/handler QLDA sang Attachment; không đổi inject repo; không rename property/DTO.
- **Phase 4** — chưa: không sửa transaction flow controller QLDA; không xóa `SignedHelper`/`SyncHelper` QLDA.
- **Phase 5** — chưa: không migration; không `ExcludeFromMigrations`; không xóa entity `TepDinhKem`; không sửa snapshot.
- **Phase 6** — chưa: không cập nhật docs chuẩn dự án ngoài file log đợt này; không commit/push.

---

## Gợi ý commit (khi user sẵn sàng)

```text
BB: Application - Complete Attachments Phase 2 (GroupTypes, Sync, SignedHelper)

Hoàn thiện BuildingBlocks.Application.Attachments: IAttachmentDto,
SignedGroupTypeHelper, SyncAttachmentsAsync (AutoDeleteMissing=false),
BulkInsertOrUpdate bắt buộc GroupTypes + HasTransaction. Backward-compatible.
```

Không include `bin/` / `obj/` / migration / QLDA business changes.
