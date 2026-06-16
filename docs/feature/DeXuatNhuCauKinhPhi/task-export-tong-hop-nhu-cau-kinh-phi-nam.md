# Export Excel — Tổng hợp nhu cầu kinh phí năm (Tình hình đề xuất kinh phí)

**Ngày tạo:** June 2026  
**Cập nhật:** June 2026  
**Trạng thái:** ✅ **IMPLEMENTED** (backend) — FE chưa gắn nút  
**Pattern tham chiếu:** `PrintController.InDanhSachXinChuTruongDauTu` (LINQ + `IExporterHelper` + Aspose)  
**Doc export tương tự:** [`task-export-danh-muc-xin-chu-truong-dau-tu.md`](./task-export-danh-muc-xin-chu-truong-dau-tu.md)  
**Hướng dẫn Aspose:** [`QLDA.WebApi/PrintTemplates/huong-dan.md`](../../../QLDA.WebApi/PrintTemplates/huong-dan.md)  
**Codegen template:** [`QLDA.Gen`](../../../QLDA.Gen/) — slug `tong-hop-nhu-cau-kinh-phi-nam`

---

## 📋 Executive Summary

**Tính năng:** User bấm **Xuất Excel** trên màn **Tổng hợp nhu cầu kinh phí năm** (FE: `/quan-ly-du-an/tinh-hinh-de-xuat-kinh-phi`), tải file `.xlsx` đúng dữ liệu grid (cùng filter, không phân trang).

| Khía cạnh | Giá trị |
|-----------|---------|
| Tên nghiệp vụ / UI | Tổng hợp nhu cầu kinh phí năm / Tình hình đề xuất kinh phí |
| **Entity nguồn grid/export** | `DeXuatNhuCauKinhPhi` — mỗi dòng = 1 đề xuất của 1 dự án, **có `DuAnId`** |
| Entity kế hoạch năm (join) | `DeXuatNhuCauKinhPhiNam` — **không có `DuAnId`**; nối qua junction `DeXuatTrinhKinhPhiNam` |
| Controller danh sách | `DeXuatNhuCauKinhPhiController` |
| API danh sách grid | `GET /api/de-xuat-nhu-cau-kinh-phi/theo-doi-tinh-hinh` |
| **API export (đã có)** | `GET /api/print/tong-hop-nhu-cau-kinh-phi-nam` |
| Query list | `TheoDoiDeXuatNhuCauKinhPhiQuery` |
| Query export | `TheoDoiDeXuatNhuCauKinhPhiGetExportQuery` |
| DTO grid | `TheoDoiDeXuatNhuCauKinhPhiDto` |
| DTO export | `TongHopNhuCauKinhPhiNamExportDto` |
| Template | `QLDA.WebApi/PrintTemplates/TongHopNhuCauKinhPhiNam.xlsx` |
| Phân quyền | `RoleConstants.GroupTongHopNhuCauKinhPhiNamExport` |

> **Không nhầm với:**
> - `GET /api/de-xuat-nhu-cau-kinh-phi/danh-sach-tien-do` → màn **Xin chủ trương đầu tư** (export `danh-sach-xin-chu-truong-dau-tu`)
> - `GET /api/tong-hop-kinh-phi/danh-sach-tien-do` → màn **Tổng hợp kế hoạch kinh phí năm** (entity `DeXuatNhuCauKinhPhiNam`, cột Số KH, Trích yếu, Tổng KP…)

**Migration:** Không cần  
**Stored procedure:** Không (LINQ + Aspose)

---

## 🗄️ Mô hình dữ liệu & `DuAnId`

Màn **theo-doi-tinh-hinh** query bảng **`DeXuatNhuCauKinhPhi`**, không query trực tiếp `DeXuatNhuCauKinhPhiNam`.

```
DuAn (dự án)
    ↑ DuAnId
DeXuatNhuCauKinhPhi          ← 1 dòng grid = 1 đề xuất theo dự án
    ↔ DeXuatTrinhKinhPhiNam   (junction: LeftId = KH năm, RightId = đề xuất)
DeXuatNhuCauKinhPhiNam       ← kế hoạch năm (So, NgayKeHoach, TongKinhPhiDeXuat…)
```

| Bảng / Entity | Có `DuAnId`? | Vai trò |
|---------------|--------------|---------|
| `DeXuatNhuCauKinhPhi` | ✅ | Đề xuất kinh phí từng dự án: `SoPhieuChuyen`, `NgayPhieuChuyen`, `DonViDeXuatId`, `TrangThaiId` (PCT) |
| `DeXuatNhuCauKinhPhiNam` | ❌ | Tổng hợp kế hoạch năm; trạng thái KH-TC / BGĐ lấy qua junction |
| `DeXuatTrinhKinhPhiNam` | — | Nối đề xuất ↔ kế hoạch năm |

**`DuAnId` trong API export:** chỉ là **tham số lọc** (`?duAnId=...`), **không** là cột Excel. Handler: `e.DuAnId == duAnId`.

**Kiểm tra DB:** mở bảng `DeXuatNhuCauKinhPhi` (không phải `DeXuatNhuCauKinhPhiNam`) để thấy `DuAnId`.

```sql
SELECT dx.Id, dx.DuAnId, dx.SoPhieuChuyen, j.LeftId AS KeHoachNamId
FROM DeXuatNhuCauKinhPhi dx
LEFT JOIN DeXuatTrinhKinhPhiNam j ON j.RightId = dx.Id
WHERE dx.IsDeleted = 0;
```

---

## 🔍 Phase 0 — Khảo sát source (đã xác minh)

### 0.1 Frontend route

| Thành phần | Giá trị |
|------------|---------|
| URL màn hình | `https://dxcenter.vietinfo.tech/quan-ly-du-an/tinh-hinh-de-xuat-kinh-phi` |
| Route slug | `tinh-hinh-de-xuat-kinh-phi` |
| API list | `GET /api/de-xuat-nhu-cau-kinh-phi/theo-doi-tinh-hinh` |
| API export | `GET /api/print/tong-hop-nhu-cau-kinh-phi-nam` |

### 0.2 Filter params (list ↔ export đồng bộ)

| Query param | Handler list | Handler export |
|-------------|--------------|----------------|
| `duAnId` | ✅ | ✅ |
| `trangThaiId` | ✅ | ✅ |
| `trangThaiKeHoachNamId` | ✅ | ✅ |
| `soPhieuChuyen` | ✅ `Contains` | ✅ `Contains` |
| `trichYeu` | ✅ `==` exact | ✅ `==` exact |
| `tuNgay` / `denNgay` | ✅ (controller list chưa truyền) | ✅ |
| `donViDeXuatId` | ✅ (controller list chưa truyền) | ✅ |
| `globalFilter` | ❌ truyền nhưng **không filter** | ❌ **không filter** |
| `pageIndex` / `pageSize` | ✅ list only | — export lấy hết |

**Sort export:** `OrderBy CreatedAt` → `ThenBy Id` (list handler không `OrderBy` explicit).

### 0.3 Logic nghiệp vụ 2 giai đoạn

1. **Phòng PCT:** đề xuất theo dự án (`DeXuatNhuCauKinhPhi.TrangThaiId`)
2. **Kế hoạch năm:** sau khi trình vào `DeXuatNhuCauKinhPhiNam` — KH-TC tổng hợp → BGĐ phê duyệt (đọc qua `DeXuatDaTrinhKeHoachNam`)

### 0.4 Cột Excel (đã implement)

| # | Header | Property DTO | Nguồn |
|---|--------|--------------|-------|
| 1 | STT | `Stt` | `index + 1` |
| 2 | Số phiếu chuyển | `SoPhieuChuyen` | Cột riêng |
| 3 | Phòng PCT trình | `PhongPctTrinh` | Multi-line — mục 0.5 |
| 4 | Phòng KH-TC tổng hợp | `PhongKhtcTongHop` | Multi-line |
| 5 | Phòng BGĐ phê duyệt | `PhongBgdPheDuyet` | Multi-line |

Placeholder template: `$Stt`, `$SoPhieuChuyen`, `$PhongPctTrinh`, `$PhongKhtcTongHop`, `$PhongBgdPheDuyet`

### 0.5 Logic ghép multi-line (đã code)

**File:** `TheoDoiDeXuatNhuCauKinhPhiGetExportQuery.cs`

| Cột Excel | Dòng 1 | Dòng 2 | Dòng 3 |
|-----------|--------|--------|--------|
| Phòng PCT trình | `TenTrangThai` (bỏ nếu `---`) | `NgayPhieuChuyen` `dd/MM/yyyy` | `SoPhieuChuyen` |
| Phòng KH-TC tổng hợp | `TenTrangThaiKeHoachNam` (bỏ nếu `--`) | `NgayDuyetKeHoach` `dd/MM/yyyy` | — |
| Phòng BGĐ phê duyệt | `TenTrangThaiBanGiamDoc` (bỏ nếu `--`) | — | — |

- Placeholder `--` / `---` / null → bỏ dòng, không crash.
- Template layout `LetterheadExport`: wrap text trên row template (R5).

### 0.6 Template QLDA.Gen

| Thành phần | Giá trị |
|------------|---------|
| Slug | `tong-hop-nhu-cau-kinh-phi-nam` |
| Descriptor | `TongHopNhuCauKinhPhiNamExportDescriptor` |
| Layout | `TemplateLayoutType.LetterheadExport` (letterhead UBND R1–R2, title R3, header xanh `#D9E1F2` R4, `$Field` R5) |
| Output | `QLDA.WebApi/PrintTemplates/TongHopNhuCauKinhPhiNam.xlsx` |

**Regenerate template:**

```bash
cd QLDA.Gen
dotnet run -- tong-hop-nhu-cau-kinh-phi-nam --force e:\SER\QLDA.WebApi\PrintTemplates
```

### 0.7 Phân quyền (đã thêm)

```csharp
// QLDA.Domain/Constants/RoleConstants.cs
public const string GroupTongHopNhuCauKinhPhiNamExport =
    $"{QLDA_TatCa},{QLDA_QuanTri},{QLDA_LDDV},{QLDA_ChuyenVien}";
```

Roles: CB/LĐ.PCT, GĐ/PGĐ, CB/LĐ.PKH-TC (+ admin `QLDA_TatCa`, `QLDA_QuanTri`).

---

## 🏗️ Luồng xử lý

```
Màn "Tình hình đề xuất kinh phí"
├── GET /api/de-xuat-nhu-cau-kinh-phi/theo-doi-tinh-hinh     → Grid (phân trang)
└── GET /api/print/tong-hop-nhu-cau-kinh-phi-nam             → Export Excel (toàn bộ filter)

DeXuatNhuCauKinhPhi (có DuAnId)
├── SoPhieuChuyen, NgayPhieuChuyen, TrangThaiId        → cột PCT
└── DeXuatDaTrinhKeHoachNam → DeXuatNhuCauKinhPhiNam    → cột KH-TC, BGĐ
```

**Chiến lược query:** Export handler **copy filter + projection** từ list handler (không extract shared method, không gọi list query với `PageSize = MaxValue`).

---

## 📂 Files đã tạo / sửa

### Tạo mới ✅

| File | Mô tả |
|------|-------|
| `QLDA.Application/DeXuatNhuCauKinhPhi/DTOs/TongHopNhuCauKinhPhiNamExportDto.cs` | 5 property = placeholder template |
| `QLDA.Application/DeXuatNhuCauKinhPhi/Queries/TheoDoiDeXuatNhuCauKinhPhiGetExportQuery.cs` | Query + handler export |
| `QLDA.WebApi/Models/DeXuatNhuCauKinhPhis/TheoDoiDeXuatNhuCauKinhPhiPrintSearchModel.cs` | Search params export |
| `QLDA.WebApi/PrintTemplates/TongHopNhuCauKinhPhiNam.xlsx` | Template Aspose |
| `QLDA.Gen/Descriptors/TongHopNhuCauKinhPhiNamExportDescriptor.cs` | Descriptor codegen |

### Sửa ✅

| File | Thay đổi |
|------|----------|
| `QLDA.Domain/Constants/RoleConstants.cs` | `GroupTongHopNhuCauKinhPhiNamExport` |
| `QLDA.WebApi/Controllers/PrintController.cs` | Region `TongHopNhuCauKinhPhiNam` + `InTongHopNhuCauKinhPhiNam` |
| `QLDA.Gen/Program.cs` | Đăng ký slug `tong-hop-nhu-cau-kinh-phi-nam` |
| `QLDA.Gen/Descriptors/IExportDescriptor.cs` | Thêm `LetterheadExport` layout |
| `QLDA.Gen/Generators/TemplateGenerator.cs` | `BuildLetterheadExport` |

### Không sửa

| File | Lý do |
|------|-------|
| `DeXuatNhuCauKinhPhiController.cs` | Export theo convention ở `PrintController` |
| `TheoDoiDeXuatNhuCauKinhPhiQueryHandler` | Giữ nguyên; export duplicate logic |
| Migration / snapshot | Không đổi DB |

---

## 🌐 API Export

```http
GET /api/print/tong-hop-nhu-cau-kinh-phi-nam?duAnId={guid}&trangThaiId={int}&trangThaiKeHoachNamId={int}&soPhieuChuyen={string}&trichYeu={string}
Authorization: Bearer {token}
```

| Query param | Kiểu | Mô tả |
|-------------|------|-------|
| `duAnId` | `Guid?` | Lọc đề xuất theo dự án (`DeXuatNhuCauKinhPhi.DuAnId`) |
| `trangThaiId` | `int?` | Trạng thái PCT |
| `trangThaiKeHoachNamId` | `int?` | Trạng thái kế hoạch năm |
| `soPhieuChuyen` | `string?` | Contains |
| `trichYeu` | `string?` | Exact match |
| `tuNgay` / `denNgay` | `DateOnly?` | Ngày phiếu chuyển |
| `donViDeXuatId` | `long?` | Phòng đề xuất |
| `globalFilter` | `string?` | Nhận param nhưng **chưa filter** (giống list) |
| `hiddenColumns` | `string[]` | Ẩn cột optional |

**Response:** `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`  
**Tên file:** `TongHopNhuCauKinhPhiNam_ddMMyyyy_HHmmss.xlsx`

**Swagger:** Authorize JWT → `GET /api/print/tong-hop-nhu-cau-kinh-phi-nam` → Execute.

---

## 🎯 Phạm vi

### Đã xong (backend)

- Endpoint export + phân quyền
- Export query EF, filter giống list, không phân trang
- Export DTO + PrintSearchModel
- Template `LetterheadExport` qua QLDA.Gen
- Multi-line cột trạng thái

### Chưa làm

- **FE:** gắn nút Xuất Excel + truyền cùng query params với grid
- **Kiểm thử thủ công** trên staging với data thật
- **`globalFilter`:** list và export đều chưa áp dụng — cần BA nếu muốn đồng bộ sau

---

## 🔌 Tích hợp FE (hướng dẫn)

```typescript
// Cùng filter object với API theo-doi-tinh-hinh
const params = new URLSearchParams({
  ...(duAnId && { duAnId }),
  ...(trangThaiId != null && { trangThaiId: String(trangThaiId) }),
  ...(trangThaiKeHoachNamId != null && { trangThaiKeHoachNamId: String(trangThaiKeHoachNamId) }),
  ...(soPhieuChuyen && { soPhieuChuyen }),
  ...(trichYeu && { trichYeu }),
});

window.open(`/api/print/tong-hop-nhu-cau-kinh-phi-nam?${params}`, '_blank');
```

---

## ✅ Validation Checklist

### Code Quality

- [x] Export DTO compile
- [x] Export query compile
- [x] QLDA.Gen descriptor + slug registered
- [x] Template trong `PrintTemplates/`
- [x] PrintController endpoint
- [ ] `dotnet build` solution pass *(verify local)*

### Chức năng

- [x] Filter export khớp `theo-doi-tinh-hinh`
- [x] STT 1, 2, 3…
- [x] Multi-line cột trạng thái
- [x] Ngày `dd/MM/yyyy`
- [x] Phân quyền constant
- [x] Không sửa migration
- [ ] So sánh số dòng grid vs export trên staging
- [ ] FE nút Xuất Excel

---

## 📊 Effort

| Phase | Task | Status |
|-------|------|--------|
| 0 | Phân tích source | ✅ |
| 1 | RoleConstants | ✅ |
| 2 | QLDA.Gen + template LetterheadExport | ✅ |
| 3 | Export DTO + Query | ✅ |
| 4 | PrintSearchModel + PrintController | ✅ |
| 5 | FE integration | ⬜ |
| 6 | Kiểm thử staging | ⬜ |

---

## 📞 Common Issues

| Vấn đề | Nguyên nhân | Giải pháp |
|--------|-------------|-----------|
| Không thấy `DuAnId` trong DB | Đang xem bảng `DeXuatNhuCauKinhPhiNam` | Xem `DeXuatNhuCauKinhPhi` hoặc dùng `duAnId` chỉ để lọc API |
| Số dòng export ≠ grid | Filter khác / list không sort | Export sort `CreatedAt, Id`; so cùng filter |
| Multi-line không xuống dòng | Template cũ | Regenerate `LetterheadExport`; row R5 có wrap text |
| 403 Forbidden | Thiếu role | `GroupTongHopNhuCauKinhPhiNamExport` |
| Template not found | File thiếu trong output | Copy/regenerate vào `PrintTemplates/` |

---

## ❓ Open Questions

1. **`globalFilter`** — có cần filter ô tìm kiếm chung không? (List + export hiện **chưa** áp dụng.)
2. **Sort grid** — FE sort client-side? Export dùng `CreatedAt` — có cần khớp thứ tự UI không?
3. **Dòng 3 cột PCT** — export dùng `SoPhieuChuyen` lặp (đã có cột riêng). Cần FE xác nhận có đúng UI không.

---

## 📝 Báo cáo implement

| Hạng mục | Giá trị |
|----------|---------|
| API endpoint | `GET /api/print/tong-hop-nhu-cau-kinh-phi-nam` |
| Handler | `TheoDoiDeXuatNhuCauKinhPhiGetExportQueryHandler` |
| Template | `TongHopNhuCauKinhPhiNam.xlsx` |
| QLDA.Gen slug | `tong-hop-nhu-cau-kinh-phi-nam` |
| Layout | `LetterheadExport` |
| Role | `GroupTongHopNhuCauKinhPhiNamExport` |
| Entity nguồn | `DeXuatNhuCauKinhPhi` (`DuAnId` = filter only) |
| FE | Chưa tích hợp |

---

## 🔗 Files tham chiếu

| File | Vai trò |
|------|---------|
| `DeXuatNhuCauKinhPhiController.cs` | API `theo-doi-tinh-hinh` |
| `TheoDoiDeXuatNhuCauKinhPhiQuery.cs` | Logic list |
| `TheoDoiDeXuatNhuCauKinhPhiGetExportQuery.cs` | Logic export |
| `TongHopNhuCauKinhPhiNamExportDto.cs` | DTO export |
| `PrintController.InTongHopNhuCauKinhPhiNam` | Endpoint |
| `TongHopNhuCauKinhPhiNamExportDescriptor.cs` | Codegen template |
| `DeXuatNhuCauKinhPhi.cs` / `DeXuatNhuCauKinhPhiNam.cs` | Entity + `DuAnId` |
| `task-export-danh-muc-xin-chu-truong-dau-tu.md` | Doc export cùng module |
