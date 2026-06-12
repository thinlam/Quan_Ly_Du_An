# Debug Report: Import Gói thầu — `dataResult: []` dù HTTP 200

**Ngày:** June 2026  
**API:** `POST /api/import/goi-thau`  
**Trạng thái:** ✅ **ĐÃ XỬ LÝ XONG** (template + 2 bug code phát sinh sau khi đọc được Excel)

| Lỗi | Triệu chứng | Đã sửa |
|-----|-------------|--------|
| #1 Thiếu Excel Table | `dataResult: []` dù HTTP 200 | ✅ Template `Import_GoiThau.xlsx` |
| #2 Parse `long?` | `giaTri: null` dù Excel có `10000000` | ✅ `StringExtension.ConvertStringToPropertyType` |
| #3 Lookup danh mục | `NguonVonId`, `HinhThucId`… NULL trong DB | ✅ `GoiThauImportRangeCommand` |

---

## 1. Triệu chứng

```http
POST /QuanLyDuAn/api/import/goi-thau
Content-Type: multipart/form-data

file: Import_GoiThau.xlsx
```

**Response HTTP 200:**

```json
{
  "result": true,
  "errorMessage": "",
  "dataResult": []
}
```

- API **không lỗi** (controller nhận file, gọi `ReadDataFromExcel`, trả `ResultApi.Ok(data)`).
- `dataResult` **rỗng** → không có dòng nào được đọc từ Excel.
- So sánh: `POST /api/import/de-xuat-chu-truong-chuyen-tiep` (có `duAnId`, `buocId`) trả `dataResult` có item bình thường.

---

## 2. Luồng thực tế (trace logic)

```
ImportController.ImportGoiThau()
  → file.OpenReadStream()
  → _excelImporter.ReadDataFromExcel<GoiThauImportDto>(stream)
       → workbook = new Workbook(stream)          ✅ File mở được
       → worksheet = workbook.Worksheets[0]       ✅ Sheet "Tài liệu"
       → tables = FindTableStartPosition(worksheet)
       → foreach (table in tables) { ... }        ❌ tables.Count == 0 → không vào vòng lặp
  → return []                                    → dataResult rỗng
  → Mediator.Send(GoiThauImportRangeCommand([])) → handler không insert gì
  → ResultApi.Ok(data)                           → HTTP 200, result true
```

**Kết luận ngắn:** `ReadDataFromExcel` **chỉ đọc dữ liệu khi sheet có Excel Table (`ListObject`)**. File gói thầu hiện tại **không có Table** → list rỗng, không exception.

---

## 3. Root cause (chính)

| Kiểm tra | DeXuat (đọc được) | GoiThau (rỗng) |
|----------|-------------------|----------------|
| `xl/tables/` trong file .xlsx | ✅ Có `table1.xml` | ❌ **Không có** |
| `worksheet.ListObjects.Count` | ≥ 1 | **0** |
| `FindTableStartPosition()` | Trả vùng A6:F8 | **Trả `[]`** |
| Vòng `foreach (tables)` | Chạy | **Không chạy** |

Đã verify bằng cách giải nén `.xlsx`:

- `QLDA.WebApi/PrintTemplates/Import_DeXuatChuTruongChuyenTiep.xlsx` → có `xl/tables/table1.xml`, tên table `DeXuatChuyenTiepImport`, ref `A6:F8`.
- `QLDA.WebApi/PrintTemplates/Import_GoiThau.xlsx` → **không có thư mục `xl/tables`**.

Template gói thầu được tạo/chỉnh **bên ngoài** (path gốc trong metadata: `C:\Users\tranp\Downloads\`), không tuân contract mà `ReadDataFromExcel` yêu cầu.

---

## 4. Contract của `ReadDataFromExcel<T>` (BuildingBlocks)

**File:** `BuildingBlocks.Infrastructure/Offices/ExcelImporter.cs`

| Quy ước | Chi tiết |
|---------|----------|
| Sheet | `Worksheets[0]` — sheet đầu tiên |
| Phạm vi đọc | **Bắt buộc** có `ListObject` (Excel Table) |
| Tìm table | `AsposeHelper.FindTableStartPosition()` — duyệt `worksheet.ListObjects` |
| Header | Dòng `startRow` trong table |
| Mô tả | Dòng `startRow + 1` (bỏ qua khi đọc) |
| Dữ liệu | Từ `startRow + 2` đến `endRow` |
| Map cột | **Theo index cột**, không theo tên header |
| Map property | `props[colIndex - startColumn]` — thứ tự khai báo property trong class DTO |
| `[Description]` | Chỉ dùng cho message lỗi `[Required]`, **không** dùng match header |
| Exception | `ManagedException` trong vòng lặp dòng → `catch` → `continue` (bỏ dòng đó) |
| Không có table | Trả `List<T>` rỗng, **không throw** |

```csharp
// ExcelImporter.cs — điểm then chốt
var tables = _asposeHelper.FindTableStartPosition(worksheet);

foreach (var (name, startRow, startColumn, endRow, endColumn) in tables)
{
    for (int rowIndex = startRow + 2; rowIndex <= endRow; rowIndex++)
    {
        // map theo index cột → property
    }
}
return records; // tables rỗng → records rỗng
```

---

## 5. Cấu trúc thực tế file `Import_GoiThau.xlsx`

### 5.1 Workbook

| Sheet index | Tên | Vai trò |
|-------------|-----|---------|
| 0 | `Tài liệu` | Sheet người dùng nhập (code đọc sheet này) |
| 1 | `Danh mục` | Combo `$cbo1`…`$cbo5` cho dropdown |

### 5.2 Header (2 dòng + merged cells) — **không** phải format Table chuẩn

| Row Excel | Nội dung |
|-----------|----------|
| 1 | Tiêu đề / layout (merged) |
| 2–3 | Header 2 tầng: `STT`, `Kế hoạch lựa chọn nhà thầu *`, `Nội dung` (merge C2:D2), `Giá gói thầu`, … |
| 3 | Sub-header: `Tên gói thầu *`, `Tóm tắt công việc chính` |
| 4+ | Dữ liệu (STT=1 ở row 4 trong file repo) |

**Merged cells (12 vùng):** `A2:A3`, `B2:B3`, `C2:D2`, `E2:E3`, `F2:F3`, `G2:G3`, …

### 5.3 Header text (sharedStrings) — có dấu `*`, không ảnh hưởng mapping hiện tại

Code **không đọc tên header**. Các khác biệt sau **không phải** nguyên nhân `dataResult: []` (vì chưa tới bước map):

| Cột | Header trong Excel |
|-----|-------------------|
| A | `STT` |
| B | `Kế hoạch lựa chọn nhà thầu *` |
| C | `Tên gói thầu *` |
| D | `Tóm tắt công việc chính` |
| E | `Giá gói thầu` |
| F | `Nguồn vốn*` |
| G–M | Hình thức, Phương thức, Thời gian…, Loại HĐ, … |

### 5.4 Placeholder combo (sheet `Tài liệu`)

Một số ô data row chứa `$cbo1`, `$cbo2`, … (từ lúc tải template qua `GetTemplate`) — đây là giá trị dropdown, không phải header.

### 5.5 Cấu trúc template **sau khi sửa** (format mới — đang dùng)

| Row Excel | Nội dung |
|-----------|----------|
| 1 | `MẪU IMPORT GÓI THẦU` (merged A1:L1) |
| 4 | Header 12 cột (trong Table) |
| 5 | Dòng mô tả / hướng dẫn nhập (skip khi đọc) |
| 6+ | Dữ liệu người dùng |

- **Table:** `GoiThauImport` ref `A4:L6` (mở rộng khi thêm dòng)
- **Không có** cột STT, **không** header merged 2 dòng

---

## 6. `GoiThauImportDto` — map theo index (sau khi có Table)

**File:** `QLDA.Application/GoiThaus/DTOs/GoiThauImportDto.cs`

| Index | Property DTO | Kiểu |
|-------|--------------|------|
| 0 | `TenKeHoachLuaChonNhaThau` | string? |
| 1 | `Ten` | string? |
| 2 | `TomTatCongViecChinhGoiThau` | string? |
| 3 | `GiaTri` | long? |
| 4 | `TenNguonVon` | string? |
| 5 | `TenHinhThucLuaChonNhaThau` | string? |
| 6 | `TenPhuongThucLuaChonNhaThau` | string? |
| 7 | `ThoiGianToChucLuaChonNhaThau` | string? |
| 8 | `ThoiGianBatDauToChucLuaChonNhaThau` | string? |
| 9 | `TenLoaiHopDong` | string? |
| 10 | `ThoiGianThucHienGoiThau` | int? |
| 11 | `TuyChonMuaThem` | string? |

**12 property — không có `Stt`.**

Excel thực tế có **13 cột A–M** (có cột `STT`). Nếu chỉ thêm Table mà không chỉnh cột → **lệch index** (cột A `STT` map vào `TenKeHoachLuaChonNhaThau`).

> Ghi chú: `docs/issues/9579/report.md` mô tả 12 cột A–L **không có STT**. Template hiện tại **có STT** → tài liệu #9579 và file Excel **không khớp**.

---

## 7. So sánh với DeXuat (tại sao API kia đọc được)

| Tiêu chí | DeXuatChuyenTiep | GoiThau |
|----------|------------------|---------|
| Excel Table | ✅ `DeXuatChuyenTiepImport` | ❌ Không có |
| Header | 1 dòng trong table + 1 dòng mô tả | 2 dòng merged ngoài table |
| Số cột vs DTO | 6 cột = 6 property (có `Stt` không lưu DB) | 13 cột vs 12 property |
| Context | `duAnId`/`buocId` qua form | Không cần form |

DeXuat trả data vì **có Table** → vòng lặp chạy. GoiThau rỗng vì **không có Table** — không liên quan `duAnId`/`buocId`.

---

## 8. Các giả thuyết đã loại trừ

| Giả thuyết | Kết quả |
|------------|---------|
| File không mở được | ❌ Loại — không throw, workbook OK |
| Sai sheet index | ❌ Loại — sheet 0 đúng là `Tài liệu` |
| Header `*` không match tên | ❌ Loại — code không match tên header |
| `ManagedException` nuốt hết dòng | ❌ Loại — chưa vào vòng đọc dòng |
| `duAnId`/`buocId` thiếu | ❌ Loại — GoiThau không dùng form context |
| Empty row skip trong handler | ❌ Loại — chưa có row nào từ Excel |

---

## 9. Watch / log nên đặt khi debug trong Visual Studio

**Breakpoint 1:** `ImportController.ImportGoiThau` dòng `ReadDataFromExcel`.

**Breakpoint 2:** `ExcelImporter.ReadDataFromExcel` sau `FindTableStartPosition`:

```
worksheet.Name                    → "Tài liệu"
worksheet.Cells.MaxDataRow        → 4 (0-based) / row 5 Excel
worksheet.Cells.MaxDataColumn     → 12 (cột M)
tables.Count                      → 0  ← xác nhận root cause
```

**Nếu sau khi sửa template có Table:**

```
tables[0].StartRow, EndRow        → vùng table
rowIndex bắt đầu                  → startRow + 2
props.Length                      → 12
(endColumn - startColumn + 1)     → phải = 12 (hoặc 13 nếu giữ STT → cần thêm property)
```

**Per cell:**

```
colIndex, cellValue, prop.Name, convertedValue
```

---

## 10. Hướng xử lý đề xuất

### Phương án A (khuyến nghị): Sửa template Excel bằng Aspose

Giữ nguyên `ReadDataFromExcel` — ít rủi ro regression cho module khác.

**Việc cần làm trên `Import_GoiThau.xlsx`:**

1. Tạo **Excel Table** (`ListObject`) trên sheet `Tài liệu`, ví dụ tên `GoiThauImport`.
2. Header **1 dòng** trong table (gộp 2 dòng header hiện tại thành 1, hoặc flatten merged).
3. (Tuỳ chọn) Dòng mô tả ngay dưới header — code sẽ skip (`startRow + 1`).
4. Data từ `startRow + 2`.
5. **12 cột** khớp thứ tự `GoiThauImportDto` — **bỏ cột STT** hoặc thêm `Stt` vào DTO (chỉ đọc, không lưu).
6. Kéo mở rộng table khi user thêm dòng (giống `Import_DeXuatChuTruongChuyenTiep`).
7. Giữ sheet `Danh mục` + `$cbo1`…`$cbo5` cho `GetTemplate`.

**Cột đề xuất (khớp DTO, không STT):**

| Col | Header table | Property |
|-----|--------------|----------|
| A | Kế hoạch lựa chọn nhà thầu | `TenKeHoachLuaChonNhaThau` |
| B | Tên gói thầu | `Ten` |
| C | Tóm tắt công việc chính | `TomTatCongViecChinhGoiThau` |
| D | Giá gói thầu | `GiaTri` |
| E | Nguồn vốn | `TenNguonVon` |
| F | Hình thức lựa chọn nhà thầu | `TenHinhThucLuaChonNhaThau` |
| G | Phương thức lựa chọn nhà thầu | `TenPhuongThucLuaChonNhaThau` |
| H | Thời gian tổ chức lựa chọn nhà thầu | `ThoiGianToChucLuaChonNhaThau` |
| I | Thời gian bắt đầu tổ chức lựa chọn nhà thầu | `ThoiGianBatDauToChucLuaChonNhaThau` |
| J | Loại hợp đồng | `TenLoaiHopDong` |
| K | Thời gian thực hiện gói thầu | `ThoiGianThucHienGoiThau` |
| L | Tùy chọn mua thêm | `TuyChonMuaThem` |

### Phương án B: Sửa `ReadDataFromExcel` (không khuyến nghị trước)

- Fallback khi không có Table (đọc theo `MaxDataRow` + heuristic header).
- Rủi ro cao: ảnh hưởng BaoCaoTienDo, GoiThau, DeXuat, GoiThau…
- Chỉ cân nhắc nếu bắt buộc hỗ trợ file ngoài không chuẩn.

### Phương án C: Không sửa controller

Controller đúng — vấn đề nằm ở **template / contract Excel**, không phải `ImportGoiThau()`.

---

## 11. Checklist sau khi sửa (tổng hợp)

- [x] `worksheet.ListObjects.Count >= 1` trên file upload
- [x] `ReadDataFromExcel<GoiThauImportDto>` trả ≥ 1 dòng
- [x] `POST /api/import/goi-thau` → `dataResult` có item (Postman)
- [x] `giaTri` lưu DB đúng (vd. `10,000,000` — test lần 3)
- [x] `NguonVonId`, `HinhThucId`, `PhuongThucId`, `LoaiHopDongId` gán sau lookup
- [x] `GET /api/template/import-goi-thau` vẫn tạo dropdown
- [x] Cập nhật `docs/issues/9579/report.md` (mapping 12 cột A–L)
- [x] `GoiThauImportTests` assert `dataResult` không rỗng

---

## 12. Tóm tắt 1 dòng (root cause ban đầu)

> **`dataResult: []` vì `Import_GoiThau.xlsx` không có Excel Table (`ListObject`) — không phải lỗi Postman hay thiếu `duAnId`/`buocId`.**

---

## 13. Đã sửa những gì (tổng hợp)

### 13.1 Template Excel

**File:** `QLDA.WebApi/PrintTemplates/Import_GoiThau.xlsx`

| Thành phần | Format cũ (lỗi) | Format mới (đã áp dụng) |
|------------|-----------------|---------------------------|
| Excel Table | ❌ Không có | ✅ `GoiThauImport` ref `A4:L6` |
| Tiêu đề | Header merged 2 dòng + STT | Hàng 1: `MẪU IMPORT GÓI THẦU` |
| Cột | A–M (13 cột, có STT) | **A–L (12 cột**, khớp DTO) |
| Header table | Hàng 2–3 merged | Hàng 4 header + hàng 5 mô tả |
| Dữ liệu | Từ hàng 4+ | Từ hàng 6 (trong table); thêm dòng → **kéo mở rộng table** |
| Combo | `$cbo1`…`$cbo5` | Giữ: cột A, E, F, G, J |
| Sheet `Danh mục` | Có | Giữ nguyên |

**Lưu ý:** Có thể giữ **giao diện cũ** (STT cột A, header merged) nếu tạo Table vùng **`B2:M`** (12 cột, không STT) — xem mục 10 phương án A. Team đã chọn **format mới từ cột A** (đơn giản hơn).

**Script tái tạo (tuỳ chọn):** `scripts/FixImportGoiThauTemplate/Program.cs`

```powershell
$env:NUGET_PACKAGES = "C:\Users\<user>\.nuget\packages"
dotnet run --project scripts/FixImportGoiThauTemplate/FixImportGoiThauTemplate.csproj
```

### 13.2 BuildingBlocks — parse `long?` từ Excel

**File:** `BuildingBlocks.CrossCutting/ExtensionMethods/StringExtension.cs`

**Vấn đề:** `ConvertStringToPropertyType` không hỗ trợ `long?` → cột `GiaTri` luôn `null` dù Excel có `10000000` hoặc `10.000.000`.

**Sửa:** Thêm block parse `long` / `long?` với culture `vi-VN` + `InvariantCulture`.

**Ảnh hưởng:** Cả import DeXuat (`SoLieuGiaiNgan`…), GoiThau (`GiaTri`), module khác dùng `long?` trong DTO import.

### 13.3 Application — gán ID danh mục khi insert

**File:** `QLDA.Application/GoiThaus/Commands/GoiThauImportRangeCommand.cs`

**Vấn đề:** `TryGetValue` gán vào biến tạm `tmp*` nhưng **không gán** `loaiHopDongId`, `hinhThucId`, `phuongThucId`, `nguonVonId` → DB luôn NULL dù `dataResult` có `tenNguonVon`, `tenHinhThuc`…

**Sửa trước (sai):**
```csharp
int? nguonVonId = null;
if (!string.IsNullOrWhiteSpace(item.TenNguonVon))
    nguonVonDict.TryGetValue(item.TenNguonVon, out var tmpNguonVonId); // không dùng tmp
```

**Sửa sau (đúng):**
```csharp
int? nguonVonId = !string.IsNullOrWhiteSpace(item.TenNguonVon)
    && nguonVonDict.TryGetValue(item.TenNguonVon, out var nguonVonIdVal)
    ? nguonVonIdVal
    : null;
```
(Tương tự `loaiHopDongId`, `hinhThucId`, `phuongThucId`.)

**Lưu ý:** Tên trong Excel phải **khớp chính xác** `Ten` trong bảng danh mục DB (`GetQueryableSet` + `Contains`).

### 13.4 Test

**File:** `QLDA.Tests/Integration/GoiThauImportTests.cs`

- `ImportGoiThau_WithRealTemplateFile_ReturnsOk` — assert `dataResult` array length > 0.

### 13.5 Docs liên quan

- `docs/issues/9579/report.md` — cập nhật bảng mapping 12 cột + combo.

---

## 14. Từng bước xử lý (timeline)

### Bước 1 — Debug & xác định root cause

1. Trace `ImportController` → `ReadDataFromExcel<GoiThauImportDto>`.
2. So sánh file DeXuat (đọc được) vs GoiThau (rỗng).
3. Giải nén `.xlsx`: DeXuat có `xl/tables/`, GoiThau **không có**.
4. Kết luận: thiếu **Excel Table** → `FindTableStartPosition` trả `[]`.

### Bước 2 — Sửa template (lỗi #1)

1. Tạo lại `Import_GoiThau.xlsx` với Aspose (`scripts/FixImportGoiThauTemplate`).
2. Table `GoiThauImport`, 12 cột A–L, header + mô tả + 1 dòng mẫu `$cbo`.
3. Verify: `ReadDataFromExcel` trả ≥ 1 dòng.
4. Test Postman: `dataResult` không còn `[]`.

### Bước 3 — Phát hiện `giaTri: null` (lỗi #2)

1. `dataResult` có string fields nhưng `giaTri: null` dù Excel có số.
2. Trace `ConvertStringToPropertyType` — thiếu handler `long?`.
3. Sửa `StringExtension.cs`, rebuild API.
4. Verify DB: `GiaTri = 10,000,000` (test lần 3).

### Bước 4 — Phát hiện ID danh mục NULL trong DB (lỗi #3)

1. `dataResult` có `tenNguonVon`, `tenHinhThuc`… nhưng DB `NguonVonId`, `HinhThucId`… = NULL.
2. Đọc `GoiThauImportRangeCommand` — lookup OK nhưng không gán biến.
3. Sửa gán `loaiHopDongId`, `hinhThucId`, `phuongThucId`, `nguonVonId`.
4. Rebuild + import lại → ID danh mục có giá trị (nếu tên khớp DB).

### Bước 5 — Thử format cũ (tham khảo, không deploy)

1. User thử giữ header merged + Table `B2:M` — về mặt kỹ thuật **được** nếu Table đúng 12 cột B–M.
2. Team quay lại **format mới A4:L6** (chạy lại script Bước 2).

---

## 15. Files đã thay đổi

| File | Thay đổi |
|------|----------|
| `QLDA.WebApi/PrintTemplates/Import_GoiThau.xlsx` | Template mới + Excel Table |
| `BuildingBlocks.../StringExtension.cs` | Parse `long?` |
| `QLDA.Application/.../GoiThauImportRangeCommand.cs` | Gán ID lookup danh mục |
| `QLDA.Tests/.../GoiThauImportTests.cs` | Assert `dataResult` không rỗng |
| `docs/issues/9579/report.md` | Mapping cột cập nhật |
| `scripts/FixImportGoiThauTemplate/*` | Script tái tạo template (tuỳ chọn) |

**Không sửa:** `ImportController.cs`, `ReadDataFromExcel`, `ExcelImporter.cs`.

---

## 16. Trả lời sếp (1 đoạn)

> Import gói thầu trả 200 nhưng không có dữ liệu vì file Excel template thiếu **Excel Table** — engine chỉ đọc trong Table. Đã sửa template, thêm parse số tiền (`long?`), và fix bug không gán ID danh mục khi insert. Không đổi API.
