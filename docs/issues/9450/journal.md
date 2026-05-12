# Journal — 06-08/05/2026

## 06/05 — PheDuyetDuToan — Thêm trạng thái vào DTO

**Vấn đề:** PheDuyetDuToanDto và PheDuyetDuToanModel không trả về thông tin trạng thái (TrangThai) cho FE.

**Thay đổi:**
- Thêm `MaTrangThai`, `TenTrangThai`, `IsSend` vào `PheDuyetDuToanDto`
- Thêm `TrangThaiId`, `TenTrangThai` vào `PheDuyetDuToanModel`
- Commit: `fix(PheDuyetDuToan): add TrangThai to DTO and model responses`

## 06/05 — Endpoint giải ngân theo nguồn vốn (PR #53)

**Yêu cầu:** Endpoint thống kê giải ngân theo nguồn vốn cho 1 dự án.

**Thay đổi:**
- Tạo `DashboardGiaiNganDto.cs` (Domain DTO)
- Tạo `DashboardGetGiaiNganTheoNguonVonQuery.cs` (Dapper raw SQL)
- Thêm endpoint `GET /api/thong-ke/giai-ngan-theo-nguon-von?duAnId={guid}` vào DashboardController
- Build: 0 errors, 0 warnings

## 08/05 — Đổi filter theo năm + thêm chi tiết giải ngân (PR #58)

**Thay đổi:**
- Đổi filter `duAnId` → `nam` (int) cho endpoint tổng hợp
- Thêm endpoint `GET /api/thong-ke/chi-tiet-giai-ngan?nam={year}` — chi tiết giải ngân theo năm
- Tạo `DashboardChiTietGiaiNganDto` + `DashboardGetChiTietGiaiNganQuery`
- Tạo `DashboardTienDoGiaiNganNguonVonQuery` — biểu đồ group by tháng

## 08/05 — Thêm filter NguonVonId cho chi tiết giải ngân (PR #60)

**Thay đổi:**
- Thêm optional param `nguonVonId` cho endpoint `chi-tiet-giai-ngan`
- Cho phép lọc chi tiết giải ngân theo nguồn vốn cụ thể

## Tóm tắt endpoints cuối cùng

| Endpoint | Filter | Response |
|----------|--------|----------|
| `giai-ngan-theo-nguon-von` | `nam` | Tổng hợp theo nguồn vốn |
| `chi-tiet-giai-ngan` | `nam`, optional `nguonVonId` | Chi tiết bản ghi |
| `tien-do-giai-ngan-nguon-von` | `nam`, optional `nguonVonId/loaiDuAnId/loaiDuAnTheoNamId` | Biểu đồ theo tháng |
