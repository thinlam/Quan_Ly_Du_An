# Report #9583 — Phê duyệt dự toán giai đoạn chuẩn bị đầu tư

## 1. Tổng quan

Triển khai workflow trạng thái cho màn **Phê duyệt dự toán** — cho phép phòng KH-TC trình duyệt, Ban Giám đốc (BGĐ) duyệt/trả lại.

```
Dự thảo → Đã trình → Đã duyệt
                ↘ Trả lại → (sửa) → Đã trình
```

## 2. Đã triển khai

### 2.1 Domain

| File | Mô tả |
|------|-------|
| `QLDA.Domain/Entities/PheDuyetDuToan.cs` | Entity chính, kế thừa `VanBanQuyetDinh`, bổ sung `TrangThaiId`, `NguoiXuLyId`, `NguoiGiaoViecId` |
| `QLDA.Domain/Entities/PheDuyetDuToanHistory.cs` | Lịch sử trình/duyệt/trả lại (người xử lý, trạng thái, nội dung, ngày) |
| `QLDA.Domain/Entities/DanhMuc/DanhMucTrangThaiPheDuyetDuToan.cs` | Danh mục trạng thái |
| `QLDA.Domain/Constants/TrangThaiPheDuyetDuToanCodes.cs` | Hằng số mã trạng thái: `DT`, `ĐTr`, `ĐD`, `TL`, `LEG` |

### 2.2 Database

| Table | Seed data |
|-------|-----------|
| `PheDuyetDuToan` | Column `TrangThaiId` default = 5 (LEG - migrated) |
| `PheDuyetDuToanHistory` | FK → `PheDuyetDuToan`, `DuAn`, `DmTrangThaiPheDuyetDuToan` |
| `DmTrangThaiPheDuyetDuToan` | 5 row: DT(1), ĐTr(2), ĐD(3), TL(4), LEG(5) |

### 2.3 Application — Commands (Workflow)

| Command | Permission | Logic |
|---------|-----------|-------|
| `PheDuyetDuToanTrinhCommand` | PhongBanID=219 (KH-TC) | Status phải DT hoặc TL → chuyển sang ĐTr, ghi history |
| `PheDuyetDuToanDuyetCommand` | Role BGĐ | Status phải ĐTr → chuyển sang ĐD, set `NguoiGiaoViecId`, ghi history |
| `PheDuyetDuToanTraLaiCommand` | Role BGĐ | Status phải ĐTr → chuyển sang TL, `NoiDung` bắt buộc, ghi history |
| `PheDuyetDuToanInsertOrUpdateCommand` | Tất cả | Insert/update trong transaction, validate DuAnId + ChucVuId |

### 2.4 Application — Queries

| Query | Mô tả |
|-------|-------|
| `PheDuyetDuToanGetQuery` | Chi tiết theo Id, include `TrangThai`, `NguoiXuLyId`, `NguoiGiaoViecId` |
| `PheDuyetDuToanGetDanhSachQuery` | Danh sách phân trang, filter theo `DuAnId`, `BuocId`, `GlobalFilter` |

### 2.5 API Endpoints

| Method | URL | Body | Permission | Precondition |
|--------|-----|------|-----------|-------------|
| `GET` | `/api/phe-duyet-du-toan/{id}/chi-tiet` | — | Auth | — |
| `GET` | `/api/phe-duyet-du-toan/danh-sach-tien-do?duAnId=&buocId=&globalFilter=&pageIndex=&pageSize=` | — | Auth | — |
| `POST` | `/api/phe-duyet-du-toan/them-moi` | `PheDuyetDuToanModel` JSON | Auth | — |
| `PUT` | `/api/phe-duyet-du-toan/cap-nhat` | `PheDuyetDuToanModel` JSON | Auth | — |
| `DELETE` | `/api/phe-duyet-du-toan/{id}/xoa` | — | Auth | — |
| `POST` | `/api/phe-duyet-du-toan/{id}/trinh` | — | PhongBanID=219 | Status = DT hoặc TL |
| `POST` | `/api/phe-duyet-du-toan/{id}/duyet` | — | Role BGĐ | Status = ĐTr |
| `POST` | `/api/phe-duyet-du-toan/{id}/tra-lai` | `{ "noiDung": "..." }` | Role BGĐ | Status = ĐTr |

### 2.6 DTOs

#### PheDuyetDuToanDto (Response)

```json
{
  "id": "guid",
  "duAnId": "guid",
  "buocId": "int?",
  "soVanBan": "string?",
  "ngayKy": "datetimeoffset?",
  "nguoiKy": "string?",
  "chucVuId": "int?",
  "giaTriDuThau": "long?",
  "trichYeu": "string?",
  "trangThaiId": "int",        // 1=DT, 2=ĐTr, 3=ĐD, 4=TL
  "nguoiXuLyId": "long?",      // UserPortalId người trình
  "nguoiGiaoViecId": "long?",  // UserPortalId người duyệt
  "danhSachTepDinhKem": [...]
}
```

#### PheDuyetDuToanModel (Request — them-moi / cap-nhat)

```json
{
  "id": "guid?",            // null = tạo mới, có giá trị = cập nhật
  "duAnId": "guid",
  "buocId": "int?",
  "soVanBan": "string?",
  "ngayKy": "datetimeoffset?",
  "nguoiKy": "string?",
  "chucVuId": "int?",
  "giaTriDuThau": "long?",
  "trichYeu": "string?",
  "danhSachTepDinhKem": [...]
}
```

#### TraLaiModel (Request — tra-lai)

```json
{
  "noiDung": "string"   // Bắt buộc, không rỗng
}
```

### 2.7 Permission Matrix

| Action | KH-TC (PhongBanID=219) | BGĐ (Role) | Khác |
|--------|:----------------------:|:----------:|:----:|
| Tạo mới (them-moi) | ✅ | ✅ | ✅ |
| Cập nhật (cap-nhat) | ✅ | ✅ | ✅ |
| Xóa (xoa) | ✅ | ✅ | ✅ |
| **Trình** (trinh) | **✅** | ❌ | ❌ |
| **Duyệt** (duyet) | ❌ | **✅** | ❌ |
| **Trả lại** (tra-lai) | ❌ | **✅** | ❌ |

### 2.8 Status Codes (DmTrangThaiPheDuyetDuToan)

| Id | Ma | Tên | Mô tả |
|----|-----|-----|-------|
| 1 | `DT` | Dự thảo | Ban đầu, có thể sửa/xóa |
| 2 | `ĐTr` | Đã trình | Đã gửi duyệt, chờ BGĐ |
| 3 | `ĐD` | Đã duyệt | Đã phê duyệt xong |
| 4 | `TL` | Trả lại | BGĐ trả lại, KH-TC sửa rồi trình lại |
| 5 | `LEG` | Migrated | Dữ liệu cũ migrate sang |

### 2.9 Response Format

```json
// Success
{ "result": true, "data": ..., "message": null }

// Error (ManagedException → HTTP 200, result=false)
{ "result": false, "data": null, "message": "Chỉ phòng KH-TC có quyền trình phê duyệt dự toán" }
```

**Lưu ý quan trọng:** ManagedException trả HTTP 200, không phải HTTP 4xx. FE phải check field `result`.

### 2.10 Integration Tests

File: `QLDA.Tests/Integration/PheDuyetDuToanControllerTests.cs`

| Test | Mô tả |
|------|-------|
| `GetChiTiet_ExistingId_ReturnsOk` | Lấy chi tiết thành công |
| `GetChiTiet_NonExistentId_ReturnsFailure` | Id không tồn tại → result=false |
| `Trinh_AsKhTcUser_ReturnsOk` | KH-TC trình thành công |
| `Trinh_AsNonKhTcUser_ReturnsFailure` | User khác trình → result=false |
| `Duyet_AsBgdUser_ReturnsOk` | BGĐ duyệt thành công (sau khi đã trình) |
| `Duyet_WithoutTrinh_ReturnsFailure` | Duyệt khi chưa trình → result=false |
| `Duyet_AsNonBgdUser_ReturnsFailure` | User khác duyệt → result=false |
| `TraLai_AsBgdUser_ReturnsOk` | BGĐ trả lại thành công |
| `TraLai_WithoutNoiDung_ReturnsFailure` | Trả lại không có lý do → result=false |
| `Trinh_AfterTraLai_ReturnsOk` | Trình lại sau khi bị trả lại → thành công |

## 3. Gợi ý FE Implementation

### 3.1 Status Badge

| Status | Color | Label |
|--------|-------|-------|
| DT (1) | `gray` / default | Dự thảo |
| ĐTr (2) | `blue` / warning | Đã trình |
| ĐD (3) | `green` / success | Đã duyệt |
| TL (4) | `red` / error | Trả lại |

### 3.2 Button Visibility Logic

```
// Nút "Trình phê duyệt"
IF trangThaiId == 1 (DT) && currentUser.PhongBanID == 219:
    SHOW button "Trình phê duyệt"

// Nút "Trình lại" (sau khi bị trả lại)
IF trangThaiId == 4 (TL) && currentUser.PhongBanID == 219:
    SHOW button "Trình lại" (enable edit trước)

// Nút "Duyệt" + "Trả lại"
IF trangThaiId == 2 (ĐTr) && currentUser.HasRole("BGĐ"):
    SHOW button "Duyệt"
    SHOW button "Trả lại"
```

### 3.3 Confirm Popups

- **Trình**: Confirm "Bạn có chắc chắn trình phê duyệt dự toán này?"
- **Duyệt**: Confirm "Bạn có chắc chắn duyệt phê duyệt dự toán này?"
- **Trả lại**: Popup nhập lý do (textarea, required) → Confirm "Bạn có chắc chắn trả lại?"

### 3.4 User Info

- `nguoiXuLyId` / `nguoiGiaoViecId` → `USER_MASTER.UserPortalId`, cần join qua API user để hiển thị tên

## 4. Files liên quan

### Domain
- `QLDA.Domain/Entities/PheDuyetDuToan.cs`
- `QLDA.Domain/Entities/PheDuyetDuToanHistory.cs`
- `QLDA.Domain/Entities/DanhMuc/DanhMucTrangThaiPheDuyetDuToan.cs`
- `QLDA.Domain/Constants/TrangThaiPheDuyetDuToanCodes.cs`

### Persistence
- `QLDA.Persistence/Configurations/PheDuyetDuToanConfiguration.cs`
- `QLDA.Persistence/Configurations/PheDuyetDuToanHistoryConfiguration.cs`
- `QLDA.Persistence/Configurations/DanhMuc/DanhMucTrangThaiPheDuyetDuToanConfiguration.cs`

### Application
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanTrinhCommand.cs`
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanDuyetCommand.cs`
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanTraLaiCommand.cs`
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanInsertOrUpdateCommand.cs`
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanInsertCommand.cs`
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanUpdateCommand.cs`
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanDeleteCommand.cs`
- `QLDA.Application/PheDuyetDuToans/Queries/PheDuyetDuToanGetQuery.cs`
- `QLDA.Application/PheDuyetDuToans/Queries/PheDuyetDuToanGetDanhSachQuery.cs`
- `QLDA.Application/PheDuyetDuToans/DTOs/PheDuyetDuToanDto.cs`
- `QLDA.Application/PheDuyetDuToans/DTOs/PheDuyetDuToanInsertDto.cs`
- `QLDA.Application/PheDuyetDuToans/DTOs/PheDuyetDuToanUpdateDto.cs`
- `QLDA.Application/PheDuyetDuToans/PheDuyetDuToanMappings.cs`

### WebApi
- `QLDA.WebApi/Controllers/PheDuyetDuToanController.cs`
- `QLDA.WebApi/Models/PheDuyetDuToans/PheDuyetDuToanModel.cs`
- `QLDA.WebApi/Models/PheDuyetDuToans/PheDuyetDuToanMappingConfiguration.cs`
- `QLDA.WebApi/Models/PheDuyetDuToans/TraLaiModel.cs`

### Tests
- `QLDA.Tests/Integration/PheDuyetDuToanControllerTests.cs`

### Docs
- `docs/feature/PheDuyetDuToan/fe-mapping-report.md` (FE mapping chi tiết)
