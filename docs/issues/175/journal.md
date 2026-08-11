# Journal — #175 Trình duyệt dự toán (`DuToanDauTu`)

## 2026-08-11 — Survey source + viết docs (chưa code)

**Việc làm:**

- Xác định màn **Trình duyệt dự toán** / form **Tờ trình phê duyệt dự toán** = module BE `DuToanDauTu` (`api/du-toan-dau-tu`), không phải `PheDuyetDuToan` / `QuyetDinhDuyetDuToan`.
- Trace Controller → Insert/Update/Get/List + attachment `EGroupType.DuToanDauTu`.
- Xác nhận **chưa có** `Ten` / `TenDuToan` trên Entity/DTO/DB.
- Xác nhận `PhuongAnThietKeId` còn map đầy đủ; yêu cầu chỉ bỏ UI, không drop cột.
- Survey `EGroupType`: chỉ có `DuToanDauTu`; chưa có CongVan/BaoGia/`_Khac` cho module này.
- Pattern multi-file tham chiếu: `QuyetDinhDuyetDuToan` + `QuyetDinhDuyetDuToan_Khac`.
- FE không có trong workspace `E:/SER`.
- Viết docs: `index.md`, `report.md`, `test-workflow.md`, journal này.
- Issue folder: `docs/issues/175/` (user chốt số **#175**).

**Quyết định tạm (chờ duyệt trước khi code):**

1. Thêm `Ten` trên `DuToanDauTu` → **có migration** — **user tự tạo tay** (`ef.bat`); agent không chạy Migrator.
2. Công văn đề nghị báo giá: **reuse** `EGroupType.DuToanDauTu` + `DanhSachTepDinhKem`; validate ≥1 file.
3. Khác: thêm `EGroupType.DuToanDauTu_Khac` + `DanhSachTepDinhKemKhac`; optional.
4. Phương án thiết kế: giữ DB; bỏ form / không bắt buộc.
5. Không tạo WebApi Model; không drop column; không sửa migration cũ.

**Chưa làm:** FE, commit/PR. Migration do user.

---

## 2026-08-11 — Implement BE (không migration)

**Đã code:**

| Hạng mục | Chi tiết |
|----------|----------|
| `Ten` | Entity + Config MaxLength(500) + DTO + Mapping + Insert/Update validate |
| `DuToanDauTu_Khac` | Enum + `DanhSachTepDinhKemKhac` |
| Công văn bắt buộc | Controller Create/Update validate ≥1 `DanhSachTepDinhKem` |
| Hydrate | Get/Update chi tiết 2 `GetAttachmentsQuery` + BaseGroupTypes |
| List | `ExpandGroupTypes` tách Công văn / Khác; project `Ten` |

**Build:** Domain / Application / Persistence / WebApi — 0 error.

**User tiếp theo:**

```bat
ef.bat QLDA add AddDuToanDauTuTen
```

Rồi apply DB + FE bind `ten` / ẩn Phương án thiết kế / 2 upload zone.
