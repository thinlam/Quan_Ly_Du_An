# Spec kỹ thuật — Fix triệt để phiếu trình KH triển khai hạng mục

**Module:** QLDA  
**Trạng thái:** ✅ **IMPLEMENTED**  
**Effort:** ~1h (BE only, không migration)  
**Ngày:** 2026-07-08  
**Cập nhật:** 2026-07-10 — DTO ngày dùng `DateOnly?`, format tại tầng export

---

## Mục lục

1. [Tóm tắt](#1-tóm-tắt)
2. [Luồng dữ liệu UI vs Print](#2-luồng-dữ-liệu-ui-vs-print)
3. [Root cause](#3-root-cause)
4. [Các bước code](#4-các-bước-code)
5. [Thay đổi đã implement](#5-thay-đổi-đã-implement)
6. [Test plan & kết quả](#6-test-plan--kết-quả)
7. [Checklist trước merge](#7-checklist-trước-merge)

---

## 1. Tóm tắt

### 1.1 API

| Thuộc tính | Giá trị |
| ---------- | ------- |
| Method | `GET` |
| URL | `/api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc` |
| Param | `id` — `Guid` kế hoạch triển khai |
| Controller | `PrintController.InPhieuTrinhKeHoachTrienKhaiHangMuc` |
| Output | Word `.docx` |

### 1.2 Yêu cầu BA

| Cột | Trước fix | Sau fix |
| --- | --------- | ------- |
| **Bắt đầu** | `2026-07-08` | `08/07/2026` |
| **Kết thúc** | `2026-07-28` | `28/07/2026` |
| **Cán bộ chủ trì** | Sai người (join `UserMaster.Id`) | Khớp UI |
| **Cán bộ phối hợp** | Sai người | Khớp UI |

**Ràng buộc:**

- DTO export giữ **`DateOnly?`** — cùng kiểu với entity, không convert sang `string` rồi parse lại.
- Format `dd/MM/yyyy` chỉ khi **render** (Word / Excel), không format trong `ExportMappings`.
- `null` → ô trống; không in `01/01/0001`.
- Print phải dùng **cùng logic ID** với UI combobox (`UserPortalId`).

---

## 2. Luồng dữ liệu UI vs Print

```mermaid
sequenceDiagram
    participant UI as Frontend combobox
    participant GetQ as KeHoachTrienKhaiHangMucGetQuery
    participant DB as HangMucKeHoach
    participant Map as ExportMappings
    participant Word as WordExporter
    participant Excel as AsposeHelper
    participant Docx as File .docx / .xlsx

    UI->>DB: Lưu CanBoChuTriId = UserPortalId
    GetQ->>DB: Trả Id (PortalId) + DateOnly ngày
    UI->>UI: Hiển thị tên từ combobox + format ngày dd/MM/yyyy

    DB->>Map: ToExportRowsAsync
    Map->>Map: Join user WHERE UserPortalId IN ids
    Map->>Map: MapItem — gán DateOnly? trực tiếp
    Map->>Word: ExportItemDto (DateOnly? + tên cán bộ)
    Word->>Word: FormatDate(DateOnly?) → "dd/MM/yyyy"
    Word->>Docx: Run text thuần

    Map->>Excel: ExportItemDto (cùng DTO)
    Excel->>Excel: PutValueSmart(DateOnly) → "dd/MM/yyyy"
    Excel->>Docx: Cell literal
```

### 2.1 Đối chiếu ID cán bộ

| Tầng | Field lưu | Join / lookup |
| ---- | --------- | ------------- |
| **UI combobox** | `UserMasterDto.Id = UserPortalId` | `UserMasterGetComboboxQueryHandler` |
| **Form save** | `HangMucKeHoach.CanBoChuTriId` | Giá trị từ combobox = PortalId |
| **Import Excel** | `CanBoChuTriId = chuTriUser.PortalId` | `ImportRangeCommand` (sau fix #import-can-bo-map) |
| **Print (sai cũ)** | `userIds.Contains(u.Id)` | Map nhầm sang user khác có cùng số PK |
| **Print (đúng)** | `userIds.Contains(u.UserPortalId.Value)` | Khớp UI |

**Ví dụ bug:**

```csharp
// ❌ Sai — nếu CanBoChuTriId đang là UserPortalId
.Where(u => userIds.Contains(u.Id))

// ✅ Đúng
.Where(u => u.UserPortalId.HasValue && userIds.Contains(u.UserPortalId.Value))
```

### 2.2 Files trong luồng

| Tầng | File | Vai trò |
| ---- | ---- | ------- |
| Domain | `HangMucKeHoach.cs` | `DateOnly?`, `CanBoChuTriId` — không đổi |
| Application | `KeHoachTrienKhaiHangMucExportMappings.cs` | Map `DateOnly?` trực tiếp + lookup PortalId |
| Application | `KeHoachTrienKhaiHangMucExportItemDto.cs` | `NgayBatDau`/`NgayKetThuc` = `DateOnly?` |
| Application | `KeHoachTrienKhaiHangMucGetPhieuTrinhPrintQuery.cs` | Load + gọi `ToExportRowsAsync` |
| Infrastructure | `KeHoachTrienKhaiHangMucWordExporter.cs` | `FormatDate(DateOnly?)` khi bind cell |
| BuildingBlocks | `AsposeHelper.cs` | `PutValueSmart` format `DateOnly` → `dd/MM/yyyy` (Excel) |
| BuildingBlocks | `UserMasterGetComboboxQueryHandler.cs` | UI: `Id = UserPortalId` |

### 2.3 Phân tách trách nhiệm format ngày

| Tầng | Trách nhiệm | Không làm |
| ---- | ----------- | --------- |
| **Application / DTO** | Giữ `DateOnly?` thuần | Không `ToString`, không `FormatDate` |
| **Infrastructure / Word** | `FormatDate(DateOnly?)` → text `dd/MM/yyyy` | Không nhận `DateTime?` |
| **BuildingBlocks / Excel** | `PutValueSmart` nhận `DateOnly` | Không để `ToString()` mặc định ISO |

---

## 3. Root cause

### 3.1 Ngày sai format

**Hai lớp nguyên nhân (fix theo thứ tự):**

1. **WordExporter** (fix lần 1): `FormatDate` hard-code `"yyyy-MM-dd"`.
2. **Export DTO** (fix triệt để): truyền `DateTime?` — Aspose/serialization render ISO trước khi Word format.

**Giải pháp cuối cùng (2026-07-10):**

- DTO dùng **`DateOnly?`** — cùng kiểu với `HangMucKeHoach`, không qua `string` trung gian.
- `ExportMappings` gán trực tiếp: `NgayBatDau = hangMuc.NgayBatDau`.
- Format `dd/MM/yyyy` tại **tầng export**:
  - Word: `KeHoachTrienKhaiHangMucWordExporter.FormatDate(DateOnly?)`
  - Excel: `AsposeHelper.PutValueSmart` xử lý `DateOnly`

> **Lý do không dùng `string?` trong DTO:** truyền ngày thì giữ `DateOnly`; format là concern của renderer, không phải mapper.

### 3.2 Cán bộ sai người

| Giả thuyết | Kết quả |
| ---------- | ------- |
| Thứ tự hạng mục in sai | ❌ Không phải — tên sai người khác hẳn |
| Join theo `UserMaster.Id` | ✅ **Root cause** — PortalId ≠ MasterId |
| DB lưu sai ID | ⚠️ Dữ liệu cũ (pre-import-fix) có thể vẫn là MasterId |

### 3.3 Không phải nguyên nhân

| Giả thuyết | Kết quả |
| ---------- | ------- |
| Template Word có DATE field | ❌ Bảng fill bằng `Run` text thuần |
| `GetQuery` trả tên cán bộ | ❌ Chỉ trả ID — FE resolve từ combobox |
| Excel export sai | ❌ Cùng DTO — `PutValueSmart` format `DateOnly` |

---

## 4. Các bước code

Thứ tự sửa **bắt buộc** theo dependency: DTO → Mapping → WordExporter → AsposeHelper → Unit test.

| Bước | File | Mục tiêu |
| ---- | ---- | -------- |
| 1 | `KeHoachTrienKhaiHangMucExportItemDto.cs` | `NgayBatDau`/`NgayKetThuc` = `DateOnly?` |
| 2 | `KeHoachTrienKhaiHangMucExportMappings.cs` | Gán `DateOnly?` trực tiếp + lookup `UserPortalId` |
| 3 | `KeHoachTrienKhaiHangMucWordExporter.cs` | `FormatDate(DateOnly?)` khi bind cell |
| 4 | `AsposeHelper.cs` | `PutValueSmart` xử lý `DateOnly` → `dd/MM/yyyy` |
| 5 | `KeHoachTrienKhaiHangMucExportMappingsTests.cs` | Cover `DateOnly?` + cán bộ |
| 6 | (tuỳ chọn) `KeHoachTrienKhaiHangMucWordExporterTests.cs` | Xóa nếu chỉ test `FormatDate(DateTime?)` cũ |

**Không sửa:** entity `HangMucKeHoach`, migration, template `.docx`, `GetPhieuTrinhPrintQuery` (đã gọi `ToExportRowsAsync`).

---

### Bước 1 — Đổi kiểu ngày trên DTO export

**File:** `QLDA.Application/KeHoachTrienKhaiHangMuc/DTOs/KeHoachTrienKhaiHangMucExportItemDto.cs`

**Trước (gốc bug):**

```csharp
public DateTime? NgayBatDau { get; set; }
public DateTime? NgayKetThuc { get; set; }
```

**Sau (trạng thái hiện tại):**

```csharp
public DateOnly? NgayBatDau { get; set; }
public DateOnly? NgayKetThuc { get; set; }
```

> Lý do: cùng kiểu với entity `HangMucKeHoach`; không convert `DateOnly` → `string` rồi format ngược lại trong Application layer.

---

### Bước 2 — Map `DateOnly?` trực tiếp + lookup cán bộ theo `UserPortalId`

**File:** `QLDA.Application/KeHoachTrienKhaiHangMuc/KeHoachTrienKhaiHangMucExportMappings.cs`

#### 2a. Lookup user (trong `ToExportRowsAsync`)

**Trước (sai nếu ID lưu là PortalId):**

```csharp
var users = userIds.Count == 0
    ? []
    : await userRepo.GetQueryableSet(OnlyUsed: false, OnlyNotDeleted: false)
        .AsNoTracking()
        .Where(u => userIds.Contains(u.Id))
        .Select(u => new { u.Id, u.HoTen })
        .ToListAsync(cancellationToken);

// ...
users.ToDictionary(u => u.Id, u => u.HoTen ?? string.Empty)
```

**Sau (khớp UI combobox):**

```csharp
// CanBoChuTriId / CanBoPhoiHopIds store UserPortalId (same as UI combobox).
var users = userIds.Count == 0
    ? []
    : await userRepo.GetQueryableSet(OnlyUsed: false, OnlyNotDeleted: false)
        .AsNoTracking()
        .Where(u => u.UserPortalId.HasValue && userIds.Contains(u.UserPortalId.Value))
        .Select(u => new { PortalId = u.UserPortalId!.Value, u.HoTen })
        .ToListAsync(cancellationToken);

// ...
users.ToDictionary(u => u.PortalId, u => u.HoTen ?? string.Empty)
```

#### 2b. `MapItem` — gán `DateOnly?` trực tiếp, không format

**Trước:**

```csharp
NgayBatDau = ToDateTime(hangMuc.NgayBatDau),
NgayKetThuc = ToDateTime(hangMuc.NgayKetThuc),
```

**Sau:**

```csharp
NgayBatDau = hangMuc.NgayBatDau,
NgayKetThuc = hangMuc.NgayKetThuc,
CanBoChuTri = ResolveName(hangMuc.CanBoChuTriId, userTenById),
CanBoPhoiHop = JoinNames(hangMuc.CanBoPhoiHopIds, userTenById),
```

#### 2c. Xóa helper convert/format ngày khỏi Mapping

**Không còn** trong `ExportMappings`:

```csharp
// ❌ Đã xóa — không format tại Application layer
private static DateTime? ToDateTime(DateOnly? date) =>
    date?.ToDateTime(TimeOnly.MinValue);

internal static string FormatDate(DateOnly? value) =>
    value.HasValue ? value.Value.ToString("dd/MM/yyyy") : string.Empty;
```

**Diff tóm tắt:**

```diff
// KeHoachTrienKhaiHangMucExportMappings.cs
- .Where(u => userIds.Contains(u.Id))
- .Select(u => new { u.Id, u.HoTen })
+ .Where(u => u.UserPortalId.HasValue && userIds.Contains(u.UserPortalId.Value))
+ .Select(u => new { PortalId = u.UserPortalId!.Value, u.HoTen })

- users.ToDictionary(u => u.Id, u => u.HoTen ?? string.Empty)
+ users.ToDictionary(u => u.PortalId, u => u.HoTen ?? string.Empty)

- NgayBatDau = ToDateTime(hangMuc.NgayBatDau),
- NgayKetThuc = ToDateTime(hangMuc.NgayKetThuc),
+ NgayBatDau = hangMuc.NgayBatDau,
+ NgayKetThuc = hangMuc.NgayKetThuc,

- private static DateTime? ToDateTime(...) { ... }
- internal static string FormatDate(...) { ... }
```

---

### Bước 3 — WordExporter format khi render

**File:** `QLDA.Infrastructure/Offices/KeHoachTrienKhaiHangMucWordExporter.cs`

#### 3a. `CreateItemRow` — gọi `FormatDate(DateOnly?)`

```csharp
FormatDate(data.NgayBatDau),
FormatDate(data.NgayKetThuc),
```

Cột cán bộ **giữ nguyên** (đã là string từ mapping):

```csharp
data.CanBoChuTri ?? string.Empty,
data.CanBoPhoiHop ?? string.Empty,
```

#### 3b. Helper format ngày ở Infrastructure

```csharp
private static string FormatDate(DateOnly? date) =>
    date?.ToString("dd/MM/yyyy", ViCulture) ?? string.Empty;
```

- `null` → `""` (ô trống).
- Không còn `FormatDate(DateTime?)` — tránh double-format / nhầm ISO.

**Diff tóm tắt:**

```diff
// KeHoachTrienKhaiHangMucWordExporter.cs — CreateItemRow
- FormatDate(data.NgayBatDau),   // DateTime? — ISO bug
- FormatDate(data.NgayKetThuc),
+ FormatDate(data.NgayBatDau),   // DateOnly? — format tại export
+ FormatDate(data.NgayKetThuc),

- internal static string FormatDate(DateTime? date) =>
-     date?.ToString("dd/MM/yyyy", ViCulture) ?? string.Empty;
+ private static string FormatDate(DateOnly? date) =>
+     date?.ToString("dd/MM/yyyy", ViCulture) ?? string.Empty;
```

---

### Bước 4 — Excel: `PutValueSmart` nhận `DateOnly`

**File:** `BuildingBlocks/src/BuildingBlocks.Infrastructure/Offices/AsposeHelper.cs`

Thêm nhánh xử lý `DateOnly` — tránh `ToString()` mặc định ra ISO `yyyy-MM-dd`:

```csharp
else if (val is DateOnly dateOnly)
{
    cell.PutValue(dateOnly.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
}
```

Excel export (`GET /api/print/ke-hoach-trien-khai-hang-muc`) dùng chung `ExportItemDto` qua `ExcelHelper.Export` → `PutValueSmart`.

---

### Bước 5 — Unit test

**File:** `QLDA.Tests/Unit/KeHoachTrienKhaiHangMucExportMappingsTests.cs`

Thêm / giữ các test sau:

#### 5a. Cán bộ chủ trì theo PortalId

```csharp
[Fact]
public void ToExportRows_ResolvesCanBoByUserPortalId()
{
    const int giaiDoanId = 1;
    const long portalId = 50_001;

    var hangMucs = new List<HangMucKeHoach>
    {
        new()
        {
            Id = Guid.NewGuid(),
            TenHangMuc = "HM export",
            GiaiDoanId = giaiDoanId,
            CanBoChuTriId = portalId,
            CreatedAt = DateTimeOffset.UtcNow,
        },
    };

    var rows = KeHoachTrienKhaiHangMucExportMappings.ToExportRows(
        hangMucs,
        new Dictionary<int, string> { [giaiDoanId] = "Giai đoạn A" },
        new Dictionary<int, int> { [giaiDoanId] = 1 },
        new Dictionary<long, string>(),
        new Dictionary<long, string> { [portalId] = "Đào Thị Bích Tuyền" });

    rows.Single(r => !r.IsGroupHeader).CanBoChuTri.Should().Be("Đào Thị Bích Tuyền");
}
```

#### 5b. Cán bộ phối hợp theo PortalId

```csharp
[Fact]
public void ToExportRows_ResolvesCanBoPhoiHopByUserPortalId()
{
    const int giaiDoanId = 1;
    const long portalId = 50_002;

    var hangMucs = new List<HangMucKeHoach>
    {
        new()
        {
            Id = Guid.NewGuid(),
            TenHangMuc = "HM export",
            GiaiDoanId = giaiDoanId,
            CanBoPhoiHopIds = [portalId],
            CreatedAt = DateTimeOffset.UtcNow,
        },
    };

    var rows = KeHoachTrienKhaiHangMucExportMappings.ToExportRows(
        hangMucs,
        new Dictionary<int, string> { [giaiDoanId] = "Giai đoạn A" },
        new Dictionary<int, int> { [giaiDoanId] = 1 },
        new Dictionary<long, string>(),
        new Dictionary<long, string> { [portalId] = "Đặng Trung Nghĩa" });

    rows.Single(r => !r.IsGroupHeader).CanBoPhoiHop.Should().Be("Đặng Trung Nghĩa");
}
```

#### 5c. Map `DateOnly?` trực tiếp (không format trong Mapping)

```csharp
[Fact]
public void ToExportRows_MapsDateOnlyValues()
{
    const int giaiDoanId = 1;

    var hangMucs = new List<HangMucKeHoach>
    {
        new()
        {
            Id = Guid.NewGuid(),
            TenHangMuc = "HM có ngày",
            GiaiDoanId = giaiDoanId,
            NgayBatDau = new DateOnly(2026, 7, 8),
            NgayKetThuc = new DateOnly(2026, 7, 28),
            CreatedAt = DateTimeOffset.UtcNow,
        },
    };

    var rows = KeHoachTrienKhaiHangMucExportMappings.ToExportRows(
        hangMucs,
        new Dictionary<int, string> { [giaiDoanId] = "Giai đoạn A" },
        new Dictionary<int, int> { [giaiDoanId] = 1 },
        new Dictionary<long, string>(),
        new Dictionary<long, string>());

    var item = rows.Single(r => !r.IsGroupHeader);
    item.NgayBatDau.Should().Be(new DateOnly(2026, 7, 8));
    item.NgayKetThuc.Should().Be(new DateOnly(2026, 7, 28));
}
```

#### 5d. Ngày null → `null` (format tại export layer)

```csharp
[Fact]
public void ToExportRows_NullDates_ReturnNull()
{
    const int giaiDoanId = 1;

    var hangMucs = new List<HangMucKeHoach>
    {
        new()
        {
            Id = Guid.NewGuid(),
            TenHangMuc = "HM không ngày",
            GiaiDoanId = giaiDoanId,
            CreatedAt = DateTimeOffset.UtcNow,
        },
    };

    var rows = KeHoachTrienKhaiHangMucExportMappings.ToExportRows(
        hangMucs,
        new Dictionary<int, string> { [giaiDoanId] = "Giai đoạn A" },
        new Dictionary<int, int> { [giaiDoanId] = 1 },
        new Dictionary<long, string>(),
        new Dictionary<long, string>());

    var item = rows.Single(r => !r.IsGroupHeader);
    item.NgayBatDau.Should().BeNull();
    item.NgayKetThuc.Should().BeNull();
}
```

> Format `dd/MM/yyyy` được verify qua manual test Word/Excel — không assert string trong Mapping test.

#### 5e. Dọn test Word cũ (nếu còn)

Nếu vẫn còn `QLDA.Tests/Unit/KeHoachTrienKhaiHangMucWordExporterTests.cs` chỉ assert `FormatDate(DateTime?)` cũ → **xóa file**.

---

### Bước 6 — Verify build + test

```bash
dotnet build QLDA.Application/QLDA.Application.csproj
dotnet build QLDA.Infrastructure/QLDA.Infrastructure.csproj
dotnet build QLDA.Tests/QLDA.Tests.csproj
dotnet test QLDA.Tests/QLDA.Tests.csproj \
  --filter "FullyQualifiedName~KeHoachTrienKhaiHangMucExportMappings"
```

Kỳ vọng: `Passed! — Failed: 0, Passed: 6` (hoặc ≥ 6 nếu thêm test khác).

> **Restart WebApi** sau deploy để load DLL mới trước khi in thủ công.

---

## 5. Thay đổi đã implement

### 5.1 Files đã thay đổi

| # | File | Thay đổi |
| - | ---- | -------- |
| 1 | `KeHoachTrienKhaiHangMucExportItemDto.cs` | `NgayBatDau`/`NgayKetThuc`: `DateTime?` → **`DateOnly?`** |
| 2 | `KeHoachTrienKhaiHangMucExportMappings.cs` | Gán `DateOnly?` trực tiếp; lookup `UserPortalId`; xóa `FormatDate` |
| 3 | `KeHoachTrienKhaiHangMucWordExporter.cs` | `FormatDate(DateOnly?)` khi bind cell |
| 4 | `AsposeHelper.cs` | `PutValueSmart` xử lý `DateOnly` → `dd/MM/yyyy` |
| 5 | `KeHoachTrienKhaiHangMucExportMappingsTests.cs` | + tests `DateOnly?` + cán bộ PortalId |
| 6 | `KeHoachTrienKhaiHangMucWordExporterTests.cs` | Xóa (chuyển sang ExportMappings) |

### 5.2 Excel export

Excel (`GET /api/print/ke-hoach-trien-khai-hang-muc`) dùng chung `ExportItemDto` / `ExportMappings`.  
`PutValueSmart` format `DateOnly` → `08/07/2026` literal — không regression ISO.

### 5.3 Tiến hóa thiết kế DTO ngày

| Phiên bản | Kiểu DTO | Format ở đâu | Ghi chú |
| --------- | -------- | ------------- | ------- |
| Gốc (bug) | `DateTime?` | Word `yyyy-MM-dd` | ISO trên file in |
| Trung gian | `string?` | `ExportMappings.FormatDate` | Hoạt động nhưng convert thừa |
| **Hiện tại** | **`DateOnly?`** | Word + `PutValueSmart` | Đúng kiểu domain, format tại export |

---

## 6. Test plan & kết quả

| # | Case | Test | Kỳ vọng |
| - | ---- | ---- | ------- |
| T1 | Ngày có giá trị | `ToExportRows_MapsDateOnlyValues` | `DateOnly(2026,7,8)` / `(2026,7,28)` |
| T2 | Ngày null | `ToExportRows_NullDates_ReturnNull` | `null` |
| T3 | Chủ trì PortalId | `ToExportRows_ResolvesCanBoByUserPortalId` | Tên đúng |
| T4 | Phối hợp PortalId | `ToExportRows_ResolvesCanBoPhoiHopByUserPortalId` | Tên đúng |
| T5 | In Word manual | curl + mở docx | `dd/MM/yyyy`, khớp UI |
| T6 | In Excel manual | curl + mở xlsx | `dd/MM/yyyy`, không ISO |
| T7 | Dữ liệu cũ MasterId | Manual | Cần re-import |

### 6.1 Lệnh chạy test

```bash
dotnet build QLDA.Tests/QLDA.Tests.csproj
dotnet test QLDA.Tests/QLDA.Tests.csproj \
  --filter "FullyQualifiedName~KeHoachTrienKhaiHangMucExportMappings"
```

### 6.2 Manual test

```bash
curl --location \
  'http://localhost:5000/api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc?id={KE_HOACH_ID}' \
  --header 'Authorization: Bearer {TOKEN}' \
  --output phieu-trinh-test.docx
```

Kiểm tra bảng **Nội dung kế hoạch triển khai**:

- Cột **Bắt đầu** / **Kết thúc** → `dd/MM/yyyy`
- Cột **Cán bộ chủ trì** / **Cán bộ phối hợp** → khớp màn hình chi tiết KH

---

## 7. Checklist trước merge

- [x] DTO `NgayBatDau`/`NgayKetThuc` = `DateOnly?` (không `string?`, không `DateTime?`)
- [x] `ExportMappings` gán `DateOnly?` trực tiếp — không `FormatDate`
- [x] Word `FormatDate(DateOnly?)` → `dd/MM/yyyy`
- [x] Excel `PutValueSmart` xử lý `DateOnly`
- [x] Lookup cán bộ theo `UserPortalId`
- [x] Unit tests pass
- [x] Manual: mở `.docx` xác nhận ngày + tên cán bộ
- [x] Restart WebApi sau deploy

---

## Phụ lục — Cấu trúc bảng Word (11 cột)

| Index | Header | Field DTO | Kiểu DTO | Format khi render |
| ----- | ------ | --------- | -------- | ----------------- |
| 5 | **Bắt đầu** | `NgayBatDau` | `DateOnly?` | `dd/MM/yyyy` |
| 6 | **Kết thúc** | `NgayKetThuc` | `DateOnly?` | `dd/MM/yyyy` |
| 8 | Cán bộ chủ trì | `CanBoChuTri` | `string?` | `HoTen` via PortalId |
| 9 | Cán bộ phối hợp | `CanBoPhoiHop` | `string?` | `HoTen` via PortalId |

---

*Cập nhật sau implement — July 10, 2026 (DTO `DateOnly?`, format tại export layer).*
