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

## 2026-08-13 — Khảo sát điều chỉnh nghiệp vụ (chưa code)

Nhận yêu cầu điều chỉnh: (1) bỏ bộ trạng thái riêng `ToTrinhThamDinhNhaThauQuyetDinh` (2 trạng thái) → dùng lại `DeXuatMacDinh` (4 trạng thái chung); (2) bỏ API `PUT quyet-dinh/{id}/duyet` riêng, Trình/Duyệt/Trả lại đi qua `QuanLyPheDuyet` có sẵn; (3) đổi `BuocXuLys` (`List<>`) thành 3 property riêng `ThuongThao/DoiChieu/ThamDinh` ở contract API; (4) đổi `Loai` của `ToTrinhThamDinhBuocXuLy` và `ToTrinhQuyetDinh` từ `int` sang `string` có ý nghĩa.

Đã khảo sát (chưa sửa code):
- Xác nhận `ToTrinhThamDinhNhaThau` đã dùng đúng convention 4 trạng thái ở bản thân entity (`TrangThaiId`) — chỉ riêng `VanBanQuyetDinh.TrangThaiDuyetId` (em tự thêm) đang dùng nhóm 2 trạng thái sai, cần sửa.
- Xác nhận `ToTrinhThamDinhNhaThau` **đã được dispatch đầy đủ** trong `PheDuyetDispatchTrinhCommand`/`DuyetCommand`/`TraLaiCommand` từ trước — không cần bổ sung dispatch.
- Xác nhận endpoint `quyet-dinh/{id}/duyet` + `ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand` không được reference ở đâu khác — an toàn để xóa.
- Xác nhận `BuocXuLys` hiện chỉ mới được ghi ở bước `them-moi`, Update/List/GetById hoàn toàn chưa xử lý — việc thêm 3 property riêng là bổ sung lần đầu cho 3 API đó, không phải sửa lại code cũ.
- Xác nhận `Loai` của cả 2 entity đang là `int` thô trong DB, không có `HasConversion` — thuận lợi để đổi sang `string`.
- Phát hiện `ToTrinhQuyetDinh.Loai` dùng CHUNG cột cho cả `HoSoMoiThauDienTu` (2 giá trị) và `ToTrinhThamDinhNhaThau` (1 giá trị) — đổi type sẽ ảnh hưởng cả `HoSoMoiThauDienTuInsertCommand`/`UpdateCommand`/`DuyetCommand` (không thể tránh, do dùng chung cột).
- Phát hiện vấn đề thiết kế: `VanBanQuyetDinh` được tạo ở `them-moi` với Id tự sinh riêng, không có cách tra cứu ngược từ `ToTrinhThamDinhNhaThau.Id` → cần quyết định trước khi implement (đề xuất set `VanBanQuyetDinh.Id = ToTrinhThamDinhNhaThau.Id`, giống pattern `HoSoMoiThauDienTuDuyetCommand`).

Đã viết `report-dieu-chinh.md` — trả lời đầy đủ 23 câu hỏi mục 21 + liệt kê file impact + 3 điểm cần xác nhận trước khi code. **Chưa implement.**

## 2026-08-13 (tiếp — implement điều chỉnh)

Người yêu cầu xác nhận implement theo `report-dieu-chinh.md` (dùng phương án đề xuất ở cả 3 điểm còn treo: (1) `VanBanQuyetDinh.Id = ToTrinhThamDinhNhaThau.Id`, (2) danh sách không cần trả 3 object, (3) constant đặt tại `QLDA.Domain/Constants/`). Đã implement:

1. **`Loai` int → string**: `ToTrinhThamDinhBuocXuLy.Loai` và `ToTrinhQuyetDinh.Loai` đổi từ `int` sang `string`. Tạo 2 constant mới `QLDA.Domain/Constants/ToTrinhThamDinhBuocXuLyLoai.cs` (`DoiChieu/ThuongThao/ThamDinh`) và `ToTrinhQuyetDinhLoai.cs` (`HoSoMoiThauToTrinh/HoSoMoiThauQuyetDinh/ToTrinhThamDinhNhaThau`). Xóa 2 enum `ELoaiBuocXuLyThamDinhNhaThau`/`ELoaiToTrinhQuyetDinh`.
2. **Sửa `HoSoMoiThauDienTu` Insert/Update/Duyệt Command**: đổi so sánh `Loai == (int)ELoaiToTrinhQuyetDinh.X` → `Loai == ToTrinhQuyetDinhLoai.X` (bắt buộc phải sửa cả module này vì dùng chung cột `ToTrinhQuyetDinh.Loai`).
3. **Bỏ bộ trạng thái riêng**: xóa `TrangThaiPheDuyetCodes.ToTrinhThamDinhNhaThauQuyetDinh`, xóa seed `DmTrangThaiPheDuyet.Id=71,72`. `VanBanQuyetDinh.TrangThaiDuyetId` giờ dùng chung nhóm `DeXuatMacDinhStt` (Id=30..33, DT/ĐTr/ĐD/TL) — đồng bộ đúng với `TrangThaiId` của `ToTrinhThamDinhNhaThau`.
4. **Bỏ API duyệt quyết định riêng**: xóa `ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand` + endpoint `PUT quyet-dinh/{id}/duyet`. `ToTrinhThamDinhNhaThauDuyetCommand` (dispatch qua `QuanLyPheDuyet`, đã có từ trước) giờ tự tìm `VanBanQuyetDinh` theo `Id == entity.Id && Loai == "ToTrinhThamDinhNhaThau"` và đồng bộ `TrangThaiDuyetId` sang "Đã duyệt" khi duyệt Tờ trình thành công.
5. **`VanBanQuyetDinh.Id = ToTrinhThamDinhNhaThau.Id`**: đổi trong `ToTrinhThamDinhNhaThauThemMoiCommand` (giống pattern `HoSoMoiThauDienTuDuyetCommand`), để có cách tra cứu tường minh mà không cần thêm cột FK mới.
6. **`BuocXuLys` (List) → 3 object riêng**: thêm `SyncBuocXuLys`/`ToBuocXuLyList` (dùng chung cho Create/Update, không duplicate logic) trong `ToTrinhThamDinhNhaThauMappings.cs`. Domain Entity vẫn giữ `List<ToTrinhThamDinhBuocXuLy>` (DB vẫn 1-N vật lý) — chỉ đổi ở tầng contract (DTO/Model): `ToTrinhThamDinhNhaThauThemMoiDto`, `ToTrinhThamDinhNhaThauDto`, `ToTrinhThamDinhNhaThauModel` đều thêm `DoiChieu/ThuongThao/ThamDinh` thay cho `BuocXuLys`.
7. **`ToTrinhThamDinhNhaThauUpdateCommand`**: bổ sung `Include(e => e.BuocXuLys)` + gọi `SyncBuocXuLys` (trước đây hoàn toàn chưa xử lý `BuocXuLys`).
8. **`ToTrinhThamDinhNhaThauGetQuery`**: bổ sung `Include(e => e.BuocXuLys)`.
9. **Controller**: xóa endpoint `DuyetQuyetDinh`; `Create`/`Update` đổi theo contract 3 object mới, lưu file đúng `GroupType` cho từng bước; `Get` (chi tiết) load thêm file của 3 bước và trả vào `ThuongThao/DoiChieu/ThamDinh`.
10. Migration mới `20260813032522_Issue179_LoaiToString` — **chỉnh tay** thứ tự Up/Down (thêm cột tạm → backfill theo giá trị int cũ → drop cột int → rename) để giữ đúng dữ liệu cũ khi đổi `int → string` (EF tự sinh sẽ cast số thành chuỗi số, sai ngữ nghĩa). **Chưa apply** — người dùng tự chạy `ef.bat QLDA update`.
11. `dotnet build SER.sln` — 0 lỗi. `dotnet ef migrations list` xác nhận migration mới ở trạng thái `(Pending)`.

**Không tạo thêm** API Trình/Duyệt/Trả lại riêng — xác nhận `ToTrinhThamDinhNhaThau` đã dispatch đầy đủ qua `QuanLyPheDuyet` từ trước, không cần đụng vào 3 file dispatch.

## 2026-08-14 — Dọn schema tờ trình (5 prop cũ + `TenNhaThau` → `NhaThauId`)

Requirement ban đầu nhầm `TenNhaThau` (lưu tên). Đổi sang FK nhà thầu; đồng thời xóa 5 property/cột cũ trên `ToTrinhThamDinhNhaThau` không còn dùng sau spec 1 gói / 1 nhà thầu.

1. **Xóa 5 prop** trên entity `ToTrinhThamDinhNhaThau`: `So`, `NgayTrinh`, `TrichYeu`, `DaThamDinh`, `NhaThaus` (`List<KetQuaThamDinhNhaThau>`). Bảng `KetQuaThamDinhNhaThau` **giữ** (FK `ToTrinhId` → `WithMany()`). `So`/`Ngay`/`TrichYeu` trên `ToTrinhThamDinhBuocXuLy` / `ToTrinhQuyetDinh` / `VanBanQuyetDinh` không đụng.
2. **`TenNhaThau` → `NhaThauId`**: `Guid?` (không phải `int` — `DanhMucNhaThau : DanhMuc<Guid>`, PK `DmNhaThau.Id` là `uniqueidentifier`) + navigation `DanhMucNhaThau? NhaThau`. EF: FK Restrict, nullable, index `IX_ToTrinhThamDinhNhaThau_NhaThauId`.
3. **Application / WebApi**: `ThongTinNhaThauDto.NhaThauId`; ThemMoi validate nhà thầu tồn tại; Update/Get/List map `nhaThauId`. Contract `them-moi`: `thongTinNhaThau.nhaThauId` (Guid). Get/Update/List: top-level `nhaThauId`. Không trả `tenNhaThau`.
4. Migration (EF generate, không sửa tay / không sửa snapshot thủ công):
   - `20260814075120_Issue179_RemoveLegacyToTrinhThamDinhNhaThauFields` — drop `DaThamDinh`, `NgayTrinh`, `So`, `TrichYeu`.
   - `20260814075953_Issue179_ReplaceTenNhaThauWithNhaThauId` — drop `TenNhaThau`, add `NhaThauId` + FK `DmNhaThau`.
5. `dotnet build SER.sln` — 0 lỗi. **Chưa** `database update` trừ khi người dùng chạy tay.

Dead code còn sót (không map cột đã xóa, ngoài scope tối thiểu): `ToTrinhThamDinhNhaThauSearchDto.So`/`TrichYeu`, `ToTrinhThamDinhNhaThauDanhSachQuery.So`/`TrichYeu` (list không filter theo 2 field này nữa), class `KetQuaThamDinhNhaThauDto` không còn caller.

## 2026-08-17 — Khảo sát GET chi tiết (chưa code)

Người yêu cầu: `GET /api/to-trinh-tham-dinh-nha-thau/{id}/chi-tiet` thiếu `GoiThauId`, `ThongTinNhaThauDto`, `ToTrinhKetQuaDto`, `QuyetDinhPheDuyetDto`. Viết docs trước, chờ xác nhận rồi mới implement.

Đã đọc flow Controller → GetQuery → DTO/Model mapping → Persistence. Kết luận chính:

- `GoiThauId` / `NhaThauId` / `NgayKetThucDanhGia` **đã có trên entity**; Get DTO/Model + `ToModel`/`ToDto` không expose `GoiThauId` và không dựng object `ThongTinNhaThau`.
- `ToTrinhKetQua` / `QuyetDinhPheDuyet` **không nằm trên parent** — lưu `ToTrinhQuyetDinh` (`EntityId`+`Loai`) và `VanBanQuyetDinh` (`Id` trùng tờ trình + `Loai`). GetQuery không đọc 2 bảng này; không load file `ToTrinhQuyetDinh` / `ToTrinhThamDinhNhaThau_QuyetDinh` / `FileEHSDT` / `FileDanhGia`.
- Nested DTO **đã có** trong `ToTrinhThamDinhNhaThauThemMoiDto.cs` — không tạo type mới.
- Get runtime trả `ToModel` (WebApi) dù `[ProducesResponseType]` khai `ToTrinhThamDinhNhaThauDto`.
- Không cần migration.

Ghi nhận root cause từng field, file sửa, nguồn dữ liệu ngay tại mục này; cập nhật `index.md`, `test-workflow.md`. **Chưa sửa code** (implement ở phần tiếp theo).

## 2026-08-17 (tiếp — implement GET chi tiết + check conflict với dev D)

Sau khi xác nhận plan, implement `GET /api/to-trinh-tham-dinh-nha-thau/{id}/chi-tiet` trên branch `bugfix/to-trinh-td-nha-thau-chi-tiet`. Chi tiết đã sửa:

1. **Check conflict trước khi làm**: dev D đang bổ sung `GoiThauId` cho `danh-sach-tien-do` — phần đó nằm ở list DTO `ToTrinhThamDinhNhaThauDto` + projection inline trong `ToTrinhThamDinhNhaThauGetDanhSachQueryHandler` (`ToTrinhThamDinhNhaThauGetDanhSachQuery.cs`). Task chi-tiet **không đụng** 2 file đó; điểm chung duy nhất là file Controller nhưng **khác method** (`Get(Guid id)` chi-tiet vs `Get([FromQuery])` danh-sach), diff xác nhận không đè vùng code của dev D → không bắt buộc sửa chung DTO/Query/Mapping của dev D, an toàn để implement mà không động logic danh-sach-tien-do.
2. **`ToTrinhThamDinhNhaThauChiTietDto`** (file mới): DTO riêng cho chi-tiet — `GoiThauId`, `NhaThauId`, `ThongTinNhaThau`, `DoiChieu`/`ThuongThao`/`ThamDinh`, `ToTrinhKetQua`, `QuyetDinhPheDuyet`. Nested DTO tái dùng từ `ToTrinhThamDinhNhaThauThemMoiDto.cs` (`ThongTinNhaThauDto`/`ToTrinhKetQuaDto`/`QuyetDinhPheDuyetDto`) — không tạo type mới.
3. **`ToTrinhThamDinhNhaThauGetChiTietQuery`** (file mới): load entity kèm `BuocXuLys`; đọc `ToTrinhQuyetDinh` (`EntityId == request.Id && Loai == ToTrinhThamDinhNhaThau`) và `VanBanQuyetDinh` (`Id == request.Id && Loai == ToTrinhThamDinhNhaThau`); trả `ToTrinhThamDinhNhaThauChiTietResult`.
4. **`ToTrinhThamDinhNhaThauMappings.ToChiTietDto`**: map đủ 4 thành phần bổ sung; **không sửa `ToDto`** (dùng cho list/PUT) để tránh đổi shape list của dev D.
5. **Controller `Get(Guid id)`**: bỏ `ToTrinhThamDinhNhaThauGetQuery` cũ, gọi `ToTrinhThamDinhNhaThauGetChiTietQuery`; load thêm file `FileEHSDT`/`FileDanhGia` (GroupId = id tờ trình), file Tờ trình kết quả (GroupId = `ToTrinhQuyetDinh.Id`, kiểu long), file Quyết định (GroupId = `VanBanQuyetDinh.Id`); trả `ResultApi.Ok(entity.ToChiTietDto(...))`. Method `Get([FromQuery])` của `danh-sach-tien-do` **giữ nguyên**.
6. `dotnet build QLDA.WebApi` — 0 warning / 0 error. Không cần migration (schema đã có đủ `GoiThauId`/`NhaThauId`/`NgayKetThucDanhGia`, và 2 bảng `ToTrinhQuyetDinh`/`VanBanQuyetDinh` đã có từ trước).

## 2026-08-21 — Khảo sát GET danh sách HSMTĐT 400 (chưa code)

`GET /api/ho-so-moi-thau-dien-tu/danh-sach` trả 400 `"Lỗi hệ thống, vui lòng thử lại sau"`.

Xác nhận exception thật (terminal WebApi 09:06:39, URL `duAnId=08def36f-...`):

`System.InvalidOperationException: The expression 'e.ToTrinh' is invalid inside an 'Include' operation...`

Nổ tại `HoSoMoiThauDienTuGetDanhSachQuery.cs:68` (`PaginatedListAsync` → `CountAsync`) vì dòng 34–35 `.Include(e => e.ToTrinh).Include(e => e.QuyetDinh)` trong khi entity đã `[NotMapped]` từ Issue #179. Insert/Update/Duyệt đã load `EntityId + Loai`; Get/danh-sach sót Include cũ.

`Loai` dùng constant: `ToTrinhQuyetDinhLoai.HoSoMoiThauToTrinh` / `HoSoMoiThauQuyetDinh`. Pattern batch: `ToTrinhThamDinhNhaThauGetDanhSachQuery`. GetQuery `{id}` cùng Include — đề xuất sửa cùng. Không migration.

Docs: `hoso-danh-sach.md`. **Chưa sửa code.**

