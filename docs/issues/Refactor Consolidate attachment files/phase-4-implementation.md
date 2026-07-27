# Phase 4 Implementation Log — Multi-GroupType & chuẩn hóa query load Attachment

> **Phạm vi đợt này:** chỉ Phase 4 (multi-GroupType + shared query helpers + migrate QLDA load patterns).
> **Không commit** trong đợt này — chờ user tự commit.
> **Không** thực hiện Phase 5, Phase 6.

Ngày: 2026-07-20  
Branch: `151-refactor-consolidate-attachment-files`

---

## Summary

Phase 4 chuẩn hóa cách **load** attachment:

1. Hoàn thiện `AttachmentSubquery` (non-generic) trong BuildingBlocks: `ForGroupTypes` / `ForExactGroupType` / `OriginalOnly` / `SignedOnly` / `ForGroupId` + **`ExpandGroupTypes`**.
2. Chuẩn hóa `GetAttachmentsQuery` (validate GroupId, dedupe BaseGroupTypes, dùng `ExpandGroupTypes`, không materialize sớm).
3. Migrate các QLDA query handler từng **filter base + KySo_** (SignedHelper) sang `ExpandGroupTypes` + `Contains` — pattern EF Core translate được trong correlated `.Select()`.
4. Standalone exact filter dùng `.ForExactGroupType(...)`.
5. Giữ nguyên API contract (`DanhSachTepDinhKem`, `TepDinhKemDto`, …).

**Quyết định EF Core quan trọng:** extension `ForGroupTypes` **không** gọi trực tiếp bên trong expression tree của parent `.Select()` (Pomelo không expand custom method). Thay vào đó:

```csharp
// Trước Select — expand 1 lần
var types = AttachmentSubquery.ExpandGroupTypes(includeSigned: true, nameof(EGroupType.X));

// Trong Select — Contains(List) translate → SQL IN
.Where(i => i.GroupId == e.Id.ToString() && types.Contains(i.GroupType))
.Select(i => i.ToDto()) // QLDA → TepDinhKemDto
```

Standalone (ngoài Select) vẫn dùng extension:

```csharp
repo.GetQueryableSet().ForExactGroupType(id.ToString(), nameof(EGroupType.BanGiaoHoSo))
```

---

## Baseline trước Phase 4

```text
Branch: 151-refactor-consolidate-attachment-files
dotnet build SER.sln -c Release → Succeeded, 0 error, 0 warning

Phase 2–3 đã có:
  AttachmentSubquery (extension, luôn IncludeSigned, return AttachmentDto)
  GetAttachmentsQuery (chưa validate GroupId rỗng / BaseGroupTypes empty)
  SignedGroupTypeHelper / AttachmentBulkInsertOrUpdateCommand
  QLDA WebApi đã dùng GetAttachmentsQuery + AttachmentBulkInsertOrUpdateCommand

QLDA Application query handlers vẫn:
  - Nhiều chỗ Where(GroupId) không filter GroupType (ForGroupId behavior)
  - 5 file dùng SignedHelper.Prefix + base (multi / nested signed)
  - AttachmentSubquery chưa được gọi từ QLDA
```

### Survey Phase 2–3 (xác nhận)

| Pattern | Kết quả |
|---|---|
| `IRepository<TepDinhKem` trong QLDA | **0** |
| `IRepository<Attachment` trong QLDA | Nhiều (đã migrate entity) |
| `AttachmentSubquery` callers trước Phase 4 | **0** (chỉ definition) |
| `GetAttachmentsQuery` | Controllers (Phase 3) |
| `GetDanhSachTepDinhKemQuery` active callers | File còn + comment trong ToTrinhThamDinh; `DuAnGetDanhSachTepDinhKemQuery` riêng |
| `SignedHelper.Prefix` trong Query | 5 file (BanGiao, KhoKhan, ThanhLy×2, ThuyetMinh) |

### Phân loại load pattern (trước sửa)

| Pattern | Mô tả | Ví dụ |
|---|---|---|
| ForGroupId | Chỉ `GroupId == id` | HopDongGetDanhSach, ~35 handlers |
| Base + signed | base OR KySo_base | BanGiaoHoSo, KhoKhan, ThanhLy, ThuyetMinh |
| Exact | Một GroupType chính xác | BanGiaoHoSoGetFileExport, Print Count |
| Nested | Property cấp 2 cùng/khác GroupType | KhoKhan.KetQua, ThanhLy 3 list |
| Complex OR multi-GroupId | Logic đặc thù | HoSoMoiThauDienTuGetDanhSach — **giữ nguyên** |

---

## Các bước triển khai

### Bước 0 — Survey + baseline build

```bash
git status
git branch --show-current   # 151-refactor-consolidate-attachment-files
dotnet build SER.sln -c Release --nologo
# + rg inventory (AttachmentSubquery, GetAttachmentsQuery, SignedHelper, DanhSachTepDinhKem, …)
```

### Bước 1 — Shared: `AttachmentSubquery`

**File:** `BuildingBlocks/.../Attachments/Common/AttachmentSubquery.cs`

Thay đổi chính:

| API | Hành vi |
|---|---|
| `ExpandGroupTypes(includeSigned, params string[])` | Dedup, bỏ empty, không double `KySo_` |
| `ForGroupTypes(query, groupId, includeSigned, params types)` | Filter `GroupId` + `IN expanded` — return `IQueryable<Attachment>` |
| `ForExactGroupType` | Exact match, không thêm KySo_ |
| `OriginalOnly` | Exact base; **throw** nếu input đã có prefix `KySo_` |
| `SignedOnly` | `WithSignedVariant` — không double prefix |
| `ForGroupId` | Mọi GroupType của 1 GroupId |
| `SelectDto()` | Optional map → `AttachmentDto` (BB) |

**Vì sao return `Attachment` thay vì `AttachmentDto`?**  
QLDA gán `List<TepDinhKemDto>` và dùng `Attachment.ToDto()` → `TepDinhKemDto`. Helper chỉ filter; caller tự project.

### Bước 2 — Shared: `SignedGroupTypeHelper`

**File:** `.../SignedGroupTypeHelper.cs`

- `ExpandWithSignedVariant`: nếu base đã `KySo_` → chỉ 1 phần tử (không `[KySo_X, KySo_X]`).
- Thêm `ExpandGroupTypes(...)` delegate sang `AttachmentSubquery`.

### Bước 3 — Shared: `GetAttachmentsQuery`

**File:** `.../Queries/GetAttachmentsQuery.cs`

```csharp
// Validate
GroupIds normalize (trim, distinct, bỏ empty) → throw nếu rỗng
BaseGroupTypes != null → ExpandGroupTypes; throw nếu sau expand vẫn rỗng
// Filter DB
Where GroupIds.Contains + (optional) groupTypes.Contains
Select ToDto — không AsEnumerable / ToList trước filter
```

### Bước 4 — QLDA: migrate Signed / Multi / Nested / Exact

#### 4.1 Pattern Multi + Nested — `KhoKhanVuongMacGetDanhSachQuery`

| Property | GroupId | Pattern | GroupType | IncludeSigned |
|---|---|---|---|---|
| `DanhSachTepDinhKem` | `e.Id` | Base+signed | KhoKhanVuongMac | true |
| `KetQua.DanhSachTepDinhKem` | `e.Id` (cùng entity) | Base+signed nested | KetQuaXuLyKhoKhanVuongMac | true |

Code:

```csharp
var groupTypesKhoKhan = AttachmentSubquery.ExpandGroupTypes(includeSigned: true, nameof(EGroupType.KhoKhanVuongMac));
var groupTypesKetQua = AttachmentSubquery.ExpandGroupTypes(includeSigned: true, nameof(EGroupType.KetQuaXuLyKhoKhanVuongMac));
// ... Contains trong Select
```

#### 4.2 Pattern Multi property — `BanGiaoHoSoGetDanhSachQuery`

| Property | GroupType | IncludeSigned |
|---|---|---|
| `DanhSachTepHSBanGiao` | BanGiaoHoSo | true |
| `DanhSachBienBanBanGiao` | BienBanBanGiao | true |

#### 4.3 Pattern Multi list tách property — `ThanhLyHopDongGetDanhSachQuery` + `...TienDoQuery`

| Property | GroupType | IncludeSigned |
|---|---|---|
| `BienBanNghiemThus` | ThanhLyHopDong_BienBanNghiemThu | true |
| `ThanhLyHopDongs` | ThanhLyHopDong | true |
| `Khacs` | ThanhLyHopDong_Khac | true |

**Không gộp** 3 list thành 1 — giữ contract FE.

#### 4.4 Pattern Multi — `ThuyetMinhDuAnGetDanhSachQuery`

| Property | GroupType | IncludeSigned |
|---|---|---|
| `DanhSachTepDinhKem` | ThuyetMinhDuAn | true |
| `DanhSachTepThamDinh` | ThuyetMinhDuAnThamDinh | true |

#### 4.5 Pattern Exact standalone — `BanGiaoHoSoGetFileExportQuery`

```csharp
.ForExactGroupType(entity.Id.ToString(), nameof(EGroupType.BanGiaoHoSo))
```

Chỉ file gốc `BanGiaoHoSo` — không KySo_ (khớp hành vi cũ).

#### 4.6 Pattern Exact trong Select — `BanGiaoHoSoPrintQuery`

```csharp
var groupTypesBanGiaoExact = AttachmentSubquery.ExpandGroupTypes(includeSigned: false, nameof(EGroupType.BanGiaoHoSo));
.Count(f => f.GroupId == ... && groupTypesBanGiaoExact.Contains(f.GroupType))
```

### Bước 5 — ForGroupId handlers (~35)

**Giữ** `.Where(i => i.GroupId == e.Id.ToString())` trong correlated Select.

Lý do: đây **đã là** pattern ForGroupId; gọi `.ForGroupId()` bên trong Select không được EF translate. Hành vi không đổi. Phase 4 không đổi scope GroupType của các endpoint này.

Ví dụ giữ nguyên: HopDong, TamUng, NghiemThu, VanBanPhapLy, …

Complex: `HoSoMoiThauDienTuGetDanhSachQuery` (OR nhiều GroupId) — **không đụng**.

### Bước 6 — Tests

| File | Nội dung |
|---|---|
| `BuildingBlocks.Tests/.../Phase4AttachmentSubqueryTests.cs` | Expand, ForGroupTypes ± signed, Original/Signed/Exact, GroupId isolation, no double prefix, empty/dup |
| `QLDA.Tests/Unit/Phase4AttachmentQueryTests.cs` | Expand khớp BanGiao/KhoKhan/ThanhLy; static check không còn `SignedHelper.Prefix` trong `*Query*.cs` |

### Bước 7 — Validation

```bash
dotnet clean SER.sln -c Release
dotnet build SER.sln -c Release --nologo
dotnet test BuildingBlocks.Tests -c Release --filter "FullyQualifiedName~Attachments"
dotnet test QLDA.Tests -c Release --filter "FullyQualifiedName~Phase4Attachment"
```

---

## Shared components

| Component | Path | Action |
|---|---|---|
| `AttachmentSubquery` | `BuildingBlocks.Application/Attachments/Common/AttachmentSubquery.cs` | Rewrite / hoàn thiện |
| `SignedGroupTypeHelper` | `.../SignedGroupTypeHelper.cs` | ExpandGroupTypes + ExpandWithSignedVariant fix |
| `GetAttachmentsQuery` | `.../Queries/GetAttachmentsQuery.cs` | Validate + ExpandGroupTypes |
| Projection | `Attachment.ToDto()` / QLDA `ToDto()` → `TepDinhKemDto` | Giữ nguyên |

`SignedHelper.cs` (QLDA) **còn file** — không còn dùng trong Query; cleanup Phase 5.

---

## Updated query handlers

| Nghiệp vụ/API | GroupId | Pattern | GroupType | IncludeSigned |
|---|---|---|---|---|
| KhoKhanVuongMac danh sách | `e.Id` | Base+signed | KhoKhanVuongMac | true |
| KhoKhan nested KetQua | `e.Id` | Nested Base+signed | KetQuaXuLyKhoKhanVuongMac | true |
| BanGiaoHoSo danh sách HS | `e.Id` | Base+signed | BanGiaoHoSo | true |
| BanGiaoHoSo danh sách BB | `e.Id` | Base+signed | BienBanBanGiao | true |
| BanGiaoHoSo export file | `entity.Id` | Exact | BanGiaoHoSo | n/a (exact) |
| BanGiaoHoSo print count | `e.Id` | Exact | BanGiaoHoSo | false |
| ThanhLyHopDong ×2 queries | `x.Id` | Multi property Base+signed | 3 types ThanhLy* | true |
| ThuyetMinhDuAn danh sách | `e.Id` | Multi property Base+signed | ThuyetMinh + ThamDinh | true |
| GetAttachmentsQuery (controllers) | GroupIds | Mediator query | BaseGroupTypes | per call (Phase 3: thường false) |

---

## Behavior preserved

- **Gộp gốc + ký số** trên cùng property: BanGiao (2 list), KhoKhan (2 tầng), ThanhLy (3 list), ThuyetMinh (2 list) — vẫn gộp qua `IncludeSigned: true`.
- **Exact only:** Export BanGiao, Print count — không lấy `KySo_`.
- **ForGroupId (~35):** vẫn load mọi GroupType theo GroupId — không thu hẹp.
- **Nested:** KhoKhan `KetQua.DanhSachTepDinhKem` dùng cùng `e.Id` + GroupType riêng (không nhầm ID cha khác entity).
- **Controllers Phase 3:** `GetAttachmentsQuery` + `IncludeSigned: false` giữ exact-match khi load chi tiết.
- **UI names:** không rename `DanhSachTepDinhKem` / `TepDinhKemDto` / …

---

## Validation

```text
Build command:
  dotnet clean SER.sln -c Release
  dotnet build SER.sln -c Release --nologo
Build result: Succeeded
Errors: 0
Warnings: 0

Test command:
  dotnet test BuildingBlocks.Tests -c Release --filter "FullyQualifiedName~Attachments"
  → Passed: 23, Failed: 0
  dotnet test QLDA.Tests -c Release --filter "FullyQualifiedName~Phase4Attachment"
  → Passed: 5, Failed: 0

rg checks:
  SignedHelper.Prefix trong QLDA.Application Query: 0
  "KySo_" + trong QLDA.Application: 0
  Không có migration mới / ModelSnapshot không đổi
```

---

## File checklist (Phase 4 only — delta so với sau Phase 3)

### BuildingBlocks

- [x] `Attachments/Common/AttachmentSubquery.cs`
- [x] `Attachments/Common/SignedGroupTypeHelper.cs`
- [x] `Attachments/Queries/GetAttachmentsQuery.cs`
- [x] `tests/.../Phase4AttachmentSubqueryTests.cs`

### QLDA.Application

- [x] `KhoKhanVuongMacs/Queries/KhoKhanVuongMacGetDanhSachQuery.cs`
- [x] `BanGiaoHoSos/Queries/BanGiaoHoSoGetDanhSachQuery.cs`
- [x] `BanGiaoHoSos/Queries/BanGiaoHoSoGetFileExportQuery.cs`
- [x] `BanGiaoHoSos/Queries/BanGiaoHoSoPrintQuery.cs`
- [x] `ThanhLyHopDongs/Queries/ThanhLyHopDongGetDanhSachQuery.cs`
- [x] `ThanhLyHopDongs/Queries/ThanhLyHopDongGetDanhSachTienDoQuery.cs`
- [x] `ThuyetMinhDuAn/Queries/ThuyetMinhDuAnGetDanhSachQuery.cs`

### QLDA.Tests

- [x] `Unit/Phase4AttachmentQueryTests.cs`

### Docs

- [x] `docs/issues/Refactor Consolidate attachment files/phase-4-implementation.md` (file này)

---

## Remaining work

### Phase 5 (chưa làm)

- Xóa entity/config/command/query `TepDinhKem` cũ còn dependency
- Xóa `QLDA.Application/Common/SignedHelper.cs`
- `ExcludeFromMigrations` / cleanup Persistence
- Không tạo migration trong Phase 4 — migration (nếu cần) thuộc Phase 5

### Phase 6 (chưa làm)

- Cập nhật documentation tổng / CLAUDE references

### Giữ lại cố ý

| Item | Lý do |
|---|---|
| `GetDanhSachTepDinhKemQuery.cs` | Compatibility; callers còn / cleanup Phase 5 |
| `SignedHelper.cs` | Không còn trong Query; xóa Phase 5 |
| `TepDinhKemDto` / mapping compatibility | UI contract |
| ~35 ForGroupId inline Where | EF-safe; hành vi đúng |
| `HoSoMoiThauDienTuGetDanhSachQuery` OR phức tạp | Ngoài pattern chuẩn — review riêng nếu cần |
| `DuAnGetDanhSachTepDinhKemQuery` | Query riêng theo DuAnId |

---

## Điểm dừng

Phase 4 **build + test thành công** → **dừng**.  
Không tự làm Phase 5/6.  
**Không commit** — user tự commit khi sẵn sàng.
