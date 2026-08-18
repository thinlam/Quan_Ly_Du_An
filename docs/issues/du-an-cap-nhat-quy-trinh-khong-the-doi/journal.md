# Work log — `du-an/cap-nhat` "Quy trình không thể đổi"

## 2026-08-18

- **Phân tích lỗi** `PUT /api/du-an/cap-nhat`.
- Trace luồng: `DuAnController.Update` → `DuAnUpdateCommandHandler.Handle`.
- Xác định root cause: guard tại `DuAnUpdateCommand.cs:51-54` ném `ManagedException("Quy trình không thể đổi")` khi request đổi `quyTrinhId` trên dự án đã có tiến độ (`HasDuAnBuocTienDoAsync`, dòng 114–127).
- Xác nhận bằng log thật `QLDA.WebApi/logs/service-20260817.log` (dòng 1574, 1615).
- Xác nhận phần `400 → 500` + chuỗi `"Response status code does not indicate success: 400"` không nằm trong code QLDA (không có `HttpClient`/`EnsureSuccessStatusCode`); do proxy/gateway trước QLDA gây ra.
- Tạo bộ docs issue `du-an-cap-nhat-quy-trinh-khong-the-doi`: `index.md`, `report.md`, `test-workflow.md`, `journal.md`.

### Quyết định / ghi chú

- Chưa implement fix (đang chờ xác nhận nghiệp vụ: có cho phép đổi quy trình khi dự án đã nhập tiến độ không).
- Nếu FE không nên đổi quy trình → Phương án A (không gửi `quyTrinhId`).
- Nếu vẫn cần đổi → Phương án B (reset tiến độ DuAnBuoc trước khi clone).
- Triệu chứng 400→500 → Phương án C (sửa proxy forward status thật).
