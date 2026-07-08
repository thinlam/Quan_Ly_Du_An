# Fix format ngày — Phiếu trình KH triển khai hạng mục

**Module:** QLDA / KeHoachTrienKhaiHangMuc  
**Trạng thái:** ✅ **IMPLEMENTED**  
**Ngày:** 2026-07-08  
**API:** `GET /api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc`

## Triệu chứng

Bảng **Nội dung kế hoạch triển khai** trong file Word in ra ngày sai format:

| Cột | Hiện tại | Yêu cầu |
|-----|----------|---------|
| Bắt đầu | `2026-07-08` | `08/07/2026` |
| Kết thúc | `2026-07-08` | `08/07/2026` |

## Kết luận nhanh

| # | Phát hiện | Mức độ |
|---|-----------|--------|
| 1 | `KeHoachTrienKhaiHangMucWordExporter.FormatDate` hard-code `"yyyy-MM-dd"` | **Cao — root cause** |
| 2 | Excel export **đã đúng** `dd/MM/yyyy` (descriptor) | OK — không sửa |
| 3 | DB lưu `DateOnly?` — không đổi schema | OK |
| 4 | Null date → `""` (không in `01/01/0001`) | OK với code hiện tại |

## Đã sửa

| File | Thay đổi |
|------|----------|
| `QLDA.Infrastructure/Offices/KeHoachTrienKhaiHangMucWordExporter.cs` | `FormatDate` → `dd/MM/yyyy` (`vi-VN`) |
| `QLDA.Infrastructure/QLDA.Infrastructure.csproj` | `InternalsVisibleTo("QLDA.Tests")` |
| `QLDA.Tests/Unit/KeHoachTrienKhaiHangMucWordExporterTests.cs` | Unit test format ngày + null |
| `QLDA.Tests/QLDA.Tests.csproj` | ProjectReference `QLDA.Infrastructure` |

## Tài liệu chi tiết

→ [`report.md`](./report.md)

## Files liên quan

| File | Vai trò |
|------|---------|
| `QLDA.Infrastructure/Offices/KeHoachTrienKhaiHangMucWordExporter.cs` | `FormatDate` — **đã sửa** |
| `QLDA.Application/.../KeHoachTrienKhaiHangMucExportMappings.cs` | Map `DateOnly?` → `DateTime?` (không format) |
| `QLDA.Gen/Descriptors/KeHoachTrienKhaiHangMucExportDescriptor.cs` | Excel đã `dd/MM/yyyy` |
| `QLDA.Tests/Integration/KeHoachTrienKhaiHangMucPhieuTrinhPrintTests.cs` | Smoke test (HTTP 200, docx magic bytes) |

## Tham chiếu

- [Spec phiếu trình Word #9469](../9469/phieu-trinh-word-spec.md)
- [Fix print phiếu trình (header/empty)](../phieu-trinh-ke-hoach-print-fix/index.md)
- [Fix thứ tự dự án print](../phieu-trinh-ke-hoach-print-duan-order/index.md)
