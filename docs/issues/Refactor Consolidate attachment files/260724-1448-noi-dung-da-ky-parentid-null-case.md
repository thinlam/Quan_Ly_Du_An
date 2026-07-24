# Report: Ký số với `ParentId = null` (ký trực tiếp)

**Ngày:** 2026-07-24  
**Trạng thái:** ✅ Đã chốt + đã sửa (Approach **P1** + API danh mục `EGroupType` cho UI)  
**Phạm vi:** `POST /api/quan-ly-ky-so/ky-so` + `GET /api/danh-muc-enum/...`  
**Liên quan:**
- Case chính `ParentId != null` + bug `KySo_KySo`: [`260724-0619-noi-dung-da-ky-grouptype-bug-report.md`](./260724-0619-noi-dung-da-ky-grouptype-bug-report.md)
- Changelog derive từ parent: [`260724-1402-noi-dung-da-ky-grouptype-fix-changelog.md`](./260724-1402-noi-dung-da-ky-grouptype-fix-changelog.md)

---

## TL;DR

| | |
|--|--|
| **Case này là gì?** | Ký **trực tiếp** — request **không gắn** file gốc (`ParentId = null` trên bản ký) |
| **Vấn đề** | BE không suy được base từ parent → thiếu `groupType` → file không hiện trên màn entity |
| **Cách đã chọn** | **P1** — bắt buộc FE truyền `GroupType` khi không có parent hợp lệ |
| **UI lấy `groupType` từ đâu?** | API danh mục enum — **không** lấy từ list file đính kèm |
| **Đã sửa chưa?** | ✅ `RequireGroupTypeWhenNoParent` + expose `EGroupType` qua `danh-muc-enum` |

---

## 1. Phân tích rõ ràng

### 1.1. Hai ý nghĩa `ParentId = null` (dễ lẫn)

| # | Ở đâu | Ý nghĩa | Docs nào xử lý |
|---|--------|---------|----------------|
| ① | Trên **file gốc** trong DB | File chưa ký (`BanGiaoHoSo`, …) — bình thường | Report `0619` |
| ② | Trên **bản ghi ký số** (request / sau insert) | **Ký trực tiếp** — không gắn file gốc | **Docs này** |

### 1.2. Vì sao case ② cần `GroupType`?

```
Có ParentId hợp lệ     → BE đọc parent.GroupType → KySo_<base>     (không bắt FE gửi type)
Không có ParentId      → BE không biết base entity là gì           → FE phải gửi GroupType
```

Ký trực tiếp = caller đã biết đang ký cho nhóm nào → **phải gửi `groupType`**.

### 1.3. UI cần danh sách `EGroupType` — không phải `groupType` trên file

| Nhu cầu | Đúng | Sai |
|---------|------|-----|
| UI chọn loại để truyền khi ký thẳng | `GET /api/danh-muc-enum/danh-sach?enumName=EGroupType` | Đọc `groupType` trong `danhSachTepDinhKem` của chi tiết hồ sơ |
| Verify file đã ký đúng | Chi tiết hồ sơ có `groupType = KySo_BanGiaoHoSo` | — |

`groupType` trên file = kết quả đã lưu.  
`EGroupType` từ danh mục enum = danh sách UI chọn để **gửi lên**.

### 1.4. Triệu chứng nếu không gửi `groupType` (trước khi siết)

| Bước | Kết quả cũ |
|------|------------|
| Handler fallback | Sentinel **`KySo`** |
| Get BanGiao Expand | Chỉ nhận `BanGiaoHoSo`, `KySo_BanGiaoHoSo`, … |
| | **`KySo` không match** → file biến mất trên UI |

Nguyên nhân **khác** bug `KySo_KySo` (report 0619): thiếu base, không phải double-prefix.

### 1.5. Quan hệ với report `0619`

| | Report 0619 | Docs này |
|--|-------------|----------|
| `ParentId` trên bản ký | **≠ null** | **= null** |
| Bug | `KySo_KySo` | `KySo` / thiếu type |
| Fix | Derive từ parent (A) | **Bắt buộc `GroupType` (P1)** + API enum cho UI |

---

## 2. Cách đã chọn — P1

| Cách | Quyết định |
|------|------------|
| **P1 — FE gửi `groupType` + BE validate** | ✅ Đã chọn & implement |
| P2 — Thêm `BaseGroupType` trên command | ❌ |
| P3 — Cấm ký khi không có parent | ❌ |

**Deal FE:**

1. Load danh mục: `GET /api/danh-muc-enum/danh-sach?enumName=EGroupType`
2. Ký trên file đã lưu → gửi `parentId` (không bắt buộc `groupType`)
3. Ký trực tiếp → `parentId = null` + `groupType` = **`rawName`** từ bước 1 (vd. `BanGiaoHoSo`)
4. Không chọn sentinel: `None`, `KySo` → BE throw

---

## 3. Các bước đã sửa (đánh dấu vị trí)

### Tổng quan file đụng

| # | File | Việc |
|---|------|------|
| 1 | `QLDA.WebApi/Controllers/QuanLyKySoController.cs` | `ToEntities(..., EGroupType.None)` |
| 2 | `QLDA.Application/KySos/Commands/NoiDungDaKyCommand.cs` | Validate + derive GroupType |
| 3 | `QLDA.WebApi/Controllers/DanhMucEnumController.cs` | Expose `EGroupType` cho UI |

---

### Sửa 1 — `QuanLyKySoController.Create` — L48–51 ⬅️

```csharp
var entities = model.DanhSachTepDinhKem.ToEntities(model.GroupId, EGroupType.None)
    .ToList();
```

Trước: `EGroupType.KySo` → dễ `KySo_KySo`.  
Sau: giữ `groupType` FE gửi khi `ParentId = null`.

---

### Sửa 2 — `NoiDungDaKyCommand.Handle` — L25–47 ⬅️

| Nhánh | Điều kiện | Dòng | Hành vi |
|-------|-----------|------|---------|
| A | ParentId + parent có DB | L38–41 | Derive từ parent |
| B | ParentId + parent không có DB | L32–36 | Clear ParentId → **Require GroupType** |
| C | ParentId == null | L44–46 | Ký trực tiếp → **Require GroupType** |

---

### Sửa 3 — `RequireGroupTypeWhenNoParent` — L77–86 ⬅️ MỚI

Throw: *Khi không có ParentId, bắt buộc truyền GroupType (vd. BanGiaoHoSo hoặc KySo_BanGiaoHoSo).*

Reject: trống, `None`, `KySo`, `KySo_KySo`, `KySo_None` (`IsMissingGroupTypeForDirectSign` L88–102).

---

### Sửa 4 — `EnsureSignedGroupType` — L104–120

Base hợp lệ → `KySo_<base>` (sau khi validate pass).

---

### Sửa 5 — `DanhMucEnumController` — L24 + L48 ⬅️ MỚI (cho UI)

**File:** `QLDA.WebApi/Controllers/DanhMucEnumController.cs`

| Vị trí | Thay đổi |
|--------|----------|
| **L24** | `GetEnumNames`: thêm `nameof(EGroupType)` |
| **L31** | Comment: EGroupType dùng cho UI chọn groupType khi ký trực tiếp |
| **L48** | `ListEnum`: `nameof(EGroupType) => EnumsExtensions.EnumAll<EGroupType>()` |

```csharp
// L17–25 GetEnumNames
var result = new List<string> {
    // ...
    nameof(EGroupType),   // L24 ⬅️
};

// L40–49 ListEnum
result = enumName switch {
    // ...
    nameof(EGroupType) => EnumsExtensions.EnumAll<EGroupType>(),  // L48 ⬅️
    _ => result
};
```

---

## 4. Flow sau sửa

### Ký trực tiếp (đúng)

```
UI: GET danh-muc-enum?enumName=EGroupType → chọn rawName=BanGiaoHoSo
        ↓
POST /ky-so: parentId=null + groupType=BanGiaoHoSo
        ↓
RequireGroupTypeWhenNoParent → pass
EnsureSignedGroupType → KySo_BanGiaoHoSo
        ↓
GET BanGiao chi tiết → file hiện với groupType=KySo_BanGiaoHoSo
```

### Thiếu type → throw

```
POST /ky-so: parentId=null + không groupType
        ↓
RequireGroupTypeWhenNoParent → THROW (không insert)
```

---

## 5. Payload deal với FE

### ✅ Ký trực tiếp

```json
{
  "groupId": "<BanGiaoHoSo.Id>",
  "danhSachTepDinhKem": [
    {
      "parentId": null,
      "groupType": "BanGiaoHoSo",
      "fileName": "hs-signed.pdf",
      "path": "/signed/hs.pdf",
      "type": "application/pdf",
      "size": 1000
    }
  ]
}
```

`groupId` = Id **hồ sơ** (từ `GET .../chi-tiet` field `id`), không phải Id file.  
`groupType` = **`rawName`** từ API danh mục enum.

### ✅ Có parent (case 0619)

```json
{
  "groupId": "<BanGiaoHoSo.Id>",
  "danhSachTepDinhKem": [
    { "parentId": "<Id-file-goc>", "fileName": "...", "path": "...", "type": "application/pdf", "size": 1000 }
  ]
}
```

### ❌ Thiếu type

```json
{ "parentId": null }
```

→ Lỗi bắt buộc `GroupType`.

---

## 6. Bản đồ vị trí sửa (quick ref)

```
QLDA.WebApi/Controllers/QuanLyKySoController.cs
  └── Create L48–51  ⬅️ ToEntities(..., None)

QLDA.Application/KySos/Commands/NoiDungDaKyCommand.cs
  └── Handle L25–47
  └── RequireGroupTypeWhenNoParent     L77–86   ⬅️ P1
  └── IsMissingGroupTypeForDirectSign  L88–102  ⬅️ P1
  └── EnsureSignedGroupType            L104–120
  └── ApplySignedGroupTypeFromParent   L63–75   (case có parent)

QLDA.WebApi/Controllers/DanhMucEnumController.cs
  └── GetEnumNames L24  ⬅️ thêm EGroupType
  └── ListEnum     L48  ⬅️ EnumAll<EGroupType>()
```

---

## 7. Hướng dẫn test từng bước (rõ ràng)

### Phần A — Test API danh mục `EGroupType` (cho UI)

#### A0. Chuẩn bị
1. Restart WebApi.
2. Mở Swagger / Postman.
3. Authorize nếu cần.

#### A1. Kiểm tra tên enum có `EGroupType`

```http
GET /api/danh-muc-enum/danh-sach-ten
```

| Pass | Fail |
|------|------|
| `dataResult` chứa `"EGroupType"` | Không thấy → chưa restart / chưa deploy |

#### A2. Lấy danh sách giá trị

```http
GET /api/danh-muc-enum/danh-sach?enumName=EGroupType
```

| Pass | Fail |
|------|------|
| `result: true`, `dataResult` **mảng nhiều phần tử** | `dataResult: []` → sai `enumName` hoặc chưa sửa controller |
| Mỗi item có `id`, `index`, `rawName`, `ten` | — |
| Có `rawName`: `BanGiaoHoSo`, `BienBanBanGiao`, `None`, `KySo` | — |

Ví dụ item:

```json
{
  "id": "BanGiaoHoSo",
  "index": 47,
  "rawName": "BanGiaoHoSo",
  "ten": "BanGiaoHoSo"
}
```

#### A3. Ghi nhận giá trị UI sẽ truyền

Copy **`rawName`** (không dùng index):

- HS bàn giao → `BanGiaoHoSo`
- Biên bản → `BienBanBanGiao`

---

### Phần B — Test end-to-end ký trực tiếp

#### B1. Lấy `groupId` = Id hồ sơ

```http
GET /api/ban-giao-ho-so/{id}/chi-tiet
```

hoặc `GET /api/ban-giao-ho-so/danh-sach` → lấy field **`id`** của hồ sơ.

Ví dụ: `08ded2d1-a532-3f1b-687a-7b55f0035b43`

#### B2. Ký trực tiếp với `groupType` từ A3

```http
POST /api/quan-ly-ky-so/ky-so
```

```json
{
  "groupId": "08ded2d1-a532-3f1b-687a-7b55f0035b43",
  "danhSachTepDinhKem": [
    {
      "parentId": null,
      "groupType": "BanGiaoHoSo",
      "fileName": "test-enum-ui.signed.pdf",
      "originalName": "test-enum-ui.signed.pdf",
      "path": "/uploads/test-enum-ui.signed.pdf",
      "type": "application/pdf",
      "size": 12345
    }
  ]
}
```

| Pass | Fail |
|------|------|
| `dataResult >= 1`, không lỗi | Message bắt buộc GroupType → kiểm tra lại `groupType` |

#### B3. Xác nhận chi tiết có file ký đúng type

```http
GET /api/ban-giao-ho-so/08ded2d1-a532-3f1b-687a-7b55f0035b43/chi-tiet
```

Trong `danhSachTepDinhKem`:

| Field | Kỳ vọng |
|-------|---------|
| `fileName` | `test-enum-ui.signed.pdf` |
| `groupType` | **`KySo_BanGiaoHoSo`** |
| `parentId` | `null` |

*(Đây là verify file đã lưu — khác với API danh mục enum ở phần A.)*

#### B4. Case fail — thiếu / sentinel `groupType`

Lần lượt thử `groupType`: bỏ trống / `null` / `KySo` / `None`

| Pass |
|------|
| API **lỗi**, message bắt buộc truyền GroupType |

---

### Checklist tick nhanh

| # | Việc | Pass? |
|---|------|-------|
| A1 | `danh-sach-ten` có `EGroupType` | ☐ |
| A2 | `danh-sach?enumName=EGroupType` trả nhiều `rawName` | ☐ |
| A3 | Copy được `BanGiaoHoSo` | ☐ |
| B1 | Có `groupId` hồ sơ | ☐ |
| B2 | `POST /ky-so` parentId=null + groupType=BanGiaoHoSo → 200 | ☐ |
| B3 | Chi tiết có `KySo_BanGiaoHoSo` | ☐ |
| B4 | groupType=KySo / thiếu → lỗi | ☐ |

---

## 8. Checklist triển khai

- [x] Phân tích: ký trực tiếp ≠ file gốc `ParentId = null`
- [x] Chốt Approach **P1**
- [x] Sửa `QuanLyKySoController` + `NoiDungDaKyCommand`
- [x] Expose `EGroupType` qua `DanhMucEnumController` (L24, L48)
- [x] Build WebApi Release — OK
- [x] Docs test từng bước (mục 7)
- [ ] FE: load enum → ký trực tiếp gửi `groupType` = `rawName`
- [ ] (Optional) Migrate row cũ orphan `GroupType = KySo`
