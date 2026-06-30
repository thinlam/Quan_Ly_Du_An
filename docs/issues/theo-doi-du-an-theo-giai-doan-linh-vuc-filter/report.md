# Report — Filter `linhVucId` cho API theo dõi dự án theo giai đoạn

*Survey codebase — 30/06/2026*

---

## 1. Tóm tắt

| Thuộc tính | Giá trị |
|------------|---------|
| Issue | API thiếu / sai filter `linhVucId` |
| Method | `GET` |
| URL | `/QuanLyDuAn/api/du-an/theo-doi-du-an-theo-giai-doan` |
| Controller | `DuAnController.GetTheoDoiDuAnTheoGiaiDoan` |
| Handler | `TheoDoiDuAnTheoGiaiDoanQuery` / `TheoDoiDuAnTheoGiaiDoanQueryHandler` |
| Search DTO | `TheoDoiDuAnTheoGiaiDoanSearchDto` |
| Response | `TheoDoiDuAnTheoGiaiDoanResultDto` (4 counter + `danhSach` phân trang) |
| Migration | **Không** cần |

---

## 2. Luồng xử lý hiện tại

```
GET /api/du-an/theo-doi-du-an-theo-giai-doan?[query]
        │
        ▼
DuAnController.GetTheoDoiDuAnTheoGiaiDoan([FromQuery] TheoDoiDuAnTheoGiaiDoanSearchDto)
        │
        ▼
TheoDoiDuAnTheoGiaiDoanQueryHandler.Handle
        │
        ├─ authManager.FilterVisible(duAn.GetQueryableSet(), DuAn)
        │
        ├─ BuildQuery(..., loai = TatCa)  → counter tongSoDuAn / conHan / quaHan / daHoanThanh
        │
        └─ BuildQuery(..., loai = search.Loai) → ProjectToDto → PaginatedList → danhSach
```

Filter màn hình phải nằm trong **`BuildQuery`** để counter và list dùng cùng subset.

---

## 3. Trạng thái code

### 3.1 Bản đã deploy (`main` — commit `6f8fa89`)

`TheoDoiDuAnTheoGiaiDoanSearchDto` chỉ có:

- `GiaiDoanId`, `NamDuAn`, `TenDuAn`, `MaDuAn`, `Loai`
- Kế thừa `CommonSearchDto` (`pageIndex`, `pageSize`, `globalFilter`, …)

`BuildQuery` **không** có điều kiện `LinhVucId` → gọi `?LinhVucId=1` **bị bỏ qua**.

### 3.2 WIP local (chưa commit)

Đã bổ sung 6 filter dashboard (ticket lớn hơn), trong đó có `LinhVucId`:

**DTO** — `TheoDoiDuAnTheoGiaiDoanSearchDto.cs`:

```csharp
/// <summary>Lĩnh vực. -1 = chưa gán.</summary>
public int? LinhVucId { get; set; }
```

**Handler** — `TheoDoiDuAnTheoGiaiDoanQuery.cs` → `BuildQuery`:

```csharp
.WhereFunc(search.LinhVucId.HasValue, q => q
    .WhereIf(search.LinhVucId > 0, e => e.LinhVucId == search.LinhVucId)
    .WhereIf(search.LinhVucId == -1, e => e.LinhVucId == null))
```

### 3.3 Vấn đề với WIP

| Case | WIP local (copy `DuAnGetDanhSach`) | Yêu cầu Issue 3 |
|------|-----------------------------------|-----------------|
| Không truyền | ✅ Không filter | ✅ Không filter |
| `linhVucId = 1` | ✅ `LinhVucId == 1` | ✅ `LinhVucId == 1` |
| `linhVucId = -1` | ❌ Chỉ dự án `LinhVucId == null` | ✅ **Tất cả** — không filter |

Dashboard FE gửi `-1` khi user chọn **“Tất cả”** trong combo lĩnh vực. Logic “chưa gán” của danh sách dự án **không** áp dụng ở đây.

---

## 4. Model dữ liệu

| Entity | Field | Kiểu | Ghi chú |
|--------|-------|------|---------|
| `DuAn` | `LinhVucId` | `int?` | FK → `DmLinhVuc` |

Filter trực tiếp trên `IQueryable<DuAn>` — **không** cần `Include` hay join bổ sung.

`ProjectToDto` hiện **không** map `LinhVucId` / tên lĩnh vực ra response — ngoài phạm vi issue (chỉ filter, không đổi DTO response).

---

## 5. Semantics `linhVucId` — chuẩn cho API này

| Input | Áp dụng WHERE? | Điều kiện |
|-------|----------------|-----------|
| `null` / không truyền | Không | — |
| `0` | Không | Coi như không chọn (giống các filter `> 0` khác trong `BuildQuery`) |
| `-1` | Không | **Tất cả** (sentinel UI) |
| `> 0` | Có | `e.LinhVucId == linhVucId` |

### So sánh với `GET /api/du-an/danh-sach`

| API | `linhVucId = -1` |
|-----|------------------|
| `danh-sach` (`DuAnGetDanhSachQuery`) | Dự án **chưa gán** lĩnh vực (`LinhVucId == null`) |
| `theo-doi-du-an-theo-giai-doan` | **Tất cả** — bỏ qua filter |

Hai API **cố ý** khác semantics sentinel `-1` vì UI khác nhau.

---

## 6. Files cần sửa

| # | File | Thay đổi |
|---|------|----------|
| 1 | `QLDA.Application/DuAns/DTOs/TheoDoiDuAnTheoGiaiDoanSearchDto.cs` | Thêm / sửa property `LinhVucId` |
| 2 | `QLDA.Application/DuAns/Queries/TheoDoiDuAnTheoGiaiDoanQuery.cs` | Thêm / sửa filter trong `BuildQuery` |

**Không sửa:** `DuAnController`, migration, `AppDbContextModelSnapshot`, WebApi model.

**Effort:** ~30 phút.

---

## 7. Bước code chi tiết

### Bước 1 — `TheoDoiDuAnTheoGiaiDoanSearchDto`

**File:** `QLDA.Application/DuAns/DTOs/TheoDoiDuAnTheoGiaiDoanSearchDto.cs`

**Trước (`main`):**

```csharp
    /// <summary>Mã dự án (contains)</summary>
    public string? MaDuAn { get; set; }

    /// <summary>Panel/tab đang chọn</summary>
    public ETheoDoiDuAnTheoGiaiDoanLoai Loai { get; set; } = ETheoDoiDuAnTheoGiaiDoanLoai.TatCa;
}
```

**Sau — thêm property `LinhVucId` trước `Loai`:**

```csharp
    /// <summary>Mã dự án (contains)</summary>
    public string? MaDuAn { get; set; }

    /// <summary>
    /// Lĩnh vực — DanhMucLinhVuc.
    /// &gt; 0: filter theo lĩnh vực; -1: Tất cả (không filter).
    /// </summary>
    public int? LinhVucId { get; set; }

    /// <summary>Panel/tab đang chọn</summary>
    public ETheoDoiDuAnTheoGiaiDoanLoai Loai { get; set; } = ETheoDoiDuAnTheoGiaiDoanLoai.TatCa;
}
```

**Nếu branch đã có WIP 6 filter** — chỉ **sửa XML doc** (đừng đổi tên property):

```diff
- /// <summary>Lĩnh vực. -1 = chưa gán.</summary>
+ /// <summary>
+ /// Lĩnh vực — DanhMucLinhVuc.
+ /// &gt; 0: filter theo lĩnh vực; -1: Tất cả (không filter).
+ /// </summary>
  public int? LinhVucId { get; set; }
```

**Controller `DuAnController` — không sửa.** ASP.NET Core bind tự động `?LinhVucId=1` → `searchDto.LinhVucId`.

---

### Bước 2 — `BuildQuery` trong `TheoDoiDuAnTheoGiaiDoanQuery`

**File:** `QLDA.Application/DuAns/Queries/TheoDoiDuAnTheoGiaiDoanQuery.cs`  
**Method:** `BuildQuery` (private static)

#### 2a. Trên `main` — chèn filter mới

**Trước (cuối chuỗi filter, ngay trước `WhereGlobalFilter`):**

```csharp
            .WhereIf(search.NamDuAn > 0,
                e => search.NamDuAn >= e.ThoiGianKhoiCong
                     && ((e.ThoiGianHoanThanh == null && e.ThoiGianKhoiCong == search.NamDuAn)
                         || search.NamDuAn <= e.ThoiGianHoanThanh))
            .WhereGlobalFilter(search, e => e.TenDuAn, e => e.MaDuAn);
```

**Sau — chèn **một dòng** `WhereIf` giữa `NamDuAn` và `WhereGlobalFilter`:**

```csharp
            .WhereIf(search.NamDuAn > 0,
                e => search.NamDuAn >= e.ThoiGianKhoiCong
                     && ((e.ThoiGianHoanThanh == null && e.ThoiGianKhoiCong == search.NamDuAn)
                         || search.NamDuAn <= e.ThoiGianHoanThanh))
            .WhereIf(
                search.LinhVucId.HasValue && search.LinhVucId.Value > 0,
                e => e.LinhVucId == search.LinhVucId!.Value)
            .WhereGlobalFilter(search, e => e.TenDuAn, e => e.MaDuAn);
```

#### 2b. Trên WIP (đã có 6 filter) — thay block `LinhVucId` sai

**Xóa** block copy từ `DuAnGetDanhSachQuery`:

```csharp
// ❌ XÓA — sai semantics dashboard
            .WhereFunc(search.LinhVucId.HasValue, q => q
                .WhereIf(search.LinhVucId > 0, e => e.LinhVucId == search.LinhVucId)
                .WhereIf(search.LinhVucId == -1, e => e.LinhVucId == null))
```

**Thay bằng:**

```csharp
// ✅ Dashboard: -1 / 0 / null = không filter
            .WhereIf(
                search.LinhVucId.HasValue && search.LinhVucId.Value > 0,
                e => e.LinhVucId == search.LinhVucId!.Value)
```

**Ngữ cảnh đầy đủ sau Bước 2 (WIP đã có 6 filter):**

```csharp
            .WhereIf(search.LoaiDuAnId > 0, e => e.LoaiDuAnId == search.LoaiDuAnId)
            .WhereFunc(search.DonViPhuTrachChinhId.HasValue, q => q
                .WhereIf(search.DonViPhuTrachChinhId > 0, e => e.DonViPhuTrachChinhId == search.DonViPhuTrachChinhId)
                .WhereIf(search.DonViPhuTrachChinhId == -1, e => e.DonViPhuTrachChinhId == null))
            .WhereIf(search.ThoiGianKhoiCong > 0, e => e.ThoiGianKhoiCong == search.ThoiGianKhoiCong)
            .WhereIf(search.ThoiGianHoanThanh > 0, e => e.ThoiGianHoanThanh == search.ThoiGianHoanThanh)
            .WhereIf(search.TrangThaiDuAnId > 0, e => e.TrangThaiDuAnId == search.TrangThaiDuAnId)
            .WhereIf(
                search.LinhVucId.HasValue && search.LinhVucId.Value > 0,
                e => e.LinhVucId == search.LinhVucId!.Value)
            .WhereGlobalFilter(search, e => e.TenDuAn, e => e.MaDuAn);
```

#### 2c. Lưu ý khi sửa `BuildQuery`

| Quy tắc | Chi tiết |
|---------|----------|
| Vị trí | Sau các filter màn hình khác, **trước** `.WhereGlobalFilter(...)` |
| Counter + list | `BuildQuery` gọi 2 lần trong `Handle` — **không** duplicate filter ở `Handle` |
| Auth | **Không** đụng `FilterVisible` ở đầu `Handle` |
| Soft-delete | **Không** thêm `.Where(e => !e.IsDeleted)` |
| `loai switch` | **Không** đổi block `ConHan` / `QuaHan` / `DaHoanThanh` phía dưới |
| Extension | `WhereIf` đã có sẵn trong project (cùng pattern `LoaiDuAnId`, `TrangThaiDuAnId`) |

**Diff tóm tắt:**

```diff
// TheoDoiDuAnTheoGiaiDoanSearchDto.cs
+ /// <summary>Lĩnh vực — DanhMucLinhVuc. &gt; 0: filter; -1: Tất cả.</summary>
+ public int? LinhVucId { get; set; }

// TheoDoiDuAnTheoGiaiDoanQuery.cs — BuildQuery
+ .WhereIf(
+     search.LinhVucId.HasValue && search.LinhVucId.Value > 0,
+     e => e.LinhVucId == search.LinhVucId!.Value)

// WIP only — xóa thêm:
- .WhereFunc(search.LinhVucId.HasValue, q => q
-     .WhereIf(search.LinhVucId > 0, e => e.LinhVucId == search.LinhVucId)
-     .WhereIf(search.LinhVucId == -1, e => e.LinhVucId == null))
```

---

### Bước 3 — Xác nhận Controller (read-only)

**File:** `QLDA.WebApi/Controllers/DuAnController.cs`

Endpoint đã bind đúng DTO — **không cần PR nào ở WebApi**:

```csharp
[HttpGet("api/du-an/theo-doi-du-an-theo-giai-doan")]
public async Task<ResultApi> GetTheoDoiDuAnTheoGiaiDoan(
    [FromQuery] TheoDoiDuAnTheoGiaiDoanSearchDto searchDto)
{
    var res = await Mediator.Send(new TheoDoiDuAnTheoGiaiDoanQuery(searchDto));
    return ResultApi.Ok(res);
}
```

Query string `LinhVucId` / `linhVucId` đều map được (ASP.NET Core case-insensitive).

---

### Bước 4 — Build

```powershell
cd e:\SER
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```

Kỳ vọng: **0 Error(s)**. Không cần `ef.bat` / migration.

---

### Bước 5 — Smoke test manual

Chạy WebApi local hoặc deploy lên `192.168.1.12:9051`, dùng token hợp lệ.

#### 5a. Baseline — không filter lĩnh vực

```bash
curl -G 'http://192.168.1.12:9051/QuanLyDuAn/api/du-an/theo-doi-du-an-theo-giai-doan' \
  --data-urlencode 'pageIndex=1' \
  --data-urlencode 'pageSize=10' \
  -H 'Authorization: Bearer <JWT_TOKEN>'
```

Ghi lại: `tongSoDuAn`, `danhSach.totalCount`, vài `id` đầu tiên.

#### 5b. Filter `LinhVucId=1`

```bash
curl -G 'http://192.168.1.12:9051/QuanLyDuAn/api/du-an/theo-doi-du-an-theo-giai-doan' \
  --data-urlencode 'pageIndex=1' \
  --data-urlencode 'pageSize=10' \
  --data-urlencode 'LinhVucId=1' \
  -H 'Authorization: Bearer <JWT_TOKEN>'
```

Kỳ vọng:

- `tongSoDuAn` ≤ baseline (thường nhỏ hơn nếu có dự án lĩnh vực khác)
- Mọi dự án trả về thuộc lĩnh vực `1` (verify qua DB: `SELECT Id, LinhVucId FROM DuAn WHERE Id IN (...)`)

#### 5c. Sentinel `LinhVucId=-1` (= Tất cả)

```bash
curl -G '...' --data-urlencode 'LinhVucId=-1' ...
```

Kỳ vọng: `tongSoDuAn` và `danhSach` **khớp** baseline (5a).

#### 5d. Kết hợp filter khác (regression)

```bash
curl -G '...' \
  --data-urlencode 'giaiDoanId=2' \
  --data-urlencode 'LinhVucId=1' \
  --data-urlencode 'loai=ConHan' ...
```

Kỳ vọng: list ⊆ (giai đoạn 2) ∩ (lĩnh vực 1) ∩ (panel ConHan); counter `conHan` đồng bộ.

#### 5e. Response mẫu (rút gọn)

```json
{
  "success": true,
  "data": {
    "tongSoDuAn": 12,
    "conHan": 8,
    "quaHan": 2,
    "daHoanThanh": 2,
    "danhSach": {
      "totalCount": 12,
      "pageIndex": 1,
      "pageSize": 10,
      "data": [ { "id": "...", "tenDuAn": "...", "maDuAn": "..." } ]
    }
  }
}
```

---

### Bước 6 — (Tùy chọn) Cập nhật guide #118

**File:** `docs/feature/DuAn/IMPLEMENTATION_GUIDE_118_theo-doi-du-an-theo-giai-doan.md`

Thêm `linhVucId` vào bảng request param (mục 5.2) với note `-1 = Tất cả`. Chỉ làm sau khi code merge.

---

## 8. Ví dụ request

```http
# Filter lĩnh vực id = 1
GET /QuanLyDuAn/api/du-an/theo-doi-du-an-theo-giai-doan
  ?pageIndex=1
  &pageSize=10
  &giaiDoanId=2
  &linhVucId=1

# Tất cả lĩnh vực (dashboard combo "Tất cả")
GET /QuanLyDuAn/api/du-an/theo-doi-du-an-theo-giai-doan
  ?pageIndex=1
  &pageSize=10
  &linhVucId=-1

# Không filter lĩnh vực
GET /QuanLyDuAn/api/du-an/theo-doi-du-an-theo-giai-doan
  ?pageIndex=1
  &pageSize=10
```

### Curl

```bash
curl -G 'http://192.168.1.12:9051/QuanLyDuAn/api/du-an/theo-doi-du-an-theo-giai-doan' \
  --data-urlencode 'pageIndex=1' \
  --data-urlencode 'pageSize=10' \
  --data-urlencode 'LinhVucId=1' \
  -H 'Authorization: Bearer <JWT_TOKEN>'
```

---

## 9. Test plan

| # | Case | Kỳ vọng |
|---|------|---------|
| T1 | Không gửi `linhVucId` | Kết quả **giống** trước khi có filter |
| T2 | `linhVucId=1` | Mọi dòng trong `danhSach` thuộc dự án có `DuAn.LinhVucId == 1`; counter giảm so với T1 |
| T3 | `linhVucId=-1` | Kết quả **giống** T1 (không filter) |
| T4 | `linhVucId=0` | Giống T1 (không filter) |
| T5 | `linhVucId=1` + `loai=ConHan` | List ⊆ subset ConHan **và** lĩnh vực 1 |
| T6 | `tongSoDuAn == conHan + quaHan + daHoanThanh` | Sau mọi combo filter ở T2, T5 |
| T7 | So với SQL/DB: `SELECT COUNT(*) FROM DuAn WHERE LinhVucId = 1 AND ...` | Counter `tongSoDuAn` khớp (trừ auth `FilterVisible`) |

### Build

```powershell
cd e:\SER
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```

---

## 10. Checklist trước merge

- [ ] `LinhVucId` trên `TheoDoiDuAnTheoGiaiDoanSearchDto` + doc `-1 = Tất cả`
- [ ] `BuildQuery` dùng `> 0` only — **không** nhánh `-1 == null`
- [ ] Counter 4 panel + `danhSach` cùng subset
- [ ] Không migration / không sửa WebApi controller
- [ ] Manual: `LinhVucId=1`, `-1`, không truyền — đúng 3 case
- [ ] `dotnet build` pass

---

## 11. Commit message đề xuất

```text
fix(du-an): apply linhVucId filter on theo-doi-du-an-theo-giai-doan API

- Add LinhVucId to search DTO; -1 means all (no filter) on dashboard
- Filter DuAn.LinhVucId in BuildQuery for counters and paginated list
```

---

## 12. Quan hệ với ticket lớn hơn

Issue này là **một phần** của [ke-hoach-trien-khai-hang-muc-import-fix](../ke-hoach-trien-khai-hang-muc-import-fix/index.md) (Phần 2 — 6 filter dashboard).

| Filter | Issue 3 | Ticket 6-filter |
|--------|---------|-----------------|
| `linhVucId` | ✅ Doc này — semantics dashboard | Có trong WIP, cần sửa `-1` |
| `loaiDuAnId`, `donViPhuTrachChinhId`, … | Ngoài phạm vi | WIP local |

Có thể merge chung 1 commit “6 filter” hoặc tách commit riêng `linhVucId` nếu review nhỏ gọn hơn.

---

*Document generated from codebase survey — June 30, 2026.*
