# Task #9591 - Chi tiết yêu cầu

## Mục tiêu

Có thể cấu hình được phong ban thực hiện cho từng bước, gắn thời gian dự kiến.

## Các thông tin cần bổ sung

### Ngày bắt đầu dự kiến
- Type: DateTime?
- Mô tả: Ngày bắt đầu dự kiến của bước

### Ngày kết thúc dự kiến
- Type: DateTime?
- Mô tả: Ngày kết thúc dự kiến của bước

### Ccb Phòng ban phụ trách
- Nguồn: Các phòng ban phụ trách chính, phòng ban phối hợp được chọn trong thông tin dự án
- Cho phép chọn 1 phòng ban

### Ccb Phòng ban phối hợp
- Nguồn: Các phòng ban phụ trách chính, phòng ban phối hợp được chọn trong thông tin dự án
- Cho phép chọn 1 phòng ban

## Ví dụ use case

Trong 1 dự án:
- Phòng KHTC, BGD sẽ thực hiện full các màn hình full bước

Dự án do phòng Trung tâm tư vấn phụ trách chính, các phòng còn lại đều phối hợp => Phân để hiển thị dự án

Sẽ cấu hình phòng ban thực hiện theo theo bước => Phân để thực hiện thao tác màn hình trong các bước

### Bước 1: Đề xuất danh mục... (có 3 màn hình)
- Gắn phòng Nền tảng phụ trách, phòng Hạ tầng phối hợp
- Các màn hình trong bước này có 4 phòng thao tác được: KHTC, BGĐ, Nền tảng, Hạ tầng
- Các phòng còn lại vẫn thấy nhưng không thao tác được

### Bước 2: Lập và trình... (có 4 màn hình)
- Gắn cho phòng Trung tâm tư vấn phụ trách, không có phối hợp
- Các màn hình trong bước này có 3 phòng thao tác được: KHTC, BGĐ, Trung tâm tư vấn
- Các phòng còn lại vẫn thấy nhưng không thao tác được
