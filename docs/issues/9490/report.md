# Issue #9490 — UC90: Bàn giao hồ sơ đã tạo lập để lưu trữ

## Tổng quan

Use case UC90 mô tả chức năng cho phép phòng ban chủ trì thực hiện bàn giao hồ sơ đã tạo lập (từ UC89) sang phòng HC-TH để lưu trữ.

---

## Các chức năng chính

| # | Chức năng | Mô tả | Endpoint |
|---|------------|-------|----------|
| 1 | Kết xuất biên bản bàn giao | Xuất file biên bản bàn giao hồ sơ | — |
| 2 | Chuyển bàn giao hồ sơ | Chuyển hồ sơ đã tạo cho phòng HC-TH | — |
| 3 | Tìm kiếm và xem hồ sơ | Tìm kiếm hồ sơ khi có yêu cầu | — |
| 4 | Kết xuất Excel | Xuất kết quả tìm kiếm ra Excel | — |
| 5 | Bổ sung màn hình Bàn giao hồ sơ | Giao diện mới cho chức năng bàn giao | — |
| 6 | Cập nhật trạng thái | Hồ sơ chuyển xuống danh sách "Hồ sơ đã bàn giao" | — |
| 7 | In Biên bản bàn giao | In mẫu Biên bản bàn giao hồ sơ | `GET /api/print/bien-ban-ban-giao-ho-so` |

---

## Tác nhân

| Tác nhân | Vai trò |
|----------|---------|
| Phòng được giao chủ trì | Thực hiện bàn giao hồ sơ |
| Phòng HC-TH | Nhận hồ sơ để lưu trữ |

---

## Luồng nghiệp vụ chính

```
1. CB phòng chủ trì chọn hồ sơ đã tạo lập (từ UC89)
2. Nhấn nút "Bàn giao"
3. Hệ thống chuyển hồ sơ xuống danh sách "Hồ sơ đã bàn giao"
4. Cập nhật trạng thái hồ sơ
5. CB có thể kết xuất biên bản bàn giao
6. CB có thể in mẫu Biên bản bàn giao hồ sơ
```

---

## Phụ thuộc

| Use case | Mối quan hệ | Mô tả |
|----------|--------------|-------|
| UC89 | Kế thừa | Sử dụng hồ sơ đã tạo lập từ UC89 |

---

## Ghi chú triển khai

- Chức năng bàn giao đã được triển khai trong module `BanGiaoHoSo` (Version 1-3)
- Xem chi tiết tại `docs/feature/BanGiaoHoSo/task-ban-giao-ho-so version 3.md`
- Entity `BanGiaoHoSo` có hỗ trợ `PhongBanNhanId` cho phòng HC-TH nhận hồ sơ

---

## API Specification

### In Biên bản Bàn Giao Hồ Sơ

| Thông tin | Chi tiết |
|-----------|----------|
| **Endpoint** | `GET /api/print/bien-ban-ban-giao-ho-so` |
| **Controller** | `PrintController.cs` |
| **File Path** | `QLDA.WebApi/Controllers/PrintController.cs` |
| **Method** | `InBienBanBanGiaoHoSo` |
| **Template** | `PrintTemplates/Word/BienBanBanGiao.docx` |

#### Request Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | `Guid` | ✅ | BanGiaoHoSo Id |

#### Response

| Type | Description |
|------|-------------|
| `FileContentResult` | File `.docx` (application/vnd.openxmlformats-officedocument.wordprocessingml.document) |

#### Mail Merge Fields

| Field Name | Description |
|------------|-------------|
| `ngay` | Ngày bàn giao (định dạng: "ngày dd tháng MM năm yyyy") |
| `phongBanChuTri` | Tên phòng ban chủ trì |
| `phongBanNhan` | Tên phòng ban nhận hồ sơ |
| `maHoSo` | Mã hồ sơ |
| `tenHoSo` | Tên hồ sơ |
| `tenDuAn` | Tên dự án |
| `maLuuTru` | Mã lưu trữ (mã dự án) |
| `tongSoTepDinhKem` | Tổng số tệp đính kèm |
| `ghiChu` | Ghi chú |

#### Data Flow

```
Người dùng → GET /api/print/bien-ban-ban-giao-ho-so?id=xxx
         → PrintController.InBienBanBanGiaoHoSo()
         → BanGiaoHoSoPrintQuery (lấy dữ liệu)
         → WordHelper.ExportFromTemplate() (Aspose.Words MailMerge)
         → Trả về file .docx
```
