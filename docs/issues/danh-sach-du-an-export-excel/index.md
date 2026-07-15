# Fix export Excel — Danh sách dự án

**Module:** QLDA / DuAn  
**Trạng thái:** ✅ **IMPLEMENTED**  
**Ngày:** 2026-07-08  

## Tóm tắt

API `GET /api/print/danh-sach-du-an` đã được sửa để export Excel khớp grid `GET /api/du-an/danh-sach`.

| # | Đã xử lý |
|---|----------|
| 1 | Bỏ cột **Ngày bắt đầu** |
| 2 | **Lãnh đạo phụ trách** lookup đúng `UserPortalId` |
| 3 | Thứ tự dòng đồng bộ `Index DESC` với grid |
| 4 | Template format **`LetterheadExport`** (giống `KeHoachTrienKhaiHangMuc.xlsx`) |

## Tài liệu chi tiết

→ [`report.md`](./report.md)

## Files đã sửa

| File | Thay đổi |
|------|----------|
| `QLDA.Gen/Descriptors/DanhSachDuAnExportDescriptor.cs` | `LetterheadExport`; xóa `ngayBatDau`; alignment cột |
| `QLDA.Application/DuAns/DTOs/DuAnExportDto.cs` | Xóa `NgayBatDau` |
| `QLDA.Application/DuAns/Queries/DuAnGetDanhSachExportQuery.cs` | Sort + lookup + mapping |
| `QLDA.WebApi/ExportTemplates/DanhSachDuAn.xlsx` | Regen: `dotnet run --project QLDA.Gen -- danh-sach-du-an --force` |

## Regen template

```bash
dotnet run --project QLDA.Gen -- danh-sach-du-an --force
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```

## Không sửa

- Migration / `AppDbContextModelSnapshot.cs`
- `PrintTemplates/DanhSachDuAnTraCuu.xlsx` (endpoint tra cứu SP riêng)
