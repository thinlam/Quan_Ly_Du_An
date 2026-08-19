# Danh sách tiến độ Tờ trình thẩm định nhà thầu thiếu file "Tờ trình kết quả"

> Ngày ghi nhận: 19/08/2026  
> Trạng thái: ✅ **IMPLEMENTED** (19/08/2026)  
> Effort thực tế: ~20 phút (BE only, không migration)

## Mô tả

`GET /api/to-trinh-tham-dinh-nha-thau/{id}/chi-tiet` trả **7 file** đầy đủ, nhưng
`GET /api/to-trinh-tham-dinh-nha-thau/danh-sach-tien-do` chỉ trả **6/7 file** cho cùng
một Tờ trình — thiếu đúng 1 file **"Tờ trình kết quả"** (nhóm `ToTrinhQuyetDinh`).
Lỗi xảy ra **đồng loạt** cho mọi dòng trong danh sách (VD item `08defd97…` và `08defc12…`).

### Endpoint liên quan

```http
GET /QuanLyDuAn/api/to-trinh-tham-dinh-nha-thau/{id}/chi-tiet
GET /QuanLyDuAn/api/to-trinh-tham-dinh-nha-thau/danh-sach-tien-do?DuAnId=<guid>&BuocId=7040&PageIndex=1&PageSize=10
```

## Nguyên nhân

File của Tờ trình **được lưu ở 2 nhóm group khác nhau**:

| Nhóm | `groupId` | `groupType` | Số file |
| ---- | --------- | ----------- | ------- |
| File trực tiếp của Tờ trình | `ToTrinhThamDinhNhaThau.Id` (Guid) | `ToTrinhThamDinhNhaThau_*` (EHSDT, FileDanhGia, DoiChieu, ThuongThao, ThamDinh, QuyetDinh) | 6 |
| **File "Tờ trình kết quả"** | **`ToTrinhQuyetDinh.Id` (long)** | `ToTrinhQuyetDinh` | 1 |

- **`chi-tiet`** (`ToTrinhThamDinhNhaThauController.cs:62–70`) gộp **cả 2 nhóm** → đủ 7 file.
- **`danh-sach-tien-do`** (`ToTrinhThamDinhNhaThauGetDanhSachQuery.cs:64–77`) chỉ load attachment với
  `GroupId ∈ {toTrinh entity id}` → file Tờ trình kết quả có `groupId = ToTrinhQuyetDinh.Id`
  ("20089", dạng long) **không khớp** → bị sót → 6/7.

Thiết kế lưu file theo `ToTrinhQuyetDinh.Id` là **có chủ đích** (comment controller dòng 62) →
fix đúng chỗ là bổ sung vào query danh sách, không đổi cách lưu.

## Kết quả implement

| # | Hạng mục | Trạng thái |
|---|----------|------------|
| 1 | `ToTrinhThamDinhNhaThauGetDanhSachQuery` — gộp file ToTrinhQuyetDinh | ✅ |
| 2 | `dotnet build` (QLDA.Application) | ✅ 0 Error |
| 3 | Smoke test manual (2 item 7/7 file) | ⏳ Pending |

**Files sẽ sửa:**

- `QLDA.Application/ToTrinhThamDinhNhaThau/Queries/ToTrinhThamDinhNhaThauGetDanhSachQuery.cs`

**Không sửa:** controller, cách lưu file, migration, `chi-tiet`.

## Tài liệu triển khai

- **[report.md](report.md)** — Spec kỹ thuật + code sau fix + smoke test.
  - [§0 Trạng thái](report.md#0-trạng-thái)
  - [§3 Code sau fix](report.md#3-trạng-thái-code-sau-fix)
  - [§8 Smoke test](report.md#8-smoke-test-manual)

## Tham chiếu

- Entity `ToTrinhQuyetDinh` (bảng dùng chung, `EntityId` + `Loai`): `QLDA.Domain/Entities/ToTrinhQuyetDinh.cs`
- `ToTrinhQuyetDinhLoai.ToTrinhThamDinhNhaThau`: `QLDA.Domain/Constants/ToTrinhQuyetDinhLoai.cs`
- Pattern load file theo groupId trong danh sách: `QuanLyPheDuyet/Queries/PheDuyetQueryableExtensions.cs`
