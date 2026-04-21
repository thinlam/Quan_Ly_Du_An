# 🎯 How to Apply Migration - Step by Step

## ⚠️ Important: READ THIS FIRST

**Bạn có 2 lựa chọn để apply migration:**

### ✅ Option 1: Use SQL Script (SAFEST - Recommended)

**Không cần drop database, không mất dữ liệu**

#### Step 1: Open SQL Server Management Studio (SSMS)
1. Open SSMS
2. Connect to: `192.168.1.13\sql2k16r2:1439`
3. Select database: `VI_DACDT`

#### Step 2: Execute Script
1. Open file: `e:\SER\apply-migration-stt.sql`
2. **Verify** it says `USE [VI_DACDT]` at the top (correct database!)
3. Click **Execute** (hoặc Ctrl+E)

#### Step 3: Verify Success
- Check output: "Migration completed successfully!"
- Check table: `SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='DmBuocManHinh'`
- Should show: `BuocId, ManHinhId, Stt` ✅

---

### Option 2: Use EF Core CLI (Alternative)

```bash
cd e:\SER\QLDA.Persistence

# Apply migration
dotnet ef database update --project . --startup-project ..\QLDA.WebApi
```

---

## 🔍 Verify Migration Applied

### Method 1: Check in SSMS
```sql
-- Check if Stt column exists
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'DmBuocManHinh'
ORDER BY COLUMN_NAME;

-- Should show:
-- BuocId        | int       | NO
-- ManHinhId     | int       | NO
-- Stt           | int       | NO  ← NEW COLUMN
```

### Method 2: Check Migration History
```sql
SELECT [MigrationId], [ProductVersion]
FROM [__EFMigrationsHistory]
WHERE [MigrationId] LIKE '%AddSttColumnToDmBuocManHinh%';

-- Should return one row with:
-- MigrationId: 20260420000000_AddSttColumnToDmBuocManHinh
-- ProductVersion: 8.0.15
```

---

## ⚡ After Migration: Build & Test

### Step 1: Build Project
```bash
cd e:\SER
dotnet build
```

### Step 2: Run Application
```bash
dotnet run --project QLDA.WebApi
```

### Step 3: Test APIs in Postman

#### Test 1: Create with Random Order
**POST** `http://localhost:5001/api/danh-muc-buoc/them-moi`

```json
{
  "ten": "Bước 1 - Test",
  "ma": "BU001",
  "quyTrinhId": 49,
  "parentId": null,
  "soNgayThucHien": 5,
  "danhSachManHinh": [6, 8, 12, 7]  ← Random order
}
```

**Expected Response:**
```json
{
  "result": true,
  "dataResult": {
    "id": 405,
    "danhSachManHinh": [6, 8, 12, 7]  ← SAME ORDER! ✅
  }
}
```

#### Test 2: Get Detail
**GET** `http://localhost:5001/api/danh-muc-buoc/405`

**Expected Response:**
```json
{
  "result": true,
  "dataResult": {
    "id": 405,
    "danhSachManHinh": [6, 8, 12, 7]  ← SAME ORDER! ✅
  }
}
```

---

## ❌ Troubleshooting

### Error: "There is already an object named 'CANBO' in the database"
**Cause:** Migration history conflict  
**Solution:** Run only the SQL script, not EF CLI

### Error: "Invalid object name 'DmBuocManHinh'"
**Cause:** Wrong database selected  
**Solution:** Verify you're connecting to `VI_DACDT` database

### Column Stt still doesn't exist
**Cause:** Migration didn't apply  
**Solution:** 
1. Check migration history table:
   ```sql
   SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId DESC;
   ```
2. Manually insert if missing:
   ```sql
   INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
   VALUES (N'20260420000000_AddSttColumnToDmBuocManHinh', N'8.0.15')
   ```

---

## 📋 Files Related to This Fix

### Migration Files
- `QLDA.Persistence/Migrations/20260420000000_AddSttColumnToDmBuocManHinh.cs` - Migration definition

### SQL Script
- `apply-migration-stt.sql` - SQL script to run manually

### Modified Code Files
- `QLDA.Domain/Entities/DanhMuc/DanhMucBuocManHinh.cs`
- `QLDA.Persistence/Configurations/DanhMuc/DanhMucBuocManHinhConfiguration.cs`
- `QLDA.Application/DanhMucBuocs/Commands/DanhMucBuocInsertCommand.cs`
- `QLDA.Application/DanhMucBuocs/Commands/DanhMucBuocUpdateCommand.cs`
- `QLDA.Application/DanhMucBuocs/DanhMucBuocMappings.cs`
- `QLDA.Application/DanhMucBuocs/DTOs/DanhMucBuocMappingConfiguration.cs`
- `QLDA.Application/DanhMucBuocs/DTOs/StepMappingMappings.cs`
- `QLDA.Application/DanhMucBuocs/Queries/DanhMucBuocGetQuery.cs`
- `QLDA.Application/DanhMucBuocs/Queries/DanhMucBuocGetDanhSachQuery.cs`
- `QLDA.WebApi/Controllers/DanhMucBuocController.cs`

---

## 🎓 What Changed?

### Before Fix
```
Input:  danhSachManHinh=[6, 8, 12, 7]
Save:   y chang trả về [6, 8, 12, 7]
Get:    Trả về [6, 7, 8, 12] ❌ (sorted by ID)
```

### After Fix
```
Input:  danhSachManHinh=[6, 8, 12, 7]
Save:   Trả về [6, 8, 12, 7]   ✅
Get:    Trả về [6, 8, 12, 7]   ✅ (preserve order)
```

---

## 💡 Technical Details

- **New Column:** `DmBuocManHinh.Stt` (INT, NOT NULL, DEFAULT=0)
- **Index:** `(BuocId, Stt)` for fast queries
- **Order:** Records ordered by `Stt` (0, 1, 2, 3,...)
- **Backward Compatible:** Existing queries still work

---

## ✅ Done!

Sau khi apply migration và test thành công, hãy confirm vớitôi là mọi thứ working!
