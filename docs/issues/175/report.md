# Issue #175 — Report: Trình duyệt dự toán (`DuToanDauTu`)

> **Status:** CODE DONE — chờ user migration + FE  
> **Date:** 2026-08-11  
> **API:** `api/du-toan-dau-tu`  
> **Migration:** **User tự tạo tay** (`ef.bat`) — agent **không** chạy migration / không sửa Migrator  
> **Chi tiết BA + survey:** xem [`index.md`](./index.md)

## Summary

Cập nhật form **Tờ trình phê duyệt dự toán** trên module `DuToanDauTu`:

| Field nghiệp vụ | Cách xử lý | Migration? |
|-----------------|------------|------------|
| Tên dự toán * | Thêm `Ten` trên Entity/DTO/Mapping; validate required | ✅ |
| Phương án thiết kế | Bỏ khỏi form; giữ `PhuongAnThietKeId` trên DB | ❌ |
| Công văn đề nghị báo giá * | Reuse `EGroupType.DuToanDauTu` + validate ≥1 file | ❌ |
| Khác (optional) | Thêm `EGroupType.DuToanDauTu_Khac` + `DanhSachTepDinhKemKhac` | ❌ |

## Architecture

Không đổi route Controller. Follow Clean Architecture + CQRS hiện có:

- Write field → Domain + Mapping + Insert/Update handler validate
- Attachment → BB `AttachmentBulkInsertOrUpdateCommand` / `GetAttachmentsQuery` (pattern `QuyetDinhDuyetDuToan`)
- **Không** tạo Application Service / WebApi Model mới

```text
FE form
  → POST/PUT api/du-toan-dau-tu
  → DuToanDauTuInsert|UpdateCommand (validate Ten)
  → Controller sync 2 GroupType attachments (+ validate ≥1 Công văn)
  → GET chi-tiet hydrate 2 list theo BaseGroupTypes
```

## Design chi tiết

### 1. `Ten` (Tên dự toán)

| Layer | Thay đổi |
|-------|----------|
| Domain | `string? Ten` hoặc `string Ten` trên `DuToanDauTu` — đề xuất `string Ten = ""` + MaxLength config |
| Persistence | `HasMaxLength` (gợi ý 500, align entity tên tương tự); **không** NOT NULL cứng nếu có data cũ — validate ở Application |
| DTO | `Ten` trên `DuToanDauTuDto` |
| Mapping | ToEntity / ToDto / list projection |
| Insert/Update | `ManagedException.ThrowIf(string.IsNullOrWhiteSpace(dto.Ten), "…")` |
| Migration | **User tự làm** sau khi Domain + EF Config có `Ten` — xem mục Migration bên dưới |

### 2. Phương án thiết kế

- FE: ẩn control.
- BE: **không** xóa property/FK; Update có thể thôi gán bắt buộc (payload không gửi → giữ/null tùy policy hiện tại — **không** force clear trừ khi FE gửi null có chủ đích).
- Scope tối thiểu: không thêm validate bắt buộc; không expose như field nghiệp vụ mới.

### 3. Attachments

| UI | GroupType | DTO |
|----|-----------|-----|
| Công văn đề nghị báo giá * | `DuToanDauTu` (reuse) | `DanhSachTepDinhKem` |
| Khác | `DuToanDauTu_Khac` (**mới**) | `DanhSachTepDinhKemKhac` |

**Controller Create/Update**

```csharp
// Validate trước sync
ManagedException.ThrowIf(
    dto.DanhSachTepDinhKem == null || dto.DanhSachTepDinhKem.Count == 0,
    "Công văn đề nghị báo giá là bắt buộc");

// Sync Công văn
await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
    GroupId = entity.Id.ToString(),
    GroupTypes = [nameof(EGroupType.DuToanDauTu)],
    Entities = dto.DanhSachTepDinhKem.ToEntities(entity.Id, EGroupType.DuToanDauTu).ToList(),
    AutoDeleteMissing = true
});

// Sync Khác (optional — list null/empty OK)
await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
    GroupId = entity.Id.ToString(),
    GroupTypes = [nameof(EGroupType.DuToanDauTu_Khac)],
    Entities = dto.DanhSachTepDinhKemKhac?.ToEntities(entity.Id, EGroupType.DuToanDauTu_Khac).ToList() ?? [],
    AutoDeleteMissing = true
});
```

**Get chi tiết:** 2 lần `GetAttachmentsQuery` với `BaseGroupTypes` riêng → map vào 2 list trên DTO.

**List:** subquery dùng `AttachmentSubquery.ExpandGroupTypes` — không lấy nhầm `_Khac` vào list Công văn.

**Lưu ý data cũ:** file đã lưu dưới `DuToanDauTu` → thuộc khu **Công văn** sau đổi (hợp lý nếu trước đây là upload chính). Không cần SQL migrate GroupType.

## Files dự kiến sửa

### BE (trong repo)

| File | Việc |
|------|------|
| `QLDA.Domain/Entities/DuToanDauTu.cs` | + `Ten` |
| `QLDA.Persistence/Configurations/DuToanDauTuConfiguration.cs` | MaxLength `Ten` |
| `QLDA.Domain/Enums/EGroupType.cs` | + `DuToanDauTu_Khac` |
| `QLDA.Application/DuToanDauTu/DTOs/DuToanDauTuDto.cs` | + `Ten`, + `DanhSachTepDinhKemKhac` |
| `QLDA.Application/DuToanDauTu/DuToanDauTuMappings.cs` | map `Ten`; filter files theo GroupType khi ToDto |
| `QLDA.Application/DuToanDauTu/Commands/DuToanDauTuInsertCommand.cs` | validate `Ten` |
| `QLDA.Application/DuToanDauTu/Commands/DuToanDauTuUpdateCommand.cs` | validate `Ten`; gán `Ten` |
| `QLDA.Application/DuToanDauTu/Queries/DuToanDauTuGetPaginatedQuery.cs` | project `Ten`; split attachment GroupTypes |
| `QLDA.WebApi/Controllers/DuToanDauTuController.cs` | validate Công văn; sync/load 2 GroupType |
| `QLDA.Migrator/Migrations/<new>_….cs` | **User tự** `ef.bat` — không nằm trong scope agent code |

### FE (repo ngoài)

| Việc | Ghi chú |
|------|---------|
| Input Tên dự toán * | Bind `ten` |
| Ẩn Phương án thiết kế | Không gửi bắt buộc |
| Upload Công văn * | Bind `danhSachTepDinhKem` |
| Upload Khác | Bind `danhSachTepDinhKemKhac` |

### Không sửa

- Migration cũ / sửa tay ModelSnapshot
- Drop `PhuongAnThietKeId`
- `PheDuyetDuToan` / `QuyetDinhDuyetDuToan` (module khác)
- Refactor ngoài phạm vi #175

## Checklist bước code (khi được duyệt)

- [x] 0. Branch (theo convention issue) — dùng branch hiện tại
- [x] 1. Domain `Ten`
- [x] 2. EF Configuration `Ten`
- [ ] 3. Migration — **bỏ qua (user tự làm)**
- [x] 4. `EGroupType.DuToanDauTu_Khac`
- [x] 5. DTO + Mapping
- [x] 6. Insert/Update validate `Ten`
- [x] 7. Controller: validate Công văn + sync 2 GroupType
- [x] 8. Get chi tiết hydrate 2 list
- [x] 9. List projection
- [x] 10. Build Domain / Application / Persistence / WebApi — 0 error
- [ ] 11. Commit / PR (khi user yêu cầu; nhớ gộp Migrator khi user đã add)

## Migration — **USER tự tạo tay**

> Agent **không** chạy `ef.bat`, **không** sửa `QLDA.Migrator/Migrations/*`, **không** sửa tay `AppDbContextModelSnapshot.cs`.

Sau khi Domain + `DuToanDauTuConfiguration` đã có `Ten`, user chạy:

```bat
ef.bat QLDA add AddDuToanDauTuTen
```

Cột đề xuất trên `DuToanDauTu`:

- `Ten` `nvarchar(…)` NULL hoặc `''` default — Application bắt buộc; data cũ backfill sau nếu cần

Commit group (rule dự án): **Domain + Persistence.Configuration + Migrator** cùng một commit khi đã có file migration.

## Rủi ro / điểm đã chốt tạm

1. **GroupType Công văn = reuse `DuToanDauTu`** — tránh orphan file cũ; tên enum không mirror label UI (OK, FE label riêng).
2. **Get hiện không filter BaseGroupTypes** — sau khi thêm `_Khac` **bắt buộc** filter, nếu không file Khác lẫn vào `DanhSachTepDinhKem`.
3. **Validate attachment ở Controller** vì sync file nằm Controller (pattern hiện tại module này); `Ten` validate ở Command.
4. **`TongMucDauTu` ↔ label “Tổng dự toán thẩm định giá”** — ngoài scope rename; ghi nhận mapping hiện có.
