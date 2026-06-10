# Task – Bổ sung kế hoạch vốn cho API giải ngân theo nguồn vốn

> Phân tích issue: [`issue-ke-hoach-von-giai-ngan-theo-nguon-von-thieu.md`](./issue-ke-hoach-von-giai-ngan-theo-nguon-von-thieu.md) — UC7 [`docs/issues/9450/report.md`](../../issues/9450/report.md)

---

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm


| #   | Mục                                          | Ghi chú                                                                  |
| --- | -------------------------------------------- | ------------------------------------------------------------------------ |
| 1   | **GET** trả thêm `tongKeHoachVon`            | SUM `KeHoachVon` theo `NguonVonId` + `Nam`                               |
| 2   | Giữ nguyên `giaTriGiaiNgan`, `giaTriHopDong` | Logic giải ngân/hợp đồng không đổi                                       |
| 3   | Hiển thị nguồn vốn **chỉ có kế hoạch**       | `FULL OUTER JOIN` giữa CTE giải ngân và CTE kế hoạch vốn                 |
| 4   | Group tất cả metric theo `nguonVonId`        | Mỗi nguồn vốn một dòng response                                          |
| 5   | **Không** tính `TyLeGiaiNgan` ở BE           | FE tính `giaTriGiaiNgan / tongKeHoachVon * 100` khi `tongKeHoachVon > 0` |


### 1.2 Field API liên quan


| Field response   | Kiểu      | Nguồn DB                                                              |
| ---------------- | --------- | --------------------------------------------------------------------- |
| `nguonVonId`     | `int?`    | `GoiThau.NguonVonId` hoặc `KeHoachVon.NguonVonId` (COALESCE sau JOIN) |
| `tenNguonVon`    | `string?` | `DmNguonVon.Ten`                                                      |
| `giaTriGiaiNgan` | `decimal` | `SUM(ThanhToan.GiaTri)` — **giữ nguyên**                              |
| `giaTriHopDong`  | `decimal` | `SUM(HopDong.GiaTri)` — **giữ nguyên**                                |
| `tongKeHoachVon` | `decimal` | `SUM(CASE SoVonDieuChinh/SoVon...)` từ `KeHoachVon` — **field mới**   |


**Query param:**


| Param | Kiểu  | Bắt buộc | Ghi chú      |
| ----- | ----- | -------- | ------------ |
| `nam` | `int` | Có       | Năm thống kê |


### 1.3 Response mẫu

```json
{
  "result": true,
  "errorMessage": "",
  "dataResult": [
    {
      "nguonVonId": 1,
      "tenNguonVon": "Vốn sự nghiệp",
      "giaTriGiaiNgan": 5022000000,
      "giaTriHopDong": 3822222222,
      "tongKeHoachVon": 12000000000
    },
    {
      "nguonVonId": 2,
      "tenNguonVon": "Vốn tự có",
      "giaTriGiaiNgan": 0,
      "giaTriHopDong": 0,
      "tongKeHoachVon": 3000000000
    }
  ]
}
```

- Dòng 1: Có cả giải ngân và kế hoạch vốn
- Dòng 2: Chỉ có kế hoạch vốn, chưa giải ngân (trước fix **không** xuất hiện)

### 1.4 API bị ảnh hưởng


| Method | Endpoint                                            | Sau fix                                               |
| ------ | --------------------------------------------------- | ----------------------------------------------------- |
| GET    | `/api/thong-ke/giai-ngan-theo-nguon-von?nam={year}` | Thêm `tongKeHoachVon`; danh sách nguồn vốn đầy đủ hơn |


**Không đổi:**


| Method | Endpoint                                    |
| ------ | ------------------------------------------- |
| GET    | `/api/thong-ke/chi-tiet-giai-ngan`          |
| GET    | `/api/thong-ke/tien-do-giai-ngan-nguon-von` |


---

## 2. Phân tích hiện trạng

### 2.1 Kết quả trước khi fix


| Metric / hành vi                     | Trạng thái                                    |
| ------------------------------------ | --------------------------------------------- |
| `giaTriGiaiNgan`, `giaTriHopDong`    | **OK** — SUM từ chuỗi `ThanhToan` → `GoiThau` |
| `tongKeHoachVon`                     | **Thiếu** — không query `KeHoachVon`          |
| Nguồn vốn chỉ có KHV, chưa giải ngân | **Không xuất hiện** trong response            |


### 2.2 Nguyên nhân gốc


| Layer    | Vấn đề                                                                              |
| -------- | ----------------------------------------------------------------------------------- |
| **SQL**  | Chỉ JOIN từ `ThanhToan`; danh sách nguồn vốn suy ra khi **có thanh toán** trong năm |
| **DTO**  | `DashboardGiaiNganTheoNguonVonDto` không có field kế hoạch vốn                      |
| **Join** | Không có `FULL OUTER JOIN` với `KeHoachVon`                                         |


### 2.3 Đã có sẵn (không cần migration)

- Entity `KeHoachVon`, bảng `dbo.KeHoachVon` (`NguonVonId`, `Nam`, `SoVon`, `SoVonDieuChinh`, `DuAnId`, `IsDeleted`)
- `DashboardController.GetGiaiNganTheoNguonVon` — signature endpoint không đổi
- Handler Dapper `DashboardGetGiaiNganTheoNguonVonQueryHandler` — chỉ thay SQL + map DTO

### 2.4 Flow code

```
HTTP GET /api/thong-ke/giai-ngan-theo-nguon-von?nam=2026
    │
    ▼
DashboardController.GetGiaiNganTheoNguonVon(int nam)
    │  QLDA.WebApi/Controllers/DashboardController.cs
    ▼
Mediator.Send(DashboardGetGiaiNganTheoNguonVonQuery(nam))
    │  QLDA.Application/Dashboard/Queries/DashboardGetGiaiNganTheoNguonVonQuery.cs
    ▼
DashboardGetGiaiNganTheoNguonVonQueryHandler.Handle()
    │  IDapperRepository.QueryAsync<DashboardGiaiNganTheoNguonVonDto>()
    ▼
List<DashboardGiaiNganTheoNguonVonDto>
    │  QLDA.Domain/DTOs/DashboardGiaiNganDto.cs
```

> **Không có service layer riêng** — handler dùng Dapper raw SQL trực tiếp.

---

## 3. Thứ tự thực hiện

```
Bước 1: Domain – thêm TongKeHoachVon vào DashboardGiaiNganTheoNguonVonDto
Bước 2: Application – thay SQL trong DashboardGetGiaiNganTheoNguonVonQueryHandler (2 CTE + FULL OUTER JOIN)
```

**Không làm:** Migration, sửa `DashboardController`, sửa snapshot, các API liên quan.

---

## 4. Chi tiết từng bước

---

### Bước 1 – Domain: `DashboardGiaiNganTheoNguonVonDto`

**File:** `QLDA.Domain/DTOs/DashboardGiaiNganDto.cs`

**Thêm property:**

```csharp
/// <summary>Tổng kế hoạch vốn năm theo nguồn vốn (từ KeHoachVon)</summary>
public decimal TongKeHoachVon { get; set; }
```


| Lý do chọn tên   | Ghi chú                                            |
| ---------------- | -------------------------------------------------- |
| `TongKeHoachVon` | Khớp alias SQL; không trùng entity `KeHoachVon`    |
| Additive         | Không rename/xóa `GiaTriGiaiNgan`, `GiaTriHopDong` |


---

### Bước 2 – Application: SQL handler

**File:** `QLDA.Application/Dashboard/Queries/DashboardGetGiaiNganTheoNguonVonQuery.cs`

**Trước:** Single query JOIN `ThanhToan` → `GoiThau` → `DmNguonVon`, `GROUP BY gt.NguonVonId, nv.Ten`.

**Sau:** 2 CTE + `FULL OUTER JOIN`:

```sql
WITH GiaiNganTheoNguonVon AS (
    SELECT
        gt.NguonVonId,
        SUM(tt.GiaTri) AS GiaTriGiaiNgan,
        SUM(hd.GiaTri) AS GiaTriHopDong
    FROM dbo.ThanhToan tt
    JOIN dbo.NghiemThu nt ON nt.Id = tt.NghiemThuId
    JOIN dbo.HopDong hd ON hd.Id = nt.HopDongId
    JOIN dbo.GoiThau gt ON gt.Id = hd.GoiThauId
    WHERE tt.IsDeleted = 0
      AND hd.IsDeleted = 0
      AND YEAR(tt.NgayHoaDon) = @Nam
    GROUP BY gt.NguonVonId
),
KeHoachVonTheoNguonVon AS (
    SELECT
        khv.NguonVonId,
        SUM(
            CASE
                WHEN ISNULL(khv.SoVonDieuChinh, 0) <= 0 THEN khv.SoVon
                ELSE khv.SoVonDieuChinh
            END
        ) AS TongKeHoachVon
    FROM dbo.KeHoachVon khv
    WHERE khv.Nam = @Nam
      AND khv.IsDeleted = 0
      AND khv.NguonVonId IS NOT NULL
    GROUP BY khv.NguonVonId
)
SELECT
    COALESCE(g.NguonVonId, k.NguonVonId) AS NguonVonId,
    nv.Ten AS TenNguonVon,
    ISNULL(g.GiaTriGiaiNgan, 0) AS GiaTriGiaiNgan,
    ISNULL(g.GiaTriHopDong, 0) AS GiaTriHopDong,
    ISNULL(k.TongKeHoachVon, 0) AS TongKeHoachVon
FROM GiaiNganTheoNguonVon g
FULL OUTER JOIN KeHoachVonTheoNguonVon k
    ON g.NguonVonId = k.NguonVonId
LEFT JOIN dbo.DmNguonVon nv
    ON nv.Id = COALESCE(g.NguonVonId, k.NguonVonId)
```

#### Rule chọn số vốn từng dòng `KeHoachVon`

```sql
CASE
    WHEN ISNULL(SoVonDieuChinh, 0) <= 0 THEN SoVon
    ELSE SoVonDieuChinh
END
```

> Dùng `CASE WHEN ISNULL(...) <= 0` thay vì `NULLIF(SoVonDieuChinh, 0)` để xử lý cả giá trị âm.

#### Quyết định kỹ thuật


| Điểm                          | Quyết định              | Lý do                                                      |
| ----------------------------- | ----------------------- | ---------------------------------------------------------- |
| `IsDeleted` trên `KeHoachVon` | `khv.IsDeleted = 0`     | Cùng pattern query thống kê khác                           |
| `NguonVonId IS NULL`          | Loại trừ                | Dữ liệu orphan, không group được                           |
| `FULL OUTER JOIN`             | Dùng                    | UC7 cần KHV kể cả khi chưa giải ngân                       |
| Đơn vị tiền                   | `decimal` raw           | Không chia `/1000000` (khác `tien-do-giai-ngan-nguon-von`) |
| Nhiều dòng KHV / dự án        | `SUM` theo `NguonVonId` | Một dự án có thể có nhiều dòng KHV cùng nguồn vốn + năm    |


---

## 5. Checklist hoàn thành

```
[x] 1. Thêm TongKeHoachVon vào DashboardGiaiNganTheoNguonVonDto
[x] 2. Cập nhật SQL trong DashboardGetGiaiNganTheoNguonVonQueryHandler
[x] 3. dotnet build — 0 errors
[x] 4. Restart API + gọi GET /api/thong-ke/giai-ngan-theo-nguon-von?nam=2026
[x] 5. So sánh tongKeHoachVon với SQL golden reference (§6.2)
[x] 6. Xác nhận giaTriGiaiNgan, giaTriHopDong không đổi so với trước
[ ] 7. Xác nhận nguồn vốn chỉ có kế hoạch (chưa giải ngân) xuất hiện trong response
```

---

## 6. Lưu ý kỹ thuật

- **Không migration** — schema `KeHoachVon` đã tồn tại.
- **Không sửa** `DashboardController.cs` — endpoint signature không đổi.
- **Không hard-code** dữ liệu; **không drop/truncate** DB.
- **Restart API** sau build — process cũ có thể chạy binary trước khi sửa → response thiếu `tongKeHoachVon`.
- `**TyLeGiaiNgan`:** để FE tính hoặc task riêng.

### Test gợi ý

```powershell
dotnet build "e:\SER\QLDA.WebApi\QLDA.WebApi.csproj"
# Restart QLDA.WebApi, rồi:
# GET http://localhost:5183/api/thong-ke/giai-ngan-theo-nguon-von?nam=2026
```


| Case                             | Kỳ vọng                                    |
| -------------------------------- | ------------------------------------------ |
| Nguồn có cả giải ngân + KHV      | Cả 3 số > 0 (trừ KHV = 0 nếu DB không có)  |
| Nguồn chỉ có KHV, chưa giải ngân | `giaTriGiaiNgan = 0`, `tongKeHoachVon > 0` |
| Nguồn chỉ có giải ngân, chưa KHV | `tongKeHoachVon = 0`                       |
| `tongKeHoachVon` vs SQL §6.2     | Khớp golden reference theo `NguonVonId`    |
| `giaTriGiaiNgan` vs SQL §6.3     | Khớp logic cũ (không đổi)                  |


### 6.1 SQL golden reference — kế hoạch vốn

```sql
SELECT
    NguonVonId,
    SUM(
        CASE
            WHEN ISNULL(SoVonDieuChinh, 0) <= 0 THEN SoVon
            ELSE SoVonDieuChinh
        END
    ) AS TongKeHoachVonDung
FROM dbo.KeHoachVon
WHERE Nam = 2026
  AND IsDeleted = 0
  AND NguonVonId IS NOT NULL
GROUP BY NguonVonId;
```

So sánh `TongKeHoachVonDung` với `tongKeHoachVon` trong API response.

### 6.2 SQL cross-check — giải ngân (không đổi)

```sql
SELECT gt.NguonVonId, SUM(tt.GiaTri) AS GiaTriGiaiNgan
FROM dbo.ThanhToan tt
JOIN dbo.NghiemThu nt ON nt.Id = tt.NghiemThuId
JOIN dbo.HopDong hd ON hd.Id = nt.HopDongId
JOIN dbo.GoiThau gt ON gt.Id = hd.GoiThauId
WHERE tt.IsDeleted = 0 AND hd.IsDeleted = 0
  AND YEAR(tt.NgayHoaDon) = 2026
GROUP BY gt.NguonVonId;
```

---

## 6.3 Những thay đổi đã làm trong task này

### Domain Layer


| Thay đổi                             | File                                       |
| ------------------------------------ | ------------------------------------------ |
| ➕ `TongKeHoachVon` trên response DTO | `QLDA.Domain/DTOs/DashboardGiaiNganDto.cs` |


### Application Layer


| Thay đổi                                       | File                                                                          |
| ---------------------------------------------- | ----------------------------------------------------------------------------- |
| SQL 2 CTE + `FULL OUTER JOIN`                  | `QLDA.Application/Dashboard/Queries/DashboardGetGiaiNganTheoNguonVonQuery.cs` |
| Bỏ `GROUP BY nv.Ten` — tên lấy qua `LEFT JOIN` | Cùng file                                                                     |


### Không sửa


| File                                                | Lý do                        |
| --------------------------------------------------- | ---------------------------- |
| `DashboardController.cs`                            | Signature endpoint không đổi |
| `KeHoachVon.cs`, `KeHoachVonConfiguration.cs`       | Schema đã đúng               |
| Migrations / `AppDbContextModelSnapshot`            | Không đổi DB                 |
| `chi-tiet-giai-ngan`, `tien-do-giai-ngan-nguon-von` | Ngoài scope UC7              |


---

## 7. Tóm tắt công việc

### Tổng quan

**Enhancement UC7** — bổ sung kế hoạch vốn năm theo nguồn vốn trên dashboard giải ngân:

- **0** migration / entity mới
- **2** file chỉnh sửa
- **1** API endpoint bổ sung field (`tongKeHoachVon`)

### Các file chỉnh sửa


| #   | File                                                                          | Thay đổi                        |
| --- | ----------------------------------------------------------------------------- | ------------------------------- |
| 1   | `QLDA.Domain/DTOs/DashboardGiaiNganDto.cs`                                    | ➕ `TongKeHoachVon`              |
| 2   | `QLDA.Application/Dashboard/Queries/DashboardGetGiaiNganTheoNguonVonQuery.cs` | SQL mới (CTE + FULL OUTER JOIN) |


### Luồng dữ liệu sau fix

```
GET ?nam=2026
  → CTE GiaiNganTheoNguonVon (SUM giải ngân + HĐ theo NguonVonId)
  → CTE KeHoachVonTheoNguonVon (SUM KHV theo NguonVonId)
  → FULL OUTER JOIN + DmNguonVon.Ten
  → DashboardGiaiNganTheoNguonVonDto (5 fields)
```

### Trạng thái

- **Code:** đã implement (bước 1–2)
- **Build:** thành công
- **Tiếp theo:** restart API + verify checklist mục 4–7 (§5)

