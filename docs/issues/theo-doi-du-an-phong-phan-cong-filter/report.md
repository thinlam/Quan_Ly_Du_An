# Report — Bổ sung filter dashboard cho API theo dõi dự án phòng phân công

*Survey codebase — 30/06/2026 · Implemented — 30/06/2026*

---

## 0. Trạng thái

| Hạng mục | Trạng thái | Ghi chú |
| -------- | ---------- | ------- |
| Investigation / docs | ✅ Done | File này |
| `TheoDoiDuAnPhongPhanCongSearchDto` | ✅ Done | `ThoiGianKhoiCong`, `ThoiGianHoanThanh` |
| `TheoDoiDuAnPhongPhanCongQuery.BuildQuery` | ✅ Done | 2 `WhereIf` + dọn duplicate `LanhDaoPhuTrachId` |
| `TrangThaiDuAnId`, `NamDuAn` | ✅ Có sẵn | Từ #9527 — không đổi logic |
| `DuAnController` | ✅ Không sửa | Bind DTO Application trực tiếp |
| Migration | ✅ Không cần | Chỉ query/filter |
| `dotnet build` | ✅ Pass | 0 Error(s) |
| Smoke test manual | ⏳ Pending | Mẫu ở [§8](#8-smoke-test-manual) |

---

## 1. Tóm tắt

| Thuộc tính | Giá trị |
| ---------- | ------- |
| Issue | API thiếu filter `thoiGianKhoiCong`, `thoiGianHoanThanh` trên dashboard |
| Status | ✅ **IMPLEMENTED** |
| Method | `GET` |
| URL | `/QuanLyDuAn/api/du-an/theo-doi-du-an-phong-phan-cong` |
| Controller | `DuAnController.GetTheoDoiDuAnPhongPhanCong` |
| Handler | `TheoDoiDuAnPhongPhanCongQuery` / `TheoDoiDuAnPhongPhanCongQueryHandler` |
| Search DTO | `TheoDoiDuAnPhongPhanCongSearchDto` |
| Response | `TheoDoiDuAnPhongPhanCongResultDto` (4 counter + `danhSach` phân trang) |
| Migration | **Không** cần |
| Pattern tham chiếu | `DuAnGetDanhSachQuery`, `DuAnSearchDto` |

---

## 2. Luồng xử lý

```
GET /api/du-an/theo-doi-du-an-phong-phan-cong?[query]
        │
        ▼
DuAnController.GetTheoDoiDuAnPhongPhanCong([FromQuery] TheoDoiDuAnPhongPhanCongSearchDto)
        │
        ▼
TheoDoiDuAnPhongPhanCongQueryHandler.Handle
        │
        ├─ authManager.FilterVisible(duAn.GetQueryableSet(), DuAn)
        │
        ├─ BuildQuery(..., loai = TatCa)  → counter tongSoDuAn / conHan / quaHan / daHoanThanh
        │
        └─ BuildQuery(..., loai = search.Loai) → ProjectToDto → PaginatedList → danhSach
```

Filter màn hình nằm trong `BuildQuery` — counter và list dùng cùng subset.

**File tham chiếu:**

| Thành phần | Vị trí |
| ---------- | ------ |
| Controller | `QLDA.WebApi/Controllers/DuAnController.cs` (~dòng 115–122) |
| Search DTO | `QLDA.Application/DuAns/DTOs/TheoDoiDuAnPhongPhanCongSearchDto.cs` |
| Handler | `QLDA.Application/DuAns/Queries/TheoDoiDuAnPhongPhanCongQuery.cs` |
| Pattern danh sách | `QLDA.Application/DuAns/Queries/DuAnGetDanhSachQuery.cs` |

---

## 3. Trạng thái code sau fix

### 3.1 Ma trận filter yêu cầu

| Param query | Property DTO | `BuildQuery` | Trạng thái |
| ----------- | ------------ | ------------ | ---------- |
| `thoiGianKhoiCong` | ✅ `ThoiGianKhoiCong` | ✅ `WhereIf(... > 0)` | **Đã thêm** |
| `thoiGianHoanThanh` | ✅ `ThoiGianHoanThanh` | ✅ `WhereIf(... > 0)` | **Đã thêm** |
| `trangThaiDuAnId` | ✅ `TrangThaiDuAnId` | ✅ `WhereIf(... > 0)` | Có sẵn #9527 |
| `namDuAn` | ✅ `NamDuAn` | ✅ Logic #9121 | Có sẵn #9527 |

### 3.2 Search DTO — sau fix

```csharp
// TheoDoiDuAnPhongPhanCongSearchDto — SAU FIX (rút gọn)
public record TheoDoiDuAnPhongPhanCongSearchDto : CommonSearchDto
{
    public long? DonViPhuTrachChinhId { get; set; }
    public long? LanhDaoPhuTrachId { get; set; }
    public int? NamDuAn { get; set; }
    public int? ThoiGianKhoiCong { get; set; }   // ✅ mới
    public int? ThoiGianHoanThanh { get; set; }  // ✅ mới
    public string? TenDuAn { get; set; }
    public string? MaDuAn { get; set; }
    public int? TrangThaiDuAnId { get; set; }
    public ETheoDoiDuAnPhongPhanCongLoai Loai { get; set; } = ETheoDoiDuAnPhongPhanCongLoai.TatCa;
}
```

### 3.3 Handler `BuildQuery` — sau fix

```csharp
// TheoDoiDuAnPhongPhanCongQuery.BuildQuery — SAU FIX (rút gọn, dòng 103–114)
    .WhereIf(search.TenDuAn.IsNotNullOrWhitespace(), ...)
    .WhereIf(search.MaDuAn.IsNotNullOrWhitespace(), ...)
    .WhereIf(search.ThoiGianKhoiCong > 0, e => e.ThoiGianKhoiCong == search.ThoiGianKhoiCong)
    .WhereIf(search.ThoiGianHoanThanh > 0, e => e.ThoiGianHoanThanh == search.ThoiGianHoanThanh)
    .WhereIf(search.NamDuAn > 0,
        e => search.NamDuAn >= e.ThoiGianKhoiCong
             && ((e.ThoiGianHoanThanh == null && e.ThoiGianKhoiCong == search.NamDuAn)
                 || search.NamDuAn <= e.ThoiGianHoanThanh))
    .WhereIf(search.TrangThaiDuAnId > 0, e => e.TrangThaiDuAnId == search.TrangThaiDuAnId)
    .WhereGlobalFilter(search, e => e.TenDuAn, e => e.MaDuAn);
```

**Dọn kỹ thuật:** Đã xóa block `LanhDaoPhuTrachId` bị duplicate (trước đây filter 2 lần ở dòng 100–102 và 111–113).

### 3.4 Trước fix (tham chiếu)

| Vấn đề | Mô tả |
| ------ | ----- |
| DTO thiếu property | `?thoiGianKhoiCong=2024` bị ASP.NET Core bỏ qua |
| `BuildQuery` thiếu WHERE | Counter + list không lọc theo năm KC/HT |
| Duplicate filter | `LanhDaoPhuTrachId` áp dụng 2 lần (vô hại nhưng thừa) |

---

## 4. Model dữ liệu

| Entity | Field | Kiểu | Ghi chú |
| ------ | ----- | ---- | ------- |
| `DuAn` | `ThoiGianKhoiCong` | `int?` | **Năm** khởi công |
| `DuAn` | `ThoiGianHoanThanh` | `int?` | **Năm** hoàn thành dự kiến |
| `DuAn` | `TrangThaiDuAnId` | `int?` | FK → `DanhMucTrangThaiDuAn` |

**Seed trạng thái dự án:**

| Id | Ma | Ten |
| -- | -- | --- |
| 1 | `DTH` | Đang thực hiện |
| 2 | `PDDT` | Đã phê duyệt đầu tư |
| 3 | `HT` | Đã hoàn thành |
| 4 | `TD` | Tạm dừng |

---

## 5. Semantics từng filter

### 5.1 `thoiGianKhoiCong`

| Input | Áp dụng WHERE? | Điều kiện |
| ----- | -------------- | --------- |
| `null` / không truyền | Không | — |
| `≤ 0` | Không | Coi như không chọn |
| `> 0` | Có | `e.ThoiGianKhoiCong == value` |

### 5.2 `thoiGianHoanThanh`

| Input | Áp dụng WHERE? | Điều kiện |
| ----- | -------------- | --------- |
| `null` / không truyền | Không | — |
| `≤ 0` | Không | — |
| `> 0` | Có | `e.ThoiGianHoanThanh == value` |

### 5.3 `trangThaiDuAnId`

| Input | Áp dụng WHERE? | Điều kiện |
| ----- | -------------- | --------- |
| `null` / không truyền / `≤ 0` | Không | — |
| `> 0` | Có | `e.TrangThaiDuAnId == value` |

### 5.4 `namDuAn` (#9121)

```
ThoiGianKhoiCong <= namDuAn <= ThoiGianHoanThanh
```

| `ThoiGianHoanThanh` | Điều kiện |
| ------------------- | --------- |
| Có giá trị | `namDuAn >= ThoiGianKhoiCong AND namDuAn <= ThoiGianHoanThanh` |
| `null` | `ThoiGianKhoiCong == namDuAn` |

### 5.5 Kết hợp filter

Tất cả filter màn hình kết hợp bằng **AND**.

### 5.6 Tương tác với panel `loai`

| `loai` | Điều kiện bổ sung |
| ------ | ----------------- |
| `TatCa` (0) | Không thêm |
| `DaHoanThanh` | `TrangThaiDuAnId == hoanThanhId` |
| `ConHan` | Chưa HT và (`ThoiGianHoanThanh == null` hoặc `namHienTai <= ThoiGianHoanThanh`) |
| `QuaHan` | Chưa HT và `ThoiGianHoanThanh != null` và `namHienTai > ThoiGianHoanThanh` |

Counter 4 panel dùng `loai = TatCa` nhưng **vẫn** áp filter màn hình.

---

## 6. Files đã sửa

| # | File | Thay đổi |
| - | ---- | -------- |
| 1 | `QLDA.Application/DuAns/DTOs/TheoDoiDuAnPhongPhanCongSearchDto.cs` | Thêm `ThoiGianKhoiCong`, `ThoiGianHoanThanh` |
| 2 | `QLDA.Application/DuAns/Queries/TheoDoiDuAnPhongPhanCongQuery.cs` | 2 `WhereIf` + xóa duplicate `LanhDaoPhuTrachId` |

**Không sửa:** `DuAnController`, migration, `AppDbContextModelSnapshot`, WebApi model.

---

## 7. Thay đổi đã thực hiện (chi tiết)

### Bước 1 — `TheoDoiDuAnPhongPhanCongSearchDto` ✅

Thêm 2 property cạnh `NamDuAn`:

```csharp
    /// <summary>Năm khởi công — logic giống DuAnSearchDto / DuAnGetDanhSachQuery</summary>
    public int? ThoiGianKhoiCong { get; set; }

    /// <summary>Năm hoàn thành — logic giống DuAnSearchDto / DuAnGetDanhSachQuery</summary>
    public int? ThoiGianHoanThanh { get; set; }
```

### Bước 2 — `BuildQuery` ✅

```diff
             .WhereIf(search.MaDuAn.IsNotNullOrWhitespace(), ...)
+            .WhereIf(search.ThoiGianKhoiCong > 0, e => e.ThoiGianKhoiCong == search.ThoiGianKhoiCong)
+            .WhereIf(search.ThoiGianHoanThanh > 0, e => e.ThoiGianHoanThanh == search.ThoiGianHoanThanh)
             .WhereIf(search.NamDuAn > 0, ...)
-            .WhereFunc(search.LanhDaoPhuTrachId.HasValue, ...)  // block duplicate — đã xóa
             .WhereIf(search.TrangThaiDuAnId > 0, ...)
```

### Bước 3 — Controller ✅ (read-only, không sửa)

```csharp
[HttpGet("api/du-an/theo-doi-du-an-phong-phan-cong")]
public async Task<ResultApi> GetTheoDoiDuAnPhongPhanCong(
    [FromQuery] TheoDoiDuAnPhongPhanCongSearchDto searchDto)
```

### Bước 4 — Build ✅

```powershell
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
# Kết quả: 0 Error(s)
```

---

## 8. Smoke test manual

⏳ **Chưa chạy trên môi trường deploy** — dùng checklist sau khi deploy.

### 8.1 Baseline

```http
GET /QuanLyDuAn/api/du-an/theo-doi-du-an-phong-phan-cong?pageIndex=1&pageSize=10
```

### 8.2 Filter từng param

```http
GET .../theo-doi-du-an-phong-phan-cong?thoiGianKhoiCong=2024&pageIndex=1&pageSize=100
GET .../theo-doi-du-an-phong-phan-cong?thoiGianHoanThanh=2026&pageIndex=1&pageSize=100
GET .../theo-doi-du-an-phong-phan-cong?trangThaiDuAnId=1&pageIndex=1&pageSize=100
GET .../theo-doi-du-an-phong-phan-cong?namDuAn=2025&pageIndex=1&pageSize=100
```

### 8.3 Kết hợp + đối chiếu danh sách dự án

```http
GET .../theo-doi-du-an-phong-phan-cong?thoiGianKhoiCong=2024&trangThaiDuAnId=1&namDuAn=2025&pageIndex=1&pageSize=10

GET /QuanLyDuAn/api/du-an/danh-sach?thoiGianKhoiCong=2024&trangThaiDuAnId=1&namDuAn=2025&pageIndex=1&pageSize=1
```

### 8.4 Counter đồng bộ

```http
GET .../theo-doi-du-an-phong-phan-cong?namDuAn=2025&loai=0
```

Kỳ vọng: `tongSoDuAn == conHan + quaHan + daHoanThanh`; `loai=1|2|3` → `danhSach.totalRows` khớp counter panel.

---

## 9. Acceptance criteria

| # | Kịch bản | Kỳ vọng | Verify |
| - | -------- | ------- | ------ |
| AC1 | Không truyền 4 param | Hành vi giữ nguyên | ⏳ |
| AC2 | Chỉ `thoiGianKhoiCong` | `DuAn.ThoiGianKhoiCong == value` | ⏳ |
| AC3 | Chỉ `thoiGianHoanThanh` | `DuAn.ThoiGianHoanThanh == value` | ⏳ |
| AC4 | Chỉ `trangThaiDuAnId` | `DuAn.TrangThaiDuAnId == value` | ⏳ |
| AC5 | Chỉ `namDuAn` | Logic #9121 | ⏳ |
| AC6 | Nhiều filter | AND | ⏳ |
| AC7 | Counter 4 panel | Cùng `BuildQuery` | ⏳ |
| AC8 | Response shape | Không đổi JSON | ✅ |
| AC9 | Build | 0 Error(s) | ✅ |

---

## 10. Checklist nghiệm thu

- [x] `TheoDoiDuAnPhongPhanCongSearchDto` có `ThoiGianKhoiCong`, `ThoiGianHoanThanh`
- [x] `BuildQuery` có 2 `WhereIf` mới
- [x] Xóa duplicate `LanhDaoPhuTrachId` trong `BuildQuery`
- [x] `dotnet build` thành công (0 Error)
- [ ] Smoke test AC1–AC7 trên môi trường test/deploy

---

## 11. Commit đề xuất

```
fix(du-an): bổ sung filter thoiGianKhoiCong/thoiGianHoanThanh cho API theo dõi phòng phân công

Dashboard truyền filter giống danh sách dự án nhưng BE chưa bind 2 field năm khởi công/hoàn thành,
gây lệch counter và list. Copy pattern từ DuAnGetDanhSachQuery vào BuildQuery chung.
```

**Phạm vi:** `TheoDoiDuAnPhongPhanCongSearchDto.cs` + `TheoDoiDuAnPhongPhanCongQuery.cs`.

---

## 12. So sánh Issue 3 vs Issue 4

| Khía cạnh | Issue 3 (giai đoạn) | Issue 4 (phòng phân công) |
| --------- | ------------------- | ------------------------- |
| API | `theo-doi-du-an-theo-giai-doan` | `theo-doi-du-an-phong-phan-cong` |
| Filter thiếu chính | 6 filter (semantics `linhVucId` đặc biệt) | 2 filter (`thoiGianKhoiCong`, `thoiGianHoanThanh`) |
| Trạng thái | WIP / chưa deploy | ✅ Implemented |
| Effort | ~30 phút–vài giờ | ~20 phút |
