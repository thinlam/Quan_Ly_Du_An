# Task – Bổ sung UC Quản lý Bàn Giao Hồ Sơ (BanGiaoHoSo)

> **✅ Updated from Task 9488 (05/05/2026)**
> - Tệp đính kèm: Dùng pattern `GroupId` thay vì FK trực tiếp
> - Loại bỏ `TepDinhKemService` và `ExportService`
> - Dùng `TepDinhKemBulkInsertOrUpdateCommand` để lưu file
> - WebApi Model pattern giống `HoSoDeXuatCapDoCnttModel`
> - Thêm `EGroupType.BanGiaoHoSo` và `EGroupType.BienBanBanGiao` vào enum

> **✅ Updated (12/05/2026)**
> - Đổi tên endpoint `thay-doi-trang-thai` → `ban-giao`
> - Thêm cột `NgayBanGiao` (DateTimeOffset?) vào entity
> - Endpoint `ban-giao` nhận thêm `DanhSachBienBan` (biên bản bàn giao)
> - Bộ hồ sơ lưu trữ bàn giao gồm 2 loại tệp: **tệp HS bàn giao** (`EGroupType.BanGiaoHoSo`) và **biên bản bàn giao** (`EGroupType.BienBanBanGiao`)

---

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm

| # | Mục | Ghi chú |
|---|-----|---------|
| 1 | Bổ sung **entity** `BanGiaoHoSo` mới | Quản lý bàn giao hồ sơ từ người dùng → Phòng HC-TH |
| 2 | Bổ sung **Enum** `ETrangThaiBanGiao` | 0: Khởi tạo, 1: Đã bàn giao |
| 3 | Bổ sung **6 API**: list (filter), insert, update, ban-giao (đổi trạng thái + biên bản), delete (có điều kiện) | Theo luồng CQRS hiện tại |
| 4 | Hỗ trợ **đính kèm file** (TepDinhKem) – 2 loại | **Tệp HS bàn giao** (`EGroupType.BanGiaoHoSo`) + **Biên bản bàn giao** (`EGroupType.BienBanBanGiao`) |

### 1.2 Model BanGiaoHoSo

| Field | Kiểu | Ghi chú |
|-------|------|---------|
| `Id` | `Guid` | Auto-generate sequential guid |
| `Ma` | `string` | Mã bản giao hồ sơ (unique) |
| `TenHoSo` | `string` | Tên hồ sơ |
| `PhongBanChuTriId` | `int?` | FK → Danh mục phòng ban (HC-TH) |
| `TrangThai` | `bit` (0/1) | 0: Khởi tạo, 1: Đã bàn giao → Enum `ETrangThaiBanGiao` |
| `NgayBanGiao` | `DateTimeOffset?` | Ngày bàn giao – set khi gọi endpoint `ban-giao` |
| `UserId` | `long?` | FK → UserMaster (người tạo/chủ sở hữu - từ Auth) |
| `IsDeleted` | `bit` | Soft delete flag |
| `CreatedAt`, `UpdatedAt` | `DateTime` | Audit fields |

**Tệp đính kèm** (TepDinhKem):
- Lưu trữ qua bảng `TepDinhKem` (không có FK trực tiếp)
- `GroupId` = `BanGiaoHoSo.Id.ToString()`
- Truy vấn: `TepDinhKem.Where(f => f.GroupId == entity.Id.ToString())`

### 1.3 Enum ETrangThaiBanGiao

```csharp
public enum ETrangThaiBanGiao {
    KhoiTao = 0,           // Khởi tạo
    DaBanGiao = 1          // Đã bàn giao
}
```

### 1.4 Logic Danh Sách

**Chưa bàn giao:**
```
UserId = userInfo.UserId (người tạo - chính mình)
TrangThai = 0
```

**Đã bàn giao:**
```
UserId = userInfo.UserId (của chính mình)
TrangThai = 1
```

**Query params:**
```
TrangThai (UI truyền - 1 param duy nhất)
UserId (KHÔNG cho UI truyền - luôn lấy từ Auth)
```

### 1.5 Tệp Đính Kèm (TepDinhKem)

Bộ hồ sơ lưu trữ bàn giao gồm **2 loại tệp**, cùng `GroupId` nhưng khác `EGroupType`:

| Loại tệp | EGroupType | Gắn khi nào | Ghi chú |
|----------|------------|------------|----------|
| Tệp HS bàn giao | `BanGiaoHoSo` | Insert / Update | Hồ sơ đính kèm thông thường |
| Biên bản bàn giao | `BienBanBanGiao` | Endpoint `ban-giao` | Biên bản chính thức khi thực hiện bàn giao |

- `GroupId` = `BanGiaoHoSo.Id.ToString()` (dùng chung cho cả 2 loại)
- Dùng `TepDinhKemBulkInsertOrUpdateCommand` để lưu file
- WebApi model insert/update có `List<TepDinhKemModel>? DanhSachTepDinhKem` (từ interface `IMayHaveTepDinhKemDto`)
- WebApi model cũng có `List<TepDinhKemModel>? DanhSachBienBanBanGiao` (chỉ đọc khi hiển thị)
- WebApi model ban-giao có `List<TepDinhKemModel>? DanhSachBienBan`
- Extension methods: `.GetDanhSachTepHSBanGiao()` và `.GetDanhSachBienBanBanGiao()`

---

## 2. Phân tích hiện trạng

### 2.1 BanGiaoHoSo là entity nghiệp vụ (không phải DanhMuc)

- Có **FK tới Phòng Ban** (`PhongBanChuTriId`)
- Có **UserId từ Auth** → phải filter theo user hiện tại
- Có logic riêng → phải viết **Commands/Queries đầy đủ** như `DuAn`, `HopDong`, `HoSoMoiThauDienTu`
- Hỗ trợ **đính kèm file** → integrate với `TepDinhKem` (2 loại: `BanGiaoHoSo` + `BienBanBanGiao`)
- **Delete có điều kiện:** chỉ xóa được khi `TrangThai = 0`
- **Ban-giao endpoint:** đổi trạng thái 0 → 1, set `NgayBanGiao`, đính kèm biên bản bàn giao

### 2.2 Pattern danh mục chuẩn trong dự án

Danh mục loại đơn giản (int PK, có ma/ten/moTa/stt/used):
- Kế thừa `DanhMuc<int>`, implement `IAggregateRoot, IMayHaveStt`
- CRUD đi qua `DanhMucGetQuery` + `DanhMucInsertOrUpdateCommand` (shared command)
- **Không cần viết Commands/Queries riêng** – chỉ cần đăng ký vào `EDanhMuc`

---

## 3. Thứ tự thực hiện

```
Bước 1: Domain - Enum ETrangThaiBanGiao
Bước 2: Domain - Entity BanGiaoHoSo (+ NgayBanGiao)
Bước 3: Persistence - Configuration BanGiaoHoSo
Bước 4: Persistence - Migration
Bước 5: Application - BanGiaoHoSo DTOs + Mappings
Bước 6: Application - BanGiaoHoSo Commands (Insert, Update, BanGiao, Delete)
Bước 7: Application - BanGiaoHoSo Queries (Get, GetList)
Bước 8: WebApi - BanGiaoHoSo Controller
Bước 9: Build + Test
```

---

## 4. Chi tiết từng bước

---

### Bước 1 – Domain: Enum `ETrangThaiBanGiao`

**File:** `QLDA.Domain/Enums/ETrangThaiBanGiao.cs`

```csharp
namespace QLDA.Domain.Enums;

/// <summary>
/// Trạng thái bàn giao hồ sơ
/// </summary>
public enum ETrangThaiBanGiao {
    /// <summary>
    /// Khởi tạo - chưa bàn giao
    /// </summary>
    KhoiTao = 0,

    /// <summary>
    /// Đã bàn giao cho phòng HC-TH
    /// </summary>
    DaBanGiao = 1
}
```

---

### Bước 2 – Domain: Entity `BanGiaoHoSo`

**File:** `QLDA.Domain/Entities/BanGiaoHoSo.cs`

```csharp
using QLDA.Domain.Enums;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng quản lý bàn giao hồ sơ từ người dùng → Phòng HC-TH
/// </summary>
public class BanGiaoHoSo : Entity<Guid>, IAggregateRoot {
    /// <summary>
    /// Mã bản giao hồ sơ
    /// </summary>
    public string? Ma { get; set; }

    /// <summary>
    /// Tên hồ sơ
    /// </summary>
    public string? TenHoSo { get; set; }

    /// <summary>
    /// FK → Phòng ban chủ trì (phòng HC-TH hoặc tương tự)
    /// </summary>
    public int? PhongBanChuTriId { get; set; }

    /// <summary>
    /// Trạng thái: 0 = Khởi tạo, 1 = Đã bàn giao
    /// </summary>
    public ETrangThaiBanGiao TrangThai { get; set; } = ETrangThaiBanGiao.KhoiTao;

    /// <summary>
    /// Ngày bàn giao – set khi gọi endpoint ban-giao
    /// </summary>
    public DateTimeOffset? NgayBanGiao { get; set; }

    /// <summary>
    /// FK → UserMaster (người tạo hồ sơ - từ Auth)
    /// </summary>
    public long? UserId { get; set; }

    #region Navigation Properties
    public UserMaster? User { get; set; }
    public DanhMucPhongBan? PhongBanChuTri { get; set; }
    #endregion
}
```

> **Lưu ý:** `Entity<Guid>` là base class cho các aggregate root có GUID key.

---

### Bước 3 – Persistence: Configuration `BanGiaoHoSo`

**File:** `QLDA.Persistence/Configurations/BanGiaoHoSoConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class BanGiaoHoSoConfiguration : AggregateRootConfiguration<BanGiaoHoSo> {
    public override void Configure(EntityTypeBuilder<BanGiaoHoSo> builder) {
        builder.ToTable("BanGiaoHoSo");
        builder.ConfigureForBase();  // Id, IsDeleted, CreatedAt, ...

        builder.Property(e => e.Ma)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.TenHoSo)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.TrangThai)
            .HasConversion<int>();  // Lưu enum dưới dạng int

        builder.Property(e => e.NgayBanGiao)
            .IsRequired(false);

        // Index: Tìm kiếm nhanh theo UserId + TrangThai
        builder.HasIndex(e => new { e.UserId, e.TrangThai });

        // FK → UserMaster
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // FK → Phòng Ban
        builder.HasOne(e => e.PhongBanChuTri)
            .WithMany()
            .HasForeignKey(e => e.PhongBanChuTriId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

---

### Bước 4 – Persistence: Migration

```bash
# Chạy trong thư mục QLDA.Migrator
dotnet ef migrations add AddBanGiaoHoSo
dotnet ef database update
```

**⚠️ IMPORTANT:** Không chạy `drop-database`. Chỉ chạy `migrations add` và `database update`.

---

### Bước 5 – Application: DTOs, Mappings

**Cấu trúc folder:**

```
QLDA.Application/BanGiaoHoSos/
  DTOs/
    BanGiaoHoSoDto.cs
    BanGiaoHoSoInsertDto.cs
    BanGiaoHoSoUpdateModel.cs
    BanGiaoHoSoBanGiaoDto.cs
    BanGiaoHoSoSearchDto.cs
    BanGiaoHoSoMappings.cs
  Commands/
    BanGiaoHoSoInsertCommand.cs
    BanGiaoHoSoUpdateCommand.cs
    BanGiaoHoSoDeleteCommand.cs
    BanGiaoHoSoBanGiaoCommand.cs
  Queries/
    BanGiaoHoSoGetQuery.cs
    BanGiaoHoSoGetDanhSachQuery.cs
```

#### `BanGiaoHoSoInsertDto.cs`

```csharp
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

public class BanGiaoHoSoInsertDto : IMayHaveTepDinhKemDto {
    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public int? PhongBanChuTriId { get; set; }
    // Tệp HS bàn giao (gắn khi insert/update)
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
```

#### `BanGiaoHoSoBanGiaoDto.cs`

```csharp
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

/// <summary>
/// DTO cho endpoint ban-giao: đổi trạng thái 0→1, set ngày bàn giao, đính kèm biên bản
/// </summary>
public class BanGiaoHoSoBanGiaoDto {
    /// <summary>Ngày bàn giao, mặc định là DateTimeOffset.Now nếu null</summary>
    public DateTimeOffset? NgayBanGiao { get; set; }
    // Biên bản bàn giao (gắn khi thực hiện bàn giao)
    public List<TepDinhKemDto>? DanhSachBienBan { get; set; }
}
```

#### `BanGiaoHoSoUpdateModel.cs`

```csharp
namespace QLDA.Application.BanGiaoHoSos.DTOs;

public class BanGiaoHoSoUpdateModel : BanGiaoHoSoInsertDto {
    public Guid Id { get; set; }
}
```

#### `BanGiaoHoSoDto.cs`

```csharp
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

public class BanGiaoHoSoDto {
    public Guid Id { get; set; }
    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public int? PhongBanChuTriId { get; set; }
    public string? TenPhongBan { get; set; }
    public int TrangThai { get; set; }  // 0: Khởi tạo, 1: Đã bàn giao
    public string? TenTrangThai { get; set; }
    public DateTimeOffset? NgayBanGiao { get; set; }
    public long? UserId { get; set; }
    public string? TenNguoiTao { get; set; }
    // Tệp HS bàn giao (EGroupType.BanGiaoHoSo)
    public List<TepDinhKemDto>? DanhSachTepHSBanGiao { get; set; }
    // Biên bản bàn giao (EGroupType.BienBanBanGiao)
    public List<TepDinhKemDto>? DanhSachBienBanBanGiao { get; set; }
}
```

#### `BanGiaoHoSoSearchDto.cs`

```csharp
namespace QLDA.Application.BanGiaoHoSos.DTOs;

// Chỉ 1 param từ UI (UserId luôn lấy từ Auth, không cho UI truyền)
public class BanGiaoHoSoSearchDto {
    public int? TrangThai { get; set; }  // 0: Khởi tạo, 1: Đã bàn giao
}
```

#### `BanGiaoHoSoMappings.cs`

```csharp
using QLDA.Domain.Enums;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

public static class BanGiaoHoSoMappings {
    public static BanGiaoHoSo ToEntity(this BanGiaoHoSoInsertDto dto) => new() {
        Id = GuidExtensions.GetSequentialGuidId(),
        Ma = dto.Ma,
        TenHoSo = dto.TenHoSo,
        PhongBanChuTriId = dto.PhongBanChuTriId,
        TrangThai = ETrangThaiBanGiao.KhoiTao,
    };

    public static void Update(this BanGiaoHoSo entity, BanGiaoHoSoUpdateModel dto) {
        entity.Ma = dto.Ma;
        entity.TenHoSo = dto.TenHoSo;
        entity.PhongBanChuTriId = dto.PhongBanChuTriId;
    }

    public static BanGiaoHoSoDto ToDto(this BanGiaoHoSo entity,
        List<TepDinhKem>? tepHSBanGiao = null,
        List<TepDinhKem>? bienBanBanGiao = null) => new() {
        Id = entity.Id,
        Ma = entity.Ma,
        TenHoSo = entity.TenHoSo,
        PhongBanChuTriId = entity.PhongBanChuTriId,
        TenPhongBan = entity.PhongBanChuTri?.Ten,
        TrangThai = (int)entity.TrangThai,
        TenTrangThai = GetTrangThaiText(entity.TrangThai),
        NgayBanGiao = entity.NgayBanGiao,
        UserId = entity.UserId,
        TenNguoiTao = entity.User?.HoTen,
        DanhSachTepHSBanGiao = tepHSBanGiao?.Select(f => f.ToDto()).ToList(),
        DanhSachBienBanBanGiao = bienBanBanGiao?.Select(f => f.ToDto()).ToList()
    };

    private static string GetTrangThaiText(ETrangThaiBanGiao trangThai) {
        return trangThai switch {
            ETrangThaiBanGiao.KhoiTao => "Khởi tạo",
            ETrangThaiBanGiao.DaBanGiao => "Đã bàn giao",
            _ => "Không xác định"
        };
    }
}
```

---

### Bước 6 – Application: Commands

#### `BanGiaoHoSoInsertCommand.cs`

```csharp
using System.Data;
using BuildingBlocks.Domain.Providers;

namespace QLDA.Application.BanGiaoHoSos.Commands;

public record BanGiaoHoSoInsertCommand(BanGiaoHoSoInsertDto Dto) : IRequest<BanGiaoHoSo>;

internal class BanGiaoHoSoInsertCommandHandler : IRequestHandler<BanGiaoHoSoInsertCommand, BanGiaoHoSo> {
    private readonly IRepository<BanGiaoHoSo, Guid> BanGiaoHoSo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProvider _userProvider;

    public BanGiaoHoSoInsertCommandHandler(IServiceProvider serviceProvider) {
        BanGiaoHoSo = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _unitOfWork = BanGiaoHoSo.UnitOfWork;
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    }

    public async Task<BanGiaoHoSo> Handle(BanGiaoHoSoInsertCommand request, CancellationToken cancellationToken = default) {
        var entity = request.Dto.ToEntity();
        entity.UserId = _userProvider.Id;  // Lấy từ JWT token qua IUserProvider

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await BanGiaoHoSo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
```

#### `BanGiaoHoSoUpdateCommand.cs`

```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.BanGiaoHoSos.Commands;

public record BanGiaoHoSoUpdateCommand(BanGiaoHoSoUpdateModel Model) : IRequest<BanGiaoHoSo>;

internal class BanGiaoHoSoUpdateCommandHandler : IRequestHandler<BanGiaoHoSoUpdateCommand, BanGiaoHoSo> {
    private readonly IRepository<BanGiaoHoSo, Guid> BanGiaoHoSo;
    private readonly IUnitOfWork _unitOfWork;

    public BanGiaoHoSoUpdateCommandHandler(IServiceProvider serviceProvider) {
        BanGiaoHoSo = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _unitOfWork = BanGiaoHoSo.UnitOfWork;
    }

    public async Task<BanGiaoHoSo> Handle(BanGiaoHoSoUpdateCommand request, CancellationToken cancellationToken = default) {
        var entity = await BanGiaoHoSo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Model.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.Update(request.Model);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await BanGiaoHoSo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
```

#### `BanGiaoHoSoBanGiaoCommand.cs`

```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Enums;

namespace QLDA.Application.BanGiaoHoSos.Commands;

/// <summary>
/// Thực hiện bàn giao hồ sơ: đổi trạng thái 0→1, set NgayBanGiao, lưu biên bản
/// </summary>
public record BanGiaoHoSoBanGiaoCommand(
    Guid Id,
    DateTimeOffset NgayBanGiao
) : IRequest<BanGiaoHoSo>;

internal class BanGiaoHoSoBanGiaoCommandHandler : IRequestHandler<BanGiaoHoSoBanGiaoCommand, BanGiaoHoSo> {
    private readonly IRepository<BanGiaoHoSo, Guid> BanGiaoHoSo;
    private readonly IUnitOfWork _unitOfWork;

    public BanGiaoHoSoBanGiaoCommandHandler(IServiceProvider serviceProvider) {
        BanGiaoHoSo = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _unitOfWork = BanGiaoHoSo.UnitOfWork;
    }

    public async Task<BanGiaoHoSo> Handle(BanGiaoHoSoBanGiaoCommand request, CancellationToken cancellationToken = default) {
        var entity = await BanGiaoHoSo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.TrangThai = ETrangThaiBanGiao.DaBanGiao;
        entity.NgayBanGiao = request.NgayBanGiao;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await BanGiaoHoSo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
```

#### `BanGiaoHoSoDeleteCommand.cs`

```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Enums;

namespace QLDA.Application.BanGiaoHoSos.Commands;

public record BanGiaoHoSoDeleteCommand(Guid Id) : IRequest;

internal class BanGiaoHoSoDeleteCommandHandler : IRequestHandler<BanGiaoHoSoDeleteCommand> {
    private readonly IRepository<BanGiaoHoSo, Guid> BanGiaoHoSo;
    private readonly IUnitOfWork _unitOfWork;

    public BanGiaoHoSoDeleteCommandHandler(IServiceProvider serviceProvider) {
        BanGiaoHoSo = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _unitOfWork = BanGiaoHoSo.UnitOfWork;
    }

    public async Task Handle(BanGiaoHoSoDeleteCommand request, CancellationToken cancellationToken = default) {
        var entity = await BanGiaoHoSo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Chỉ cho phép xóa khi TrangThai = 0 (Khởi tạo)
        if (entity.TrangThai != ETrangThaiBanGiao.KhoiTao) {
            throw new InvalidOperationException("Chỉ có thể xóa bản giao hồ sơ ở trạng thái 'Khởi tạo'");
        }

        entity.IsDeleted = true;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await BanGiaoHoSo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}
```

---

### Bước 7 – Application: Queries

#### `BanGiaoHoSoGetQuery.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using QLDA.Application.TepDinhKems.Queries;

namespace QLDA.Application.BanGiaoHoSos.Queries;

public record BanGiaoHoSoGetQuery : IRequest<BanGiaoHoSo> {
    public Guid Id { get; set; }
}

internal class BanGiaoHoSoGetQueryHandler : IRequestHandler<BanGiaoHoSoGetQuery, BanGiaoHoSo> {
    private readonly IRepository<BanGiaoHoSo, Guid> BanGiaoHoSo;

    public BanGiaoHoSoGetQueryHandler(IServiceProvider serviceProvider) {
        BanGiaoHoSo = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
    }

    public async Task<BanGiaoHoSo> Handle(BanGiaoHoSoGetQuery request, CancellationToken cancellationToken = default) {
        var entity = await BanGiaoHoSo.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.PhongBanChuTri)
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);
        return entity;
    }
}
```

#### `BanGiaoHoSoGetDanhSachQuery.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;

namespace QLDA.Application.BanGiaoHoSos.Queries;

// Không implement IMayHaveGlobalFilter - không có search full-text
// UserId luôn lấy từ IUserProvider trong handler, không cho UI truyền
public record BanGiaoHoSoGetDanhSachQuery(BanGiaoHoSoSearchDto SearchDto) 
    : AggregateRootPagination, IRequest<PaginatedList<BanGiaoHoSoDto>> {
}

internal class BanGiaoHoSoGetDanhSachQueryHandler : IRequestHandler<BanGiaoHoSoGetDanhSachQuery, PaginatedList<BanGiaoHoSoDto>> {
    private readonly IRepository<BanGiaoHoSo, Guid> BanGiaoHoSo;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUserProvider _userProvider;

    public BanGiaoHoSoGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        BanGiaoHoSo = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    }

    public async Task<PaginatedList<BanGiaoHoSoDto>> Handle(BanGiaoHoSoGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        // Khai báo tường minh IQueryable để tránh lỗi type inference với IIncludableQueryable
        IQueryable<BanGiaoHoSo> queryable = BanGiaoHoSo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => e.UserId == _userProvider.Id)  // Luôn filter theo người dùng hiện tại (từ IUserProvider)
            .Include(e => e.User)
            .Include(e => e.PhongBanChuTri);

        // Filter theo TrangThai nếu được truyền (1 param duy nhất từ UI)
        if (request.SearchDto.TrangThai.HasValue) {
            queryable = queryable.Where(e => (int)e.TrangThai == request.SearchDto.TrangThai.Value);
        }

        return await queryable
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new BanGiaoHoSoDto {
                Id = e.Id,
                Ma = e.Ma,
                TenHoSo = e.TenHoSo,
                PhongBanChuTriId = e.PhongBanChuTriId,
                TenPhongBan = e.PhongBanChuTri!.Ten,
                TrangThai = (int)e.TrangThai,
                TenTrangThai = GetTrangThaiText(e.TrangThai),
                NgayBanGiao = e.NgayBanGiao,
                UserId = e.UserId,
                TenNguoiTao = e.User!.HoTen,
                // Tệp HS bàn giao (EGroupType.BanGiaoHoSo)
                DanhSachTepHSBanGiao = TepDinhKem.GetQueryableSet()
                    .Where(f => f.GroupId == e.Id.ToString() && f.EGroupType == EGroupType.BanGiaoHoSo && !f.IsDeleted)
                    .Select(f => f.ToDto()).ToList(),
                // Biên bản bàn giao (EGroupType.BienBanBanGiao)
                DanhSachBienBanBanGiao = TepDinhKem.GetQueryableSet()
                    .Where(f => f.GroupId == e.Id.ToString() && f.EGroupType == EGroupType.BienBanBanGiao && !f.IsDeleted)
                    .Select(f => f.ToDto()).ToList()
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }

    private static string GetTrangThaiText(ETrangThaiBanGiao trangThai) {
        return trangThai switch {
            ETrangThaiBanGiao.KhoiTao => "Khởi tạo",
            ETrangThaiBanGiao.DaBanGiao => "Đã bàn giao",
            _ => "Không xác định"
        };
    }
}
```

### Bước 8 – WebApi: BanGiaoHoSo Model + Mapping

> **IUserProvider** (đã đăng ký sẵn trong BuildingBlocks DI) được inject trực tiếp trong handler – **không cần tạo `IUserContext` hay sửa `WebApplicationExtensions.cs`**.

**File:** `QLDA.WebApi/Models/BanGiaoHoSos/BanGiaoHoSoModel.cs`

```csharp
using SequentialGuid;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.BanGiaoHoSos;

public class BanGiaoHoSoModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel {
    public Guid? Id { get; set; }
    
    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public int? PhongBanChuTriId { get; set; }
    public int TrangThai { get; set; }  // 0: Khởi tạo, 1: Đã bàn giao
    public DateTimeOffset? NgayBanGiao { get; set; }
    // Tệp HS bàn giao (EGroupType.BanGiaoHoSo) - từ interface IMayHaveTepDinhKemDto
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
    // Biên bản bàn giao (EGroupType.BienBanBanGiao) – chỉ đọc khi hiển thị
    public List<TepDinhKemModel>? DanhSachBienBanBanGiao { get; set; }
}
```

**File:** `QLDA.WebApi/Models/BanGiaoHoSos/BanGiaoHoSoBanGiaoModel.cs` *(tạo mới)*

```csharp
namespace QLDA.WebApi.Models.BanGiaoHoSos;

/// <summary>
/// Model cho endpoint PUT /ban-giao: nhận ngày bàn giao + biên bản bàn giao
/// </summary>
public class BanGiaoHoSoBanGiaoModel {
    /// <summary>Ngày bàn giao, nếu null sẽ dùng DateTimeOffset.Now</summary>
    public DateTimeOffset? NgayBanGiao { get; set; }
    // Biên bản bàn giao (đính kèm khi thực hiện bàn giao)
    public List<TepDinhKemModel>? DanhSachBienBan { get; set; }
}
```

**File:** `QLDA.WebApi/Models/BanGiaoHoSos/BanGiaoHoSoMappingConfiguration.cs`

```csharp
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.BanGiaoHoSos;

public static class BanGiaoHoSoMappingConfiguration {
    public static BanGiaoHoSoModel ToModel(this BanGiaoHoSo entity,
        List<TepDinhKem>? tepHSBanGiao = null,
        List<TepDinhKem>? bienBanBanGiao = null) => new() {
        Id = entity.Id,
        Ma = entity.Ma,
        TenHoSo = entity.TenHoSo,
        PhongBanChuTriId = entity.PhongBanChuTriId,
        TrangThai = (int)entity.TrangThai,
        NgayBanGiao = entity.NgayBanGiao,
        DanhSachTepDinhKem = tepHSBanGiao?.Select(f => f.ToModel()).ToList(),
        DanhSachBienBanBanGiao = bienBanBanGiao?.Select(f => f.ToModel()).ToList()
    };

    public static BanGiaoHoSo ToEntity(this BanGiaoHoSoModel model) => new() {
        Id = model.GetId(),
        Ma = model.Ma,
        TenHoSo = model.TenHoSo,
        PhongBanChuTriId = model.PhongBanChuTriId,
        TrangThai = (ETrangThaiBanGiao)(model.TrangThai)
    };

    public static void Update(this BanGiaoHoSo entity, BanGiaoHoSoModel model) {
        entity.Ma = model.Ma;
        entity.TenHoSo = model.TenHoSo;
        entity.PhongBanChuTriId = model.PhongBanChuTriId;
    }

    /// <summary>Tệp HS bàn giao (EGroupType.BanGiaoHoSo) – gắn khi insert/update</summary>
    public static List<TepDinhKem> GetDanhSachTepHSBanGiao(this BanGiaoHoSoModel model, Guid groupId) {
        if (model.DanhSachTepDinhKem?.Any() != true) return [];
        return model.DanhSachTepDinhKem
            .Select(f => new TepDinhKem {
                Id = f.Id ?? Guid.NewGuid(),
                ParentId = f.ParentId,
                GroupId = groupId.ToString(),
                GroupType = EGroupType.BanGiaoHoSo.ToString(),
                Type = f.Type,
                FileName = f.FileName,
                OriginalName = f.OriginalName,
                Path = f.Path,
                Size = f.Size
            })
            .ToList();
    }

    /// <summary>Biên bản bàn giao (EGroupType.BienBanBanGiao) – gắn khi thực hiện bàn giao</summary>
    public static List<TepDinhKem> GetDanhSachBienBanBanGiao(this BanGiaoHoSoBanGiaoModel model, Guid groupId) {
        if (model.DanhSachBienBan?.Any() != true) return [];
        return model.DanhSachBienBan
            .Select(f => new TepDinhKem {
                Id = f.Id ?? Guid.NewGuid(),
                ParentId = f.ParentId,
                GroupId = groupId.ToString(),
                GroupType = EGroupType.BienBanBanGiao.ToString(),
                Type = f.Type,
                FileName = f.FileName,
                OriginalName = f.OriginalName,
                Path = f.Path,
                Size = f.Size
            })
            .ToList();
    }
}
```

---

### Bước 9 – WebApi: BanGiaoHoSo Controller

**File:** `QLDA.WebApi/Controllers/BanGiaoHoSoController.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using QLDA.Application.BanGiaoHoSos.Commands;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Application.BanGiaoHoSos.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.WebApi.Models;
using QLDA.WebApi.Models.BanGiaoHoSos;
using QLDA.Domain.Entities;

namespace QLDA.WebApi.Controllers;

[ApiController]
[Route("api/ban-giao-ho-so")]
[Tags("Bàn giao hồ sơ")]
[Authorize]
public class BanGiaoHoSoController(IServiceProvider sp) : AggregateRootController(sp) {
    private readonly IMediator Mediator = sp.GetRequiredService<IMediator>();

    [HttpGet("{id}/chi-tiet")]
    [ProducesResponseType<ResultApi<BanGiaoHoSoModel>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new BanGiaoHoSoGetQuery { Id = id });
        // Tải cả 2 loại tệp đính kèm
        var allFiles = await Mediator.Send(new GetDanhSachTepDinhKemQuery { GroupId = [entity.Id.ToString()] });
        var tepHS = allFiles.Where(f => f.EGroupType == EGroupType.BanGiaoHoSo).ToList();
        var bienBan = allFiles.Where(f => f.EGroupType == EGroupType.BienBanBanGiao).ToList();
        return ResultApi.Ok(entity.ToModel(tepHS, bienBan));
    }

    [HttpGet("danh-sach")]
    [ProducesResponseType<ResultApi<PaginatedList<BanGiaoHoSoDto>>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> GetList([FromQuery] BanGiaoHoSoSearchDto searchDto, 
        [FromQuery] AggregateRootPagination pagination) {
        var res = await Mediator.Send(new BanGiaoHoSoGetDanhSachQuery(searchDto) {
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize
        });
        return ResultApi.Ok(res);
    }

    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Insert([FromBody] BanGiaoHoSoModel model) {
        var insertDto = new BanGiaoHoSoInsertDto {
            Ma = model.Ma,
            TenHoSo = model.TenHoSo,
            PhongBanChuTriId = model.PhongBanChuTriId,
            DanhSachTepDinhKem = model.DanhSachTepDinhKem  // Tệp HS bàn giao
        };
        
        var entity = await Mediator.Send(new BanGiaoHoSoInsertCommand(insertDto));
        // UserId được set từ IUserProvider.Id trong handler
        
        // Save tệp HS bàn giao (EGroupType.BanGiaoHoSo)
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = model.GetDanhSachTepHSBanGiao(entity.Id)
        });

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Update([FromBody] BanGiaoHoSoModel model) {
        var entity = await Mediator.Send(new BanGiaoHoSoGetQuery(model.GetId()));
        entity.Update(model);
        
        await Mediator.Send(new BanGiaoHoSoUpdateCommand(new BanGiaoHoSoUpdateModel {
            Id = entity.Id,
            Ma = entity.Ma,
            TenHoSo = entity.TenHoSo,
            PhongBanChuTriId = entity.PhongBanChuTriId,
            DanhSachTepDinhKem = model.DanhSachTepDinhKem  // Tệp HS bàn giao
        }));

        // Update tệp HS bàn giao (EGroupType.BanGiaoHoSo)
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = model.GetDanhSachTepHSBanGiao(entity.Id)
        });

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("{id}/ban-giao")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> BanGiao(Guid id, [FromBody] BanGiaoHoSoBanGiaoModel model) {
        var ngayBanGiao = model.NgayBanGiao ?? DateTimeOffset.Now;
        var bienBanEntities = model.GetDanhSachBienBanBanGiao(id);

        // Thực hiện bàn giao: đổi TrangThai 0→1, set NgayBanGiao
        var entity = await Mediator.Send(new BanGiaoHoSoBanGiaoCommand(id, ngayBanGiao));

        // Lưu biên bản bàn giao (EGroupType.BienBanBanGiao)
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = bienBanEntities
        });

        return ResultApi.Ok(1);
    }

    [HttpDelete("{id}/xoa-tam")]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> SoftDelete(Guid id) {
        await Mediator.Send(new BanGiaoHoSoDeleteCommand(id));
        return ResultApi.Ok(1);
    }
}
```

---

## 5. Checklist hoàn thành

```
[x] 1. Tạo ETrangThaiBanGiao enum
[x] 2. Tạo BanGiaoHoSo entity (+ NgayBanGiao)
[x] 3. Tạo BanGiaoHoSoConfiguration (EF)
[ ] 4. Chạy Migration (không drop database)
[x] 5. Tạo BanGiaoHoSo DTOs + Mappings
[x] 6. Tạo BanGiaoHoSoInsertCommand / UpdateCommand / BanGiaoCommand / DeleteCommand
[x] 7. Tạo BanGiaoHoSoGetQuery / GetDanhSachQuery
[x] 8. IUserProvider đã đăng ký sẵn trong BuildingBlocks DI - inject trong handler
[ ] 9. WebApplicationExtensions.cs - KHÔNG cần sửa (IUserProvider đã có sẵn)
[x] 10. Tạo BanGiaoHoSo Model + BanGiaoHoSoBanGiaoModel + Mapping + Controller
[x] 11. Thêm EGroupType.BanGiaoHoSo và EGroupType.BienBanBanGiao vào enum
[ ] 12. Build + Test
```

---

## 6. Lưu ý kỹ thuật

- **⚠️ KHÔNG chạy `drop-database`** – chỉ chạy `migrations add` và `database update`
- **UserId từ Auth:** Lấy từ `IUserProvider.Id` (BuildingBlocks) inject trong handler – không cho user tự chỉ định
- **Delete có điều kiện:** Chỉ xóa được khi `TrangThai = 0` (Khởi tạo), throw exception nếu vi phạm
- **Ban-giao endpoint:** Đổi trạng thái 0→1, set `NgayBanGiao`, lưu biên bản bàn giao
- **Filter:** Chỉ theo `TrangThai` (1 param duy nhất từ UI) - không có GlobalFilter
- **Migration tên:** `AddBanGiaoHoSo` – chạy từ folder `QLDA.Migrator`
- **Index:** Tạo index trên `(UserId, TrangThai)` để tối ưu query danh sách
- **2 loại tệp:**
  - `EGroupType.BanGiaoHoSo` – tệp HS bàn giao (gắn khi insert/update)
  - `EGroupType.BienBanBanGiao` – biên bản bàn giao (gắn khi gọi endpoint ban-giao)
  - Cả 2 dùng chung `GroupId = BanGiaoHoSo.Id.ToString()`

---

## 7. 📋 TÓM TẮT CÔNG VIỆC

### 🎯 Tổng quan

**Bàn Giao Hồ Sơ CRUD** gồm:
- ✅ **1 Entity** chính (`BanGiaoHoSo`)
- ✅ **1 Enum** mới (`ETrangThaiBanGiao`)
- ✅ **~16 Files** cần tạo/sửa
- ✅ **6 API endpoints** mới
- ✅ Support **file attachment** 2 loại qua `TepDinhKem`: **tệp HS** (`BanGiaoHoSo`) + **biên bản** (`BienBanBanGiao`)

---

### 📁 Tóm tắt files

**Domain Layer (2 files)**
- `ETrangThaiBanGiao.cs` (enum mới)
- `BanGiaoHoSo.cs` (entity mới)

**Persistence Layer (1 file)**
- `BanGiaoHoSoConfiguration.cs`

**Application Layer (8 files)**
- DTOs: Insert, Update, BanGiao, Search, Display + Mappings
- Commands: Insert, Update, BanGiao, Delete
- Queries: Get, GetList

**WebApi Layer (4 files – 4 mới)**
- `BanGiaoHoSoModel.cs` + `BanGiaoHoSoBanGiaoModel.cs` + `BanGiaoHoSoMappingConfiguration.cs`
- `BanGiaoHoSoController.cs` (6 endpoints)
- `WebApplicationExtensions.cs` – **không sửa** (IUserProvider đã đăng ký sẵn)

---

### 🔌 API Endpoints (6 mới)

| Method | Route | Mô tả | Response |
|--------|-------|-------|----------|
| `GET` | `/api/ban-giao-ho-so/{id}/chi-tiet` | Chi tiết 1 bản giao (cả 2 loại tệp) | `BanGiaoHoSoModel` |
| `GET` | `/api/ban-giao-ho-so/danh-sach` | Danh sách phân trang (filter theo user) | `PaginatedList<BanGiaoHoSoDto>` |
| `POST` | `/api/ban-giao-ho-so/them-moi` | Thêm mới bản giao + tệp HS | `Guid` (Id) |
| `PUT` | `/api/ban-giao-ho-so/cap-nhat` | Cập nhật bản giao + tệp HS | `Guid` (Id) |
| `PUT` | `/api/ban-giao-ho-so/{id}/ban-giao` | Thực hiện bàn giao: đổi trạng thái 0→1, set ngày, đính kèm biên bản | `int` (1) |
| `DELETE` | `/api/ban-giao-ho-so/{id}/xoa-tam` | Xóa (chỉ khi TrangThai = 0) | `int` (1) |

---

### 🔒 Permission Logic

| Thao tác | Điều kiện | UserId |
|---------|-----------|--------|
| View list | Danh sách của chính mình | `userInfo.UserId` |
| Create | Tạo bản giao mới | Auto set từ Auth |
| Update | Sửa thông tin + tệp HS | Chỉ khi TrangThai = 0 |
| Ban-giao | 0→1, set ngày, đính kèm biên bản | Free |
| Delete | Xóa bản giao | Chỉ khi TrangThai = 0 |
| File HS ops | Tệp HS bàn giao | Gắn lúc insert/update |
| Biên bản ops | Biên bản bàn giao | Gắn lúc ban-giao |

---

### 📎 TepDinhKem Pattern (Theo Task 9488) – 2 Loại Tệp

**Bộ hồ sơ lưu trữ bàn giao gồm 2 loại tệp, cùng `GroupId` nhưng khác `EGroupType`:**

| Loại | EGroupType | Gắn khi | Extension method |
|------|------------|---------|-----------------|
| Tệp HS bàn giao | `BanGiaoHoSo` | Insert / Update | `GetDanhSachTepHSBanGiao()` |
| Biên bản bàn giao | `BienBanBanGiao` | Endpoint `ban-giao` | `GetDanhSachBienBanBanGiao()` |

1. **EGroupType enum** - Thêm 2 entries:
```csharp
public enum EGroupType {
    // ... existing entries ...
    BanGiaoHoSo,       // ← Tệp HS bàn giao
    BienBanBanGiao,    // ← Biên bản bàn giao
}
```

2. **Lưu tệp HS bàn giao** (Insert/Update):
```csharp
await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
    GroupId = entity.Id.ToString(),
    Entities = model.GetDanhSachTepHSBanGiao(entity.Id)
    // → files[i].EGroupType = EGroupType.BanGiaoHoSo
});
```

3. **Lưu biên bản bàn giao** (endpoint ban-giao):
```csharp
await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
    GroupId = entity.Id.ToString(),
    Entities = model.GetDanhSachBienBanBanGiao(entity.Id)
    // → files[i].EGroupType = EGroupType.BienBanBanGiao
});
```

4. **Lấy file** - Query tách theo EGroupType trong GetDanhSachQuery:
```csharp
DanhSachTepHSBanGiao = TepDinhKem.GetQueryableSet()
    .Where(f => f.GroupId == e.Id.ToString() && f.EGroupType == EGroupType.BanGiaoHoSo && !f.IsDeleted)
    .Select(f => f.ToDto()).ToList(),
DanhSachBienBanBanGiao = TepDinhKem.GetQueryableSet()
    .Where(f => f.GroupId == e.Id.ToString() && f.EGroupType == EGroupType.BienBanBanGiao && !f.IsDeleted)
    .Select(f => f.ToDto()).ToList()
```

---

**⚩️ Dependency Injection**

`IUserProvider` đã được đăng ký sẵn trong BuildingBlocks DI – **không cần sửa `WebApplicationExtensions.cs`**.

```csharp
// Inject trong handler constructor:
_userProvider = serviceProvider.GetRequiredService<IUserProvider>();

// Sử dụng:
entity.UserId = _userProvider.Id;  // long – lấy từ JWT claim "UserId"
```
