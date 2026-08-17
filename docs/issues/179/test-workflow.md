# Kế hoạch kiểm thử — Issue 179

> **Đã implement xong code + migration (chưa apply DB)**. Tài liệu này là hướng dẫn test thực tế — cập nhật lại theo code đã viết (field `VanBanQuyetDinh.TrangThaiDuyetId`, không phải `TrangThaiId`).

## 1. Build & lint

```powershell
dotnet build SER.sln
```

- Kết quả hiện tại: **0 Error(s)**.

## 2. Migration

Các migration Issue 179 (EF generate). **Chưa apply** trừ khi đã chạy tay:

| Migration | Việc |
|---|---|
| `20260812075056_Issue179_ToTrinhThamDinhNhaThau` | EntityId/Loai, BuocXuLy, GoiThauId, TenNhaThau (sau đó bị thay) |
| `20260813032522_Issue179_LoaiToString` | `Loai` int → string |
| `20260814075120_Issue179_RemoveLegacyToTrinhThamDinhNhaThauFields` | Drop `So`/`NgayTrinh`/`TrichYeu`/`DaThamDinh` trên `ToTrinhThamDinhNhaThau` |
| `20260814075953_Issue179_ReplaceTenNhaThauWithNhaThauId` | Drop `TenNhaThau`, add `NhaThauId` FK `DmNhaThau` |

```powershell
ef.bat QLDA update
```

hoặc:

```powershell
dotnet ef database update --project QLDA.Migrator\QLDA.Migrator.csproj --startup-project QLDA.Migrator\QLDA.Migrator.csproj --context AppDbContext
```

- Xác nhận connection string dev/test trong `QLDA.WebApi/appsettings.Development.json` trước khi chạy.
- Sau khi update: `ToTrinhThamDinhNhaThau` **không** còn cột `So`/`NgayTrinh`/`TrichYeu`/`DaThamDinh`/`TenNhaThau`; có `NhaThauId` (uniqueidentifier, nullable) + FK `DmNhaThau`. `ToTrinhThamDinhBuocXuLy.Loai` là string (`DoiChieu`/`ThuongThao`/`ThamDinh`). Không seed `DmTrangThaiPheDuyet` Id=71/72.

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
- 1 `NhaThauId` hợp lệ (`GET` danh mục nhà thầu / bảng `DmNhaThau` — **Guid**, không phải int).
- 1 `ChucVuId` hợp lệ (`GET api/dm-chuc-vu`).
- Không bắt buộc phải có file thật để test nhanh — có thể để `null`/bỏ trống các mảng `File`, `FileEHSDT`, `FileDanhGia` (Command chỉ gọi `AttachmentBulkInsertOrUpdateCommand` khi `Count > 0`).

### 4.1. Payload mẫu

```json
{
  "duAnId": "00000000-0000-0000-0000-000000000000",
  "buocId": 1,
  "goiThauId": "00000000-0000-0000-0000-000000000000",
  "trangThaiDangTaiId": null,
  "thongTinNhaThau": {
    "nhaThauId": "00000000-0000-0000-0000-000000000000",
    "fileEHSDT": [],
    "ngayKetThucDanhGia": "2026-08-10T00:00:00+07:00",
    "fileDanhGia": []
  },
  "doiChieu": {
    "so": "DC-001",
    "ngay": "2026-08-05T00:00:00+07:00",
    "noiDung": null,
    "file": []
  },
  "thuongThao": {
    "so": "TT-002",
    "ngay": "2026-08-06T00:00:00+07:00",
    "noiDung": "Nội dung thương thảo",
    "file": []
  },
  "thamDinh": {
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

Thay `duAnId`/`goiThauId`/`nhaThauId`/`chucVuId` bằng dữ liệu thật lấy ở bước 4.0. Payload **không** gửi `so`/`ngayTrinh`/`trichYeu`/`tenNhaThau` ở cấp tờ trình.

### 4.2. Happy path
1. Gọi `POST /api/to-trinh-tham-dinh-nha-thau/them-moi` với payload trên.
2. Kiểm tra response trả về `{ id, toTrinhQuyetDinhId, vanBanQuyetDinhId }`.
3. Query DB xác nhận:
   - `ToTrinhThamDinhNhaThau.GoiThauId` / `NhaThauId` / `NgayKetThucDanhGia` khớp payload; **không** có cột `TenNhaThau`/`So`/`NgayTrinh`/`TrichYeu`/`DaThamDinh`; không có `GiaTri`/`HinhThucLCNT`.
   - `ToTrinhThamDinhBuocXuLy` có đúng 3 dòng (`Loai='DoiChieu'|'ThuongThao'|'ThamDinh'`), cùng `ToTrinhId`.
   - `ToTrinhQuyetDinh` 1 dòng, `EntityId` = id Tờ trình, `Loai = 'ToTrinhThamDinhNhaThau'`.
   - `VanBanQuyetDinh.Id` = id Tờ trình, `Loai = 'ToTrinhThamDinhNhaThau'`, `TrangThaiDuyetId` = trạng thái **Dự thảo** (`DeXuatMacDinh.DT`), không phải seed 71/72.

### 4.3. Validate
1. Gọi API với `goiThauId` không tồn tại → kỳ vọng lỗi `ManagedException` "Không tìm thấy gói thầu", không tạo record rác.
2. Gọi API với `thongTinNhaThau.nhaThauId` không tồn tại → kỳ vọng lỗi `ManagedException` "Không tìm thấy nhà thầu".
3. Gọi API để trống `doiChieu.noiDung` → kỳ vọng lưu thành công (nullable).
4. Gọi API không truyền `toTrinhKetQua`/`quyetDinhPheDuyet` (null) → kỳ vọng không tạo `ToTrinhQuyetDinh`/`VanBanQuyetDinh`, response trả `toTrinhQuyetDinhId`/`vanBanQuyetDinhId` = null.

## 5. Test API tổng hợp `GET api/tong-hop-van-ban-quyet-dinh/danh-sach-day-du`

1. Trước khi duyệt Tờ trình vừa tạo → gọi API tổng hợp (`?duAnId=...`), xác nhận quyết định **không xuất hiện** (trạng thái chưa `ĐD`).
2. Duyệt qua `QuanLyPheDuyet` (không còn `PUT quyet-dinh/{id}/duyet`) — `ToTrinhThamDinhNhaThauDuyetCommand` đồng bộ `VanBanQuyetDinh.TrangThaiDuyetId` sang `ĐD`.
3. Gọi lại API tổng hợp → record **xuất hiện**.
4. Hồi quy: `VanBanQuyetDinh` cũ `TrangThaiDuyetId = null` **vẫn xuất hiện**.
5. Hồi quy: nghiệp vụ khác tạo mới không set `TrangThaiDuyetId` — vẫn xuất hiện.

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
| **TC-01** | Happy path — tạo mới đủ 7 mục | Payload mục 4.1, `nhaThauId` = Guid `DmNhaThau` | `200 OK`, `{id, toTrinhQuyetDinhId, vanBanQuyetDinhId}`; lưu `GoiThauId`/`NhaThauId`/`NgayKetThucDanhGia`; **không** cột `TenNhaThau`/`So`/`NgayTrinh`/`TrichYeu` | ⬜ Cần re-test sau migration 2026-08-14 |
| **TC-02** | `ToTrinhThamDinhBuocXuLy` đủ 3 dòng | Cùng payload TC-01 | 3 dòng, `Loai='DoiChieu'/'ThuongThao'/'ThamDinh'` | ⬜ |
| **TC-03** | `ToTrinhQuyetDinh` tạo đúng | Cùng payload TC-01 | `EntityId` = Id TC-01, `Loai='ToTrinhThamDinhNhaThau'` | ⬜ |
| **TC-04** | `VanBanQuyetDinh` tạo đúng, trạng thái Dự thảo | Cùng payload TC-01 | `Id` = Id tờ trình, `Loai='ToTrinhThamDinhNhaThau'`, `TrangThaiDuyetId` = DT (không phải 71) | ⬜ |
| **TC-05** | API tổng hợp — chưa duyệt KHÔNG hiển thị | `GET danh-sach-day-du?duAnId=...` | Record QD không có | ⬜ |
| **TC-06** | Duyệt qua QuanLyPheDuyet | Dispatch duyệt `ToTrinhThamDinhNhaThau` | `VanBanQuyetDinh.TrangThaiDuyetId` → ĐD | ⬜ |
| **TC-07** | API tổng hợp — sau duyệt PHẢI hiển thị | Sau TC-06 | Record xuất hiện | ⬜ |
| **TC-08** | Duyệt 2 lần | Gọi duyệt lần 2 | Lỗi: chỉ duyệt khi Đã trình | ⬜ |
| **TC-09** | `goiThauId` không tồn tại | GUID random | `ManagedException` "Không tìm thấy gói thầu" | ⬜ |
| **TC-09b** | `nhaThauId` không tồn tại | GUID random trong `thongTinNhaThau` | `ManagedException` "Không tìm thấy nhà thầu" | ⬜ |
| **TC-10** | Không gửi `toTrinhKetQua`/`quyetDinhPheDuyet` | Bỏ 2 field | Không tạo dòng tương ứng | ⬜ |
| **TC-11** | `doiChieu.noiDung = null` | Để trống nội dung Đối chiếu | `NoiDung=NULL` trên dòng `Loai='DoiChieu'` | ⬜ |
| **TC-12** | Hồi quy `HoSoMoiThauDienTu` | Tạo/sửa/duyệt | `ToTrinhQuyetDinh.Loai` string `HoSoMoiThauToTrinh`/`HoSoMoiThauQuyetDinh` | ⬜ |
| **TC-13** | Hồi quy VanBanQuyetDinh cũ | `GET danh-sach-day-du` | `TrangThaiDuyetId=NULL` vẫn hiện | ⬜ |
| **TC-14** | Get/Update/List dùng `nhaThauId` | Chi tiết + cap-nhat + danh-sach-tien-do | JSON `nhaThauId` (Guid), không `tenNhaThau` | ⬜ |
| **TC-15** | GET chi tiết đủ 4 field mới | Sau TC-01, `GET .../{id}/chi-tiet` | Có `goiThauId`, `thongTinNhaThau`, `toTrinhKetQua`, `quyetDinhPheDuyet` khớp payload ThemMoi; file đúng GroupType | ⬜ Chờ test (đã implement 2026-08-17) |
| **TC-16** | GET chi tiết khi ThemMoi bỏ mục 6–7 | Sau TC-10 | `toTrinhKetQua`/`quyetDinhPheDuyet` = `null`; `goiThauId` + `thongTinNhaThau` vẫn có | ⬜ Chờ test (đã implement 2026-08-17) |

## 9. Test API `GET api/to-trinh-tham-dinh-nha-thau/{id}/chi-tiet`

> **Đã implement 2026-08-17** (branch `bugfix/to-trinh-td-nha-thau-chi-tiet`). `dotnet build QLDA.WebApi` — 0 warning / 0 error. Chạy các bước sau để verify.

1. Happy path: tạo tờ trình đủ 7 mục (TC-01) → GET chi tiết → JSON có `goiThauId`, `thongTinNhaThau` (`nhaThauId`, `ngayKetThucDanhGia`, `fileEHSDT`, `fileDanhGia`), `toTrinhKetQua`, `quyetDinhPheDuyet`. Giữ field cũ: `doiChieu`/`thuongThao`/`thamDinh`, top-level `nhaThauId`.
2. File: EHSDT/Đánh giá `GroupId` = id tờ trình; file tờ trình kết quả `GroupId` = `ToTrinhQuyetDinh.Id` (long); file quyết định `GroupId` = id tờ trình (`VanBanQuyetDinh.Id`).
3. Bỏ `toTrinhKetQua`/`quyetDinhPheDuyet` khi tạo → GET trả `null` 2 object đó; `goiThauId` + `thongTinNhaThau` vẫn có.
4. Record sẵn `08defc12-4e20-3b60-687a-7b38f8073d8e`: đối chiếu cột DB với JSON, không đoán.
5. Hồi quy `danh-sach-tien-do`: gọi GET danh sách → shape list **không đổi** (không thêm field chi-tiet vào list).
