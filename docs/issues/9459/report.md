# UC22 - Implementation Report: Quản lý phê duyệt (unified)

## Issue #9459 | Branch: `feature/9459-quan-ly-phe-duyet-noi-dung-trinh-duyet`
## Status: MERGED via PR #59 (2026-05-08)

## Summary

Module QuanLyPheDuyet — màn hình tổng hợp quản lý phê duyệt cho TẤT CẢ các loại entity. BGĐ duyệt/từ chối/trả lại, P.HC-TH phát hành. Unified dispatch pattern thay thế module PheDuyetNoiDung riêng lẻ.

## Architecture

### Unified Dispatch Pattern

`QuanLyPheDuyetController` là **center provider** cho tất cả các loại phê duyệt. Dispatch commands theo `type` parameter đến đúng entity-specific handler.

```
QuanLyPheDuyetController (api/phe-duyet)
    ├── {type}/{id}/trinh → PheDuyetDispatchTrinhCommand → type switch → entity command
    ├── {type}/{id}/duyet → PheDuyetDispatchDuyetCommand → type switch → entity command
    ├── {type}/{id}/tra-lai → PheDuyetDispatchTraLaiCommand → type switch → entity command
    ├── {type}/{id}/chuyen-phat-hanh → PheDuyetChuyenPhatHanhCommand
    ├── danh-sach → PheDuyetGetDanhSachQuery (filter by type, duAnId)
    ├── lich-su → PheDuyetGetLichSuQuery (unified PheDuyetHistory)
    └── {type}/{id}/chi-tiet → PheDuyetGetChiTietQuery
```

### Unified PheDuyetHistory (Polymorphic)

Single `PheDuyetHistory` thay vì N per-entity history tables:

```csharp
public class PheDuyetHistory : Entity<Guid>, IAggregateRoot
{
    public string EntityName { get; set; }  // "PheDuyetDuToan", etc.
    public Guid EntityId { get; set; }      // Polymorphic FK (no constraint)
    public Guid DuAnId { get; set; }
    public long? NguoiXuLyId { get; set; }
    public int? TrangThaiId { get; set; }
    public string? NoiDung { get; set; }
    public DateTimeOffset NgayXuLy { get; set; }
}
```

### DanhMucTrangThaiPheDuyet (Shared Status Catalog)

Single entity với `Loai` discriminator:

| Loai | Id | Ma | Ten |
|------|----|----|-----|
| PheDuyetDuToan | 1 | DT | Dự thảo |
| PheDuyetDuToan | 2 | ĐTr | Đã trình |
| PheDuyetDuToan | 3 | ĐD | Đã duyệt |
| PheDuyetDuToan | 4 | TL | Trả lại |
| DungChung | 5 | LEG | Migrated |

### Constants

- `TrangThaiPheDuyetCodes.Loai.PheDuyetDuToan` - Loai discriminator
- `TrangThaiPheDuyetCodes.DuToan.*` - DuToan status codes (DT, ĐTr, ĐD, TL, LEG)
- `PheDuyetEntityNames.PheDuyetDuToan` = "PheDuyetDuToan"

## Files Created

### Domain Layer (3 files)
- `QLDA.Domain/Constants/TrangThaiPheDuyetCodes.cs` - Status codes + Loai constants
- `QLDA.Domain/Constants/PheDuyetEntityNames.cs` - Entity name constants for polymorphic dispatch
- `QLDA.Domain/Entities/PheDuyetHistory.cs` - Unified history entity

### Persistence Layer (1 file)
- `QLDA.Persistence/Configurations/PheDuyetHistoryConfiguration.cs` - Composite index (EntityName, EntityId)

### Application Layer — QuanLyPheDuyet (10 files)
- `QLDA.Application/QuanLyPheDuyet/Commands/` - 4 dispatch command handlers
  - `PheDuyetDispatchTrinhCommand` - dispatch trinh theo type
  - `PheDuyetDispatchDuyetCommand` - dispatch duyet theo type
  - `PheDuyetDispatchTraLaiCommand` - dispatch tra lai theo type
  - `PheDuyetChuyenPhatHanhCommand` - chuyen P.HC-TH phat hanh
- `QLDA.Application/QuanLyPheDuyet/Queries/` - 3 query handlers
  - `PheDuyetGetDanhSachQuery` - paginated list (filter by type, duAnId)
  - `PheDuyetGetChiTietQuery` - chi tiet theo type + id
  - `PheDuyetGetLichSuQuery` - unified history from PheDuyetHistory
- `QLDA.Application/QuanLyPheDuyet/DTOs/` - 3 DTOs
  - `PheDuyetListItemDto`, `PheDuyetChiTietDto`, `PheDuyetHistoryDto`

### WebApi Layer (3 files)
- `QLDA.WebApi/Controllers/QuanLyPheDuyetController.cs` - 7 endpoints
- `QLDA.WebApi/Models/QuanLyPheDuyet/TrinhModel.cs`
- `QLDA.WebApi/Models/QuanLyPheDuyet/ChuyenPhatHanhModel.cs`

### Migration (1 file)
- `QLDA.Migrator/Migrations/20260508035251_RefactorPheDuyet.cs` - DB migration

### Tests (1 file)
- `QLDA.Tests/Integration/QuanLyPheDuyetControllerTests.cs` - 20 integration tests

## Files Deleted (refactored away)

| Layer | Files | Reason |
|-------|-------|--------|
| Domain | `PheDuyetNoiDung.cs`, `PheDuyetNoiDungHistory.cs` | Replaced by unified PheDuyetHistory |
| Persistence | `PheDuyetNoiDungConfiguration.cs`, `PheDuyetNoiDungHistoryConfiguration.cs` | No longer needed |
| Application | `PheDuyetNoiDungs/` (11 files: Commands, Queries, DTOs) | Replaced by QuanLyPheDuyet dispatch pattern |
| WebApi | `PheDuyetNoiDungController.cs`, `Models/PheDuyetNoiDungs/` (6 files) | Replaced by QuanLyPheDuyetController |
| Tests | `PheDuyetNoiDungControllerTests.cs` | Controller deleted |
| Domain | `TrangThaiPheDuyetDuToanCodes.cs`, `TrangThaiPheDuyetNoiDungCodes.cs` | Merged into `TrangThaiPheDuyetCodes.cs` |
| Domain | `DanhMucTrangThaiPheDuyetDuToan.cs` | Merged into shared `DanhMucTrangThaiPheDuyet` |

## Files Modified

| File | Change |
|------|--------|
| `PheDuyetDuToan.cs` | Remove `ICollection<PheDuyetDuToanHistory>` nav prop |
| `PheDuyetDuToanDuyetCommand.cs` | Write to unified `PheDuyetHistory`, enforce LDDV role |
| `PheDuyetDuToanTraLaiCommand.cs` | Write to unified `PheDuyetHistory`, enforce LDDV role |
| `PheDuyetDuToanTrinhCommand.cs` | Write to unified `PheDuyetHistory` |
| `PheDuyetDuToanDto.cs` | Updated codes reference |
| `DanhMucTrangThaiPheDuyetConfiguration.cs` | Composite unique (Ma+Loai), seed data |
| `AppDbContextModelSnapshot.cs` | Updated for new schema |
| `WebApplicationExtensions.cs` | Added SQLite provider support |
| `Program.cs` | Added `--provider` CLI arg, SQLite DB init |
| `VisibilityFilterExtensions.cs` | Updated for new entity types |
| `WebApiFixture.cs` | Updated for PheDuyetHistory seeding |

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

## PR History

| PR | Date | Description |
|----|------|-------------|
| #55 | 2026-05-07 | PheDuyetNoiDung UC22 + FK-based status tracking |
| #59 | 2026-05-08 | QuanLyPheDuyet unified dispatch + SQLite support + 20 integration tests |

## Build & Test Results

- **Build**: 0 errors, 0 warnings
- **Tests**: 62 total, 58 passed, 4 skipped, 0 failed
- **PheDuyetDuToan tests**: 10 passed (role-based Duyet/TraLai/Trinh)
- **QuanLyPheDuyet tests**: 20 passed (dispatch pattern, all flows)
- **Skipped tests**: 4 pre-existing (GoiThau/HopDong/VanBan TienDo endpoints)

## Design Decisions

1. **Unified dispatch pattern** — single controller handles all approval types via `type` parameter
2. **Unified PheDuyetHistory** — 1 polymorphic table thay vì N per-entity history tables
3. **FK-based status** (`int? TrangThaiId`) — DB-enforced referential integrity
4. **Role enforcement active** — Duyet/TraLai commands enforce `QLDA_LDDV` role
5. **SQLite provider support** — `--provider sqlite` CLI arg for local dev/testing
6. **Shared DanhMucTrangThaiPheDuyet** with `Loai` discriminator — DRY, extensible

## Unresolved Questions

1. Chưa có notification/gửi thông báo kết quả xử lý đến đơn vị trình duyệt
2. `PhongHCTHID` trong `appsettings.json` đang = 0, cần cấu hình ID phòng HC-TH thực tế
3. Chưa mở rộng dispatch cho các entity khác (chỉ có PheDuyetDuToan hiện tại)
