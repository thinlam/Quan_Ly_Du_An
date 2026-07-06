# Lỗi API in Excel — Tình hình thực hiện đấu thầu chưa lọc theo bộ lọc

> Ngày ghi nhận: 06/07/2026  
> Trạng thái: ✅ **IMPLEMENTED** (06/07/2026)  
> Effort thực tế: ~2 giờ (BE only, không migration)

## Tóm tắt

API export Excel `GET /api/print/tinh-hinh-thuc-hien-dau-thau` trước đây **chỉ lọc theo tab** (`loai`). Đã bổ sung filter `namDuAn`, `giaiDoanId`, `duAnId` trên query print — dữ liệu Excel khớp bộ lọc UI.

| # | Triệu chứng (trước fix) | Trạng thái |
|---|------------------------|------------|
| 1 | Excel chứa gói thầu năm khác dù UI chọn `namDuAn=2026` | ✅ Fixed |
| 2 | Excel chứa gói thầu giai đoạn khác | ✅ Fixed |
| 3 | Excel chứa gói thầu dự án khác | ✅ Fixed |

## Endpoint

```http
GET /QuanLyDuAn/api/print/tinh-hinh-thuc-hien-dau-thau?loai=1&namDuAn=2026&giaiDoanId=22&duAnId=08dec1fd-220c-da70-687a-7b47980360c9
Authorization: Bearer {token}
Role: GroupTinhHinhThucHienDauThauExport
```

### Không lọc năm

Bỏ `namDuAn` khỏi URL — backend không áp dụng WHERE năm.

## Kết quả implement

| Hạng mục | File | Trạng thái |
|----------|------|------------|
| Helper filter | `GoiThauTinhHinhDauThauQueryableExtensions.cs` | ✅ |
| Print DTO | `TinhHinhThucHienDauThauPrintSearchDto.cs` | ✅ |
| Print handler | `GoiThauGetTinhHinhDauThauPrintQuery.cs` | ✅ |
| List handler (refactor) | `GoiThauGetTinhHinhDauThauQuery.cs` | ✅ |
| Unit test | `GoiThauTinhHinhDauThauQueryableExtensionsTests.cs` | ✅ |
| Integration test | `TinhHinhThucHienDauThauExportTests.cs` | ✅ |

## Điểm quan trọng

| Param | Logic |
|-------|--------|
| `namDuAn` (print) | #9121: `ThoiGianKhoiCong` / `ThoiGianHoanThanh` — **không** `NgayBatDau` |
| `nam` (list) | `DuAn.NgayBatDau` trong khoảng năm (giữ cũ) |
| `giaiDoanId` | Lọc khi `> 0`; `-1` = bỏ qua |
| `duAnId` | `GoiThau.DuAnId` |
| Không truyền param | Không thêm WHERE tương ứng |

## Tài liệu triển khai

| File | Nội dung |
|------|----------|
| **[report.md](report.md)** | Spec as-built, SQL verify, test plan, checklist |
| [task-export gốc](../../feature/GoiThau/task-export-tinh-hinh-thuc-hien-dau-thau.md) | ⏳ Cần cập nhật filter as-built |

## QA còn lại

1. **Restart WebApi** sau build (tránh chạy DLL cũ).
2. Export với filter đầy đủ → so sánh số dòng với grid.
3. Export **không** `namDuAn` → nhiều dòng hơn (nếu dự án không thỏa năm 2026 theo #9121).
