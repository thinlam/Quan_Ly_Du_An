# Task #9626 - Hiệu chỉnh không tính ngày kết thúc dự kiến từ số ngày thực hiện

## 1. Trạng thái & Mục tiêu (Ghi nhận 25/06/2026)

### Trạng thái
- Ngày ghi nhận: 25/06/2026
- Tình trạng: Đã code tắt tạm (comment-out + TODO)

### Mục tiêu
Bỏ cơ chế tự động cộng `SoNgayThucHien` để tính `NgayDuKienKetThuc` của bước dự án (`DuAnBuoc`). Khoảng thời gian dự kiến cho từng bước đã được cấu hình riêng theo issue [#9591](../9591/index.md), không còn dùng `SoNgayThucHien` cho mục đích này nữa.

### Luồng triển khai dự kiến
1. **Khảo sát:** Xác nhận `DuAnBuocCloneCommand` là nơi duy nhất dùng `SoNgayThucHien` để tính ngày.
2. **Hiệu chỉnh code:** Comment-out khối tính `NgayDuKienBatDau` + `NgayDuKienKetThuc` trong `DuAnBuocCloneCommand.Clone()` — để các field này = null, người dùng tự cấu hình qua issue #9591.
3. **Đánh dấu TODO:** Ghi chú rõ đoạn code cần xử lý khi thay thế bằng logic mới.

## 2. Mô tả sau phân tích (25/06/2026)

### Hiện trạng (trước khi tắt)
Trong `QLDA.Application/DuAnBuocs/Commands/DuAnBuocCloneCommand.cs:165-176`, khi clone cây bước, mỗi node lá có `DuAn.NgayBatDau != null` sẽ được tự động tính:

```csharp
node.NgayDuKienBatDau = firstNode ? startDate : endDate!.Value.AddDays(1);
node.NgayDuKienKetThuc = node.NgayDuKienBatDau!.Value.AddDays(
    step.SoNgayThucHien == 0 ? 1 : step.SoNgayThucHien
);
endDate = node.NgayDuKienKetThuc.Value;
```

### Vấn đề
- `SoNgayThucHien` ở `DanhMucBuoc` (template) đang được dùng cho 2 mục đích: mô tả template và tự động tính timeline dự án.
- Sau khi issue #9591 cấu hình `NgayBatDauDuKien` / `NgayKetThucDuKien` riêng cho từng `DuAnBuoc`, việc tự động tính từ `SoNgayThucHien` trở nên **thừa** và có nguy cơ ghi đè dữ liệu người dùng đã nhập.

### Hành vi sau khi tắt tạm
- Node lá (leaf) khi `DuAn.NgayBatDau != null`: `NgayDuKienBatDau = null`, `NgayDuKienKetThuc = null`
- Node lá khi `DuAn.NgayBatDau == null`: rơi vào nhánh `else` — cũng null
- Node không phải lá: null (giữ nguyên)
- Người dùng cấu hình ngày thủ công qua màn hình `DuAnBuoc` (đã có sẵn từ #9591)

→ Chi tiết kỹ thuật xem [report.md](./report.md)

---

<!-- REGION: Nội dung gốc từ yêu cầu ban đầu (không chỉnh sửa) -->

## 3. Nội dung gốc (Yêu cầu ban đầu)

> **Mô tả:**
> Hiện tại khi chọn ngày bắt đầu -> thì chỗ tiến độ tg dự kiến sẽ + số ngày thực hiện để nó ra khoảng tg => này hiệu chỉnh lại ko cho tính ngày nữa vì đã có chỗ cấu hình riêng cho khoảng tg ở task [#9591](https://pmis.vietinfo.tech:8080/issues/9591)
>
> **Liên quan:** #9591 — Cấu hình phòng ban thực hiện cho từng bước (đã có cấu hình riêng cho khoảng thời gian dự kiến từng bước)

<!-- END REGION -->
