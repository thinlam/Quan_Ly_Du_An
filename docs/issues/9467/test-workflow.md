# UC40 - Test Workflow: Phân khai kinh phí

## Thông tin chung

- **Issue**: #9467
- **Branch**: `feature/9467-phan-khai-kinh-phi`
- **Test files**:
  - `QLDA.Tests/Integration/PhanKhaiKinhPhiControllerTests.cs` (10 tests — CRUD endpoints)
- **Total tests**: 10 passed, 0 failed, 0 skipped
- **Status**: IMPLEMENTED

## Chạy test

```bash
# PhanKhaiKinhPhi tests
test.bat phankhaikinhphi

# Toàn bộ integration tests
test.bat int

# Toàn bộ tests
test.bat
```

## API Endpoints

| Endpoint | Method | Mô tả |
|----------|--------|-------|
| `api/phan-khai-kinh-phi/{id}/chi-tiet` | GET | Chi tiết phân khai kinh phí |
| `api/phan-khai-kinh-phi/danh-sach` | GET | Danh sách phân khai kinh phí (filter by duAnId, globalFilter, pagination) |
| `api/phan-khai-kinh-phi/them-moi` | POST | Thêm mới phân khai kinh phí |
| `api/phan-khai-kinh-phi/cap-nhat` | PUT | Cập nhật phân khai kinh phí |
| `api/phan-khai-kinh-phi/{id}/xoa` | DELETE | Xóa phân khai kinh phí |

## Ma trận test case — PhanKhaiKinhPhiController

| # | Endpoint | Method | Test case |
|---|----------|--------|-----------|
| 1 | `/{id}/chi-tiet` | GET | GetChiTiet_ExistingId_ReturnsOk |
| 2 | `/{id}/chi-tiet` | GET | GetChiTiet_NonExistentId_ReturnsFailure |
| 3 | `/danh-sach` | GET | GetDanhSach_ReturnsOk |
| 4 | `/danh-sach?duAnId=` | GET | GetDanhSach_WithDuAnIdFilter_ReturnsOk |
| 5 | `/them-moi` | POST | Create_ValidModel_ReturnsOk |
| 6 | `/them-moi` | POST | Create_WithMissingRequiredFields_ReturnsBadRequest |
| 7 | `/cap-nhat` | PUT | Update_ExistingEntity_ReturnsOk |
| 8 | `/cap-nhat` | PUT | Update_NonExistentId_ReturnsFailure |
| 9 | `/{id}/xoa` | DELETE | Delete_ExistingEntity_ReturnsOk |
| 10 | `/{id}/xoa` | DELETE | Delete_NonExistentId_ReturnsFailure |

## Entity & DTOs

```
PhanKhaiKinhPhi
├── Id (Guid)
├── DuAnId (Guid) FK → DuAn
├── SoToTrinh (string)
├── NgayToTrinh (DateTimeOffset)
├── NguonVonId (int) FK → NguonVon
├── KinhPhiDeXuat (decimal)
└── KinhPhiPhanKhai (decimal)
```

## Seed Data

- Tests dùng `fixture.SeededDuAnId` làm DuAnId
- NguonVonId = 1 (hardcoded trong test fixture)
