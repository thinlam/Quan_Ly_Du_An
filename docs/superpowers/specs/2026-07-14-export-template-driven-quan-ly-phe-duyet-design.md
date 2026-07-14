# Design: Export Excel Quản lý phê duyệt — template-driven

**Ngày:** 2026-07-14  
**Module:** `QuanLyPheDuyet`  
**Scope:** Refactor để layout/format Excel do **file template** quyết định; code chỉ cung cấp dữ liệu map `$Placeholder`.  
**Không commit trong phiên docs** — người implement tự commit khi sẵn sàng.

---

## 1. Hiện trạng (đã khảo sát source)

### 1.1 Runtime export **đã** template-driven

Luồng API:

```
GET /api/print/danh-sach-quan-ly-phe-duyet
  → PrintController.InDanhSachQuanLyPheDuyet
  → PheDuyetGetDanhSachQuery (PageSize=0)
  → PheDuyetExportMappings.ToExportDtos
  → ExporterHelper.Export(AsposeInstruction<PheDuyetExportDto>)
```

`ExporterHelper.Export` (`BuildingBlocks/.../ExcelHelper.cs`):

1. `ExtractTemplateBinding(worksheet)` — quét **dòng đầu tiên** có ô bắt đầu bằng `$`, lấy map `{ colIndex → PropertyName }`.
2. Với mỗi item: `InsertRow` + `CopyRow` từ dòng template → **giữ style** (align, font, border, wrap, number format).
3. `PutValueSmart(cell, value)` — **chỉ ghi giá trị**, không ghi đè style.
4. Xóa dòng placeholder gốc; `AutoFitColumnsAndRows = false` trên endpoint này → không đổi width template.

Kết luận: đổi thứ tự cột / align / width trong `DanhSachQuanLyPheDuyet.xlsx` **không cần sửa service** đã hoạt động đúng với engine hiện có.

### 1.2 Chỗ đang “hard-code layout”

| Thành phần | Vai trò thật | Có quyết định layout lúc export? |
|------------|--------------|----------------------------------|
| `DanhSachQuanLyPheDuyetExportDescriptor.Columns` | Input cho `QLDA.Gen` **sinh/ghi đè** file `.xlsx` | **Không** lúc runtime; **Có** nếu chạy Gen `--force` |
| `PheDuyetExportDto` | Dữ liệu bind `$Name` | Không (chỉ tên property) |
| `PrintTemplates/DanhSachQuanLyPheDuyet.xlsx` | Header, width, align, `$Stt`… | **Có** — SOT đúng yêu cầu |

Vấn đề thực tế: `Columns` (order / Header / Width / `ColumnAlign`) bị hiểu như cấu hình export, và `dotnet run --project QLDA.Gen -- danh-sach-quan-ly-phe-duyet --force` có thể **ghi đè** chỉnh sửa tay trên template.

### 1.3 Dữ liệu hiện có (không đổi nghiệp vụ)

`PheDuyetExportDto`:

| Property | Placeholder |
|----------|-------------|
| `Stt` | `$Stt` |
| `TenDuAn` | `$TenDuAn` |
| `TenGiaiDoan` | `$TenGiaiDoan` |
| `TenBuoc` | `$TenBuoc` |
| `NguoiTrinh` | `$NguoiTrinh` |
| `NguoiDuyet` | `$NguoiDuyet` |
| `TenTrangThai` | `$TenTrangThai` |

> Ví dụ trong yêu cầu (`$LoaiDeXuat`, `$PhongBanPhuTrach`) chỉ minh họa cơ chế map; **không** thêm cột/DTO trong phạm vi refactor này trừ khi nghiệp vụ yêu cầu riêng sau.

---

## 2. Mục tiêu

1. Template Excel là **nguồn duy nhất** cho thứ tự cột, header text, width, align, font, border, wrap, merge, format số/ngày.
2. Code chỉ map property → `$Placeholder` (giữ `ExporterHelper` hiện có).
3. Descriptor **không** còn được coi / dùng như cấu hình layout runtime; tránh regenerate ghi đè layout đã chỉnh tay.
4. Không sửa migration; không đổi logic lấy dữ liệu; không đụng export khác; build pass.

---

## 3. Các hướng tiếp cận

### A — Tái khẳng định SOT + dọn descriptor (khuyến nghị)

- **Không** đổi `ExporterHelper` / `AsposeHelper` (đã đủ).
- Làm rõ descriptor: `Columns` chỉ là **catalog field** cho Gen bootstrap (nếu còn giữ).
- Bỏ tham số `Width` / `ColumnAlign` khỏi khai báo descriptor này (dùng constructor chỉ `name` + `header`).
- Thêm cờ `HandMaintainedTemplate => true` (optional trên `IExportDescriptor`, default `false`) để Gen **từ chối `--force` ghi đè** khi file đã tồn tại.
- Cập nhật docs feature + checklist AC chỉnh tay trên `.xlsx`.

| Ưu | Nhược |
|----|-------|
| Đúng root cause, ít rủi ro regression | Vẫn còn `List<ExportColumn>` vì `IExportDescriptor` bắt buộc |
| Không đụng BuildingBlocks runtime | Cần kỷ luật không force-regen (có cờ chặn) |

### B — Xóa hoàn toàn Columns khỏi descriptor này

- Thay `IExportDescriptor` hoặc tách `IHandMaintainedExportDescriptor` không có `Columns`.
- Gen không generate được template này nữa.

| Ưu | Nhược |
|----|-------|
| Không còn danh sách cột trong code | Phá contract Gen chung; scope lan sang interface / Program.cs |

### C — Sửa lại engine export riêng cho PheDuyet

- Tự parse placeholder khác cơ chế hiện có.

| Ưu | Nhược |
|----|-------|
| — | Trùng với engine đã có; vi phạm “tái sử dụng helper” |

**Chọn A.**

---

## 4. Thiết kế chi tiết (Approach A)

### 4.1 Phân tách trách nhiệm

```text
┌─────────────────────────────────────────────────────────┐
│  PrintTemplates/DanhSachQuanLyPheDuyet.xlsx              │
│  SOT: cột, header, width, align, style, $Placeholder    │
└───────────────────────────┬─────────────────────────────┘
                            │ ExtractTemplateBinding
                            ▼
┌─────────────────────────────────────────────────────────┐
│  ExporterHelper.Export + PutValueSmart + CopyRow        │
│  Chỉ fill value theo tên property                       │
└───────────────────────────┬─────────────────────────────┘
                            ▲
┌───────────────────────────┴─────────────────────────────┐
│  PheDuyetExportDto + PheDuyetExportMappings             │
│  Chỉ dữ liệu (không layout)                             │
└─────────────────────────────────────────────────────────┘

QLDA.Gen descriptor:
  - HandMaintained = true → không overwrite template đã chỉnh
  - Columns (nếu còn) = catalog tên field; KHÔNG phải layout runtime
```

### 4.2 Thay đổi file (dự kiến)

| File | Thay đổi |
|------|----------|
| `QLDA.Gen/Descriptors/IExportDescriptor.cs` | Thêm `bool HandMaintainedTemplate => false` |
| `QLDA.Gen/Descriptors/DanhSachQuanLyPheDuyetExportDescriptor.cs` | `HandMaintainedTemplate => true`; bỏ Width/Align hard-code; comment SOT |
| `QLDA.Gen/Generators/TemplateGenerator.cs` | Nếu `HandMaintainedTemplate` và file tồn tại: skip (hoặc báo lỗi rõ khi `--force`) |
| `docs/feature/QuanLyPheDuyet/task-export-excel-quan-ly-phe-duyet.md` | Mục “Template là SOT” + quy tắc chỉnh cột |
| `QLDA.Tests/...` (tuỳ chọn) | Test binding order từ workbook fixture / temp xlsx |
| `PrintController` / Query / Mappings / DTO | **Không đổi** trừ xác nhận `AutoFitColumnsAndRows = false` |

### 4.3 Quy tắc vận hành template

| Muốn làm | Làm ở đâu |
|----------|-----------|
| Đổi thứ tự cột | Sửa header row + dòng `$Field` trong `.xlsx` |
| Đổi align / width / font / border | Format ô (và cột) trong `.xlsx` |
| Ẩn cột có sẵn trong DTO | Xóa cột đó khỏi template (không cần sửa code) |
| Thêm cột dữ liệu mới | Thêm property vào DTO + map trong `PheDuyetExportMappings` + thêm `$NewField` trong template |
| Regenerate từ Gen | **Cấm** với template hand-maintained (cờ chặn) |

### 4.4 Giữ nguyên style khi nhiều dòng

Đã có sẵn:

- `CopyRow(templateRow → newRow)` trước khi `PutValueSmart`
- Không gọi AutoFit trên endpoint này

Không thêm logic ghi style sau khi fill.

### 4.5 Error / edge

| Case | Hành vi hiện tại / giữ nguyên |
|------|-------------------------------|
| Template thiếu `$` | Exception `"Khong tim thay cau hinh table"` |
| Có `$Field` nhưng DTO không có property | Ô trống (TryGetValue miss) |
| Có property nhưng template không có `$` | Property bị bỏ qua (đúng — template quyết định cột nào xuất) |
| 0 dòng data | HTTP 400 `"Không có dữ liệu để xuất"` |

### 4.6 Testing

1. **Không regression API khác:** không sửa `ExporterHelper` signature / behavior.
2. **Manual AC:** 4 trường hợp đổi template (thứ tự / thêm cột / align) — xem plan.
3. **Optional unit:** tạo workbook temp với `$B | $A` rồi assert `ColumnMappings` order theo cột Excel, không theo declaration order của DTO.
4. **Build:** `dotnet build` solution / WebApi + chạy `PheDuyetExportTests` nếu có DB fixture.

### 4.7 Ngoài scope

- Migration / snapshot
- Đổi filter / join / resolve tên người
- Thêm `LoaiDeXuat` / `PhongBanPhuTrach` vào DTO
- Refactor toàn bộ descriptor export khác sang `HandMaintainedTemplate`
- Tạo export engine mới

---

## 5. Acceptance Criteria (map)

| AC | Cách đạt |
|----|----------|
| Đổi thứ tự cột chỉ sửa template | Runtime đã đọc cột từ template; docs + cấm Gen overwrite |
| Đổi align / width chỉ sửa template | CopyRow + PutValueSmart + AutoFit=false |
| Không sửa service khi di chuyển cột | PrintController / Query không đụng |
| Không hard-code width/align trong descriptor này | Bỏ args Width/`ColumnAlign` khỏi list Columns |
| Placeholder map theo tên property | Giữ `ExtractTemplateBinding` + reflection DTO |
| Nhiều dòng giữ style dòng mẫu | Giữ `CopyRow` |
| Build OK / không ảnh hưởng API export khác | Không đổi BuildingBlocks runtime; chỉ Gen + docs (+ test tùy chọn) |

---

## 6. Self-review checklist

- [x] Không placeholder TBD trong quyết định chính
- [x] Root cause đúng (Gen dual-SOT), không bịa bug runtime
- [x] Scope hẹp: một descriptor + Gen guard + docs
- [x] Ví dụ `$LoaiDeXuat` làm minh họa, không nhầm thành requirement cột mới
- [x] Commit do người dùng tự làm khi implement

---

**Trạng thái design:** đề xuất Approach A — chờ duyệt trước khi code.  
**Plan chi tiết bước code:** `docs/superpowers/plans/2026-07-14-export-template-driven-quan-ly-phe-duyet.md`
