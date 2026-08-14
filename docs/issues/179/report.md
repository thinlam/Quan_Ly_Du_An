# Báo cáo khảo sát — Issue 179: API `to-trinh-tham-dinh-nha-thau/them-moi`

> Trạng thái: **KHẢO SÁT XONG — CHƯA CODE**. Tài liệu này trả lời đầy đủ 28 câu hỏi bắt buộc ở mục 36 của yêu cầu task, kèm phát hiện quan trọng cần chốt hướng trước khi implement.

---

## 0. Phát hiện quan trọng nhất — ĐỌC TRƯỚC

### 0.1. Build hiện tại đang LỖI (không liên quan issue này, nhưng chặn mọi thứ)

`QLDA.Domain/Entities/ToTrinhQuyetDinh.cs` đã bị sửa dở: 2 property `HoSoMoiThauToTrinhId` / `HoSoMoiThauQuyetDinhId` đã bị **comment out**, và `EntityId` + `Loai` đã được thêm vào entity — **nhưng EF Configuration chưa được cập nhật theo**. Kết quả: `dotnet build SER.sln` lỗi CS1061 tại:

- `QLDA.Persistence/Configurations/HoSoMoiThauDienTuConfiguration.cs` (dòng 42-50)
- `QLDA.Persistence/Configurations/ToTrinhQuyetDinhConfiguration.cs` (dòng 14-21, class thực chất tên là `ChiDinhThauConfiguration`)

→ Việc "remove `HoSoMoiThauToTrinhId`/`HoSoMoiThauQuyetDinhId`" mà task yêu cầu (mục 9) **thực chất đã được bắt đầu nhưng chưa hoàn tất** — phải hoàn tất nó thì mới build được, bất kể có làm feature mới hay không.

### 0.2. API `POST api/to-trinh-tham-dinh-nha-thau/them-moi` **ĐÃ TỒN TẠI** — nhưng khác hoàn toàn cấu trúc yêu cầu

Đây là phát hiện lớn nhất, ảnh hưởng phạm vi toàn bộ task:

- Entity `ToTrinhThamDinhNhaThau` (`QLDA.Domain/Entities/ToTrinhThamDinhNhaThau.cs`) đã tồn tại — là entity kiểu **workflow trình/duyệt theo Dự án** (`ITienDo`, có `DuAnId`, `BuocId`, `TrangThaiId` → `DanhMucTrangThaiPheDuyet`, `TrangThaiDangTaiId`, `DaThamDinh`), có **danh sách nhiều nhà thầu** con (`List<KetQuaThamDinhNhaThau>`), mỗi nhà thầu gắn với 1 `GoiThauId` + `KetQuaDanhGia` (text).
- API `them-moi` hiện tại (`ToTrinhThamDinhNhaThauController.Create`) nhận `ToTrinhThamDinhNhaThauModel`, gọi `DuAnUpdateStepCommand`/`DuAnUpdatePhaseCommand` (cập nhật bước dự án), tạo entity, rồi lưu 3 nhóm file (`ToTrinhThamDinhNhaThau`, `NoiDungToTrinhThamDinhNhaThau`, `KetQuaThamDinhNhaThau` theo từng nhà thầu con).
- API này **không có**: `GoiThauId` cấp cha đơn lẻ, `ThongTinNhaThau` (1 nhà thầu + file E-HSDT/đánh giá), `ThongTinDoiChieu`/`ThongTinThuongThao`/`ThongTinThamDinh`, `ToTrinhKetQua` (dùng `ToTrinhQuyetDinh`), `QuyetDinhPheDuyet` (dùng `VanBanQuyetDinh`).
- Đây rõ ràng là **một tính năng "Tờ trình thẩm định nhà thầu" khác, phiên bản cũ hơn**, được thiết kế trước khi có UI mới (`e-hsdt1.lovable.app`). Task hiện tại mô tả một **business flow mới, đơn giản hơn** (1 gói thầu / 1 nhà thầu / đối chiếu / thương thảo / thẩm định / tờ trình / quyết định).

**→ Cần quyết định hướng trước khi code** (xem mục "Xung đột cần xác nhận" cuối file). Bản thân việc "implement đúng yêu cầu" và "reuse tối đa, thay đổi tối thiểu" đang mâu thuẫn nhau vì API endpoint đã có chủ nhưng hành vi khác hẳn.

---

## 1. `ToTrinhThamDinhNhaThau`

**Q1. Entity chính đã tồn tại chưa?**
Đã tồn tại: `QLDA.Domain/Entities/ToTrinhThamDinhNhaThau.cs`.

```csharp
[DisplayName("Tờ trình thẩm định nhà thầu")]
public class ToTrinhThamDinhNhaThau : Entity<Guid>, IAggregateRoot, ITienDo
{
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string So { get; set; } = string.Empty;
    public DateTimeOffset NgayTrinh { get; set; }
    public string? TrichYeu { get; set; }
    public int? TrangThaiId { get; set; }          // FK DanhMucTrangThaiPheDuyet
    public int? TrangThaiDangTaiId { get; set; }
    public bool? DaThamDinh { get; set; }
    public List<KetQuaThamDinhNhaThau>? NhaThaus { get; set; } = [];
}
```

**Q2. Table nào lưu thông tin chính?**
Bảng `ToTrinhThamDinhNhaThau` (đúng tên entity, xem `ToTrinhThamDinhNhaThauConfiguration.cs`). Entity **chưa có `GoiThauId`** ở cấp cha — hiện tại `GoiThauId` chỉ nằm trong bảng con `KetQuaThamDinhNhaThau` (1-nhiều nhà thầu, mỗi nhà thầu 1 gói thầu). Theo spec mới, `GoiThauId` là 1-1 ở cấp Tờ trình (1 tờ trình ứng với đúng 1 gói thầu, 1 nhà thầu).

→ **Cần thêm `GoiThauId` (Guid) vào `ToTrinhThamDinhNhaThau`** (migration mới), giữ nguyên các field workflow hiện có (`DuAnId`, `BuocId`, `So`, `NgayTrinh`, `TrichYeu`, `TrangThaiId`, `TrangThaiDangTaiId`).

---

## 2. `ThongTinNhaThau`

**Q3. Reuse entity/table nào?**
Không có entity nào lưu đúng "1 nhà thầu + ngày kết thúc đánh giá" theo cấp Tờ trình hiện tại — bảng gần nhất là `KetQuaThamDinhNhaThau` (`Id`, `ToTrinhId`, `NhaThauId`, `GoiThauId`, `KetQuaDanhGia`) nhưng nó được thiết kế cho **N nhà thầu / tờ trình**, không có `NgayKetThucDanhGia`.

Theo spec mới (1 tờ trình = 1 nhà thầu), đề xuất **field hóa trực tiếp** trên `ToTrinhThamDinhNhaThau` thay vì bảng con riêng:

```csharp
public string? TenNhaThau { get; set; }
public DateTimeOffset? NgayKetThucDanhGia { get; set; }
```

(Không cần bảng con `KetQuaThamDinhNhaThau` cho spec mới — bảng này vẫn giữ nguyên cho code cũ nếu quyết định giữ cả 2 flow, xem mục xung đột).

File `FileEHSDT` / `FileDanhGia` → `TepDinhKem` (`Attachment`), `GroupId = ToTrinhThamDinhNhaThau.Id`, `GroupType` cần 2 giá trị `EGroupType` mới (chưa có sẵn — xem mục File).

---

## 3-6. `ThongTinDoiChieu` / `ThongTinThuongThao` / `ThongTinThamDinh`

**Q4. Dùng chung model/table nào?**
**Chưa có bảng dùng chung nào phù hợp trong source hiện tại.** Đã rà soát các bảng có field `So/Ngay/NoiDung` gần giống (`KetQuaTrungThau` có field `DanhSachBienBanThuongThao` nhưng chỉ là danh sách file, không có So/Ngay/NoiDung riêng cho "thương thảo"; `ToTrinhQuyetDinh` có So/Ngay/TrichYeu/NguoiKy/ChucVu — không có `NoiDung`, không thiết kế cho object nhỏ dạng biên bản).

→ **Không có convention sẵn để tái sử dụng 100%** cho 3 mục này. Đề xuất **tạo 1 bảng mới dùng chung** (đúng tinh thần "1 table, 1 model, phân biệt bằng `Loai`" mà task yêu cầu — đây là bảng mới nhưng **không phải bảng riêng cho từng mục**, tuân thủ đúng nguyên tắc không tạo 3 bảng):

```csharp
public class ToTrinhThamDinhBuocXuLy : IHasKey<long>
{
    public long Id { get; set; }
    public Guid ToTrinhId { get; set; }       // FK ToTrinhThamDinhNhaThau.Id
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? NoiDung { get; set; }       // nullable — ĐốiChiếu có thể trống theo yêu cầu UI
    public int Loai { get; set; }              // enum ELoaiBuocXuLyThamDinhNhaThau: DoiChieu / ThuongThao / ThamDinh
}
```

Vì Loai này chỉ scope trong tính năng "Tờ trình thẩm định nhà thầu" (không phải dùng chung toàn hệ thống như `ToTrinhQuyetDinh`), đặt tên bảng gắn với tính năng để tránh nhầm với `ToTrinhQuyetDinh` dùng chung toàn cục.

**Q5. Field nào phân biệt 3 loại?** `Loai` (int, enum mới).

**Q6. Enum/constant hiện có là gì?** Không có sẵn — phải tạo enum mới `ELoaiBuocXuLyThamDinhNhaThau { DoiChieu = 1, ThuongThao = 2, ThamDinh = 3 }` đặt tại `QLDA.Domain/Enums/`. Đây **không vi phạm** nguyên tắc "không tự tạo enum nếu đã có" vì đã khảo sát và xác nhận **chưa có convention nào** cho khái niệm nghiệp vụ này.

`ThongTinThamDinh.So` (ẩn UI) vẫn dùng chung field `So` ở trên, giữ giá trị nếu FE có gửi, không thêm field riêng.

---

## 7-13. `ToTrinhKetQua` → `ToTrinhQuyetDinh`

**Q7. Xác nhận dùng `ToTrinhQuyetDinh`?** Đúng — entity đã tồn tại đúng như mô tả task, hiện tại đang dùng cho tính năng "Hồ sơ mời thầu điện tử" (`HoSoMoiThauDienTu.ToTrinh` / `.QuyetDinh`).

**Q8. `EntityId` đã tồn tại trong entity + DB chưa?**
- Trong entity C#: **đã có** (dòng 15 của `ToTrinhQuyetDinh.cs`, đang ở dạng dở dang — xem mục 0.1).
- Trong DB / migration snapshot: **CHƯA CÓ**. Đã kiểm tra `AppDbContextModelSnapshot.cs` (dòng 8411-8460) — bảng `ToTrinhQuyetDinh` hiện tại trong DB chỉ có: `Id, ChucVu, CreatedAt, CreatedBy, HoSoMoiThauQuyetDinhId, HoSoMoiThauToTrinhId, Index, IsDeleted, Ngay, NgayKy, NguoiKy, So, TrichYeu, UpdatedAt, UpdatedBy`. Không có `EntityId`, không có `Loai`.

**Q9. `Loai` đã tồn tại trong entity + DB chưa?** Tương tự — có trong entity C# (dòng 23), **chưa có trong DB**.

**Q10. `HoSoMoiThauToTrinhId` còn tồn tại ở đâu?**
- `QLDA.Persistence/Configurations/HoSoMoiThauDienTuConfiguration.cs` dòng 42-45 (`HasForeignKey<ToTrinhQuyetDinh>(x => x.HoSoMoiThauToTrinhId)`) — **lỗi build**.
- `QLDA.Persistence/Configurations/ToTrinhQuyetDinhConfiguration.cs` dòng 14-16 — **lỗi build**.
- Trong DB (migration `20260715022910_Init.cs` + snapshot) — cột vật lý vẫn còn, cần migration để xóa.

**Q11. `HoSoMoiThauQuyetDinhId` còn tồn tại ở đâu?** Tương tự Q10, cùng 2 file, cùng migration gốc.

**Q12. Query/Command nào đang reference 2 field cũ (gián tiếp qua navigation `ToTrinh`/`QuyetDinh`)?**
Navigation `HoSoMoiThauDienTu.ToTrinh` / `.QuyetDinh` (kiểu `ToTrinhQuyetDinh?`) hiện được EF map 1-1 thông qua đúng 2 FK trên. Các nơi đang dùng navigation này (KHÔNG dùng trực tiếp tên field FK, nhưng phụ thuộc vào chúng để EF `Include()` hoạt động):
- `QLDA.Application/HoSoMoiThauDienTus/Commands/HoSoMoiThauDienTuUpdateCommand.cs` — `.Include(e => e.ToTrinh).Include(e => e.QuyetDinh)`, gán/tạo mới `entity.ToTrinh`/`entity.QuyetDinh`.
- `QLDA.Application/HoSoMoiThauDienTus/Commands/HoSoMoiThauDienTuInsertCommand.cs` — set `entity.ToTrinh`/`entity.QuyetDinh` khi tạo mới (cần đọc lại để xác nhận trước khi sửa).
- `QLDA.Application/HoSoMoiThauDienTus/Commands/HoSoMoiThauDienTuDuyetCommand.cs` — đọc `entity.QuyetDinh?.So/TrichYeu/NguoiKy/Ngay/NgayKy` để tạo `VanBanQuyetDinh` khi duyệt.
- `QLDA.Application/HoSoMoiThauDienTus/DTOs/HoSoMoiThauDienTuMappings.cs` — map `dto.ToTrinh`/`dto.QuyetDinh` ↔ `entity.ToTrinh`/`entity.QuyetDinh`.
- `QLDA.WebApi/Models/HoSoMoiThauDienTus/HoSoMoiThauDienTuModel.cs` — property `ToTrinh`/`QuyetDinh` kiểu `ToTrinhQuyetDinhModel`.
- `QLDA.Domain/Entities/HoSoMoiThauDienTu.cs` — 2 navigation property.

→ Sau khi bỏ FK riêng, EF **không thể tự map navigation 1-1 qua `EntityId+Loai`** (vì đó không phải shadow FK chuẩn 1-1 nữa, mà là kiểu "polymorphic" — nhiều nghiệp vụ share 1 bảng qua cặp `EntityId/Loai`, không thể convention-map bằng `HasForeignKey`). Cần đổi cách các Command/Query trên **load `ToTrinhQuyetDinh` bằng query tường minh** (`_repo.GetQueryableSet().Where(x => x.EntityId == entity.Id && x.Loai == (int)ELoaiToTrinhQuyetDinh.HoSoMoiThauToTrinh)`) thay vì `.Include()`. Đây là phần việc bắt buộc phải làm để hoàn tất phần "remove field cũ" mà task yêu cầu, đồng thời để build được — **không thể tránh chạm vào `HoSoMoiThauDienTu` module** dù nó không phải trọng tâm issue.

**Q13. Enum/constant nào đang dùng cho `Loai`?** Không có — comment trong entity gốc (`//hosoMoiThauQuyetDinh/HoSoMoiThauToTrinh/ToTrinhThamDinhNhaThau`) là gợi ý duy nhất. Đề xuất tạo enum mới:

```csharp
namespace QLDA.Domain.Enums;
public enum ELoaiToTrinhQuyetDinh
{
    HoSoMoiThauToTrinh = 1,
    HoSoMoiThauQuyetDinh = 2,
    ToTrinhThamDinhNhaThau = 3,
}
```

**Q14. Giá trị dùng cho `ToTrinhThamDinhNhaThau`?** `ELoaiToTrinhQuyetDinh.ToTrinhThamDinhNhaThau = 3` (đúng thứ tự nêu trong comment gốc của entity, giữ 2 giá trị cũ y nguyên để không phá dữ liệu hiện có sau migrate).

Mapping `ToTrinhKetQua → ToTrinhQuyetDinh` (mục 11 của task) — xác nhận đúng theo yêu cầu:

```text
So       → So
Ngay     → Ngay
NguoiKy  → NguoiKy
ChucVuId → ChucVu   (entity dùng int? ChucVu, không đổi tên)
TrichYeu → TrichYeu
EntityId = ToTrinhThamDinhNhaThau.Id
Loai     = (int)ELoaiToTrinhQuyetDinh.ToTrinhThamDinhNhaThau
```

---

## 14-16. `QuyetDinhPheDuyet` → `VanBanQuyetDinh`

**Q15. Entity/table hiện tại.**

```csharp
public class VanBanQuyetDinh : Entity<Guid>, IAggregateRoot, ITienDo, IVanBanQuyetDinh, INguoiKy, IEntityType {
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? CoQuanQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? Loai { get; set; }
}
```

Bảng `VanBanQuyetDinh`, mapping **TPT (Table-Per-Type)** — nhiều entity con kế thừa (`VanBanPhapLy`, `VanBanChuTruong`, `PheDuyetDuToan`, `QuyetDinhDuyetDuAn`, `QuyetDinhDuyetQuyetToan`, `QuyetDinhLapBanQLDA`, `QuyetDinhLapBenMoiThau`, `QuyetDinhLapHoiDongThamDinh`...). `Loai` là `string` (lưu tên `EnumLoaiVanBanQuyetDinh`, ví dụ `"HoSoMoiThauDienTu"`).

**Đã có sẵn tiền lệ y hệt yêu cầu ở mục 17**: `QuyetDinhLapBanQLDA : VanBanQuyetDinh` đã tự thêm `TrangThaiId` (int?) + navigation `TrangThai` → `DanhMucTrangThaiPheDuyet` (xem `QuyetDinhLapBanQLDA.cs`, cũng có ở `PheDuyetDuToan`). Tuy nhiên các trường hợp đó thêm `TrangThaiId` ở **bảng con** (TPT), còn task 179 yêu cầu thêm ở **bảng cha `VanBanQuyetDinh`** để mọi loại văn bản (kể cả những loại chưa có `TrangThaiId` riêng) đều có thể lọc theo `ĐD/NULL` dùng chung 1 field. → Cần thêm `VanBanQuyetDinh.TrangThaiId` (nullable, FK `DanhMucTrangThaiPheDuyet`), **giữ nguyên** các `TrangThaiId` riêng đã có ở `PheDuyetDuToan`/`QuyetDinhLapBanQLDA` (không đụng, không gây trùng tên vì đó là 2 bảng khác trong chiến lược TPT).

**Q16. Mapping `QuyetDinhPheDuyet`:**

```text
So       → So
Ngay     → Ngay
NguoiKy  → NguoiKy
NgayKy   → NgayKy
ChucVu   → (chưa có field ChucVu trên VanBanQuyetDinh — xem bên dưới)
TrichYeu → TrichYeu
DuAnId   = ToTrinhThamDinhNhaThau.DuAnId  (bắt buộc, non-nullable trên VanBanQuyetDinh)
BuocId   = ToTrinhThamDinhNhaThau.BuocId
Loai     = nameof(EnumLoaiVanBanQuyetDinh.ToTrinhThamDinhNhaThau)  (giá trị mới, xem bên dưới)
```

`VanBanQuyetDinh` **hiện không có field `ChucVu`/`ChucVuId`** ở bảng cha (chỉ có ở 1 số bảng con TPT như `VanBanPhapLy.ChucVuId`, `VanBanChuTruong.ChucVuId`, `PheDuyetDuToan.ChucVuId` — kiểu `int?`, FK `DanhMucChucVu`). Vì task 179 dùng thẳng bảng cha (không tạo entity con TPT mới), cần bổ sung `ChucVuId` (int?, FK `DanhMucChucVu`) trực tiếp trên `VanBanQuyetDinh` — đúng tinh thần "nếu đang dùng `ChucVuId` thay vì text `ChucVu` thì reuse", ở đây source dùng cả 2 kiểu tuỳ bảng, chọn `ChucVuId` (kiểu int FK) theo đúng pattern các bảng con mới nhất (`PheDuyetDuToan`, `VanBanPhapLy`, `VanBanChuTruong`) thay vì text tự do.

**`EnumLoaiVanBanQuyetDinh` cần thêm giá trị mới:**

```csharp
[Description("Tờ trình thẩm định nhà thầu")] ToTrinhThamDinhNhaThau,
```

(file `QLDA.Domain/Enums/EnumLoaiVanBanQuyetDinh.cs`). Đồng thời bổ sung dictionary tương ứng trong `LoaiVanBanQuyetDinhConst` nếu cần hiển thị `PartialView` (constant string, ví dụ `"TOTRINHTHAMDINHNHATHAU"` — cần xác nhận với FE tên partial view mong muốn, không tự đặt tuỳ tiện).

**Q17. `TrangThaiId` FK tới entity nào?** `DanhMucTrangThaiPheDuyet` (`QLDA.Domain/Entities/DanhMuc/DanhMucTrangThaiPheDuyet.cs`) — đúng bảng danh mục trạng thái phê duyệt dùng chung toàn hệ thống, có field `Loai` (string) để phân nhóm theo nghiệp vụ (ví dụ `Loai = "HoSoMoiThauDienTu"`, `Ma = "ĐD"`).

**Q18. Mã trạng thái "Chờ duyệt" hiện tại?**
Đã rà soát toàn bộ dữ liệu seed trong `AppDbContextModelSnapshot.cs` (nhóm `Loai = "PheDuyetDuToan" | "HoSoDeXuatCapDoCntt" | "HoSoMoiThauDienTu" | "PhanKhaiKinhPhi" | "QuyetDinhDieuChinh" | "DeXuatMacDinh" | "ThanhLyHopDong"`). Tất cả đều theo bộ mã chuẩn: `DT` (Dự thảo) → `ĐTr` (Đã trình) → `ĐD` (Đã duyệt) → `TL` (Trả lại) / `TC` (Từ chối). **Không có mã nào tên là "CHỜ DUYỆT" theo đúng nghĩa đen** — khái niệm gần nhất về mặt luồng là `ĐTr` (Đã trình, tức đang chờ người có thẩm quyền duyệt).

→ **Không có nhóm `Loai` nào tên `"ToTrinhThamDinhNhaThau"` trong danh mục `DanhMucTrangThaiPheDuyet` hiện tại** (nhóm `Loai="ToTrinhThamDinhNhaThau"` chưa tồn tại trong seed data — entity `ToTrinhThamDinhNhaThau` hiện tại dùng chung nhóm `"DeXuatMacDinh"` khi trình mới, xem `ToTrinhThamDinhNhaThauInsertCommand.cs` dòng 31-32: `s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt`).

**Đề xuất:** với riêng `VanBanQuyetDinh` của Tờ trình thẩm định nhà thầu, seed thêm 1 dòng danh mục mới trong `DanhMucTrangThaiPheDuyet` với `Loai = "ToTrinhThamDinhNhaThau"`, `Ma = "ĐTr"` (dùng đúng mã chuẩn "Đã trình" sẵn có trong hệ thống, đại diện cho khái niệm "chờ duyệt" — **không tự chế mã mới** như `"CD"`), và `Ma = "ĐD"` cho trạng thái đã duyệt. Đây là dữ liệu danh mục (migration `InsertData`), không phải thay đổi logic code.

**Q19. Xác nhận `ĐD = Đã duyệt`?** Đúng — xác nhận qua toàn bộ seed data hiện có, `"ĐD"` luôn là mã trạng thái cuối "Đã duyệt" cho mọi nhóm nghiệp vụ.

---

## Flow duyệt

**Q20. Handler/Command nào thực hiện duyệt?**
Với entity `ToTrinhThamDinhNhaThau` hiện tại **chưa có Command `Duyệt`** thực thụ theo nghĩa cập nhật `VanBanQuyetDinh` — chỉ có `ToTrinhThamDinhNhaThauTrinhCommand` (trình) và `ToTrinhThamDinhNhaThauTraLaiCommand` (trả lại), dùng `PheDuyetDispatch*` pattern chung (`QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchDuyetCommand.cs`, `PheDuyetDispatchHelper.cs`) — đây là hệ thống "Quản lý phê duyệt" tập trung, không phải command riêng theo từng entity như `HoSoMoiThauDienTuDuyetCommand`.

→ Cần **tạo mới 1 Command `ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand`** (hoặc tích hợp vào `PheDuyetDispatchDuyetCommand` nếu route qua `QuanLyPheDuyet` — cần khảo sát sâu hơn `PheDuyetDispatchHelper.cs` trước khi chốt, xem mục "Việc cần làm tiếp" bên dưới) để khi duyệt Quyết định phê duyệt của Tờ trình thẩm định nhà thầu thì set `VanBanQuyetDinh.TrangThaiId = (Id của trạng thái Ma="ĐD", Loai="ToTrinhThamDinhNhaThau")`.

**Q21. Chỗ nào update `VanBanQuyetDinh.TrangThaiId = ĐD`?** Sẽ nằm trong Command mới ở Q20 — pattern y hệt `HoSoMoiThauDienTuDuyetCommand.cs` (query `DanhMucTrangThaiPheDuyet` theo `Ma`+`Loai`, gán vào entity, `SaveChangesAsync`).

---

## API tổng hợp

**Q22. Query/Handler của `tong-hop-van-ban-quyet-dinh/danh-sach-day-du`?**
`QLDA.Application/TongHopVanBanQuyetDinhs/Queries/TongHopVanBanQuyetDinhGetListQuery.cs`, gọi từ `QLDA.WebApi/Controllers/TongHopVanBanQuyetDinhController.cs` (`[HttpGet("danh-sach-day-du")]`).

**Q23. Query hiện tại?**

```csharp
var query = _authManager.FilterVisible(VanBanQuyetDinh.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
        .WhereIf(request.Loai.HasValue, e => e.Loai == request.Loai.ToString())
        .WhereIf(request.DuAnId.HasValue, e => e.DuAnId == request.DuAnId)
        // ... các WhereIf khác (BuocId, CoQuanQuyetDinh, TrichYeu, TuNgay, DenNgay)
        .WhereGlobalFilter(...);
```

Không có filter theo `TrangThaiId` hiện tại — mọi `VanBanQuyetDinh` đều xuất hiện miễn khớp filter khác.

**Q24. File cần sửa để thêm `TrangThai.Ma == "ĐD" OR TrangThaiId == null`?**
Duy nhất `QLDA.Application/TongHopVanBanQuyetDinhs/Queries/TongHopVanBanQuyetDinhGetListQuery.cs` — thêm 1 `.Where()` (không dùng `WhereIf` vì đây là điều kiện luôn áp dụng, không phụ thuộc filter FE truyền vào):

```csharp
.Where(e => e.TrangThaiId == null || e.TrangThai!.Ma == "ĐD")
```

Cần thêm navigation `VanBanQuyetDinh.TrangThai` (→ `DanhMucTrangThaiPheDuyet`) để viết được `e.TrangThai.Ma` trong LINQ (hiện `VanBanQuyetDinh` chưa có navigation này — chỉ `PheDuyetDuToan`/`QuyetDinhLapBanQLDA` có `TrangThai` navigation ở bảng con).

---

## File — `GroupType` dự kiến

Đã rà soát toàn bộ `EGroupType` (`QLDA.Domain/Enums/EGroupType.cs`, 100 giá trị) và `TepDinhKemMappingConfigurations.cs`. Các giá trị liên quan trực tiếp tính năng "thẩm định nhà thầu" đã tồn tại (`ToTrinhThamDinhNhaThau`, `NoiDungToTrinhThamDinhNhaThau`, `KetQuaThamDinhNhaThau`, `NoiDungThamDinhNhaThau`) nhưng **không đủ số lượng nhóm cho spec mới** (thiếu FileEHSDT, FileDanhGia, File đối chiếu, File thương thảo, File Tờ trình kết quả, File Quyết định phê duyệt — 1 số cái trùng tên nhưng khác role).

Đề xuất **thêm các giá trị mới vào cuối enum `EGroupType`** (thêm enum member = không đổi schema DB, `GroupType` lưu dạng string tên enum trong bảng `Attachment` dùng chung):

| File nghiệp vụ | `EGroupType` đề xuất | Ghi chú |
|---|---|---|
| File E-HSDT | `ToTrinhThamDinhNhaThau_FileEHSDT` | mới |
| File đánh giá | `ToTrinhThamDinhNhaThau_FileDanhGia` | mới |
| File đối chiếu | `ToTrinhThamDinhNhaThau_DoiChieu` | mới |
| File thương thảo | `ToTrinhThamDinhNhaThau_ThuongThao` | mới |
| File thẩm định | `ToTrinhThamDinhNhaThau_ThamDinh` | mới (khác `NoiDungThamDinhNhaThau`/`NoiDungToTrinhThamDinhNhaThau` đang dùng cho flow cũ) |
| File Tờ trình kết quả | `ToTrinhQuyetDinh` | **đã có sẵn** trong enum (giá trị 93) — đúng semantic "file của ToTrinhQuyetDinh", tái sử dụng được vì `ToTrinhQuyetDinh` dùng `EntityId` của chính nó (`ToTrinhQuyetDinh.Id`) làm `GroupId` |
| File Quyết định phê duyệt | `ToTrinhThamDinhNhaThau_QuyetDinh` | mới — không trùng với `HoSoMoiThauDienTuQuyetDinh` (đang dùng cho `HoSoMoiThauDienTu`) |

Đặt tiền tố `ToTrinhThamDinhNhaThau_` cho các GroupType mới để không đụng 4 giá trị cũ đang dùng cho flow "thẩm định nhà thầu" phiên bản cũ (nếu giữ song song 2 flow — xem mục xung đột).

`GroupId` cho từng nhóm:
- FileEHSDT / FileDanhGia / DoiChieu / ThuongThao / ThamDinh / QuyetDinh → `GroupId = ToTrinhThamDinhNhaThau.Id.ToString()`.
- File Tờ trình kết quả (`EGroupType.ToTrinhQuyetDinh`) → `GroupId = ToTrinhQuyetDinh.Id.ToString()` (Id kiểu `long`, đúng pattern `ToTrinhQuyetDinhModel`/`ToTrinhQuyetDinhDto` đã dùng ở `HoSoMoiThauDienTu`).

---

## Migration

**Q26. Thay đổi schema thực tế:**

1. `ToTrinhQuyetDinh`: xóa cột `HoSoMoiThauToTrinhId`, `HoSoMoiThauQuyetDinhId`; thêm cột `EntityId` (uniqueidentifier, nullable), `Loai` (int, not null).
2. `VanBanQuyetDinh`: thêm cột `TrangThaiId` (int, nullable) + FK → `DanhMucTrangThaiPheDuyet` (`OnDelete Restrict`, `IsRequired(false)`), thêm cột `ChucVuId` (int, nullable) + FK → `DanhMucChucVu`.
3. `ToTrinhThamDinhNhaThau`: thêm `GoiThauId` (uniqueidentifier, not null) + FK → `GoiThau`; thêm `TenNhaThau` (nvarchar), `NgayKetThucDanhGia` (datetimeoffset, nullable).
4. Bảng mới `ToTrinhThamDinhBuocXuLy` (hoặc tên tương đương đã chốt) cho Đối chiếu/Thương thảo/Thẩm định: `Id (bigint identity), ToTrinhId (uniqueidentifier, FK), So (nvarchar), Ngay (datetimeoffset?), NoiDung (nvarchar(max)?), Loai (int)`.
5. `DanhMucTrangThaiPheDuyet`: seed thêm 2 dòng `Loai="ToTrinhThamDinhNhaThau"` (`Ma="ĐTr"`, `Ma="ĐD"`) — cần xác nhận thêm `Ma="DT"/"TL"/"TC"` nếu Command insert cũng cần trạng thái dự thảo/trả lại/từ chối cho Quyết định (task chỉ mô tả 2 trạng thái Chờ duyệt/Đã duyệt cho `VanBanQuyetDinh`, nên tối thiểu chỉ cần `ĐTr` + `ĐD`).
6. `EnumLoaiVanBanQuyetDinh`: thêm `ToTrinhThamDinhNhaThau` (chỉ là C# enum, không đổi schema, nhưng giá trị string được lưu trong cột `VanBanQuyetDinh.Loai` sẵn có).

**Q27. Xác nhận không add duplicate `EntityId`/`Loai`?** Đã xác nhận qua đọc trực tiếp snapshot (mục Q8/Q9) — 2 cột này **hoàn toàn chưa có trong DB**, migration mới sẽ add mới (không trùng), đồng thời **drop** 2 cột cũ.

**Q28. Danh sách file dự kiến sửa/tạo** (chi tiết ở mục "Danh sách file" cuối tài liệu).

---

## Rà soát khác theo yêu cầu khảo sát ban đầu

- **`GoiThau`**: có `GiaTri` (long?) và `HinhThucLuaChonNhaThauId` (int? → `DanhMucHinhThucLuaChonNhaThau`) — đúng 2 field "GiaTri/HinhThucLCNT" cần load-only, không lưu lại xuống Tờ trình.
- **`KetQuaTrungThau`**: có `DonViTrungThauId` (→ `DanhMucNhaThau`), `GiaTriTrungThau` (long), `SoNgayTrienKhai` (int?), `SoNgayThucHienHopDong` (int?), khóa theo `GoiThauId` — đúng 4 field load-only theo mục 27 của task.
- **`TepDinhKem`**: runtime là `BuildingBlocks.Domain.Entities.Attachment`; ghi qua `AttachmentBulkInsertOrUpdateCommand` (`GroupId` + `GroupTypes` bắt buộc); đọc qua `GetAttachmentsQuery` → `.ToAttachmentEntities()` → `.ToModel()`/`.ToDto()`. Đúng pattern đã ghi trong `CLAUDE.md` §14 và đã áp dụng y hệt ở `ToTrinhThamDinhNhaThauController` hiện tại.
- **Danh mục chức vụ**: `DanhMucChucVu : DanhMuc<int>` (`QLDA.Domain/Entities/DanhMuc/DanhMucChucVu.cs`), navigation collection hiện có tới `VanBanPhapLy`, `VanBanChuTruong`, `PheDuyetDuToan` — chưa có tới `VanBanQuyetDinh` (vì `ChucVuId` sẽ được thêm mới ở bảng cha).
- **Danh mục trạng thái**: `DanhMucTrangThaiPheDuyet` — bảng dùng chung toàn hệ thống, phân nhóm bằng field `Loai` (string) + `Ma` (string).
- **Flow phê duyệt hiện tại**: có 2 kiểu song song trong source — (a) Command riêng theo entity kiểu `HoSoMoiThauDienTuDuyetCommand` (tự query trạng thái, set trực tiếp, tạo `VanBanQuyetDinh`); (b) hệ thống tập trung `QuanLyPheDuyet` (`PheDuyetDispatch*Command`) dùng cho `ToTrinhThamDinhNhaThau` hiện tại (Trình/Trả lại). Cần khảo sát sâu `PheDuyetDispatchHelper.cs` để xác định Quyết định phê duyệt (mục 7) nên duyệt qua flow nào — **để trong mục "Việc cần làm tiếp" vì ngoài phạm vi khảo sát ban đầu**.

---

## Danh sách file dự kiến tạo/sửa (khi bắt đầu implement)

### Sửa (bắt buộc để hết lỗi build + đổi model dùng chung)
- `QLDA.Domain/Entities/ToTrinhQuyetDinh.cs` — dọn code, thêm XML doc thay comment tạm.
- `QLDA.Persistence/Configurations/ToTrinhQuyetDinhConfiguration.cs` — bỏ 2 `HasOne` cũ, thêm index `EntityId`+`Loai` nếu cần truy vấn nhanh.
- `QLDA.Persistence/Configurations/HoSoMoiThauDienTuConfiguration.cs` — bỏ 2 `HasOne` cũ.
- `QLDA.Domain/Entities/HoSoMoiThauDienTu.cs` — bỏ navigation `ToTrinh`/`QuyetDinh` (chuyển sang `[NotMapped]` hoặc bỏ hẳn, load thủ công ở Command/Query).
- `QLDA.Application/HoSoMoiThauDienTus/Commands/HoSoMoiThauDienTuUpdateCommand.cs`, `HoSoMoiThauDienTuInsertCommand.cs`, `HoSoMoiThauDienTuDuyetCommand.cs`, `DTOs/HoSoMoiThauDienTuMappings.cs` — đổi cách load/gán `ToTrinh`/`QuyetDinh` qua `EntityId+Loai` thay vì navigation.
- `QLDA.Domain/Entities/VanBanQuyetDinh.cs` — thêm `TrangThaiId`, `ChucVuId`, navigation `TrangThai`, `ChucVu`.
- `QLDA.Persistence/Configurations/VanBanQuyetDinhConfiguration.cs` — map FK mới.
- `QLDA.Application/TongHopVanBanQuyetDinhs/Queries/TongHopVanBanQuyetDinhGetListQuery.cs` — thêm filter `ĐD OR NULL`.
- `QLDA.Domain/Enums/EnumLoaiVanBanQuyetDinh.cs` — thêm `ToTrinhThamDinhNhaThau`.
- `QLDA.Domain/Enums/EGroupType.cs` — thêm các GroupType mới ở mục File.
- `QLDA.Domain/Entities/ToTrinhThamDinhNhaThau.cs` — thêm `GoiThauId`, `TenNhaThau`, `NgayKetThucDanhGia`.
- `QLDA.Persistence/Configurations/ToTrinhThamDinhNhaThauConfiguration.cs` — map FK `GoiThauId`.

### Tạo mới
- `QLDA.Domain/Enums/ELoaiToTrinhQuyetDinh.cs`.
- `QLDA.Domain/Enums/ELoaiBuocXuLyThamDinhNhaThau.cs` (DoiChieu/ThuongThao/ThamDinh).
- `QLDA.Domain/Entities/ToTrinhThamDinhBuocXuLy.cs` (tên chờ chốt) + Configuration tương ứng.
- Command `ToTrinhThamDinhNhaThauThemMoiCommand` (hoặc mở rộng `ToTrinhThamDinhNhaThauInsertCommand` hiện tại — tuỳ hướng chốt ở mục xung đột) + DTO/Model tương ứng cho toàn bộ payload mới (`ThongTinNhaThau`, `ThongTinDoiChieu`, `ThongTinThuongThao`, `ThongTinThamDinh`, `ToTrinhKetQua`, `QuyetDinhPheDuyet`).
- Command duyệt Quyết định phê duyệt (nếu không đi qua `QuanLyPheDuyet` sẵn có).
- Migration mới (`ef.bat QLDA add <TenMigration>`).

---

## Xung đột cần Product/Tech Lead xác nhận trước khi code

1. **API `them-moi` đã có chủ nhưng hành vi khác hẳn** (mục 0.2). 3 hướng khả thi:
   - **(A)** Viết đè hoàn toàn logic `Create` trong `ToTrinhThamDinhNhaThauController` theo spec mới, giữ nguyên route — chấp nhận thay đổi breaking behavior của API cũ (nếu FE/API cũ chưa ai dùng thật).
   - **(B)** Mở rộng entity `ToTrinhThamDinhNhaThau` + payload để **vừa hỗ trợ flow cũ (N nhà thầu) vừa hỗ trợ flow mới (1 gói thầu/1 nhà thầu + đối chiếu/thương thảo/thẩm định/tờ trình/quyết định)** trong cùng 1 entity — phức tạp hơn nhưng không phá dữ liệu/API cũ.
   - **(C)** Xác nhận flow cũ là **code chết/thử nghiệm chưa dùng thật** → cho phép xoá sạch, viết lại theo đúng spec 179.
   
   → Cần xác nhận trước khi động vào `ToTrinhThamDinhNhaThauController`/`ToTrinhThamDinhNhaThauInsertCommand`/`ToTrinhThamDinhNhaThauModel` vì đây là phần đổi lớn nhất.

2. **Trạng thái "Chờ duyệt"**: chưa có mã chuẩn trong `DanhMucTrangThaiPheDuyet` cho nhóm `Loai="ToTrinhThamDinhNhaThau"`. Đề xuất dùng `Ma="ĐTr"` (Đã trình) làm "chờ duyệt" theo đúng convention có sẵn — cần xác nhận tên hiển thị (`Ten`) mong muốn trên UI, tránh tự đặt.

3. **Flow duyệt Quyết định phê duyệt**: đi qua Command riêng (như `HoSoMoiThauDienTuDuyetCommand`) hay tích hợp vào hệ thống `QuanLyPheDuyet`/`PheDuyetDispatch*` đang dùng cho chính `ToTrinhThamDinhNhaThau` (Trình/Trả lại)? Cần khảo sát thêm `PheDuyetDispatchHelper.cs` trước khi chốt — chưa nằm trong khảo sát lần này.

4. **`ChucVuId` trên `VanBanQuyetDinh`**: thêm ở bảng cha (ảnh hưởng mọi loại văn bản qua TPT) hay chỉ set khi `Loai = ToTrinhThamDinhNhaThau` (các loại khác để `null`)? Đề xuất bảng cha nullable, không ảnh hưởng logic cũ (giữ đúng nguyên tắc "không ảnh hưởng nghiệp vụ cũ").

5. **`GroupType` file Tờ trình kết quả**: đề xuất tái sử dụng `EGroupType.ToTrinhQuyetDinh` sẵn có (giá trị 93). Cần xác nhận giá trị này hiện chưa được dùng cho nghiệp vụ khác trùng lặp (đã grep, hiện tại **chưa thấy chỗ nào dùng** `nameof(EGroupType.ToTrinhQuyetDinh)` trong code — an toàn để dùng).

---

## Bổ sung sau khi đọc `ToTrinhThamDinhNhaThauDuyetCommand.cs`

Đã đọc thêm `QLDA.Application/ToTrinhThamDinhNhaThau/Commands/ToTrinhThamDinhNhaThauDuyetCommand.cs` — **command này đã tồn tại và chỉ cập nhật `ToTrinhThamDinhNhaThau.TrangThaiId`** (trạng thái phê duyệt của bản thân Tờ trình, nhóm `Loai = PheDuyetEntityNames.DeXuatMacDinhStt` dùng chung, mã `DT/ĐTr/ĐD`), **không đụng đến `VanBanQuyetDinh`**. Đây là bằng chứng thêm cho thấy khái niệm "duyệt Tờ trình" (entity `ToTrinhThamDinhNhaThau`, đã có sẵn) và "duyệt Quyết định phê duyệt" (entity `VanBanQuyetDinh`, theo task 179 mục 7) là **2 hành động khác nhau, 2 trạng thái khác nhau** — khớp với mục 26 của task ("`TrangThaiDangTai` không nhầm với `VanBanQuyetDinh.TrangThaiId`" — ở đây là `ToTrinhThamDinhNhaThau.TrangThaiId` chứ không phải `TrangThaiDangTai`, nhưng nguyên tắc tách biệt là như nhau).

→ Đề xuất: **tạo Command mới riêng** (ví dụ `ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand`) chỉ để duyệt `VanBanQuyetDinh` liên kết với Tờ trình, độc lập với `ToTrinhThamDinhNhaThauDuyetCommand` hiện có (không sửa command cũ, không gộp 2 trạng thái làm một) — đúng nguyên tắc thay đổi tối thiểu + không ảnh hưởng flow cũ.

## Việc cần làm tiếp (trước khi viết code)

1. Xác nhận hướng xử lý xung đột #1 ở trên với người ra yêu cầu.
2. Đọc kỹ `PheDuyetDispatchHelper.cs` + `PheDuyetDispatchDuyetCommand.cs` để quyết định flow duyệt Quyết định phê duyệt.
3. Xác nhận tên bảng mới cho Đối chiếu/Thương thảo/Thẩm định (`ToTrinhThamDinhBuocXuLy` là tên đề xuất, có thể đổi theo convention team muốn).
4. Xác nhận tên hiển thị (`Ten`) + `PartialView` constant cho `EnumLoaiVanBanQuyetDinh.ToTrinhThamDinhNhaThau` trong `LoaiVanBanQuyetDinhConst`.
5. Sau khi chốt, mới bắt đầu: (a) fix build lỗi hiện tại, (b) domain + EF configuration, (c) migration, (d) Application layer (Command/Query/DTO), (e) WebApi layer (Model/Controller), (f) sửa API tổng hợp, (g) build + test thủ công theo `test-workflow.md`.
