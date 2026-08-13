# Journal — Issue #178 Form KHLCNT bổ sung field

## 2026-08-12

- Survey source theo yêu cầu "Quyết định duyệt Kế hoạch lựa chọn nhà thầu".
- Kết luận module form = `KeHoachLuaChonNhaThau` (`api/ke-hoach-lua-chon-nha-thau`), không phải `QuyetDinhDuyetKHLCNT`.
- 4 field (Tổng dự toán, Dự toán thẩm định, Nguồn vốn, Thời gian thực hiện) **đã có** từ #170 (entity/DTO/command/list + 3 migration).
- Nguồn vốn dự án: `DuAnNguonVon` + CBB `GET api/danh-muc-nguon-von/danh-sach?duAnId=` — reuse, không tạo mới.
- Gap còn lại: **Số lượng gói thầu** — chưa có field; có thể count từ `GoiThaus` (A) hoặc persist `SoLuongGoiThau` (B).
- Viết docs `docs/issues/178/index.md`, `report.md`, `journal.md`. Chưa implement — chờ user chốt approach.

- User: code theo `index.md`, migration để tự làm.
- Chốt **Approach B** — thêm `int? SoLuongGoiThau` trên Entity + DTO Insert/Update/Dto + Mapping + List projection.
- **Không** chạy `ef.bat add` — user tự migration.
- Không thêm validate bắt buộc (optional như `ThoiGianThucHien`).
- Build `QLDA.Application` succeeded, 0 error.
