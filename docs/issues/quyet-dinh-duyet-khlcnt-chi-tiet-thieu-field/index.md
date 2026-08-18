# API chi tiết Quyết định duyệt KHLCNT thiếu 5 field

## Tóm tắt vấn đề

Endpoint **GET `api/quyet-dinh-duyet-khlcnt/{id}/chi-tiet`** hiện chưa trả về 5 field:

| Field            | Kiểu   | Ý nghĩa                          |
| ---------------- | ------ | -------------------------------- |
| `tongDuToan`     | long   | Tổng dự toán                     |
| `duToanThamDinh` | long?  | Dự toán thẩm định                |
| `nguonVonId`     | int?   | Nguồn vốn (theo nguồn vốn dự án) |
| `thoiGianThucHien`| int?  | Thời gian thực hiện (năm)        |
| `soLuongGoiThau` | int?   | Số lượng gói thầu                |

## Case test

- Endpoint: `api/quyet-dinh-duyet-khlcnt/chi-tiet`
- `id = 08DEFCD8-E0D5-5A24-687A-7B2A14078F2D`

Kết quả mong đợi (giá trị mẫu để kiểm tra, không hard-code trong code):

```json
{
  "tongDuToan": 1234567,
  "duToanThamDinh": 1234567,
  "nguonVonId": 16,
  "thoiGianThucHien": 1,
  "soLuongGoiThau": 1
}
```

## Phân tích nguyên nhân gốc

5 field trên **không nằm trên entity** `QuyetDinhDuyetKHLCNT` mà nằm trên entity liên kết
**`KeHoachLuaChonNhaThau`**, được trỏ qua `QuyetDinhDuyetKHLCNT.KeHoachLuaChonNhaThauId` (quan hệ 1-1).

| Tầng            | File                                                                      | Trạng thái                                              |
| --------------- | ------------------------------------------------------------------------- | ------------------------------------------------------- |
| Entity          | `QLDA.Domain/Entities/KeHoachLuaChonNhaThau.cs` (dòng 20-40)              | ✅ Đã có 5 field                                         |
| EF Config / DB  | Bảng `KeHoachLuaChonNhaThau`                                              | ✅ Đã có cột (entity + DB sẵn sàng, không cần migration) |
| Query Handler   | `QLDA.Application/.../Queries/QuyetDinhDuyetKHLCNTGetQuery.cs` (dòng 22)  | ❌ Không `Include(e => e.KeHoachLuaChonNhaThau)`         |
| Response Model  | `QLDA.WebApi/Models/.../QuyetDinhDuyetKHLCNTModel.cs`                     | ❌ Không có 5 property                                   |
| Mapping ToModel | `QLDA.WebApi/Models/.../QuyetDinhDuyetKHLCNTMappingConfiguration.cs`      | ❌ Không map 5 field                                     |

### Flow hiện tại

1. `QuyetDinhDuyetKHLCNTController.Get` (line 29) gọi `QuyetDinhDuyetKHLCNTGetQuery`.
2. `QuyetDinhDuyetKHLCNTGetQueryHandler` (line 22) chỉ `.Include(e => e.VanBanQuyetDinh)`
   → `entity.KeHoachLuaChonNhaThau` trả về `null`.
3. `QuyetDinhDuyetKHLCNTMappingConfiguration.ToModel` (line 8-28) chỉ map
   `Id, KeHoachLuaChonNhaThauId, VanBanQuyetDinh, DanhSachTepDinhKem` → không map 5 field
   (cũng không có gì để map vì navigation đang null).

### Vì sao "đúng nguyên nhân"

- Entity `KeHoachLuaChonNhaThau` và DB đã có đủ dữ liệu (đây là dữ liệu thật của KHLCNT, không phải tạo mới).
- 5 field chỉ đơn thuần **chưa được Include / khai báo / map** trong luồng chi tiết.
- Không cần migration, không cần model mới, không hard-code.

## Kế hoạch sửa (chỉ tác động endpoint chi tiết)

1. **`QuyetDinhDuyetKHLCNTGetQuery.cs`** — thêm `.Include(e => e.KeHoachLuaChonNhaThau)`.
2. **`QuyetDinhDuyetKHLCNTModel.cs`** — thêm 5 property khớp kiểu entity.
3. **`QuyetDinhDuyetKHLCNTMappingConfiguration.ToModel`** — map 5 field từ `entity.KeHoachLuaChonNhaThau`.

## Phạm vi ngoài chi tiết (đã rà soát)

- **`danh-sach-tien-do`** (`QuyetDinhDuyetKHLCNTGetDanhSachQuery` + `QuyetDinhDuyetKHLCNTDto`) — **cũng thiếu** 5 field
  (DTO không có property, projection không select, không Include). Đây là phạm vi mở rộng tùy chọn.
- **`them-moi` / `cap-nhat`** — không bị ảnh hưởng: 5 field là dữ liệu chỉ-đọc đến từ KHLCNT, không nằm trong write-path
  của Quyết định duyệt KHLCNT.

## Ràng buộc (không làm)

- Không tạo migration (DB đã có cột).
- Không tạo model mới trong `QLDA.WebApi`.
- Không refactor lan sang endpoint khác (trừ khi được xác nhận mở rộng).
- Không hard-code giá trị mẫu.

## Tài liệu liên quan

- Issue gốc: mô tả ở trên.
- Luồng chuẩn tham chiếu (module KHLCNT trả đủ field): `KeHoachLuaChonNhaThauMappings.cs`, `KeHoachLuaChonNhaThauGetDanhSachQuery.cs`.
