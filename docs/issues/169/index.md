# Issue #169 — Chỉnh sửa màn hình 9667 (Kết quả trúng thầu)

> **Nguồn:** PMIS / Redmine issue #169  
> **Màn hình:** 9667 — Kết quả trúng thầu  
> **Trạng thái:** ✅ BE code xong — chờ migration + FE  
> **Entity BE:** `QLDA.Domain/Entities/KetQuaTrungThau.cs`  
> **API chính:** `api/ket-qua-trung-thau`

## 1. Mô tả yêu cầu

Chỉnh sửa màn hình **9667 - Kết quả trúng thầu**:

| # | Hạng mục | Mô tả |
|---|----------|--------|
| 1 | Combobox Gói thầu | `goi-thau/combobox` nhận thêm `IsThamDinh`; màn 9667 gọi với `IsThamDinh=true` → chỉ gói thầu đã tích thẩm định bên E-HSMT |
| 2 | Label | Đổi label **Đơn vị thực hiện** → **Đơn vị trúng thầu** (chỉ UI; field BE đã là `DonViTrungThauId`) |
| 3 | Biên bản thương thảo | Field đính kèm file mới |
| 4 | Trạng thái đăng tải | Combobox UI; **dataType = `boolean`** (đã / chưa đăng tải); bind create / detail / update |

### Layout tham khảo (sau chỉnh sửa)

- Gói thầu *
- Đơn vị trúng thầu *
- Giá trị thầu
- Số quyết định *
- Giá trị trúng thầu *
- Ngày mở thầu
- Ngày quyết định *
- Thời gian thực hiện gói thầu
- Loại hợp đồng
- Ngày đăng EHSMT
- Thời gian thực hiện hợp đồng
- **Biên bản thương thảo** — file attachment
- **Trạng thái đăng tải** — combobox UI, giá trị `boolean` (đã / chưa đăng tải)
- Trích yếu

## 2. Kết quả điều tra (pre-implement)

### 2.1. Màn hình 9667 nằm ở đâu?

| Lớp | Vị trí | Ghi chú |
|-----|--------|---------|
| **FE** | Repo Frontend (không có trong workspace `E:/SER`) | Màn hình ID **9667** = DanhMucManHinh / form Kết quả trúng thầu |
| **BE API** | `QLDA.WebApi/Controllers/KetQuaTrungThauController.cs` | Route `api/ket-qua-trung-thau` |
| **Application** | `QLDA.Application/KetQuaTrungThaus/` | Commands / Queries / DTOs / Mappings |
| **Domain** | `QLDA.Domain/Entities/KetQuaTrungThau.cs` | Entity Kết quả LCNT |

> Repo hiện tại chỉ có BE. Label UI (#2) và gọi combobox `IsThamDinh=true` (#1 FE) phải làm ở repo Frontend. BE chỉ cung cấp API/DTO.

### 2.2. API `goi-thau/combobox`

| Thành phần | File |
|------------|------|
| Controller | `QLDA.WebApi/Controllers/GoiThauController.cs` — `[HttpGet("combobox")]` → `GetCbo` |
| Search DTO | `QLDA.Application/GoiThaus/DTOs/GoiThauSearchDto.cs` |
| Query / Handler | `QLDA.Application/GoiThaus/Queries/GoiThauGetDanhSachQuery.cs` (`IsCbo = true`) |

Hiện **chưa có** `IsThamDinh` trên `GoiThauSearchDto`.

Cache profile Combobox dùng `VaryByQueryKeys = ["*"]` → thêm query param không gây cache collision.

### 2.3. Field “đã thẩm định E-HSMT”

| Entity | Property | Kiểu | Ý nghĩa |
|--------|----------|------|---------|
| `HoSoMoiThauDienTu` | `ThamDinh` | `bool?` | Checkbox thẩm định trên màn E-HSMT |

File: `QLDA.Domain/Entities/HoSoMoiThauDienTu.cs`

Quan hệ: `HoSoMoiThauDienTu.GoiThauId` → `GoiThau` (không có navigation ngược trên `GoiThau`).

**Điều kiện lọc đề xuất khi `IsThamDinh = true`:**

```csharp
// Tồn tại HoSoMoiThauDienTu (chưa xóa) của gói thầu với ThamDinh == true
.Where(e => hsmtRepo.GetQueryableSet()
    .Any(h => h.GoiThauId == e.Id && h.ThamDinh == true))
```

Khi **không truyền** / `null`: giữ behavior hiện tại (backward compatible).

### 2.4. Pattern attachment sẽ reuse

Theo `docs/code-standards.md` §14 + pattern multi-GroupType:

| Tham chiếu | Lý do |
|------------|--------|
| `KetQuaTrungThauController` hiện tại | Đã dùng `AttachmentBulkInsertOrUpdateCommand` + `GetAttachmentsQuery` với `EGroupType.KetQuaTrungThau` |
| `QuyetDinhDuyetDuToanController` | Multi list: `DanhSachTepDinhKem` + `DanhSachTepDinhKemKhac` (2 GroupType) |
| `BanGiaoHoSoController` | `DanhSachBienBan` + `EGroupType.BienBanBanGiao` |

**Đề xuất cho Biên bản thương thảo:**

- Thêm `EGroupType.KetQuaTrungThau_BienBanThuongThao` (hoặc tên tương đương)
- DTO thêm `DanhSachBienBanThuongThao` (list `TepDinhKem*`)
- Sync riêng 1 lần `AttachmentBulkInsertOrUpdateCommand` với `GroupTypes` của GroupType mới
- Get chi tiết: `GetAttachmentsQuery` với `BaseGroupTypes` tương ứng
- **Không** tạo cơ chế upload mới; **không** cần cột DB (GroupType là string trên bảng `TepDinhKem`)

### 2.5. “Trạng thái đăng tải” — đã có gì?

> **Quyết định nghiệp vụ (#169):** dataType = **`boolean`** — gồm 2 giá trị **đã đăng tải** / **chưa đăng tải**.  
> FE vẫn có thể hiện CBB 2 option; BE lưu `bool` (không dùng danh mục / `int?` Id).

| Nơi dùng | Kiểu | Ghi chú |
|----------|------|---------|
| `HoSoMoiThauDienTu.TrangThaiDangTai` | `bool` | **Pattern đúng để reuse** cho màn 9667 |
| `ToTrinhKetQuaGoiThau.TrangThaiDangTaiId` | `int?` | Pattern khác — **không** dùng cho #169 |
| `TrienKhaiKeHoachLCNT.TrangThaiDangTaiId` | `int?` | Pattern khác — **không** dùng cho #169 |
| `ToTrinhThamDinhNhaThau.TrangThaiDangTaiId` | `int?` | Pattern khác — **không** dùng cho #169 |

**Không có** trong `EDanhMuc`: không có `DanhMucTrangThaiDangTai` — và **không cần tạo**.

| Giá trị BE (`bool`) | Ý nghĩa UI |
|---------------------|------------|
| `false` | Chưa đăng tải |
| `true` | Đã đăng tải |

**KetQuaTrungThau hiện chưa có** `TrangThaiDangTai` → bổ sung cột `bit` → **cần migration**.

### 2.6. Label “Đơn vị thực hiện”

BE field đã đúng nghiệp vụ: `DonViTrungThauId` → `DanhMucNhaThau`.  
Chỉ đổi label FE; **không** rename property/DB.

## 3. Acceptance Criteria

- [ ] `goi-thau/combobox` nhận thêm `IsThamDinh` (`bool?`)
- [ ] `IsThamDinh=true` chỉ trả gói thầu có E-HSMT `ThamDinh == true`
- [ ] Không truyền `IsThamDinh` → behavior cũ
- [ ] Màn 9667 (FE) gọi combobox với `IsThamDinh=true`
- [ ] Label Đơn vị thực hiện → Đơn vị trúng thầu (FE)
- [ ] Có upload Biên bản thương thảo (load lại khi detail/edit)
- [ ] Có CBB Trạng thái đăng tải (`boolean` đã/chưa; create/detail/update bind đúng)
- [ ] Không duplicate enum/danh mục/API đã có
- [ ] Build thành công
- [ ] Migration chỉ khi schema đổi (`TrangThaiDangTai` bit); không sửa ModelSnapshot thủ công
- [ ] Không sửa ngoài phạm vi issue

## 4. Tài liệu liên quan trong issue folder

| File | Mục đích |
|------|----------|
| [report.md](./report.md) | Phân tích kỹ thuật + danh sách file sửa + design đề xuất |
| [journal.md](./journal.md) | Nhật ký công việc |
| [test-workflow.md](./test-workflow.md) | Cách verify sau khi implement |
