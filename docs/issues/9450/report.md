# Issue #9450 — Dashboard giải ngân theo nguồn vốn & theo năm

## Yêu cầu

Dashboard tình hình thực hiện giải ngân theo nguồn vốn (UC7):
1. Hiển thị tổng hợp kế hoạch vốn năm theo từng nguồn vốn
2. Xem tổng giá trị đã thực hiện (giải ngân) trong năm theo từng nguồn vốn
3. Xem tỷ lệ % so với kế hoạch theo từng nguồn vốn
4. Xem danh sách từ số tổng
5. Xem chi tiết

## Status: MERGED via PR #53, #58, #60 (2026-05-08)

## Giải pháp

### API Endpoints

#### 1. Giải ngân theo nguồn vốn (tổng hợp)

```
GET /api/thong-ke/giai-ngan-theo-nguon-von?nam={year}
```

Trả về `List<DashboardGiaiNganTheoNguonVonDto>` — tổng hợp giải ngân + giá trị hợp đồng theo nguồn vốn, filter theo năm.

#### 2. Chi tiết giải ngân

```
GET /api/thong-ke/chi-tiet-giai-ngan?nam={year}&nguonVonId={id}
```

Trả về `List<DashboardChiTietGiaiNganDto>` — chi tiết từng bản ghi giải ngân, filter theo năm + nguồn vốn (optional).

#### 3. Tiến độ giải ngân (biểu đồ)

```
GET /api/thong-ke/tien-do-giai-ngan-nguon-von?nam={year}&nguonVonId={id}&loaiDuAnId={id}&loaiDuAnTheoNamId={id}
```

Trả về `List<TinhHinhGiaiNganDto>` — dữ liệu biểu đồ giải ngân theo tháng, group theo nguồn vốn / loại dự án / loại dự án theo năm.

### Files tạo mới

| File | Mô tả |
|------|-------|
| `QLDA.Domain/DTOs/DashboardGiaiNganDto.cs` | DTO: `DashboardGiaiNganTheoNguonVonDto` (NguonVonId, TenNguonVon, GiaTriGiaiNgan, GiaTriHopDong), `DashboardChiTietGiaiNganDto` (TenDuAn, GiaTriGiaiNgan, GiaTriHopDong, Ngay, TrangThaiGiaiNgan) |
| `QLDA.Domain/DTOs/TinhHinhGiaiNganDto.cs` | DTO: LoaiDuAnId, LoaiDuAnTheoNamId, NguonVonId, GiaTriHopDong, GiaTriGiaiNgan, Thang, Nam |
| `QLDA.Domain/DTOs/TinhHinhGiaiNganSearchDto.cs` | Search DTO: Nam, NguonVonId, LoaiDuAnTheoNamId, LoaiDuAnId |
| `QLDA.Application/Dashboard/Queries/DashboardGetGiaiNganTheoNguonVonQuery.cs` | Query + Handler — Dapper raw SQL, filter by `YEAR(NgayHoaDon) = @Nam` |
| `QLDA.Application/Dashboard/Queries/DashboardGetChiTietGiaiNganQuery.cs` | Query + Handler — Dapper raw SQL, filter by năm + optional NguonVonId |
| `QLDA.Application/Dashboard/Queries/DashboardTienDoGiaiNganNguonVonQuery.cs` | Query + Handler — Dapper raw SQL, group by tháng/năm/nguồn vốn/loại dự án |

### Files sửa đổi

| File | Thay đổi |
|------|----------|
| `QLDA.WebApi/Controllers/DashboardController.cs` | Thêm 3 endpoints: `GetGiaiNganTheoNguonVon`, `GetChiTietGiaiNgan`, `TienDoGiaiNganNguonVon` |

### SQL Queries

#### Giải ngân theo nguồn vốn (tổng hợp)
```sql
SELECT gt.NguonVonId, nv.Ten AS TenNguonVon,
    SUM(tt.GiaTri) AS GiaTriGiaiNgan,
    SUM(hd.GiaTri) AS GiaTriHopDong
FROM dbo.ThanhToan tt
JOIN dbo.NghiemThu nt ON nt.Id = tt.NghiemThuId
JOIN dbo.HopDong hd ON hd.Id = nt.HopDongId
JOIN dbo.GoiThau gt ON gt.Id = hd.GoiThauId
JOIN dbo.DmNguonVon nv ON nv.Id = gt.NguonVonId
WHERE tt.IsDeleted = 0 AND hd.IsDeleted = 0
AND YEAR(tt.NgayHoaDon) = @Nam
GROUP BY gt.NguonVonId, nv.Ten
```

#### Chi tiết giải ngân
```sql
SELECT da.TenDuAn, tt.GiaTri AS GiaTriGiaiNgan, hd.GiaTri AS GiaTriHopDong,
    tt.NgayHoaDon AS Ngay,
    CASE WHEN tt.GiaTri > 0 THEN N'Đã giải ngân' ELSE N'Chưa giải ngân' END AS TrangThaiGiaiNgan
FROM dbo.ThanhToan tt
JOIN dbo.NghiemThu nt ON nt.Id = tt.NghiemThuId
JOIN dbo.HopDong hd ON hd.Id = nt.HopDongId
JOIN dbo.GoiThau gt ON gt.Id = hd.GoiThauId
JOIN dbo.DuAn da ON da.Id = gt.DuAnId
WHERE tt.IsDeleted = 0 AND hd.IsDeleted = 0
AND YEAR(tt.NgayHoaDon) = @Nam
-- optional: AND gt.NguonVonId = @NguonVonId
```

#### Tiến độ giải ngân (biểu đồ)
```sql
SELECT (SUM(tt.GiaTri)/1000000) AS GiaTriGiaiNgan,
    gt.NguonVonId, d.LoaiDuAnId, d.LoaiDuAnTheoNamId,
    YEAR(tt.NgayHoaDon) as Nam, MONTH(tt.NgayHoaDon) AS Thang
FROM dbo.ThanhToan tt
JOIN dbo.NghiemThu nt ON nt.Id = tt.NghiemThuId
JOIN dbo.HopDong hd ON hd.Id = nt.HopDongId
JOIN dbo.GoiThau gt ON gt.Id = hd.GoiThauId
JOIN dbo.DuAn d ON d.Id = gt.DuAnId
WHERE tt.IsDeleted = 0 AND hd.IsDeleted = 0
    AND (@LoaiDuAnId IS NULL OR d.LoaiDuAnId = @LoaiDuAnId)
    AND (@LoaiDuAnTheoNamId IS NULL OR d.LoaiDuAnTheoNamId = @LoaiDuAnTheoNamId)
    AND (@NguonVonId IS NULL OR gt.NguonVonId = @NguonVonId)
    AND tt.NgayHoaDon >= @FirstDayOfYear AND tt.NgayHoaDon < @FirstDayOfNextYear
GROUP BY gt.NguonVonId, d.LoaiDuAnTheoNamId, d.LoaiDuAnId, MONTH(tt.NgayHoaDon), YEAR(tt.NgayHoaDon)
```

### Response mẫu

**Tổng hợp:**
```json
{
  "isSuccess": true,
  "data": [
    { "nguonVonId": 1, "tenNguonVon": "Ngân sách nhà nước", "giaTriGiaiNgan": 5000000000, "giaTriHopDong": 8000000000 }
  ]
}
```

**Chi tiết:**
```json
{
  "isSuccess": true,
  "data": [
    { "tenDuAn": "DA001", "giaTriGiaiNgan": 1000000000, "giaTriHopDong": 2000000000, "ngay": "2026-03-15", "trangThaiGiaiNgan": "Đã giải ngân" }
  ]
}
```

## PR History

| PR | Date | Description |
|----|------|-------------|
| #53 | 2026-05-06 | Endpoint giai-ngan-theo-nguon-von ban đầu (filter by duAnId) |
| #58 | 2026-05-08 | Đổi filter theo năm, thêm API chi tiết giải ngân |
| #60 | 2026-05-08 | Thêm filter NguonVonId cho API chi tiết giải ngân |

## Evolución del filtro

| Version | Filter | Endpoint |
|---------|--------|----------|
| PR #53 | `duAnId` (Guid) | `giai-ngan-theo-nguon-von?duAnId={guid}` |
| PR #58 | `nam` (int) | `giai-ngan-theo-nguon-von?nam={year}` + `chi-tiet-giai-ngan?nam={year}` |
| PR #60 | `nam` + optional `nguonVonId` | `chi-tiet-giai-ngan?nam={year}&nguonVonId={id}` |

## Checklist

- [x] Endpoint tổng hợp giải ngân theo nguồn vốn (filter by năm)
- [x] Endpoint chi tiết giải ngân (filter by năm + optional nguồn vốn)
- [x] Endpoint tiến độ giải ngân biểu đồ (group by tháng, filter multi)
- [x] Build thành công (0 errors, 0 warnings)
- [x] MERGED vào main
- [ ] FE tích hợp
