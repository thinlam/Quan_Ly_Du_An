# QuanLyPheDuyet Workflow

## Overview

QuanLyPheDuyet (Approval Management) is a **polymorphic workflow system** used by multiple modules. All entities share the same state machine and permission pattern via `PheDuyetDispatch*` commands.

## Supported Entity Types

| Entity | Type Code | Feature |
|--------|-----------|---------|
| PheDuyetDuToan | `PheDuyetDuToan` | Phê duyệt dự toán |
| PhanKhaiKinhPhi | `PhanKhaiKinhPhi` | Phân khai kinh phí |
| HoSoDeXuatCapDoCntt | `HoSoDeXuatCapDoCntt` | Hồ sơ đề xuất cấp độ ATTT |
| HoSoMoiThauDienTu | `HoSoMoiThauDienTu` | Hồ sơ mời thầu điện tử |
| QuyetDinhDieuChinh | `QuyetDinhDieuChinh` | Quyết định điều chỉnh |

## State Machine

```
[Dự thảo] --trình--> [Đã trình] --duyệt--> [Đã duyệt]
                      |
                      +--trả lại--> [Trả lại] --trình--> [Đã trình]
                      |
                      +--từ chối--> [Từ chối]
```

| State | Code | Description |
|-------|------|-------------|
| Dự thảo | `DT` | Initial draft, editable |
| Đã trình | `ĐTr` | Submitted for approval |
| Đã duyệt | `ĐD` | Approved |
| Trả lại | `TL` | Returned with feedback, can re-submit |
| Từ chối | `TC` | Rejected, workflow ends |

## Actions & Permissions

| Action | Endpoint | Permission | From State |
|--------|----------|------------|------------|
| **Tạo mới** | `POST /api/{type}/them-moi` | None (middleware) | — |
| **Cập nhật** | `PUT /api/{type}/cap-nhat` | Owner or staff | `DT`, `null` (legacy), `LEG` (Migrated) |
| **Trình** | `POST /api/phe-duyet/{type}/{id}/trinh` | `PhongBanID == 219` (KH-TC) | `DT`, `null` (legacy), `LEG` (Migrated) |
| **Duyệt** | `POST /api/phe-duyet/{type}/{id}/duyet` | `HasRole(QLDA_LDDV)` | `ĐTr` |
| **Trả lại** | `POST /api/phe-duyet/{type}/{id}/tra-lai` | `HasRole(QLDA_LDDV)` | `ĐTr` |
| **Từ chối** | `POST /api/phe-duyet/{type}/{id}/tu-choi` | `QLDA_LDDV` OR `PhongBanID == P.HC-TH` OR `HasRole(QLDA_QuanTri)` | `ĐTr` |
| **Chuyển phát hành** | `POST /api/phe-duyet/{type}/{id}/chuyen-phat-hanh` | `PhongBanID == P.HC-TH` OR `HasRole(QLDA_LDDV)` | `ĐD` |

## Trạng thái Dự thảo — Chi tiết kỹ thuật

Trạng thái "Dự thảo" trong hệ thống được biểu diễn bởi **3 giá trị khác nhau** tùy nguồn gốc dữ liệu:

| Giá trị | Ý nghĩa | Nguồn |
|---------|---------|-------|
| `null` | Dữ liệu legacy chưa có cột trạng thái | Bản ghi cũ trước khi thêm cột `TrangThaiId` |
| `LEG` (Mã `LEG`) | Bản ghi migrated từ hệ thống cũ, đã mang trạng thái legacy | Dữ liệu migrate, `Used=false` trong seed |
| `DT` (Mã `DT`) | Bản ghi tạo mới với trạng thái Dự thảo tiêu chuẩn | Bản ghi mới |

> **Quy tắc hiển thị:** Cả 3 giá trị trên đều hiển thị là **"Dự thảo"** trên giao diện danh sách và chi tiết. Logic xử lý:
> ```csharp
> // Trong tất cả query danh sách/chi tiết:
> TenTrangThai = (TrangThaiId == null || TrangThai.Ma == "LEG")
>     ? TrangThaiPheDuyetCodes.Default.TenDuThao  // "Dự thảo"
>     : TrangThai.Ten;
> MaTrangThai = (TrangThaiId == null || TrangThai.Ma == "LEG")
>     ? TrangThaiPheDuyetCodes.Default.DuThao      // "DT"
>     : TrangThai.Ma;
> ```

### Trạng thái LEG (Migrated)

- **Mã:** `LEG`
- **Used:** `false` trong seed data — không hiển thị trong combobox chọn trạng thái
- **Mục đích:** Đánh dấu bản ghi đã được migrate từ hệ thống cũ, giữ nguyên trạng thái gốc
- **Không được phép tạo mới:** Chỉ tồn tại trong dữ liệu legacy

## Key Files

**Dispatch (type-based router):**
- `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchTrinhCommand.cs`
- `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchDuyetCommand.cs`
- `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchTraLaiCommand.cs`
- `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchTuChoiCommand.cs`

**Queries:**
- `QLDA.Application/QuanLyPheDuyet/Queries/PheDuyetGetDanhSachQuery.cs`
- `QLDA.Application/QuanLyPheDuyet/Queries/PheDuyetGetChiTietQuery.cs`
- `QLDA.Application/QuanLyPheDuyet/Queries/PheDuyetGetLichSuQuery.cs`

**Controllers:**
- `QLDA.WebApi/Controllers/QuanLyPheDuyetController.cs`

**Config:**
- `QLDA.Domain/Constants/TrangThaiPheDuyetCodes.cs`
- `QLDA.Domain/Constants/PheDuyetEntityNames.cs`
- `QLDA.Domain/Constants/RoleConstants.cs`

## Roles Reference

| Role | Constant | Description |
|------|----------|-------------|
| Admin | `QLDA_TatCa` | Full access |
| System Admin | `QLDA_QuanTri` | System admin |
| Staff | `QLDA_ChuyenVien` | Department staff |
| Unit Leader | `QLDA_LDDV` | Lãnh đạo đơn vị |
| Admin Dept | `QLDA_HC_TH` | Phòng Hành chính - Tổng hợp |
| Planning | `QLDA_KH_TC` | Kế hoạch - Tổng hợp |

## Implementation Guide — Adding a New Entity

### Step 1: Add Entity Name Constant
File: `QLDA.Domain/Constants/PheDuyetEntityNames.cs`
```csharp
public const string MyEntity = "MyEntity";
```

### Step 2: Add Status Codes
File: `QLDA.Domain/Constants/TrangThaiPheDuyetCodes.cs`
```csharp
public static class MyEntity {
    public const string DuThao = "DT";
    public const string DaTrinh = "ĐTr";
    public const string DaDuyet = "ĐD";
    public const string TraLai = "TL";
    public const string TuChoi = "TC";
}
```

### Step 3: Create Commands
Location: `QLDA.Application/MyEntity/Commands/`

| Command | Purpose | Permission |
|---------|---------|------------|
| `MyEntityInsertCommand` | Create new record, set initial `TrangThaiId` to `DuThao` | None |
| `MyEntityUpdateCommand` | Update record, validate status is `DuThao` or `TraLai` | Owner/staff |
| `MyEntityDeleteCommand` | Soft delete, validate status is `DuThao` only | Owner/staff |
| `MyEntityTrinhCommand` | Submit for approval | `PhongBanID == 219` (KH-TC) |
| `MyEntityDuyetCommand` | Approve | `HasRole(QLDA_LDDV)` |
| `MyEntityTraLaiCommand` | Return with reason | `HasRole(QLDA_LDDV)`, requires `NoiDung` |
| `MyEntityTuChoiCommand` | Reject with reason | `QLDA_LDDV` OR `QLDA_QuanTri`, requires `NoiDung` |

### Step 4: Wire into Dispatch Handlers
File: `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatch*Command.cs` (4 files)

Add case to switch expression in each handler:
```csharp
PheDuyetEntityNames.MyEntity => new MyEntityXxxCommand(request.Id, request.NoiDung),
```
Add `using QLDA.Application.MyEntity.Commands;` to each file.

### Step 5: Add to Query Handlers (Optional)
- `PheDuyetGetDanhSachQueryHandler` — add `GetMyEntityItems()` method
- `PheDuyetGetLichSuQueryHandler` — already polymorphic via `EntityName`, no changes needed

### Status Validation Pattern

Each workflow command must validate current status before transitioning:

| Command | Valid From States | Target Status |
|---------|-------------------|---------------|
| `TrinhCommand` | `DT` (or `null`/`LEG` for legacy) | `ĐTr` |
| `DuyetCommand` | `ĐTr` | `ĐD` |
| `TraLaiCommand` | `ĐTr` | `TL` |
| `TuChoiCommand` | `ĐTr` | `TC` |

```csharp
// Example: TrinhCommand validate
var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(...)
    .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.MyEntity.DaTrinh && s.Loai == PheDuyetEntityNames.MyEntity);
ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id) {
    throw new ManagedException("Chỉ có thể trình khi trạng thái là Dự thảo");
}
```

### Validation Rules

1. **NoiDung (reason) validation:** Use `string.IsNullOrWhiteSpace()` not `== null` or `ManagedException.ThrowIfNull`
2. **Status code lookup:** Always use `TrangThaiPheDuyetCodes.{Entity}.{Code}` constant, never hardcode strings like `"DDC"`, `"CTD"`, `"CPD"`, `"DPD"`
3. **Null-safe status comparison:** Use `trangThaiXxx?.Id` and handle nullable properly
4. **Insert:** New records auto-assign `TrangThaiId = trangThaiDuThao.Id`
5. **Update/Delete:** Validate against `DuThao` (default initial status)
6. **Workflow:** Validate current status before transition (never assume current status is non-null)