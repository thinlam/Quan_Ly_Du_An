# Bug: API danh sách phê duyệt thiếu file ký số (Hồ sơ mời thầu điện tử)

> Ngày: 2026-07-27  
> API: `GET /api/phe-duyet/danh-sach?type=HoSoMoiThauDienTu&trangThai=ĐTr&...`  
> Query/Handler: `PheDuyetGetDanhSachQuery` → `PheDuyetQueryableExtensions.AttachTepDinhKem`  
> Phạm vi sửa: **chỉ** load tệp đính kèm trên danh sách phê duyệt — không migration, không refactor lan man.

---

## 1. Triệu chứng

Với `type=HoSoMoiThauDienTu`, API danh sách phê duyệt:

- Trả được **file gốc** (`GroupType = HoSoMoiThauDienTu`, …).
- **Thiếu file ký số** (`GroupType = KySo_HoSoMoiThauDienTu`, `KySo_HoSoMoiThauDienTuToTrinh`, …).
- Số lượng file trên UI / `danhSachTepDinhKem.length` **không khớp** tổng gốc + ký số.

Ví dụ mong đợi: 2 file gốc + 1 file ký số → API phải trả **3** file; count trên màn hình = 3.

---

## 2. So sánh với API chi tiết / danh sách Hồ sơ mời thầu điện tử (đúng)

### 2.1. Chi tiết — `HoSoMoiThauDienTuController.Get`

```csharp
var files = (await Mediator.Send(new GetAttachmentsQuery(
    GroupIds: [groupId],
    BaseGroupTypes: [EGroupType.HoSoMoiThauDienTu.ToString()]
    // IncludeSigned = true (mặc định)
))).ToAttachmentEntities();
```

`GetAttachmentsQuery` gọi:

```csharp
AttachmentSubquery.ExpandGroupTypes(baseGroupTypes, includeSigned: true)
// → ["HoSoMoiThauDienTu", "KySo_HoSoMoiThauDienTu"]
```

→ Filter `GroupType IN (base, KySo_base)` — **có đủ file ký số**.

### 2.2. Danh sách hồ sơ — `HoSoMoiThauDienTuGetDanhSachQuery`

```csharp
var groupTypesOnEntityId = AttachmentSubquery.ExpandGroupTypes(
    includeSigned: true,
    nameof(EGroupType.HoSoMoiThauDienTu),
    nameof(EGroupType.HoSoMoiThauDienTuToTrinh),
    // … các base HSMTĐT khác
);
// Where: groupTypesOnEntityId.Contains(i.GroupType)
```

→ Cũng mở rộng sang `KySo_*` trước khi filter.

### 2.3. Helper chuẩn dự án

| Helper | Vai trò |
|--------|---------|
| `AttachmentSubquery.ExpandGroupTypes` | base → `[base, KySo_base]` (mặc định includeSigned = true) |
| `SignedGroupTypeHelper.WithSignedVariant` | `"X"` → `"KySo_X"` |
| `GetAttachmentsQuery` | Hydration controller; mặc định IncludeSigned = true |

**Quy ước:** mặc định luôn gồm file ký số; chỉ bỏ khi `IncludeSigned = false` tường minh.  
**Không** loại file vì `ParentId != null` (file ký số thường trỏ ParentId về file gốc).

---

## 3. Chỗ sai (chi tiết)

### 3.1. File / method

| | |
|--|--|
| **File** | `QLDA.Application/QuanLyPheDuyet/Queries/PheDuyetQueryableExtensions.cs` |
| **Method** | `AttachTepDinhKem` (gọi từ `ApplyDanhSachFilters` sau khi materialize list) |
| **Caller** | `PheDuyetGetDanhSachQueryHandler.Handle` |

`PheDuyetGetDanhSachQueryHandler` **không** tự load file — ủy quyền cho `ApplyDanhSachFilters` → `AttachTepDinhKem`.

### 3.2. Code trước khi sửa (sai / lệch chuẩn)

```csharp
private static void AttachTepDinhKem(
    List<PheDuyetListItemDto> items,
    IRepository<Attachment, Guid> tepDinhKemRepo)
{
    var groupIds = items.Select(i => i.EntityId)...;

    var files = tepDinhKemRepo.GetQueryableSet()
        .AsNoTracking()
        .Where(i => groupIds.Contains(i.GroupId))   // ← chỉ GroupId
        .ToList();                                    // ← không ExpandGroupTypes

    AssignAttachments(items, files);
}
```

### 3.3. Vì sao sai

1. **Lệch pattern đọc file chuẩn của dự án**  
   Chi tiết / list HSMTĐT dùng `ExpandGroupTypes` / `GetAttachmentsQuery` để luôn gồm `KySo_<base>`.  
   Danh sách phê duyệt **không** gọi helper đó → không có contract rõ ràng “mặc định include ký số”.

2. **Với `type = HoSoMoiThauDienTu` cần nhiều base GroupType**  
   Không chỉ `HoSoMoiThauDienTu` mà còn `…ToTrinh`, `…QuyetDinh`, thẩm định, …  
   Nếu sau này (hoặc FE / logic trung gian) scope exact `GroupType == EntityName` (`HoSoMoiThauDienTu`) **mà không** expand → **mọi** `KySo_*` bị loại, dù cùng `GroupId`.

3. **Không có cờ `IncludeSigned`**  
   Không opt-out tường minh; không đồng bộ với `GetAttachmentsQuery(IncludeSigned = true mặc định)`.

4. **Không dedupe theo `Id`**  
   Nếu join / batch trùng bản ghi, list + count có thể lệch (yêu cầu: không trả trùng).

5. **Không liên quan lọc `ParentId`**  
   `AttachTepDinhKem` cũ **không** `Where(ParentId == null)` — đúng về mặt này.  
   Vẫn phải **giữ** nguyên khi sửa: file có `ParentId` / `KySo_*` vẫn trả về.

> Ghi chú tách bug `GroupId` tờ trình/QĐ (lưu sai Id tờ trình): xem `docs/issues/hsmtdt-groupid-tep-totrinh-quyetdinh/report.md`.  
> Bug **này** tập trung **GroupType / ExpandGroupTypes / IncludeSigned** trên màn phê duyệt.

---

## 4. Cách sửa (chi tiết từng chỗ)

### Nguyên tắc

- Dùng `AttachmentSubquery.ExpandGroupTypes` — **không** hard-code `OR GroupType == "KySo_..."`.
- `IncludeSigned = true` mặc định; chỉ `false` khi caller truyền rõ.
- HSMTĐT: expand **đủ** base GroupType nghiệp vụ + `KySo_*`.
- Loại phê duyệt khác: **không** đổi (vẫn load theo `GroupId`, mọi GroupType).
- Không lọc `ParentId`; dedupe theo `Attachment.Id`.
- Không migration.

---

### 4.1. `PheDuyetGetDanhSachQuery` — thêm `IncludeSigned`

**File:** `QLDA.Application/QuanLyPheDuyet/Queries/PheDuyetGetDanhSachQuery.cs`

```csharp
/// <summary>
/// Mặc định true — lấy cả file gốc và KySo_* (qua ExpandGroupTypes).
/// Chỉ đặt false khi caller tường minh không cần file ký số.
/// </summary>
public bool IncludeSigned { get; set; } = true;
```

Trong `Handle`, truyền xuống:

```csharp
PheDuyetQueryableExtensions.ApplyDanhSachFilters(
    ...,
    includeAttachments: request.IncludeAttachments,
    includeSigned: request.IncludeSigned);
```

---

### 4.2. `ApplyDanhSachFilters` / `AttachTepDinhKem` — ExpandGroupTypes cho HSMTĐT

**File:** `QLDA.Application/QuanLyPheDuyet/Queries/PheDuyetQueryableExtensions.cs`

**Thêm danh sách base type** (khớp list/chi tiết hồ sơ):

```csharp
private static readonly string[] HoSoMoiThauDienTuBaseGroupTypes =
[
    nameof(EGroupType.HoSoMoiThauDienTu),
    nameof(EGroupType.HoSoMoiThauDienTuToTrinh),
    nameof(EGroupType.HoSoMoiThauDienTuQuyetDinh),
    nameof(EGroupType.HoSoMoiThauDienTuQuyetDinhTD),
    nameof(EGroupType.HoSoMoiThauDienTuCamKetTD),
    nameof(EGroupType.HoSoMoiThauDienTuBaoCaoTD),
];
```

**Sửa `AttachTepDinhKem`:**

```csharp
private static void AttachTepDinhKem(
    List<PheDuyetListItemDto> items,
    IRepository<Attachment, Guid> tepDinhKemRepo,
    bool includeSigned = true)
{
    var groupIds = items.Select(i => i.EntityId)
        .Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
    // ...

    var query = tepDinhKemRepo.GetQueryableSet()
        .AsNoTracking()
        .Where(i => groupIds.Contains(i.GroupId));

    var hsmtGroupIds = items
        .Where(i => i.EntityName == PheDuyetEntityNames.HoSoMoiThauDienTu
                    && !string.IsNullOrEmpty(i.EntityId))
        .Select(i => i.EntityId)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    if (hsmtGroupIds.Count > 0) {
        var hsmtGroupTypes = AttachmentSubquery.ExpandGroupTypes(
            includeSigned,
            HoSoMoiThauDienTuBaseGroupTypes);
        // HSMTĐT: GroupId đúng hồ sơ + GroupType ∈ (base ∪ KySo_base)
        // Loại khác trong cùng batch: không áp filter GroupType
        query = query.Where(i =>
            !hsmtGroupIds.Contains(i.GroupId!)
            || hsmtGroupTypes.Contains(i.GroupType));
    }

    var files = query.ToList().DistinctBy(f => f.Id).ToList();
    AssignAttachments(items, files);
}
```

**`AssignAttachments`:** thêm `.DistinctBy(f => f.Id)` trước `ToDto()` để list + count thống nhất, không trùng.

---

### 4.3. Việc **không** làm

| Không làm | Lý do |
|-----------|--------|
| Hard-code `GroupType == "KySo_HoSoMoiThauDienTu"` | Đã có `ExpandGroupTypes` |
| `Where(ParentId == null)` | Loại hết file ký số |
| Sửa API chi tiết HSMTĐT | Đã đúng |
| Migration / đổi snapshot | Ngoài phạm vi |
| Đổi loại phê duyệt khác | Giữ load theo `GroupId` |

---

## 5. Kết quả mong đợi

| Trường hợp | Kỳ vọng |
|------------|---------|
| 2 gốc + 1 `KySo_HoSoMoiThauDienTu` cùng `GroupId = EntityId` | `danhSachTepDinhKem` length = **3** |
| `IncludeSigned = false` | Chỉ base GroupType, không `KySo_*` |
| `type` khác HSMTĐT | Hành vi cũ (mọi GroupType theo GroupId) |
| File có `ParentId != null` | Vẫn có trong list |

---

## 6. Checklist kiểm thử

- [ ] `GET /api/phe-duyet/danh-sach?type=HoSoMoiThauDienTu&trangThai=ĐTr&pageIndex=1&pageSize=10` — đủ gốc + `KySo_*` cùng `groupId = entityId`
- [ ] Count UI = `danhSachTepDinhKem.length`
- [ ] So với `GET /api/ho-so-moi-thau-dien-tu/{id}`: cùng tập file gắn `GroupId = hồ sơ` (trừ nhánh legacy ToTrinh/QĐ Id nếu còn)
- [ ] `type=BanGiaoHoSo` (hoặc loại khác) — không regress số file
- [ ] Unit: `IncludeSigned` default true; `AssignAttachments` giữ file có `ParentId` + `KySo_*`; dedupe theo Id

---

## 7. File đụng tới

| File | Thay đổi |
|------|----------|
| `QLDA.Application/.../PheDuyetGetDanhSachQuery.cs` | `IncludeSigned`; truyền vào filter |
| `QLDA.Application/.../PheDuyetQueryableExtensions.cs` | `ExpandGroupTypes` cho HSMTĐT; dedupe |
| `QLDA.Tests/Unit/PheDuyetQueryableExtensionsAttachmentTests.cs` | Assert signed + default IncludeSigned |
| `docs/issues/phe-duyet-hsmtdt-thieu-ky-so/report.md` | Doc này |
