# Báo cáo khảo sát — Issue 182: reset tiến độ `DuAnBuoc` khi cập nhật Dự án

> Trạng thái: **ĐÃ CODE.** Không đổi schema / không migration. `dotnet test --filter FullyQualifiedName~DuAnControllerTests.Update_` — 5 case T1–T5 pass.

**Cách sửa (tóm tắt):** chỉ bọc lời gọi `DuAnBuocCloneCommand` trong `DuAnController.Update`. Đọc `QuyTrinhId` cũ (AsNoTracking) **trước** `DuAnUpdateCommand`. Chỉ clone khi QT đổi **và** chưa có tiến độ trên `DuAnBuoc`. Chi tiết mục 6–7.

---

## 1. Root cause

`DuAnController.Update` (`PUT api/du-an/cap-nhat`) luôn gọi clone sau update:

```312:312:QLDA.WebApi/Controllers/DuAnController.cs
            await Mediator.Send(new DuAnBuocCloneCommand(entity), cancellationToken);
```

Không so sánh `QuyTrinhId` cũ/mới, không kiểm tra tiến độ đã nhập.

`DuAn.Update()` ghi đè `QuyTrinhId` trên entity tracked **trước khi** controller nhận entity trả về:

```65:65:QLDA.Application/DuAns/DTOs/DuAnMappings.cs
        entity.QuyTrinhId = dto.QuyTrinhId;//PHẢI CLONE LẠI BƯỚC
```

Nếu chỉ đọc `entity.QuyTrinhId` sau `DuAnUpdateCommand` thì **không còn giá trị cũ**. Phải đọc `QuyTrinhId` **trước** `UpdateCommand` (AsNoTracking / query riêng).

---

## 2. Flow hiện tại `DuAnBuocCloneCommand`

File: `QLDA.Application/DuAnBuocs/Commands/DuAnBuocCloneCommand.cs`.

Đây **không** phải clone thuần “copy rồi thôi”. Handler `Clone()`:

| `QuyTrinhId` trên `DuAn` (đã update) | Hành vi |
|---|---|
| `null` | `ExecuteDelete` toàn bộ `DuAnBuoc` của dự án |
| Có giá trị | Load `DanhMucBuoc` (`Used`, `!IsDeleted`, cùng `QuyTrinhId`) → sync theo `BuocId` |

Sync khi có quy trình:

1. Load `DuAnBuoc` hiện có; nếu trùng `BuocId` thì giữ 1 dòng, xóa duplicate.
2. Duyệt tree bước mới (`ToSteps().ToTreeList()`):
   - `BuocId` chưa có → **insert** `DuAnBuoc` mới (`TenBuoc`, `PartialView`, `Used`, `DuAnBuocManHinhs`).
   - `BuocId` đã có → **update** `TenBuoc`/`PartialView`/`Used` + sync màn hình; **không** copy tiến độ từ catalog (catalog không có tiến độ user).
3. `BuocId` không còn trong quy trình mới → **`ExecuteDelete`**.
4. Tính `NgayDuKienBatDau`/`NgayDuKienKetThuc` đang **TODO tắt**. Nhánh `else` gán **cả hai ngày dự kiến = null** trên node (kể cả bước trùng `BuocId`).

→ Gọi clone khi user đã nhập tiến độ: xóa bước không còn trong QT mới, thêm bước mới, **wipe ngày dự kiến** trên bước trùng. Đúng bug G-312.

Caller:

| Nơi | Khi nào | Giữ? |
|---|---|---|
| `DuAnController.Create` | Sau insert, trước `DuAnBuocMapPhongBanCommand` | **Không đụng** |
| `DuAnController.Update` | Unconditional sau `DuAnUpdateCommand` | **Chỗ sửa** |

Không caller khác (đã grep).

---

## 3. Field xác định Quy trình dự án

`DuAn.QuyTrinhId` (`int?`). Payload cập nhật: `DuAnUpdateModel.QuyTrinhId`.

Bước catalog: `DanhMucBuoc.QuyTrinhId`. Clone lọc `e.QuyTrinhId == request.DuAn.QuyTrinhId`.

So sánh: `oldQuyTrinhId` (DB trước update) vs `entity.QuyTrinhId` (sau `DuAnUpdateCommand`).

---

## 4. Field tiến độ trên `DuAnBuoc`

Entity: `QLDA.Domain/Entities/DuAnBuoc.cs`.

**Coi là tiến độ user đã nhập** (một bước thỏa ≥ 1 điều kiện → cả dự án “đã nhập tiến độ”):

| Property | Điều kiện “đã nhập” |
|---|---|
| `NgayDuKienBatDau` | `!= null` |
| `NgayDuKienKetThuc` | `!= null` |
| `NgayThucTeBatDau` | `!= null` |
| `NgayThucTeKetThuc` | `!= null` |
| `TrangThaiId` | `!= null` (trạng thái thực hiện bước) |
| `GhiChu` | không null/empty |
| `TrachNhiemThucHien` | không null/empty |
| `IsKetThuc` | `== true` |

**Không** tính:

- `PhongPhuTrachChinhId` — `DuAnBuocMapPhongBanCommand` gán khi **thêm mới**; nếu tính là tiến độ thì dự án mới vừa tạo sẽ luôn “đã nhập”, Case 3 không bao giờ clone khi đổi QT.
- `TenBuoc` / `PartialView` / `Used` / `DuAnBuocManHinhs` — metadata bước, không phải tiến độ user.

Task nêu `PhongBanPhuTrach` / `TrangThaiThucHien` — map source: `PhongPhuTrachChinhId` (loại khỏi check), `TrangThaiId`.

Query tiến độ: `DuAnBuoc` theo `DuAnId`, **không** `FilterVisible` — phải thấy mọi bước của dự án, không lọc quyền.

---

## 5. Trùng `BuocId` giữa hai quy trình

Clone đã match theo `BuocId` (giữ dòng cũ, thêm mới, xóa thiếu). **Không implement merge phức tạp** — spec chỉ yêu cầu 3 case: nếu đã có tiến độ thì **không gọi clone**. Bước trùng `BuocId` được bảo vệ vì không chạy sync/wipe.

---

## 6. Cách sửa (chọn hướng)

**Sửa tại chỗ quyết định có gọi clone hay không** — `DuAnController.Update` — không sửa `DuAnBuocCloneCommand`.

Lý do:

- Clone command đang đúng cho **thêm mới** và cho Case 3 (đổi QT + chưa có tiến độ). Sửa bên trong clone sẽ ảnh hưởng `them-moi`.
- Spec: không merge từng `BuocId`; chỉ bật/tắt lời gọi clone theo 3 case.
- Comment `//PHẢI CLONE LẠI BƯỚC` trong `DuAn.Update()` là ý định cũ (luôn clone) — **không** sửa mapping; chặn ở controller.

Không tạo Application Service / helper class mới. Helper = **private method trên controller**.

---

## 7. Chi tiết sửa — `DuAnController.cs`

### 7.1. Inject repository

Controller đang `DuAnController(IServiceProvider serviceProvider)`. Thêm 2 field (pattern `TemplateController`):

```csharp
private readonly IRepository<DuAn, Guid> _duAnRepo =
    serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo =
    serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
```

Cần `using Microsoft.EntityFrameworkCore` nếu file chưa có (`.AsNoTracking()`, `.AnyAsync()`).

### 7.2. Đọc `QuyTrinhId` cũ **trước** `DuAnUpdateCommand`

Trong `Update`, ngay sau `BeginTransaction`, **trước** `Mediator.Send(DuAnUpdateCommand)`:

```csharp
var oldQuyTrinhId = await _duAnRepo.GetQueryableSet()
    .AsNoTracking()
    .Where(e => e.Id == updateDto.Id)
    .Select(e => e.QuyTrinhId)
    .FirstOrDefaultAsync(cancellationToken);

var entity = await Mediator.Send(new DuAnUpdateCommand(updateDto), cancellationToken);

if (oldQuyTrinhId != entity.QuyTrinhId
    && !await HasDuAnBuocTienDoAsync(entity.Id, cancellationToken))
{
    await Mediator.Send(new DuAnBuocCloneCommand(entity), cancellationToken);
}
```

Xóa dòng unconditional hiện tại (khoảng dòng 312).

**Bắt buộc AsNoTracking + query trước update:** `DuAnUpdateCommand` load entity tracked rồi `entity.Update(dto)` gán `QuyTrinhId = dto.QuyTrinhId`. Cùng DbContext/transaction: nếu đọc lại tracked entity thì đã là giá trị mới.

So sánh `int?` bằng `!=` (null vs 5 = đổi QT; 5 vs 5 = không đổi).

Phần còn lại của `Update` (DuToan, KeHoachVon, file, `SaveChanges`, `Commit`) **giữ nguyên**.

### 7.3. Helper `HasDuAnBuocTienDoAsync`

Private trên controller. `true` nếu **ít nhất một** `DuAnBuoc` của dự án đã có tiến độ (mục 4).

```csharp
private async Task<bool> HasDuAnBuocTienDoAsync(Guid duAnId, CancellationToken cancellationToken)
{
    return await _duAnBuocRepo.GetQueryableSet(OnlyUsed: false)
        .AnyAsync(e =>
            e.DuAnId == duAnId && (
                e.NgayDuKienBatDau != null
                || e.NgayDuKienKetThuc != null
                || e.NgayThucTeBatDau != null
                || e.NgayThucTeKetThuc != null
                || e.TrangThaiId != null
                || e.IsKetThuc
                || (e.GhiChu != null && e.GhiChu != "")
                || (e.TrachNhiemThucHien != null && e.TrachNhiemThucHien != "")
            ), cancellationToken);
}
```

| Quy tắc | Lý do |
|---|---|
| `OnlyUsed: false` | Không bỏ bước `Used=false` nếu user đã nhập tiến độ |
| Không `FilterVisible` | Phải thấy mọi bước của dự án; đây không phải list API theo quyền |
| Không `PhongPhuTrachChinhId` | Tự gán lúc `them-moi` (`DuAnBuocMapPhongBanCommand`) |
| `IsKetThuc` bool | `true` = đã kết thúc bước = đã nhập |

Không `SaveChanges` trong helper — chỉ đọc.

### 7.4. Map 3 case → code

| Case | `oldQuyTrinhId != entity.QuyTrinhId` | `HasDuAnBuocTienDo` | Gọi clone? |
|---|---|---|---|
| 1 — không đổi QT | false | (bỏ qua, short-circuit) | Không |
| 2 — đổi QT + đã có tiến độ | true | true | Không |
| 3 — đổi QT + chưa tiến độ | true | false | Có — `DuAnBuocCloneCommand(entity)` như hiện tại |

Case 3 reuse nguyên command (insert bước mới / xóa bước cũ / sync metadata). An toàn vì chưa có tiến độ để mất.

### 7.5. `Create` (`them-moi`)

**Không sửa.** Vẫn:

1. `DuAnInsertCommand`
2. `DuAnBuocCloneCommand`
3. `DuAnBuocMapPhongBanCommand`

---

## 8. File đụng tới

| File | Việc |
|---|---|
| `QLDA.WebApi/Controllers/DuAnController.cs` | **Bắt buộc.** Inject 2 repo; đổi `Update`; thêm `HasDuAnBuocTienDoAsync`. |
| `QLDA.Tests/Integration/DuAnControllerTests.cs` | Nên thêm 5 case (`test-workflow.md`) nếu seed/fixture cho phép 2 `QuyTrinhId`. Test `Update_ExistingDuAn_ReturnsOk` hiện có **không** assert `DuAnBuoc` — không đủ để bắt regression. |

**Không sửa:**

- `DuAnBuocCloneCommand.cs` / handler
- `DuAnUpdateCommand.cs`, `DuAnMappings.Update`
- `DuAnBuocMapPhongBanCommand`
- Entity / EF configuration / migration / snapshot
- API contract (`DuAnUpdateModel` giữ `QuyTrinhId`)

---

## 9. Việc không làm

- Không đổi flow `them-moi`.
- Không clone khi không đổi QT.
- Không clone khi đã có tiến độ (kể cả đổi QT).
- Không merge từng bước / không sửa clone handler.
- Không Application Service / helper class ngoài private method trên controller.
- Không migration, không đổi schema, không đổi contract.

---

## 10. Rủi ro khi implement

- `GetQueryableSet(OnlyUsed: false)` — xác nhận signature repo đúng (BuildingBlocks). Nếu không có overload, dùng set không lọc `Used` tương đương.
- Helper chạy trong transaction của `Update` — `AsNoTracking` cho `oldQuyTrinhId` tránh dính tracked entity sau đó.
- Test SQLite: unique `DmQuyTrinh.MacDinh` có thể fail nếu seed 2 quy trình `MacDinh=true` (lọc unique SQL Server không áp SQLite). Seed QT thứ 2 với `MacDinh=false`, hoặc chỉ gán `DanhMucBuoc.QuyTrinhId` giả.

