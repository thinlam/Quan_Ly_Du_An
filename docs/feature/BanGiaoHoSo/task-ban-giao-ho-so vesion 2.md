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

> **✅ Updated (13/05/2026) – Version 2**
> - Thêm 3 trường bị thiếu vào entity và tất cả các layer: `DuAnId` (Guid? – FK → DuAn), `BuocId` (int? – FK → DuAnBuoc), `GhiChu` (string?)
> - Trả thêm `CreatedAt` trong danh sách
> - Validate Update và Delete: chỉ được thực hiện khi `TrangThai = KhoiTao`
> - Bỏ `UserId` khỏi entity – dùng `CreatedBy` từ base class `Entity<T>` (tự động set bởi EF interceptor)
> - Không tạo FK đến `UserMaster` (bảng đặc biệt, bị force-replace bởi DB khác) – dùng **LeftOuterJoin** để lấy `TenNguoiTao`
> - Controller Insert/Update nhận thẳng `InsertDto`/`UpdateModel`; endpoint `ban-giao` nhận `BanGiaoHoSoBanGiaoModel` (WebApi layer)

> **✅ Updated (13/05/2026) – Version 3**
> - `GetDanhSachQuery`: Đổi hoàn toàn sang **Method Syntax** – bỏ Query Syntax (`from/join/orderby/select`)
> - `GetDanhSachQuery`: Dùng `.WhereIf()` thay vì `if (...HasValue) { queryable = queryable.Where(...) }`
> - `BanGiaoHoSoModel` (WebApi): Chỉ là **response model** cho endpoint `chi-tiet` – bỏ `IHasKey`, `IMustHaveId`, `IMayHaveTepDinhKemModel`, `GetId()`; `Id` đổi từ `Guid?` → `Guid`
> - `BanGiaoHoSoMappingConfiguration` (WebApi): Bỏ `ToEntity`, `Update`, `GetDanhSachTepHSBanGiao(BanGiaoHoSoModel)` vì controller insert/update không nhận `BanGiaoHoSoModel` nữa → dead code; chỉ giữ `ToModel()` + `GetDanhSachBienBanBanGiao(BanGiaoHoSoBanGiaoModel)`

> **✅ Updated (13/05/2026) – Version 4**
> - `BanGiaoHoSoSearchDto`: Thêm `DuAnId (Guid?)` và `BuocId (int?)` làm filter params
> - `GetDanhSachQuery`: Thêm `.WhereIf(DuAnId)` và `.WhereIf(BuocId)` vào query pipeline

> **✅ Updated (13/05/2026) – Version 5**
> - `DanhMucDonVi` (bảng DB `DM_DONVI`) là bảng ngoại lệ như `UserMaster` – **không tạo FK, không navigation property**
> - Entity: Bỏ `public DanhMucDonVi? PhongBanChuTri { get; set; }` navigation
> - Configuration: Bỏ `builder.HasOne(e => e.PhongBanChuTri)...` FK config
> - `GetDanhSachQuery`: Dùng `LinqExtensions.LeftOuterJoin()` (đã implement sẵn) thay vì `GroupJoin + SelectMany` thủ công; join cả `UserMaster` lẫn `DanhMucDonVi`

> **✅ Updated (13/05/2026) – Version 6**
> - `Entity<T>.CreatedBy` là **`string`** (không phải `long`) → filter phải dùng `e.CreatedBy == _userProvider.Id.ToString()`
> - `LeftOuterJoin` với `UserMaster`: outer key là `string` (`e.CreatedBy`), inner key phải là `string` → `u => u.Id.ToString()` (không dùng `(long?)u.Id`)
> - `DanhMucDonVi.Id` là **`long`** → `IRepository<DanhMucDonVi, long>` (không phải `int`); `LeftOuterJoin` inner key: `d => (long?)d.Id`
> - `BanGiaoHoSoDto.CreatedAt` đổi thành **`DateTimeOffset?`** (khớp với `Entity<T>.CreatedAt` kiểu `DateTimeOffset`)
> - `PhongBanChuTriId` trong tất cả DTO/Model là **`long?`** (không phải `int?`)
> - `BanGiaoHoSoGetQuery`: Bỏ `.Include(e => e.PhongBanChuTri)` (navigation đã bị xóa từ Version 5)

> **✅ Updated (13/05/2026) – Version 7**
> - `NgayBanGiao` trong **response DTO/Model** (`BanGiaoHoSoDto`, `BanGiaoHoSoModel`) đổi thành **`DateOnly?`** – client chỉ cần hiển thị ngày, không cần giờ/timezone
> - Entity vẫn lưu `DateTimeOffset?` (UTC), conversion xảy ra tại mapping:
>   - `DateTimeOffset?` → `DateOnly?`: `entity.NgayBanGiao.HasValue ? DateOnly.FromDateTime(entity.NgayBanGiao.Value.LocalDateTime) : null`
> - Input (BanGiaoHoSoBanGiaoDto / BanGiaoHoSoBanGiaoModel) đã là `DateOnly?` từ trước – **không đổi**

> **✅ Updated (13/05/2026) – Version 8**
> - `PhongBanChuTriId` **KHÔNG cho UI truyền** – tự động lấy từ phòng ban của người tạo via `IUserProvider`
> - Giá trị: `_userProvider.Info?.PhongBanId ?? _userProvider.Info?.DonViId` (ưu tiên PhongBanId, fallback DonViId)
> - `BanGiaoHoSoInsertDto`: Bỏ `public long? PhongBanChuTriId { get; set; }` khỏi DTO
> - `BanGiaoHoSoMappings.ToEntity()`: Bỏ `PhongBanChuTriId = dto.PhongBanChuTriId`
> - `BanGiaoHoSoMappings.Update()`: Bỏ `entity.PhongBanChuTriId = dto.PhongBanChuTriId` (phòng ban không thay đổi sau khi tạo)
> - `BanGiaoHoSoInsertCommandHandler`: Inject `IUserProvider`, sau `dto.ToEntity()` gán `entity.PhongBanChuTriId = _userProvider.Info?.PhongBanId ?? _userProvider.Info?.DonViId`

> **✅ Updated (13/05/2026) – Version 9**
> - `ETrangThaiBanGiao`: Đổi giá trị enum từ `0/1` thành `1/2` – `KhoiTao = 1`, `DaBanGiao = 2`
> - Tất cả comment, điều kiện và mô tả liên quan đã được cập nhật

---

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm

| # | Mục | Ghi chú |
|---|-----|---------|
| 1 | Bổ sung **entity** `BanGiaoHoSo` mới | Quản lý bàn giao hồ sơ từ người dùng → Phòng HC-TH |
| 2 | Bổ sung **Enum** `ETrangThaiBanGiao` | 1: Khởi tạo, 2: Đã bàn giao |
| 3 | Bổ sung **6 API**: list (filter), insert, update, ban-giao (đổi trạng thái + biên bản), delete (có điều kiện) | Theo luồng CQRS hiện tại |
| 4 | Hỗ trợ **đính kèm file** (TepDinhKem) – 2 loại | **Tệp HS bàn giao** (`EGroupType.BanGiaoHoSo`) + **Biên bản bàn giao** (`EGroupType.BienBanBanGiao`) |

### 1.2 Model BanGiaoHoSo

| Field | Kiểu | Ghi chú |
|-------|------|---------|
| `Id` | `Guid` | Auto-generate sequential guid |
| `Ma` | `string` | Mã bản giao hồ sơ (unique) |
| `TenHoSo` | `string` | Tên hồ sơ |
| `DuAnId` | `Guid?` | FK → DuAn |
| `BuocId` | `int?` | FK → DuAnBuoc |
| `GhiChu` | `string?` | Ghi chú |
| `PhongBanChuTriId` | `long?` | Ref → DanhMucDonVi (⚠️ không FK) – **tự động set từ `_userProvider.Info?.PhongBanId ?? _userProvider.Info?.DonViId`; UI không truyền** |
| `TrangThai` | `int` (1/2) | 1: Khởi tạo, 2: Đã bàn giao → Enum `ETrangThaiBanGiao` |
| `NgayBanGiao` | `DateTimeOffset?` | Ngày bàn giao – set khi gọi endpoint `ban-giao` (Entity lưu UTC; DTO/Model trả về `DateOnly?`) |
| `IsDeleted` | `bit` | Soft delete flag |
| `CreatedAt`, `UpdatedAt` | `DateTime` | Audit fields |
| `CreatedBy` | `long?` | Người tạo – **từ base class `Entity<T>`**, tự động set bởi EF, không khai báo thủ công |

**Tệp đính kèm** (TepDinhKem):
- Lưu trữ qua bảng `TepDinhKem` (không có FK trực tiếp)
- `GroupId` = `BanGiaoHoSo.Id.ToString()`
- Truy vấn: `TepDinhKem.Where(f => f.GroupId == entity.Id.ToString())`

### 1.3 Enum ETrangThaiBanGiao

```csharp
public enum ETrangThaiBanGiao {
    KhoiTao = 1,           // Khởi tạo
    DaBanGiao = 2          // Đã bàn giao
}
```

### 1.4 Logic Danh Sách

**Chưa bàn giao:**
```
CreatedBy = IUserProvider.Id (người tạo - chính mình)
TrangThai = 1
```

**Đã bàn giao:**
```
CreatedBy = IUserProvider.Id (của chính mình)
TrangThai = 2
```

**Query params:**
```
TrangThai (UI truyền - 1 param duy nhất)
CreatedBy (KHÔNG cho UI truyền - luôn lấy từ IUserProvider)
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
- **Delete có điều kiện:** chỉ xóa được khi `TrangThai = 1`
- **Ban-giao endpoint:** đổi trạng thái 1 → 2, set `NgayBanGiao`, đính kèm biên bản bàn giao

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
    KhoiTao = 1,

    /// <summary>
    /// Đã bàn giao cho phòng HC-TH
    /// </summary>
    DaBanGiao = 2
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
    /// FK → DuAn
    /// </summary>
    public Guid? DuAnId { get; set; }

    /// <summary>
    /// FK → DuAnBuoc
    /// </summary>
    public int? BuocId { get; set; }

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? GhiChu { get; set; }

    /// <summary>
    /// FK → Phòng ban chủ trì (phòng HC-TH hoặc tương tự)
    /// </summary>
    public int? PhongBanChuTriId { get; set; }

    /// <summary>
    /// Trạng thái: 1 = Khởi tạo, 2 = Đã bàn giao
    /// </summary>
    public ETrangThaiBanGiao TrangThai { get; set; } = ETrangThaiBanGiao.KhoiTao;

    /// <summary>
    /// Ngày bàn giao – set khi gọi endpoint ban-giao
    /// </summary>
    public DateTimeOffset? NgayBanGiao { get; set; }

    // ⚠️ KHÔNG khai báo UserId – dùng CreatedBy từ base class Entity<Guid>
    // ⚠️ KHÔNG navigation đến UserMaster (bảng đặc biệt, không tạo FK)
    // ⚠️ KHÔNG navigation đến DanhMucDonVi/PhongBanChuTri (bảng DM_DONVI, không tạo FK)

    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DuAnBuoc? Buoc { get; set; }
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

        builder.Property(e => e.GhiChu)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(e => e.TrangThai)
            .HasConversion<int>();  // Lưu enum dưới dạng int

        builder.Property(e => e.NgayBanGiao)
            .IsRequired(false);

        // Index: Tìm kiếm nhanh theo CreatedBy + TrangThai
        builder.HasIndex(e => new { e.CreatedBy, e.TrangThai });

        // ⚠️ KHÔNG tạo FK → UserMaster (bảng bị force-replace bởi DB khác)
        // ⚠️ KHÔNG tạo FK → DanhMucDonVi/PhongBanChuTri (bảng DM_DONVI, bị force-replace)
        // TenNguoiTao và TenPhongBan lấy qua LeftOuterJoin trong GetDanhSachQuery

        // FK → DuAn
        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.SetNull);

        // FK → DuAnBuoc
        builder.HasOne(e => e.Buoc)
            .WithMany()
            .HasForeignKey(e => e.BuocId)
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
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GhiChu { get; set; }
    // ⚠️ PhongBanChuTriId KHÔNG khai báo ở đây – tự động set từ _userProvider.Info?.PhongBanId ?? _userProvider.Info?.DonViId trong InsertCommand
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
    /// <summary>Ngày bàn giao (DateOnly). Server tự convert sang DateTimeOffset UTC via DateOnlyExtensions.</summary>
    public DateOnly? NgayBanGiao { get; set; }
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
    public Guid? DuAnId { get; set; }
    public string? TenDuAn { get; set; }
    public int? BuocId { get; set; }
    public string? TenBuoc { get; set; }
    public string? GhiChu { get; set; }
    public long? PhongBanChuTriId { get; set; }
    public string? TenPhongBan { get; set; }
    public int TrangThai { get; set; }  // 1: Khởi tạo, 2: Đã bàn giao
    public string? TenTrangThai { get; set; }
    public DateOnly? NgayBanGiao { get; set; }  // DateOnly – entity lưu DateTimeOffset, convert khi map
    public string? TenNguoiTao { get; set; }  // từ UserMaster qua LeftOuterJoin (CreatedBy)
    public DateTimeOffset? CreatedAt { get; set; }  // DateTimeOffset – audit field
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
    public int? TrangThai { get; set; }  // 1: Khởi tạo, 2: Đã bàn giao
    public Guid? DuAnId { get; set; }   // Lọc theo dự án
    public int? BuocId { get; set; }    // Lọc theo bước (int? vì DuAnBuoc.Id là int)
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
        DuAnId = dto.DuAnId,
        BuocId = dto.BuocId,
        GhiChu = dto.GhiChu,
        // ⚠️ PhongBanChuTriId KHÔNG set ở đây – InsertCommandHandler set sau khi ToEntity() qua _userProvider.Info?.PhongBanId ?? _userProvider.Info?.DonViId
        TrangThai = ETrangThaiBanGiao.KhoiTao,
    };

    public static void Update(this BanGiaoHoSo entity, BanGiaoHoSoUpdateModel dto) {
        entity.Ma = dto.Ma;
        entity.TenHoSo = dto.TenHoSo;
        entity.DuAnId = dto.DuAnId;
        entity.BuocId = dto.BuocId;
        entity.GhiChu = dto.GhiChu;
        // ⚠️ PhongBanChuTriId KHÔNG cập nhật khi update – phòng ban cố định theo người tạo
    }

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
        TenPhongBan = entity.PhongBanChuTri?.Ten,
        TrangThai = (int)entity.TrangThai,
        TenTrangThai = GetTrangThaiText(entity.TrangThai),
        NgayBanGiao = entity.NgayBanGiao.HasValue ? DateOnly.FromDateTime(entity.NgayBanGiao.Value.LocalDateTime) : null,
        // TenNguoiTao: không map ở đây – GetDanhSachQuery dùng LeftOuterJoin để lấy
        DanhSachTepHSBanGiao = tepHSBanGiao?.Select(f => f.ToDto()).ToList(),
        DanhSachBienBanBanGiao = bienBanBanGiao?.Select(f => f.ToDto()).ToList()
    };

    /// <summary>Tệp HS bàn giao – extension trên InsertDto, gắn khi insert/update</summary>
    public static List<TepDinhKem> GetDanhSachTepHSBanGiao(this BanGiaoHoSoInsertDto dto, Guid groupId) {
        if (dto.DanhSachTepDinhKem?.Any() != true) return [];
        return dto.DanhSachTepDinhKem
            .Select(f => new TepDinhKem {
                Id = f.Id ?? GuidExtensions.GetSequentialGuidId(),
                ParentId = f.ParentId,
                GroupId = groupId.ToString(),
                GroupType = EGroupType.BanGiaoHoSo.ToString(),
                Type = f.Type,
                FileName = f.FileName,
                OriginalName = f.OriginalName,
                Path = f.Path,
                Size = f.Size
            }).ToList();
    }

    /// <summary>Biên bản bàn giao – extension trên BanGiaoDto, gắn khi thực hiện bàn giao</summary>
    public static List<TepDinhKem> GetDanhSachBienBanBanGiao(this BanGiaoHoSoBanGiaoDto dto, Guid groupId) {
        if (dto.DanhSachBienBan?.Any() != true) return [];
        return dto.DanhSachBienBan
            .Select(f => new TepDinhKem {
                Id = f.Id ?? GuidExtensions.GetSequentialGuidId(),
                ParentId = f.ParentId,
                GroupId = groupId.ToString(),
                GroupType = EGroupType.BienBanBanGiao.ToString(),
                Type = f.Type,
                FileName = f.FileName,
                OriginalName = f.OriginalName,
                Path = f.Path,
                Size = f.Size
            }).ToList();
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
        // PhongBanChuTriId = phòng ban của người tạo (ưu tiên PhongBanId, fallback DonViId)
        entity.PhongBanChuTriId = _userProvider.Info?.PhongBanId ?? _userProvider.Info?.DonViId;
        // CreatedBy được tự động set bởi EF interceptor từ JWT token – không cần gán thủ công

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
using QLDA.Domain.Enums;

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

        // Chỉ cho phép cập nhật khi TrangThai = 1 (Khởi tạo)
        if (entity.TrangThai != ETrangThaiBanGiao.KhoiTao) {
            throw new InvalidOperationException("Chỉ có thể cập nhật bản giao hồ sơ ở trạng thái 'Khởi tạo'");
        }

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
public record BanGiaoHoSoBanGiaoCommand(Guid Id, DateOnly? NgayBanGiao) : IRequest<BanGiaoHoSo>;

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

        // Chỉ cho phép xóa khi TrangThai = 1 (Khởi tạo)
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
            // ⚠️ Không Include PhongBanChuTri (DM_DONVI, không FK) và không Include User (UserMaster, không FK)
            .Include(e => e.DuAn)
            .Include(e => e.Buoc)
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
    private readonly IRepository<BanGiaoHoSo, Guid> _banGiaoRepository;
    private readonly IRepository<TepDinhKem, Guid> _tepDinhKemRepository;
    private readonly IRepository<UserMaster, long> _userMasterRepository;
    private readonly IRepository<DanhMucDonVi, int> _danhMucDonViRepository;  // ⚠️ DM_DONVI – không FK
    private readonly IUserProvider _userProvider;

    public BanGiaoHoSoGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _banGiaoRepository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _tepDinhKemRepository = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _userMasterRepository = serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
        _danhMucDonViRepository = serviceProvider.GetRequiredService<IRepository<DanhMucDonVi, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    }

    public async Task<PaginatedList<BanGiaoHoSoDto>> Handle(BanGiaoHoSoGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        // LeftOuterJoin UserMaster và DanhMucDonVi (không FK – bảng đặc biệt)
        var users = _userMasterRepository.GetQueryableSet().AsNoTracking();
        var donVis = _danhMucDonViRepository.GetQueryableSet().AsNoTracking();

        var queryable = _banGiaoRepository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => e.CreatedBy == _userProvider.Id.ToString())  // CreatedBy là string trong Entity<T>
            .Include(e => e.DuAn)
            .Include(e => e.Buoc)
            .WhereIf(request.SearchDto.TrangThai.HasValue, e => (int)e.TrangThai == request.SearchDto.TrangThai!.Value)
            .WhereIf(request.SearchDto.DuAnId.HasValue, e => e.DuAnId == request.SearchDto.DuAnId!.Value)
            .WhereIf(request.SearchDto.BuocId.HasValue, e => e.BuocId == request.SearchDto.BuocId!.Value)
            .LeftOuterJoin(users, e => e.CreatedBy, u => u.Id.ToString(), (e, user) => new { e, user })  // cả 2 key là string
            .LeftOuterJoin(donVis, x => x.e.PhongBanChuTriId, d => (long?)d.Id, (x, donVi) => new { x.e, x.user, donVi })
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
                TenPhongBan = x.donVi != null ? x.donVi.TenDonVi : null,
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

// Response model cho endpoint chi-tiet – không implement request interfaces
public class BanGiaoHoSoModel {
    public Guid Id { get; set; }
    public string? Ma { get; set; }
    public string? TenHoSo { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GhiChu { get; set; }
    public long? PhongBanChuTriId { get; set; }
    public int TrangThai { get; set; }  // 1: Khởi tạo, 2: Đã bàn giao
    public DateOnly? NgayBanGiao { get; set; }  // DateOnly – entity lưu DateTimeOffset, convert khi map
    // Tệp HS bàn giao (EGroupType.BanGiaoHoSo)
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
    // Biên bản bàn giao (EGroupType.BienBanBanGiao)
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
    /// <summary>Ngày bàn giao (DateOnly), nếu null sẽ dùng ngày hiện tại. Server tự quy đổi sang UTC.</summary>
    public DateOnly? NgayBanGiao { get; set; }
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
        DuAnId = entity.DuAnId,
        BuocId = entity.BuocId,
        GhiChu = entity.GhiChu,
        PhongBanChuTriId = entity.PhongBanChuTriId,
        TrangThai = (int)entity.TrangThai,
        NgayBanGiao = entity.NgayBanGiao.HasValue ? DateOnly.FromDateTime(entity.NgayBanGiao.Value.LocalDateTime) : null,
        DanhSachTepDinhKem = tepHSBanGiao?.Select(f => f.ToModel()).ToList(),
        DanhSachBienBanBanGiao = bienBanBanGiao?.Select(f => f.ToModel()).ToList()
    };

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
    public async Task<ResultApi> Insert([FromBody] BanGiaoHoSoInsertDto dto) {
        // Nhận thẳng InsertDto – không qua BanGiaoHoSoModel trung gian
        var entity = await Mediator.Send(new BanGiaoHoSoInsertCommand(dto));

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = dto.GetDanhSachTepHSBanGiao(entity.Id)
        });

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Update([FromBody] BanGiaoHoSoUpdateModel dto) {
        // Nhận thẳng UpdateModel (Id + fields) – không qua BanGiaoHoSoModel trung gian
        var entity = await Mediator.Send(new BanGiaoHoSoUpdateCommand(dto));

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = dto.GetDanhSachTepHSBanGiao(entity.Id)
        });

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("{id}/ban-giao")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> BanGiao(Guid id, [FromBody] BanGiaoHoSoBanGiaoModel model) {
        var entity = await Mediator.Send(new BanGiaoHoSoBanGiaoCommand(id, model.NgayBanGiao));

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = model.GetDanhSachBienBanBanGiao(entity.Id)
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
[x] 4. Chạy Migration (không drop database)
[x] 5. Tạo BanGiaoHoSo DTOs + Mappings
[x] 6. Tạo BanGiaoHoSoInsertCommand / UpdateCommand / BanGiaoCommand / DeleteCommand
[x] 7. Tạo BanGiaoHoSoGetQuery / GetDanhSachQuery
[x] 8. IUserProvider đã đăng ký sẵn trong BuildingBlocks DI - inject trong handler
[x] 9. WebApplicationExtensions.cs - KHÔNG cần sửa (IUserProvider đã có sẵn)
[x] 10. Tạo BanGiaoHoSo Model + BanGiaoHoSoBanGiaoModel + Mapping + Controller
[x] 11. Thêm EGroupType.BanGiaoHoSo và EGroupType.BienBanBanGiao vào enum
[x] 12. Build + Test
```

---

## 6. Lưu ý kỹ thuật

- **⚠️ KHÔNG chạy `drop-database`** – chỉ chạy `migrations add` và `database update`
- **CreatedBy từ base class:** Không khai báo `UserId` thủ công – `Entity<T>` đã có `CreatedBy` (long?) tự động set bởi EF interceptor từ JWT
- **⚠️ Không FK đến UserMaster:** Bảng bị force-replace bởi DB khác → dùng **LeftOuterJoin** để lấy `TenNguoiTao`
- **⚠️ Không FK đến DanhMucDonVi (`DM_DONVI`):** Bảng bị force-replace tương tự UserMaster → dùng **LeftOuterJoin** để lấy `TenPhongBan`; không khai báo navigation property trong entity
- **LeftOuterJoin:** Dùng `LinqExtensions.LeftOuterJoin()` đã implement sẵn, KHÔNG dùng `GroupJoin + SelectMany` thủ công
- **Delete có điều kiện:** Chỉ xóa được khi `TrangThai = 1` (Khởi tạo), throw exception nếu vi phạm
- **Update có điều kiện:** Chỉ cập nhật được khi `TrangThai = 1` (Khởi tạo), throw exception nếu vi phạm
- **Controller nhận DTO trực tiếp:** Insert → `BanGiaoHoSoInsertDto`, Update → `BanGiaoHoSoUpdateModel`, BanGiao → `BanGiaoHoSoBanGiaoDto`
- **TepDinhKem helpers** (`GetDanhSachTepHSBanGiao`, `GetDanhSachBienBanBanGiao`): extension method trên DTO trong `BanGiaoHoSoMappings.cs`
- **Ban-giao endpoint:** Đổi trạng thái 0→1, set `NgayBanGiao`, lưu biên bản bàn giao
- **Filter:** Chỉ theo `TrangThai` (1 param duy nhất từ UI) - không có GlobalFilter
- **Migration tên:** `AddBanGiaoHoSo` – chạy từ folder `QLDA.Migrator`
- **Index:** Tạo index trên `(CreatedBy, TrangThai)` để tối ưu query danh sách
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
| `PUT` | `/api/ban-giao-ho-so/{id}/ban-giao` | Thực hiện bàn giao: đổi trạng thái 1→2, set ngày, đính kèm biên bản | `int` (1) |
| `DELETE` | `/api/ban-giao-ho-so/{id}/xoa-tam` | Xóa (chỉ khi TrangThai = 1) | `int` (1) |

---

### 🔒 Permission Logic

| Thao tác | Điều kiện | UserId |
|---------|-----------|--------|
| View list | Danh sách của chính mình | `userInfo.UserId` |
| Create | Tạo bản giao mới | Auto set từ Auth |
| Update | Sửa thông tin + tệp HS | Chỉ khi TrangThai = 1 |
| Ban-giao | 1→2, set ngày, đính kèm biên bản | Free |
| Delete | Xóa bản giao | Chỉ khi TrangThai = 1 |
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
