# Report: Issue #9579 - Import Gói Thầu

## Status: ✅ COMPLETED

## Feature
Excel import for Gói thầu (bid packages) with dropdown selectors in template.

## Endpoints
| Endpoint                        | Method | Purpose                                |
| ------------------------------- | ------ | -------------------------------------- |
| `/api/template/import-goi-thau` | GET    | Download Excel template with dropdowns |
| `/api/import/goi-thau`          | POST   | Import Gói thầu từ Excel file        |

## IMPORTANT: Column Position Mapping

**Import dựa vào VỊ TRÍ CỘT (index), không phải property name hay Description.**

Excel `IImporterHelper.ReadDataFromExcel<T>()` map theo thứ tự khai báo properties trong DTO:
- Column A (index 0) → Property thứ 1 trong DTO
- Column B (index 1) → Property thứ 2 trong DTO
- Column C (index 2) → Property thứ 3 trong DTO
- ...

`[Description]` attribute chỉ dùng cho error messages khi validation fail.

## Excel Column → DTO Property (Position-Based)

| Excel Col | Header (VN)                    | DTO Property                        | Type    | Position |
|-----------|--------------------------------|-------------------------------------|---------|----------|
| A         | Tên kế hoạch lựa chọn nhà thầu | `TenKeHoachLuaChonNhaThau`          | string  | 0        |
| B         | Tên gói thầu                    | `Ten`                               | string  | 1        |
| C         | Tóm tắt công việc               | `TomTatCongViecChinhGoiThau`        | string  | 2        |
| D         | Giá trị                         | `GiaTri`                            | long?   | 3        |
| E         | Loại hợp đồng                   | `TenLoaiHopDong`                    | string  | 4        |
| F         | Hình thức lựa chọn nhà thầu     | `TenHinhThucLuaChonNhaThau`          | string  | 5        |
| G         | Phương thức lựa chọn nhà thầu   | `TenPhuongThucLuaChonNhaThau`       | string  | 6        |
| H         | Nguồn vốn                       | `TenNguonVon`                       | string  | 7        |
| I         | Thời gian tổ chức lựa nhà thầu  | `ThoiGianToChucLuaChonNhaThau`      | string  | 8        |
| J         | Thời gian bắt đầu tổ chức       | `ThoiGianBatDauToChucLuaChonNhaThau`| string  | 9        |
| K         | Thời gian thực hiện (ngày)      | `ThoiGianThucHienGoiThau`           | int?    | 10       |
| L         | Tùy chọn mua thêm               | `TuyChonMuaThem`                    | string  | 11       |

## Template Combos (Dropdown Data)
| # | Entity                      | Combo Name | Data Source           |
|---|-----------------------------|------------|-----------------------|
| 1 | KeHoachLuaChonNhaThau       | $cbo1      | Tên kế hoạch          |
| 2 | NguonVon                    | $cbo2      | Tên nguồn vốn         |
| 3 | HinhThucLuaChonNhaThau      | $cbo3      | Tên hình thức         |
| 4 | PhuongThucLuaChonNhaThau    | $cbo4      | Tên phương thức       |
| 5 | LoaiHopDong                 | $cbo5      | Tên loại hợp đồng     |

## Import Flow
1. FE reads Excel → `GoiThauImportDto` (position-based mapping)
2. POST to `/api/import/goi-thau` with `GoaThauImportRangeCommand`
3. Handler maps `TenKeHoachLuaChonNhaThau` → KeHoach lookup → get `Id` + `DuAnId`
4. Handler maps string names → IDs for LoaiHopDong, HinhThuc, PhuongThuc, NguonVon
5. Insert `GoiThau` with `DaDuyet = false`

## Tests
- `GoaThauImportTests.cs`: 4 tests, all passed
- Pre-existing failures: 12 (QuyetDinhDieuChinhControllerTests) - unrelated

## Files
| File                                                              | Status     |
| ----------------------------------------------------------------- | ---------- |
| `QLDA.Application/GoiThaus/Commands/GoiThauImportRangeCommand.cs` | ✅ Created  |
| `QLDA.Application/GoiThaus/DTOs/GoiThauImportDto.cs`              | ✅ Modified |
| `QLDA.WebApi/Controllers/ImportController.cs`                     | ✅ Modified |
| `QLDA.WebApi/Controllers/TemplateController.cs`                   | ✅ Modified |
| `QLDA.Tests/Integration/GoaThauImportTests.cs`                    | ✅ Created  |

## Commit
`c4b2f0b` - refactor(import): remove unused GoaThau import models and streamline DTO