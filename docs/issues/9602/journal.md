# Journal — Issue #9602

## 14/05 — Update HopDong Date Fields

**Issue:** #9602 — Đổi tên Ngày dự kiến kết thúc, Bổ sung Ngày dự kiến kết thúc gói thầu

**Mục tiêu:**
1. Rename `NgayDuKienKetThuc` → `NgayDuKienKetThucHopDong`
2. Add `NgayDuKienKetThucGoiThau`
3. DTOs sử dụng `DateOnly` thay vì `DateTimeOffset`
4. Auto-calculate từ `KetQuaTrungThau`

---

### Domain Layer

- `HopDong.cs`: Rename field + add new field

### Application Layer

- `HopDongDto.cs`, `HopDongInsertDto.cs`, `HopDongUpdateDto.cs`:
  - Change `DateTimeOffset?` → `DateOnly?` cho date fields
  - Add XML comments

- `HopDongMappings.cs`:
  - `ToEntity()`: `DateOnly?.ToStartOfDayUtc()` → `DateTimeOffset?`
  - `ToDto()`: `DateTimeOffset?.ToDateOnlyVn()` → `DateOnly?`
  - `Update()`: `DateOnly?.ToStartOfDayUtc()` → `DateTimeOffset?`

- `HopDongInsertCommand.cs`, `HopDongUpdateCommand.cs`:
  - Add `CalculateExpectedEndDatesAsync()` method
  - Logic: `NgayHieuLuc.AddDays(SoNgayThucHienHopDong/SoNgayTrienKhai)`

- `HopDongGetDanhSachQuery.cs`:
  - Convert dates via `ToDateOnlyVn()`

### Persistence Layer

- `HopDongConfiguration.cs`:
  - Rename column mapping
  - Add new column mapping

### Migration

- `ef.sh QLDA add UpdateHopDongDateFields`
- `ef.sh QLDA update --sqlite`

---

**Kết quả:** Build 0 errors, SQLite updated

**Files changed:** 11 files

**Commit:** `feat(hop-dong): rename NgayDuKienKetThuc, add NgayDuKienKetThucGoiThau with auto-calculation`