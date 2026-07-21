# Design: Export Excel Quản lý phê duyệt — template-driven

**Ngày:** 2026-07-14  
**Cập nhật:** 2026-07-21  
**Module:** `QuanLyPheDuyet`  
**Scope:** Layout/format Excel do **file template** quyết định; code chỉ cung cấp dữ liệu map `$Placeholder`.  
**Trạng thái:** ✅ **IMPLEMENTED** (Approach A + cột `$TepDinhKem`)

**Tài liệu liên quan:** [task-export-excel-quan-ly-phe-duyet.md](../../feature/QuanLyPheDuyet/task-export-excel-quan-ly-phe-duyet.md) (v1.4)

---

## 1. Hiện trạng (đã khảo sát source)

### 1.1 Runtime export **đã** template-driven

Luồng API:

```
GET /api/print/danh-sach-quan-ly-phe-duyet
  → PrintController.InDanhSachQuanLyPheDuyet
  → PheDuyetGetDanhSachQuery (PageSize=0, IncludeAttachments=true)
  → PheDuyetQueryableExtensions.ApplyDanhSachFilters + AttachTepDinhKem
  → PheDuyetExportMappings.ToExportDtos
  → ExporterHelper.Export(AsposeInstruction<PheDuyetExportDto>)
```

> Không còn `PheDuyetGetDanhSachExportQuery` — export tái sử dụng cùng query với API danh sách.

`ExporterHelper.Export` (`BuildingBlocks/.../ExcelHelper.cs`):

1. `ExtractTemplateBinding(worksheet)` — quét **dòng đầu tiên** có ô bắt đầu bằng `$`, lấy map `{ colIndex → PropertyName }`.
2. Với mỗi item: `InsertRow` + `CopyRow` từ dòng template → **giữ style** (align, font, border, wrap, number format).
3. `PutValueSmart(cell, value)` — **chỉ ghi giá trị**, không ghi đè style.
4. Xóa dòng placeholder gốc; `AutoFitColumnsAndRows = false` trên endpoint này → không đổi width template.

Kết luận: đổi thứ tự cột / align / width trong `DanhSachQuanLyPheDuyet.xlsx` **không cần sửa service** — engine hiện có đã đúng.

### 1.2 Chỗ đang “hard-code layout”

| Thành phần | Vai trò thật | Có quyết định layout lúc export? |
|------------|--------------|----------------------------------|
| `DanhSachQuanLyPheDuyetExportDescriptor.Columns` | Catalog field cho Gen bootstrap | **Không** lúc runtime; **Có** nếu chạy Gen `--force` (bị chặn) |
| `PheDuyetExportDto` | Dữ liệu bind `$Name` | Không (chỉ tên property) |
| `PheDuyetExportMappings` | Map list DTO → export DTO | Không (chỉ dữ liệu) |
| `PrintTemplates/DanhSachQuanLyPheDuyet.xlsx` | Header, width, align, `$Stt`… | **Có** — SOT |

**Đã triển khai:** `HandMaintainedTemplate => true` trên descriptor — Gen skip (kể cả `--force`) khi file template đã tồn tại.

### 1.3 Dữ liệu bind (`PheDuyetExportDto`)

| Cột (A→H) | Property | Placeholder | Nguồn |
|-----------|----------|-------------|-------|
| STT | `Stt` | `$Stt` | Index + 1 |
| Dự án | `TenDuAn` | `$TenDuAn` | `PheDuyetListItemDto` |
| Giai đoạn | `TenGiaiDoan` | `$TenGiaiDoan` | `PheDuyetListItemDto` |
| Tên bước | `TenBuoc` | `$TenBuoc` | `PheDuyetListItemDto` |
| Người trình | `NguoiTrinh` | `$NguoiTrinh` | Resolve `UserMaster` |
| Người duyệt | `NguoiDuyet` | `$NguoiDuyet` | Resolve `UserMaster` |
| Trạng thái | `TenTrangThai` | `$TenTrangThai` | `PheDuyetListItemDto` |
| Tệp đính kèm | `TepDinhKem` | `$TepDinhKem` | `FormatTepDinhKem(DanhSachTepDinhKem)` |

Template row layout: header **R4**, placeholder **R5**. Merge letterhead `A1:D2`, `E1:H2`, title `A3:H3`.

**Cột `TepDinhKem` (thêm 21/07/2026):**

- Kiểu: `string?` — không phải collection trong export DTO.
- Nhiều file → ghép `OriginalName` (fallback `FileName`) bằng `Environment.NewLine`.
- Cột H trên template bật **wrap text** để hiển thị nhiều dòng.
- Export bắt buộc `IncludeAttachments = true` khi gọi `PheDuyetGetDanhSachQuery`.

> Ví dụ minh họa cơ chế (`$LoaiDeXuat`, `$PhongBanPhuTrach`) **không** nằm trong phạm vi hiện tại trừ khi nghiệp vụ yêu cầu riêng.

### 1.4 Tệp đính kèm — tách khỏi EF query (list + export)

**Vấn đề đã xử lý:** Subquery `TepDinhKem` trong EF `Select` làm lệch số dòng list vs export; đồng thời regression `danhSachTepDinhKem: null`.

**Pattern hiện tại (`PheDuyetQueryableExtensions`):**

```text
EF Select PheDuyetListItemDto (không embed file)
  → ToList()
  → if includeAttachments: AttachTepDinhKem (batch in-memory)
     else: EnsureEmptyAttachments → []
```

**Liên kết:** `Attachment.GroupId == PheDuyet.EntityId.ToString()` (case-insensitive).

**Contract API list:** `DanhSachTepDinhKem` luôn là array (`[]` hoặc có phần tử), không `null`. Default `IncludeAttachments = true`.

---

## 2. Mục tiêu

1. Template Excel là **nguồn duy nhất** cho thứ tự cột, header text, width, align, font, border, wrap, merge, format số/ngày.
2. Code chỉ map property → `$Placeholder` (giữ `ExporterHelper` hiện có).
3. Descriptor **không** còn được coi / dùng như cấu hình layout runtime; tránh regenerate ghi đè layout đã chỉnh tay.
4. Không sửa migration; không đổi logic lọc/join/resolve tên người; không đụng export khác; build pass.
5. *(Bổ sung 21/07)* Export đủ cột nghiệp vụ incl. tệp đính kèm; đồng bộ dữ liệu file với API danh sách.

---

## 3. Các hướng tiếp cận

### A — Tái khẳng định SOT + dọn descriptor ✅ **ĐÃ CHỌN & TRIỂN KHAI**

- **Không** đổi `ExporterHelper` / `AsposeHelper`.
- Descriptor: `Columns` chỉ là **catalog field** cho Gen bootstrap.
- Bỏ `Width` / `ColumnAlign` khỏi khai báo descriptor.
- `HandMaintainedTemplate => true` — Gen từ chối ghi đè template đã chỉnh.
- Docs feature + checklist AC chỉnh tay trên `.xlsx`.

### B — Xóa hoàn toàn Columns khỏi descriptor này

- Tách interface / không generate template.

| Ưu | Nhược |
|----|-------|
| Không còn danh sách cột trong code | Phá contract Gen chung |

### C — Sửa lại engine export riêng cho PheDuyet

| Ưu | Nhược |
|----|-------|
| — | Trùng engine hiện có |

---

## 4. Thiết kế chi tiết (Approach A — as-built)

### 4.1 Phân tách trách nhiệm

```text
┌─────────────────────────────────────────────────────────┐
│  PrintTemplates/DanhSachQuanLyPheDuyet.xlsx              │
│  SOT: cột A–H, header, width, align, style, $Placeholder│
└───────────────────────────┬─────────────────────────────┘
                            │ ExtractTemplateBinding
                            ▼
┌─────────────────────────────────────────────────────────┐
│  ExporterHelper.Export + PutValueSmart + CopyRow        │
│  Chỉ fill value theo tên property                       │
└───────────────────────────┬─────────────────────────────┘
                            ▲
┌───────────────────────────┴─────────────────────────────┐
│  PheDuyetGetDanhSachQuery (PageSize=0)                  │
│  → PheDuyetExportMappings.ToExportDtos                  │
│  Chỉ dữ liệu (không layout)                             │
└─────────────────────────────────────────────────────────┘

QLDA.Gen descriptor:
  - HandMaintainedTemplate = true → không overwrite template
  - Columns = catalog tên field; KHÔNG phải layout runtime
```

### 4.2 Thay đổi file (đã thực hiện)

| File | Thay đổi |
|------|----------|
| `QLDA.Gen/Descriptors/IExportDescriptor.cs` | `HandMaintainedTemplate` (default `false`) |
| `QLDA.Gen/Descriptors/DanhSachQuanLyPheDuyetExportDescriptor.cs` | `HandMaintainedTemplate => true`; catalog incl. `TepDinhKem`; bỏ Width/Align |
| `QLDA.Gen/Generators/TemplateGenerator.cs` | Skip khi `HandMaintainedTemplate` và file tồn tại |
| `QLDA.Application/.../PheDuyetExportDto.cs` | + `TepDinhKem` |
| `QLDA.Application/.../PheDuyetExportMappings.cs` | `ToExportDtos` + `FormatTepDinhKem` |
| `QLDA.Application/.../PheDuyetQueryableExtensions.cs` | `AttachTepDinhKem` in-memory; `EnsureEmptyAttachments` |
| `QLDA.Application/.../PheDuyetListItemDto.cs` | `DanhSachTepDinhKem` non-null default `[]` |
| `QLDA.WebApi/PrintTemplates/DanhSachQuanLyPheDuyet.xlsx` | 8 cột incl. `$TepDinhKem` (hand-maintained) |
| `QLDA.WebApi/Controllers/PrintController.cs` | Export `IncludeAttachments = true` |
| `docs/feature/QuanLyPheDuyet/task-export-excel-quan-ly-phe-duyet.md` | v1.4 |
| `QLDA.Tests/...` | Integration + unit tests attachment/export map |

**Không đổi:** `ExporterHelper` signature/behavior; migration; filter/join logic.

### 4.3 Quy tắc vận hành template

| Muốn làm | Làm ở đâu |
|----------|-----------|
| Đổi thứ tự cột | Sửa header row (R4) + dòng `$Field` (R5) trong `.xlsx` |
| Đổi align / width / font / border | Format ô/cột trong `.xlsx` |
| Ẩn cột có sẵn trong DTO | Xóa cột khỏi template (không cần sửa code) |
| Thêm cột dữ liệu mới | Property DTO + map `PheDuyetExportMappings` + `$NewField` trên template |
| Regenerate từ Gen | **Cấm** — `HandMaintainedTemplate` chặn overwrite |

### 4.4 Giữ nguyên style khi nhiều dòng

Đã có sẵn:

- `CopyRow(templateRow → newRow)` trước khi `PutValueSmart`
- Không gọi AutoFit trên endpoint này
- Cột `TepDinhKem`: wrap text trên template để nhiều tên file xuống dòng

Không thêm logic ghi style sau khi fill.

### 4.5 Error / edge

| Case | Hành vi |
|------|---------|
| Template thiếu `$` | Exception `"Khong tim thay cau hinh table"` |
| Có `$Field` nhưng DTO không có property | Ô trống |
| Có property nhưng template không có `$` | Property bị bỏ qua |
| 0 dòng data | HTTP 400 `"Không có dữ liệu để xuất"` |
| Không có file đính kèm | `TepDinhKem` = `""`; API list `danhSachTepDinhKem: []` |
| Nhiều file | `TepDinhKem` = tên ghép `\n`; list = array nhiều phần tử |

### 4.6 Testing

1. **Không regression API khác:** không sửa `ExporterHelper`.
2. **Integration:** `PheDuyetExportTests` — smoke, parity số dòng, attachment contract.
3. **Unit:** `PheDuyetExportMappingsTests`, `PheDuyetQueryableExtensionsAttachmentTests`.
4. **Manual AC:** đổi thứ tự cột / align trên template → export phản ánh đúng.
5. **Build:** `dotnet build` + test projects trên.

### 4.7 Ngoài scope (giữ nguyên)

- Migration / snapshot
- Đổi filter / join / resolve tên người (trừ bugfix)
- Thêm cột ví dụ minh họa (`LoaiDeXuat`, `PhongBanPhuTrach`) chưa có nghiệp vụ
- Refactor toàn bộ descriptor export khác sang `HandMaintainedTemplate`
- Tạo export engine mới

---

## 5. Acceptance Criteria (map)

| AC | Trạng thái |
|----|------------|
| Đổi thứ tự cột chỉ sửa template | ✅ Runtime đọc cột từ template; Gen chặn overwrite |
| Đổi align / width chỉ sửa template | ✅ CopyRow + PutValueSmart + AutoFit=false |
| Không sửa service khi di chuyển cột | ✅ PrintController / Query không hard-code layout |
| Không hard-code width/align trong descriptor | ✅ Catalog chỉ name + header |
| Placeholder map theo tên property | ✅ `ExtractTemplateBinding` + DTO |
| Nhiều dòng giữ style dòng mẫu | ✅ `CopyRow` |
| Cột tệp đính kèm xuất đúng tên file | ✅ `$TepDinhKem` + `FormatTepDinhKem` |
| API list `danhSachTepDinhKem` không null | ✅ `EnsureEmptyAttachments` + DTO default |
| Build OK / không ảnh hưởng export khác | ✅ |

---

## 6. Self-review checklist

- [x] Không placeholder TBD trong quyết định chính
- [x] Root cause đúng (Gen dual-SOT + EF subquery file)
- [x] Scope hẹp: descriptor + Gen guard + mappings + template + docs
- [x] Ví dụ `$LoaiDeXuat` làm minh họa, không nhầm requirement
- [x] Approach A implemented
- [x] Cột `TepDinhKem` documented end-to-end

---

## 7. Changelog

| Version | Ngày | Nội dung |
|---------|------|----------|
| **1.0** | 14/07/2026 | Design ban đầu — Approach A, template SOT |
| **1.1** | 21/07/2026 | As-built: `HandMaintainedTemplate` implemented; + cột `$TepDinhKem`; gộp export query; attachment in-memory pattern; cập nhật AC + file list |

---

**Trạng thái design:** ✅ Implemented (Approach A).  
**Task doc:** [task-export-excel-quan-ly-phe-duyet.md](../../feature/QuanLyPheDuyet/task-export-excel-quan-ly-phe-duyet.md) v1.4  
**Plan chi tiết bước code:** `docs/superpowers/plans/2026-07-14-export-template-driven-quan-ly-phe-duyet.md`
