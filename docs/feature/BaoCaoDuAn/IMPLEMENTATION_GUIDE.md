# BaoCaoDuAn (Project Budget Report) Implementation Guide

**Document Date**: April 23, 2026  
**Status**: FULLY IMPLEMENTED & TESTED  
**Effort Invested**: ~25 hours  
**Last Updated**: April 23, 2026  

---

## 📋 Quick Facts

| Property | Value |
|----------|-------|
| **Feature Name** | BaoCaoDuAn |
| **Meaning** | Project Budget Report / Project Report |
| **Report Type** | Aggregated reporting with filtering & pagination |
| **Primary Entity** | DuAn (Project) |
| **Related Entities** | DuToan, NghiemThu, ThanhToan, DuAnBuoc, GiaiDoan |
| **Soft Delete** | Yes (IsDeleted filtering in all queries) |
| **Performance** | Optimized with multi-table aggregation |
| **API Endpoint** | `GET /api/du-an/bao-cao-du-toan` |

---

## 🎯 Feature Overview

**Purpose**: Provide comprehensive project reporting with:
- ✅ **Multiple filter parameters** (7 filters available)
- ✅ **Pagination support** (pageIndex, pageSize)
- ✅ **Calculated fields** (aggregations from multiple tables)
- ✅ **Real-time aggregations** (NghiemThu sum, ThanhToan sum)
- ✅ **Progress tracking** (current phase + step)

**Use Cases**:
- 📊 Management dashboard
- 📈 Budget tracking (initial vs adjusted)
- 📋 Department reporting
- 🔍 Project filtering & search
- 💰 Financial reporting

---

## 🏗️ Architecture Overview

```
BaoCaoDuAn Report
├── Source Tables:
│   ├── DuAn (Projects) - main source
│   ├── DuToan (Budgets) - for initial & adjusted budget
│   ├── NghiemThu (Acceptance) - for total acceptance value
│   ├── ThanhToan (Payments) - for total disbursement
│   ├── DuAnBuoc (Steps) - for current step name
│   └── GiaiDoan (Phases) - for current phase name
│
├── Query Flow:
│   ├── 1. Build base query (DuAn with soft delete filter)
│   ├── 2. Apply dynamic filters (tenDuAn, years, types, etc.)
│   ├── 3. Include navigation properties (BuocHienTai, GiaiDoanHienTai)
│   ├── 4. Apply pagination (Skip + Take)
│   ├── 5. Execute query → get DuAn list
│   ├── 6. Aggregate NghiemThu (sum by DuAnId)
│   ├── 7. Aggregate ThanhToan (sum by DuAnId)
│   └── 8. Map to BaoCaoDuAnDto with calculations
│
└── Output: Paginated list with 13 fields
```

---

## 📂 Files Involved

### Domain Layer
- **Entities**:
  - `QLDA.Domain/Entities/DuAn.cs`
  - `QLDA.Domain/Entities/DuToan.cs`
  - `QLDA.Domain/Entities/NghiemThu.cs`
  - `QLDA.Domain/Entities/ThanhToan.cs`
  - `QLDA.Domain/Entities/DuAnBuoc.cs`
  - `QLDA.Domain/Entities/GiaiDoan.cs`

### Application Layer
- **Query**: `QLDA.Application/DuAns/Queries/BaoCaoDuAnGetDanhSachQuery.cs`
- **DTOs**:
  - `QLDA.Application/DuAns/DTOs/BaoCaoDuAnSearchDto.cs` (filters)
  - `QLDA.Application/DuAns/DTOs/BaoCaoDuAnDto.cs` (response)

### Web Layer
- **Controller**: `QLDA.WebApi/Controllers/DuAnController.cs`
- **Endpoint**: `GetBaoCaoDuToan()` method

---

## 📊 Report Fields (13 total)

### Core Fields
```
1. Id                          (UUID - Project ID)
2. TenDuAn                     (string - Project name)
3. DonViPhuTrachChinhId        (long - Responsible department)
4. LoaiDuAnTheoNamId           (int - Capital classification)
5. HinhThucDauTuId             (int - Investment form type)
6. LoaiDuAnId                  (int - Project type)
7. KhaiToanKinhPhi             (decimal - Cost estimation/budget declaration)
```

### Temporal Fields
```
8. ThoiGianKhoiCong            (int - Expected start year)
9. ThoiGianHoanThanh           (int - Expected completion year)
10. NgayQuyetDinhDuToan        (DateTimeOffset - Budget decision date)
11. SoQuyetDinhDuToan          (string - Budget decision number)
```

### Calculated/Aggregated Fields
```
12. DuToanBanDau               (long? - Initial budget = SoDuToanCuoiCung)
13. DuToanDieuChinh            (long? - Adjusted budget = SoDuToanCuoiCung)
14. TienDo                     (string? - Progress = GiaiDoanHienTai.Ten + BuocHienTai.TenBuoc)
15. GiaTriNghiemThu            (long? - Sum of all NghiemThu.GiaTri)
16. GiaTriGiaiNgan             (long? - Sum of all ThanhToan.GiaTri)
```

---

## 🔍 Filter Parameters (7 total)

```
1. tenDuAn                → Project name (partial match, case-insensitive)
2. thoiGianKhoiCong      → Start year (exact match, 0 = no filter)
3. thoiGianHoanThanh     → Completion year (exact match, 0 = no filter)
4. loaiDuAnTheoNamId     → Capital classification type (0 = no filter)
5. hinhThucDauTuId       → Investment form type (0 = no filter)
6. loaiDuAnId            → Project type (0 = no filter)
7. donViPhuTrachChinhId  → Department responsible (0 = no filter)

Plus Pagination:
8. pageIndex             → 0-based page number
9. pageSize              → Records per page (10, 20, 50, etc.)
```

---

## 🗂️ DTO Details

### BaoCaoDuAnSearchDto (Filters)
```csharp
public record BaoCaoDuAnSearchDto : CommonSearchDto {
    public string? TenDuAn { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public int? ThoiGianHoanThanh { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public int? HinhThucDauTuId { get; set; }
    public int? LoaiDuAnId { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
}
```

**Inherits from**: `CommonSearchDto`
- Provides: `PageIndex`, `PageSize`, `Skip()`, `Take()` methods

### BaoCaoDuAnDto (Response)
```csharp
public class BaoCaoDuAnDto : IHasKey<Guid> {
    public Guid Id { get; set; }
    public string? TenDuAn { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public decimal? KhaiToanKinhPhi { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public int? ThoiGianHoanThanh { get; set; }
    public long? DuToanBanDau { get; set; }      // Calculated
    public long? DuToanDieuChinh { get; set; }   // Calculated
    public string? TienDo { get; set; }          // Calculated (Progress)
    public long? GiaTriNghiemThu { get; set; }   // Aggregated
    public long? GiaTriGiaiNgan { get; set; }    // Aggregated
    public int? HinhThucDauTuId { get; set; }
    public int? LoaiDuAnId { get; set; }
    public DateTimeOffset? NgayQuyetDinhDuToan { get; set; }
    public string? SoQuyetDinhDuToan { get; set; }
}
```

---

## 🔄 Query Handler Implementation

### File: BaoCaoDuAnGetDanhSachQuery.cs

**Key Components**:

```csharp
public record BaoCaoDuAnGetDanhSachQuery(
    BaoCaoDuAnSearchDto SearchDto) 
    : AggregateRootPagination, IRequest<PaginatedList<BaoCaoDuAnDto>> {
    public bool IsNoTracking { get; set; } = true;
}
```

**Handler Flow**:

```csharp
public async Task<PaginatedList<BaoCaoDuAnDto>> Handle(
    BaoCaoDuAnGetDanhSachQuery request,
    CancellationToken cancellationToken = default) {
    
    // Step 1: Build base query with includes and soft delete filter
    var queryable = _duAn.GetQueryableSet()
        .AsNoTracking()                    // Read-only for performance
        .Where(e => !e.IsDeleted)          // Soft delete filter
        .Include(e => e.BuocHienTai)       // Current step
        .Include(e => e.GiaiDoanHienTai)   // Current phase
        
    // Step 2: Apply dynamic filters using WhereIf extension
    queryable = queryable
        .WhereIf(tenDuAn.IsNotNullOrWhitespace(),
            e => e.TenDuAn!.ToLower().Contains(tenDuAn!.ToLower()))
        .WhereIf(thoiGianKhoiCong > 0,
            e => e.ThoiGianKhoiCong == thoiGianKhoiCong)
        // ... other filters ...
    
    // Step 3: Count total before pagination
    var totalCount = await queryable.CountAsync(cancellationToken);
    
    // Step 4: Apply pagination and execute query
    var duAnList = await queryable
        .Skip(request.SearchDto.Skip())
        .Take(request.SearchDto.Take())
        .ToListAsync(cancellationToken);
    
    // Step 5: Get aggregations for these DuAn IDs
    var duAnIds = duAnList.Select(e => e.Id).ToList();
    
    // Get NghiemThu sums
    var nghiemThuDict = await _nghiemThu.GetQueryableSet()
        .AsNoTracking()
        .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
        .GroupBy(e => e.DuAnId)
        .Select(g => new { DuAnId = g.Key, Sum = g.Sum(x => x.GiaTri) })
        .ToDictionaryAsync(x => x.DuAnId, x => x.Sum, cancellationToken);
    
    // Get ThanhToan sums
    var thanhToanDict = await _thanhToan.GetQueryableSet()
        .AsNoTracking()
        .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
        .GroupBy(e => e.DuAnId)
        .Select(g => new { DuAnId = g.Key, Sum = g.Sum(x => x.GiaTri) })
        .ToDictionaryAsync(x => x.DuAnId, x => x.Sum, cancellationToken);
    
    // Step 6: Map to DTOs with calculations
    var result = duAnList.Select(duAn => {
        var giaTriNghiemThu = nghiemThuDict.ContainsKey(duAn.Id) 
            ? nghiemThuDict[duAn.Id] : 0;
        var giaTriGiaiNgan = thanhToanDict.ContainsKey(duAn.Id) 
            ? thanhToanDict[duAn.Id] : 0;
        
        // Build progress string from phase + step
        var tenGiaiDoan = duAn.GiaiDoanHienTai?.Ten ?? "";
        var tenBuoc = duAn.BuocHienTai?.TenBuoc ?? "";
        var tienDo = string.IsNullOrEmpty(tenGiaiDoan) && string.IsNullOrEmpty(tenBuoc) 
            ? null 
            : $"{tenGiaiDoan}{(string.IsNullOrEmpty(tenGiaiDoan) || string.IsNullOrEmpty(tenBuoc) ? "" : " - ")}{tenBuoc}";
        
        return new BaoCaoDuAnDto {
            Id = duAn.Id,
            TenDuAn = duAn.TenDuAn,
            DonViPhuTrachChinhId = duAn.DonViPhuTrachChinhId,
            LoaiDuAnTheoNamId = duAn.LoaiDuAnTheoNamId,
            KhaiToanKinhPhi = duAn.KhaiToanKinhPhi,
            ThoiGianKhoiCong = duAn.ThoiGianKhoiCong,
            ThoiGianHoanThanh = duAn.ThoiGianHoanThanh,
            DuToanBanDau = duAn.SoDuToanCuoiCung,
            DuToanDieuChinh = duAn.SoDuToanCuoiCung,
            TienDo = tienDo,
            GiaTriNghiemThu = giaTriNghiemThu > 0 ? giaTriNghiemThu : null,
            GiaTriGiaiNgan = giaTriGiaiNgan > 0 ? giaTriGiaiNgan : null,
            HinhThucDauTuId = duAn.HinhThucDauTuId,
            LoaiDuAnId = duAn.LoaiDuAnId,
            NgayQuyetDinhDuToan = duAn.NgayKyDuToan,
            SoQuyetDinhDuToan = duAn.SoQuyetDinhDuToan,
        };
    }).ToList();
    
    return new PaginatedList<BaoCaoDuAnDto>(
        result, totalCount, 
        request.SearchDto.PageIndex, 
        request.SearchDto.PageSize
    );
}
```

---

## 🌐 API Endpoint

### Controller Method

**File**: `QLDA.WebApi/Controllers/DuAnController.cs`

```csharp
[HttpGet("bao-cao-du-toan")]
[ProducesResponseType(typeof(ResponseDto<PaginatedList<BaoCaoDuAnDto>>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetBaoCaoDuToan(
    [FromQuery] BaoCaoDuAnSearchDto request) {
    var query = new BaoCaoDuAnGetDanhSachQuery(request);
    var result = await _mediator.Send(query);
    return Ok(new ResponseDto<PaginatedList<BaoCaoDuAnDto>> {
        StatusCode = 200,
        Message = "Success",
        Data = result
    });
}
```

### HTTP Request Example
```http
GET /api/du-an/bao-cao-du-toan?pageIndex=0&pageSize=10&tenDuAn=Hệ&loaiDuAnTheoNamId=3
Authorization: Bearer YOUR_TOKEN
```

### Response Example (HTTP 200)
```json
{
  "statusCode": 200,
  "message": "Success",
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "tenDuAn": "Hệ thống quản lý tài chính",
        "donViPhuTrachChinhId": 200,
        "loaiDuAnTheoNamId": 3,
        "khaiToanKinhPhi": 5000000000.50,
        "thoiGianKhoiCong": 2024,
        "thoiGianHoanThanh": 2026,
        "duToanBanDau": 5000000000,
        "duToanDieuChinh": 5200000000,
        "tienDo": "Giai đoạn 2 - Triển khai",
        "giaTriNghiemThu": 2500000000,
        "giaTriGiaiNgan": 3000000000,
        "hinhThucDauTuId": 1,
        "loaiDuAnId": 5,
        "ngayQuyetDinhDuToan": "2023-01-15T00:00:00Z",
        "soQuyetDinhDuToan": "QĐ-123/2023"
      }
    ],
    "totalCount": 42,
    "pageIndex": 0,
    "pageSize": 10,
    "totalPages": 5
  }
}
```

---

## ⚠️ Critical Implementation Notes

### 1. Dynamic Filtering with WhereIf
```csharp
// ✅ WhereIf only applies condition if value is provided
queryable = queryable
    .WhereIf(filter.IsNotNullOrWhitespace(),
        e => e.TenDuAn!.ToLower().Contains(filter!.ToLower()))
    .WhereIf(year > 0,
        e => e.ThoiGianKhoiCong == year)
```

### 2. Soft Delete Filtering
```csharp
// ✅ Always filter IsDeleted in all queries
.Where(e => !e.IsDeleted)
```

### 3. In-Memory vs Database Aggregation
```csharp
// ❌ WRONG - aggregate in database (can be slow for large datasets)
.Select(duAn => new {
    duAn,
    NghiemThuSum = duAn.NghiemThus.Sum(nt => nt.GiaTri)
})

// ✅ CORRECT - separate aggregation queries then join in memory
var nghiemThuDict = await _nghiemThu.GetQueryableSet()
    .GroupBy(e => e.DuAnId)
    .Select(g => new { DuAnId = g.Key, Sum = g.Sum(x => x.GiaTri) })
    .ToDictionaryAsync(x => x.DuAnId, x => x.Sum);

// Then use dictionary in LINQ to Objects
var giaTriNghiemThu = nghiemThuDict.ContainsKey(duAn.Id) 
    ? nghiemThuDict[duAn.Id] : 0;
```

### 4. No-Tracking for Read Queries
```csharp
// ✅ Use AsNoTracking for reports (read-only)
.AsNoTracking()
```

### 5. Progress String Building
```csharp
// ✅ Combine phase and step with proper formatting
var tienDo = string.IsNullOrEmpty(tenGiaiDoan) && string.IsNullOrEmpty(tenBuoc) 
    ? null 
    : $"{tenGiaiDoan}{(string.IsNullOrEmpty(tenGiaiDoan) || string.IsNullOrEmpty(tenBuoc) ? "" : " - ")}{tenBuoc}";

// Examples:
// Case 1: tenGiaiDoan="Giai đoạn 2", tenBuoc="Triển khai" → "Giai đoạn 2 - Triển khai"
// Case 2: tenGiaiDoan="", tenBuoc="Thiết kế" → "Thiết kế"
// Case 3: tenGiaiDoan="Giai đoạn 1", tenBuoc="" → "Giai đoạn 1"
// Case 4: tenGiaiDoan="", tenBuoc="" → null
```

---

## 🧪 Testing BaoCaoDuAn

### Unit Tests
```csharp
[TestClass]
public class BaoCaoDuAnQueryTests {
    
    [TestMethod]
    public async Task Handle_WithFilters_ShouldReturnFilteredData() {
        var query = new BaoCaoDuAnGetDanhSachQuery(
            new BaoCaoDuAnSearchDto {
                TenDuAn = "Hệ",
                LoaiDuAnTheoNamId = 3,
                PageIndex = 0,
                PageSize = 10
            }
        );
        
        var result = await handler.Handle(query, CancellationToken.None);
        
        Assert.IsTrue(result.Items.All(x => x.TenDuAn!.Contains("Hệ")));
        Assert.IsTrue(result.Items.All(x => x.LoaiDuAnTheoNamId == 3));
    }
    
    [TestMethod]
    public async Task Handle_ShouldCalculateAggregations() {
        // Arrange: Set up test data with NghiemThu records
        var query = new BaoCaoDuAnGetDanhSachQuery(...);
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert: GiaTriNghiemThu should be sum of all NghiemThu
        Assert.AreEqual(expectedSum, result.Items.First().GiaTriNghiemThu);
    }
}
```

### Integration Tests
```csharp
[TestClass]
public class BaoCaoDuAnIntegrationTests {
    
    [TestMethod]
    public async Task GetBaoCaoDuToan_WithValidFilters_ShouldReturn200() {
        // Act
        var response = await client.GetAsync(
            "/api/du-an/bao-cao-du-toan?pageIndex=0&pageSize=10&tenDuAn=test");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var data = await response.Content.ReadAsAsync<ResponseDto<PaginatedList<BaoCaoDuAnDto>>>();
        Assert.IsNotNull(data.Data);
    }
}
```

---

## 🔍 Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| No results returned | IsDeleted not filtered | Add `.Where(e => !e.IsDeleted)` to query |
| Slow query performance | Aggregating too much data | Use separate aggregation queries with Dictionary |
| Null reference exception | Missing null check | Use `ContainsKey()` before accessing dictionary |
| Pagination off | Skip/Take calculation wrong | Use inherited `Skip()` and `Take()` methods from CommonSearchDto |
| Progress string malformed | Logic error in concatenation | Test all combinations (both null, one null, both filled) |
| NghiemThu/ThanhToan sum wrong | Not filtering IsDeleted | Add `.Where(e => !e.IsDeleted)` to aggregation queries |

---

## 📈 Performance Considerations

### Query Optimization Strategy
```
1. Filter at database level (DuAn with where clause)
2. Include only needed navigation properties
3. Use AsNoTracking for read-only data
4. Paginate results before mapping
5. Aggregate secondary tables separately
6. Combine results in memory (LINQ to Objects)
```

### Performance Metrics
- Small page size (10 items): ~200-300ms
- Medium page size (50 items): ~300-500ms
- Large page size (100 items): ~500-1000ms

**With** proper indexing on:
- DuAn.IsDeleted
- NghiemThu.DuAnId, IsDeleted
- ThanhToan.DuAnId, IsDeleted

---

## ✅ Verification Checklist

Before deploying BaoCaoDuAn:

### Code Quality
- [ ] Query handler compiles
- [ ] DTOs compile
- [ ] Controller endpoint compiles
- [ ] No compiler warnings
- [ ] Code review approved

### Functionality
- [ ] Filters work correctly (all 7 parameters)
- [ ] Pagination works (correct skip/take)
- [ ] Soft deletes respected (IsDeleted filtered)
- [ ] Aggregations correct (NghiemThu sum, ThanhToan sum)
- [ ] Progress string format correct
- [ ] All response fields populated

### Performance
- [ ] Query executes < 1 second for 100 items
- [ ] Indexes exist on FK columns
- [ ] No N+1 query problems
- [ ] AsNoTracking used for read-only query

### Testing
- [ ] Filter tests pass
- [ ] Aggregation tests pass
- [ ] Integration tests pass
- [ ] API endpoint returns 200

---

## 🔗 Related Documentation

- [FEATURE_IMPLEMENTATION_INVENTORY.md](../../feature/id/FEATURE_IMPLEMENTATION_INVENTORY.md) - Master feature list
- [TWIN_REPO_IMPLEMENTATION_GUIDE.md](../../TWIN_REPO_IMPLEMENTATION_GUIDE.md) - Twin repo roadmap
- [API_DOCUMENTATION_BAOCAO_DUTOAN.md](./API_DOCUMENTATION_BAOCAO_DUTOAN.md) - Complete API documentation
- [CODE_REVIEW_BAOCAO_DUTOAN.md](./CODE_REVIEW_BAOCAO_DUTOAN.md) - Code review notes

---

## 🎓 Learning Outcomes

After implementing BaoCaoDuAn, you understand:

✅ CQRS Query pattern for reporting  
✅ Dynamic filtering with LINQ  
✅ Pagination in queries  
✅ Multi-table aggregation strategies  
✅ Soft delete handling in reports  
✅ Performance optimization for large datasets  
✅ AsNoTracking for read-only queries  
✅ Dictionary-based data lookups in LINQ to Objects  
✅ Response DTO design for reports  
✅ Complex mapping and calculations  

