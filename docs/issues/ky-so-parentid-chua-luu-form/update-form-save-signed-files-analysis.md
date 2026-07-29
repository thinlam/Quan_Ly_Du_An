# Phân tích source - lưu file ký số cùng API lưu/cập nhật form

> Ngày phân tích: 2026-07-28  
> Trạng thái: Chưa sửa code, chỉ phân tích source và chốt hướng thiết kế  
> Mục tiêu: Với form đã có `FormId`, khi user upload file mới rồi ký số ngay trên FE, FE không gọi `POST /api/quan-ly-ky-so/ky-so`; thay vào đó gửi cả file gốc + file signed trong `DanhSachTepDinhKem` của request lưu form, và BE phải lưu được cả hai.

---

## 1. Kết luận ngắn

Sau khi đọc source hiện tại, vấn đề không nằm ở riêng handler cập nhật của một form cụ thể, mà nằm ở **shared attachment mapping convention**:

- Luồng form `Create` và `Update` hiện đã dùng cùng pattern:
  - map `DanhSachTepDinhKem` -> `Attachment`
  - gọi `AttachmentBulkInsertOrUpdateCommand`
  - sync theo `GroupId + GroupTypes`
- Tuy nhiên, việc xác định một file có phải file ký số hay không hiện đang dựa chủ yếu vào:
  - `ParentId != null`
  - hoặc `GroupType` FE truyền vào đã là `KySo_*`
- Với case mới:
  - `Id = null`
  - `ParentId = null`
  - FE chỉ muốn gửi `KySo = true`
  - không muốn gọi API `quan-ly-ky-so/ky-so`

thì source hiện tại **chưa có chỗ nào đọc cờ `KySo`** để map file đó thành signed attachment.

Nói ngắn gọn:

- `Create` và `Update` form đang tái sử dụng đúng command sync chung.
- Điểm thiếu nằm ở **model/mapping shared**, không phải ở `SyncCollection`.
- Nếu không bổ sung nhận biết `KySo=true`, file signed mới sẽ bị lưu như file thường với `GroupType = <baseGroupType>`, không phải `KySo_<baseGroupType>`.

---

## 2. Source đã kiểm tra

### 2.1 API form create/update

Đã kiểm tra pattern trên controller form, ví dụ:

- `QLDA.WebApi/Controllers/DeXuatNhuCauKinhPhiController.cs`
- `QLDA.WebApi/Controllers/ToTrinhCoThamDinhController.cs`
- cùng nhiều controller khác đang dùng `AttachmentBulkInsertOrUpdateCommand`

Pattern hiện tại gần như giống nhau:

1. Insert hoặc update entity chính
2. Map `model.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.<NghiepVu>)`
3. Gọi:

```csharp
await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
    GroupId = entity.Id.ToString(),
    GroupTypes = [nameof(EGroupType.<NghiepVu>)],
    Entities = files,
    AutoDeleteMissing = true
});
```

### 2.2 Shared mapping/model của attachment

Đã kiểm tra:

- `QLDA.WebApi/Models/TepDinhKems/TepDinhKemModel.cs`
- `QLDA.WebApi/Models/TepDinhKems/TepDinhKemMappingConfigurations.cs`
- `QLDA.Application/TepDinhKems/DTOs/TepDinhKemDto.cs`
- `QLDA.Application/TepDinhKems/DTOs/TepDinhKemInsertDto.cs`
- `QLDA.Application/TepDinhKems/DTOs/TepDinhKemMappingConfiguration.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/DTOs/AttachmentDto.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/DTOs/AttachmentInsertOrUpdateModel.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/DTOs/AttachmentMapping.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/Common/AttachmentCollectionExtensions.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/Common/SignedGroupTypeHelper.cs`

### 2.3 Shared sync/load logic

Đã kiểm tra:

- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/Commands/AttachmentBulkInsertOrUpdateCommand.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Common/SyncHelper.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/Queries/GetAttachmentsQuery.cs`

### 2.4 API ký số riêng

Đã kiểm tra:

- `QLDA.WebApi/Controllers/QuanLyKySoController.cs`
- `QLDA.Application/KySos/Commands/NoiDungDaKyCommand.cs`

### 2.5 Docs/test liên quan

Đã kiểm tra:

- `docs/issues/ky-so-parentid-chua-luu-form/report.md`
- `docs/code-standards.md`
- `QLDA.Tests/Unit/TepDinhKemResolveGroupTypeTests.cs`
- `BuildingBlocks/tests/BuildingBlocks.Tests/Application/Attachments/Phase3AttachmentHelperTests.cs`

---

## 3. Handler thêm mới hiện đang lưu file signed như thế nào

## 3.1 Luồng form create

Ví dụ ở `DeXuatNhuCauKinhPhiController.Create`:

1. Insert entity form
2. Lấy `savedEntity.Id`
3. Map list file:

```csharp
model.DanhSachTepDinhKem?.ToEntities(savedEntity.Id, EGroupType.DeXuatNhuCauKinhPhi)
```

4. Gọi `AttachmentBulkInsertOrUpdateCommand`

Điểm quan trọng: bản thân controller create **không có logic riêng cho file signed**. Nó phụ thuộc hoàn toàn vào mapper chung.

## 3.2 File signed hiện được nhận biết bằng gì

Trong mapper chung, signed attachment hiện được suy ra từ `ParentId`:

- `QLDA.WebApi/Models/TepDinhKems/TepDinhKemMappingConfigurations.cs`
- `QLDA.Application/TepDinhKems/DTOs/TepDinhKemMappingConfiguration.cs`
- `BuildingBlocks/.../AttachmentCollectionExtensions.cs`
- `BuildingBlocks/.../AttachmentMapping.cs`

Tất cả đều dùng cùng convention:

```csharp
GroupType = SignedGroupTypeHelper.ResolveSignedGroupType(baseGroupType, model.ParentId != null)
```

`SignedGroupTypeHelper.ResolveSignedGroupType()` hiện hoạt động như sau:

- `ParentId == null` -> `GroupType = <baseGroupType>`
- `ParentId != null` -> `GroupType = KySo_<baseGroupType>`

## 3.3 Hệ quả

Handler thêm mới hiện chỉ lưu signed-file đúng khi rơi vào một trong các trường hợp:

1. FE gửi `ParentId != null`, hoặc
2. FE gửi `GroupType` đã là signed variant và flow map đó giữ lại được signed intent

Nó **không** hỗ trợ tự nhiên cho case:

```json
{
  "id": null,
  "parentId": null,
  "kySo": true
}
```

vì source hiện tại không hề có field `KySo` trên attachment model dùng cho form.

---

## 4. Handler cập nhật hiện thiếu hoặc sai chỗ nào

## 4.1 Bản thân handler update không lệch create

Các form đang theo pattern chuẩn đều làm giống create:

- update entity chính
- map lại toàn bộ `DanhSachTepDinhKem`
- gọi `AttachmentBulkInsertOrUpdateCommand`
- load lại attachments bằng `GetAttachmentsQuery`

Vì vậy, **không có sự khác biệt logic lớn giữa insert và update** ở controller form.

## 4.2 Chỗ thiếu thực sự

Điểm thiếu nằm ở shared contract và shared mapping:

### Thiếu #1 - `TepDinhKemModel` không có `KySo`

`QLDA.WebApi/Models/TepDinhKems/TepDinhKemModel.cs` hiện chỉ có:

- `Id`
- `GroupId`
- `GroupType`
- `Type`
- `FileName`
- `OriginalName`
- `Path`
- `Size`
- `ParentId`

Không có:

```csharp
public bool KySo { get; set; }
```

hoặc equivalent nullable flag.

### Thiếu #2 - shared mapper chỉ suy signed bằng `ParentId != null`

Các mapper hiện đều lấy signed intent từ `ParentId`, ví dụ:

```csharp
ResolveSignedGroupType(baseGroupType, model.ParentId != null)
```

Nên case signed-file mới nhưng `ParentId = null` sẽ bị map thành file thường.

### Thiếu #3 - normalize trong `AttachmentBulkInsertOrUpdateCommand` cũng đang bám `ParentId`

Ngay cả sau khi map ra `Attachment`, command sync chung vẫn có đoạn re-normalize:

```csharp
entity.GroupType = matchedBase.ResolveSignedGroupType(entity.ParentId != null);
```

Điều này có nghĩa:

- dù caller đã đưa file signed intent từ trước,
- nếu representation cuối cùng không mang được tín hiệu signed nào ngoài `ParentId`,
- command vẫn có thể normalize nó về base group type.

### Thiếu #4 - read model không expose cờ `KySo`

`ToModel()` hiện trả về `GroupType`, `ParentId`, ... nhưng không có `KySo`.

FE vẫn có thể suy từ `GroupType.StartsWith("KySo_")`, nhưng nếu mục tiêu là test/payload rõ ràng theo field `KySo`, thì read model hiện chưa nhất quán với write model.

---

## 5. API `quan-ly-ky-so/ky-so` hiện làm gì và vì sao không nên dùng cho case này

`QuanLyKySoController.Create` đang đi theo luồng khác:

1. `model.DanhSachTepDinhKem.ToEntities(model.GroupId, EGroupType.None)`
2. `NoiDungDaKyCommand`

`NoiDungDaKyCommand` được thiết kế cho luồng ký riêng:

- nếu có parent hợp lệ -> derive `KySo_<base>` từ parent
- nếu không có parent -> bắt caller truyền `GroupType`
- command này insert file signed trực tiếp

Điểm quan trọng:

- API này dành cho case "ký xong là lưu signed file ngay"
- Không phải luồng "FE gom cả file gốc + file signed rồi lưu cùng form"

Với yêu cầu mới, luồng đúng cần là:

- signed file đi qua **chính API form create/update**
- tái sử dụng `AttachmentBulkInsertOrUpdateCommand`
- không ép FE gọi `quan-ly-ky-so/ky-so`

API ký số riêng vẫn nên giữ nguyên cho màn khác.

---

## 6. `SyncCollection` có phải là nguyên nhân không

Ngắn gọn: **không phải nguyên nhân gốc**.

## 6.1 Vì sao không phải

`AttachmentBulkInsertOrUpdateCommand` + `SyncAttachmentsAsync()` hiện đã xử lý khá đúng cho case hỗn hợp:

- `Id != null` và tồn tại trong DB -> update/giữ lại
- `Id` chưa tồn tại trong DB -> add
- `AutoDeleteMissing = true` -> soft-delete file trong scope không còn trong request

Khi `AutoDeleteMissing = false`, command còn chủ động thu hẹp `existing` về intersection của request IDs để tránh xóa nhầm.

## 6.2 Điều cần chú ý

`SyncCollection` chỉ làm việc đúng khi dữ liệu đầu vào đã được map đúng:

- file cũ phải giữ đúng `Id`
- file mới phải có `Id` mới hoặc `Id = null` rồi mapper sinh mới
- signed-file phải có `GroupType` đúng là `KySo_<base>`

Nếu mapping sai ngay từ đầu, sync vẫn chạy "đúng kỹ thuật" nhưng ra kết quả nghiệp vụ sai.

---

## 7. Cách bảo toàn file cũ khi sync

Đây là behavior hiện tại của luồng chuẩn và cũng là behavior nên giữ nguyên khi sửa:

### Request item `Id != null` và đúng record DB

- giữ nguyên bản ghi cũ
- update metadata nếu có thay đổi
- không insert trùng

### Request item `Id == null`

- coi là file mới
- insert bản ghi mới

### File DB không còn trong request

- nếu controller đang gọi `AttachmentBulkInsertOrUpdateCommand` với `AutoDeleteMissing = true`
  - file trong đúng scope `GroupId + GroupTypes` sẽ bị soft-delete
- nếu `AutoDeleteMissing = false`
  - không xóa

### Điều kiện để không làm mất file cũ

Khi sửa cần giữ các nguyên tắc sau:

1. Không đổi `ResolveId` hiện có
2. Không bỏ qua item `Id != null`
3. Không normalize signed-file mới thành base group type
4. Chỉ sync trong đúng scope `GroupTypes` của uploader hiện tại
5. Không gom tất cả file signed về `GroupType = KySo`

---

## 8. Những file code nên sửa nếu triển khai

## 8.1 Shared write model / mapping

Các file gần như chắc chắn phải sửa:

- `QLDA.WebApi/Models/TepDinhKems/TepDinhKemModel.cs`
- `QLDA.WebApi/Models/TepDinhKems/TepDinhKemMappingConfigurations.cs`
- `QLDA.Application/TepDinhKems/DTOs/TepDinhKemInsertDto.cs`
- `QLDA.Application/TepDinhKems/DTOs/TepDinhKemDto.cs` hoặc DTO read/write tương ứng nếu muốn expose `KySo`
- `QLDA.Application/TepDinhKems/DTOs/TepDinhKemMappingConfiguration.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/Common/IAttachmentDto.cs` nếu muốn shared DTO contract biết `KySo`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/DTOs/AttachmentDto.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/DTOs/AttachmentInsertOrUpdateModel.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/DTOs/AttachmentMapping.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/Common/AttachmentCollectionExtensions.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/Common/SignedGroupTypeHelper.cs`
- `BuildingBlocks/src/BuildingBlocks.Application/Attachments/Commands/AttachmentBulkInsertOrUpdateCommand.cs`

## 8.2 Có thể không cần sửa controller form

Nếu sửa shared mapper đúng cách, đa số controller form hiện tại **không cần đổi**, vì chúng đã cùng gọi:

```csharp
model.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.<NghiepVu>)
```

và sau đó dùng `AttachmentBulkInsertOrUpdateCommand`.

Đây là hướng tốt nhất vì:

- insert và update tiếp tục dùng chung logic
- không tạo service mới
- không copy logic giữa các handler

## 8.3 Test nên bổ sung/cập nhật

- `QLDA.Tests/Unit/TepDinhKemResolveGroupTypeTests.cs`
- `BuildingBlocks/tests/BuildingBlocks.Tests/Application/Attachments/Phase3AttachmentHelperTests.cs`
- có thể thêm test mới cho `AttachmentBulkInsertOrUpdateCommand` hoặc `AttachmentCollectionExtensions`

---

## 9. Hướng sửa phù hợp nhất với source hiện tại

## 9.1 Hướng khuyến nghị

Thay vì sửa từng controller/hừng hực thêm if-else theo từng form, nên sửa ở **shared attachment mapping**:

### Nguyên tắc mới

File được coi là signed nếu:

1. `KySo == true`, hoặc
2. `ParentId != null`, hoặc
3. `GroupType` FE/DB đang là signed variant `KySo_<base>`

Từ đó derive:

- signed -> `KySo_<baseGroupType>`
- không signed -> `<baseGroupType>`

### Lợi ích

- không cần FE truyền `UploadFieldKey`
- không cần FE truyền `GroupType = KySo`
- không cần FE gọi API ký riêng
- không phụ thuộc signed-file mới có `ParentId`
- create/update đều tái sử dụng logic đang có

## 9.2 Điều không nên làm

Không nên sửa theo kiểu:

- thêm branch riêng trong từng controller update
- nếu `FileName.Contains(".signed.")` thì coi là file ký
- ép tất cả file signed về `GroupType = KySo`
- đổi `NoiDungDaKyCommand` để chen vào flow form update

Các cách này sẽ tạo convention mới, lệch chuẩn hiện tại và khó giữ thống nhất giữa read/write.

---

## 10. Request mẫu để FE test

## 10.1 Case 1 - form đã có ID, thêm file mới chưa ký

```json
{
  "id": "FORM_ID",
  "danhSachTepDinhKem": [
    {
      "id": null,
      "parentId": null,
      "fileName": "DanhSach.xlsx",
      "originalName": "DanhSach.xlsx",
      "path": "2026/07/28/DanhSach.xlsx",
      "type": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      "size": 9533,
      "kySo": false
    }
  ]
}
```

Kỳ vọng:

- insert 1 attachment mới
- `GroupId = FORM_ID`
- `GroupType = <baseGroupType của form>`

## 10.2 Case 2 - form đã có ID, thêm file mới rồi ký trước khi lưu

```json
{
  "id": "FORM_ID",
  "danhSachTepDinhKem": [
    {
      "id": null,
      "parentId": null,
      "fileName": "DanhSach.xlsx",
      "originalName": "DanhSach.xlsx",
      "path": "2026/07/28/DanhSach.xlsx",
      "type": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      "size": 9533,
      "kySo": false
    },
    {
      "id": null,
      "parentId": null,
      "fileName": "DanhSach.signed.pdf",
      "originalName": "DanhSach.signed.pdf",
      "path": "2026/07/28/DanhSach.signed.pdf",
      "type": "application/pdf",
      "size": 10533,
      "kySo": true
    }
  ]
}
```

Kỳ vọng sau khi sửa:

- không cần gọi `POST /api/quan-ly-ky-so/ky-so`
- file gốc lưu với `GroupType = <base>`
- file signed lưu với `GroupType = KySo_<base>`
- `ParentId` vẫn được phép `null`

## 10.3 Case 3 - file cũ + file mới + file signed mới

```json
{
  "id": "FORM_ID",
  "danhSachTepDinhKem": [
    {
      "id": "EXISTING_FILE_ID",
      "groupId": "FORM_ID",
      "groupType": "DeXuatNhuCauKinhPhi",
      "parentId": null,
      "fileName": "Cu.pdf",
      "originalName": "Cu.pdf",
      "path": "2026/07/01/Cu.pdf",
      "type": "application/pdf",
      "size": 1234,
      "kySo": false
    },
    {
      "id": null,
      "parentId": null,
      "fileName": "Moi.docx",
      "originalName": "Moi.docx",
      "path": "2026/07/28/Moi.docx",
      "type": "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "size": 4567,
      "kySo": false
    },
    {
      "id": null,
      "parentId": null,
      "fileName": "Moi.signed.pdf",
      "originalName": "Moi.signed.pdf",
      "path": "2026/07/28/Moi.signed.pdf",
      "type": "application/pdf",
      "size": 5678,
      "kySo": true
    }
  ]
}
```

Kỳ vọng:

- file cũ giữ nguyên
- 2 file mới được insert
- không insert trùng file cũ

### Lưu ý quan trọng

Payload mẫu trên là **payload mục tiêu** sau khi sửa. Ở source hiện tại, field `kySo` chưa có trong attachment model nên BE chưa dùng được field này.

---

## 11. Cách load lại file gốc và file signed

Hiện tại nhiều màn đang load theo:

```csharp
await Mediator.Send(new GetAttachmentsQuery(
    GroupIds: [entity.Id.ToString()]
))
```

`GetAttachmentsQuery` có `IncludeSigned = true` mặc định khi filter theo `BaseGroupTypes`, còn khi không truyền `BaseGroupTypes` thì nó lấy toàn bộ attachments theo `GroupId`.

Điều này có nghĩa:

- nếu signed file được lưu đúng `GroupType = KySo_<base>`
- và cùng `GroupId = FormId`

thì read side hiện tại về cơ bản đã đủ để load lại cả file gốc và file signed.

Điểm cần kiểm tra khi triển khai thật:

- mapping `ToModel()` có nên expose thêm `KySo` để FE không phải tự suy từ `GroupType`
- với các màn có nhiều uploader trên cùng `GroupId`, cần giữ đúng `BaseGroupTypes` khi query/split

---

## 12. Các test case bắt buộc khi sửa

### Test 1 - Form đã có ID, thêm file mới chưa ký

Kỳ vọng:

- insert thành công
- đúng `GroupId`
- đúng base `GroupType`

### Test 2 - Form đã có ID, thêm file mới rồi ký trước khi lưu

Kỳ vọng:

- không gọi API ký riêng
- lưu được cả file gốc và file signed
- file signed có `GroupType = KySo_<base>`
- `ParentId = null` vẫn chấp nhận

### Test 3 - Form có file cũ, thêm file mới và file signed

Kỳ vọng:

- file cũ không mất
- 2 file mới được insert
- không insert trùng file cũ

### Test 4 - Lưu nhiều lần

Kỳ vọng:

- file đã có `Id` không bị nhân đôi
- file signed không bị nhân đôi
- không xuất hiện `.signed.signed.pdf` do BE ký lại sai

### Test 5 - Xóa file mới trên FE trước khi bấm lưu

Kỳ vọng:

- file bị remove khỏi request thì không được insert
- file cũ còn lại không bị ảnh hưởng

### Test 6 - Load lại chi tiết form

Kỳ vọng:

- trả đủ file gốc
- trả đủ file signed
- đúng uploader/group type

---

## 13. Build và trạng thái test hiện tại

Tại thời điểm viết tài liệu này:

- Chưa sửa code
- Chưa chạy build/test cho thay đổi mới

Vì vậy trạng thái đúng cần báo là:

- **Build:** chưa chạy cho fix này
- **Test case thực thi:** chưa chạy, mới dừng ở mức phân tích source và xác định điểm lệch logic

---

## 14. Đề xuất thực hiện ở bước tiếp theo

Nếu triển khai code, nên đi theo thứ tự:

1. Bổ sung cờ `KySo` vào shared attachment request/response model ở mức cần thiết
2. Đổi shared mapping để signed intent = `KySo || ParentId != null || signed GroupType`
3. Giữ nguyên controller create/update hiện có, chỉ hưởng lợi từ shared mapper mới
4. Bổ sung unit test cho:
   - signed file mới với `KySo=true`, `ParentId=null`
   - update giữ file cũ + insert file mới + insert file signed
5. Build và chạy test liên quan

Hướng này thỏa cả các yêu cầu:

- không tạo service mới
- không sửa migration/schema
- không ép FE gọi API ký riêng
- không làm mất file cũ khi sync
- tái sử dụng tối đa logic create/update hiện có

---

## 15. Kết luận cuối

Insert và update form hiện đã khá đồng nhất; vấn đề không phải "update handler quên sync", mà là:

- attachment contract hiện không có `KySo`
- signed intent đang bị ràng vào `ParentId`
- normalize shared command cũng đang tiếp tục bám vào `ParentId`

Vì vậy, để hỗ trợ case:

- file mới chưa có `Id`
- file signed mới chưa có `ParentId`
- FE chỉ muốn gửi `KySo=true`

thì hướng đúng là sửa **shared attachment mapping + normalize logic**, không sửa riêng một controller đơn lẻ và không chuyển flow sang `quan-ly-ky-so/ky-so`.
