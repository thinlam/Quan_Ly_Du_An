# Journal — #169 Chỉnh sửa màn hình 9667

## 2026-08-10 — Phân tích source (chưa code)

**Việc làm:**

- Xác định màn 9667 = module BE `KetQuaTrungThau` (`api/ket-qua-trung-thau`); FE không nằm trong repo `E:/SER`.
- Trace `GET goi-thau/combobox` → `GoiThauController.GetCbo` → `GoiThauGetDanhSachQuery` (`IsCbo=true`) + `GoiThauSearchDto`.
- Xác định field thẩm định E-HSMT: `HoSoMoiThauDienTu.ThamDinh` (`bool?`).
- Xác định attachment pattern: BB `Attachment*` + multi-GroupType (`QuyetDinhDuyetDuToan`, `BanGiaoHoSo`).
- Xác định “Trạng thái đăng tải”: khảo sát có cả `bool` (E-HSMT) và `int?` (ToTrinh…).
- Viết docs: `index.md`, `report.md`, `test-workflow.md`.

**Quyết định tạm (chờ duyệt):**

1. Thêm `bool? IsThamDinh` vào `GoiThauSearchDto`, filter qua `HoSoMoiThauDienTu.ThamDinh == true`.
2. Biên bản thương thảo: `EGroupType.KetQuaTrungThau_BienBanThuongThao` + `DanhSachBienBanThuongThao`.
3. Trạng thái đăng tải: thêm `bool TrangThaiDangTai` vào `KetQuaTrungThau` → **có migration**.
4. Label Đơn vị trúng thầu: chỉ FE.

**Chưa làm:** implement code, migration, FE.

---

## 2026-08-10 — Chốt dataType Trạng thái đăng tải

**Xác nhận từ BA/user:** Trạng thái đăng tải có **`dataType = boolean`**, gồm **đã đăng tải** / **chưa đăng tải**.

**Cập nhật docs:**

- BE field: `bool TrangThaiDangTai` (reuse pattern `HoSoMoiThauDienTu`), **không** dùng `TrangThaiDangTaiId`.
- FE: CBB 2 option map `true`/`false`.
- Sửa `index.md`, `report.md`, `test-workflow.md`, journal này.

---

## 2026-08-10 — Implement BE (chưa migration)

**Đã code:**

1. `GoiThauSearchDto.IsThamDinh` + filter trong `GoiThauGetDanhSachQuery` qua `HoSoMoiThauDienTu.ThamDinh`.
2. `EGroupType.KetQuaTrungThau_BienBanThuongThao` + `DanhSachBienBanThuongThao` trên DTO/Controller/list query.
3. `bool TrangThaiDangTai` trên entity + Persistence config (default `false`) + DTO/Mappings.

**Chưa làm (user tự migrate sau):**

```bat
ef.bat add AddTrangThaiDangTaiToKetQuaTrungThau
```

Hoặc tương đương `dotnet ef migrations add ...` qua Migrator. **Không** sửa ModelSnapshot tay.
