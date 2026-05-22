# Server Overload / 503 Errors - Critical Findings

**Issue ID:** server-overload-503  
**Created:** 2026-05-21  
**Status:** Open  
**Priority:** P0 - Critical

---

## Summary

Scout analysis identified multiple endpoints and functions that can cause server overload leading to HTTP 503 (Service Unavailable) errors. These issues stem from missing pagination, N+1 query patterns, lack of caching, no rate limiting, and memory-intensive operations.

---

## Critical Issues

### CR-001: PrintController - No Pagination on Export Endpoints

**Severity:** CRITICAL  
**Location:** `QLDA.WebApi/Controllers/PrintController.cs`  
**Lines:** 35-635 (12 endpoints)

**Problem:**
All 12 print/export endpoints pass `PageIndex = 0, PageSize = 0` to stored procedures, meaning **no pagination** - all matching records are exported at once.

| Endpoint | Stored Procedure | Risk |
|----------|-----------------|------|
| `InDuAnTraCuu` | `usp_In_DanhSach_DuAn_TraCuu` | High |
| `InDuAn` | `usp_In_DanhSach_DuAn` | High |
| `InGoiThau` | `usp_In_DanhSach_GoiThau` | High |
| `InHopDong` | `usp_In_DanhSach_HopDong` | High |
| `InPhuLucHopDong` | `usp_In_DanhSach_PhuLucHopDong` | High |
| `InBaoCaoTienDo` | `usp_In_DanhSach_BaoCaoTienDo` | High |
| `InBaoCaoBaoHanhSanPham` | `usp_In_DanhSach_BaoCaoBaoHanhSanPham` | High |
| `InBaoCaoBanGiaoSanPham` | `usp_In_DanhSach_BaoCaoBanGiaoSanPham` | High |
| `InKhoKhanVuongMac` | `usp_In_DanhSach_KhoKhanVuongMac` | High |
| `InTongHopVanBanQuyetDinh` | `usp_In_DanhSach_TongHopVanBanQuyetDinh` | High |
| `InDanhSachTreHanPhongBan` | `usp_In_DanhSachTreHanPhongBan` | High |
| `InBaoCaoTienDoDuAn` | `usp_In_BaoCao_TienDo_DuAn` | High |

**Code Path:**
```csharp
// PrintController.cs:28-35 - GetStoreQueryHandler
var data = await _dapperRepository.QueryStoredProcAsync<object>(request.ProcName, request.Params, ...);
return _exporter.Export(new AsposeInstruction<dynamic> {
    Items = data.ToList(),  // LINE 33: Entire result loaded into memory
```

**Impact:**
- Large exports cause OOM (Out of Memory) → 503
- `Aspose.Cells` synchronous processing loads entire dataset into memory
- Excel generation is memory-bound for large datasets

**Fix Required:**
- Enforce max `PageSize` (e.g., 10,000 rows per request)
- Implement streaming response for large exports
- Add pagination to stored procedures

---

### CR-002: DashboardRepository.GetTongHopAsync - Full Table Load

**Severity:** CRITICAL  
**Location:** `QLDA.Persistence/Repositories/DashboardRepository.cs`  
**Lines:** 36-89

**Problem:**
```csharp
var duAnData = await BaseQuery(nam)
    .Include(e => e.LoaiDuAnTheoNam)
    .Include(e => e.BuocHienTai)
    .Include(e => e.GiaiDoanHienTai)
    .Select(e => new DuAnDashboardProjection { ... })
    .ToListAsync(cancellationToken);  // LINE 51: ALL projects for year loaded to memory

// Then in-memory grouping (lines 53-88):
TheoLoai = duAnData.Where(...).GroupBy(...).Select(...).ToList()
TheoBuoc = duAnData.Where(...).GroupBy(...).Select(...).ToList()
TheoGiaiDoan = duAnData.Where(...).GroupBy(...).Select(...).ToList()
```

**Impact:**
- For databases with 10k+ projects, this loads entire dataset into memory
- Multiple `.Where().GroupBy()` chains on in-memory data
- CPU and memory spike under concurrent requests

**Fix Required:**
- Move aggregation to SQL side (stored procedure with GROUP BY)
- Add Redis cache with 5-minute TTL

---

### CR-003: DuAnGetDanhSachTepDinhKemQuery - N+1 Query Pattern

**Severity:** HIGH  
**Location:** `QLDA.Application/DuAns/Queries/DuAnGetDanhSachTepDinhKemQuery.cs`  
**Lines:** 45-115

**Problem:**
16 sequential database queries to collect group IDs:
```csharp
AddIds(await _goiThauRepo.GetQueryableSet()...Select(e => e.Id.ToString()).ToListAsync(cancellationToken));  // Query 1
AddIds(await _hopDongRepo.GetQueryableSet()...Select(e => e.Id.ToString()).ToListAsync(cancellationToken)); // Query 2
AddIds(await _nghiemThuRepo.GetQueryableSet()...Select(e => e.Id.ToString()).ToListAsync(cancellationToken)); // Query 3
// ... 13 more sequential queries
```

**Fix Required:**
- Replace with single query using UNION ALL
- Or use async parallel execution with `Task.WhenAll`

---

### CR-004: No Rate Limiting on Export Endpoints

**Severity:** HIGH  
**Location:** All Controllers  
**Finding:** No rate limiting middleware found in codebase

**Impact:**
- Export endpoints are vulnerable to DoS attacks
- Malicious user could hammer export endpoints repeatedly

**Fix Required:**
- Add `Microsoft.AspNetCore.RateLimiting` middleware
- Apply `[EnableRateLimiting]` to export endpoints

---

### CR-005: No Caching on Dashboard Endpoints

**Severity:** HIGH  
**Location:** `QLDA.WebApi/Controllers/DashboardController.cs`

**Problem:**
- Every `GetTongHop` request recomputes full aggregation from DB
- No Redis or distributed cache configured
- Only `AddMemoryCache()` in `WebApplicationExtensions.cs:102`

**Fix Required:**
- Add Redis distributed cache
- Implement `ResponseCache` on expensive endpoints

---

## Medium Priority Issues

### MED-001: ImportController Synchronous File Processing

**Location:** `QLDA.WebApi/Controllers/ImportController.cs`, `ExcelImporter.cs`

**Problem:** Files read synchronously and processed in-memory

---

### MED-002: TemplateController Sequential Queries

**Location:** `QLDA.WebApi/Controllers/TemplateController.cs` (lines 42-107)

**Problem:** 5-6 sequential queries per template generation, no caching

---

### MED-003: DuAnGetDanhSachQuery Eager Loading

**Location:** `QLDA.Application/DuAns/Queries/DuAnGetDanhSachQuery.cs` (line 30)

**Problem:** `Include(e => e.DuToans)` eagerly loads DuToans collection for each project

---

## Recommended Fix Order

1. **P0 - Immediate:** Cap `PageSize` in `PrintController.GetStoreQuery` (max 10k)
2. **P0 - Short-term:** Add `[EnableRateLimiting]` to export endpoints
3. **P1 - Medium-term:** Replace `GetTongHopAsync` with SQL-side aggregation + Redis cache
4. **P1 - Medium-term:** Replace N+1 in `DuAnGetDanhSachTepDinhKemQuery` with single query
5. **P2 - Long-term:** Move to Redis for distributed cache, add horizontal scaling strategy

---

## Verification

Scout command used:
```
grep -rn "RateLimiting\|throttle" --include="*.cs"
grep -rn "ToListAsync\|ToList()" --include="*.cs"
grep -rn "PageSize\|Pagination" --include="*.cs"
```

---

**Status:** Open - Awaiting prioritization and fix scheduling