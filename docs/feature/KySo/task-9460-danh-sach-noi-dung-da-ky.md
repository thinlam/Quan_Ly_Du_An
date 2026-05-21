# Task #9460 – API danh sách file đã ký (`TepDinhKem`)

> **Thiết kế V2** — **không** query bảng `NoiDungDaKySo` (bảng này sẽ bỏ / đã bỏ khi refactor [task-9460-noi-dung-da-ky.md](./task-9460-noi-dung-da-ky.md) V2).  
> Liên quan: [task-9460-ky-so-crud.md](./task-9460-ky-so-crud.md), [task-fix-datetime-datetimeoffset-dateonly.md](./task-fix-datetime-datetimeoffset-dateonly.md).

---

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm

| # | Mục | Ghi chú |
|---|-----|---------|
| 1 | **1 API GET** danh sách file đã ký | Phân trang |
| 2 | Nguồn: **`TepDinhKem`** | Chỉ bản **đã ký** |
| 3 | Điều kiện cố định | `ParentId IS NOT NULL` |
| 4 | Filter 4 tham số + join `UserMaster` | Giống spec UC |

### 1.2 Định nghĩa “file đã ký”

| Điều kiện | Ý nghĩa |
|-----------|---------|
| `ParentId != null` | Bản tệp con — trỏ tới file gốc trước khi ký; **không** lấy file gốc (`ParentId == null`) |
| `GroupType IN ('KySo', 'NoiDungDaKySo')` | Ký lần đầu hoặc ký lại (V2 trên `TepDinhKem`) |
| `IsDeleted = false` | Mặc định qua `GetQueryableSet()` |

```
File gốc (GoiThau, …)     ParentId = null     → KHÔNG có trong list API này
  └─ ký lần 1 (KySo)      ParentId = id gốc   → CÓ trong list
  └─ ký lại (NoiDungDaKySo) ParentId = id bản ký trước → CÓ trong list
```

### 1.3 Đầu vào (4 tham số)

| Tham số (spec) | Query param | Kiểu | Map `TepDinhKem` |
|----------------|-------------|------|------------------|
| Người ký | `createUserId` | `long?` | `CreatedBy == createUserId.ToString()` |
| Từ ngày | `tuNgay` | `DateOnly?` | `CreatedAt >= tuNgay.ToStartOfDayUtc()` |
| Đến ngày | `denNgay` | `DateOnly?` | `CreatedAt <= denNgay.ToEndOfDayUtc()` |
| Loại | `groupType` | `string?` | `GroupType == groupType` (nếu null → không lọc thêm, vẫn giữ `IN (KySo, NoiDungDaKySo)`) |

**Điều kiện ngày (spec):** `tuNgay <= CreateDate <= denNgay` → cột audit **`CreatedAt`**.

### 1.4 Đầu ra (map spec → `TepDinhKem`)

| Output (spec) | DTO | Cột `TepDinhKem` |
|---------------|-----|------------------|
| `FileName` | `FileName` | `FileName` |
| `FileOrginal` | `FileOrginal` | `OriginalName` |
| `GroupId` | `GroupId` | `GroupId` |
| `GroupName` | `GroupName` | `GroupType` |
| `CreateUserName` | `CreateUserName` | Join `UserMaster.HoTen` qua `CreatedBy` |

**Bổ sung hữu ích:**

| DTO | Nguồn |
|-----|--------|
| `Id` | `TepDinhKem.Id` |
| `ParentId` | `ParentId` |
| `CreateUserId` | `CreatedBy` → `long?` |
| `CreateDate` | `CreatedAt.ToDateOnlyVn()` |

### 1.5 API

```
GET /api/ky-so/noi-dung-da-ky/danh-sach
```

Đặt trên **`KySoController`** (cùng nhóm upload/ký file), không dùng `QuanLyKySoController` (bảng `KySo` chứng thư).

---

## 2. Hiện trạng

| Thành phần | Trạng thái |
|------------|------------|
| `POST /api/ky-so/them-moi` | Insert `TepDinhKem` (file có `ParentId`) |
| Bảng `NoiDungDaKySo` | V1 — **không dùng** cho API list; có thể drop sau |
| `GET` danh sách file đã ký | ❌ Chưa có |
| `GroupTypeConstants.NoiDungDaKySo` | Cần có khi V2 upload ký lại (nếu chưa có thì dùng literal `"NoiDungDaKySo"`) |

---

## 3. Thứ tự thực hiện

```
Bước 1: Application – NoiDungDaKySearchDto + NoiDungDaKyDto (tên giữ theo UC, nguồn TepDinhKem)
Bước 2: Application – NoiDungDaKyGetDanhSachQuery (filter ParentId + GroupType)
Bước 3: WebApi – KySoController GET
Bước 4: Build + Postman
```

**Không** migration. **Không** WebApi Model.

---

## 4. Chi tiết implementation

### Bước 1 – DTOs

**`NoiDungDaKySearchDto.cs`** — kế thừa `CommonSearchDto` (`TuNgay`, `DenNgay` đã có):

```csharp
public class NoiDungDaKySearchDto : CommonSearchDto {
    public long? CreateUserId { get; set; }
    /// <summary>Loại — map TepDinhKem.GroupType. Null = mọi loại ký số (KySo + NoiDungDaKySo).</summary>
    public string? GroupType { get; set; }
}
```

**`NoiDungDaKyDto.cs`:**

```csharp
public class NoiDungDaKyDto {
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string? FileName { get; set; }
    public string? FileOrginal { get; set; }
    public string? GroupId { get; set; }
    public string? GroupName { get; set; }
    public long? CreateUserId { get; set; }
    public string? CreateUserName { get; set; }
    public DateOnly? CreateDate { get; set; }
}
```

---

### Bước 2 – Query

**File:** `QLDA.Application/KySos/Queries/NoiDungDaKyGetDanhSachQuery.cs`

**Filter cốt lõi:**

```csharp
var signedGroupTypes = new[] {
    GroupTypeConstants.KySo,
    GroupTypeConstants.NoiDungDaKySo  // hoặc "NoiDungDaKySo" nếu constant chưa thêm
};

var query = _tepDinhKemRepository.GetQueryableSet()
    .AsNoTracking()
    .Where(e => e.ParentId != null)
    .Where(e => signedGroupTypes.Contains(e.GroupType))
    .WhereIf(search.CreateUserId.HasValue,
        e => e.CreatedBy == search.CreateUserId!.Value.ToString())
    .WhereIf(search.TuNgay.HasValue,
        e => e.CreatedAt >= search.TuNgay!.Value.ToStartOfDayUtc())
    .WhereIf(search.DenNgay.HasValue,
        e => e.CreatedAt <= search.DenNgay!.Value.ToEndOfDayUtc())
    .WhereIf(!string.IsNullOrWhiteSpace(search.GroupType),
        e => e.GroupType == search.GroupType);
```

Join `UserMaster` + `Select` → `NoiDungDaKyDto` (pattern `BanGiaoHoSoGetDanhSachQuery`).

---

### Bước 3 – Controller

```csharp
[HttpGet("noi-dung-da-ky/danh-sach")]
public async Task<ResultApi> GetNoiDungDaKyList(
    [FromQuery] NoiDungDaKySearchDto searchDto,
    [FromQuery] AggregateRootPagination pagination) {
    var res = await Mediator.Send(new NoiDungDaKyGetDanhSachQuery(searchDto) {
        PageIndex = pagination.PageIndex,
        PageSize = pagination.PageSize,
    });
    return ResultApi.Ok(res);
}
```

---

## 5. SQL tham khảo

```sql
SELECT Id, ParentId, FileName, CreatedAt, CreatedBy
FROM TepDinhKem
WHERE IsDeleted = 0
  AND ParentId IS NOT NULL
  AND GroupType IN ('KySo', 'NoiDungDaKySo')
  -- 2026-05-21 00:00 VN  →  2026-05-20 17:00:00 UTC
  AND CreatedAt >= '2026-05-20T17:00:00.0000000+00:00'
  -- 2026-05-21 23:59:59 VN  →  2026-05-21 16:59:59 UTC
  AND CreatedAt <= '2026-05-21T16:59:59.0000000+00:00'
  AND CreatedBy = '4'
ORDER BY CreatedAt DESC;
```

---

## 6. So sánh V1 (bỏ) vs V2 (áp dụng)

| | V1 (cũ — doc sai) | V2 (đúng) |
|---|-------------------|-----------|
| Bảng | `NoiDungDaKySo` | `TepDinhKem` |
| Nhận diện file đã ký | Có row trong bảng riêng | `ParentId IS NOT NULL` |
| `GroupName` output | Cột `GroupName` | `GroupType` |
| `FileOrginal` | Cột `FileOrginal` | `OriginalName` |
| Đồng bộ upload | Insert 2 nơi | Chỉ `TepDinhKem` / `KySoThemMoiCommand` |

---

## 7. Checklist

```
[ ] 1. NoiDungDaKySearchDto + NoiDungDaKyDto
[ ] 2. NoiDungDaKyGetDanhSachQuery — ParentId != null + GroupType ký số
[ ] 3. KySoController GET noi-dung-da-ky/danh-sach
[ ] 4. GroupTypeConstants.NoiDungDaKySo (nếu V2 upload đã merge)
[ ] 5. Build + Postman
```

---

## 8. Lưu ý

- API này **không** thay thế list file gốc (`ParentId == null`) trên màn hình đối tượng (Gói thầu, Hợp đồng, …).
- Khi drop bảng `NoiDungDaKySo`, **không** sửa task list — chỉ đảm bảo `POST them-moi` không còn `NoiDungDaKyInsertCommand`.
- `CreateUserName`: xác nhận `CreatedBy` lưu `UserMaster.Id` hay `UserPortalId` trước khi join.

---

## 9. Trạng thái

- ⏳ Doc V2 (TepDinhKem) — cập nhật theo yêu cầu drop `NoiDungDaKySo`
