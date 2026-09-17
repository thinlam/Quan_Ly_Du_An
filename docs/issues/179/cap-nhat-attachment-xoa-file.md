# Bug: Cập nhật Tờ trình thẩm định nhà thầu không xóa attachment khi `File = []`

> Follow-up của Issue #179 (`PUT api/to-trinh-tham-dinh-nha-thau/cap-nhat`).
> Phát hiện khi test acceptance cho phần cập nhật child objects.

## 1. Mô tả

API `PUT api/to-trinh-tham-dinh-nha-thau/cap-nhat`: khi FE gửi `"file": []`
(người dùng chủ động xóa hết file của một mục), các file cũ trong DB **vẫn còn**,
không bị xóa.

## 2. Tái hiện (thực tế)

1. `them-moi` tạo tờ trình có `quyetDinhPheDuyet.file = [file1]`.
2. `cap-nhat` gửi `quyetDinhPheDuyet.file = []`.
3. `GET /api/to-trinh-tham-dinh-nha-thau/{id}/chi-tiet` → `quyetDinhPheDuyet.file`
   **vẫn trả về file1**:

```json
{
  "quyetDinhPheDuyet": {
    "so": "6.QUYET-DINH-DUYET-KET-QUA",
    "ngayKy": "2026-09-10",
    "file": [
      {
        "id": "475b9dc6-84e1-43f6-b583-35b57fa9e786",
        "fileName": "Qui trinh QLDA (tach theo loại DA) 3 (1)_100920261059042.docx",
        "groupType": "ToTrinhThamDinhNhaThau_QuyetDinh",
        "groupId": "08df0eef-9e1e-a9eb-687a-7b223406e766"
      }
    ]
  }
}
```

→ File `.docx` không bị xóa dù FE gửi `[]`. (Data fields như `so`/`nguoiKy`/`ngayKy`/`chucVuId` thì vẫn update đúng — phần fix data trước đã OK, chỉ còn bug attachment.)

## 3. Root cause

`ToTrinhThamDinhNhaThauController.Update` dùng điều kiện `File is { Count: > 0 }`
cho **tất cả các nhóm attachment**:

```csharp
if (dto.QuyetDinhPheDuyet?.File is { Count: > 0 } fileQuyetDinh)
{
    await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
    {
        GroupId = vanBanQuyetDinhId.ToString(),
        GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau_QuyetDinh)],
        Entities = [.. fileQuyetDinh.ToEntities(...)],
        AutoDeleteMissing = true
    }, cancellationToken);
}
```

Khi `File = []`:
- `Count > 0` = **false** → cả block bị skip.
- `AttachmentBulkInsertOrUpdateCommand` **không được gọi** → `AutoDeleteMissing = true`
  không chạy → file cũ không bị xóa.

## 4. Các nhóm attachment cùng bug (9/9 trong `Update`)

| # | Nhóm | GroupType | Dòng (`Controller.cs`) |
|---|---|---|---|
| 1 | `DanhSachTepDinhKem` (legacy) | `ToTrinhThamDinhNhaThau` | 220 |
| 2 | `DanhSachTepThamDinh` (legacy) | `NoiDungToTrinhThamDinhNhaThau` | 230 |
| 3 | `ThongTinNhaThau.FileEHSDT` | `ToTrinhThamDinhNhaThau_FileEHSDT` | 242 |
| 4 | `ThongTinNhaThau.FileDanhGia` | `ToTrinhThamDinhNhaThau_FileDanhGia` | 252 |
| 5 | `DoiChieu.File` | `ToTrinhThamDinhNhaThau_DoiChieu` | 265 |
| 6 | `ThuongThao.File` | `ToTrinhThamDinhNhaThau_ThuongThao` | 278 |
| 7 | `ThamDinh.File` | `ToTrinhThamDinhNhaThau_ThamDinh` | 291 |
| 8 | `ToTrinhKetQua.File` | `ToTrinhQuyetDinh` | 305 |
| 9 | `QuyetDinhPheDuyet.File` | `ToTrinhThamDinhNhaThau_QuyetDinh` | 330 |

## 5. Quy tắc attachment cần support

| FE gửi | Hành vi mong muốn |
|---|---|
| `null` (không gửi field) | **Không cập nhật** attachment → file cũ giữ nguyên |
| `[]` | User chủ động xóa hết → gọi `AttachmentBulkInsertOrUpdateCommand` với `Entities = []` + `AutoDeleteMissing = true` → **xóa hết file cũ** của nhóm đó |
| `[file1, file2]` | Insert/update list mới + `AutoDeleteMissing = true` → xóa file cũ không còn trong list |

→ **Không được dùng `is { Count: > 0 }`** vì nó không phân biệt được `null` và `[]`.
Chỉ skip khi `File == null`.

## 6. Hướng sửa

Chỉ sửa **1 file**: `QLDA.WebApi/Controllers/ToTrinhThamDinhNhaThauController.cs` (method `Update`).

Đổi **9 điều kiện** từ `is { Count: > 0 }` → `is { }` (skip khi `null`, chạy khi `[]`/non-empty):

```csharp
// null → skip (file cũ giữ nguyên)
// []    → chạy Entities=[] + AutoDeleteMissing=true → xóa hết file nhóm
// [f1,f2] → sync list mới, xóa file cũ ngoài list
if (dto.QuyetDinhPheDuyet?.File is { } fileQuyetDinh) { ... }
```

Riêng 2 block Tờ trình kết quả / Quyết định phê duyệt:
- **Giữ nguyên** điều kiện `result.ToTrinhQuyetDinhId is { }` / `result.VanBanQuyetDinhId is { }`
  (chỉ xử lý khi child object tồn tại).
- Chỉ đổi phần `File is { Count: > 0 }` → `File is { }`.

**Không đụng**: `them-moi` (create — không có file cũ để xóa, `Count > 0` là an toàn),
`GET chi-tiet`, command, entity, DTO, migration (không cần migration).

## 7. Kế hoạch test

Với **từng nhóm** trong 9 nhóm ở mục 4, test đủ 3 case:

| Case | Input `File` | Kỳ vọng |
|---|---|---|
| A | `null` (bỏ field) | File DB của nhóm đó giữ nguyên |
| B | `[]` | File DB của nhóm đó bị xóa hết (soft-delete, `IsDeleted = true`) |
| C | `[file mới]` | DB đồng bộ theo list mới; file cũ ngoài list bị xóa |

Test thực tế (case đang tái hiện — nhóm 9 `QuyetDinhPheDuyet`):

1. Tạo tờ trình có `quyetDinhPheDuyet.file = [file1]`.
2. `PUT cap-nhat` với `quyetDinhPheDuyet.file = []`.
3. `GET chi-tiet` → `quyetDinhPheDuyet.file` phải **rỗng**.
4. Query DB: `Attachment` (`GroupId` = id tờ trình, `GroupType = 'ToTrinhThamDinhNhaThau_QuyetDinh'`) → `IsDeleted = true` (hoặc không còn trong `GetAttachmentsQuery`).

## 8. Trạng thái

- ~~Bug xác nhận còn tồn tại (chưa fix)~~ → **ĐÃ FIX 2026-09-10**.
- Phạm vi đã áp: **đồng bộ cả 9 nhóm** (gồm 2 nhóm legacy `DanhSachTepDinhKem`/`DanhSachTepThamDinh`).
- File sửa: `QLDA.WebApi/Controllers/ToTrinhThamDinhNhaThauController.cs` (method `Update`, 9 block).
- `dotnet build QLDA.WebApi` → 0 warning / 0 error.
- `them-moi` giữ nguyên `Count > 0` (create — không có file cũ, an toàn).
- Chờ test lại 3 case (mục 7) sau khi restart app.