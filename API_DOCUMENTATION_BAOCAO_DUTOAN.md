# API Documentation - Báo Cáo Dự Toán Dự Án

## Endpoint Overview

**Báo Cáo Dự Toán Dự Án** provides comprehensive project budget reports with filter capabilities and pagination.

---

## Endpoint Details

### GET /api/du-an/bao-cao-du-toan

**Description:** Retrieve paginated project budget reports with filterable parameters

**Method:** GET

**Authentication:** Required (Bearer Token)

**Content-Type:** application/json

---

## Request Parameters

All parameters are optional query parameters. Pagination parameters are required.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `tenDuAn` | string | Project name (partial match, case-insensitive) | `Dự án cơ sở dữ liệu` |
| `thoiGianKhoiCong` | integer | Start year (exact match) | `2024` |
| `thoiGianHoanThanh` | integer | Completion year (exact match) | `2026` |
| `loaiDuAnTheoNamId` | integer | Capital classification type ID | `1` |
| `hinhThucDauTuId` | integer | Investment form type ID | `2` |
| `loaiDuAnId` | integer | Project type ID | `3` |
| `donViPhuTrachChinhId` | long | Department/Unit in charge ID | `100` |
| `pageIndex` | integer | Page index (0-based) | `0` |
| `pageSize` | integer | Number of records per page | `10` |

### Capital Classification Types (LoaiDuAnTheoNamId)
- `1` - Chuẩn bị đầu tư (Preparing for Investment)
- `2` - Chuyển tiếp (Transitional)
- `3` - Khởi công mới (New Project)
- `4` - Khối lượng tồn đọng (Remaining Workload)

---

## Example Requests

### 1. Basic List with Pagination
```http
GET /api/du-an/bao-cao-du-toan?pageIndex=0&pageSize=10
Host: api.example.com
Authorization: Bearer YOUR_TOKEN
```

### 2. Filter by Project Name
```http
GET /api/du-an/bao-cao-du-toan?tenDuAn=Dự%20án%20A&pageIndex=0&pageSize=10
Host: api.example.com
Authorization: Bearer YOUR_TOKEN
```

### 3. Filter by Capital Type and Investment Form
```http
GET /api/du-an/bao-cao-du-toan?loaiDuAnTheoNamId=2&hinhThucDauTuId=1&pageIndex=0&pageSize=10
Host: api.example.com
Authorization: Bearer YOUR_TOKEN
```

### 4. Filter by Department and Year Range
```http
GET /api/du-an/bao-cao-du-toan?donViPhuTrachChinhId=123&thoiGianKhoiCong=2024&thoiGianHoanThanh=2026&pageIndex=0&pageSize=20
Host: api.example.com
Authorization: Bearer YOUR_TOKEN
```

### 5. Complex Multi-Filter Query
```http
GET /api/du-an/bao-cao-du-toan?tenDuAn=Hệ%20thống&loaiDuAnTheoNamId=3&donViPhuTrachChinhId=200&thoiGianKhoiCong=2024&pageIndex=1&pageSize=25
Host: api.example.com
Authorization: Bearer YOUR_TOKEN
```

---

## Response Format

### Success Response (HTTP 200)
```json
{
  "statusCode": 200,
  "message": "Success",
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "tenDuAn": "Dự án cơ sở dữ liệu tập trung",
        "donViPhuTrachChinhId": 123,
        "loaiDuAnTheoNamId": 2,
        "khaiToanKinhPhi": 5000000000.50,
        "thoiGianKhoiCong": 2024,
        "thoiGianHoanThanh": 2026,
        "duToanBanDau": 5000000000,
        "duToanDieuChinh": 5200000000,
        "tienDo": "Thiết kế chi tiết",
        "giaTriNghiemThu": 2500000000,
        "hinhThucDauTuId": 1,
        "loaiDuAnId": 5
      },
      {
        "id": "660e8400-e29b-41d4-a716-446655440111",
        "tenDuAn": "Hệ thống quản lý dự án",
        "donViPhuTrachChinhId": 200,
        "loaiDuAnTheoNamId": 1,
        "khaiToanKinhPhi": 3000000000.00,
        "thoiGianKhoiCong": 2023,
        "thoiGianHoanThanh": 2025,
        "duToanBanDau": 3000000000,
        "duToanDieuChinh": null,
        "tienDo": "Đang thực hiện",
        "giaTriNghiemThu": 1500000000,
        "hinhThucDauTuId": 2,
        "loaiDuAnId": 3
      }
    ],
    "totalCount": 42,
    "pageIndex": 0,
    "pageSize": 10,
    "totalPages": 5
  }
}
```

### Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique project identifier |
| `tenDuAn` | string | Project name |
| `donViPhuTrachChinhId` | long | Primary responsible department ID |
| `loaiDuAnTheoNamId` | integer | Capital classification type (1-4) |
| `khaiToanKinhPhi` | decimal | Cost estimation (Budget Declaration) |
| `thoiGianKhoiCong` | integer | Expected start year |
| `thoiGianHoanThanh` | integer | Expected completion year |
| `duToanBanDau` | long | Initial budget allocation (first budget record) |
| `duToanDieuChinh` | long? | Adjusted/supplementary budget (last record if >1, null if =1) |
| `tienDo` | string | Current implementation progress/step name |
| `giaTriNghiemThu` | long? | Total acceptance value across all phases |
| `hinhThucDauTuId` | integer | Investment form ID |
| `loaiDuAnId` | integer | Project type ID |
| `totalCount` | integer | Total number of records matching filters |
| `pageIndex` | integer | Current page index (0-based) |
| `pageSize` | integer | Records per page |
| `totalPages` | integer | Total number of pages |

### Budget Logic Explanation

**DuToanBanDau (Initial Budget):**
- Retrieves the first DuToan record for the project
- Represents the initial approved budget allocation

**DuToanDieuChinh (Adjusted Budget):**
- If the project has **more than 1** DuToan record: Returns the **last** DuToan
- If the project has **exactly 1** DuToan record: Returns **null**
- Represents budget adjustments/supplements if they exist

**GiaTriNghiemThu (Acceptance Value):**
- Sums all acceptance test values (NghiemThu.GiaTri) for the project
- Represents cumulative project deliverables acceptance
- Null if no acceptance records exist

---

## Error Responses

### Invalid Request (HTTP 400)
```json
{
  "statusCode": 400,
  "message": "Invalid request parameters",
  "errors": {
    "pageSize": "Page size must be between 1 and 100"
  }
}
```

### Unauthorized (HTTP 401)
```json
{
  "statusCode": 401,
  "message": "Unauthorized - Invalid or missing authentication token"
}
```

### Server Error (HTTP 500)
```json
{
  "statusCode": 500,
  "message": "An unexpected error occurred",
  "details": "Internal server error details"
}
```

---

## Response Headers

```
Content-Type: application/json
X-Page-Index: 0
X-Page-Size: 10
X-Total-Count: 42
X-Total-Pages: 5
```

---

## Filtering Logic

### Text Search (tenDuAn)
- **Type:** Partial match
- **Case-Sensitive:** No
- **Behavior:** Contains filter
- **Example:** `tenDuAn=hệ thống` matches "Hệ Thống Quản Lý Dự Án"

### Year Filters (thoiGianKhoiCong, thoiGianHoanThanh)
- **Type:** Exact match integer
- **Behavior:** Matches exact year values
- **Example:** `thoiGianKhoiCong=2024` returns projects starting in 2024 only

### Category Filters (loaiDuAnTheoNamId, hinhThucDauTuId, loaiDuAnId)
- **Type:** Exact match integer (ID)
- **Behavior:** Matches category ID exactly
- **Example:** `loaiDuAnTheoNamId=2` returns only transitional projects

### Department Filter (donViPhuTrachChinhId)
- **Type:** Long integer
- **Behavior:** Exact match by department ID
- **Example:** `donViPhuTrachChinhId=123` returns projects from department 123

### Filter Combinations
- All filters are **combined with AND logic**
- Order of parameters doesn't matter
- Missing parameters are ignored (optional)

---

## Pagination Details

### Pagination Parameters
- **pageIndex:** 0-based (0 = first page)
- **pageSize:** Records per page (minimum 1, maximum 100)
- **Default:** pageIndex=0, pageSize=10

### Example Pagination
```
Page 1: pageIndex=0, pageSize=10  (Records 1-10)
Page 2: pageIndex=1, pageSize=10  (Records 11-20)
Page 3: pageIndex=2, pageSize=10  (Records 21-30)
```

### Response Pagination Info
```json
{
  "data": {
    "items": [...],
    "totalCount": 42,      // Total matching records
    "pageIndex": 0,        // Current page
    "pageSize": 10,        // Records per page
    "totalPages": 5        // Total pages needed
  }
}
```

---

## Data Sources

| Field | Source Table | Column |
|-------|--------------|--------|
| tenDuAn | DuAn | TenDuAn |
| donViPhuTrachChinhId | DuAn | DonViPhuTrachChinhId |
| loaiDuAnTheoNamId | DuAn | LoaiDuAnTheoNamId |
| khaiToanKinhPhi | DuAn | KhaiToanKinhPhi |
| thoiGianKhoiCong | DuAn | ThoiGianKhoiCong |
| thoiGianHoanThanh | DuAn | ThoiGianHoanThanh |
| duToanBanDau | DuToan | SoDuToan (first record) |
| duToanDieuChinh | DuToan | SoDuToan (last record if count > 1) |
| tienDo | DuAnBuoc | TenBuoc |
| giaTriNghiemThu | NghiemThu | SUM(GiaTri) |
| hinhThucDauTuId | DuAn | HinhThucDauTuId |
| loaiDuAnId | DuAn | LoaiDuAnId |

---

## Performance Considerations

- **Pagination:** Applied at database level for efficiency
- **Eager Loading:** Related DuAnBuoc records loaded in single query
- **Batch Processing:** DuToan and NghiemThu records fetched once per page
- **No N+1:** Avoids repeated queries for related data
- **Tracking:** Read-only queries for optimal memory usage

---

## Related Endpoints

- `GET /api/du-an/danh-sach` - Standard project list
- `GET /api/du-an/{id}/chi-tiet` - Project details
- `GET /api/du-an/danh-sach-tre-han` - Overdue projects

---

## Change Log

### Version 1.0 (2024-12-31)
- Initial implementation
- Support for 7 filter parameters
- Pagination support
- Budget calculation logic implemented

---

## Notes

1. **No Database Changes:** This API uses existing database schema
2. **Backward Compatible:** Does not affect other APIs
3. **Security:** Requires proper authentication/authorization
4. **Performance:** Optimized for large datasets with pagination
5. **Null Handling:** Fields like `duToanDieuChinh` and `giaTriNghiemThu` can be null

---

**Last Updated:** 2024-12-31
**Status:** Production Ready
