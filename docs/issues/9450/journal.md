# Journal — 06/05/2026

## PheDuyetDuToan — Thêm trạng thái vào DTO

**Vấn đề:** PheDuyetDuToanDto và PheDuyetDuToanModel không trả về thông tin trạng thái (TrangThai) cho FE.

**Thay đổi:**
- Thêm `MaTrangThai`, `TenTrangThai`, `IsSend` vào `PheDuyetDuToanDto`
- Thêm `TrangThaiId`, `TenTrangThai` vào `PheDuyetDuToanModel`
- Cập nhật `ToDto()`, `ToModel()`, `GetDanhSachQuery` Select, `GetQuery` Include
- Commit: `fix(PheDuyetDuToan): add TrangThai to DTO and model responses`

## Issue #9450 — Dashboard giải ngân theo nguồn vốn

**Yêu cầu:** Endpoint thống kê giải ngân theo nguồn vốn cho 1 dự án.

**Thay đổi:**
- Tạo `DashboardGiaiNganDto.cs` (Domain DTO)
- Tạo `DashboardGetGiaiNganTheoNguonVonQuery.cs` (Dapper raw SQL)
- Thêm endpoint `GET /api/thong-ke/giai-ngan-theo-nguon-von?duAnId={guid}` vào DashboardController
- Build: 0 errors, 0 warnings

**Branch:** `feat/9450-phe-duyet-du-toan-trang-thai`
