# Bug — Export Excel Tổng hợp văn bản quyết định

> Ngày ghi nhận: 30/06/2026  
> Trạng thái: 🔍 Điều tra xong — chưa fix  
> Effort ước lượng: ~2–3 giờ (BE only, không migration)

## Mô tả lỗi

API print/export danh sách **Tổng hợp văn bản quyết định** trả **400 Bad Request** với message chung `"Lỗi hệ thống, vui lòng thử lại sau"` thay vì file Excel.

### Endpoint

```
GET /QuanLyDuAn/api/print/danh-sach-tong-hop-van-ban-quyet-dinh?pageIndex=1&pageSize=10
Authorization: Bearer {token}
Accept: text/plain   # hoặc application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
```

> **Lưu ý:** Với GET, không cần header `Content-Type: application/json`.

### Kết quả mong muốn

- HTTP **200**, body là file `.xlsx`
- Dữ liệu export **khớp filter** đang dùng trên màn hình (cùng tiêu chí với API danh sách)
- Tên file tải: `DanhSachTongHopVanBanQuyetDinh_ddMMyyyy_HHmmss.xlsx`

## Phát hiện chính (sau đọc source)

| # | Phát hiện | Mức độ |
|---|-----------|--------|
| 1 | SP `usp_In_DanhSach_TongHopVanBanQuyetDinh` **không có trong repo** — export phụ thuộc DB/DBA | Cao |
| 2 | `ExceptionMiddleware` map mọi exception không phải `ManagedException` → **400** + message chung (che exception thật) | Cao |
| 3 | Export vẫn dùng `GetStoreQuery` + Dapper SP — **chưa** chuyển sang LINQ như `InKhoKhanVuongMac` (đã fix #kho-khan) | Cao |
| 4 | `TongHopVanBanQuyetDinhPrintSearchModel` **không có** `PageIndex`/`PageSize`; controller hardcode `PageIndex = 0`, `PageSize = 0` | Trung bình |
| 5 | Print model **thiếu** `CoQuanQuyetDinh` so với `TongHopVanBanQuyetDinhSearchModel` (grid) | Trung bình |
| 6 | `TongHopVanBanQuyetDinhGetListQuery` **không dùng** `FilterVisible()` — lệch phân quyền so với pattern chuẩn | Trung bình |
| 7 | Template `DanhSachTongHopVanBanQuyetDinh.xlsx` **có trong git** (`PrintTemplates/`) và `csproj` copy ra output | OK |

## Màn hình / API liên quan

| Thành phần | API |
|------------|-----|
| Danh sách grid | `GET /api/tong-hop-van-ban-quyet-dinh/danh-sach-day-du` |
| Export Excel (lỗi) | `GET /api/print/danh-sach-tong-hop-van-ban-quyet-dinh` |

## Hướng fix khuyến nghị

Phương án B — migrate sang LINQ (giống `InKhoKhanVuongMac`), xem **mục 10** trong report:

| Bước | Việc làm |
|------|----------|
| 1 | Tạo `TongHopVanBanQuyetDinhExportDto` |
| 2 | Tạo `TongHopVanBanQuyetDinhGetListExportQuery` (+ `FilterVisible`) |
| 3 | Sửa `TongHopVanBanQuyetDinhPrintSearchModel` kế thừa `CommonSearchModel` |
| 4 | Sửa `PrintController.InTongHopVanBanQuyetDinh` — bỏ SP |
| 5 | (Tùy chọn) Thêm `FilterVisible` cho grid query |
| 6 | Build + smoke test |
| 7 | Checklist trước merge |

## Tài liệu triển khai

- **[report.md](report.md)** — Spec kỹ thuật đầy đủ: luồng xử lý, nguyên nhân, checklist điều tra log/DB, **7 bước code chi tiết** (DTO, Query, PrintSearchModel, PrintController, build/test), kế hoạch test.

## Tham chiếu pattern đã fix

- [Khó khăn vướng mắc export Excel](../kho-khan-vuong-mac-export-excel/report.md) — đã migrate từ SP sang `KhoKhanVuongMacGetDanhSachExportQuery` + `_excelExporter.Export`
- [Issue #9609 — Loại dự án theo năm](../9609/report.md) — đã bổ sung `LoaiDuAnTheoNamId` cho print model
