# Twin Repository Migration: BaoCaoDuAn (Project Budget Report)

**From**: QLDA Main Repository  
**Feature**: Project Budget Report with Filters & Pagination  
**Status**: Ready for Migration  
**Last Updated**: April 23, 2026  
**Estimated Effort**: 10-12 hours  

---

## 📋 Executive Summary

**What is BaoCaoDuAn?**
A comprehensive project budget reporting API that aggregates data from multiple tables (DuAn, DuToan, NghiemThu, ThanhToan) with filtering, pagination, and calculated fields.

**Key Complexity**: Multi-table aggregation with smart dictionary-based lookups

**Implementation Complexity**: Medium-High  
**Dependencies**: Requires all related entities (DuAn, DuToan, NghiemThu, ThanhToan, DuAnBuoc, GiaiDoan)

---

## 🎯 Feature Scope

### What's Included
- ✅ Dynamic filtering (7 parameters)
- ✅ Pagination support (pageIndex, pageSize)
- ✅ Aggregated reporting (sum calculations)
- ✅ Progress tracking (current phase + step)
- ✅ Soft delete handling
- ✅ Optimized multi-table queries
- ✅ Complete API endpoint

### What's NOT Included
- ❌ Export to Excel/PDF (implement separately)
- ❌ Historical report tracking
- ❌ Custom report templates
- ❌ Scheduled reports

---

## 📂 Source Files in QLDA

```
QLDA.Application/DuAns/
├── Queries/
│   └── BaoCaoDuAnGetDanhSachQuery.cs          [Query handler - 130 lines]
└── DTOs/
    ├── BaoCaoDuAnSearchDto.cs                  [Filter DTO - 30 lines]
    └── BaoCaoDuAnDto.cs                        [Response DTO - 60 lines]

QLDA.WebApi/Controllers/
└── DuAnController.cs                           [API endpoint - 30 lines]
```

---

## 🚀 Step-by-Step Implementation

### Phase 1: Application Layer DTOs (1-2 hours)

#### 1.1 Create Filter DTO
**Copy from**: `QLDA.Application/DuAns/DTOs/BaoCaoDuAnSearchDto.cs`

```csharp
// File: [YourRepo].Application/DuAns/DTOs/BaoCaoDuAnSearchDto.cs

using [YourRepo].Application.Common.Interfaces;

namespace [YourRepo].Application.DuAns.DTOs;

public record BaoCaoDuAnSearchDto : CommonSearchDto {
    ///<summary>
    /// Tên dự án (Project name - partial match, case-insensitive)
    ///</summary>
    public string? TenDuAn { get; set; }
    
    ///<summary>
    /// Thời gian khởi công (Expected start year - exact match, 0 = no filter)
    ///</summary>
    public int? ThoiGianKhoiCong { get; set; }
    
    ///<summary>
    /// Thời gian hoàn thành (Expected completion year - exact match, 0 = no filter)
    ///</summary>
    public int? ThoiGianHoanThanh { get; set; }
    
    ///<summary>
    /// Loại dự án theo năm (Capital classification type, 0 = no filter)
    ///</summary>
    public int? LoaiDuAnTheoNamId { get; set; }
    
    ///<summary>
    /// Hình thức đầu tư (Investment form type, 0 = no filter)
    ///</summary>
    public int? HinhThucDauTuId { get; set; }
    
    ///<summary>
    /// Loại dự án (Project type, 0 = no filter)
    ///</summary>
    public int? LoaiDuAnId { get; set; }
    
    ///<summary>
    /// Đơn vị phụ trách chính (Responsible department, 0 = no filter)
    ///</summary>
    public long? DonViPhuTrachChinhId { get; set; }
}
```

**Key Points**:
- ✅ Inherits from `CommonSearchDto` (provides PageIndex, PageSize, Skip(), Take())
- ✅ All filter fields are nullable (optional)
- ✅ Use `int?` and `long?` for ID fields (0 = no filter in WhereIf logic)

#### 1.2 Create Response DTO
**Copy from**: `QLDA.Application/DuAns/DTOs/BaoCaoDuAnDto.cs`

```csharp
// File: [YourRepo].Application/DuAns/DTOs/BaoCaoDuAnDto.cs

using [YourRepo].Application.Common.Interfaces;

namespace [YourRepo].Application.DuAns.DTOs;

public class BaoCaoDuAnDto : IHasKey<Guid> {
    public Guid Id { get; set; }
    public string? TenDuAn { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public int? LoaiDuAnTheoNamId { get; set; }
    public decimal? KhaiToanKinhPhi { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public int? ThoiGianHoanThanh { get; set; }
    
    public long? DuToanBanDau { get; set; }      // ← Calculated
    public long? DuToanDieuChinh { get; set; }   // ← Calculated
    public string? TienDo { get; set; }          // ← Calculated
    public long? GiaTriNghiemThu { get; set; }   // ← Aggregated
    public long? GiaTriGiaiNgan { get; set; }    // ← Aggregated
    
    public int? HinhThucDauTuId { get; set; }
    public int? LoaiDuAnId { get; set; }
    public DateTimeOffset? NgayQuyetDinhDuToan { get; set; }
    public string? SoQuyetDinhDuToan { get; set; }
}
```

**Key Points**:
- ✅ Has 13 fields (7 direct + 3 calculated + 3 aggregated)
- ✅ Use `long?` for aggregated values (can be null)
- ✅ TienDo is string (concatenated phase + step)

---

### Phase 2: Query Handler (3-4 hours)

#### 2.1 Create Query Record
**Copy from**: `QLDA.Application/DuAns/Queries/BaoCaoDuAnGetDanhSachQuery.cs`

```csharp
// File: [YourRepo].Application/DuAns/Queries/BaoCaoDuAnGetDanhSachQuery.cs

using Microsoft.EntityFrameworkCore;
using [YourRepo].Application.Common.Mapping;
using [YourRepo].Application.DuAns.DTOs;
using [YourRepo].Domain.Entities;

namespace [YourRepo].Application.DuAns.Queries;

public record BaoCaoDuAnGetDanhSachQuery(
    BaoCaoDuAnSearchDto SearchDto) 
    : AggregateRootPagination, IRequest<PaginatedList<BaoCaoDuAnDto>> {
    public bool IsNoTracking { get; set; } = true;
}

internal class BaoCaoDuAnGetDanhSachQueryHandler : 
    IRequestHandler<BaoCaoDuAnGetDanhSachQuery, PaginatedList<BaoCaoDuAnDto>> {
    
    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IRepository<DuToan, Guid> _duToan;
    private readonly IRepository<NghiemThu, Guid> _nghiemThu;
    private readonly IRepository<ThanhToan, Guid> _thanhToan;

    public BaoCaoDuAnGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _duToan = serviceProvider.GetRequiredService<IRepository<DuToan, Guid>>();
        _nghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
    }

    public async Task<PaginatedList<BaoCaoDuAnDto>> Handle(
        BaoCaoDuAnGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        
        // STEP 1: Build base query
        var queryable = _duAn.GetQueryableSet()
            .AsNoTracking()                    // ← Read-only optimization
            .Where(e => !e.IsDeleted)          // ← Soft delete filter
            .Include(e => e.BuocHienTai)       // ← Navigation: current step
            .Include(e => e.GiaiDoanHienTai)   // ← Navigation: current phase
            
        // STEP 2: Apply dynamic filters
        queryable = queryable
            .WhereIf(request.SearchDto.TenDuAn.IsNotNullOrWhitespace(),
                e => e.TenDuAn!.ToLower().Contains(request.SearchDto.TenDuAn!.ToLower()))
            .WhereIf(request.SearchDto.ThoiGianKhoiCong > 0, 
                e => e.ThoiGianKhoiCong == request.SearchDto.ThoiGianKhoiCong)
            .WhereIf(request.SearchDto.ThoiGianHoanThanh > 0, 
                e => e.ThoiGianHoanThanh == request.SearchDto.ThoiGianHoanThanh)
            .WhereIf(request.SearchDto.LoaiDuAnTheoNamId > 0, 
                e => e.LoaiDuAnTheoNamId == request.SearchDto.LoaiDuAnTheoNamId)
            .WhereIf(request.SearchDto.HinhThucDauTuId > 0, 
                e => e.HinhThucDauTuId == request.SearchDto.HinhThucDauTuId)
            .WhereIf(request.SearchDto.LoaiDuAnId > 0, 
                e => e.LoaiDuAnId == request.SearchDto.LoaiDuAnId)
            .WhereIf(request.SearchDto.DonViPhuTrachChinhId > 0, 
                e => e.DonViPhuTrachChinhId == request.SearchDto.DonViPhuTrachChinhId);

        // STEP 3: Count total BEFORE pagination
        var totalCount = await queryable.CountAsync(cancellationToken);

        // STEP 4: Apply pagination and execute query
        var duAnList = await queryable
            .Skip(request.SearchDto.Skip())
            .Take(request.SearchDto.Take())
            .ToListAsync(cancellationToken);

        // STEP 5: Get aggregation data for these DuAn IDs
        var duAnIds = duAnList.Select(e => e.Id).ToList();
        
        // Get NghiemThu (Acceptance) sum per DuAn
        var nghiemThuDict = await _nghiemThu.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
            .GroupBy(e => e.DuAnId)
            .Select(g => new {
                DuAnId = g.Key,
                Sum = g.Sum(x => x.GiaTri)
            })
            .ToDictionaryAsync(x => x.DuAnId, x => x.Sum, cancellationToken);

        // Get ThanhToan (Payment) sum per DuAn
        var thanhToanDict = await _thanhToan.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
            .GroupBy(e => e.DuAnId)
            .Select(g => new {
                DuAnId = g.Key,
                Sum = g.Sum(x => x.GiaTri)
            })
            .ToDictionaryAsync(x => x.DuAnId, x => x.Sum, cancellationToken);

        // STEP 6: Map to DTOs with calculations
        var result = duAnList.Select(duAn => {
            // Get aggregation values from dictionaries
            var giaTriNghiemThu = nghiemThuDict.ContainsKey(duAn.Id) 
                ? nghiemThuDict[duAn.Id] : 0;
            var giaTriGiaiNgan = thanhToanDict.ContainsKey(duAn.Id) 
                ? thanhToanDict[duAn.Id] : 0;

            // Build progress string (combine phase + step)
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
            request.SearchDto.PageSize);
    }
}
```

**Critical Points**:
- ✅ AsNoTracking() for read-only performance
- ✅ WhereIf() for optional filters
- ✅ Count() BEFORE pagination
- ✅ Pagination applied to DuAn query (not aggregations)
- ✅ Aggregation done in separate queries
- ✅ Dictionary lookup for combining results

---

### Phase 3: Controller Endpoint (1-2 hours)

#### 3.1 Add Controller Method
**Modify**: `[YourRepo].WebApi/Controllers/DuAnController.cs`

```csharp
// Add this method to DuAnController class:

[HttpGet("bao-cao-du-toan")]
[ProducesResponseType(typeof(ResponseDto<PaginatedList<BaoCaoDuAnDto>>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GetBaoCaoDuToan(
    [FromQuery] BaoCaoDuAnSearchDto request) {
    
    if (request == null) {
        return BadRequest(new ResponseDto<object> {
            StatusCode = 400,
            Message = "Search parameters are required"
        });
    }
    
    var query = new BaoCaoDuAnGetDanhSachQuery(request);
    var result = await _mediator.Send(query);
    
    return Ok(new ResponseDto<PaginatedList<BaoCaoDuAnDto>> {
        StatusCode = 200,
        Message = "Success",
        Data = result
    });
}
```

**Key Points**:
- ✅ Route: `bao-cao-du-toan` (kebab-case)
- ✅ HTTP Method: GET
- ✅ Parameters: From query string
- ✅ Response: Wrapped in ResponseDto with metadata

---

### Phase 4: Testing (2-3 hours)

#### 4.1 Unit Tests
```csharp
// File: [YourRepo].Tests/Application/DuAns/Queries/BaoCaoDuAnGetDanhSachQueryTests.cs

[TestClass]
public class BaoCaoDuAnGetDanhSachQueryTests {
    
    [TestMethod]
    public async Task Handle_WithTenDuAnFilter_ShouldReturnMatchingProjects() {
        var query = new BaoCaoDuAnGetDanhSachQuery(
            new BaoCaoDuAnSearchDto {
                TenDuAn = "Hệ",
                PageIndex = 0,
                PageSize = 10
            }
        );
        
        var result = await handler.Handle(query, CancellationToken.None);
        
        Assert.IsTrue(result.Items.All(x => x.TenDuAn!.Contains("Hệ")));
    }
    
    [TestMethod]
    public async Task Handle_WithYearFilter_ShouldReturnExactMatches() {
        var query = new BaoCaoDuAnGetDanhSachQuery(
            new BaoCaoDuAnSearchDto {
                ThoiGianKhoiCong = 2024,
                PageIndex = 0,
                PageSize = 10
            }
        );
        
        var result = await handler.Handle(query, CancellationToken.None);
        
        Assert.IsTrue(result.Items.All(x => x.ThoiGianKhoiCong == 2024));
    }
    
    [TestMethod]
    public async Task Handle_ShouldAggregateNghiemThuValues() {
        // Arrange: Set up test data with known NghiemThu sums
        var query = new BaoCaoDuAnGetDanhSachQuery(...);
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert: Verify aggregation is correct
        var firstItem = result.Items.First();
        Assert.AreEqual(expectedSum, firstItem.GiaTriNghiemThu);
    }
}
```

#### 4.2 Integration Tests
```csharp
[TestClass]
public class BaoCaoDuAnIntegrationTests {
    
    [TestMethod]
    public async Task GetBaoCaoDuToan_WithValidRequest_ShouldReturn200() {
        var response = await client.GetAsync(
            "/api/du-an/bao-cao-du-toan?pageIndex=0&pageSize=10");
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
    
    [TestMethod]
    public async Task GetBaoCaoDuToan_WithMultipleFilters_ShouldFilterCorrectly() {
        var response = await client.GetAsync(
            "/api/du-an/bao-cao-du-toan?tenDuAn=test&loaiDuAnTheoNamId=3&pageIndex=0&pageSize=10");
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var data = await response.Content.ReadAsAsync<ResponseDto<PaginatedList<BaoCaoDuAnDto>>>();
        Assert.IsTrue(data.Data.Items.All(x => x.LoaiDuAnTheoNamId == 3));
    }
}
```

---

## ✅ Validation Checklist

### Code Quality
- [ ] DTOs compile without errors
- [ ] Query handler compiles without errors
- [ ] Controller endpoint compiles without errors
- [ ] No compiler warnings
- [ ] Code review approved

### Functionality
- [ ] All 7 filters work correctly
- [ ] Pagination works (correct skip/take)
- [ ] Soft deletes respected (IsDeleted = false)
- [ ] Aggregations correct (sum values match)
- [ ] Progress string formatted correctly
- [ ] Response fields populated

### API Testing
- [ ] GET /api/du-an/bao-cao-du-toan returns 200
- [ ] With filters: /api/du-an/bao-cao-du-toan?tenDuAn=test returns filtered results
- [ ] With pagination: /api/du-an/bao-cao-du-toan?pageIndex=1&pageSize=10 works
- [ ] With complex filters: combines filters correctly

### Database
- [ ] No orphaned data issues
- [ ] Query performance < 1 second
- [ ] Indexes on FK columns exist
- [ ] No N+1 query problems

### Unit Tests
- [ ] Filter tests pass
- [ ] Aggregation tests pass
- [ ] Pagination tests pass

### Integration Tests
- [ ] API endpoint tests pass
- [ ] Database query tests pass

---

## 📊 Effort Breakdown

| Phase | Task | Hours | Status |
|-------|------|-------|--------|
| 1 | DTOs (Filter + Response) | 1.5 | |
| 2 | Query Handler | 4 | |
| 3 | Controller Endpoint | 1.5 | |
| 4 | Testing | 3 | |
| **Total** | | **10-12** | |

---

## 🔍 Key Implementation Patterns

### Pattern 1: Dynamic Filtering with WhereIf
```csharp
queryable = queryable
    .WhereIf(filter.HasValue, e => e.Field == filter)
```

### Pattern 2: Aggregation with Dictionary
```csharp
var dict = await query
    .GroupBy(e => e.ParentId)
    .Select(g => new { ParentId = g.Key, Sum = g.Sum(x => x.Value) })
    .ToDictionaryAsync(x => x.ParentId, x => x.Sum);

// Usage:
var value = dict.ContainsKey(id) ? dict[id] : 0;
```

### Pattern 3: Pagination Before Aggregation
```csharp
// ✅ Correct: Paginate DuAn, then aggregate
var duAnList = await duAnQuery.Skip(...).Take(...).ToListAsync();
var duAnIds = duAnList.Select(e => e.Id).ToList();
var aggregated = await otherQuery.Where(e => duAnIds.Contains(e.ParentId)).GroupBy(...).ToListAsync();
```

---

## 🚀 Next Steps After Implementation

Once BaoCaoDuAn is implemented:

1. ✅ Test with actual production-like data
2. ✅ Optimize query if needed (add indexes)
3. ✅ Add Excel export feature (separate task)
4. ✅ Add scheduled report generation (separate task)
5. ✅ Create user documentation

---

## 📞 Troubleshooting

**Problem**: Query returns no results  
**Solution**: Check if IsDeleted filter is applied correctly

**Problem**: Aggregation values are wrong  
**Solution**: Verify NghiemThu/ThanhToan data is correct, check the filter logic

**Problem**: Pagination doesn't work  
**Solution**: Verify PageIndex & PageSize are passed, ensure Skip()/Take() are correct

**Problem**: Performance is slow  
**Solution**: Check indexes, verify AsNoTracking() is used, consider caching common filters

