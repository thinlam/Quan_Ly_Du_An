# Issue — Form lưu file ký số: `GroupType='KySo_'` không thuộc scope

> Ngày ghi nhận / sửa: 2026-07-27  
> Trạng thái: ✅ Đã sửa (BE)  
> API tái hiện: `POST /api/de-xuat-chu-truong-chuyen-tiep/them-moi`  
> Phạm vi: helper resolve GroupType dùng chung (mọi form insert/update qua `ToEntities`)  
> Liên quan: convention `KySo_<base>` — `SignedGroupTypeHelper`

---

## 1. Triệu chứng

API tạo mới đề xuất chủ trương chuyển tiếp trả:

```json
{
  "result": false,
  "errorMessage": "GroupType='KySo_' không thuộc scope GroupTypes=[DeXuatChuyenTiep]",
  "dataResult": null,
  "statusCode": 200
}
```

Tái hiện:

1. Mở form tạo mới (ví dụ Đề xuất chủ trương chuyển tiếp).
2. Thêm file gốc + file ký số (`ParentId` trỏ file gốc).
3. FE gửi `groupType` rỗng (`""`) hoặc không chuẩn (vd. chỉ `"KySo_"`).
4. Gọi `POST .../them-moi` → lỗi scope như trên.

Cùng pattern trên các form khác dùng `ToEntities` + `AttachmentBulkInsertOrUpdateCommand` (vd. Hồ sơ mời thầu điện tử).

---

## 2. Luồng call (trước / sau)

```
Controller.Create (them-moi)
  → DeXuatChuyenTiepInsertCommand          // lưu entity nghiệp vụ
  → DanhSachTepDinhKem.ToEntities(         // ← CHỖ LỖI #1 (ResolveGroupType)
        savedEntity.Id,
        EGroupType.DeXuatChuyenTiep)       // base ĐÚNG đã truyền
  → AttachmentBulkInsertOrUpdateCommand    // ← CHỖ LỖI #2 (NormalizeEntities validate)
        GroupTypes = [DeXuatChuyenTiep]
        Entities   = files (có GroupType="KySo_")
```

Controller **đã** truyền đúng base:

```72:80:QLDA.WebApi/Controllers/DeXuatChuTruongChuyenTiepController.cs
        List<Attachment> files = [.. model.DanhSachTepDinhKem?.ToEntities(savedEntity.Id,
            EGroupType.DeXuatChuyenTiep) ?? []];

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = savedEntity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.DeXuatChuyenTiep)],
            Entities = files,
            AutoDeleteMissing = true
        });
```

Lỗi không nằm ở endpoint riêng — nằm ở helper resolve dùng chung.

---

## 3. Chỗ lỗi — đánh dấu & mô tả chi tiết

### 3.1. Lỗi chính — `ResolveGroupType` ưu tiên FE, tạo ra `KySo_`

**File:** `QLDA.WebApi/Models/TepDinhKems/TepDinhKemMappingConfigurations.cs`  
**Method:** `ResolveGroupType` (private)

#### Code cũ (BUG) — đánh dấu

```csharp
private static string ResolveGroupType(this TepDinhKemModel model, string rawGroupType)
{
    // ❌ BUG: `??` chỉ fallback khi null — chuỗi rỗng "" vẫn được giữ
    var resolved = model.GroupType ?? rawGroupType;

    // File gốc (ParentId == null): giữ nguyên GroupType
    if (model.ParentId is null)
        return resolved;

    // ❌ BUG: ParentId != null + resolved="" → ghép prefix thành "KySo_"
    return resolved.StartsWith(KySoPrefix, StringComparison.Ordinal)
        ? resolved
        : $"{KySoPrefix}{resolved}";
}
```

#### Vì sao lỗi

| Bước | Giá trị | Ý nghĩa |
|------|---------|---------|
| Caller truyền | `rawGroupType = "DeXuatChuyenTiep"` | Base form đúng |
| FE gửi | `model.GroupType = ""` | Chuỗi rỗng, **không phải null** |
| `model.GroupType ?? rawGroupType` | `""` | `??` không bỏ qua empty → **bỏ mất base handler** |
| `ParentId != null` | true | File ký số |
| `$"{KySoPrefix}{resolved}"` | `"KySo_" + ""` = **`"KySo_"`** | Không có base nghiệp vụ |

**Quy tắc bị vi phạm:** trong luồng form insert/update, Backend đã biết `baseGroupType` — **không được** phụ thuộc / ưu tiên `GroupType` FE; không được để rỗng rồi prefix thành `KySo_`.

#### Chi tiết hậu quả

Entity sau `ToEntities`:

| File | ParentId | GroupType thực tế (sai) | Kỳ vọng |
|------|----------|-------------------------|---------|
| Gốc | null | `""` (hoặc giá trị FE) | `DeXuatChuyenTiep` |
| Ký | có | **`KySo_`** | `KySo_DeXuatChuyenTiep` |

---

### 3.2. Lỗi phụ — `NormalizeEntities` không coi `KySo_` là “trống”

**File:** `BuildingBlocks/.../AttachmentBulkInsertOrUpdateCommand.cs`  
**Method:** `NormalizeEntities`

#### Code cũ (thiếu defense)

```csharp
// Chỉ coi trống khi IsNullOrWhiteSpace — "KySo_" KHÔNG vào nhánh này
if (string.IsNullOrWhiteSpace(entity.GroupType))
{
    entity.GroupType = allowedBases[0]
        .ResolveSignedGroupType(entity.ParentId != null);
    continue;
}

var entityBaseType = entity.GroupType.ToBaseGroupType() ?? entity.GroupType;
// "KySo_".ToBaseGroupType() → "" (strip prefix)

// allowedBases = ["DeXuatChuyenTiep"] — không chứa ""
// allAllowed = ["DeXuatChuyenTiep", "KySo_DeXuatChuyenTiep"] — không chứa "KySo_"
if (allowedBases.Contains(entityBaseType) || allAllowed.Contains(entity.GroupType))
{
    // không vào
}

// ❌ Throw đúng message user thấy
throw new ManagedException(
    $"GroupType='{entity.GroupType}' không thuộc scope GroupTypes=[...]");
```

#### Vì sao lỗi (ở lớp này)

- Đây là **lớp validate/normalize sau map**, không phải nguồn tạo `KySo_`.
- Nhưng khi `ToEntities` đã sinh `KySo_`, nhánh “GroupType trống → gán từ scope” **không bắt** vì `"KySo_"` không white-space.
- `ToBaseGroupType("KySo_")` = `""` → không match `DeXuatChuyenTiep` → throw.

Validation scope **vẫn cần giữ** cho GroupType lạ thật sự. Chỉ thiếu: coi prefix-only / base rỗng như blank để gán lại từ `GroupTypes` của form.

---

## 4. Cách sửa — đánh dấu chỗ sửa & lý do

### 4.1. Sửa chính — `ResolveGroupType` dùng base handler, bỏ qua FE trên form

**File:** `QLDA.WebApi/Models/TepDinhKems/TepDinhKemMappingConfigurations.cs`

#### Code sau (FIX) — đánh dấu

```csharp
private static string ResolveGroupType(this TepDinhKemModel model, string rawGroupType)
{
    // ✅ Form có base thật (DeXuatChuyenTiep, HoSoMoiThauDienTu, …)
    //    → ƯU TIÊN rawGroupType từ caller; bỏ qua FE
    // ✅ API ký trực tiếp (None/KySo) → mới fallback GroupType FE
    var preferred = IsUsableBusinessGroupType(rawGroupType)
        ? rawGroupType
        : (IsUsableBusinessGroupType(model.GroupType) ? model.GroupType! : rawGroupType);

    var baseType = preferred.ToBaseGroupType() ?? preferred;
    if (string.IsNullOrWhiteSpace(baseType))
        return string.Empty;

    // ✅ Single source of truth — ParentId null → base; có ParentId → KySo_<base>
    return SignedGroupTypeHelper.ResolveSignedGroupType(baseType, model.ParentId != null);
}

/// <summary>
/// Base hợp lệ: không null/rỗng/sentinel None|KySo|KySo_ (prefix-only).
/// </summary>
private static bool IsUsableBusinessGroupType(string? groupType)
{
    if (string.IsNullOrWhiteSpace(groupType))
        return false;

    var baseType = groupType.ToBaseGroupType() ?? groupType;
    if (string.IsNullOrWhiteSpace(baseType))
        return false;

    return baseType != nameof(EGroupType.None)
        && baseType != nameof(EGroupType.KySo);
}
```

#### Vì sao sửa như vậy

| Yêu cầu | Cách đáp ứng |
|---------|----------------|
| Form biết `baseGroupType` | `IsUsableBusinessGroupType(raw)` true → luôn dùng `raw`, **ghi đè FE** |
| Không tạo `KySo_` từ `""` | Không còn `"KySo_" + ""`; dùng `ResolveSignedGroupType(base, isChild)` |
| Ký lẻ / API ký (`EGroupType.None`) | `raw` không usable → mới lấy FE nếu FE usable |
| Không hard-code endpoint | Sửa helper chung → mọi form `ToEntities(..., EGroupType.X)` hưởng |

#### Kết quả sau fix (form `DeXuatChuyenTiep`)

| File | ParentId | FE GroupType | GroupType BE |
|------|----------|--------------|--------------|
| Gốc | null | `""` / bất kỳ | `DeXuatChuyenTiep` |
| Ký | có | `""` / `KySo_` | `KySo_DeXuatChuyenTiep` |

---

### 4.2. Sửa phụ (defense) — `NormalizeEntities` + `IsBlankOrPrefixOnly`

**File 1:** `BuildingBlocks/.../SignedGroupTypeHelper.cs`

```csharp
/// <summary>
/// GroupType trống hoặc chỉ còn prefix (vd <c>KySo_</c> → base rỗng).
/// </summary>
public static bool IsBlankOrPrefixOnly(string? groupType)
{
    if (string.IsNullOrWhiteSpace(groupType))
        return true;

    var baseType = groupType.ToBaseGroupType() ?? groupType;
    return string.IsNullOrWhiteSpace(baseType); // ✅ "KySo_" → true
}
```

**File 2:** `BuildingBlocks/.../AttachmentBulkInsertOrUpdateCommand.cs` — `NormalizeEntities`

```csharp
// ✅ Trước đây chỉ IsNullOrWhiteSpace — giờ gồm cả "KySo_"
if (SignedGroupTypeHelper.IsBlankOrPrefixOnly(entity.GroupType))
{
    // Scope form đã biết qua request.GroupTypes
    entity.GroupType = allowedBases[0]
        .ResolveSignedGroupType(entity.ParentId != null);
    continue;
}

// Giữ nguyên validate scope cho GroupType lạ thật sự
// → vẫn throw nếu ngoài [base, KySo_base]
```

#### Vì sao sửa như vậy

- **Defense in depth:** nếu có path map khác vẫn lọt `KySo_`, bulk command tự normalize theo scope form thay vì throw.
- **Không bỏ validation toàn cục:** GroupType sai scope (vd. `WrongType` khi không match) vẫn báo lỗi.
- **Chỉ ký lẻ** (API ký, `ParentId == null`, không có context form) mới bắt buộc FE truyền GroupType hợp lệ — xử lý ở `NoiDungDaKyCommand` (không đổi trong issue này).

---

## 5. Quy tắc nghiệp vụ sau fix (tóm tắt)

| Case | Ai quyết định GroupType | Validate / báo lỗi từ FE? |
|------|-------------------------|---------------------------|
| **1. Form** insert/update | Handler/`ToEntities` truyền `baseGroupType` | Không — ghi đè FE |
| **2. Ký có parent** trong DB | Derive `KySo_<base của cha>` (`NoiDungDaKy`) | Không bắt buộc FE |
| **3. Ký lẻ** (`ParentId == null`) | FE phải truyền GroupType | Có — bắt buộc + hợp lệ |

---

## 6. Kiểm thử

**File:** `QLDA.Tests/Unit/TepDinhKemResolveGroupTypeTests.cs`

| Test | Nội dung |
|------|----------|
| `ToEntities_FormBase_IgnoresEmptyFeGroupType_SignedChild` | `""` + ParentId → `KySo_DeXuatChuyenTiep` |
| `ToEntities_FormBase_IgnoresFeKySoPrefixOnly` | FE `"KySo_"` → ghi đè bằng base form |
| `ToEntities_FormBase_OverwritesWrongFeGroupType` | FE sai → vẫn base handler |
| `ToEntities_NoneBase_FallsBackToFeGroupType_ForDirectSign` | `None` + FE usable → lấy FE (ký trực tiếp) |
| Theory 6 scope `HoSoMoiThauDienTu*` | Cùng helper, nhiều GroupType |
| `GetDanhSachTepDinhKem_HoSoMoiThauDienTu_...` | Mapping HS mời thầu (chính + thẩm định) |

BB: `IsBlankOrPrefixOnly_DetectsEmptySignedPrefix` trong `Phase3AttachmentHelperTests`.

Chạy:

```bash
dotnet test QLDA.Tests/QLDA.Tests.csproj --filter "FullyQualifiedName~TepDinhKemResolveGroupTypeTests"
```

---

## 7. File đã đụng

| # | File | Vai trò |
|---|------|---------|
| 1 | `QLDA.WebApi/Models/TepDinhKems/TepDinhKemMappingConfigurations.cs` | **Fix gốc** `ResolveGroupType` + `IsUsableBusinessGroupType` |
| 2 | `BuildingBlocks/.../SignedGroupTypeHelper.cs` | Thêm `IsBlankOrPrefixOnly` |
| 3 | `BuildingBlocks/.../AttachmentBulkInsertOrUpdateCommand.cs` | Defense normalize blank/`KySo_` |
| 4 | `QLDA.Tests/Unit/TepDinhKemResolveGroupTypeTests.cs` | Regression form + HS mời thầu |
| 5 | `BuildingBlocks.Tests/.../Phase3AttachmentHelperTests.cs` | Test helper blank prefix |

**Không sửa:** migration, schema DB, controller endpoint riêng (controller đã truyền base đúng).

---

## 8. Checklist

- [x] Trace từ `them-moi` → `ToEntities` → `AttachmentBulkInsertOrUpdate`
- [x] Xác định nguồn `KySo_` = empty FE + prefix khi có ParentId
- [x] Sửa helper resolve dùng chung (không hard-code 1 endpoint)
- [x] Defense bulk normalize blank / prefix-only
- [x] Giữ validation scope cho GroupType lạ
- [x] Unit test DeXuatChuyenTiep + HoSoMoiThauDienTu
- [ ] Manual: restart WebApi → thử `them-moi` / `ho-so-moi-thau-dien-tu/them-moi` với file ký + `groupType` rỗng
