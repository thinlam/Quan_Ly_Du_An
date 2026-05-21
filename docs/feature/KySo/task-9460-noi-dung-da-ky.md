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

### 2.3 Bảng `NoiDungDaKySo` chưa tồn tại

Cần tạo mới từ đầu: Domain entity → Persistence configuration → Migration → Application command → cập nhật Controller.

---

## 3. Thứ tự thực hiện

```
Bước 1: Domain   – Entity NoiDungDaKySo
Bước 2: Persist  – Configuration NoiDungDaKySoConfiguration
Bước 3: Migrator – Migration AddNoiDungDaKySo
Bước 4: App      – NoiDungDaKyInsertCommand
Bước 5: WebApi   – Cập nhật KySoController.Create
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

        await _repository.AddAsync(entity, cancellationToken);
        // SaveChanges sẽ do caller (controller transaction) thực hiện
    }
}
```

> **Lưu ý:** `SaveChangesAsync` và transaction được quản lý bởi controller (xem Bước 5). Handler chỉ `AddAsync`.

---

### Bước 5 – WebApi: Cập nhật `KySoController.Create`

**File sửa:** `QLDA.WebApi/Controllers/KySoController.cs`

```csharp
[HttpPost("them-moi")]
[Consumes(MediaTypeNames.Application.Json)]
public async Task<ResultApi> Create([FromBody] KySoModel model) {
    ManagedException.ThrowIfNull(model.DanhSachTepDinhKem);
    model.DanhSachTepDinhKem ??= [];

    // ── Bước 1: Insert tệp đã ký vào TepDinhKem ──────────────────────────
    // DanhSachTepDinhKem chứa các tệp đã ký (ParentId != null)
    // Sau lệnh này: TepDinhKem mới với Id vừa được sinh
    await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
        KySo      = true,
        GroupId   = model.GroupId.ToString(),
        Entities  = [.. model.DanhSachTepDinhKem.ToEntities(model.GroupId, GroupTypeConstants.KySo)]
    });

    // ── Bước 2: Insert NoiDungDaKySo từ tệp đã ký mới ──────────────────────
    // Lấy TepDinhKem mới vừa được insert ở Bước 1 (ParentId != null)
    // Sau đó insert ID của tệp mới này vào NoiDungDaKySo
    var tepDaKyMoi = model.DanhSachTepDinhKem.FirstOrDefault(e => e.ParentId != null);
    if (tepDaKyMoi is not null) {
        await Mediator.Send(new NoiDungDaKyInsertCommand {
            TepDinhKemId = tepDaKyMoi.GetId(),     // ← ID tệp đã ký mới từ Bước 1
            FileName     = tepDaKyMoi.FileName,    // ← Tên tệp đã ký
            FileOrginal  = tepDaKyMoi.OriginalName, // ← Tên tệp gốc
            GroupId      = model.GroupId.ToString(),
            GroupName    = GroupTypeConstants.KySo,
        });
    }

    return ResultApi.Ok(1);
}
```

> **Lưu ý về transaction:** `TepDinhKemBulkInsertOrUpdateCommand` tự mở transaction riêng khi không có transaction active. Nếu cần 2 việc trong cùng 1 transaction, cần mở transaction ngoài trước rồi commit sau. Hiện tại, 2 insert độc lập nhau là chấp nhận được vì nếu insert `NoiDungDaKySo` lỗi, `TepDinhKem` vẫn đã được commit. Nếu yêu cầu atomic, cần refactor dùng `IUnitOfWork` chung.

---

## 5. Mapping dữ liệu tóm tắt

```
【BƯỚC 1】Insert tệp đã ký (ParentId != null) vào TepDinhKem
  → sinh Id mới (e.g. Id = 2)

【BƯỚC 2】Lấy tệp vừa insert ở Bước 1, mapping vào NoiDungDaKySo:

model.DanhSachTepDinhKem
  .FirstOrDefault(e => e.ParentId != null)  ← tệp ĐÃ KÝ MỚI
  │
  ├─ .GetId()        → NoiDungDaKySo.TepDinhKemId      (Id tệp mới: 2)
  ├─ .FileName       → NoiDungDaKySo.FileName          (tên tệp đã ký)
  ├─ .OriginalName   → NoiDungDaKySo.FileOrginal       (tên gốc)
  ├─ model.GroupId   → NoiDungDaKySo.GroupId
  └─ GroupTypeConstants.KySo → NoiDungDaKySo.GroupName

  CreatedBy (auto AuditInterceptor)  → CreateUserId
  CreatedAt (auto ConfigureForBase)  → CreateDate

【EXAMPLE】
  - Tệp A gốc (id=1) đã tồn tại trong DB
  - Chị gọi API ký số, gửi lên tệp đã ký (ParentId=1)
    - Bước 1: Insert tệp đã ký → id=2
    - Bước 2: Insert NoiDungDaKySo { TepDinhKemId: 2 }
```
