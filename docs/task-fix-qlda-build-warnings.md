# Task — Làm sạch build warnings QLDA

**Ngày tạo:** June 2026  
**Trạng thái:** ✅ **DONE** — `dotnet build --no-restore` tại `QLDA.WebApi`: **0 warning(s)**, 0 error

---

## Mục tiêu

Giảm rõ rệt số warning khi build solution QLDA mà **không đổi logic nghiệp vụ**, **không sửa migration / `AppDbContextModelSnapshot`**, **không dùng `#pragma warning disable`** hoặc tắt nullable project-wide.

Sau mỗi nhóm sửa: chạy lại `dotnet build` và ghi lại số warning còn lại.

---

## Ràng buộc bắt buộc

| Không làm | Lý do |
|-----------|--------|
| Sửa file trong `QLDA.Migrator/Migrations/` | Immutable snapshot DB |
| Sửa `AppDbContextModelSnapshot.cs` | Quy tắc dự án |
| `#pragma warning disable` / `<Nullable>disable</Nullable>` | Che warning, không xử lý gốc |
| Đổi schema DB nếu không thật sự cần | Ngoài phạm vi task |
| Sửa logic nghiệp vụ vì warning style/nullability | Chỉ annotate / init / guard an toàn |

---

## Baseline — Phân bổ warning theo project

| Project | Warnings | Ghi chú |
|---------|----------|---------|
| `QLDA.Application` | **156** | Trọng tâm — nullability, ToDictionary, field chưa inject |
| `QLDA.WebApi` | **31** | Models CS8618, duplicate using, XML comment |
| `QLDA.Domain` | **17** | Entity CS8618, 1 CS0108, 1 CS8619 |
| `BuildingBlocks.Infrastructure` | **8** | CS0618 Aspose `EnsureLicense` |
| Các project khác | **0** | BuildingBlocks.*, QLDA.Persistence, QLDA.Infrastructure |

---

## Baseline — Phân loại theo mã CS

| Mã | Mô tả ngắn | Ước lượng | Nhóm xử lý |
|----|------------|-----------|------------|
| **CS8618** | Non-nullable property/field chưa init | ~45 | 1 — Nullability (init) |
| **CS8602** | Dereference possibly null | ~35 | 1 — Nullability |
| **CS8604** | Possible null argument | ~25 | 1 — Nullability |
| **CS8601** | Possible null assignment | ~20 | 1 — Nullability |
| **CS8714** | `string?` làm TKey `ToDictionary` | ~15 | 8 — Dictionary key |
| **CS8621** | Lambda return nullability mismatch (kèm CS8714) | ~15 | 8 — Dictionary key |
| **CS0105** | Duplicate `using` | **10** | 3 — Duplicate using |
| **CS0618** | Obsolete `EnsureLicense` | **8** | 5 — Aspose |
| **CS0649** | Field never assigned | **6** | 7 — Field never assigned |
| **CS0168** | Variable declared but never used | **5** | 6 — Unused variable |
| **CS0162** | Unreachable code | **2** | 6 — Unreachable code |
| **CS8603** | Possible null return | **2** | 1 — Nullability |
| **CS1572 / CS1573** | XML param mismatch | **2** | 4 — XML comment |
| **CS0169** | Field never used | **1** | 6 — Unused field |
| **CS0108** | Member hides inherited | **1** | 1 — Nullability (keyword `new`) |
| **CS8619** | Nullability collection mismatch | **1** | 1 — Nullability |
| **CS8620** | Include nullability variance | **1** | 9 — Include/Select |
| **CS8714** (AuthorizationBehavior) | Generic nullability | **1** | Ghi chú — có thể để sau |

> **CancellationToken:** Không xuất hiện trong baseline build output dưới dạng warning riêng. Phase riêng sẽ **quét thủ công** các handler/controller async thiếu truyền token (nhóm 2).

---

## Thứ tự thực hiện đề xuất

Ưu tiên nhóm **ít rủi ro / nhanh** trước, sau đó nullability hàng loạt theo layer.

```
Phase A  → Duplicate using (CS0105)           ~10 warnings, ~15 phút
Phase B  → Aspose obsolete (CS0618)          8 warnings, ~10 phút
Phase C  → XML comment (CS1572/CS1573)       2 warnings, ~5 phút
Phase D  → Unused / unreachable (CS0168…)    ~8 warnings, ~20 phút
Phase E  → Field never assigned (CS0649…)    ~7 warnings, ~30 phút
Phase F  → Dictionary nullable key (CS8714)  ~30 warnings, ~45 phút
Phase G  → QLDA.Domain entities (CS8618…)     17 warnings, ~30 phút
Phase H  → QLDA.WebApi Models (CS8618…)     ~15 warnings, ~30 phút
Phase I  → QLDA.Application nullability     phần còn lại, ~2–3 giờ
Phase J  → CancellationToken audit          quét + sửa từng module
```

Sau **mỗi phase**: `cd QLDA.WebApi && dotnet build` → cập nhật bảng tiến độ cuối doc.

---

## Phase A — Duplicate using (CS0105)

**Cách sửa:** Xóa dòng `using` trùng trong cùng file. Không đổi namespace.

| File | Using trùng |
|------|-------------|
| `QLDA.Application/BanGiaoHoSos/Queries/BanGiaoHoSoGetDanhSachQuery.cs` | `QLDA.Application.TepDinhKems.DTOs` |
| `QLDA.Application/DeXuatNhuCauKinhPhi/DTOs/DeXuatNhuCauKinhPhiDto.cs` | `QLDA.Application.Common.Interfaces` |
| `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchChuyenCommand.cs` | `QLDA.Application.ChuTruongLapKeHoachs.Commands` |
| `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchDuyetCommand.cs` |同上 |
| `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchTraLaiCommand.cs` |同上 |
| `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchTrinhCommand.cs` |同上 |
| `QLDA.WebApi/Controllers/DeXuatChuTruongChuyenTiepController.cs` | `DeXuatChuyenTieps.Commands`, `.DTOs`, `.Queries` |

**Kỳ vọng:** −10 warnings.

---

## Phase B — Aspose obsolete (CS0618)

**Interface:** `BuildingBlocks.CrossCutting/Offices/IAsposeHelper.cs`

```csharp
[Obsolete("Use EnsureCellsLicense or EnsureWordsLicense")]
void EnsureLicense(ref bool isLicenseSet);

void EnsureCellsLicense();  // Excel / Aspose.Cells
void EnsureWordsLicense();  // Word / Aspose.Words
```

**Cách sửa:** Trong `ExcelHelper.cs` và `ExcelImporter.cs` (BuildingBlocks.Infrastructure):

- Thay `_asposeHelper.EnsureLicense(ref _isLicenseSet)` → `_asposeHelper.EnsureCellsLicense()`
- Có thể **xóa field** `bool _isLicenseSet` nếu không còn dùng (method mới idempotent theo XML doc)
- **Không** gọi `EnsureWordsLicense` ở file Excel — chỉ Cells

| File | Số chỗ |
|------|--------|
| `BuildingBlocks.Infrastructure/Offices/ExcelHelper.cs` | 7 |
| `BuildingBlocks.Infrastructure/Offices/ExcelImporter.cs` | 1 |

**Kỳ vọng:** −8 warnings.

---

## Phase C — XML comment (CS1572 / CS1573)

| File | Vấn đề | Sửa |
|------|--------|-----|
| `QLDA.WebApi/Controllers/KeHoachLuaChonNhaThauRutGonController.cs` ~L93–104 | `<param name="updateDto">` không khớp tham số `Dto` | Đổi thành `<param name="Dto">` hoặc rename param method → `updateDto` (ưu tiên sửa XML cho khớp tên hiện tại) |

Tham số thực tế method `Update`:

```csharp
Update([FromBody] KeHoachLuaChonNhaThauRutGonDto Dto,
       [FromServices] IUnitOfWork unitOfWork,
       CancellationToken cancellationToken = default)
```

XML phải có: `Dto`, `unitOfWork`, `cancellationToken`.

**Kỳ vọng:** −2 warnings.

---

## Phase D — Unused variable / unreachable code

### CS0168 — `catch` không dùng biến

| File | Sửa |
|------|-----|
| `DuToanDauTu/Commands/DuToanDauTuTraLaiCommand.cs` | `catch (DbUpdateException)` và `catch (Exception)` — bỏ tên biến |
| `KeHoachTrienKhaiChiTietDuAn/Commands/KeHoachTrienKhaiChiTietDuAnUpdateCommand.cs` | `catch (Exception)` |
| `KeHoachTrienKhaiHangMuc/Commands/KeHoachTrienKhaiHangMucUpdateCommand.cs` | `catch (Exception)` |
| `KeHoachLuaChonNhaThauRutGon/Commands/KeHoachLuaChonNhaThauRutGonInsertCommand.cs` | `catch (Exception)` |
| `TongHopDeXuatChuTruong/Queries/TongHopDeXuatChuTruongGetDanhSachQuery.cs` | `catch (Exception)` |

> Đọc từng `catch`: nếu block rỗng hoặc chỉ rethrow thì giữ hành vi; **không** thêm log mới trừ khi đã có pattern trong file.

### CS0162 — Unreachable code

| File | Nguyên nhân | Hướng xử lý |
|------|-------------|-------------|
| `QLDA.Application/Common/Extensions/VisibilityFilterExtensions.cs` L23, L49 | `return query;` ngay sau comment `//Tạm tắt` — code filter phía dưới không chạy | **Không xóa logic filter** (nghiệp vụ tạm tắt). Wrap phần filter sau `return` trong `#if false` **không được** (che warning). **Đề xuất:** refactor thành `if (!_enabled) return query;` với `private const bool VisibilityFilterEnabled = false;` — hành vi giữ nguyên, hết unreachable. |

**Kỳ vọng:** −7 warnings.

---

## Phase E — Field never assigned (CS0649 / CS0169)

Đọc handler trước khi sửa — phân biệt **field thừa** vs **thiếu inject**.

| File | Field | Phân tích / hành động |
|------|-------|----------------------|
| `ToTrinhPheDuyet/Commands/ToTrinhPheDuyetTraLaiCommand.cs` | `_dbContext`, `_duAnBuocRepo`, `_auth` | Constructor chỉ resolve qua `IServiceProvider` một phần — **thêm** `GetRequiredService` cho 3 field hoặc **xóa** field + dùng local nếu code không reference |
| `HoSoDeXuatCapDoCnttDuyetCommand.cs` | `_settings` | Constructor thiếu `_settings = serviceProvider.GetRequiredService<IAppSettingsProvider>()` — **đây là bug tiềm ẩn** (check `if (_settings == null)` luôn true). **Bắt buộc inject**, không xóa field |
| `HoSoDeXuatCapDoCnttTraLaiCommand.cs` | `_settings` |同上 |
| `GoiThaus/Commands/GoiThauDeleteCommand.cs` | `_KetQuaTrungThau` | Kiểm tra có dùng trong `Handle` không — inject hoặc xóa |
| `HopDongs/Commands/HopDongDeleteCommand.cs` | `PhuLucHopDong` | CS0169 — field không dùng → **xóa** |

**Kỳ vọng:** −7 warnings; fix `_settings` là correction an toàn (hiện handler luôn throw hoặc path sai).

---

## Phase F — Dictionary key nullable (CS8714 / CS8621)

**Pattern chuẩn** (áp dụng cho `DanhMucTrangThaiPheDuyet` → key `Ma`):

```csharp
var statusByMa = await _statusRepository.GetQueryableSet()
    .Where(s => !string.IsNullOrWhiteSpace(s.Ma))
    .ToDictionaryAsync(s => s.Ma!, s => s, cancellationToken);
```

Hoặc sync:

```csharp
var dict = items
    .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
    .ToDictionary(x => x.Ma!, x => x);
```

**Không** dùng `x.Ma!` nếu chưa `.Where` lọc null/empty.

### Files cần sửa (từ baseline)

| File |
|------|
| `DuongDiTrangThaiToTrinhs/Queries/DuongDiTrangThaiToTrinhGetQuery.cs` |
| `KeHoachLuaChonNhaThauRutGon/Commands/KeHoachLuaChonNhaThauRutGonTrinhCommand.cs` |
| `QuyetDinhDuyetDuToan/Commands/QuyetDinhDuyetDuToanUpdateCommand.cs` |
| `ToTrinhPheDuyet/Commands/ToTrinhPheDuyetUpdateCommand.cs` |
| `ToTrinhPheDuyet/Commands/ToTrinhPheDuyetTrinhCommand.cs` |
| `ToTrinhPheDuyet/Commands/ToTrinhPheDuyetTraLaiCommand.cs` |
| `ToTrinhPheDuyet/Commands/ToTrinhPheDuyetDuyetCommand.cs` |
| `ToTrinhPheDuyet/Commands/ToTrinhPheDuyetInsertCommand.cs` |
| `ToTrinhPheDuyet/Commands/ToTrinhPheDuyetDeleteCommand.cs` |
| `ThoaThuanGiaoViec/Commands/ThoaThuanGiaoViecChuyenCommand.cs` |
| `ThoaThuanGiaoViec/Commands/ThoaThuanGiaoViecTrinhCommand.cs` |
| `ToTrinhCoThamDinh/Commands/ToTrinhCoThamDinhUpdateCommand.cs` |
| `ToTrinhCoThamDinh/Commands/ToTrinhCoThamDinhThaoTacCommand.cs` |
| `ToTrinhCoThamDinh/Queries/ToTrinhCoThamDinhGetPaginatedQuery.cs` |

**Kỳ vọng:** −~30 warnings.

---

## Phase G — QLDA.Domain

### CS0108 — `ToTrinhThamDinhNhaThau.Id`

Thêm `new` nếu shadow `Entity<Guid>.Id` là chủ ý:

```csharp
public new Guid Id { get; set; }
```

Hoặc xóa property trùng nếu không cần — **đọc EF config trước**, không đổi mapping.

### CS8618 — Entity string / navigation

| Entity | Property | Sửa đề xuất |
|--------|----------|-------------|
| `DuongDiTrangThaiToTrinh` | `Loai` | `= string.Empty` |
| `HangMucKeHoach` | `TenHangMuc` | `= string.Empty`; `KeHoach` | `= default!` |
| `KeHoachTrienKhaiChiTietDuAn` | `MaMoc` | `= string.Empty` |
| `KeHoachTrienKhaiHangMuc`, `TrienKhaiKeHoachLCNT`, `ThamDinhKeHoach`, `ThuyetMinhDuAn`, `ToTrinhCoThamDinh`, `ToTrinhKetQuaGoiThau`, `ToTrinhPheDuyet`, `ToTrinhThamDinhNhaThau` | `So` / `Loai` | `= string.Empty` |
| `CanBoTrienKhaiKeHoach` | `KeHoachTrienKhai` | `= default!` |
| `QuyetDinhDuyetDuToanChiPhi` | `ChiPhi` | `= string.Empty` hoặc `= default!` tùy kiểu |

### CS8619 — `ToTrinhEntityNames.cs` L60

Lọc null trước khi gán `HashSet<string>`:

```csharp
.Where(x => x != null)
.Select(x => x!)
// hoặc .OfType<string>()
```

**Kỳ vọng:** −17 warnings.

---

## Phase H — QLDA.WebApi Models

Init string bắt buộc trên WebApi Models (không tạo mapping mới — chỉ init):

| Model file | Properties |
|------------|------------|
| `TrienKhaiKeHoachLCNTModel.cs` | `So` |
| `ToTrinhThamDinhNhaThauModel.cs` | `So` |
| `DonViTuVanKeHoachModel.cs` | `TenDonVi` |
| `HangMucTrienKhaiModel.cs` | `TenHangMuc` |
| `KeHoachTrienKhaiHangMucModel.cs` | `So`, `TenHangMuc` |
| `XuLyChungModel.cs` | `MaTrangThaiTiepTheo` |
| `ThuyetMinhDuAnModel.cs` | `So` |
| `ToTrinhPheDuyetModel.cs` | `So`, `Loai` |
| `ToTrinhCoThamDinhModel.cs` | `So`, `Loai` |
| `ToTrinhKetQuaGoiThauModel.cs` | `So` |

Nullability trong Controllers / MappingConfiguration: xử lý theo Phase I pattern (`!`, `??`, `?.`).

**Kỳ vọng:** −~20 warnings.

---

## Phase I — QLDA.Application nullability (chi tiết theo pattern)

### I.1 — DTO CS8618

| DTO | Properties |
|-----|------------|
| `KeHoachTrienKhaiChiTietDuAnDto` | `MaMoc` |
| `KeHoachTrienKhaiHangMucInsUpdDto`, `KeHoachTrienKhaiHangMucDto` | `TenHangMuc` |
| `ToTrinhThamDinhNhaThauDto` | `So` |
| `QuyetDinhPheDuyetDuToanDto` | `ThoiGian` |
| `ToTrinhPheDuyetInsUpdDto`, `ToTrinhPheDuyetDto` | `Loai` |
| `ThanhToanUpdateDto` | `Ten` |
| `ToTrinhCoThamDinhInsUpdDto`, `ToTrinhCoThamDinhDto` | `Loai` |
| `TongHopDeXuatChuTruongDto` | `Data`, `TenDuAn` |
| `QuyetDinhDieuChinhGetDanhSachQuery` (nested) | `DanhSachTepDinhKem` → `= new()` |
| `DuongDiTrangThaiToTrinhGetQuery` (nested) | `Loai` |

### I.2 — Query filter `string.Contains` (CS8602 / CS8604)

Pattern khi `request.Keyword` optional:

```csharp
if (!string.IsNullOrWhiteSpace(request.Keyword))
{
    var kw = request.Keyword;
    query = query.Where(e => e.Ten.Contains(kw));
}
```

Files tiêu biểu: `DeXuatNhuCauKinhPhiGetDanhSachQuery`, `TheoDoiDeXuatNhuCauKinhPhiQuery`, `KeHoachTrienKhaiHangMucGetDanhSachQuery`, các `*GetDanhSachQuery` có `.Contains(...)`.

### I.3 — Mapping CS8601 / CS8602

| File | Ghi chú |
|------|---------|
| `DuToanDauTuMappings.cs` | Null-coalesce hoặc `!` sau Include |
| `QuyetDinhDuyetDuToanMappings.cs` | Nhiều chỗ — ưu tiên `?.` / `??` |
| `KeHoachTrienKhaiHangMucMappings.cs` | Assignment |
| `ToTrinhCoThamDinhMappings.cs` | Assignment |

### I.4 — Include / collection (CS8620)

`ToTrinhKetQuaGoiThauGetQuery.cs`: collection navigation nullable vs `IEnumerable<>` — cast/Include chain hoặc `!` sau khi đã `.Include(...)`.

### I.5 — `PheDuyetDispatchDuyetCommand.cs` L59

`NoiDung` có thể null → `request.NoiDung ?? string.Empty` hoặc guard nếu bắt buộc nghiệp vụ.

### I.6 — `AuthorizationBehavior.cs` CS8714

Generic `TRequest` nullability — có thể cần constraint `where TRequest : notnull` trên handler behavior. **Đánh giá riêng** — ít file, không ảnh hưởng runtime nếu thêm constraint đúng MediatR.

**Kỳ vọng:** −~80 warnings còn lại trong Application.

---

## Phase J — CancellationToken audit

Baseline **không** liệt kê warning thiếu token (compiler không bắt). Quét thủ công:

```powershell
# Gợi ý quét — chạy local
rg "ToListAsync\(\)" QLDA.Application --glob "*.cs" | rg -v "cancellationToken"
rg "SaveChangesAsync\(\)" QLDA.Application --glob "*.cs"
```

**Quy tắc:**

- Handler đã có `CancellationToken cancellationToken` → truyền xuống mọi EF/repo/UoW async
- Không tạo `new CancellationToken()` / `CancellationToken.None` thay thế

Ưu tiên module vừa sửa trong Phase E–I.

---

## Checklist verify sau mỗi phase

```powershell
cd E:\SER\QLDA.WebApi
dotnet build
# Ghi: Build succeeded, X warning(s)
```

| Tiêu chí | Pass |
|----------|------|
| 0 error | ✅ |
| Warning giảm so với phase trước | ✅ |
| Không sửa migration/snapshot | ✅ |
| Không `#pragma warning disable` | ✅ |
| Smoke: API khởi động được (optional) | `dotnet run` |

---

## Bảng tiến độ (cập nhật khi implement)

| Phase | Mô tả | Warnings trước | Warnings sau | Ngày | Ghi chú |
|-------|--------|----------------|--------------|------|---------|
| — | Baseline | — | **212** | 2026-06-16 | Terminal dev local |
| A | Duplicate using | 212 | ~202 | 2026-06-16 | 10 file |
| B | Aspose EnsureCellsLicense | ~202 | ~194 | 2026-06-16 | ExcelHelper + ExcelImporter |
| C | XML comment | ~194 | ~192 | 2026-06-16 | KeHoachLuaChonNhaThauRutGonController |
| D | Unused / unreachable | ~192 | ~185 | 2026-06-16 | VisibilityFilterEnabled flag |
| E | Field never assigned | ~185 | ~178 | 2026-06-16 | HoSoDeXuatCapDoCntt inject _settings |
| F | ToDictionary nullable key | ~178 | ~148 | 2026-06-16 | 14+ handler files |
| G | Domain entities | ~148 | ~131 | 2026-06-16 | 14 file, 0 warnings Domain |
| H | WebApi Models + controllers | ~131 | ~0 | 2026-06-16 | Models init + mapping fixes |
| I | Application nullability | ~131 | **0** | 2026-06-16 | Application 0 warnings |
| J | CancellationToken | — | — | | Không có warning compiler; chưa audit riêng |

**Kết quả cuối:** `QLDA.Application`, `BuildingBlocks.Infrastructure`, `QLDA.WebApi` — **0 warnings** (`dotnet build --no-restore`).

---

## Rủi ro cần lưu ý khi implement

1. **`HoSoDeXuatCapDoCntt*._settings`** — Thiếu inject có thể là bug runtime thật; sửa warning = sửa behavior (cho phép handler chạy đúng).
2. **`VisibilityFilterExtensions`** — Filter đang tắt có chủ đích (`//Tạm tắt`); refactor bằng flag, không bật lại filter.
3. **`ToTrinhPheDuyetTraLaiCommand`** — Nếu `_dbContext`/`_auth` thực sự cần trong `Handle` mà chưa inject → có thể đang lỗi im lặng; đọc full `Handle` trước khi xóa field.
4. **Domain `new Guid Id`** — Xác nhận EF không double-map key.

---

## Lệnh hữu ích khi implement

```powershell
# Build + đếm warning
cd E:\SER\QLDA.WebApi
dotnet build 2>&1 | Select-String "Warning\(s\)"

# Liệt kê warning theo file (PowerShell)
dotnet build 2>&1 | Select-String "warning CS" | ForEach-Object {
  if ($_.Line -match '\\([^\\]+\.cs)\(') { $matches[1] }
} | Group-Object | Sort-Object Count -Descending
```

---

## Tham chiếu

- Quy tắc dự án: `CLAUDE.md` (migration, DTO mapping)
- Aspose interface: `BuildingBlocks/src/BuildingBlocks.CrossCutting/Offices/IAsposeHelper.cs`
- Code standards: `docs/code-standards.md`
