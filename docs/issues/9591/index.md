# Task #9591 - Cấu hình phòng ban thực hiện cho từng bước

## 1. Trạng thái & Mục tiêu (Ghi nhận 12/06/2026)

### Trạng thái
- Ngày ghi nhận: 12/06/2026
- Tình trạng: Mới

### Mục tiêu
Bổ sung khả năng cấu hình phòng ban thực hiện và thời gian dự kiến cho từng bước trong tiến độ thực hiện dự án. Từ đó phân quyền thao tác màn hình theo bước dựa trên cấu hình phòng ban này.

### Luồng triển khai dự kiến
1. **Khảo sát:** Đánh giá entity của Bước/Tiến độ.
2. **Domain/Persistence:** Bổ sung các cột ngày dự kiến và reference tới phòng ban phụ trách, phối hợp.
3. **Application:** Cập nhật DTOs, Mapping và các logic Validation liên quan đến nguồn chọn (chỉ được chọn phòng ban lấy từ dự án).
4. **Api & Phân quyền:** Phân quyền theo quy tắc mới cho các API thao tác trong bước.

## 2. Mô tả sau phân tích (12/06/2026)

### Các thông tin cần bổ sung cho từng bước:
- **Ngày bắt đầu dự kiến**
- **Ngày kết thúc dự kiến**
- **Ccb Phòng ban phụ trách**: Lấy danh sách từ các phòng ban phụ trách chính và phòng ban phối hợp được chọn trong thông tin dự án, sau đó **cho phép chọn 1**.
- **Ccb Phòng ban phối hợp**: Lấy danh sách từ các phòng ban phụ trách chính và phòng ban phối hợp được chọn trong thông tin dự án, sau đó **cho phép chọn 1**.

### Mục đích
Có thể cấu hình được phòng ban thực hiện cho từng bước, gắn thời gian dự kiến.

→ Chi tiết triển khai xem [report.md](./report.md)

---

<!-- REGION: Nội dung gốc từ yêu cầu ban đầu (không chỉnh sửa) -->

## 3. Nội dung gốc (Yêu cầu ban đầu)

> **Mô tả:**
> Bổ sung các thông tin:
> - Ngày bắt đầu dự kiến
> - Ngày kết thúc dự kiến
> - Ccb Phòng ban phụ trách -> Sẽ lấy các phòng ban phụ trách chính, phòng ban phối hợp được chọn trong thông tin dự án và cho chọn 1
> - Ccb Phòng ban phối hợp -> Sẽ lấy các phòng ban phụ trách chính, phòng ban phối hợp được chọn trong thông tin dự án và cho chọn 1
>
> **Mục đích:** có thể cấu hình được phong ban thực hiện cho từng bước, gắn thời gian dự kiến
>
> **Ví dụ:** trong 1 dự án
> - Phòng KHTC, BGD sẽ thực hiện full các màn hình full bước
> - Dự án do phòng Trung tâm tư vấn phụ trách chính, các phòng còn lại đều phối hợp => Phân để hiển thị dự án
> - Sẽ cấu hình phòng ban thực hiện theo theo bước => Phân để thực hiện thao tác màn hình trong các bước
>
> **Bước 1:** Đề xuất danh mục... (có 3 màn hình) gắn phòng Nền tảng phụ trách, phòng Hạ tầng phối hợp => các màn hình trong bước này có 4 phòng thao tác đc (KHTC, BGĐ, Nền tảng, Hạ tầng), các phòng còn lại vẫn thấy không thao được
>
> **Bước 2:** Lập và trình... (có 4 màn hình) gắn cho phòng Trung tâm tư vấn phụ trách, không có phối hợp => các màn hình trong bước này có 3 phòng thao tác đc (KHTC, BGĐ, Trung tâm tư vấn), các phòng còn lại vẫn thấy không thao được

<!-- END REGION -->
