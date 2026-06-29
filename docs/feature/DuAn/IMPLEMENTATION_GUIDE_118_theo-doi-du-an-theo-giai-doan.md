# PMIS #118 — Thống kê Theo dõi dự án/hạng mục theo từng giai đoạn

**Document date:** June 29, 2026  
**Status:** ✅ **IMPLEMENTED** (June 29, 2026)  
**Module:** QLDA — `DuAn`  
**Pattern tham chiếu:** `TheoDoiDuAnPhongPhanCongQuery` (#9527), `DuAnGetDanhSachQuery` (filter `GiaiDoanId`), `DashboardGetTheoGiaiDoanQuery`

**Mục lục:** [0. Trạng thái](#0-trạng-thái-hiện-tại) · [1. Tóm tắt BA](#1-tóm-tắt-nghiệp-vụ) · [2. Khảo sát source](#2-khảo-sát-source-đã-xác-minh) · [3. So sánh API phòng ban](#3-so-sánh-với-api-phòng-ban-9527) · [4. Kiến trúc](#4-kiến-trúc-đề-xuất) · [5. API contract](#5-api-contract) · [6. Logic nghiệp vụ](#6-logic-nghiệp-vụ-chi-tiết) · [7. Files cần tạo/sửa](#7-files-cần-tạosửa) · [8. Test plan](#8-test-plan) · [9. Checklist](#9-checklist-nghiệm-thu) · [10. Commit](#10-thứ-tự-commit-đề-xuất)

---

## 0. Trạng thái hiện tại

| Hạng mục | Trạng thái | Ghi chú |
| -------- | ---------- | ------- |
| Implementation guide | ✅ Done | File này |
| API theo dõi + 4 panel | ✅ Done | `GET /api/du-an/theo-doi-du-an-theo-giai-doan` |
| Query handler Application | ✅ Done | `TheoDoiDuAnTheoGiaiDoanQuery` |
| DTO / Search / Enum | ✅ Done | |
| Controller endpoint | ✅ Done | `DuAnController` |
| Integration test | ⏳ Chưa làm | Tùy chọn, khuyến nghị |
| Migration | ✅ Không cần | Dùng field `DuAn` + `DmGiaiDoan` + `DmTrangThaiDuAn` có sẵn |

---

## 1. Tóm tắt nghiệp vụ

### 1.1 Mô tả màn hình

Màn **Thống kê theo dõi dự án/hạng mục theo từng giai đoạn**, gồm:

1. **Chọn giai đoạn** (`GiaiDoanId`) — filter chính của màn hình  
2. **4 panel thống kê** phía trên: Tổng số · Còn hạn · Quá hạn · Đã hoàn thành  
3. **Bảng danh sách** phía dưới, lọc theo panel user chọn + filter màn hình (giai đoạn, năm, từ khóa, phân trang)

> **Khác biệt so với #9527 (theo phòng ban):**  
> Filter/group theo **giai đoạn** thay vì `DonViPhuTrachChinhId`.

### 1.2 Phạm vi dữ liệu — dự án vs hạng mục

| Entity | Field giai đoạn | Ghi chú |
| ------ | --------------- | ------- |
| `DuAn` | `GiaiDoanHienTaiId` (+ fallback `BuocHienTai.Buoc.GiaiDoanId`) | **Phạm vi chính** — cùng pattern `DuAnGetDanhSachQuery` |
| `HangMucKeHoach` | `GiaiDoanId` | Entity riêng trong `KeHoachTrienKhaiHangMuc`; **không có** `TrangThaiDuAnId`, `ThoiGianHoanThanh` như `DuAn` |

**Đề xuất kỹ thuật (giống #9527):** Backend lần này chỉ phục vụ **`DuAn`**. Phần “hạng mục” trong title issue **out of scope** trừ khi BA xác nhận cần union 2 nguồn dữ liệu → issue riêng.

### 1.3 Quy tắc phân loại (đã chốt từ BA)

| Panel | Điều kiện |
| ----- | --------- |
| **Tổng số** | Toàn bộ dự án thuộc giai đoạn được chọn (sau filter màn hình) |
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
- Mỗi dự án chỉ thuộc **đúng 1** trong 3 bucket (Còn hạn / Quá hạn / Đã hoàn thành).

### 1.4 Mapping UI → cột bảng

| # | Cột UI | Nguồn dữ liệu |
| - | ------ | -------------- |
| 1 | STT | Tính server-side: `skip + index + 1` (pattern `TheoDoiDuAnPhongPhanCongQuery.ApplyStt`) |
| 2 | Mã dự án | `DuAn.MaDuAn` |
| 3 | Tên dự án | `DuAn.TenDuAn` |
| 4 | Thời gian thực hiện | `"{ThoiGianKhoiCong} - {ThoiGianHoanThanh}"` (năm) |
| 5 | Hình thức quản lý dự án | `DanhMucHinhThucQuanLy.Ten` |
| 6 | Hình thức đầu tư | `DanhMucHinhThucDauTu.Ten` |
| 7 | Tổng mức đầu tư | `DuAn.TongMucDauTu` |
| 8 | Lãnh đạo phụ trách | `UserMaster.HoTen` qua `LanhDaoPhuTrachId` |
| 9 | Đơn vị/phòng phụ trách | `DmDonVi.TenDonVi` qua `DonViPhuTrachChinhId` |
| — | (ẩn) Giai đoạn | `DanhMucGiaiDoan.Ten` — hiển thị khi cần, hoặc ẩn vì user đã chọn giai đoạn |
| — | (ẩn) Id | `DuAn.Id` — cho menu action FE |

---

## 2. Khảo sát source (đã xác minh)

### 2.1 Entity & field giai đoạn trên `DuAn`

```text
DuAn
├── GiaiDoanHienTaiId   int?   → DanhMucGiaiDoan (DmGiaiDoan) — giai đoạn hiện tại (denormalized)
├── BuocHienTaiId       int?   → DuAnBuoc
│   └── BuocHienTai.Buoc.GiaiDoanId   int?   — fallback khi GiaiDoanHienTaiId == null
├── ThoiGianKhoiCong      int?   ← năm khởi công (dự kiến)
├── ThoiGianHoanThanh     int?   ← năm hoàn thành (dùng cho Còn hạn / Quá hạn)
├── TrangThaiDuAnId       int?   → DanhMucTrangThaiDuAn
├── MaDuAn, TenDuAn, TongMucDauTu
├── HinhThucQuanLyDuAnId, HinhThucDauTuId
├── LanhDaoPhuTrachId, DonViPhuTrachChinhId
└── (không dùng) LoaiDuAnTheoNamId, BuocId trực tiếp cho filter giai đoạn
```

**Danh mục giai đoạn:**

| Thành phần | Vị trí |
| ---------- | ------ |
| Entity | `QLDA.Domain/Entities/DanhMuc/DanhMucGiaiDoan.cs` |
| Table | `DmGiaiDoan` |
| Nav trên DuAn | `DuAn.GiaiDoanHienTai` |

### 2.2 Logic filter `GiaiDoanId` hiện có (tái sử dụng)

Từ `DuAnGetDanhSachQuery` (dòng 48–50) — **đây là convention chính thức**, không tự đoán field khác:

```csharp
.WhereFunc(search.GiaiDoanId.HasValue, q => q
    .WhereIf(search.GiaiDoanId > 0,
        e => e.GiaiDoanHienTaiId == null
            ? e.BuocHienTai != null
              && e.BuocHienTai.Buoc != null
              && e.BuocHienTai.Buoc.GiaiDoanId == search.GiaiDoanId
            : e.GiaiDoanHienTaiId == search.GiaiDoanId)
    .WhereIf(search.GiaiDoanId == -1,
        e => e.GiaiDoanHienTaiId == null && e.BuocHienTaiId == null))
```

| `GiaiDoanId` | Ý nghĩa |
| ------------ | ------- |
| `> 0` | Dự án đang ở giai đoạn đó (`GiaiDoanHienTaiId`) hoặc suy ra từ bước hiện tại |
| `-1` | Chưa xác định giai đoạn (cả `GiaiDoanHienTaiId` và `BuocHienTaiId` đều null) |
| `null` / không gửi | **Không filter** theo giai đoạn — chỉ dùng nếu BA cho phép xem tất cả |

> **Lưu ý màn hình:** UI thường **bắt buộc chọn 1 giai đoạn** (`GiaiDoanId > 0`). Nếu FE luôn gửi `giaiDoanId`, handler không cần default fallback.

### 2.3 Trạng thái dự án (seed)

| Id | Ma | Ten |
| -- | -- | --- |
| 1 | `DTH` | Đang thực hiện |
| 2 | `PDDT` | Đã phê duyệt đầu tư |
| 3 | `HT` | **Đã hoàn thành** |
| 4 | `TD` | Tạm dừng |

**Constant tái sử dụng:** `QLDA.Domain.Constants.DanhMucTrangThaiDuAnCodes.HoanThanh` (`"HT"`).

### 2.4 API / Query hiện có (gần nhất)

| API | Route | Handler | Ghi chú |
| --- | ----- | ------- | ------- |
| **Theo phòng ban (#9527)** | `GET /api/du-an/theo-doi-du-an-phong-phan-cong` | `TheoDoiDuAnPhongPhanCongQuery` | ✅ **Clone pattern** — 4 panel + list, chỉ đổi filter |
| Danh sách dự án | `GET /api/du-an/danh-sach` | `DuAnGetDanhSachQuery` | ✅ `FilterVisible` + filter `GiaiDoanId` |
| Dashboard theo giai đoạn | `GET /api/thong-ke/theo-giai-doan` | `DashboardGetTheoGiaiDoanQuery` | ⚠️ Chỉ **count** theo `GiaiDoanHienTaiId`, không panel, không auth, không list |
| Export danh sách | `GET /api/print/...` | `DuAnGetDanhSachExportQuery` | ✅ Cùng filter `GiaiDoanId` |

### 2.5 Phân quyền (bắt buộc)

Theo `CLAUDE.md` + pattern `TheoDoiDuAnPhongPhanCongQuery`:

```csharp
var root = authManager.FilterVisible(duAn.GetQueryableSet(), AuthorizationResourceKeys.DuAn);
```

- **Không** copy `DashboardRepository` (không `FilterVisible`).  
- **Không** copy `DuAnGetTheoPhongBanGetQuery` (`GetOriginalSet`, không auth).  
- Filter giai đoạn: `GiaiDoanId` từ request (FE chọn trên màn hình).

### 2.6 Filter năm / từ khóa — tái sử dụng

Từ `TheoDoiDuAnPhongPhanCongQuery` / `DuAnGetDanhSachQuery`:

| Param | Logic |
| ----- | ----- |
| `NamDuAn` | `NamDuAn >= ThoiGianKhoiCong` AND (`ThoiGianHoanThanh == null && ThoiGianKhoiCong == NamDuAn` OR `NamDuAn <= ThoiGianHoanThanh`) |
| `TenDuAn`, `MaDuAn` | Contains (case-insensitive) |
| `GlobalFilter` | `WhereGlobalFilter` trên `TenDuAn`, `MaDuAn` |
| `PageIndex`, `PageSize` | `CommonSearchDto` — `PageIndex` **1-based** |

---

## 3. So sánh với API phòng ban (#9527)

| Khía cạnh | `theo-doi-du-an-phong-phan-cong` | `theo-doi-du-an-theo-giai-doan` (mới) |
| --------- | -------------------------------- | ------------------------------------- |
| Filter chính | `DonViPhuTrachChinhId` | `GiaiDoanId` (logic `DuAnGetDanhSachQuery`) |
| Default filter | Fallback `user.PhongBanID` khi không gửi phòng | **Không fallback** — FE chọn giai đoạn |
| Panel 4 counter | ✅ Giống | ✅ Giống |
| Phân loại Còn hạn/Quá hạn/HT | ✅ Giống | ✅ Giống |
| Auth | `FilterVisible` | `FilterVisible` |
| DTO list | `TheoDoiDuAnPhongPhanCongDto` | `TheoDoiDuAnTheoGiaiDoanDto` (+ optional `TenGiaiDoan`) |
| Enum panel | `ETheoDoiDuAnPhongPhanCongLoai` | `ETheoDoiDuAnTheoGiaiDoanLoai` (tên mới, cùng giá trị) |

**Chiến lược implement:** Copy cấu trúc `TheoDoiDuAnPhongPhanCongQuery`, thay `BuildQuery` filter phòng → filter giai đoạn, giữ nguyên logic count + `ApplyLoaiFilter`.

---

## 4. Kiến trúc đề xuất

### 4.1 Luồng xử lý

```text
GET /api/du-an/theo-doi-du-an-theo-giai-doan
        │
        ▼
DuAnController (mỏng — Mediator.Send only)
        │
        ▼
TheoDoiDuAnTheoGiaiDoanQuery + Handler
        │
        ├─ 1. Resolve trangThaiHoanThanhId (Ma = HT)
        ├─ 2. BuildBaseQuery(search)     ← auth + filter giai đoạn + filter màn hình (KHÔNG filter Loai)
        ├─ 3. Count ConHan / QuaHan / DaHoanThanh trên cùng base query
        ├─ 4. TongSoDuAn = ConHan + QuaHan + DaHoanThanh
        ├─ 5. ApplyLoaiFilter(base, Loai) → paginate → map DTO
        └─ 6. Return TheoDoiDuAnTheoGiaiDoanResultDto
```

### 4.2 Nguyên tắc tránh lệch số liệu

| Quy tắc | Cách làm |
| ------- | -------- |
| Một nguồn filter | Private method `BuildQuery(...)` dùng chung cho **count** và **list** |
| Một định nghĩa loại | Cùng switch `Loai` như `TheoDoiDuAnPhongPhanCongQuery` |
| Không duplicate string | `DanhMucTrangThaiDuAnCodes.HoanThanh` |
| Năm hiện tại | `IDateTimeProvider.OffsetUtcNow.Year` |
| Include nav cho filter giai đoạn | Cần `.Include(e => e.BuocHienTai).ThenInclude(b => b.Buoc)` **hoặc** dùng subquery EF-translatable (ưu tiên copy expression từ `DuAnGetDanhSachQuery` — đã proven) |

### 4.3 Pseudocode handler

```csharp
public async Task<TheoDoiDuAnTheoGiaiDoanResultDto> Handle(...)
{
    var search = request.SearchDto;
    var hoanThanhId = await ResolveHoanThanhIdAsync(ct);
    var namHienTai = _dateTimeProvider.OffsetUtcNow.Year;

    var root = _authManager.FilterVisible(_duAn.GetQueryableSet(), AuthorizationResourceKeys.DuAn);

    // Count — 1 round-trip (GroupBy pattern từ TheoDoiDuAnPhongPhanCongQuery)
    var counters = await BuildQuery(root, search, namHienTai, hoanThanhId, TatCa)
        .GroupBy(_ => AggregateAllRows)
        .Select(g => new TheoDoiCounters(
            g.Count(e => e.TrangThaiDuAnId == hoanThanhId),
            g.Count(e => e.TrangThaiDuAnId != hoanThanhId
                && (e.ThoiGianHoanThanh == null || namHienTai <= e.ThoiGianHoanThanh)),
            g.Count(e => e.TrangThaiDuAnId != hoanThanhId
                && e.ThoiGianHoanThanh != null
                && namHienTai > e.ThoiGianHoanThanh)))
        .FirstOrDefaultAsync(ct) ?? TheoDoiCounters.Empty;

    var danhSach = await ProjectToDto(
            BuildQuery(root, search, namHienTai, hoanThanhId, search.Loai)
                .OrderBy(e => e.TenDuAn))
        .PaginatedListAsync(search.Skip(), search.Take(), ct);

    ApplyStt(danhSach.Data, search.Skip());

    return new TheoDoiDuAnTheoGiaiDoanResultDto
    {
        TongSoDuAn = counters.Total,
        ConHan = counters.ConHan,
        QuaHan = counters.QuaHan,
        DaHoanThanh = counters.DaHoanThanh,
        DanhSach = danhSach,
    };
}
```

> **Tái sử dụng tối đa:** Có thể extract shared `TheoDoiCounters` + logic phân loại vào helper nội bộ sau — **không bắt buộc** lần đầu; ưu tiên copy có kiểm soát từ `TheoDoiDuAnPhongPhanCongQuery`.

---

## 5. API contract

### 5.1 Endpoint

```http
GET /api/du-an/theo-doi-du-an-theo-giai-doan
```

| | |
| - | - |
| Controller | `DuAnController` |
| Auth | Mặc định `[Authorize]` của project |
| Response wrapper | `ResultApi<TheoDoiDuAnTheoGiaiDoanResultDto>` |

### 5.2 Request — `TheoDoiDuAnTheoGiaiDoanSearchDto`

Kế thừa `CommonSearchDto` (pagination + `GlobalFilter`).

```csharp
namespace QLDA.Application.DuAns.DTOs;

public record TheoDoiDuAnTheoGiaiDoanSearchDto : CommonSearchDto
{
    /// <summary>
    /// Giai đoạn cần thống kê — logic giống DuAnSearchDto.GiaiDoanId.
    /// &gt; 0: filter theo giai đoạn; -1: chưa xác định giai đoạn.
    /// </summary>
    public int? GiaiDoanId { get; set; }

    /// <summary>Filter năm dự án — logic NamDuAn (#9121)</summary>
    public int? NamDuAn { get; set; }

    /// <summary>Tên dự án (contains)</summary>
    public string? TenDuAn { get; set; }

    /// <summary>Mã dự án (contains)</summary>
    public string? MaDuAn { get; set; }

    /// <summary>Panel/tab đang chọn</summary>
    public ETheoDoiDuAnTheoGiaiDoanLoai Loai { get; set; } = ETheoDoiDuAnTheoGiaiDoanLoai.TatCa;
}
```

**Query string ví dụ:**

```http
GET /api/du-an/theo-doi-du-an-theo-giai-doan
  ?giaiDoanId=2
  &namDuAn=2026
  &globalFilter=phần mềm
  &loai=ConHan
  &pageIndex=1
  &pageSize=10
```

### 5.3 Enum — `ETheoDoiDuAnTheoGiaiDoanLoai`

```csharp
namespace QLDA.Domain.Enums;

public enum ETheoDoiDuAnTheoGiaiDoanLoai
{
    TatCa = 0,
    ConHan = 1,
    QuaHan = 2,
    DaHoanThanh = 3,
}
```

> Tách enum riêng (không reuse `ETheoDoiDuAnPhongPhanCongLoai`) để Swagger/FE mapping rõ ngữ cảnh, dù giá trị giống nhau.

### 5.4 Response

```csharp
public class TheoDoiDuAnTheoGiaiDoanResultDto
{
    public int TongSoDuAn { get; set; }
    public int ConHan { get; set; }
    public int QuaHan { get; set; }
    public int DaHoanThanh { get; set; }

    public PaginatedList<TheoDoiDuAnTheoGiaiDoanDto> DanhSach { get; set; } = new();
}

public class TheoDoiDuAnTheoGiaiDoanDto
{
    public int Stt { get; set; }
    public Guid Id { get; set; }
    public string? MaDuAn { get; set; }
    public string? TenDuAn { get; set; }
    public int? GiaiDoanId { get; set; }
    public string? TenGiaiDoan { get; set; }
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
          "giaiDoanId": 2,
          "tenGiaiDoan": "Thực hiện đầu tư",
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

> **FE note:** 4 số panel **luôn trả về** (tính trên full base query sau filter giai đoạn). `danhSach.totalRows` thay đổi theo `loai` đang chọn.

### 5.5 Map `TenGiaiDoan` trong projection

Ưu tiên resolve giống `DuAnGetDanhSachQuery` dòng 87:

```csharp
GiaiDoanId = e.GiaiDoanHienTaiId != null
    ? e.GiaiDoanHienTaiId
    : e.BuocHienTai != null && e.BuocHienTai.Buoc != null
        ? e.BuocHienTai.Buoc.GiaiDoanId
        : null,

TenGiaiDoan = e.GiaiDoanHienTai != null
    ? e.GiaiDoanHienTai.Ten
    : e.BuocHienTai != null && e.BuocHienTai.Buoc != null && e.BuocHienTai.Buoc.GiaiDoan != null
        ? e.BuocHienTai.Buoc.GiaiDoan.Ten
        : null,
```

---

## 6. Logic nghiệp vụ chi tiết

### 6.1 `BuildQuery` — filter chung (không gồm `Loai`)

Thứ tự áp dụng:

1. `_authManager.FilterVisible(..., AuthorizationResourceKeys.DuAn)`
2. `.AsNoTracking()`
3. **Filter giai đoạn** — copy từ `DuAnGetDanhSachQuery` (mục 2.2)
4. `NamDuAn` — copy từ `TheoDoiDuAnPhongPhanCongQuery` dòng 115–118
5. `TenDuAn`, `MaDuAn` — contains
6. `WhereGlobalFilter(search, e => e.TenDuAn, e => e.MaDuAn)`

**Không có** filter `DonViPhuTrachChinhId` trong API này.

### 6.2 Phân loại Còn hạn / Quá hạn / Đã hoàn thành

Giả sử `hoanThanhId` = id của `Ma = HT`, `nam` = năm hiện tại:

```text
Đã hoàn thành:
  TrangThaiDuAnId == hoanThanhId

Còn hạn (chưa hoàn thành):
  TrangThaiDuAnId != hoanThanhId
  AND (
        ThoiGianHoanThanh == null
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

### 6.3 Công thức tổng

```csharp
TongSoDuAn = ConHan + QuaHan + DaHoanThanh;
// Không dùng Count(*) riêng trên baseQuery nếu có thể tránh lệch định nghĩa
```

### 6.4 Edge case — `ThoiGianHoanThanh` null

| Tình huống | Xử lý | Lý do |
| ---------- | ----- | ----- |
| Chưa HT, `ThoiGianHoanThanh == null` | **Còn hạn** | Đồng bộ với #9527 — mọi dự án thuộc đúng 1 bucket |
| Đã HT | **Đã hoàn thành** | Ưu tiên trạng thái, bỏ qua năm |

### 6.5 Edge case — giai đoạn

| Tình huống | Xử lý |
| ---------- | ----- |
| `GiaiDoanHienTaiId` set | Filter/match trực tiếp |
| `GiaiDoanHienTaiId` null, có `BuocHienTai` | Suy `GiaiDoanId` từ `Buoc.GiaiDoanId` |
| Cả hai null | Chỉ vào kết quả khi `GiaiDoanId == -1` hoặc không filter giai đoạn |
| Dự án đổi giai đoạn | Chỉ hiện ở giai đoạn **hiện tại** — không lịch sử |

### 6.6 Format `ThoiGianThucHien`

Copy từ `TheoDoiDuAnPhongPhanCongQuery.ProjectToDto` (dòng 145–149).

### 6.7 Map tên Lãnh đạo / Đơn vị

Copy pattern subquery trong `TheoDoiDuAnPhongPhanCongQuery.ProjectToDto` — batch qua `userMaster` / `dmDonVi` trong cùng `Select`.

---

## 7. Files cần tạo/sửa

### 7.1 Tạo mới

| File | Mô tả |
| ---- | ----- |
| `QLDA.Domain/Enums/ETheoDoiDuAnTheoGiaiDoanLoai.cs` | Enum panel |
| `QLDA.Application/DuAns/DTOs/TheoDoiDuAnTheoGiaiDoanSearchDto.cs` | Request |
| `QLDA.Application/DuAns/DTOs/TheoDoiDuAnTheoGiaiDoanDto.cs` | Dòng list |
| `QLDA.Application/DuAns/DTOs/TheoDoiDuAnTheoGiaiDoanResultDto.cs` | Response wrapper |
| `QLDA.Application/DuAns/Queries/TheoDoiDuAnTheoGiaiDoanQuery.cs` | Query + Handler |

### 7.2 Sửa

| File | Thay đổi |
| ---- | -------- |
| `QLDA.WebApi/Controllers/DuAnController.cs` | Thêm `GET theo-doi-du-an-theo-giai-doan` — **chỉ** `Mediator.Send` |

### 7.3 Không tạo / không sửa

| ❌ | Lý do |
| -- | ----- |
| Model trong `QLDA.WebApi/Models` | DTO đặt Application (`CLAUDE.md`) |
| Migration / `AppDbContextModelSnapshot.cs` | Không đổi schema |
| `DashboardGetTheoGiaiDoanQuery` | API dashboard khác mục đích — giữ nguyên |
| `TheoDoiDuAnPhongPhanCongQuery` | API cũ giữ nguyên |

### 7.4 Controller mẫu (mỏng)

```csharp
/// <summary>
/// Thống kê theo dõi dự án theo giai đoạn — 4 panel + danh sách phân trang
/// </summary>
[HttpGet("api/du-an/theo-doi-du-an-theo-giai-doan")]
[Consumes(MediaTypeNames.Application.Json)]
[ProducesResponseType<ResultApi<TheoDoiDuAnTheoGiaiDoanResultDto>>(StatusCodes.Status200OK)]
[ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
public async Task<ResultApi> GetTheoDoiDuAnTheoGiaiDoan(
    [FromQuery] TheoDoiDuAnTheoGiaiDoanSearchDto searchDto)
{
    var res = await Mediator.Send(new TheoDoiDuAnTheoGiaiDoanQuery(searchDto));
    return ResultApi.Ok(res);
}
```

### 7.5 Refactor tùy chọn (sau khi ship)

| Ý tưởng | Lợi ích |
| ------- | ------- |
| Extract `TheoDoiDuAnClassificationHelper` | DRY logic count/loại giữa 2 API theo dõi |
| Shared base DTO | `TheoDoiDuAnListItemDto` nếu 2 DTO list giống hệt |

> **Không làm trong PR đầu** — giữ diff nhỏ, copy có kiểm soát.

---

## 8. Test plan

### 8.1 Case phân loại (giống #9527)

| # | Trạng thái | ThoiGianHoanThanh | Năm HT | Bucket |
| - | ---------- | ----------------- | ------ | ------ |
| T1 | HT | 2024 | 2026 | DaHoanThanh |
| T2 | DTH | 2027 | 2026 | ConHan |
| T3 | DTH | 2024 | 2026 | QuaHan |
| T4 | DTH | null | 2026 | ConHan (edge) |
| T5 | TD | 2024 | 2026 | QuaHan (chưa HT) |

### 8.2 Case tổng & panel

| # | Kịch bản | Kỳ vọng |
| - | -------- | ------- |
| S1 | 12 ConHan + 12 QuaHan + 4 HT trong giai đoạn X | `tongSoDuAn = 28` |
| S2 | `loai=QuaHan` | `danhSach.totalRows = 12`, không có HT |
| S3 | Đổi `loai` giữa các request | 4 panel **không đổi**; chỉ list đổi |
| S4 | Đổi `giaiDoanId` | Panel + list chỉ còn dự án thuộc giai đoạn đó |
| S5 | `namDuAn=2026` | Chỉ dự án có khoảng năm chứa 2026 |

### 8.3 Case giai đoạn

| # | Dự án | `giaiDoanId` request | Kỳ vọng |
| - | ----- | -------------------- | ------- |
| G1 | `GiaiDoanHienTaiId = 2` | `2` | ✅ Có trong kết quả |
| G2 | `GiaiDoanHienTaiId = null`, `Buoc.GiaiDoanId = 2` | `2` | ✅ Có (fallback) |
| G3 | `GiaiDoanHienTaiId = 3` | `2` | ❌ Không có |
| G4 | Cả giai đoạn và bước null | `-1` | ✅ Chỉ case `giaiDoanId=-1` |

### 8.4 Case đặc biệt (theo BA)

| # | Kịch bản | Kỳ vọng |
| - | -------- | ------- |
| E1 | Dự án HT, `ThoiGianHoanThanh = 2024` | Chỉ **DaHoanThanh** |
| E2 | Dự án HT, `ThoiGianHoanThanh = 2028` | Chỉ **DaHoanThanh** |
| E3 | Không filter phòng ban | API **không** có param `donViPhuTrachChinhId` |
| E4 | So sánh với `theo-doi-du-an-phong-phan-cong` cùng dataset | Count khác nhau khi filter khác nhau — đúng nghiệp vụ |

### 8.5 Case auth

| # | User | Kỳ vọng |
| - | ---- | ------- |
| A1 | Chuyên viên phòng A | Chỉ dự án trong phạm vi `FilterVisible` |
| A2 | KH-TC bypass | Thấy theo filter giai đoạn, không bị ownership cắt sai |

### 8.6 Build

```bash
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```

---

## 9. Checklist nghiệm thu

- [x] API `GET /api/du-an/theo-doi-du-an-theo-giai-doan` hoạt động
- [x] Trả đủ 4 counter + `danhSach` phân trang
- [x] `tongSoDuAn == conHan + quaHan + daHoanThanh` mọi lần gọi
- [x] HT không nằm trong ConHan/QuaHan
- [x] `loai` lọc đúng list; counter không đổi theo `loai`
- [x] Filter theo `giaiDoanId` — **không** theo phòng ban
- [x] Logic `GiaiDoanId` khớp `DuAnGetDanhSachQuery`
- [x] `FilterVisible` ở đầu query
- [x] Không model WebApi mới; không logic nghiệp vụ trong Controller
- [x] Dùng `DanhMucTrangThaiDuAnCodes.HoanThanh`, không magic string
- [x] Build pass, không warning mới liên quan

---

## 10. Thứ tự commit đề xuất

```bash
git commit -m "feat: add project tracking statistics by phase api

- Add summary counters for total, on-time, overdue and completed projects by phase
- Add filtered project list by tracking status and selected phase
- Reuse phase filter logic from DuAnGetDanhSachQuery"
```

**Nhóm file trong 1 commit** (chỉ Application + Domain enum + WebApi controller — không migration):

1. `QLDA.Domain/Enums/ETheoDoiDuAnTheoGiaiDoanLoai.cs`
2. `QLDA.Application/DuAns/DTOs/TheoDoiDuAnTheoGiaiDoan*.cs`
3. `QLDA.Application/DuAns/Queries/TheoDoiDuAnTheoGiaiDoanQuery.cs`
4. `QLDA.WebApi/Controllers/DuAnController.cs`

---

## Phụ lục A — API liên quan (không trùng)

| API | Mục đích | Khác UC #118 |
| --- | -------- | ------------ |
| `GET /api/thong-ke/theo-giai-doan?nam=` | Dashboard count theo giai đoạn | Không panel, không list, không auth, filter năm cứng |
| `GET /api/du-an/theo-doi-du-an-phong-phan-cong` | Theo dõi theo **phòng** | Filter `DonViPhuTrachChinhId` |
| `GET /api/du-an/danh-sach?giaiDoanId=` | List dự án thường | Không 4 panel |

## Phụ lục B — Câu hỏi mở (confirm với BA nếu cần)

1. **Hạng mục:** Có cần list `HangMucKeHoach` trên cùng màn không? (entity khác, không có `TrangThaiDuAnId`)  
2. **`GiaiDoanId` bắt buộc:** FE có luôn gửi `giaiDoanId > 0` không? Hay cần xem “tất cả giai đoạn”?  
3. **Group nhiều giai đoạn:** Màn hình chỉ chọn 1 giai đoạn hay cần API trả summary group-by? (doc hiện tại: **chọn 1**, panel count trong phạm vi giai đoạn đó)  
4. **`ThoiGianHoanThanh` null:** Chốt gom **Còn hạn** như #9527?

---

*Document generated from codebase survey — June 29, 2026.*
