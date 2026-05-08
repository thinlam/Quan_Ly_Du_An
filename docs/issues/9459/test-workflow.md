# UC22 - Test Workflow: Quản lý phê duyệt (unified)

## Thông tin chung

- **Issue**: #9459
- **Branch**: `feature/9459-quan-ly-phe-duyet-noi-dung-trinh-duyet`
- **Test files**:
  - `QLDA.Tests/Integration/PheDuyetDuToanControllerTests.cs` (10 tests — direct entity endpoints)
  - `QLDA.Tests/Integration/QuanLyPheDuyetControllerTests.cs` (20 tests — unified dispatch endpoints)
- **Total tests**: 62 (58 passed, 4 skipped, 0 failed)
- **Status tracking**: `int? TrangThaiId` FK → `DanhMucTrangThaiPheDuyet`
- **History**: Unified `PheDuyetHistory` (polymorphic, EntityName + EntityId)

## Chạy test

```bash
# PheDuyetDuToan tests (direct entity endpoints)
test.bat pheduyetdutoan

# QuanLyPheDuyet tests (unified dispatch)
test.bat "FullyQualifiedName~QuanLyPheDuyetControllerTests"

# Toàn bộ integration tests
test.bat int

# Toàn bộ tests
test.bat

# Test cụ thể (filter trực tiếp)
test.bat "FullyQualifiedName~QuanLyPheDuyetControllerTests.ChuyenPhatHanh_AsHcthUser_ReturnsOk"

# Build only (không chạy test)
test.bat build
```

## Flow trạng thái (DuToan)

```
DT (id:1, Dự thảo) ──Trinh──→ ĐTr (id:2, Đã trình) ──Duyet──→ ĐD (id:3, Đã duyệt) ──PhatHanh──→ Done
                                  │
                                  └──TraLai──→ TL (id:4, Trả lại) ──Trinh──→ ĐTr (loop)
```

## QuanLyPheDuyet Dispatch Pattern

Controller `api/phe-duyet/{type}/{id}/{action}` dispatch theo `type`:

```
type = "PheDuyetDuToan"
    ├── /trinh → PheDuyetDispatchTrinhCommand → PheDuyetDuToanTrinhCommand (KH-TC role)
    ├── /duyet → PheDuyetDispatchDuyetCommand → PheDuyetDuToanDuyetCommand (LDDV role)
    ├── /tra-lai → PheDuyetDispatchTraLaiCommand → PheDuyetDuToanTraLaiCommand (LDDV role + lý do)
    └── /chuyen-phat-hanh → PheDuyetChuyenPhatHanhCommand (HC-TH or LDDV role, requires Đã duyệt)
```

## Constants Reference

```csharp
// Entity names (for polymorphic dispatch & history)
PheDuyetEntityNames.PheDuyetDuToan  // "PheDuyetDuToan"

// Loai discriminator
TrangThaiPheDuyetCodes.Loai.PheDuyetDuToan  // "PheDuyetDuToan"

// DuToan status codes
TrangThaiPheDuyetCodes.DuToan.DuThao   // "DT"
TrangThaiPheDuyetCodes.DuToan.DaTrinh  // "ĐTr"
TrangThaiPheDuyetCodes.DuToan.DaDuyet  // "ĐD"
TrangThaiPheDuyetCodes.DuToan.TraLai   // "TL"
TrangThaiPheDuyetCodes.DuToan.Legacy   // "LEG"
```

## Seeded DanhMucTrangThaiPheDuyet IDs

| Id | Ma | Ten | Loai |
|----|----|-----|------|
| 1 | DT | Dự thảo | PheDuyetDuToan |
| 2 | ĐTr | Đã trình | PheDuyetDuToan |
| 3 | ĐD | Đã duyệt | PheDuyetDuToan |
| 4 | TL | Trả lại | PheDuyetDuToan |
| 5 | LEG | Migrated | DungChung |

## QuanLyPheDuyet API Endpoints

| Endpoint | Method | Role | Description |
|----------|--------|------|-------------|
| `api/phe-duyet/danh-sach` | GET | Any | Danh sách phê duyệt (filter by type, duAnId) |
| `api/phe-duyet/lich-su` | GET | Any | Lịch sử phê duyệt (unified PheDuyetHistory) |
| `api/phe-duyet/{type}/{id}/chi-tiet` | GET | Any | Chi tiết theo type + id |
| `api/phe-duyet/{type}/{id}/trinh` | POST | KH-TC | Trình phê duyệt |
| `api/phe-duyet/{type}/{id}/duyet` | POST | LDDV | Duyệt (requires LDDV role) |
| `api/phe-duyet/{type}/{id}/tra-lai` | POST | LDDV | Trả lại (requires LDDV role + lý do) |
| `api/phe-duyet/{type}/{id}/chuyen-phat-hanh` | POST | HC-TH/LDDV | Chuyển P.HC-TH phát hành |

## Ma trận test case — PheDuyetDuToanController (direct endpoints)

| # | Endpoint | Method | Role | Test case | Seed Status |
|---|----------|--------|------|-----------|-------------|
| 1 | `/api/phe-duyet-du-toan/{id}/chi-tiet` | GET | Any | GetChiTiet_ExistingId_ReturnsOk | - |
| 2 | `/{id}/chi-tiet` | GET | Any | GetChiTiet_NonExistentId_ReturnsFailure | - |
| 3 | `/{id}/trinh` | POST | KH-TC | Trinh_AsKhTcUser_ReturnsOk | DT (id:1) |
| 4 | `/{id}/trinh` | POST | Non-KH-TC | Trinh_AsNonKhTcUser_ReturnsFailure | DT (id:1) |
| 5 | `/{id}/duyet` | POST | LDDV | Duyet_AsBgdUser_ReturnsOk | ĐTr (id:2) |
| 6 | `/{id}/duyet` | POST | Non-LDDV | Duyet_AsNonBgdUser_ReturnsFailure | ĐTr (id:2) |
| 7 | `/{id}/duyet` | POST | LDDV | Duyet_WithoutTrinh_ReturnsFailure | DT (id:1) |
| 8 | `/{id}/tra-lai` | POST | LDDV | TraLai_AsBgdUser_ReturnsOk | ĐTr (id:2) |
| 9 | `/{id}/tra-lai` | POST | LDDV | TraLai_WithoutNoiDung_ReturnsFailure | ĐTr (id:2) |
| 10 | `/{id}/trinh` | POST | KH-TC | Trinh_AfterTraLai_ReturnsOk | TL → ĐTr loop |

## Ma trận test case — QuanLyPheDuyetController (dispatch endpoints)

| # | Endpoint | Method | Role | Test case | Seed Status |
|---|----------|--------|------|-----------|-------------|
| 1 | `danh-sach` | GET | Any | GetDanhSach_ReturnsOk | - |
| 2 | `danh-sach?type=` | GET | Any | GetDanhSach_FilterByType_ReturnsOk | - |
| 3 | `lich-su` | GET | Any | GetLichSu_ReturnsOk | - |
| 4 | `lich-su?entityId=` | GET | Any | GetLichSu_FilterByEntityId_ReturnsOk | - |
| 5 | `/{type}/{id}/chi-tiet` | GET | Any | GetChiTiet_ExistingId_ReturnsOk | - |
| 6 | `/{type}/{id}/chi-tiet` | GET | Any | GetChiTiet_NonExistentId_ReturnsFailure | fake ID |
| 7 | `/{type}/{id}/chi-tiet` | GET | Any | GetChiTiet_InvalidType_ReturnsFailure | InvalidType |
| 8 | `/{type}/{id}/trinh` | POST | KH-TC | Trinh_AsKhTcUser_ReturnsOk | DT (id:1) |
| 9 | `/{type}/{id}/trinh` | POST | Non-KH-TC | Trinh_AsNonKhTcUser_ReturnsFailure | DT (id:1) |
| 10 | `/{type}/{id}/trinh` | POST | KH-TC | Trinh_InvalidType_ReturnsFailure | InvalidType |
| 11 | `/{type}/{id}/duyet` | POST | LDDV | Duyet_AsBgdUser_ReturnsOk | ĐTr (id:2) |
| 12 | `/{type}/{id}/duyet` | POST | Non-LDDV | Duyet_AsNonBgdUser_ReturnsFailure | ĐTr (id:2) |
| 13 | `/{type}/{id}/duyet` | POST | LDDV | Duyet_WithoutTrinh_ReturnsFailure | DT (id:1) |
| 14 | `/{type}/{id}/tra-lai` | POST | LDDV | TraLai_AsBgdUser_ReturnsOk | ĐTr (id:2) |
| 15 | `/{type}/{id}/tra-lai` | POST | LDDV | TraLai_WithoutNoiDung_ReturnsFailure | ĐTr (id:2) |
| 16 | `/{type}/{id}/trinh` | POST | KH-TC | Trinh_AfterTraLai_ReturnsOk | TL → ĐTr loop |
| 17 | `/{type}/{id}/chuyen-phat-hanh` | POST | HC-TH | ChuyenPhatHanh_AsHcthUser_ReturnsOk | ĐD (id:3) |
| 18 | `/{type}/{id}/chuyen-phat-hanh` | POST | LDDV | ChuyenPhatHanh_AsBgdUser_ReturnsOk | ĐD (id:3) |
| 19 | `/{type}/{id}/chuyen-phat-hanh` | POST | HC-TH | ChuyenPhatHanh_WithoutDuyet_ReturnsFailure | ĐTr (id:2) |
| 20 | `/{type}/{id}/chuyen-phat-hanh` | POST | Non-HC-TH | ChuyenPhatHanh_AsNonHcthNonBgdUser_ReturnsFailure | ĐD (id:3) |

## Role mapping trong test

| Client | Roles | PhongBanId | Mô tả |
|--------|-------|------------|-------|
| AuthedClient | QLDA_QuanTri, QLDA_TatCa | 1 | Admin mặc định (không có LDDV, không phải KH-TC, không phải HC-TH) |
| BgdClient | QLDA_QuanTri, QLDA_LDDV | 1 | Lãnh đạo (Duyet/TraLai/ChuyenPhatHanh) |
| KhTcClient | *(none)* | 219 | P.Kế toán (Trinh) |
| HcthClient | QLDA_HC_TH | 300 | P.HC-TH (ChuyenPhatHanh) |

## SQLite Compatibility Notes

SQLite EF Core không hỗ trợ `DateTimeOffset` trong aggregate/ORDER BY. Query handlers sử dụng client-side evaluation cho:
- `danh-sach`: Materialize history → GroupBy + Max client-side → merge với entity data
- `lich-su`: Materialize → OrderByDescending client-side
- `chi-tiet`: Materialize history → OrderByDescending client-side

## Lưu ý khi test trên server thật

1. Migration đã được tạo, chứa seed data `DmTrangThaiPheDuyet` (5 statuses with Loai)
2. Unified `PheDuyetHistory` table đã có trong migration
3. Role `QLDA_LDDV` cần được cấu hình cho users BGĐ
4. Role `QLDA_HC_TH` cần được cấu hình cho users P.HC-TH
5. `PhongHCTHID` trong `appsettings.json` cần set ID phòng HC-TH thực tế
6. SQLite provider: `./run.bat --sqlite` hoặc `dotnet run -- --provider sqlite`
