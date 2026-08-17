# Issue 179 — API `to-trinh-tham-dinh-nha-thau/them-moi`

## 1. Mô tả nghiệp vụ

Bổ sung/điều chỉnh API tạo mới **Tờ trình thẩm định nhà thầu**:

```http
POST api/to-trinh-tham-dinh-nha-thau/them-moi
```

UI tham khảo: `https://e-hsdt1.lovable.app/them-moi`.

Tờ trình gồm 7 mục trên UI:

| # | Mục UI | Field |
|---|--------|-------|
| 1 | Thông tin Gói thầu | `GoiThauId` (chỉ lưu Id, `GiaTri`/`HinhThucLCNT` load lại từ `GoiThau`) |
| 2 | Thông tin Nhà thầu | `NhaThauId` (FK `DmNhaThau`, không lưu tên), `FileEHSDT`, `NgayKetThucDanhGia`, `FileDanhGia` |
| 3 | Thông tin Đối chiếu | `So`, `Ngay`, `File`, `NoiDung` (nullable) |
| 4 | Thông tin Thương thảo | `So`, `Ngay`, `NoiDung`, `File` |
| 5 | Thông tin Thẩm định | `Ngay`, `So` (ẩn trên UI), `File`, `NoiDung` |
| 6 | Tờ trình kết quả | `So`, `Ngay`, `NguoiKy`, `ChucVuId`, `TrichYeu`, `File` |
| 7 | Quyết định phê duyệt | `So`, `Ngay`, `NguoiKy`, `NgayKy`, `ChucVu`, `TrichYeu`, `File` |

Ngoài ra:

- `DonViTrungThau`, `GiaTriTrungThau`, `SoNgayTrienKhai`, `SoNgayThucHienHopDong` chỉ **load** từ `KetQuaTrungThau` theo `GoiThauId`, không lưu duplicate.
- `TrangThaiDangTai` là trạng thái đăng tải riêng của `ToTrinhThamDinhNhaThau`.
- Quyết định phê duyệt (mục 7) khi tạo mới đồng bộ trạng thái **Dự thảo** (`DT`) với tờ trình; chỉ khi duyệt xong (`Ma = "ĐD"`) mới xuất hiện trong `GET api/tong-hop-van-ban-quyet-dinh/danh-sach-day-du`.

## 2. Nguyên tắc bắt buộc

- Reuse entity/table hiện có, không tạo bảng mới cho `ToTrinhKetQua` (dùng `ToTrinhQuyetDinh`) và `QuyetDinhPheDuyet` (dùng `VanBanQuyetDinh`).
- File đính kèm luôn qua `TepDinhKem` (runtime là `Attachment` — BuildingBlocks) + `GroupId` + `GroupType`.
- `ToTrinhQuyetDinh` bỏ 2 FK riêng `HoSoMoiThauToTrinhId` / `HoSoMoiThauQuyetDinhId`, dùng chung `EntityId + Loai`.
- `VanBanQuyetDinh` bổ sung `TrangThaiId` (nullable) — dữ liệu cũ `NULL` mặc định là **đã duyệt**.
- API tổng hợp `tong-hop-van-ban-quyet-dinh/danh-sach-day-du` chỉ lấy `TrangThaiId == null || TrangThai.Ma == "ĐD"`.

## 3. Tài liệu liên quan trong issue này

- [`report.md`](./report.md) — Báo cáo khảo sát chi tiết source hiện tại + trả lời đầy đủ 28 câu hỏi bắt buộc + thiết kế đề xuất + rủi ro/xung đột cần xác nhận trước khi code.
- [`journal.md`](./journal.md) — Nhật ký công việc theo ngày.
- [`test-workflow.md`](./test-workflow.md) — Kế hoạch kiểm thử dự kiến sau khi implement.

## 4. Trạng thái hiện tại

**Đã implement theo hướng (A)** ở mục "Xung đột" của `report.md` — viết đè logic `POST them-moi` theo spec mới, giữ nguyên route. `Update` / `Get chi tiết` / `danh-sach-tien-do` đã map `DoiChieu`/`ThuongThao`/`ThamDinh` và `NhaThauId`. `dotnet build SER.sln` — 0 lỗi.

Entity `ToTrinhThamDinhNhaThau` (sau 2026-08-14):

- **Đã xóa** (cột/prop cũ không dùng): `So`, `NgayTrinh`, `TrichYeu`, `DaThamDinh`, collection `NhaThaus`.
- **Đã đổi** `TenNhaThau` (string) → `NhaThauId` (`Guid?`, FK `DmNhaThau`) + navigation `DanhMucNhaThau? NhaThau`. Tên nhà thầu lấy từ danh mục, không lưu duplicate.
- **Giữ**: `DuAnId`, `BuocId`, `TrangThaiId`, `TrangThaiDangTaiId`, `GoiThauId`, `NgayKetThucDanhGia`, `BuocXuLys`.

`So` / `Ngay` / `TrichYeu` trên bước xử lý (`ToTrinhThamDinhBuocXuLy`), tờ trình kết quả (`ToTrinhQuyetDinh`) và quyết định (`VanBanQuyetDinh`) **không** bị xóa.

Migration mới (EF generate, chưa apply trừ khi đã `ef.bat QLDA update`):

- `20260814075120_Issue179_RemoveLegacyToTrinhThamDinhNhaThauFields`
- `20260814075953_Issue179_ReplaceTenNhaThauWithNhaThauId`

Chi tiết theo ngày: `journal.md`. Test: `test-workflow.md`. Còn thiếu validator FluentValidation riêng (ngoài scope).
