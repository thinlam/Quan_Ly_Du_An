# Issue #178 — Bổ sung field form Quyết định duyệt Kế hoạch lựa chọn nhà thầu

**Issue:** #178 (PMIS/Redmine liên quan branch `178-9665---feature/add-ke-hoach-lcnt-fields`)  
**Liên quan:** [#170](../170/index.md) — đã bổ sung phần lớn field trên cùng module  
**Branch:** `178-9665---feature/add-ke-hoach-lcnt-fields`  
**Trạng thái:** Code `SoLuongGoiThau` (Approach B) done — migration để user tự tạo  
**Phạm vi code:** Chỉ `KeHoachLuaChonNhaThau` (+ reuse CBB nguồn vốn sẵn có). Không đụng `QuyetDinhDuyetKHLCNT` entity riêng, không đụng RutGon.

---

## 0. Kết luận nhanh (5 điểm bắt buộc trước khi code)

### 1. Module / entity / API hiện tại

| Hạng mục | Giá trị |
|----------|---------|
| Màn hình nghiệp vụ | Form **Quyết định / Kế hoạch lựa chọn nhà thầu** (Controller XML: *"Quyết định kế hoạch lựa chọn nhà thầu"*) |
| Module xử lý form | **`KeHoachLuaChonNhaThau`** — **không** phải entity `QuyetDinhDuyetKHLCNT` |
| API | `api/ke-hoach-lua-chon-nha-thau` |
| Controller | `QLDA.WebApi/Controllers/KeHoachLuaChonNhaThauController.cs` |
| Entity | `QLDA.Domain/Entities/KeHoachLuaChonNhaThau.cs` (TPT từ `VanBanQuyetDinh`) |
| CQRS | `QLDA.Application/KeHoachLuaChonNhaThaus/` |

Endpoints form:

| Flow | Method | Path |
|------|--------|------|
| Thêm mới | POST | `/them-moi` |
| Cập nhật | PUT | `/cap-nhat` |
| Chi tiết | GET | `/{id}/chi-tiet` |
| Danh sách | GET | `/danh-sach-tien-do` |

**Lưu ý phân biệt module:**

- `QuyetDinhDuyetKHLCNT` (`api/quyet-dinh-duyet-khlcnt`) chỉ là bản ghi liên kết `KeHoachLuaChonNhaThauId` + `VanBanQuyetDinh` — **không** chứa Tổng dự toán / Nguồn vốn / Thời gian thực hiện.
- Các field form nằm trên **`KeHoachLuaChonNhaThau`** (DTO dùng `IQuyetDinh`: `SoQuyetDinh`, `NgayQuyetDinh`, …).

### 2. Field đã tồn tại (code + migration đã có trên branch)

| Nghiệp vụ | Property | Kiểu | Layer đã có |
|-----------|----------|------|-------------|
| Tổng dự toán | `TongDuToan` | `long` (DTO Insert/Update: `long?` + validate) | Entity, EF FK/config, DTO×3, Mapping, Insert/Update validate, List projection, Migration |
| Dự toán thẩm định | `DuToanThamDinh` | `long?` | Như trên (đổi tên từ `TongDuToanThamDinhGia` qua migration rename) |
| Nguồn vốn | `NguonVonId` | `int?` + nav `NguonVon` | Như trên + validate thuộc `DuAnNguonVon` |
| Thời gian thực hiện (năm) | `ThoiGianThucHien` | `int?` | Như trên |
| Số tờ trình (label FE) | `So` / DTO `SoQuyetDinh` | `string?` | Reuse base `VanBanQuyetDinh` — không đổi schema |

Migrations liên quan (#170):

1. `20260810095922_AddKeHoachLuaChonNhaThauBoSungThongTin`
2. `20260811022108_RenameTongDuToanThamDinhGiaToDuToanThamDinh`
3. `20260811023400_FixRenameTongDuToanThamDinhGiaToDuToanThamDinh`

Datatype tiền: `bigint` / `long` — cùng pattern `DuToanDauTu.TongDuToan`.

### 3. Field phải thêm mới (gap còn lại so với yêu cầu task này)

| Nghiệp vụ | Hiện trạng | Hành động đề xuất |
|-----------|------------|-------------------|
| **Số lượng gói thầu** | Persist `int? SoLuongGoiThau` (Approach B) trên Entity/DTO/Mapping/List | Code done; **user tự migration** cột `SoLuongGoiThau int NULL` |

Không thiếu (đã làm #170): Tổng dự toán, Dự toán thẩm định, Nguồn vốn, Thời gian thực hiện.

### 4. Nguồn vốn của Dự án đang lưu / lấy ở đâu

| Thành phần | Chi tiết |
|------------|----------|
| Junction | `DuAnNguonVon` (`LeftId` = `DuAnId`, `RightId` = `NguonVonId`) |
| Danh mục | `DanhMucNguonVon` → bảng `DmNguonVon` |
| DuAn DTO | `DanhSachNguonVon: List<int>?` |
| **CBB API đã có — reuse** | `GET api/danh-muc-nguon-von/danh-sach?duAnId={guid}` |
| Query | `DanhMucNguonVonGetDanhSachQuery` — khi có `duAnId` filter `DuAnNguonVons.Any(i => i.LeftId == …)` |
| Validate khi lưu KHLCNT | Insert/Update: nếu `NguonVonId > 0` thì phải thuộc `DuAn.DuAnNguonVons` |

**Không** tạo bảng/danh mục/API nguồn vốn mới. Flow CBB:

`Chọn Dự án` → `DuAnId` → `GET …/danh-muc-nguon-von/danh-sach?duAnId=` → chọn → lưu `NguonVonId` trên KHLCNT.

### 5. File dự kiến cần sửa (chỉ nếu làm `SoLuongGoiThau`)

Tùy approach (xem §3). **Không sửa** Controller signature nếu chỉ mở rộng DTO. **Không sửa** migration cũ / snapshot thủ công.

---

## 1. Survey source chi tiết

### 1.1. Entity hiện tại (`KeHoachLuaChonNhaThau`)

```csharp
public long TongDuToan { get; set; }
public long? DuToanThamDinh { get; set; }
public int? NguonVonId { get; set; }
public int? ThoiGianThucHien { get; set; }
public DanhMucNguonVon? NguonVon { get; set; }
public ICollection<GoiThau>? GoiThaus { get; set; }
```

### 1.2. DTO Insert / Update / Detail / List

Đã có đủ 4 field trên `KeHoachLuaChonNhaThauDto`, `InsertDto`, `UpdateDto`; list projection trong `KeHoachLuaChonNhaThauGetDanhSachQuery` đã map.

### 1.3. Validate hiện có

- `TongDuToan` bắt buộc (*"Tổng dự toán là bắt buộc"*)
- `NguonVonId` thuộc dự án (*"Nguồn vốn không thuộc dự án"*)
- Trùng số tờ trình (*"Số tờ trình đã tồn tại"*)

### 1.4. `QuyetDinhDuyetKHLCNT` — ngoài scope field

Entity chỉ có `KeHoachLuaChonNhaThauId` + navigation `VanBanQuyetDinh`. Form field mới **không** đưa vào module này.

---

## 2. Mapping yêu cầu task ↔ hiện trạng

| # | Yêu cầu | Hiện trạng | Cần làm thêm? |
|---|---------|------------|---------------|
| 1 | Tổng dự toán (tiền) | Đã có `TongDuToan` `long`/`bigint` | Không (verify API sau apply migration) |
| 2 | Dự toán thẩm định | Đã có `DuToanThamDinh` full flow | Không (verify) |
| 3 | Nguồn vốn CBB theo dự án | Đã có `NguonVonId` + CBB sẵn có | Không (reuse API) |
| 4 | Thời gian thực hiện (năm) | Đã có `ThoiGianThucHien` `int?` | Không (verify) |
| 5 | Số lượng gói thầu | Approach B `SoLuongGoiThau` | Code done; chờ migration tay |

---

## 3. Approaches cho **Số lượng gói thầu** (cần chốt)

### Approach A — Computed (khuyến nghị nếu UI chỉ hiển thị)

- Không thêm cột DB.
- Detail/List: `SoLuongGoiThau = e.GoiThaus!.Count(g => !g.IsDeleted)` (hoặc Count theo convention soft-delete của `GoiThau`).
- Insert/Update: **không** nhận field (hoặc ignore nếu FE gửi).
- Pros: không trùng dữ liệu với danh sách gói thầu; đúng rule *"có thể tính thì không tạo field trùng"*.
- Cons: lúc mới tạo KHLCNT chưa có gói thầu → luôn = 0; không cho BA nhập tay số dự kiến.

### Approach B — Persist field nhập tay

- Thêm `int? SoLuongGoiThau` trên Entity + DTO Insert/Update/Detail/List + Mapping + Migration mới.
- Pros: form lập kế hoạch nhập trước khi có danh sách gói thầu.
- Cons: có thể lệch với `GoiThaus.Count` nếu không sync.

### Approach C — Persist + sync khi CRUD gói thầu

- Như B + cập nhật count khi thêm/xóa gói thầu.
- Pros: đủ cả nhập tay lẫn đồng bộ.
- Cons: scope lớn, đụng module `GoiThau` — **không khuyến nghị** cho task này.

**Đã chốt:** **Approach B** (persist nhập tay). Migration **không** tạo bởi agent — user tự `ef.bat QLDA add …`.

### Files dự kiến nếu chọn A

1. `…/DTOs/KeHoachLuaChonNhaThauDto.cs` — thêm `SoLuongGoiThau` (read-only trên response)
2. `…/KeHoachLuaChonNhaThauMappings.cs` — map count khi có collection
3. `…/Queries/KeHoachLuaChonNhaThauGetDanhSachQuery.cs` — projection count
4. `…/Queries/KeHoachLuaChonNhaThauQuery.cs` hoặc Controller Get — đảm bảo Include/count khi ToDto
5. Docs `report.md` / `journal.md`

Không migration.

### Files dự kiến nếu chọn B

1. `QLDA.Domain/Entities/KeHoachLuaChonNhaThau.cs`
2. `QLDA.Persistence/Configurations/KeHoachLuaChonNhaThauConfiguration.cs` (chỉ nếu cần config; thường không)
3. Migration mới qua `ef.bat QLDA add …` (Domain + Config + Migrator cùng commit group)
4. `DTOs/KeHoachLuaChonNhaThauDto.cs`, `InsertDto.cs`, `UpdateDto.cs`
5. `KeHoachLuaChonNhaThauMappings.cs`
6. `Commands/KeHoachLuaChonNhaThauInsertCommand.cs` / `UpdateCommand.cs` — validate optional nếu BA yêu cầu
7. `Queries/KeHoachLuaChonNhaThauGetDanhSachQuery.cs`
8. Docs `report.md` / `journal.md`

---

## 4. Việc không làm

- Không sửa `QuyetDinhDuyetKHLCNT` / RutGon / module khác
- Không tạo Model WebApi mới cho field này
- Không tạo danh mục nguồn vốn mới / hard-code list
- Không sửa migration cũ / `AppDbContextModelSnapshot` thủ công
- Không drop DB / xóa data
- Không refactor `IQuyetDinh` / rename `SoQuyetDinh`

---

## 5. Acceptance (sau khi chốt + implement phần còn lại)

1. Field 1–4 tiếp tục hoạt động đầy đủ trên thêm mới / cập nhật / chi tiết / danh sách.
2. CBB nguồn vốn chỉ theo `duAnId` (API sẵn có).
3. `SoLuongGoiThau` theo approach đã chốt, không trùng nghĩa với dữ liệu đã có nếu chọn A.
4. Nếu B: migration mới chỉ thêm cột trên `KeHoachLuaChonNhaThau`.

---

## 6. Liên kết docs #170

Chi tiết thiết kế 4 field đầu + bước code đã làm: [`../170/index.md`](../170/index.md), [`../170/report.md`](../170/report.md), [`../170/test-workflow.md`](../170/test-workflow.md).
