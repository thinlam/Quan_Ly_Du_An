# Báo cáo khảo sát & implement — Issue 180: loại `Chứng từ` cho Văn bản pháp lý

## 1. Trace source hiện tại

Flow `POST api/van-ban-phap-ly/them-moi`:

```
VanBanPhapLyController.Create(VanBanPhapLyModel)            QLDA.WebApi/Controllers/VanBanPhapLyController.cs:60
→ model.ToEntity()                                          QLDA.WebApi/Models/VanBanPhapLys/VanBanPhapLyMappingConfiguration.cs
→ VanBanPhapLyInsertOrUpdateCommand(entity)                 QLDA.Application/VanBanPhapLys/Commands/VanBanPhapLyInsertOrUpdateCommand.cs
→ VanBanPhapLy : VanBanQuyetDinh                            QLDA.Domain/Entities/VanBanPhapLy.cs, VanBanQuyetDinh.cs
→ Loai = EnumLoaiVanBanQuyetDinh.VanBanPhapLy.ToString()    HARD-CODED trước khi sửa
→ EF TPT: cột VanBanQuyetDinh.Loai (string)                 QLDA.Persistence/Configurations/VanBanQuyetDinhConfiguration.cs
```

## 2. Trả lời các câu hỏi bắt buộc

**Q1. API đang nhận `Loai` ở DTO/Command nào?**
Không — trước khi sửa `VanBanPhapLyModel` **không có field `Loai`**. Mapping hard-code
`Loai = EnumLoaiVanBanQuyetDinh.VanBanPhapLy.ToString()` tại `VanBanPhapLyMappingConfiguration.cs`
(dòng 43 — `ToEntity`, dòng 58 — `Update`). Sau khi sửa, model nhận `Loai` (string?).

**Q2. `Loai` hiện được định nghĩa theo cách nào?**
- **Enum:** `EnumLoaiVanBanQuyetDinh` (`QLDA.Domain/Enums/EnumLoaiVanBanQuyetDinh.cs`).
- **Lưu DB:** dạng **string = tên enum** vào cột `VanBanQuyetDinh.Loai` (bảng cha, TPT) — cột đã tồn tại.
- **Map tên hiển thị:** `TongHopVanBanQuyetDinhGetListQuery.cs` dùng
  `e.Loai.GetDescriptionFromName<EnumLoaiVanBanQuyetDinh>()` → lấy từ `[Description]`.
- **Enum-list FE:** `DanhMucEnumController.ListEnum` (`EnumsExtensions.EnumAll<EnumLoaiVanBanQuyetDinh>()`)
  tự include mọi enum member.

**Q3. Có bảng danh mục nào khác không?**
Có `DanhMucLoaiVanBan` (`LoaiVanBanId`, int) — nhưng đây là khái niệm **khác**
("Loại văn bản" cấu hình được). Task `loai = ChungTu` thuộc enum `EnumLoaiVanBanQuyetDinh`, **không đụng** danh mục này.

**Q4. Có Validator giới hạn danh sách loại không?**
Không có FluentValidation cho VanBanPhapLy. Phần "giới hạn loại hợp lệ" được xử lý ngay tại mapping
(whitelist `VanBanPhapLy` + `ChungTu`).

**Q5. Handler thêm mới lưu `Loai` như thế nào?**
`VanBanPhapLyInsertOrUpdateCommandHandler` lưu entity trực tiếp; `Loai` được set sẵn từ
`model.ToEntity()`. Không đổi handler.

## 3. Các file đã sửa

| File | Thay đổi |
|------|----------|
| `QLDA.Domain/Enums/EnumLoaiVanBanQuyetDinh.cs` | Thêm `[Description("Chứng từ")] ChungTu,` ngay sau `VanBanPhapLy` |
| `QLDA.WebApi/Models/VanBanPhapLys/VanBanPhapLyModel.cs` | Thêm `public string? Loai { get; set; }` |
| `QLDA.WebApi/Models/VanBanPhapLys/VanBanPhapLyMappingConfiguration.cs` | `ToEntity()`: `Loai = ResolveLoai(model.Loai)`; `Update()`: đổi sang `ChungTu` nếu FE gửi, ngược lại giữ `entity.Loai`; thêm helper `ResolveLoai` |

Logic `ResolveLoai`:

```csharp
private static string ResolveLoai(string? loai) =>
    loai == nameof(EnumLoaiVanBanQuyetDinh.ChungTu)
        ? nameof(EnumLoaiVanBanQuyetDinh.ChungTu)
        : EnumLoaiVanBanQuyetDinh.VanBanPhapLy.ToString();
```

- Create: không gửi `loai` / gửi giá trị khác → lưu `VanBanPhapLy` (giữ nguyên behavior cũ).
- Create: gửi `"ChungTu"` → lưu `ChungTu`.
- Update: gửi `"ChungTu"` → đổi sang `ChungTu`; không gửi / giá trị khác → giữ nguyên `entity.Loai`.

## 4. Migration

**Không cần migration.**

- Cột `VanBanQuyetDinh.Loai` (string) đã tồn tại từ trước — thêm enum member không đổi schema.
- Không sửa `AppDbContextModelSnapshot.cs`.
- Không cần thêm dữ liệu seed danh mục.

## 5. Những chỗ KHÔNG đổi (tránh lan man)

- `VanBanPhapLyDto`, `VanBanPhapLyGetDanhSachQuery` — list tiến độ không trả `Loai`, không yêu cầu.
- Path DTO dead-code: `VanBanPhapLyInsertCommand/UpdateCommand` + `VanBanPhapLyInsertDto/UpdateDto`
  (controller không dùng) — giữ nguyên.
- `LoaiVanBanQuyetDinhConst.Dictionary` — chưa thêm vì cần tên `PartialView` cho `ChungTu`
  mà FE chưa xác nhận. Nếu sau này cần hiển thị partial view riêng, bổ sung
  `{ nameof(EnumLoaiVanBanQuyetDinh.ChungTu), "CHUNGTU" }` sau khi FE chốt tên view.

## 6. Ảnh hưởng / blast radius

| Thay đổi | Phạm vi ảnh hưởng | Rủi ro |
|----------|-------------------|--------|
| Thêm enum member `ChungTu` | `EnumAll` list API (thêm 1 option), `GetDescriptionFromName` (resolves "Chứng từ") | Thấp |
| Thêm `VanBanPhapLyModel.Loai` | Chỉ mapping create/update; null default → không đổi contract cũ | Thấp |
| Đổi mapping `Loai` | Chỉ tạo/cập nhật VanBanPhapLy | Thấp |

Không có HIGH/CRITICAL. Không ảnh hưởng dữ liệu DB hiện có.

## 7. Kết quả build

```
dotnet build SER.sln
Build succeeded.
0 Warning(s)
0 Error(s)
```