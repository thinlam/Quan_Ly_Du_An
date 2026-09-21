# Test Workflow — #180 loại `Chứng từ` cho Văn bản pháp lý

## Build

```bash
dotnet build SER.sln
```

Yêu cầu: 0 warning, 0 error.

## Kiểm thử thủ công (cần DB + server chạy)

### TC1 — Tạo mới loại `ChungTu`

```bash
curl -X POST "http://localhost:5000/api/van-ban-phap-ly/them-moi" \
  -H "Content-Type: application/json" \
  -d '{
    "duAnId": "<guid du an ton tai>",
    "buocId": 1,
    "loai": "ChungTu",
    "soVanBan": "CT-2026-001",
    "ngayVanBan": "2026-09-21T00:00:00+07:00",
    "coQuanQuyetDinh": "Công ty X",
    "trichYeu": "Chứng từ thanh toán đợt 1",
    "nguoiKy": "Nguyễn A",
    "ngayKy": "2026-09-21T00:00:00+07:00",
    "danhSachTepDinhKem": []
  }'
```

Kỳ vọng:
- HTTP 200, trả về `id` của bản ghi mới.
- DB: bảng `VanBanPhapLy` có dòng mới, cột `VanBanQuyetDinh.Loai = 'ChungTu'`
  (bảng cha TPT, cùng Id).

### TC2 — Không gửi `loai` (behavior cũ giữ nguyên)

Bỏ field `loai` khỏi payload. Kỳ vọng: `Loai = 'VanBanPhapLy'`.

### TC3 — Gửi `loai` giá trị khác (vd `"abc"`)

Kỳ vọng: vẫn lưu `Loai = 'VanBanPhapLy'` (whitelist fallback).

### TC4 — Cập nhật sang `ChungTu`

```bash
curl -X PUT "http://localhost:5000/api/van-ban-phap-ly/cap-nhat" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "<id o TC1>",
    "duAnId": "<guid>",
    "loai": "ChungTu",
    "soVanBan": "CT-2026-001",
    "trichYeu": "Chứng từ thanh toán đợt 1",
    "danhSachTepDinhKem": []
  }'
```

Kỳ vọng: `Loai` giữ = `'ChungTu'`. Update không gửi `loai` → `Loai` giữ nguyên giá trị cũ.

### TC5 — Danh sách tổng hợp hiển thị tên loại

```bash
curl "http://localhost:5000/api/tong-hop-van-ban-quyet-dinh/danh-sach-day-du?duAnId=<guid>"
```

Kỳ vọng: bản ghi tạo ở TC1 trả về `loai = "Chứng từ"` (từ `[Description]` của enum).

### TC6 — Enum list cho FE

```bash
curl "http://localhost:5000/api/danh-muc-enum/danh-sach?enumName=ELoaiVanBanQuyetDinh"
```

Kỳ vọng: có item `{ rawName: "ChungTu", ten: "Chứng từ" }`.

## Verify DB trực tiếp (SQL)

```sql
SELECT v.Id, v.Loai, v.So, v.TrichYeu
FROM VanBanQuyetDinh v
INNER JOIN VanBanPhapLy p ON p.Id = v.Id
WHERE v.Loai = 'ChungTu';
```

## Regression

- Tạo mới Văn bản pháp lý không gửi `loai` → vẫn `VanBanPhapLy` (TC2).
- Tạo mới loại khác (VD `KeHoachLuaChonNhaThau`) ở các API tương ứng → không đổi
  (không chạm tới các mapping khác).

## Lưu ý

- Không có migration mới cho issue này.
- Nếu sau này FE cần partial view riêng cho `Chứng từ`, bổ sung
  `LoaiVanBanQuyetDinhConst.Dictionary` (`ChungTu → "CHUNGTU"`) sau khi chốt tên view — ngoài scope hiện tại.