# Nhật ký công việc — Issue 179

## 2026-08-12

- Khảo sát toàn bộ source liên quan trước khi code, theo đúng yêu cầu "đọc và khảo sát trước, không sửa vội":
  - `GoiThau`, `KetQuaTrungThau`, `ToTrinhQuyetDinh`, `VanBanQuyetDinh`, `HoSoMoiThauDienTu`, `ToTrinhThamDinhNhaThau`, `KetQuaThamDinhNhaThau`, `ToTrinhKetQuaGoiThau`, `ToTrinhCoThamDinh`.
  - `TepDinhKem`/`Attachment`, `EGroupType`, `DanhMucChucVu`, `DanhMucTrangThaiPheDuyet`, `PheDuyetEntityNames`, `TrangThaiPheDuyetCodes`, `EnumLoaiVanBanQuyetDinh`, `LoaiVanBanQuyetDinhConst`.
  - API `api/tong-hop-van-ban-quyet-dinh/danh-sach-day-du` (Query + Controller).
  - Flow duyệt hiện có: `HoSoMoiThauDienTuDuyetCommand`, `ToTrinhThamDinhNhaThauDuyetCommand`, hệ thống `QuanLyPheDuyet`/`PheDuyetDispatch*`.
- Chạy thử `dotnet build SER.sln` để xác nhận trạng thái build hiện tại → phát hiện **build đang lỗi CS1061** tại `HoSoMoiThauDienTuConfiguration.cs` và `ToTrinhQuyetDinhConfiguration.cs` do `ToTrinhQuyetDinh.HoSoMoiThauToTrinhId`/`HoSoMoiThauQuyetDinhId` đã bị comment trong entity nhưng Configuration chưa cập nhật theo.
- Đối chiếu `AppDbContextModelSnapshot.cs` để xác nhận DB hiện tại: `ToTrinhQuyetDinh` **chưa có** cột `EntityId`/`Loai`; `VanBanQuyetDinh` **chưa có** cột `TrangThaiId`.
- Phát hiện quan trọng: API `POST api/to-trinh-tham-dinh-nha-thau/them-moi` **đã tồn tại** (`ToTrinhThamDinhNhaThauController.Create`) nhưng theo cấu trúc nghiệp vụ hoàn toàn khác spec mới trong task (workflow trình/duyệt theo dự án, N nhà thầu/tờ trình, không có Đối chiếu/Thương thảo/Thẩm định/Tờ trình kết quả/Quyết định phê duyệt như yêu cầu).
- Viết đầy đủ `index.md`, `report.md` (trả lời 28 câu hỏi bắt buộc mục 36 của task + liệt kê xung đột cần xác nhận), `journal.md`, `test-workflow.md`.
- **Chưa code.** Đang chờ xác nhận hướng xử lý xung đột giữa API `them-moi` hiện có và spec mới (xem `report.md` mục "Xung đột cần Product/Tech Lead xác nhận").

### Việc tiếp theo
- Chờ xác nhận từ người yêu cầu về hướng xử lý API `them-moi` hiện có (ghi đè / mở rộng song song / xác nhận code chết).
- Sau khi chốt: fix lỗi build hiện tại → domain/EF → migration → Application → WebApi → sửa API tổng hợp → build lại + test theo `test-workflow.md`.

## 2026-08-12 (tiếp — implement)

Người yêu cầu xác nhận code theo `report.md` (hướng A — viết đè endpoint `them-moi` theo spec mới). Đã implement:

1. **Fix build lỗi cũ**: xóa 2 `HasOne` cũ trong `ToTrinhQuyetDinhConfiguration.cs` (đổi tên class từ `ChiDinhThauConfiguration` → `ToTrinhQuyetDinhConfiguration`) và `HoSoMoiThauDienTuConfiguration.cs`. Thêm enum `ELoaiToTrinhQuyetDinh` (HoSoMoiThauToTrinh=1, HoSoMoiThauQuyetDinh=2, ToTrinhThamDinhNhaThau=3).
2. **`HoSoMoiThauDienTu`**: đổi `ToTrinh`/`QuyetDinh` từ navigation EF sang `[NotMapped]`; cập nhật `HoSoMoiThauDienTuInsertCommand`, `HoSoMoiThauDienTuUpdateCommand`, `HoSoMoiThauDienTuDuyetCommand` để load/ghi qua `IRepository<ToTrinhQuyetDinh, long>` lọc theo `EntityId`+`Loai` thay vì `.Include()`.
3. **`ToTrinhThamDinhNhaThau`**: thêm `GoiThauId`, `TenNhaThau`, `NgayKetThucDanhGia`, navigation `GoiThau`, collection `BuocXuLys`.
4. **Entity mới `ToTrinhThamDinhBuocXuLy`**: 1 bảng dùng chung cho Đối chiếu/Thương thảo/Thẩm định, phân biệt bằng `Loai` (enum mới `ELoaiBuocXuLyThamDinhNhaThau`).
5. **`VanBanQuyetDinh`**: thêm `TrangThaiDuyetId` (nullable, đổi tên khác `TrangThaiId` vì tên này đã bị 2 bảng con TPT `PheDuyetDuToan`/`QuyetDinhLapBanQLDA` dùng riêng — trùng tên sẽ vỡ build CS0108) và `NguoiKyChucVuId` (tương tự, tránh trùng `ChucVuId` đã có ở `PheDuyetDuToan`/`VanBanPhapLy`/`VanBanChuTruong`).
6. **`EnumLoaiVanBanQuyetDinh`**: thêm `ToTrinhThamDinhNhaThau`; `LoaiVanBanQuyetDinhConst`: thêm hằng `TOTRINHTHAMDINHNHATHAU`.
7. **`TrangThaiPheDuyetCodes`**: thêm nhóm `ToTrinhThamDinhNhaThauQuyetDinh` (ChoDuyet="ĐTr", DaDuyet="ĐD"); seed 2 dòng mới (Id=71,72) trong `DanhMucTrangThaiPheDuyetConfiguration.cs` với `Loai = PheDuyetEntityNames.ToTrinhThamDinhNhaThau`.
8. **`EGroupType`**: thêm 6 giá trị mới (`ToTrinhThamDinhNhaThau_FileEHSDT/FileDanhGia/DoiChieu/ThuongThao/ThamDinh/QuyetDinh`); tái dùng `EGroupType.ToTrinhQuyetDinh` có sẵn cho file Tờ trình kết quả.
9. **Application**: `ToTrinhThamDinhNhaThauThemMoiDto` (+ DTO con `ThongTinNhaThauDto`, `ThongTinBuocXuLyDto`, `ToTrinhKetQuaDto`, `QuyetDinhPheDuyetDto`) và `ToTrinhThamDinhNhaThauThemMoiCommand` — tạo `ToTrinhThamDinhNhaThau` + 3 `ToTrinhThamDinhBuocXuLy` + `ToTrinhQuyetDinh` (nếu có `ToTrinhKetQua`) + `VanBanQuyetDinh` trạng thái Chờ duyệt (nếu có `QuyetDinhPheDuyet`) trong 1 transaction.
10. **`ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand`** mới — duyệt riêng `VanBanQuyetDinh` (Chờ duyệt → "ĐD"), độc lập với `ToTrinhThamDinhNhaThauDuyetCommand` cũ (duyệt bản thân Tờ trình).
11. **Controller**: viết lại `POST them-moi` theo DTO mới + lưu 7 nhóm `TepDinhKem`; thêm `PUT quyet-dinh/{id}/duyet`.
12. **`TongHopVanBanQuyetDinhGetListQuery`**: thêm `.Where(e => e.TrangThaiDuyetId == null || e.TrangThaiDuyet!.Ma == "ĐD")`.
13. Tạo migration `20260812075056_Issue179_ToTrinhThamDinhNhaThau` — đã **chỉnh tay thứ tự** các bước Up() để backfill đúng dữ liệu cũ (`Loai` theo cột FK cũ nào có giá trị, gộp `HoSoMoiThauQuyetDinhId` vào `EntityId` trước khi rename) tránh mất dữ liệu, vì đây là migration **mới tạo, chưa apply**. **Chưa chạy `ef.bat QLDA update`** — theo yêu cầu, người dùng tự migrate tay.
14. `dotnet build SER.sln` — 0 lỗi. `dotnet ef migrations list` xác nhận migration ở trạng thái `(Pending)`.

### Việc còn lại (ngoài scope lần này, ghi nhận để theo dõi)
- Chưa cập nhật `Update`/`Get` (chi tiết)/`danh-sach-tien-do` của `ToTrinhThamDinhNhaThau` để hiển thị đầy đủ dữ liệu mới (BuocXuLys, ToTrinhQuyetDinh, VanBanQuyetDinh) — chỉ mới đảm bảo `them-moi` hoạt động đúng theo yêu cầu.
- Chưa thêm validator (FluentValidation) riêng cho `ToTrinhThamDinhNhaThauThemMoiDto`.
- Migration chưa được áp dụng vào DB thật — người dùng tự chạy `ef.bat QLDA update` sau khi review.
