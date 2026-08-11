# Issue #169 — Report: Chỉnh sửa màn hình 9667 (KetQuaTrungThau)

> **Status:** BE updated — `TrangThaiDangTai` = `ETrangThaiMoiThau` (0/1/2); apply migration `ChangeKetQuaTrungThauTrangThaiDangTaiToEnum`  
> **Date:** 2026-08-11

## Summary

Issue #169 chỉnh màn **9667 / Kết quả trúng thầu**. Trong repo BE (`E:/SER`), màn này map 1-1 tới module `KetQuaTrungThau`. Frontend (label + gọi combobox) nằm repo khác — BE chỉ mở rộng API/DTO/attachment/field lưu trạng thái.

## 1. Hiện trạng BE

### 1.1. API Kết quả trúng thầu

`KetQuaTrungThauController` — `api/ket-qua-trung-thau`:

| Method | Route | Handler |
|--------|-------|---------|
| GET | `{id}/chi-tiet` | `KetQuaTrungThauGetQuery` + hydrate `GetAttachmentsQuery` |
| POST | `them-moi` | `KetQuaTrungThauInsertCommand` + `AttachmentBulkInsertOrUpdateCommand` |
| PUT | `cap-nhat` | `KetQuaTrungThauUpdateCommand` + attachment sync |
| DELETE | `{id}/xoa` | `KetQuaTrungThauDeleteCommand` |
| GET | `danh-sach-tien-do` | `KetQuaTrungThauGetDanhSachQuery` |

Attachment hiện tại:

- `GroupId` = `entity.Id`
- `GroupType` = `EGroupType.KetQuaTrungThau`
- DTO: `DanhSachTepDinhKem`

### 1.2. Entity fields hiện có (map layout)

| UI (issue) | BE property | Có sẵn? |
|------------|-------------|---------|
| Gói thầu | `GoiThauId` | ✅ |
| Đơn vị trúng thầu | `DonViTrungThauId` | ✅ (label FE sai “Đơn vị thực hiện”) |
| Giá trị thầu | từ `GoiThau.GiaTri` (FE) | — |
| Số quyết định | `SoQuyetDinh` | ✅ |
| Giá trị trúng thầu | `GiaTriTrungThau` | ✅ |
| Ngày mở thầu | `NgayMoThau` | ✅ |
| Ngày quyết định | `NgayQuyetDinh` | ✅ |
| Thời gian thực hiện gói thầu | `SoNgayTrienKhai` | ✅ |
| Loại hợp đồng | `LoaiHopDongId` | ✅ (#9643) |
| Ngày đăng EHSMT | `NgayEHSMT` | ✅ |
| Thời gian thực hiện hợp đồng | `SoNgayThucHienHopDong` | ✅ |
| Biên bản thương thảo | — | ❌ cần GroupType + list DTO |
| Trạng thái đăng tải | — | ✅ `ETrangThaiMoiThau TrangThaiDangTai` (DaDang=0, ChuaDang=1, Huy=2) |
| Trích yếu | `TrichYeu` | ✅ |

## 2. Design đề xuất

### 2.1. `IsThamDinh` trên `goi-thau/combobox`

**Approach (recommended):** Optional filter trên search DTO hiện có — không tạo endpoint mới.

```text
GET api/goi-thau/combobox?IsThamDinh=true
GET api/goi-thau/combobox                    → behavior cũ
```

**Thay đổi:**

1. `GoiThauSearchDto` thêm `bool? IsThamDinh`
2. `GoiThauGetDanhSachQueryHandler` inject `IRepository<HoSoMoiThauDienTu, Guid>` (hoặc query qua context sẵn có)
3. Filter:

```csharp
.WhereIf(request.SearchDto.IsThamDinh == true,
    e => hsmt.GetQueryableSet()
        .Any(h => h.GoiThauId == e.Id && h.ThamDinh == true))
```

- `IsThamDinh == false` / `null`: **không** áp filter thẩm định (giữ tương thích).
- Nguồn truth: `HoSoMoiThauDienTu.ThamDinh` — không hard-code magic number.

**Callers BE của combobox:** chỉ `GoiThauController.GetCbo`. Các màn FE khác gọi cùng URL — không truyền `IsThamDinh` thì an toàn. Cache: `VaryByQueryKeys = ["*"]` đã cover.

**FE 9667:** thêm query `IsThamDinh=true` khi load CBB gói thầu.

### 2.2. Label Đơn vị trúng thầu

- **Chỉ FE** — đổi text hiển thị.
- BE giữ `DonViTrungThauId` / `DonViTrungThau`.

### 2.3. Biên bản thương thảo (attachment)

**Approach (recommended):** Multi-GroupType giống `QuyetDinhDuyetDuToan` / `BanGiaoHoSo`.

| Thành phần | Giá trị đề xuất |
|------------|-----------------|
| `EGroupType` mới | `KetQuaTrungThau_BienBanThuongThao` |
| DTO property | `DanhSachBienBanThuongThao` (`List<TepDinhKemDto>` / Insert-Update tương ứng) |
| `GroupId` | `KetQuaTrungThau.Id.ToString()` |
| Write | `AttachmentBulkInsertOrUpdateCommand` riêng, `GroupTypes = [nameof(...BienBanThuongThao)]`, `AutoDeleteMissing = true` |
| Read | `GetAttachmentsQuery(GroupIds, BaseGroupTypes: [...])` → map vào DTO |

**Không** gộp vào `DanhSachTepDinhKem` hiện tại (tránh FE lẫn file quyết định / file khác với biên bản).

**Migration:** không cần cho attachment (chỉ thêm enum value → string `GroupType`).

### 2.4. Trạng thái đăng tải (`ETrangThaiMoiThau` — 0/1/2)

> **Cập nhật 2026-08-11:** đổi từ `bool` sang enum `ETrangThaiMoiThau` (reuse danh mục đã có).

| Layer | Thay đổi |
|-------|----------|
| Domain / DTO | `ETrangThaiMoiThau TrangThaiDangTai` (default `ChuaDang`) |
| Persistence | cột `int` |
| Migration | `ChangeKetQuaTrungThauTrangThaiDangTaiToEnum` (bit→int, AlterColumn only) |

| id | rawName | ten |
|----|---------|-----|
| 0 | `DaDang` | Đã đăng |
| 1 | `ChuaDang` | Chưa đăng |
| 2 | `Huy` | Hủy |

**FE:** CBB bind `TrangThaiDangTai` (`0|1|2`) qua `api/danh-muc-enum/danh-sach?enumName=ETrangThaiMoiThau`.
## 3. Danh sách file dự kiến phải sửa

### 3.1. BE — bắt buộc (trong repo này)

| File | Việc |
|------|------|
| `QLDA.Application/GoiThaus/DTOs/GoiThauSearchDto.cs` | Thêm `bool? IsThamDinh` |
| `QLDA.Application/GoiThaus/Queries/GoiThauGetDanhSachQuery.cs` | Filter theo `HoSoMoiThauDienTu.ThamDinh` |
| `QLDA.Domain/Enums/EGroupType.cs` | Thêm `KetQuaTrungThau_BienBanThuongThao` |
| `QLDA.Domain/Entities/KetQuaTrungThau.cs` | `ETrangThaiMoiThau TrangThaiDangTai` |
| `QLDA.Application/KetQuaTrungThaus/DTOs/KetQuaTrungThauDto.cs` | `TrangThaiDangTai` + `DanhSachBienBanThuongThao` |
| `QLDA.Application/KetQuaTrungThaus/DTOs/KetQuaTrungThauInsertDto.cs` |同上 |
| `QLDA.Application/KetQuaTrungThaus/DTOs/KetQuaTrungThauUpdateDto.cs` |同上 |
| `QLDA.Application/KetQuaTrungThaus/KetQuaTrungThauMappings.cs` | Map field mới |
| `QLDA.WebApi/Controllers/KetQuaTrungThauController.cs` | Sync/load attachment GroupType mới |
| `QLDA.Migrator/Migrations/..._ChangeKetQuaTrungThauTrangThaiDangTaiToEnum.cs` | bit → int + remap |

Có thể cần cập nhật list query / validators nếu project bắt buộc map đầy đủ field trên list DTO.

### 3.2. FE — ngoài repo (ghi nhận để ticket FE)

| Việc | Ghi chú |
|------|---------|
| Form màn 9667 | Label Đơn vị trúng thầu |
| CBB Gói thầu | `goi-thau/combobox?IsThamDinh=true` |
| Upload Biên bản thương thảo | Bind `DanhSachBienBanThuongThao` |
| CBB Trạng thái đăng tải | Bind `TrangThaiDangTai` (`0|1|2` = `ETrangThaiMoiThau`) |

### 3.3. Không sửa

- `AppDbContextModelSnapshot.cs` thủ công
- Migration cũ
- Module ngoài `GoiThau` combobox filter / `KetQuaTrungThau` / `EGroupType`
- Tạo `DanhMucTrangThaiDangTai` mới

## 4. Migration policy

| Thay đổi | Cần migration? |
|----------|----------------|
| `IsThamDinh` filter | ❌ |
| `EGroupType` mới (attachment) | ❌ |
| `TrangThaiDangTai` (`ETrangThaiMoiThau` / int) trên `KetQuaTrungThau` | ✅ |
| Label FE | ❌ |

Tạo migration bằng `ef.bat add` sau khi Domain + Persistence.Configuration đã cập nhật (cùng commit group theo rule dự án).

## 5. Rủi ro / điểm cần xác nhận trước khi code

1. **Default `TrangThaiDangTai`:** `ChuaDang` (1) cho bản ghi mới (property initializer).
2. **Nhiều E-HSMT / 1 gói thầu:** filter `Any(ThamDinh == true)` — nếu nghiệp vụ yêu cầu “E-HSMT mới nhất đã thẩm định” thì cần siết thêm (hiện issue chỉ nói “đã tích thẩm định”).
3. **GetAttachmentsQuery hiện tại** trên chi tiết `KetQuaTrungThau` không truyền `BaseGroupTypes` → lấy mọi GroupType của GroupId. Sau khi thêm GroupType biên bản, **phải** split rõ 2 list (hoặc filter bằng `BaseGroupType()`), tránh nhét biên bản vào `DanhSachTepDinhKem`.
## 6. Approaches đã cân nhắc

| # | Approach | Pros | Cons | Chọn? |
|---|----------|------|------|-------|
| A | Optional `IsThamDinh` trên combobox hiện có | Backward compatible, ít surface | — | ✅ |
| B | Endpoint combobox riêng cho 9667 | Cách ly | Duplicate API | ❌ |
| C | Attachment GroupType riêng cho biên bản | Đúng multi-file pattern | Thêm enum | ✅ |
| D | Nhét biên bản vào `DanhSachTepDinhKem` | Ít code | FE khó tách field | ❌ |
| E | `bool TrangThaiDangTai` như E-HSMT (đã/chưa) | Đúng dataType boolean ban đầu | Không đủ 3 trạng thái | ❌ (superseded) |
| F | `ETrangThaiMoiThau` (0/1/2) reuse enum + danh-muc-enum | Đủ DaDang/ChuaDang/Huy, FE CBB sẵn | Cần migration bit→int | ✅ |

## 7. Next step

Apply migration rồi FE bind enum:

```bat
ef.bat QLDA update
```

FE: `api/danh-muc-enum/danh-sach?enumName=ETrangThaiMoiThau`, gửi `trangThaiDangTai: 0|1|2`. Verify theo [test-workflow.md](./test-workflow.md).
