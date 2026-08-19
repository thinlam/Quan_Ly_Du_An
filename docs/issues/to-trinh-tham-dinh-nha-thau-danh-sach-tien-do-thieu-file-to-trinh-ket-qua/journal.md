# Journal — `danh-sach-tien-do` Tờ trình thẩm định nhà thầu sót file "Tờ trình kết quả"

## 19/08 — Ghi nhận + khảo sát + docs

- Báo lỗi: `chi-tiet` trả 7 file, `danh-sach-tien-do` chỉ 6/7 cho cùng Tờ trình (item `08defd97…`, `08defc12…`).
- Khảo sát:
  - File Tờ trình kết quả được lưu với `GroupId = ToTrinhQuyetDinh.Id` (long), `GroupType = ToTrinhQuyetDinh` (controller tạo mới dòng 173–196, chi-tiet đọc dòng 62–70).
  - `ToTrinhThamDinhNhaThauGetDanhSachQuery` chỉ load attachment `GroupId ∈ {toTrinh entity ids}` (dòng 64–77) → sót nhóm `ToTrinhQuyetDinh`.
  - `ToTrinhQuyetDinh` liên kết qua `EntityId` + `Loai = ToTrinhQuyetDinhLoai.ToTrinhThamDinhNhaThau`.
- Quyết định: fix ở **query danh sách** (gộp thêm file nhóm `ToTrinhQuyetDinh`), không đổi cách lưu / chi-tiet / controller.
- Tạo docs: `docs/issues/to-trinh-tham-dinh-nha-thau-danh-sach-tien-do-thieu-file-to-trinh-ket-qua/` (index.md, report.md).

**Files dự kiến sửa:**
- `QLDA.Application/ToTrinhThamDinhNhaThau/Queries/ToTrinhThamDinhNhaThauGetDanhSachQuery.cs`

## 19/08 — Implement

- Đã sửa `ToTrinhThamDinhNhaThauGetDanhSachQuery.cs`:
  - Thêm `IRepository<ToTrinhQuyetDinh, long>` + using `QLDA.Domain.Constants` / `QLDA.Domain.Enums`.
  - Sau khi gán `DanhSachTepDinhKem` theo groupId hiện có, query `ToTrinhQuyetDinh`
    (`EntityId ∈ toTrinhIds && Loai == "ToTrinhThamDinhNhaThau"`), load attachment
    (`GroupId ∈ ToTrinhQuyetDinh.Id`, `GroupType == "ToTrinhQuyetDinh"`) và append vào
    từng item qua `EntityId` (`DistinctBy(f => f.Id)`).
- Build `QLDA.Application` — 0 Error(s), 0 Warning(s).
- Build `QLDA.WebApi` bị chặn do process đang chạy (PID 13180) khóa DLL — chỉ là lỗi copy, không phải compile error.

## 19/08 — Bổ sung variant ký số `KySo_ToTrinhQuyetDinh`

- Báo lỗi từ tester: vẫn thiếu 1 file dù fix đầu đã chạy → nguyên nhân là **file Tờ trình kết quả đã ký số** (`GroupType = KySo_ToTrinhQuyetDinh`).
- Phân tích: `chi-tiet` load nhóm Tờ trình kết quả qua `GetAttachmentsQuery` (IncludeSigned = true) → gồm cả `KySo_ToTrinhQuyetDinh`. Fix đầu dùng exact `GroupType == "ToTrinhQuyetDinh"` → sót variant ký số. 6 nhóm file trực tiếp load theo groupId không filter groupType nên không bị lệch.
- Sửa lại: dùng `AttachmentSubquery.ExpandGroupTypes(["ToTrinhQuyetDinh"], includeSigned: true)` → `["ToTrinhQuyetDinh", "KySo_ToTrinhQuyetDinh"]`, khớp logic `GetAttachmentsQueryHandler`.
- Build `QLDA.Application` — 0 Error(s), 0 Warning(s).
- Cập nhật docs: `index.md`, `report.md` (§3.3 bảng lệch KySo), `test-workflow.md` (case file ký số).
