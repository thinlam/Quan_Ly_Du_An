# Test workflow — `du-an/cap-nhat` "Quy trình không thể đổi"

Hướng dẫn tái hiện, xác nhận lỗi và kiểm thử sau khi sửa.

---

## 1. Chuẩn bị

- Chạy `QLDA.WebApi` (cấu hình DB `VI_DACDT` như log: server `192.168.1.13\sql2k16r2, 1439` — **chỉ dùng môi trường test/dev, không chạy migration lên prod**).
- Có token hợp lệ của user có quyền sửa dự án.
- Chọn 1 dự án **đã có tiến độ** ở các bước (kiểm tra `DuAnBuoc` có `NgayDuKien*` / `TrangThaiId` / `GhiChu`...).

## 2. Tái hiện lỗi (trước fix)

```http
PUT /api/du-an/cap-nhat
Content-Type: application/json
Authorization: Bearer <token>

{
  "id": "<duAnId đã có tiến độ>",
  "tenDuAn": "test đổi quy trình",
  "quyTrinhId": "<một quy trình KHÁC quy trình hiện tại>",
  "diaDiem": "test"
}
```

**Kỳ vọng (bug):**
- Backend log: `ERR ... custom message: Quy trình không thể đổi`
- FE / proxy: `500` + `"Response status code does not indicate success: 400 (Bad Request)."`
- (Nếu gọi thẳng QLDA không qua proxy) response body: `{ result:false, errorMessage:"Quy trình không thể đổi", statusCode:200 }`

## 3. Ca kiểm thử sau fix

| # | Kịch bản | Payload | Kỳ vọng |
|---|----------|---------|---------|
| 1 | Đổi quy trình của dự án **có tiến độ** | `quyTrinhId` khác | Tuỳ phương án: chặn với message rõ ràng (A) hoặc đổi + reset tiến độ (B) |
| 2 | Sửa dự án **giữ nguyên quy trình** | `quyTrinhId` = quy trình hiện tại | ✅ Thành công, không lỗi |
| 3 | Sửa dự án **chưa có tiến độ** | `quyTrinhId` khác | ✅ Được phép đổi, `DuAnBuoc` clone lại |
| 4 | Sửa dự án **không gửi `quyTrinhId`** | thiếu field | ✅ Thành công (field optional) |
| 5 | `id` không tồn tại | `id` = random | `ManagedException "Không tìm thấy dữ liệu"` (HTTP 200) |
| 6 | Không có quyền sửa | token user không thuộc phòng | `ForbiddenException` (HTTP 200 / body 403) |

## 4. Xác nhận fix

1. Chạy lại các ca 1–6 ở trên.
2. Kiểm tra body trả về từ FE phải hiển thị message thật (`"Quy trình không thể đổi"` hoặc message mới theo nghiệp vụ), **không còn** `"Response status code does not indicate success: 400"`.
3. Nếu sửa proxy (Phương án C): đảm bảo HTTP status + body forward đúng.

## 5. Chạy test hiện có

```powershell
# từ thư mục gốc repo
.\test.bat
```

Kiểm tra không vỡ các test liên quan Dự án / DuAnBuoc clone (xem `QLDA.Tests/Integration`).
