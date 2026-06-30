# Issue 4 — API theo dõi phòng phân công thiếu filter dashboard

> Ngày ghi nhận: 30/06/2026  
> Trạng thái: ✅ **IMPLEMENTED** (30/06/2026)  
> Effort thực tế: ~20 phút (BE only, không migration)

## Mô tả

Màn **Theo dõi dự án theo phòng phân công** (#9527) có bộ filter trên dashboard giống danh sách dự án. Trước fix, API `theo-doi-du-an-phong-phan-cong` **chưa bind** `thoiGianKhoiCong` / `thoiGianHoanThanh` → số liệu 4 panel counter và danh sách **lệch** khi dashboard truyền filter.

### Endpoint kiểm tra

```http
GET /QuanLyDuAn/api/du-an/theo-doi-du-an-phong-phan-cong?pageIndex=1&pageSize=10&thoiGianKhoiCong=2024&trangThaiDuAnId=1&namDuAn=2025
Authorization: Bearer <JWT_TOKEN>
```

## Yêu cầu BA

Bổ sung filter (logic **giống** `GET /api/du-an/danh-sach` — `DuAnGetDanhSachQuery`):

| Param | Ý nghĩa | Không truyền |
|-------|---------|--------------|
| `thoiGianKhoiCong` | Năm khởi công dự án (`DuAn.ThoiGianKhoiCong`) | Không áp dụng WHERE |
| `thoiGianHoanThanh` | Năm hoàn thành dự án (`DuAn.ThoiGianHoanThanh`) | Không áp dụng WHERE |
| `trangThaiDuAnId` | Trạng thái dự án (`DuAn.TrangThaiDuAnId`) | Không áp dụng WHERE |
| `namDuAn` | Năm dự án nằm trong khoảng thực hiện (#9121) | Không áp dụng WHERE |

**Mục tiêu:** Counter 4 panel và `danhSach` dùng **cùng subset** sau filter — đồng bộ với dashboard.

## Kết quả implement

| # | Hạng mục | Trạng thái |
|---|----------|------------|
| 1 | `TheoDoiDuAnPhongPhanCongSearchDto` — thêm `ThoiGianKhoiCong`, `ThoiGianHoanThanh` | ✅ |
| 2 | `BuildQuery` — thêm 2 `WhereIf` (copy `DuAnGetDanhSachQuery`) | ✅ |
| 3 | `TrangThaiDuAnId`, `NamDuAn` — đã có từ #9527, giữ nguyên | ✅ |
| 4 | Xóa duplicate `LanhDaoPhuTrachId` trong `BuildQuery` | ✅ |
| 5 | `dotnet build` — 0 Error(s) | ✅ |
| 6 | Smoke test manual (§8 report) | ⏳ Pending |

**Files đã sửa:**

- `QLDA.Application/DuAns/DTOs/TheoDoiDuAnPhongPhanCongSearchDto.cs`
- `QLDA.Application/DuAns/Queries/TheoDoiDuAnPhongPhanCongQuery.cs`

**Không sửa:** `DuAnController`, migration, WebApi model.

## Tài liệu triển khai

- **[report.md](report.md)** — Spec kỹ thuật + code đã áp dụng + smoke test.
  - [§0 Trạng thái](report.md#0-trạng-thái)
  - [§3 Code sau fix](report.md#3-trạng-thái-code-sau-fix)
  - [§8 Smoke test](report.md#8-smoke-test-manual)

## Tham chiếu

- [Theo dõi phòng phân công #9527](../../feature/DuAn/IMPLEMENTATION_GUIDE_9527_theo-doi-du-an-phong-phan-cong.md)
- [Filter `lanhDaoPhuTrachId` & `trangThaiDuAnId` #9527](../../feature/DuAn/IMPLEMENTATION_GUIDE_theo-doi-du-an-phong-phan-cong-search-filters.md)
- Pattern danh sách dự án: `DuAnGetDanhSachQuery`, `DuAnSearchDto`
- Ticket lớn (API giai đoạn khác): [ke-hoach-trien-khai-hang-muc-import-fix](../ke-hoach-trien-khai-hang-muc-import-fix/index.md) Phần 2
