# Test / verify — `SoLuongGoiThau` trên QĐ duyệt KHLCNT

Chạy sau khi entity + migration **mới** đã apply DB **dev**. Không drop DB. Không update staging/prod từ đây.

## 1. Snapshot / migration review (trước update)

- Entity `KeHoachLuaChonNhaThau` trong snapshot **không** còn `SoLuongGoiThau`.
- Entity `QuyetDinhDuyetKHLCNT` **có** `b.Property<int?>("SoLuongGoiThau")`.
- `Up()`: add cột bảng `QuyetDinhDuyetKHLCNT`, drop cột bảng `KeHoachLuaChonNhaThau`.
- Không sửa file `20260812044306_AddKeHoachLuaChonNhaThauSoLuongGoiThau`.

## 2. SQL sau update

```sql
-- Cột trên QĐ
SELECT c.name, t.name AS type_name, c.is_nullable
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'dbo.QuyetDinhDuyetKHLCNT')
  AND c.name = N'SoLuongGoiThau';

-- Không còn trên KHLCNT
SELECT c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.KeHoachLuaChonNhaThau')
  AND c.name = N'SoLuongGoiThau';
```

Kỳ vọng: query 1 có 1 dòng `int` nullable; query 2 rỗng.

## 3. API

| # | Gọi | Kỳ vọng |
| --- | --- | --- |
| 1 | `POST/PUT api/ke-hoach-lua-chon-nha-thau` gửi `soLuongGoiThau` | Bỏ qua / không persist KHLCNT |
| 2 | GET chi tiết KHLCNT | Response **không** `soLuongGoiThau`; vẫn 4 field `tongDuToan`, `duToanThamDinh`, `nguonVonId`, `thoiGianThucHien` |
| 3 | `POST api/quyet-dinh-duyet-khlcnt/them-moi` kèm `soLuongGoiThau` | Lưu bảng `QuyetDinhDuyetKHLCNT` |
| 4 | `GET api/quyet-dinh-duyet-khlcnt/{id}/chi-tiet` | `soLuongGoiThau` từ QĐ; 4 field kia vẫn từ kế hoạch |
| 5 | `PUT .../cap-nhat` đổi `soLuongGoiThau` | Cập nhật cột QĐ, không ghi KHLCNT |

## 4. Không pass nếu

- Chi tiết QĐ vẫn lấy số từ kế hoạch (entity QĐ null, KHLCNT còn cột).
- Snapshot vẫn `SoLuongGoiThau` dưới `KeHoachLuaChonNhaThau`.
- 4 cột KHLCNT bị xóa nhầm.
