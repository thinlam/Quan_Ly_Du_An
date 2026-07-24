# Report: File ký số biến mất khỏi `GET /api/ban-giao-ho-so/{id}/chi-tiet`

**Ngày:** 2026-07-24  
**Phạm vi:** QLDA — module Bàn giao hồ sơ (+ API ký số dùng chung)  
**Loại:** Bug analysis → ✅ Đã fix (Approach A) — case **`ParentId != null` + parent có trong DB**  
**Changelog chi tiết (dòng sửa + lý do):** [`260724-1402-noi-dung-da-ky-grouptype-fix-changelog.md`](./260724-1402-noi-dung-da-ky-grouptype-fix-changelog.md)  
**Case `ParentId = null` (ngoài phạm vi report này):** [`260724-1448-noi-dung-da-ky-parentid-null-case.md`](./260724-1448-noi-dung-da-ky-parentid-null-case.md)

## TL;DR

File ký số (`attachment` có `ParentId != null`) của một tệp thuộc `BanGiaoHoSo` hoặc `BienBanBanGiao` **bị mất hoàn toàn khỏi response** của `GET /api/ban-giao-ho-so/{id}/chi-tiet`. Nguyên nhân: `QuanLyKySoController.Create` truyền `EGroupType.KySo` (enum không có base) vào `ToEntities`, khiến `SignedGroupTypeHelper.ResolveSignedGroupType` resolve sai thành `"KySo_KySo"` thay vì `"KySo_BanGiaoHoSo"` / `"KySo_BienBanBanGiao"`. Filter ở `BanGiaoHoSoController.Get` dùng `ToBaseGroupType()` không match `KySo_KySo` với base nào → file bị loại khỏi cả 2 list.

## Repro theo lập trình (không cần chạy)

### Bước 1 — Insert file HS gốc

```http
POST /api/ban-giao-ho-so/them-moi
{
  "ma": "BGHS-001",
  "tenHoSo": "Hồ sơ bàn giao gói thầu X",
  "duAnId": "<guid>",
  "buocId": 1,
  "danhSachTepDinhKem": [
    { "fileName": "hs-ban-giao.pdf", "originalName": "hs-ban-giao.pdf",
      "path": "/uploads/hs-ban-giao.pdf", "size": 102400, "type": "application/pdf" }
  ]
}
```

DB sau bước 1:
| Id | GroupId | GroupType | ParentId |
|---|---|---|---|
| `A` | `<BanGiaoHoSo.Id>` | `BanGiaoHoSo` | null |

### Bước 2 — Upload biên bản qua endpoint `/ban-giao`

```http
PUT /api/ban-giao-ho-so/{id}/ban-giao
{
  "ngayBanGiao": "2026-07-24",
  "phongBanNhanId": 42,
  "danhSachBienBan": [
    { "fileName": "bien-ban.pdf", "originalName": "bien-ban.pdf",
      "path": "/uploads/bien-ban.pdf", "size": 51200, "type": "application/pdf" }
  ]
}
```

DB sau bước 2:
| Id | GroupId | GroupType | ParentId |
|---|---|---|---|
| `A` | `<BanGiaoHoSo.Id>` | `BanGiaoHoSo` | null |
| `B` | `<BanGiaoHoSo.Id>` | `BienBanBanGiao` | null |

### Bước 3 — Ký số file `hs-ban-giao.pdf`

```http
POST /api/quan-ly-ky-so/ky-so
{
  "groupId": "<BanGiaoHoSo.Id>",
  "danhSachTepDinhKem": [
    { "id": "<new-guid>", "parentId": "<A>",
      "fileName": "hs-ban-giao-signed.pdf",
      "originalName": "hs-ban-giao-signed.pdf",
      "path": "/signed/hs-ban-giao.pdf", "size": 102500, "type": "application/pdf" }
  ]
}
```

DB **mong đợi**:
| Id | GroupId | GroupType | ParentId |
|---|---|---|---|
| `A` | `<BanGiaoHoSo.Id>` | `BanGiaoHoSo` | null |
| `B` | `<BanGiaoHoSo.Id>` | `BienBanBanGiao` | null |
| `C` | `<BanGiaoHoSo.Id>` | **`KySo_BanGiaoHoSo`** | `A` |

DB **thực tế** (do bug):
| Id | GroupId | GroupType | ParentId |
|---|---|---|---|
| `A` | `<BanGiaoHoSo.Id>` | `BanGiaoHoSo` | null |
| `B` | `<BanGiaoHoSo.Id>` | `BienBanBanGiao` | null |
| `C` | `<BanGiaoHoSo.Id>` | **`KySo_KySo`** ← ❌ | `A` |

### Bước 4 — Get chi tiết

```http
GET /api/ban-giao-ho-so/{id}/chi-tiet
```

Response thực tế:
```json
{
  "id": "<BanGiaoHoSo.Id>",
  "danhSachTepDinhKem": [
    { "id": "A", "fileName": "hs-ban-giao.pdf", ... }
  ],
  "danhSachBienBanBanGiao": [
    { "id": "B", "fileName": "bien-ban.pdf", ... }
  ]
}
```

→ **File `C` (ký số) biến mất hoàn toàn** — không có ở `danhSachTepDinhKem` lẫn `danhSachBienBanBanGiao`.

## Chuỗi nguyên nhân (đã xác nhận qua đọc code)

### Bước hỏng 1 — Controller hard-code base groupType

**File:** `QLDA.WebApi/Controllers/QuanLyKySoController.cs:44-54`

```csharp
[HttpPost("api/quan-ly-ky-so/ky-so")]
public async Task<ResultApi> Create([FromBody] KySoModel model) {
    ManagedException.ThrowIfNull(model.DanhSachTepDinhKem);
    model.DanhSachTepDinhKem ??= [];

    var entities = model.DanhSachTepDinhKem.ToEntities(model.GroupId, EGroupType.KySo)
        .ToList();                                                 // ← ❌

    var count = await Mediator.Send(new NoiDungDaKyCommand {
        GroupId = model.GroupId.ToString(),
        Entities = entities,
    });
    return ResultApi.Ok(count);
}
```

`EGroupType.KySo.ToString()` = `"KySo"` — chuỗi **không có underscore** và không đại diện cho một base entity cụ thể nào.

### Bước hỏng 2 — `ToEntities` resolve sang `"KySo_KySo"`

**File:** `QLDA.Application/TepDinhKems/DTOs/TepDinhKemMappingConfiguration.cs:19-32`

```csharp
private static Attachment ToEntity(this TepDinhKemInsertDto insertDto, Guid groupId,
    EGroupType groupType = EGroupType.None)
    => new() {
        Id = GuidExtensions.GetSequentialGuidId(),
        ParentId = insertDto.ParentId,
        GroupId = groupId.ToString(),
        GroupType = SignedGroupTypeHelper.ResolveSignedGroupType(
            groupType.ToString(), insertDto.ParentId != null),     // ← ParentId != null → isChild=true
        ...
    };
```

### Bước hỏng 3 — `ResolveSignedGroupType` không detect `"KySo"` là base hợp lệ

**File:** `BuildingBlocks/src/BuildingBlocks.Application/Attachments/Common/SignedGroupTypeHelper.cs:16-24`

```csharp
public static string ResolveSignedGroupType(this string baseGroupType, bool isChild) {
    if (string.IsNullOrEmpty(baseGroupType))
        return baseGroupType ?? string.Empty;

    return isChild && !baseGroupType.StartsWith(Prefix, StringComparison.Ordinal)
        ? Prefix + baseGroupType                                  // ← "KySo_" + "KySo" = "KySo_KySo"
        : baseGroupType;
}
```

Trace:
- `baseGroupType = "KySo"`, `isChild = true`
- `"KySo".StartsWith("KySo_")` → **false** (chuỗi `"KySo"` không chứa `_` ở cuối)
- → return `"KySo_" + "KySo"` = **`"KySo_KySo"`**

### Bước hỏng 4 — Handler không sửa lại

**File:** `QLDA.Application/KySos/Commands/NoiDungDaKyCommand.cs:20-37`

```csharp
public async Task<int> Handle(NoiDungDaKyCommand request, CancellationToken cancellationToken = default) {
    var toInsert = new List<Attachment>();

    foreach (var entity in request.Entities.Where(e => e.ParentId != null)) {
        entity.GroupId = request.GroupId;

        var parent = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == entity.ParentId, cancellationToken);
        ManagedException.ThrowIfNull(parent, "Không tìm thấy tệp cha (ParentId)");

        //if (IsSignedVersion(parent.GroupType)) {
        //    entity.ParentId = parent.Id;
        //} else {
        //    entity.GroupType = GroupTypeConstants.KySo;        // ← ❌ comment-out
        //}

        toInsert.Add(entity);                                    // ← GroupType giữ "KySo_KySo"
    }
    ...
}
```

Handler chỉ dùng `parent` để validate tồn tại, **không** derive `entity.GroupType` từ `parent.GroupType`. Đoạn code đúng (resolve từ parent) đang bị comment-out — không rõ lý do tại sao bị tắt.

### Bước hỏng 5 — `Get` filter loại bỏ file ký số

**File:** `QLDA.WebApi/Controllers/BanGiaoHoSoController.cs:26-39`

```csharp
var allFiles = (await _mediator.Send(new GetAttachmentsQuery(
    GroupIds: [entity.Id.ToString()],
    BaseGroupTypes: [
        nameof(EGroupType.BanGiaoHoSo),          // "BanGiaoHoSo"
        nameof(EGroupType.BienBanBanGiao)       // "BienBanBanGiao"
    ]
))).ToAttachmentEntities();
```

`GetAttachmentsQueryHandler` (`BuildingBlocks/src/BuildingBlocks.Application/Attachments/Queries/GetAttachmentsQuery.cs:37-44`) mở rộng thành filter `IN (...)`:

```
[ "BanGiaoHoSo", "KySo_BanGiaoHoSo",
  "BienBanBanGiao", "KySo_BienBanBanGiao" ]
```

File `C` có `GroupType = "KySo_KySo"` → **không thuộc IN list** → bị EF Core filter bỏ ngay từ query. (Không đến được controller.)

**Trường hợp controller tự filter lại** (line 34-39):
```csharp
var tepHS = allFiles
    .Where(f => f.GroupType.ToBaseGroupType() == nameof(EGroupType.BanGiaoHoSo))
    .ToList();
```

`ToBaseGroupType("KySo_KySo")`:
- `"KySo_KySo".StartsWith("KySo_")` → **true**
- → return `"KySo_KySo"[5..]` = `"KySo"`

So sánh `"KySo" == "BanGiaoHoSo"` → **false**. Tương tự với `BienBanBanGiao` → **false**.

→ File bị loại khỏi cả 2 list. Hai bước filter (query + controller) đều loại file ký số.

## Tại sao convention là `"KySo_<base>"` chứ không phải `"KySo_KySo"`

`SignedGroupTypeHelper` (BuildingBlocks) là single source of truth cho convention ký số (`docs/issues/Refactor Consolidate attachment files/plan.md` đã xác nhận):

| Pattern | Nghĩa |
|---|---|
| `BanGiaoHoSo` | file gốc |
| `KySo_BanGiaoHoSo` | file ký số của file gốc `BanGiaoHoSo` |
| `KySo_KySo` | **không hợp lệ** — double prefix, không thuộc entity nào |

`ToBaseGroupType` luôn strip đúng **một lần** prefix `"KySo_"`. Strip `"KySo_KySo"` → `"KySo"` → không phải base của entity nào.

## Tại sao `EGroupType.KySo` tồn tại trong enum

Enum `EGroupType` ở `QLDA.Domain/Enums/EGroupType.cs:31` có giá trị `KySo`. Giá trị này có vẻ được dùng làm **fallback mặc định** cho `NoiDungDaKyCommand` khi controller không biết base GroupType. Nhưng convention của `ResolveSignedGroupType` lại không hỗ trợ fallback này — nó chỉ nối prefix khi base không bắt đầu bằng `"KySo_"`.

→ **Convention không nhất quán giữa enum và helper.** Helper cần treat `"KySo"` (không có base) như sentinel value — ví dụ throw exception, hoặc fallback lấy base từ caller.

## Impact

| Mức | Đối tượng | Hậu quả |
|---|---|---|
| Feature | Toàn bộ BanGiaoHoSo/BienBanBanGiao | File ký số của hồ sơ bàn giao không hiển thị trên UI |
| Feature | Mọi entity khác dùng `QuanLyKySoController.Create` (nếu có) | Cùng bug — file ký số bị orphan khỏi base GroupType |
| Data | DB | File vẫn tồn tại (không bị xóa), không bị orphan tuyệt đối vì `GroupId` đúng. Chỉ là `GroupType` sai convention. |
| Audit | Lịch sử | Commit `163cf1c` (2026-05-22, "Fix mất tệp HS bàn giao") fix scope `GroupTypes` cho `TepDinhKemBulkInsertOrUpdateCommand`. Bug hiện tại cùng pattern nhưng ở command `NoiDungDaKyCommand` và helper. |

## Hướng fix (đề xuất, chưa thực hiện)

### Phương án A — Handler derive từ parent.GroupType (khuyến nghị)

Sửa `NoiDungDaKyCommandHandler`:

```csharp
foreach (var entity in request.Entities.Where(e => e.ParentId != null)) {
    entity.GroupId = request.GroupId;

    var parent = await _repository.GetQueryableSet()
        .FirstOrDefaultAsync(e => e.Id == entity.ParentId, cancellationToken);
    ManagedException.ThrowIfNull(parent, "Không tìm thấy tệp cha (ParentId)");

    var baseType = parent.GroupType.ToBaseGroupType() ?? parent.GroupType;
    entity.GroupType = baseType.ResolveSignedGroupType(isChild: true);

    toInsert.Add(entity);
}
```

Đồng thời sửa `QuanLyKySoController.Create` — bỏ `ToEntities(..., EGroupType.KySo)`, để handler tự derive. Caller chỉ cần truyền danh sách file ký số với `ParentId` chính xác.

**Ưu điểm:** Base GroupType luôn chính xác vì đến từ parent (đã được insert qua controller với scope đúng).
**Nhược điểm:** Caller phải đảm bảo parent đã insert thành công trước khi gọi ký số.

### Phương án B — Caller truyền baseGroupType

Thêm property `BaseGroupType` vào `NoiDungDaKyCommand`. Caller truyền base rõ ràng:

```csharp
new NoiDungDaKyCommand {
    GroupId = model.GroupId.ToString(),
    BaseGroupType = nameof(EGroupType.BanGiaoHoSo),  // ← caller quyết định
    Entities = entities
}
```

Handler:
```csharp
foreach (var entity in request.Entities.Where(e => e.ParentId != null)) {
    entity.GroupId = request.GroupId;
    entity.GroupType = request.BaseGroupType.ResolveSignedGroupType(isChild: true);
    toInsert.Add(entity);
}
```

**Ưu điểm:** Caller kiểm soát hoàn toàn, không cần parent lookup.
**Nhược điểm:** Caller có thể truyền sai — phải validate scope.

### Phương án C — Migrate dữ liệu cũ + scope guard ở helper

- Migrate `GroupType = "KySo_KySo"` → dựa trên parent.
- Thêm validation trong `ResolveSignedGroupType`: nếu `baseGroupType` rỗng hoặc không map được với một base entity hợp lệ → throw.

## Câu hỏi chưa giải quyết

| # | Câu hỏi | Cần từ |
|---|---|---|
| 1 | Có controller nào khác ngoài `QuanLyKySoController.Create` cũng gọi `NoiDungDaKyCommand` không? | User xác nhận — code hiện tại chỉ có 1 caller |
| 2 | UI có đang dùng `/api/quan-ly-ky-so/ky-so` cho BanGiaoHoSo/BienBanBanGiao không, hay chỉ các entity khác? | Product |
| 3 | Có cần migrate dữ liệu `GroupType = "KySo_KySo"` cũ trong DB không? | DBA/Product |
| 4 | Phương án A (derive từ parent) hay B (caller truyền) phù hợp hơn với UX hiện tại? | Product |

## Files liên quan

| File | Vai trò |
|---|---|
| `QLDA.WebApi/Controllers/QuanLyKySoController.cs:44-57` | Caller — hard-code `EGroupType.KySo` |
| `QLDA.WebApi/Controllers/BanGiaoHoSoController.cs:26-39` | Read-side — filter loại file ký số |
| `QLDA.Application/TepDinhKems/DTOs/TepDinhKemMappingConfiguration.cs:19-32` | `ToEntities` resolve GroupType |
| `QLDA.Application/KySos/Commands/NoiDungDaKyCommand.cs:20-37` | Handler — không derive GroupType từ parent |
| `BuildingBlocks/src/BuildingBlocks.Application/Attachments/Common/SignedGroupTypeHelper.cs:16-24` | Helper — convention `"KySo_<base>"` |
| `BuildingBlocks/src/BuildingBlocks.Application/Attachments/Queries/GetAttachmentsQuery.cs:37-44` | Filter ở query layer |

## Verification

Không có test trong report này. Khi tiến hành fix, viết test gọi trực tiếp `ToEntities(dto, groupId, EGroupType.KySo)` và assert `entity.GroupType` — test này sẽ **pass trên code hiện tại** (document bug) và **fail sau khi fix** (đảm bảo regression không quay lại).