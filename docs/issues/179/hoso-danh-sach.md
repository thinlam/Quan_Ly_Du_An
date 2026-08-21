# Issue 179 leftover — `GET /api/ho-so-moi-thau-dien-tu/danh-sach` trả 400 vì `.Include()` property `[NotMapped]`

> **Chưa implement.** Khảo sát + plan. Chờ xác nhận rồi mới sửa code.

```http
GET /api/ho-so-moi-thau-dien-tu/danh-sach?pageIndex=1&pageSize=10&duAnId=08def36f-9d7f-6e89-687a-7b2ea004c65e
```

Response hiện tại:

```json
{
  "result": false,
  "errorMessage": "Lỗi hệ thống, vui lòng thử lại sau",
  "dataResult": null,
  "statusCode": 400
}
```

---

## 1. Root cause (đã xác nhận bằng exception thật)

**Không phải phỏng đoán.** Stack trace request `09:06:39` (localhost:5193) khớp đúng URL trên.

| Mục | Chi tiết |
|-----|----------|
| Exception | `System.InvalidOperationException` |
| Message | `The expression 'e.ToTrinh' is invalid inside an 'Include' operation, since it does not represent a property access: 't => t.MyProperty'. ...` |
| Client thấy | Middleware bọc thành `"Lỗi hệ thống, vui lòng thử lại sau"` + HTTP **400** (`ExceptionMiddleware` / `UnhandledExceptionBehavior`) |
| Nơi nổ | `HoSoMoiThauDienTuGetDanhSachQueryHandler.Handle` → `PaginatedListAsync` → `CountAsync` (dòng **68** file query) |
| Controller | `HoSoMoiThauDienTuController.GetAll` dòng **58** |
| EF | `NavigationExpandingExpressionVisitor.ProcessInclude` — fail **lúc compile query**, trước khi chạy SQL |

Hai dòng gây lỗi:

```34:35:QLDA.Application/HoSoMoiThauDienTus/Queries/HoSoMoiThauDienTuGetDanhSachQuery.cs
            .Include(e => e.ToTrinh)
            .Include(e => e.QuyetDinh)
```

Hai property **không còn navigation EF**:

```34:37:QLDA.Domain/Entities/HoSoMoiThauDienTu.cs
    [NotMapped]
    public ToTrinhQuyetDinh? QuyetDinh { get; set; }
    [NotMapped]
    public ToTrinhQuyetDinh? ToTrinh { get; set; }
```

`HoSoMoiThauDienTuConfiguration` **không** `HasOne` `ToTrinh`/`QuyetDinh` — comment Issue #179: load thủ công `EntityId + Loai`.

EF không coi `[NotMapped]` là navigation → `.Include(e => e.ToTrinh)` invalid.

---

## 2. Vì sao entity đã `[NotMapped]` nhưng handler vẫn `.Include()`

Issue #179 đổi `ToTrinhQuyetDinh` sang bảng dùng chung (`EntityId` + `Loai`), bỏ FK 1-1.

Đã cập nhật **write path**:

- `HoSoMoiThauDienTuInsertCommand` — tách `ToTrinh`/`QuyetDinh` khỏi entity trước `Add`, ghi repo với `Loai`
- `HoSoMoiThauDienTuUpdateCommand` — load thủ công `EntityId + Loai`, **không** Include
- `HoSoMoiThauDienTuDuyetCommand` — load `HoSoMoiThauQuyetDinh` thủ công

**Chưa cập nhật read path** (sót):

| File | Dòng | Vấn đề |
|------|------|--------|
| `HoSoMoiThauDienTuGetDanhSachQuery.cs` | 34–35, và `Select` 91–98 dùng `e.ToTrinh` / `e.QuyetDinh` | API danh sách **400** (bug này) |
| `HoSoMoiThauDienTuGetQuery.cs` | 24–25 | `GET /{id}` **cùng exception** nếu gọi — cùng nguyên nhân, cùng module |

Journal #179 (2026-08-12) ghi rõ Insert/Update/Duyệt đổi sang query thủ công; **không** ghi Get/danh-sach.

`HoSoMoiThauDienTuDto` (list) hiện **không** có property `ToTrinh`/`QuyetDinh` — list chỉ dùng 2 navigation trong subquery file legacy. Sau khi bỏ Include, giữ `e.ToTrinh` trong `.Select()` LINQ-to-SQL cũng **không dịch được** (NotMapped). Phải tách load file + map object ra khỏi IQueryable.

---

## 3. `Loai` — không đoán

`QLDA.Domain/Constants/ToTrinhQuyetDinhLoai.cs`:

| Constant | Giá trị string (DB) | Dùng cho |
|----------|---------------------|----------|
| `HoSoMoiThauToTrinh` | `"HoSoMoiThauToTrinh"` | Tờ trình HSMTĐT |
| `HoSoMoiThauQuyetDinh` | `"HoSoMoiThauQuyetDinh"` | Quyết định HSMTĐT |
| `ToTrinhThamDinhNhaThau` | `"ToTrinhThamDinhNhaThau"` | Nghiệp vụ khác — **không** dùng ở đây |

Insert/Update/Duyệt đã gán đúng 2 constant đầu. List phải filter y như vậy.

---

## 4. Pattern reuse (batch, không N+1)

**Load 1 record** (đã có sẵn, HSMTĐT):

```csharp
// HoSoMoiThauDienTuUpdateCommand.cs:36-39
entity.ToTrinh = await _toTrinhQuyetDinhRepo.GetQueryableSet()
    .FirstOrDefaultAsync(x => x.EntityId == entity.Id && x.Loai == ToTrinhQuyetDinhLoai.HoSoMoiThauToTrinh, ...);
entity.QuyetDinh = await _toTrinhQuyetDinhRepo.GetQueryableSet()
    .FirstOrDefaultAsync(x => x.EntityId == entity.Id && x.Loai == ToTrinhQuyetDinhLoai.HoSoMoiThauQuyetDinh, ...);
```

**Load danh sách (batch)** — `ToTrinhThamDinhNhaThauGetDanhSachQuery.cs` ~87–93:

1. `PaginatedListAsync` trước (không Include NotMapped).
2. Lấy `ids` trang hiện tại.
3. **Một** query `ToTrinhQuyetDinh` với `toTrinhIds.Contains(e.EntityId) && e.Loai == ...`.
4. Dictionary / group theo `EntityId` rồi gán lại item.

List HSMTĐT làm tương tự, **một** query với:

```csharp
ids.Contains(e.EntityId) && (
    e.Loai == ToTrinhQuyetDinhLoai.HoSoMoiThauToTrinh
    || e.Loai == ToTrinhQuyetDinhLoai.HoSoMoiThauQuyetDinh)
```

Rồi tách:

- `Loai == HoSoMoiThauToTrinh` → `dto.ToTrinh`
- `Loai == HoSoMoiThauQuyetDinh` → `dto.QuyetDinh`
- không có dòng → `null`

Không tạo helper/service Application mới. Không query trong `foreach`.

DTO lồng: reuse `ToTrinhQuyetDinhDto` (`QLDA.Application/HoSoMoiThauDienTus/DTOs/ToTrinhQuyetDinhDto.cs`) — không tạo type mới.

File đính kèm: comment handler đã nói dữ liệu mới `GroupId = HoSo.Id`. Subquery hiện còn nhánh legacy `e.ToTrinh.Id` / `e.QuyetDinh.Id`. Sau khi batch load `ToTrinhQuyetDinh`, hydrate file **sau** pagination (cùng kiểu list tờ trình thẩm định): `GroupId` ∈ { `hoSo.Id`, `toTrinh.Id`, `quyetDinh.Id` } + `ExpandGroupTypes` sẵn có. Không giữ `e.ToTrinh` trong `.Select()` IQueryable.

---

## 5. File dự kiến sửa

| File | Việc |
|------|------|
| `QLDA.Application/HoSoMoiThauDienTus/Queries/HoSoMoiThauDienTuGetDanhSachQuery.cs` | Bỏ Include `ToTrinh`/`QuyetDinh`; bỏ `e.ToTrinh`/`e.QuyetDinh` khỏi Select; paginate; batch `ToTrinhQuyetDinh`; map DTO; hydrate file |
| `QLDA.Application/HoSoMoiThauDienTus/DTOs/HoSoMoiThauDienTuDto.cs` | Thêm `ToTrinhQuyetDinhDto? ToTrinh`, `ToTrinhQuyetDinhDto? QuyetDinh` (list hiện thiếu — FE/requirement cần object) |

**Cùng nguyên nhân, cùng module — đề xuất sửa luôn (xác nhận):**

| File | Việc |
|------|------|
| `QLDA.Application/HoSoMoiThauDienTus/Queries/HoSoMoiThauDienTuGetQuery.cs` | Bỏ Include; load thủ công 2 dòng `EntityId + Loai` (pattern UpdateCommand). `GET /{id}` + `ToModel` đang cần `entity.ToTrinh`/`QuyetDinh` |

Không đụng:

- Domain `[NotMapped]` / EF Configuration / migration / snapshot
- Insert / Update / Duyệt (đã đúng)
- Nghiệp vụ tờ trình thẩm định nhà thầu

**Domain / Persistence / Migration: không.**

---

## 6. Cách sửa (checklist)

Thứ tự bắt buộc — **không** bỏ `[NotMapped]`, **không** restore EF navigation.

| Bước | Việc | File / chỗ |
|------|------|------------|
| 1 | Thêm `ToTrinh` / `QuyetDinh` (nếu chưa có) | `HoSoMoiThauDienTuDto.cs` sau `DanhSachTepDinhKem` — **đã có dòng 23–24** |
| 2 | Thêm extension `ToTrinhQuyetDinh.ToDto()` | `HoSoMoiThauDienTuMappings.cs` — **sau `Update()`, trước `}` class (~dòng 69–73)** |
| 3 | Inject `IRepository<ToTrinhQuyetDinh, long>` | `HoSoMoiThauDienTuGetDanhSachQuery.cs` — field + constructor handler |
| 4 | **Xóa** `.Include(e => e.ToTrinh)` và `.Include(e => e.QuyetDinh)` | `HoSoMoiThauDienTuGetDanhSachQuery.cs` dòng **34–35** |
| 5 | **Xóa** nhánh `e.ToTrinh` / `e.QuyetDinh` trong subquery file | cùng file, dòng **91–98** |
| 6 | `PaginatedListAsync` giữ nguyên filter | cùng file |
| 7 | Batch `ToTrinhQuyetDinh` + gán `item.ToTrinh = tt.ToDto()` | cùng file, **sau** `PaginatedListAsync` |
| 8 | (Legacy) batch file theo `ToTrinhQuyetDinh.Id` | cùng file, sau bước 7 |
| 9 | `GET {id}`: bỏ Include, load thủ công EntityId+Loai | `HoSoMoiThauDienTuGetQuery.cs` dòng **24–25** |

Luồng sau khi sửa:

```text
queryable (không Include NotMapped)
  → Select DTO hồ sơ + file GroupId = HoSo.Id
  → PaginatedListAsync
  → 1 query ToTrinhQuyetDinh WHERE EntityId IN (ids) AND Loai IN (2 constant)
  → foreach gán DTO từ dictionary (không query DB)
  → (optional) 1 query file legacy theo ToTrinhQuyetDinh.Id
  → return
```

---

## 6.1. Code — DTO list

**File:** `QLDA.Application/HoSoMoiThauDienTus/DTOs/HoSoMoiThauDienTuDto.cs`  
**Class:** `HoSoMoiThauDienTuDto`  
**Chỗ dán:** ngay sau `DanhSachTepDinhKem` (cuối class, trước `}`).

> Hiện file **đã có** 2 property này (dòng 23–24). Nếu còn thì **không thêm lần 2**.

```csharp
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public ToTrinhQuyetDinhDto? ToTrinh { get; set; }
    public ToTrinhQuyetDinhDto? QuyetDinh { get; set; }
}
```

JSON camelCase: `toTrinh`, `quyetDinh`. Không có dữ liệu → `null`.

---

## 6.2. Code — map `ToTrinhQuyetDinh` → `ToTrinhQuyetDinhDto`

**Không** để helper mơ hồ trong handler. Gắn extension vào file mapping sẵn có.

| | |
|--|--|
| **File** | `QLDA.Application/HoSoMoiThauDienTus/DTOs/HoSoMoiThauDienTuMappings.cs` |
| **Class** | `HoSoMoiThauDienTuMappings` (static, cùng file với `ToEntity` / `Update`) |
| **Chỗ dán** | **sau** method `Update(...)` (khoảng dòng 69), **trước** `}` đóng class (dòng 73) |
| **Tên method** | `ToDto` — extension trên entity `ToTrinhQuyetDinh` |
| **Gọi từ đâu** | `HoSoMoiThauDienTuGetDanhSachQueryHandler` khi gán `item.ToTrinh` / `item.QuyetDinh` (mục 6.3): `tt.ToDto()` |

Không tạo file mới. Không tạo `Application/Services`. Handler chỉ `using QLDA.Application.HoSoMoiThauDienTus.DTOs` (đã có).

**Code dán vào `HoSoMoiThauDienTuMappings`:**

```csharp
    public static void Update(this HoSoMoiThauDienTu entity, HoSoMoiThauDienTuUpdateModel dto)
    {
        // ... existing Update ...
    }

    /// <summary>
    /// Map 1 dòng ToTrinhQuyetDinh (load thủ công EntityId+Loai) sang DTO list/chi tiết.
    /// Dùng cho GET danh-sach — không Include [NotMapped].
    /// </summary>
    public static ToTrinhQuyetDinhDto ToDto(this ToTrinhQuyetDinh e) => new()
    {
        Id = e.Id,
        So = e.So,
        Ngay = e.Ngay,
        TrichYeu = e.TrichYeu,
        NguoiKy = e.NguoiKy,
        ChucVu = e.ChucVu,
    };
}
```

**Cách gọi trong handler danh sách** (sau dictionary, không query DB):

```csharp
item.ToTrinh = toTrinhByHoSoId.TryGetValue(item.Id, out var tt) ? tt.ToDto() : null;
item.QuyetDinh = quyetDinhByHoSoId.TryGetValue(item.Id, out var qd) ? qd.ToDto() : null;
```

`GET /{id}` **không** cần `ToDto` — gán thẳng `entity.ToTrinh` / `entity.QuyetDinh` (entity `[NotMapped]`) rồi Controller `ToModel`.

---

## 6.3. Code — `HoSoMoiThauDienTuGetDanhSachQueryHandler`

### Constructor — thêm repo

```csharp
private readonly IRepository<ToTrinhQuyetDinh, long> ToTrinhQuyetDinh;

public HoSoMoiThauDienTuGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
    HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
    TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
    ToTrinhQuyetDinh = serviceProvider.GetRequiredService<IRepository<ToTrinhQuyetDinh, long>>();
}
```

### Query hồ sơ — **bỏ 2 Include NotMapped**

Giữ Include navigation thật (`DuAn`, `Buoc`, …). Có thể bỏ luôn các Include này vì `.Select` đã project `Ten*` — không bắt buộc; tối thiểu **phải xóa** `ToTrinh`/`QuyetDinh`.

```csharp
var queryable = HoSoMoiThauDienTu.GetQueryableSet()
    .AsNoTracking()
    .Include(e => e.DuAn)
    .Include(e => e.Buoc)
    .Include(e => e.HinhThucLuaChonNhaThau)
    .Include(e => e.GoiThau)
    .Include(e => e.TrangThaiPheDuyet)
    // KHÔNG Include ToTrinh / QuyetDinh
    .WhereGlobalFilter(request, e => e.ThoiGianThucHien);

if (request.SearchDto.DuAnId.HasValue)
    queryable = queryable.Where(e => e.DuAnId == request.SearchDto.DuAnId);
if (request.SearchDto.LoaiDuAnTheoNamId > 0)
    queryable = queryable.Where(e => e.DuAn!.LoaiDuAnTheoNamId == request.SearchDto.LoaiDuAnTheoNamId);
if (request.SearchDto.GoiThauId.HasValue)
    queryable = queryable.Where(e => e.GoiThauId == request.SearchDto.GoiThauId);
```

### Select — **chỉ file theo HoSo.Id**, không đụng `e.ToTrinh`

```csharp
var groupTypesOnEntityId = AttachmentSubquery.ExpandGroupTypes(
    includeSigned: true,
    nameof(EGroupType.HoSoMoiThauDienTu),
    nameof(EGroupType.HoSoMoiThauDienTuToTrinh),
    nameof(EGroupType.HoSoMoiThauDienTuQuyetDinh),
    nameof(EGroupType.HoSoMoiThauDienTuQuyetDinhTD),
    nameof(EGroupType.HoSoMoiThauDienTuCamKetTD),
    nameof(EGroupType.HoSoMoiThauDienTuBaoCaoTD));

var result = await queryable
    .Select(e => new HoSoMoiThauDienTuDto {
        Id = e.Id,
        DuAnId = e.DuAnId,
        BuocId = e.BuocId,
        TenDuAn = e.DuAn!.TenDuAn,
        TenBuoc = e.Buoc!.TenBuoc,
        HinhThucLuaChonNhaThauId = e.HinhThucLuaChonNhaThauId,
        ThamDinh = e.ThamDinh ?? false,
        TenHinhThucLuaChonNhaThau = e.HinhThucLuaChonNhaThau!.Ten,
        GoiThauId = e.GoiThauId,
        TenGoiThau = e.GoiThau!.Ten,
        GiaTri = e.GiaTri,
        ThoiGianThucHien = e.ThoiGianThucHien,
        TrangThaiDangTai = e.TrangThaiDangTai,
        TrangThaiId = e.TrangThaiId,
        TenTrangThai = e.TrangThaiId == null
            ? TrangThaiPheDuyetCodes.Default.TenDuThao
            : e.TrangThaiPheDuyet!.Ten,
        DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
            .Where(i => i.GroupId == e.Id.ToString() && groupTypesOnEntityId.Contains(i.GroupType))
            .Select(i => i.ToDto())
            .ToList(),
        // ToTrinh / QuyetDinh gán sau — không map trong Select này
    })
    .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
```

### Batch `ToTrinhQuyetDinh` — **một query, không foreach-query**

| | |
|--|--|
| **File** | `QLDA.Application/HoSoMoiThauDienTus/Queries/HoSoMoiThauDienTuGetDanhSachQuery.cs` |
| **Class** | `HoSoMoiThauDienTuGetDanhSachQueryHandler` |
| **Method** | `Handle(...)` |
| **Chỗ dán 2 dòng `item.ToTrinh` / `item.QuyetDinh`** | **cuối `Handle`**, sau `PaginatedListAsync` và sau khi build 2 dictionary — **trong** `foreach (var item in result.Data)` |

**Không** dán vào `HoSoMoiThauDienTuMappings.cs` (file đó chỉ có `tt.ToDto()`).  
**Không** dán vào `HoSoMoiThauDienTuGetQuery.cs` (`GET /{id}` gán `entity.ToTrinh = await ...FirstOrDefaultAsync`, không dùng dictionary).

```csharp
var ids = result.Data.Select(x => x.Id).ToList();
if (ids.Count == 0)
    return result;

var loaiToTrinh = ToTrinhQuyetDinhLoai.HoSoMoiThauToTrinh;       // "HoSoMoiThauToTrinh"
var loaiQuyetDinh = ToTrinhQuyetDinhLoai.HoSoMoiThauQuyetDinh;   // "HoSoMoiThauQuyetDinh"

var vanBan = await ToTrinhQuyetDinh.GetQueryableSet()
    .AsNoTracking()
    .Where(e => e.EntityId != null
        && ids.Contains(e.EntityId.Value)
        && (e.Loai == loaiToTrinh || e.Loai == loaiQuyetDinh))
    .ToListAsync(cancellationToken);

var toTrinhByHoSoId = vanBan
    .Where(e => e.Loai == loaiToTrinh && e.EntityId.HasValue)
    .GroupBy(e => e.EntityId!.Value)
    .ToDictionary(g => g.Key, g => g.First());

var quyetDinhByHoSoId = vanBan
    .Where(e => e.Loai == loaiQuyetDinh && e.EntityId.HasValue)
    .GroupBy(e => e.EntityId!.Value)
    .ToDictionary(g => g.Key, g => g.First());

foreach (var item in result.Data) {
    item.ToTrinh = toTrinhByHoSoId.TryGetValue(item.Id, out var tt) ? tt.ToDto() : null;
    item.QuyetDinh = quyetDinhByHoSoId.TryGetValue(item.Id, out var qd) ? qd.ToDto() : null;
}
```

`foreach` ở đây **chỉ gán memory**, không `await` repo.

### File legacy (GroupId = `ToTrinhQuyetDinh.Id` long)

Dữ liệu mới đã nằm `GroupId = HoSo.Id` (đã lấy trong Select). Bản ghi cũ có thể gắn file theo Id tờ trình/QĐ. Một query thêm, rồi append — **không** query từng item.

```csharp
var legacyGroupIds = vanBan.Select(x => x.Id.ToString()).ToList();
var groupTypesToTrinh = AttachmentSubquery.ExpandGroupTypes(
    includeSigned: true, nameof(EGroupType.HoSoMoiThauDienTuToTrinh));
var groupTypesQuyetDinh = AttachmentSubquery.ExpandGroupTypes(
    includeSigned: true, nameof(EGroupType.HoSoMoiThauDienTuQuyetDinh));

var legacyFiles = legacyGroupIds.Count == 0
    ? []
    : await TepDinhKem.GetQueryableSet().AsNoTracking()
        .Where(i => legacyGroupIds.Contains(i.GroupId)
            && (groupTypesToTrinh.Contains(i.GroupType) || groupTypesQuyetDinh.Contains(i.GroupType)))
        .Select(i => i.ToDto())
        .ToListAsync(cancellationToken);

var filesByGroupId = legacyFiles
    .Where(f => f.GroupId != null)
    .GroupBy(f => f.GroupId!)
    .ToDictionary(g => g.Key, g => g.ToList());

foreach (var item in result.Data) {
    void Append(long? id) {
        if (id is { } x && filesByGroupId.TryGetValue(x.ToString(), out var extra))
            item.DanhSachTepDinhKem = (item.DanhSachTepDinhKem ?? [])
                .Concat(extra).DistinctBy(f => f.Id).ToList();
    }
    Append(item.ToTrinh?.Id);
    Append(item.QuyetDinh?.Id);
}

return result;
```

---

## 6.4. Code — `HoSoMoiThauDienTuGetQuery` (`GET /{id}`)

Cùng bug Include. Controller `Get` dùng `entity.ToTrinh` / `entity.QuyetDinh` để load file + `ToModel` — phải gán 2 property `[NotMapped]` **sau** khi load hồ sơ.

```csharp
internal class HoSoMoiThauDienTuGetQueryHandler : IRequestHandler<HoSoMoiThauDienTuGetQuery, HoSoMoiThauDienTu> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IRepository<ToTrinhQuyetDinh, long> ToTrinhQuyetDinh;

    public HoSoMoiThauDienTuGetQueryHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        ToTrinhQuyetDinh = serviceProvider.GetRequiredService<IRepository<ToTrinhQuyetDinh, long>>();
    }

    public async Task<HoSoMoiThauDienTu> Handle(HoSoMoiThauDienTuGetQuery request,
        CancellationToken cancellationToken = default) {
        var entity = await HoSoMoiThauDienTu.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.DuAn)
            .Include(e => e.Buoc)
            .Include(e => e.HinhThucLuaChonNhaThau)
            .Include(e => e.GoiThau)
            .Include(e => e.TrangThaiPheDuyet)
            // KHÔNG Include ToTrinh / QuyetDinh
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity!.ToTrinh = await ToTrinhQuyetDinh.GetQueryableSet()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.EntityId == entity.Id && x.Loai == ToTrinhQuyetDinhLoai.HoSoMoiThauToTrinh,
                cancellationToken);
        entity.QuyetDinh = await ToTrinhQuyetDinh.GetQueryableSet()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.EntityId == entity.Id && x.Loai == ToTrinhQuyetDinhLoai.HoSoMoiThauQuyetDinh,
                cancellationToken);

        return entity;
    }
}
```

Đây là **1 hồ sơ** → 2 `FirstOrDefault` (giống `UpdateCommand`). Không N+1 list.

Cần `using QLDA.Domain.Constants;`.

---

## 6.5. Không viết như thế này

```csharp
// SAI — Include NotMapped (bug hiện tại)
.Include(e => e.ToTrinh)
.Include(e => e.QuyetDinh)

// SAI — NotMapped trong Select SQL
e.ToTrinh != null && i.GroupId == e.ToTrinh.Id.ToString()

// SAI — N+1
foreach (var item in result.Data) {
    item.ToTrinh = await repo.FirstOrDefaultAsync(...); // cấm
}

// SAI — bỏ [NotMapped] / thêm HasOne để Include chạy lại
// SAI — Loai tự bịa enum/số; phải dùng ToTrinhQuyetDinhLoai.HoSoMoiThauToTrinh / HoSoMoiThauQuyetDinh
```

Controller `GetAll` **không đổi** — vẫn `Mediator.Send(new HoSoMoiThauDienTuGetDanhSachQuery(dto))`.

---

## 7. Test sau khi code

```http
GET /api/ho-so-moi-thau-dien-tu/danh-sach?pageIndex=1&pageSize=10&duAnId=<guid>
```

| # | Case | Kỳ vọng |
|---|------|---------|
| 1 | Request đang 400 | Không còn 400 / không `InvalidOperationException` |
| 2 | `pageIndex`/`pageSize` | Pagination đúng |
| 3 | `duAnId` | Chỉ hồ sơ dự án đó |
| 4 | Có dòng `Loai=HoSoMoiThauToTrinh` | `toTrinh` có So/Ngay/… |
| 5 | Có dòng `Loai=HoSoMoiThauQuyetDinh` | `quyetDinh` có dữ liệu |
| 6 | Không có dòng liên kết | `toTrinh`/`quyetDinh` = `null` |
| 7 | Không exception | 200, `result: true` |
| 8 | N+1 | 1 query list + 1 query `ToTrinhQuyetDinh` (+ 1 file), không foreach-query |
| 9 | `dotnet build SER.sln` | 0 error |

---

## 8. Cần xác nhận trước khi code

1. Sửa luôn `HoSoMoiThauDienTuGetQuery` (`GET /{id}`) vì cùng Include NotMapped — **đề xuất có**.
2. List DTO thêm `toTrinh` / `quyetDinh` (`ToTrinhQuyetDinhDto`) — **đề xuất có**. Shape list đổi: thêm 2 field, không xóa field cũ.
3. Hydrate file legacy theo `ToTrinhQuyetDinh.Id` (mục 6.3 cuối) — **đề xuất có** để không sót file bản ghi cũ; file mới đã lấy theo `HoSo.Id`.
