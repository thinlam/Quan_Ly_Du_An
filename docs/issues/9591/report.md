# Report #9591 — Bổ sung cấu hình phòng ban thực hiện và thời gian dự kiến cho từng bước

## 1. Yêu cầu chi tiết

### Các thông tin cần bổ sung cho từng bước:
- **Ngày bắt đầu dự kiến**
- **Ngày kết thúc dự kiến**
- **Ccb Phòng ban phụ trách**: Lấy danh sách từ các phòng ban phụ trách chính và phòng ban phối hợp được chọn trong thông tin dự án, sau đó **cho phép chọn 1**.
- **Ccb Phòng ban phối hợp**: Lấy danh sách từ các phòng ban phụ trách chính và phòng ban phối hợp được chọn trong thông tin dự án, sau đó **cho phép chọn 1**.

### Mục đích triển khai:
Cấu hình phòng ban thực hiện cho từng bước cụ thể, gắn thời gian dự kiến và sử dụng cấu hình này để phân quyền thực hiện các thao tác trên màn hình thuộc các bước tương ứng.

---

## 2. Quy tắc phân quyền và Ví dụ

**Nguyên tắc chung:**
- **Phòng KHTC** và **Ban Giám Đốc (BGĐ)**: Có quyền thao tác trên **tất cả các màn hình ở tất cả các bước**.
- **Phân quyền để hiển thị dự án:** Phụ thuộc vào phòng ban phụ trách chính và các phòng ban phối hợp của dự án đó.
- **Phân quyền để thực hiện thao tác màn hình:** Phụ thuộc vào cấu hình phòng ban theo từng bước.

**Ví dụ cụ thể:**
Giả sử có một dự án do phòng **Trung tâm tư vấn** phụ trách chính, các phòng còn lại phối hợp.

- **Bước 1: Đề xuất danh mục... (Có 3 màn hình)**
  - **Cấu hình:** Phòng **Nền tảng** phụ trách, phòng **Hạ tầng** phối hợp.
  - **Quyền thao tác:** Có tổng cộng **4 phòng** thao tác được (✅ KHTC, ✅ BGĐ, ✅ Nền tảng, ✅ Hạ tầng).
  - Các phòng còn lại: Chỉ được xem, không thể thao tác.

- **Bước 2: Lập và trình... (Có 4 màn hình)**
  - **Cấu hình:** Phòng **Trung tâm tư vấn** phụ trách, **Không** phòng ban phối hợp.
  - **Quyền thao tác:** Có tổng cộng **3 phòng** thao tác được (✅ KHTC, ✅ BGĐ, ✅ Trung tâm tư vấn).
  - Các phòng còn lại: Chỉ được xem, không thể thao tác.
