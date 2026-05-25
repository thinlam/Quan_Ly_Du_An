# Task – Fix junction `DeXuatTrinhKinhPhiNam`, `DanhMucDonViCbo`, Swagger `POST /api/tong-hop-kinh-phi/them-moi`

> Tham chiếu format: [`task-9460-ky-so-crud.md`](../KySo/task-9460-ky-so-crud.md), pattern junction: [`task-fix-don-vi-phoi-hop-ids.md`](../DeXuatChuTruongMoi/task-fix-don-vi-phoi-hop-ids.md)

---

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm

| # | Phần | Mục | Ghi chú |
|---|------|-----|---------|
| 1 | **PHẦN 1** | Sửa entity + EF `DeXuatTrinhKinhPhiNam` | Chỉ junction 2 cột; composite PK |
| 2 | **PHẦN 1** | Migration (nếu DB có cột dư) | Drop column, **không** drop cả bảng |
| 3 | **PHẦN 2** | `DanhMucDonViCbo` không sinh table | Keyless / không DbSet; drop table nếu đã tồn tại |
| 4 | **PHẦN 3** | Request body API `them-moi` | Mảng `Guid`, không nested full entity |
| 5 | Chung | Build + test Swagger/Postman | Không đổi route, không đổi `ResultApi` wrapper |

### 1.2 API bị ảnh hưởng

| Method | Endpoint | Sau fix |
|--------|----------|---------|
| POST | `/api/tong-hop-kinh-phi/them-moi` | Body gọn; lưu junction qua list Guid |
| PUT | `/api/tong-hop-kinh-phi/cap-nhat` | *(Ngoài scope tối thiểu — chỉ sửa nếu còn dùng `DeXuats` full object)* |
| GET | `/api/tong-hop-kinh-phi/{id}/chi-tiet` | Có thể bổ sung `danhSachDeXuatIds` response *(tùy product)* |

> **Không đổi** route, tag controller, wrapper `ResultApi`.

### 1.3 Payload mong muốn (POST `them-moi`)

Tên field theo convention dự án (ưu tiên khớp `DeXuatNhuCauKinhPhiNamInsertDto.DanhSachDeXuat`):

```json
{
  "so": "string",
  "ngayKeHoach": "2026-05-25T00:00:00.000Z",
  "trichYeu": "string",
  "ghiChu": "string",
  "tongKinhPhiDeXuat": 0,
  "maTrangThai": "string",
  "trangThaiId": 0,
  "tenTrangThai": "string",
  "danhSachDeXuat": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  ],
  "danhSachTepDinhKem": []
}
```

| Field | Kiểu | Map DB / nghiệp vụ |
|-------|------|---------------------|
| `danhSachDeXuat` | `List<Guid>?` | `DeXuatTrinhKinhPhiNam.RightId` (`DeXuatNhuCauKinhPhiId`) |
| — | — | `DeXuatTrinhKinhPhiNam.LeftId` = `DeXuatNhuCauKinhPhiNam.Id` sau insert |
| `maTrangThai`, `tenTrangThai` | string | **Read-only / display** — insert gán trạng thái DT server-side (handler hiện tại) |
| `danhSachTepDinhKem` | list | `TepDinhKem` group `NhuCauKinhPhi` |

> **Không** nhận `deXuats: [{ id, createdAt, deXuatNhuCauKinhPhi: { ... } }]`.

### 1.4 Bảng junction mục tiêu

| Cột DB | Property entity | Ghi chú |
|--------|-----------------|--------|
| `DeXuatKinhPhiNamId` | `LeftId` | FK → `DeXuatNhuCauKinhPhiNam.Id` |
| `DeXuatNhuCauKinhPhiId` | `RightId` | Id bản ghi `DeXuatNhuCauKinhPhi` được trình |

**Không có:** `Id`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `IsDeleted`, `Index`.

---

## 2. Phân tích hiện trạng

### 2.1 `DeXuatTrinhKinhPhiNam` – Domain / EF / DB

| Layer | Hiện trạng | Vấn đề |
|-------|------------|--------|
| **Domain** | `DeXuatTrinhKinhPhiNam : Entity<Guid>, IAggregateRoot` | Kế thừa base → Swagger/EF có thể expose `Id`, audit… dù config đã composite key |
| **Persistence** | `DeXuatTrinhKinhPhiNamConfiguration`: `HasKey(LeftId, RightId)`, rename cột | Đúng hướng junction; base class `AggregateRootConfiguration` không phù hợp entity đơn giản |
| **Migration Init** | Bảng **chỉ 2 cột** + composite PK | Khớp nghiệp vụ trong repo hiện tại |
| **Snapshot** | Chỉ `LeftId`, `RightId` | Không có cột audit trong model |
| **WebApi Model** | `DeXuatNhuCauKinhPhiNamModel.DeXuats` = `List<DeXuatTrinhKinhPhiNam>?` | **Nguyên nhân Swagger phức tạp** — client thấy full entity + navigation |

**File liên quan:**

- `QLDA.Domain/Entities/DeXuatTrinhKinhPhiNam.cs`
- `QLDA.Persistence/Configurations/DeXuatTrinhKinhPhiNamConfiguration.cs`
- `QLDA.Domain/Entities/DeXuatNhuCauKinhPhiNam.cs` — `ICollection<DeXuatTrinhKinhPhiNam>? DeXuats`
- `QLDA.WebApi/Models/DeXuatNhuCauKinhPhiNams/DeXuatNhuCauKinhPhiNamModel.cs`
- `QLDA.WebApi/Controllers/DeXuatNhuCauKinhPhiNamController.cs` — `POST them-moi` dùng `DeXuatNhuCauKinhPhiNamModel`

### 2.2 Application – đã có DTO list Guid (chưa dùng ở WebApi)

| File | Ghi chú |
|------|---------|
| `QLDA.Application/DeXuatKinhPhiNam/DTOs/DeXuatKinhPhiNamInsertDto.cs` | Đã có `List<Guid>? DanhSachDeXuat` |
| `QLDA.Application/DeXuatKinhPhiNam/DeXuatNhuCauKinhPhiMappings.cs` | `ToEntity(dto)` map junction; `SyncDeXuatIds(entity, ids)` |
| `QLDA.Application/DeXuatKinhPhiNam/Commands/DeXuatKinhPhiNamInsertCommand.cs` | Insert parent → `SyncDeXuatIds` sau `SaveChanges`; đọc `request.Dto.DeXuats?.Select(x => x.RightId)` |

**Gap:** Controller gọi `model.ToEntity()` **không** map `DeXuats` / `DanhSachDeXuat` → junction không lưu khi POST qua WebApi model.

### 2.3 `DanhMucDonViCbo`

| Vị trí | Hiện trạng trong repo |
|--------|----------------------|
| `QLDA.Application/DeXuatChuTruongMoi/DTOs/DeXuatChuTruongMoiDto.cs` | Class `DanhMucDonViCbo` (DTO thuần) |
| `QLDA.Application/DeXuatChuTruongMoi/Queries/DeXuatChuTruongMoiGetDanhSachQuery.cs` | **Trùng** class `DanhMucDonViCbo` trong file Query |
| `QLDA.Domain` / `QLDA.Persistence` / `QLDA.Migrator` | **Không** có entity `DanhMucDonViCbo`, **không** có table trong migration |

**Đọc dữ liệu:** Join `DmDonVi` + `DeXuatDonViXuLy` trong LINQ (`Select` → `DTOs.DanhMucDonViCbo`).

> Nếu môi trường dev **có** table `DanhMucDonViCbo` trên SQL Server: có thể do branch khác / entity từng được thêm vào `DbContext`. Bước implement phải **grep toàn solution** trước khi quyết định migration drop.

### 2.4 Pattern tham chiếu (bắt buộc bám)

| Pattern | File |
|---------|------|
| Junction entity | `DeXuatDonViXuLy : IJunctionEntity<Guid, long>` |
| Junction EF config | `DeXuatDonViXuLyConfiguration` |
| API list id | `DeXuatChuTruongMoiModel.DonViPhoiHopIds` |
| Sync junction | `DeXuatChuTruongMoiMappings.SyncDonViPhoiHopIds` |
| POST không full object | `task-fix-don-vi-phoi-hop-ids.md` |

---

## 3. Thứ tự thực hiện

```
Bước 1:  Domain – DeXuatTrinhKinhPhiNam → IJunctionEntity<Guid, Guid>
Bước 2:  Persistence – DeXuatTrinhKinhPhiNamConfiguration (bỏ audit base nếu có)
Bước 3:  Kiểm tra DB thực tế → ef.bat QLDA add (chỉ khi có cột/table dư)
Bước 4:  Application – dọn repository/handler (bỏ IRepository<DeXuatTrinhKinhPhiNam, Guid> thừa)
Bước 5:  WebApi – Model + Mapping + Controller POST (danhSachDeXuat)
Bước 6:  DanhMucDonViCbo – audit entity/DbSet; keyless hoặc DTO-only; migration drop table (nếu có)
Bước 7:  dotnet build solution
Bước 8:  Test Swagger / Postman
```

**Commit group (nếu có migration):** Domain + Persistence.Configuration + Migrator **cùng một commit** (theo `CLAUDE.md`).

---

## 4. Chi tiết từng bước

---

### Bước 1 – Domain: `DeXuatTrinhKinhPhiNam`

**File:** `QLDA.Domain/Entities/DeXuatTrinhKinhPhiNam.cs`

**Trước:**

```csharp
public class DeXuatTrinhKinhPhiNam : Entity<Guid>, IAggregateRoot {
    public Guid LeftId { get; set; }
    public Guid RightId { get; set; }
    public DeXuatNhuCauKinhPhiNam? DeXuatNhuCauKinhPhi { get; set; }
}
```

**Sau (theo `DeXuatDonViXuLy`):**

```csharp
using QLDA.Domain.Interfaces;

namespace QLDA.Domain.Entities;

/// <summary>
/// Junction: Tổng hợp KP năm ↔ Đề xuất nhu cầu kinh phí được trình
/// </summary>
public class DeXuatTrinhKinhPhiNam : IJunctionEntity<Guid, Guid>, IAggregateRoot {
    public Guid LeftId { get; set; }
    public Guid RightId { get; set; }

    #region Navigation Properties
    public DeXuatNhuCauKinhPhiNam? DeXuatNhuCauKinhPhiNam { get; set; }
    #endregion
}
```

| Thay đổi | Lý do |
|----------|-------|
| Bỏ `Entity<Guid>` | Không còn `Id` / audit trên junction |
| Giữ `IAggregateRoot` | Khớp pattern `DeXuatDonViXuLy`, repository hiện tại |
| Đổi tên navigation (tùy chọn) | Tránh nhầm với entity `DeXuatNhuCauKinhPhi`; cập nhật Configuration `WithMany` |

---

### Bước 2 – Persistence: `DeXuatTrinhKinhPhiNamConfiguration`

**File:** `QLDA.Persistence/Configurations/DeXuatTrinhKinhPhiNamConfiguration.cs`

```csharp
public class DeXuatTrinhKinhPhiNamConfiguration : AggregateRootConfiguration<DeXuatTrinhKinhPhiNam> {
    public override void Configure(EntityTypeBuilder<DeXuatTrinhKinhPhiNam> builder) {
        builder.ToTable(nameof(DeXuatTrinhKinhPhiNam));

        builder.HasKey(e => new { e.LeftId, e.RightId });

        builder.Property(e => e.LeftId).HasColumnName("DeXuatKinhPhiNamId");
        builder.Property(e => e.RightId).HasColumnName("DeXuatNhuCauKinhPhiId");

        // Không gọi ConfigureForBase() — junction không có audit columns

        builder.HasOne(e => e.DeXuatNhuCauKinhPhiNam)
            .WithMany(e => e.DeXuats)
            .HasForeignKey(e => e.LeftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Kiểm tra sau khi sửa entity:**

```bash
cd QLDA.Migrator
dotnet ef migrations add FixDeXuatTrinhKinhPhiNamJunction --startup-project ../QLDA.Migrator
```

| Kết quả migration | Hành động |
|-------------------|-----------|
| **Empty** / no-op | DB đã đúng 2 cột (Init) → có thể `ef.bat QLDA remove` migration vừa add |
| **DropColumn** `Id`, `CreatedAt`, … | Apply lên DB dev đang có cột dư (ảnh screenshot task) |
| **DropTable** `DanhMucDonViCbo` | Chỉ khi bước 6 xác nhận table tồn tại |

```bash
# Từ root repo
ef.bat QLDA add FixDeXuatTrinhKinhPhiNamAndDanhMucDonViCbo
ef.bat QLDA update
# hoặc local: ef.bat QLDA update --sqlite
```

> **KHÔNG** sửa tay file migration `.cs` sau khi generate. **KHÔNG** sửa snapshot cũ.

---

### Bước 3 – Migration: cột cần remove (nếu DB có)

Chỉ chạy khi SSMS / `INFORMATION_SCHEMA.COLUMNS` xác nhận bảng `DeXuatTrinhKinhPhiNam` **có** các cột:

| Cột drop | Ghi chú |
|----------|---------|
| `Id` | PK cũ single-column (nếu từng tạo nhầm) |
| `CreatedAt` | Audit |
| `CreatedBy` | Audit |
| `UpdatedAt` | Audit |
| `UpdatedBy` | Audit |
| `IsDeleted` | Soft delete — không dùng cho junction |
| `Index` | STT / ordering base |

**Không làm:** `DropTable("DeXuatTrinhKinhPhiNam")` khi chỉ cần drop column.

---

### Bước 4 – Application: Insert / Update / Repository

#### 4.1 `DeXuatNhuCauKinhPhiNamInsertCommand`

**File:** `QLDA.Application/DeXuatKinhPhiNam/Commands/DeXuatKinhPhiNamInsertCommand.cs`

- Đổi command nhận `DeXuatNhuCauKinhPhiNamInsertDto` (hoặc record kèm `List<Guid>? DanhSachDeXuat`) thay vì entity từ WebApi map thiếu field.
- Lấy ids: `request.Dto.DanhSachDeXuat` thay vì `request.Dto.DeXuats?.Select(x => x.RightId)`.
- **Xóa** field `_repoDeXuat` nếu không dùng (dead code hiện tại).

**Luồng giữ nguyên:**

1. Insert `DeXuatNhuCauKinhPhiNam` (trạng thái DT server-side).
2. `SaveChanges` → có `entity.Id`.
3. `entity.SyncDeXuatIds(danhSachDeXuat)`.
4. `SaveChanges` lần 2.

#### 4.2 `DeXuatNhuCauKinhPhiNamUpdateCommand` *(nếu PUT cần sync junction)*

- `Include(e => e.DeXuats)`.
- Gọi `SyncDeXuatIds` từ list Guid trong DTO.

#### 4.3 Repository

- Junction **không** có `Guid Id` → tránh `IRepository<DeXuatTrinhKinhPhiNam, Guid>` cho thao tác đơn lẻ; sync qua navigation collection parent (pattern `DeXuatDonViXuLy`).

---

### Bước 5 – WebApi: Model + Mapping + Controller

#### 5.1 `DeXuatNhuCauKinhPhiNamModel`

**File:** `QLDA.WebApi/Models/DeXuatNhuCauKinhPhiNams/DeXuatNhuCauKinhPhiNamModel.cs`

**Xóa:**

```csharp
public List<DeXuatTrinhKinhPhiNam>? DeXuats { get; set; }
```

**Thêm:**

```csharp
/// <summary>Danh sách Id DeXuatNhuCauKinhPhi được trình trong tổng hợp KP năm</summary>
public List<Guid>? DanhSachDeXuat { get; set; }
```

> Có thể alias JSON `deXuatNhuCauKinhPhiIds` bằng `[JsonPropertyName("deXuatNhuCauKinhPhiIds")]` **chỉ khi** FE đã chốt tên — ưu tiên `danhSachDeXuat` khớp InsertDto.

#### 5.2 `DeXuatNhuCauKinhPhiNamMappingConfiguration`

**File:** `QLDA.WebApi/Models/DeXuatNhuCauKinhPhiNams/DeXuatNhuCauKinhPhiNamMappingConfiguration.cs`

Thêm:

```csharp
public static DeXuatNhuCauKinhPhiNamInsertDto ToInsertDto(this DeXuatNhuCauKinhPhiNamModel model) =>
    new() {
        So = model.So,
        NgayKeHoach = model.NgayKeHoach,
        TrichYeu = model.TrichYeu,
        GhiChu = model.GhiChu,
        TongKinhPhiDeXuat = model.TongKinhPhiDeXuat,
        TrangThaiId = model.TrangThaiId,
        DanhSachDeXuat = model.DanhSachDeXuat,
        DanhSachTepDinhKem = model.DanhSachTepDinhKem?.Select(...).ToList(),
    };
```

`ToModel` (GET): map `DanhSachDeXuat = entity.DeXuats?.Select(x => x.RightId).ToList()` nếu product cần trả ids.

#### 5.3 Controller `POST them-moi`

**File:** `QLDA.WebApi/Controllers/DeXuatNhuCauKinhPhiNamController.cs`

**Trước:**

```csharp
var entity = model.ToEntity();
var savedEntity = await Mediator.Send(new DeXuatNhuCauKinhPhiNamInsertCommand(entity));
```

**Sau:**

```csharp
var savedEntity = await Mediator.Send(
    new DeXuatNhuCauKinhPhiNamInsertCommand(model.ToInsertDto()));
```

- POST **không** gọi `model.GetId()` trên parent (để DB sinh `Id`) — align `task-fix-don-vi-phoi-hop-ids`.
- `TepDinhKem` dùng `savedEntity.Id` sau insert.

#### 5.4 `[ProducesResponseType]`

Đổi request type Swagger sang `DeXuatNhuCauKinhPhiNamModel` (đã gọn) hoặc `DeXuatNhuCauKinhPhiNamInsertDto` nếu controller bind trực tiếp DTO Application (ưu tiên **một** model cho Swagger).

---

### Bước 6 – `DanhMucDonViCbo`

#### 6.1 Audit (bắt buộc trước code)

```bash
# Tìm entity/DbSet/configuration
rg "DanhMucDonViCbo" --glob "*.cs"
rg "DbSet.*DanhMucDonViCbo" 
```

| Kết quả audit | Hành động |
|---------------|-----------|
| Chỉ có class trong **Application DTO/Query** (repo hiện tại) | **Không** migration; gom class trùng → một file DTO |
| Có `QLDA.Domain` entity + `DbSet` | Xem 6.2 |
| Table có trên DB, không có trong code | Migration `DropTable` |

#### 6.2 Nếu tồn tại Domain entity (môi trường lỗi)

**Option A – Chỉ DTO (khuyến nghị cho UC Đề xuất chủ trương):**

- Xóa entity Domain / DbSet / Configuration.
- Giữ `QLDA.Application/DeXuatChuTruongMoi/DTOs/DanhMucDonViCbo.cs`.
- Xóa class trùng trong `DeXuatChuTruongMoiGetDanhSachQuery.cs` → `using DTOs`.

**Option B – Keyless đọc view/table ngoài (chỉ khi bắt buộc map SQL view):**

```csharp
// Domain
[Keyless]
public class DanhMucDonViCbo { public long Id { get; set; } public string? TenDonVi { get; set; } }

// Persistence
builder.Entity<DanhMucDonViCbo>().HasNoKey().ToView("vw_DanhMucDonViCbo"); // hoặc ToTable nếu đọc bảng có sẵn, không insert
```

> Với list **Đề xuất chủ trương mới**, hiện **không cần** Option B — join `DmDonVi` đã đủ.

#### 6.3 Migration drop table

Chỉ khi `DanhMucDonViCbo` tồn tại trong DB:

```sql
-- Kiểm tra
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DanhMucDonViCbo';
```

EF migration generate: `migrationBuilder.DropTable(name: "DanhMucDonViCbo");`

#### 6.4 Đảm bảo API list vẫn chạy

`DeXuatChuTruongMoiGetDanhSachQuery` — không đổi logic join; chỉ đổi namespace DTO sau khi gom class.

---

## 5. Checklist hoàn thành

```
[ ] 1. DeXuatTrinhKinhPhiNam → IJunctionEntity<Guid, Guid> (bỏ Entity<Guid>)
[ ] 2. EF Configuration: composite key, không ConfigureForBase
[ ] 3. Migration: drop cột dư (nếu DB có) — không drop bảng junction
[ ] 4. WebApi Model: DanhSachDeXuat thay DeXuats
[ ] 5. Controller POST: InsertDto + command handler
[ ] 6. Insert handler: DanhSachDeXuat + SyncDeXuatIds
[ ] 7. DanhMucDonViCbo: audit + gom DTO / drop table nếu có
[ ] 8. dotnet build (0 error)
[ ] 9. Swagger POST them-moi: body không còn nested DeXuatNhuCauKinhPhi
[ ] 10. Postman: insert + verify bảng DeXuatTrinhKinhPhiNam
```

---

## 6. Lưu ý kỹ thuật

- **DTO mapping** nằm **Application** (`DeXuatNhuCauKinhPhiNamMappings`) — không tạo WebApi model map entity phức tạp (`CLAUDE.md`).
- **RightId** = `DeXuatNhuCauKinhPhi.Id` (module đề xuất nhu cầu KP), **LeftId** = `DeXuatNhuCauKinhPhiNam.Id` (tổng hợp KP năm). Tên cột `DeXuatNhuCauKinhPhiId` giữ nguyên theo migration Init.
- Init migration **đã** đúng 2 cột — dev DB có thêm cột thường do apply schema cũ hoặc entity `Entity<Guid>` từng được scaffold. Luôn **đối chiếu DB thật** trước khi commit migration.
- `maTrangThai`, `tenTrangThai` trên request có thể bỏ qua khi insert — handler gán `TrangThaiId` từ `DanhMucTrangThaiPheDuyet` mã `DT`.
- **Không** push `bin/`, `obj/`, `logs/`, `publish/`.

---

## 7. Kịch bản test (Swagger / Postman)

### 7.1 POST `/api/tong-hop-kinh-phi/them-moi`

**Request:**

```http
POST /api/tong-hop-kinh-phi/them-moi
Content-Type: application/json

{
  "so": "TH-2026-001",
  "ngayKeHoach": "2026-05-25T00:00:00Z",
  "trichYeu": "Tổng hợp KP năm test",
  "tongKinhPhiDeXuat": 1000000,
  "danhSachDeXuat": [
    "<guid-de-xuat-nhu-cau-kinh-phi-1>",
    "<guid-de-xuat-nhu-cau-kinh-phi-2>"
  ],
  "danhSachTepDinhKem": []
}
```

| Kiểm tra | Kỳ vọng |
|----------|---------|
| Status | `200`, `data` = `Guid` parent mới |
| Swagger schema | Không có object `deXuats` lồng audit |
| DB `DeXuatTrinhKinhPhiNam` | 2 row / 2 id; chỉ 2 cột + PK composite |
| DB không có cột `Id` trên junction | SSMS confirm |

### 7.2 SQL verify junction

```sql
SELECT DeXuatKinhPhiNamId, DeXuatNhuCauKinhPhiId
FROM DeXuatTrinhKinhPhiNam
WHERE DeXuatKinhPhiNamId = '<parent-id-vua-tao>';
```

### 7.3 GET list Đề xuất chủ trương (DanhMucDonViCbo)

```http
GET /api/de-xuat-chu-truong-moi/danh-sach?duAnId=<guid>
```

| Kiểm tra | Kỳ vọng |
|----------|---------|
| `danhSachDonViPhoiHop` | Vẫn có `id`, `tenDonVi` sau khi gom DTO |
| Table `DanhMucDonViCbo` | Không tồn tại (hoặc đã drop) |

### 7.4 Build

```bash
dotnet build QLDA.sln
```

---

## 8. Mẫu báo cáo sau khi implement (trả lời 7 câu hỏi)

> Dev điền sau khi xong task — copy section này vào PR / comment.

| # | Câu hỏi | Điền khi xong |
|---|---------|---------------|
| 1 | Đã sửa những file nào? | *(list path)* |
| 2 | Entity `DeXuatTrinhKinhPhiNam` đã sửa ra sao? | `IJunctionEntity<Guid, Guid>`, chỉ `LeftId`/`RightId` |
| 3 | Migration mới tên gì? | `FixDeXuatTrinhKinhPhiNam...` / hoặc *Không có* |
| 4 | Đã remove những cột nào khỏi `DeXuatTrinhKinhPhiNam`? | `Id`, `CreatedAt`, … hoặc *Không có trên DB* |
| 5 | `DanhMucDonViCbo` xử lý thế nào? | DTO-only / keyless / drop table |
| 6 | Request body `them-moi` sau sửa? | JSON mẫu §1.3 |
| 7 | Cách test lại? | §7 Swagger + SQL |

---

## 9. Phụ lục – File map nhanh

| Layer | File | Thao tác |
|-------|------|----------|
| Domain | `Entities/DeXuatTrinhKinhPhiNam.cs` | Sửa junction |
| Persistence | `Configurations/DeXuatTrinhKinhPhiNamConfiguration.cs` | Sửa EF |
| Migrator | `ef.bat QLDA add ...` | Có điều kiện |
| Application | `DeXuatKinhPhiNam/DeXuatNhuCauKinhPhiMappings.cs` | Giữ / tinh chỉnh |
| Application | `DeXuatKinhPhiNam/DTOs/DeXuatKinhPhiNamInsertDto.cs` | Đã có `DanhSachDeXuat` |
| Application | `DeXuatKinhPhiNam/Commands/DeXuatKinhPhiNamInsertCommand.cs` | Dùng DTO + dọn repo |
| WebApi | `Models/.../DeXuatNhuCauKinhPhiNamModel.cs` | List Guid |
| WebApi | `Models/.../DeXuatNhuCauKinhPhiNamMappingConfiguration.cs` | `ToInsertDto` |
| WebApi | `Controllers/DeXuatNhuCauKinhPhiNamController.cs` | POST |
| Application | `DeXuatChuTruongMoi/DTOs/...` + Query | Gom `DanhMucDonViCbo` |

---

*Task doc – chưa implement code. Tạo từ phân tích codebase + yêu cầu user.*
