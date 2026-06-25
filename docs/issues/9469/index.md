# Issue #9469 — Kết xuất Excel Kế hoạch triển khai hạng mục

## Mô tả nghiệp vụ

CB/LĐ có thể **kết xuất Excel** danh sách hạng mục công việc trong Kế hoạch triển khai theo mẫu **"mẫu kế hoạch triển khai sau khi đã lập.xlsx"**.

Dữ liệu export phải:
- Bám đúng nội dung kế hoạch đang hiển thị trên màn hình (cùng nguồn dữ liệu với API danh sách/chi tiết hiện có).
- **Group theo Giai đoạn** dự án (ví dụ: A. Chuẩn bị đầu tư, B. Thực hiện đầu tư).
- Trong mỗi giai đoạn liệt kê các **hạng mục/công việc** thuộc giai đoạn đó.

## Màn hình liên quan

| Thành phần | API hiện tại |
|------------|--------------|
| Danh sách kế hoạch triển khai | `GET /api/ke-hoach-trien-khai-hang-muc/danh-sach-tien-do` |
| Chi tiết kế hoạch (có `HangMucTrienKhai`) | `GET /api/ke-hoach-trien-khai-hang-muc/{id}/chi-tiet` |

## Mẫu Excel (tham chiếu)

File preview cấu trúc: [`template-preview.json`](template-preview.json)

![Mẫu Excel](template-preview.png) *(ảnh mẫu từ issue)*

## Tài liệu triển khai

- **[report.md](report.md)** — Spec kỹ thuật đầy đủ: API, DTO, query, template, mapping cột, checklist implement.
