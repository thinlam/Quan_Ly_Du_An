# UC40 - Implementation Report: Phân khai kinh phí

## Issue #9467 | Branch: `feature/9467-phan-khai-kinh-phi`
## Status: IMPLEMENTED (merged via PR #9467)

## Summary

UC40 — Phân khai kinh phí cho các nội dung được giao dự toán. Entity `PhanKhaiKinhPhi` lưu thông tin phân khai theo từng `DuAn`, hỗ trợ CRUD operations qua REST API.

## Architecture

### Entity: PhanKhaiKinhPhi

```
PhanKhaiKinhPhi
├── Id (Guid) — PK
├── DuAnId (Guid) — FK → DuAn
├── SoToTrinh (string)
├── NgayToTrinh (DateTimeOffset)
├── NguonVonId (int) — FK → NguonVon
├── KinhPhiDeXuat (decimal)
└── KinhPhiPhanKhai (decimal)
```

### Controller: PhanKhaiKinhPhiController

```
api/phan-khai-kinh-phi
├── {id}/chi-tiet     GET  → PhanKhaiKinhPhiGetQuery
├── danh-sach         GET  → PhanKhaiKinhPhiGetDanhSachQuery (filter by duAnId, globalFilter, pagination)
├── them-moi          POST → PhanKhaiKinhPhiInsertCommand
├── cap-nhat          PUT  → PhanKhaiKinhPhiUpdateCommand
└── {id}/xoa         DELETE → PhanKhaiKinhPhiDeleteCommand
```

## Test Coverage

| Test | Status |
|------|--------|
| GetChiTiet_ExistingId_ReturnsOk | ✅ |
| GetChiTiet_NonExistentId_ReturnsFailure | ✅ |
| GetDanhSach_ReturnsOk | ✅ |
| GetDanhSach_WithDuAnIdFilter_ReturnsOk | ✅ |
| Create_ValidModel_ReturnsOk | ✅ |
| Create_WithMissingRequiredFields_ReturnsBadRequest | ✅ |
| Update_ExistingEntity_ReturnsOk | ✅ |
| Update_NonExistentId_ReturnsFailure | ✅ |
| Delete_ExistingEntity_ReturnsOk | ✅ |
| Delete_NonExistentId_ReturnsFailure | ✅ |

**Total: 10 passed, 0 failed**

## Running Tests

```bash
test.bat phankhaikinhphi
```
