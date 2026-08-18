# Lỗi cập nhật Dự án — `PUT /api/du-an/cap-nhat` → "Quy trình không thể đổi"

## Tóm tắt

Khi **cập nhật Dự án** (`PUT du-an/cap-nhat`), nếu payload đổi `quyTrinhId` của một dự án **đã có tiến độ ghi nhận ở các bước (DuAnBuoc)** thì backend trả lỗi **"Quy trình không thể đổi"**, và phía FE lại nhìn thấy `500` / `"Response status code does not indicate success: 400 (Bad Request)."`.

Đây là **lỗi nghiệp vụ (business validation)** có chủ đích trong code, không phải lỗi hệ thống ngẫu nhiên.

---

## Actor / người dùng bị ảnh hưởng

- Người dùng có quyền **chỉnh sửa dự án** (Lãnh đạo phụ trách / Người tạo / Phòng ban phụ trách chính / Phòng ban phối hợp) thao tác sửa Dự án ở màn hình chi tiết dự án.

---

## Triệu chứng báo cáo

| # | Triệu chứng | Mức độ |
|---|-------------|--------|
| 1 | UI hiện `Request failed with status code 500` | Blocker |
| 2 | Network Response (body) lại là: `{ result: false, errorMessage: "Response status code does not indicate success: 400 (Bad Request).", dataResult: null, statusCode: 200 }` | — |
| 3 | Lỗi chỉ xảy ra khi thay đổi **Quy trình** của dự án đã có tiến độ | High |

---

## Payload ví dụ

```json
{
  "id": "08defc3c-2db8-a6d1-687a-7b252c02763d",
  "tenDuAn": "kiểm tra tiến độ",
  "quyTrinhId": 48,
  "diaDiem": "..."
}
```

> Lưu ý: Giá trị `quyTrinhId` (48 trong ví dụ / 46 trong log thật) **khác** với quy trình hiện tại của dự án ⇒ kích hoạt guard.

---

## Endpoint liên quan

| Endpoint | Method | Controller |
|----------|--------|------------|
| `PUT /api/du-an/cap-nhat` | `Update` | `QLDA.WebApi/Controllers/DuAnController.cs:302` |

---

## File/điểm lỗi chính (root cause)

- `QLDA.Application/DuAns/Commands/DuAnUpdateCommand.cs` — `DuAnUpdateCommandHandler.Handle`, dòng **51–54**
- Helper `HasDuAnBuocTienDoAsync`, dòng **114–127**

---

## Liên quan

- Middleware xử lý exception: `BuildingBlocks/src/BuildingBlocks.Application/Middlewares/ExceptionMiddleware.cs` (dòng 20–21)
- Log chứng cứ: `QLDA.WebApi/logs/service-20260817.log` (dòng 1574, 1615)
