# Issue #9609 — Bổ sung tiêu chí tìm kiếm "Loại dự án theo năm"

> Ngày triển khai: 18/06/2026
> Trạng thái: ✅ Hoàn thành (BE)
> Build: 0 errors

## Mô tả gốc (Yêu cầu ban đầu)

> Bổ sung thêm tiêu chí tìm kiếm CBB "Loại dự án theo năm" ở tất cả màn hình
> (tiêu chí tìm kiếm với các entities có liên kết DuAn)
>
> `.WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)`

### Note từ FE dev
> Em có bổ sung UI search ở các màn hình hết rồi. Nhờ anh bs giúp em param search `loaiDuAnTheoNamId` ở api danh sách của các màn hình sau nha:
> **Gói thầu, Hợp đồng, Phụ lục hợp đồng, Báo cáo tiến độ, Khó khăn vướng mắc, Tổng hợp văn bản quyết định.**

→ Recommendation: scout trước xem những entities nào có liên kết với DuAn, sau đó áp dụng đồng nhất cho tất cả.

## Tóm tắt triển khai

| Hạng mục | Chi tiết |
|----------|----------|
| **Phạm vi BE** | Toàn bộ endpoints `danh-sach-tien-do` / `danh-sach` của entities có liên kết `DuAn` navigation |
| **Số màn hình FE (đợt 1)** | 6 (theo note FE) |
| **Số entity bổ sung (đợt 2)** | 21 entity còn lại có `DuAn? DuAn` navigation |
| **Tổng files chỉnh sửa** | 96 files (35 Query + 13 DTO + 32 Controller + 16 SearchModel + PrintController) |
| **Pattern** | `.WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)` |
| **Kiểu dữ liệu** | `int?` nullable, filter `> 0` (nhất quán với `LoaiDuAnId`/`BuocId` hiện có) |
| **Build** | `dotnet build QLDA.WebApi.csproj` → **0 errors** |

## Danh mục "Loại dự án theo năm" (seed data)

| Id | Ma | Ten |
|----|-----|-----|
| 1 | CBDT | Chuẩn bị đầu tư |
| 2 | CT | Chuyển tiếp |
| 3 | KCM | Khởi công mới |
| 4 | KLTD | Khối lượng tồn đọng |

Entity: `QLDA.Domain/Entities/DanhMuc/DanhMucLoaiDuAnTheoNam.cs`
Trên `DuAn`: `public int? LoaiDuAnTheoNamId { get; set; }` (line 152) + navigation `LoaiDuAnTheoNam`

→ Chi tiết triển khai xem [report.md](./report.md)
