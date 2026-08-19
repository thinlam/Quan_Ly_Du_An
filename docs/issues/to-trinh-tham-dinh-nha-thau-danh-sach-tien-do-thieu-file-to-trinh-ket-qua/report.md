# Report — `danh-sach-tien-do` Tờ trình thẩm định nhà thầu sót file "Tờ trình kết quả"

*Survey codebase — 19/08/2026 · Implemented — 19/08/2026*

---

## 0. Trạng thái

| Hạng mục | Trạng thái | Ghi chú |
| -------- | ---------- | ------- |
| Investigation / docs | ✅ Done | File này |
| `ToTrinhThamDinhNhaThauGetDanhSachQuery` | ✅ Done | Gộp file nhóm `ToTrinhQuyetDinh` |
| Controller / `chi-tiet` | ✅ Không sửa | Giữ nguyên cách load hiện có |
| Migration | ✅ Không cần | Chỉ query |
| `dotnet build` | ✅ Done | `QLDA.Application` 0 Error; WebApi chỉ bị lock DLL do app đang chạy |
| Smoke test manual | ⏳ Pending | Mẫu ở [§8](#8-smoke-test-manual) |

---

## 1. Tóm tắt

| Thuộc tính | Giá trị |
| ---------- | ------- |
| Issue | `danh-sach-tien-do` thiếu 1 file Tờ trình kết quả (6/7) so với `chi-tiet` |
| Status | ✅ **IMPLEMENTED** |
| Method | `GET` |
| URL | `/QuanLyDuAn/api/to-trinh-tham-dinh-nha-thau/danh-sach-tien-do` |
| Controller | `ToTrinhThamDinhNhaThauController.Get` (dòng 295) |
| Handler | `ToTrinhThamDinhNhaThauDanhSachQueryHandler` |
| Migration | **Không** cần |

---

## 2. Luồng xử lý

```
GET /api/to-trinh-tham-dinh-nha-thau/danh-sach-tien-do?DuAnId=…&BuocId=…
        │
        ▼
ToTrinhThamDinhNhaThauController.Get
        │
        ▼
ToTrinhThamDinhNhaThauDanhSachQueryHandler.Handle
        │
        ├─ queryable = ToTrinhThamDinhNhaThau (Include GoiThau) → PaginatedList
        │
        ├─ Load attachment: GroupId ∈ {toTrinh entity ids}   ← 6 file (nhóm ToTrinhThamDinhNhaThau_*)
        │
        ├─ ✅ MỚI: Load ToTrinhQuyetDinh (EntityId ∈ toTrinh ids && Loai == "ToTrinhThamDinhNhaThau")
        │         → load attachment: GroupId ∈ {ToTrinhQuyetDinh.Id} (groupType ToTrinhQuyetDinh)
        │         → append vào DanhSachTepDinhKem từng item qua EntityId   ← +1 file
        │
        └─ return result
```

---

## 3. Trạng thái code sau fix

### 3.1 `ToTrinhThamDinhNhaThauGetDanhSachQuery.cs` — sau fix

**Thêm using:**

```csharp
using QLDA.Domain.Constants;   // ToTrinhQuyetDinhLoai
using QLDA.Domain.Enums;       // EGroupType
```

**Thêm field (cạnh `TepDinhKem`):**

```csharp
private readonly IRepository<ToTrinhQuyetDinh, long> ToTrinhQuyetDinh =
    ServiceProvider.GetRequiredService<IRepository<ToTrinhQuyetDinh, long>>();
```

**Thêm block gộp file Tờ trình kết quả (sau vòng gán `DanhSachTepDinhKem` hiện tại):**

```csharp
// File "Tờ trình kết quả" — GroupId là ToTrinhQuyetDinh.Id (long), không nằm trong
// groupIds của toTrinh nên danh-sach-tien-do sót file mà chi-tiet vẫn đủ (Issue #179).
var toTrinhIds = result.Data.Select(x => x.Id).ToList();
var toTrinhQuyetDinhs = toTrinhIds.Count == 0
    ? []
    : await ToTrinhQuyetDinh.GetQueryableSet().AsNoTracking()
        .Where(e => toTrinhIds.Contains(e.EntityId) && e.Loai == ToTrinhQuyetDinhLoai.ToTrinhThamDinhNhaThau)
        .Select(e => new { e.Id, e.EntityId })
        .ToListAsync(cancellationToken);

var ketQuaGroupIds = toTrinhQuyetDinhs.Select(x => x.Id.ToString()).ToList();
var ketQuaFiles = ketQuaGroupIds.Count == 0
    ? []
    : await TepDinhKem.GetQueryableSet().AsNoTracking()
        .Where(i => ketQuaGroupIds.Contains(i.GroupId) && i.GroupType == nameof(EGroupType.ToTrinhQuyetDinh))
        .Select(i => i.ToDto())
        .ToListAsync(cancellationToken);

var ketQuaByToTrinhId = toTrinhQuyetDinhs
    .Where(x => x.EntityId.HasValue)
    .SelectMany(x => ketQuaFiles.Where(f => f.GroupId == x.Id.ToString()), (x, f) => new { x.EntityId, f })
    .GroupBy(x => x.EntityId!.Value)
    .ToDictionary(g => g.Key, g => g.Select(x => x.f).ToList());

foreach (var item in result.Data)
{
    if (item.Id is { } id && ketQuaByToTrinhId.TryGetValue(id, out var files))
        item.DanhSachTepDinhKem = item.DanhSachTepDinhKem!.Concat(files).DistinctBy(f => f.Id).ToList();
}
```

### 3.2 Trước fix (tham chiếu)

| Vấn đề | Mô tả |
| ------ | ----- |
| `danh-sach-tien-do` thiếu file Tờ trình kết quả | Chỉ load attachment theo `GroupId ∈ toTrinh entity ids`; file Tờ trình kết quả nằm ở group `ToTrinhQuyetDinh.Id` (long) → không khớp |
| `chi-tiet` đủ 7 file | Controller gộp thêm nhóm `ToTrinhQuyetDinh` riêng (dòng 62–70) |

---

## 4. Model dữ liệu

| Entity | Field | Kiểu | Ghi chú |
| ------ | ----- | ---- | ------- |
| `ToTrinhThamDinhNhaThau` | `Id` | `Guid` | GroupId cho 6 nhóm file trực tiếp |
| `ToTrinhQuyetDinh` | `Id` | `long` | **GroupId** cho file Tờ trình kết quả |
| `ToTrinhQuyetDinh` | `EntityId` | `Guid?` | = `ToTrinhThamDinhNhaThau.Id` (bảng dùng chung) |
| `ToTrinhQuyetDinh` | `Loai` | `string` | `ToTrinhQuyetDinhLoai.ToTrinhThamDinhNhaThau` = `"ToTrinhThamDinhNhaThau"` |
| `Attachment` | `GroupId` | `string` | Lưu dạng chuỗi Guid hoặc long |
| `Attachment` | `GroupType` | `string` | `"ToTrinhQuyetDinh"` cho file Tờ trình kết quả |

---

## 5. Semantics

- File trực tiếp của Tờ trình: `GroupId = ToTrinhThamDinhNhaThau.Id`, `GroupType = ToTrinhThamDinhNhaThau_*` → load theo `groupIds` cũ (giữ nguyên).
- File Tờ trình kết quả: `GroupId = ToTrinhQuyetDinh.Id`, `GroupType = ToTrinhQuyetDinh` → load mới theo map `EntityId → Id`.
- Gộp bằng **`DistinctBy(f => f.Id)`** — chống trùng file giữa 2 nhóm (an toàn về mặt phòng xa).

---

## 6. Files sẽ sửa

| # | File | Thay đổi |
| - | ---- | -------- |
| 1 | `QLDA.Application/ToTrinhThamDinhNhaThau/Queries/ToTrinhThamDinhNhaThauGetDanhSachQuery.cs` | Thêm repo `ToTrinhQuyetDinh`, block gộp file, 2 using |

**Không sửa:** controller, `chi-tiet`, cách lưu file, migration, `AppDbContextModelSnapshot`.

---

## 7. Build

```powershell
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
# Kỳ vọng: 0 Error(s)
```

---

## 8. Smoke test manual

⏳ **Chưa chạy** — dùng checklist sau khi build/deploy.

### 8.1 So sánh trước/sau trên cùng dataset

```http
GET /QuanLyDuAn/api/to-trinh-tham-dinh-nha-thau/{id}/chi-tiet        # kỳ vọng: 7 file
GET /QuanLyDuAn/api/to-trinh-tham-dinh-nha-thau/danh-sach-tien-do?DuAnId=<guid>&BuocId=7040&PageIndex=1&PageSize=10   # kỳ vọng: 7 file
```

### 8.2 Đối tượng kiểm tra

| Item `Id` | Trước fix | Sau fix |
| --------- | --------- | ------- |
| `08defd97-eaf5-c226-687a-7b350801bae5` | 6/7 | 7/7 |
| `08defc12-4e20-3b60-687a-7b38f8073d8e` | 6/7 | 7/7 |

Kỳ vọng: mỗi item có đủ file Tờ trình kết quả (`groupType = "ToTrinhQuyetDinh"`) trong `danhSachTepDinhKem`, khớp chi-tiet.

---

## 9. Acceptance criteria

| # | Kịch bản | Kỳ vọng | Verify |
| - | -------- | ------- | ------ |
| AC1 | Item có ToTrinhQuyetDinh + file | `danhSachTepDinhKem` đủ file (7/7) | ⏳ |
| AC2 | Item không có ToTrinhQuyetDinh | Không đổi (giữ nguyên 6 file trực tiếp) | ⏳ |
| AC3 | Không trùng file | `DistinctBy(f => f.Id)` | ⏳ |
| AC4 | Response shape | Không đổi JSON | ✅ |
| AC5 | Build | 0 Error(s) | ⏳ |

---

## 10. Checklist nghiệm thu

- [ ] `ToTrinhThamDinhNhaThauGetDanhSachQuery` có repo `ToTrinhQuyetDinh` + block gộp file
- [ ] `dotnet build` thành công (0 Error)
- [ ] Smoke test AC1–AC3 trên môi trường test/deploy (2 item 7/7)

---

## 11. Commit đề xuất

```
fix(to-trinh-tham-dinh-nha-thau): danh-sach-tien-do sót file Tờ trình kết quả

chi-tiet gộp 8 nhóm file (6 nhóm gắn theo ToTrinhThamDinhNhaThau.Id + nhóm
ToTrinhQuyetDinh gắn theo ToTrinhQuyetDinh.Id long) nên đủ 7 file. danh-sach-tien-do
chỉ load theo GroupId ∈ toTrinh entity ids nên mất file Tờ trình kết quả.
Bổ sung query ToTrinhQuyetDinh (EntityId + Loai) và append file vào từng item.
```

**Phạm vi:** `ToTrinhThamDinhNhaThauGetDanhSachQuery.cs`.
