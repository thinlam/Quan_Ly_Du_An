# Task – Bổ sung UC Quản lý Bàn Giao Hồ Sơ (BanGiaoHoSo) – Version 3

> **Kế thừa từ Version 2** – Xem chi tiết đầy đủ tại `task-ban-giao-ho-so vesion 2.md`

> **✅ Added (14/05/2026) – Version 3**
> - Endpoint `ban-giao` nhận thêm `PhongBanNhanId` (long?) – phòng ban nhận hồ sơ
> - Entity thêm cột `PhongBanNhanId` (long?) – lưu phòng ban nhận
> - `BanGiaoHoSoDto` (response danh sách/chi tiết): thêm `PhongBanNhanId` (long?) và `TenPhongBanNhan` (string?)
> - `GetDanhSachQuery`: Join thêm `DanhMucDonVi` lần 2 cho `PhongBanNhanId` → lấy `TenPhongBanNhan`
> - `BanGiaoHoSoGetQuery` (chi tiết): vì không có FK/navigation đến DanhMucDonVi, `TenPhongBanNhan` sẽ được trả về `null` (client dùng danh sách `/danh-sach` để có đủ thông tin)
> - Migration mới: `AddBanGiaoHoSoPhongBanNhan`

---

## 1. Thay đổi so với Version 2

### 1.1 Tóm tắt thay đổi

| Layer | File | Thay đổi |
|-------|------|---------|
| Domain | `BanGiaoHoSo.cs` | Thêm `PhongBanNhanId` (long?) |
| Persistence | `BanGiaoHoSoConfiguration.cs` | Không thay đổi cấu hình (không FK đến DM_DONVI) |
| Persistence | Migration | Thêm migration `AddBanGiaoHoSoPhongBanNhan` |
| Application | `BanGiaoHoSoBanGiaoDto.cs` | Thêm `PhongBanNhanId` (long?) |
| Application | `BanGiaoHoSoDto.cs` | Thêm `PhongBanNhanId` (long?), `TenPhongBanNhan` (string?) |
| Application | `BanGiaoHoSoBanGiaoCommand.cs` | Set `entity.PhongBanNhanId` từ DTO |
| Application | `BanGiaoHoSoGetDanhSachQuery.cs` | Join thêm `DanhMucDonVi` lần 2 cho `PhongBanNhanId` |
| Application | `BanGiaoHoSoMappings.cs` | Cập nhật `ToDto()` thêm `PhongBanNhanId`, `TenPhongBanNhan` |
| WebApi | `BanGiaoHoSoBanGiaoModel.cs` | Thêm `PhongBanNhanId` (long?) |
| WebApi | `BanGiaoHoSoModel.cs` | Thêm `PhongBanNhanId` (long?), `TenPhongBanNhan` (string?) |
| WebApi | `BanGiaoHoSoMappingConfiguration.cs` | Cập nhật `ToModel()` thêm `PhongBanNhanId`, `TenPhongBanNhan` |

---

## 2. Chi tiết từng bước

---

### Bước 1 – Domain: Cập nhật Entity `BanGiaoHoSo`

**File:** `QLDA.Domain/Entities/BanGiaoHoSo.cs`

Thêm field `PhongBanNhanId` vào sau `PhongBanChuTriId`:

```csharp
/// <summary>
/// FK → Phòng ban chủ trì (phòng HC-TH hoặc tương tự)
/// ⚠️ Không FK, không navigation – lấy TenPhongBan qua LeftOuterJoin
/// </summary>
public long? PhongBanChuTriId { get; set; }

/// <summary>
/// Phòng ban nhận hồ sơ – được set khi gọi endpoint ban-giao
/// ⚠️ Không FK, không navigation – lấy TenPhongBanNhan qua LeftOuterJoin
/// </summary>
public long? PhongBanNhanId { get; set; }
```

**Entity đầy đủ sau khi cập nhật:**

```csharp
using QLDA.Domain.Enums;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng quản lý bàn giao hồ sơ từ người dùng → Phòng HC-TH
/// </summary>
public class BanGiaoHoSo : Entity<Guid>, IAggregateRoot {
    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GhiChu { get; set; }

    /// <summary>
    /// Phòng ban chủ trì (người tạo) – ⚠️ không FK, không navigation
    /// </summary>
    public long? PhongBanChuTriId { get; set; }

    /// <summary>
    /// Phòng ban nhận hồ sơ – được set khi gọi endpoint ban-giao
    /// ⚠️ Không FK, không navigation – lấy TenPhongBanNhan qua LeftOuterJoin
    /// </summary>
    public long? PhongBanNhanId { get; set; }

    public ETrangThaiBanGiao TrangThai { get; set; } = ETrangThaiBanGiao.KhoiTao;
    public DateTimeOffset? NgayBanGiao { get; set; }

    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DuAnBuoc? Buoc { get; set; }
    #endregion
}
```

---

### Bước 2 – Persistence: Migration

```bash
# Chạy trong thư mục QLDA.Migrator
dotnet ef migrations add AddBanGiaoHoSoPhongBanNhan
dotnet ef database update
```

**⚠️ IMPORTANT:** Không chạy `drop-database`. Chỉ chạy `migrations add` và `database update`.

> **Lưu ý:** Không cần sửa `BanGiaoHoSoConfiguration.cs` vì `PhongBanNhanId` là kiểu nullable scalar, EF tự nhận diện. Không tạo FK đến DM_DONVI (bảng đặc biệt).

---

### Bước 3 – Application: Cập nhật DTOs

#### `BanGiaoHoSoBanGiaoDto.cs` – Thêm `PhongBanNhanId`

```csharp
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

/// <summary>
/// DTO cho endpoint ban-giao: đổi trạng thái 1→2, set ngày bàn giao, đính kèm biên bản, set phòng ban nhận
/// </summary>
public class BanGiaoHoSoBanGiaoDto {
    /// <summary>Ngày bàn giao (DateOnly). Server tự convert sang DateTimeOffset UTC via DateOnlyExtensions.</summary>
    public DateOnly? NgayBanGiao { get; set; }

    /// <summary>Phòng ban nhận hồ sơ.</summary>
    public long? PhongBanNhanId { get; set; }

    /// <summary>Biên bản bàn giao (gắn khi thực hiện bàn giao)</summary>
    public List<TepDinhKemDto>? DanhSachBienBan { get; set; }
}
```

#### `BanGiaoHoSoDto.cs` – Thêm `PhongBanNhanId` và `TenPhongBanNhan`

```csharp
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.DTOs;

public class BanGiaoHoSoDto {
    public Guid Id { get; set; }
    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public Guid? DuAnId { get; set; }
    public string? TenDuAn { get; set; }
    public int? BuocId { get; set; }
    public string? TenBuoc { get; set; }
    public string? GhiChu { get; set; }
    public long? PhongBanChuTriId { get; set; }
    public string? TenPhongBan { get; set; }          // Phòng ban chủ trì (người tạo)
    public long? PhongBanNhanId { get; set; }          // ← THÊM MỚI
    public string? TenPhongBanNhan { get; set; }       // ← THÊM MỚI – chỉ có giá trị khi TrangThai = DaBanGiao
    public int TrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public DateOnly? NgayBanGiao { get; set; }
    public string? TenNguoiTao { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public List<TepDinhKemDto>? DanhSachTepHSBanGiao { get; set; }
    public List<TepDinhKemDto>? DanhSachBienBanBanGiao { get; set; }
}
```

---

### Bước 4 – Application: Cập nhật `BanGiaoHoSoBanGiaoCommand`

**File:** `QLDA.Application/BanGiaoHoSos/Commands/BanGiaoHoSoBanGiaoCommand.cs`

Thêm `PhongBanNhanId` vào record và set vào entity:

```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Enums;

namespace QLDA.Application.BanGiaoHoSos.Commands;

/// <summary>
/// Thực hiện bàn giao hồ sơ: đổi trạng thái 1→2, set NgayBanGiao, set PhongBanNhanId, lưu biên bản
/// </summary>
public record BanGiaoHoSoBanGiaoCommand(Guid Id, DateOnly? NgayBanGiao, long? PhongBanNhanId) : IRequest<BanGiaoHoSo>;

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
        entity.NgayBanGiao = request.NgayBanGiao?.ToStartOfDayUtc() ?? DateTimeOffset.UtcNow;
        entity.PhongBanNhanId = request.PhongBanNhanId;  // ← THÊM MỚI

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await BanGiaoHoSo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
```

---

### Bước 5 – Application: Cập nhật `BanGiaoHoSoMappings.cs`

Cập nhật `ToDto()` để map thêm `PhongBanNhanId` (TenPhongBanNhan không map ở đây – lấy qua LeftOuterJoin trong Query):

```csharp
public static BanGiaoHoSoDto ToDto(this BanGiaoHoSo entity,
    List<TepDinhKem>? tepHSBanGiao = null,
    List<TepDinhKem>? bienBanBanGiao = null) => new() {
    Id = entity.Id,
    Ma = entity.Ma,
    TenHoSo = entity.TenHoSo,
    DuAnId = entity.DuAnId,
    TenDuAn = entity.DuAn?.Ten,
    BuocId = entity.BuocId,
    TenBuoc = entity.Buoc?.TenBuoc,
    GhiChu = entity.GhiChu,
    PhongBanChuTriId = entity.PhongBanChuTriId,
    PhongBanNhanId = entity.PhongBanNhanId,   // ← THÊM MỚI
    // TenPhongBan, TenPhongBanNhan, TenNguoiTao: lấy qua LeftOuterJoin trong GetDanhSachQuery
    TrangThai = (int)entity.TrangThai,
    TenTrangThai = GetTrangThaiText(entity.TrangThai),
    NgayBanGiao = entity.NgayBanGiao.HasValue ? DateOnly.FromDateTime(entity.NgayBanGiao.Value.LocalDateTime) : null,
    DanhSachTepHSBanGiao = tepHSBanGiao?.Select(f => f.ToDto()).ToList(),
    DanhSachBienBanBanGiao = bienBanBanGiao?.Select(f => f.ToDto()).ToList()
};
```

---

### Bước 6 – Application: Cập nhật `BanGiaoHoSoGetDanhSachQuery.cs`

Join thêm `DanhMucDonVi` lần 2 cho `PhongBanNhanId`:

```csharp
internal class BanGiaoHoSoGetDanhSachQueryHandler : IRequestHandler<BanGiaoHoSoGetDanhSachQuery, PaginatedList<BanGiaoHoSoDto>> {
    // ... (inject giữ nguyên từ Version 2, DanhMucDonVi repository đã có)

    public async Task<PaginatedList<BanGiaoHoSoDto>> Handle(BanGiaoHoSoGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var users = _userMasterRepository.GetQueryableSet().AsNoTracking();
        var donVis = _danhMucDonViRepository.GetQueryableSet().AsNoTracking();

        var queryable = _banGiaoRepository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => e.CreatedBy == _userProvider.Id.ToString())
            .Include(e => e.DuAn)
            .Include(e => e.Buoc)
            .WhereIf(request.SearchDto.TrangThai.HasValue, e => (int)e.TrangThai == request.SearchDto.TrangThai!.Value)
            .WhereIf(request.SearchDto.DuAnId.HasValue, e => e.DuAnId == request.SearchDto.DuAnId!.Value)
            .WhereIf(request.SearchDto.BuocId.HasValue, e => e.BuocId == request.SearchDto.BuocId!.Value)
            // Join UserMaster để lấy TenNguoiTao
            .LeftOuterJoin(users, e => e.CreatedBy, u => u.Id.ToString(), (e, user) => new { e, user })
            // Join DanhMucDonVi lần 1: PhongBanChuTriId → TenPhongBan
            .LeftOuterJoin(donVis, x => x.e.PhongBanChuTriId, d => (long?)d.Id, (x, donViChuTri) => new { x.e, x.user, donViChuTri })
            // Join DanhMucDonVi lần 2: PhongBanNhanId → TenPhongBanNhan  ← THÊM MỚI
            .LeftOuterJoin(donVis, x => x.e.PhongBanNhanId, d => (long?)d.Id, (x, donViNhan) => new { x.e, x.user, x.donViChuTri, donViNhan })
            .OrderByDescending(x => x.e.CreatedAt)
            .Select(x => new BanGiaoHoSoDto {
                Id = x.e.Id,
                Ma = x.e.Ma,
                TenHoSo = x.e.TenHoSo,
                DuAnId = x.e.DuAnId,
                TenDuAn = x.e.DuAn!.TenDuAn,
                BuocId = x.e.BuocId,
                TenBuoc = x.e.Buoc!.Ten,
                GhiChu = x.e.GhiChu,
                PhongBanChuTriId = x.e.PhongBanChuTriId,
                TenPhongBan = x.donViChuTri != null ? x.donViChuTri.TenDonVi : null,
                PhongBanNhanId = x.e.PhongBanNhanId,                                      // ← THÊM MỚI
                TenPhongBanNhan = x.donViNhan != null ? x.donViNhan.TenDonVi : null,      // ← THÊM MỚI
                TrangThai = (int)x.e.TrangThai,
                TenTrangThai = GetTrangThaiText(x.e.TrangThai),
                NgayBanGiao = x.e.NgayBanGiao.HasValue ? DateOnly.FromDateTime(x.e.NgayBanGiao.Value.LocalDateTime) : null,
                TenNguoiTao = x.user != null ? x.user.HoTen : null,
                CreatedAt = x.e.CreatedAt,
                DanhSachTepHSBanGiao = _tepDinhKemRepository.GetQueryableSet()
                    .Where(f => f.GroupId == x.e.Id.ToString() && f.EGroupType == EGroupType.BanGiaoHoSo && !f.IsDeleted)
                    .Select(f => f.ToDto()).ToList(),
                DanhSachBienBanBanGiao = _tepDinhKemRepository.GetQueryableSet()
                    .Where(f => f.GroupId == x.e.Id.ToString() && f.EGroupType == EGroupType.BienBanBanGiao && !f.IsDeleted)
                    .Select(f => f.ToDto()).ToList()
            });

        return await queryable
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
    // ... GetTrangThaiText giữ nguyên
}
```

---

### Bước 7 – WebApi: Cập nhật Models

#### `BanGiaoHoSoBanGiaoModel.cs` – Thêm `PhongBanNhanId`

```csharp
namespace QLDA.WebApi.Models.BanGiaoHoSos;

/// <summary>
/// Model cho endpoint PUT /ban-giao: nhận ngày bàn giao, phòng ban nhận + biên bản bàn giao
/// </summary>
public class BanGiaoHoSoBanGiaoModel {
    /// <summary>Ngày bàn giao (DateOnly), nếu null sẽ dùng ngày hiện tại.</summary>
    public DateOnly? NgayBanGiao { get; set; }

    /// <summary>Phòng ban nhận hồ sơ.</summary>
    public long? PhongBanNhanId { get; set; }    // ← THÊM MỚI

    /// <summary>Biên bản bàn giao (đính kèm khi thực hiện bàn giao)</summary>
    public List<TepDinhKemModel>? DanhSachBienBan { get; set; }
}
```

#### `BanGiaoHoSoModel.cs` – Thêm `PhongBanNhanId` và `TenPhongBanNhan`

```csharp
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.BanGiaoHoSos;

// Response model cho endpoint chi-tiet
public class BanGiaoHoSoModel {
    public Guid Id { get; set; }
    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GhiChu { get; set; }
    public long? PhongBanChuTriId { get; set; }
    public long? PhongBanNhanId { get; set; }      // ← THÊM MỚI
    public string? TenPhongBanNhan { get; set; }   // ← THÊM MỚI (null nếu chưa bàn giao)
    public int TrangThai { get; set; }
    public DateOnly? NgayBanGiao { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
    public List<TepDinhKemModel>? DanhSachBienBanBanGiao { get; set; }
}
```

#### `BanGiaoHoSoMappingConfiguration.cs` – Cập nhật `ToModel()`

```csharp
public static BanGiaoHoSoModel ToModel(this BanGiaoHoSo entity,
    List<TepDinhKem>? tepHSBanGiao = null,
    List<TepDinhKem>? bienBanBanGiao = null) => new() {
    Id = entity.Id,
    Ma = entity.Ma,
    TenHoSo = entity.TenHoSo,
    DuAnId = entity.DuAnId,
    BuocId = entity.BuocId,
    GhiChu = entity.GhiChu,
    PhongBanChuTriId = entity.PhongBanChuTriId,
    PhongBanNhanId = entity.PhongBanNhanId,    // ← THÊM MỚI
    // TenPhongBanNhan: null ở chi-tiet vì không có FK/navigation đến DM_DONVI
    // Client dùng /danh-sach để có TenPhongBanNhan đầy đủ
    TrangThai = (int)entity.TrangThai,
    NgayBanGiao = entity.NgayBanGiao.HasValue ? DateOnly.FromDateTime(entity.NgayBanGiao.Value.LocalDateTime) : null,
    DanhSachTepDinhKem = tepHSBanGiao?.Select(f => f.ToModel()).ToList(),
    DanhSachBienBanBanGiao = bienBanBanGiao?.Select(f => f.ToModel()).ToList()
};
```

---

### Bước 8 – WebApi: Cập nhật Controller – Endpoint `ban-giao`

**File:** `QLDA.WebApi/Controllers/BanGiaoHoSoController.cs`

Chỉ cập nhật action `BanGiao` – truyền thêm `model.PhongBanNhanId` vào command:

```csharp
[HttpPut("{id}/ban-giao")]
[Consumes(MediaTypeNames.Application.Json)]
[ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
public async Task<ResultApi> BanGiao(Guid id, [FromBody] BanGiaoHoSoBanGiaoModel model) {
    var entity = await Mediator.Send(new BanGiaoHoSoBanGiaoCommand(id, model.NgayBanGiao, model.PhongBanNhanId));  // ← Thêm PhongBanNhanId

    await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
        GroupId = entity.Id.ToString(),
        Entities = model.GetDanhSachBienBanBanGiao(entity.Id)
    });

    return ResultApi.Ok(1);
}
```

---

## 3. Checklist hoàn thành

```
[ ] 1. Cập nhật BanGiaoHoSo entity – thêm PhongBanNhanId (long?)
[ ] 2. Chạy Migration AddBanGiaoHoSoPhongBanNhan (không drop database)
[ ] 3. Cập nhật BanGiaoHoSoBanGiaoDto – thêm PhongBanNhanId
[ ] 4. Cập nhật BanGiaoHoSoDto – thêm PhongBanNhanId, TenPhongBanNhan
[ ] 5. Cập nhật BanGiaoHoSoBanGiaoCommand – truyền và set PhongBanNhanId
[ ] 6. Cập nhật BanGiaoHoSoMappings.ToDto() – map PhongBanNhanId
[ ] 7. Cập nhật BanGiaoHoSoGetDanhSachQuery – join DanhMucDonVi lần 2 cho PhongBanNhanId
[ ] 8. Cập nhật BanGiaoHoSoBanGiaoModel (WebApi) – thêm PhongBanNhanId
[ ] 9. Cập nhật BanGiaoHoSoModel (WebApi) – thêm PhongBanNhanId, TenPhongBanNhan
[ ] 10. Cập nhật BanGiaoHoSoMappingConfiguration.ToModel() – map PhongBanNhanId
[ ] 11. Cập nhật BanGiaoHoSoController.BanGiao() – truyền PhongBanNhanId vào command
[ ] 12. Build + Test
```

---

## 4. Lưu ý kỹ thuật

- **`PhongBanNhanId`** chỉ có giá trị sau khi gọi endpoint `ban-giao` (khi `TrangThai = DaBanGiao`)
- **`TenPhongBanNhan`** trong response danh sách (`BanGiaoHoSoDto`): dùng LeftOuterJoin lần 2 trên cùng repository `DanhMucDonVi` – tốt hơn là join riêng; với record chưa bàn giao (`PhongBanNhanId = null`) sẽ trả `null`
- **`TenPhongBanNhan`** trong response chi-tiết (`BanGiaoHoSoModel`): **trả `null`** vì không có navigation property đến `DM_DONVI`; nếu client cần `TenPhongBanNhan` ở chi-tiết, phải query `/danh-sach` hoặc gọi API phòng ban riêng
- **⚠️ DM_DONVI** vẫn là bảng ngoại lệ – không tạo FK, không navigation property
- **Migration** chỉ add column `PhongBanNhanId bigint NULL` vào bảng `BanGiaoHoSo`
