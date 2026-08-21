# Journal

## 2026-08-21

- Trace source: `SoLuongGoiThau` trên `KeHoachLuaChonNhaThau` (`int?`), snapshot bảng cùng tên, migration `20260812044306_AddKeHoachLuaChonNhaThauSoLuongGoiThau`.
- `QuyetDinhDuyetKHLCNT` entity/config/snapshot **chưa** có cột. GET chi tiết map từ `KeHoachLuaChonNhaThau?.SoLuongGoiThau`. Write QĐ không persist field.
- 4 cột mới đúng trên KHLCNT: `TongDuToan`, `DuToanThamDinh`, `NguonVonId`, `ThoiGianThucHien`.
- Không Validator. Không `ef remove` migration #178. Docs only — chưa sửa code theo task này.
