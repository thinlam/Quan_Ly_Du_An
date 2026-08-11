# Implementation Report — Bổ sung field Kế hoạch lựa chọn nhà thầu

## Issue #170 | Branch: `170-9663---feature/ke-hoach-lua-chon-nha-thau-bo-sung-thong-tin`
## Status: CODE DONE (migration do dev tự tạo — chưa PR)

Chi tiết BA + survey + thiết kế + gap + bước code: xem [`index.md`](./index.md).

## Summary

Đã bổ sung field trên Domain / EF Config / DTO / Mapping / Insert·Update validate / List projection cho **Kế hoạch lựa chọn nhà thầu**.

| Field nghiệp vụ | Cách xử lý | Status |
|-----------------|------------|--------|
| Số tờ trình | Reuse `So` / `SoQuyetDinh`; message trùng → *"Số tờ trình đã tồn tại"* | Done |
| Tổng dự toán | `TongDuToan` (`long` entity; `long?` Insert/Update + validate required) | Done (chờ migration) |
| Dự toán thẩm định | `DuToanThamDinh` (`long?`) | Done (chờ migration) |
| Nguồn vốn | `NguonVonId` (`int?`) + FK; validate thuộc `DuAnNguonVon` | Done (chờ migration) |
| Thời gian thực hiện (năm) | `ThoiGianThucHien` (`int?`) | Done (chờ migration) |

## Architecture

Không đổi Controller. CBB nguồn vốn: `GET api/danh-muc-nguon-von/danh-sach?duAnId=`.

## Checklist bước code

- [x] 0. Branch (đã có sẵn)
- [x] 1. Domain entity
- [x] 2. EF Configuration (FK `NguonVon`)
- [ ] 3. Migration — **user tự tạo** (`ef.bat QLDA add …`)
- [x] 4. DTO Insert/Update/Dto
- [x] 5. Mappings
- [x] 6. InsertCommand validate
- [x] 7. UpdateCommand validate
- [x] 8. List projection
- [x] 9. Chi tiết qua `ToDto`
- [x] 10. Build compile OK
- [ ] 11. Commit / PR (khi user yêu cầu)

## Files Changed

| Layer | File | Thay đổi |
|-------|------|----------|
| Domain | `QLDA.Domain/Entities/KeHoachLuaChonNhaThau.cs` | +4 field + nav `NguonVon` |
| Persistence | `QLDA.Persistence/Configurations/KeHoachLuaChonNhaThauConfiguration.cs` | FK `NguonVon` |
| Application | `DTOs/KeHoachLuaChonNhaThauDto.cs` | +4 field |
| Application | `DTOs/KeHoachLuaChonNhaThauInsertDto.cs` | +4 field (`TongDuToan` nullable để validate) |
| Application | `DTOs/KeHoachLuaChonNhaThauUpdateDto.cs` | +4 field |
| Application | `KeHoachLuaChonNhaThauMappings.cs` | map 4 field |
| Application | `Commands/KeHoachLuaChonNhaThauInsertCommand.cs` | validate TongDuToan / NguonVon / message số tờ trình |
| Application | `Commands/KeHoachLuaChonNhaThauUpdateCommand.cs` | validate TongDuToan / NguonVon |
| Application | `Queries/KeHoachLuaChonNhaThauGetDanhSachQuery.cs` | projection 4 field |
| Migrator | *(chưa)* | User tự `ef.bat QLDA add` |

## Migration (user tự làm)

Gợi ý:

```bat
ef.bat QLDA add AddKeHoachLuaChonNhaThauBoSungThongTin
```

Cần có trên table `KeHoachLuaChonNhaThau`:

- `TongDuToan` `bigint` NOT NULL — **có data cũ thì set default / backfill trước khi NOT NULL**
- `DuToanThamDinh` `bigint` NULL
- `NguonVonId` `int` NULL + FK → `DanhMucNguonVon`
- `ThoiGianThucHien` `int` NULL

Không sửa migration cũ / không sửa tay snapshot.

## Test / Verify

- [x] Build Application / Persistence / WebApi — 0 error
- [ ] Thêm mới lưu đủ field (sau khi apply migration)
- [ ] `TongDuToan` bắt buộc
- [ ] `DuToanThamDinh` null được
- [ ] `NguonVonId` chọn theo nguồn vốn dự án
- [ ] `ThoiGianThucHien` lưu số năm
- [ ] Update / Chi tiết / Danh sách đồng bộ

## PR

- PR: *(chưa có)*
