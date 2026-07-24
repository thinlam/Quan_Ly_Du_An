# Design: Mặc định IncludeSigned = true + chuẩn hóa load tệp đính kèm / ký số

**Ngày:** 2026-07-23  
**Trạng thái:** Implemented (Approach A) — 2026-07-24  
**Phạm vi:** Logic đọc (read hydration) + chuẩn hóa write mapping trùng lặp; **không** migration; **không** đổi sync/insert/update ngoài phạm vi mapping helper  
**Liên quan:** Refactor Attachment Phase 1–6 (`docs/issues/Refactor Consolidate attachment files/`), `docs/code-standards.md` §14

### Implementation notes (2026-07-24)

- Đã bỏ `IncludeSigned: false` trên toàn bộ `QLDA.WebApi/Controllers` (44 file + 2 mẫu BanGiao/BaoCao đã sửa tay).
- `BanGiaoHoSo` chi tiết: split bằng `ToBaseGroupType()`.
- `BaoCaoBanGiaoSanPham` chi tiết: thêm `BaseGroupTypes`; list dùng `ExpandGroupTypes`.
- Write BanGiao: thin wrapper → `ToEntities`.
- `BanGiaoHoSoPrintQuery`: giữ `includeSigned: false` (chỉ đếm file gốc) + comment.
- Build `SER.sln` Release: 0 error/0 warning; BB Attachment 23 + QLDA Phase3/4 14 tests passed.

---

## 1. Mục tiêu nghiệp vụ

1. API lấy tệp đính kèm **mặc định trả cả file gốc và file ký số** (`KySo_*`).
2. Chỉ khi nghiệp vụ **không** muốn file ký số mới truyền rõ `IncludeSigned = false`.
3. Các module (đặc biệt `BanGiaoHoSo`, `BaoCaoBanGiaoSanPham` và các module tương tự) dùng **cùng helper chung** của BuildingBlocks Attachment.
4. Không còn query/mapping attachment trùng lặp với logic khác nhau nếu nghiệp vụ giống nhau.
5. Không đụng logic insert/update/sync ngoài phạm vi; không tạo migration; build solution sạch.

---

## 2. Hiện trạng — Helper chung đã có (đúng hướng, giữ nguyên)

Đây là **single source of truth** đã có sau refactor Attachment. Spec này **không tạo helper mới song song**, chỉ chuẩn hóa caller + bỏ override mặc định sai.

| Helper | File | Vai trò |
|--------|------|---------|
| `SignedGroupTypeHelper` | `BuildingBlocks.../SignedGroupTypeHelper.cs` | Convention `KySo_` / `ResolveSignedGroupType` / `WithSignedVariant` / `ToBaseGroupType` / `ExpandWithSignedVariant` |
| `AttachmentSubquery` | `BuildingBlocks.../AttachmentSubquery.cs` | `ExpandGroupTypes(includeSigned = true)`, `ForGroupTypes`, `OriginalOnly`, `SignedOnly` |
| `GetAttachmentsQuery` | `BuildingBlocks.../GetAttachmentsQuery.cs` | Read hydration controller; **`IncludeSigned = true` đã là mặc định** |
| `AttachmentCollectionExtensions` | `BuildingBlocks.../AttachmentCollectionExtensions.cs` | Write: `ToEntities(groupId, baseGroupType)`; Read bridge: `ToAttachmentEntities()`; `BaseGroupType()` |
| `AttachmentBulkInsertOrUpdateCommand` | BB Commands | Sync write — **ngoài phạm vi đổi hành vi** |

### Hành vi mặc định ở tầng BB (đã đúng)

```csharp
// GetAttachmentsQuery — IncludeSigned mặc định TRUE
public record GetAttachmentsQuery(
    List<string> GroupIds,
    List<string>? BaseGroupTypes = null,
    bool IncludeSigned = true);

// ExpandGroupTypes — includeSigned mặc định TRUE
AttachmentSubquery.ExpandGroupTypes(baseGroupTypes, includeSigned = true);
```

**Vấn đề chính không nằm ở BB default**, mà ở **caller WebApi / một số filter sau query** đang **ép `IncludeSigned: false`** hoặc **lọc exact `GroupType` khiến file ký số bị loại**.

---

## 3. Phân biệt hai phía: Write vs Read (quan trọng)

| Phía | Method điển hình | `IncludeSigned` có nghĩa? |
|------|------------------|---------------------------|
| **Write** (Insert/Update) | `model.GetDanhSachTepDinhKem(entity.Id)` | **Không** — method này map payload UI → `Attachment` entities. File ký số đã nằm trong list với `ParentId != null`; helper resolve `GroupType = KySo_*` theo `ParentId`. |
| **Read** (Get chi tiết / danh sách) | `GetAttachmentsQuery` / `ExpandGroupTypes` trong `.Select()` | **Có** — quyết định có mở rộng filter sang `KySo_*` hay không. |

Ví dụ trong yêu cầu gốc:

```csharp
model.GetDanhSachTepDinhKem(entity.Id, new AttachmentQueryOptions { IncludeSigned = false });
```

**Hiện tại `AttachmentQueryOptions` chưa tồn tại.** Và gắn `IncludeSigned` vào write-side `GetDanhSachTepDinhKem` dễ gây hiểu nhầm: write không “query” theo signed — nó chỉ map list đã có.

**Khuyến nghị trong spec này:**

- **Read:** đổi quy ước gọi `GetAttachmentsQuery` / `ExpandGroupTypes` → mặc định include signed; chỉ truyền `false` khi cần.
- **Write:** chuẩn hóa mapping về `*.ToEntities(groupId, baseGroupType)` / `GetDanhSachTepDinhKem` thin wrapper; **không** thêm `IncludeSigned` vào write trừ khi sau này có use-case tách payload rõ ràng.
- Nếu muốn API options object: chỉ thêm `AttachmentQueryOptions` cho **read helpers**, không gắn vào write mapping.

---

## 4. Logic hiện tại khác nhau ở điểm nào?

### 4.1. Read — Controller chi tiết (`GetAttachmentsQuery`)

**~46 controller** trong `QLDA.WebApi/Controllers` đang truyền tường minh:

```csharp
IncludeSigned: false
```

Trong khi BB mặc định đã là `true`. Đây là **override hàng loạt** sau Phase 3/4 (giữ exact-match khi load chi tiết) — **đối lập** với mục tiêu mới.

Hai pattern phổ biến:

| Pattern | Ví dụ | Hệ quả thực tế |
|---------|--------|----------------|
| A. Có `BaseGroupTypes` + `IncludeSigned: false` | `BanGiaoHoSoController`, `ToTrinhCoThamDinhController` | Chỉ lấy đúng base GroupType, **không** lấy `KySo_*` |
| B. Không truyền `BaseGroupTypes`, chỉ `IncludeSigned: false` | `BaoCaoBanGiaoSanPhamController` | `IncludeSigned` **bị bỏ qua** (handler chỉ Expand khi `BaseGroupTypes != null`) → thực tế lấy **mọi** GroupType của GroupId (kể cả KySo nếu có) |

→ Cùng một flag `IncludeSigned: false` nhưng **hành vi không đồng nhất** giữa các màn hình.

### 4.2. Read — Filter sau query (BanGiaoHoSo chi tiết)

```csharp
// BanGiaoHoSoController.Get
var allFiles = GetAttachmentsQuery(..., BaseGroupTypes: [BanGiaoHoSo, BienBanBanGiao], IncludeSigned: false);
var tepHS = allFiles.Where(f => f.GroupType == EGroupType.BanGiaoHoSo.ToString());
var bienBan = allFiles.Where(f => f.GroupType == EGroupType.BienBanBanGiao.ToString());
```

Dù sau này đổi `IncludeSigned` thành `true`, filter `GroupType == "BanGiaoHoSo"` **vẫn loại** `KySo_BanGiaoHoSo` vì so khớp exact string.

**Phải đổi filter** sang:

```csharp
f.GroupType.ToBaseGroupType() == nameof(EGroupType.BanGiaoHoSo)
// hoặc
f.BaseGroupType() == nameof(EGroupType.BanGiaoHoSo)  // nếu đi từ DTO
```

### 4.3. Read — List query (subquery trong `.Select`)

| Module | Cách load | Include signed? |
|--------|-----------|-----------------|
| `BanGiaoHoSoGetDanhSachQuery` | `ExpandGroupTypes(includeSigned: true, ...)` | Có |
| `KhoKhanVuongMacGetDanhSachQuery` | `ExpandGroupTypes(includeSigned: true, ...)` | Có |
| `ThuyetMinhDuAnGetDanhSachQuery` | `ExpandGroupTypes(includeSigned: true, ...)` | Có |
| `ThanhLyHopDongGetDanhSach*` | `ExpandGroupTypes(includeSigned: true, ...)` | Có |
| `BanGiaoHoSoPrintQuery` | `ExpandGroupTypes(includeSigned: false, BanGiaoHoSo)` | Không (đếm file gốc) |
| `BaoCaoBanGiaoSanPhamGetDanhSachQuery` | `Where(GroupId == id)` **không** filter GroupType | Lấy tất cả (gốc + KySo + mọi type trên GroupId) |
| Nhiều list khác (`VanBanPhapLy`, `BaoCaoTienDo`, …) | Chỉ `GroupId`, không Expand | Lấy tất cả GroupType |

→ **Danh sách** nhiều nơi đã “có signed” (hoặc all types); **chi tiết** lại hay `IncludeSigned: false` → UI chi tiết thiếu file ký số dù list có.

### 4.4. Write — Mapping Insert/Update không thống nhất

| Module | Method | Cách map |
|--------|--------|----------|
| Hầu hết WebApi models | `TepDinhKemMappingConfigurations.GetDanhSachTepDinhKem(this XModel, Guid)` | `DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.X)` — dùng helper WebApi `ToEntities` (ResolveGroupType theo ParentId) |
| `BaoCaoBanGiaoSanPham` | Cùng pattern trên | Chuẩn thin wrapper |
| `BanGiaoHoSo` (Application) | `GetDanhSachTepHSBanGiao` | **Tự `new Attachment { ... ResolveSignedGroupType }`** — duplicate logic |
| `BanGiaoHoSo` (WebApi) | `GetDanhSachBienBanBanGiao` | **Tự `new Attachment { ... }`** — duplicate; dùng `Guid.NewGuid()` thay vì sequential |
| `HoSoMoiThauDienTu` | Nhiều method theo loại file | Đã dùng `ToEntities` — OK về resolve ký số |
| `ToTrinhCoThamDinh` | `dto.DanhSachTepDinhKem?.ToEntities(...)` trực tiếp | OK |

**Khác biệt write quan trọng:**

- `GetDanhSachTepHSBanGiao` / `GetDanhSachBienBanBanGiao` **không** đi qua `TepDinhKemMappingConfigurations.ToEntities` → thiếu `ResolveId` (giữ Id khi cùng GroupId/GroupType; tạo Id mới khi copy từ nhóm khác).
- Tên method khác nhau (`GetDanhSachTepHSBanGiao` vs `GetDanhSachTepDinhKem`) dù nghiệp vụ giống: map list → entities theo một `baseGroupType`.

---

## 5. So sánh cụ thể: BanGiaoHoSo vs BaoCaoBanGiaoSanPham

| Tiêu chí | BanGiaoHoSo | BaoCaoBanGiaoSanPham |
|----------|-------------|----------------------|
| Write map | Custom `GetDanhSachTepHSBanGiao` / `GetDanhSachBienBanBanGiao` | `GetDanhSachTepDinhKem` → `ToEntities` |
| Chi tiết Get | `BaseGroupTypes` + `IncludeSigned: false` + filter exact GroupType | `IncludeSigned: false` **không hiệu lực** (null BaseGroupTypes) → lấy all |
| List | Expand `includeSigned: true` theo 2 GroupType | Subquery all by GroupId |
| Multi GroupType trên 1 entity | Có: `BanGiaoHoSo` + `BienBanBanGiao` | Một base: `BaoCaoBanGiaoSanPham` |
| ParentId / KySo_ khi write | Resolve qua `ResolveSignedGroupType` | Resolve qua `ResolveGroupType` trong mapping WebApi |

**Kết luận:** Hai module “bàn giao” đang trả file theo **hai kiểu khác nhau**; list BanGiao đã include signed còn chi tiết thì loại — đây là bug UX/consistency cần fix trong cùng đợt.

---

## 6. Thiết kế mục tiêu

### 6.1. Quy ước mới (Read)

| Tình huống | Cách gọi |
|------------|----------|
| Mặc định (gốc + ký số) | `new GetAttachmentsQuery(GroupIds: [...], BaseGroupTypes: [base])` — **không** truyền `IncludeSigned` |
| Chỉ file gốc | `IncludeSigned: false` tường minh |
| Nhiều base trên 1 GroupId | Một lần query với nhiều `BaseGroupTypes`; gán property bằng `ToBaseGroupType()` / `BaseGroupType()` |
| List subquery | `ExpandGroupTypes(includeSigned: true, ...)` — có thể bỏ named arg `true` vì mặc định; giữ tường minh khi review |

### 6.2. Hydration chuẩn (Controller chi tiết)

**Một GroupType:**

```csharp
var files = (await Mediator.Send(new GetAttachmentsQuery(
    GroupIds: [entity.Id.ToString()],
    BaseGroupTypes: [nameof(EGroupType.BaoCaoBanGiaoSanPham)]
    // IncludeSigned mặc định true
))).ToAttachmentEntities();

return ResultApi.Ok(entity.ToModel(files));
```

**Hai GroupType (BanGiaoHoSo):**

```csharp
var allFiles = (await Mediator.Send(new GetAttachmentsQuery(
    GroupIds: [entity.Id.ToString()],
    BaseGroupTypes: [
        nameof(EGroupType.BanGiaoHoSo),
        nameof(EGroupType.BienBanBanGiao)
    ]
))).ToAttachmentEntities();

var tepHS = allFiles
    .Where(f => f.GroupType.ToBaseGroupType() == nameof(EGroupType.BanGiaoHoSo))
    .ToList();
var bienBan = allFiles
    .Where(f => f.GroupType.ToBaseGroupType() == nameof(EGroupType.BienBanBanGiao))
    .ToList();
```

### 6.3. Chuẩn hóa Write mapping

Ưu tiên:

```csharp
// Thin wrapper — giữ tên API contract nếu cần
public static List<Attachment> GetDanhSachTepDinhKem(this BaoCaoBanGiaoSanPhamModel model, Guid groupId)
    => model.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.BaoCaoBanGiaoSanPham).ToList() ?? [];
```

Với BanGiaoHoSo:

```csharp
// Thay custom Select new Attachment bằng ToEntities / cùng helper WebApi
dto.DanhSachTepDinhKem?.ToEntities(groupId, EGroupType.BanGiaoHoSo).ToList() ?? [];
// Biên bản:
model.DanhSachBienBan?.ToEntities(groupId, EGroupType.BienBanBanGiao).ToList() ?? [];
```

Có thể **giữ tên** `GetDanhSachTepHSBanGiao` / `GetDanhSachBienBanBanGiao` như thin wrapper để giảm churn caller, nhưng body phải gọi helper chung (không duplicate resolve Id/GroupType).

### 6.4. `AttachmentQueryOptions` — quyết định thiết kế

**Không bắt buộc** cho đợt này nếu chỉ cần flip default + sửa filter.

| Approach | Mô tả | Ưu | Nhược |
|----------|-------|----|------|
| **A (khuyến nghị)** | Không thêm options type; bỏ `IncludeSigned: false` ở read callers; sửa filter `ToBaseGroupType()`; chuẩn hóa write thin wrapper | Ít surface area; khớp BB hiện có | Không có object options như ví dụ yêu cầu |
| **B** | Thêm `AttachmentQueryOptions { IncludeSigned = true }` chỉ cho read helper mới (vd extension trên Mediator/repo) | API rõ ràng theo ví dụ user | Thêm abstraction; write `GetDanhSachTepDinhKem` vẫn không cần |
| **C** | Thêm overload `GetDanhSachTepDinhKem(..., bool includeSigned = true)` trên write models | Khớp ví dụ user | **Sai tầng** — write không filter theo IncludeSigned |

**Khuyến nghị: Approach A**, cập nhật docs/code-standards §14 cho khớp. Nếu sau này cần options object → Approach B chỉ cho read.

### 6.5. Ngoại lệ giữ `IncludeSigned: false`

Chỉ giữ khi nghiệp vụ **xác nhận** chỉ cần file gốc, ví dụ:

- `BanGiaoHoSoPrintQuery` đếm `TongSoTepDinhKem` — hiện `includeSigned: false`. Cần xác nhận: đếm chỉ gốc hay gốc+ký số?
- Màn hình tách riêng cột “file gốc” / “file ký số” (load 2 lần) — truyền `false` cho lần load gốc.

Mọi chỗ khác: **xóa** `IncludeSigned: false` (dùng mặc định true).

### 6.6. Cập nhật tài liệu chuẩn

Sửa `docs/code-standards.md` §14:

- Ví dụ hydration **không** còn khuyến khích `IncludeSigned: false` làm mẫu mặc định.
- Ghi rõ: mặc định include signed; chỉ opt-out tường minh.
- Ghi chú trap: `IncludeSigned` chỉ có hiệu lực khi có `BaseGroupTypes`.
- Ghi chú trap: sau khi load multi-type, filter bằng `ToBaseGroupType()`, không so exact `GroupType`.

---

## 7. Danh sách file dự kiến cần sửa

### 7.1. Bắt buộc — Read controllers (`IncludeSigned: false` → bỏ hoặc giữ có lý do)

Tất cả file dưới đây trong `QLDA.WebApi/Controllers` (46 file; ~70+ chỗ gọi):

- `BanGiaoHoSoController.cs` — **đồng thời sửa filter `ToBaseGroupType()`**
- `BaoCaoBanGiaoSanPhamController.cs` — nên thêm `BaseGroupTypes: [BaoCaoBanGiaoSanPham]` để scope chặt + include signed
- `ToTrinhCoThamDinhController.cs`
- `BaoCaoTienDoController.cs`, `BaoCaoBaoHanhSanPhamController.cs`, `BaoCaoKetQuaKhaoSatController.cs`
- `VanBanPhapLyController.cs`, `VanBanChuTruongController.cs`
- `PheDuyetDuToanController.cs`, `PhanKhaiKinhPhiController.cs`
- `DeXuatChuTruongMoiController.cs`, `DeXuatChuTruongChuyenTiepController.cs`
- `DeXuatNhuCauKinhPhiController.cs`, `DeXuatNhuCauKinhPhiNamController.cs`
- `ThuyetMinhDuAnController.cs`, `KhoKhanVuongMacController.cs`
- `QuyetDinh*` (DuyetDuAn, DuyetKHLCNT, DuyetDuToan, DuyetQuyetToan, LapBanQLDA, LapBenMoiThau, LapHoiDongThamDinh)
- `HopDongController.cs`, `PhuLucHopDongController.cs`, `ThanhLyHopDongController.cs`
- `TamUngController.cs`, `ThanhToanController.cs`, `NghiemThuController.cs`
- `GoiThauController.cs`, `KetQuaTrungThauController.cs`
- `KeHoachLuaChonNhaThauController.cs`, `KeHoachLuaChonNhaThauRutGonController.cs`
- `KeHoachTrienKhaiHangMucController.cs`, `KeHoachTrienKhaiChiTietDuAnController.cs`
- `TrienKhaiKeHoachLCNTController.cs`, `ChuTruongLapKeHoachController.cs`
- `DangTaiKeHoachLcntLenMangController.cs`, `HoSoMoiThauDienTuController.cs`
- `HoSoDeXuatCapDoCnttController.cs`, `DuToanDauTuController.cs`, `DuAnController.cs`
- `ToTrinhThamDinhNhaThauController.cs`, `ToTrinhPheDuyetController.cs`, `ToTrinhKetQuaGoiThauController.cs`
- `ThoaThuanGiaoViecController.cs`

### 7.2. Write mapping — chuẩn hóa trùng lặp

| File | Việc làm |
|------|----------|
| `QLDA.Application/BanGiaoHoSos/DTOs/BanGiaoHoSoMappings.cs` | `GetDanhSachTepHSBanGiao` / `GetDanhSachBienBanBanGiao` → dùng `ToEntities` (Application `AttachmentCollectionExtensions` hoặc map qua DTO helper tương đương) |
| `QLDA.WebApi/Models/BanGiaoHoSos/BanGiaoHoSoMappingConfiguration.cs` | `GetDanhSachBienBanBanGiao` → `ToEntities(..., BienBanBanGiao)` |
| `QLDA.WebApi/Models/TepDinhKems/TepDinhKemMappingConfigurations.cs` | Giữ thin wrappers; không đổi hành vi resolve ParentId |

### 7.3. List / print — rà & chỉnh có chủ đích

| File | Việc làm |
|------|----------|
| `BanGiaoHoSoGetDanhSachQuery.cs` | Đã `includeSigned: true` — giữ |
| `BanGiaoHoSoPrintQuery.cs` | **Cần quyết định nghiệp vụ** (giữ false hay đổi true) |
| `BaoCaoBanGiaoSanPhamGetDanhSachQuery.cs` | Nên đổi sang `ExpandGroupTypes` theo `BaoCaoBanGiaoSanPham` (tránh lẫn GroupType khác nếu sau này thêm) |
| Các list đã Expand `true` | Giữ |
| Các list chỉ filter `GroupId` | Optional harden: thêm Expand theo base GroupType (không bắt buộc nếu GroupId chỉ có 1 loại file) |

### 7.4. Docs / tests

| File | Việc làm |
|------|----------|
| `docs/code-standards.md` §14 | Đổi ví dụ mặc định include signed + ghi trap |
| `docs/issues/Refactor Consolidate attachment files/*` | Có thể thêm note “supplemented by 2026-07-23 spec” — optional |
| `BuildingBlocks.Tests` Phase3/4 attachment tests | Bổ sung/assert default `IncludeSigned=true`; test Expand |
| `QLDA.Tests` Phase3/4 | Cập nhật nếu có assert `IncludeSigned: false` |

### 7.5. Không sửa

- Migration / `AppDbContextModelSnapshot`
- `AttachmentBulkInsertOrUpdateCommand` hành vi sync
- Rename API contract (`DanhSachTepDinhKem`, …)
- Entity Domain Attachment schema

---

## 8. Rủi ro & regression

| Rủi ro | Mức | Mitigation |
|--------|-----|------------|
| UI/FE nhận thêm file `KySo_*` trong cùng list → có thể hiển thị trùng nếu FE không phân biệt ParentId | Trung bình | FE thường đã hỗ trợ ParentId; verify 1–2 màn ký số |
| Multi-property model (gốc vs ký số tách cột) bị gộp nhầm nếu bỏ `false` | Trung bình | Giữ `IncludeSigned: false` + load `KySo_*` riêng nơi đã tách property |
| BanGiao filter exact GroupType quên sửa | Cao | Checklist bắt buộc trong PR |
| `IncludeSigned` với `BaseGroupTypes == null` vẫn no-op | Thấp (đã hiểu) | Khi scope chặt luôn truyền BaseGroupTypes |
| Write BanGiao đổi sang `ToEntities` → đổi cách gen Id | Trung bình | Dùng cùng `ResolveId` WebApi path; regression insert/update BanGiao |

**GitNexus:** Trước khi edit symbol, chạy `impact` upstream cho `GetAttachmentsQuery` / mapping methods; trước commit chạy `detect_changes`.

---

## 9. Kế hoạch triển khai (sau khi approve spec)

1. Cập nhật `code-standards.md` §14 (quy ước).
2. Sửa `BanGiaoHoSo` + `BaoCaoBanGiaoSanPham` end-to-end (read + write mapping) làm mẫu chuẩn.
3. Bulk bỏ `IncludeSigned: false` trên controllers (trừ ngoại lệ đã liệt kê).
4. Chuẩn hóa write BanGiao thin wrapper.
5. Harden list BaoCaoBanGiaoSanPham (ExpandGroupTypes).
6. Cập nhật / chạy unit tests BB + QLDA attachment.
7. `dotnet build SER.sln` — sửa hết lỗi compile.
8. `detect_changes` trước commit (khi user yêu cầu commit).

---

## 10. Tiêu chí Done

- [x] Không còn caller Get chi tiết “mặc định” ép `IncludeSigned: false` trừ ngoại lệ có comment lý do.
- [x] BanGiaoHoSo chi tiết trả cả `BanGiaoHoSo` + `KySo_BanGiaoHoSo` (và tương tự biên bản).
- [x] BaoCaoBanGiaoSanPham chi tiết scope theo `BaseGroupTypes` + include signed.
- [x] Write BanGiao không còn `new Attachment { ... }` duplicate.
- [x] `code-standards.md` §14 khớp quy ước mới.
- [x] Build solution 0 error; tests attachment liên quan pass.
- [x] Không có migration mới.

---

## 11. Câu hỏi chờ xác nhận (trước implement)

1. **`BanGiaoHoSoPrintQuery.TongSoTepDinhKem`:** giữ chỉ đếm file gốc (`includeSigned: false`) hay đếm cả ký số?
2. **Approach A vs B:** đồng ý **không** tạo `AttachmentQueryOptions` trong đợt này (chỉ flip default + chuẩn hóa helper), đúng không?
3. Các màn đã **tách** `DanhSachTepDinhKem` / `DanhSachKySo*` (nếu còn): giữ 2 lần query với `IncludeSigned: false` cho gốc — cần liệt kê giữ nguyên khi rà, hay gộp hết vào một list?

---

## Phụ lục A — Sơ đồ luồng Read mục tiêu

```text
Controller Get(id)
  → Get entity (query handler, không load file)
  → GetAttachmentsQuery(GroupIds, BaseGroupTypes, IncludeSigned=true mặc định)
       → ExpandGroupTypes → [base, KySo_base]
       → filter GroupId + GroupType IN (...)
  → ToAttachmentEntities()
  → (multi-type) split bằng ToBaseGroupType()
  → ToModel / ToDto
```

## Phụ lục B — Sơ đồ Write mục tiêu

```text
Controller Insert/Update
  → model.GetDanhSachTep*(entity.Id)   // thin wrapper
       → DanhSach*.ToEntities(groupId, baseGroupType)
            → Resolve GroupType theo ParentId (KySo_ nếu child)
  → AttachmentBulkInsertOrUpdateCommand(GroupId, GroupTypes=[base], Entities, AutoDeleteMissing)
       // Sync vẫn Expand KySo_ trong scope — không đổi lệnh này
```
