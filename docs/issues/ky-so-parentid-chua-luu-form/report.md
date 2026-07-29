# Issue — Ký số báo “Không tìm thấy tệp cha (ParentId)” khi file mới chưa lưu form

> Ngày ghi nhận / sửa: 2026-07-24  
> Trạng thái: ✅ Đã sửa (BE) — FE còn nên đồng bộ payload metadata  
> Phạm vi BE: `NoiDungDaKyCommand` + filter list nội dung đã ký  
> Liên quan: `docs/feature/KySo/task-9460-noi-dung-da-ky.md`

---

## Triệu chứng (trước khi sửa)

API:

```http
POST /api/quan-ly-ky-so/ky-so
```

Lỗi:

> Không tìm thấy tệp cha (ParentId)

Tái hiện:

1. Vào màn hình **cập nhật** form (ví dụ Tờ trình có thẩm định).
2. Thêm một file mới (upload lên storage / giữ trên FE).
3. **Chưa** nhấn Cập nhật / Lưu form.
4. Gọi ký số ngay trên file vừa thêm.

Kết quả (cũ): `parentId` gửi lên **không có bản ghi** tương ứng trong bảng tệp đính kèm (`Attachment` / `TepDinhKem`) → API throw.

---

## Kết luận ngắn

| Câu hỏi | Kết luận |
|---------|----------|
| API ký số có bắt buộc `parentId` tồn tại trong DB không? | **Không còn.** Parent thiếu / không có → vẫn insert file ký số. |
| Nghiệp vụ đã chốt | **Cho phép ký số khi chưa có file gốc trong DB** (và khi không gửi `parentId`). |
| Payload `type` / `fileName` / `path` / `groupName` lệch? | **Sai phía FE** — BE không validate đồng bộ; `groupName` không tồn tại trên model BE. |
| BE đã sửa? | **Có** — xem §8. |

**Root cause đã xử lý (BE):** bỏ bắt buộc lookup parent; luôn lưu entity như file ký số (`GroupType` chứa `KySo`). Nếu `ParentId` trỏ tới bản ghi chưa có → set `ParentId = null` rồi insert.

---

## 1. Luồng nghiệp vụ

### 1.1 Lưu file gốc trên form (ví dụ Tờ trình có thẩm định)

`PUT /api/to-trinh-co-tham-dinh/cap-nhat`:

```
Controller.Update
  → ToTrinhCoThamDinhUpdateCommand          // chỉ cập nhật entity tờ trình
  → AttachmentBulkInsertOrUpdateCommand     // mới ghi Attachment vào DB
       GroupType = QuyetDinhKeHoachThue / QuyetDinhKeHoachThueThamDinh
```

File mới chỉ có bản ghi DB **sau** khi gọi cập nhật form. Trước đó file thường chỉ có trên FE + path storage (nếu đã upload binary).

File tham chiếu:

- `QLDA.WebApi/Controllers/ToTrinhCoThamDinhController.cs`
- `BuildingBlocks.../AttachmentBulkInsertOrUpdateCommand.cs`

### 1.2 API ký số

`POST /api/quan-ly-ky-so/ky-so`:

```
QuanLyKySoController.Create(KySoModel)
  → DanhSachTepDinhKem.ToEntities(GroupId, EGroupType.KySo)
  → NoiDungDaKyCommand { GroupId, Entities }
```

File tham chiếu:

- `QLDA.WebApi/Controllers/QuanLyKySoController.cs`
- `QLDA.Application/KySos/Commands/NoiDungDaKyCommand.cs`
- Spec: `docs/feature/KySo/task-9460-noi-dung-da-ky.md`

### 1.3 Hành vi cũ (đã bỏ) — chỗ throw

```csharp
// TRƯỚC — NoiDungDaKyCommand
foreach (var entity in request.Entities.Where(e => e.ParentId != null)) {
    entity.GroupId = request.GroupId;

    var parent = await _repository.GetQueryableSet()
        .FirstOrDefaultAsync(e => e.Id == entity.ParentId, cancellationToken);
    ManagedException.ThrowIfNull(parent, "Không tìm thấy tệp cha (ParentId)"); // ← lỗi cũ

    toInsert.Add(entity);
}
```

Ý nghĩa cũ:

- Chỉ xử lý item có `ParentId != null`.
- **Bắt buộc** tìm được parent theo `Id` trong DB.
- Với bước tái hiện (file gốc chưa lưu form), `ParentId` FE gửi không có row → luôn fail.

---

## 2. Các điểm sai / rủi ro còn lại (FE)

### Sai #1 — FE gọi ký số khi file gốc chưa có trong DB *(đã xử lý phía BE)*

| | |
|--|--|
| **Ở đâu** | FE: màn cập nhật form → ký số trên file vừa thêm, trước `cap-nhat`. |
| **Trước** | BE throw `"Không tìm thấy tệp cha (ParentId)"`. |
| **Sau (BE)** | Vẫn insert file ký; `ParentId` không tồn tại → set `null`. |

### Sai #2 — Payload metadata không đồng nhất (`type` / `fileName` / `path`) — *còn FE*

| Field | Giá trị quan sát | Vấn đề |
|-------|------------------|--------|
| `type` | MIME Word | Không khớp đuôi `.pdf` của `fileName` |
| `fileName` | `....pdf` | Gợi ý file đã convert PDF để ký |
| `path` | `....docx` | Vẫn trỏ file Word gốc trên storage |
| `groupName` | `""` | **Không có field** trên model BE — FE gửi thừa |

**Cách sửa (FE):**

1. Binary thực sự dùng để ký (thường PDF sau convert).
2. Đồng bộ `path` / `fileName` / `type` = PDF.
3. Bỏ `groupName`. Dùng `groupId` + `groupType` đúng convention.

### Sai #3 — `GroupType` ký số lệch convention `KySo_<BaseType>` — *còn FE / optional BE*

| | |
|--|--|
| **Rủi ro** | `ToEntities(..., EGroupType.KySo)` + `ParentId != null` có thể ra `KySo_KySo` nếu FE không gửi base type đúng. |
| **BE hiện tại** | `EnsureSignedGroupType`: trống → `KySo`; chưa có `KySo` → `WithSignedVariant(base)`. |
| **Khuyến nghị FE** | Gửi `groupType = KySo_<base>` (vd. `KySo_QuyetDinhKeHoachThue`). |

---

## 3. Quyết định nghiệp vụ (đã chốt)

**Chọn hướng B (rút gọn):** cho phép ký khi chưa có parent trong DB — **không** ép lưu form trước.

Không làm full B1/B2 (insert cặp parent+child cùng request). Chỉ:

- Parent thiếu / không gửi → vẫn insert file ký số.
- `ParentId` không tồn tại → `ParentId = null`.
- Luôn đảm bảo `GroupType` chứa `KySo`.

---

## 4. Checklist payload đúng (khi đã có parent trong DB)

```json
{
  "groupId": "{TO_TRINH_ID}",
  "danhSachTepDinhKem": [
    {
      "id": null,
      "parentId": "{PARENT_ID}",
      "groupId": "{TO_TRINH_ID}",
      "groupType": "KySo_QuyetDinhKeHoachThue",
      "type": "application/pdf",
      "fileName": "ten-file-da-ky.pdf",
      "originalName": "ten-file-da-ky.pdf",
      "path": "/path/to/ten-file-da-ky.pdf",
      "size": 12345
    }
  ]
}
```

Khi **chưa** có parent trong DB: vẫn gọi được API; `parentId` sẽ bị clear về `null` nếu không tìm thấy.

Không gửi: `groupName`; metadata Word trên item PDF đã ký.

---

## 5. File / symbol liên quan

| Layer | File | Vai trò |
|-------|------|---------|
| WebApi | `QLDA.WebApi/Controllers/QuanLyKySoController.cs` | Endpoint ký số |
| Application | `QLDA.Application/KySos/Commands/NoiDungDaKyCommand.cs` | Insert file ký (đã nới ParentId) |
| Application | `QLDA.Application/KySos/Queries/NoiDungDaKyQueryableExtensions.cs` | List nội dung đã ký — filter `GroupType.Contains("KySo")` |
| WebApi | `QLDA.WebApi/Models/TepDinhKems/TepDinhKemMappingConfigurations.cs` | `ToEntities` / `ResolveGroupType` |
| Docs | `docs/feature/KySo/task-9460-noi-dung-da-ky.md` | Spec gốc (parent bắt buộc — **đã lệch nghiệp vụ mới**) |

---

## 6. Code sau khi sửa

### `NoiDungDaKyCommand`

```csharp
foreach (var entity in request.Entities) {
    entity.GroupId = request.GroupId;
    EnsureSignedGroupType(entity);

    // ParentId có nhưng chưa có bản ghi cha → vẫn lưu file ký số.
    // ParentId null → cũng lưu, coi như file ký số độc lập.
    if (entity.ParentId is { } parentId) {
        var parentExists = await _repository.GetQueryableSet()
            .AnyAsync(e => e.Id == parentId, cancellationToken);
        if (!parentExists)
            entity.ParentId = null;
    }

    toInsert.Add(entity);
}
```

`EnsureSignedGroupType`:

- `GroupType` trống → `KySo`
- Đã chứa `"KySo"` → giữ
- Còn lại → `SignedGroupTypeHelper.WithSignedVariant(GroupType)`

### `NoiDungDaKyQueryableExtensions`

```csharp
// File ký số: GroupType chứa KySo (có hoặc không có ParentId).
var files = await query
    .Where(e => e.GroupType.Contains("KySo"))
    // ...
```

Đã bỏ điều kiện `ParentId != null` khi list nội dung đã ký.

---

## 7. Checklist trạng thái

- [x] Đã đọc luồng lưu tệp form + API ký số
- [x] Xác nhận BE **trước đây** bắt buộc ParentId tồn tại trong DB
- [x] Chốt nghiệp vụ: không có ParentId / parent chưa có → vẫn lưu như file ký số
- [x] Sửa `NoiDungDaKyCommand`
- [x] Sửa list nội dung đã ký (`NoiDungDaKyQueryableExtensions`)
- [ ] FE đồng bộ payload `type` / `fileName` / `path` + `groupType = KySo_<base>` (Sai #2, #3)
- [ ] Cập nhật / ghi chú lệch với spec `task-9460` (parent bắt buộc) khi có PR docs riêng
