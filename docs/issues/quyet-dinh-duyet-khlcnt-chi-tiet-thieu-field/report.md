# Báo cáo triển khai — API chi tiết Quyết định duyệt KHLCNT thiếu 5 field

## Tóm tắt

Bổ sung 5 field `TongDuToan, DuToanThamDinh, NguonVonId, ThoiGianThucHien, SoLuongGoiThau`
cho endpoint **GET `api/quyet-dinh-duyet-khlcnt/{id}/chi-tiet`**. Các field này đã tồn tại trên
entity `KeHoachLuaChonNhaThau` và DB; chỉ thiếu ở tầng Include + Model + Mapping của endpoint chi tiết.

## Nguyên nhân

`QuyetDinhDuyetKHLCNT` không chứa 5 field; chúng nằm trên entity liên kết `KeHoachLuaChonNhaThau`
(1-1 qua `KeHoachLuaChonNhaThauId`). Luồng chi tiết:
- Query Handler không `Include(KeHoachLuaChonNhaThau)` → navigation null.
- Model không khai báo 5 property.
- Mapping `ToModel` không map.

## Kiến trúc / Cách tiếp cận

Clean Architecture + CQRS. Thay đổi thuộc đúng lớp:
- **Application**: Query Handler bổ sung `Include` (đọc dữ liệu).
- **WebApi**: Model khai báo property, Mapping `ToModel` map từ navigation.

Không thêm `Application/Services`, không đưa business logic vào Controller, không migration
(vì DB đã có cột trong bảng `KeHoachLuaChonNhaThau`).

## Thay đổi code

| # | File | Nội dung |
| - | ---- | -------- |
| 1 | `QLDA.Application/QuyetDinhDuyetKHLCNTs/Queries/QuyetDinhDuyetKHLCNTGetQuery.cs` | Thêm `.Include(e => e.KeHoachLuaChonNhaThau)` |
| 2 | `QLDA.WebApi/Models/QuyetDinhDuyetKHLCNTs/QuyetDinhDuyetKHLCNTModel.cs` | Thêm 5 property |
| 3 | `QLDA.WebApi/Models/QuyetDinhDuyetKHLCNTs/QuyetDinhDuyetKHLCNTMappingConfiguration.cs` | `ToModel` map 5 field từ `entity.KeHoachLuaChonNhaThau` |

## Trạng thái

- [ ] Đang triển khai / chờ duyệt phạm vi
- [ ] Build thành công
- [ ] Test case `08DEFCD8-E0D5-5A24-687A-7B2A14078F2D` trả đủ 5 field
- [ ] Đã kiểm tra regression

## PR

(chưa tạo)
