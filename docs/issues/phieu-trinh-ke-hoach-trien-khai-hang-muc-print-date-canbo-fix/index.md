# Fix triệt để — In phiếu trình KH triển khai hạng mục

**Module:** QLDA / KeHoachTrienKhaiHangMuc  
**Trạng thái:** ✅ **IMPLEMENTED**  
**Ngày:** 2026-07-08  
**API:** `GET /api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc`

## Triệu chứng

UI hiển thị đúng, nhưng file Word in ra sai ở hai nhóm dữ liệu:

| Vấn đề | UI | Word (trước fix) |
|--------|-----|------------------|
| Ngày bắt đầu / kết thúc | `08/07/2026` | `2026-07-08` |
| Cán bộ chủ trì / phối hợp | Đào Thị Bích Tuyền, Đặng Trung Nghĩa… | Nguyễn Thanh Hải, Nguyễn Văn Dũng… |

## Kết luận nhanh

| # | Root cause | Mức độ |
|---|------------|--------|
| 1 | DTO export truyền `DateTime?` → Word/Aspose render ISO `yyyy-MM-dd` | **Cao** (đã fix → `DateOnly?`) |
| 2 | Export lookup cán bộ theo `UserMaster.Id` trong khi DB/UI lưu `UserPortalId` | **Cao** |
| 3 | UI combobox (`UserMasterGetComboboxQuery`) dùng `UserPortalId` làm `Id` | Tham chiếu đúng |

## Đã sửa

| # | Thay đổi |
|---|----------|
| 1 | `NgayBatDau` / `NgayKetThuc` trong export DTO → **`DateOnly?`** (cùng kiểu entity) |
| 2 | Format `dd/MM/yyyy` tại **tầng export** — Word `FormatDate(DateOnly?)`, Excel `PutValueSmart` |
| 3 | Lookup cán bộ theo `UserPortalId` (khớp UI combobox + import) |
| 4 | Unit test: format ngày, null date, chủ trì + phối hợp theo PortalId |

> **Lưu ý:** Dữ liệu import/nhập **trước** khi fix import-can-bo-map vẫn có thể lưu `UserMaster.Id` — cần re-import hoặc sửa tay.

## Files đã sửa

| File | Thay đổi |
|------|----------|
| `KeHoachTrienKhaiHangMucExportItemDto.cs` | `NgayBatDau`/`NgayKetThuc`: `DateTime?` → **`DateOnly?`** |
| `KeHoachTrienKhaiHangMucExportMappings.cs` | Gán `DateOnly?` trực tiếp; lookup `UserPortalId` |
| `KeHoachTrienKhaiHangMucWordExporter.cs` | `FormatDate(DateOnly?)` khi bind cell |
| `AsposeHelper.cs` | `PutValueSmart` xử lý `DateOnly` → `dd/MM/yyyy` |
| `KeHoachTrienKhaiHangMucExportMappingsTests.cs` | +4 unit tests |

## Tài liệu chi tiết

→ [`report.md`](./report.md)

## Đối chiếu UI vs Print

| Khía cạnh | API UI | API Print |
|-----------|--------|-----------|
| ID cán bộ lưu DB | `CanBoChuTriId` / `CanBoPhoiHopIds` = **UserPortalId** | Cùng field |
| Nguồn tên cán bộ | Combobox `UserMasterDto.Id = UserPortalId` | `ExportMappings` join `UserPortalId` |
| Format ngày hiển thị | FE format `DateOnly` → `dd/MM/yyyy` | Word/Excel format tại export layer |

## Tham chiếu

- [Fix format ngày (lần 1 — Word only)](../phieu-trinh-ke-hoach-date-format/index.md)
- [Fix import/map cán bộ UserPortalId](../ke-hoach-trien-khai-hang-muc-import-can-bo-map/index.md)
- [Spec phiếu trình Word #9469](../9469/phieu-trinh-word-spec.md)
