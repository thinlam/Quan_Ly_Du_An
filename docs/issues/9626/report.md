# Report #9626 — Hiệu chỉnh không tính ngày kết thúc dự kiến từ số ngày thực hiện

> Cập nhật: 25/06/2026 — tắt tạm cơ chế tự động tính ngày trong `DuAnBuocCloneCommand`

## 1. Bối cảnh

### Vấn đề
Trước đây, khi người dùng tạo dự án mới và chọn `NgayBatDau`, hệ thống tự động duyệt qua cây bước (`DanhMucBuoc` theo `QuyTrinhId`) và gán:
- `NgayDuKienBatDau` cho từng node lá
- `NgayDuKienKetThuc` = `NgayDuKienBatDau` + `SoNgayThucHien` ngày

Cơ chế này phụ thuộc vào `DanhMucBuoc.SoNgayThucHien` — trường mô tả template, không phải dữ liệu dự án thực tế.

### Liên quan
- Issue #9591 — Cấu hình phòng ban thực hiện và thời gian dự kiến cho từng bước
  - Đã thêm 2 field `NgayBatDauDuKien` / `NgayKetThucDuKien` cho entity `DuAnBuoc`
  - Cho phép người dùng cấu hình ngày thủ công theo từng bước

### Mục đích thay đổi
- Bỏ cơ chế tự động tính ngày từ `SoNgayThucHien`
- Để người dùng cấu hình ngày dự kiến thủ công qua màn hình `DuAnBuoc` (đã có sẵn từ #9591)
- Tránh ghi đè dữ liệu người dùng đã nhập khi clone cây bước

---

## 2. Code đã thay đổi (tắt tạm)

### File: `QLDA.Application/DuAnBuocs/Commands/DuAnBuocCloneCommand.cs`

**Trước (dòng 165-176):**

```csharp
if (isLeaf && request.DuAn.NgayBatDau != null) {
    var startDate = request.DuAn.NgayBatDau;
    var endDate = request.DuAn.NgayBatDau;
    node.NgayDuKienBatDau = firstNode ? startDate : endDate!.Value.AddDays(1);
    node.NgayDuKienKetThuc = node.NgayDuKienBatDau!.Value.AddDays(
        step.SoNgayThucHien == 0 ? 1 : step.SoNgayThucHien
    );
    ;
    endDate = node.NgayDuKienKetThuc.Value;
    firstNode = false;
} else {
    node.NgayDuKienBatDau = null;
    node.NgayDuKienKetThuc = null;
}
```

**Sau (comment-out + TODO):**

```csharp
if (isLeaf && request.DuAn.NgayBatDau != null) {
    // TODO: TẠM TẮT - bật lại sau khi fix SoNgayThucHien → NgayDuKienKetThuc (#9626)
    // var startDate = request.DuAn.NgayBatDau;
    // var endDate = request.DuAn.NgayBatDau;
    // node.NgayDuKienBatDau = firstNode ? startDate : endDate!.Value.AddDays(1);
    // node.NgayDuKienKetThuc = node.NgayDuKienBatDau!.Value.AddDays(step.SoNgayThucHien == 0 ? 1 : step.SoNgayThucHien);
    // endDate = node.NgayDuKienKetThuc.Value;
    // firstNode = false;
} else {
    node.NgayDuKienBatDau = null;
    node.NgayDuKienKetThuc = null;
}
```

### Hành vi hiện tại

| Trường hợp | `NgayDuKienBatDau` | `NgayDuKienKetThuc` |
|------------|--------------------|--------------------|
| Node lá, `DuAn.NgayBatDau != null` | `null` | `null` |
| Node lá, `DuAn.NgayBatDau == null` | `null` (rơi nhánh `else`) | `null` |
| Node không phải lá | `null` (giữ nguyên) | `null` (giữ nguyên) |

---

## 3. Hướng xử lý triệt để (chưa làm)

### Phương án A: Xóa hẳn code
- Xóa luôn khối `if (isLeaf && ...)` và biến `firstNode`/`endDate`/`startDate`
- Migration cleanup: nếu DB đang có dữ liệu `NgayDuKienBatDau`/`NgayDuKienKetThuc` cũ, cần quyết định giữ hay xóa

### Phương án B: Tính theo `NgayBatDauDuKien` / `NgayKetThucDuKien` từ #9591
- Thay vì dùng `SoNgayThucHien`, dùng 2 field mới từ issue #9591
- Cần xác nhận field nào được set ở đâu (template hay runtime)

### Phương án C: Bỏ hẳn auto-clone ngày
- Người dùng phải tự nhập ngày cho từng bước sau khi clone
- Đây là hành vi gần nhất với "tắt tạm" hiện tại

→ Cần xác nhận với PM/BA phương án nào phù hợp trước khi triển khai triệt để.

---

## 4. Verification

### Build
- `dotnet build QLDA.Application/QLDA.Application.csproj` → **PASS** (0 errors)
- Warning: `CS0219 firstNode assigned but never used` (vô hại, do code bên trong đã comment)

### Test
- `dotnet test QLDA.Tests/QLDA.Tests.csproj` → **138 total, 116 passed, 17 skipped, 5 failed**
- 5 test failed đều là pre-existing (đã fail từ trước khi sửa), thuộc module `PhanKhaiKinhPhi` / `TinhHinhThucHienDauThauExport` — **không liên quan** thay đổi
- Số test fail/pass giống hệt main branch (verified bằng `git stash` + rerun)

### Impact Analysis
- `DuAnBuocCloneCommandHandler`: 0 upstream callers trong graph (chỉ gọi qua MediatR)
- `detect_changes`: chỉ ảnh hưởng `DuAnBuocCloneCommand.cs` + 2 file docs đã có từ trước (`AGENTS.md`, `CLAUDE.md`, `DuAnController.cs`)
- Risk: **LOW**

---

## 5. Cách bật lại

Khi đã có phương án triệt để, có 2 lựa chọn:

### Cách 1: Khôi phục code cũ
Xóa 6 dòng `//` trong `DuAnBuocCloneCommand.cs:165-176`, đoạn code sẽ hoạt động lại như cũ.

### Cách 2: Thay thế bằng logic mới
Tham khảo Phương án B (dùng field từ #9591) hoặc Phương án C (bỏ auto-clone) tùy quyết định của PM.

---

## 6. Files liên quan

| File | Trạng thái |
|------|------------|
| `QLDA.Application/DuAnBuocs/Commands/DuAnBuocCloneCommand.cs` | Đã sửa (comment-out) |
| `QLDA.Application/DanhMucBuocs/DanhMucBuocMappings.cs:17,33` | Không sửa (giữ nguyên 2 method ghi `SoNgayThucHien` vì vẫn cần cho Insert/Update template) |
| `docs/issues/9626/index.md` | File mô tả task |
| `docs/issues/9626/report.md` | File này |

---

## 7. Câu hỏi chưa giải quyết

- PM muốn chọn phương án A / B / C nào để xử lý triệt để?
- Có cần migration cleanup dữ liệu `NgayDuKienBatDau`/`NgayDuKienKetThuc` cũ trong DB không?
- Có cần thông báo cho user biết field này sẽ không auto-fill nữa không?
