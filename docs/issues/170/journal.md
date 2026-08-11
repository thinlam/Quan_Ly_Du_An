# Journal — Issue #170 Bổ sung field KHLCNT

## 2026-08-10

- Survey source `KeHoachLuaChonNhaThau` (Controller / CQRS / DTO / Entity / EF / snapshot).
- Kết luận: thiếu 4 field mới trên entity → cần migration; `So`/`SoQuyetDinh` reuse không rename DB.
- Nguồn vốn reuse `DuAnNguonVon` + `GET api/danh-muc-nguon-von/danh-sach?duAnId=`.
- Viết `index.md` (survey + design). Chưa implement code.
- Chuyển docs từ folder tạm sang `docs/issues/170/`.
- Bổ sung `report.md` (skeleton — status DRAFT, điền files/PR sau khi implement).
- Bổ sung `index.md` §5 gap (chỗ thiếu) + §6 các bước code; sync checklist vào `report.md`.
- Implement code (Domain, Config, DTO, Mapping, Insert/Update, List). **Migration để user tự tạo.**
- Build Application / Persistence / WebApi: succeeded, 0 error.
- Rename `TongDuToanThamDinhGia` → `DuToanThamDinh` (API `duToanThamDinh`) + cập nhật migration chưa apply.
