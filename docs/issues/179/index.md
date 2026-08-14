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
| 2 | Thông tin Nhà thầu | `TenNhaThau`, `FileEHSDT`, `NgayKetThucDanhGia`, `FileDanhGia` |
| 3 | Thông tin Đối chiếu | `So`, `Ngay`, `File`, `NoiDung` (nullable) |
| 4 | Thông tin Thương thảo | `So`, `Ngay`, `NoiDung`, `File` |
| 5 | Thông tin Thẩm định | `Ngay`, `So` (ẩn trên UI), `File`, `NoiDung` |
| 6 | Tờ trình kết quả | `So`, `Ngay`, `NguoiKy`, `ChucVuId`, `TrichYeu`, `File` |
| 7 | Quyết định phê duyệt | `So`, `Ngay`, `NguoiKy`, `NgayKy`, `ChucVu`, `TrichYeu`, `File` |

Ngoài ra:

- `DonViTrungThau`, `GiaTriTrungThau`, `SoNgayTrienKhai`, `SoNgayThucHienHopDong` chỉ **load** từ `KetQuaTrungThau` theo `GoiThauId`, không lưu duplicate.
- `TrangThaiDangTai` là trạng thái đăng tải riêng của `ToTrinhThamDinhNhaThau`.
- Quyết định phê duyệt (mục 7) khi tạo mới phải ở trạng thái **CHỜ DUYỆT**, chỉ khi duyệt xong (`TrangThai.Ma = "ĐD"`) mới xuất hiện trong `GET api/tong-hop-van-ban-quyet-dinh/danh-sach-day-du`.

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

**Đã implement theo hướng (A)** ở mục "Xung đột" của `report.md` — viết đè logic `POST them-moi` theo spec mới, giữ nguyên route. Đã fix lỗi build cũ, thêm domain/EF, migration (chưa apply DB), Application (DTO + Command), sửa Controller + API tổng hợp. `dotnet build SER.sln` — 0 lỗi. Chi tiết xem `journal.md` mục "2026-08-12 (tiếp — implement)".

Còn thiếu: cập nhật `Update`/`Get chi tiết`/`danh-sach-tien-do` để hiển thị đầy đủ dữ liệu mới, validator riêng, và test thủ công theo `test-workflow.md` (chưa có môi trường DB để test).
