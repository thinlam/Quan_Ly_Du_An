# Refactor: `GoiThauGetTinhHinhDauThauPrintQuery` — truyền thẳng SearchDto

> **Trạng thái:** Implemented  
> **Liên quan:** Issue #103, branch `refactor/tinh-hinh-dau-thau-print-query`

---

## Vấn đề hiện tại

Luồng export tình hình đấu thầu đang **lệch convention** so với các print/export khác trong project:


| Export khác (chuẩn)                                                          | Tình hình đấu thầu (hiện tại)                                                              |
| ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| Search DTO ở **Application** (`DuAnPrintSearchDto`, `GoiThauPrintSearchDto`) | Search model ở **WebApi** (`TinhHinhThucHienDauThauPrintSearchModel`)                      |
| Controller bind `[FromQuery] XxxPrintSearchDto`                              | Controller bind WebApi model                                                               |
| `Mediator.Send(new XxxQuery(searchDto))`                                     | `Mediator.Send(XxxQuery.Create(searchModel.Loai))` — chỉ truyền `Loai`, bỏ `HiddenColumns` |
| Validate trong **Handler** bằng `ManagedException.ThrowIf`                   | Factory `Create(int? loai)` validate + branch `if (loai is null)`                          |


Factory `Create()` trên query record là lớp trung gian không cần thiết; validate null/0 bằng `if` riêng cũng không cần khi đã có `ThrowIf` và enum `TatCa = 0`.

---

## Mục tiêu

1. **Truyền thẳng search DTO vào query** — giống `DuAnGetDanhSachExportQuery(DuAnPrintSearchDto SearchDto)`.
2. **Xóa `GoiThauGetTinhHinhDauThauPrintQuery.Create()`** — không factory trên query record.
3. **Validate bằng `ManagedException.ThrowIf` trong Handler** — không `if (loai is null)` riêng trên query/controller.
4. **Search DTO thuộc Application layer** — xóa model trùng ở WebApi (theo rule DTO trong Application).

**Không đổi hành vi API:**


| Request `loai`              | Kết quả                                  |
| --------------------------- | ---------------------------------------- |
| không truyền / `null` / `0` | 3 sheet (Chưa có KQ / Có KQ / Đã lên HĐ) |
| `1` / `2` / `3`             | 1 sheet tương ứng                        |
| giá trị khác (`4`, `-1`, …) | `400` + message lỗi                      |


---

## Thiết kế đề xuất

### 1. Thêm Application Search DTO

**File mới:** `QLDA.Application/GoiThaus/DTOs/TinhHinhThucHienDauThauPrintSearchDto.cs`

```csharp
namespace QLDA.Application.GoiThaus.DTOs;

/// <summary>
/// Search DTO cho print/export báo cáo tình hình thực hiện đấu thầu — không phân trang
/// </summary>
public record TinhHinhThucHienDauThauPrintSearchDto
{
    /// <summary>
    /// Tab loại (<see cref="QLDA.Domain.Enums.TinhHinhThucHienDauThauLoai"/>).
    /// Null hoặc 0 (TatCa) = xuất 3 sheet.
    /// </summary>
    public int? Loai { get; set; }

    public List<string>? HiddenColumns { get; set; }
}
```

> `HiddenColumns` giữ trên DTO để controller đọc trực tiếp sau khi gọi Mediator (handler **không** cần dùng field này).

---

### 2. Đổi signature Query

**Trước:**

```csharp
public record GoiThauGetTinhHinhDauThauPrintQuery : IRequest<...>
{
    public TinhHinhThucHienDauThauLoai Loai { get; init; } = TatCa;
    public static GoiThauGetTinhHinhDauThauPrintQuery Create(int? loai) { ... }
}
```

**Sau:**

```csharp
public record GoiThauGetTinhHinhDauThauPrintQuery(
    TinhHinhThucHienDauThauPrintSearchDto SearchDto)
    : IRequest<TinhHinhThucHienDauThauPrintResultDto>;
```

Không property `Loai` typed enum trên query — parse enum **một lần** trong Handler từ `SearchDto.Loai`.

---

### 3. Validate trong Handler (ThrowIf, không branch null)

**Nguyên tắc:** `TatCa = 0` → `null` từ query string coalesce về `0`, không cần `if (loai is null)`.

```csharp
public async Task<TinhHinhThucHienDauThauPrintResultDto> Handle(
    GoiThauGetTinhHinhDauThauPrintQuery request,
    CancellationToken cancellationToken = default)
{
    var loaiValue = request.SearchDto.Loai ?? 0;

    ManagedException.ThrowIf(
        !Enum.IsDefined(typeof(TinhHinhThucHienDauThauLoai), loaiValue),
        "Loại tab không hợp lệ. Chỉ chấp nhận giá trị 1 (Chưa có kết quả), 2 (Có kết quả), 3 (Đã lên hợp đồng), hoặc bỏ trống để xuất cả 3 tab.");

    var loai = (TinhHinhThucHienDauThauLoai)loaiValue;

    if (loai is TinhHinhThucHienDauThauLoai.TatCa)
    {
        // ... 3 sheet (giữ nguyên logic hiện tại)
    }

    // ... 1 sheet
}
```

**Vì sao không cần check null riêng:**

- `SearchDto.Loai ?? 0` → `TatCa` khi FE không truyền hoặc truyền `0`.
- `Enum.IsDefined` chấp nhận `0, 1, 2, 3` — hợp lệ.
- Chỉ **một** điểm validate; message lỗi tập trung ở Handler.

**Không** dùng `throw new ManagedException(...)` trong `GetExportItemsAsync` switch `_` — case đó không còn tới được sau khi Handler đã validate (có thể giữ defensive hoặc bỏ tùy preference; khuyến nghị bỏ `_ => throw` vì dead code).

---

### 4. Controller — bind Application DTO, gọi thẳng Mediator

**Trước:**

```csharp
public async Task<IActionResult> InTinhHinhThucHienDauThau(
    [FromQuery] TinhHinhThucHienDauThauPrintSearchModel searchModel, ...)
{
    var result = await Mediator.Send(
        GoiThauGetTinhHinhDauThauPrintQuery.Create(searchModel.Loai),
        cancellationToken);

    var hiddenColumns = searchModel.HiddenColumns ?? [];
    ...
}
```

**Sau:**

```csharp
using QLDA.Application.GoiThaus.DTOs;

public async Task<IActionResult> InTinhHinhThucHienDauThau(
    [FromQuery] TinhHinhThucHienDauThauPrintSearchDto searchDto,
    CancellationToken cancellationToken = default)
{
    ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
    ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

    var result = await Mediator.Send(
        new GoiThauGetTinhHinhDauThauPrintQuery(searchDto),
        cancellationToken);

    var hiddenColumns = searchDto.HiddenColumns ?? [];
    ...
}
```

Controller **chỉ** lo HTTP (template path, auth, export Excel) — không validate `Loai`.

---

### 5. Xóa WebApi model trùng

**Xóa:** `QLDA.WebApi/Models/GoiThaus/TinhHinhThucHienDauThauPrintSearchModel.cs`

Binding query string vẫn hoạt động vì property name giữ nguyên (`Loai`, `HiddenColumns`).

---

## So sánh với pattern tham chiếu

```csharp
// DuAn — chuẩn project
[HttpGet("...")]
public async Task<IActionResult> InDuAn([FromQuery] DuAnPrintSearchDto searchDto)
{
    var data = await Mediator.Send(new DuAnGetDanhSachExportQuery(searchDto));
    ...
}
```

Sau refactor, tình hình đấu thầu follow cùng pattern:

```
HTTP query string
    → TinhHinhThucHienDauThauPrintSearchDto (Application)
    → GoiThauGetTinhHinhDauThauPrintQuery(SearchDto)
    → Handler: ThrowIf + nghiệp vụ
    → Controller: Export Excel + HiddenColumns
```

---

## Files cần sửa


| File                                                                       | Thao tác                                              |
| -------------------------------------------------------------------------- | ----------------------------------------------------- |
| `QLDA.Application/GoiThaus/DTOs/TinhHinhThucHienDauThauPrintSearchDto.cs`  | **Thêm**                                              |
| `QLDA.Application/GoiThaus/Queries/GoiThauGetTinhHinhDauThauPrintQuery.cs` | Sửa signature, xóa `Create()`, validate trong Handler |
| `QLDA.WebApi/Controllers/PrintController.cs`                               | Bind Application DTO, `new Query(searchDto)`          |
| `QLDA.WebApi/Models/GoiThaus/TinhHinhThucHienDauThauPrintSearchModel.cs`   | **Xóa**                                               |
| `docs/feature/GoiThau/task-export-tinh-hinh-thuc-hien-dau-thau.md`         | Cập nhật mục as-built (optional, sau implement)       |


**Không đụng:** Domain enum, template Excel, integration tests (URL/params giữ nguyên).

---

## Checklist implement

- [ ] Tạo `TinhHinhThucHienDauThauPrintSearchDto` trong Application
- [ ] Đổi query sang primary constructor `(SearchDto)`
- [ ] Xóa `Create(int? loai)` và property `Loai` enum trên query
- [ ] Handler: `Loai ?? 0` + `ManagedException.ThrowIf(!Enum.IsDefined(...))`
- [ ] PrintController: `[FromQuery] TinhHinhThucHienDauThauPrintSearchDto`
- [ ] Xóa WebApi `TinhHinhThucHienDauThauPrintSearchModel`
- [ ] Chạy `TinhHinhThucHienDauThauExportTests` (6 cases: loai 1/2/3, no loai, invalid loai)

---

## Commit gợi ý

Một commit gọn trên branch refactor:

```
refactor(export): truyền SearchDto trực tiếp vào print query tình hình đấu thầu
```

Hoặc tách nếu PR đang gom nhiều thứ — commit này chỉ Application + PrintController + xóa WebApi model.