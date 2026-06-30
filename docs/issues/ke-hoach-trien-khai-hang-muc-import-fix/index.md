# Issue — Sửa template import KH triển khai hạng mục + bổ sung filter dashboard

> Ngày ghi nhận: 30/06/2026  
> Trạng thái: 🔍 Điều tra xong — chưa fix  
> Effort ước lượng: ~4–6 giờ (BE only, không migration)

## Tóm tắt

Ticket gồm **hai hạng mục độc lập**:

| # | Hạng mục | API chính | Trạng thái code |
|---|----------|-----------|-----------------|
| **1** | Sửa cấu trúc cột template import Kế hoạch triển khai hạng mục | `GET /api/template/import-ke-hoach-trien-khai-hang-muc` | ❌ Template 11 cột cũ (còn Tờ trình / Ngày trình / Trích yếu; thiếu Dự án, Đơn vị chủ trì / phối hợp) |
| **2** | Bổ sung filter dashboard cho API theo dõi theo giai đoạn | `GET /api/du-an/theo-doi-du-an-theo-giai-doan` | ❌ Thiếu 6 filter so với màn dashboard / `GET /api/du-an/danh-sach` |

## Phần 1 — Template import

### Endpoint kiểm tra

```http
GET /QuanLyDuAn/api/template/import-ke-hoach-trien-khai-hang-muc?duAnId=08ded5f5-3b4e-5676-687a-7b278006675e
```

### Thay đổi cột (BA)

| Hành động | Cột |
|-----------|-----|
| **Bổ sung** | Dự án · Đơn vị chủ trì · Đơn vị phối hợp (nhiều) · Cán bộ phối hợp (nhiều) |
| **Bỏ** | Tờ trình · Ngày trình · Trích yếu |

> Cột **Cán bộ phối hợp** đã có trong template cũ nhưng chỉ hỗ trợ **1 người**; cần nâng cấp lên **nhiều cán bộ** (đồng bộ export: `", "`).

### Phát hiện chính (sau đọc source)

| # | Phát hiện | Mức độ |
|---|-----------|--------|
| 1 | `KeHoachTrienKhaiHangMucImportDescriptor` vẫn khai báo 11 cột cũ (#9469) | Cao |
| 2 | `KeHoachTrienKhaiHangMucImportRangeCommand` **bắt buộc** `So` (Tờ trình) + group theo `(So, NgayTrinh, TrichYeu)` | Cao |
| 3 | `TemplateController` **không nhận** `duAnId` dù FE đã gửi query param | Trung bình |
| 4 | Import hiện lấy `duAnId`/`buocId` từ **form POST**, không từ cột Excel | Trung bình |
| 5 | `DonViChuTriId` trên entity có sẵn; export đã hiển thị đủ 4 cột đơn vị/cán bộ | OK |
| 6 | `IMPLEMENTATION_GUIDE.md` (#9469) mô tả spec cũ — cần cập nhật sau khi fix | Thấp |

## Phần 2 — Filter API dashboard

### Endpoint (API số 2)

```http
GET /QuanLyDuAn/api/du-an/theo-doi-du-an-theo-giai-doan
```

> **Giải thích "API số 2":** Trong cùng ticket BA, API 1 là tải mẫu import; API 2 là API thống kê dashboard **Theo dõi dự án theo giai đoạn** (#118). Dashboard FE đã có bộ filter giống danh sách dự án nhưng BE chưa bind → số liệu 4 panel lệch.

### Filter cần bổ sung

```text
loaiDuAnId
donViPhuTrachChinhId
thoiGianKhoiCong
thoiGianHoanThanh
trangThaiDuAnId
linhVucId
```

Logic tham chiếu: `DuAnGetDanhSachQuery` — filter không truyền thì không áp dụng.

### Phát hiện chính

| # | Phát hiện | Mức độ |
|---|-----------|--------|
| 1 | `TheoDoiDuAnTheoGiaiDoanSearchDto` chỉ có `GiaiDoanId`, `NamDuAn`, `TenDuAn`, `MaDuAn`, `Loai` | Cao |
| 2 | `TheoDoiDuAnPhongPhanCongSearchDto` đã có `donViPhuTrachChinhId`, `trangThaiDuAnId` — **không** có 4 filter còn lại | Trung bình (nếu BA nhầm API) |
| 3 | Counter 4 panel dùng chung `BuildQuery` — thêm filter vào đó sẽ đồng bộ panel + list | OK (pattern đúng) |

## Hướng fix khuyến nghị

| Phần | Bước chính |
|------|------------|
| **1 — Template** | Bước 1–8 trong [report.md §1.11](report.md#111-bước-code-chi-tiết--phần-i) |
| **2 — Filter** | Bước 9–11 trong [report.md §2.9](report.md#29-bước-code-chi-tiết--phần-ii) |

## Tài liệu triển khai

- **[report.md](report.md)** — Spec kỹ thuật đầy đủ + **11 bước code chi tiết** (DTO, Query, Command, Controller, regen template, test, build).

## Tham chiếu

- [Import KH triển khai — guide gốc #9469](../../feature/KeHoachTrienKhaiHangMuc/IMPLEMENTATION_GUIDE.md)
- [Theo dõi theo giai đoạn #118](../../feature/DuAn/IMPLEMENTATION_GUIDE_118_theo-doi-du-an-theo-giai-doan.md)
- [Filter phòng phân công #9527](../../feature/DuAn/IMPLEMENTATION_GUIDE_theo-doi-du-an-phong-phan-cong-search-filters.md)
- Export cột chuẩn: `KeHoachTrienKhaiHangMucExportDescriptor.cs`
