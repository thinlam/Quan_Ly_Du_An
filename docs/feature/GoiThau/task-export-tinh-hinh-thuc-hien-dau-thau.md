# Export Excel — Báo cáo tình hình thực hiện đấu thầu (Issue #103)

> **Trạng thái:** ✅ **IMPLEMENTED** (backend) — FE chưa gắn nút  
> **Pattern tham chiếu:** `PrintController.InDanhSachPhanKhaiKinhPhi` (LINQ + `IExporterHelper` + Aspose)  
> **Multi-sheet:** `IExporterHelper.ExportDynamicMultiSheet` (BuildingBlocks)  
> **Hướng dẫn Aspose:** [`QLDA.WebApi/PrintTemplates/huong-dan.md`](../../../QLDA.WebApi/PrintTemplates/huong-dan.md)  
> **Doc export tương tự:** [`task-export-tinh-hinh-de-xuat-nhu-cau.md`](../DeXuatNhuCauKinhPhi/task-export-tinh-hinh-de-xuat-nhu-cau.md)

---

## 📋 Executive Summary

**Tính năng:** User trên màn **Báo cáo tình hình thực hiện đấu thầu** bấm **Kết xuất Excel** → tải file `.xlsx`.

| Khía cạnh | Giá trị |
|-----------|---------|
| Tên nghiệp vụ / UI | Báo cáo tình hình thực hiện đấu thầu |
| FE route | `/quan-ly-du-an/bao-cao/tinh-hinh-lua-thau` |
| Entity nguồn | `GoiThau` (+ join `DuAn`, `DuAnBuoc`, danh mục, `KetQuaTrungThau`, `HopDong`) |
| Controller danh sách | `GoiThauController` |
| API danh sách (đã có) | `GET /api/goi-thau/tinh-hinh-thuc-hien-dau-thau` |
| **API export (đã triển khai)** | `GET /api/print/tinh-hinh-thuc-hien-dau-thau` |
| Query list | `GoiThauGetTinhHinhDauThauQuery` |
| Query export | `GoiThauGetTinhHinhDauThauExportQuery` |
| DTO grid | `GoiThauDto` (ID, FE resolve tên) |
| DTO export | `TinhHinhThucHienDauThauExportDto` (tên hiển thị đầy đủ) |
| Template | `QLDA.WebApi/PrintTemplates/TinhHinhThucHienDauThau.xlsx` |
| Phân quyền | `RoleConstants.GroupTinhHinhThucHienDauThauExport` |
| Integration test | `QLDA.Tests/Integration/TinhHinhThucHienDauThauExportTests.cs` |

> **Không nhầm với:** `GET /api/print/danh-sach-goi-thau` — export danh sách gói thầu trong dự án (stored procedure, filter khác, không có 3 tab trạng thái đấu thầu).

**Migration:** Không cần  
**Stored procedure:** Không (LINQ + Aspose)

---

## ✅ Tóm tắt triển khai (as-built)

### Hành vi export theo `loai`

| `loai` | Kết quả Excel |
|--------|---------------|
| `1` | **1 sheet** — tab *Chưa có kết quả lựa chọn nhà thầu* |
| `2` | **1 sheet** — tab *Có kết quả lựa chọn nhà thầu* |
| `3` | **1 sheet** — tab *Đã lên hợp đồng* |
| `null` / không truyền / `0` | **3 sheet** trong cùng file: `Chưa có kết quả`, `Có kết quả`, `Đã lên hợp đồng` |
| `-1`, `4`, … | **400 Bad Request** — message validate rõ ràng |

Mỗi sheet có **STT riêng** bắt đầu từ 1.

### Khác biệt so với API list

| Khía cạnh | API list | API export |
|-----------|----------|------------|
| Param tab | `trangThai` | `loai` (cùng giá trị 1/2/3) |
| Filter `nam`, `giaiDoanId`, `duAnId` | ✅ Có | ❌ **Không có** — export lấy toàn bộ gói thầu, chỉ lọc theo tab (`loai`) |
| Phân trang | ✅ | ❌ — full data |
| Multi-tab | 1 tab / request | Có thể 3 sheet / request (khi không truyền `loai`) |

> **Lý do:** Export báo cáo tổng hợp theo tab, không đồng bộ filter màn hình list. Nếu sau này BA yêu cầu khớp filter grid, thêm lại params và logic tương ứng.

### Bug đã sửa trong quá trình implement

| Bug | Nguyên nhân | Fix |
|-----|-------------|-----|
| Cột **Bước** trống | Join sai: `DuAnBuoc.DuAnId + DuAnBuoc.BuocId` | `GoiThau.BuocId` = **`DuAnBuoc.Id`** (PK bước dự án), không phải `DanhMucBuoc.BuocId` |
| 400 khi export 3 sheet | `Task.WhenAll` chạy 3 query song song trên cùng `DbContext` | Gọi **tuần tự** 3 lần `Mediator.Send` trong `foreach` |

---

## 🔍 Phase 0 — Khảo sát source (list API)

### API danh sách hiện tại

```http
GET /api/goi-thau/tinh-hinh-thuc-hien-dau-thau?pageIndex=1&pageSize=10&trangThai=1&nam=2026&giaiDoanId=&duAnId=
```

| Thành phần | File |
|------------|------|
| Controller | `QLDA.WebApi/Controllers/GoiThauController.cs` → `GetTinhHinhThucHienDauThau` |
| Search DTO | `QLDA.Application/GoiThaus/DTOs/TinhHinhDauThauSearchDto.cs` |
| Query + Handler | `QLDA.Application/GoiThaus/Queries/GoiThauGetTinhHinhDauThauQuery.cs` |
| Response DTO | `GoiThauDto` — trả ID, FE resolve tên |

### Mapping tab ↔ `trangThai` ↔ `loai`

| Giá trị | Tab UI | Điều kiện EF |
|---------|--------|--------------|
| `1` | Chưa có kết quả lựa chọn nhà thầu | `KetQuaTrungThau == null && HopDong == null` |
| `2` | Có kết quả lựa chọn nhà thầu | `KetQuaTrungThau != null && HopDong == null` |
| `3` | Đã lên hợp đồng | `KetQuaTrungThau != null && HopDong != null` |

Export handler dùng cùng `switch` trên `Loai`. List handler dùng `TrangThai` với logic tương đương.

### Điểm lưu ý từ handler list

| # | Quan sát | Export |
|---|----------|--------|
| 1 | Không gọi `_authManager.FilterVisible()` | Giữ nguyên — duplicate behavior list |
| 2 | Không filter `DaDuyet` | Giữ nguyên |
| 3 | `.Include(KetQuaTrungThau, HopDong)` cho tab filter | ✅ Có |
| 4 | `TrangThai` ngoài 1–3 → không lọc tab | Export **validate** `loai ∈ {0,1,2,3}` hoặc null |

---

## 🏗️ Luồng xử lý (as-built)

```
Màn "Báo cáo tình hình thực hiện đấu thầu"
├── GET /api/goi-thau/tinh-hinh-thuc-hien-dau-thau?trangThai={tab}&nam=...  → Grid (phân trang + filter)
└── GET /api/print/tinh-hinh-thuc-hien-dau-thau?loai={tab}                   → Export Excel (chỉ lọc tab)

PrintController.InTinhHinhThucHienDauThau
├── loai = null | 0
│   ├── foreach tab (1,2,3): Mediator.Send(GoiThauGetTinhHinhDauThauExportQuery { Loai = tab })
│   └── ExportDynamicMultiSheet → 3 sheet
└── loai = 1 | 2 | 3
    ├── Mediator.Send(GoiThauGetTinhHinhDauThauExportQuery { Loai })
    └── Export (AsposeInstruction) → 1 sheet

GoiThauGetTinhHinhDauThauExportQuery
├── Filter tab: Loai 1/2/3 (switch KetQuaTrungThau / HopDong)
├── Join display: DuAn, DuAnBuoc (Id == BuocId), NguonVon, HinhThuc, PhuongThuc, LoaiHopDong
└── Không: TepDinhKem, cột thao tác, filter nam/giaiDoan/duAn
```

---

## 📂 Files đã tạo / sửa

### Tạo mới ✅

| File | Mô tả |
|------|-------|
| `QLDA.Application/GoiThaus/DTOs/TinhHinhThucHienDauThauExportDto.cs` | 10 property khớp placeholder template |
| `QLDA.Application/GoiThaus/Queries/GoiThauGetTinhHinhDauThauExportQuery.cs` | Query + handler export |
| `QLDA.WebApi/Models/GoiThaus/TinhHinhThucHienDauThauPrintSearchModel.cs` | `Loai`, `HiddenColumns` |
| `QLDA.WebApi/PrintTemplates/TinhHinhThucHienDauThau.xlsx` | Template Aspose |
| `QLDA.Gen/Descriptors/TinhHinhThucHienDauThauExportDescriptor.cs` | Descriptor codegen |
| `QLDA.Tests/Integration/TinhHinhThucHienDauThauExportTests.cs` | Smoke test export |

### Sửa ✅

| File | Thay đổi |
|------|----------|
| `QLDA.Domain/Constants/RoleConstants.cs` | `GroupTinhHinhThucHienDauThauExport` |
| `QLDA.WebApi/Controllers/PrintController.cs` | Region `TinhHinhThucHienDauThau` + endpoint + multi-sheet |
| `QLDA.Gen/Program.cs` | Slug `tinh-hinh-thuc-hien-dau-thau` |

### Không sửa (theo thiết kế)

| File | Lý do |
|------|-------|
| `GoiThauController.cs` | Export ở `PrintController` |
| `GoiThauGetTinhHinhDauThauQuery.cs` | List giữ nguyên |
| Migration / `AppDbContextModelSnapshot` | Không đổi DB |

---

## 🌐 API Export (spec as-built)

```http
# 1 sheet — tab đang chọn
GET /api/print/tinh-hinh-thuc-hien-dau-thau?loai=2
Authorization: Bearer {token}

# 3 sheet — cả 3 tab
GET /api/print/tinh-hinh-thuc-hien-dau-thau
```

| Query param | Kiểu | Bắt buộc | Mô tả |
|-------------|------|----------|-------|
| `loai` | `int?` | ❌ | `1` / `2` / `3` = 1 sheet theo tab; **null / 0 / bỏ trống** = 3 sheet |
| `hiddenColumns` | `string[]` | ❌ | Ẩn cột (convention PrintController) |

**Validation:**

```csharp
ManagedException.ThrowIf(searchModel.Loai is int loai && loai is not (0 or 1 or 2 or 3),
    "Loại tab không hợp lệ. Chỉ chấp nhận giá trị 1 (Chưa có kết quả), 2 (Có kết quả), 3 (Đã lên hợp đồng), hoặc bỏ trống để xuất cả 3 tab.");
```

**Response:** `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`  
**Tên file download:** `TinhHinhThucHienDauThau_ddMMyyyy_HHmmss.xlsx` (`GetDownloadFileName`)

**Dữ liệu rỗng:** Vẫn trả file Excel (header + 0 dòng). Không throw.

**Tên sheet (multi-export):**

| `Loai` | Tên sheet Excel |
|--------|-----------------|
| 1 | `Chưa có kết quả` |
| 2 | `Có kết quả` |
| 3 | `Đã lên hợp đồng` |

---

## 📊 Cột Excel (10 cột)

| # | Header | Property DTO | Nguồn |
|---|--------|--------------|-------|
| 1 | STT | `Stt` | `index + 1` (mỗi sheet đếm lại từ 1) |
| 2 | Dự án | `TenDuAn` | `GoiThau.DuAn.TenDuAn` |
| 3 | Bước | `TenBuoc` | `DuAnBuoc.TenBuoc ?? Buoc.Ten` qua `DuAnBuoc.Id == GoiThau.BuocId` |
| 4 | Tên gói thầu | `TenGoiThau` | `GoiThau.Ten` |
| 5 | Giá gói thầu | `GiaGoiThau` | `GoiThau.GiaTri` — format `#,##0` |
| 6 | Nguồn vốn | `TenNguonVon` | `NguonVon.Ten` |
| 7 | Hình thức lựa chọn nhà thầu | `TenHinhThucLuaChonNhaThau` | `HinhThucLuaChonNhaThau.Ten` |
| 8 | Phương thức lựa chọn nhà thầu | `TenPhuongThucLuaChonNhaThau` | `PhuongThucLuaChonNhaThau.Ten` |
| 9 | Thời gian tổ chức lựa chọn nhà thầu | `ThoiGianToChucLuaChonNhaThau` | `GoiThau.ThoiGianLuaNhaThau` |
| 10 | Loại hợp đồng | `TenLoaiHopDong` | `LoaiHopDong.Ten` |

**Không export:** cột thao tác, tệp đính kèm (`DanhSachTepDinhKem`).

Placeholder template: `$Stt`, `$TenDuAn`, `$TenBuoc`, … (PascalCase).

### Join `TenBuoc` (đã fix)

```csharp
// GoiThau.BuocId = DuAnBuoc.Id (PK), KHÔNG phải DanhMucBuoc.BuocId
TenBuoc = e.DuAnBuoc != null
    ? (e.DuAnBuoc.TenBuoc ?? (e.DuAnBuoc.Buoc != null ? e.DuAnBuoc.Buoc.Ten : null))
    : duAnBuocQuery
        .Where(dab => dab.Id == e.BuocId)
        .Select(dab => dab.TenBuoc ?? (dab.Buoc != null ? dab.Buoc.Ten : null))
        .FirstOrDefault()
```

---

## 🧩 Code tham chiếu (as-built)

### PrintSearchModel

```csharp
public record TinhHinhThucHienDauThauPrintSearchModel
{
    /// Tab: 1 / 2 / 3. Null hoặc 0 = xuất 3 sheet (mỗi tab một sheet).
    public int? Loai { get; set; }
    public List<string>? HiddenColumns { get; set; }
}
```

### PrintController — multi-sheet vs single-sheet

```csharp
private static readonly (int Loai, string SheetTitle)[] TinhHinhThucHienDauThauSheetTabs =
[
    (1, "Chưa có kết quả"),
    (2, "Có kết quả"),
    (3, "Đã lên hợp đồng"),
];

if (searchModel.Loai is null or 0)
{
    var sheets = new List<SheetInstruction>(TinhHinhThucHienDauThauSheetTabs.Length);
    foreach (var tab in TinhHinhThucHienDauThauSheetTabs)
    {
        var rows = await Mediator.Send(
            new GoiThauGetTinhHinhDauThauExportQuery { Loai = tab.Loai }, cancellationToken);
        sheets.Add(new SheetInstruction
        {
            Title = tab.SheetTitle,
            Items = ExporterHelper.ConvertToDictionaryList(rows),
            HiddenColumns = hiddenColumns,
        });
    }
    exportResult = _excelExporter.ExportDynamicMultiSheet(new DynamicMultiSheetInstruction
    {
        TemplatePath = templatePath,
        Sheets = sheets,
    });
}
else
{
    var data = await Mediator.Send(
        new GoiThauGetTinhHinhDauThauExportQuery { Loai = searchModel.Loai }, cancellationToken);
    exportResult = _excelExporter.Export(new AsposeInstruction<TinhHinhThucHienDauThauExportDto> { ... });
}
```

> **Lưu ý:** Không dùng `Task.WhenAll` cho 3 query — EF `DbContext` scoped không thread-safe.

### RoleConstants

```csharp
public const string GroupTinhHinhThucHienDauThauExport =
    $"{QLDA_TatCa},{QLDA_QuanTri},{QLDA_LDDV},{QLDA_ChuyenVien}";
```

---

## 📐 Template QLDA.Gen

| Thành phần | Giá trị |
|------------|---------|
| Slug | `tinh-hinh-thuc-hien-dau-thau` |
| Descriptor | `TinhHinhThucHienDauThauExportDescriptor` |
| Layout | `LetterheadExport` |
| Title | `BÁO CÁO TÌNH HÌNH THỰC HIỆN ĐẤU THẦU` |
| Output | `QLDA.WebApi/PrintTemplates/TinhHinhThucHienDauThau.xlsx` |

**Regenerate template:**

```bash
cd QLDA.Gen
dotnet run -- tinh-hinh-thuc-hien-dau-thau --force e:\SER\QLDA.WebApi\PrintTemplates
```

---

## 🔌 Tích hợp FE (còn lại)

```typescript
// Export tab đang chọn → 1 sheet
const params = new URLSearchParams({ loai: String(activeTab) }); // 1 | 2 | 3
window.open(`/api/print/tinh-hinh-thuc-hien-dau-thau?${params}`, '_blank');

// Export cả 3 tab → 3 sheet (không truyền loai)
window.open('/api/print/tinh-hinh-thuc-hien-dau-thau', '_blank');
```

| Tab FE | `loai` (export) | `trangThai` (list) |
|--------|-----------------|---------------------|
| Chưa có kết quả lựa chọn nhà thầu | `1` | `1` |
| Có kết quả lựa chọn nhà thầu | `2` | `2` |
| Đã lên hợp đồng | `3` | `3` |

**Gợi ý UX:**

- Nút export trên tab → truyền `loai` = tab hiện tại.
- Nút export tổng hợp (nếu có) → không truyền `loai`.
- List API vẫn dùng `nam`, `giaiDoanId`, `duAnId` — export **không** đồng bộ các filter này (theo thiết kế hiện tại).

---

## ✅ Acceptance Criteria (Issue #103)

| # | Tiêu chí | Trạng thái | Cách verify |
|---|----------|------------|-------------|
| 1 | Export tab 1 đúng "Chưa có kết quả" | ✅ | `loai=1` — so sánh số dòng với list `trangThai=1` (không filter nam) |
| 2 | Export tab 2 đúng "Có kết quả" | ✅ | `loai=2` |
| 3 | Export tab 3 đúng "Đã lên hợp đồng" | ✅ | `loai=3` |
| 4 | Đủ 10 cột nghiệp vụ, không cột thao tác / tệp | ✅ | Mở file Excel |
| 5 | Cột Bước có tên hiển thị | ✅ | Join `DuAnBuoc.Id == GoiThau.BuocId` |
| 6 | Không truyền `loai` → 3 sheet | ✅ | Gọi API không param — kiểm tra 3 tab sheet |
| 7 | `loai` ngoài 0–3 (trừ null) → lỗi | ✅ | `loai=4`, `loai=-1` → 400 |
| 8 | `loai=0` → 3 sheet (giống null) | ✅ | Gọi `?loai=0` |
| 9 | Không sửa migration | ✅ | Review diff |
| 10 | FE gắn nút export | ⏳ | Chưa làm |

---

## ✅ Validation Checklist

### Code Quality

- [x] Export DTO compile
- [x] Export query compile
- [x] QLDA.Gen descriptor + slug
- [x] Template trong `PrintTemplates/`
- [x] PrintController endpoint + `[Authorize]`
- [x] `dotnet build` solution pass

### Chức năng

- [x] Lọc tab export khớp logic list (`KetQuaTrungThau` / `HopDong`)
- [x] STT 1, 2, 3… (per sheet)
- [x] Tên danh mục hiển thị (không export raw ID)
- [x] Format giá gói thầu `#,##0`
- [x] Validate `loai` — null/0/1/2/3 hợp lệ; khác → 400
- [x] Multi-sheet khi không chọn loại
- [x] Không sửa migration
- [x] Integration test smoke: `TinhHinhThucHienDauThauExportTests`

### Test cases

```bash
dotnet test QLDA.Tests --filter TinhHinhThucHienDauThauExportTests
```

| Test | Mô tả |
|------|-------|
| `ValidLoai_ReturnsExcel` (1,2,3) | 200 + content-type xlsx |
| `NoLoai_ReturnsExcel` | 200 — 3 sheet |
| `InvalidLoai_ReturnsBadRequest` (4, -1) | 400 |

---

## 📊 Effort

| Phase | Task | Trạng thái |
|-------|------|------------|
| 0 | Phân tích source | ✅ Done |
| 1 | RoleConstants | ✅ Done |
| 2 | QLDA.Gen + template | ✅ Done |
| 3 | Export DTO + Query | ✅ Done |
| 4 | PrintSearchModel + PrintController | ✅ Done |
| 5 | Multi-sheet + fix TenBuoc + fix DbContext | ✅ Done |
| 6 | Integration test | ✅ Done |
| 7 | FE gắn nút + verify staging | ⏳ Pending |

---

## 📞 Common Issues

| Vấn đề | Nguyên nhân | Giải pháp |
|--------|-------------|-----------|
| Cột Bước trống | Join sai `DuAnBuoc.BuocId` | Dùng `DuAnBuoc.Id == GoiThau.BuocId` — **đã fix** |
| 400 "Lỗi hệ thống" khi export 3 sheet | Query song song trên 1 DbContext | Gọi tuần tự — **đã fix** |
| Số dòng export ≠ grid có filter nam | Export không có filter `nam` | Cố ý — export full theo tab; list vẫn filter riêng |
| 403 Forbidden | Thiếu role | `GroupTinhHinhThucHienDauThauExport` |
| Template not found | File chưa deploy | Regenerate + copy `PrintTemplates/` |
| Nhầm API | Gọi `danh-sach-goi-thau` | Dùng `tinh-hinh-thuc-hien-dau-thau` |

---

## ❓ Open Questions

| # | Câu hỏi | Trạng thái |
|---|---------|------------|
| 1 | Export có cần đồng bộ filter `nam` / `giaiDoanId` / `duAnId` với grid? | **Đã quyết:** Không — API export chỉ còn `loai` |
| 2 | Phân quyền export có chặt hơn export khác? | Dùng `GroupTinhHinhThucHienDauThauExport` = TatCa + QuanTri + LDDV + ChuyenVien |
| 3 | Sort export theo cột UI? | Chưa — dùng `GetOrderedSet()` entity |
| 4 | Auth `FilterVisible` cho list + export? | Chưa — giữ behavior list hiện tại |
| 5 | FE: export tab vs export tổng 3 sheet? | Cần BA/FE xác nhận nút nào gọi API nào |

---

## 🔗 Files tham chiếu

| File | Vai trò |
|------|---------|
| `GoiThauController.cs` | API list |
| `GoiThauGetTinhHinhDauThauQuery.cs` | Logic lọc 3 tab (list) |
| `GoiThauGetTinhHinhDauThauExportQuery.cs` | Logic export + join tên |
| `TinhHinhThucHienDauThauPrintSearchModel.cs` | Query params export |
| `PrintController.cs` → `InTinhHinhThucHienDauThau` | Endpoint + multi-sheet |
| `TinhHinhThucHienDauThauExportDto.cs` | DTO export |
| `TinhHinhThucHienDauThauExportDescriptor.cs` | QLDA.Gen columns |
| `TinhHinhThucHienDauThauExportTests.cs` | Integration tests |
| `BuildingBlocks.../DynamicMultiSheetInstruction.cs` | Multi-sheet export |
| `PrintController.InDanhSachPhanKhaiKinhPhi` | Pattern single-sheet export |
