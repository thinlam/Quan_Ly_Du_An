# Bug: `GroupId` tệp tờ trình / quyết định Hồ sơ mời thầu điện tử

Tài liệu mô tả **chỗ sai**, **vì sao sai**, và **cách sửa chi tiết** (sửa tại nguồn lưu dữ liệu).

> **Không migration.** Không đổi Id thật của entity `ToTrinh` / `QuyetDinh`. Chỉ đổi `Attachment.GroupId` (bảng `TepDinhKem`).
>
> **Không** workaround bằng cách ghép file thủ công trong vòng `foreach` ở màn phê duyệt.

---

## 1. Triệu chứng

Khi một Hồ sơ mời thầu điện tử có:

- tệp hồ sơ
- tệp tờ trình
- tệp quyết định
- (và các phiên bản ký số tương ứng)

thì trên **danh sách phê duyệt** (`PheDuyet`), `DanhSachTepDinhKem` **thiếu** tệp tờ trình / quyết định (chỉ còn tệp gắn đúng `GroupId = HoSoMoiThauDienTu.Id`).

---

## 2. Chuỗi dữ liệu liên quan

```text
Create/Update HSMTĐT
  → SaveDanhSachTepDinhKemAsync
    → SyncTepDinhKemAsync(GroupId, GroupType, Entities)
      → AttachmentBulkInsertOrUpdateCommand  (lưu TepDinhKem)

Danh sách phê duyệt
  → AttachTepDinhKem / AssignAttachments
    → filesByGroupId.TryGetValue(item.EntityId, ...)
      → EntityId = HoSoMoiThauDienTu.Id.ToString()
```

**Quy ước đúng (mục tiêu):** mọi tệp thuộc một hồ sơ dùng **cùng một `GroupId` = `HoSoMoiThauDienTu.Id`**; phân loại bằng `GroupType`.

| Loại tệp        | GroupId đúng              | GroupType                      |
|-----------------|---------------------------|--------------------------------|
| Tệp hồ sơ       | `HoSoMoiThauDienTu.Id`    | `HoSoMoiThauDienTu`            |
| Tệp tờ trình    | `HoSoMoiThauDienTu.Id`    | `HoSoMoiThauDienTuToTrinh`     |
| Tệp quyết định  | `HoSoMoiThauDienTu.Id`    | `HoSoMoiThauDienTuQuyetDinh`   |

---

## 3. Chỗ sai (chi tiết)

### 3.1. Nguyên nhân gốc — lưu sai `GroupId` khi Create/Update

**File:** `QLDA.WebApi/Controllers/HoSoMoiThauDienTuController.cs`  
**Method:** `SaveDanhSachTepDinhKemAsync` (được gọi từ `Create` và `Update`)

**Code hiện tại (sai):**

```csharp
// Tệp hồ sơ — ĐÚNG: GroupId = entity.Id
await SyncTepDinhKemAsync(
    entityId.ToString(),
    model.GetDanhSachTepDinhKem(entityId),
    EGroupType.HoSoMoiThauDienTu.ToString(),
    cancellationToken);

// Tệp tờ trình — SAI: GroupId = ToTrinh.Id
if (entity.ToTrinh != null || entityOld?.ToTrinh != null) {
    var toTrinhId = entity.ToTrinh != null ? entity.ToTrinh.Id : entityOld?.ToTrinh?.Id;
    await SyncTepDinhKemAsync(
        (toTrinhId ?? 0).ToString(),                                    // ← SAI
        model.ToTrinh?.GetDanhSachTepDinhKemToTrinh(toTrinhId ?? 0) ?? [], // ← SAI (truyền Id tờ trình)
        EGroupType.HoSoMoiThauDienTuToTrinh.ToString(),
        cancellationToken);
}

// Tệp quyết định — SAI: GroupId = QuyetDinh.Id
if (entity.QuyetDinh != null || entityOld?.QuyetDinh != null) {
    var quyetDinhId = entity.QuyetDinh != null ? entity.QuyetDinh.Id : entityOld?.QuyetDinh?.Id;
    await SyncTepDinhKemAsync(
        (quyetDinhId ?? 0).ToString(),                                      // ← SAI
        model.QuyetDinh?.GetDanhSachTepDinhKemQuyetDinh(quyetDinhId ?? 0) ?? [], // ← SAI
        EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString(),
        cancellationToken);
}
```

**Vì sao sai:**

1. `SyncTepDinhKemAsync` ghi `Attachment.GroupId` = tham số `groupId` truyền vào.
2. Tệp tờ trình / quyết định bị lưu với `GroupId` là **Id của `ToTrinhQuyetDinh`** (kiểu `long`), không phải Id hồ sơ.
3. `GroupType` vẫn đúng (`HoSoMoiThauDienTuToTrinh` / `HoSoMoiThauDienTuQuyetDinh`) — phân loại loại tệp ổn — nhưng **khóa gom nhóm** (`GroupId`) lệch khỏi Id hồ sơ.
4. Các nhóm thẩm định (`CamKetTD`, `QuyetDinhTD`, `BaoCaoTD`) đã dùng `entityId` — **đúng**; chỉ nhánh tờ trình / quyết định lệch.

**Kết quả trong DB (ví dụ):**

| File           | GroupId (sai)     | GroupType                    |
|----------------|-------------------|------------------------------|
| file_hoso.pdf  | `{guid-hoso}`     | `HoSoMoiThauDienTu`          |
| file_totrinh.pdf | `12345` (ToTrinh.Id) | `HoSoMoiThauDienTuToTrinh` |
| file_qd.pdf    | `67890` (QuyetDinh.Id) | `HoSoMoiThauDienTuQuyetDinh` |

Ba tệp thuộc **một** hồ sơ nhưng nằm dưới **ba** `GroupId` khác nhau.

---

### 3.2. Mapping cũng gắn `GroupId` theo Id tờ trình / quyết định

**File:** `QLDA.WebApi/Models/HoSoMoiThauDienTus/HoSoMoiThauDienTuMappingConfiguration.cs`

```csharp
public static List<Attachment> GetDanhSachTepDinhKemToTrinh(
    this ToTrinhQuyetDinhModel model, long groupId)
    => model.DanhSachTepDinhKem?.ToEntities(
        groupId.ToString(),                          // ← mỗi Attachment.GroupId = groupId truyền vào
        EGroupType.HoSoMoiThauDienTuToTrinh).ToList() ?? [];

public static List<Attachment> GetDanhSachTepDinhKemQuyetDinh(
    this ToTrinhQuyetDinhModel model, long groupId)
    => model.DanhSachTepDinhKem?.ToEntities(
        groupId.ToString(),
        EGroupType.HoSoMoiThauDienTuQuyetDinh).ToList() ?? [];
```

**Vì sao sai (bổ sung cho 3.1):**

- Tham số tên `groupId` nhưng caller đang truyền `toTrinhId` / `quyetDinhId`.
- `ToEntities(...)` gán `Attachment.GroupId` trên từng entity trước khi bulk sync.
- Dù có sửa `SyncTepDinhKemAsync(..., groupId: entity.Id)` mà vẫn gọi `GetDanhSachTepDinhKemToTrinh(toTrinhId)`, **Entities** vẫn mang `GroupId` cũ của tờ trình → có thể lệch so với `GroupId` trên command (tùy handler resolve). Phải sửa cả phía mapping / tham số truyền vào.

---

### 3.3. Triệu chứng lộ ra ở gán tệp danh sách phê duyệt

**File:** `QLDA.Application/QuanLyPheDuyet/Queries/PheDuyetQueryableExtensions.cs`  
**Methods:** `AttachTepDinhKem` + `AssignAttachments`

```csharp
// Chỉ lấy file có GroupId ∈ danh sách EntityId của item phê duyệt
var groupIds = items.Select(i => i.EntityId)...;
var files = tepDinhKemRepo.GetQueryableSet()
    .Where(i => groupIds.Contains(i.GroupId))
    .ToList();

// Gán theo đúng một khóa: EntityId (= Id hồ sơ / entity được phê duyệt)
foreach (var item in items) {
    item.DanhSachTepDinhKem =
        !string.IsNullOrEmpty(item.EntityId)
        && filesByGroupId.TryGetValue(item.EntityId, out var matched)
            ? matched
            : [];
}
```

**Vì sao “thiếu file”:**

1. Với HSMTĐT, `item.EntityId` = `HoSoMoiThauDienTu.Id.ToString()`.
2. Query chỉ nạp file có `GroupId` trùng `EntityId` đó.
3. File tờ trình (`GroupId = ToTrinh.Id`) và quyết định (`GroupId = QuyetDinh.Id`) **không nằm trong** `groupIds` → không vào `files` → không vào `filesByGroupId`.
4. `TryGetValue(item.EntityId, ...)` chỉ thấy tệp hồ sơ (+ thẩm định nếu có), **không** thấy tờ trình / quyết định.

Đây là **hệ quả** của dữ liệu lưu sai — **không** phải chỗ cần “vá” bằng cách join thêm `ToTrinh.Id` / `QuyetDinh.Id` trong `foreach`. Sửa đúng là chỉnh nguồn lưu (mục 4).

> Ghi chú: `HoSoMoiThauDienTuGetDanhSachQuery` đã subquery theo cả `e.Id`, `e.ToTrinh.Id`, `e.QuyetDinh.Id` nên **màn danh sách HSMTĐT** vẫn thấy đủ file (kể cả dữ liệu cũ). Màn **phê duyệt** thì không — vì chỉ lookup theo `EntityId`.

---

### 3.4. Đọc chi tiết Get cũng đang theo Id tờ trình / quyết định

**File:** `QLDA.WebApi/Controllers/HoSoMoiThauDienTuController.cs` — method `Get`

```csharp
filesToTrinh = (await Mediator.Send(new GetAttachmentsQuery(
    GroupIds: [entity.ToTrinh.Id.ToString()],   // đọc theo ToTrinh.Id
    BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuToTrinh.ToString()]
))).ToAttachmentEntities();

filesQuyetDinh = (await Mediator.Send(new GetAttachmentsQuery(
    GroupIds: [entity.QuyetDinh.Id.ToString()], // đọc theo QuyetDinh.Id
    BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString()]
))).ToAttachmentEntities();
```

Sau khi sửa **lưu** sang `GroupId = entity.Id`, nếu **không** sửa `Get` thì màn chi tiết sẽ không thấy tệp tờ trình / quyết định mới. Cần đọc theo `entity.Id` (có thể giữ thêm lookup Id cũ nếu cần tương thích dữ liệu cũ — tùy chọn, ngoài phạm vi tối thiểu nếu chỉ yêu cầu dữ liệu mới).

---

## 4. Cách sửa (chi tiết từng chỗ)

### Nguyên tắc

- Tất cả tệp thuộc HSMTĐT: `GroupId = HoSoMoiThauDienTu.Id.ToString()`.
- Phân biệt loại bằng `GroupType` (giữ nguyên enum hiện có).
- Không đổi PK / Id của `ToTrinh`, `QuyetDinh`.
- Không migration; không refactor ngoài phạm vi.

---

### 4.1. Sửa `SaveDanhSachTepDinhKemAsync`

**File:** `QLDA.WebApi/Controllers/HoSoMoiThauDienTuController.cs`

**Hướng sửa:**

```csharp
private async Task SaveDanhSachTepDinhKemAsync(...) {
    var groupId = entity.Id.ToString();   // chung cho mọi loại tệp của hồ sơ

    await SyncTepDinhKemAsync(
        groupId,
        model.GetDanhSachTepDinhKem(entity.Id),
        EGroupType.HoSoMoiThauDienTu.ToString(),
        cancellationToken);

    if (entity.ToTrinh != null || entityOld?.ToTrinh != null) {
        await SyncTepDinhKemAsync(
            groupId,   // KHÔNG còn toTrinhId.ToString()
            model.ToTrinh?.GetDanhSachTepDinhKemToTrinh(groupId) ?? [],
            EGroupType.HoSoMoiThauDienTuToTrinh.ToString(),
            cancellationToken);
    }

    if (entity.QuyetDinh != null || entityOld?.QuyetDinh != null) {
        await SyncTepDinhKemAsync(
            groupId,   // KHÔNG còn quyetDinhId.ToString()
            model.QuyetDinh?.GetDanhSachTepDinhKemQuyetDinh(groupId) ?? [],
            EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString(),
            cancellationToken);
    }

    // Thẩm định: giữ nguyên (đã dùng entity.Id)
    ...
}
```

**Chi tiết thay đổi:**

| Trước | Sau |
|-------|-----|
| `Sync(..., toTrinhId.ToString(), Get...ToTrinh(toTrinhId), ...)` | `Sync(..., groupId, Get...ToTrinh(groupId), ...)` |
| `Sync(..., quyetDinhId.ToString(), Get...QuyetDinh(quyetDinhId), ...)` | `Sync(..., groupId, Get...QuyetDinh(groupId), ...)` |

`Create` / `Update` không cần đổi signature — chỉ cần sửa helper vì cả hai đều gọi `SaveDanhSachTepDinhKemAsync`.

**Lưu ý khi Update bản ghi cũ:** lần Update sau sẽ sync với `GroupId = entity.Id` + `AutoDeleteMissing = true` theo `GroupTypes` tương ứng → file mới gắn đúng Id hồ sơ. File cũ còn `GroupId = ToTrinh.Id` **không** tự xóa bởi sync mới (khác GroupId). Có thể:

- chấp nhận orphan dữ liệu cũ + query list HSMTĐT vẫn đọc được nhờ nhánh `ToTrinh.Id` / `QuyetDinh.Id`, hoặc
- (ngoài phạm vi tối thiểu) thêm bước migrate/soft-delete orphan theo Id cũ khi Update.

---

### 4.2. Sửa mapping `GetDanhSachTepDinhKemToTrinh` / `GetDanhSachTepDinhKemQuyetDinh`

**File:** `QLDA.WebApi/Models/HoSoMoiThauDienTus/HoSoMoiThauDienTuMappingConfiguration.cs`

**Hướng sửa (đổi tham số nhận `string`/`Guid` của hồ sơ, không còn `long` Id tờ trình):**

```csharp
public static List<Attachment> GetDanhSachTepDinhKemToTrinh(
    this ToTrinhQuyetDinhModel model, string groupId)  // = HoSoMoiThauDienTu.Id.ToString()
    => model.DanhSachTepDinhKem?.ToEntities(
        groupId,
        EGroupType.HoSoMoiThauDienTuToTrinh).ToList() ?? [];

public static List<Attachment> GetDanhSachTepDinhKemQuyetDinh(
    this ToTrinhQuyetDinhModel model, string groupId)
    => model.DanhSachTepDinhKem?.ToEntities(
        groupId,
        EGroupType.HoSoMoiThauDienTuQuyetDinh).ToList() ?? [];
```

Hoặc overload nhận `Guid groupId` rồi `.ToString()` bên trong — miễn caller truyền **Id hồ sơ**.

**Không** đổi Id entity `ToTrinh` / `QuyetDinh` trong DTO insert/update.

---

### 4.3. Sửa `Get` (đọc chi tiết) cho khớp dữ liệu mới

**File:** cùng controller — method `Get`

Đổi `GroupIds` tờ trình / quyết định sang `entity.Id.ToString()`:

```csharp
GroupIds: [entity.Id.ToString()],
BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuToTrinh.ToString()]

GroupIds: [entity.Id.ToString()],
BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString()]
```

(Tuỳ chọn tương thích cũ: truyền cả `entity.Id` và `ToTrinh.Id` / `QuyetDinh.Id` trong `GroupIds`.)

---

### 4.4. Việc **không** làm

| Không làm | Lý do |
|-----------|--------|
| Vá `AssignAttachments` để merge thêm file theo `ToTrinh.Id` / `QuyetDinh.Id` | Workaround; lệch chuẩn “một GroupId = một hồ sơ” |
| Đổi Id `ToTrinh` / `QuyetDinh` | Ngoài phạm vi; không liên quan liên kết file |
| Migration DB schema | Không đổi schema; chỉ đổi giá trị `GroupId` lúc ghi |
| Refactor lớn ngoài Create/Update/Save/mapping/(Get) | Phạm vi yêu cầu |

Giữ logic query theo `ToTrinh.Id` / `QuyetDinh.Id` trong `HoSoMoiThauDienTuGetDanhSachQuery` là **ổn** để đọc dữ liệu cũ.

---

## 5. Kết quả mong muốn sau khi sửa

Sau Create hoặc Update:

| Loại tệp       | GroupId                 | GroupType                    |
|----------------|-------------------------|------------------------------|
| Hồ sơ          | `HoSoMoiThauDienTu.Id`  | `HoSoMoiThauDienTu` (+ KySo_*) |
| Tờ trình       | `HoSoMoiThauDienTu.Id`  | `HoSoMoiThauDienTuToTrinh` (+ KySo_*) |
| Quyết định     | `HoSoMoiThauDienTu.Id`  | `HoSoMoiThauDienTuQuyetDinh` (+ KySo_*) |

→ `filesByGroupId.TryGetValue(item.EntityId, ...)` lấy **đủ** toàn bộ tệp của hồ sơ trên màn phê duyệt.

---

## 6. Checklist kiểm thử

- [ ] **Create** hồ sơ có tệp hồ sơ + tờ trình + quyết định → DB: cả ba `GroupId` = Id hồ sơ, `GroupType` khác nhau đúng.
- [ ] **Update** đổi/thêm/xóa từng nhóm tệp → sync đúng theo `GroupType`, không xóa nhầm nhóm khác.
- [ ] **Get** chi tiết → thấy đủ tệp tờ trình / quyết định sau khi lưu mới.
- [ ] **Danh sách phê duyệt** (có attachments) → `DanhSachTepDinhKem` gồm cả hồ sơ + tờ trình + quyết định (+ ký số nếu có).
- [ ] **Danh sách HSMTĐT** vẫn ổn với bản ghi cũ (GroupId = ToTrinh/QuyetDinh Id) nhờ query tương thích.
- [ ] Không tạo migration; Id `ToTrinh` / `QuyetDinh` không đổi.

Chi tiết bước Postman: mục **§8**.

---

## 7. File cần đụng tới

| File | Việc |
|------|------|
| `QLDA.WebApi/Controllers/HoSoMoiThauDienTuController.cs` | `SaveDanhSachTepDinhKemAsync` (+ `Get` đọc theo `entity.Id`) |
| `QLDA.WebApi/Models/HoSoMoiThauDienTus/HoSoMoiThauDienTuMappingConfiguration.cs` | `GetDanhSachTepDinhKemToTrinh` / `GetDanhSachTepDinhKemQuyetDinh` nhận GroupId hồ sơ |

**Không sửa (trong phạm vi bug này):** `PheDuyetQueryableExtensions.AssignAttachments` — giữ lookup theo `EntityId`; sẽ đúng sau khi dữ liệu lưu đúng.

---

## 8. Cách test Postman

### 8.1. Chuẩn bị

| Item | Giá trị |
|------|---------|
| Base URL | `{{baseUrl}}` (vd: `https://localhost:7xxx` hoặc env đang dùng) |
| Auth | Header `Authorization: Bearer {{token}}` (user đủ quyền HSMTĐT + xem phê duyệt) |
| Prefetch | Có sẵn `duAnId`, `buocId`, `goiThauId`, `hinhThucLuaChonNhaThauId` hợp lệ |
| File | Đã upload trước (có `path`, `fileName`, `originalName`, `size`) — hoặc copy metadata file từ GET hồ sơ khác |

**Environment variables gợi ý:**

| Variable | Sau bước nào |
|----------|----------------|
| `hosoId` | Response `POST them-moi` → `data` (Guid) |
| `duAnId` | Prefetch |
| `filePathHoso` / `filePathToTrinh` / `filePathQd` | File đã upload |

### 8.2. Endpoints dùng

| # | Method | URL | Mục đích |
|---|--------|-----|----------|
| 1 | `POST` | `/api/ho-so-moi-thau-dien-tu/them-moi` | Create + lưu 3 nhóm tệp |
| 2 | `GET` | `/api/ho-so-moi-thau-dien-tu/{{hosoId}}` | Chi tiết + kiểm tra file trả về |
| 3 | `PUT` | `/api/ho-so-moi-thau-dien-tu/cap-nhat` | Update + sync lại tệp |
| 4 | `GET` | `/api/ho-so-moi-thau-dien-tu/danh-sach?duAnId={{duAnId}}` | List HSMTĐT (tương thích dữ liệu cũ) |
| 5 | `GET` | `/api/phe-duyet/danh-sach?type=HoSoMoiThauDienTu&duAnId={{duAnId}}` | **Bug lộ ở đây** — `DanhSachTepDinhKem` |

> `type` phải đúng literal: `HoSoMoiThauDienTu`.  
> Danh sách phê duyệt chỉ có dòng nếu đã có bản ghi `PheDuyet` với `EntityId = hosoId` (thường sau khi trình / có luồng tạo PheDuyet). Nếu chưa có dòng → kiểm chứng GroupId bằng SQL (§8.6) vẫn đủ chứng minh fix lưu.

### 8.3. Test A — Create (quan trọng nhất)

**Request**

```http
POST {{baseUrl}}/api/ho-so-moi-thau-dien-tu/them-moi
Content-Type: application/json
Authorization: Bearer {{token}}
```

**Body mẫu** (điền Id dự án / gói thầu / path file thật):

```json
{
  "duAnId": "{{duAnId}}",
  "buocId": 1,
  "thamDinh": false,
  "hinhThucLuaChonNhaThauId": 1,
  "goiThauId": "{{goiThauId}}",
  "giaTri": 1000000,
  "thoiGianThucHien": "30 ngày",
  "trangThaiDangTai": false,
  "danhSachTepDinhKem": [
    {
      "id": null,
      "fileName": "hoso.pdf",
      "originalName": "hoso.pdf",
      "path": "{{filePathHoso}}",
      "size": 1024,
      "type": null
    }
  ],
  "toTrinh": {
    "so": "TT-001",
    "ngay": "2026-07-27T00:00:00+07:00",
    "trichYeu": "Tờ trình HSMTĐT test GroupId",
    "nguoiKy": "Nguyễn A",
    "chucVu": 1,
    "danhSachTepDinhKem": [
      {
        "id": null,
        "fileName": "totrinh.pdf",
        "originalName": "totrinh.pdf",
        "path": "{{filePathToTrinh}}",
        "size": 2048
      }
    ]
  },
  "quyetDinh": {
    "so": "QD-001",
    "ngay": "2026-07-27T00:00:00+07:00",
    "trichYeu": "Quyết định HSMTĐT test GroupId",
    "nguoiKy": "Nguyễn B",
    "chucVu": 1,
    "danhSachTepDinhKem": [
      {
        "id": null,
        "fileName": "quyetdinh.pdf",
        "originalName": "quyetdinh.pdf",
        "path": "{{filePathQd}}",
        "size": 3072
      }
    ]
  }
}
```

**Tests tab (Postman):**

```js
pm.test("Create 200 + có Id", function () {
  pm.response.to.have.status(200);
  const json = pm.response.json();
  pm.expect(json.data).to.be.ok;
  pm.environment.set("hosoId", json.data);
});
```

**Kỳ vọng sau Create**

| Trước fix (bug) | Sau fix |
|-----------------|---------|
| DB: 1 file `GroupId = hosoId` + 2 file `GroupId = ToTrinh.Id` / `QuyetDinh.Id` | DB: **cả 3** file `GroupId = hosoId` |
| `GroupType` vẫn đúng từng loại | `GroupType` vẫn đúng từng loại |

### 8.4. Test B — Get chi tiết

```http
GET {{baseUrl}}/api/ho-so-moi-thau-dien-tu/{{hosoId}}
Authorization: Bearer {{token}}
```

**Tests tab:**

```js
pm.test("Có đủ 3 nhóm tệp", function () {
  const d = pm.response.json().data;
  pm.expect(d.danhSachTepDinhKem, "tệp hồ sơ").to.be.an("array").that.is.not.empty;
  pm.expect(d.toTrinh.danhSachTepDinhKem, "tệp tờ trình").to.be.an("array").that.is.not.empty;
  pm.expect(d.quyetDinh.danhSachTepDinhKem, "tệp quyết định").to.be.an("array").that.is.not.empty;
});

pm.test("GroupId tờ trình / QĐ = Id hồ sơ (sau fix)", function () {
  const d = pm.response.json().data;
  const hosoId = String(d.id);
  d.toTrinh.danhSachTepDinhKem.forEach(f => {
    pm.expect(String(f.groupId), f.fileName).to.eql(hosoId);
  });
  d.quyetDinh.danhSachTepDinhKem.forEach(f => {
    pm.expect(String(f.groupId), f.fileName).to.eql(hosoId);
  });
  d.danhSachTepDinhKem.forEach(f => {
    pm.expect(String(f.groupId), f.fileName).to.eql(hosoId);
  });
});
```

**Lưu ý:**

- **Trước khi sửa `Get`:** nếu chỉ sửa Save, Get vẫn query theo `ToTrinh.Id` → `toTrinh.danhSachTepDinhKem` có thể **rỗng** dù DB đã lưu đúng `GroupId = hosoId`. Phải sửa Get theo §4.3 rồi mới assert được qua API.
- Khi đó dùng SQL (§8.6) để xác nhận Save đúng trước.

### 8.5. Test C — Update

1. `GET {{hosoId}}` → copy body response `data` làm base.
2. Giữ nguyên file cũ (có `id`, `groupId`, `groupType`) + thêm 1 file mới `id: null` vào `toTrinh.danhSachTepDinhKem` (hoặc xóa 1 file để test soft-delete).
3. `PUT /api/ho-so-moi-thau-dien-tu/cap-nhat` với body đó (đảm bảo có `"id": "{{hosoId}}"`).

**Kỳ vọng:**

- File tờ trình / quyết định sau sync: `GroupId = hosoId`.
- Không xóa nhầm nhóm khác (`HoSoMoiThauDienTu` vs `...ToTrinh` vs `...QuyetDinh`).
- `toTrinh.id` / `quyetDinh.id` (long) **không đổi** — chỉ `TepDinhKem.GroupId` đổi theo quy ước mới.

### 8.6. Xác nhận DB (bắt buộc để chứng minh root cause / fix)

Chạy sau Create/Update (thay `@hosoId`):

```sql
SELECT Id, GroupId, GroupType, FileName, IsDeleted
FROM TepDinhKem   -- hoặc tên bảng Attachment map tới
WHERE GroupId = @hosoId
   OR GroupType IN (
        'HoSoMoiThauDienTu',
        'HoSoMoiThauDienTuToTrinh',
        'HoSoMoiThauDienTuQuyetDinh',
        'KySo_HoSoMoiThauDienTu',
        'KySo_HoSoMoiThauDienTuToTrinh',
        'KySo_HoSoMoiThauDienTuQuyetDinh'
      )
ORDER BY GroupType, FileName;
```

**Pass sau fix:** mọi row tờ trình / quyết định của hồ sơ này có `GroupId = @hosoId` (Guid string), **không** còn `GroupId` = số long Id tờ trình/QĐ.

**Fail trước fix (repro bug):**

```text
GroupId = '{guid}'     GroupType = HoSoMoiThauDienTu
GroupId = '12345'      GroupType = HoSoMoiThauDienTuToTrinh   ← sai
GroupId = '67890'      GroupType = HoSoMoiThauDienTuQuyetDinh ← sai
```

### 8.7. Test D — Danh sách phê duyệt (nơi bug lộ)

```http
GET {{baseUrl}}/api/phe-duyet/danh-sach?type=HoSoMoiThauDienTu&duAnId={{duAnId}}&pageIndex=0&pageSize=50
Authorization: Bearer {{token}}
```

**Tests tab:**

```js
pm.test("Item hồ sơ có đủ tệp theo EntityId", function () {
  const hosoId = pm.environment.get("hosoId");
  const items = pm.response.json().data?.data ?? pm.response.json().data;
  const item = (Array.isArray(items) ? items : []).find(
    x => String(x.entityId).toLowerCase() === String(hosoId).toLowerCase()
  );
  pm.expect(item, "tìm thấy item phê duyệt của hồ sơ").to.be.ok;

  const files = item.danhSachTepDinhKem || [];
  const types = files.map(f => f.groupType);

  pm.expect(files.length, "phải có > 1 file (hoso + totrinh + qd)").to.be.above(1);
  pm.expect(types.some(t => t === "HoSoMoiThauDienTu" || t === "KySo_HoSoMoiThauDienTu")).to.be.true;
  pm.expect(types.some(t => t === "HoSoMoiThauDienTuToTrinh" || t === "KySo_HoSoMoiThauDienTuToTrinh")).to.be.true;
  pm.expect(types.some(t => t === "HoSoMoiThauDienTuQuyetDinh" || t === "KySo_HoSoMoiThauDienTuQuyetDinh")).to.be.true;

  files.forEach(f => {
    pm.expect(String(f.groupId).toLowerCase()).to.eql(String(hosoId).toLowerCase());
  });
});
```

| Trước fix | Sau fix |
|-----------|---------|
| `danhSachTepDinhKem` chỉ có tệp `GroupType = HoSoMoiThauDienTu` (thiếu tờ trình / QĐ) | Có đủ 3 `GroupType` (và KySo_* nếu có), cùng `groupId = entityId` |

### 8.8. Test E — List HSMTĐT (tương thích cũ)

```http
GET {{baseUrl}}/api/ho-so-moi-thau-dien-tu/danh-sach?duAnId={{duAnId}}
Authorization: Bearer {{token}}
```

- Bản ghi **mới** (sau fix): `danhSachTepDinhKem` vẫn đủ (subquery theo `e.Id`).
- Bản ghi **cũ** (GroupId = ToTrinh/QuyetDinh Id): vẫn đủ nhờ nhánh `e.ToTrinh.Id` / `e.QuyetDinh.Id` — regression check, không được phá.

### 8.9. Thứ tự chạy nhanh (Collection folder)

1. Login / set `token`
2. **A** Create 3 nhóm tệp → set `hosoId`
3. **SQL** §8.6 (pass/fail GroupId)
4. **B** Get chi tiết (sau khi đã sửa Get)
5. **C** Update (optional)
6. Trình / đảm bảo có dòng `PheDuyet` nếu cần
7. **D** `phe-duyet/danh-sach` — assert đủ 3 GroupType
8. **E** `ho-so-moi-thau-dien-tu/danh-sach` — không regress

### 8.10. Checklist Postman

- [ ] A Create → `hosoId` lưu env
- [ ] SQL: 3 GroupType cùng `GroupId = hosoId`
- [ ] B Get: `toTrinh` / `quyetDinh` / hồ sơ đều có file; `groupId` = `hosoId`
- [ ] C Update: sync đúng, không xóa nhầm nhóm
- [ ] D Phê duyệt: đủ tệp tờ trình + quyết định trong `danhSachTepDinhKem`
- [ ] E List HSMTĐT: bản ghi cũ vẫn thấy file
