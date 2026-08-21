# Test / verify — unique QĐ duyệt KHLCNT vs soft-delete

Không apply staging/prod từ máy dev.

## Kết quả đã chạy (2026-08-21, catalog `VI_DACDT`)

| Bước | Kết quả |
| --- | --- |
| Build `QLDA.Persistence` | OK |
| Duplicate active trước update | Rỗng (sau update cũng rỗng) |
| `ef.bat QLDA update` | Apply `20260821061550_FilterQuyetDinhDuyetKHLCNTUniqueIndexExcludeDeleted` |
| `sys.indexes.filter_definition` | `([IsDeleted]=(0))`, unique |
| `__EFMigrationsHistory` | Có migration trên |
| Dòng `732a4c8e-93af-4794-82f6-074615dadfc1` | 1 row, `IsDeleted = 1` |
| `POST them-moi` sau update | Chưa ghi nhận trong session này — retry trên API trỏ **cùng** catalog |

## 1. Build

```bat
dotnet build QLDA.Persistence/QLDA.Persistence.csproj
dotnet build QLDA.Migrator/QLDA.Migrator.csproj
```

## 2. Data check trước migrate (môi trường khác)

```sql
SELECT KeHoachLuaChonNhaThauId, COUNT(*) AS SoDongActive
FROM QuyetDinhDuyetKHLCNT
WHERE IsDeleted = 0 AND KeHoachLuaChonNhaThauId IS NOT NULL
GROUP BY KeHoachLuaChonNhaThauId
HAVING COUNT(*) > 1;
```

Kết quả rỗng mới `ef.bat QLDA update`.

## 3. Index + history sau migrate

```sql
SELECT i.name, i.is_unique, i.filter_definition
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID(N'dbo.QuyetDinhDuyetKHLCNT')
  AND i.name = N'IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId';

SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
WHERE MigrationId LIKE '%FilterQuyetDinhDuyetKHLCNTUniqueIndexExcludeDeleted%';
```

Kỳ vọng: `is_unique = 1`, filter chứa `[IsDeleted] = 0` (SQL Server có thể hiện `([IsDeleted]=(0))`). Có dòng history.

Chưa apply: filter vẫn `[KeHoachLuaChonNhaThauId] IS NOT NULL` hoặc history trống → API vẫn unique cũ.

## 4. Dòng chiếm slot (case screenshot)

```sql
SELECT Id, KeHoachLuaChonNhaThauId, IsDeleted, CreatedAt
FROM QuyetDinhDuyetKHLCNT
WHERE KeHoachLuaChonNhaThauId = '732a4c8e-93af-4794-82f6-074615dadfc1';
```

Sau fix: dòng xóa `IsDeleted = 1` **không** chặn insert.

## 5. Case API

Tiền điều kiện: kế hoạch đã có QĐ, user có quyền them-moi / xóa. API phải cùng DB đã migrate.

| # | Bước | Kỳ vọng |
| --- | --- | --- |
| 1 | `POST .../them-moi` lần 1 cho KH-A | 200, tạo QĐ |
| 2 | `POST .../them-moi` lần 2 cho KH-A (chưa xóa) | Fail unique — **đúng**, 1 QĐ active |
| 3 | Xóa QĐ (soft-delete) | 200, `IsDeleted = 1`, dòng còn trong bảng |
| 4 | `POST .../them-moi` lại cho KH-A | **200**, dòng mới `IsDeleted = 0` |
| 5 | List/chi-tiet | Chỉ QĐ mới |

Gợi ý retry: kế hoạch `732a4c8e-93af-4794-82f6-074615dadfc1` (QĐ cũ đã `IsDeleted = 1`).

## 6. Không pass nếu

- Bước 4 vẫn báo `IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId` → API trỏ DB khác, hoặc môi trường chưa update.
- Bước 2 thành công → unique bị bỏ hẳn (sai).
