# Lỗi print phiếu trình kế hoạch triển khai hạng mục

## Mô tả

API xuất Word phiếu trình kế hoạch triển khai hạng mục (`GET /api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc`) bị lỗi với bản ghi **đã duyệt**: báo không có dữ liệu, file tải về không mở được, và các placeholder header chưa được điền.

| Khía cạnh | Chi tiết |
|-----------|----------|
| **Endpoint** | `GET /api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc?id={keHoachId}` |
| **ID test** | `30e8aa4e-4c4f-4c3d-8e0f-c419e941e44d` |
| **Trạng thái bản ghi** | Đã duyệt |
| **Spec gốc** | [docs/issues/9469/phieu-trinh-word-spec.md](../9469/phieu-trinh-word-spec.md) |
| **Trạng thái fix** | 📋 Đã phân tích — chờ implement |

## Hiện trạng (bug)

1. Gọi API với bản ghi đã duyệt → message **"Không có dữ liệu để xuất"**.
2. Client vẫn nhận response và lưu thành file `.docx` nhưng **không mở được** (thực tế là JSON lỗi).
3. Khi export thành công, template **chưa điền** các placeholder header:
   - `<So>` — Số phiếu trình
   - `<NgayLap>` — Ngày lập phiếu trình
   - `<DuAn>` — Tên dự án
   - `<TrichYeu>` — Trích yếu

## Yêu cầu xử lý

- Bản ghi đã duyệt (và mọi trạng thái có quyền xem + có hạng mục) phải in được.
- Nếu thật sự không có dữ liệu → trả message lỗi rõ ràng, **không** trả body dạng file.
- Map đầy đủ 4 placeholder header ở trên.
- Sau khi fix, test lại bằng ID `30e8aa4e-4c4f-4c3d-8e0f-c419e941e44d`.

## Tài liệu triển khai

| Tài liệu | Nội dung |
|----------|----------|
| **[report.md](report.md)** | Root cause + **8 bước code chi tiết** (loader, print query, Word exporter, test) + test plan |

## Thứ tự implement (tóm tắt)

1. Tạo `KeHoachTrienKhaiHangMucExportRowLoader` — extract map hạng mục
2. Refactor `KeHoachTrienKhaiHangMucGetExportQuery` dùng loader
3. Sửa `KeHoachTrienKhaiHangMucGetPhieuTrinhPrintQuery` — load 1 lần + message lỗi riêng
4. Sửa `KeHoachTrienKhaiHangMucWordExporter` — `Range.Replace` 4 placeholder
5. Integration test + build + curl ID test

## API test nhanh

```bash
curl --location 'http://192.168.1.12:9051/QuanLyDuAn/api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc?id=30e8aa4e-4c4f-4c3d-8e0f-c419e941e44d' \
  --header 'Authorization: Bearer <JWT_TOKEN>' \
  --output phieu-trinh-test.docx
```

**Kỳ vọng sau fix:**

- HTTP `200`, `Content-Type: application/vnd.openxmlformats-officedocument.wordprocessingml.document`
- File mở được bằng Word, header đã điền đủ 4 trường + bảng hạng mục
- Nếu lỗi: JSON `ResultApi` với message rõ (FE không tải nhầm thành `.docx`)
