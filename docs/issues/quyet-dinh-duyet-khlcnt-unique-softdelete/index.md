# Unique constraint khi thêm mới Quyết định duyệt KHLCNT sau soft-delete

> Chưa có số PMIS/Redmine. Đổi tên folder thành `docs/issues/{số issue}/` khi có ticket.

## Trạng thái (2026-08-21)

**Code + migration xong.** Unique index đã filter `IsDeleted = 0`. Đã `ef.bat QLDA update` trên catalog `VI_DACDT` và verify SQL. Staging/production **chưa** apply. Chưa commit/push.

Chi tiết: [report.md](./report.md) · Verify: [test-workflow.md](./test-workflow.md)

## Tóm tắt

User **xóa** (soft-delete) Quyết định duyệt KHLCNT của một kế hoạch, rồi **tạo mới** quyết định cho **cùng** `KeHoachLuaChonNhaThauId`. API `POST /api/quyet-dinh-duyet-khlcnt/them-moi` fail với:

> Khoá chính đã tồn tại: bảng `[QuyetDinhDuyetKHLCNT]`, chỉ mục `[IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId]`

Đây **không** phải trùng khóa chính (`Id`). Đây là **unique index trên FK** `KeHoachLuaChonNhaThauId` đang unique **cả bảng**, kể cả dòng `IsDeleted = 1`.

## Source

- **Repo:** SER / QLDA
- **Module:** `QuyetDinhDuyetKHLCNTs`
- **Endpoint:** `POST /QuanLyDuAn/api/quyet-dinh-duyet-khlcnt/them-moi`

## Actor

Người dùng có quyền tạo Quyết định duyệt KHLCNT (sau khi đã xóa quyết định cũ của cùng kế hoạch).

## Expected vs actual

| | |
| --- | --- |
| **Expected** | Cho phép thêm mới nếu mọi dòng trùng `KeHoachLuaChonNhaThauId` trước đó đều `IsDeleted = 1`. Unique chỉ áp dụng record **còn sống** (`IsDeleted = 0`). |
| **Actual (trước fix)** | SQL Server chặn insert vì unique index không lọc soft-delete. |
| **Sau fix (DB đã update)** | Unique không tính dòng `IsDeleted = 1`. Cả bảng, không chỉ 1 kế hoạch. Vẫn chặn nếu đã có 1 QĐ **active**. |

## Sai ở đâu (1 câu)

Index unique `IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId` do quan hệ 1-1 `HasOne/WithOne` sinh ra, filter mặc định chỉ `[KeHoachLuaChonNhaThauId] IS NOT NULL` — **không** loại `IsDeleted = 1`.

## Cách sửa (đã làm)

Khai báo **filtered unique index** trên Configuration (pattern `HopDong` / `DangTaiKeHoachLcntLenMang`), `ef.bat QLDA add` rồi `update` trên DB dev. Không check uniqueness ở Application.

## Files

| File | Việc |
| --- | --- |
| `QLDA.Persistence/Configurations/QuyetDinhDuyetKHLCNTConfiguration.cs` | `.HasIndex(...).IsUnique().HasFilter("[IsDeleted] = 0")` |
| `QLDA.Migrator/Migrations/20260821061550_FilterQuyetDinhDuyetKHLCNTUniqueIndexExcludeDeleted.cs` | Drop index cũ, tạo lại unique `WHERE [IsDeleted] = 0` |
| Snapshot | Do `ef add`, không sửa tay |

## Related

- Chi tiết thiếu field (bug khác cùng module): [../quyet-dinh-duyet-khlcnt-chi-tiet-thieu-field/](../quyet-dinh-duyet-khlcnt-chi-tiet-thieu-field/)
- Pattern đã đúng trên bảng anh em: `QLDA.Persistence/Configurations/DangTaiKeHoachLcntLenMangConfiguration.cs`, `HopDongConfiguration.cs`
