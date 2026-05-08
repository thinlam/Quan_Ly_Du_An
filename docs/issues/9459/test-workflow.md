# UC22 - Test Workflow: Quản lý phê duyệt (unified)

## Thông tin chung

- **Issue**: #9459
- **Branch**: `feature/9459-quan-ly-phe-duyet-noi-dung-trinh-duyet`
- **Test file**: `QLDA.Tests/Integration/PheDuyetDuToanControllerTests.cs`
- **Total tests**: 42 (38 passed, 4 skipped, 0 failed)
- **Status tracking**: `int? TrangThaiId` FK → `DanhMucTrangThaiPheDuyet`
- **History**: Unified `PheDuyetHistory` (polymorphic, EntityName + EntityId)

## Chạy test

```bash
# Toàn bộ tests PheDuyetDuToan
dotnet test QLDA.Tests/QLDA.Tests.csproj --filter "FullyQualifiedName~PheDuyetDuToan"

# Test cụ thể
dotnet test QLDA.Tests/QLDA.Tests.csproj --filter "FullyQualifiedName~PheDuyetDuToanControllerTests.Duyet_AsBgdUser_ReturnsOk"

# Toàn bộ tests
dotnet test QLDA.Tests/QLDA.Tests.csproj
```

## Flow trạng thái (DuToan)

```
DT (id:1, Dự thảo) ──Trinh──→ ĐTr (id:2, Đã trình) ──Duyet──→ ĐD (id:3, Đã duyệt)
                                  │
                                  └──TraLai──→ TL (id:4, Trả lại) ──Trinh──→ ĐTr (loop)
```

## QuanLyPheDuyet Dispatch Pattern

Controller `api/phe-duyet/{type}/{id}/{action}` dispatch theo `type`:

```
type = "PheDuyetDuToan"
    ├── /trinh → PheDuyetDuToanTrinhCommand → sets TrangThaiId = ĐTr, writes PheDuyetHistory
    ├── /duyet → PheDuyetDuToanDuyetCommand → sets TrangThaiId = ĐD, writes PheDuyetHistory (LDDV role required)
    ├── /tra-lai → PheDuyetDuToanTraLaiCommand → sets TrangThaiId = TL, writes PheDuyetHistory (LDDV role required)
    └── /chuyen-phat-hanh → PheDuyetChuyenPhatHanhCommand → entity-specific logic
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
| 5 | LEG | Migrated | PheDuyetDuToan |

## QuanLyPheDuyet API Endpoints

| Endpoint | Method | Role | Description |
|----------|--------|------|-------------|
| `api/phe-duyet/danh-sach` | GET | Any | Danh sách phê duyệt (filter by type, duAnId) |
| `api/phe-duyet/lich-su` | GET | Any | Lịch sử phê duyệt (unified PheDuyetHistory) |
| `api/phe-duyet/{type}/{id}/chi-tiet` | GET | Any | Chi tiết theo type + id |
| `api/phe-duyet/{type}/{id}/trinh` | POST | Any | Trình phê duyệt |
| `api/phe-duyet/{type}/{id}/duyet` | POST | LDDV | Duyệt (requires LDDV role) |
| `api/phe-duyet/{type}/{id}/tra-lai` | POST | LDDV | Trả lại (requires LDDV role + lý do) |
| `api/phe-duyet/{type}/{id}/chuyen-phat-hanh` | POST | Any | Chuyển P.HC-TH phát hành |

## Ma trận test case (PheDuyetDuToan)

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

## Role mapping trong test

| Client | Roles | PhongBanId | Mô tả |
|--------|-------|------------|-------|
| AuthedClient | QLDA_QuanTri, QLDA_TatCa | 1 | Admin mặc định (không có LDDV, không phải KH-TC) |
| BgdClient | QLDA_QuanTri, QLDA_LDDV | 1 | Lãnh đạo (Duyet/TraLai) |
| KhTcClient | *(none)* | 219 | P.Kế toán (Trinh) |

## Refactoring Notes

### What changed (PheDuyetNoiDung → QuanLyPheDuyet)

- **Deleted**: `PheDuyetNoiDung` entity, `PheDuyetNoiDungHistory` entity → replaced by unified `PheDuyetHistory`
- **Deleted**: `PheDuyetNoiDungs/` Application layer (11 files) → replaced by `QuanLyPheDuyet/` dispatch pattern (10 files)
- **Deleted**: `PheDuyetNoiDungController` + 6 models → replaced by `QuanLyPheDuyetController` + 2 models
- **Deleted**: `PheDuyetNoiDungControllerTests.cs` (20 tests) → not yet replaced with QuanLyPheDuyet tests
- **Added**: Unified dispatch commands route `{type}` to correct entity-specific handlers
- **Added**: `PheDuyetEntityNames` constants for type-safe entity dispatch
- **Fixed**: Duyet/TraLai commands enforce `QLDA_LDDV` role (was commented out)
- **Added**: SQLite provider support (`--provider sqlite` CLI arg)

### Unified PheDuyetHistory

All approval history now writes to single `PheDuyetHistory` table:
- `EntityName` = entity type (e.g., "PheDuyetDuToan")
- `EntityId` = polymorphic FK (no DB constraint)
- `TrangThaiId` = FK to `DanhMucTrangThaiPheDuyet`

## Lưu ý khi test trên server thật

1. Migration đã được tạo, chứa seed data `DmTrangThaiPheDuyet` (5 statuses with Loai)
2. Unified `PheDuyetHistory` table đã có trong migration
3. Role `QLDA_LDDV` cần được cấu hình cho users BGĐ
4. `PhongHCTHID` trong `appsettings.json` cần set ID phòng HC-TH thực tế
5. SQLite provider: `./run.bat --sqlite` hoặc `dotnet run -- --provider sqlite`
