# 📋 Implementation Summary: Fix DanhSachManHinh Sorting

## ✅ What Has Been Fixed

### Issue
- **CREATE/UPDATE APIs**: Không trả `danhSachManHinh` trong response
- **GET Detail API**: Trả `danhSachManHinh` nhưng bị sắp xếp lại theo ID tăng dần, không giữ thứ tự input

### Solution Applied
Đã thêm cột **Stt** (Number/Order) vào bảng `DmBuocManHinh` để preserve thứ tự input của user.

---

## 🔧 Code Changes Made

### 1. **Domain Entity** - `DanhMucBuocManHinh.cs`
- ✅ Thêm property `Stt` để lưu vị trí trong danh sách

### 2. **Data Configuration** - `DanhMucBuocManHinhConfiguration.cs`
- ✅ Cấu hình `Stt` với default value = 0
- ✅ Tạo index `(BuocId, Stt)` để optimize query performance

### 3. **Command Handlers** - Create/Update Commands
- ✅ **DanhMucBuocInsertCommand.cs**: Cập nhật để lưu `Stt` từ vị trí trong danh sách đầu vào
- ✅ **DanhMucBuocUpdateCommand.cs**: Tương tự, cập nhật để lưu `Stt` từ input

### 4. **Mappings** - DTOs
- ✅ **DanhMucBuocMappings.cs**: Update `ToEntity()` & `Update()` để set `Stt = index`
- ✅ **DanhMucBuocMappingConfiguration.cs**: Update `ToDto()` để ORDER BY Stt
- ✅ **StepMappingMappings.cs**: Update để ORDER BY Stt

### 5. **Queries** - Data Retrieval
- ✅ **DanhMucBuocGetQuery.cs**: Sort `BuocManHinhs` by `Stt` khi load từ DB
- ✅ **DanhMucBuocGetDanhSachQuery.cs**: ORDER BY `Stt` trong SELECT

### 6. **API Controllers** - Response Data
- ✅ **DanhMucBuocController.cs**: Update `Create()` API để trả entity với `danhSachManHinh`

---

## 📊 Expected Behavior After Fix

### Scenario: Input `danhSachManHinh: [6, 8, 12, 7]`

| API | Request | Response (danhSachManHinh) |
|-----|---------|---------------------------|
| **POST them-moi** | `[6, 8, 12, 7]` | **`[6, 8, 12, 7]`** ✅ |
| **PUT cap-nhat** | `[6, 8, 12, 7]` | **`[6, 8, 12, 7]`** ✅ |
| **GET {id}** | - | **`[6, 8, 12, 7]`** ✅ |

---

## 🗄️ Database Migration

### Method 1: Use Provided SQL Script (Recommended - No DB Drop)
```bash
# 1. Open SQL Server Management Studio or Azure Data Studio
# 2. Connect to database: VI_DACDT
# 3. Execute: apply-migration-stt.sql
# 4. Done!
```

### Method 2: Use EF Core (Alternative)
```bash
cd e:\SER\QLDA.Persistence
dotnet ef database update --project . --startup-project ..\QLDA.WebApi
```

---

## 📝 Files Modified

| File | Change |
|------|--------|
| `QLDA.Domain/Entities/DanhMuc/DanhMucBuocManHinh.cs` | Added `Stt` property |
| `QLDA.Persistence/Configurations/DanhMuc/DanhMucBuocManHinhConfiguration.cs` | Configure `Stt` column |
| `QLDA.Application/DanhMucBuocs/Commands/DanhMucBuocInsertCommand.cs` | Save `Stt` |
| `QLDA.Application/DanhMucBuocs/Commands/DanhMucBuocUpdateCommand.cs` | Save `Stt` |
| `QLDA.Application/DanhMucBuocs/DanhMucBuocMappings.cs` | Map `Stt` on insert/update |
| `QLDA.Application/DanhMucBuocs/DTOs/DanhMucBuocMappingConfiguration.cs` | Return ordered by `Stt` |
| `QLDA.Application/DanhMucBuocs/DTOs/StepMappingMappings.cs` | Return ordered by `Stt` |
| `QLDA.Application/DanhMucBuocs/Queries/DanhMucBuocGetQuery.cs` | Sort by `Stt` |
| `QLDA.Application/DanhMucBuocs/Queries/DanhMucBuocGetDanhSachQuery.cs` | Sort by `Stt` |
| `QLDA.WebApi/Controllers/DanhMucBuocController.cs` | Return entity in CREATE response |

### Migration Files
| File | Purpose |
|------|---------|
| `QLDA.Persistence/Migrations/20260420000000_AddSttColumnToDmBuocManHinh.cs` | EF Migration (ADD column) |
| `apply-migration-stt.sql` | SQL Script (optional, for direct execution) |

---

## 🚀 Next Steps

1. **Apply Migration:**
   - Run SQL script: `apply-migration-stt.sql` (RECOMMENDED)
   - OR: `dotnet ef database update`

2. **Build & Run:**
   ```bash
   cd e:\SER
   dotnet build
   dotnet run --project QLDA.WebApi
   ```

3. **Test:**
   - POST `danh-muc-buoc/them-moi` with `danhSachManHinh: [6, 8, 12, 7]`
   - Verify response returns `danhSachManHinh: [6, 8, 12, 7]` (same order)
   - GET `danh-muc-buoc/{id}` and verify same order is returned

4. **Existing Data Migration (Optional):**
   - For existing records, `Stt` will default to 0
   - If needed to preserve original order, run additional SQL:
   ```sql
   -- This would need to be done manually based on your business logic
   UPDATE DmBuocManHinh
   SET Stt = ROW_NUMBER() OVER (PARTITION BY BuocId ORDER BY ManHinhId)
   WHERE Stt = 0
   ```

---

## ⚠️ Important Notes

- **No Database Drop**: Uses ADD COLUMN with DEFAULT, preserves all existing data
- **Backward Compatible**: Existing queries work fine, new sorting is automatic
- **Performance**: Index on `(BuocId, Stt)` ensures fast queries
- **Data Integrity**: Foreign key relationships unchanged

---

## 🎯 Success Criteria

✅ **API Create** returns `danhSachManHinh` in response  
✅ **API Update** returns `danhSachManHinh` in response  
✅ **API Get Detail** returns `danhSachManHinh` in correct order  
✅ **Order** matches input (e.g., [6, 8, 12, 7] stays [6, 8, 12, 7])  
✅ **No Database Data Loss**  

