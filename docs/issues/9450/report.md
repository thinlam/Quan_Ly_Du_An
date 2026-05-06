# Issue #9450 — Dashboard giải ngân theo nguồn vốn

## Yêu cầu

Dashboard tình hình thực hiện giải ngân theo nguồn vốn:
1. Hiển thị tổng hợp kế hoạch vốn năm theo từng nguồn vốn
2. Xem tổng giá trị đã thực hiện (giải ngân) trong năm theo từng nguồn vốn
3. Xem tỷ lệ % so với kế hoạch theo từng nguồn vốn
4. Xem danh sách từ số tổng
5. Xem chi tiết

## Giải pháp

### Endpoint mới

```
GET /api/thong-ke/giai-ngan-theo-nguon-von?duAnId={guid}
```

### Files tạo mới

| File | Mô tả |
|------|-------|
| `QLDA.Domain/DTOs/DashboardGiaiNganDto.cs` | DTO: NguonVonId, TenNguonVon, GiaTriGiaiNgan, GiaTriHopDong |
| `QLDA.Application/Dashboard/Queries/DashboardGetGiaiNganTheoNguonVonQuery.cs` | Query + Handler dùng Dapper raw SQL |

### Files sửa đổi

| File | Thay đổi |
|------|----------|
| `QLDA.WebApi/Controllers/DashboardController.cs` | Thêm endpoint `GetGiaiNganTheoNguonVon` |

### SQL Query

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
AND gt.DuAnId = @DuAnId
GROUP BY gt.NguonVonId, nv.Ten
```

### Response mẫu

```json
{
  "isSuccess": true,
  "data": [
    {
      "nguonVonId": 1,
      "tenNguonVon": "Ngân sách nhà nước",
      "giaTriGiaiNgan": 5000000000,
      "giaTriHopDong": 8000000000
    }
  ]
}
```

## Trạng thái

- [x] Endpoint API hoàn thành
- [x] Build thành công (0 errors, 0 warnings)
- [ ] FE tích hợp
- [x] Test trên staging
