# Kế hoạch kiểm thử — Issue 182

> Phase 2 **đã code.** Verify: build + filter test dưới. Không migration.

## 1. Build

```powershell
dotnet build SER.sln
dotnet test QLDA.Tests/QLDA.Tests.csproj --filter "FullyQualifiedName~QLDA.Tests.Integration.DuAnControllerTests.Update_SameQuyTrinh|FullyQualifiedName~QLDA.Tests.Integration.DuAnControllerTests.Update_Change"
```

Kỳ vọng: 0 Error; T1–T5 (T4 đổi nghĩa — xem bảng).

## 2. Điều kiện “đã nhập tiến độ”

Một `DuAnBuoc` của dự án thỏa **bất kỳ** điều kiện:

- `NgayDuKienBatDau` / `NgayDuKienKetThuc` / `NgayThucTeBatDau` / `NgayThucTeKetThuc` ≠ null
- `TrangThaiId` ≠ null
- `GhiChu` hoặc `TrachNhiemThucHien` không rỗng
- `IsKetThuc == true`

Không dùng `PhongPhuTrachChinhId`. Không dùng “chỉ cần có dòng `DuAnBuoc`”.

## 3. Test case

Chuẩn bị: 2 quy trình A/B. API `PUT api/du-an/cap-nhat`.

`ManagedException` → HTTP **200**, body `result: false`, `errorMessage` như dưới (pattern hiện tại, không phải HTTP 400).

| ID | Input | Kỳ vọng |
|---|---|---|
| **T1** | Không đổi `QuyTrinhId` + chưa nhập tiến độ | Update OK. `DuAnBuoc` giữ nguyên. |
| **T2** | Không đổi `QuyTrinhId` + ≥ 1 bước đã nhập tiến độ | Update OK. Tiến độ còn. `QuyTrinhId` vẫn A. |
| **T3** | Đổi A→B + mọi bước chưa tiến độ | Update OK. `DuAn.QuyTrinhId = B`. Clone bước theo QT B. |
| **T4** | Đổi A→B + ≥ 1 bước đã nhập tiến độ | **Reject.** `errorMessage = "Quy trình không thể đổi"`. `DuAn.QuyTrinhId` vẫn **A**. `DuAnBuoc` không clone/reset. Field khác của payload (vd. `GhiChu`) **không** lưu. |
| **T5** | Chỉ đổi `GhiChu` / lãnh đạo / phòng, không đổi QT | Update OK. `DuAnBuoc` không đổi. |

## 4. SQL sau T4 (reject)

```sql
SELECT QuyTrinhId, GhiChu FROM DuAn
WHERE Id = CAST('...' AS uniqueidentifier);

SELECT Id, BuocId, NgayThucTeBatDau, TrangThaiId, IsKetThuc
FROM DuAnBuoc
WHERE DuAnId = CAST('...' AS uniqueidentifier) AND IsDeleted = 0
ORDER BY Id;
```

`QuyTrinhId` phải còn A. Snapshot bước = trước PUT. `GhiChu` dự án không đổi nếu payload có ghi chú mới.

## 5. Regression thêm mới

`POST api/du-an/them-moi` vẫn clone + `DuAnBuocMapPhongBanCommand`.
