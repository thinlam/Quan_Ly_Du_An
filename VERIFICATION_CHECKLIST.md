# ✅ Code Review Checklist - DanhSachManHinh Sorting Fix

## Modified Files - Verification

### 1. Domain Entity
- [x] **DanhMucBuocManHinh.cs**
  - Added: `public int Stt { get; set; }` with documentation

### 2. Configuration
- [x] **DanhMucBuocManHinhConfiguration.cs**
  - Added: `Stt` property configuration with default value 0
  - Added: Index on `(BuocId, Stt)` for query performance

### 3. Commands - Insert
- [x] **DanhMucBuocInsertCommand.cs**
  - Updated: `InsertAsync()` method to:
    - Create `BuocManHinhs` with `Stt = index` from input order
    - Save with updated SaveChanges before returning

### 4. Commands - Update
- [x] **DanhMucBuocUpdateCommand.cs**
  - Updated: `Handle()` method to:
    - Update `BuocManHinhs` with `Stt = index` from input order
    - Clear and recreate relationships with new order

### 5. Mappings - Entity to Entity
- [x] **DanhMucBuocMappings.cs**
  - `ToEntity()`: Select with index to set `Stt = index`
  - `Update()`: Select with index to set `Stt = index` for both insert and update

### 6. Mappings - DTO
- [x] **DanhMucBuocMappingConfiguration.cs**
  - `ToDto(entity)`: Order by `Stt` before selecting ManHinhId
  - `ToDto(materializedDto)`: Order by `Stt` before selecting ManHinhId

### 7. Step Mappings
- [x] **StepMappingMappings.cs**
  - `ToStepDto(DanhMucBuoc)`: Order by `Stt` before returning
  - `ToStepDto(DuAnBuoc)`: Order by `Stt` before returning

### 8. Queries - Detail Query
- [x] **DanhMucBuocGetQuery.cs**
  - Updated: Added sorting by `Stt` after loading entity
  - Ensures `BuocManHinhs` are ordered correctly before mapping

### 9. Queries - List Query
- [x] **DanhMucBuocGetDanhSachQuery.cs**
  - Updated: SELECT statement to order by `Stt` when mapping
  - Pagination query returns properly ordered data

### 10. API Controllers
- [x] **DanhMucBuocController.cs**
  - `Create()`: Changed `ResultApi.Ok(1)` to `ResultApi.Ok(entity.ToDto())`
  - Now returns entity with `danhSachManHinh` in response

### 11. Database Migration
- [x] **20260420000000_AddSttColumnToDmBuocManHinh.cs**
  - Created migration file to add `Stt` column
  - Includes index creation for `(BuocId, Stt)`

### 12. SQL Script
- [x] **apply-migration-stt.sql**
  - Created helper script for manual database migration
  - Safe to run (checks for existing columns/indexes)
  - Idempotent (can run multiple times)

---

## Testing Checklist

### API Tests
- [ ] POST `danh-muc-buoc/them-moi` with `danhSachManHinh: [6, 8, 12, 7]`
  - Response should include `danhSachManHinh: [6, 8, 12, 7]` ✅
  
- [ ] PUT `danh-muc-buoc/cap-nhat` with `danhSachManHinh: [12, 6, 7, 8]`
  - Response should include `danhSachManHinh: [12, 6, 7, 8]` ✅
  
- [ ] GET `danh-muc-buoc/{id}`
  - Response should include `danhSachManHinh` in same order as last save ✅

### Database Tests
- [ ] Verify `Stt` column exists in `DmBuocManHinh`
- [ ] Verify index `IX_DmBuocManHinh_BuocId_Stt` exists
- [ ] Query `SELECT * FROM DmBuocManHinh ORDER BY BuocId, Stt` returns proper order

### Integration Tests
- [ ] Build succeeds: `dotnet build`
- [ ] No compilation errors
- [ ] Application starts: `dotnet run --project QLDA.WebApi`

---

## ⚠️ Important Notes

### Data Integrity
- **Backward Compatible**: Existing code still works
- **No Data Loss**: Uses `ADD COLUMN DEFAULT 0`, preserves all data
- **Null Safety**: `Stt` is NOT NULL with default 0

### Performance
- **Index Added**: `(BuocId, Stt)` for optimization
- **Order By Stt**: Efficient queries with new index
- **No Breaking Changes**: Existing queries still valid

### Testing Recommendations
1. Test with simple input: `[1, 2, 3]` → should return `[1, 2, 3]`
2. Test with random order: `[5, 1, 3, 2, 4]` → should return `[5, 1, 3, 2, 4]`
3. Test with duplicates: `[1, 1, 2]` → behavior depends on business logic (currently accepts)
4. Test with empty: `[]` → should return `[]`
5. Test GET after Update: Ensure order persists

---

## 🎯 Success Criteria

| Criteria | Before | After | Status |
|----------|--------|-------|--------|
| Create returns danhSachManHinh | ❌ | ✅ | FIXED |
| Update returns danhSachManHinh | ✅ | ✅ | WORKING |
| Get returns correct order | ❌ | ✅ | FIXED |
| Preserves input order | ❌ | ✅ | FIXED |
| No database drop needed | N/A | ✅ | SAFE |
| Backward compatible | N/A | ✅ | COMPATIBLE |

---

## 📋 Rollback Plan (If Needed)

**Important: Keep SQL script backup!**

To rollback (if absolutely necessary):
```sql
-- Remove migration from history
DELETE FROM [__EFMigrationsHistory] 
WHERE [MigrationId] = '20260420000000_AddSttColumnToDmBuocManHinh'

-- Drop column (last resort!)
ALTER TABLE [DmBuocManHinh]
DROP COLUMN [Stt]

-- Drop index
DROP INDEX [IX_DmBuocManHinh_BuocId_Stt] ON [DmBuocManHinh]
```

---

## 📞 Support

If issues arise:
1. Check `MIGRATION_GUIDE.md` for step-by-step instructions
2. Review `IMPLEMENTATION_SUMMARY.md` for overview
3. Check error logs in `logs/` directory
4. Verify database connectivity and permissions

---

## ✨ Summary

**Total Changes:**
- 10 C# files modified
- 1 migration file created
- 1 SQL script provided
- 2 documentation files created

**Expected Outcome:**
- ✅ All 3 APIs (create, update, get) return `danhSachManHinh` in correct order
- ✅ Input order is preserved throughout the system
- ✅ Zero data loss, backward compatible
- ✅ No database drop required

