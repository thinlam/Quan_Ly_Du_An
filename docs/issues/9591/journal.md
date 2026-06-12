# Journal #9591 — Hành trình triển khai

## 12/06/2026 — Giai đoạn 1: Tiếp nhận yêu cầu

- Ghi nhận task #9591: Bổ sung cấu hình phòng ban thực hiện và thời gian dự kiến cho từng bước.
- Lập `index.md` gốc với trạng thái (Mới), mục tiêu, luồng triển khai dự kiến (4 bước: Khảo sát → Domain/Persistence → Application → Api & Phân quyền).

## 12/06/2026 — Giai đoạn 2: Phân tích yêu cầu

- Chi tiết hóa 4 field cần bổ sung cho từng bước:
  - Ngày bắt đầu dự kiến (`DateTime?`)
  - Ngày kết thúc dự kiến (`DateTime?`)
  - Ccb Phòng ban phụ trách (chọn 1 từ danh sách phòng ban của dự án)
  - Ccb Phòng ban phối hợp (chọn 1 từ danh sách phòng ban của dự án)
- Làm rõ use case phân quyền:
  - KHTC & BGĐ: full quyền trên tất cả màn hình, tất cả bước.
  - Phòng ban khác: quyền thao tác phụ thuộc vào cấu hình phòng ban theo từng bước.
  - Phân 2 tầng: hiển thị dự án (theo phòng ban dự án) vs. thao tác màn hình (theo phòng ban bước).

## 12/06/2026 — Giai đoạn 3: Tổng hợp báo cáo

- Tổng hợp quy tắc phân quyền + ví dụ cụ thể vào `report.md`.
- Ví dụ Bước 1 (Đề xuất danh mục): 4 phòng thao tác (KHTC, BGĐ, Nền tảng, Hạ tầng).
- Ví dụ Bước 2 (Lập và trình): 3 phòng thao tác (KHTC, BGĐ, Trung tâm tư vấn).

## 12/06/2026 — Giai đoạn 4: Hợp nhất hồ sơ

- Gộp 4 file nguồn thành 3 file đích:
  - `index.md` ← gộp `index.md` (gốc) + `9591-index.md` (sau phân tích).
  - `report.md` ← gộp `report.md` + `9591-report.md` (dedupe, giữ bản có Type).
  - `journal.md` ← tạo mới (file này).
- Xóa 2 file trùng lặp: `9591-index.md`, `9591-report.md`.
