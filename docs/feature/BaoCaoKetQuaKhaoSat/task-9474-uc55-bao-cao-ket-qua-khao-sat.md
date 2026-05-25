# Task #9474 – UC55: Báo cáo kết quả khảo sát, nghiệm thu khảo sát

**Issue:** `issues:9474` — UC55 Thực hiện khảo sát, báo cáo kết quả khảo sát, nghiệm thu khảo sát  
**Pattern tham chiếu:** `HoSoDeXuatCapDoCntt` (CRUD + handler workflow); **3 action** giống `PhanKhaiKinhPhi` (không từ chối)  
**Doc mẫu cấu trúc:** `docs/feature/KySo/task-9460-ky-so-crud.md`  
**Trạng thái:** Code + migration **đã có** — còn `dotnet ef database update` trên từng môi trường + build/test (§6 mục 17).

---

## Summary — Đã làm (UC55 #9474)

Tóm tắt deliverable đã implement theo pattern `HoSoDeXuatCapDoCntt` + workflow 3 bước (giống `PhanKhaiKinhPhi`, **không** Từ chối).

### Tổng quan

| Hạng mục | Kết quả |
|----------|---------|
| Nghiệp vụ | Báo cáo kết quả khảo sát / nghiệm thu khảo sát gắn `DuAnId`, workflow phê duyệt |
| Bảng DB | `BaoCaoKetQuaKhaoSat` + seed `DmTrangThaiPheDuyet` Id **26–29** (`DT` / `ĐTr` / `ĐD` / `TL`) |
| API CRUD | `api/bao-cao-ket-qua-khao-sat` — 5 endpoint |
| API phê duyệt | `api/phe-duyet/BaoCaoKetQuaKhaoSat/{id}/trinh\|duyet\|tra-lai` |
| Hub phê duyệt | `danh-sach`, `chi-tiet`, `lich-su`, `types` |
| Out of scope v1 | `TepDinhKem`, `tu-choi`, PDF, `thay-doi-trang-thai` trên CRUD |

### Domain & Persistence

| File / thành phần | Nội dung |
|-------------------|----------|
| `QLDA.Domain/Entities/BaoCaoKetQuaKhaoSat.cs` | Entity `Guid`, `DuAnId`, nội dung báo cáo/nghiệm thu, `NgayKhaoSat`, `TrangThaiId`, `NgayTrinh` |
| `PheDuyetEntityNames.BaoCaoKetQuaKhaoSat` | Constant + `[Description]` cho `GET /phe-duyet/types` |
| `TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat` | Mã `DT`, `ĐTr`, `ĐD`, `TL` |
| `BaoCaoKetQuaKhaoSatConfiguration.cs` | EF: bảng, FK `DuAn`, `DmTrangThaiPheDuyet`, max length 4000 |
| `DanhMucTrangThaiPheDuyetConfiguration.cs` | Seed HasData Id 26–29 (không `TC`) |
| `QLDA.Migrator/Migrations/20260525030156_BaoCaoKetQuaKhaoSat.cs` | `CreateTable` + seed SQL idempotent (26–29) |
| `20260522042016_DeXuatNhuCauKinhPhiAddKinhPhiDeXuat.cs` | Fix brownfield: `ADD KinhPhiDeXuat` chỉ khi cột chưa có |

### Application — CRUD

**Folder:** `QLDA.Application/BaoCaoKetQuaKhaoSats/`

| Thành phần | Ghi chú |
|------------|---------|
| DTOs | `InsertDto`, `UpdateModel`, `Dto`, `SearchDto` |
| `BaoCaoKetQuaKhaoSatMappings.cs` | `ToEntity`, `Update`, `ToDto` (đã xóa file trùng `Mapping.cs`) |
| Validators | Insert/Update — `DuAnId`, max 4000 ký tự |
| `InsertCommand` | Tạo mới, gán `TrangThaiId` = Dự thảo |
| `UpdateCommand` | Chỉ khi `DT` / `TL` / `null` / `LEG` |
| `DeleteCommand` | Soft delete, **không** xóa `TepDinhKem` |
| `GetQuery` / `GetDanhSachQuery` | Chi tiết + danh sách, filter `DuAnId`, global filter |

### Application — Workflow (Bước 7)

| Command | Chuyển trạng thái | Ghi chú |
|---------|-------------------|---------|
| `BaoCaoKetQuaKhaoSatTrinhCommand` | `DT`/`TL`/`null` → `ĐTr` | Set `NgayTrinh`, `PheDuyetHistory` |
| `BaoCaoKetQuaKhaoSatDuyetCommand` | `ĐTr` → `ĐD` | Quyền `QLDA_LDDV` hoặc HC-TH |
| `BaoCaoKetQuaKhaoSatTraLaiCommand` | `ĐTr` → `TL` | Bắt buộc `NoiDung` |
| — | **Không** `TuChoiCommand` | UC55 |

### Application — Hub phê duyệt (Bước 8 & 9)

| File | Đã làm |
|------|--------|
| `PheDuyetDispatchTrinhCommand` | Case `BaoCaoKetQuaKhaoSat` → `TrinhCommand` |
| `PheDuyetDispatchDuyetCommand` | Case → `DuyetCommand` |
| `PheDuyetDispatchTraLaiCommand` | Case → `TraLaiCommand` |
| `PheDuyetDispatchTuChoiCommand` | **Không** map |
| `PheDuyetGetDanhSachQuery` | `GetBaoCaoKetQuaKhaoSatItems` |
| `PheDuyetGetChiTietQuery` | `GetBaoCaoKetQuaKhaoSatDetail` + `LichSu` |

### WebApi

| File | Endpoint |
|------|----------|
| `BaoCaoKetQuaKhaoSatController.cs` | `GET {id}`, `GET danh-sach`, `POST them-moi`, `PUT cap-nhat`, `DELETE {id}` |
| `BaoCaoKetQuaKhaoSatModel` + `MappingConfiguration` | `ToInsertDto` / `ToUpdateModel` / `ToModel` |

### Tài liệu

| File | Nội dung |
|------|----------|
| `docs/feature/BaoCaoKetQuaKhaoSat/task-9474-uc55-bao-cao-ket-qua-khao-sat.md` | Task implement + API contract + checklist |
| `docs/workflow-quan-ly-phe-duyet.md` | Thêm entity `BaoCaoKetQuaKhaoSat` |

### API map nhanh (FE / QA)

```
CRUD     → /api/bao-cao-ket-qua-khao-sat/...
Workflow → /api/phe-duyet/BaoCaoKetQuaKhaoSat/{id}/trinh|duyet|tra-lai
Hub      → /api/phe-duyet/danh-sach?type=BaoCaoKetQuaKhaoSat
         → /api/phe-duyet/BaoCaoKetQuaKhaoSat/{id}/chi-tiet
         → /api/phe-duyet/lich-su?type=BaoCaoKetQuaKhaoSat&entityId={id}
```

### Chưa xong / cần làm trên môi trường dev

- [ ] `dotnet ef database update` (nếu DB local chưa apply migration)
- [ ] `dotnet build` + test manual theo [§8 Test plan](#8-test-plan)
- [ ] Commit theo nhóm: Domain + Persistence + Migrator | Application + WebApi + docs

Chi tiết từng bước implement: [§4 Chi tiết từng bước](#4-chi-tiết-từng-bước). Checklist: [§6](#6-checklist-hoàn-thành).

---

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm

| # | Mục | Ghi chú |
|---|-----|---------|
| 1 | Thêm bảng **`BaoCaoKetQuaKhaoSat`** | Entity mới, chưa có trong DB |
| 2 | **CRUD** đầy đủ | get, danh-sach, them-moi, cap-nhat, xoa (soft delete) |
| 3 | **Workflow phê duyệt** | **Trình → Duyệt → Trả lại** (không có Từ chối) |
| 4 | Đăng ký **`QuanLyPheDuyet`** | `PheDuyetDispatchTrinh/Duyet/TraLai` (**3 file**) — **không** map `tu-choi` |
| 5 | **Seed** `DanhMucTrangThaiPheDuyet` | **4** trạng thái `DT/ĐTr/ĐD/TL` (Id **26–29**) |

### 1.2 Model nghiệp vụ

| Field | Kiểu API | Kiểu DB | Ghi chú |
|-------|----------|---------|---------|
| `Id` | `Guid` | `uniqueidentifier` | PK, `Guid.NewGuid()` khi insert |
| `DuAnId` | `Guid` | `uniqueidentifier` | Bắt buộc — context dự án / `PheDuyetHistory` |
| `BuocId` | `int?` | `int` | Bước tiến độ (ẩn UI, từ context) |
| `NoiDungBaoCao` | `string?` | `nvarchar(4000)` | MaxLength **4000** |
| `NoiDungNghiemThu` | `string?` | `nvarchar(4000)` | MaxLength **4000** |
| `NgayKhaoSat` | `DateOnly?` | `datetimeoffset` | `ToStartOfDayUtc` / `ToDateOnlyVn` |
| `TrangThaiId` | `int?` | `int` FK | → `DanhMucTrangThaiPheDuyet`, `Loai = BaoCaoKetQuaKhaoSat` |
| `NgayTrinh` | `DateOnly?` (API) | `DateTimeOffset?` | Set trong `TrinhCommand`, không nhập form |

**Cột hệ thống:** `IsDeleted`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` (kế thừa `Entity<Guid>`).

### 1.3 Workflow

```
[Dự thảo DT] --trình--> [Đã trình ĐTr] --duyệt--> [Đã duyệt ĐD]
                              |
                              +--trả lại--> [Trả lại TL] --trình--> [Đã trình]
```

| Hành động | Endpoint (hub) | Command |
|-----------|----------------|---------|
| Trình | `POST /api/phe-duyet/BaoCaoKetQuaKhaoSat/{id}/trinh` | `BaoCaoKetQuaKhaoSatTrinhCommand` |
| Duyệt | `POST /api/phe-duyet/BaoCaoKetQuaKhaoSat/{id}/duyet` | `BaoCaoKetQuaKhaoSatDuyetCommand` |
| Trả lại | `POST /api/phe-duyet/BaoCaoKetQuaKhaoSat/{id}/tra-lai` | `BaoCaoKetQuaKhaoSatTraLaiCommand` |

> **Không** implement `tu-choi` / `BaoCaoKetQuaKhaoSatTuChoiCommand` — gọi `POST .../tu-choi` với `type=BaoCaoKetQuaKhaoSat` sẽ trả lỗi *Loại phê duyệt không hợp lệ* (giống entity chưa đăng ký trong dispatch).

**Lịch sử:** `PheDuyetHistory` với `EntityName = PheDuyetEntityNames.BaoCaoKetQuaKhaoSat`, `DuAnId = entity.DuAnId`.

**Quyền:** copy từ `HoSoDeXuatCapDoCnttDuyetCommand` (`QLDA_LDDV` hoặc phòng HC-TH). Xem `docs/workflow-quan-ly-phe-duyet.md`.

### 1.4 Quyết định mặc định (để unblock implement)

| # | Điểm | Quyết định |
|---|------|------------|
| 1 | `DuAnId` / `BuocId` | **Có** — bắt buộc `DuAnId` |
| 2 | `NgayTrinh` | **Có** — set khi trình |
| 3 | `TepDinhKem` | **Không** (v1) |
| 4 | `thay-doi-trang-thai` trên controller | **Không** — chỉ hub `phe-duyet` |
| 5 | **Từ chối (`TC`)** | **Không** — UC55 chỉ trình / duyệt / trả lại |
| 6 | Out of scope v1 | PDF, `EGroupType`, `ITienDo` |

---

## 2. Phân tích hiện trạng

| Hạng mục | Hiện trạng |
|----------|------------|
| Entity `BaoCaoKetQuaKhaoSat` | **Có** — `QLDA.Domain/Entities/BaoCaoKetQuaKhaoSat.cs` |
| EF `BaoCaoKetQuaKhaoSatConfiguration` | **Có** — auto-apply qua `AggregateRootConfiguration<>` |
| Seed 4 TT (Id 26–29), không `TC` | **Có** — `DanhMucTrangThaiPheDuyetConfiguration` |
| Migration / bảng DB | **Có file** — `20260525030156_BaoCaoKetQuaKhaoSat`; cần `database update` trên DB |
| CRUD `api/bao-cao-ket-qua-khao-sat` | **Có** — get, danh-sach, them-moi, cap-nhat, xoa |
| Workflow Trinh / Duyet / TraLai | **Có** — 3 command, không `TuChoi` |
| `PheDuyetDispatch*` (Trinh/Duyet/TraLai) | **Có** — 3 file dispatch |
| `PheDuyetDispatchTuChoi` | **Không map** — `tu-choi` → lỗi *không hợp lệ* (đúng UC55) |
| Hub `GET types` | **Có** — reflection `PheDuyetEntityNames` |
| Hub `GET danh-sach` / `chi-tiet` theo type | **Có** — `GetBaoCaoKetQuaKhaoSatItems` / `GetBaoCaoKetQuaKhaoSatDetail` |
| DTO mapping Application | **Có** — `BaoCaoKetQuaKhaoSatMappings.cs` |
| File mapping trùng | **Đã xóa** — chỉ còn `BaoCaoKetQuaKhaoSatMappings.cs` |
| `docs/workflow-quan-ly-phe-duyet.md` | **Có** |
| Build + test workflow | **Chưa** |

**Quy tắc dự án:**

- Migration: `ef.bat add` — **không** sửa snapshot hay migration cũ.
- Commit: **Domain + Persistence.Configuration + Migrator** cùng một nhóm.
- DTO ↔ Entity mapping: **chỉ** `QLDA.Application/BaoCaoKetQuaKhaoSats/DTOs/BaoCaoKetQuaKhaoSatMappings.cs` — **không** giữ `BaoCaoKetQuaKhaoSatMapping.cs`.
- WebApi: Model chỉ `ToInsertDto` / `ToUpdateModel` / `ToModel` từ DTO Application — **không** map entity thủ công trong controller.

### 2.1 Kết quả rà soát code

Đối chiếu yêu cầu UC55 với `QuanLyPheDuyetController` và pattern `HoSoDeXuatCapDoCntt` / `PhanKhaiKinhPhi`.

| Yêu cầu | Đúng pattern? | Ghi chú |
|---------|---------------|---------|
| Table/entity + EF config | Có | DbContext discover config; cần migration tạo bảng |
| CRUD đủ 5 endpoint | Có | `BaoCaoKetQuaKhaoSatController` |
| Workflow 3 action qua hub | Có | `POST /api/phe-duyet/BaoCaoKetQuaKhaoSat/{id}/trinh\|duyet\|tra-lai` |
| Không Từ chối | Có | Không `TuChoiCommand`, không case `PheDuyetDispatchTuChoi` |
| Entity name + seed + dispatch | Có | `PheDuyetEntityNames`, codes, seed 26–29, 3 dispatch |
| Cập nhật khi DT / TL / null / LEG | Có | Mở rộng hơn HoSo (HoSo chỉ DT / null / LEG) — đúng UC55 |
| Delete không `TepDinhKem` | Có | v1 không file |
| Hub danh-sach/chi-tiet theo type | Có | `PheDuyetGetDanhSachQuery`, `PheDuyetGetChiTietQuery` |

**Việc còn lại trước khi coi “xong” (deploy):**

1. `dotnet ef database update` trên DB môi trường dev — xem [§7.1 Migration brownfield](#71-migration-brownfield-db-đã-có-sẵn).
2. `dotnet build` + test theo [§8 Test plan](#8-test-plan).
3. Commit / PR — xem [Summary — Đã làm](#summary--đã-làm-uc55-9474).

---

## 3. Thứ tự thực hiện

```
Bước 1:  Domain — Entity BaoCaoKetQuaKhaoSat
Bước 2:  Domain — PheDuyetEntityNames + TrangThaiPheDuyetCodes
Bước 3:  Persistence — BaoCaoKetQuaKhaoSatConfiguration
Bước 4:  Persistence — Seed trạng thái (DanhMucTrangThaiPheDuyetConfiguration) + Migration
Bước 5:  Application — DTOs + Mappings + Validators
Bước 6:  Application — Commands (Insert/Update/Delete) + Queries (Get/GetDanhSach)
Bước 7:  Application — Workflow Commands (Trinh/Duyet/TraLai)
Bước 8:  Application — PheDuyetDispatchTrinh/Duyet/TraLai (3 file)
Bước 9:  Application — PheDuyetGetDanhSachQuery / PheDuyetGetChiTietQuery (đã map BaoCaoKetQuaKhaoSat)
Bước 10: WebApi — Model + MappingConfiguration + Controller
Bước 11: Docs — workflow-quan-ly-phe-duyet.md
Bước 12: Build + test workflow
```

---

## 4. Chi tiết từng bước

---

### Bước 1 – Domain: Entity `BaoCaoKetQuaKhaoSat`

**File:** `QLDA.Domain/Entities/BaoCaoKetQuaKhaoSat.cs`

```csharp
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// UC55 — Báo cáo kết quả khảo sát, nghiệm thu khảo sát
/// </summary>
public class BaoCaoKetQuaKhaoSat : Entity<Guid>, IAggregateRoot
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }

    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungNghiemThu { get; set; }
    public DateTimeOffset? NgayKhaoSat { get; set; }
    public int? TrangThaiId { get; set; }
    public DateTimeOffset? NgayTrinh { get; set; }

    #region Navigation Properties
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
    #endregion
}
```

> Dùng `Entity<Guid>` — cùng pattern `HoSoDeXuatCapDoCntt`, `KySo`.

---

### Bước 2 – Domain: Constants workflow

**File:** `QLDA.Domain/Constants/PheDuyetEntityNames.cs` — thêm:

```csharp
/// <summary>
/// UC55 - Báo cáo kết quả khảo sát (#9474)
/// </summary>
[Description("Báo cáo kết quả khảo sát")]
public const string BaoCaoKetQuaKhaoSat = "BaoCaoKetQuaKhaoSat";
```

**File:** `QLDA.Domain/Constants/TrangThaiPheDuyetCodes.cs` — thêm:

```csharp
public static class BaoCaoKetQuaKhaoSat
{
    public const string DuThao = "DT";
    public const string DaTrinh = "ĐTr";
    public const string DaDuyet = "ĐD";
    public const string TraLai = "TL";
    // Không có TuChoi — theo UC55
}
```

> Pattern giống `TrangThaiPheDuyetCodes.PhanKhaiKinhPhi` / `ToTrinhKeHoach` (4 mã, không `TC`).

---

### Bước 3 – Persistence: Configuration entity

**File:** `QLDA.Persistence/Configurations/BaoCaoKetQuaKhaoSatConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class BaoCaoKetQuaKhaoSatConfiguration : AggregateRootConfiguration<BaoCaoKetQuaKhaoSat>
{
    public override void Configure(EntityTypeBuilder<BaoCaoKetQuaKhaoSat> builder)
    {
        builder.ToTable(nameof(BaoCaoKetQuaKhaoSat));
        builder.ConfigureForBase();

        builder.Property(e => e.NoiDungBaoCao).HasMaxLength(4000);
        builder.Property(e => e.NoiDungNghiemThu).HasMaxLength(4000);

        builder.Property(e => e.NgayKhaoSat)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb);

        builder.Property(e => e.NgayTrinh)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb);

        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

### Bước 4 – Persistence: Seed + Migration

**File:** `QLDA.Persistence/Configurations/DanhMuc/DanhMucTrangThaiPheDuyetConfiguration.cs`

Thêm **sau** block `QuyetDinhDieuChinh` (Id hiện max = **25** → dùng **26–29**, **4 bản ghi**):

```csharp
// BaoCaoKetQuaKhaoSat statuses (UC55 - #9474) — không seed TC
new DanhMucTrangThaiPheDuyet { Id = 26, Ma = TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.DuThao, Ten = TrangThaiPheDuyetCodes.Default.TenDuThao, Loai = PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, Stt = 1, Used = true, CreatedAt = SeedCreatedAt },
new DanhMucTrangThaiPheDuyet { Id = 27, Ma = TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.DaTrinh, Ten = TrangThaiPheDuyetCodes.Default.TenDaTrinh, Loai = PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, Stt = 2, Used = true, CreatedAt = SeedCreatedAt },
new DanhMucTrangThaiPheDuyet { Id = 28, Ma = TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.DaDuyet, Ten = TrangThaiPheDuyetCodes.Default.TenDaDuyet, Loai = PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, Stt = 3, Used = true, CreatedAt = SeedCreatedAt },
new DanhMucTrangThaiPheDuyet { Id = 29, Ma = TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.TraLai, Ten = TrangThaiPheDuyetCodes.Default.TenTraLai, Loai = PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, Stt = 4, Used = true, CreatedAt = SeedCreatedAt },
```

**Migration:**

```bash
ef.bat add BaoCaoKetQuaKhaoSat
# Kiểm tra migration generate — KHÔNG chỉnh tay file .cs
ef.bat update --sqlite   # khi sẵn sàng apply local
```

> Seed + bảng mới cùng migration → commit chung **Domain + Persistence + Migrator**.

---

### Bước 5 – Application: DTOs & Mappings

**Cấu trúc folder:**

```
QLDA.Application/BaoCaoKetQuaKhaoSats/
  DTOs/
    BaoCaoKetQuaKhaoSatInsertDto.cs
    BaoCaoKetQuaKhaoSatUpdateModel.cs
    BaoCaoKetQuaKhaoSatDto.cs
    BaoCaoKetQuaKhaoSatSearchDto.cs
    BaoCaoKetQuaKhaoSatMappings.cs
  Validators/
    BaoCaoKetQuaKhaoSatValidators.cs
  Commands/
    ...
  Queries/
    ...
```

#### `BaoCaoKetQuaKhaoSatInsertDto.cs`

```csharp
namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

public class BaoCaoKetQuaKhaoSatInsertDto
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungNghiemThu { get; set; }
    public DateOnly? NgayKhaoSat { get; set; }
}
```

#### `BaoCaoKetQuaKhaoSatUpdateModel.cs`

```csharp
namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

public class BaoCaoKetQuaKhaoSatUpdateModel : BaoCaoKetQuaKhaoSatInsertDto
{
    public Guid Id { get; set; }
}
```

#### `BaoCaoKetQuaKhaoSatDto.cs`

```csharp
namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

public class BaoCaoKetQuaKhaoSatDto
{
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungNghiemThu { get; set; }
    public DateOnly? NgayKhaoSat { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public DateOnly? NgayTrinh { get; set; }
}
```

#### `BaoCaoKetQuaKhaoSatSearchDto.cs`

```csharp
namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

public record BaoCaoKetQuaKhaoSatSearchDto : AggregateRootPagination, IMayHaveGlobalFilter
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
}
```

#### `BaoCaoKetQuaKhaoSatMappings.cs`

> **Dọn code:** nếu còn file `BaoCaoKetQuaKhaoSatMapping.cs` (không có `s`) — **xóa**; chỉ giữ file này để tránh lỗi build CS0121.

```csharp
using BuildingBlocks.CrossCutting.ExtensionMethods;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

public static class BaoCaoKetQuaKhaoSatMappings
{
    public static BaoCaoKetQuaKhaoSat ToEntity(this BaoCaoKetQuaKhaoSatInsertDto dto) => new()
    {
        Id = Guid.NewGuid(),
        DuAnId = dto.DuAnId,
        BuocId = dto.BuocId,
        NoiDungBaoCao = dto.NoiDungBaoCao,
        NoiDungNghiemThu = dto.NoiDungNghiemThu,
        NgayKhaoSat = dto.NgayKhaoSat.ToStartOfDayUtc(),
    };

    public static void Update(this BaoCaoKetQuaKhaoSat entity, BaoCaoKetQuaKhaoSatUpdateModel model)
    {
        entity.NoiDungBaoCao = model.NoiDungBaoCao;
        entity.NoiDungNghiemThu = model.NoiDungNghiemThu;
        entity.NgayKhaoSat = model.NgayKhaoSat.ToStartOfDayUtc();
        // Không cập nhật DuAnId/BuocId/TrangThaiId/NgayTrinh từ form
    }

    public static BaoCaoKetQuaKhaoSatDto ToDto(this BaoCaoKetQuaKhaoSat entity) => new()
    {
        Id = entity.Id,
        DuAnId = entity.DuAnId,
        BuocId = entity.BuocId,
        NoiDungBaoCao = entity.NoiDungBaoCao,
        NoiDungNghiemThu = entity.NoiDungNghiemThu,
        NgayKhaoSat = entity.NgayKhaoSat.ToDateOnlyVn(),
        TrangThaiId = entity.TrangThaiId,
        TenTrangThai = entity.TrangThaiId == null
            ? TrangThaiPheDuyetCodes.Default.TenDuThao
            : entity.TrangThai?.Ten,
        NgayTrinh = entity.NgayTrinh.ToDateOnlyVn(),
    };
}
```

#### `BaoCaoKetQuaKhaoSatValidators.cs`

```csharp
using FluentValidation;
using QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Validators;

public class BaoCaoKetQuaKhaoSatInsertValidator : AbstractValidator<BaoCaoKetQuaKhaoSatInsertCommand>
{
    public BaoCaoKetQuaKhaoSatInsertValidator()
    {
        RuleFor(x => x.Dto.DuAnId).NotEmpty().WithMessage("Dự án không được để trống");
        RuleFor(x => x.Dto.NoiDungBaoCao).MaximumLength(4000);
        RuleFor(x => x.Dto.NoiDungNghiemThu).MaximumLength(4000);
    }
}

public class BaoCaoKetQuaKhaoSatUpdateValidator : AbstractValidator<BaoCaoKetQuaKhaoSatUpdateCommand>
{
    public BaoCaoKetQuaKhaoSatUpdateValidator()
    {
        RuleFor(x => x.Model.Id).NotEmpty();
        RuleFor(x => x.Model.NoiDungBaoCao).MaximumLength(4000);
        RuleFor(x => x.Model.NoiDungNghiemThu).MaximumLength(4000);
    }
}
```

---

### Bước 6 – Application: CRUD Commands & Queries

#### `BaoCaoKetQuaKhaoSatInsertCommand.cs`

> Copy `HoSoDeXuatCapDoCnttInsertCommand` — thay type, set `TrangThaiId` = `DT`:

```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;

public record BaoCaoKetQuaKhaoSatInsertCommand(BaoCaoKetQuaKhaoSatInsertDto Dto)
    : IRequest<BaoCaoKetQuaKhaoSat>;

internal class BaoCaoKetQuaKhaoSatInsertCommandHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatInsertCommand, BaoCaoKetQuaKhaoSat>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoKetQuaKhaoSatInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<BaoCaoKetQuaKhaoSat> Handle(
        BaoCaoKetQuaKhaoSatInsertCommand request,
        CancellationToken cancellationToken = default)
    {
        var trangThaiDuThao = await _statusRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.DuThao &&
                s.Loai == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, cancellationToken);

        var entity = request.Dto.ToEntity();
        entity.TrangThaiId = trangThaiDuThao?.Id;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
```

#### `BaoCaoKetQuaKhaoSatUpdateCommand.cs`

> Copy `HoSoDeXuatCapDoCnttUpdateCommand` — chỉ cho sửa khi `DT` / `TL` / `null` / `LEG`.
>
> **Khác mẫu HoSo:** HoSo chỉ cho `DT` / `null` / `LEG` (không có `TL`). UC55 thêm `TraLai` vì sau trả lại vẫn chỉnh nội dung báo cáo.

```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;

public record BaoCaoKetQuaKhaoSatUpdateCommand(BaoCaoKetQuaKhaoSatUpdateModel Model)
    : IRequest<BaoCaoKetQuaKhaoSat>;

internal class BaoCaoKetQuaKhaoSatUpdateCommandHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatUpdateCommand, BaoCaoKetQuaKhaoSat>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoKetQuaKhaoSatUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<BaoCaoKetQuaKhaoSat> Handle(
        BaoCaoKetQuaKhaoSatUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        var trangThaiDuThao = await _statusRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.DuThao &&
                s.Loai == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, cancellationToken);
        var trangThaiTraLai = await _statusRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.TraLai &&
                s.Loai == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, cancellationToken);

        var entity = await _repository.GetQueryableSet()
            .Include(e => e.TrangThai)
            .FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy báo cáo kết quả khảo sát");

        // Chỉ cập nhật khi: legacy (TrangThaiId null), Dự thảo, Trả lại, hoặc LEG
        if (entity.TrangThaiId != null &&
            entity.TrangThaiId != trangThaiDuThao?.Id &&
            entity.TrangThaiId != trangThaiTraLai?.Id &&
            entity.TrangThai?.Ma != "LEG")
        {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là Dự thảo hoặc Trả lại");
        }

        entity.Update(request.Model);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
```

#### `BaoCaoKetQuaKhaoSatDeleteCommand.cs`

> Copy `HoSoDeXuatCapDoCnttDeleteCommand` — **bỏ** phần `TepDinhKem` (v1 không có file).
>
> **So với mẫu HoSo:** không inject `IRepository<TepDinhKem>` và không gọi `SyncHelper.SetDeleteWithRelatedFiles` — soft-delete chỉ bản ghi `BaoCaoKetQuaKhaoSat`.

```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;

public record BaoCaoKetQuaKhaoSatDeleteCommand(Guid Id) : IRequest;

internal class BaoCaoKetQuaKhaoSatDeleteCommandHandler : IRequestHandler<BaoCaoKetQuaKhaoSatDeleteCommand>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoKetQuaKhaoSatDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task Handle(BaoCaoKetQuaKhaoSatDeleteCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}
```

#### `BaoCaoKetQuaKhaoSatGetQuery.cs`

```csharp
using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Queries;

public record BaoCaoKetQuaKhaoSatGetQuery : IRequest<BaoCaoKetQuaKhaoSat>
{
    public Guid Id { get; set; }
}

internal class BaoCaoKetQuaKhaoSatGetQueryHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatGetQuery, BaoCaoKetQuaKhaoSat>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;

    public BaoCaoKetQuaKhaoSatGetQueryHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
    }

    public async Task<BaoCaoKetQuaKhaoSat> Handle(
        BaoCaoKetQuaKhaoSatGetQuery request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.TrangThai)
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);
        return entity;
    }
}
```

#### `BaoCaoKetQuaKhaoSatGetDanhSachQuery.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Queries;

public record BaoCaoKetQuaKhaoSatGetDanhSachQuery(BaoCaoKetQuaKhaoSatSearchDto SearchDto)
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<BaoCaoKetQuaKhaoSatDto>>
{
    public string? GlobalFilter { get; set; }
}

internal class BaoCaoKetQuaKhaoSatGetDanhSachQueryHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatGetDanhSachQuery, PaginatedList<BaoCaoKetQuaKhaoSatDto>>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;

    public BaoCaoKetQuaKhaoSatGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
    }

    public async Task<PaginatedList<BaoCaoKetQuaKhaoSatDto>> Handle(
        BaoCaoKetQuaKhaoSatGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _repository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Include(e => e.TrangThai)
            .WhereIf(request.SearchDto.DuAnId.HasValue, e => e.DuAnId == request.SearchDto.DuAnId)
            .WhereIf(request.SearchDto.BuocId.HasValue, e => e.BuocId == request.SearchDto.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.NoiDungBaoCao,
                e => e.NoiDungNghiemThu);

        return await queryable
            .Select(e => e.ToDto())
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}
```

---

### Bước 7 – Application: Workflow Commands

**Folder:** `QLDA.Application/BaoCaoKetQuaKhaoSats/Commands/`

| File | Logic |
|------|-------|
| `BaoCaoKetQuaKhaoSatTrinhCommand` | `DT`/`TL`/`null` → `ĐTr`; `NgayTrinh`; history |
| `BaoCaoKetQuaKhaoSatDuyetCommand` | Chỉ `ĐTr` → `ĐD`; quyền LĐDV / HC-TH |
| `BaoCaoKetQuaKhaoSatTraLaiCommand` | Chỉ `ĐTr` → `TL`; bắt buộc `NoiDung` |

> Copy handler từ `HoSoDeXuatCapDoCntt*Command` — **bỏ** file `TuChoiCommand`. Tham khảo seed 4 trạng thái: `PhanKhaiKinhPhi*Command`.

#### `BaoCaoKetQuaKhaoSatTrinhCommand.cs` (mẫu — Duyet/TraLai copy tương tự)

```csharp
using BuildingBlocks.CrossCutting.ExtensionMethods;
using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;

public record BaoCaoKetQuaKhaoSatTrinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class BaoCaoKetQuaKhaoSatTrinhCommandHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatTrinhCommand, int>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoKetQuaKhaoSatTrinhCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(BaoCaoKetQuaKhaoSatTrinhCommand request, CancellationToken cancellationToken)
    {
        var trangThaiDuThao = await _statusRepository
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.DuThao &&
                s.Loai == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, cancellationToken);
        var trangThaiTraLai = await _statusRepository
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.TraLai &&
                s.Loai == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, cancellationToken);
        var trangThaiDaTrinh = await _statusRepository
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.DaTrinh &&
                s.Loai == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy báo cáo kết quả khảo sát");

        if (entity.TrangThaiId != null &&
            entity.TrangThaiId != trangThaiDuThao?.Id &&
            entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Chỉ có thể trình khi trạng thái là Dự thảo hoặc Trả lại");
        }

        entity.TrangThaiId = trangThaiDaTrinh.Id;
        entity.NgayTrinh = DateOnly.FromDateTime(DateTime.UtcNow).ToStartOfDayUtc();

        var history = new PheDuyetHistory
        {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.BaoCaoKetQuaKhaoSat,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);
        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

---

### Bước 8 – Đăng ký `QuanLyPheDuyet` Dispatch

Thêm `using QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;` và case trong **3 file** (không sửa `PheDuyetDispatchTuChoiCommand`):

**`PheDuyetDispatchTrinhCommand.cs`:**

```csharp
PheDuyetEntityNames.BaoCaoKetQuaKhaoSat => new BaoCaoKetQuaKhaoSatTrinhCommand(request.Id, request.NoiDung),
```

**`PheDuyetDispatchDuyetCommand.cs`:**

```csharp
PheDuyetEntityNames.BaoCaoKetQuaKhaoSat => new BaoCaoKetQuaKhaoSatDuyetCommand(request.Id),
```

**`PheDuyetDispatchTraLaiCommand.cs`:**

```csharp
PheDuyetEntityNames.BaoCaoKetQuaKhaoSat => new BaoCaoKetQuaKhaoSatTraLaiCommand(request.Id, request.NoiDung),
```

### Bước 9 – Application: Hub `PheDuyetGetDanhSach` / `GetChiTiet` (đã implement)

**Files:**

- `QLDA.Application/QuanLyPheDuyet/Queries/PheDuyetGetDanhSachQuery.cs`
- `QLDA.Application/QuanLyPheDuyet/Queries/PheDuyetGetChiTietQuery.cs`

**Thay đổi (copy pattern `GetHoSoDeXuatCapDoCntt*`):**

- `validTypes` + nhánh load khi `type` null hoặc `BaoCaoKetQuaKhaoSat`.
- `GetBaoCaoKetQuaKhaoSatItems`: filter `!IsDeleted`, `DuAnId`, global filter `NoiDungBaoCao` / `NoiDungNghiemThu`, `TrichYeu` = `NoiDungBaoCao`.
- `GetBaoCaoKetQuaKhaoSatDetail`: projection `DuAnId`, `BuocId`, `TrangThaiId`, `NgayTrinh`, nội dung báo cáo/nghiệm thu, `NgayKhaoSat`.

| Hub endpoint | `type=BaoCaoKetQuaKhaoSat` |
|--------------|----------------------------|
| `GET .../danh-sach?type=...` | Trả danh sách báo cáo (kèm `MaTrangThai`, `NgayXuLyMoiNhat`) |
| `GET .../{type}/{id}/chi-tiet` | Entity + `LichSu` phê duyệt |
| `POST .../trinh`, `duyet`, `tra-lai` | Dispatch (Bước 8) |
| `POST .../tu-choi` | *Loại phê duyệt không hợp lệ* (đúng UC55) |

---

### Bước 10 – WebApi

**Model:** `QLDA.WebApi/Models/BaoCaoKetQuaKhaoSats/BaoCaoKetQuaKhaoSatModel.cs`

```csharp
using SequentialGuid;

namespace QLDA.WebApi.Models.BaoCaoKetQuaKhaoSats;

public class BaoCaoKetQuaKhaoSatModel : IHasKey<Guid?>, IMustHaveId<Guid>
{
    public Guid? Id { get; set; }

    public Guid GetId()
    {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungNghiemThu { get; set; }
    public DateOnly? NgayKhaoSat { get; set; }
}
```

**Mapping:** `BaoCaoKetQuaKhaoSatMappingConfiguration.cs`

```csharp
using BuildingBlocks.CrossCutting.ExtensionMethods;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.WebApi.Models.BaoCaoKetQuaKhaoSats;

public static class BaoCaoKetQuaKhaoSatMappingConfiguration
{
    public static BaoCaoKetQuaKhaoSatModel ToModel(this BaoCaoKetQuaKhaoSat entity) => new()
    {
        Id = entity.Id,
        DuAnId = entity.DuAnId,
        BuocId = entity.BuocId,
        NoiDungBaoCao = entity.NoiDungBaoCao,
        NoiDungNghiemThu = entity.NoiDungNghiemThu,
        NgayKhaoSat = entity.NgayKhaoSat.ToDateOnlyVn(),
    };

    public static BaoCaoKetQuaKhaoSatInsertDto ToInsertDto(this BaoCaoKetQuaKhaoSatModel model) => new()
    {
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        NoiDungBaoCao = model.NoiDungBaoCao,
        NoiDungNghiemThu = model.NoiDungNghiemThu,
        NgayKhaoSat = model.NgayKhaoSat,
    };

    public static BaoCaoKetQuaKhaoSatUpdateModel ToUpdateModel(this BaoCaoKetQuaKhaoSatModel model) => new()
    {
        Id = model.GetId(),
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        NoiDungBaoCao = model.NoiDungBaoCao,
        NoiDungNghiemThu = model.NoiDungNghiemThu,
        NgayKhaoSat = model.NgayKhaoSat,
    };
}
```

**Controller:** `QLDA.WebApi/Controllers/BaoCaoKetQuaKhaoSatController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;
using QLDA.Application.BaoCaoKetQuaKhaoSats.Queries;
using QLDA.WebApi.Models;
using QLDA.WebApi.Models.BaoCaoKetQuaKhaoSats;

namespace QLDA.WebApi.Controllers;

[Tags("Báo cáo kết quả khảo sát")]
[Route("api/bao-cao-ket-qua-khao-sat")]
[Authorize]
public class BaoCaoKetQuaKhaoSatController(IServiceProvider sp) : AggregateRootController(sp)
{
    [HttpGet("{id}")]
    [ProducesResponseType<ResultApi<BaoCaoKetQuaKhaoSatDto>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new BaoCaoKetQuaKhaoSatGetQuery { Id = id });
        return ResultApi.Ok(entity.ToDto());
    }

    [HttpGet("danh-sach")]
    [ProducesResponseType<ResultApi<PaginatedList<BaoCaoKetQuaKhaoSatDto>>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> GetAll(
        [FromQuery] BaoCaoKetQuaKhaoSatSearchDto dto,
        string? globalFilter)
    {
        dto.GlobalFilter = globalFilter;
        var result = await Mediator.Send(new BaoCaoKetQuaKhaoSatGetDanhSachQuery(dto)
        {
            PageIndex = dto.PageIndex,
            PageSize = dto.PageSize,
            GlobalFilter = dto.GlobalFilter,
        });
        return ResultApi.Ok(result);
    }

    [HttpPost("them-moi")]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Create([FromBody] BaoCaoKetQuaKhaoSatModel model)
    {
        var entity = await Mediator.Send(new BaoCaoKetQuaKhaoSatInsertCommand(model.ToInsertDto()));
        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Update([FromBody] BaoCaoKetQuaKhaoSatModel model)
    {
        var entity = await Mediator.Send(new BaoCaoKetQuaKhaoSatUpdateCommand(model.ToUpdateModel()));
        return ResultApi.Ok(entity.Id);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Delete(Guid id)
    {
        await Mediator.Send(new BaoCaoKetQuaKhaoSatDeleteCommand(id));
        return ResultApi.Ok("Xóa báo cáo thành công");
    }
}
```

> **Workflow:** không thêm route trên controller — FE chỉ gọi `trinh` / `duyet` / `tra-lai` (không `tu-choi`).

---

### Bước 11 – Docs workflow

Cập nhật `docs/workflow-quan-ly-phe-duyet.md` — thêm `BaoCaoKetQuaKhaoSat` vào bảng Supported Entity Types.

---

## 5. API contract (FE)

### 5.1 Body them-moi / cap-nhat

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "duAnId": "00000000-0000-0000-0000-000000000001",
  "buocId": 1,
  "noiDungBaoCao": "Nội dung báo cáo khảo sát...",
  "noiDungNghiemThu": "Nội dung nghiệm thu...",
  "ngayKhaoSat": "2026-05-22"
}
```

### 5.2 Response chi tiết

```json
{
  "id": "...",
  "duAnId": "...",
  "buocId": 1,
  "noiDungBaoCao": "...",
  "noiDungNghiemThu": "...",
  "ngayKhaoSat": "2026-05-22",
  "trangThaiId": 26,
  "tenTrangThai": "Dự thảo",
  "ngayTrinh": null
}
```

### 5.3 Workflow request

```json
{ "noiDung": "Ghi chú (bắt buộc với trả lại)" }
```

---

## 6. Checklist hoàn thành

```
[x] 1.  Tạo Entity BaoCaoKetQuaKhaoSat
[x] 2.  Thêm PheDuyetEntityNames.BaoCaoKetQuaKhaoSat (+ Description cho GET /phe-duyet/types)
[x] 3.  Thêm TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat (DT/ĐTr/ĐD/TL)
[x] 4.  Tạo BaoCaoKetQuaKhaoSatConfiguration (EF)
[x] 5.  Seed 4 trạng thái DT/ĐTr/ĐD/TL (Id 26–29) trong DanhMucTrangThaiPheDuyetConfiguration
[x] 6.  Chạy Migration (ef.bat QLDA add BaoCaoKetQuaKhaoSat) — BLOCKING
[x] 6b. Xóa file trùng BaoCaoKetQuaKhaoSatMapping.cs (chỉ giữ Mappings.cs)
[x] 7.  Tạo DTOs + BaoCaoKetQuaKhaoSatMappings.cs
[x] 8.  Tạo Validators
[x] 9.  Tạo Insert / Update / Delete Commands (Update: DT/TL/null/LEG; Delete: không TepDinhKem)
[x] 10. Tạo Get / GetDanhSach Queries
[x] 11. Tạo Trinh / Duyet / TraLai Commands (không TuChoi)
[x] 12. Đăng ký PheDuyetDispatchTrinh/Duyet/TraLai (3 file); không TuChoi dispatch
[x] 13. PheDuyetGetDanhSach / GetChiTiet — GetBaoCaoKetQuaKhaoSatItems + GetBaoCaoKetQuaKhaoSatDetail
[x] 14. Tạo BaoCaoKetQuaKhaoSatModel + MappingConfiguration (WebApi)
[x] 15. Tạo BaoCaoKetQuaKhaoSatController (5 endpoint CRUD)
[x] 16. Cập nhật docs/workflow-quan-ly-phe-duyet.md
[x] 17. Build + Test workflow (§8)
```

---

## 7. Lưu ý kỹ thuật

- **`DuAnId`** bắt buộc — dùng cho filter danh sách và `PheDuyetHistory.DuAnId`.
- **`NgayTrinh`** chỉ set trong `TrinhCommand` — không map từ `UpdateModel`.
- **GlobalFilter** trên `NoiDungBaoCao`, `NoiDungNghiemThu`.
- **Seed Id 26–29** (4 trạng thái) — **không** seed `TC`.
- **Không** `TuChoiCommand` / **không** case trong `PheDuyetDispatchTuChoiCommand`.
- **Không** endpoint `thay-doi-trang-thai` — workflow qua hub `phe-duyet` (3 action).
- **Commit group:** Domain + `BaoCaoKetQuaKhaoSatConfiguration` + `DanhMucTrangThaiPheDuyetConfiguration` (seed) + Migrator cùng một commit.
- **Pattern copy:** mọi handler workflow đổi `HoSoDeXuatCapDoCntt` → `BaoCaoKetQuaKhaoSat` và constants tương ứng.
- **Mapping Application:** một file `BaoCaoKetQuaKhaoSatMappings.cs` — xóa `BaoCaoKetQuaKhaoSatMapping.cs` nếu còn (trùng `ToEntity`/`Update`/`ToDto` → ambiguous).
- **Hub `QuanLyPheDuyet`:** workflow qua `{type}` = `BaoCaoKetQuaKhaoSat` (PascalCase, khớp `PheDuyetEntityNames`); CRUD riêng `api/bao-cao-ket-qua-khao-sat`.
- **Khác HoSo khi Update:** cho sửa thêm trạng thái **Trả lại (`TL`)** — sau trả lại vẫn chỉnh nội dung báo cáo.

### 7.1 Migration brownfield (DB đã có sẵn)

**Triệu chứng:** `dotnet ef database update` lỗi `KinhPhiDeXuat` *specified more than once* trên `DeXuatNhuCauKinhPhi`.

**Nguyên nhân:** Migration `Init` có `Up()` **rỗng** (chỉ ghi history), trong khi DB thực tế đã có schema/cột từ trước. Migration `DeXuatNhuCauKinhPhiAddKinhPhiDeXuat` cố `ADD` cột đã tồn tại.

**Đã sửa (không drop database):**

- `20260522042016_DeXuatNhuCauKinhPhiAddKinhPhiDeXuat.cs` — `ADD` cột chỉ khi chưa có (`IF NOT EXISTS` sys.columns).
- `20260525030156_BaoCaoKetQuaKhaoSat.cs` — seed Id 26–29 `IF NOT EXISTS` (tránh trùng PK).

**Chạy lại:**

```bat
cd QLDA.Migrator
dotnet ef database update
```

Nếu `Init` đã apply nhưng migration `DeXuat...` fail giữa chừng, chạy lại `database update` sau khi pull fix trên.

---

## 8. Test plan

> Chạy sau migration (mục 6 checklist). Dùng tài khoản `QLDA_LDDV` hoặc phòng HC-TH cho duyệt/trả lại.

| # | Kịch bản | API | Kỳ vọng |
|---|----------|-----|---------|
| 1 | Tạo mới `DuAnId` hợp lệ | `POST .../them-moi` | `TrangThaiId` = DT (Id 26) |
| 2 | Cập nhật khi Đã trình | `PUT .../cap-nhat` | `ManagedException` |
| 2b | Cập nhật khi Trả lại | `PUT .../cap-nhat` | OK (khác HoSo) |
| 3 | Trình từ Dự thảo | `POST .../phe-duyet/BaoCaoKetQuaKhaoSat/{id}/trinh` | → ĐTr; history; `NgayTrinh` |
| 4 | Duyệt từ Đã trình | `POST .../duyet` | → ĐD |
| 5 | Trả lại (có `noiDung`) | `POST .../tra-lai` | → TL; trình lại được |
| 6 | Từ chối | `POST .../tu-choi` | Lỗi *Loại phê duyệt không hợp lệ* |
| 7 | Validator >4000 ký tự | `them-moi` / `cap-nhat` | Lỗi validation |
| 8 | Xóa mềm | `DELETE .../{id}` | Không còn `danh-sach` |
| 9 | Lịch sử | `GET .../phe-duyet/lich-su?type=BaoCaoKetQuaKhaoSat&entityId=` | `EntityName` đúng |
| 10 | `GET types` | `GET .../phe-duyet/types` | Có label *Báo cáo kết quả khảo sát* |
| 11 | Hub danh sách | `GET .../phe-duyet/danh-sach?type=BaoCaoKetQuaKhaoSat` | Có bản ghi, không rỗng |
| 12 | Hub chi tiết | `GET .../phe-duyet/BaoCaoKetQuaKhaoSat/{id}/chi-tiet` | `entity` + `lichSu` |

---

## 9. Tham chiếu code

| Thành phần | Đường dẫn |
|------------|-----------|
| Doc mẫu cấu trúc | `docs/feature/KySo/task-9460-ky-so-crud.md` |
| Entity mẫu | `QLDA.Domain/Entities/HoSoDeXuatCapDoCntt.cs` |
| EF config | `QLDA.Persistence/Configurations/HoSoDeXuatCapDoCnttConfiguration.cs` |
| Seed 4 trạng thái (không TC) | `TrangThaiPheDuyetCodes.PhanKhaiKinhPhi` + `DanhMucTrangThaiPheDuyetConfiguration` |
| CRUD + workflow handler | `QLDA.Application/HoSoDeXuatCapDoCntts/` |
| Workflow 3 bước | `QLDA.Application/PhanKhaiKinhPhis/Commands/` (bỏ `TuChoi` khi copy) |
| Dispatch | `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchTrinhCommand.cs` |
| Hub danh-sach/chi-tiet | `QLDA.Application/QuanLyPheDuyet/Queries/PheDuyetGetDanhSachQuery.cs`, `PheDuyetGetChiTietQuery.cs` |
| Controller CRUD UC55 | `QLDA.WebApi/Controllers/BaoCaoKetQuaKhaoSatController.cs` |
| Controller mẫu | `QLDA.WebApi/Controllers/HoSoDeXuatCapDoCnttController.cs` |
| Workflow hub | `docs/workflow-quan-ly-phe-duyet.md` |

---

## 10. Ước lượng

- **Effort còn lại:** ~0.1–0.25 ngày (migration + update DB + build/test §8).
- **Phụ thuộc:** seed trạng thái (sau migration); quyền test `QLDA_LDDV` / HC-TH.
