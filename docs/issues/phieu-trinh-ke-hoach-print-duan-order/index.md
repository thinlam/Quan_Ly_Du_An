# Lỗi in phiếu trình KH triển khai — thiếu Dự án & sai thứ tự giai đoạn

> Ngày ghi nhận: 06/07/2026  
> Trạng thái: ✅ **IMPLEMENTED** (06/07/2026)  
> Liên quan: [#9469 phieu-trinh-word-spec](../9469/phieu-trinh-word-spec.md), [print-fix trước đó](../phieu-trinh-ke-hoach-print-fix/index.md)

## Tóm tắt

API in phiếu trình Word (`GET /api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc`) trước đây **không khớp UI** ở hai điểm. Đã fix trên BE:

| # | Triệu chứng (trước fix) | Trạng thái |
|---|------------------------|------------|
| 1 | Dòng `Dự án:` trên file in **trống** dù form đã chọn dự án `t01` | ✅ Fixed (code) |
| 2 | Thứ tự giai đoạn/hạng mục **đảo ngược** so với UI | ✅ Fixed (code) |

## Endpoint

```http
GET /QuanLyDuAn/api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc?id={keHoachId}
Authorization: Bearer {token}
Role: GroupKeHoachTrienKhaiHangMucExport
```

## Kết quả implement

| Hạng mục | File | Trạng thái |
|----------|------|------------|
| Sort giai đoạn theo dự án | `KeHoachTrienKhaiHangMucImportGiaiDoanHelper.cs` | ✅ |
| Group/item order | `KeHoachTrienKhaiHangMucExportMapper.cs` | ✅ |
| Loader + DuAnId | `KeHoachTrienKhaiHangMucExportRowLoader.cs` | ✅ |
| Print handler | `KeHoachTrienKhaiHangMucGetPhieuTrinhPrintQuery.cs` | ✅ |
| Excel export | `KeHoachTrienKhaiHangMucGetExportQuery.cs` | ✅ |
| Unit test | `KeHoachTrienKhaiHangMucExportMapperTests.cs` | ✅ 2 tests |
| Integration test | `KeHoachTrienKhaiHangMucPhieuTrinhPrintTests.cs` | ✅ Partial |

## Điểm quan trọng

| Vấn đề | Fix as-built |
|--------|--------------|
| Dự án trống | `EnsureDuAnLoadedAsync` fallback `_duAnRepo`; `DuAnDisplay` = `{MaDuAn} — {TenDuAn}` |
| Sai thứ tự giai đoạn | `GetGiaiDoanSortByDuAnAsync` (`DuAnBuoc.Buoc.Stt`) thay `DanhMucGiaiDoan.Stt` |
| Sai thứ tự HM | Giữ index list gốc; load HM `OrderBy CreatedAt` |
| Excel | Cùng `ExportRowLoader` + `DuAnId` |

## Acceptance Criteria

- [x] Code fix DuAn + sort theo quy trình dự án
- [x] Unit test mapper pass
- [ ] QA manual: `Dự án:` hiển thị `t01` (hoặc `MaDuAn — TenDuAn`)
- [ ] QA manual: thứ tự giai đoạn/hạng mục khớp UI
- [ ] (Tuỳ chọn) `MaDuAn` trên GetQuery DTO cho FE

## Tài liệu triển khai

| File | Nội dung |
|------|----------|
| **[report.md](report.md)** | Spec as-built, luồng sau fix, test plan, checklist |
| [phieu-trinh-word-spec](../9469/phieu-trinh-word-spec.md) | Spec gốc #9469 |

## QA còn lại

1. Mở form kế hoạch có dự án `t01` → in phiếu trình → kiểm tra dòng `Dự án:` và thứ tự group A/B.
2. So sánh với grid UI: Xin chủ trương trước, Chuẩn bị THĐT sau; HM 1,2 / 3 đúng group.
3. Export Excel cùng `id` — thứ tự group đồng bộ với phiếu trình.
