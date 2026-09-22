# Journal — #180 Bổ sung loại `Chứng từ` cho Văn bản pháp lý

## 21/09 — Khảo sát + implement

**Branch:** `Feature/add-chung-tu-van-ban-phap-ly`

**Khảo sát:**
- Trace flow `POST api/van-ban-phap-ly/them-moi`: Controller → `VanBanPhapLyModel.ToEntity()` →
  `VanBanPhapLyInsertOrUpdateCommand` → entity `VanBanPhapLy : VanBanQuyetDinh`.
- Xác nhận `Loai` là **enum** `EnumLoaiVanBanQuyetDinh`, lưu string tên enum vào cột
  `VanBanQuyetDinh.Loai` (đã có, TPT) → **không cần migration**.
- Xác nhận `DanhMucLoaiVanBan` (`LoaiVanBanId`) là khái niệm khác — không đụng.
- Xác nhận không có Validator VanBanPhapLy; path DTO `VanBanPhapLyInsert/UpdateCommand` là dead-code.

**Quyết định:**
- Thêm enum member `ChungTu` với `[Description("Chứng từ")]`.
- Thêm field `Loai` vào `VanBanPhapLyModel`.
- Mapping: whitelist `VanBanPhapLy` + `ChungTu`; giá trị khác/trống → mặc định `VanBanPhapLy`
  (giữ 100% behavior cũ). Update giữ nguyên `entity.Loai` nếu không gửi `loai`.

**Files changed:**
- `QLDA.Domain/Enums/EnumLoaiVanBanQuyetDinh.cs`
- `QLDA.WebApi/Models/VanBanPhapLys/VanBanPhapLyModel.cs`
- `QLDA.WebApi/Models/VanBanPhapLys/VanBanPhapLyMappingConfiguration.cs`
- `docs/issues/180/` (index.md, report.md, journal.md, test-workflow.md)

**Build:** `dotnet build SER.sln` — 0 warning, 0 error.

## 21/09 — Bổ sung filter `loai` cho `van-ban-phap-ly/danh-sach-tien-do`

**Lý do:** sau khi thêm loại `ChungTu`, list tiến độ cần phân biệt loại; yêu cầu mặc định chỉ lấy VBPL.

**Files changed:**
- `QLDA.Application/VanBanPhapLys/Queries/VanBanPhapLyGetDanhSachQuery.cs` — thêm `EnumLoaiVanBanQuyetDinh? Loai`; handler luôn áp `.Where(e => e.Loai == loai)`, default `VanBanPhapLy` khi null.
- `QLDA.WebApi/Controllers/VanBanPhapLyController.cs` — thêm query param `loai`.
- `docs/issues/180/` — cập nhật index.md, report.md, test-workflow.md.

**Build:** `dotnet build SER.sln` — 0 warning, 0 error.

## Còn lại
- [ ] Xác nhận FE tên `PartialView` cho `Chứng từ` (nếu cần hiển thị partial riêng trên
      `tong-hop-van-ban-quyet-dinh`) → bổ sung vào `LoaiVanBanQuyetDinhConst.Dictionary`.
- [ ] Test thủ công theo `test-workflow.md`.