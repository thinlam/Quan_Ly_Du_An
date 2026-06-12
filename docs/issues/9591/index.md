# Task #9591: Bổ sung cấu hình phòng ban thực hiện và thời gian dự kiến cho từng bước

## Trạng thái
- Ngày ghi nhận: 12/06/2026
- Tình trạng: Mới

## Mục tiêu
Bổ sung khả năng cấu hình phòng ban thực hiện và thời gian dự kiến cho từng bước trong tiến độ thực hiện dự án. Từ đó phân quyền thao tác màn hình theo bước dựa trên cấu hình phòng ban này.

## Luồng triển khai dự kiến
1. **Khảo sát:** Đánh giá entity của Bước/Tiến độ.
2. **Domain/Persistence:** Bổ sung các cột ngày dự kiến và reference tới phòng ban phụ trách, phối hợp.
3. **Application:** Cập nhật DTOs, Mapping và các logic Validation liên quan đến nguồn chọn (chỉ được chọn phòng ban lấy từ dự án).
4. **Api & Phân quyền:** Phân quyền theo quy tắc mới cho các API thao tác trong bước.
