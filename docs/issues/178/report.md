# Implementation Report — Issue #178 Form KHLCNT bổ sung field

## Issue #178 | Branch: `178-9665---feature/add-ke-hoach-lcnt-fields`
## Status: CODE DONE — migration `SoLuongGoiThau` để user tự tạo

Chi tiết survey: [`index.md`](./index.md). Nền tảng 4 field: [#170](../170/report.md).

## Summary

| Field nghiệp vụ | Status |
|-----------------|--------|
| Tổng dự toán | Đã có (#170) |
| Dự toán thẩm định | Đã có (#170) |
| Nguồn vốn (CBB theo dự án) | Đã có (#170) + reuse CBB |
| Thời gian thực hiện (năm) | Đã có (#170) |
| Số lượng gói thầu | **Approach B** — `int? SoLuongGoiThau` (code done, chờ migration tay) |

## Module

- Form: `KeHoachLuaChonNhaThau` — `api/ke-hoach-lua-chon-nha-thau`
- Không đụng `QuyetDinhDuyetKHLCNT`

## Checklist

- [x] Survey + docs
- [x] Chốt Approach B (persist; user sẽ migration tay)
- [x] Domain `SoLuongGoiThau`
- [x] DTO Insert/Update/Dto
- [x] Mapping
- [x] List projection
- [ ] Migration — **user tự** `ef.bat QLDA add …`
- [ ] Build / test API sau khi apply migration
- [ ] Commit / PR (khi user yêu cầu)

## Files Changed (#178)

| Layer | File | Thay đổi |
|-------|------|----------|
| Domain | `QLDA.Domain/Entities/KeHoachLuaChonNhaThau.cs` | +`SoLuongGoiThau` |
| Application | `DTOs/KeHoachLuaChonNhaThauDto.cs` | +field |
| Application | `DTOs/KeHoachLuaChonNhaThauInsertDto.cs` | +field |
| Application | `DTOs/KeHoachLuaChonNhaThauUpdateDto.cs` | +field |
| Application | `KeHoachLuaChonNhaThauMappings.cs` | map field |
| Application | `Queries/KeHoachLuaChonNhaThauGetDanhSachQuery.cs` | projection |
| Docs | `docs/issues/178/*` | cập nhật |

EF Configuration: không cần đổi (int? default).

## Migration (user tự làm)

```bat
ef.bat QLDA add AddKeHoachLuaChonNhaThauSoLuongGoiThau
```

Cần cột trên `KeHoachLuaChonNhaThau`:

- `SoLuongGoiThau` `int` NULL

Không sửa migration cũ / không sửa tay snapshot.

## PR

- PR: *(chưa có)*
