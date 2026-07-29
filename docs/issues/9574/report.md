# Summary – HoSoMoiThauDienTu (iss #9574)

## Tổng quan

Bổ sung entity `HoSoMoiThauDienTu` (E-HSMT) với CRUD đầy đủ và đính kèm tệp.

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm

| # | Mục | Ghi chú |
|---|-----|---------|
| 1 | Bổ sung **entity** `HoSoMoiThauDienTu` mới | Hiện chưa có bảng trong DB |
| 2 | Bổ sung **5 API**: get, list, insert, update, delete (soft) | Theo luồng CQRS hiện tại |
| 3 | Hỗ trợ **đính kèm file** (TepDinhKem) | Gửi trong body, không có endpoint riêng |

### 1.2 Model HoSoMoiThauDienTu

| Field | Kiểu | Ghi chú |
|-------|------|---------|
| `Id` | `Guid` | Auto-generate sequential guid |
| `DuAnId` | `Guid` | FK → DuAn |
| `BuocId` | `int` | FK → DuAnBuoc — PK của `DuAnBuoc` là `int` (`Entity<int>`), không phải Guid |
| `HinhThucLuaChonNhaThauId` | `int` | FK → DanhMucHinhThucLuaChonNhaThau |
| `GoiThauId` | `Guid` | FK → GoiThau |
| `GiaTri` | `long` | Giá trị hồ sơ |
| `ThoiGianThucHien` | `string (nvarchar(200))` | Thời gian thực hiện |
| `TrangThaiDangTai` | `bool` | Trạng thái đang tải (bit) |
| `TrangThaiId` | `int?` | FK → DanhMucTrangThaiPheDuyet (Dự thảo/Trình duyệt/Phê duyệt/...) |
| Files (TepDinhKem) | - | Đính kèm files cho hồ sơ |

### 1.3 Danh mục HinhThucLuaChonNhaThau (DanhMucHinhThucLuaChonNhaThau)

**ĐÃ TỒN TẠI** - Entity `DanhMucHinhThucLuaChonNhaThau` đã được tạo sẵn trong domain.

Tham khảo pattern danh mục chuẩn trong dự án:

```json
{
  "id": 0,
  "ma": null,
  "ten": null,
  "moTa": null,
  "stt": null,
  "used": true
}
```

**Kế thừa:** `DanhMuc<int>`, implement `IAggregateRoot, IMayHaveStt`

---

## 2. Phân tích hiện trạng

### 2.1 HoSoMoiThauDienTu là entity nghiệp vụ (không phải DanhMuc)

- Có **FK tới nhiều entities** (`DuAn`, `DuAnBuoc`, `GoiThau`, `DmHinhThucLuaChonNhaThau`)
- Có logic riêng → phải viết **Commands/Queries đầy đủ** như `DuAn`, `HopDong`, `GoiThau`
- Hỗ trợ **đính kèm file** → integrate với `TepDinhKem` (group type: `HoSoMoiThauDienTu`)

### 2.2 Pattern danh mục chuẩn trong dự án

Danh mục loại đơn giản (int PK, có ma/ten/moTa/stt/used):
- Kế thừa `DanhMuc<int>`, implement `IAggregateRoot, IMayHaveStt`
- CRUD đi qua `DanhMucGetQuery` + `DanhMucInsertOrUpdateCommand` (shared command)
- **Không cần viết Commands/Queries riêng** – chỉ cần đăng ký vào `EDanhMuc`

### 2.3 DanhMucHinhThucLuaChonNhaThau

**ĐÃ TỒN TẠI HOÀN TOÀN** - Tất cả đều đã có, không cần làm gì:
- ✅ Entity `DanhMucHinhThucLuaChonNhaThau` – `QLDA.Domain/Entities/DanhMuc/`
- ✅ `DanhMucHinhThucLuaChonNhaThauConfiguration` – `QLDA.Persistence/Configurations/DanhMuc/`
- ✅ `EDanhMuc.DanhMucHinhThucLuaChonNhaThau` – đã có trong `EDanhMuc.cs`
- ✅ `DanhMucMappings` case – đã có trong cả 2 method `ToEntity`
- ✅ `DanhMucHinhThucLuaChonNhaThauController` – đã có

**Không cần cập nhật bất kỳ file nào của DanhMucHinhThucLuaChonNhaThau.** FK từ `HoSoMoiThauDienTu` chỉ cần khai báo một chiều trong `HoSoMoiThauDienTuConfiguration` với `.WithMany()` (không cần nav property ngược).

---

## 3. Thứ tự thực hiện

```
Bước 1: Domain - Entity HoSoMoiThauDienTu
Bước 2: Persistence - Configuration HoSoMoiThauDienTu
Bước 3: Domain - Thêm EGroupType.HoSoMoiThauDienTu
Bước 4: Persistence - Migration
Bước 5: Application - HoSoMoiThauDienTu DTOs + Commands + Queries
Bước 6: WebApi - HoSoMoiThauDienTu Model + MappingConfiguration
Bước 7: WebApi - HoSoMoiThauDienTu Controller
```

> DanhMucHinhThucLuaChonNhaThau (entity, config, controller, EDanhMuc, DanhMucMappings) đã tồn tại đầy đủ - không cần làm gì.

---

## 4. Chi tiết từng bước

---

### Bước 1 – Domain: Entity `HoSoMoiThauDienTu`

**File:** `QLDA.Domain/Entities/HoSoMoiThauDienTu.cs`

```csharp
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng quản lý hồ sơ mời thầu điện tử
/// </summary>
public class HoSoMoiThauDienTu : Entity<Guid>, IAggregateRoot {
    /// <summary>
    /// FK → DuAn
    /// </summary>
    public Guid? DuAnId { get; set; }

    /// <summary>
    /// FK → DuAnBuoc
    /// </summary>
    public int? BuocId { get; set; }

    /// <summary>
    /// FK → DmHinhThucLuaChonNhaThau
    /// </summary>
    public int? HinhThucLuaChonNhaThauId { get; set; }

    /// <summary>
    /// FK → GoiThau
    /// </summary>
    public Guid? GoiThauId { get; set; }

    /// <summary>
    /// Giá trị hồ sơ
    /// </summary>
    public long? GiaTri { get; set; }

    /// <summary>
    /// Thời gian thực hiện
    /// </summary>
    public string? ThoiGianThucHien { get; set; }

    /// <summary>
    /// Trạng thái đang tải (bit)
    /// </summary>
    public bool TrangThaiDangTai { get; set; }

    /// <summary>
    /// FK → Trạng thái phê duyệt (DanhMucTrangThaiPheDuyet table)
    /// </summary>
    public int? TrangThaiId { get; set; }

    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DuAnBuoc? Buoc { get; set; }
    public DanhMucHinhThucLuaChonNhaThau? HinhThucLuaChonNhaThau { get; set; }
    public GoiThau? GoiThau { get; set; }
     public DanhMucTrangThaiPheDuyet? TrangThaiPheDuyet { get; set; }
    #endregion
}
```

> **Lưu ý:** `Entity<Guid>` là base class cho các aggregate root có GUID key.

---

### Bước 2 – Persistence: Configuration `HoSoMoiThauDienTu`

**File:** `QLDA.Persistence/Configurations/HoSoMoiThauDienTuConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class HoSoMoiThauDienTuConfiguration : AggregateRootConfiguration<HoSoMoiThauDienTu> {
    public override void Configure(EntityTypeBuilder<HoSoMoiThauDienTu> builder) {
        builder.ToTable("HoSoMoiThauDienTu");
        builder.ConfigureForBase();  // Id, IsDeleted, CreatedAt, ...

        builder.Property(e => e.ThoiGianThucHien).HasMaxLength(200);

        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Buoc)
            .WithMany()
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.HinhThucLuaChonNhaThau)
            .WithMany()
            .HasForeignKey(e => e.HinhThucLuaChonNhaThauId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.GoiThau)
            .WithMany()
            .HasForeignKey(e => e.GoiThauId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.TrangThaiPheDuyet)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

> **Lưu ý:** Không có FK config cho TepDinhKem. File được liên kết qua `GroupId`/`GroupType` string-based (xem Bước 3).

---

### Bước 3 – Domain: Thêm `EGroupType.HoSoMoiThauDienTu`

**File:** `QLDA.Domain/Enums/EGroupType.cs`

Thêm giá trị mới vào enum:

```csharp
HoSoMoiThauDienTu,
```



### Bước 4 – Persistence: Migration

```bash
# Chạy trong thư mục QLDA.Migrator
dotnet ef migrations add AddHoSoMoiThauDienTu
dotnet ef database update
```

**⚠️ IMPORTANT:** Không chạy `drop-database`. Chỉ chạy `migrations add` và `database update`.

---

### Bước 5 – Application: HoSoMoiThauDienTu DTOs, Commands, Queries

**Cấu trúc folder:**

```
QLDA.Application/HoSoMoiThauDienTus/
  DTOs/
    HoSoMoiThauDienTuDto.cs
    HoSoMoiThauDienTuInsertDto.cs
    HoSoMoiThauDienTuUpdateModel.cs
    HoSoMoiThauDienTuSearchDto.cs
    HoSoMoiThauDienTuMappings.cs
  Commands/
    HoSoMoiThauDienTuInsertCommand.cs
    HoSoMoiThauDienTuUpdateCommand.cs
    HoSoMoiThauDienTuDeleteCommand.cs
  Queries/
    HoSoMoiThauDienTuGetDanhSachQuery.cs
    HoSoMoiThauDienTuGetQuery.cs
```

#### `HoSoMoiThauDienTuInsertDto.cs`

```csharp
using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public class HoSoMoiThauDienTuInsertDto : IMayHaveTepDinhKemDto {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? HinhThucLuaChonNhaThauId { get; set; }
    public Guid? GoiThauId { get; set; }
    public long? GiaTri { get; set; }
    public string? ThoiGianThucHien { get; set; }
    public bool TrangThaiDangTai { get; set; }
    public int? TrangThaiId { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
```

#### `HoSoMoiThauDienTuUpdateModel.cs`

```csharp
namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public class HoSoMoiThauDienTuUpdateModel : HoSoMoiThauDienTuInsertDto {
    public Guid Id { get; set; }
}
```

#### `HoSoMoiThauDienTuDto.cs`

```csharp
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public class HoSoMoiThauDienTuDto {
    public Guid Id { get; set; }
    public Guid? DuAnId { get; set; }
    public string? TenDuAn { get; set; }
    public int? BuocId { get; set; }
    public string? TenBuoc { get; set; }
    public int? HinhThucLuaChonNhaThauId { get; set; }
    public string? TenHinhThucLuaChonNhaThau { get; set; }
    public Guid? GoiThauId { get; set; }
    public string? TenGoiThau { get; set; }
    public long? GiaTri { get; set; }
    public string? ThoiGianThucHien { get; set; }
    public bool TrangThaiDangTai { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
```

#### `HoSoMoiThauDienTuSearchDto.cs`

```csharp
namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public class HoSoMoiThauDienTuSearchDto {
    public string? GlobalFilter { get; set; }
    public Guid? DuAnId { get; set; }
    public Guid? GoiThauId { get; set; }
}
```

#### `HoSoMoiThauDienTuMappings.cs`

```csharp
namespace QLDA.Application.HoSoMoiThauDienTus.DTOs;

public static class HoSoMoiThauDienTuMappings {
    public static HoSoMoiThauDienTu ToEntity(this HoSoMoiThauDienTuInsertDto dto) => new() {
        Id = GuidExtensions.GetSequentialGuidId(),
        DuAnId = dto.DuAnId,
        BuocId = dto.BuocId,
        HinhThucLuaChonNhaThauId = dto.HinhThucLuaChonNhaThauId,
        GoiThauId = dto.GoiThauId,
        GiaTri = dto.GiaTri,
        ThoiGianThucHien = dto.ThoiGianThucHien,
        TrangThaiDangTai = dto.TrangThaiDangTai,
        TrangThaiId = dto.TrangThaiId,
    };

    public static void Update(this HoSoMoiThauDienTu entity, HoSoMoiThauDienTuUpdateModel dto) {
        entity.DuAnId = dto.DuAnId;
        entity.BuocId = dto.BuocId;
        entity.HinhThucLuaChonNhaThauId = dto.HinhThucLuaChonNhaThauId;
        entity.GoiThauId = dto.GoiThauId;
        entity.GiaTri = dto.GiaTri;
        entity.ThoiGianThucHien = dto.ThoiGianThucHien;
        entity.TrangThaiDangTai = dto.TrangThaiDangTai;
        entity.TrangThaiId = dto.TrangThaiId;
    }

    public static HoSoMoiThauDienTuDto ToDto(this HoSoMoiThauDienTu entity,
        List<TepDinhKem>? files = null) => new() {
        Id = entity.Id,
        DuAnId = entity.DuAnId,
        TenDuAn = entity.DuAn?.TenDuAn,
        BuocId = entity.BuocId,
        TenBuoc = entity.Buoc?.TenBuoc,
        HinhThucLuaChonNhaThauId = entity.HinhThucLuaChonNhaThauId,
        TenHinhThucLuaChonNhaThau = entity.HinhThucLuaChonNhaThau?.Ten,
        GoiThauId = entity.GoiThauId,
        TenGoiThau = entity.GoiThau?.TenGoiThau,
        GiaTri = entity.GiaTri,
        ThoiGianThucHien = entity.ThoiGianThucHien,
        TrangThaiDangTai = entity.TrangThaiDangTai,
        TrangThaiId = entity.TrangThaiId,
        TenTrangThai = entity.TrangThaiPheDuyet?.Ten,
        DanhSachTepDinhKem = files?.Select(f => new TepDinhKemDto {
            Id = f.Id,
            ParentId = f.ParentId,
            GroupId = f.GroupId,
            GroupType = f.GroupType,
            FileName = f.FileName,
            OriginalName = f.OriginalName,
            Path = f.Path,
            Size = f.Size,
            Type = f.Type,
        }).ToList(),
    };
}
```

#### `HoSoMoiThauDienTuInsertCommand.cs`

```csharp
using System.Data;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

public record HoSoMoiThauDienTuInsertCommand(HoSoMoiThauDienTuInsertDto Dto) : IRequest<HoSoMoiThauDienTu>;

internal class HoSoMoiThauDienTuInsertCommandHandler : IRequestHandler<HoSoMoiThauDienTuInsertCommand, HoSoMoiThauDienTu> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuInsertCommandHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _unitOfWork = HoSoMoiThauDienTu.UnitOfWork;
    }

    public async Task<HoSoMoiThauDienTu> Handle(HoSoMoiThauDienTuInsertCommand request, CancellationToken cancellationToken = default) {
        var entity = request.Dto.ToEntity();

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await HoSoMoiThauDienTu.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
```

#### `HoSoMoiThauDienTuUpdateCommand.cs`

```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

public record HoSoMoiThauDienTuUpdateCommand(HoSoMoiThauDienTuUpdateModel Model) : IRequest<HoSoMoiThauDienTu>;

internal class HoSoMoiThauDienTuUpdateCommandHandler : IRequestHandler<HoSoMoiThauDienTuUpdateCommand, HoSoMoiThauDienTu> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuUpdateCommandHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _unitOfWork = HoSoMoiThauDienTu.UnitOfWork;
    }

    public async Task<HoSoMoiThauDienTu> Handle(HoSoMoiThauDienTuUpdateCommand request, CancellationToken cancellationToken = default) {
        var entity = await HoSoMoiThauDienTu.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.Update(request.Model);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await HoSoMoiThauDienTu.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
```

#### `HoSoMoiThauDienTuDeleteCommand.cs`

```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

public record HoSoMoiThauDienTuDeleteCommand(Guid Id) : IRequest;

internal class HoSoMoiThauDienTuDeleteCommandHandler : IRequestHandler<HoSoMoiThauDienTuDeleteCommand> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuDeleteCommandHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _unitOfWork = HoSoMoiThauDienTu.UnitOfWork;
    }

    public async Task Handle(HoSoMoiThauDienTuDeleteCommand request, CancellationToken cancellationToken = default) {
        var entity = await HoSoMoiThauDienTu.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await HoSoMoiThauDienTu.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}
```

#### `HoSoMoiThauDienTuGetQuery.cs`

```csharp
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.HoSoMoiThauDienTus.Queries;

public record HoSoMoiThauDienTuGetQuery : IRequest<HoSoMoiThauDienTu> {
    public Guid Id { get; set; }
}

internal class HoSoMoiThauDienTuGetQueryHandler : IRequestHandler<HoSoMoiThauDienTuGetQuery, HoSoMoiThauDienTu> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;

    public HoSoMoiThauDienTuGetQueryHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
    }

    public async Task<HoSoMoiThauDienTu> Handle(HoSoMoiThauDienTuGetQuery request, CancellationToken cancellationToken = default) {
        var entity = await HoSoMoiThauDienTu.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.DuAn)
            .Include(e => e.Buoc)
            .Include(e => e.HinhThucLuaChonNhaThau)
            .Include(e => e.GoiThau)
            .Include(e => e.TrangThaiPheDuyet)
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);
        return entity;
    }
}
```

#### `HoSoMoiThauDienTuGetDanhSachQuery.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;

namespace QLDA.Application.HoSoMoiThauDienTus.Queries;

public record HoSoMoiThauDienTuGetDanhSachQuery(HoSoMoiThauDienTuSearchDto SearchDto) 
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<HoSoMoiThauDienTuDto>> {
    public string? GlobalFilter { get; set; }
}

internal class HoSoMoiThauDienTuGetDanhSachQueryHandler : IRequestHandler<HoSoMoiThauDienTuGetDanhSachQuery, PaginatedList<HoSoMoiThauDienTuDto>> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;

    public HoSoMoiThauDienTuGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
    }

    public async Task<PaginatedList<HoSoMoiThauDienTuDto>> Handle(HoSoMoiThauDienTuGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = HoSoMoiThauDienTu.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Include(e => e.DuAn)
            .Include(e => e.Buoc)
            .Include(e => e.HinhThucLuaChonNhaThau)
            .Include(e => e.GoiThau)
            .Include(e => e.TrangThaiPheDuyet)
            .WhereGlobalFilter(
                request,  // Truyền request (implement IMayHaveGlobalFilter)
                e => e.ThoiGianThucHien
            );

        // Filter by DuAnId if provided
        if (request.SearchDto.DuAnId.HasValue) {
            queryable = queryable.Where(e => e.DuAnId == request.SearchDto.DuAnId);
        }

        // Filter by GoiThauId if provided
        if (request.SearchDto.GoiThauId.HasValue) {
            queryable = queryable.Where(e => e.GoiThauId == request.SearchDto.GoiThauId);
        }

        return await queryable
            .Select(e => e.ToDto())
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}
```

---

### Bước 6 – WebApi: HoSoMoiThauDienTu Model + MappingConfiguration

**File:** `QLDA.WebApi/Models/HoSoMoiThauDienTus/HoSoMoiThauDienTuModel.cs`

```csharp
using SequentialGuid;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.HoSoMoiThauDienTus;

public class HoSoMoiThauDienTuModel : IHasKey<Guid?>, IMustHaveId<Guid>,
    IMayHaveTepDinhKemModel {

    public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? HinhThucLuaChonNhaThauId { get; set; }
    public Guid? GoiThauId { get; set; }
    public long? GiaTri { get; set; }
    public string? ThoiGianThucHien { get; set; }
    public bool TrangThaiDangTai { get; set; }
    public int? TrangThaiId { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}
```

**File:** `QLDA.WebApi/Models/HoSoMoiThauDienTus/HoSoMoiThauDienTuMappingConfiguration.cs`

```csharp
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Enums;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.HoSoMoiThauDienTus;

public static class HoSoMoiThauDienTuMappingConfiguration {

    public static HoSoMoiThauDienTuModel ToModel(
        this HoSoMoiThauDienTu entity,
        List<TepDinhKem>? files = null) => new() {
        Id = entity.Id,
        DuAnId = entity.DuAnId,
        BuocId = entity.BuocId,
        HinhThucLuaChonNhaThauId = entity.HinhThucLuaChonNhaThauId,
        GoiThauId = entity.GoiThauId,
        GiaTri = entity.GiaTri,
        ThoiGianThucHien = entity.ThoiGianThucHien,
        TrangThaiDangTai = entity.TrangThaiDangTai,
        TrangThaiId = entity.TrangThaiId,
        DanhSachTepDinhKem = files?.Select(f => new TepDinhKemModel {
            Id = f.Id,
            ParentId = f.ParentId,
            GroupId = f.GroupId,
            GroupType = f.GroupType,
            FileName = f.FileName,
            OriginalName = f.OriginalName,
            Path = f.Path,
            Size = f.Size,
            Type = f.Type,
        }).ToList()
    };

    public static HoSoMoiThauDienTuInsertDto ToInsertDto(this HoSoMoiThauDienTuModel model) => new() {
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        HinhThucLuaChonNhaThauId = model.HinhThucLuaChonNhaThauId,
        GoiThauId = model.GoiThauId,
        GiaTri = model.GiaTri,
        ThoiGianThucHien = model.ThoiGianThucHien,
        TrangThaiDangTai = model.TrangThaiDangTai,
        TrangThaiId = model.TrangThaiId,
        DanhSachTepDinhKem = model.DanhSachTepDinhKem?.Select(m => new TepDinhKemDto {
            Id = m.Id,
            ParentId = m.ParentId,
            GroupId = m.GroupId,
            GroupType = m.GroupType,
            FileName = m.FileName,
            OriginalName = m.OriginalName,
            Path = m.Path,
            Size = m.Size,
            Type = m.Type,
        }).ToList()
    };

    public static HoSoMoiThauDienTuUpdateModel ToUpdateModel(this HoSoMoiThauDienTuModel model) => new() {
        Id = model.GetId(),
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        HinhThucLuaChonNhaThauId = model.HinhThucLuaChonNhaThauId,
        GoiThauId = model.GoiThauId,
        GiaTri = model.GiaTri,
        ThoiGianThucHien = model.ThoiGianThucHien,
        TrangThaiDangTai = model.TrangThaiDangTai,
        TrangThaiId = model.TrangThaiId,
    };

    public static List<TepDinhKem> GetDanhSachTepDinhKem(
        this HoSoMoiThauDienTuModel model, Guid groupId)
        => model.DanhSachTepDinhKem?
            .Select(m => new TepDinhKem {
                Id = m.Id ?? Guid.NewGuid(),
                ParentId = m.ParentId,
                GroupId = groupId.ToString(),
                GroupType = EGroupType.HoSoMoiThauDienTu.ToString(),
                Type = m.Type,
                FileName = m.FileName,
                OriginalName = m.OriginalName,
                Path = m.Path,
                Size = m.Size
            }).ToList() ?? [];
}
```

---

### Bước 7 – WebApi: HoSoMoiThauDienTu Controller

**File:** `QLDA.WebApi/Controllers/HoSoMoiThauDienTuController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using QLDA.Application.HoSoMoiThauDienTus.Commands;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.HoSoMoiThauDienTus.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.WebApi.Models.HoSoMoiThauDienTus;

namespace QLDA.WebApi.Controllers;

[Tags("Hồ sơ mời thầu điện tử")]
[Route("api/ho-so-moi-thau-dien-tu")]
public class HoSoMoiThauDienTuController(IServiceProvider sp) : AggregateRootController(sp) {

    [HttpGet("{id}")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new HoSoMoiThauDienTuGetQuery { Id = id });
        var files = await Mediator.Send(new GetDanhSachTepDinhKemQuery {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToModel(files));
    }

    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetAll([FromQuery] HoSoMoiThauDienTuSearchDto dto, string? globalFilter) {
        dto.GlobalFilter = globalFilter;
        var result = await Mediator.Send(new HoSoMoiThauDienTuGetDanhSachQuery(dto));
        return ResultApi.Ok(result);
    }

    [HttpPost("them-moi")]
    public async Task<ResultApi> Create([FromBody] HoSoMoiThauDienTuModel model) {
        var entity = await Mediator.Send(new HoSoMoiThauDienTuInsertCommand(model.ToInsertDto()));

        if (model.DanhSachTepDinhKem?.Count > 0) {
            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
                GroupId = entity.Id.ToString(),
                Entities = model.GetDanhSachTepDinhKem(entity.Id)
            });
        }

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    public async Task<ResultApi> Update([FromBody] HoSoMoiThauDienTuModel model) {
        var entity = await Mediator.Send(new HoSoMoiThauDienTuUpdateCommand(model.ToUpdateModel()));

        if (model.DanhSachTepDinhKem?.Count > 0) {
            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
                GroupId = entity.Id.ToString(),
                Entities = model.GetDanhSachTepDinhKem(entity.Id)
            });
        }

        return ResultApi.Ok(entity.Id);
    }

    [HttpDelete("{id}")]
    public async Task<ResultApi> Delete(Guid id) {
        await Mediator.Send(new HoSoMoiThauDienTuDeleteCommand(id));
        return ResultApi.Ok(1);
    }
}
```

> **Lưu ý:** Controller nhận `HoSoMoiThauDienTuModel` (single model cho cả insert và update), không có endpoint upload file riêng. File được gửi trong `DanhSachTepDinhKem` cùng body, lưu qua `TepDinhKemBulkInsertOrUpdateCommand` với `EGroupType.HoSoMoiThauDienTu`.

---

## 5. Checklist hoàn thành

```
[x] DanhMucHinhThucLuaChonNhaThau entity            ← ĐÃ CÓ
[x] DanhMucHinhThucLuaChonNhaThauConfiguration      ← ĐÃ CÓ
[x] EDanhMuc.DanhMucHinhThucLuaChonNhaThau          ← ĐÃ CÓ
[x] DanhMucMappings cases                           ← ĐÃ CÓ
[x] DanhMucHinhThucLuaChonNhaThauController         ← ĐÃ CÓ

[x] 1. Tạo HoSoMoiThauDienTu entity (không có nav TepDinhKems)
[x] 2. Tạo HoSoMoiThauDienTuConfiguration (không có FK TepDinhKem)
[x] 3. Thêm EGroupType.HoSoMoiThauDienTu vào EGroupType.cs
[x] 4. Chạy Migration (không drop database)
[x] 5. Tạo HoSoMoiThauDienTu DTOs (InsertDto: IMayHaveTepDinhKemDto) + Mappings
[x] 6. Tạo HoSoMoiThauDienTuInsertCommand / UpdateCommand / DeleteCommand
[x] 7. Tạo HoSoMoiThauDienTuGetQuery / GetDanhSachQuery
[x] 8. Tạo HoSoMoiThauDienTuModel + HoSoMoiThauDienTuMappingConfiguration (WebApi)
[x] 9. Tạo HoSoMoiThauDienTuController
[x] 10. Build + Test
```

---

## 6. Lưu ý kỹ thuật

- **⚠️ KHÔNG chạy `drop-database`** – chỉ chạy `migrations add` và `database update`
- **Attachments:** File được liên kết qua `GroupId = entity.Id.ToString()` + `GroupType = EGroupType.HoSoMoiThauDienTu.ToString()`. **Không có FK từ entity sang TepDinhKem**, không có nav property, không có service riêng.
- **File gửi trong body:** `DanhSachTepDinhKem` là list trong `HoSoMoiThauDienTuModel`, không có endpoint upload riêng
- **Get chi tiết:** Sau khi lấy entity, gọi thêm `GetDanhSachTepDinhKemQuery { GroupId = [entity.Id.ToString()] }` rồi trả về `entity.ToModel(files)`
- **Global Filter:** Tìm kiếm trên `ThoiGianThucHien` theo default, có thể mở rộng
- **TrangThaiId:** FK → `DanhMucTrangThaiPheDuyet`
- **Migration:** Tên suggestion: `AddHoSoMoiThauDienTu` – chạy từ folder `QLDA.Migrator`

---

## 7. 📋 TÓM TẮT CÔNG VIỆC

### 🎯 Tổng quan

**HoSoMoiThauDienTu CRUD** gồm:
- ✅ **1 Entity** chính (`HoSoMoiThauDienTu`) - entity mới
- ✅ **1 Danh mục** (`DanhMucHinhThucLuaChonNhaThau`) - **ĐÃ TỒN TẠI ĐẦY ĐỦ**, không cần cập nhật gì
- ✅ **~12 Files** cần tạo/sửa
- ✅ **5 API endpoints** mới
- ✅ Support **file attachment** qua `TepDinhKem` (gửi trong body, dùng `TepDinhKemBulkInsertOrUpdateCommand`)

---

### 📁 Tóm tắt files

**Domain Layer (1 file)**
- `HoSoMoiThauDienTu.cs` (entity mới)

> `DanhMucHinhThucLuaChonNhaThau.cs` đã tồn tại

**Persistence Layer (1 file)**
- `HoSoMoiThauDienTuConfiguration.cs` (entity mới)

> `DanhMucHinhThucLuaChonNhaThauConfiguration.cs` đã tồn tại và **không cần sửa**

**Application Layer (8 files)**
- DTOs: InsertDto (IMayHaveTepDinhKemDto), UpdateModel, SearchDto, Dto + Mappings
- Commands: Insert, Update, Delete
- Queries: Get, GetDanhSach

**WebApi Layer (2 files)**
- `HoSoMoiThauDienTuModel.cs` + `HoSoMoiThauDienTuMappingConfiguration.cs`
- `HoSoMoiThauDienTuController.cs`

> `DanhMucHinhThucLuaChonNhaThauController.cs` đã tồn tại

---

### 🔌 API Endpoints

| Method | Route | Mô tả |
|--------|-------|-------|
| `GET` | `/api/ho-so-moi-thau-dien-tu/{id}` | Chi tiết + danh sách file |
| `GET` | `/api/ho-so-moi-thau-dien-tu/danh-sach` | Danh sách phân trang |
| `POST` | `/api/ho-so-moi-thau-dien-tu/them-moi` | Thêm mới (có file trong body) |
| `PUT` | `/api/ho-so-moi-thau-dien-tu/cap-nhat` | Cập nhật (có file trong body) |
| `DELETE` | `/api/ho-so-moi-thau-dien-tu/{id}` | Xóa mềm |
