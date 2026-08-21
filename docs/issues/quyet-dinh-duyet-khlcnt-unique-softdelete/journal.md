# Journal

## 2026-08-21

### Sáng — khảo sát + docs

- Bug unique khi `POST them-moi` QĐ duyệt KHLCNT sau soft-delete.
- Index `IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId` tới từ `HasOne/WithOne`, filter mặc định `[KeHoachLuaChonNhaThauId] IS NOT NULL`, không loại `IsDeleted`.
- Delete handler set `IsDeleted = true` — đúng soft-delete; lỗi ở schema.
- Pattern đúng sẵn có: `HopDongConfiguration`, `DangTaiKeHoachLcntLenMangConfiguration`.
- Docs: `index.md` / `report.md` / `test-workflow.md`.

### Chiều — implement + migrate

- Thêm `HasIndex` + `HasFilter("[IsDeleted] = 0")` vào `QuyetDinhDuyetKHLCNTConfiguration`. Giữ `WithOne`.
- `QLDA.Persistence` build OK.
- `ef.bat QLDA add FilterQuyetDinhDuyetKHLCNTUniqueIndexExcludeDeleted` — thành công. Log 10622/10400 là warn model sẵn có, không fail.
- File: `20260821061550_FilterQuyetDinhDuyetKHLCNTUniqueIndexExcludeDeleted.cs` (`Up`: drop + create unique `WHERE [IsDeleted] = 0`).
- `ef.bat add` **không** đổi index SQL. Screenshot API vẫn lỗi unique trên `732a4c8e-93af-4794-82f6-074615dadfc1` vì chưa `update`.
- `ef.bat QLDA update` trên catalog `VI_DACDT` — apply `20260821061550_...`: `DROP INDEX` rồi `CREATE UNIQUE INDEX ... WHERE [IsDeleted] = 0`.
- Verify SQL: filter `([IsDeleted]=(0))`; history có migration; GUID screenshot còn 1 dòng `IsDeleted = 1`; không có duplicate active.
- Phạm vi: cả bảng `QuyetDinhDuyetKHLCNT`. Staging/prod chưa apply. Chưa commit/push.
