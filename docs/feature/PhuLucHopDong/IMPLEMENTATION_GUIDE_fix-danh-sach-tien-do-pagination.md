# Fix phân trang API danh sách tiến độ Phụ lục hợp đồng

**Document date:** June 26, 2026  
**Status:** ✅ **IMPLEMENTED**  
**Module:** QLDA — `PhuLucHopDong` (+ `BuildingBlocks.PaginatedList`)  
**Pattern tham chiếu:** `HopDongGetDanhSachQuery`, `VanBanPhapLyGetDanhSachQuery`, `BaoCaoTienDoGetDanhSachQuery`

**Mục lục:** [0. Trạng thái](#0-trạng-thái-hiện-tại) · [1. Triệu chứng](#1-triệu-chứng-ban-đầu) · [2. Root cause](#2-root-cause-đã-xác-minh) · [3. Khảo sát flow](#3-khảo-sát-flow) · [4. Bước code chi tiết](#4-bước-code-chi-tiết) · [5. API contract](#5-api-contract) · [6. Công thức phân trang](#6-công-thức-phân-trang) · [7. Test plan](#7-test-plan) · [8. Checklist](#8-checklist-nghiệm-thu) · [9. Commit](#9-commit-đề-xuất)

---

## 0. Trạng thái hiện tại

| Hạng mục | Trạng thái | Ghi chú |
| -------- | ---------- | ------- |
| Investigation / docs | ✅ Done | File này |
| Fix handler `PhuLucHopDongGetDanhSachQuery` | ✅ Done | `List<>` → `PaginatedList<>`, `PaginatedListAsync` |
| Fix `PaginatedList` metadata (BuildingBlocks) | ✅ Done | `pageNumber`/`totalPages` tính đúng 1-based |
| Fix callers truyền sai ctor param | ✅ Done | `BaoCaoDuAn`, `TheoDoiDeXuatNhuCauKinhPhi` |
| Controller | ✅ Không sửa | Đã map `PageIndex`/`PageSize` từ đầu |
| Search model WebApi | ✅ Không sửa | `PhuLucHopDongSearchModel` kế thừa `CommonSearchModel` |
| Migration | ✅ Không cần | |
| Integration test | ⏳ Tùy chọn | Khuyến nghị thêm case cơ bản |
| Manual verify Postman | ✅ Done | `totalRows=12` → `pageNumber=1`, `totalPages=2` |

---

## 1. Triệu chứng ban đầu

**Endpoint:**

```http
GET /QuanLyDuAn/api/phu-luc-hop-dong/danh-sach-tien-do?pageIndex=1&pageSize=10
```

**FE truyền:** `pageIndex`, `pageSize` (và các filter màn hình tiến độ).

**Lỗi 1 — shape response sai:** Handler trả `List<PhuLucHopDongDto>` → `dataResult` là **mảng phẳng** → UI không bind được → màn hình không load.

**Lỗi 2 — metadata phân trang sai (phát hiện sau fix lỗi 1):** Sau khi bọc `PaginatedList`, response có shape đúng nhưng `pageNumber=0`, `totalPages=1` dù `totalRows=12` — do bug trong `PaginatedList` constructor (gán `skip` vào `PageNumber`, `totalPages` fallback sai ở trang đầu).

---

## 2. Root cause (đã xác minh)

### 2.1 Lỗi handler Phụ lục hợp đồng

| Layer | Khai báo / kỳ vọng | Trước fix |
| ----- | ------------------- | --------- |
| Controller `[ProducesResponseType]` | `ResultApi<PaginatedList<PhuLucHopDongDto>>` | ✅ Đúng |
| Controller truyền query | `PageIndex`, `PageSize` | ✅ Đúng |
| Query record | `IRequest<PaginatedList<PhuLucHopDongDto>>` | ❌ `IRequest<List<>>` |
| Handler return | `.PaginatedListAsync(...)` | ❌ `.ToListAsync()` |

> BE nhận đủ `pageIndex`/`pageSize` nhưng **bỏ qua** khi query DB.

**Git history liên quan:**

| Commit | Thay đổi |
| ------ | -------- |
| `d933553` | Query trả `List<>` + `PaginatedListAsync` bị comment từ đầu |
| `faf8242` | Refactor buoc auth → `.ToListAsync()` (regression) |

### 2.2 Lỗi `PaginatedList` (BuildingBlocks) — ảnh hưởng toàn QLDA

Constructor cũ nhận tham số tên `pageNumber` nhưng `CreateAsync` truyền **`skip`** (offset):

```csharp
// TRƯỚC (sai)
PageNumber = pageNumber;           // skip=0 → pageNumber=0
TotalPages = pageNumber > 0 && pageSize > 0
    ? ceil(count/pageSize) : 1;    // skip=0 → totalPages luôn = 1
```

Hệ quả với `pageIndex=1`, `pageSize=10`, `totalRows=12`:

| Field | Trước fix BB | Sau fix BB |
| ----- | ------------ | ---------- |
| `pageNumber` | `0` | `1` |
| `totalPages` | `1` | `2` |
| `hasNextPage` | `true` (tình cờ) | `true` |

### 2.3 Callers truyền sai tham số constructor

Một số handler gọi `new PaginatedList<>(..., pageIndex, pageSize)` thay vì `skip`:

| File | Trước | Sau |
| ---- | ----- | --- |
| `BaoCaoDuAnGetDanhSachQuery.cs` | `search.PageIndex` | `search.Skip()` |
| `TheoDoiDeXuatNhuCauKinhPhiQuery.cs` | `pagedData.PageNumber` | `request.Skip()` |
| `PheDuyetGetDanhSachQuery.cs` | `request.Skip()` | ✅ Đã đúng từ trước |

---

## 3. Khảo sát flow

### 3.1 Controller — không đổi

**File:** `QLDA.WebApi/Controllers/PhuLucHopDongController.cs`

```csharp
[HttpGet("danh-sach-tien-do")]
[ProducesResponseType<ResultApi<PaginatedList<PhuLucHopDongDto>>>(StatusCodes.Status200OK)]
public async Task<ResultApi> Get([FromQuery] PhuLucHopDongSearchModel searchModel) {
    var res = await Mediator.Send(new PhuLucHopDongGetDanhSachQuery() {
        IsNoTracking = true,
        PageSize = searchModel.PageSize,
        PageIndex = searchModel.PageIndex,
        // ... filters ...
    });
    return ResultApi.Ok(res);
}
```

### 3.2 Search model — không đổi

**File:** `QLDA.WebApi/Models/PhuLucHopDongs/PhuLucHopDongSearchModel.cs`

`CommonSearchModel` → `AggregateRootPagination` → `PageIndex` (1-based), `PageSize`, `Skip()`, `Take()`.

### 3.3 Handler — sau fix

**File:** `QLDA.Application/PhuLucHopDongs/Queries/PhuLucHopDongGetDanhSachQuery.cs`

```csharp
public record PhuLucHopDongGetDanhSachQuery
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PhuLucHopDongDto>>, IFromDateToDate

// Handler:
return await queryable
    .Select(e => new PhuLucHopDongDto { ... })
    .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
```

**Filter giữ nguyên:** `FilterVisibleChildEntities`, `DuAnId`, `LoaiDuAnTheoNamId`, `HopDongId`, `BuocId`, `Ten`, `SoPhuLucHopDong`, `NoiDung`, `TuNgay`, `DenNgay`, `WhereGlobalFilter`.

### 3.4 Endpoint liên quan (ngoài scope)

| Endpoint | Handler return | Ghi chú |
| -------- | -------------- | ------- |
| `GET danh-sach-cbobox` | `List<PhuLucHopDongDto>` | Combobox — cố ý không paging |

---

## 4. Bước code chi tiết

Thứ tự implement: **Phase 1** (handler PLHĐ) → verify shape response → **Phase 2** (metadata `PaginatedList` + callers).

### 4.0 Tổng quan files

| # | File | Phase | Hành động |
| - | ---- | ----- | --------- |
| 1 | `QLDA.Application/PhuLucHopDongs/Queries/PhuLucHopDongGetDanhSachQuery.cs` | 1 | Sửa return type + `PaginatedListAsync` |
| 2 | `BuildingBlocks/src/BuildingBlocks.Application/Common/DTOs/PaginatedList.cs` | 2 | Sửa constructor tính `pageNumber`/`totalPages` |
| 3 | `QLDA.Application/DuAns/Queries/BaoCaoDuAnGetDanhSachQuery.cs` | 2 | ctor: `PageIndex` → `Skip()` |
| 4 | `QLDA.Application/DeXuatNhuCauKinhPhi/Queries/TheoDoiDeXuatNhuCauKinhPhiQuery.cs` | 2 | ctor: `PageNumber` → `Skip()` |

**Không sửa:** `PhuLucHopDongController.cs`, `PhuLucHopDongSearchModel.cs`, migration.

---

### 4.1 Phase 1 — `PhuLucHopDongGetDanhSachQuery.cs`

**File:** `QLDA.Application/PhuLucHopDongs/Queries/PhuLucHopDongGetDanhSachQuery.cs`

#### Bước 1.1 — Đổi generic `IRequest` trên query record

```csharp
// TRƯỚC
public record PhuLucHopDongGetDanhSachQuery
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<List<PhuLucHopDongDto>>, IFromDateToDate

// SAU
public record PhuLucHopDongGetDanhSachQuery
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PhuLucHopDongDto>>, IFromDateToDate
```

#### Bước 1.2 — Đổi handler interface + method signature

```csharp
// TRƯỚC
internal class PhuLucHopDongGetDanhSachQueryHandler
    : IRequestHandler<PhuLucHopDongGetDanhSachQuery, List<PhuLucHopDongDto>>
{
    public async Task<List<PhuLucHopDongDto>> Handle(
        PhuLucHopDongGetDanhSachQuery request,
        CancellationToken cancellationToken = default)

// SAU
internal class PhuLucHopDongGetDanhSachQueryHandler
    : IRequestHandler<PhuLucHopDongGetDanhSachQuery, PaginatedList<PhuLucHopDongDto>>
{
    public async Task<PaginatedList<PhuLucHopDongDto>> Handle(
        PhuLucHopDongGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
```

#### Bước 1.3 — Đổi dòng return cuối handler (giữ nguyên toàn bộ filter phía trên)

```csharp
// TRƯỚC
        return await queryable
            .Select(e => new PhuLucHopDongDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                Ten = e.Ten,
                SoPhuLucHopDong = e.SoPhuLucHopDong,
                NoiDung = e.NoiDung,
                Ngay = e.Ngay,
                HopDongId = e.HopDongId,
                GiaTri = e.GiaTri,
                NgayDuKienKetThuc = e.NgayDuKienKetThuc
            }).ToListAsync(cancellationToken);
        //   .PaginatedListAsync(request.Skip(), request.Take(), : cancellationToken);

// SAU
        return await queryable
            .Select(e => new PhuLucHopDongDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                Ten = e.Ten,
                SoPhuLucHopDong = e.SoPhuLucHopDong,
                NoiDung = e.NoiDung,
                Ngay = e.Ngay,
                HopDongId = e.HopDongId,
                GiaTri = e.GiaTri,
                NgayDuKienKetThuc = e.NgayDuKienKetThuc
            }).PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
```

> `PaginatedListAsync` nằm trong `QLDA.Application.Common.Mapping` (global using sẵn có).  
> `request.Skip()` = `(PageIndex - 1) * PageSize`, `request.Take()` = `PageSize`.

#### Bước 1.4 — Build + smoke test Phase 1

```bash
dotnet build QLDA.Application/QLDA.Application.csproj
```

Gọi API — kỳ vọng `dataResult` là **object** có `data[]`, chưa cần `pageNumber` đúng:

```http
GET /QuanLyDuAn/api/phu-luc-hop-dong/danh-sach-tien-do?pageIndex=1&pageSize=10
```

---

### 4.2 Phase 2 — `PaginatedList.cs` (BuildingBlocks)

**File:** `BuildingBlocks/src/BuildingBlocks.Application/Common/DTOs/PaginatedList.cs`

#### Bước 2.1 — Sửa constructor (tham số thứ 3 là `skip`, không phải `pageNumber`)

```csharp
// TRƯỚC
    public PaginatedList(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = pageNumber > 0 && pageSize > 0
            ? (int)Math.Ceiling(count / (double)pageSize)
            : 1;
        TotalRows = count;
        Data = [.. items];
    }

// SAU
    /// <param name="skip">Offset bản ghi (từ AggregateRootPagination.Skip).</param>
    /// <param name="pageSize">Số bản ghi mỗi trang.</param>
    public PaginatedList(IReadOnlyCollection<T> items, int count, int skip, int pageSize)
    {
        PageNumber = pageSize > 0 ? skip / pageSize + 1 : 1;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 1;
        TotalRows = count;
        Data = [.. items];
    }
```

#### Bước 2.2 — `CreateAsync` / `Create` — **không đổi**

Hai method vẫn truyền `(skip, take)` vào constructor — constructor mới tự derive `PageNumber`:

```csharp
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int skip, int take, ...)
    {
        int count = await source.CountAsync(cancellationToken: cancellationToken);
        if (skip >= 0 && take > 0)
            source = source.Skip(skip).Take(take);

        return new PaginatedList<T>(await source.ToListAsync(...), count, skip, take);
    }
```

#### Bước 2.3 — Build BuildingBlocks

```bash
dotnet build BuildingBlocks/src/BuildingBlocks.Application/BuildingBlocks.Application.csproj
```

---

### 4.3 Phase 2 — Sửa callers gọi constructor trực tiếp

Chỉ sửa nơi truyền **`PageIndex`** hoặc **`PageNumber`** vào ctor — phải truyền **`Skip()`** (offset).

#### Bước 3.1 — `BaoCaoDuAnGetDanhSachQuery.cs`

**File:** `QLDA.Application/DuAns/Queries/BaoCaoDuAnGetDanhSachQuery.cs`

Query đã `.Skip(search.Skip()).Take(search.Take())` — chỉ ctor cuối cần sửa:

```csharp
// TRƯỚC
        return new PaginatedList<BaoCaoDuAnDto>(
            result, totalCount,
            search.PageIndex,
            search.PageSize);

// SAU
        return new PaginatedList<BaoCaoDuAnDto>(
            result, totalCount,
            search.Skip(),
            search.PageSize);
```

#### Bước 3.2 — `TheoDoiDeXuatNhuCauKinhPhiQuery.cs`

**File:** `QLDA.Application/DeXuatNhuCauKinhPhi/Queries/TheoDoiDeXuatNhuCauKinhPhiQuery.cs`

```csharp
// TRƯỚC
        return new PaginatedList<TheoDoiDeXuatNhuCauKinhPhiDto>(
            items,
            pagedData.TotalRows,
            pagedData.PageNumber,
            request.Take());

// SAU
        return new PaginatedList<TheoDoiDeXuatNhuCauKinhPhiDto>(
            items,
            pagedData.TotalRows,
            request.Skip(),
            request.Take());
```

#### Bước 3.3 — Callers đã đúng — không sửa

`PheDuyetGetDanhSachQuery.cs`, `PheDuyetGetLichSuQuery.cs` đã truyền `request.Skip()`, `request.Take()` — giữ nguyên.

---

### 4.4 Code sau fix — tham chiếu đầy đủ

#### `PhuLucHopDongGetDanhSachQuery.cs` (handler cuối)

```csharp
public record PhuLucHopDongGetDanhSachQuery
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PhuLucHopDongDto>>, IFromDateToDate
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
    public string? Ten { get; set; }
    public string? SoPhuLucHopDong { get; set; }
    public string? NoiDung { get; set; }
    public Guid? HopDongId { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
}

internal class PhuLucHopDongGetDanhSachQueryHandler
    : IRequestHandler<PhuLucHopDongGetDanhSachQuery, PaginatedList<PhuLucHopDongDto>>
{
    // ... ctor inject: PhuLucHopDong, TepDinhKem, _duAnBuocRepo, _buocAuth, _authContext ...

    public async Task<PaginatedList<PhuLucHopDongDto>> Handle(
        PhuLucHopDongGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _buocAuth.FilterVisibleChildEntities(
                PhuLucHopDong.GetQueryableSet().AsNoTracking(),
                _duAnBuocRepo, _authContext, e => e.BuocId)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.HopDongId != null, e => e.HopDongId == request.HopDongId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.Ten.IsNotNullOrWhitespace(), e => e.Ten!.ToLower().Contains(request.Ten!.ToLower()))
            .WhereIf(request.SoPhuLucHopDong.IsNotNullOrWhitespace(),
                e => e.SoPhuLucHopDong!.ToLower().Contains(request.SoPhuLucHopDong!.ToLower()))
            .WhereIf(request.NoiDung.IsNotNullOrWhitespace(),
                e => e.NoiDung!.ToLower().Contains(request.NoiDung!.ToLower()))
            .WhereIf(request.TuNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(request,
                e => e.Ten,
                e => e.NoiDung,
                e => e.SoPhuLucHopDong,
                e => e.HopDong!.Ten);

        return await queryable
            .Select(e => new PhuLucHopDongDto
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                Ten = e.Ten,
                SoPhuLucHopDong = e.SoPhuLucHopDong,
                NoiDung = e.NoiDung,
                Ngay = e.Ngay,
                HopDongId = e.HopDongId,
                GiaTri = e.GiaTri,
                NgayDuKienKetThuc = e.NgayDuKienKetThuc
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}
```

#### `PaginatedList.cs` (constructor sau fix)

```csharp
public PaginatedList(IReadOnlyCollection<T> items, int count, int skip, int pageSize)
{
    PageNumber = pageSize > 0 ? skip / pageSize + 1 : 1;
    TotalPages = pageSize > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 1;
    TotalRows = count;
    Data = [.. items];
}
```

---

### 4.5 Build & deploy

```bash
# Stop QLDA.WebApi nếu đang chạy (tránh lock DLL)
dotnet build BuildingBlocks/src/BuildingBlocks.Application/BuildingBlocks.Application.csproj
dotnet build QLDA.Application/QLDA.Application.csproj
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```

Restart WebApi → test Postman.

### 4.6 Impact

| Symbol | Risk | Ghi chú |
| ------ | ---- | ------- |
| `PhuLucHopDongGetDanhSachQuery` | **LOW** | 1 controller endpoint |
| `PaginatedList<T>` constructor | **MEDIUM** | Toàn bộ API paging QLDA — metadata đúng hơn |

---

## 5. API contract

### 5.1 Request

```http
GET /api/phu-luc-hop-dong/danh-sach-tien-do
```

| Param | Type | Mô tả |
| ----- | ---- | ----- |
| `pageIndex` | int | Trang (1-based) |
| `pageSize` | int | Số bản ghi/trang (max 100) |
| `duAnId` | Guid? | Lọc theo dự án |
| `buocId` | int? | Lọc theo bước |
| `globalFilter` | string? | Tìm nhanh |
| `ten` | string? | Tên phụ lục |
| `soPhuLucHopDong` | string? | Số phụ lục |
| `noiDung` | string? | Nội dung |
| `hopDongId` | Guid? | Hợp đồng |
| `tuNgay` | DateOnly? | Từ ngày |
| `denNgay` | DateOnly? | Đến ngày |
| `loaiDuAnTheoNamId` | int? | Loại DA theo năm (#9609) |

### 5.2 Response (sau fix đầy đủ)

```json
{
  "result": true,
  "errorMessage": "",
  "dataResult": {
    "totalRows": 12,
    "pageNumber": 1,
    "totalPages": 2,
    "hasPreviousPage": false,
    "hasNextPage": true,
    "data": [ /* tối đa pageSize items */ ]
  }
}
```

### 5.3 Quy tắc

- Filter áp dụng **trước** `Skip/Take` → `totalRows` = số bản ghi sau filter + auth.
- `data.Count` ≤ `pageSize` (trừ trang cuối).
- Trang cuối: `hasNextPage = false`, `pageNumber = totalPages`.

---

## 6. Công thức phân trang

Dựa trên `AggregateRootPagination` + `PaginatedList` sau fix:

```text
skip       = (pageIndex - 1) * pageSize
pageNumber = skip / pageSize + 1        (1-based)
totalPages = ceil(totalRows / pageSize)
```

| `pageIndex` | `pageSize` | `totalRows` | `pageNumber` | `totalPages` | `data.length` (trang 1) |
| ----------- | ---------- | ----------- | ------------ | ------------ | ----------------------- |
| 1 | 10 | 5 | 1 | 1 | 5 |
| 1 | 10 | 12 | 1 | 2 | 10 |
| 1 | 10 | 29 | 1 | 3 | 10 |
| 2 | 10 | 29 | 2 | 3 | 10 |
| 3 | 10 | 29 | 3 | 3 | 9 |
| 1 | 10 | 0 | 1 | 0 | 0 |

**FE note:** Có thể bind pager từ `totalRows` + `totalPages` + `pageNumber` — metadata đã nhất quán trên mọi API dùng `PaginatedListAsync`.

---

## 7. Test plan

### 7.1 Manual — Phụ lục hợp đồng

| # | Case | Kỳ vọng |
| - | ---- | ------- |
| T1 | `pageIndex=1&pageSize=10` | `dataResult` là object, không phải array |
| T2 | `totalRows=12` | `pageNumber=1`, `totalPages=2`, `data.Count=10`, `hasNextPage=true` |
| T3 | `totalRows=29`, trang 1 | `pageNumber=1`, `totalPages=3` |
| T4 | `pageIndex=2&pageSize=10` | `pageNumber=2`, `hasPreviousPage=true` |
| T5 | `globalFilter` không match | `totalRows=0`, `data=[]` |
| T6 | User không quyền bước | Chỉ PLHĐ visible qua `FilterVisibleChildEntities` |

### 7.2 Regression — API paging khác

Spot-check 1–2 endpoint dùng `PaginatedListAsync` (vd. `hop-dong/danh-sach-tien-do`) — `pageNumber` phải là **1** ở trang đầu, không còn **0**.

### 7.3 Build

```bash
dotnet build BuildingBlocks/src/BuildingBlocks.Application/BuildingBlocks.Application.csproj
dotnet build QLDA.Application/QLDA.Application.csproj
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```

> **Lưu ý:** Stop `QLDA.WebApi` trước khi build nếu process đang lock DLL.

---

## 8. Checklist nghiệm thu

- [x] Handler trả `PaginatedList<PhuLucHopDongDto>`, dùng `PaginatedListAsync`
- [x] `PaginatedList` tính `pageNumber` 1-based, `totalPages = ceil(totalRows/pageSize)`
- [x] Callers ctor dùng `Skip()` thay vì `PageIndex` / `PageNumber` cũ
- [x] Controller / SearchModel không đổi
- [x] Không migration
- [x] Postman verify: `totalRows=12` → `pageNumber=1`, `totalPages=2`
- [ ] Integration test (tùy chọn)
- [ ] `detect_changes()` trước commit

---

## 9. Commit đề xuất

Có thể gộp 1 commit hoặc tách 2:

**Option A — 1 commit:**

```
fix(phu-luc-hop-dong): restore pagination and fix PaginatedList metadata

- PhuLucHopDongGetDanhSachQuery returns PaginatedList via PaginatedListAsync
- PaginatedList computes pageNumber (1-based) and totalPages from skip/pageSize
- Fix BaoCaoDuAn and TheoDoiDeXuatNhuCauKinhPhi ctor callers
```

**Option B — 2 commits:**

```
fix(phu-luc-hop-dong): restore pagination for danh-sach-tien-do API
```

```
fix(building-blocks): correct PaginatedList pageNumber and totalPages
```

**Files changed:**

| File | Phase |
| ---- | ----- |
| `QLDA.Application/PhuLucHopDongs/Queries/PhuLucHopDongGetDanhSachQuery.cs` | 1 |
| `BuildingBlocks/.../PaginatedList.cs` | 2 |
| `QLDA.Application/DuAns/Queries/BaoCaoDuAnGetDanhSachQuery.cs` | 2 |
| `QLDA.Application/DeXuatNhuCauKinhPhi/Queries/TheoDoiDeXuatNhuCauKinhPhiQuery.cs` | 2 |

---

**Verified:** Restart WebApi sau build → `GET .../danh-sach-tien-do?pageIndex=1&pageSize=10` trả đúng shape + metadata.
