# Issue: `ef.bat QLDA update` fail — duplicate column `ThamDinh`

**Ngày ghi nhận:** 2026-06-26  
**Liên quan task:** #110 — `NguoiDungMacDinhTheoPhong`  
**Ràng buộc xử lý:** Không drop database. Không sửa file migration (`.cs` trong `QLDA.Migrator/Migrations/`).

---

## 1. Lỗi là gì?

Khi chạy `ef.bat QLDA update`, EF Core dừng với:

```txt
Applying migration '20260625063211_UpdateHinhThucLCNT'.
ALTER TABLE [HoSoMoiThauDienTu] ADD [ThamDinh] bit NULL;

SqlException (2705):
Column names in each table must be unique.
Column name 'ThamDinh' in table 'HoSoMoiThauDienTu' is specified more than once.
```

**Diễn giải ngắn:** SQL Server từ chối thêm cột `ThamDinh` vì bảng `HoSoMoiThauDienTu` **đã có** cột đó.

---

## 2. Lỗi KHÔNG phải do migration `AddNguoiDungMacDinhTheoPhong`

Log terminal ghi rõ migration đang apply là:

```txt
Applying migration '20260625063211_UpdateHinhThucLCNT'
```

Migration task #110 (`20260626032714_AddNguoiDungMacDinhTheoPhong`) **chưa tới lượt chạy** vì bị chặn ở bước trước.

Đã kiểm tra file `20260626032714_AddNguoiDungMacDinhTheoPhong.cs`:

- Chỉ có `CreateTable("NguoiDungMacDinhTheoPhong", ...)`
- **Không** có `AddColumn` / `ALTER TABLE` cho `HoSoMoiThauDienTu.ThamDinh`

---

## 3. Nguyên nhân gốc (root cause)

### 3.1. DB drift — lệch giữa schema thực tế và migration history

| Nguồn | Trạng thái cột `HoSoMoiThauDienTu.ThamDinh` |
|-------|---------------------------------------------|
| **Database thực tế** | Đã có cột `ThamDinh` (`bit NULL`) |
| **`__EFMigrationsHistory`** | Chưa ghi `20260625063211_UpdateHinhThucLCNT` |
| **Migration `UpdateHinhThucLCNT`** | Vẫn pending → EF cố `ADD ThamDinh` |

EF Core tin rằng cột chưa tồn tại (vì migration chưa apply), nhưng DB đã có → **duplicate column**.

### 3.2. Timeline trong source (đã đối chiếu)

```
Migration 20260624082121_UpdateToTrinHQuetDinh
  └─ Snapshot: HoSoMoiThauDienTu KHÔNG có property ThamDinh

Migration 20260625063211_UpdateHinhThucLCNT   ← migration đang fail
  └─ Up(): ADD HoSoMoiThauDienTu.ThamDinh
  └─ Up(): ADD DanhMucHinhThucLuaChonNhaThau.LaChiDinhThau
  └─ Snapshot sau migration: đã có ThamDinh

Migration 20260626032714_AddNguoiDungMacDinhTheoPhong
  └─ Up(): chỉ CREATE TABLE NguoiDungMacDinhTheoPhong
```

### 3.3. Vì sao DB đã có `ThamDinh` trước khi migration chạy?

Các khả năng thường gặp (một hoặc kết hợp):

1. Cột được add **thủ công** trên DB dev (SSMS / script SQL).
2. DB được restore/copy từ môi trường khác đã có schema mới hơn history local.
3. Dev khác đã apply schema nhưng **chưa commit** / **chưa đồng bộ** `__EFMigrationsHistory`.
4. Entity `HoSoMoiThauDienTu.ThamDinh` đã deploy code trước, DB được chỉnh tay cho khớp app.

**Kết luận:** Đây là **lệch migration history**, không phải bug trong Entity/Configuration của task `NguoiDungMacDinhTheoPhong`.

---

## 4. Kiểm tra Entity / Configuration / Snapshot

### 4.1. Entity — `QLDA.Domain/Entities/HoSoMoiThauDienTu.cs`

```csharp
public bool? ThamDinh { get; set; }
```

Property hợp lệ, khớp kiểu `bit NULL` trên SQL Server. **Không cần xóa** cho task #110.

### 4.2. Configuration — `QLDA.Persistence/Configurations/HoSoMoiThauDienTuConfiguration.cs`

Không có `builder.Property(e => e.ThamDinh)` riêng → EF map theo **convention**. **Bình thường**, không gây duplicate.

### 4.3. `AppDbContextModelSnapshot.cs`

Snapshot hiện tại **đã có**:

```csharp
b.Property<bool?>("ThamDinh")
    .HasColumnType("bit");
```

trong entity `HoSoMoiThauDienTu` — khớp với model sau `UpdateHinhThucLCNT`. **Không cần sửa tay snapshot** cho case này.

---

## 5. Cách xác minh trên DB (chạy tay, read-only)

```sql
-- A) Cột đã tồn tại chưa?
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'HoSoMoiThauDienTu'
  AND COLUMN_NAME = 'ThamDinh';

-- B) Migration nào đã apply?
SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
ORDER BY MigrationId;

-- C) UpdateHinhThucLCNT đã trong history chưa?
SELECT 1
FROM __EFMigrationsHistory
WHERE MigrationId = '20260625063211_UpdateHinhThucLCNT';
```

**Kỳ vọng khi gặp lỗi:**

- Query (A): có 1 dòng `ThamDinh`
- Query (C): **không** có dòng → đúng pattern DB drift

---

## 6. Cách xử lý (không drop database, không sửa migration file)

### Phương án khuyến nghị: đồng bộ history + bổ sung cột còn thiếu

Migration `UpdateHinhThucLCNT` làm **2 việc**. DB có thể đã có `ThamDinh` nhưng **chưa** có `LaChiDinhThau`.

**Bước 1 — Kiểm tra cột `LaChiDinhThau`:**

```sql
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'DanhMucHinhThucLuaChonNhaThau'
  AND COLUMN_NAME = 'LaChiDinhThau';
```

**Bước 2 — Add `LaChiDinhThau` nếu thiếu** (phần còn lại của migration pending):

```sql
IF COL_LENGTH('DanhMucHinhThucLuaChonNhaThau', 'LaChiDinhThau') IS NULL
BEGIN
    ALTER TABLE [DanhMucHinhThucLuaChonNhaThau] ADD [LaChiDinhThau] bit NULL;
END;
```

**Bước 3 — Ghi nhận migration `UpdateHinhThucLCNT` đã apply** (vì `ThamDinh` đã có sẵn, không chạy lại `ADD`):

```sql
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625063211_UpdateHinhThucLCNT'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260625063211_UpdateHinhThucLCNT', N'8.0.15');
END;
```

> `ProductVersion` lấy từ file `.Designer.cs` migration mới nhất trong repo (hiện tại `8.0.15`).

**Bước 4 — Apply migration task #110:**

```bash
ef.bat QLDA update
```

Lần này EF sẽ skip `UpdateHinhThucLCNT` (đã có trong history) và chỉ apply `AddNguoiDungMacDinhTheoPhong`.

---

## 7. Việc KHÔNG được làm

| Hành động | Lý do |
|-----------|-------|
| `DROP DATABASE` | Mất dữ liệu dev — **cấm** theo quy ước team |
| Sửa tay `20260625063211_UpdateHinhThucLCNT.cs` | File migration immutable sau khi tạo |
| Sửa tay `AppDbContextModelSnapshot.cs` cho case này | Snapshot đã đúng; drift nằm ở DB history |
| Gộp `ADD ThamDinh` vào `AddNguoiDungMacDinhTheoPhong` | Sai scope task #110, gây rối lịch sử migration |
| Xóa cột `ThamDinh` trên DB để migration chạy lại | Phá schema đang dùng bởi app / dữ liệu hiện có |

---

## 8. Phòng ngừa cho lần sau

1. Sau khi `ef.bat QLDA add`, luôn `ef.bat QLDA update` trên DB dev **trước** khi add migration tiếp theo.
2. Không `ALTER TABLE` thủ công trên DB dev nếu team dùng EF migrations làm nguồn sự thật schema.
3. Khi pull code có migration mới: chạy `ef.bat QLDA list` và so với `__EFMigrationsHistory`.
4. Nếu restore DB từ môi trường khác: đồng bộ luôn bảng `__EFMigrationsHistory`.

---

## 9. Tóm tắt 1 dòng

**Lỗi duplicate `ThamDinh` = migration `UpdateHinhThucLCNT` pending trong khi DB đã có cột đó; `AddNguoiDungMacDinhTheoPhong` không liên quan.**
