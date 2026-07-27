# Issue — API danh sách Hồ sơ mời thầu điện tử trả thiếu file ký số so với API chi tiết

> Ngày ghi nhận / sửa: 2026-07-24  
> Trạng thái: ✅ Đã sửa (Approach A)  
> Phạm vi: **chỉ** logic load/tổng hợp attachment của API danh sách  
> Liên quan: `docs/superpowers/specs/2026-07-23-attachment-include-signed-default-true-design.md`, Attachment Phase 4 (`AttachmentSubquery.ExpandGroupTypes`)

---

## Triệu chứng (trước khi sửa)

API lỗi:

```http
GET /api/ho-so-moi-thau-dien-tu/danh-sach
```

Dữ liệu kiểm tra:

```text
HoSoMoiThauDienTuId: 08dee930-5abe-25bd-687a-7b4630000f5a
DuAnId: 08dedfdb-50a1-3067-687a-7b122003ce93
BuocId: 6894
```

| API | Số file (trước) | Số file (sau — mong đợi) |
|-----|----------------:|-------------------------:|
| Chi tiết `GET /api/ho-so-moi-thau-dien-tu/{id}` | **10** | **10** (không đổi) |
| Danh sách `GET .../danh-sach` | **8** | **10** |

Chi tiết đủ 10 file:

| Nhóm file            | File gốc | File ký số | Tổng |
| -------------------- | -------: | ---------: | ---: |
| Quyết định thẩm định |        2 |          2 |    4 |
| Cam kết thẩm định    |        1 |          1 |    2 |
| Tờ trình             |        1 |          1 |    2 |
| Quyết định           |        1 |          1 |    2 |
| **Tổng**             |    **5** |      **5** | **10** |

Hai file thiếu trên danh sách (trước khi sửa):

```text
KySo_HoSoMoiThauDienTuToTrinh
GroupId: 10084
File: Qui trinh QLDA (tach theo loại DA) 3 (1)_240720261004021.signed.pdf
```

```text
KySo_HoSoMoiThauDienTuQuyetDinh
GroupId: 10083
File: 1.1TTCDS_Danh sách UC (v2.0)_240720261004032.signed.pdf
```

File gốc `ToTrinh` / `QuyetDinh` đã có trong list; chỉ thiếu bản `KySo_*`.

---

## Kết luận ngắn

| Câu hỏi | Kết luận |
|---------|----------|
| DB thiếu file ký số? | **Không** — chi tiết đã trả đủ; dữ liệu đúng. |
| API chi tiết sai? | **Không** — dùng `GetAttachmentsQuery` + `ExpandGroupTypes` (mặc định `IncludeSigned = true`). |
| Root cause | API danh sách filter **exact** `GroupType` gốc cho `ToTrinh`/`QuyetDinh` → loại hết `KySo_*`. |
| Cách sửa | Approach A — `ExpandGroupTypes(includeSigned: true)` + `Contains` trong subquery list. |
| File sửa | `HoSoMoiThauDienTuGetDanhSachQuery.cs` |
| Migration / DB? | **Không** đụng. |
| API chi tiết? | **Không** đổi. |

---

## 1. Mapping GroupId / GroupType

| Nhóm | Base GroupType | Signed GroupType | GroupId |
|------|----------------|------------------|---------|
| Hồ sơ (base, nếu có) | `HoSoMoiThauDienTu` | `KySo_HoSoMoiThauDienTu` | `HoSoMoiThauDienTu.Id` (Guid) |
| Quyết định thẩm định | `HoSoMoiThauDienTuQuyetDinhTD` | `KySo_HoSoMoiThauDienTuQuyetDinhTD` | `HoSoMoiThauDienTu.Id` |
| Cam kết thẩm định | `HoSoMoiThauDienTuCamKetTD` | `KySo_HoSoMoiThauDienTuCamKetTD` | `HoSoMoiThauDienTu.Id` |
| Báo cáo thẩm định | `HoSoMoiThauDienTuBaoCaoTD` | `KySo_HoSoMoiThauDienTuBaoCaoTD` | `HoSoMoiThauDienTu.Id` |
| Tờ trình | `HoSoMoiThauDienTuToTrinh` | `KySo_HoSoMoiThauDienTuToTrinh` | `ToTrinh.Id` (vd. `10084`) |
| Quyết định | `HoSoMoiThauDienTuQuyetDinh` | `KySo_HoSoMoiThauDienTuQuyetDinh` | `QuyetDinh.Id` (vd. `10083`) |

Mỗi bản ký số là một `Attachment` riêng (`GroupType` bắt đầu `KySo_`). **Không** lọc bỏ vì có `ParentId`.

---

## 2. Luồng đọc file

### 2.1 API chi tiết — đúng (không đổi)

`HoSoMoiThauDienTuController.Get`:

```
GetAttachmentsQuery(GroupIds=[entity.Id], BaseGroupTypes=[HoSoMoiThauDienTu])
GetAttachmentsQuery(GroupIds=[ToTrinh.Id], BaseGroupTypes=[HoSoMoiThauDienTuToTrinh])
GetAttachmentsQuery(GroupIds=[QuyetDinh.Id], BaseGroupTypes=[HoSoMoiThauDienTuQuyetDinh])
GetAttachmentsQuery(GroupIds=[entity.Id], BaseGroupTypes=[CamKetTD | QuyetDinhTD | BaoCaoTD])
```

`GetAttachmentsQuery` → `ExpandGroupTypes(..., includeSigned: true)` → gốc + `KySo_*`.

File: `QLDA.WebApi/Controllers/HoSoMoiThauDienTuController.cs`

### 2.2 API danh sách — trước (bug)

```csharp
// TRƯỚC
DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
    .Where(i =>
        !i.IsDeleted &&
        (
            i.GroupId == e.Id.ToString()   // mọi type trên entity.Id
            || (e.ToTrinh != null
                && i.GroupId == e.ToTrinh.Id.ToString()
                && i.GroupType == EGroupType.HoSoMoiThauDienTuToTrinh.ToString())  // exact ← thiếu KySo_
            || (e.QuyetDinh != null
                && i.GroupId == e.QuyetDinh.Id.ToString()
                && i.GroupType == EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString()) // exact ← thiếu KySo_
        )
    )...
```

Vì sao list = 8 (không phải 5)?

| Nhóm | Trên list (cũ) |
|------|----------------|
| Thẩm định | `GroupId == entity.Id` không filter type → có cả `KySo_*` |
| ToTrinh / QuyetDinh gốc | Exact match → có |
| `KySo_...ToTrinh` / `KySo_...QuyetDinh` | Exact không khớp → **thiếu** |

### 2.3 API danh sách — sau (đã sửa)

File: `QLDA.Application/HoSoMoiThauDienTus/Queries/HoSoMoiThauDienTuGetDanhSachQuery.cs`

```csharp
var groupTypesOnEntityId = AttachmentSubquery.ExpandGroupTypes(
    includeSigned: true,
    nameof(EGroupType.HoSoMoiThauDienTu),
    nameof(EGroupType.HoSoMoiThauDienTuQuyetDinhTD),
    nameof(EGroupType.HoSoMoiThauDienTuCamKetTD),
    nameof(EGroupType.HoSoMoiThauDienTuBaoCaoTD));
var groupTypesToTrinh = AttachmentSubquery.ExpandGroupTypes(
    includeSigned: true, nameof(EGroupType.HoSoMoiThauDienTuToTrinh));
var groupTypesQuyetDinh = AttachmentSubquery.ExpandGroupTypes(
    includeSigned: true, nameof(EGroupType.HoSoMoiThauDienTuQuyetDinh));

// Trong .Select():
DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
    .Where(i =>
        (i.GroupId == e.Id.ToString() && groupTypesOnEntityId.Contains(i.GroupType))
        || (e.ToTrinh != null
            && i.GroupId == e.ToTrinh.Id.ToString()
            && groupTypesToTrinh.Contains(i.GroupType))
        || (e.QuyetDinh != null
            && i.GroupId == e.QuyetDinh.Id.ToString()
            && groupTypesQuyetDinh.Contains(i.GroupType))
    ).Select(i => i.ToDto()).ToList()
```

Cũng bỏ `!i.IsDeleted` thừa (`GetQueryableSet()` đã filter).

### 2.4 Helper dùng chung

| Helper | File | Vai trò |
|--------|------|---------|
| `AttachmentSubquery.ExpandGroupTypes` | `BuildingBlocks.../AttachmentSubquery.cs` | base → `[base, KySo_base]` |
| `GetAttachmentsQuery` | BB Queries | Hydration controller chi tiết |
| `SignedGroupTypeHelper` | BB Common | Convention `KySo_` |

Pattern giống `BanGiaoHoSoGetDanhSachQuery`, `BaoCaoBanGiaoSanPhamGetDanhSachQuery`.

---

## 3. Approach đã chọn

| Approach | Mô tả | Kết quả |
|----------|--------|---------|
| **A** | `ExpandGroupTypes` trong subquery list | ✅ **Đã implement** |
| B | Post-load `GetAttachmentsQuery` sau page | Không chọn (thêm round-trip) |
| C | Hard-code OR `KySo_*` | Không chọn (trùng logic) |

---

## 4. Phạm vi

### Đã sửa

- `QLDA.Application/HoSoMoiThauDienTus/Queries/HoSoMoiThauDienTuGetDanhSachQuery.cs`

### Không sửa

- API chi tiết (`HoSoMoiThauDienTuController.Get`)
- Write / `AttachmentBulkInsertOrUpdateCommand`
- Migration, snapshot, dữ liệu DB
- Helper BB

---

## 5. Kết quả mong đợi / test

Với hồ sơ `08dee930-5abe-25bd-687a-7b4630000f5a`:

- Danh sách trả **10** file, khớp chi tiết.
- Có đủ `KySo_HoSoMoiThauDienTuToTrinh` và `KySo_HoSoMoiThauDienTuQuyetDinh`.
- Hồ sơ không có file ký số: chỉ trả file gốc.

| Metric | Trước | Sau |
|--------|------:|----:|
| File list | 8 | 10 |
| `KySo_...ToTrinh` | thiếu | có |
| `KySo_...QuyetDinh` | thiếu | có |
| API chi tiết | 10 | 10 |

**Build:** `dotnet build QLDA.Application -c Release` → 0 error / 0 warning (2026-07-24).

**Manual API** (cần verify khi WebApi chạy):

1. Case có ký số — hồ sơ trên → list = 10.
2. Case không ký số → list = số file gốc (khớp chi tiết).
3. Entity không có ToTrinh/QuyetDinh → không throw.

---

## 6. Checklist

- [x] Phân tích root cause (exact GroupType ToTrinh/QuyetDinh)
- [x] Chốt Approach A
- [x] Sửa `HoSoMoiThauDienTuGetDanhSachQuery` + `ExpandGroupTypes`
- [x] Build `QLDA.Application` Release — 0 error / 0 warning
- [ ] Test manual API (hồ sơ `08dee930-...`)
- [ ] Commit (khi user yêu cầu)
