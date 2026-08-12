# Kế hoạch kiểm thử — Issue 179

> **Đã implement xong code + migration (chưa apply DB)**. Tài liệu này là hướng dẫn test thực tế — cập nhật lại theo code đã viết (field `VanBanQuyetDinh.TrangThaiDuyetId`, không phải `TrangThaiId`).

## 1. Build & lint

```powershell
dotnet build SER.sln
```

- Kết quả hiện tại: **0 Error(s)**.

## 2. Migration

Migration `20260812075056_Issue179_ToTrinhThamDinhNhaThau` đã được tạo sẵn (xem `QLDA.Migrator/Migrations/`), **chưa apply vào DB**. Trước khi test cần tự chạy (đúng theo yêu cầu — không tự động migrate):

```powershell
ef.bat QLDA update
```

hoặc trực tiếp:

```powershell
dotnet ef database update --project QLDA.Migrator\QLDA.Migrator.csproj --startup-project QLDA.Migrator\QLDA.Migrator.csproj --context AppDbContext
```

- Xác nhận đúng connection string dev/test trong `QLDA.WebApi/appsettings.Development.json` trước khi chạy.
- Sau khi update, kiểm tra nhanh: bảng `ToTrinhQuyetDinh` có cột `EntityId`/`Loai`, không còn `HoSoMoiThauQuyetDinhId`; bảng `VanBanQuyetDinh` có cột `TrangThaiDuyetId`/`NguoiKyChucVuId`; bảng `ToTrinhThamDinhBuocXuLy` được tạo mới; `DmTrangThaiPheDuyet` có 2 dòng mới `Id=71` (`Ma="ĐTr"`) và `Id=72` (`Ma="ĐD"`) với `Loai="ToTrinhThamDinhNhaThau"`.

## 3. Chạy API

```powershell
dotnet run --project QLDA.WebApi
```

Mở `http://localhost:5183/swagger` (theo `launchSettings.json` profile `http`), tìm tag **"Tờ trình thẩm định nhà thầu"**.

## 4. Test API `POST api/to-trinh-tham-dinh-nha-thau/them-moi` (thủ công qua Swagger/Postman)

### 4.0. Chuẩn bị dữ liệu mẫu

Cần có sẵn (lấy qua Swagger các API GET tương ứng, hoặc query DB):
- 1 `DuAnId` hợp lệ (`GET api/du-an/...`).
- 1 `BuocId` hợp lệ thuộc dự án đó (bảng `DuAnBuoc`).
- 1 `GoiThauId` hợp lệ (`GET api/goi-thau/danh-sach` hoặc query bảng `GoiThau`).
- 1 `ChucVuId` hợp lệ (`GET api/dm-chuc-vu`).
- Không bắt buộc phải có file thật để test nhanh — có thể để `null`/bỏ trống các mảng `File`, `FileEHSDT`, `FileDanhGia` (Command chỉ gọi `AttachmentBulkInsertOrUpdateCommand` khi `Count > 0`).

### 4.1. Payload mẫu

```json
{
  "duAnId": "00000000-0000-0000-0000-000000000000",
  "buocId": 1,
  "goiThauId": "00000000-0000-0000-0000-000000000000",
  "so": "TT-179-001",
  "ngayTrinh": "2026-08-12T00:00:00+07:00",
  "trichYeu": "Tờ trình thẩm định nhà thầu test issue 179",
  "trangThaiDangTaiId": null,
  "thongTinNhaThau": {
    "tenNhaThau": "Công ty TNHH Test",
    "fileEHSDT": [],
    "ngayKetThucDanhGia": "2026-08-10T00:00:00+07:00",
    "fileDanhGia": []
  },
  "thongTinDoiChieu": {
    "so": "DC-001",
    "ngay": "2026-08-05T00:00:00+07:00",
    "noiDung": null,
    "file": []
  },
  "thongTinThuongThao": {
    "so": "TT-002",
    "ngay": "2026-08-06T00:00:00+07:00",
    "noiDung": "Nội dung thương thảo",
    "file": []
  },
  "thongTinThamDinh": {
    "ngay": "2026-08-07T00:00:00+07:00",
    "so": null,
    "noiDung": "Nội dung thẩm định",
    "file": []
  },
  "toTrinhKetQua": {
    "so": "KQ-001",
    "ngay": "2026-08-08T00:00:00+07:00",
    "nguoiKy": "Nguyễn Văn A",
    "chucVuId": 1,
    "trichYeu": "Trích yếu tờ trình kết quả",
    "file": []
  },
  "quyetDinhPheDuyet": {
    "so": "QD-001",
    "ngay": "2026-08-09T00:00:00+07:00",
    "nguoiKy": "Nguyễn Văn B",
    "ngayKy": "2026-08-09T00:00:00+07:00",
    "chucVuId": 1,
    "trichYeu": "Trích yếu quyết định phê duyệt",
    "file": []
  }
}
```

Thay `duAnId`/`goiThauId`/`chucVuId` bằng dữ liệu thật lấy ở bước 4.0.

### 4.2. Happy path
1. Gọi `POST /api/to-trinh-tham-dinh-nha-thau/them-moi` với payload trên.
2. Kiểm tra response trả về `{ id, toTrinhQuyetDinhId, vanBanQuyetDinhId }`.
3. Query DB xác nhận:
   - `ToTrinhThamDinhNhaThau.GoiThauId` = đúng giá trị gửi lên; không có cột `GiaTri`/`HinhThucLCNT` trên bảng này.
   - `ToTrinhThamDinhBuocXuLy` có đúng 3 dòng (`Loai=1,2,3` tương ứng Đối chiếu/Thương thảo/Thẩm định), cùng `ToTrinhId` = id vừa tạo.
   - `ToTrinhQuyetDinh` có 1 dòng mới, `EntityId` = id Tờ trình, `Loai = 3` (`ToTrinhThamDinhNhaThau`), `So/Ngay/NguoiKy/ChucVu/TrichYeu` khớp `toTrinhKetQua`.
   - `VanBanQuyetDinh` có 1 dòng mới, `TrangThaiDuyetId` **khác null** và trỏ tới bản ghi `Id=71` (`Ma="ĐTr"`), `Loai = "ToTrinhThamDinhNhaThau"`, `So/Ngay/NguoiKy/NgayKy/NguoiKyChucVuId/TrichYeu` khớp `quyetDinhPheDuyet`.

### 4.3. Validate
1. Gọi API với `goiThauId` không tồn tại → kỳ vọng lỗi `ManagedException` "Không tìm thấy gói thầu", không tạo record rác.
2. Gọi API để trống `thongTinDoiChieu.noiDung` → kỳ vọng lưu thành công (nullable).
3. Gọi API không truyền `toTrinhKetQua`/`quyetDinhPheDuyet` (null) → kỳ vọng không tạo `ToTrinhQuyetDinh`/`VanBanQuyetDinh`, response trả `toTrinhQuyetDinhId`/`vanBanQuyetDinhId` = null.

## 5. Test API tổng hợp `GET api/tong-hop-van-ban-quyet-dinh/danh-sach-day-du`

1. Trước khi duyệt Quyết định phê duyệt vừa tạo ở bước 4.2 → gọi API tổng hợp (`?duAnId=...`), xác nhận record **không xuất hiện** trong danh sách trả về.
2. Gọi `PUT /api/to-trinh-tham-dinh-nha-thau/quyet-dinh/{vanBanQuyetDinhId}/duyet` → kiểm tra `VanBanQuyetDinh.TrangThaiDuyetId` được cập nhật sang `Id=72` (`Ma="ĐD"`).
3. Gọi lại API tổng hợp → xác nhận record **xuất hiện** trong danh sách.
4. Kiểm tra hồi quy: các `VanBanQuyetDinh` cũ có `TrangThaiDuyetId = null` (dữ liệu trước migration) **vẫn xuất hiện** trong danh sách (không bị lọc mất).
5. Kiểm tra hồi quy: `VanBanQuyetDinh` của nghiệp vụ khác (`HoSoMoiThauDienTu`, `VanBanPhapLy`, ...) tạo mới sau migration này (không set `TrangThaiDuyetId`) — xác nhận **vẫn xuất hiện** đúng như hành vi cũ.

## 6. Test hồi quy `HoSoMoiThauDienTu` (module bị ảnh hưởng gián tiếp do đổi `ToTrinhQuyetDinh`)

1. Tạo mới hồ sơ mời thầu điện tử có kèm `ToTrinh`/`QuyetDinh`.
2. Cập nhật hồ sơ, đổi nội dung `ToTrinh`/`QuyetDinh`.
3. Duyệt hồ sơ (`HoSoMoiThauDienTuDuyetCommand`) → xác nhận `VanBanQuyetDinh` vẫn được tạo đúng như trước (không có `TrangThaiId` bị set ngoài ý muốn — phải giữ `null` vì đây là nghiệp vụ cũ).
4. Xác nhận không còn tham chiếu nào tới `HoSoMoiThauToTrinhId`/`HoSoMoiThauQuyetDinhId` trong toàn bộ solution (`grep` sau khi sửa xong).

## 7. GitNexus

- Chạy `detect_changes()` trước khi commit để xác nhận phạm vi thay đổi đúng như dự kiến (không đụng module ngoài scope ngoài `HoSoMoiThauDienTu` đã biết trước).
- Chạy `impact({ target: "ToTrinhQuyetDinh", direction: "upstream" })` và `impact({ target: "VanBanQuyetDinh", direction: "upstream" })` trước khi sửa 2 entity này để xác nhận đầy đủ danh sách nơi bị ảnh hưởng, đối chiếu với danh sách đã liệt kê thủ công trong `report.md`.

## 8. Bảng test case chi tiết

| ID | Test case | Input | Kỳ vọng | Trạng thái |
|---|---|---|---|---|
| **TC-01** | Happy path — tạo mới đủ 7 mục | Payload đầy đủ (mục 4.1), `duAnId=1690F8E4-...`, `goiThauId=79CF32A2-...`, `buocId=5613`, `chucVuId=1` | `200 OK`, trả `{id, toTrinhQuyetDinhId, vanBanQuyetDinhId}`; `ToTrinhThamDinhNhaThau` lưu đúng `GoiThauId/TenNhaThau/NgayKetThucDanhGia`, không có `GiaTri/HinhThucLCNT` | ✅ **Pass** (đã verify qua SQL, dòng `Id=08DEF855-...` khớp 100%) |
| **TC-02** | Bảng `ToTrinhThamDinhBuocXuLy` tạo đủ 3 dòng | Cùng payload TC-01 | 3 dòng `ToTrinhId` = Id TC-01, `Loai=1` (Đối chiếu), `Loai=2` (Thương thảo), `Loai=3` (Thẩm định), đúng `So/Ngay/NoiDung` | ⬜ Chưa xác nhận — anh chạy SQL mục 4.2 câu 1 |
| **TC-03** | `ToTrinhQuyetDinh` (Tờ trình kết quả) tạo đúng | Cùng payload TC-01 | 1 dòng `EntityId` = Id TC-01, `Loai=3`, `So=KQ-001`, `NguoiKy=Nguyễn Văn A`, `ChucVu=1`, `TrichYeu` khớp | ⬜ Chưa xác nhận — anh chạy SQL mục 4.2 câu 2 |
| **TC-04** | `VanBanQuyetDinh` (Quyết định phê duyệt) tạo đúng, trạng thái Chờ duyệt | Cùng payload TC-01 | 1 dòng mới, `Loai='ToTrinhThamDinhNhaThau'`, `So=QD-001`, `NguoiKyChucVuId=1`, `TrangThaiDuyetId=71` (ĐTr) — **không phải NULL** | ⬜ Chưa xác nhận |
| **TC-05** | API tổng hợp — Quyết định Chờ duyệt KHÔNG hiển thị | `GET danh-sach-day-du?duAnId=1690F8E4-...` (trước khi duyệt) | Record `QD-001` không có trong kết quả | ⬜ |
| **TC-06** | Duyệt Quyết định phê duyệt | `PUT quyet-dinh/{vanBanQuyetDinhId}/duyet` | `200 OK`; `VanBanQuyetDinh.TrangThaiDuyetId` chuyển `71 → 72` (ĐD) | ⬜ |
| **TC-07** | API tổng hợp — sau khi duyệt PHẢI hiển thị | `GET danh-sach-day-du?duAnId=...` (sau khi duyệt) | Record `QD-001` xuất hiện trong kết quả | ⬜ |
| **TC-08** | Duyệt 2 lần liên tiếp (đã Đã duyệt lại duyệt nữa) | Gọi lại `PUT .../duyet` lần 2 với cùng Id | Lỗi `ManagedException`: "Chỉ có thể duyệt Quyết định khi đang ở trạng thái Chờ duyệt" | ⬜ |
| **TC-09** | `goiThauId` không tồn tại | Payload với `goiThauId` random GUID | Lỗi `ManagedException`: "Không tìm thấy gói thầu"; không tạo record rác trong `ToTrinhThamDinhNhaThau` | ⬜ |
| **TC-10** | Không gửi `toTrinhKetQua`/`quyetDinhPheDuyet` (null) | Payload bỏ 2 field này | `200 OK`; không tạo dòng `ToTrinhQuyetDinh`/`VanBanQuyetDinh`; response `toTrinhQuyetDinhId=null`, `vanBanQuyetDinhId=null` | ⬜ |
| **TC-11** | `thongTinDoiChieu.noiDung = null` | Payload để trống `noiDung` mục Đối chiếu | `200 OK`; dòng `ToTrinhThamDinhBuocXuLy` (Loai=1) có `NoiDung=NULL` | ⬜ |
| **TC-12** | Hồi quy — `HoSoMoiThauDienTu` tạo/sửa/duyệt vẫn hoạt động | Tạo mới + cập nhật `ToTrinh`/`QuyetDinh` + duyệt qua `api/ho-so-moi-thau-dien-tu` | `ToTrinhQuyetDinh` ghi đúng `EntityId/Loai=1,2`; `VanBanQuyetDinh` tạo ra có `TrangThaiDuyetId = NULL` (không bị set) | ⬜ |
| **TC-13** | Hồi quy — dữ liệu `VanBanQuyetDinh` cũ (trước migration) vẫn hiển thị API tổng hợp | `GET danh-sach-day-du` không filter theo `duAnId` cụ thể | Các record cũ có `TrangThaiDuyetId=NULL` vẫn xuất hiện đầy đủ, không bị lọc mất | ⬜ |
