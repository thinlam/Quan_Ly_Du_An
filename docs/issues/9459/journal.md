# Journal - UC22: Quản lý phê duyệt nội dung trình duyệt

**Date**: 2026-05-06
**Issue**: #9459
**Branch**: `feature/9459-quan-ly-phe-duyet-noi-dung-trinh-duyet`

## Context

Triển khai module mới - màn hình tổng hợp phê duyệt nội dung trình duyệt. Yêu cầu: BGĐ duyệt/từ chối/ký số/chuyển QLVB, P.HC-TH phát hành, CB/LĐ gửi lại.

## Key Decisions

1. **Unified overlay pattern** thay vì per-entity approval. PheDuyetNoiDung là approval tracking layer trên VanBanQuyetDinh có sẵn. Tránh duplicate approval logic cho mỗi VBQD subtype.

2. **String status codes** (CXL, DD, TC...) thay vì int FK như PheDuyetDuToan. Lý do: đơn giản hơn, không cần join DanhMuc table để check status trong code. DanhMucTrangThaiPheDuyetNoiDung chỉ dùng cho display (Ma→Ten).

3. **Active role checks** - không comment out như PheDuyetDuToan. Commands check `QLDA_LDDV` cho BGĐ, `QLDA_HC_TH` cho P.HC-TH.

## Challenges & Solutions

### Ambiguous RoleConstants
Both `BuildingBlocks.Domain.Constants` and `QLDA.Domain.Constants` have `RoleConstants`, both in global usings. Fix: using alias `using QLDARoleConstants = QLDA.Domain.Constants.RoleConstants;` trong commands.

### SQLite test limitations
- **SQL APPLY**: Correlated subquery trong Select (TepDinhKem) không supported. Fix: bỏ subquery khỏi DanhSach list view, fetch riêng trong ChiTiet.
- **DateTimeOffset ORDER BY**: SQLite không hỗ trợ. Fix: `.ToListAsync()` trước, sort client-side.

### Test isolation
`Trinh_Creates` và `Trinh_Duplicate` share VBQD → test order issue. Fix: Trinh_Creates tạo VBQD riêng thay vì dùng shared seed.

### BgdClient missing LDDV role
BgdClient chỉ có `QLDA_QuanTri`, commands check `QLDA_LDDV`. Fix: thêm `QLDA_LDDV` vào BgdClient roles.

## Lessons Learned

1. Khi thêm entity mới, check ngay global usings for namespace conflicts
2. SQLite test limitations: tránh correlated subquery trong Select, tránh DateTimeOffset trong OrderBy
3. Test isolation: mỗi test nên tạo data riêng, không phụ thuộc shared seed cho mutation tests
4. DuAn property là `TenDuAn` không phải `Ten` - check entity definition trước khi viết query

## Metrics

- Files created: 24
- Files modified: 6+2 (fixture + EF enum)
- Build: 0 errors, 0 warnings
- Tests: 20/20 passed
- Time: ~2h implementation

## Next Steps

- [ ] EF migration cho production deploy
- [ ] Configure PhongHCTHID thực tế
- [ ] Add QLDA_HC_TH role vào auth system
- [ ] Notification khi xử lý xong (UC22 bước 7)
