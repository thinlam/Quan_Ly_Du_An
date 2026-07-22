# Bug: API chi tiết Tờ trình có thẩm định thiếu file ký số

> Cập nhật: 22/07/2026  
> Trạng thái: 🟡 Đã sửa code (cần verify build + API)  
> Module: QLDA — `ToTrinhCoThamDinh`

---

## 1. Triệu chứng

Hai API cùng entity `ToTrinhCoThamDinh` trả về danh sách tệp đính kèm **không nhất quán**:

| API | Endpoint | Kết quả |
|-----|----------|---------|
| Danh sách | `GET /api/to-trinh-co-tham-dinh/danh-sach-tien-do` | ✅ Có file ký số (`.signed.pdf`) |
| Chi tiết | `GET /api/to-trinh-co-tham-dinh/{id}/chi-tiet` | ❌ Không có file ký số |

### Ví dụ thực tế

**Request danh sách** (có ký số):

```
GET /api/to-trinh-co-tham-dinh/danh-sach-tien-do
    ?pageIndex=1&pageSize=10
    &duAnId=08dedfdb-50a1-3067-687a-7b122003ce93
    &buocId=6898
```

Trong `danhSachTepDinhKem` của bản ghi `groupId = e2ddf27a-a3d5-412a-9819-608fc7280d0f`:

```json
{
  "id": "e6cfc44d-8f57-4bc4-a67c-9ba5ec6e9114",
  "groupId": "e2ddf27a-a3d5-412a-9819-608fc7280d0f",
  "groupType": "KySo_QuyetDinhKeHoachThue",
  "fileName": "DanhSach_ChiTietThongKePhongBan_220720260943_220720261451047.signed.pdf",
  "parentId": "e6cfc44d-8f57-4bc4-a67c-9ba5ec6e9114"
}
```

**Request chi tiết** (thiếu ký số):

```
GET /api/to-trinh-co-tham-dinh/e2ddf27a-a3d5-412a-9819-608fc7280d0f/chi-tiet
```

Trong `danhSachTepDinhKem` chỉ còn file gốc:

```json
{
  "id": "e6cfc44d-8f57-4bc4-a67c-9ba5ec6e9114",
  "groupId": "e2ddf27a-a3d5-412a-9819-608fc7280d0f",
  "groupType": "QuyetDinhKeHoachThue",
  "fileName": "DanhSach_ChiTietThongKePhongBan_220720260943_220720261451047.xlsx",
  "parentId": null
}
```

→ Cùng một `groupId`, nhưng API chi tiết **không trả về** bản ký số `KySo_QuyetDinhKeHoachThue`.

---

## 2. Nguyên nhân gốc (Root cause)

### 2.1. Cơ chế lưu file ký số

Khi user ký số một tệp, hệ thống tạo bản ghi `Attachment` mới với:

| Trường | File gốc | File ký số |
|--------|----------|------------|
| `GroupId` | `{ToTrinhCoThamDinh.Id}` | `{ToTrinhCoThamDinh.Id}` (giữ nguyên) |
| `GroupType` | `QuyetDinhKeHoachThue` | `KySo_QuyetDinhKeHoachThue` |
| `ParentId` | `null` | `Id` của file gốc |

Logic gán prefix `KySo_` nằm tại `TepDinhKemMappingConfigurations.ResolveGroupType()`:

```csharp
// File con (ký số - ParentId != null): thêm prefix KySo_ nếu chưa có
return resolved.StartsWith(KySoPrefix, StringComparison.Ordinal)
    ? resolved
    : $"{KySoPrefix}{resolved}";
```

Helper tương ứng: `QLDA.Application/Common/SignedHelper.cs`

```csharp
public const string Prefix = "KySo_";

public static string[] WithSignedVariant(this string baseGroupType)
    => [baseGroupType, Prefix + baseGroupType];
```

### 2.2. API danh sách — load **tất cả** attachment theo GroupId

File: `QLDA.Application/ToTrinhCoThamDinh/Queries/ToTrinhCoThamDinhGetPaginatedQuery.cs`

```csharp
DanhSachTepDinhKem = _tepDinhKem.GetQueryableSet()
    .Where(i => i.GroupId == e.Id.ToString())   // ← Không lọc GroupType
    .Select(i => i.ToDto()).ToList(),
```

→ Lấy **mọi** `GroupType` thuộc `GroupId`, bao gồm cả `QuyetDinhKeHoachThue` và `KySo_QuyetDinhKeHoachThue`.

### 2.3. API chi tiết — lọc **chỉ** GroupType gốc

File: `QLDA.WebApi/Controllers/ToTrinhCoThamDinhController.cs` — action `Get(Guid id)`

```csharp
var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
{
    GroupId = [entity.Id.ToString()],
    EGroupTypes = [nameof(EGroupType.QuyetDinhKeHoachThue)]   // ← Chỉ base type
});
var danhSachTepThamDinh = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
{
    GroupId = [entity.Id.ToString()],
    EGroupTypes = [nameof(EGroupType.QuyetDinhKeHoachThueThamDinh)]  // ← Cùng vấn đề
});
```

Handler `GetDanhSachTepDinhKemQuery` dùng **exact match** trên `GroupType`:

```csharp
// QLDA.Application/TepDinhKems/Queries/GetDanhSachTepDinhKemQuery.cs
.WhereIf(request.EGroupTypes.Count != 0,
    o => request.GroupId.Contains(o.GroupId) && request.EGroupTypes.Contains(o.GroupType),
    o => request.GroupId.Contains(o.GroupId))
```

→ Chỉ trả về `GroupType == "QuyetDinhKeHoachThue"`, **loại bỏ** `GroupType == "KySo_QuyetDinhKeHoachThue"`.

### 2.4. Sơ đồ luồng dữ liệu

```
                    ┌─────────────────────────────────────┐
                    │  Bảng Attachment (TepDinhKem)       │
                    │  GroupId = e2ddf27a-...               │
                    ├─────────────────────────────────────┤
                    │  [A] GroupType: QuyetDinhKeHoachThue  │  ← file .xlsx gốc
                    │  [B] GroupType: KySo_QuyetDinhKeHoach │  ← file .signed.pdf
                    │      Thue, ParentId = A.Id            │
                    └─────────────────────────────────────┘
                           │                    │
              danh-sach-tien-do          chi-tiet
              (filter GroupId only)    (filter GroupType exact)
                           │                    │
                           ▼                    ▼
                    [A] + [B] ✅            chỉ [A] ❌
```

---

## 3. Phạm vi ảnh hưởng

### 3.1. Endpoint bị lỗi

| Endpoint | Method | Mức độ |
|----------|--------|--------|
| `GET /api/to-trinh-co-tham-dinh/{id}/chi-tiet` | `Get(Guid id)` | 🔴 Thiếu file ký số trong `danhSachTepDinhKem` và `danhSachTepThamDinh` |
| `PUT /api/to-trinh-co-tham-dinh/cap-nhat` | `Update(...)` | 🟡 Response sau cập nhật cũng thiếu file ký số (dòng 109–118 dùng cùng filter) |

### 3.2. Endpoint không bị lỗi (về ký số)

| Endpoint | Lý do |
|----------|-------|
| `GET /api/to-trinh-co-tham-dinh/danh-sach-tien-do` | Không lọc `EGroupTypes` |
| `POST /api/to-trinh-co-tham-dinh/them-moi` | Không trả danh sách tệp |

### 3.3. Tác động nghiệp vụ

- FE mở màn chi tiết không thấy / không tải được file đã ký số dù danh sách hiển thị có.
- User không xem được nội dung đã ký sau khi phê duyệt / trình ký.
- Dữ liệu DB **đúng** — lỗi nằm ở tầng query API, không mất dữ liệu.

---

## 4. So sánh với pattern chuẩn trong codebase

### Pattern đúng (các controller khác)

Nhiều controller **không truyền `EGroupTypes`** khi load chi tiết → lấy hết file theo `GroupId`:

```csharp
// TamUngController, HopDongController, BaoCaoTienDoController, ...
var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery() {
    GroupId = [entity.Id.ToString()]   // Không filter GroupType → có cả KySo_
});
```

### Pattern chuẩn Phase 4 (BuildingBlocks)

Khi **cần** lọc theo `GroupType`, phải mở rộng cả biến thể ký số:

```csharp
var attachmentTypes = AttachmentSubquery.ExpandGroupTypes(
    includeSigned: true,
    nameof(EGroupType.QuyetDinhKeHoachThue));
// → ["QuyetDinhKeHoachThue", "KySo_QuyetDinhKeHoachThue"]
```

Hoặc dùng helper sẵn có trong QLDA:

```csharp
using QLDA.Application.Common;

EGroupTypes = [.. nameof(EGroupType.QuyetDinhKeHoachThue).WithSignedVariant()]
// → ["QuyetDinhKeHoachThue", "KySo_QuyetDinhKeHoachThue"]
```

### Pattern sai (hiện tại của ToTrinhCoThamDinh)

```csharp
EGroupTypes = [nameof(EGroupType.QuyetDinhKeHoachThue)]
// → Chỉ match exact "QuyetDinhKeHoachThue", bỏ sót "KySo_QuyetDinhKeHoachThue"
```

---

## 5. Hướng sửa đề xuất

### Option A — Mở rộng EGroupTypes (khuyến nghị)

Giữ phân tách `danhSachTepDinhKem` / `danhSachTepThamDinh`, nhưng include cả biến thể ký số.

**Bắt buộc** có `using QLDA.Application.Common;` — `WithSignedVariant` là extension method của `SignedHelper`, thiếu using sẽ lỗi CS1061.

```csharp
using QLDA.Application.Common;

// chi-tiet + cap-nhat response
var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
{
    GroupId = [entity.Id.ToString()],
    EGroupTypes = [.. nameof(EGroupType.QuyetDinhKeHoachThue).WithSignedVariant()]
});

var danhSachTepThamDinh = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
{
    GroupId = [entity.Id.ToString()],
    EGroupTypes = [.. nameof(EGroupType.QuyetDinhKeHoachThueThamDinh).WithSignedVariant()]
});
```

**Ưu điểm:** Giữ đúng phân loại tệp đính kèm vs tệp thẩm định, đồng thời có file ký số.

### Option B — Bỏ filter EGroupTypes (đơn giản hơn)

```csharp
var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
{
    GroupId = [entity.Id.ToString()]
});
```

**Nhược điểm:** Trộn cả `QuyetDinhKeHoachThueThamDinh` vào `danhSachTepDinhKem` nếu không tách ở mapping layer.

### File cần sửa

| File | Vị trí |
|------|--------|
| `QLDA.WebApi/Controllers/ToTrinhCoThamDinhController.cs` | `Get(Guid id)` — dòng 34–43 |
| `QLDA.WebApi/Controllers/ToTrinhCoThamDinhController.cs` | `Update(...)` — dòng 109–118 |

---

## 6. Cách kiểm tra sau khi sửa

1. Tạo / mở một `ToTrinhCoThamDinh` có file đính kèm `.xlsx`.
2. Ký số file → xác nhận DB có 2 bản ghi:
   - `GroupType = QuyetDinhKeHoachThue`, `ParentId = null`
   - `GroupType = KySo_QuyetDinhKeHoachThue`, `ParentId = {id file gốc}`
3. Gọi `GET .../danh-sach-tien-do` → `danhSachTepDinhKem` có cả 2 file.
4. Gọi `GET .../{id}/chi-tiet` → `danhSachTepDinhKem` **cũng phải có cả 2 file**.
5. Lặp lại với `danhSachTepThamDinh` / `QuyetDinhKeHoachThueThamDinh` nếu có ký số tệp thẩm định.

---

## 7. Tóm tắt

| Hạng mục | Nội dung |
|----------|----------|
| **Loại lỗi** | Logic query — filter `GroupType` thiếu biến thể ký số |
| **Root cause** | API chi tiết filter `EGroupTypes = ["QuyetDinhKeHoachThue"]` (exact match), trong khi file ký số lưu `GroupType = "KySo_QuyetDinhKeHoachThue"` |
| **Vì sao danh sách vẫn đúng** | Query danh sách chỉ filter theo `GroupId`, không lọc `GroupType` |
| **Mức độ** | Medium — ảnh hưởng hiển thị / tải file ký số ở màn chi tiết |
| **Sửa** | Dùng `WithSignedVariant()` hoặc `AttachmentSubquery.ExpandGroupTypes(includeSigned: true, ...)` khi gọi `GetDanhSachTepDinhKemQuery` |
