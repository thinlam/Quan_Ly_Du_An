# Task #9460 – API danh sách file đã ký (`TepDinhKem`)

> **V2 (áp dụng)** — nguồn `TepDinhKem`; phân loại theo **`GroupType` = hiện hành / lịch sử**, không theo “lần ký đầu / lần ký cuối”.  
> **V1 (tham khảo, §6)** — query bảng `NoiDungDaKySo` riêng (đã bỏ).  
> Liên quan: [task-9460-noi-dung-da-ky.md](./task-9460-noi-dung-da-ky.md) (upload/ký lại), [task-9460-ky-so-crud.md](./task-9460-ky-so-crud.md), [task-export-excel-noi-dung-da-ky.md](./task-export-excel-noi-dung-da-ky.md) (export Excel + filter chuẩn hóa).

**Trạng thái:** ✅ **IMPLEMENTED** — cập nhật 06/07/2026

---

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm

| # | Mục | Ghi chú |
|---|-----|---------|
| 1 | **1 API GET** danh sách file đã ký | Phân trang |
| 2 | Nguồn: **`TepDinhKem`** | Chỉ bản **đã ký** |
| 3 | Điều kiện cố định | `ParentId IS NOT NULL` |
| 4 | Filter FE + join `UserMaster` | `GroupType` cố định BE |

### 1.2 `GroupType` — hiện hành vs lịch sử (không phải “lần đầu / lần cuối”)

| `GroupType` | Vai trò |
|-------------|---------|
| **`KySo`** | Bản **đang hiển thị** / hiện hành (luôn tối đa **một** bản active trên một chuỗi ký) |
| **`NoiDungDaKySo`** | Bản **lịch sử** (các phiên bản đã thay thế) |

**Ví dụ:** File ký số sai → upload bản ký mới:

| Bản | `GroupType` | Ghi chú |
|-----|-------------|---------|
| File ký ban đầu (sai) | `NoiDungDaKySo` | Chuyển sang lịch sử khi có bản mới |
| File ký mới nhất | `KySo` | Hiện hành |

**N file ký:** FE/màn chi tiết lấy **`KySo`** + `IsDeleted = false` cho file đang dùng; tab lịch sử lấy **cả hai** `GroupType`.

### 1.3 Định nghĩa API danh sách (lịch sử ký số)

| Điều kiện | Ý nghĩa |
|-----------|---------|
| `ParentId != null` | Chỉ bản tệp đã ký (con); **không** lấy file gốc (`ParentId == null`) |
| `GroupType IN ('KySo', 'NoiDungDaKySo')` | **Cả** hiện hành và lịch sử — bắt buộc lấy 2 type |
| `GetQueryableSet(OnlyNotDeleted: false)` | **Bao gồm** `IsDeleted = true` — xem §1.4 |

```
File gốc (GoiThau, …)              ParentId = null        → KHÔNG có trong API lịch sử
  └─ ký lần 1                      GroupType = KySo       → CÓ (hiện hành)
  └─ ký lại: bản cũ → lịch sử      GroupType = NoiDungDaKySo
       bản mới → hiện hành         GroupType = KySo
```

### 1.4 `IsDeleted`

| Trường hợp | Hành vi |
|------------|---------|
| Ký lại / thay bản | **Không** tự `IsDeleted` bản cũ — chỉ đổi `GroupType` bản cũ → `NoiDungDaKySo`, bản mới → `KySo` |
| User “ký đời” / ẩn khỏi hiện hành | `IsDeleted = true` thủ công — **vẫn** hiện trong API **lịch sử** (`OnlyNotDeleted: false`) |
| Lấy file **hiện hành** (màn đối tượng) | `GroupType = 'KySo'` AND `IsDeleted = false` (query riêng, không phải API này) |

### 1.5 Đầu vào (query params — FE)

```
GET /api/ky-so/noi-dung-da-ky/danh-sach
  ?nguoiKyId=
  &tuNgay=
  &denNgay=
  &duAnId=
  &globalFilter=
  &pageIndex=
  &pageSize=
```

| Tham số | Kiểu | Map / hành vi |
|---------|------|----------------|
| `nguoiKyId` | `long?` | `CreatedBy == nguoiKyId.ToString()` — **`UserPortalId`**, không phải `USER_MASTER.Id` |
| `tuNgay` | `DateOnly?` | `CreatedAt >= tuNgay.ToStartOfDayUtc()` — xem §1.5.1 |
| `denNgay` | `DateOnly?` | `CreatedAt <= denNgay.ToEndOfDayUtc()` — xem §1.5.1 |
| `duAnId` | `Guid?` | `GroupId IN (...)` — resolve qua `DuAnTepDinhKemGroupIdQueryExtensions` |
| `globalFilter` | `string?` | OR: `FileName`, `OriginalName`, `UserMaster.HoTen` (sau join, in-memory) |
| `pageIndex`, `pageSize` | int | Phân trang (`AggregateRootPagination`) |

**`GroupType`:** BE cố định `IN ('KySo', 'NoiDungDaKySo')` — **FE không truyền**.

**Điều kiện ngày:** `tuNgay <= CreatedAt <= denNgay` (cột audit **`CreatedAt`**).

#### 1.5.1 Default khoảng ngày & bind `dd-MM`

| Tình huống | `tuNgay` | `denNgay` |
|------------|----------|-----------|
| FE **không** truyền cả hai | `hôm nay − 1 năm` | `hôm nay` |
| Chỉ có `denNgay` | `denNgay − 1 năm` | giá trị FE |
| Chỉ có `tuNgay` | giá trị FE | `hôm nay` |

**Bind ngày** (`NoiDungDaKyDateModelBinder` — chỉ áp dụng cho `NoiDungDaKySearchDto`):

| Format query | Ý nghĩa |
|--------------|---------|
| `dd-MM-yyyy`, `dd/MM/yyyy`, `yyyy-MM-dd` | Ngày đầy đủ |
| `dd-MM` / `dd/MM` trên **`tuNgay`** | Cùng ngày-tháng, **năm trước** |
| `dd-MM` / `dd/MM` trên **`denNgay`** (hoặc bỏ trống) | **Hôm nay** |

Logic resolve: `NoiDungDaKyQueryableExtensions.ResolveDateRange`.

### 1.6 Đầu ra (`TepDinhKemDto`)

| Cột UI / spec | Field response | Nguồn |
|---------------|----------------|--------|
| Tên file | `FileName` | `TepDinhKem.FileName` |
| Tên gốc | `OriginalName` | `TepDinhKem.OriginalName` |
| Loại file | `Type` | `TepDinhKem.Type` |
| Dung lượng | `Size` | `TepDinhKem.Size` |
| Nhóm / đối tượng | `GroupId`, `GroupType` | `TepDinhKem` |
| Người tạo / ký | `TenNguoiTao` | Join `UserMaster.HoTen` qua `CreatedBy` ↔ `UserPortalId` |
| Id, ParentId | `Id`, `ParentId` | `TepDinhKem` |

`CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt` có trên DTO nhưng **`[JsonIgnore]`** — FE dùng `TenNguoiTao` cho tên người ký.

### 1.7 API

```
GET /api/ky-so/noi-dung-da-ky/danh-sach
```

Đặt trên **`KySoController`** (cùng nhóm upload/ký file), không dùng `QuanLyKySoController` (bảng `KySo` chứng thư).

**Export Excel** (cùng filter, không phân trang): `GET /api/print/danh-sach-noi-dung-da-ky` — chi tiết [task-export-excel-noi-dung-da-ky.md](./task-export-excel-noi-dung-da-ky.md).

---

## 2. Hiện trạng (sau implement)

| Thành phần | Trạng thái | File |
|------------|------------|------|
| `POST /api/ky-so/them-moi` | ✅ | Upload/ký file |
| `GET /api/ky-so/noi-dung-da-ky/danh-sach` | ✅ | `KySoController` |
| `GET /api/print/danh-sach-noi-dung-da-ky` | ✅ | `PrintController` |
| Filter dùng chung list + export | ✅ | `NoiDungDaKyQueryableExtensions` |
| Lọc theo dự án (`duAnId`) | ✅ | `DuAnTepDinhKemGroupIdQueryExtensions` |
| Bind ngày `dd-MM` | ✅ | `NoiDungDaKyDateModelBinder` |
| Bảng `NoiDungDaKySo` | V1 — **không dùng** | Có thể drop sau |
| `GroupTypeConstants.NoiDungDaKySo` | ✅ | `QLDA.Domain.Constants` |

---

## 3. Kiến trúc code

```
KySoController.GetNoiDungDaKyList
  └─ NoiDungDaKyGetDanhSachQuery
       └─ NoiDungDaKyQueryableExtensions.ApplyFiltersAsync  ← filter + join UserMaster
            ├─ ResolveDateRange (default 1 năm)
            ├─ DuAnTepDinhKemGroupIdQueryExtensions.ResolveGroupIdsAsync (nếu có duAnId)
            ├─ Query TepDinhKem (ParentId, GroupType, nguoiKyId, GroupId)
            ├─ Lọc CreatedAt in-memory
            ├─ Load UserMaster chỉ CreatedBy có trong danh sách file
            └─ globalFilter (FileName | OriginalName | HoTen)
       └─ Map → TepDinhKemDto → PaginatedList
```

**Không** migration. **Không** WebApi Model cho search — dùng DTO Application.

---

## 4. Chi tiết implementation

### 4.1 DTOs

**`NoiDungDaKySearchDto`** — kế thừa `CommonSearchDto` (`TuNgay`, `DenNgay`, `DuAnId`, `GlobalFilter`, …):

```csharp
public record NoiDungDaKySearchDto : CommonSearchDto {
    /// <summary>Người ký — UserPortalId (map TepDinhKem.CreatedBy).</summary>
    public long? NguoiKyId { get; set; }
}
```

**Response:** `TepDinhKemDto` — không tạo DTO riêng cho list.

### 4.2 Filter dùng chung

**File:** `QLDA.Application/KySos/Queries/NoiDungDaKyQueryableExtensions.cs`

| Bước | Mô tả |
|------|--------|
| 1 | `ResolveDateRange` — default 1 năm nếu không có ngày |
| 2 | `ResolveGroupIdsAsync` khi `duAnId` có giá trị |
| 3 | Query DB: `ParentId != null`, `GroupType ∈ {KySo, NoiDungDaKySo}`, `CreatedBy`, `GroupId` |
| 4 | Lọc `CreatedAt` theo `tuNgay` / `denNgay` (in-memory) |
| 5 | Lấy `CreatedBy` distinct từ danh sách file → query `UserMaster` **chỉ** `UserPortalId` trong tập đó |
| 6 | Join → `NoiDungDaKyJoinedRow`; `globalFilter` OR tên file / tên gốc / họ tên |
| 7 | `OrderByDescending(CreatedAt)` |

**Convention `CreatedBy`:** lưu **`UserPortalId`** (JWT `UserId`), join `UserMaster` qua `UserPortalId.ToString()`.

### 4.3 Query handler

**File:** `QLDA.Application/KySos/Queries/NoiDungDaKyGetDanhSachQuery.cs`

```csharp
var rows = await _tepDinhKemRepository
    .GetQueryableSet(OnlyNotDeleted: false, OrderByIndex: false)
    .AsNoTracking()
    .ApplyFiltersAsync(search, users, serviceProvider, _clock, cancellationToken);

return PaginatedList<TepDinhKemDto>.Create(dtos, request.Skip(), request.Take());
```

### 4.4 Controller

**File:** `QLDA.WebApi/Controllers/KySoController.cs`

```csharp
[HttpGet("noi-dung-da-ky/danh-sach")]
public async Task<ResultApi> GetNoiDungDaKyList(
    [FromQuery] NoiDungDaKySearchDto searchDto,
    [FromQuery] AggregateRootPagination pagination)
{
    var res = await Mediator.Send(new NoiDungDaKyGetDanhSachQuery(searchDto) {
        PageIndex = pagination.PageIndex,
        PageSize = pagination.PageSize,
    });
    return ResultApi.Ok(res);
}
```

### 4.5 Bind ngày

**File:** `QLDA.WebApi/ModelBinding/NoiDungDaKyDateModelBinder.cs`  
Đăng ký trong `WebApplicationExtensions` (`NoiDungDaKyDateModelBinderProvider`).

### 4.6 Lọc theo dự án

**File:** `QLDA.Application/DuAns/Queries/DuAnTepDinhKemGroupIdQueryExtensions.cs`  
Thu thập mọi `GroupId` (string) của entity thuộc `DuAnId` → `WHERE GroupId IN (...)`.

---

## 5. SQL tham khảo

```sql
-- API lịch sử: không lọc IsDeleted; cả 2 GroupType
SELECT Id, ParentId, FileName, GroupType, IsDeleted, CreatedAt, CreatedBy
FROM TepDinhKem
WHERE ParentId IS NOT NULL
  AND GroupType IN ('KySo', 'NoiDungDaKySo')
  AND CreatedAt >= @tuNgayStartUtc
  AND CreatedAt <= @denNgayEndUtc
  AND (@nguoiKyId IS NULL OR CreatedBy = CAST(@nguoiKyId AS NVARCHAR))
  AND (@duAnId IS NULL OR GroupId IN (...))
ORDER BY CreatedAt DESC;

-- UserMaster: chỉ load UserPortalId có trong CreatedBy của kết quả trên
SELECT * FROM USER_MASTER
WHERE User_PortalID IS NOT NULL
  AND CAST(User_PortalID AS NVARCHAR) IN (...);
```

---

## 6. So sánh V1 (giữ tham khảo) vs V2 (áp dụng)

| | V1 (cũ) | V2 (đúng) |
|---|---------|-----------|
| Bảng | `NoiDungDaKySo` | `TepDinhKem` |
| Nhận diện file đã ký | Có row trong bảng riêng | `ParentId IS NOT NULL` |
| Ý nghĩa `GroupType` | Hay hiểu nhầm **lần đầu = KySo, ký lại = NoiDungDaKySo** (bản **mới** gán `NoiDungDaKySo`) | **Hiện hành = `KySo`**, **lịch sử = `NoiDungDaKySo`** (bản **cũ** demote khi ký lại) |
| Ký lại | Tự `IsDeleted` bản cũ | Chỉ đổi `GroupType` bản cũ; `IsDeleted` chỉ khi user ẩn thủ công |
| API lịch sử | Chỉ bảng `NoiDungDaKySo` | `GroupType IN ('KySo','NoiDungDaKySo')`, `OnlyNotDeleted: false` |
| Tên người ký | Cột riêng | `TenNguoiTao` ← join `UserMaster.HoTen` |
| Đồng bộ upload | Insert 2 nơi | Chỉ `TepDinhKem` / `NoiDungDaKyCommand` |

---

## 7. Checklist

```
[x] 1. NoiDungDaKySearchDto + TepDinhKemDto (response)
[x] 2. NoiDungDaKyQueryableExtensions — filter dùng chung + default 1 năm
[x] 3. NoiDungDaKyGetDanhSachQuery — ParentId != null + GroupType ký số
[x] 4. KySoController GET noi-dung-da-ky/danh-sach
[x] 5. NoiDungDaKyDateModelBinder (dd-MM / dd-MM-yyyy)
[x] 6. duAnId → DuAnTepDinhKemGroupIdQueryExtensions
[x] 7. Export Excel (task-export-excel-noi-dung-da-ky.md)
[x] 8. Integration tests — NoiDungDaKyExportTests
```

---

## 8. Lưu ý

- API này = **lịch sử ký số** (cả `KySo` + `NoiDungDaKySo`, kể cả `IsDeleted`). Không dùng để lấy **một** file hiện hành — màn đối tượng filter `GroupType = 'KySo'` AND `IsDeleted = false`.
- API này **không** thay thế list file gốc (`ParentId == null`) trên màn hình đối tượng (Gói thầu, Hợp đồng, …).
- **Handler upload** (`NoiDungDaKyCommand`): ký lại → `parent.GroupType = NoiDungDaKySo`, bản mới `KySo`; **không** tự `IsDeleted` khi ký lại (xem [task-9460-noi-dung-da-ky.md](./task-9460-noi-dung-da-ky.md) §1.3).
- **Query list:** `GetQueryableSet(OnlyNotDeleted: false)` — user “ký đời” (`IsDeleted = true`) vẫn thấy trong lịch sử.
- **`TenNguoiTao`:** `TepDinhKem.CreatedBy` = **`UserPortalId`**; join `UserMaster.UserPortalId` — **không** dùng `USER_MASTER.Id`.
- **UserMaster:** chỉ load user có `UserPortalId` nằm trong `CreatedBy` của file đã lọc (không `ToList` cả bảng).
- **`globalFilter`:** áp dụng sau join, in-memory — list và export dùng chung logic `ApplyFiltersAsync`.
- Khi drop bảng `NoiDungDaKySo` (V1), **không** đổi contract API list V2.

---

## 9. Trạng thái

| Mục | Trạng thái |
|-----|------------|
| Doc V2 — hiện hành / lịch sử theo `GroupType` | ✅ |
| API list + filter (ngày, người ký, dự án, keyword) | ✅ |
| Export Excel cùng filter | ✅ |
| Test integration | ✅ `QLDA.Tests/Integration/NoiDungDaKyExportTests.cs` |
