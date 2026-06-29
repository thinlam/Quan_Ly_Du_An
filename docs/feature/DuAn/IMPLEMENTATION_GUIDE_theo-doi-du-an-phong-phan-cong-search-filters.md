# Bổ sung filter `lanhDaoPhuTrachId` & `trangThaiDuAnId` — API theo dõi dự án phòng phân công

**Document date:** June 29, 2026  
**Status:** ✅ **IMPLEMENTED** (June 29, 2026)  
**Module:** QLDA — `DuAn` (UC #9527 — theo dõi dự án phòng phân công)  
**Pattern tham chiếu:** `DuAnGetDanhSachQuery`, `DuAnGetDanhSachExportQuery`, `IMPLEMENTATION_GUIDE_9527_theo-doi-du-an-phong-phan-cong.md`

**Mục lục:** [0. Trạng thái](#0-trạng-thái-hiện-tại) · [1. Mục tiêu](#1-mục-tiêu) · [2. Khảo sát source](#2-khảo-sát-source-đã-xác-minh) · [3. Thiết kế](#3-thiết-kế) · [4. Bước code](#4-bước-code-chi-tiết) · [5. API contract](#5-api-contract) · [6. Test plan](#6-test-plan) · [7. Checklist](#7-checklist-nghiệm-thu) · [8. Commit](#8-commit-đề-xuất)

---

## 0. Trạng thái hiện tại

| Hạng mục | Trạng thái | Ghi chú |
| -------- | ---------- | ------- |
| Investigation / docs | ✅ Done | File này |
| `TheoDoiDuAnPhongPhanCongSearchDto` | ✅ Done | `LanhDaoPhuTrachId`, `TrangThaiDuAnId` |
| `TheoDoiDuAnPhongPhanCongQuery.BuildQuery` | ✅ Done | `WhereFunc` + `WhereIf` |
| `DuAnController` | ✅ Không cần sửa | Đã bind `TheoDoiDuAnPhongPhanCongSearchDto` |
| WebApi SearchModel mới | ✅ Không cần | DTO Application bind trực tiếp (`CLAUDE.md`) |
| Migration | ✅ Không cần | Chỉ query/filter |
| Integration test | ⏳ Tùy chọn | Khuyến nghị 4 case filter cơ bản |
| Manual verify curl | ⏳ Pending | Mẫu ở [6.1](#61-curl-mẫu) |

---

## 1. Mục tiêu

Bổ sung 2 query param cho API danh sách theo dõi dự án phòng phân công:

| Param | Ý nghĩa | Map entity |
| ----- | ------- | ---------- |
| `lanhDaoPhuTrachId` | Lọc theo lãnh đạo phụ trách dự án | `DuAn.LanhDaoPhuTrachId` (`long?` → `UserMaster`) |
| `trangThaiDuAnId` | Lọc theo **trạng thái dự án** (không phải trạng thái phê duyệt) | `DuAn.TrangThaiDuAnId` (`int?` → `DanhMucTrangThaiDuAn`) |

### Acceptance criteria

| # | Kịch bản | Kỳ vọng |
| - | -------- | ------- |
| AC1 | Không truyền 2 param | Hành vi **giữ nguyên** như hiện tại |
| AC2 | Chỉ `lanhDaoPhuTrachId` | Chỉ dự án có `LanhDaoPhuTrachId` khớp |
| AC3 | Chỉ `trangThaiDuAnId` | Chỉ dự án có `TrangThaiDuAnId` khớp |
| AC4 | Cả 2 param | AND — thỏa **cả hai** điều kiện |
| AC5 | Response | Giữ nguyên `tongSoDuAn`, `conHan`, `quaHan`, `daHoanThanh`, `danhSach` (paging/totalRows/data) |
| AC6 | Build | Không warning/error mới |

---

## 2. Khảo sát source (đã xác minh)

### 2.1 Luồng hiện tại

```text
GET /QuanLyDuAn/api/du-an/theo-doi-du-an-phong-phan-cong
        │
        ▼
DuAnController.GetTheoDoiDuAnPhongPhanCong
  └─ [FromQuery] TheoDoiDuAnPhongPhanCongSearchDto   ← bind trực tiếp, không qua WebApi model
        │
        ▼
TheoDoiDuAnPhongPhanCongQuery + Handler
  ├─ FilterVisible(DuAn) — đầu query
  ├─ BuildQuery(...) — filter màn hình + panel `loai`
  │    ├─ Dùng cho COUNT 4 panel (loai = TatCa)
  │    └─ Dùng cho LIST phân trang (loai = search.Loai)
  └─ Return TheoDoiDuAnPhongPhanCongResultDto
```

**File tham chiếu:**

| Thành phần | Vị trí |
| ---------- | ------ |
| Controller | `QLDA.WebApi/Controllers/DuAnController.cs` (dòng ~115–122) |
| Search DTO | `QLDA.Application/DuAns/DTOs/TheoDoiDuAnPhongPhanCongSearchDto.cs` |
| Handler | `QLDA.Application/DuAns/Queries/TheoDoiDuAnPhongPhanCongQuery.cs` |
| Entity | `QLDA.Domain/Entities/DuAn.cs` |
| Guide gốc UC | `docs/feature/DuAn/IMPLEMENTATION_GUIDE_9527_theo-doi-du-an-phong-phan-cong.md` |

### 2.2 Search DTO hiện tại — thiếu 2 field

```csharp
// TheoDoiDuAnPhongPhanCongSearchDto — HIỆN TẠI
public record TheoDoiDuAnPhongPhanCongSearchDto : CommonSearchDto
{
    public long? DonViPhuTrachChinhId { get; set; }
    public int? NamDuAn { get; set; }
    public string? TenDuAn { get; set; }
    public string? MaDuAn { get; set; }
    public ETheoDoiDuAnPhongPhanCongLoai Loai { get; set; } = ETheoDoiDuAnPhongPhanCongLoai.TatCa;
    // ❌ Chưa có LanhDaoPhuTrachId
    // ❌ Chưa có TrangThaiDuAnId
}
```

→ Query string `?lanhDaoPhuTrachId=1&trangThaiDuAnId=2` **bị bỏ qua** vì DTO không có property tương ứng.

### 2.3 Handler `BuildQuery` — chưa lọc 2 field

```csharp
// TheoDoiDuAnPhongPhanCongQuery.BuildQuery — HIỆN TẠI (rút gọn)
query = query
    .AsNoTracking()
    .WhereIf(donViPhuTrachChinhId is > 0, e => e.DonViPhuTrachChinhId == donViPhuTrachChinhId)
    .WhereIf(search.TenDuAn.IsNotNullOrWhitespace(), ...)
    .WhereIf(search.MaDuAn.IsNotNullOrWhitespace(), ...)
    .WhereIf(search.NamDuAn > 0, ...)
    .WhereGlobalFilter(search, e => e.TenDuAn, e => e.MaDuAn);
// ❌ Không WhereIf LanhDaoPhuTrachId / TrangThaiDuAnId
```

### 2.4 Entity field — đã có sẵn

```text
DuAn
├── LanhDaoPhuTrachId   long?   → UserMaster (legacy, join in-memory trong ProjectToDto)
└── TrangThaiDuAnId     int?    → DanhMucTrangThaiDuAn
```

**Seed trạng thái dự án** (`DanhMucTrangThaiDuAn`):

| Id | Ma | Ten |
| -- | -- | --- |
| 1 | `DTH` | Đang thực hiện |
| 2 | `PDDT` | Đã phê duyệt đầu tư |
| 3 | `HT` | Đã hoàn thành |
| 4 | `TD` | Tạm dừng |

> **Phân biệt:** `trangThaiDuAnId` = `DuAn.TrangThaiDuAnId`. **Không** nhầm với `TrangThaiId` / `DanhMucTrangThaiPheDuyet` dùng ở module phân khai, quyết định, v.v.

### 2.5 Pattern đã có — `DuAnGetDanhSachQuery`

Cùng module `DuAn`, đã implement đúng 2 filter này — **nên copy pattern**:

```csharp
// DuAnGetDanhSachQuery.cs — dòng 42–46
.WhereFunc(request.SearchDto.LanhDaoPhuTrachId.HasValue, q => q
    .WhereIf(request.SearchDto.LanhDaoPhuTrachId > 0, e => e.LanhDaoPhuTrachId == request.SearchDto.LanhDaoPhuTrachId)
    .WhereIf(request.SearchDto.LanhDaoPhuTrachId == -1, e => e.LanhDaoPhuTrachId == null)
)
.WhereIf(request.SearchDto.TrangThaiDuAnId > 0, e => e.TrangThaiDuAnId == request.SearchDto.TrangThaiDuAnId)
```

| Giá trị `lanhDaoPhuTrachId` | Hành vi |
| --------------------------- | ------- |
| Không gửi / `null` | Không lọc |
| `> 0` | `DuAn.LanhDaoPhuTrachId == value` |
| `-1` | Dự án **chưa** gán lãnh đạo (`LanhDaoPhuTrachId == null`) |

| Giá trị `trangThaiDuAnId` | Hành vi |
| ------------------------- | ------- |
| Không gửi / `null` / `0` | Không lọc |
| `> 0` | `DuAn.TrangThaiDuAnId == value` |

`DuAnSearchDto` đã khai báo 2 property tương tự (`QLDA.Application/DuAns/DTOs/DuAnSearchDto.cs` dòng 51, 71).

### 2.6 Response DTO — không cần đổi

`TheoDoiDuAnPhongPhanCongDto` **đã có** `TrangThaiDuAnId` trong response; `LanhDaoPhuTrach` là tên hiển thị (string). Không cần thêm field response cho task này.

### 2.7 Ảnh hưởng 4 panel counter

`BuildQuery` được dùng chung cho:

1. **Count** 4 panel (`loai = TatCa`)
2. **List** phân trang (`loai = search.Loai`)

→ Khi thêm filter vào `BuildQuery`, **cả counter và list** đều lọc theo subset màn hình — **đúng UX** (giống `donViPhuTrachChinhId`, `namDuAn`, `tenDuAn`).

Ví dụ: `trangThaiDuAnId=1` (Đang thực hiện) → `tongSoDuAn` chỉ đếm dự án DTH; `daHoanThanh` có thể = 0.

### 2.8 Controller — không cần sửa

```csharp
// DuAnController.cs — ĐÃ ĐÚNG
public async Task<ResultApi> GetTheoDoiDuAnPhongPhanCong(
    [FromQuery] TheoDoiDuAnPhongPhanCongSearchDto searchDto)
{
    var res = await Mediator.Send(new TheoDoiDuAnPhongPhanCongQuery(searchDto));
    return ResultApi.Ok(res);
}
```

ASP.NET model binding tự map query string camelCase → property PascalCase khi thêm field vào DTO.

---

## 3. Thiết kế

### 3.1 Nguyên tắc

| Quy tắc | Cách làm |
| ------- | -------- |
| Logic lọc | Chỉ trong `TheoDoiDuAnPhongPhanCongQuery.BuildQuery` (Application) |
| Không logic trong Controller | Giữ controller mỏng |
| Pattern `WhereIf` | Giống `DuAnGetDanhSachQuery` |
| Một nguồn filter | Thêm vào `BuildQuery` — counter + list đồng bộ |
| Auth | Giữ `FilterVisible` ở đầu — **không đổi** |
| Migration | **Không** |

### 3.2 Vị trí thêm filter trong `BuildQuery`

Thêm **sau** filter `NamDuAn`, **trước** `WhereGlobalFilter` (thứ tự không bắt buộc về SQL, nhưng đồng bộ với `DuAnGetDanhSachQuery`):

```text
FilterVisible
  → AsNoTracking
  → DonViPhuTrachChinhId
  → TenDuAn / MaDuAn
  → NamDuAn
  → LanhDaoPhuTrachId      ← MỚI
  → TrangThaiDuAnId        ← MỚI
  → WhereGlobalFilter
  → Apply loai (ConHan / QuaHan / DaHoanThanh)
```

### 3.2 Tương tác `loai` × `trangThaiDuAnId`

Filter kết hợp bằng **AND**:

| `loai` | `trangThaiDuAnId` | Kết quả |
| ------ | ----------------- | ------- |
| `DaHoanThanh` | `3` (HT) | Dự án hoàn thành |
| `DaHoanThanh` | `1` (DTH) | **Rỗng** — mâu thuẫn có chủ đích |
| `ConHan` | `1` (DTH) | Dự án DTH còn hạn |
| `TatCa` | `2` (PDDT) | Mọi bucket chỉ tính trên dự án PDDT |

Đây là hành vi mong đợi — FE chịu trách nhiệm gửi combo hợp lý.

---

## 4. Bước code chi tiết

### 4.1 Mở rộng `TheoDoiDuAnPhongPhanCongSearchDto`

**File:** `QLDA.Application/DuAns/DTOs/TheoDoiDuAnPhongPhanCongSearchDto.cs`

```csharp
/// <summary>Lãnh đạo phụ trách — UserMaster.Id. Gửi -1 để lọc dự án chưa gán.</summary>
public long? LanhDaoPhuTrachId { get; set; }

/// <summary>Trạng thái dự án — DanhMucTrangThaiDuAn (KHÔNG phải trạng thái phê duyệt)</summary>
public int? TrangThaiDuAnId { get; set; }
```

### 4.2 Mở rộng `BuildQuery` trong handler

**File:** `QLDA.Application/DuAns/Queries/TheoDoiDuAnPhongPhanCongQuery.cs`

Thêm vào method `BuildQuery`, sau block `NamDuAn`:

```csharp
.WhereFunc(search.LanhDaoPhuTrachId.HasValue, q => q
    .WhereIf(search.LanhDaoPhuTrachId > 0, e => e.LanhDaoPhuTrachId == search.LanhDaoPhuTrachId)
    .WhereIf(search.LanhDaoPhuTrachId == -1, e => e.LanhDaoPhuTrachId == null))
.WhereIf(search.TrangThaiDuAnId > 0, e => e.TrangThaiDuAnId == search.TrangThaiDuAnId)
```

> **Lưu ý:** Cần `using` extension `WhereFunc` nếu chưa có (đã dùng ở `DuAnGetDanhSachQuery` qua `BuildingBlocks` / `QLDA.Application.Common`).

### 4.3 Files **không** sửa

| File | Lý do |
| ---- | ----- |
| `DuAnController.cs` | DTO bind tự động |
| `QLDA.WebApi/Models/*` | Không tạo model WebApi (`CLAUDE.md`) |
| `TheoDoiDuAnPhongPhanCongDto.cs` | Response đủ field |
| Migration / `AppDbContextModelSnapshot.cs` | Không đổi schema |

### 4.4 Diff tóm tắt (ước lượng)

```diff
// TheoDoiDuAnPhongPhanCongSearchDto.cs
+ public long? LanhDaoPhuTrachId { get; set; }
+ public int? TrangThaiDuAnId { get; set; }

// TheoDoiDuAnPhongPhanCongQuery.cs — BuildQuery
+ .WhereFunc(search.LanhDaoPhuTrachId.HasValue, q => q
+     .WhereIf(search.LanhDaoPhuTrachId > 0, e => e.LanhDaoPhuTrachId == search.LanhDaoPhuTrachId)
+     .WhereIf(search.LanhDaoPhuTrachId == -1, e => e.LanhDaoPhuTrachId == null))
+ .WhereIf(search.TrangThaiDuAnId > 0, e => e.TrangThaiDuAnId == search.TrangThaiDuAnId)
```

**Tổng:** 2 file Application, ~10 dòng code.

---

## 5. API contract

### 5.1 Endpoint (không đổi)

```http
GET /QuanLyDuAn/api/du-an/theo-doi-du-an-phong-phan-cong
```

### 5.2 Query params — bổ sung

| Param | Kiểu | Bắt buộc | Mô tả |
| ----- | ---- | -------- | ----- |
| `lanhDaoPhuTrachId` | `long?` | Không | `DuAn.LanhDaoPhuTrachId`. `-1` = chưa gán |
| `trangThaiDuAnId` | `int?` | Không | `DuAn.TrangThaiDuAnId` — danh mục trạng thái **dự án** |

**Params hiện có (giữ nguyên):** `donViPhuTrachChinhId`, `namDuAn`, `tenDuAn`, `maDuAn`, `globalFilter`, `loai`, `pageIndex`, `pageSize`.

### 5.3 Ví dụ request

```http
GET /QuanLyDuAn/api/du-an/theo-doi-du-an-phong-phan-cong
  ?pageIndex=1
  &pageSize=10
  &donViPhuTrachChinhId=218
  &lanhDaoPhuTrachId=1
  &trangThaiDuAnId=2
  &loai=ConHan
```

```bash
curl --location 'http://192.168.1.12:9051/QuanLyDuAn/api/du-an/theo-doi-du-an-phong-phan-cong?pageIndex=1&pageSize=10&lanhDaoPhuTrachId=1&trangThaiDuAnId=2' \
  --header 'Authorization: Bearer <JWT_TOKEN>'
```

### 5.4 Response (không đổi shape)

```json
{
  "success": true,
  "data": {
    "tongSoDuAn": 5,
    "conHan": 3,
    "quaHan": 1,
    "daHoanThanh": 1,
    "danhSach": {
      "totalRows": 3,
      "pageNumber": 1,
      "totalPages": 1,
      "data": [ "..." ]
    }
  }
}
```

- 4 counter: tính trên subset sau **tất cả** filter màn hình (gồm 2 param mới), `loai` **không** áp dụng cho counter.
- `danhSach`: thêm filter `loai` như hiện tại.

---

## 6. Test plan

### 6.1 Curl mẫu

```bash
# Baseline — không filter mới
curl -G 'http://192.168.1.12:9051/QuanLyDuAn/api/du-an/theo-doi-du-an-phong-phan-cong' \
  --data-urlencode 'pageIndex=1' \
  --data-urlencode 'pageSize=10' \
  -H 'Authorization: Bearer <TOKEN>'

# Chỉ lãnh đạo
curl -G '.../theo-doi-du-an-phong-phan-cong' \
  --data-urlencode 'pageIndex=1' \
  --data-urlencode 'pageSize=10' \
  --data-urlencode 'lanhDaoPhuTrachId=1' \
  -H 'Authorization: Bearer <TOKEN>'

# Chỉ trạng thái dự án
curl -G '.../theo-doi-du-an-phong-phan-cong' \
  --data-urlencode 'trangThaiDuAnId=2' \
  -H 'Authorization: Bearer <TOKEN>'

# Kết hợp (acceptance criteria)
curl -G '.../theo-doi-du-an-phong-phan-cong' \
  --data-urlencode 'pageIndex=1' \
  --data-urlencode 'pageSize=10' \
  --data-urlencode 'lanhDaoPhuTrachId=1' \
  --data-urlencode 'trangThaiDuAnId=2' \
  -H 'Authorization: Bearer <TOKEN>'

# Chưa gán lãnh đạo (pattern -1)
curl -G '.../theo-doi-du-an-phong-phan-cong' \
  --data-urlencode 'lanhDaoPhuTrachId=-1' \
  -H 'Authorization: Bearer <TOKEN>'
```

### 6.2 Case chức năng

| # | Input | Kỳ vọng |
| - | ----- | ------- |
| F1 | Không gửi 2 param | Kết quả giống trước fix (so sánh `totalRows` / `tongSoDuAn`) |
| F2 | `lanhDaoPhuTrachId={id có dữ liệu}` | Mọi item có LĐ phụ trách đúng `id` |
| F3 | `trangThaiDuAnId=2` | Mọi item `trangThaiDuAnId == 2` |
| F4 | Cả 2 param | Chỉ dự án thỏa **cả hai** |
| F5 | `lanhDaoPhuTrachId=-1` | Chỉ dự án `lanhDaoPhuTrach` null/empty |
| F6 | `trangThaiDuAnId=0` hoặc không gửi | Không lọc theo trạng thái |
| F7 | F2 + `loai=QuaHan` | List quá hạn trong subset LĐ; counter vẫn trên full subset LĐ |
| F8 | F3 + `donViPhuTrachChinhId` | AND với filter phòng |

### 6.3 Case regression

| # | Kiểm tra |
| - | -------- |
| R1 | `tongSoDuAn == conHan + quaHan + daHoanThanh` sau filter |
| R2 | Paging: `pageIndex`, `pageSize`, `totalRows`, `stt` đúng |
| R3 | `FilterVisible` — user không thấy dự án ngoài phạm vi auth dù filter LĐ |
| R4 | Sort `OrderBy TenDuAn` không đổi |

### 6.4 Build

```bash
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```

---

## 7. Checklist nghiệm thu

- [x] `TheoDoiDuAnPhongPhanCongSearchDto` có `LanhDaoPhuTrachId`, `TrangThaiDuAnId`
- [x] `BuildQuery` có `WhereIf` / `WhereFunc` đúng pattern `DuAnGetDanhSachQuery`
- [x] Không logic lọc trong Controller
- [x] Không model WebApi mới
- [x] Không migration
- [ ] AC1–AC6 pass (curl / Postman)
- [x] `tongSoDuAn` counter phản ánh subset sau filter mới (logic trong `BuildQuery` chung)
- [x] Build pass, không warning mới

---

## 8. Commit đề xuất

```bash
git commit -m "feat(du-an): add leader and project status filters to assigned dept tracking

- Extend TheoDoiDuAnPhongPhanCongSearchDto with lanhDaoPhuTrachId and trangThaiDuAnId
- Apply filters in BuildQuery using same pattern as DuAnGetDanhSachQuery"
```

**Nhóm file (1 commit — chỉ Application):**

1. `QLDA.Application/DuAns/DTOs/TheoDoiDuAnPhongPhanCongSearchDto.cs`
2. `QLDA.Application/DuAns/Queries/TheoDoiDuAnPhongPhanCongQuery.cs`

---

## Phụ lục A — So sánh với `DuAnGetDanhSach`

| | `GET /api/du-an/danh-sach` | `GET /api/du-an/theo-doi-du-an-phong-phan-cong` |
| - | -------------------------- | ----------------------------------------------- |
| Search DTO | `DuAnSearchDto` | `TheoDoiDuAnPhongPhanCongSearchDto` |
| `lanhDaoPhuTrachId` | ✅ Có | ⏳ Thêm (task này) |
| `trangThaiDuAnId` | ✅ Có | ⏳ Thêm (task này) |
| Response | `PaginatedList<DuAnDto>` | Summary 4 panel + `PaginatedList<TheoDoiDuAnPhongPhanCongDto>` |
| Filter logic | Inline trong handler | `BuildQuery` private method |

## Phụ lục B — Câu hỏi đã chốt

| Câu hỏi | Quyết định |
| ------- | ---------- |
| Counter có theo 2 filter mới? | **Có** — thêm vào `BuildQuery` chung |
| Hỗ trợ `lanhDaoPhuTrachId=-1`? | **Có** — đồng bộ `DuAnGetDanhSachQuery` |
| `trangThaiDuAnId` vs trạng thái phê duyệt? | Chỉ `DuAn.TrangThaiDuAnId` |
| Sửa Controller? | **Không** |

---

*Document generated from codebase survey — June 29, 2026.*
