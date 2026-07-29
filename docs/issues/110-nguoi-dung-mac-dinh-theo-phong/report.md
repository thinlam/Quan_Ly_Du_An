# Issue #110 – CRUD Người dùng mặc định theo phòng ban

**Document date**: June 26, 2026  
**Last updated**: June 26, 2026  
**Status**: **IMPLEMENTED**  
**Module**: QLDA  
**Entity**: `NguoiDungMacDinhTheoPhong`

---

## 0. Trạng thái triển khai

| Hạng mục | Trạng thái |
|----------|------------|
| Domain entity | ✅ Done |
| EF Configuration | ✅ Done |
| Application (DTO / Query / Command) | ✅ Done |
| WebApi Controller | ✅ Done |
| Migration file | ✅ `20260626032714_AddNguoiDungMacDinhTheoPhong` |
| `dotnet build` | ✅ Pass |
| `ef.bat QLDA update` | ✅ Pass — xem [mục 14](#14-troubleshooting-migration) nếu gặp lỗi `ThamDinh` |

**Docs liên quan:**

- [ISSUE-migration-thamdinh-duplicate-column.md](./ISSUE-migration-thamdinh-duplicate-column.md) — phân tích lỗi migration `UpdateHinhThucLCNT` (không liên quan task #110)

**Mục lục:** [3. Thứ tự](#3-thứ-tự-thực-hiện) · [4. Chi tiết code](#4-chi-tiết-từng-bước-code) · [5–7. Tóm tắt layer](#5-domain--persistence-tóm-tắt) · [8. Checklist](#8-checklist-hoàn-thành) · [13. Test plan](#13-test-plan-gợi-ý) · [14. Migration troubleshooting](#14-troubleshooting-migration)

---

## 1. Tóm tắt nghiệp vụ

Quản lý danh sách **người dùng mặc định** gắn với từng **phòng ban**.

| Quy tắc | Mô tả |
|---------|--------|
| Quan hệ | 1 phòng ban → **nhiều** người dùng mặc định |
| Unique | Không trùng cặp `(PhongBanId, NguoiDungId)` trên bản ghi đang active |
| CRUD | Danh sách (filter + phân trang), thêm, sửa, xóa mềm |
| Hiển thị | Danh sách trả thêm `TenPhongBan`, `TenNguoiDung` |

**Ví dụ dữ liệu:**

| Phòng ban | Người dùng |
|-----------|------------|
| Phòng A | Trần A |
| Phòng A | Trần B |
| Phòng B | Trần C |

---

## 2. Phân tích source & quy ước

### 2.1 Entity liên quan (kiểu dữ liệu thực tế)

| Bảng / Entity | PK | Field liên quan | Ghi chú |
|---------------|-----|-----------------|---------|
| `DmDonVi` | `long` | `TenDonVi` | Legacy — **phòng ban** |
| `UserMaster` | `long` | `HoTen`, `UserName` | Legacy — **người dùng** |

> Spec gợi ý `int` cho `PhongBanId` / `NguoiDungId` — **không đúng source**. Đã dùng `long`.

### 2.2 Module tham chiếu khi implement

| Module | Dùng cho |
|--------|----------|
| `NhaThauNguoiDung` | Junction table, unique index cặp |
| `DanhMucLoaiDieuChinh` | CRUD controller, soft delete |
| `BanGiaoHoSoGetDanhSachQuery` | Join `DmDonVi` + `UserMaster` |
| `KySoInsertCommand` | Transaction pattern cho Insert |

### 2.3 Quy tắc kiến trúc đã tuân thủ

1. Không navigation property tới `DmDonVi` / `UserMaster` trên entity.
2. Không FK trong EF Configuration tới legacy tables.
3. DTO trong Application — WebApi dùng DTO trực tiếp, không tạo business model riêng.
4. `GetQueryableSet()` đã filter `IsDeleted` — không filter thừa.
5. Migration tạo bằng `ef.bat QLDA add` — không sửa tay file migration / snapshot.

---

## 3. Thứ tự thực hiện

```
Bước 1:  Domain        → Entity NguoiDungMacDinhTheoPhong
Bước 2:  Persistence   → NguoiDungMacDinhTheoPhongConfiguration
Bước 3:  Migration     → ef.bat QLDA add AddNguoiDungMacDinhTheoPhong
Bước 4:  Application   → DTOs (tách file) + Mappings
Bước 5:  Application   → NguoiDungMacDinhTheoPhongValidation
Bước 6:  Application   → GetDanhSachQuery, GetByIdQuery
Bước 7:  Application   → InsertCommand, UpdateCommand, DeleteCommand
Bước 8:  WebApi        → NguoiDungMacDinhTheoPhongController
Bước 9:  Verify        → dotnet build + ef.bat QLDA update
```

**Commit group:** Bước 1 + 2 + 3 cùng một commit (Domain + Configuration + Migrator).

---

## 4. Chi tiết từng bước code

### Bước 1 – Domain: Entity

**Tạo file:** `QLDA.Domain/Entities/NguoiDungMacDinhTheoPhong.cs`

```csharp
namespace QLDA.Domain.Entities;

/// <summary>
/// Cấu hình người dùng mặc định theo phòng ban
/// </summary>
public class NguoiDungMacDinhTheoPhong : Entity<Guid>, IAggregateRoot
{
    public long PhongBanId { get; set; }
    public long NguoiDungId { get; set; }
}
```

**Lưu ý:** Không thêm `public DmDonVi? PhongBan` hay `UserMaster? NguoiDung` — legacy tables join trong Application.

---

### Bước 2 – Persistence: EF Configuration

**Tạo file:** `QLDA.Persistence/Configurations/NguoiDungMacDinhTheoPhongConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class NguoiDungMacDinhTheoPhongConfiguration
    : AggregateRootConfiguration<NguoiDungMacDinhTheoPhong>
{
    public override void Configure(EntityTypeBuilder<NguoiDungMacDinhTheoPhong> builder)
    {
        builder.ToTable(nameof(NguoiDungMacDinhTheoPhong));
        builder.ConfigureForBase();

        builder.HasIndex(e => new { e.PhongBanId, e.NguoiDungId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Không FK → DmDonVi / UserMaster
    }
}
```

`AppDbContext` auto-discover qua `ApplyConfigurationsFromAssembly` — **không** cần thêm `DbSet` thủ công.

---

### Bước 3 – Migration

```bash
ef.bat QLDA add AddNguoiDungMacDinhTheoPhong
ef.bat QLDA update
```

**Output:** `20260626032714_AddNguoiDungMacDinhTheoPhong.cs`

`Up()` chỉ gồm:

- `CreateTable("NguoiDungMacDinhTheoPhong", ...)`
- Index `IX_NguoiDungMacDinhTheoPhong_Index`
- Unique index `IX_NguoiDungMacDinhTheoPhong_PhongBanId_NguoiDungId` filter `[IsDeleted] = 0`

Nếu `update` fail lỗi `ThamDinh` → xem [mục 14](#14-troubleshooting-migration).

---

### Bước 4 – Application: DTOs + Mappings

**Convention:** 1 DTO = 1 file (giống `KySos/DTOs/`).

**`NguoiDungMacDinhTheoPhongDto.cs`**

```csharp
namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

public record NguoiDungMacDinhTheoPhongDto
{
    public Guid Id { get; init; }
    public long PhongBanId { get; init; }
    public string? TenPhongBan { get; init; }
    public long NguoiDungId { get; init; }
    public string? TenNguoiDung { get; init; }
}
```

**`NguoiDungMacDinhTheoPhongCreateDto.cs`**

```csharp
public record NguoiDungMacDinhTheoPhongCreateDto
{
    public long PhongBanId { get; init; }
    public long NguoiDungId { get; init; }
}
```

**`NguoiDungMacDinhTheoPhongUpdateDto.cs`**

```csharp
public record NguoiDungMacDinhTheoPhongUpdateDto
{
    public Guid Id { get; init; }
    public long PhongBanId { get; init; }
    public long NguoiDungId { get; init; }
}
```

**`NguoiDungMacDinhTheoPhongSearchDto.cs`**

```csharp
using QLDA.Application.Common.Interfaces;

public record NguoiDungMacDinhTheoPhongSearchDto : CommonSearchDto
{
    public long? PhongBanId { get; init; }
    public long? NguoiDungId { get; init; }
    public string? Keyword { get; init; }
}
```

**`NguoiDungMacDinhTheoPhongMappings.cs`** — dùng cho `GetByIdQuery`:

```csharp
internal static class NguoiDungMacDinhTheoPhongMappings
{
    public static IQueryable<NguoiDungMacDinhTheoPhongDto> SelectDto(
        this IQueryable<NguoiDungMacDinhTheoPhong> query,
        IQueryable<DmDonVi> dmDonVi,
        IQueryable<UserMaster> userMaster)
    {
        return from cfg in query
            join pb in dmDonVi on cfg.PhongBanId equals pb.Id into pbJoin
            from pb in pbJoin.DefaultIfEmpty()
            join nd in userMaster on cfg.NguoiDungId equals nd.Id into ndJoin
            from nd in ndJoin.DefaultIfEmpty()
            select new NguoiDungMacDinhTheoPhongDto
            {
                Id = cfg.Id,
                PhongBanId = cfg.PhongBanId,
                TenPhongBan = pb.TenDonVi,
                NguoiDungId = cfg.NguoiDungId,
                TenNguoiDung = nd.HoTen
            };
    }
}
```

---

### Bước 5 – Application: Validation dùng chung

**Tạo file:** `Commands/NguoiDungMacDinhTheoPhongValidation.cs`

```csharp
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Commands;

internal static class NguoiDungMacDinhTheoPhongValidation
{
    private const string DuplicateMessage =
        "Người dùng này đã được cấu hình mặc định cho phòng ban đã chọn.";

    public static void EnsureRequired(long phongBanId, long nguoiDungId)
    {
        ManagedException.ThrowIf(phongBanId <= 0, "Phòng ban là bắt buộc");
        ManagedException.ThrowIf(nguoiDungId <= 0, "Người dùng là bắt buộc");
    }

    public static async Task EnsureReferencesExistAsync(
        long phongBanId, long nguoiDungId,
        IRepository<DmDonVi, long> dmDonVi,
        IRepository<UserMaster, long> userMaster,
        CancellationToken cancellationToken)
    {
        var phongBanExists = await dmDonVi.GetQueryableSet()
            .AnyAsync(e => e.Id == phongBanId, cancellationToken);
        ManagedException.ThrowIf(!phongBanExists, "Không tìm thấy phòng ban");

        var nguoiDungExists = await userMaster.GetQueryableSet()
            .AnyAsync(e => e.Id == nguoiDungId, cancellationToken);
        ManagedException.ThrowIf(!nguoiDungExists, "Không tìm thấy người dùng");
    }

    public static async Task EnsureNotDuplicateAsync(
        long phongBanId, long nguoiDungId,
        IRepository<NguoiDungMacDinhTheoPhong, Guid> repository,
        CancellationToken cancellationToken,
        Guid? excludeId = null)
    {
        var exists = await repository.GetQueryableSet()
            .WhereIf(excludeId.HasValue, e => e.Id != excludeId)
            .AnyAsync(e => e.PhongBanId == phongBanId && e.NguoiDungId == nguoiDungId,
                cancellationToken);
        ManagedException.ThrowIf(exists, DuplicateMessage);
    }
}
```

Gọi từ Insert (không `excludeId`) và Update (truyền `dto.Id`).

---

### Bước 6 – Application: Queries

**Tạo file:** `Queries/NguoiDungMacDinhTheoPhongGetDanhSachQuery.cs`

```csharp
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Queries;

public record NguoiDungMacDinhTheoPhongGetDanhSachQuery(NguoiDungMacDinhTheoPhongSearchDto SearchDto)
    : IRequest<PaginatedList<NguoiDungMacDinhTheoPhongDto>>;

internal class NguoiDungMacDinhTheoPhongGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NguoiDungMacDinhTheoPhongGetDanhSachQuery, PaginatedList<NguoiDungMacDinhTheoPhongDto>>
{
    private readonly IRepository<NguoiDungMacDinhTheoPhong, Guid> _repository =
        serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>();

    private readonly IRepository<DmDonVi, long> _dmDonVi =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();

    private readonly IRepository<UserMaster, long> _userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    public async Task<PaginatedList<NguoiDungMacDinhTheoPhongDto>> Handle(
        NguoiDungMacDinhTheoPhongGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var search = request.SearchDto;
        var keyword = search.Keyword ?? search.GlobalFilter;

        var queryable = _repository.GetQueryableSet()
            .AsNoTracking()
            .WhereIf(search.PhongBanId.HasValue, e => e.PhongBanId == search.PhongBanId)
            .WhereIf(search.NguoiDungId.HasValue, e => e.NguoiDungId == search.NguoiDungId);

        var dmDonVi = _dmDonVi.GetQueryableSet().AsNoTracking();
        var userMaster = _userMaster.GetQueryableSet().AsNoTracking();

        var query = from cfg in queryable
            join pb in dmDonVi on cfg.PhongBanId equals pb.Id into pbJoin
            from pb in pbJoin.DefaultIfEmpty()
            join nd in userMaster on cfg.NguoiDungId equals nd.Id into ndJoin
            from nd in ndJoin.DefaultIfEmpty()
            where string.IsNullOrEmpty(keyword)
                  || (pb.TenDonVi != null && pb.TenDonVi.Contains(keyword))
                  || (nd.HoTen != null && nd.HoTen.Contains(keyword))
                  || (nd.UserName != null && nd.UserName.Contains(keyword))
            orderby cfg.CreatedAt descending
            select new NguoiDungMacDinhTheoPhongDto
            {
                Id = cfg.Id,
                PhongBanId = cfg.PhongBanId,
                TenPhongBan = pb.TenDonVi,
                NguoiDungId = cfg.NguoiDungId,
                TenNguoiDung = nd.HoTen
            };

        return await query.PaginatedListAsync(search.Skip(), search.Take(), cancellationToken);
    }
}
```

**Tạo file:** `Queries/NguoiDungMacDinhTheoPhongGetByIdQuery.cs`

```csharp
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Queries;

public record NguoiDungMacDinhTheoPhongGetByIdQuery(Guid Id) : IRequest<NguoiDungMacDinhTheoPhongDto>;

internal class NguoiDungMacDinhTheoPhongGetByIdQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NguoiDungMacDinhTheoPhongGetByIdQuery, NguoiDungMacDinhTheoPhongDto>
{
    private readonly IRepository<NguoiDungMacDinhTheoPhong, Guid> _repository =
        serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>();

    private readonly IRepository<DmDonVi, long> _dmDonVi =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();

    private readonly IRepository<UserMaster, long> _userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    public async Task<NguoiDungMacDinhTheoPhongDto> Handle(
        NguoiDungMacDinhTheoPhongGetByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var dmDonVi = _dmDonVi.GetQueryableSet().AsNoTracking();
        var userMaster = _userMaster.GetQueryableSet().AsNoTracking();

        var dto = await _repository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .SelectDto(dmDonVi, userMaster)
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIfNull(dto, "Không tìm thấy dữ liệu");
        return dto;
    }
}
```

**Không cần `FilterVisible`** — entity không có `DuAnId`.

---

### Bước 7 – Application: Commands

**Tạo file:** `Commands/NguoiDungMacDinhTheoPhongInsertCommand.cs`

```csharp
using System.Data;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.Queries;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Commands;

public record NguoiDungMacDinhTheoPhongInsertCommand(NguoiDungMacDinhTheoPhongCreateDto Dto)
    : IRequest<NguoiDungMacDinhTheoPhongDto>;

internal class NguoiDungMacDinhTheoPhongInsertCommandHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NguoiDungMacDinhTheoPhongInsertCommand, NguoiDungMacDinhTheoPhongDto>
{
    private readonly IRepository<NguoiDungMacDinhTheoPhong, Guid> _repository =
        serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>();

    private readonly IRepository<DmDonVi, long> _dmDonVi =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();

    private readonly IRepository<UserMaster, long> _userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    private readonly IMediator _mediator = serviceProvider.GetRequiredService<IMediator>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider
        .GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>().UnitOfWork;

    public async Task<NguoiDungMacDinhTheoPhongDto> Handle(
        NguoiDungMacDinhTheoPhongInsertCommand request,
        CancellationToken cancellationToken = default)
    {
        var dto = request.Dto;
        NguoiDungMacDinhTheoPhongValidation.EnsureRequired(dto.PhongBanId, dto.NguoiDungId);
        await NguoiDungMacDinhTheoPhongValidation.EnsureReferencesExistAsync(
            dto.PhongBanId, dto.NguoiDungId, _dmDonVi, _userMaster, cancellationToken);
        await NguoiDungMacDinhTheoPhongValidation.EnsureNotDuplicateAsync(
            dto.PhongBanId, dto.NguoiDungId, _repository, cancellationToken);

        var entity = new NguoiDungMacDinhTheoPhong
        {
            PhongBanId = dto.PhongBanId,
            NguoiDungId = dto.NguoiDungId
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(
            IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return await _mediator.Send(
            new NguoiDungMacDinhTheoPhongGetByIdQuery(entity.Id), cancellationToken);
    }
}
```

**Tạo file:** `Commands/NguoiDungMacDinhTheoPhongUpdateCommand.cs`

```csharp
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.Queries;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Commands;

public record NguoiDungMacDinhTheoPhongUpdateCommand(NguoiDungMacDinhTheoPhongUpdateDto Dto)
    : IRequest<NguoiDungMacDinhTheoPhongDto>;

internal class NguoiDungMacDinhTheoPhongUpdateCommandHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NguoiDungMacDinhTheoPhongUpdateCommand, NguoiDungMacDinhTheoPhongDto>
{
    private readonly IRepository<NguoiDungMacDinhTheoPhong, Guid> _repository =
        serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>();

    private readonly IRepository<DmDonVi, long> _dmDonVi =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();

    private readonly IRepository<UserMaster, long> _userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    private readonly IMediator _mediator = serviceProvider.GetRequiredService<IMediator>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider
        .GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>().UnitOfWork;

    public async Task<NguoiDungMacDinhTheoPhongDto> Handle(
        NguoiDungMacDinhTheoPhongUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        var dto = request.Dto;
        NguoiDungMacDinhTheoPhongValidation.EnsureRequired(dto.PhongBanId, dto.NguoiDungId);
        await NguoiDungMacDinhTheoPhongValidation.EnsureReferencesExistAsync(
            dto.PhongBanId, dto.NguoiDungId, _dmDonVi, _userMaster, cancellationToken);
        await NguoiDungMacDinhTheoPhongValidation.EnsureNotDuplicateAsync(
            dto.PhongBanId, dto.NguoiDungId, _repository, cancellationToken, dto.Id);

        var entity = await _repository.GetOrderedSet()
            .FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu");

        entity.PhongBanId = dto.PhongBanId;
        entity.NguoiDungId = dto.NguoiDungId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(
            new NguoiDungMacDinhTheoPhongGetByIdQuery(entity.Id), cancellationToken);
    }
}
```

**Tạo file:** `Commands/NguoiDungMacDinhTheoPhongDeleteCommand.cs`

```csharp
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Commands;

public record NguoiDungMacDinhTheoPhongDeleteCommand(Guid Id) : IRequest<bool>;

internal class NguoiDungMacDinhTheoPhongDeleteCommandHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NguoiDungMacDinhTheoPhongDeleteCommand, bool>
{
    private readonly IRepository<NguoiDungMacDinhTheoPhong, Guid> _repository =
        serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>();

    private readonly IUnitOfWork _unitOfWork = serviceProvider
        .GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>().UnitOfWork;

    public async Task<bool> Handle(
        NguoiDungMacDinhTheoPhongDeleteCommand request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOrderedSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu");

        entity.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
```

**Lưu ý `GetOrderedSet()` vs `GetQueryableSet()`:**

- Load/sửa/xóa entity → `GetOrderedSet()` (track entity)
- List/filter → `GetQueryableSet()` (đã filter `IsDeleted`)

---

### Bước 8 – WebApi: Controller

**Tạo file:** `QLDA.WebApi/Controllers/NguoiDungMacDinhTheoPhongController.cs`

```csharp
using System.Net.Mime;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.Commands;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.Queries;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers;

[Tags("Người dùng mặc định theo phòng ban")]
[Route("api/nguoi-dung-mac-dinh-theo-phong")]
[Authorize(Roles = RoleConstants.GroupAdminOrManager)]
public class NguoiDungMacDinhTheoPhongController(IServiceProvider serviceProvider)
    : AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<PaginatedList<NguoiDungMacDinhTheoPhongDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetDanhSach([FromQuery] NguoiDungMacDinhTheoPhongSearchDto searchDto)
    {
        var res = await Mediator.Send(new NguoiDungMacDinhTheoPhongGetDanhSachQuery(searchDto));
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<NguoiDungMacDinhTheoPhongDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id:guid}")]
    public async Task<ResultApi> GetById(Guid id)
    {
        var dto = await Mediator.Send(new NguoiDungMacDinhTheoPhongGetByIdQuery(id));
        return ResultApi.Ok(dto);
    }

    [ProducesResponseType<ResultApi<NguoiDungMacDinhTheoPhongDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] NguoiDungMacDinhTheoPhongCreateDto dto)
    {
        var result = await Mediator.Send(new NguoiDungMacDinhTheoPhongInsertCommand(dto));
        return ResultApi.Ok(result);
    }

    [ProducesResponseType<ResultApi<NguoiDungMacDinhTheoPhongDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat/{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(Guid id, [FromBody] NguoiDungMacDinhTheoPhongUpdateDto dto)
    {
        ManagedException.ThrowIf(id != dto.Id, "Id không khớp");
        var result = await Mediator.Send(new NguoiDungMacDinhTheoPhongUpdateCommand(dto));
        return ResultApi.Ok(result);
    }

    [ProducesResponseType<ResultApi<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("xoa/{id:guid}")]
    public async Task<ResultApi> Delete(Guid id)
    {
        await Mediator.Send(new NguoiDungMacDinhTheoPhongDeleteCommand(id));
        return ResultApi.Ok(true);
    }
}
```

Không tạo `Models/NguoiDungMacDinhTheoPhong/` — dùng DTO Application trực tiếp.

MediatR auto-register handler qua assembly scan — **không** cần đăng ký DI thủ công.

---

### Bước 9 – Verify

```bash
dotnet build SER.sln
ef.bat QLDA update
```

Kiểm tra bảng:

```sql
SELECT TOP 5 * FROM NguoiDungMacDinhTheoPhong;
```

---

## 5. Domain & Persistence (tóm tắt)

### 5.1 Entity

**File:** `QLDA.Domain/Entities/NguoiDungMacDinhTheoPhong.cs`

```csharp
public class NguoiDungMacDinhTheoPhong : Entity<Guid>, IAggregateRoot
{
    public long PhongBanId { get; set; }
    public long NguoiDungId { get; set; }
}
```

| Field | Kiểu | Ghi chú |
|-------|------|---------|
| `Id` | `Guid` | Sequential guid qua `Entity<Guid>` |
| `PhongBanId` | `long` | → `DmDonVi.Id` |
| `NguoiDungId` | `long` | → `UserMaster.Id` |
| Audit | inherited | `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`, `IsDeleted`, `Index` |

### 5.2 EF Configuration

**File:** `QLDA.Persistence/Configurations/NguoiDungMacDinhTheoPhongConfiguration.cs`

- `ToTable(nameof(NguoiDungMacDinhTheoPhong))`
- `ConfigureForBase()`
- Unique index `(PhongBanId, NguoiDungId)` filter `[IsDeleted] = 0`
- Không FK tới legacy tables

### 5.3 Migration

**File đã tạo:**

```
QLDA.Migrator/Migrations/20260626032714_AddNguoiDungMacDinhTheoPhong.cs
QLDA.Migrator/Migrations/20260626032714_AddNguoiDungMacDinhTheoPhong.Designer.cs
```

**Nội dung `Up()`:** chỉ `CreateTable NguoiDungMacDinhTheoPhong` + 2 index — **không** đụng bảng khác.

**Apply migration:**

```bash
ef.bat QLDA update
```

**Bảng DB:** `NguoiDungMacDinhTheoPhong`

| Column | Type |
|--------|------|
| Id | uniqueidentifier (PK) |
| PhongBanId | bigint |
| NguoiDungId | bigint |
| CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, IsDeleted, Index | theo `ConfigureForBase()` |

**Commit group (project rules):** Domain + Persistence.Configuration + Migrator cùng một commit.

---

## 6. Application layer (tóm tắt)

### 6.1 Cấu trúc thư mục (đã tạo)

```
QLDA.Application/NguoiDungMacDinhTheoPhongs/
├── DTOs/
│   ├── NguoiDungMacDinhTheoPhongDto.cs           # 1 file / 1 DTO
│   ├── NguoiDungMacDinhTheoPhongCreateDto.cs
│   ├── NguoiDungMacDinhTheoPhongUpdateDto.cs
│   ├── NguoiDungMacDinhTheoPhongSearchDto.cs
│   └── NguoiDungMacDinhTheoPhongMappings.cs     # SelectDto() join legacy
├── Queries/
│   ├── NguoiDungMacDinhTheoPhongGetDanhSachQuery.cs
│   └── NguoiDungMacDinhTheoPhongGetByIdQuery.cs
└── Commands/
    ├── NguoiDungMacDinhTheoPhongInsertCommand.cs
    ├── NguoiDungMacDinhTheoPhongUpdateCommand.cs
    ├── NguoiDungMacDinhTheoPhongDeleteCommand.cs
    └── NguoiDungMacDinhTheoPhongValidation.cs    # validation dùng chung
```

### 6.2 DTOs

| DTO | File | Mục đích |
|-----|------|----------|
| `NguoiDungMacDinhTheoPhongDto` | `...Dto.cs` | List / detail — có `TenPhongBan`, `TenNguoiDung` |
| `NguoiDungMacDinhTheoPhongCreateDto` | `...CreateDto.cs` | POST them-moi |
| `NguoiDungMacDinhTheoPhongUpdateDto` | `...UpdateDto.cs` | PUT cap-nhat |
| `NguoiDungMacDinhTheoPhongSearchDto` | `...SearchDto.cs` | Kế thừa `CommonSearchDto` |

**SearchDto fields:**

```csharp
public record NguoiDungMacDinhTheoPhongSearchDto : CommonSearchDto
{
    public long? PhongBanId { get; init; }
    public long? NguoiDungId { get; init; }
    public string? Keyword { get; init; }   // handler: Keyword ?? GlobalFilter
}
```

Kế thừa `CommonSearchDto` → có sẵn `PageIndex`, `PageSize`, `GlobalFilter`.

### 6.3 Queries

**GetDanhSach** — filter `PhongBanId`, `NguoiDungId`, keyword (tên phòng ban / họ tên / username); join `DmDonVi` + `UserMaster`; order `CreatedAt desc`; phân trang.

**GetById** — join lấy tên; `ManagedException.ThrowIfNull(dto, "Không tìm thấy dữ liệu")`.

### 6.4 Validation — `NguoiDungMacDinhTheoPhongValidation`

| Method | Mục đích |
|--------|----------|
| `EnsureRequired` | `PhongBanId`, `NguoiDungId` > 0 |
| `EnsureReferencesExistAsync` | Check tồn tại `DmDonVi`, `UserMaster` |
| `EnsureNotDuplicateAsync` | Check unique; `excludeId` khi update |

| Check | Message |
|-------|---------|
| Phòng ban bắt buộc | `"Phòng ban là bắt buộc"` |
| Người dùng bắt buộc | `"Người dùng là bắt buộc"` |
| Không tìm thấy phòng ban | `"Không tìm thấy phòng ban"` |
| Không tìm thấy người dùng | `"Không tìm thấy người dùng"` |
| Trùng cặp | `"Người dùng này đã được cấu hình mặc định cho phòng ban đã chọn."` |

### 6.5 Commands

| Command | Return | Ghi chú |
|---------|--------|---------|
| `InsertCommand` | `NguoiDungMacDinhTheoPhongDto` | Transaction + validate + `GetByIdQuery` trả kết quả |
| `UpdateCommand` | `NguoiDungMacDinhTheoPhongDto` | `GetOrderedSet()` load → gán field → save |
| `DeleteCommand` | `bool` | Soft delete `IsDeleted = true` |

---

## 7. WebApi layer

### 7.1 Controller

**File:** `QLDA.WebApi/Controllers/NguoiDungMacDinhTheoPhongController.cs`

- Route: `api/nguoi-dung-mac-dinh-theo-phong`
- Auth: `[Authorize(Roles = RoleConstants.GroupAdminOrManager)]`
- Không chứa business logic — chỉ gọi Mediator

### 7.2 API endpoints

| Method | Route | Handler | Input |
|--------|-------|---------|-------|
| `GET` | `danh-sach` | `GetDanhSachQuery` | Query: `NguoiDungMacDinhTheoPhongSearchDto` |
| `GET` | `{id:guid}` | `GetByIdQuery` | Route: `Guid id` |
| `POST` | `them-moi` | `InsertCommand` | Body: `NguoiDungMacDinhTheoPhongCreateDto` |
| `PUT` | `cap-nhat/{id:guid}` | `UpdateCommand` | Route `id` + body `UpdateDto` (validate `id == dto.Id`) |
| `DELETE` | `xoa/{id:guid}` | `DeleteCommand` | Route: `Guid id` |

### 7.3 Ví dụ API

**GET danh-sach**

```http
GET /api/nguoi-dung-mac-dinh-theo-phong/danh-sach?PhongBanId=10&PageIndex=1&PageSize=20&Keyword=Trần
```

**POST them-moi**

```json
{ "phongBanId": 10, "nguoiDungId": 1001 }
```

**PUT cap-nhat/{id}**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "phongBanId": 10,
  "nguoiDungId": 1002
}
```

---

## 8. Checklist hoàn thành

- [x] Entity không có navigation tới `DmDonVi` / `UserMaster`
- [x] Unique index filter `[IsDeleted] = 0`
- [x] Handler check trùng cặp đúng message nghiệp vụ
- [x] Update bỏ qua chính record khi check trùng (`excludeId`)
- [x] Danh sách join `TenDonVi` → `TenPhongBan`, `HoTen` → `TenNguoiDung`
- [x] Controller không chứa business logic
- [x] Không model nghiệp vụ riêng ở WebApi
- [x] DTO tách file riêng (convention giống `KySos`)
- [x] `dotnet build` pass
- [x] `ef.bat QLDA update` apply thành công trên DB dev
- [x] Manual test Postman / FE

---

## 9. Danh sách file đã tạo

| # | File |
|---|------|
| 1 | `QLDA.Domain/Entities/NguoiDungMacDinhTheoPhong.cs` |
| 2 | `QLDA.Persistence/Configurations/NguoiDungMacDinhTheoPhongConfiguration.cs` |
| 3 | `QLDA.Migrator/Migrations/20260626032714_AddNguoiDungMacDinhTheoPhong.cs` |
| 4 | `QLDA.Migrator/Migrations/20260626032714_AddNguoiDungMacDinhTheoPhong.Designer.cs` |
| 5 | `QLDA.Application/NguoiDungMacDinhTheoPhongs/DTOs/NguoiDungMacDinhTheoPhongDto.cs` |
| 6 | `QLDA.Application/NguoiDungMacDinhTheoPhongs/DTOs/NguoiDungMacDinhTheoPhongCreateDto.cs` |
| 7 | `QLDA.Application/NguoiDungMacDinhTheoPhongs/DTOs/NguoiDungMacDinhTheoPhongUpdateDto.cs` |
| 8 | `QLDA.Application/NguoiDungMacDinhTheoPhongs/DTOs/NguoiDungMacDinhTheoPhongSearchDto.cs` |
| 9 | `QLDA.Application/NguoiDungMacDinhTheoPhongs/DTOs/NguoiDungMacDinhTheoPhongMappings.cs` |
| 10 | `QLDA.Application/NguoiDungMacDinhTheoPhongs/Queries/NguoiDungMacDinhTheoPhongGetDanhSachQuery.cs` |
| 11 | `QLDA.Application/NguoiDungMacDinhTheoPhongs/Queries/NguoiDungMacDinhTheoPhongGetByIdQuery.cs` |
| 12 | `QLDA.Application/NguoiDungMacDinhTheoPhongs/Commands/NguoiDungMacDinhTheoPhongInsertCommand.cs` |
| 13 | `QLDA.Application/NguoiDungMacDinhTheoPhongs/Commands/NguoiDungMacDinhTheoPhongUpdateCommand.cs` |
| 14 | `QLDA.Application/NguoiDungMacDinhTheoPhongs/Commands/NguoiDungMacDinhTheoPhongDeleteCommand.cs` |
| 15 | `QLDA.Application/NguoiDungMacDinhTheoPhongs/Commands/NguoiDungMacDinhTheoPhongValidation.cs` |
| 16 | `QLDA.WebApi/Controllers/NguoiDungMacDinhTheoPhongController.cs` |

---

## 10. Phạm vi KHÔNG làm (out of scope)

| Hạng mục | Lý do |
|----------|-------|
| Sửa `PhanQuyenChucNang` | Nghiệp vụ khác |
| Auto-assign user khi tạo phiếu | Issue riêng |
| Frontend | Issue backend #110 |
| Seed data | Không yêu cầu |
| Unit test | Chưa yêu cầu |

---

## 11. Rủi ro & lưu ý vận hành

1. **Legacy tables:** Không FK — join `DefaultIfEmpty`, tên có thể `null` nếu orphan data.
2. **Unique:** Có cả DB index + app validation → message lỗi thân thiện.
3. **Authorization:** `GroupAdminOrManager` — không cần `FilterVisible` (không có `DuAnId`).
4. **UserMaster:** `GetQueryableSet()` filter `Used = true` — user inactive fail validation tồn tại.

---

## 12. Tham chiếu code

| Mục đích | File |
|----------|------|
| Junction + unique | `QLDA.Domain/Entities/NhaThauNguoiDung.cs` |
| EF unique index | `QLDA.Persistence/Configurations/NhaThauNguoiDungConfiguration.cs` |
| CRUD controller | `QLDA.WebApi/Controllers/DanhMucLoaiDieuChinhController.cs` |
| Soft delete | `QLDA.Application/DanhMucLoaiDieuChinhs/Commands/DanhMucLoaiDieuChinhDeleteCommand.cs` |
| Join legacy | `QLDA.Application/BanGiaoHoSos/Queries/BanGiaoHoSoGetDanhSachQuery.cs` |
| DTO 1 file / 1 record | `QLDA.Application/KySos/DTOs/` |
| CommonSearchDto | `QLDA.Application/Common/DTOs/CommonSearchDto.cs` |

---

## 13. Test plan (gợi ý)

| # | Case | Kỳ vọng |
|---|------|---------|
| 1 | GET danh-sach không filter | 200, paginated list |
| 2 | Filter `PhongBanId` | Chỉ record thuộc phòng ban |
| 3 | Keyword tên user / phòng ban | Match partial |
| 4 | POST hợp lệ | 200, trả dto có tên |
| 5 | POST trùng cặp | 400, message duplicate |
| 6 | POST phòng ban không tồn tại | 400 |
| 7 | PUT đổi user | 200 |
| 8 | PUT trùng cặp (khác record) | 400 |
| 9 | PUT route id ≠ body id | 400, `"Id không khớp"` |
| 10 | DELETE | Soft delete, GET by id → 404 |

---

## 14. Troubleshooting migration

Nếu `ef.bat QLDA update` fail với lỗi duplicate column `ThamDinh` trên `HoSoMoiThauDienTu`:

- **Không phải** do migration `AddNguoiDungMacDinhTheoPhong` — EF bị chặn ở migration cũ hơn (`UpdateHinhThucLCNT`).
- **Nguyên nhân:** DB drift — cột đã tồn tại nhưng `__EFMigrationsHistory` chưa ghi migration đó.

**Xử lý:** Xem chi tiết và script SQL trong [ISSUE-migration-thamdinh-duplicate-column.md](./ISSUE-migration-thamdinh-duplicate-column.md).

**Ràng buộc:** Không drop database. Không sửa file migration (`.cs` trong `QLDA.Migrator/Migrations/`).

**Sau khi fix drift:**

```bash
ef.bat QLDA update
```

Kiểm tra bảng mới:

```sql
SELECT TOP 5 * FROM NguoiDungMacDinhTheoPhong;
SELECT * FROM __EFMigrationsHistory
WHERE MigrationId LIKE '%NguoiDungMacDinhTheoPhong%';
```

