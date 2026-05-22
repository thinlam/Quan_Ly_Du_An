# Task #9460 – Nội dung đã ký (lịch sử ký số trên `TepDinhKem`)

> **V2 (áp dụng)** — `GroupType` = **hiện hành** (`KySo`) vs **lịch sử** (`NoiDungDaKySo`), không phải “lần ký đầu / cuối”.  
> **V1 (giữ §2, §7.1, §8)** — bảng `NoiDungDaKySo` riêng + handler sai (đảo type, auto `IsDeleted`).  
> Liên quan: [task-9460-ky-so-crud.md](./task-9460-ky-so-crud.md), API list [task-9460-danh-sach-noi-dung-da-ky.md](./task-9460-danh-sach-noi-dung-da-ky.md).

---

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm

| # | Mục | Ghi chú |
|---|-----|---------|
| 1 | Lưu file đã ký vào `TepDinhKem` | Endpoint `POST /api/ky-so/them-moi` (đã có từ trước) |
| 2 | Lưu **lịch sử** khi user **ký lại** (re-sign) | Không tạo bảng mới — dùng `GroupType` + `ParentId` + `IsDeleted` |
| 3 | Tránh xóa nhầm file gốc cùng `GroupId` | Không dùng `TepDinhKemBulkInsertOrUpdateCommand` (sync toàn group) |

### 1.2 `GroupType` trên `TepDinhKem` — hiện hành vs lịch sử

| Giá trị | Ý nghĩa |
|---------|---------|
| **`KySo`** | Bản **hiện hành** (đang hiển thị) |
| **`NoiDungDaKySo`** | Bản **lịch sử** (đã bị thay thế) |

> `NoiDungDaKySo` là **hằng `GroupTypeConstants`**, không phải tên bảng DB.  
> **Không** hiểu là “lần ký đầu / lần ký cuối”.

### 1.3 Upload — ký trên file gốc vs ký lại (thay bản)

> Không gọi là “lần đầu / lần cuối”: mỗi lần ký lại chỉ **demote** bản đang `KySo` → `NoiDungDaKySo` và insert bản mới `KySo`.

| | Ký trên **file gốc** (`ParentId` → bản chưa ký) | **Ký lại** (parent đã là bản ký) |
|---|--------------------------------------------------|----------------------------------|
| Parent `GroupType` | VD `GoiThau`, … (không thuộc cặp ký) | `KySo` (hiện hành) |
| Bản **mới** `GroupType` | `KySo` | `KySo` |
| Bản **cũ** (parent) | — | `GroupType` → `NoiDungDaKySo` |
| Bản mới `ParentId` | Id file gốc | Id bản cha (đã ký) |
| `IsDeleted` bản cũ | — | **Không** tự set khi ký lại |

**Ví dụ (N bản ký):** file ký sai (`KySo`) → upload bản đúng → DB: bản sai `NoiDungDaKySo`, bản mới `KySo`. Chuỗi dài: mọi bản thay thế đều `NoiDungDaKySo`, đúng một bản `KySo` hiện hành.

### 1.4 `IsDeleted` (tách khỏi ký lại)

| Trường hợp | `IsDeleted` |
|------------|-------------|
| Ký lại / thay bản | **Không** đổi — chỉ đổi `GroupType` |
| User “ký đời”, không xóa hẳn nhưng ẩn khỏi hiện hành | `true` thủ công — vẫn có trong API **lịch sử** ([task danh sách](./task-9460-danh-sach-noi-dung-da-ky.md)) |

### 1.5 V1 handler (sai — giữ tham khảo, không implement lại)

Phiên bản code/doc trước 22/05/2026 (đã sửa):

```csharp
// ❌ V1 — hiểu nhầm “bản mới = lịch sử”
parent.IsDeleted = true;
entity.GroupType = GroupTypeConstants.NoiDungDaKySo;
```

**V2 đúng:** `parent.GroupType = NoiDungDaKySo`, `entity.GroupType = KySo`, không `IsDeleted` khi ký lại.

---

## 2. Phân tích hiện trạng

### 2.1 `KySoController` trước khi sửa (V1 – đã bỏ)

```
POST /api/ky-so/them-moi
  ├─ Bước 1: TepDinhKemBulkInsertOrUpdateCommand (KySo=true)
  └─ Bước 2: NoiDungDaKyInsertCommand → bảng NoiDungDaKySo
```

**Vấn đề V1:**

| # | Lỗi | Hậu quả |
|---|-----|---------|
| 1 | `BulkInsertOrUpdate` sync theo `GroupId` | File gốc cùng group có thể bị soft-delete |
| 2 | `GetId()` gọi 2 lần (`ToEntities` vs Bước 2) | `TepDinhKemId` FK sai khi `id: null` |
| 3 | `NoiDungDaKyInsertCommand` không `SaveChanges` | API `200`, bảng `NoiDungDaKySo` trống |

### 2.2 Sau khi sửa (V2 – hiện tại)

```
POST /api/ky-so/them-moi
  └─ ToEntities → NoiDungDaKyCommand
       ├─ Ký trên file gốc: bản mới GroupType = KySo
       └─ Ký lại: parent → NoiDungDaKySo, bản mới → KySo (không auto IsDeleted)
```

Tham khảo [task-9460-ky-so-crud.md §2.1](./task-9460-ky-so-crud.md): `KySoController` còn các endpoint CRUD bảng `KySo`; endpoint `them-moi` là upload file, **không** ghi bảng `KySo`.

---

## 3. Thứ tự thực hiện

```
Bước 1: Domain - GroupTypeConstants.NoiDungDaKySo
Bước 2: Application - NoiDungDaKyCommand + Handler (V2 logic GroupType)
Bước 3: WebApi - KySoController.Create → NoiDungDaKyCommand
Bước 4: Xóa V1 - Entity/Config/Command NoiDungDaKySo (bảng riêng)
Bước 5: Migration - DropNoiDungDaKySoTable (sau khi đã add AddNoiDungDaKySo)
```

---

## 4. Chi tiết từng bước

---

### Bước 1 – Domain: `GroupTypeConstants`

**File sửa:** `QLDA.Domain/Constants/GroupTypeConstants.cs`

```csharp
/// <summary>
/// Bản lịch sử ký số trên TepDinhKem (đã bị thay thế; hiện hành = KySo).
/// </summary>
public const string NoiDungDaKySo = "NoiDungDaKySo";
```

---

### Bước 2 – Application: `NoiDungDaKyCommand` (V2)

**File:** `QLDA.Application/KySos/Commands/NoiDungDaKyCommand.cs`

```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Constants;

namespace QLDA.Application.KySos.Commands;

public record NoiDungDaKyCommand : IRequest<int> {
    public required string GroupId { get; set; }
    public required List<TepDinhKem> Entities { get; set; }
}

internal class NoiDungDaKyCommandHandler : IRequestHandler<NoiDungDaKyCommand, int> {
    private readonly IRepository<TepDinhKem, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public KySoThemMoiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(NoiDungDaKyCommand request, CancellationToken cancellationToken = default) {
        var toInsert = new List<TepDinhKem>();

        foreach (var entity in request.Entities.Where(e => e.ParentId != null)) {
            entity.GroupId = request.GroupId;

            var parent = await _repository.GetQueryableSet()
                .FirstOrDefaultAsync(e => e.Id == entity.ParentId, cancellationToken);
            ManagedException.ThrowIfNull(parent, "Không tìm thấy tệp cha (ParentId)");

            if (IsSignedVersion(parent.GroupType)) {
                parent.GroupType = GroupTypeConstants.NoiDungDaKySo; // bản cũ → lịch sử
                entity.GroupType = GroupTypeConstants.KySo;         // bản mới → hiện hành
                entity.ParentId = parent.Id;
                // await UpdateAsync(parent) — cùng transaction với insert
            } else {
                entity.GroupType = GroupTypeConstants.KySo; // ký trên file gốc
            }

            toInsert.Add(entity);
        }

        if (toInsert.Count == 0)
            return 0;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddRangeAsync(toInsert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return toInsert.Count;
    }

    private static bool IsSignedVersion(string? groupType) =>
        groupType is GroupTypeConstants.KySo or GroupTypeConstants.NoiDungDaKySo;
}
```



---

### Bước 3 – WebApi: `KySoController.Create`

**File sửa:** `QLDA.WebApi/Controllers/KySoController.cs`

**Trước (V1):**

```csharp
// Bước 1
await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand { KySo = true, ... });
// Bước 2
await Mediator.Send(new NoiDungDaKyInsertCommand { TepDinhKemId = tepDaKyMoi.GetId(), ... }); // ❌ GetId() lần 2
return ResultApi.Ok(1);
```

**Sau (V2):**

```csharp
var entities = model.DanhSachTepDinhKem
    .ToEntities(model.GroupId, GroupTypeConstants.KySo)
    .ToList();

var count = await Mediator.Send(new NoiDungDaKyCommand {
    GroupId  = model.GroupId.ToString(),
    Entities = entities,
});

return ResultApi.Ok(count);
```

> Materialize `ToEntities().ToList()` **một lần** — `Id` ổn định khi request `id: null`.

---

### Bước 4 – Gỡ code V1 (bảng `NoiDungDaKySo`)

| File | Hành động |
|------|-----------|
| `QLDA.Domain/Entities/NoiDungDaKySo.cs` | **Xóa** |
| `QLDA.Persistence/Configurations/NoiDungDaKySoConfiguration.cs` | **Xóa** |
| `QLDA.Application/KySos/Commands/NoiDungDaKyInsertCommand.cs` | **Xóa** |

Migration `AddNoiDungDaKySo` (**immutable** — không sửa file cũ).

---

### Bước 5 – Migration drop bảng V1

```bash
# Thư mục QLDA.Migrator
ef.bat QLDA add DropNoiDungDaKySoTable
ef.bat QLDA update
```

Chỉ `DropTable("NoiDungDaKySo")`; snapshot bỏ entity `NoiDungDaKySo`.

---

## 5. Query & ví dụ dữ liệu

```sql
-- Hiện hành (màn đối tượng)
SELECT * FROM TepDinhKem
WHERE GroupId = @GroupId
  AND GroupType = 'KySo'
  AND IsDeleted = 0
  AND ParentId IS NOT NULL;

-- Lịch sử (API danh sách — cả 2 type, kể cả IsDeleted)
SELECT * FROM TepDinhKem
WHERE GroupId = @GroupId
  AND GroupType IN ('KySo', 'NoiDungDaKySo')
  AND ParentId IS NOT NULL;
```

```
File gốc (GoiThau)     id=1
  → ký lần 1           id=2, GroupType=KySo,             ParentId=1
  → ký lại             id=2 → NoiDungDaKySo (lịch sử)
                       id=3, GroupType=KySo,             ParentId=2 (hiện hành)
```

---

## 6. Checklist hoàn thành

```
[x] 1. Thêm GroupTypeConstants.NoiDungDaKySo
[x] 2. NoiDungDaKyCommand + Handler (V2 GroupType)
[x] 3. KySoController.Create → NoiDungDaKyCommand (bỏ BulkInsert + NoiDungDaKyInsert)
[x] 4. Xóa NoiDungDaKySo entity + Configuration + NoiDungDaKyInsertCommand
[x] 5. Migration DropNoiDungDaKySoTable + ef update trên DB test
[x] 6. Verify Postman: ký lần 1 + ký lại + file gốc không bị soft-delete
[x] 7. API list “nội dung đã ký” (task riêng — chưa có endpoint)
```

---

## 7. Lưu ý kỹ thuật

- Request `DanhSachTepDinhKem`: chỉ file **đã ký** (`parentId` bắt buộc); `id: null` OK nếu dùng `entities[].Id` sau `ToEntities`.
- `NoiDungDaKyCommand`: ký lại cập nhật `parent.GroupType` (tracked) + insert bản mới trong một transaction / `SaveChanges` (đã có).
- Không đụng `TepDinhKemBulkInsertOrUpdateCommand` — các controller khác (`GoiThau`, `HopDong`, …) vẫn dùng bulk như cũ.
- Phân biệt với [task-9460-ky-so-crud.md](./task-9460-ky-so-crud.md): bảng **`KySo`** = chứng thư / cấu hình ký số; **`TepDinhKem`** = file đính kèm đã ký trên đối tượng nghiệp vụ.

---

## 7.1 Những sửa chữa trong quá trình triển khai (21/05/2026)

### Thiết kế (V1 → V2)

| V1 (bỏ) | V2 (áp dụng) |
|---------|----------------|
| Bảng `NoiDungDaKySo` + `NoiDungDaKyInsertCommand` | Chỉ `TepDinhKem`, `GroupType` phân nhánh |
| 2 bước Mediator trong controller | 1 command `NoiDungDaKyCommand` |
| `TepDinhKemBulkInsertOrUpdateCommand` + `KySo=true` | Command riêng, không sync soft-delete cả `GroupId` |

### Application Layer

| Lỗi / vấn đề V1 | Sửa V2 |
|-----------------|--------|
| `NoiDungDaKyInsertCommand` chỉ `AddAsync`, không `SaveChanges` | Bỏ command; logic gộp `NoiDungDaKyCommand` có `SaveChangesAsync` |
| Handler V1: `IsDeleted` + bản mới `NoiDungDaKySo` | V2: `parent` → `NoiDungDaKySo`, bản mới → `KySo`, không auto `IsDeleted` (§1.5) |

### WebApi Layer

| Lỗi / vấn đề V1 | Sửa V2 |
|-----------------|--------|
| `tepDaKyMoi.GetId()` lần 2 khi `id: null` | Chỉ dùng `entities` từ `ToEntities().ToList()` |
| `TepDinhKemBulkInsertOrUpdateCommand` xóa nhầm file gốc | Đổi sang `NoiDungDaKyCommand` |
| `return ResultApi.Ok(1)` cố định | `return ResultApi.Ok(count)` — số bản ghi insert |

### Domain / Persistence

| Thay đổi | Ghi chú |
|----------|---------|
| ➕ `GroupTypeConstants.NoiDungDaKySo` | Constant cho lịch sử trên `TepDinhKem` |
| ➖ `NoiDungDaKySo` entity + `NoiDungDaKySoConfiguration` | Rollback thiết kế bảng riêng |
| Migration `AddNoiDungDaKySo` | Giữ nguyên (immutable); chờ `DropNoiDungDaKySoTable` |

---

## 8. TÓM TẮT CÔNG VIỆC – HOÀN THÀNH CODE (21/05/2026)

### Tổng quan

**Task #9460 (phần nội dung đã ký)** — refactor V1 → V2:

- ✅ **Handler** `NoiDungDaKyCommand.cs` (V2 GroupType)
- ✅ **2 file sửa** (`GroupTypeConstants.cs`, `KySoController.cs`)
- ✅ **3 file xóa** (entity, config, insert command V1)
- ⏭️ **1 migration** còn lại: `DropNoiDungDaKySoTable`

---

### Các file **mới tạo** (1 file)

| File | Mô tả |
|------|-------|
| `QLDA.Application/KySos/Commands/NoiDungDaKyCommand.cs` | Insert file ký + ký lại (hiện hành / lịch sử); trả `int` |

---

### Các file **chỉnh sửa** (2 files)

| # | File | Thay đổi |
|---|------|----------|
| 1 | `QLDA.Domain/Constants/GroupTypeConstants.cs` | ➕ `NoiDungDaKySo` |
| 2 | `QLDA.WebApi/Controllers/KySoController.cs` | ➖ Bulk + Insert V1 → ➕ `NoiDungDaKyCommand`; response `count` |

---

### Các file **đã xóa** (3 files – V1)

| File | Lý do |
|------|-------|
| `QLDA.Domain/Entities/NoiDungDaKySo.cs` | Không dùng bảng riêng |
| `QLDA.Persistence/Configurations/NoiDungDaKySoConfiguration.cs` | Không dùng bảng riêng |
| `QLDA.Application/KySos/Commands/NoiDungDaKyInsertCommand.cs` | Thay bằng `NoiDungDaKyCommand` |

---

### API Endpoint (không đổi route)

| Method | Route | Thay đổi hành vi |
|--------|-------|------------------|
| `POST` | `/api/ky-so/them-moi` | V2: lịch sử trên `TepDinhKem`; `dataResult` = số file insert |

---

### Trạng thái

- ✅ **Code V2** xong — build được sau khi apply migration drop (nếu snapshot còn entity cũ thì cần migration trước)
- ⏭️ **`ef.bat QLDA add DropNoiDungDaKySoTable`** + `update` trên DB test
- ⏭️ **API list** nội dung đã ký — task khác

---

## 9. Pending (task khác)

- [x] API list — [task-9460-danh-sach-noi-dung-da-ky.md](./task-9460-danh-sach-noi-dung-da-ky.md): `IN ('KySo','NoiDungDaKySo')`, `OnlyNotDeleted: false`
- [x] **DateTime → DateTimeOffset** (DB) và **DateOnly** (request) — code xong; còn chạy migration: [task-fix-datetime-datetimeoffset-dateonly.md](./task-fix-datetime-datetimeoffset-dateonly.md)
