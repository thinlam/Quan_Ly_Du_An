# Issue 3 — API theo dõi giai đoạn thiếu filter `linhVucId`

> Ngày ghi nhận: 30/06/2026  
> Trạng thái: 🔍 Điều tra xong — chưa fix (có WIP local chưa deploy)  
> Effort ước lượng: ~30 phút (BE only, không migration)

## Mô tả

Màn **Thống kê theo dõi dự án/hạng mục theo từng giai đoạn** (#118) có dropdown **Lĩnh vực** trên dashboard, nhưng API `theo-doi-du-an-theo-giai-doan` trên môi trường đang chạy **chưa lọc** theo `linhVucId` → danh sách và 4 panel counter không khớp filter UI.

### Endpoint kiểm tra

```http
GET /QuanLyDuAn/api/du-an/theo-doi-du-an-theo-giai-doan?pageIndex=1&pageSize=10&LinhVucId=1
Authorization: Bearer <JWT_TOKEN>
```

## Yêu cầu BA

| Giá trị `linhVucId` | Hành vi |
|---------------------|---------|
| Không truyền / `null` | Giữ nguyên logic cũ — **không** filter lĩnh vực |
| `-1` | **Tất cả** — không filter lĩnh vực |
| `> 0` | Chỉ trả về dự án có `DuAn.LinhVucId` khớp |

> **Lưu ý:** Semantics `-1` trên màn dashboard là **“Tất cả”**, **khác** API `GET /api/du-an/danh-sach` (ở đó `-1` = dự án **chưa gán** lĩnh vực).

## Phát hiện chính (sau đọc source)

| # | Phát hiện | Mức độ |
|---|-----------|--------|
| 1 | Bản **đã deploy** (`main`): `TheoDoiDuAnTheoGiaiDoanSearchDto` **không có** `LinhVucId`; `BuildQuery` **không** filter | Cao |
| 2 | **WIP local** (chưa commit): đã thêm `LinhVucId` + filter copy từ `DuAnGetDanhSachQuery` — `-1` hiểu là *chưa gán*, **sai** yêu cầu dashboard | Cao |
| 3 | Field dữ liệu: `DuAn.LinhVucId` (`int?`) — filter trực tiếp, **không** cần join thêm | OK |
| 4 | Counter 4 panel dùng chung `BuildQuery` — sửa filter ở đó sẽ đồng bộ panel + list | OK |
| 5 | Controller `DuAnController` bind `[FromQuery]` tự động — **không** cần sửa WebApi | OK |

## Hướng fix khuyến nghị

| Bước | Việc làm |
|------|----------|
| 1 | Thêm / sửa `LinhVucId` trên `TheoDoiDuAnTheoGiaiDoanSearchDto` |
| 2 | Sửa `BuildQuery` — pattern dashboard (`> 0` only; `-1` = Tất cả) |
| 3 | Xác nhận `DuAnController` — không sửa |
| 4 | `dotnet build` |
| 5 | Smoke test: không truyền / `=1` / `=-1` |
| 6 | (Tùy chọn) Cập nhật guide #118 |

**Không** tạo migration. **Không** sửa `DuAnGetDanhSachQuery` (API danh sách giữ semantics riêng).

## Tài liệu triển khai

- **[report.md](report.md)** — Spec kỹ thuật đầy đủ + **6 bước code chi tiết** (DTO, Query, Controller, build, smoke test, guide).
  - [§7 Bước 1 — SearchDto](report.md#bước-1--theodoiduantheogiaidoansearchdto)
  - [§7 Bước 2 — BuildQuery](report.md#bước-2--buildquery-trong-theodoiduantheogiaidoanquery)
  - [§7 Bước 5 — Smoke test](report.md#bước-5--smoke-test-manual)

## Tham chiếu

- [Theo dõi theo giai đoạn #118](../../feature/DuAn/IMPLEMENTATION_GUIDE_118_theo-doi-du-an-theo-giai-doan.md)
- [Ticket gốc — 6 filter dashboard](../ke-hoach-trien-khai-hang-muc-import-fix/index.md) (Phần 2)
- Pattern danh sách dự án: `DuAnGetDanhSachQuery` (chỉ tham chiếu `> 0`, **không** copy nhánh `-1`)
