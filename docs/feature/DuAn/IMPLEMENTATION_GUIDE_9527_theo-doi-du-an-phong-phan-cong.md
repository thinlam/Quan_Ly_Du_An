# PMIS #9527 — UC Theo dõi dự án theo phòng được phân công (#113)

**Document date:** June 26, 2026  
**Status:** ✅ **IMPLEMENTED** (June 26, 2026)  
**Module:** QLDA — `DuAn`  
**Pattern tham chiếu:** `DuAnGetDanhSachQuery`, `DuAnGetDanhSachExportQuery`, `DuAnGetTheoPhongBanGetQuery`, `TheoDoiDeXuatNhuCauKinhPhiQuery`

**Mục lục:** [0. Trạng thái](#0-trạng-thái-hiện-tại) · [1. Tóm tắt BA](#1-tóm-tắt-nghiệp-vụ) · [2. Khảo sát source](#2-khảo-sát-source-đã-xác-minh) · [3. Kiến trúc](#3-kiến-trúc-đề-xuất) · [4. API contract](#4-api-contract) · [5. Logic nghiệp vụ](#5-logic-nghiệp-vụ-chi-tiết) · [6. Files cần tạo/sửa](#6-files-cần-tạosửa) · [7. Test plan](#7-test-plan) · [8. Checklist](#8-checklist-nghiệm-thu) · [9. Commit](#9-thứ-tự-commit-đề-xuất)

---

## 0. Trạng thái hiện tại

| Hạng mục | Trạng thái | Ghi chú |
| -------- | ---------- | ------- |
| Implementation guide | ✅ Done | File này |
| API theo dõi + 4 panel | ✅ Done | `GET /api/du-an/theo-doi-du-an-phong-phan-cong` |
| Query handler Application | ✅ Done | `TheoDoiDuAnPhongPhanCongQuery` |
| DTO / Search / Enum | ✅ Done | |
| Controller endpoint | ✅ Done | `DuAnController` |
| Integration test | ⏳ Chưa làm | Tùy chọn, khuyến nghị |
| Migration | ✅ Không cần | Dùng field `DuAn` + `DmTrangThaiDuAn` có sẵn |

---

## 1. Tóm tắt nghiệp vụ

### 1.1 Mô tả màn hình

Màn **Danh sách dự án** theo phòng/đơn vị **phụ trách chính** (`DonViPhuTrachChinhId`), gồm:

1. **4 panel thống kê** phía trên: Tổng số · Còn hạn · Quá hạn · Đã hoàn thành  
2. **Bảng danh sách** phía dưới, lọc theo panel user chọn + filter màn hình (phòng, năm, từ khóa, phân trang)

> **Phạm vi issue:** UI chỉ hiển thị **dự án** (`DuAn`). Phần “hạng mục” trong title issue **không có trên mockup** — tạm **out of scope** cho backend lần này. Nếu BA cần `HangMucKeHoach` → issue riêng.

### 1.2 Quy tắc phân loại (đã chốt từ BA)

| Panel | Điều kiện |
| ----- | --------- |
| **Tổng số** | Toàn bộ dự án thuộc phòng phụ trách (sau filter màn hình) |
| **Còn hạn** | `Năm hiện tại ≤ ThoiGianHoanThanh` **và** trạng thái dự án **≠** Đã hoàn thành |
| **Quá hạn** | `Năm hiện tại > ThoiGianHoanThanh` **và** trạng thái dự án **≠** Đã hoàn thành |
| **Đã hoàn thành** | Trạng thái dự án = **Đã hoàn thành** |

**Ràng buộc bắt buộc:**

```text
Tổng số = Còn hạn + Quá hạn + Đã hoàn thành
```

- Dự án **Đã hoàn thành** không được rơi vào Còn hạn / Quá hạn.  
- Dự án **chưa hoàn thành** + năm hoàn thành **<** năm hiện tại → **Quá hạn**.  
- Dự án **chưa hoàn thành** + năm hoàn thành **≥** năm hiện tại → **Còn hạn**.

### 1.3 Mapping UI → cột bảng

| # | Cột UI | Nguồn dữ liệu |
| - | ------ | -------------- |
| 1 | STT | Tính server-side: `(PageIndex - 1) * PageSize + index + 1` |
| 2 | Mã dự án | `DuAn.MaDuAn` |
| 3 | Tên dự án | `DuAn.TenDuAn` |
| 4 | Thời gian thực hiện | Hiển thị `"{ThoiGianKhoiCong} - {ThoiGianHoanThanh}"` (năm) |
| 5 | Hình thức quản lý dự án | `DanhMucHinhThucQuanLy.Ten` (nav EF) |
| 6 | Hình thức đầu tư | `DanhMucHinhThucDauTu.Ten` (nav EF) |
| 7 | Tổng mức đầu tư | `DuAn.TongMucDauTu` |
| 8 | Lãnh đạo phụ trách | `UserMaster.HoTen` qua `LanhDaoPhuTrachId` |
| 9 | Đơn vị/phòng phụ trách | `DmDonVi.TenDonVi` qua `DonViPhuTrachChinhId` |
| — | (ẩn) Id | `DuAn.Id` — cho menu action FE |

---

## 2. Khảo sát source (đã xác minh)

### 2.1 Entity & field liên quan

```text
DuAn
├── MaDuAn, TenDuAn
├── ThoiGianKhoiCong      int?   ← năm khởi công (dự kiến)
├── ThoiGianHoanThanh     int?   ← năm hoàn thành (dùng cho Còn hạn / Quá hạn)
├── TongMucDauTu          long?
├── HinhThucQuanLyDuAnId  int?   → DanhMucHinhThucQuanLy
├── HinhThucDauTuId       int?   → DanhMucHinhThucDauTu
├── LanhDaoPhuTrachId     long?  → UserMaster (legacy, join in-memory)
├── DonViPhuTrachChinhId  long?  → DmDonVi (legacy, join in-memory)
└── TrangThaiDuAnId       int?   → DanhMucTrangThaiDuAn
```

**Trạng thái dự án (seed):**

| Id | Ma | Ten |
| -- | -- | --- |
| 1 | `DTH` | Đang thực hiện |
| 2 | `PDDT` | Đã phê duyệt đầu tư |
| 3 | `HT` | **Đã hoàn thành** |
| 4 | `TD` | Tạm dừng |

**Constant tái sử dụng:** `QLDA.Domain.Constants.DanhMucTrangThaiDuAnCodes.HoanThanh` (`"HT"`).

**File tham chiếu:**

| Thành phần | Vị trí |
| ---------- | ------ |
| Entity | `QLDA.Domain/Entities/DuAn.cs` |
| Mã trạng thái | `QLDA.Domain/Constants/DanhMucTrangThaiDuAnCodes.cs` |
| Seed | `QLDA.Persistence/Configurations/DanhMuc/DanhMucTrangThaiDuAnConfiguration.cs` |

### 2.2 API / Query hiện có (gần nhất)

| API | Route | Handler | Ghi chú |
| --- | ----- | ------- | ------- |
| Danh sách dự án | `GET /api/du-an/danh-sach` | `DuAnGetDanhSachQuery` | ✅ `FilterVisible`, filter `DonViPhuTrachChinhId`, `NamDuAn`, `GlobalFilter` |
| Export danh sách | `GET /api/print/...` | `DuAnGetDanhSachExportQuery` | ✅ Join tên LĐ / đơn vị / hình thức — **pattern map cột UI** |
| Theo phòng ban (cũ) | `GET /api/du-an/danh-sach-theo-phong-ban` | `DuAnGetTheoPhongBanGetQuery` | ⚠️ Chỉ lấy phòng user, **loại HT**, không panel, **không** `FilterVisible` |
| Trễ hạn bước | `GET /api/du-an/danh-sach-tre-han` | `DuAnGetDanhSachTreHanQuery` | Logic trễ hạn **theo DuAnBuoc** — **khác** UC này |

### 2.3 Phân quyền (bắt buộc)

Theo `CLAUDE.md` + `DuAnAuthorizationProvider`:

```csharp
// ✅ ĐÚNG — FilterVisible ngay đầu query
var queryable = _authManager.FilterVisible(DuAn.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
    .WhereIf(search.DonViPhuTrachChinhId > 0, e => e.DonViPhuTrachChinhId == search.DonViPhuTrachChinhId)
    ...
```

- **Không** copy `DuAnGetTheoPhongBanGetQuery` nguyên xi (`GetOriginalSet`, không auth).  
- Filter phòng: `DonViPhuTrachChinhId` từ request (FE chọn phòng trên màn hình).  
- User chỉ thấy dự án trong phạm vi ownership sau `FilterVisible` (xem issue `9584`).

### 2.4 Filter năm / từ khóa — tái sử dụng

Từ `DuAnSearchDto` / `DuAnGetDanhSachQuery`:

| Param | Logic |
| ----- | ----- |
| `NamDuAn` | `NamDuAn >= ThoiGianKhoiCong` AND (`ThoiGianHoanThanh == null && ThoiGianKhoiCong == NamDuAn` OR `NamDuAn <= ThoiGianHoanThanh`) |
| `GlobalFilter` | `WhereGlobalFilter` trên `TenDuAn` (và có thể thêm `MaDuAn`) |
| `TenDuAn`, `MaDuAn` | Contains (giống list hiện tại) |
| `PageIndex`, `PageSize` | `AggregateRootPagination` — `PageIndex` **1-based** (`Skip = (PageIndex - 1) * PageSize`) |

---

## 3. Kiến trúc đề xuất

### 3.1 Luồng xử lý

```text
GET /api/du-an/theo-doi-du-an-phong-phan-cong
        │
        ▼
DuAnController (mỏng — Mediator.Send only)
        │
        ▼
TheoDoiDuAnPhongPhanCongQuery + Handler
        │
        ├─ 1. Resolve trangThaiHoanThanhId (Ma = HT)
        ├─ 2. BuildBaseQuery(search)     ← auth + filter màn hình (KHÔNG filter Loai)
        ├─ 3. Count ConHan / QuaHan / DaHoanThanh trên cùng base query
        ├─ 4. TongSoDuAn = sum 3 count
        ├─ 5. ApplyLoaiFilter(base, Loai) → paginate → map DTO
        └─ 6. Return TheoDoiDuAnPhongPhanCongResultDto
```

### 3.2 Nguyên tắc tránh lệch số liệu

| Quy tắc | Cách làm |
| ------- | -------- |
| Một nguồn filter | Private method `BuildBaseQuery(IQueryable<DuAn>, SearchDto, int trangThaiHoanThanhId)` dùng chung cho **count** và **list** |
| Một định nghĩa loại | Private method `ApplyTheoDoiLoaiFilter(query, loai, namHienTai, trangThaiHoanThanhId)` |
| Không duplicate string | Dùng `DanhMucTrangThaiDuAnCodes.HoanThanh`, không hard-code `"Đã hoàn thành"` |
| Năm hiện tại | `IDateTimeProvider` (inject) — `NamHienTai = dateTimeProvider.OffsetUtcNow.Year` (hoặc timezone VN nếu provider hỗ trợ) |

### 3.3 Pseudocode handler

```csharp
public async Task<TheoDoiDuAnPhongPhanCongResultDto> Handle(...)
{
    var hoanThanhId = await GetTrangThaiIdByMaAsync(DanhMucTrangThaiDuAnCodes.HoanThanh, ct);
    var namHienTai = _dateTimeProvider.OffsetUtcNow.Year;

    var baseQuery = BuildBaseQuery(
        _authManager.FilterVisible(_duAn.GetQueryableSet(), AuthorizationResourceKeys.DuAn),
        request.SearchDto);

    // Count — 1 round-trip (conditional aggregation)
    var stats = await baseQuery
        .Select(e => new {
            IsHoanThanh = e.TrangThaiDuAnId == hoanThanhId,
            e.ThoiGianHoanThanh,
        })
        .GroupBy(_ => 1)
        .Select(g => new {
            DaHoanThanh = g.Count(x => x.IsHoanThanh),
            ConHan = g.Count(x => !x.IsHoanThanh && ClassifyConHan(x.ThoiGianHoanThanh, namHienTai)),
            QuaHan = g.Count(x => !x.IsHoanThanh && ClassifyQuaHan(x.ThoiGianThanh, namHienTai)),
        })
        .FirstOrDefaultAsync(ct) ?? new { DaHoanThanh = 0, ConHan = 0, QuaHan = 0 };

    var tongSo = stats.ConHan + stats.QuaHan + stats.DaHoanThanh;

    var listQuery = ApplyLoaiFilter(baseQuery, request.SearchDto.Loai, namHienTai, hoanThanhId);

    var paged = await listQuery
        .OrderBy(e => e.TenDuAn)
        .Select(/* project ids + nav Ten */)
        .PaginatedListAsync(request.SearchDto.Skip(), request.SearchDto.Take(), ct);

    // Resolve UserMaster / DmDonVi names (pattern DuAnGetDanhSachExportQuery)
    var items = MapToDto(paged.Data, request.SearchDto);

    return new TheoDoiDuAnPhongPhanCongResultDto {
        TongSoDuAn = tongSo,
        ConHan = stats.ConHan,
        QuaHan = stats.QuaHan,
        DaHoanThanh = stats.DaHoanThanh,
        DanhSach = new PaginatedList<TheoDoiDuAnPhongPhanCongDto>(items, paged.TotalRows, paged.PageNumber, pageSize),
    };
}
```

> **Lưu ý EF:** Nếu `ClassifyConHan/QuaHan` không dịch được SQL, inline điều kiện trong `.Count(...)` như pseudocode trên.

---

## 4. API contract

### 4.1 Endpoint

```http
GET /api/du-an/theo-doi-du-an-phong-phan-cong
```

| | |
| - | - |
| Controller | `DuAnController` |
| Auth | Mặc định `[Authorize]` của project (giống `danh-sach`) |
| Response wrapper | `ResultApi<TheoDoiDuAnPhongPhanCongResultDto>` |

### 4.2 Request — `TheoDoiDuAnPhongPhanCongSearchDto`

Kế thừa `CommonSearchDto` (pagination + `GlobalFilter`).

```csharp
namespace QLDA.Application.DuAns.DTOs;

public record TheoDoiDuAnPhongPhanCongSearchDto : CommonSearchDto
{
    /// <summary>Đơn vị/phòng phụ trách chính — bắt buộc khi màn hình chọn phòng</summary>
    public long? DonViPhuTrachChinhId { get; set; }

    /// <summary>Filter năm dự án — tái dùng logic NamDuAn (#9121)</summary>
    public int? NamDuAn { get; set; }

  /// <summary>Tên dự án (contains)</summary>
    public string? TenDuAn { get; set; }

    /// <summary>Mã dự án (contains)</summary>
    public string? MaDuAn { get; set; }

    /// <summary>Panel/tab đang chọn</summary>
    public ETheoDoiDuAnPhongPhanCongLoai Loai { get; set; } = ETheoDoiDuAnPhongPhanCongLoai.TatCa;
}
```

**Query string ví dụ:**

```http
GET /api/du-an/theo-doi-du-an-phong-phan-cong
  ?donViPhuTrachChinhId=123
  &namDuAn=2026
  &globalFilter=phần mềm
  &loai=ConHan
  &pageIndex=1
  &pageSize=10
```

### 4.3 Enum — `ETheoDoiDuAnPhongPhanCongLoai`

```csharp
namespace QLDA.Domain.Enums;

public enum ETheoDoiDuAnPhongPhanCongLoai
{
    TatCa = 0,
    ConHan = 1,
    QuaHan = 2,
    DaHoanThanh = 3,
}
```

### 4.4 Response

```csharp
public class TheoDoiDuAnPhongPhanCongResultDto
{
    public int TongSoDuAn { get; set; }
    public int ConHan { get; set; }
    public int QuaHan { get; set; }
    public int DaHoanThanh { get; set; }

    public PaginatedList<TheoDoiDuAnPhongPhanCongDto> DanhSach { get; set; } = new();
}

public class TheoDoiDuAnPhongPhanCongDto
{
    public int Stt { get; set; }
    public Guid Id { get; set; }
    public string? MaDuAn { get; set; }
    public string? TenDuAn { get; set; }
    /// <summary>VD: "2026 - 2027"</summary>
    public string? ThoiGianThucHien { get; set; }
    public string? HinhThucQuanLyDuAn { get; set; }
    public string? HinhThucDauTu { get; set; }
    public long? TongMucDauTu { get; set; }
    public string? LanhDaoPhuTrach { get; set; }
    public string? DonViPhuTrachChinh { get; set; }

    // Optional — FE resolve catalog nếu cần
    public int? HinhThucQuanLyDuAnId { get; set; }
    public int? HinhThucDauTuId { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public int? ThoiGianHoanThanh { get; set; }
    public int? TrangThaiDuAnId { get; set; }
}
```

**JSON response mẫu (rút gọn):**

```json
{
  "success": true,
  "data": {
    "tongSoDuAn": 28,
    "conHan": 12,
    "quaHan": 12,
    "daHoanThanh": 4,
    "danhSach": {
      "totalRows": 12,
      "pageNumber": 1,
      "totalPages": 2,
      "data": [
        {
          "stt": 1,
          "id": "...",
          "maDuAn": "04/2026-DA",
          "tenDuAn": "Mua sắm bản quyền phần mềm...",
          "thoiGianThucHien": "2026 - 2027",
          "hinhThucQuanLyDuAn": "Chủ đầu tư thuê tổ chức tư vấn quản lý dự án",
          "hinhThucDauTu": "Thuê dịch vụ công nghệ thông tin...",
          "tongMucDauTu": 5089000000,
          "lanhDaoPhuTrach": "Võ Thị Trung Trinh",
          "donViPhuTrachChinh": "Phòng Hạ tầng số và An toàn thông tin"
        }
      ]
    }
  }
}
```

> **FE note:** 4 số panel **luôn trả về** (tính trên full base query). `danhSach.totalRows` thay đổi theo `loai` đang chọn.

---

## 5. Logic nghiệp vụ chi tiết

### 5.1 `BuildBaseQuery` — filter chung (không gồm `Loai`)

Thứ tự áp dụng:

1. `_authManager.FilterVisible(..., AuthorizationResourceKeys.DuAn)`
2. `.AsNoTracking()`
3. `DonViPhuTrachChinhId` (khi `> 0`)
4. `NamDuAn` — copy từ `DuAnGetDanhSachQuery` dòng 65–66
5. `TenDuAn`, `MaDuAn` — contains
6. `WhereGlobalFilter(search, e => e.TenDuAn)` — có thể mở rộng `e => e.MaDuAn`

### 5.2 Phân loại Còn hạn / Quá hạn / Đã hoàn thành

Giả sử `hoanThanhId` = id của `Ma = HT`, `nam` = năm hiện tại:

```text
Đã hoàn thành:
  TrangThaiDuAnId == hoanThanhId

Còn hạn (chưa hoàn thành):
  TrangThaiDuAnId != hoanThanhId
  AND (
        ThoiGianHoanThanh == null          ← xem mục 5.4
        OR nam <= ThoiGianHoanThanh
      )

Quá hạn (chưa hoàn thành):
  TrangThaiDuAnId != hoanThanhId
  AND ThoiGianHoanThanh != null
  AND nam > ThoiGianHoanThanh
```

**Filter list theo `Loai`:**

| Loai | Filter thêm |
| ---- | ----------- |
| `TatCa` | Không thêm |
| `ConHan` | Điều kiện Còn hạn |
| `QuaHan` | Điều kiện Quá hạn |
| `DaHoanThanh` | `TrangThaiDuAnId == hoanThanhId` |

### 5.3 Công thức tổng

```csharp
TongSoDuAn = ConHan + QuaHan + DaHoanThanh;
// Không dùng Count(*) riêng nếu có thể tránh — tránh lệch do định nghĩa khác nhau
```

### 5.4 Edge case — `ThoiGianHoanThanh` null

| Tình huống | Đề xuất kỹ thuật | Lý do |
| ---------- | ---------------- | ----- |
| Chưa HT, `ThoiGianHoanThanh == null` | **Còn hạn** | Không có hạn để “quá”; đảm bảo mọi dự án thuộc đúng 1 trong 3 bucket → tổng khớp |
| Đã HT | **Đã hoàn thành** | Ưu tiên trạng thái, bỏ qua năm |

> Nếu BA muốn xử lý khác (vd. loại khỏi Còn hạn/Quá hạn) → cập nhật doc trước khi code.

### 5.5 Format `ThoiGianThucHien`

```csharp
static string? FormatThoiGianThucHien(int? tu, int? den)
{
    if (tu is null && den is null) return null;
    if (tu is not null && den is not null) return $"{tu} - {den}";
    return (tu ?? den)?.ToString();
}
```

### 5.6 Map tên Lãnh đạo / Đơn vị

Theo `DuAnGetDanhSachExportQuery` (dòng 100–121):

- Batch load `UserMaster` / `DmDonVi` sau paginate (tránh N+1).  
- **Không** tạo navigation EF tới legacy tables.

---

## 6. Files cần tạo/sửa

### 6.1 Tạo mới

| File | Mô tả |
| ---- | ----- |
| `QLDA.Domain/Enums/ETheoDoiDuAnPhongPhanCongLoai.cs` | Enum panel |
| `QLDA.Application/DuAns/DTOs/TheoDoiDuAnPhongPhanCongSearchDto.cs` | Request |
| `QLDA.Application/DuAns/DTOs/TheoDoiDuAnPhongPhanCongDto.cs` | Dòng list |
| `QLDA.Application/DuAns/DTOs/TheoDoiDuAnPhongPhanCongResultDto.cs` | Response wrapper |
| `QLDA.Application/DuAns/Queries/TheoDoiDuAnPhongPhanCongQuery.cs` | Query + Handler |

### 6.2 Sửa

| File | Thay đổi |
| ---- | -------- |
| `QLDA.WebApi/Controllers/DuAnController.cs` | Thêm `GET theo-doi-du-an-phong-phan-cong` — **chỉ** `Mediator.Send` |

### 6.3 Không tạo

| ❌ | Lý do |
| -- | ----- |
| Model trong `QLDA.WebApi/Models` | DTO đặt Application (`CLAUDE.md`) |
| Migration | Không đổi schema |
| Sửa `DuAnGetTheoPhongBanGetQuery` | API cũ giữ nguyên; UC mới là endpoint riêng |

### 6.4 Controller mẫu (mỏng)

```csharp
[HttpGet("api/du-an/theo-doi-du-an-phong-phan-cong")]
[ProducesResponseType<ResultApi<TheoDoiDuAnPhongPhanCongResultDto>>(StatusCodes.Status200OK)]
public async Task<ResultApi> GetTheoDoiDuAnPhongPhanCong(
    [FromQuery] TheoDoiDuAnPhongPhanCongSearchDto searchDto)
{
    var res = await Mediator.Send(new TheoDoiDuAnPhongPhanCongQuery(searchDto));
    return ResultApi.Ok(res);
}
```

### 6.5 Test (khuyến nghị)

| File | Nội dung |
| ---- | -------- |
| `QLDA.Tests/.../TheoDoiDuAnPhongPhanCongQueryTests.cs` | Unit/integration: 4 case phân loại + tổng khớp |

---

## 7. Test plan

### 7.1 Case phân loại

| # | Trạng thái | ThoiGianHoanThanh | Năm HT | Bucket |
| - | ---------- | ----------------- | ------ | ------ |
| T1 | HT | 2024 | 2026 | DaHoanThanh |
| T2 | DTH | 2027 | 2026 | ConHan |
| T3 | DTH | 2024 | 2026 | QuaHan |
| T4 | DTH | null | 2026 | ConHan (edge) |
| T5 | TD | 2024 | 2026 | QuaHan (chưa HT) |

### 7.2 Case tổng & panel

| # | Kịch bản | Kỳ vọng |
| - | -------- | ------- |
| S1 | 12 ConHan + 12 QuaHan + 4 HT | `tongSoDuAn = 28` |
| S2 | `loai=QuaHan` | `danhSach.totalRows = 12`, không có HT |
| S3 | Đổi `loai` giữa các request | 4 panel **không đổi**; chỉ list đổi |
| S4 | `donViPhuTrachChinhId` khác phòng | Chỉ dự án phòng đó (trong phạm vi auth) |
| S5 | `namDuAn=2026` | Chỉ dự án có khoảng năm chứa 2026 |

### 7.3 Case auth

| # | User | Kỳ vọng |
| - | ---- | ------- |
| A1 | Chuyên viên phòng A | Chỉ dự án phòng A được phân công / ownership |
| A2 | KH-TC bypass | Thấy theo filter, không bị ownership cắt |

### 7.4 Build

```bash
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```

---

## 8. Checklist nghiệm thu

- [x] API `GET /api/du-an/theo-doi-du-an-phong-phan-cong` hoạt động
- [x] Trả đủ 4 counter + `danhSach` phân trang
- [x] `tongSoDuAn == conHan + quaHan + daHoanThanh` mọi lần gọi
- [x] HT không nằm trong ConHan/QuaHan
- [x] `loai` lọc đúng list; counter không đổi theo `loai`
- [x] `FilterVisible` ở đầu query
- [x] Không model WebApi mới; không logic nghiệp vụ trong Controller
- [x] Dùng `DanhMucTrangThaiDuAnCodes.HoanThanh`, không magic string
- [x] Build pass, không warning mới liên quan

---

## 9. Thứ tự commit đề xuất

```bash
git commit -m "feat: add assigned department project tracking api

- Add summary counters for total, active, overdue and completed projects
- Add filtered project list by tracking status
- Reuse application query handler for business rules"
```

**Nhóm file trong 1 commit** (chỉ Application + WebApi controller — không migration):

1. `QLDA.Domain/Enums/ETheoDoiDuAnPhongPhanCongLoai.cs`
2. `QLDA.Application/DuAns/DTOs/TheoDoiDuAnPhongPhanCong*.cs`
3. `QLDA.Application/DuAns/Queries/TheoDoiDuAnPhongPhanCongQuery.cs`
4. `QLDA.WebApi/Controllers/DuAnController.cs`

---

## Phụ lục A — So sánh API cũ vs mới

| | `danh-sach-theo-phong-ban` (cũ) | `theo-doi-du-an-phong-phan-cong` (mới) |
| - | ------------------------------- | -------------------------------------- |
| Phòng | Cố định `user.PhongBanID` | Filter `DonViPhuTrachChinhId` |
| Hoàn thành | Loại trừ HT | Có panel HT |
| Panel thống kê | Không | 4 counter |
| Auth | `GetOriginalSet`, không FilterVisible | `FilterVisible` |
| Response | `PaginatedList<DuAn>` entity | DTO + summary |

## Phụ lục B — Câu hỏi mở (confirm với BA nếu cần)

1. **Hạng mục:** Có cần list `HangMucKeHoach` trên cùng màn không?  
2. **ThoiGianHoanThanh null:** Chốt gom **Còn hạn** như mục 5.4?  
3. **Default phòng:** Khi FE không gửi `donViPhuTrachChinhId`, có fallback `user.PhongBanID` không? (đề xuất: **có**, giống UX phòng mặc định)

---

*Document generated from codebase survey — June 26, 2026.*
