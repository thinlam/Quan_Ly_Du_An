# Changelog — Fix GroupType ký số `KySo_KySo` → `KySo_<base>` (Approach A)

**Ngày sửa:** 2026-07-24  
**Liên quan:** [`260724-0619-noi-dung-da-ky-grouptype-bug-report.md`](./260724-0619-noi-dung-da-ky-grouptype-bug-report.md)  
**Trạng thái:** ✅ Đã implement + build `QLDA.WebApi` Release (0 error)

---

## 1. Tóm tắt

| # | File | Dòng (sau sửa) | Việc làm |
|---|------|----------------|----------|
| 1 | `QLDA.WebApi/Controllers/QuanLyKySoController.cs` | **48–51** | Đổi `ToEntities(..., EGroupType.KySo)` → `EGroupType.None` + comment |
| 2 | `QLDA.Application/KySos/Commands/NoiDungDaKyCommand.cs` | **25–45** | Vòng `foreach`: load parent thật; có parent → derive GroupType; không có → clear ParentId + fallback |
| 3 | Cùng file | **61–73** | Thêm `ApplySignedGroupTypeFromParent` |
| 4 | Cùng file | **75–91** | Sửa `EnsureSignedGroupType` (chuẩn hóa `KySo_KySo`, bỏ `Contains("KySo")`) |

**Không sửa:** `BanGiaoHoSoController.Get`, `SignedGroupTypeHelper`, migration / dữ liệu cũ.

---

## 2. File 1 — `QuanLyKySoController.cs`

**Path:** `QLDA.WebApi/Controllers/QuanLyKySoController.cs`  
**Method:** `Create` — `POST /api/quan-ly-ky-so/ky-so`  
**Vùng sửa:** dòng **48–51** (trong method bắt đầu dòng 44)

### Trước

```csharp
var entities = model.DanhSachTepDinhKem.ToEntities(model.GroupId, EGroupType.KySo)
    .ToList();
```

### Sau (dòng 48–51)

```csharp
// Không truyền EGroupType.KySo — ToEntities sẽ ra KySo_KySo khi có ParentId.
// NoiDungDaKyCommand derive GroupType = KySo_<base> từ parent khi ParentId tồn tại.
var entities = model.DanhSachTepDinhKem.ToEntities(model.GroupId, EGroupType.None)
    .ToList();
```

### Note chi tiết

1. `ToEntities` (WebApi `TepDinhKemMappingConfigurations.ResolveGroupType`): nếu `ParentId != null` và raw type chưa bắt đầu bằng `KySo_`, sẽ prefix `KySo_` + raw.
2. Raw = `EGroupType.KySo` (`"KySo"`) → kết quả **`KySo_KySo`** — đây là bug gốc phía caller.
3. Đổi sang `EGroupType.None` để **không** gán nhầm base giả `"KySo"`. Giá trị tạm từ map vẫn có thể là `KySo_None` / `None` — **không quan trọng** vì handler sẽ ghi đè khi có parent.
4. Phải ghi rõ `EGroupType.None` (không bỏ tham số) vì overload `ToEntities(Guid, EGroupType)` vs `ToEntities(Guid, string)` **ambiguous** nếu không truyền arg thứ 3.

### Vì sao sửa chỗ này

- Cắt nguồn tạo `KySo_KySo` ngay tại API entry.
- Không bắt FE/caller biết base entity (`BanGiaoHoSo` / `BienBanBanGiao` / …) — base lấy từ parent trong handler.

---

## 3. File 2 — `NoiDungDaKyCommand.cs`

**Path:** `QLDA.Application/KySos/Commands/NoiDungDaKyCommand.cs`  
**Class:** `NoiDungDaKyCommandHandler`

### 3.1. `Handle` — vòng xử lý từng entity (dòng **25–45**)

#### Trước (sau commit ParentId optional)

```csharp
foreach (var entity in request.Entities) {
    entity.GroupId = request.GroupId;
    EnsureSignedGroupType(entity);   // ← nếu đã là KySo_KySo thì Contains("KySo") → giữ nguyên ❌

    if (entity.ParentId is { } parentId) {
        var parentExists = await ...AnyAsync(...);
        if (!parentExists)
            entity.ParentId = null;
    }
    toInsert.Add(entity);
}
```

#### Sau (dòng 25–45)

```csharp
foreach (var entity in request.Entities) {
    entity.GroupId = request.GroupId;

    if (entity.ParentId is { } parentId) {
        var parent = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == parentId, cancellationToken);

        if (parent is null) {
            // Ký trước khi lưu form — vẫn lưu file ký số độc lập.
            entity.ParentId = null;
            EnsureSignedGroupType(entity);
        }
        else {
            // Derive KySo_<base> từ parent (vd. BanGiaoHoSo → KySo_BanGiaoHoSo).
            ApplySignedGroupTypeFromParent(entity, parent);
        }
    }
    else {
        EnsureSignedGroupType(entity);
    }

    toInsert.Add(entity);
}
```

#### Note chi tiết từng nhánh

| Nhánh | Điều kiện | Hành vi | Lý do |
|-------|-----------|---------|--------|
| A | Có `ParentId` + parent **có** trong DB | `ApplySignedGroupTypeFromParent` | Case chuẩn: ký sau khi file gốc đã lưu → GroupType đúng `KySo_BanGiaoHoSo`… |
| B | Có `ParentId` + parent **không** có | `ParentId = null` + `EnsureSignedGroupType` | Giữ quyết định nghiệp vụ trước: cho phép ký trước khi lưu form, không throw |
| C | Không có `ParentId` | `EnsureSignedGroupType` | File ký độc lập / không gửi parent |

#### Thay đổi kỹ thuật so với bản cũ

- `AnyAsync` → `FirstOrDefaultAsync`: cần **entity parent** (để đọc `GroupType`), không chỉ biết tồn tại.
- **Không** gọi `EnsureSignedGroupType` trước khi có parent: bản cũ gọi sớm → `KySo_KySo` bị “khóa” vì `Contains("KySo")` rồi return, không bao giờ sửa lại.

---

### 3.2. Method mới `ApplySignedGroupTypeFromParent` (dòng **61–73**)

```csharp
private static void ApplySignedGroupTypeFromParent(Attachment entity, Attachment parent) {
    var baseType = parent.GroupType.ToBaseGroupType() ?? parent.GroupType;

    if (string.IsNullOrWhiteSpace(baseType) || baseType == nameof(EGroupType.KySo)) {
        EnsureSignedGroupType(entity);
        return;
    }

    entity.GroupType = SignedGroupTypeHelper.WithSignedVariant(baseType);
}
```

#### Note chi tiết

| Dòng | Ý nghĩa |
|------|---------|
| **65** | `ToBaseGroupType()`: `BanGiaoHoSo` → `BanGiaoHoSo`; `KySo_BanGiaoHoSo` → `BanGiaoHoSo` (ký lại trên file đã ký vẫn ra đúng base) |
| **67–70** | Guard: parent base rỗng hoặc sentinel `KySo` → không tạo thêm `KySo_KySo`; fallback `EnsureSignedGroupType` |
| **72** | `WithSignedVariant("BanGiaoHoSo")` → **`KySo_BanGiaoHoSo`** (không double-prefix nếu đã có `KySo_`) |

#### Ví dụ bảng

| Parent.GroupType | baseType sau strip | entity.GroupType sau fix |
|------------------|--------------------|---------------------------|
| `BanGiaoHoSo` | `BanGiaoHoSo` | `KySo_BanGiaoHoSo` |
| `BienBanBanGiao` | `BienBanBanGiao` | `KySo_BienBanBanGiao` |
| `KySo_BanGiaoHoSo` | `BanGiaoHoSo` | `KySo_BanGiaoHoSo` |
| `KySo` / `KySo_KySo` | `KySo` | fallback `KySo` (guard) |

---

### 3.3. `EnsureSignedGroupType` (dòng **75–91**)

#### Trước

```csharp
if (string.IsNullOrWhiteSpace(entity.GroupType)) {
    entity.GroupType = nameof(EGroupType.KySo);
    return;
}
if (entity.GroupType.Contains("KySo", StringComparison.Ordinal))  // ← KySo_KySo bị giữ nguyên
    return;
entity.GroupType = SignedGroupTypeHelper.WithSignedVariant(entity.GroupType);
```

#### Sau (dòng 80–90)

```csharp
if (string.IsNullOrWhiteSpace(entity.GroupType)
    || entity.GroupType == nameof(EGroupType.KySo)
    || entity.GroupType == $"{SignedGroupTypeHelper.Prefix}{nameof(EGroupType.KySo)}") {
    entity.GroupType = nameof(EGroupType.KySo);
    return;
}

if (entity.GroupType.StartsWith(SignedGroupTypeHelper.Prefix, StringComparison.Ordinal))
    return;

entity.GroupType = SignedGroupTypeHelper.WithSignedVariant(entity.GroupType);
```

#### Note chi tiết

1. **Chuẩn hóa `KySo_KySo` → `KySo`:** chỉ dùng khi **không** derive được từ parent (orphan / parent thiếu). Sentinel `KySo` vẫn match filter list nội dung đã ký (`GroupType.Contains("KySo")`).
2. **Đổi `Contains("KySo")` → `StartsWith("KySo_")`:**  
   - Trước: mọi chuỗi chứa substring `KySo` (kể cả sai `KySo_KySo`) đều “ok” → không sửa.  
   - Sau: chỉ coi là signed hợp lệ khi đã đúng prefix convention; `KySo` / `KySo_KySo` đi nhánh chuẩn hóa ở trên.
3. Nếu FE đã gửi sẵn `KySo_BanGiaoHoSo` mà không có parent: `StartsWith("KySo_")` → giữ nguyên (không phá).

---

## 4. Vì sao chọn Approach A (derive từ parent)

Báo cáo gốc đề xuất 3 hướng; đã chọn **A**.

| Approach | Ý tưởng | Quyết định |
|----------|---------|------------|
| **A — Derive từ parent.GroupType** | Parent đã insert với base đúng (`BanGiaoHoSo`…) → file ký = `KySo_` + base đó | ✅ **Chọn** |
| B — Caller truyền `BaseGroupType` | FE/controller phải biết và gửi đúng base | ❌ Dễ sai; API ký số dùng chung nhiều màn |
| C — Migrate DB + siết helper | Sửa data cũ + throw khi base invalid | ⏸ Ngoài phạm vi đợt này (data cũ optional sau) |

### Lý do chi tiết chọn A

1. **Single source of truth:** file gốc đã có `GroupType` đúng lúc Insert/Update/BanGiao (`BanGiaoHoSo`, `BienBanBanGiao`, …). Không cần FE đoán lại.
2. **Khớp convention BuildingBlocks:** `SignedGroupTypeHelper` — gốc = base, ký = `KySo_<base>`. Read-side (`GetAttachmentsQuery` + `ExpandGroupTypes` / `ToBaseGroupType`) đã filter đúng pair này.
3. **Sửa đúng tầng bug:** bug không nằm ở filter BanGiao Get (filter đúng), mà ở **write** GroupType sai → sửa write.
4. **Tương thích quyết định ParentId optional:** vẫn cho ký khi parent chưa có (nhánh B trong bảng trên); không quay lại throw `"Không tìm thấy tệp cha"`.
5. **Ít surface area:** chỉ 2 file; không đổi contract API request; không migration.

### Lý do **không** chọn B

- `POST /ky-so` phục vụ nhiều entity; bắt caller truyền `BaseGroupType` dễ lệch (vd. gửi `BanGiaoHoSo` khi đang ký biên bản).
- Parent trong DB đã đủ thông tin → truyền thêm param là thừa và dễ conflict với parent thật.

### Lý do **không** chọn C trong đợt này

- Row cũ `KySo_KySo` vẫn tồn tại nhưng **không bị xóa**; chỉ không hiện trên UI cho đến khi migrate / ký lại.
- Siết `ResolveSignedGroupType` throw có thể phá caller khác chưa sẵn sàng.
- Có thể làm script migrate riêng sau (join parent → rewrite GroupType).

---

## 5. Chuỗi nguyên nhân → chỗ sửa (map 1-1)

```
Caller ToEntities(..., KySo)     →  sửa Controller L48–51 (None)
        ↓
Resolve → KySo_KySo
        ↓
Handler không overwrite từ parent →  sửa Handle L25–45 + ApplySignedGroupTypeFromParent L61–73
        ↓
EnsureSigned giữ KySo_KySo       →  sửa EnsureSignedGroupType L80–90
        ↓
Get BanGiao Expand không match   →  không cần sửa (đã đúng; data mới sẽ match)
```

Sau fix, file ký mới:

| Bước | GroupType |
|------|-----------|
| Parent HS | `BanGiaoHoSo` |
| Insert ký số | **`KySo_BanGiaoHoSo`** |
| Get chi tiết Expand | có trong `IN (..., KySo_BanGiaoHoSo, ...)` |
| Split `ToBaseGroupType` | `BanGiaoHoSo` → vào `DanhSachTepDinhKem` |

---

## 6. Phạm vi / không làm

| Làm | Không làm |
|-----|-----------|
| Derive GroupType khi có parent | Đổi API contract / thêm field FE bắt buộc |
| Bỏ hard-code `EGroupType.KySo` ở Create | Sửa `BanGiaoHoSoController.Get` |
| Chuẩn hóa `KySo_KySo` khi orphan | Migration SQL dữ liệu cũ |
| Giữ ký khi parent chưa có | Đổi `SignedGroupTypeHelper` BB |

---

## 7. Kiểm tra đề xuất

1. Ký file HS bàn giao (`parentId` = file `BanGiaoHoSo`) → DB `GroupType = KySo_BanGiaoHoSo` → chi tiết hiện trong list tệp HS.
2. Ký biên bản (`parentId` = file `BienBanBanGiao`) → `KySo_BienBanBanGiao` → list biên bản.
3. Ký với `parentId` không tồn tại → insert OK, `ParentId` null, `GroupType` = `KySo`.
4. (Optional) Script: `UPDATE ... SET GroupType = 'KySo_' + ToBase(parent) WHERE GroupType = 'KySo_KySo' AND ParentId IS NOT NULL`.

---

## 8. Tham chiếu helper

| Helper | File | Dùng ở |
|--------|------|--------|
| `ToBaseGroupType` | `BuildingBlocks.../SignedGroupTypeHelper.cs` ~L29–32 | L65 command |
| `WithSignedVariant` | cùng file ~L40–45 | L72, L90 |
| `Prefix` = `"KySo_"` | cùng file ~L10 | L82, L87 |
