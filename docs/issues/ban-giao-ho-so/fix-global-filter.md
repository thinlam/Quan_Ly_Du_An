# Fix `globalFilter` — API danh sách Bàn giao hồ sơ

**Ngày tạo:** June 2026  
**Trạng thái:** 📋 **PLANNED** — chưa implement  
**Module:** `BanGiaoHoSo`  
**Endpoint:** `GET /api/ban-giao-ho-so/danh-sach`  
**Pattern tham chiếu:** `WhereGlobalFilter` + `IMayHaveGlobalFilter` (BuildingBlocks), `TamUngGetDanhSachQuery`, `HoSoDeXuatCapDoCnttGetDanhSachQuery`

---

## Executive Summary

API danh sách bàn giao hồ sơ nhận query param `globalFilter` từ FE nhưng **không lọc dữ liệu**. Khi gọi với `globalFilter=54444`, response vẫn trả về các hồ sơ có `ma = 1`, `ma = 10101`, … thay vì chỉ record `ma = 54444`.

**Nguyên nhân gốc:** Module `BanGiaoHoSo` được implement **cố ý bỏ qua** full-text search — comment trong source ghi rõ *"Không implement IMayHaveGlobalFilter"*. `BanGiaoHoSoSearchDto` không có property `GlobalFilter`, handler không gọi `WhereGlobalFilter`.

**Phạm vi sửa:** Chỉ Application layer (SearchDto + Query handler). Không migration, không đổi response DTO.

---

## Triệu chứng & dữ liệu test

### Request

```
GET /QuanLyDuAn/api/ban-giao-ho-so/danh-sach
  ?trangThai=2
  &pageIndex=1
  &pageSize=10
  &globalFilter=54444
  &buocId=4827
  &duAnId=08deab16-341d-3a23-687a-7b40b8010d4a
```

| Param | Giá trị | Ý nghĩa |
|-------|---------|---------|
| `trangThai` | `2` | `ETrangThaiBanGiao.DaBanGiao` (Đã bàn giao) |
| `buocId` | `4827` | Lọc theo bước dự án |
| `duAnId` | `08deab16-...` | Lọc theo dự án |
| `globalFilter` | `54444` | Tìm kiếm full-text |

### Kết quả hiện tại (SAI)

- `totalRows = 3`
- Data chứa hồ sơ `ma = 1`, `ma = 10101`, `ma = 54444`, …
- `globalFilter` bị **bỏ qua hoàn toàn**

### Kết quả kỳ vọng (ĐÚNG)

- `totalRows = 1`
- Chỉ còn record:

| Field | Giá trị |
|-------|---------|
| `id` | `08deb170-f677-d039-687a-7b356802b551` |
| `ma` | `54444` |
| `tenHoSo` | `thêm mới` |

- Các filter `trangThai`, `duAnId`, `buocId`, phân trang vẫn hoạt động bình thường
- `globalFilter` kết hợp với các filter khác bằng **AND**
- Các field tìm kiếm bên trong `globalFilter` kết hợp bằng **OR**

---

## Khảo sát source hiện tại

### Luồng xử lý

```
BanGiaoHoSoController.GetList
  └─ [FromQuery] BanGiaoHoSoSearchDto searchDto     ← không có GlobalFilter
  └─ [FromQuery] AggregateRootPagination pagination
  └─ MediatR → BanGiaoHoSoGetDanhSachQuery
       └─ BanGiaoHoSoGetDanhSachQueryHandler.Handle
            ├─ FilterVisibleChildEntities (auth theo BuocId)
            ├─ Where CreatedBy == current user
            ├─ WhereIf TrangThai / DuAnId / BuocId
            ├─ LeftOuterJoin UserMaster (TenNguoiTao)
            ├─ LeftOuterJoin DmDonVi × 2 (TenPhongBan, TenPhongBanNhan)
            └─ Select → BanGiaoHoSoDto → PaginatedListAsync
```

### File liên quan

| File | Vai trò | Vấn đề |
|------|---------|--------|
| `QLDA.WebApi/Controllers/BanGiaoHoSoController.cs` | Bind query params, gọi MediatR | OK — không cần sửa nếu thêm `GlobalFilter` vào SearchDto |
| `QLDA.Application/BanGiaoHoSos/DTOs/BanGiaoHoSoSearchDto.cs` | Search params | ❌ Thiếu `GlobalFilter`, không implement `IMayHaveGlobalFilter` |
| `QLDA.Application/BanGiaoHoSos/Queries/BanGiaoHoSoGetDanhSachQuery.cs` | Query handler | ❌ Comment "không có search full-text", không gọi filter |
| `BuildingBlocks.../GlobalFilterExtensions.cs` | `WhereGlobalFilter` helper | ✅ Sẵn có — trim, lowercase, contains, null-safe |
| `QLDA.Domain/Entities/BanGiaoHoSo.cs` | Entity | Nav: `DuAn`, `Buoc`. Không nav tới `UserMaster`, `DmDonVi` |

### Code hiện tại (điểm lỗi)

**SearchDto — không có GlobalFilter:**

```csharp
public class BanGiaoHoSoSearchDto {
    public int? TrangThai { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
}
```

**Handler — chỉ filter 3 field, bỏ qua globalFilter:**

```csharp
// Không implement IMayHaveGlobalFilter - không có search full-text
var queryable = _buocAuth.FilterVisibleChildEntities(...)
    .Where(e => e.CreatedBy == _authContext.UserId.ToString())
    .WhereIf(request.SearchDto.TrangThai.HasValue, e => (int)e.TrangThai == request.SearchDto.TrangThai!.Value)
    .WhereIf(request.SearchDto.DuAnId.HasValue, e => e.DuAnId == request.SearchDto.DuAnId!.Value)
    .WhereIf(request.SearchDto.BuocId.HasValue, e => e.BuocId == request.SearchDto.BuocId!.Value)
    // ← THIẾU globalFilter
    .LeftOuterJoin(...)
```

### Pattern chuẩn trong project

Các module khác dùng:

1. SearchDto / Query implement `IMayHaveGlobalFilter` với property `string? GlobalFilter`
2. Handler gọi `.WhereGlobalFilter(request, e => field1, e => field2, ...)`
3. Helper tự xử lý: trim, `ToLower()`, `Contains`, null-check

**Ví dụ tham chiếu — `TamUngGetDanhSachQuery`:**

```csharp
.WhereGlobalFilter(
    request,
    e => e.SoPhieuChi,
    e => e.NoiDung,
    e => e.HopDong!.Ten
);
```

**Ví dụ SearchDto — `HoSoDeXuatCapDoCnttSearchDto`:**

```csharp
public record HoSoDeXuatCapDoCnttSearchDto : AggregateRootPagination, IMayHaveGlobalFilter {
    public string? GlobalFilter { get; set; }
    // ...
}
```

### Đặc thù module BanGiaoHoSo

Khác với `TamUng`, module này có **join legacy tables** để lấy field hiển thị:

| Field DTO | Nguồn dữ liệu | Filter qua entity nav? |
|-----------|---------------|------------------------|
| `ma` | `BanGiaoHoSo.Ma` | ✅ Trực tiếp |
| `tenHoSo` | `BanGiaoHoSo.TenHoSo` | ✅ Trực tiếp |
| `tenDuAn` | `BanGiaoHoSo.DuAn.TenDuAn` | ✅ Nav property |
| `tenBuoc` | `BanGiaoHoSo.Buoc.TenBuoc` | ✅ Nav property |
| `ghiChu` | `BanGiaoHoSo.GhiChu` | ✅ Trực tiếp |
| `tenPhongBan` | `DmDonVi` via `PhongBanChuTriId` | ❌ LeftOuterJoin |
| `tenPhongBanNhan` | `DmDonVi` via `PhongBanNhanId` | ❌ LeftOuterJoin |
| `tenNguoiTao` | `UserMaster` via `CreatedBy` | ❌ LeftOuterJoin |
| `tenTrangThai` | Computed từ enum `ETrangThaiBanGiao` | ❌ Không phải string DB |
| `ngayBanGiao` | `BanGiaoHoSo.NgayBanGiao` (DateTimeOffset?) | ⚠️ Không phải string — xem mục 4.3 |

> **Lưu ý:** `WhereGlobalFilter` chỉ nhận `Expression<Func<T, string?>>`. Không dùng trực tiếp cho field từ join anonymous type. Cần chiến lược kết hợp — xem mục 4.

---

## Thiết kế giải pháp

### 4.1 Nguyên tắc filter

```
Kết quả = (auth filters) AND (CreatedBy) AND (trangThai?) AND (duAnId?) AND (buocId?) AND (globalFilter?)
```

Trong `globalFilter`:

```
match = Ma.Contains(k)
     OR TenHoSo.Contains(k)
     OR TenDuAn.Contains(k)
     OR TenBuoc.Contains(k)
     OR GhiChu.Contains(k)
     OR TenPhongBan.Contains(k)
     OR TenPhongBanNhan.Contains(k)
     OR TenNguoiTao.Contains(k)
     OR TenTrangThai.Contains(k)
     OR [NgayBanGiao text — nếu support được]
```

- `Contains` — không phân biệt hoa thường (theo `GlobalFilterExtensions`: trim + `ToLower()`)
- Null-safe trước khi `Contains`
- `k` = `request.SearchDto.GlobalFilter?.Trim()` — generic, không hardcode

### 4.2 Chiến lược implement (khuyến nghị)

**Bước 1 — SearchDto:** Thêm `GlobalFilter` + implement `IMayHaveGlobalFilter`

```csharp
using BuildingBlocks.CrossCutting.ExtensionMethods;

public class BanGiaoHoSoSearchDto : IMayHaveGlobalFilter {
    public int? TrangThai { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
}
```

> ASP.NET Core model binding tự map `globalFilter` query param → `GlobalFilter` property. **Không cần sửa Controller.**

**Bước 2 — Handler:** Áp dụng filter **sau join, trước Select** vì nhiều field chỉ có sau join.

**Lý do không chỉ dùng `WhereGlobalFilter` trên entity:**

- `WhereGlobalFilter` tạo một `Where` với OR nội bộ
- Nếu thêm `WhereIf` riêng cho joined fields → bị **AND** với block trước → logic sai
- Field join (`TenNguoiTao`, `TenPhongBan`, …) chỉ có sau `LeftOuterJoin`

**Cấu trúc query sau sửa:**

```csharp
var keyword = request.SearchDto.GlobalFilter?.Trim();
var keywordLower = keyword?.ToLower(CultureInfo.CurrentCulture);

var queryable = _buocAuth.FilterVisibleChildEntities(...)
    .Where(e => e.CreatedBy == _authContext.UserId.ToString())
    .WhereIf(request.SearchDto.TrangThai.HasValue, ...)
    .WhereIf(request.SearchDto.DuAnId.HasValue, ...)
    .WhereIf(request.SearchDto.BuocId.HasValue, ...)
    .LeftOuterJoin(users, ...)
    .LeftOuterJoin(donVis, ...)   // PhongBanChuTri
    .LeftOuterJoin(donVis, ...)   // PhongBanNhan
    .WhereIf(!string.IsNullOrWhiteSpace(keywordLower), x =>
        (x.e.Ma != null && x.e.Ma.ToLower().Contains(keywordLower!))
        || (x.e.TenHoSo != null && x.e.TenHoSo.ToLower().Contains(keywordLower!))
        || (x.e.DuAn != null && x.e.DuAn.TenDuAn != null && x.e.DuAn.TenDuAn.ToLower().Contains(keywordLower!))
        || (x.e.Buoc != null && x.e.Buoc.TenBuoc != null && x.e.Buoc.TenBuoc.ToLower().Contains(keywordLower!))
        || (x.e.GhiChu != null && x.e.GhiChu.ToLower().Contains(keywordLower!))
        || (x.donViChuTri != null && x.donViChuTri.TenDonVi != null && x.donViChuTri.TenDonVi.ToLower().Contains(keywordLower!))
        || (x.donViNhan != null && x.donViNhan.TenDonVi != null && x.donViNhan.TenDonVi.ToLower().Contains(keywordLower!))
        || (x.user != null && x.user.HoTen != null && x.user.HoTen.ToLower().Contains(keywordLower!))
        || TenTrangThaiMatches(x.e.TrangThai, keywordLower!)
        // || NgayBanGiaoMatches(x.e.NgayBanGiao, keywordLower!)  // optional
    )
    .OrderByDescending(x => x.e.CreatedAt)
    .Select(x => new BanGiaoHoSoDto { ... });
```

**Phương án thay thế (đơn giản hơn, cover case test):**

Chỉ dùng `WhereGlobalFilter` trên entity **trước join** cho các field entity/nav — đủ fix case `ma = 54444`:

```csharp
.WhereGlobalFilter(
    request.SearchDto,
    e => e.Ma,
    e => e.TenHoSo,
    e => e.GhiChu,
    e => e.DuAn!.TenDuAn,
    e => e.Buoc!.TenBuoc
)
```

→ Fix được bug report nhưng **không search** `tenPhongBan`, `tenNguoiTao`, `tenTrangThai`.

**Khuyến nghị triển khai:** Dùng phương án đầy đủ (sau join) để đáp ứng yêu cầu nghiệp vụ.

### 4.3 Xử lý `tenTrangThai`

Enum `ETrangThaiBanGiao`:

| Value | Text hiển thị |
|-------|---------------|
| `1` | Khởi tạo |
| `2` | Đã bàn giao |

Helper static (EF-translatable):

```csharp
private static bool TenTrangThaiMatches(ETrangThaiBanGiao trangThai, string keywordLower) =>
    (trangThai == ETrangThaiBanGiao.KhoiTao && "khởi tạo".Contains(keywordLower))
    || (trangThai == ETrangThaiBanGiao.DaBanGiao && "đã bàn giao".Contains(keywordLower));
```

> Dùng `text.Contains(keyword)` thay vì `keyword.Contains(text)` để user gõ `"bàn"` vẫn match `"Đã bàn giao"`.

### 4.4 Xử lý `ngayBanGiao` (optional)

- `WhereGlobalFilter` chỉ hỗ trợ `string?`
- Project **chưa có pattern** filter ngày dạng text trong globalFilter
- `NgayBanGiao` lưu `DateTimeOffset?`, hiển thị `DateOnly` (`dd/MM/yyyy`)

**Khuyến nghị:** Bỏ qua trong phase 1 trừ khi test yêu cầu. Nếu cần thêm:

```csharp
|| (x.e.NgayBanGiao != null
    && EF.Functions.Like(
        SqlServerDbFunctionsExtensions.DatePart("day", x.e.NgayBanGiao) ... ))  // phức tạp, dễ lỗi EF translation
```

Hoặc format string trong SQL — cần spike riêng. **Không block fix chính.**

### 4.5 Những gì KHÔNG sửa

| Thành phần | Lý do |
|------------|-------|
| Migration / EF config | Không đổi schema |
| `BanGiaoHoSoDto` | Response structure giữ nguyên |
| `BanGiaoHoSoController` | Model binding tự map `globalFilter` |
| Auth `FilterVisibleChildEntities` | Giữ nguyên |
| Filter `CreatedBy == current user` | Giữ nguyên (business rule hiện tại) |
| Phân trang `PaginatedListAsync` | Giữ nguyên — filter trước paginate |

---

## Checklist implement

### Application layer

- [ ] `BanGiaoHoSoSearchDto` — thêm `GlobalFilter`, implement `IMayHaveGlobalFilter`
- [ ] `BanGiaoHoSoGetDanhSachQuery.cs`:
  - [ ] Xóa comment "không có search full-text"
  - [ ] Thêm `WhereIf` globalFilter sau join, trước `OrderByDescending`
  - [ ] Cover đủ field: `ma`, `tenHoSo`, `tenDuAn`, `tenBuoc`, `ghiChu`, `tenPhongBan`, `tenPhongBanNhan`, `tenNguoiTao`, `tenTrangThai`
  - [ ] Null-safe trước mọi `.Contains()`
  - [ ] `ToLower()` consistent với `GlobalFilterExtensions`

### Verify

- [ ] `dotnet build` — không lỗi compile
- [ ] EF query translate được (không throw runtime `InvalidOperationException`)

---

## Test plan

### Postman — case chính (bug report)

```
GET http://192.168.1.12:9051/QuanLyDuAn/api/ban-giao-ho-so/danh-sach
  ?trangThai=2
  &pageIndex=1
  &pageSize=10
  &globalFilter=54444
  &buocId=4827
  &duAnId=08deab16-341d-3a23-687a-7b40b8010d4a
```

| # | Assertion |
|---|-----------|
| 1 | HTTP 200 |
| 2 | `totalRows = 1` |
| 3 | `data[0].ma = "54444"` |
| 4 | `data[0].id = "08deb170-f677-d039-687a-7b356802b551"` |
| 5 | Không có record `ma = 1`, `ma = 10101` |

### Postman — regression

| # | Request | Kỳ vọng |
|---|---------|---------|
| 1 | Không truyền `globalFilter` | Giữ behavior cũ — trả full list theo filter khác |
| 2 | `globalFilter=` (empty) | Giữ behavior cũ |
| 3 | Chỉ `globalFilter=thêm` | Match `tenHoSo` chứa "thêm" |
| 4 | `globalFilter=bàn giao` | Match `tenTrangThai = "Đã bàn giao"` |
| 5 | `globalFilter=xyz` không match | `totalRows = 0`, `data = []` |
| 6 | `pageSize=10`, nhiều kết quả | Phân trang đúng `totalRows` vs `data.Count` |

### SQL / EF sanity

- Filter áp dụng **trước** `Skip/Take` → `totalRows` phản ánh đúng số bản ghi sau filter
- Không load toàn bộ DB rồi filter in-memory

---

## Rủi ro & lưu ý

| Rủi ro | Mức | Giảm thiểu |
|--------|-----|------------|
| EF không translate `ToLower().Contains()` sau join | Thấp | Pattern đã dùng rộng rãi trong project |
| Performance subquery/join filter | Thấp | Dataset nhỏ per user (`CreatedBy` filter), có index trên `Ma` |
| `tenTrangThai` partial match | Thấp | Dùng `statusText.ToLower().Contains(keyword)` |
| Breaking change API contract | Không | Chỉ thêm behavior cho param đã có trên FE |

---

## Files cần sửa (ước lượng)

| File | Thay đổi |
|------|----------|
| `QLDA.Application/BanGiaoHoSos/DTOs/BanGiaoHoSoSearchDto.cs` | +`GlobalFilter`, `IMayHaveGlobalFilter` |
| `QLDA.Application/BanGiaoHoSos/Queries/BanGiaoHoSoGetDanhSachQuery.cs` | +global filter logic sau join |

**Không sửa:** Controller, DTO response, Domain, Persistence, Migration.

---

## Tham chiếu source

```
QLDA.WebApi/Controllers/BanGiaoHoSoController.cs          → endpoint danh-sach
QLDA.Application/BanGiaoHoSos/DTOs/BanGiaoHoSoSearchDto.cs
QLDA.Application/BanGiaoHoSos/Queries/BanGiaoHoSoGetDanhSachQuery.cs
QLDA.Application/BanGiaoHoSos/DTOs/BanGiaoHoSoDto.cs    → field hiển thị grid
QLDA.Domain/Entities/BanGiaoHoSo.cs
QLDA.Domain/Enums/ETrangThaiBanGiao.cs
BuildingBlocks.../GlobalFilterExtensions.cs               → pattern chuẩn
QLDA.Application/TamUngs/Queries/TamUngGetDanhSachQuery.cs → ví dụ WhereGlobalFilter đơn giản
```
