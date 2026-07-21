# Design: ISS 9467 — Xuất tờ trình phân khai kinh phí

**Ngày:** 2026-07-15  
**Module:** `PhanKhaiKinhPhi` (UC40)  
**Phạm vi (đã chốt):** **Chỉ** hoàn thiện **xuất tờ trình phân khai kinh phí**  
(`GET /api/print/phieu-trinh-phan-khai-kinh-phi` + `ToTrinhPhanKhaiKinhPhi.docx`).  
**Không** làm / không sửa: import, Excel danh sách/kết quả vốn, phiếu trình giao nhiệm vụ (`phieu-trinh-giao-nhiem-vu-...`).  
**Không commit trong phiên docs** — implement + commit khi user duyệt design.

---

## 0. Tóm tắt nhanh (mục 8 — báo cáo trước khi code)

| Hạng mục | Kết luận khảo sát source |
|----------|---------------------------|
| Import lưu vào đâu? | Bảng/entity **`PhanKhaiKinhPhi`** (1 dòng = **1 bản ghi**; không có bảng cha) |
| ID input API xuất? | **`Guid id` = `PhanKhaiKinhPhi.Id`** → xuất **đúng 1 bản ghi** đó (**B1**, đã chốt) |
| Template? | **`PrintTemplates/Word/ToTrinhPhanKhaiKinhPhi.docx`** — **file chưa có trong repo** (blocker) |
| API mẫu? | Hoàn thiện stub `InPhieuTrinhPhanKhaiKinhPhi`; pattern tham chiếu: phiếu trình KH hạng mục / `WordHelper` |
| File dự kiến sửa | Xem §7 |
| Impact | **MEDIUM** trên `InPhieuTrinhPhanKhaiKinhPhi` + print query; **không** migration (§8) |

---

## 1. Bối cảnh & mục tiêu

Issue gốc có 2 phần: (1) import file phân khai, (2) xuất tờ trình. Import **đã xong**. Task này hoàn thiện xuất tờ trình từ **dữ liệu đã lưu** (tạo tay hoặc import), không đọc lại Excel.

### Ngoài phạm vi (cứng)

- Import file phân khai (mọi handler / template / validation).
- `phieu-trinh-giao-nhiem-vu-phan-khai-kinh-phi` và Excel print liên quan `PhanKhaiKinhPhi`.
- Migration / bảng trung gian chỉ để in.
- Gom nhiều bản ghi theo `SoToTrinh` (B2) — **không** làm trong task này.
- Hard-code path template ngoài `AppContext.BaseDirectory` + `PrintTemplates/Word/...`.

---

## 2. Hiện trạng source (đã đọc)

### 2.1 Model dữ liệu

**Entity** `QLDA.Domain/Entities/PhanKhaiKinhPhi.cs` → table `PhanKhaiKinhPhi`:

| Field | Ý nghĩa |
|-------|---------|
| `Id` | PK – identity từng dòng phân khai |
| `DuAnId` / `DuAn` | Dự án / dự toán |
| `BuocId` | Bước workflow |
| `SoToTrinh`, `TrichYeu`, `NgayToTrinh` | Thông tin tờ trình |
| `NguonVonId` / `NguonVon` | Nguồn vốn |
| `KinhPhiDeXuat`, `KinhPhiPhanKhai` | Số tiền |
| `ThuyetMinh` | Nội dung / thuyết minh phân khai |
| `TrangThaiId` / `TrangThai` | Trạng thái phê duyệt |

**Không có** `BatchId` / parent “hồ sơ phân khai”. Quan hệ “nhiều dự án trên một tờ trình” **không được model hóa tường minh** — chỉ có thể suy luận mềm qua `SoToTrinh` (+ tùy chọn `NgayToTrinh`) nếu nghiệp vụ nhập cùng số.

### 2.2 Import — lưu gì

- **API template:** `GET api/template/import-phan-khai-kinh-phi` → `Import_PhanKhaiKinhPhi.xlsx`
- **Handler:** `PhanKhaiKinhPhiImportRangeCommand`  
  - Resolve `TenDuAn` → `DuAnId`  
  - Parse nguồn vốn (`PhanKhaiKinhPhiImportDisplay`) → `NguonVonId` thuộc dự án  
  - Insert nhiều `PhanKhaiKinhPhi` độc lập (`SuccessCount = số dòng OK`)  
  - Trạng thái mặc định: `DuThao`

→ Xuất tờ trình **phải query DB**, không đọc lại file import.

### 2.3 API xuất tờ trình — đã stub, chưa đủ

```
GET /api/print/phieu-trinh-phan-khai-kinh-phi?id={guid}
→ PrintController.InPhieuTrinhPhanKhaiKinhPhi
→ PhanKhaiKinhPhiGetDanhSachExportQuery { Id = id }
→ WordHelper.ExportFromTemplate (scalar fields only)
→ ToTrinhPhanKhaiKinhPhi.docx   ❌ FILE THIẾU
```

**Lỗ hổng hiện tại:**

1. Template `ToTrinhPhanKhaiKinhPhi.docx` **không tồn tại** dưới `QLDA.WebApi/PrintTemplates/Word/` (chỉ có `PhieuTrinhGiaoNhiemVu.docx`, `PhieuTrinhKeHoach...`, …).
2. Chỉ map vài field header từ `data[0]`; **không** bảng nhiều dòng / không tính tổng đầy đủ.
3. `NullReference` risk khi `data` rỗng (`data![0]`).
4. **Không** `[Authorize(Roles = ...)]` (khác Excel export cùng module đã gắn `GroupPhanKhaiKinhPhiExport`).
5. Reuse `GetDanhSachExportQuery` — thiếu `TenNguonVon`, `ThuyetMinh`, auth `FilterVisible`; designed cho Excel danh sách, không cho phiếu trình Word.
6. Tên file download hiện qua `GetDownloadFileName` → `ToTrinhPhanKhaiKinhPhi_ddMMyyyy_HHmmss.docx` (chưa khớp naming “to-trinh-phan-khai-kinh-phi” trong AC — có thể đổi khi hoàn thiện).

### 2.4 Sibling API (tham chiếu gần)

| Endpoint | Template | Data |
|----------|----------|------|
| `GET .../phieu-trinh-giao-nhiem-vu-phan-khai-kinh-phi?id=` | `PhieuTrinhGiaoNhiemVu.docx` ✅ | `PhanKhaiKinhPhiGetQuery` — **1 entity** |
| `GET .../danh-sach-phan-khai-kinh-phi` | Excel | Filter grid, có role |
| `GET .../ket-qua-phan-khai-von-duoc-duyet` | Excel | Đã duyệt |

Placeholder `PhieuTrinhGiaoNhiemVu` (MailMerge «Field»): `So`, `NgayToTrinh`, `TenDuAn`, `TenNguonVon`, `TrichYeu`, `KinhPhiDeXuat`, `ThuyetMinh` (+ controller còn đẩy `KinhPhiPhanKhai`, `TongMucDauTu`).

### 2.5 Pattern xuất phiếu trình **đúng chuẩn** trong project

`GET /api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc?id=` (#9469):

1. Controller mỏng → MediatR `*GetPhieuTrinhPrintQuery`
2. Query load header + **toàn bộ dòng con** (không pageSize)
3. `ManagedException` khi không có dữ liệu in được
4. `*WordExporter` (Infrastructure): MailMerge / `Range.Replace` header + fill **table**
5. `[Authorize(Roles = Group...Export)]`

`IWordHelper` cũng hỗ trợ overload `ExportFromTemplate(path, DataSet tables, fieldValues)` + `ExecuteWithRegions` (bảng MailMerge vùng) — phù hợp nếu template dùng region; KeHoach chọn fill table bằng code vì template góc.

---

## 3. Phân quyền

Issue yêu cầu: **CB.PKH-TC**, **LD.PKH-TC**.

Trong code, các role NVTT được map vào group ASP.NET:

```csharp
RoleConstants.GroupPhanKhaiKinhPhiExport =
    $"{QLDA_TatCa},{QLDA_QuanTri},{QLDA_LDDV},{QLDA_ChuyenVien}";
```

Đã dùng cho `danh-sach-phan-khai-kinh-phi` / kết quả phân khai. **Khuyến nghị:** gắn cùng attribute cho `InPhieuTrinhPhanKhaiKinhPhi` — không invent role string riêng.

Bổ sung phía query (khi viết print query mới): `FilterVisible(..., AuthorizationResourceKeys.DuAn)` vì entity có `DuAnId` (list query hiện tại **chưa** FilterVisible — lỗ hổng sẵn có; print mới nên làm đúng rule, không “sửa lan” list trừ khi bắt buộc).

---

## 4. Các hướng tiếp cận

### A — Hoàn thiện stub hiện có + scalar MailMerge only (1 bản ghi / Id)

- Giữ query `GetDanhSachExportQuery` hoặc chuyển sang `GetQuery`.
- Thêm file template với placeholder khớp controller.
- Auth + empty-check.

| Ưu | Nhược |
|----|-------|
| Ít file, khớp API đã có | **Không** đáp ứng AC “nhiều dòng / đủ danh sách”; dễ lệch mẫu tờ trình thật |

### B — Print query riêng + WordExporter + bảng chi tiết (khuyến nghị)

Giống #9469 KeHoach phiếu trình:

- `PhanKhaiKinhPhiGetPhieuTrinhPrintQuery` + DTO header + `Rows[]`
- (Tùy quyết định §5) gom dòng theo `SoToTrinh` hoặc chỉ 1 Id
- `PhanKhaiKinhPhiWordExporter` hoặc `WordHelper` + `DataSet` region
- Sửa `InPhieuTrinhPhanKhaiKinhPhi`: auth, empty message, download name
- **Bắt buộc** có `ToTrinhPhanKhaiKinhPhi.docx`

| Ưu | Nhược |
|----|-------|
| Đúng architecture CQRS + pattern Print Word đã proven | Nhiều file hơn A; cần template + quy ước gom dòng |

### C — In theo filter danh sách (nhiều Id / search như Excel)

- Endpoint nhận search model thay vì 1 Id.

| Ưu | Nhược |
|----|-------|
| In “mẻ” linh hoạt | **Lệch** convention đã stub (`?id=`); FE/UC nói “hồ sơ/lần phân khai” |

**Khuyến nghị: B rút gọn (B1)** — giữ endpoint `?id=`, map 1 bản ghi, không WordExporter phức tạp trừ khi template bắt buộc bảng.

---

## 5. Quyết định đã chốt — ID / phạm vi dòng

| Option | Input `id` | Phạm vi xuất | Trạng thái |
|--------|------------|--------------|------------|
| **B1** | `PhanKhaiKinhPhi.Id` | **Chỉ 1 bản ghi** (header + nội dung theo template) | **Đã chốt** |
| B2 | Anchor + cùng `SoToTrinh` | Nhiều dòng | Out of scope |
| B3 | Parent mới | Cần migration | Out of scope |

**Lý do:** Entity phẳng; API stub sẵn dùng 1 `id`; user xác nhận phạm vi **chỉ** xuất tờ trình phân khai kinh phí (không mở rộng gom hồ sơ / giao nhiệm vụ).

Khi không tìm thấy bản ghi:

```text
Không tìm thấy dữ liệu phân khai kinh phí để xuất tờ trình.
```

Không xuất file trắng.

---

## 6. Thiết kế đề xuất (Approach B — sau khi chốt §5)

### 6.1 Luồng

```
FE → GET /api/print/phieu-trinh-phan-khai-kinh-phi?id={PhanKhaiKinhPhi.Id}
  → [Authorize GroupPhanKhaiKinhPhiExport]
  → PhanKhaiKinhPhiGetPhieuTrinhPrintQuery (hoặc load đủ nav + FilterVisible)
       → 1 bản ghi theo Id
       → Throw ManagedException nếu không có
  → map field ↔ placeholder template
  → WordHelper.ExportFromTemplate + ToTrinhPhanKhaiKinhPhi.docx
  → File .docx (Content-Type Word OOXML)
```

### 6.2 Map dữ liệu tối thiểu (1 bản ghi — theo entity + stub hiện có)

| Placeholder (stub / sibling) | Nguồn |
|------------------------------|--------|
| `So` | `SoToTrinh` |
| `ngay` / `NgayToTrinh` / `NamToTrinh` | `NgayToTrinh` (+7) |
| `TrichYeu` | `TrichYeu` |
| `TenDuAn` | `DuAn.TenDuAn` |
| `TenNguonVon` | `NguonVon.Ten` |
| `ThuyetMinh` | `ThuyetMinh` |
| `KinhPhiDeXuat` | `KinhPhiDeXuat` |
| `KinhPhiPhanKhai` | `KinhPhiPhanKhai` |
| `TongMucDauTu` | `DuAn.TongMucDauTu` |

> Chốt tên placeholder theo file `ToTrinhPhanKhaiKinhPhi.docx` khi có template. Không hard-code nội dung nghiệp vụ.

### 6.3 Template

| Item | Chi tiết |
|------|----------|
| Path runtime | `{BaseDir}/PrintTemplates/Word/ToTrinhPhanKhaiKinhPhi.docx` |
| Trạng thái | **Thiếu** — phải thêm vào repo + `CopyToOutputDirectory` như các `.docx` Word khác |
| Engine | `«Field»` MailMerge (`UseNonMergeFields = true`) và/hoặc region bảng; hoặc fill table kiểu KeHoach |

**Action item trước/implement:** lấy mẫu Word chuẩn từ BA/issue hoặc regenerate; inspect placeholder rồi mới finalize DTO map.

### 6.4 Tên file tải về

Ưu tiên AC: chứa `to-trinh-phan-khai-kinh-phi`, ví dụ:

`to-trinh-phan-khai-kinh-phi_{ddMMyyyy_HHmmss}.docx`

(hoặc giữ `GetDownloadFileName` với file template đổi tên cho khớp).

---

## 7. Danh sách file dự kiến

### Tạo mới

| File | Vai trò |
|------|---------|
| `QLDA.WebApi/PrintTemplates/Word/ToTrinhPhanKhaiKinhPhi.docx` | Template (blocker) |
| `QLDA.Application/PhanKhaiKinhPhis/Queries/PhanKhaiKinhPhiGetPhieuTrinhPrintQuery.cs` | Load 1 Id + FilterVisible + empty check *(nếu không đủ GetQuery)* |
| `QLDA.Application/PhanKhaiKinhPhis/DTOs/PhanKhaiKinhPhiPhieuTrinhPrintDto.cs` | DTO map field in *(nếu cần tách khỏi entity)* |
| `QLDA.Tests/...` | AC: not found + file content type / map cơ bản |

### Sửa

| File | Thay đổi |
|------|----------|
| `QLDA.WebApi/Controllers/PrintController.cs` — `InPhieuTrinhPhanKhaiKinhPhi` | Auth, gọi print query, empty handling, download name, bỏ map lỗi trên `data[0]` |
| DI registration (nếu thêm WordExporter) | Đăng ký exporter giống KeHoach |

### Không đụng

- Import (`PhanKhaiKinhPhiImportRangeCommand`, descriptor, template import)  
- Migration / schema `PhanKhaiKinhPhi`  
- Excel: `danh-sach-phan-khai-kinh-phi`, `ket-qua-phan-khai-von-duoc-duyet`  
- Word sibling: `InPhieuTrinhGiaoNhiemVuPhanKhai` / `PhieuTrinhGiaoNhiemVu.docx`  
- Gom B2 theo `SoToTrinh`

---

## 8. Impact analysis (tĩnh — GitNexus MCP không khả dụng session này)

**Targets dự kiến chỉnh:**

| Symbol | Risk ước lượng | Callers / note |
|--------|----------------|----------------|
| `PrintController.InPhieuTrinhPhanKhaiKinhPhi` | **MEDIUM** | Endpoint FE/Print; thay body implementation; contract URL/query `id` **giữ nguyên** |
| `PhanKhaiKinhPhiGetDanhSachExportQuery` | **LOW** nếu ngừng reuse cho Word | Vẫn dùng cho Excel `danh-sach-phan-khai-kinh-phi` — **không** phá nếu tạo query print riêng |
| `WordHelper.ExportFromTemplate` | **LOW** | Shared; chỉ gọi thêm / overload regions |
| `PhanKhaiKinhPhi` entity | **NONE** (không sửa) | |

**Blast radius khuyến nghị:** chỉ Print Word phiếu trình PKKP + Application print query/DTO (+ exporter).  
Trước lúc edit symbol: chạy lại `gitnexus_impact` khi MCP/index sẵn sàng.  
Trước commit: `detect_changes`.

---

## 9. Tiêu chí nghiệm thu (map AC)

| AC | Cách đạt |
|----|----------|
| Import thành công → in được | Query DB entity sau import |
| Tạo tay → in được | Cùng query |
| Đúng template / mở được / Content-Type Word | Template + Aspose Word |
| Map dự án, nguồn vốn, nội dung, giá trị, ngày, … | Map 1 entity + nav theo template |
| (AC multi-dòng hồ sơ) | **Ngoài phạm vi B1** — mỗi `Id` = một lần xuất |
| Không dữ liệu → lỗi nghiệp vụ, không file trắng | `ManagedException` trước export |
| Role CB/LD.PKH-TC | `GroupPhanKhaiKinhPhiExport` |
| Không ảnh hưởng import | Không sửa import handlers |
| Build OK, no migration | Không đụng Migrator |

---

## 10. Open questions còn lại

1. ~~B1 vs B2~~ → **B1 đã chốt** (chỉ xuất tờ trình PKKP / 1 Id).  
2. File mẫu `ToTrinhPhanKhaiKinhPhi.docx` lấy từ đâu (BA / attach issue / clone chỉnh từ `PhieuTrinhGiaoNhiemVu`)? ← còn blocker  
3. Download name: `to-trinh-phan-khai-kinh-phi_...` hay `ToTrinhPhanKhaiKinhPhi_...`? (mặc định đề xuất slug AC nếu không ý kiến)

---

## 11. Next steps

1. Implementation plan (đã có bước code):  
   [`docs/superpowers/plans/2026-07-15-iss9467-xuat-to-trinh-phan-khai-kinh-phi.md`](../plans/2026-07-15-iss9467-xuat-to-trinh-phan-khai-kinh-phi.md)  
2. Chạy plan (subagent / inline) — không sửa import; impact trước khi edit symbol.
