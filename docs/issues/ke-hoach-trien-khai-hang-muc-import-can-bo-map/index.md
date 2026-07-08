# Bug import KH triển khai hạng mục — map sai cán bộ

**Module:** QLDA / KeHoachTrienKhaiHangMuc  
**Trạng thái:** ✅ **IMPLEMENTED**  
**Ngày:** 2026-07-08  
**API:** `POST /api/import/ke-hoach-trien-khai-hang-muc`

## Triệu chứng

| Excel (dropdown) | UI sau import |
|------------------|---------------|
| Cán bộ chủ trì: **Đào Thị Bích Tuyền** | **Lê Trường Anh** |
| Cán bộ phối hợp: **Đặng Trung Nghĩa** | **Võ Nguyễn Nhựt Minh** |

Import **thành công** (không báo lỗi) nhưng tên hiển thị trên màn hình sai.

## Kết luận nhanh

| # | Phát hiện | Mức độ | Ảnh hưởng cán bộ |
|---|-----------|--------|------------------|
| 1 | Import lưu `UserMaster.Id` (PK) nhưng UI combobox dùng `UserPortalId` làm `Id` | **Cao — root cause** | Hiển thị sai tên trên UI |
| 2 | `ReadDataFromExcel` map cột theo **thứ tự property**, không theo header `[Description]` | Trung bình | Rủi ro lệch cột nếu đổi thứ tự DTO |
| 3 | Cột Excel / DTO / template hiện tại **khớp nhau** (11 cột) | OK | Không phải nguyên nhân chính |
| 4 | Lookup cán bộ **theo tên**, không fallback index | OK | Logic đúng |
| 5 | Nguồn dropdown template vs import handler **cùng filter** (`LaDonViChinh` + `DonViId`) | OK | Không lệch danh sách |
| 6 | Filter đơn vị import vs template **khác nhau** (`DonViCapChaId != null`) | Thấp | Chỉ ảnh hưởng đơn vị, không cán bộ |

## Đã xử lý

| # | Thay đổi |
|---|----------|
| 1 | Import lưu `CanBoChuTriId` / `CanBoPhoiHopIds` = `UserPortalId` |
| 2 | Export lookup cán bộ theo `UserPortalId` |
| 3 | Template combo cán bộ dùng `UserPortalId` |
| 4 | Integration + unit test (8/8 passed) |

> **Lưu ý:** Dữ liệu import **trước fix** vẫn có `UserMaster.Id` trong DB — cần re-import hoặc sửa tay.

## Files đã sửa

| File | Thay đổi |
|------|----------|
| `KeHoachTrienKhaiHangMucImportRangeCommand.cs` | Lưu PortalId; đồng bộ filter đơn vị |
| `KeHoachTrienKhaiHangMucExportMappings.cs` | Lookup user theo PortalId |
| `KeHoachTrienKhaiHangMucGetImportTemplateQuery.cs` | ComboData.Id = UserPortalId |
| `KeHoachTrienKhaiHangMucImportCommandTests.cs` | Integration test |
| `KeHoachTrienKhaiHangMucExportMappingsTests.cs` | Unit test export |

## Tài liệu chi tiết

→ [`report.md`](./report.md)

## Files liên quan

| File | Vai trò |
|------|---------|
| `KeHoachTrienKhaiHangMucImportRangeCommand.cs` | Lookup + lưu `UserPortalId` |
| `KeHoachTrienKhaiHangMucImportDto.cs` | DTO đọc Excel |
| `KeHoachTrienKhaiHangMucGetImportTemplateQuery.cs` | Nguồn dropdown template |
| `ExcelImporter.ReadDataFromExcel` | Map cột → property |
| `UserMasterGetComboboxQueryHandler.cs` | UI combobox dùng `UserPortalId` |
| `KeHoachTrienKhaiHangMucExportMappings.cs` | Export lookup `UserPortalId` |

## Tham chiếu

- [Import template column fix](../ke-hoach-trien-khai-hang-muc-import-fix/index.md) — đổi cấu trúc cột template
- [Export danh sách dự án — fix UserPortalId](../danh-sach-du-an-export-excel/report.md) — cùng pattern bug ID user
- [IMPLEMENTATION_GUIDE #9469](../../feature/KeHoachTrienKhaiHangMuc/IMPLEMENTATION_GUIDE.md) — spec cũ ghi `UserMaster.Id` (cần cập nhật sau fix)
