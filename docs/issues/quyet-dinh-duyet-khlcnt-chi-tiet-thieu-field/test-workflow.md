# Hướng dẫn kiểm thử — API chi tiết Quyết định duyệt KHLCNT thiếu 5 field

## Build

```bash
dotnet build SER.sln
```

## Case test thủ công

Endpoint: `GET api/quyet-dinh-duyet-khlcnt/{id}/chi-tiet`

Case: `id = 08DEFCD8-E0D5-5A24-687A-7B2A14078F2D`

Kiểm tra response có đủ 5 field với giá trị thật lấy từ bảng `KeHoachLuaChonNhaThau`
(liên kết qua `KeHoachLuaChonNhaThauId`):

```json
{
  "id": "08DEFCD8-E0D5-5A24-687A-7B2A14078F2D",
  "tongDuToan": ...,
  "duToanThamDinh": ...,
  "nguonVonId": ...,
  "thoiGianThucHien": ...,
  "soLuongGoiThau": ...,
  "vanBanQuyetDinh": {...},
  "danhSachTepDinhKem": [...]
}
```

## Verification SQL (kiểm tra dữ liệu nguồn)

```sql
SELECT k.TongDuToan, k.DuToanThamDinh, k.NguonVonId, k.ThoiGianThucHien, k.SoLuongGoiThau
FROM QuyetDinhDuyetKHLCNT q
JOIN KeHoachLuaChonNhaThau k ON k.Id = q.KeHoachLuaChonNhaThauId
WHERE q.Id = '08DEFCD8-E0D5-5A24-687A-7B2A14078F2D';
```

Response API phải khớp kết quả truy vấn trên.

## Regression

- Endpoint `danh-sach-tien-do` (nếu có mở rộng phạm vi) trả đủ 5 field.
- `them-moi` / `cap-nhat` không đổi hành vi.
