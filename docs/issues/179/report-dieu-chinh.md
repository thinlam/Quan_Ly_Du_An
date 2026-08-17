# Báo cáo khảo sát — Điều chỉnh nghiệp vụ Tờ trình thẩm định nhà thầu (tiếp theo Issue #179)

> Trạng thái: **ĐÃ IMPLEMENT** (xem `journal.md`).  
> **Cập nhật 2026-08-14:** entity không còn `So`/`NgayTrinh`/`TrichYeu`/`DaThamDinh`/`NhaThaus`/`TenNhaThau`; dùng `NhaThauId` (`Guid?`). Các câu Q11–Q15 bên dưới mô tả source **tại thời điểm khảo sát 2026-08-13** — Update/Get/List sau đó đã được sửa (BuocXuLys + `NhaThauId`).

---

## A. Trạng thái

**Q1. `ToTrinhThamDinhNhaThau` hiện đang dùng những constant trạng thái nào?**

Bản thân entity `ToTrinhThamDinhNhaThau.TrangThaiId` (workflow của chính Tờ trình) **đã dùng đúng convention 4 trạng thái chung** `PheDuyetEntityNames.DeXuatMacDinhStt` (`TrangThaiPheDuyetCodes.DeXuatMacDinh.{DuThao,DaTrinh,DaDuyet,TraLai}` = `DT/ĐTr/ĐD/TL`) — xem `ToTrinhThamDinhNhaThauInsertCommand.cs`, `ToTrinhThamDinhNhaThauTrinhCommand.cs`, `ToTrinhThamDinhNhaThauTraLaiCommand.cs`, `ToTrinhThamDinhNhaThauDuyetCommand.cs`. **Phần này đã đúng theo yêu cầu task, không cần sửa.**

Bộ `TrangThaiPheDuyetCodes.ToTrinhThamDinhNhaThauQuyetDinh` (`ChoDuyet="ĐTr"`, `DaDuyet="ĐD"`) mà task yêu cầu bỏ là **field riêng em tự tạo ở lượt trước cho `VanBanQuyetDinh.TrangThaiDuyetId`** — không phải trạng thái của bản thân `ToTrinhThamDinhNhaThau`. Đây chính là phần cần điều chỉnh.

**Q2. `ToTrinhThamDinhNhaThauQuyetDinh` đang được reference ở đâu?**

3 nơi:
- `QLDA.Domain/Constants/TrangThaiPheDuyetCodes.cs` — khai báo class.
- `QLDA.Persistence/Configurations/DanhMuc/DanhMucTrangThaiPheDuyetConfiguration.cs` — seed 2 dòng `Id=71,72`.
- `QLDA.Application/ToTrinhThamDinhNhaThau/Commands/ToTrinhThamDinhNhaThauThemMoiCommand.cs` — dùng để set `VanBanQuyetDinh.TrangThaiDuyetId` khi tạo mới.
- `QLDA.Application/ToTrinhThamDinhNhaThau/Commands/ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand.cs` — dùng để tìm trạng thái Chờ duyệt/Đã duyệt khi duyệt.

**Q3. Có thể thay bằng `DeXuatMacDinh` trực tiếp không?**

**Có, và nên làm vậy.** `VanBanQuyetDinh.TrangThaiDuyetId` của Tờ trình thẩm định nhà thầu nên trỏ tới **cùng nhóm `DeXuatMacDinhStt`** (`Id=30..33`, `DT/ĐTr/ĐD/TL`) mà `ToTrinhThamDinhNhaThau.TrangThaiId` đang dùng — không cần seed riêng 2 dòng `Id=71,72` nữa. Về bản chất, trạng thái của Quyết định (`VanBanQuyetDinh`) **luôn đồng bộ với trạng thái của Tờ trình** (Tờ trình `ĐTr` → Quyết định `ĐTr`; Tờ trình `ĐD` → Quyết định `ĐD`), nên dùng chung 1 nhóm danh mục là hợp lý, không phải 2 nhóm khác nhau.

**Q4. Có logic nào đang giả định tờ trình chỉ có `ĐTr/ĐD` không?**

Không. `ToTrinhThamDinhNhaThauThemMoiCommand` hiện tại **không** set `TrangThaiId` bằng `ĐTr` — nó set bằng `DT` (Dự thảo, đúng theo convention 4 trạng thái, giống `ToTrinhThamDinhNhaThauInsertCommand` cũ). Chỉ có `VanBanQuyetDinh.TrangThaiDuyetId` là bị gán cứng `ĐTr` (giả định sai — coi Quyết định luôn ở trạng thái "Chờ duyệt" ngay từ khi tạo, dù Tờ trình đang ở Dự thảo). **Đây là lỗi logic cần sửa**: khi tạo mới (Dự thảo), `VanBanQuyetDinh.TrangThaiDuyetId` phải đồng bộ = `DT`, không phải `ĐTr`.

---

## B. `QuanLyPheDuyet`

**Q5. `QuanLyPheDuyet` hiện xử lý Trình/Duyệt/Trả lại như thế nào?**

Pattern dispatch tập trung: `PheDuyetDispatchTrinhCommand`/`PheDuyetDispatchDuyetCommand`/`PheDuyetDispatchTraLaiCommand` — mỗi command switch theo `request.Type` (string, giá trị = `PheDuyetEntityNames.*`) để route sang command cụ thể của từng entity nghiệp vụ. Dispatch Duyệt/Trả lại còn check quyền `EnsureCanApproveDuAnAsync` ở tầng dispatch trước khi route.

**Q6. `ToTrinhThamDinhNhaThau` đã được dispatch trong `QuanLyPheDuyet` chưa?**

**Đã có đầy đủ cả 3** (đã khảo sát trực tiếp, xem file):
- `PheDuyetDispatchTrinhCommand.cs` dòng 50: `PheDuyetEntityNames.ToTrinhThamDinhNhaThau => new ToTrinhThamDinhNhaThauTrinhCommand(...)`.
- `PheDuyetDispatchDuyetCommand.cs` dòng 55: `... => new ToTrinhThamDinhNhaThauDuyetCommand(request.Id)`.
- `PheDuyetDispatchTraLaiCommand.cs` dòng 55: `... => new ToTrinhThamDinhNhaThauTraLaiCommand(...)`.

**Q7. Nếu chưa thì cần bổ sung ở đâu?**

Không cần — đã có sẵn từ trước (không phải do em thêm ở issue #179). **Không cần sửa 3 file dispatch này.**

---

## C. API duyệt quyết định riêng

**Q8. Endpoint `quyet-dinh/{vanBanQuyetDinhId}/duyet` đang nằm ở controller nào?**

`QLDA.WebApi/Controllers/ToTrinhThamDinhNhaThauController.cs`, method `DuyetQuyetDinh` (`[HttpPut("quyet-dinh/{vanBanQuyetDinhId}/duyet")]`).

**Q9. Command/Handler nào chỉ phục vụ endpoint này?**

`QLDA.Application/ToTrinhThamDinhNhaThau/Commands/ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand.cs` (record + handler `ToTrinhThamDinhNhaThauDuyetQuyetDinhCommandHandler`) — **chỉ được gọi từ endpoint này**, không nơi nào khác trong solution reference tới (đã grep xác nhận).

**Q10. Remove endpoint thì những class nào trở thành dead code?**

- `ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand` + handler — xóa toàn bộ file.
- `TrangThaiPheDuyetCodes.ToTrinhThamDinhNhaThauQuyetDinh` — xóa (thay bằng `DeXuatMacDinh` theo Q3).
- 2 dòng seed `Id=71,72` trong `DanhMucTrangThaiPheDuyetConfiguration.cs` — xóa (không cần nhóm danh mục riêng nữa). **Chú ý:** đây là seed data đã được migrate — cần migration mới để xóa 2 dòng này khỏi DB (không sửa migration cũ).
- Method `DuyetQuyetDinh` trong controller — xóa.

---

## D. Bước xử lý (`BuocXuLys`)

**Q11. `BuocXuLys` hiện đang xuất hiện ở những file nào?**

- `QLDA.Domain/Entities/ToTrinhThamDinhNhaThau.cs` — property `public List<ToTrinhThamDinhBuocXuLy>? BuocXuLys { get; set; }`.
- `QLDA.Persistence/Configurations/ToTrinhThamDinhBuocXuLyConfiguration.cs` — `WithMany(e => e.BuocXuLys)`.
- `QLDA.Application/ToTrinhThamDinhNhaThau/Commands/ToTrinhThamDinhNhaThauThemMoiCommand.cs` — tạo 3 dòng `ToTrinhThamDinhBuocXuLy` (Loai=1,2,3) khi `them-moi`.
- Migration `20260812075056_Issue179_ToTrinhThamDinhNhaThau.cs`.

**Q12. Insert đang lưu `BuocXuLys` như thế nào?**

Chỉ có `ToTrinhThamDinhNhaThauThemMoiCommand` (API `them-moi` mới) lưu — dùng 1 `foreach` loop qua tuple `(ThongTinBuocXuLyDto?, ELoaiBuocXuLyThamDinhNhaThau)[]`, `AddAsync` từng dòng nếu DTO tương ứng khác null. **`ToTrinhThamDinhNhaThauInsertCommand` cũ (API insert cũ, không dùng cho `them-moi` mới) hoàn toàn không biết gì về `BuocXuLys`.**

**Q13. Update đang sync `BuocXuLys` như thế nào?**

**Chưa xử lý gì** *(tại thời điểm khảo sát 2026-08-13)* — `ToTrinhThamDinhNhaThauUpdateCommand.cs` lúc đó chỉ update `So/NgayTrinh/TrichYeu/TrangThaiDangTaiId/DaThamDinh` + `SyncNhaThauIds`. **Đã bổ sung** `Include(BuocXuLys)` + `SyncBuocXuLys` (2026-08-13). **2026-08-14:** Update không còn gán `So`/`NgayTrinh`/`TrichYeu`/`DaThamDinh`; gán `NhaThauId`; `SyncNhaThauIds` đã xóa.

**Q14. List đang map `BuocXuLys` như thế nào?**

**Không map 3 bước** — `ToTrinhThamDinhNhaThauDanhSachQuery` không trả `DoiChieu`/`ThuongThao`/`ThamDinh` (đã xác nhận không bắt buộc). **2026-08-14:** list select `NhaThauId` (không còn `So`/`NgayTrinh`/`TrichYeu`/`DaThamDinh`/`TenNhaThau`).

**Q15. GetById đang map `BuocXuLys` như thế nào?**

`ToTrinhThamDinhNhaThauGetQuery` **đã** `.Include(e => e.BuocXuLys)` (2026-08-13). `ToModel()` trả `DoiChieu`/`ThuongThao`/`ThamDinh` + `NhaThauId`. Không còn `.Include(e => e.NhaThaus)`.

**Kết luận D:** hiện tại `BuocXuLys` **chỉ mới được ghi ở bước Insert (`them-moi`)**, hoàn toàn chưa được đọc lại ở Update/List/GetById. Việc chuyển từ `List<>` sang 3 property riêng (`ThuongThao/DoiChieu/ThamDinh`) là làm **lần đầu** cho Update/List/GetById (không phải sửa lại code cũ dùng List), và **sửa lại phần đã có** ở `them-moi`.

---

## E. Loại (`Loai`)

**Q16-17. `ToTrinhThamDinhBuocXuLy.Loai`:** hiện là `int` (property `public int Loai { get; set; }`, gán qua enum `ELoaiBuocXuLyThamDinhNhaThau` rồi cast `(int)`). Cột DB: `int NOT NULL` (xác nhận qua `AppDbContextModelSnapshot.cs`, không có `HasConversion`/`ValueConverter` nào — lưu số thô 1/2/3).

**Q18-19. `ToTrinhQuyetDinh.Loai`:** hiện là `int` (property `public int Loai { get; set; }`, gán qua enum `ELoaiToTrinhQuyetDinh`). Cột DB: `int NOT NULL`, cũng không có conversion.

**Q20. Có EF enum conversion nào đang tồn tại không?**

Không — cả 2 property đều khai báo kiểu `int` thô trong entity (enum chỉ dùng ở tầng Command để gán giá trị qua `(int)enumValue`, EF hoàn toàn không biết có enum). Đây là điểm thuận lợi: **đổi sang `string` không cần EF `HasConversion` phức tạp**, chỉ cần đổi property type từ `int` → `string` và bỏ hẳn 2 enum `ELoaiBuocXuLyThamDinhNhaThau`/`ELoaiToTrinhQuyetDinh` (thay bằng constant string).

**Giá trị string đề xuất (bám đúng tên hiện có trong enum, không tự đặt tên mới):**

```csharp
public static class ToTrinhThamDinhBuocXuLyLoai
{
    public const string DoiChieu = "DoiChieu";
    public const string ThuongThao = "ThuongThao";
    public const string ThamDinh = "ThamDinh";
}
```

Với `ToTrinhQuyetDinh.Loai` — hiện `ELoaiToTrinhQuyetDinh` có 3 giá trị dùng cho **2 nghiệp vụ khác nhau** (`HoSoMoiThauToTrinh`, `HoSoMoiThauQuyetDinh` — của `HoSoMoiThauDienTu`; `ToTrinhThamDinhNhaThau` — của tính năng này). Đề xuất:

```csharp
public static class ToTrinhQuyetDinhLoai
{
    public const string HoSoMoiThauToTrinh = "HoSoMoiThauToTrinh";
    public const string HoSoMoiThauQuyetDinh = "HoSoMoiThauQuyetDinh";
    public const string ToTrinhThamDinhNhaThau = "ToTrinhThamDinhNhaThau";
}
```

**Lưu ý quan trọng:** `ToTrinhQuyetDinh.Loai` đang dùng chung cho **cả `HoSoMoiThauDienTu`** (2 giá trị đầu) — đổi sang string sẽ **ảnh hưởng cả module `HoSoMoiThauDienTu`** (`HoSoMoiThauDienTuInsertCommand`/`UpdateCommand`/`DuyetCommand` đang query `x.Loai == (int)ELoaiToTrinhQuyetDinh.HoSoMoiThauToTrinh`). Task yêu cầu "Không refactor lan sang module không liên quan" — nhưng đây là **buộc phải sửa** vì cùng 1 cột `Loai` dùng chung, không thể chỉ đổi type cho riêng 1 phần dữ liệu trong cùng cột. Sẽ báo rõ phạm vi ảnh hưởng này khi implement.

---

## F. Migration

**Q21. Với implementation hiện tại có thật sự cần migration không?**

**Có, cần 1 migration mới**, gồm:
1. `ToTrinhThamDinhBuocXuLy.Loai`: `int` → `nvarchar` (hoặc `varchar`, độ dài đủ chứa `"ThuongThao"` — đề xuất `nvarchar(50)`).
2. `ToTrinhQuyetDinh.Loai`: `int` → `nvarchar(50)`.
3. Xóa 2 dòng seed `DmTrangThaiPheDuyet.Id=71,72` (`DeleteData`).
4. Nếu quyết định không cần `VanBanQuyetDinh.TrangThaiDuyetId` trỏ riêng nhóm `ToTrinhThamDinhNhaThau` nữa (theo Q3) — không đổi schema cột này (`TrangThaiDuyetId` vẫn là FK `int?` tới `DmTrangThaiPheDuyet.Id`, chỉ đổi **giá trị** Id được set trong code từ `71/72` → `30/31/32/33` theo đúng trạng thái Tờ trình, không đổi cấu trúc bảng).

**Q22. Nếu cần, dữ liệu numeric cũ sẽ được map sang string như thế nào?**

Vì migration #179 (`20260812075056_Issue179_ToTrinhThamDinhNhaThau`) **vừa mới apply** và **chưa có PR/release nào dùng dữ liệu thật** (chỉ có 1 dòng test `TC-01` do anh tạo thủ công), rủi ro mất dữ liệu gần như không có. Nhưng để an toàn và đúng nguyên tắc "không mất dữ liệu cũ", migration mới vẫn nên có bước `UPDATE` backfill tường minh (không dựa vào giả định "chưa có ai dùng"):

```sql
-- ToTrinhThamDinhBuocXuLy.Loai (int → string), map theo đúng ELoaiBuocXuLyThamDinhNhaThau cũ
UPDATE ToTrinhThamDinhBuocXuLy SET Loai = 'DoiChieu'   WHERE Loai = '1';
UPDATE ToTrinhThamDinhBuocXuLy SET Loai = 'ThuongThao' WHERE Loai = '2';
UPDATE ToTrinhThamDinhBuocXuLy SET Loai = 'ThamDinh'   WHERE Loai = '3';

-- ToTrinhQuyetDinh.Loai (int → string), map theo đúng ELoaiToTrinhQuyetDinh cũ
UPDATE ToTrinhQuyetDinh SET Loai = 'HoSoMoiThauToTrinh'      WHERE Loai = '1';
UPDATE ToTrinhQuyetDinh SET Loai = 'HoSoMoiThauQuyetDinh'    WHERE Loai = '2';
UPDATE ToTrinhQuyetDinh SET Loai = 'ToTrinhThamDinhNhaThau'  WHERE Loai = '3';
```

Thứ tự thao tác trong migration (giống pattern đã dùng ở migration #179 — backfill trước khi đổi type triệt tiêu dữ liệu): **Add cột string tạm → backfill theo giá trị int cũ → Drop cột int cũ → Rename cột string tạm thành `Loai`** (SQL Server không cho đổi trực tiếp `int` → `nvarchar` cùng tên nếu muốn giữ được giá trị theo mapping tùy biến, phải qua cột tạm).

---

## G. File impact — danh sách dự kiến sửa (implement ở bước sau, sau khi anh xác nhận báo cáo này)

### Domain
- `QLDA.Domain/Entities/ToTrinhThamDinhNhaThau.cs` — đổi `BuocXuLys` (List) → không còn expose trực tiếp kiểu List (vẫn giữ navigation EF `List<ToTrinhThamDinhBuocXuLy>` cho quan hệ 1-N vật lý trong DB — **DB vẫn phải là 1-N** vì đó là 3 dòng riêng trong 1 bảng; việc chuyển sang 3 property riêng là ở tầng **DTO/Model contract**, không phải ở Domain Entity. Domain Entity giữ nguyên `List<ToTrinhThamDinhBuocXuLy>`).
- `QLDA.Domain/Entities/ToTrinhThamDinhBuocXuLy.cs` — đổi `Loai` từ `int` → `string`.
- `QLDA.Domain/Entities/ToTrinhQuyetDinh.cs` — đổi `Loai` từ `int` → `string`.
- `QLDA.Domain/Constants/TrangThaiPheDuyetCodes.cs` — xóa class `ToTrinhThamDinhNhaThauQuyetDinh`.
- Xóa `QLDA.Domain/Enums/ELoaiBuocXuLyThamDinhNhaThau.cs` (thay bằng constant string).
- Xóa `QLDA.Domain/Enums/ELoaiToTrinhQuyetDinh.cs` (thay bằng constant string).
- Tạo `QLDA.Domain/Constants/ToTrinhThamDinhBuocXuLyLoai.cs` (constant mới).
- Tạo `QLDA.Domain/Constants/ToTrinhQuyetDinhLoai.cs` (constant mới).

### Application
- `ToTrinhThamDinhNhaThauThemMoiCommand.cs` — sửa gán `Loai` (enum → constant string), sửa set `VanBanQuyetDinh.TrangThaiDuyetId` (bỏ `ToTrinhThamDinhNhaThauQuyetDinh.ChoDuyet`, dùng `DeXuatMacDinh.DuThao` — đồng bộ với `TrangThaiId` của Tờ trình vừa tạo).
- `ToTrinhThamDinhNhaThauUpdateCommand.cs` — bổ sung sync 3 field `ThuongThao/DoiChieu/ThamDinh` (theo pattern sync child entity hiện có trong project — cần khảo sát thêm pattern `SyncNhaThauIds`/tương tự trước khi implement).
- `ToTrinhThamDinhNhaThauDanhSachQuery.cs` — danh sách không trả 3 bước; **2026-08-14** trả `NhaThauId` (không `TenNhaThau`).
- `ToTrinhThamDinhNhaThauGetQuery.cs` — thêm `.Include(e => e.BuocXuLys)`.
- `ToTrinhThamDinhNhaThauMappings.cs` / `ToTrinhThamDinhNhaThauDto.cs` — thêm 3 property `ThuongThao/DoiChieu/ThamDinh` (kiểu DTO tương ứng), map từ `entity.BuocXuLys` sang 3 property qua `FirstOrDefault(x => x.Loai == ...)`.
- Xóa `ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand.cs`.
- `ToTrinhThamDinhNhaThauDuyetCommand.cs` (đã có sẵn, dùng chung qua `QuanLyPheDuyet`) — bổ sung: sau khi set `entity.TrangThaiId = trangThaiDaDuyet`, tìm `VanBanQuyetDinh` liên kết và set `TrangThaiDuyetId` tương ứng = `ĐD` (cần xác định cách liên kết `VanBanQuyetDinh` ↔ `ToTrinhThamDinhNhaThau` — hiện tại **chưa có link tường minh nào**, xem mục "Vấn đề cần quyết định" bên dưới).
- Tương tự cân nhắc `ToTrinhThamDinhNhaThauTrinhCommand.cs`/`TraLaiCommand.cs` để đồng bộ `VanBanQuyetDinh.TrangThaiDuyetId` theo từng bước chuyển trạng thái của Tờ trình (Dự thảo→Đã trình→Trả lại), nếu muốn 2 trạng thái luôn đồng bộ hoàn toàn.

### Persistence
- `ToTrinhThamDinhBuocXuLyConfiguration.cs` — đổi `Loai` sang `HasMaxLength` string.
- `ToTrinhQuyetDinhConfiguration.cs` — đổi `Loai` sang `HasMaxLength` string.
- `DanhMucTrangThaiPheDuyetConfiguration.cs` — xóa seed `Id=71,72`.
- `HoSoMoiThauDienTuInsertCommand.cs`/`UpdateCommand.cs`/`DuyetCommand.cs` — đổi so sánh `(int)ELoaiToTrinhQuyetDinh.X` → constant string mới (bắt buộc vì dùng chung cột `Loai`).

### WebApi
- `ToTrinhThamDinhNhaThauController.cs` — xóa method `DuyetQuyetDinh` + route `quyet-dinh/{id}/duyet`.
- `ToTrinhThamDinhNhaThauModel.cs`/`MappingConfiguration.cs` (dùng cho Update) — thêm 3 property `ThuongThao/DoiChieu/ThamDinh` nếu Update API dùng Model riêng (cần xác nhận Update hiện dùng `ToTrinhThamDinhNhaThauModel` hay domain entity trực tiếp — đã xác nhận: `Update` controller nhận `ToTrinhThamDinhNhaThauModel`).

### Migration
- Migration mới (tên đề xuất: `Issue179_2_LoaiToString`) — theo mô tả mục F.

---

## Vấn đề cần anh xác nhận trước khi implement

1. **Liên kết `VanBanQuyetDinh` ↔ `ToTrinhThamDinhNhaThau` để đồng bộ trạng thái khi duyệt qua `QuanLyPheDuyet`.**

   Hiện tại `ToTrinhThamDinhNhaThauThemMoiCommand` tạo `VanBanQuyetDinh` với `Id` tự sinh (Guid mới, không liên quan `ToTrinhThamDinhNhaThau.Id`). Muốn `ToTrinhThamDinhNhaThauDuyetCommand` (được gọi qua `QuanLyPheDuyet`) tìm đúng `VanBanQuyetDinh` tương ứng để đồng bộ trạng thái, cần 1 trong 2 cách:

   - **(A)** Set `VanBanQuyetDinh.Id = ToTrinhThamDinhNhaThau.Id` khi tạo (giống đúng pattern `HoSoMoiThauDienTuDuyetCommand` đang làm: `new VanBanQuyetDinh { Id = entity.Id, ... }`). Khi đó `Duyệt`/`Trình`/`TràLại` chỉ cần `_vanBanQuyetDinhRepo.GetQueryableSet().FirstOrDefaultAsync(x => x.Id == request.Id)`.
   - **(B)** Giữ nguyên `VanBanQuyetDinh.Id` tự sinh riêng, thêm 1 cách tra cứu khác (ví dụ so theo `DuAnId+BuocId+Loai`, nhưng không unique nếu có nhiều Tờ trình cùng dự án/bước).

   → Đề xuất chọn **(A)** vì đúng pattern có sẵn của hệ thống (`HoSoMoiThauDienTu`), đơn giản, không cần thêm cột. Xin xác nhận trước khi implement.

2. **Danh sách (`danh-sach-tien-do`) có cần trả `ThuongThao/DoiChieu/ThamDinh` không?** Đã chốt **không**. List trả `NhaThauId` (2026-08-14), không trả `TenNhaThau`.

3. **`ToTrinhQuyetDinhLoai`/`ToTrinhThamDinhBuocXuLyLoai` đặt ở đâu** — đề xuất `QLDA.Domain/Constants/` (cùng nơi với `TrangThaiPheDuyetCodes`, `PheDuyetEntityNames`) — xin xác nhận tên file/namespace nếu muốn khác.

Sau khi anh xác nhận 3 điểm trên, em sẽ implement theo đúng file impact đã liệt kê ở mục G.
