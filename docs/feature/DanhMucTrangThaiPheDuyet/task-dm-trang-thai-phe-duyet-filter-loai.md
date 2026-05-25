# Task – Lọc danh mục trạng thái phê duyệt theo `Loai` (Cách 2)

**Phạm vi:** Chỉ endpoint `GET /api/danh-muc-trang-thai-phe-duyet/danh-sach-day-du`  
**Phương án:** **Cách 2** — API nhận query `Loai`, backend filter trực tiếp trên `DmTrangThaiPheDuyet.Loai`, chỉ trả item thuộc loại đó.  


---

## Summary

| Hạng mục | Nội dung |
|----------|----------|
| Vấn đề | `danh-sach-day-du` trả **toàn bộ** bản ghi `DmTrangThaiPheDuyet` (mọi `Loai` trộn chung) |
| Yêu cầu | FE gọi kèm `Loai` (vd. `PheDuyetDuToan`) → BE chỉ trả 4–5 trạng thái của nghiệp vụ đó |
| Không đổi | `GET danh-sach`, `POST them-moi`, `PUT cap-nhat`, `DELETE xoa-tam`, `GET {id}` |
| DB / Migration | **Không** — cột `Loai` đã có trên entity + seed |

### API map (sau khi làm)

```
GET /api/danh-muc-trang-thai-phe-duyet/danh-sach-day-du?Loai=PheDuyetDuToan&pageIndex=0&pageSize=20&globalFilter=
```

**Không** thêm `Loai` vào `GET .../danh-sach`.

---

## 1. Phân tích yêu cầu

### 1.1 Bối cảnh

Bảng `dbo.DmTrangThaiPheDuyet` dùng chung cho nhiều luồng phê duyệt. Mỗi nhóm trạng thái được phân biệt bởi cột **`Loai`** (string), đồng bộ với `PheDuyetEntityNames` (vd. `PheDuyetDuToan`, `BaoCaoKetQuaKhaoSat`, …).

Hiện tại FE load dropdown/combobox trạng thái qua `danh-sach-day-du` nhưng nhận **list gộp** → phải tự lọc client (Cách 1) hoặc gọi API mới (không chọn).

**Cách 2 (đã chốt):** Backend lọc theo `Loai` ngay trong query.

### 1.2 Việc cần làm

| # | Mục | Ghi chú |
|---|-----|---------|
| 1 | Thêm query param **`Loai`** | Trên action `GetAll` (`danh-sach-day-du`) |
| 2 | Filter EF/query | `WHERE Loai = @loai` (và giữ logic `IsDeleted`, `Used`, phân trang, `globalFilter` hiện có) |
| 3 | Tương thích ngược | `Loai` **null/empty** → hành vi như hiện tại (trả tất cả, không filter theo loại) |
| 4 | Không đổi contract response | Vẫn `PaginatedList<DanhMucDto<int>>` qua `ResultApi` |

### 1.3 Out of scope

| Mục | Lý do |
|-----|--------|
| Endpoint `danh-sach` | PM/ảnh task: **“api danh-sach-day-du thôi nha”** |
| Thêm field `loai` vào `DanhMucDto` response | Không yêu cầu; FE đã biết `Loai` từ request |
| Migration / seed mới | Schema đã đủ |
| API CRUD danh mục khác | Chỉ `DanhMucTrangThaiPheDuyet` |

### 1.4 Giá trị `Loai` hợp lệ

Tham chiếu `QLDA.Domain/Constants/PheDuyetEntityNames.cs` (đồng nhất seed `DanhMucTrangThaiPheDuyetConfiguration`):

| Constant | Giá trị `Loai` | Ghi chú |
|----------|----------------|---------|
| `PheDuyetDuToan` | `PheDuyetDuToan` | Id seed 1–5 |
| `HoSoDeXuatCapDoCntt` | `HoSoDeXuatCapDoCntt` | Id 7–11 |
| `HoSoMoiThauDienTu` | `HoSoMoiThauDienTu` | Id 12–16 |
| `PhanKhaiKinhPhi` | `PhanKhaiKinhPhi` | Id 17–20 (không TC) |
| `QuyetDinhDieuChinh` | `QuyetDinhDieuChinh` | Id 21–25 |
| `BaoCaoKetQuaKhaoSat` | `BaoCaoKetQuaKhaoSat` | Id 26–29 |
| `ToTrinhKeHoach` | `ToTrinhKeHoach` | (nếu đã seed) |
| `DeXuatMacDinhStt` | `DeXuatMacDinh` | Tên constant ≠ giá trị string |
| `Default` | `Default` | Bản ghi legacy `LEG` (Id 6), `Used = false` |

> FE truyền **đúng chuỗi** như cột DB / `PheDuyetEntityNames`, không phải Description tiếng Việt.

**Ví dụ SQL tương đương:**

```sql
SELECT * FROM dbo.DmTrangThaiPheDuyet
WHERE Loai = N'PheDuyetDuToan' AND IsDeleted = 0;
```

---

## 2. Phân tích hiện trạng

### 2.1 Controller

**File:** `QLDA.WebApi/Controllers/DanhMucTrangThaiPheDuyetController.cs`

```csharp
[HttpGet("danh-sach-day-du")]
public async Task<ResultApi> GetAll([FromQuery] AggregateRootPagination req, string? globalFilter) {
    var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
        DanhMuc = EDanhMuc.DanhMucTrangThaiPheDuyet,
        PageIndex = req.PageIndex,
        GlobalFilter = globalFilter,
        PageSize = req.PageSize,
        GetAll = true
    });
    return ResultApi.Ok(res);
}
```

- **Chưa** có tham số `Loai`.
- `GetAll = true` → trong handler vẫn áp dụng `Used` filter theo logic `WhereIf` (xem §2.2).

### 2.2 Query handler dùng chung

**File:** `QLDA.Application/Common/Queries/DanhMucGetDanhSachQuery.cs`

- `DanhMucTrangThaiPheDuyet` đi qua `GetDanhMucAsync<DanhMucTrangThaiPheDuyet, int, DanhMucDto<int>>`.
- Method generic chỉ filter: `!IsDeleted`, `Ids`, `Used`/`GetAll`, `GlobalFilter` (Ten/MoTa).
- **Không** đọc property `Loai` vì `DanhMuc<TKey>` base không có field này.

### 2.3 Entity & DB

| Thành phần | Chi tiết |
|------------|----------|
| Entity | `DanhMucTrangThaiPheDuyet.Loai` (`string?`) |
| Bảng | `DmTrangThaiPheDuyet` |
| Seed | `DanhMucTrangThaiPheDuyetConfiguration.cs` — mỗi nhóm 4–5 dòng, `Loai = PheDuyetEntityNames.*` |

### 2.4 Response hiện tại

`DanhMucDto<int>` (`BuildingBlocks.Application`) — các field: `id`, `ma`, `ten`, `moTa`, `stt`, `used`. **Không** có `loai` trong list DTO (chi tiết `{id}` dùng `DanhMucTrangThaiPheDuyetModel` có `Loai`).

---

## 3. Thứ tự thực hiện

```
Bước 1: Application — Thêm property Loai vào DanhMucGetDanhSachQuery
Bước 2: Application — Filter theo Loai (chỉ nhánh DanhMucTrangThaiPheDuyet)
Bước 3: WebApi — Truyền query param Loai từ controller danh-sach-day-du
Bước 4: Build + test manual (Postman / FE)
```

**Không có** bước Domain / Persistence / Migrator.

---

## 4. Chi tiết từng bước

### Bước 1 – Application: mở rộng `DanhMucGetDanhSachQuery`

**File:** `QLDA.Application/Common/Queries/DanhMucGetDanhSachQuery.cs`

Thêm property (optional):

```csharp
/// <summary>
/// Lọc danh mục trạng thái phê duyệt theo Loai (PheDuyetEntityNames).
/// Chỉ áp dụng khi DanhMuc = DanhMucTrangThaiPheDuyet.
/// </summary>
public string? Loai { get; set; }
```

### Bước 2 – Application: filter trong handler

**Cách triển khai đề xuất (tối thiểu diff):** tách nhánh riêng thay vì sửa generic `GetDanhMucAsync` cho mọi danh mục.

Trong `Handle`, thay dòng:

```csharp
EDanhMuc.DanhMucTrangThaiPheDuyet => await GetDanhMucAsync<...>(...)
```

bằng gọi method mới, ví dụ `GetDanhMucTrangThaiPheDuyetAsync(request, cancellationToken)`.

**Logic filter (pseudo):**

```csharp
private async Task<PaginatedList<DanhMucDto<int>>> GetDanhMucTrangThaiPheDuyetAsync(
    DanhMucGetDanhSachQuery request,
    CancellationToken cancellationToken) {
    var query = DanhMucTrangThaiPheDuyet.GetQueryableSet()
        .Where(x => !x.IsDeleted);

    if (!string.IsNullOrWhiteSpace(request.Loai))
        query = query.Where(x => x.Loai == request.Loai);

    // Giữ nguyên WhereIf Used/Ids/GetAll như GetDanhMucAsync
    // Giữ nguyên GlobalFilter (Ten/MoTa) nếu có
    // PaginatedListAsync(request.Skip(), request.Take(), ...)
}
```

**Quy tắc:**

| `Loai` request | Kết quả |
|----------------|---------|
| `null` / `""` / whitespace | Không filter `Loai` (giữ behavior cũ) |
| `"PheDuyetDuToan"` | Chỉ Id 1–5 (và bỏ qua nhóm khác) |
| Giá trị không tồn tại trong DB | `data: []`, `totalCount: 0` (200 OK) — **không** bắt buộc 400 |

> Nếu product muốn **bắt buộc** `Loai` trên `danh-sach-day-du`, thêm validation ở controller — hiện doc giả định **optional** để không break client cũ.

### Bước 3 – WebApi: controller

**File:** `QLDA.WebApi/Controllers/DanhMucTrangThaiPheDuyetController.cs`

```csharp
[HttpGet("danh-sach-day-du")]
public async Task<ResultApi> GetAll(
    [FromQuery] AggregateRootPagination req,
    string? globalFilter,
    string? loai) {  // hoặc [FromQuery(Name = "Loai")] string? loai
    var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
        DanhMuc = EDanhMuc.DanhMucTrangThaiPheDuyet,
        PageIndex = req.PageIndex,
        PageSize = req.PageSize,
        GlobalFilter = globalFilter,
        GetAll = true,
        Loai = loai
    });
    return ResultApi.Ok(res);
}
```

**Swagger:** ghi chú query `Loai` — ví dụ `PheDuyetDuToan`, `BaoCaoKetQuaKhaoSat`.

---

## 5. API contract (FE / QA)

### 5.1 Request

| Method | Route | Auth |
|--------|-------|------|
| `GET` | `/api/danh-muc-trang-thai-phe-duyet/danh-sach-day-du` | `GroupAdminOrManager` (như hiện tại) |

**Query parameters:**

| Param | Kiểu | Bắt buộc | Mô tả |
|-------|------|----------|--------|
| `pageIndex` | int | Có (pagination) | Như `AggregateRootPagination` |
| `pageSize` | int | Có | |
| `globalFilter` | string? | Không | Tìm theo `Ten` / `MoTa` |
| **`Loai`** | string? | Không* | Filter server-side theo cột `Loai` |

\*Khuyến nghị FE **luôn truyền** `Loai` theo nghiệp vụ màn hình; BE vẫn chấp nhận không truyền.

**Ví dụ:**

```http
GET /api/danh-muc-trang-thai-phe-duyet/danh-sach-day-du?Loai=BaoCaoKetQuaKhaoSat&pageIndex=0&pageSize=50
```

### 5.2 Response

Không đổi shape — `ResultApi` bọc `PaginatedList<DanhMucDto<int>>`:

```json
{
  "success": true,
  "data": {
    "data": [
      { "id": 26, "ma": "DT", "ten": "Dự thảo", "moTa": null, "stt": 1, "used": true }
    ],
    "pageIndex": 0,
    "pageSize": 50,
    "totalCount": 4
  }
}
```

### 5.3 So sánh Cách 1 vs Cách 2

| | Cách 1 (FE filter) | Cách 2 (đã chọn) |
|--|-------------------|------------------|
| API | Không đổi BE | Thêm param `Loai` |
| Payload | Trả full list | Chỉ subset |
| DB round-trip | Lớn hơn cần thiết | Đúng nhóm trạng thái |
| Bảo trì | FE phải biết Id/Ma từng loại | FE chỉ cần constant `Loai` |

---

## 6. Checklist hoàn thành

```
[ ] 1. Thêm `Loai` vào `DanhMucGetDanhSachQuery`
[ ] 2. Handler: filter `DanhMucTrangThaiPheDuyet` theo `Loai` (nhánh riêng)
[ ] 3. Controller `danh-sach-day-du`: bind query `Loai`
[ ] 4. `dotnet build` solution
[ ] 5. Test: Loai=PheDuyetDuToan → ~5 bản ghi Used
[ ] 6. Test: Loai=BaoCaoKetQuaKhaoSat → 4 bản ghi (26–29)
[ ] 7. Test: không truyền Loai → giống response trước khi sửa
[ ] 8. Test: Loai=KhongTonTai → data rỗng, 200
[ ] 9. Xác nhận `danh-sach` không đổi behavior
```

---

## 7. Test plan

### 7.1 Postman / curl

1. **Filter theo dự toán**
   - `GET .../danh-sach-day-du?Loai=PheDuyetDuToan&pageIndex=0&pageSize=100`
   - Expect: chỉ mã `DT`, `ĐTr`, `ĐD`, `TL`, `TC` (Id 1–5), không có Id 26–29.

2. **Filter báo cáo khảo sát**
   - `Loai=BaoCaoKetQuaKhaoSat`
   - Expect: Id 26–29, **không** có `TC`.

3. **Backward compatible**
   - Gọi không có `Loai` → số lượng bản ghi ≥ từng case trên (full catalog Used).

4. **Global filter + Loai**
   - `Loai=PheDuyetDuToan&globalFilter=dự`
   - Expect: intersection (cả hai điều kiện).

### 7.2 FE integration

- Màn hình phê duyệt / dropdown trạng thái: truyền `Loai` = `PheDuyetEntityNames` của module (cùng giá trị dùng trong `PheDuyetGetDanhSachQuery?type=...` nếu có).
- Không gọi `danh-sach` để thay thế trừ khi cần list không phân trang (ngoài scope task).

---

## 8. Lưu ý kỹ thuật

- **Commit:** chỉ Application + WebApi (một commit nhỏ, không Migrator).
- **Không** sửa `DanhMucDto` / mapping chung trừ khi product yêu cầu trả thêm `loai` trong list.
- Property entity là `Loai` (PascalCase); ASP.NET Core bind query `Loai` hoặc `loai` (case-insensitive).
- Record `LEG` (`Loai = Default`, `Used = false`) thường không xuất hiện khi `GetAll = true` + filter `Used` — giữ nguyên logic handler, không đổi trong task này trừ khi bug hiện hữu.
- Tham chiếu implement workflow dùng `Loai`: các `*Command` trong Application (vd. `s.Loai == PheDuyetEntityNames.PheDuyetDuToan`).

---

## 9. Files dự kiến thay đổi

| # | File | Thay đổi |
|---|------|----------|
| 1 | `QLDA.Application/Common/Queries/DanhMucGetDanhSachQuery.cs` | `+ Loai`, `+ GetDanhMucTrangThaiPheDuyetAsync` |
| 2 | `QLDA.WebApi/Controllers/DanhMucTrangThaiPheDuyetController.cs` | `+ param loai` trên `GetAll` |

**Ước lượng:** ~30–50 dòng, không file mới bắt buộc.

---

## 10. Liên kết

| Tài liệu / code | Mục đích |
|-----------------|----------|
| `docs/feature/KySo/task-9460-ky-so-crud.md` | Format task doc |
| `docs/feature/BaoCaoKetQuaKhaoSat/task-9474-uc55-bao-cao-ket-qua-khao-sat.md` | Ví dụ seed `Loai = BaoCaoKetQuaKhaoSat` |
| `QLDA.Domain/Constants/PheDuyetEntityNames.cs` | Danh sách giá trị `Loai` |
| `QLDA.Persistence/Configurations/DanhMuc/DanhMucTrangThaiPheDuyetConfiguration.cs` | Seed data |
