# Issue #9489 — UC89: Tạo lập hồ sơ để bàn giao

## Tổng quan

Use case UC89 mô tả chức năng cho phép phòng ban chủ trì tạo lập và quản lý hồ sơ để chuẩn bị bàn giao cho phòng HC-TH.

---

## Các chức năng chính

| # | Chức năng | Mô tả |
|---|------------|-------|
| 1 | Xem danh sách hồ sơ | Hiển thị danh sách hồ sơ đã tạo lập |
| 2 | Tạo mới hồ sơ | Tạo mới hồ sơ để bàn giao |
| 3 | Đưa file đính kèm từ dự án | Chọn file (VB, QĐ, HĐ, …) đã đính kèm trong quá trình thực hiện dự án |
| 4 | Đính kèm file từ máy tính | Upload thêm file hồ sơ từ máy tính |
| 5 | Chỉnh sửa hồ sơ | Hoàn chỉnh hồ sơ tạo lập |
| 6 | Kết xuất Excel | Xuất danh sách VB, QĐ, HĐ, … ra file Excel |

---

## Tác nhân

| Tác nhân | Vai trò |
|----------|---------|
| Phòng được giao chủ trì | Tạo lập và quản lý hồ sơ để bàn giao |

---

## Luồng nghiệp vụ chính

```
1. CB phòng chủ trì xem danh sách hồ sơ đã tạo lập
2. Tạo mới hồ sơ để bàn giao
3. Chọn file (VB, QĐ, HĐ, …) từ dự án để đưa vào hồ sơ
4. Upload thêm file hồ sơ từ máy tính (nếu cần)
5. Chỉnh sửa, hoàn chỉnh hồ sơ
6. Kết xuất Excel danh sách tài liệu
7. → Chuyển sang UC90 để thực hiện bàn giao
```

---

## Phụ thuộc

| Use case | Mối quan hệ | Mô tả |
|----------|--------------|-------|
| UC90 | Kế thừa | Hồ sơ tạo lập sẽ được bàn giao qua UC90 |

---

## Chi tiết chức năng

### 3.1 Đưa file đính kèm từ dự án

- Hỗ trợ chọn từng file hoặc chọn tất cả file cùng lúc
- Các loại file: Văn bản (VB), Quyết định (QĐ), Hợp đồng (HĐ), …
- File đã được đính kèm trong quá trình thực hiện dự án

### 3.2 Đính kèm file từ máy tính

- Upload file trực tiếp từ máy tính cục bộ
- Hỗ trợ các định dạng phổ biến (PDF, DOCX, XLSX, …)

### 3.3 Kết xuất Excel

- Xuất danh sách các file trong hồ sơ ra file Excel
- Bao gồm: tên file, loại file, ngày tạo, …

---

## Ghi chú triển khai

- Chức năng tạo lập hồ sơ đã được triển khai trong module `BanGiaoHoSo`
- Xem chi tiết tại `docs/use-cases/TaoLapHoSoDeBanGiao/UC89-TaoLapHoSoDeBanGiao.md`
