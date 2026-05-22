# Task #9460 – API danh sách file đã ký (`TepDinhKem`)

> **V2 (áp dụng)** — nguồn `TepDinhKem`; phân loại theo **`GroupType` = hiện hành / lịch sử**, không theo “lần ký đầu / lần ký cuối”.  
> **V1 (tham khảo, §6)** — query bảng `NoiDungDaKySo` riêng (đã bỏ).  
> Liên quan: [task-9460-noi-dung-da-ky.md](./task-9460-noi-dung-da-ky.md) (upload/ký lại), [task-9460-ky-so-crud.md](./task-9460-ky-so-crud.md).

---

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm

| # | Mục | Ghi chú |
|---|-----|---------|
| 1 | **1 API GET** danh sách file đã ký | Phân trang |
| 2 | Nguồn: **`TepDinhKem`** | Chỉ bản **đã ký** |
| 3 | Điều kiện cố định | `ParentId IS NOT NULL` |
| 4 | Filter 3 tham số (FE) + join `UserMaster` | `GroupType` cố định BE |

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

### 1.5 Đầu vào (3 tham số — FE)

| Tham số (spec) | Query param | Kiểu | Map `TepDinhKem` |
|----------------|-------------|------|------------------|
| Người ký | `createUserId` | `long?` | `CreatedBy == createUserId.ToString()` |
| Từ ngày | `tuNgay` | `DateOnly?` | `CreatedAt >= tuNgay.ToStartOfDayUtc()` |
| Đến ngày | `denNgay` | `DateOnly?` | `CreatedAt <= denNgay.ToEndOfDayUtc()` |

**`GroupType`:** BE cố định `IN ('KySo', 'NoiDungDaKySo')` — **FE không truyền**.

**Điều kiện ngày (spec):** `tuNgay <= CreateDate <= denNgay` → cột audit **`CreatedAt`**.

### 1.6 Đầu ra (map spec → `TepDinhKem`)

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

### 1.7 API

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
Bước 1: Application – NoiDungDaKySearchDto + `TepDinhKemDto` (response)
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
}
```

**Response:** dùng `TepDinhKemDto` (`GroupType`, `OriginalName`, …) — không tạo DTO riêng.

---

### Bước 2 – Query

**File:** `QLDA.Application/KySos/Queries/NoiDungDaKyGetDanhSachQuery.cs`

**Filter cốt lõi:**

```csharp
var signedGroupTypes = new[] {
    GroupTypeConstants.KySo,
    GroupTypeConstants.NoiDungDaKySo  // hoặc "NoiDungDaKySo" nếu constant chưa thêm
};

// Lịch sử: cả KySo + NoiDungDaKySo, kể cả IsDeleted (user ẩn nhưng vẫn xem lịch sử)
var query = _tepDinhKemRepository.GetQueryableSet(OnlyNotDeleted: false)
    .AsNoTracking()
    .Where(e => e.ParentId != null)
    .Where(e => signedGroupTypes.Contains(e.GroupType))
    .WhereIf(search.CreateUserId.HasValue,
        e => e.CreatedBy == search.CreateUserId!.Value.ToString())
    .WhereIf(search.TuNgay.HasValue,
        e => e.CreatedAt >= search.TuNgay!.Value.ToStartOfDayUtc())
    .WhereIf(search.DenNgay.HasValue,
        e => e.CreatedAt <= search.DenNgay!.Value.ToEndOfDayUtc());
```

`Select` → `TepDinhKemDto`.

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
-- API lịch sử: không lọc IsDeleted; cả 2 GroupType
SELECT Id, ParentId, FileName, GroupType, IsDeleted, CreatedAt, CreatedBy
FROM TepDinhKem
WHERE ParentId IS NOT NULL
  AND GroupType IN ('KySo', 'NoiDungDaKySo')
  -- 2026-05-21 00:00 VN  →  2026-05-20 17:00:00 UTC
  AND CreatedAt >= '2026-05-20T17:00:00.0000000+00:00'
  -- 2026-05-21 23:59:59 VN  →  2026-05-21 16:59:59 UTC
  AND CreatedAt <= '2026-05-21T16:59:59.0000000+00:00'
  AND CreatedBy = '4'
ORDER BY CreatedAt DESC;
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
| `GroupName` output | Cột `GroupName` | `GroupType` |
| `FileOrginal` | Cột `FileOrginal` | `OriginalName` |
| Đồng bộ upload | Insert 2 nơi | Chỉ `TepDinhKem` / `NoiDungDaKyCommand` |

---

## 7. Checklist

```
[ ] 1. NoiDungDaKySearchDto + TepDinhKemDto (response)
[ ] 2. NoiDungDaKyGetDanhSachQuery — ParentId != null + GroupType ký số
[ ] 3. KySoController GET noi-dung-da-ky/danh-sach
[ ] 4. GroupTypeConstants.NoiDungDaKySo (nếu V2 upload đã merge)
[ ] 5. Build + Postman
```

---

## 8. Lưu ý

- API này = **lịch sử ký số** (cả `KySo` + `NoiDungDaKySo`, kể cả `IsDeleted`). Không dùng để lấy **một** file hiện hành — màn đối tượng filter `GroupType = 'KySo'` AND `IsDeleted = false`.
- API này **không** thay thế list file gốc (`ParentId == null`) trên màn hình đối tượng (Gói thầu, Hợp đồng, …).
- **Handler upload** (`NoiDungDaKyCommand`): ký lại → `parent.GroupType = NoiDungDaKySo`, bản mới `KySo`; **không** tự `IsDeleted` khi ký lại (xem [task-9460-noi-dung-da-ky.md](./task-9460-noi-dung-da-ky.md) §1.3).
- **Query list:** `GetQueryableSet(OnlyNotDeleted: false)` — user “ký đời” (`IsDeleted = true`) vẫn thấy trong lịch sử.
- Khi drop bảng `NoiDungDaKySo` (V1), **không** đổi contract API list V2.
- `CreateUserName`: join `UserMaster` qua `CreatedBy` ↔ `UserPortalId` (đã dùng trong handler).

---

## 9. Trạng thái

- ✅ Doc V2 — hiện hành / lịch sử theo `GroupType`; API list lấy cả 2 type + `IsDeleted`
- ⏳ Checklist §7 — build + Postman
