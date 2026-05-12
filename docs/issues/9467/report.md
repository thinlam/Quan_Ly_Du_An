# UC40 - Implementation Report: Phân khai kinh phí

## Issue #9467 | Branch: `feature/9467-phan-khai-kinh-phi`
## Status: MERGED via PR #61 (2026-05-11)

## Summary

UC40 — **Phân khai kinh phí cho các nội dung được giao dự toán.**

CB.PKH-TC và LD.PKH-TC thực hiện phân khai kinh phí cho các dự án/dự toán đã được cấp kinh phí. Entity `PhanKhaiKinhPhi` lưu thông tin phân khai theo từng `DuAn`, hỗ trợ CRUD operations qua REST API.

## Use Case Description

| Actor | Actions |
|-------|---------|
| CB.PKH-TC, LD.PKH-TC | Lọc danh sách dự án/dự toán trình xin giao kinh phí |
| CB.PKH-TC, LD.PKH-TC | Kết xuất danh sách ra Excel theo mẫu |
| CB.PKH-TC, LD.PKH-TC | Import file Excel phân khai vốn sau khi phân bổ cho Trung Tâm |
| CB.PKH-TC, LD.PKH-TC | Nhập thông tin phân khai cho dự án/dự toán được cấp kinh phí |
| CB.PKH-TC, LD.PKH-TC | Xin ý kiến các phòng chuyên môn về QĐ giao nhiệm vụ chủ trì + hình thức QLDA |
| CB.PKH-TC, LD.PKH-TC | Tạo phiếu trình đề xuất phân khai kinh phí (theo mẫu hoặc đính kèm file) |
| CB.PKH-TC, LD.PKH-TC | Tạo dự thảo QĐ phân khai kinh phí (theo mẫu hoặc đính kèm file) |
| CB.PKH-TC, LD.PKH-TC | Tạo phiếu trình đề xuất giao nhiệm vụ cho Phòng chủ trì |
| CB.PKH-TC, LD.PKH-TC | Tạo dự thảo QĐ giao nhiệm vụ cho dự án giao vốn mới |
| LD.PKH-TC | Ký số phiếu trình hoặc đính kèm file |
| CB.PKH-TC, LD.PKH-TC | Chuyển trình BGĐ Trung tâm |

> **Giao diện:** Nguồn vốn chọn từ Kế hoạch vốn → Kinh phí đề xuất theo nguồn vốn trong Kế hoạch vốn.

> **Phòng PCT** có chức năng: Xem, Chỉnh sửa (đã trình thì không sửa), Trình (cho phòng BGĐ), Xóa (đã trình thì không xóa).

---

## Architecture

### Entity: PhanKhaiKinhPhi

| Field | Type | Mô tả |
|-------|------|-------|
| `Id` | `Guid` | PK |
| `DuAnId` | `Guid` | FK → DuAn |
| `SoToTrinh` | `string` | Số phiếu trình |
| `NgayToTrinh` | `DateTimeOffset` | Ngày trình |
| `NguonVonId` | `int` | FK → NguonVon |
| `KinhPhiDeXuat` | `long` | Kinh phí đề xuất |
| `KinhPhiPhanKhai` | `long` | Kinh phí phân khai |

### DTOs

- `PhanKhaiKinhPhiModel` — request/response model (API layer)
- `PhanKhaiKinhPhiDto` — list item DTO, có thêm `TenNguonVon`, `MaTrangThai`, `TenTrangThai`

---

## API Endpoints (FE Mapping)

### 1. Chi tiết phân khai kinh phí

```
GET /api/phan-khai-kinh-phi/{id}/chi-tiet
```

| Param | Type | Mô tả |
|-------|------|-------|
| `id` | `Guid` | PhanKhaiKinhPhi Id |

**Response `200`:**
```json
{
  "result": true,
  "dataResult": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "duAnId": "...",
    "soToTrinh": "001/PK-TC/2026",
    "ngayToTrinh": "2026-05-11T00:00:00Z",
    "nguonVonId": 1,
    "kinhPhiDeXuat": 100000000,
    "kinhPhiPhanKhai": 80000000,
    "trangThaiId": 1,
    "tenTrangThai": "Dự thảo"
  }
}
```

**Response `400`** — entity không tồn tại hoặc lỗi validation.

---

### 2. Danh sách phân khai kinh phí

```
GET /api/phan-khai-kinh-phi/danh-sach
```

| Param | Type | Mô tả |
|-------|------|-------|
| `duAnId` | `Guid?` | Filter theo dự án |
| `globalFilter` | `string?` | Tìm kiếm toàn text |
| `pageIndex` | `int` | Trang (mặc định 0) |
| `pageSize` | `int` | Kích thước trang (mặc định 20) |

**Response `200`:**
```json
{
  "result": true,
  "dataResult": {
    "items": [
      {
        "id": "...",
        "duAnId": "...",
        "soToTrinh": "001/PK-TC/2026",
        "ngayToTrinh": "2026-05-11T00:00:00Z",
        "nguonVonId": 1,
        "tenNguonVon": "Ngân sách Trung ương",
        "kinhPhiDeXuat": 100000000,
        "kinhPhiPhanKhai": 80000000,
        "trangThaiId": 1,
        "maTrangThai": "DT",
        "tenTrangThai": "Dự thảo"
      }
    ],
    "pageIndex": 0,
    "pageSize": 20,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 3. Thêm mới phân khai kinh phí

```
POST /api/phan-khai-kinh-phi/them-moi
Content-Type: application/json
```

**Request:**
```json
{
  "duAnId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "soToTrinh": "001/PK-TC/2026",
  "ngayToTrinh": "2026-05-11T00:00:00Z",
  "nguonVonId": 1,
  "kinhPhiDeXuat": 100000000,
  "kinhPhiPhanKhai": 80000000
}
```

**Response `200`:**
```json
{
  "result": true,
  "dataResult": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response `400`** — validation error (thiếu required fields).

---

### 4. Cập nhật phân khai kinh phí

```
PUT /api/phan-khai-kinh-phi/cap-nhat
Content-Type: application/json
```

**Request:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "duAnId": "...",
  "soToTrinh": "001/PK-TC/2026",
  "ngayToTrinh": "2026-05-11T00:00:00Z",
  "nguonVonId": 1,
  "kinhPhiDeXuat": 120000000,
  "kinhPhiPhanKhai": 90000000
}
```

**Response `200`:**
```json
{
  "result": true,
  "dataResult": { ... updated model ... }
}
```

---

### 5. Xóa phân khai kinh phí

```
DELETE /api/phan-khai-kinh-phi/{id}/xoa
```

| Param | Type | Mô tả |
|-------|------|-------|
| `id` | `Guid` | PhanKhaiKinhPhi Id |

**Response `200`:**
```json
{
  "result": true,
  "dataResult": 1
}
```

---

## Files Created

| Layer | Files |
|-------|-------|
| **Domain** | `PhanKhaiKinhPhi.cs` — entity |
| **Persistence** | `PhanKhaiKinhPhiConfiguration.cs` — EF config |
| **Application** | `PhanKhaiKinhPhiInsertCommand.cs`, `PhanKhaiKinhPhiUpdateCommand.cs`, `PhanKhaiKinhPhiDeleteCommand.cs`, `PhanKhaiKinhPhiGetQuery.cs`, `PhanKhaiKinhPhiGetDanhSachQuery.cs` |
| **DTOs** | `PhanKhaiKinhPhiDto.cs`, `PhanKhaiKinhPhiInsertDto.cs`, `PhanKhaiKinhPhiUpdateDto.cs` |
| **WebApi** | `PhanKhaiKinhPhiController.cs`, `PhanKhaiKinhPhiMappingConfiguration.cs`, `PhanKhaiKinhPhiModel.cs` |
| **Migration** | `20260511073726_AddPhanKhaiKinhPhi.cs` + Designer |
| **Tests** | `PhanKhaiKinhPhiControllerTests.cs` — 10 tests |

---

## Test Coverage

| # | Test | Status |
|---|------|--------|
| 1 | `GetChiTiet_ExistingId_ReturnsOk` | ✅ |
| 2 | `GetChiTiet_NonExistentId_ReturnsFailure` | ✅ |
| 3 | `GetDanhSach_ReturnsOk` | ✅ |
| 4 | `GetDanhSach_WithDuAnIdFilter_ReturnsOk` | ✅ |
| 5 | `Create_ValidModel_ReturnsOk` | ✅ |
| 6 | `Create_WithMissingRequiredFields_ReturnsBadRequest` | ✅ |
| 7 | `Update_ExistingEntity_ReturnsOk` | ✅ |
| 8 | `Update_NonExistentId_ReturnsFailure` | ✅ |
| 9 | `Delete_ExistingEntity_ReturnsOk` | ✅ |
| 10 | `Delete_NonExistentId_ReturnsFailure` | ✅ |

```
test.bat phankhaikinhphi   →  10 passed, 0 failed
```

---

## Running Tests

```bash
test.bat phankhaikinhphi     # PhanKhaiKinhPhi tests
test.bat int                 # All integration tests
test.bat                     # All tests
```
