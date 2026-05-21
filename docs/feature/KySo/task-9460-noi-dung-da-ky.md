# Task #9460 – Nội dung đã ký (lịch sử ký số trên `TepDinhKem`)

> Liên quan UC ký số: [task-9460-ky-so-crud.md](./task-9460-ky-so-crud.md) (CRUD bảng `KySo` + danh mục).  
> Task này chỉ xử lý **`POST /api/ky-so/them-moi`** — lưu file đã ký + lịch sử khi ký lại.  
> **Logic mẫu:** [tepdinhkem-logic-goi-thau (1).md](./tepdinhkem-logic-goi-thau%20(1).md)

---

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm

| # | Mục | Ghi chú |
|---|-----|---------|
| 1 | Lưu file đã ký vào `TepDinhKem` | Endpoint `POST /api/ky-so/them-moi` (đã có từ trước) |
| 2 | Lưu **lịch sử** khi user **ký lại** (re-sign) | Không tạo bảng mới — dùng `GroupType` + `ParentId` + `IsDeleted` |
| 3 | Tránh xóa nhầm file gốc cùng `GroupId` | Không dùng `TepDinhKemBulkInsertOrUpdateCommand` (sync toàn group) |

### 1.2 `GroupType` trên `TepDinhKem`

| Giá trị | Ý nghĩa |
|---------|---------|
| `KySo` | Lần ký đầu (parent = file gốc `GoiThau`, `HopDong`, …) |
| `NoiDungDaKySo` | Phiên bản sau khi ký lại (parent = bản `KySo` / `NoiDungDaKySo` đang active) |

> `NoiDungDaKySo` là **hằng `GroupTypeConstants`**, không phải tên bảng DB.

### 1.3 Ký lần đầu vs ký lại

| | Ký lần đầu | Ký lại |
|---|------------|--------|
| Parent `GroupType` | Không thuộc `(KySo, NoiDungDaKySo)` | `KySo` hoặc `NoiDungDaKySo` |
| Bản mới `GroupType` | `KySo` | `NoiDungDaKySo` |
| Bản mới `ParentId` | Id file gốc | Id bản cha (đã ký) |
| Bản cha | — | `IsDeleted = true` |

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
  └─ ToEntities → KySoThemMoiCommand
       ├─ Ký lần 1: GroupType = KySo
       └─ Ký lại: parent.IsDeleted=true, GroupType = NoiDungDaKySo
```

Tham khảo [task-9460-ky-so-crud.md §2.1](./task-9460-ky-so-crud.md): `KySoController` còn các endpoint CRUD bảng `KySo`; endpoint `them-moi` là upload file, **không** ghi bảng `KySo`.

---

## 3. Thứ tự thực hiện

```
Bước 1: Domain - GroupTypeConstants.NoiDungDaKySo
Bước 2: Application - KySoThemMoiCommand + Handler
Bước 3: WebApi - KySoController.Create → KySoThemMoiCommand
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
/// Lịch sử phiên bản ký số trên bảng TepDinhKem (ký lại sau lần đầu).
/// </summary>
public const string NoiDungDaKySo = "NoiDungDaKySo";
```

---

### Bước 2 – Application: `KySoThemMoiCommand`

**File mới:** `QLDA.Application/KySos/Commands/KySoThemMoiCommand.cs`

```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Constants;

namespace QLDA.Application.KySos.Commands;

public record KySoThemMoiCommand : IRequest<int> {
    public required string GroupId { get; set; }
    public required List<TepDinhKem> Entities { get; set; }
}

internal class KySoThemMoiCommandHandler : IRequestHandler<KySoThemMoiCommand, int> {
    private readonly IRepository<TepDinhKem, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public KySoThemMoiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(KySoThemMoiCommand request, CancellationToken cancellationToken = default) {
        var toInsert = new List<TepDinhKem>();

        foreach (var entity in request.Entities.Where(e => e.ParentId != null)) {
            entity.GroupId = request.GroupId;

            var parent = await _repository.GetQueryableSet()
                .FirstOrDefaultAsync(e => e.Id == entity.ParentId, cancellationToken);
            ManagedException.ThrowIfNull(parent, "Không tìm thấy tệp cha (ParentId)");

            if (IsSignedVersion(parent.GroupType)) {
                parent.IsDeleted = true;
                entity.GroupType = GroupTypeConstants.NoiDungDaKySo;
                entity.ParentId = parent.Id;
            } else {
                entity.GroupType = GroupTypeConstants.KySo;
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

var count = await Mediator.Send(new KySoThemMoiCommand {
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
-- Bản ký đang dùng
SELECT * FROM TepDinhKem
WHERE GroupId = @GroupId
  AND GroupType IN ('KySo', 'NoiDungDaKySo')
  AND IsDeleted = 0;

-- Lịch sử (đã thay thế)
SELECT * FROM TepDinhKem
WHERE GroupId = @GroupId
  AND GroupType IN ('KySo', 'NoiDungDaKySo')
  AND IsDeleted = 1
  AND ParentId IS NOT NULL;
```

```
File gốc (GoiThau)     id=1
  → ký lần 1           id=2, GroupType=KySo,           ParentId=1
  → ký lại             id=3, GroupType=NoiDungDaKySo, ParentId=2, id=2 IsDeleted=1
```

---

## 6. Checklist hoàn thành

```
[x] 1. Thêm GroupTypeConstants.NoiDungDaKySo
[x] 2. Tạo KySoThemMoiCommand + Handler
[x] 3. KySoController.Create → KySoThemMoiCommand (bỏ BulkInsert + NoiDungDaKyInsert)
[x] 4. Xóa NoiDungDaKySo entity + Configuration + NoiDungDaKyInsertCommand
[x] 5. Migration DropNoiDungDaKySoTable + ef update trên DB test
[x] 6. Verify Postman: ký lần 1 + ký lại + file gốc không bị soft-delete
[x] 7. API list “nội dung đã ký” (task riêng — chưa có endpoint)
```

---

## 7. Lưu ý kỹ thuật

- Request `DanhSachTepDinhKem`: chỉ file **đã ký** (`parentId` bắt buộc); `id: null` OK nếu dùng `entities[].Id` sau `ToEntities`.
- `KySoThemMoiCommand` cập nhật `parent.IsDeleted` trong cùng transaction với `AddRange` — cần `SaveChanges` một lần (đã có).
- Không đụng `TepDinhKemBulkInsertOrUpdateCommand` — các controller khác (`GoiThau`, `HopDong`, …) vẫn dùng bulk như cũ.
- Phân biệt với [task-9460-ky-so-crud.md](./task-9460-ky-so-crud.md): bảng **`KySo`** = chứng thư / cấu hình ký số; **`TepDinhKem`** = file đính kèm đã ký trên đối tượng nghiệp vụ.

---

## 7.1 Những sửa chữa trong quá trình triển khai (21/05/2026)

### Thiết kế (V1 → V2)

| V1 (bỏ) | V2 (áp dụng) |
|---------|----------------|
| Bảng `NoiDungDaKySo` + `NoiDungDaKyInsertCommand` | Chỉ `TepDinhKem`, `GroupType` phân nhánh |
| 2 bước Mediator trong controller | 1 command `KySoThemMoiCommand` |
| `TepDinhKemBulkInsertOrUpdateCommand` + `KySo=true` | Command riêng, không sync soft-delete cả `GroupId` |

### Application Layer

| Lỗi / vấn đề V1 | Sửa V2 |
|-----------------|--------|
| `NoiDungDaKyInsertCommand` chỉ `AddAsync`, không `SaveChanges` | Bỏ command; logic gộp `KySoThemMoiCommand` có `SaveChangesAsync` |
| — | Handler set `GroupType` theo `parent.GroupType` (`IsSignedVersion`) |
| — | Ký lại: `parent.IsDeleted = true`, `entity.ParentId = parent.Id` |

### WebApi Layer

| Lỗi / vấn đề V1 | Sửa V2 |
|-----------------|--------|
| `tepDaKyMoi.GetId()` lần 2 khi `id: null` | Chỉ dùng `entities` từ `ToEntities().ToList()` |
| `TepDinhKemBulkInsertOrUpdateCommand` xóa nhầm file gốc | Đổi sang `KySoThemMoiCommand` |
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

- ✅ **1 file mới** (`KySoThemMoiCommand.cs`)
- ✅ **2 file sửa** (`GroupTypeConstants.cs`, `KySoController.cs`)
- ✅ **3 file xóa** (entity, config, insert command V1)
- ⏭️ **1 migration** còn lại: `DropNoiDungDaKySoTable`

---

### Các file **mới tạo** (1 file)

| File | Mô tả |
|------|-------|
| `QLDA.Application/KySos/Commands/KySoThemMoiCommand.cs` | Insert file ký + xử lý ký lại; trả `int` số bản ghi |

---

### Các file **chỉnh sửa** (2 files)

| # | File | Thay đổi |
|---|------|----------|
| 1 | `QLDA.Domain/Constants/GroupTypeConstants.cs` | ➕ `NoiDungDaKySo` |
| 2 | `QLDA.WebApi/Controllers/KySoController.cs` | ➖ `TepDinhKemBulkInsertOrUpdateCommand` + `NoiDungDaKyInsertCommand` → ➕ `KySoThemMoiCommand`; response `count` |

---

### Các file **đã xóa** (3 files – V1)

| File | Lý do |
|------|-------|
| `QLDA.Domain/Entities/NoiDungDaKySo.cs` | Không dùng bảng riêng |
| `QLDA.Persistence/Configurations/NoiDungDaKySoConfiguration.cs` | Không dùng bảng riêng |
| `QLDA.Application/KySos/Commands/NoiDungDaKyInsertCommand.cs` | Thay bằng `KySoThemMoiCommand` |

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

- [ ] API/query list “nội dung đã ký” filter `GroupType IN ('KySo','NoiDungDaKySo')`
- [ ] **DateTime → DateTimeOffset** (DB) và **DateOnly** (request) — theo task schema chung
