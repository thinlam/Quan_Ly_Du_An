# Lỗi in phiếu trình KH triển khai — thiếu Dự án & sai thứ tự giai đoạn

> Ngày ghi nhận: 06/07/2026  
> Trạng thái: ✅ **IMPLEMENTED** (commit `af1fa46`, 06/07/2026)  
> Liên quan: [#9469 phieu-trinh-word-spec](../9469/phieu-trinh-word-spec.md), [print-fix trước đó](../phieu-trinh-ke-hoach-print-fix/index.md)

## Tóm tắt

API in phiếu trình Word (`GET /api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc`) trước đây **không khớp UI** ở hai điểm. Đã fix trên BE:

| # | Triệu chứng (trước fix) | Trạng thái |
|---|------------------------|------------|
| 1 | Dòng `Dự án:` trên file in **trống** dù form đã chọn dự án `t01` | ✅ Fixed (code) |
| 2 | Thứ tự giai đoạn/hạng mục **đảo ngược** so với UI | ✅ Fixed (code) |

## Endpoint

```http
GET /QuanLyDuAn/api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc?id={keHoachId}
Authorization: Bearer {token}
Role: GroupKeHoachTrienKhaiHangMucExport
```

## Kết quả implement

| Hạng mục | File | Trạng thái |
|----------|------|------------|
| Sort giai đoạn theo dự án | `KeHoachTrienKhaiHangMucImportGiaiDoanHelper.cs` | ✅ |
| Map rows (Excel + Word) | `KeHoachTrienKhaiHangMucExportMappings.cs` | ✅ |
| Print handler (Word) | `KeHoachTrienKhaiHangMucGetPhieuTrinhPrintQuery.cs` | ✅ |
| Excel export | `KeHoachTrienKhaiHangMucGetExportQuery.cs` | ✅ |
| Unit test | `KeHoachTrienKhaiHangMucExportMappingsTests.cs` | ✅ 2 tests |
| Integration test | `KeHoachTrienKhaiHangMucPhieuTrinhPrintTests.cs` | ✅ Partial |

## Điểm quan trọng

| Vấn đề | Fix as-built |
|--------|--------------|
| Dự án trống | `EnsureDuAnLoadedAsync` + `DuAnDisplay` = `{MaDuAn} — {TenDuAn}` |
| Sai thứ tự giai đoạn | `GetGiaiDoanSortByDuAnAsync` (`DuAnBuoc.Buoc.Stt`) |
| Sai thứ tự HM | Giữ index list gốc; load HM `OrderBy CreatedAt` |
| Excel + Word | Cùng `ExportMappings.ToExportRowsAsync` |

## QA còn lại

1. In phiếu trình → kiểm tra `Dự án:` và thứ tự group/hạng mục khớp UI.
2. Export Excel cùng `id` — thứ tự đồng bộ với phiếu trình.

Chi tiết: **[report.md](report.md)**
