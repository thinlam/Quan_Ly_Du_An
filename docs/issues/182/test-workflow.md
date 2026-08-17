# Kế hoạch kiểm thử — Issue 182

> Đã code. Verify: `dotnet build SER.sln` + `dotnet test --filter DuAnControllerTests`. Không migration.

## 1. Build

```powershell
dotnet build SER.sln
```

Kỳ vọng: 0 Error.

## 2. Điều kiện “đã nhập tiến độ”

Một `DuAnBuoc` của dự án thỏa **bất kỳ** điều kiện:

- `NgayDuKienBatDau` / `NgayDuKienKetThuc` / `NgayThucTeBatDau` / `NgayThucTeKetThuc` ≠ null
- `TrangThaiId` ≠ null
- `GhiChu` hoặc `TrachNhiemThucHien` không rỗng
- `IsKetThuc == true`

Không dùng `PhongPhuTrachChinhId`.

## 3. Test case

Chuẩn bị: 2 quy trình A/B (`DanhMucQuyTrinh` + `DanhMucBuoc`). Dự án có `DuAnBuoc` sau `them-moi`. API `PUT api/du-an/cap-nhat`.

| ID | Input | Kỳ vọng |
|---|---|---|
| **T1** | Không đổi `QuyTrinhId` + chưa nhập tiến độ | Không clone. Số bước / `BuocId` / ngày giữ nguyên. |
| **T2** | Không đổi `QuyTrinhId` + ≥ 1 bước đã nhập tiến độ (vd. `NgayThucTeBatDau`) | Không clone. Tiến độ còn nguyên. |
| **T3** | Đổi `QuyTrinhId` A→B + mọi bước chưa tiến độ | Clone theo QT B. Bước mới theo `DanhMucBuoc` của B. |
| **T4** | Đổi `QuyTrinhId` A→B + ≥ 1 bước đã nhập tiến độ | Không clone/reset. Tiến độ + tập `BuocId` cũ còn nguyên. |
| **T5** | Chỉ đổi `GhiChu` / `LanhDaoPhuTrachId` / `DonViPhuTrachChinhId` (không đổi QT) | `DuAnBuoc` không đổi (regression). |

## 4. Gợi ý kiểm tra DB sau T4

```sql
-- số bước và tiến độ không đổi so với trước PUT
SELECT Id, BuocId, NgayDuKienBatDau, NgayDuKienKetThuc, NgayThucTeBatDau, TrangThaiId, IsKetThuc
FROM DuAnBuoc
WHERE DuAnId = @id AND IsDeleted = 0;
```

## 5. Regression thêm mới

`POST api/du-an/them-moi` vẫn clone + `DuAnBuocMapPhongBanCommand`. Không đổi hành vi.
