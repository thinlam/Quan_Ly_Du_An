# Issue 180 — Bổ sung loại `Chứng từ` cho API thêm mới Văn bản pháp lý

## 1. Mô tả nghiệp vụ

API tạo mới **Văn bản pháp lý** cần bổ sung thêm **1 loại mới: `Chứng từ`** (`loai = ChungTu`).

```http
POST /van-ban-phap-ly/them-moi
```

Yêu cầu:

- API có thể tạo Văn bản pháp lý thuộc loại **Chứng từ**.
- Không thay đổi behavior của các loại Văn bản pháp lý đang có (mặc định `VanBanPhapLy`).

## 2. Tác nhân / UI

- Người dùng lập hồ sơ Văn bản pháp lý trên FE; chọn loại **Chứng từ** khi tạo/cập nhật.
- Danh sách loại hiển thị lấy từ enum-list API:
  `GET api/danh-muc-enum/danh-sach?enumName=ELoaiVanBanQuyetDinh`
  (tự động include `ChungTu` khi thêm enum member).

## 3. API liên quan

| Endpoint | Method | Ghi chú |
|----------|--------|---------|
| `api/van-ban-phap-ly/them-moi` | POST | Tạo mới — nhận `loai: "ChungTu"` |
| `api/van-ban-phap-ly/cap-nhat` | PUT | Cập nhật — giữ nguyên `Loai` nếu không gửi; đổi sang `ChungTu` nếu gửi |
| `api/van-ban-phap-ly/{id}/chi-tiet` | GET | Chi tiết (không trả `Loai` — không đổi) |
| `api/van-ban-phap-ly/danh-sach-tien-do` | GET | Danh sách tiến độ (không trả `Loai` — không đổi) |
| `api/tong-hop-van-ban-quyet-dinh/danh-sach-day-du` | GET | Trả `loai` dạng tên hiển thị — tự hiển thị **"Chứng từ"** |
| `api/danh-muc-enum/danh-sach?enumName=ELoaiVanBanQuyetDinh` | GET | Danh sách loại cho FE — tự include `ChungTu` |

## 4. Request mẫu

```json
POST /api/van-ban-phap-ly/them-moi
{
  "duAnId": "<guid>",
  "buocId": 1,
  "loai": "ChungTu",
  "soVanBan": "CT-2026-001",
  "ngayVanBan": "2026-09-21T00:00:00+07:00",
  "coQuanQuyetDinh": "Công ty X",
  "trichYeu": "Chứng từ thanh toán đợt 1",
  "nguoiKy": "Nguyễn A",
  "ngayKy": "2026-09-21T00:00:00+07:00",
  "danhSachTepDinhKem": []
}
```

## 5. Tài liệu liên quan trong issue này

- [`report.md`](./report.md) — Báo cáo khảo sát source hiện tại, các file đã sửa, lý do không cần migration.
- [`journal.md`](./journal.md) — Nhật ký công việc theo ngày.
- [`test-workflow.md`](./test-workflow.md) — Kế hoạch kiểm thử và cách verify.

## 6. Trạng thái hiện tại

**Đã implement** — `dotnet build SER.sln` 0 lỗi. Chi tiết: `report.md`.