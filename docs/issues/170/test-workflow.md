# Test workflow — Issue #170 KHLCNT bổ sung field

## Điều kiện trước khi test API

1. Đã tạo + apply migration cột mới trên `KeHoachLuaChonNhaThau` (user tự làm).
2. Chạy WebApi (Swagger): thường `https://localhost:7130/swagger` hoặc profile trong `launchSettings.json`.
3. Có token user có quyền thao tác bước tiến độ KHLCNT của 1 dự án.
4. Dự án test đã có ≥1 nguồn vốn (`DanhSachNguonVon`) — CBB:

```http
GET /api/danh-muc-nguon-von/danh-sach?duAnId={duAnId}
```

## Endpoint

Base: `/api/ke-hoach-lua-chon-nha-thau`

| Flow | Method | Path |
|------|--------|------|
| Thêm mới | POST | `/them-moi` |
| Cập nhật | PUT | `/cap-nhat` |
| Chi tiết | GET | `/{id}/chi-tiet` |
| Danh sách | GET | `/danh-sach-tien-do?duAnId={duAnId}&buocId={buocId}` |

## Sample body — Thêm mới

```json
{
  "duAnId": "{guid-du-an}",
  "buocId": 0,
  "ten": "KHLCNT test #170",
  "soQuyetDinh": "TT-170-001",
  "ngayQuyetDinh": "2026-08-10T00:00:00+07:00",
  "trichYeu": "Test bổ sung field",
  "tongDuToan": 1500000000,
  "duToanThamDinh": 1400000000,
  "nguonVonId": 1,
  "thoiGianThucHien": 3,
  "danhSachTepDinhKem": []
}
```

`buocId` / `nguonVonId`: lấy đúng ID thực tế trên môi trường của bạn.

## Checklist

### Happy path

- [ ] **Thêm mới** đủ field → 200, có `id`
- [ ] **Chi tiết** → có `tongDuToan`, `duToanThamDinh`, `nguonVonId`, `thoiGianThucHien`, `soQuyetDinh`
- [ ] **Danh sách** (`duAnId`) → cùng các field trên
- [ ] **Cập nhật** đổi `tongDuToan` / `thoiGianThucHien` / `nguonVonId` → chi tiết phản ánh đúng
- [ ] `duToanThamDinh: null` (hoặc omit) → lưu null OK

### Validate / lỗi

- [ ] Thiếu `tongDuToan` (null / không gửi) → lỗi *"Tổng dự toán là bắt buộc"*
- [ ] `nguonVonId` không thuộc dự án → *"Nguồn vốn không thuộc dự án"*
- [ ] Trùng `soQuyetDinh` cùng dự án → *"Số tờ trình đã tồn tại"*

### CBB nguồn vốn

- [ ] `GET /api/danh-muc-nguon-von/danh-sach?duAnId=...` chỉ trả nguồn vốn của dự án đó
- [ ] Chọn 1 id từ CBB → insert/update OK

## Không có automated test sẵn

Module này chưa có test file trong `QLDA.Tests`. Hiện test bằng **Swagger / Postman** sau khi apply migration.

Compile-only (đã chạy lúc implement):

```bat
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```
