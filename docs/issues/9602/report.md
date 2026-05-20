# Issue #9602 — FE Mapping: Đổi tên Ngày dự kiến kết thúc, Bổ sung Ngày dự kiến kết thúc gói thầu

## API Endpoints

| Action | Method | Route | Body Type |
|--------|--------|-------|-----------|
| Thêm mới | `POST` | `/api/hop-dong` | `HopDongInsertDto` |
| Cập nhật | `PUT` | `/api/hop-dong` | `HopDongUpdateDto` |
| Chi tiết | `GET` | `/api/hop-dong/{id}/chi-tiet` | — |

Response cho cả 3 endpoint: `ResultApi<HopDongDto>`

---

## 1. Đổi tên trường Ngày dự kiến kết thúc

### Thay đổi
| API Field cũ | API Field mới | Type | Notes |
|--------------|---------------|------|-------|
| `NgayDuKienKetThuc` | `NgayDuKienKetThucHopDong` | `DateOnly?` | Ngày dự kiến kết thúc hợp đồng |

### Input Fields (Create/Update)
> Trên `HopDongInsertDto` / `HopDongUpdateDto`

| FE Field Label | API Field | Type | Required | Notes |
|----------------|-----------|------|----------|-------|
| Ngày dự kiến kết thúc hợp đồng | `NgayDuKienKetThucHopDong` | `DateOnly?` | No | Date picker, auto-fill từ Kết quả trúng thầu |

---

## 2. Bổ sung trường Ngày dự kiến kết thúc gói thầu

### Input Fields (Create/Update)
| FE Field Label | API Field | Type | Required | Notes |
|----------------|-----------|------|----------|-------|
| Ngày dự kiến kết thúc gói thầu | `NgayDuKienKetThucGoiThau` | `DateOnly?` | No | Date picker, auto-fill từ Kết quả trúng thầu |

---

## 3. Response Fields

> Trên `HopDongDto`

| API Field | Type | Notes |
|-----------|------|-------|
| `NgayHieuLuc` | `DateOnly?` | Ngày hiệu lực hợp đồng |
| `NgayDuKienKetThucHopDong` | `DateOnly?` | Ngày dự kiến kết thúc hợp đồng |
| `NgayDuKienKetThucGoiThau` | `DateOnly?` | Ngày dự kiến kết thúc gói thầu |

---

## 4. Auto-calculation Logic

### Nguồn dữ liệu
| Field | Entity | Source |
|-------|--------|--------|
| `SoNgayThucHienHopDong` | `KetQuaTrungThau` | Kết quả trúng thầu |
| `SoNgayTrienKhai` | `KetQuaTrungThau` | Kết quả trúng thầu |

### Công thức
```
NgayDuKienKetThucHopDong = NgayHieuLuc.AddDays(KetQuaTrungThau.SoNgayThucHienHopDong)
NgayDuKienKetThucGoiThau = NgayHieuLuc.AddDays(KetQuaTrungThau.SoNgayTrienKhai)
```

### Business Rules
1. Khi tạo/cập nhật Hợp đồng với `GoiThauId`:
   - Load `KetQuaTrungThau` qua quan hệ `GoiThau → KetQuaTrungThau`
   - Nếu `NgayHieuLuc` có giá trị và các trường ngày kết thúc null → tự động tính toán
2. **User override**: Nếu DTO truyền lên giá trị cụ thể → sử dụng giá trị user

### Example
- Kết quả trúng thầu: `SoNgayThucHienHopDong = 10`, `SoNgayTrienKhai = 10`
- Chọn ngày hiệu lực: `20/05/2026`
- → `NgayDuKienKetThucHopDong = 30/05/2026`
- → `NgayDuKienKetThucGoiThau = 30/05/2026`

---

## 5. DTO Structure

### HopDongInsertDto

```json
{
  "duAnId": "guid",
  "goiThauId": "guid",
  "ten": "Hợp đồng ABC",
  "soHopDong": "HD-1234/2026",
  "ngayKy": "2026-05-10T00:00:00+07:00",
  "ngayHieuLuc": "2026-05-20",
  "ngayDuKienKetThucHopDong": "2026-05-30",
  "ngayDuKienKetThucGoiThau": "2026-05-30",
  "giaTri": 5000000000,
  "loaiHopDongId": 1,
  "isBienBan": false
}
```

### HopDongDto (Response)

```json
{
  "id": "guid",
  "duAnId": "guid",
  "goiThauId": "guid",
  "ten": "Hợp đồng ABC",
  "soHopDong": "HD-1234/2026",
  "ngayKy": "2026-05-10T00:00:00+07:00",
  "ngayHieuLuc": "2026-05-20",
  "ngayDuKienKetThucHopDong": "2026-05-30",
  "ngayDuKienKetThucGoiThau": "2026-05-30",
  "giaTri": 5000000000,
  "loaiHopDongId": 1,
  "isBienBan": false
}
```

---

## 6. Tóm tắt thay đổi FE

| # | Thay đổi | Loại |
|---|----------|------|
| 1 | Đổi tên `NgayDuKienKetThuc` → `NgayDuKienKetThucHopDong` | **Rename** |
| 2 | Thêm field `NgayDuKienKetThucGoiThau` | **New field** |
| 3 | Auto-fill ngày kết thúc từ Kết quả trúng thầu khi chọn ngày hiệu lực | **Auto-calculation** |
| 4 | Date fields dùng `DateOnly` (yyyy-MM-dd format) thay vì `DateTimeOffset` | **Type change** |

### Lưu ý quan trọng
- `NgayHieuLuc`, `NgayDuKienKetThucHopDong`, `NgayDuKienKetThucGoiThau` đều là `DateOnly` (chỉ có ngày, không có giờ)
- Khi user chọn ngày hiệu lực → trigger auto-fill cho 2 trường ngày kết thúc
- User vẫn có thể chỉnh sửa manually các ngày kết thúc
- Relationship: `HopDong → GoiThau → KetQuaTrungThau` (SoNgayThucHienHopDong, SoNgayTrienKhai)