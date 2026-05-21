# Task – Chuẩn hóa ngày: `DateTime` → `DateTimeOffset` (DB) + `DateOnly` (API)

> **Audit toàn QLDA (21/05/2026):** Trong `QLDA.Domain`, chỉ **2 entity** còn `DateTime?` cho ngày nghiệp vụ (`KySo`, `HoSoDeXuatCapDoCntt`). Phần lớn bảng khác đã `DateTimeOffset?` ở DB; nhiều API vẫn trả `DateTimeOffset?` thay vì `DateOnly?` — xem §2.4 (phase sau).  
> Liên quan: [task-9460-ky-so-crud.md](./task-9460-ky-so-crud.md), [task-9460-noi-dung-da-ky.md](./task-9460-noi-dung-da-ky.md)  
> Convention: [HopDong #9602](../../issues/9602/journal.md), [DateTimeOffsetUtcLocalConversion.md](../DateTimeOffsetUtcLocalConversion.md), `DateOnlyExtensions`

---

## 1. Mục tiêu

| Tầng | Kiểu chuẩn | Ghi chú |
|------|------------|---------|
| **Database / Entity** | `DateTimeOffset?` | Cột SQL `datetimeoffset`; lưu UTC (HasConversion) |
| **Request / Response DTO** | `DateOnly?` | JSON `yyyy-MM-dd`; không gửi giờ/timezone |
| **Mapping** | `ToStartOfDayUtc()` / `ToDateOnlyVn()` | Giống `HopDongMappings` |

**Không** dùng `DateTime` cho field “chỉ có ngày” (hiệu lực chứng thư, ngày trình, …).

---

## 2. Hiện trạng — audit kỹ (21/05/2026)

### 2.0 Tổng quan `grep DateTime` trong QLDA

| Lớp | `DateTime?` / `DateTime` nghiệp vụ | Ghi chú |
|-----|-----------------------------------|---------|
| **QLDA.Domain** | **2 entity**, 3 property | Bảng dưới §2.1–2.2 |
| **QLDA.Application** | KySo DTO + HoSo DTO + ToTrinh **Update** DTO | Mapping/command dùng `DateTime.UtcNow` / `ToDateTime` |
| **QLDA.WebApi** | `HoSoDeXuatCapDoCnttMappingConfiguration` (`.DateTime`) | Model `DateTimeOffset?` ↔ entity `DateTime?` |
| **QLDA.Persistence** | Không cấu hình `HasConversion` cho `KySo`, `HoSoDeXuatCapDoCntt` | Khác `HopDong`, `DuAn`, … |
| **Migrations snapshot** | `KySo.HieuLuc*`, `HoSoDeXuatCapDoCntt.NgayTrinh` = `datetime2` | Cần migration đổi kiểu |

`DateTime.Now` trong `PrintController` (tên file export) — **không** đổi (không phải field DB).

---

### 2.1 Bảng `KySo` — **Phase A (ưu tiên #9460)**

| Vị trí | Field | Hiện tại | Chuẩn |
|--------|-------|----------|-------|
| `QLDA.Domain/Entities/KySo.cs` | `HieuLucTu`, `HieuLucDen` | `DateTime?` | `DateTimeOffset?` |
| DB `KySo` (Init migration) | cột tương ứng | `datetime2` | `datetimeoffset` |
| `QLDA.Application/.../KySoInsertDto.cs` | `HieuLucTu`, `HieuLucDen` | `DateOnly?` | ✅ đúng |
| `QLDA.Application/.../KySoUpdateModel.cs` | (kế thừa InsertDto) | `DateOnly?` | ✅ đúng |
| `QLDA.Application/.../KySoDto.cs` | `HieuLucTu`, `HieuLucDen` | `DateTime?` | `DateOnly?` |
| `QLDA.Application/.../KySoMappings.cs` | ToEntity / Update | `dto.HieuLuc*.ToDateTime(TimeOnly.MinValue)` | `dto.HieuLuc*.ToStartOfDayUtc()` |
| `QLDA.Application/.../KySoMappings.cs` | ToDto | gán thẳng `entity.HieuLuc*` | `entity.HieuLuc*.ToDateOnlyVn()` |
| `QLDA.Persistence/Configurations/KySoConfiguration.cs` | — | chưa HasConversion | thêm HasConversion UTC (giống `HopDongConfiguration`) |
| `docs/feature/KySo/task-9460-ky-so-crud.md` | spec | `DateTime?` | cập nhật doc sau khi code xong |

**Hệ quả bug tiềm ẩn:** Insert/Update đã nhận `DateOnly` nhưng map sang `DateTime` local (không UTC, không offset) → lệch chuẩn các bảng khác (`HopDong`, `DuAn`, …).

### 2.2 Bảng `HoSoDeXuatCapDoCntt` — **Phase B (cùng kiểu lỗi, nên gộp PR)**

| Vị trí | Field | Hiện tại | Chuẩn |
|--------|-------|----------|-------|
| `QLDA.Domain/Entities/HoSoDeXuatCapDoCntt.cs` | `NgayTrinh` | `DateTime?` | `DateTimeOffset?` |
| DB (migration `AddDmCapDoCnttAndHoSoDeXuatCapDoCntt`) | `NgayTrinh` | `datetime2` | `datetimeoffset` |
| `HoSoDeXuatCapDoCnttInsertDto` / `UpdateModel` / `Dto` | `NgayTrinh` | `DateTime?` | `DateOnly?` |
| `HoSoDeXuatCapDoCnttMappings.cs` | Update | `DateTime.UtcNow` fallback | `DateOnly.FromDateTime(DateTime.UtcNow)` hoặc `ToStartOfDayUtc()` |
| `HoSoDeXuatCapDoCnttTrinhCommand` | set ngày trình | `entity.NgayTrinh = DateTime.UtcNow` | `DateTimeOffset.UtcNow` hoặc `DateOnly.FromDateTime(...).ToStartOfDayUtc()` |
| `HoSoDeXuatCapDoCnttThayDoiTrangThaiCommand` | tương tự | `DateTime.UtcNow` | đồng bộ Phase B |
| `HoSoDeXuatCapDoCnttConfiguration.cs` | — | chưa HasConversion | thêm HasConversion |
| WebApi `HoSoDeXuatCapDoCnttModel` | `NgayTrinh` | `DateTimeOffset?` | `DateOnly?` |
| WebApi `HoSoDeXuatCapDoCnttMappingConfiguration` | map | `model.NgayTrinh?.DateTime` | `model.NgayTrinh.ToStartOfDayUtc()` / `ToDateOnlyVn()` |

---

### 2.3 DTO lệch entity (Domain đã `DateTimeOffset`, API vẫn `DateTime`)

| Module | File | Vấn đề |
|--------|------|--------|
| ToTrinhKeHoach | `ToTrinhKeHoachUpdateDto.cs` | `NgayToTrinh`: `DateTime?` — entity `DateTimeOffset?`; InsertDto đã `DateTimeOffset?` |
| ToTrinhKeHoach | `ToTrinhKeHoachUpdateCommand.cs` | Gán thẳng `entity.NgayToTrinh = request.Dto.NgayToTrinh` — cần `DateOnly?` + `ToStartOfDayUtc()` (giống query list đã dùng `TuNgay`/`DenNgay` `DateOnly`) |

**Phase C:** ToTrinhKeHoach (chỉ DTO/mapping, **không** đổi cột DB nếu đã `datetimeoffset`).

---

### 2.4 Nhóm Ký số — **không đổi kiểu ngày**

| Thành phần | Lý do |
|------------|-------|
| `TepDinhKem` + `POST /api/ky-so/them-moi` | Không field ngày nghiệp vụ |
| `NoiDungDaKySo` (V1, đã drop) | Audit fields đã `DateTimeOffset` |
| `DmPhuongThucKySo` | Không field ngày |

---

### 2.5 Phase D — API vẫn `DateTimeOffset?` cho “ngày thuần” (không còn `DateTime` ở Domain)

Đã chuẩn DB (`DateTimeOffset` + HasConversion) nhưng **request/response** nhiều module vẫn ISO datetime — chưa thống nhất `DateOnly` như `HopDong`, `BanGiaoHoSo`:

| Nhóm ví dụ | Field API (`DateTimeOffset?`) | Entity |
|------------|------------------------------|--------|
| VanBan / QuyetDinh | `Ngay`, `NgayKy`, `NgayQuyetDinh`, `NgayVanBan` | `DateTimeOffset?` |
| BaoCao / NghiemThu / ThanhToan | `Ngay`, `NgayHoaDon` | `DateTimeOffset?` |
| DuAn / DuToan / KetQuaTrungThau | `NgayBatDau`, `NgayKyDuToan`, `NgayEHSMT`, … | `DateTimeOffset?` |
| TamUng | `NgayTamUng`, `NgayBaoLanh`, … | `DateTimeOffset?` |

**Đã làm mẫu `DateOnly`:** `HopDong` (một phần), `BanGiaoHoSo`, search `TuNgay`/`DenNgay` (`CommonSearchDto`).

→ Task riêng / từng module; **không** gộp hết vào PR Ký số.

---

## 3. Quy ước mapping (copy từ HopDong)

```csharp
using BuildingBlocks.CrossCutting.ExtensionMethods;

// Insert / Update: DateOnly → Entity
entity.HieuLucTu = dto.HieuLucTu.ToStartOfDayUtc();
entity.HieuLucDen = dto.HieuLucDen.ToStartOfDayUtc();

// Response DTO: Entity → DateOnly
HieuLucTu = entity.HieuLucTu.ToDateOnlyVn(),
HieuLucDen = entity.HieuLucDen.ToDateOnlyVn(),
```

**Persistence** (`KySoConfiguration.cs`):

```csharp
builder.Property(e => e.HieuLucTu)
    .HasConversion(
        toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
        fromDb => fromDb);

builder.Property(e => e.HieuLucDen)
    .HasConversion(
        toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
        fromDb => fromDb);
```

Extension: `ToStartOfDayUtc` dùng offset VN (+7) rồi chuyển UTC — xem `DateOnlyExtensions.cs`.

---

## 4. Thứ tự thực hiện (theo phase)

> **Commit group (mỗi phase có migration):** Domain + Configuration + Migration + snapshot — **một commit** (theo `CLAUDE.md`).

### Phase A — `KySo` (#9460)

```
A1: Domain KySo → DateTimeOffset?
A2: KySoConfiguration HasConversion
A3: Migration KySoHieuLucToDateTimeOffset
A4: KySoDto + KySoMappings (DateOnly / ToStartOfDayUtc / ToDateOnlyVn)
A5: Docs task-9460-ky-so-crud.md
A6: Postman QuanLyKySo
```

### Phase B — `HoSoDeXuatCapDoCntt` (gộp PR nếu team đồng ý)

```
B1: Domain + HoSoDeXuatCapDoCnttConfiguration + Migration
B2: Application DTOs + Mappings + TrinhCommand / ThayDoiTrangThaiCommand
B3: WebApi Model + MappingConfiguration
B4: Postman HoSoDeXuatCapDoCntt
```

### Phase C — `ToTrinhKeHoachUpdate` (không migration DB)

```
C1: ToTrinhKeHoachUpdateDto → DateOnly?
C2: ToTrinhKeHoachUpdateCommand + mapping ToStartOfDayUtc
```

### Phase D — toàn solution (backlog)

Chuyển dần từng module `DateTimeOffset?` → `DateOnly?` ở API (pattern #9602).

---

## 5. Chi tiết từng bước

### Bước 1 – Domain

**File:** `QLDA.Domain/Entities/KySo.cs`

```csharp
public DateTimeOffset? HieuLucTu { get; set; }
public DateTimeOffset? HieuLucDen { get; set; }
```

---

### Bước 2 – Persistence configuration

**File:** `QLDA.Persistence/Configurations/KySoConfiguration.cs`

- Thêm `HasConversion` cho `HieuLucTu`, `HieuLucDen` (mẫu `HopDongConfiguration`).

---

### Bước 3 – Migration

```bash
cd QLDA.Migrator
ef.bat QLDA add KySoHieuLucToDateTimeOffset
ef.bat QLDA update
```

**Yêu cầu:**

- **Không** sửa `AppDbContextModelSnapshot.cs` hay migration cũ thủ công.
- Migration mới do EF generate; nếu sai logic convert → `ef.bat QLDA remove` rồi add lại.

**SQL Server (gợi ý — EF có thể generate tương đương):**

- `AlterColumn` `HieuLucTu`, `HieuLucDen`: `datetime2` → `datetimeoffset`.
- Dữ liệu cũ (nếu có): coi giá trị `datetime2` là **giờ local VN** hoặc **UTC** — **thống nhất một cách** trước khi deploy (khuyến nghị: `AT TIME ZONE 'SE Asia Standard Time'` rồi cast sang `datetimeoffset` UTC, hoặc migrate trên DB test trống).

**Kiểm tra sau migrate:**

```sql
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'KySo'
  AND COLUMN_NAME IN ('HieuLucTu', 'HieuLucDen');
-- Kỳ vọng: datetimeoffset
```

---

### Bước 4 – Application

| File | Thay đổi |
|------|----------|
| `KySoDto.cs` | `DateTime?` → `DateOnly?` cho `HieuLucTu`, `HieuLucDen` |
| `KySoMappings.cs` | `ToStartOfDayUtc()` khi ghi entity; `ToDateOnlyVn()` khi `ToDto` |
| `KySoGetDanhSachQuery.cs` | Không đổi query nếu dùng `Select(e => e.ToDto())` — mapping đủ |

**Không** tạo Model mapping riêng ở WebApi — `QuanLyKySoController` dùng DTO Application (`KySoInsertDto`, `KySoUpdateModel`, `KySoDto`).

---

### Bước 5 – Cập nhật tài liệu feature

- `task-9460-ky-so-crud.md`: bảng field §1.2, snippet entity/DTO.
- `task-9460-noi-dung-da-ky.md` §9: đánh dấu done item DateTime khi xong.

---

### Bước 6 – Verify

```bash
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```

**Postman / API**

| API | Body mẫu | Kỳ vọng |
|-----|----------|---------|
| `POST /api/quan-ly-ky-so` | `"hieuLucTu": "2026-05-01"` | Lưu DB `datetimeoffset` (UTC start-of-day VN) |
| `GET /api/quan-ly-ky-so/{id}` | — | Response `"hieuLucTu": "2026-05-01"` (`DateOnly`) |
| `PUT /api/quan-ly-ky-so` | đổi `hieuLucDen` | Round-trip đúng ngày |

**Regression:** `POST /api/ky-so/them-moi` (upload file) — không đụng field ngày.

---

## 6. Breaking changes

| Đối tượng | Thay đổi |
|-----------|----------|
| Client gọi `QuanLyKySo` | Response `hieuLucTu`/`hieuLucDen`: từ ISO datetime → `yyyy-MM-dd` |
| DB | Cột `KySo.HieuLuc*` đổi kiểu — cần chạy migration trên mọi môi trường |

---

## 7. Checklist

### Phase A — KySo

```
[x] A1. Domain KySo → DateTimeOffset?
[x] A2. KySoConfiguration HasConversion
[ ] A3. Migration + ef update (`ef.bat QLDA add ConvertKySoAndHoSoDateFieldsToDateTimeOffset`)
[x] A4. KySoDto + KySoMappings
[ ] A5. Postman QuanLyKySo
[ ] A6. task-9460-ky-so-crud.md
```

### Phase B — HoSoDeXuatCapDoCntt

```
[x] B1. Domain + Configuration (+ migration chung A3)
[x] B2. Application DTOs + Mappings + Commands (bỏ DateTime.UtcNow)
[x] B3. WebApi Model + MappingConfiguration
[ ] B4. Postman
```

### Phase C — ToTrinhKeHoach Update

```
[x] C1. ToTrinhKeHoachUpdateDto → DateOnly?
[x] C2. UpdateCommand mapping ToStartOfDayUtc
```

---

## 8. Tóm tắt file dự kiến

### Phase A

| Hành động | File |
|-----------|------|
| Sửa | `QLDA.Domain/Entities/KySo.cs` |
| Sửa | `QLDA.Persistence/Configurations/KySoConfiguration.cs` |
| Sửa | `QLDA.Application/KySos/DTOs/KySoDto.cs` |
| Sửa | `QLDA.Application/KySos/DTOs/KySoMappings.cs` |
| Mới (EF) | `QLDA.Migrator/Migrations/*_KySoHieuLucToDateTimeOffset.*` |
| Sửa doc | `docs/feature/KySo/task-9460-ky-so-crud.md` |

### Phase B (thêm)

| Hành động | File |
|-----------|------|
| Sửa | `QLDA.Domain/Entities/HoSoDeXuatCapDoCntt.cs` |
| Sửa | `QLDA.Persistence/Configurations/HoSoDeXuatCapDoCnttConfiguration.cs` |
| Sửa | `QLDA.Application/HoSoDeXuatCapDoCntts/DTOs/*` (Insert, Update, Dto, Mappings) |
| Sửa | `QLDA.Application/HoSoDeXuatCapDoCntts/Commands/HoSoDeXuatCapDoCnttTrinhCommand.cs` |
| Sửa | `QLDA.Application/HoSoDeXuatCapDoCntts/Commands/HoSoDeXuatCapDoCnttThayDoiTrangThaiCommand.cs` |
| Sửa | `QLDA.WebApi/Models/HoSoDeXuatCapDoCntts/HoSoDeXuatCapDoCnttModel.cs` |
| Sửa | `QLDA.WebApi/Models/HoSoDeXuatCapDoCntts/HoSoDeXuatCapDoCnttMappingConfiguration.cs` |
| Mới (EF) | `*_HoSoDeXuatCapDoCnttNgayTrinhToDateTimeOffset.*` |

### Phase C

| Sửa | `ToTrinhKeHoachUpdateDto.cs`, `ToTrinhKeHoachUpdateCommand.cs` |

**Không sửa (Ký số upload):** `KySoInsertDto`, `KySoUpdateModel`, `KySoController`, `NoiDungDaKyCommand`.

---

## 9. Trạng thái

- ✅ **Code** Phase A + B + C (21/05/2026)
- ⏳ **Migration:** chạy `ef.bat QLDA add ConvertKySoAndHoSoDateFieldsToDateTimeOffset` rồi `ef.bat QLDA update` (hoặc `--sqlite` trên máy dev)
- ⏳ **Verify:** `dotnet build` + Postman
