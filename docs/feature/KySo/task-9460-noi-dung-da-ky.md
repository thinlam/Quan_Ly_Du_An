# Task #9460 – Bổ sung bảng `NoiDungDaKySo` (Nội dung đã ký)

## 1. Phân tích yêu cầu

### 1.1 Bối cảnh

API `POST /api/ky-so/them-moi` hiện làm **1 việc**: insert vào bảng `TepDinhKem` (đã hoàn thành).

Task này bổ sung **việc thứ 2**: sau khi insert `TepDinhKem`, tiếp tục insert vào bảng lịch sử `NoiDungDaKySo`.

### 1.2 Schema bảng `NoiDungDaKySo`

| Trường | Kiểu | Nguồn dữ liệu | Ghi chú |
|--------|------|---------------|---------|
| `Id` | `Guid` | Auto-generate | PK của bảng `NoiDungDaKySo` |
| `TepDinhKemId` | `Guid` | `TepDinhKem.Id` | **Tệp đã ký mới vừa insert ở Bước 1** (ParentId ≠ null) |
| `FileName` | `string?` | `TepDinhKem.FileName` | Tên file đã ký |
| `FileOrginal` | `string?` | `TepDinhKem.OriginalName` | Tên file gốc trước khi upload |
| `GroupId` | `string?` | `TepDinhKem.GroupId` | Guid (dạng string) của đối tượng chủ (HopDong, GoiThau, …) |
| `GroupName` | `string?` | `TepDinhKem.GroupType` | Loại đối tượng chủ (e.g. `"KySo"`) |
| `CreateUserId` | `string` | `Entity<Guid>.CreatedBy` | Auto-set bởi `AuditInterceptor` |
| `CreateDate` | `DateTimeOffset` | Shadow property `CreatedAt` | Auto-set bởi `ConfigureForBase` |

> **Cách lấy TepDinhKemId:**
> 
> 1. **Tệp gốc (id=1):** Đã tồn tại trong DB trước đó, **KHÔNG nằm trong request** này
> 2. **Request này:** Chỉ chứa `DanhSachTepDinhKem` = các tệp **đã ký mới** (ParentId ≠ null)
> 3. **Bước 1:** Insert tệp đã ký mới (ParentId=1 trỏ về tệp gốc) → sinh id=2
> 4. **Bước 2:** Lấy tệp id=2 vừa insert → Insert vào NoiDungDaKySo
>
> **Ví dụ:**
> - Chị có tệp A gốc (id=1) đã tồn tại 
> - Chị gọi API ký số, gửi lên tệp đã ký (ParentId=1)
> - Bước 1: Insert → sinh id=2
> - Bước 2: Insert NoiDungDaKySo { TepDinhKemId: 2 }

---

## 2. Phân tích hiện trạng

### 2.1 Luồng hiện tại của `KySoController.Create`

```
POST /api/ky-so/them-moi
  └─ TepDinhKemBulkInsertOrUpdateCommand
       └─ Insert/Update tất cả TepDinhKem trong DanhSachTepDinhKem
```

### 2.2 Luồng sau khi bổ sung

```
POST /api/ky-so/them-moi
  ├─ Bước 1: TepDinhKemBulkInsertOrUpdateCommand          (đã có)
  │    └─ Insert các tệp đã ký vào TepDinhKem (đúng)
  └─ Bước 2: NoiDungDaKyInsertCommand                     (cần thêm)
       └─ Lấy TepDinhKem đầu tiên (ParentId != null) vừa insert ở Bước 1 (mới sửa)
          Insert 1 bản ghi NoiDungDaKySo với Id của tệp đó
```

### 2.3 Trạng thái triển khai (đã code, cần sửa)

| Hạng mục | Trạng thái |
|----------|------------|
| Domain `NoiDungDaKySo` | ✅ Đã có |
| Configuration + Migration `AddNoiDungDaKySo` | ✅ Đã có — cần **chạy migration** trên DB môi trường test/prod |
| `NoiDungDaKyInsertCommand` | ⚠️ Đã có — **thiếu `SaveChangesAsync`** |
| `KySoController.Create` Bước 2 | ⚠️ Đã có — **sai `TepDinhKemId`** khi request `id: null` |

### 2.4 Review code — lỗi khiến bảng `NoiDungDaKySo` trống dù API `200 OK`

API trả `{"result":true,"dataResult":1}` chỉ phản ánh **Bước 1** (`TepDinhKem` đã commit). Bảng lịch sử vẫn trống vì:

| # | Lỗi | Mô tả |
|---|-----|--------|
| 1 | **Không persist Bước 2** | `NoiDungDaKyInsertCommandHandler` chỉ `AddAsync`, không `SaveChangesAsync`. Controller cũng không gọi save sau Bước 2. |
| 2 | **`GetId()` gọi 2 lần** | Request `id: null` → `ToEntities()` gọi `GetId()` lần 1 (Id **A** insert DB). Bước 2 `tepDaKyMoi.GetId()` lần 2 → Id **B** ≠ A → FK sai khi đã save. |
| 3 | **Sync soft-delete file gốc** (cần xác nhận) | Bước 1 dùng `SyncHelper` soft-delete: mọi `TepDinhKem` cùng `GroupId` **không có trong request** có thể bị `IsDeleted = true` (kể cả file gốc). Cờ `KySo` chỉ được xử lý ở `InsertOrUpdateCascadeAsync` — luồng hiện tại **không** dùng method đó. |

**Cách verify trên DB:**

```sql
-- Bước 1 – thường có dòng
SELECT Id, ParentId, FileName, IsDeleted
FROM TepDinhKem
WHERE GroupId = '<groupId-trong-request>';

-- Bước 2 – hiện thường trống nếu chưa sửa
SELECT * FROM NoiDungDaKySo
WHERE GroupId = '<groupId-trong-request>';
```

---

## 3. Thứ tự thực hiện

```
[Đã xong] Bước 1–3: Domain, Configuration, Migration AddNoiDungDaKySo
[Sửa]     Bước 4:   NoiDungDaKyInsertCommand — thêm SaveChangesAsync
[Sửa]     Bước 5:   KySoController.Create — dùng entity.Id sau ToEntities (không GetId() lần 2)
[Tùy chọn] Bước 6: TepDinhKemBulkInsertOrUpdateCommand — khi KySo=true không soft-delete file gốc
```

---

## 4. Chi tiết từng bước

---

### Bước 1 – Domain: Entity `NoiDungDaKySo`

**File mới:** `QLDA.Domain/Entities/NoiDungDaKySo.cs`

```csharp
namespace QLDA.Domain.Entities;

/// <summary>
/// Lịch sử nội dung đã ký số
/// </summary>
public class NoiDungDaKySo : Entity<Guid>, IAggregateRoot {
    /// <summary>
    /// FK → TepDinhKem – tệp đã ký (ParentId != null) vừa được insert ở Bước 1
    /// </summary>
    public Guid TepDinhKemId { get; set; } 

    /// <summary>
    /// Tên tệp đã lưu (từ TepDinhKem.FileName)
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Tên tệp gốc (từ TepDinhKem.OriginalName)
    /// </summary>
    public string? FileOrginal { get; set; }

    /// <summary>
    /// Id đối tượng chủ (từ TepDinhKem.GroupId)
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Tên loại đối tượng chủ (từ TepDinhKem.GroupType)
    /// </summary>
    public string? GroupName { get; set; }

    // CreateUserId → CreatedBy (kế thừa từ Entity<Guid>, auto-set bởi AuditInterceptor)
    // CreateDate   → CreatedAt shadow property (auto-set bởi ConfigureForBase)

    #region Navigation Properties
    public TepDinhKem? TepDinhKem { get; set; }
    #endregion
}
```

---

### Bước 2 – Persistence: Configuration `NoiDungDaKySo`

**File mới:** `QLDA.Persistence/Configurations/NoiDungDaKySoConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class NoiDungDaKySoConfiguration : AggregateRootConfiguration<NoiDungDaKySo> {
    public override void Configure(EntityTypeBuilder<NoiDungDaKySo> builder) {
        builder.ToTable("NoiDungDaKySo");
        builder.ConfigureForBase();

        builder.Property(e => e.FileName).HasMaxLength(500);
        builder.Property(e => e.FileOrginal).HasMaxLength(500);
        builder.Property(e => e.GroupId).HasMaxLength(100);
        builder.Property(e => e.GroupName).HasMaxLength(200);

        builder.HasOne(e => e.TepDinhKem)
            .WithMany()
            .HasForeignKey(e => e.TepDinhKemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

### Bước 3 – Migration

> Chạy lệnh trong thư mục `e:\SER\QLDA.Migrator`

```bash
ef.bat QLDA add AddNoiDungDaKySo
```

Kiểm tra migration được tạo ra có đúng bảng `NoiDungDaKySo` với đủ cột không.

---

### Bước 4 – Application: `NoiDungDaKyInsertCommand`

**File mới:** `QLDA.Application/KySos/Commands/NoiDungDaKyInsertCommand.cs`

```csharp
namespace QLDA.Application.KySos.Commands;

public record NoiDungDaKyInsertCommand : IRequest {
    public required Guid TepDinhKemId { get; set; }
    public string? FileName { get; set; }
    public string? FileOrginal { get; set; }
    public string? GroupId { get; set; }
    public string? GroupName { get; set; }
}

internal class NoiDungDaKyInsertCommandHandler : IRequestHandler<NoiDungDaKyInsertCommand> {
    private readonly IRepository<NoiDungDaKySo, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public NoiDungDaKyInsertCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<NoiDungDaKySo, Guid>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task Handle(NoiDungDaKyInsertCommand request, CancellationToken cancellationToken = default) {
        var entity = new NoiDungDaKySo {
            TepDinhKemId = request.TepDinhKemId,
            FileName     = request.FileName,
            FileOrginal  = request.FileOrginal,
            GroupId      = request.GroupId,
            GroupName    = request.GroupName,
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}
```

> **Lưu ý:** Pattern giống `KySoInsertCommand` — handler tự transaction + `SaveChangesAsync`. **Không** để comment “caller save” vì `KySoController` không gọi `SaveChanges` sau Mediator.

> **Using cần thêm:** `using System.Data;`

---

### Bước 5 – WebApi: Cập nhật `KySoController.Create`

**File sửa:** `QLDA.WebApi/Controllers/KySoController.cs`

```csharp
[HttpPost("them-moi")]
[Consumes(MediaTypeNames.Application.Json)]
public async Task<ResultApi> Create([FromBody] KySoModel model) {
    ManagedException.ThrowIfNull(model.DanhSachTepDinhKem);
    model.DanhSachTepDinhKem ??= [];

    // Materialize 1 lần — Id sinh từ GetId() chỉ gọi 1 lần / item (khi request id: null)
    var entities = model.DanhSachTepDinhKem
        .ToEntities(model.GroupId, GroupTypeConstants.KySo)
        .ToList();

    // ── Bước 1: Insert tệp đã ký vào TepDinhKem ──────────────────────────
    await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
        KySo    = true,
        GroupId = model.GroupId.ToString(),
        Entities = entities,
    });

    // ── Bước 2: Insert NoiDungDaKySo từ entity vừa insert (ParentId != null) ─
    // Dùng entities[].Id — KHÔNG gọi model.GetId() lần 2
    var tepDaKy = entities.FirstOrDefault(e => e.ParentId != null);
    if (tepDaKy is not null) {
        await Mediator.Send(new NoiDungDaKyInsertCommand {
            TepDinhKemId = tepDaKy.Id,
            FileName     = tepDaKy.FileName,
            FileOrginal  = tepDaKy.OriginalName,
            GroupId      = model.GroupId.ToString(),
            GroupName    = GroupTypeConstants.KySo,
        });
    }

    return ResultApi.Ok(1);
}
```

> **Quan trọng — `TepDinhKemId`:**
> - ❌ `tepDaKyMoi.GetId()` trên `TepDinhKemModel` khi `Id == null` → Guid **khác** Id đã insert ở Bước 1.
> - ✅ `tepDaKy.Id` từ list `entities` đã truyền vào `TepDinhKemBulkInsertOrUpdateCommand`.

> **Lưu ý về transaction:** `TepDinhKemBulkInsertOrUpdateCommand` và `NoiDungDaKyInsertCommand` mỗi cái tự commit transaction. Hai bước **độc lập** (Bước 1 xong rồi mới Bước 2). Nếu cần atomic, mở `IUnitOfWork` transaction ở controller trước cả hai lệnh Mediator.

> **Request mẫu (Postman):** `parentId` bắt buộc có để Bước 2 chạy; `id: null` là hợp lệ nếu dùng `entities[].Id` như trên.

---

### Bước 6 (tùy chọn) – `TepDinhKemBulkInsertOrUpdateCommand` khi `KySo = true`

**Vấn đề:** `InsertOrUpdateAsync` sync theo `GroupId` — request ký số chỉ gửi file child → file gốc cùng group có thể bị soft-delete.

**Hướng xử lý (chọn một):**

1. Khi `request.KySo == true`, trong `InsertOrUpdateAsync` **không** soft-delete các bản ghi có `ParentId == null` (file gốc); chỉ add/update file trong request.
2. Hoặc chuyển luồng ký số sang `InsertOrUpdateCascadeAsync` (đã có `!request.KySo` khi xóa) — cần review kỹ trước khi đổi.

**Phụ:** Dòng 32–33 trong handler đang gọi `InsertOrUpdateAsync` **hai lần** liên tiếp — nên xóa bớt một lần khi sửa file này.

---

## 5. Mapping dữ liệu tóm tắt

```
【BƯỚC 1】Insert tệp đã ký (ParentId != null) vào TepDinhKem
  → sinh Id mới (e.g. Id = 2)

【BƯỚC 2】Lấy entity vừa map ở Bước 1 (cùng list `entities`), mapping vào NoiDungDaKySo:

entities.FirstOrDefault(e => e.ParentId != null)  ← tệp ĐÃ KÝ MỚI (domain entity)
  │
  ├─ .Id             → NoiDungDaKySo.TepDinhKemId      (cùng Id đã insert Bước 1)
  ├─ .FileName       → NoiDungDaKySo.FileName
  ├─ .OriginalName   → NoiDungDaKySo.FileOrginal
  ├─ model.GroupId   → NoiDungDaKySo.GroupId
  └─ GroupTypeConstants.KySo → NoiDungDaKySo.GroupName

  Handler: AddAsync + SaveChangesAsync + CommitTransaction

  CreatedBy (auto AuditInterceptor)  → CreateUserId
  CreatedAt (auto ConfigureForBase)  → CreateDate

【EXAMPLE】
  - Tệp A gốc (id=1) đã tồn tại trong DB
  - Gọi API ký số, gửi tệp đã ký (ParentId=1, id=null trong JSON)
    - ToEntities → sinh Id=2 (một lần)
    - Bước 1: Insert TepDinhKem id=2
    - Bước 2: Insert NoiDungDaKySo { TepDinhKemId: 2 } + SaveChanges
```

---

## 6. Checklist sau khi sửa code

- [ ] Gọi lại `POST /api/ky-so/them-moi` — `SELECT * FROM NoiDungDaKySo` có 1 dòng mới
- [ ] `TepDinhKemId` trong `NoiDungDaKySo` = `Id` của dòng `TepDinhKem` vừa insert (cùng `ParentId IS NOT NULL`)
- [ ] Migration `AddNoiDungDaKySo` đã apply trên DB server test
- [ ] (Tùy chọn) File gốc cùng `GroupId` vẫn `IsDeleted = 0` sau ký số
- [ ] UI/API danh sách “nội dung đã ký”: hiện **chưa có** query endpoint — cần task riêng nếu màn hình cần load bảng
