# Báo cáo — chuyển `SoLuongGoiThau` sang `QuyetDinhDuyetKHLCNT`

Không lấy Services làm source of truth. Không đoán thêm business ngoài rule đã chốt.

---

## 1. Sai ở đâu / vì sao

Issue #178 (và GET chi tiết QĐ) gắn `SoLuongGoiThau` vào **kế hoạch**. Snapshot:

```csharp
modelBuilder.Entity("QLDA.Domain.Entities.KeHoachLuaChonNhaThau", b =>
{
    b.HasBaseType("QLDA.Domain.Entities.VanBanQuyetDinh");
    b.Property<int?>("SoLuongGoiThau").HasColumnType("int");
    b.ToTable("KeHoachLuaChonNhaThau", (string)null);
});
```

Rule đúng:

* KHLCNT chỉ **4 cột mới**: `TongDuToan`, `DuToanThamDinh`, `NguonVonId`, `ThoiGianThucHien`.
* `SoLuongGoiThau` thuộc bảng **`QuyetDinhDuyetKHLCNT`**.
* Type giữ `int?` như source hiện tại — không đổi `int` non-null, không đổi tên.

GET `chi-tiet` QĐ đang `SoLuongGoiThau = entity.KeHoachLuaChonNhaThau?.SoLuongGoiThau`. Write QĐ (`ToEntity` / `Update` / InsertOrUpdate) **không** persist số lượng — field chỉ “đi nhờ” navigation kế hoạch.

---

## 2. Trace tầng

```text
KeHoachLuaChonNhaThau.cs          SoLuongGoiThau int?
        ↓ convention (Config không khai property)
Snapshot / bảng KeHoachLuaChonNhaThau
        ↓
InsertDto / UpdateDto / Dto + Mappings + GetDanhSach
        ↓
api/ke-hoach-lua-chon-nha-thau  them-moi / cap-nhat / chi-tiet / danh-sach

QuyetDinhDuyetKHLCNT.cs           KHÔNG có property
        ↓
QuyetDinhDuyetKHLCNTConfiguration  chỉ FK + unique filter
        ↓
QuyetDinhDuyetKHLCNTModel          có soLuongGoiThau
        ↓
ToModel  ← đọc từ KeHoach (sai chỗ)
ToEntity/Update  ← không ghi SoLuongGoiThau
```

---

## 3. Cách sửa (khi được phép code)

Thứ tự: Entity → Application/WebApi map → **rồi** `ef.bat add`. Không sửa snapshot/migration cũ.

### 3.1 Domain

* Xóa `SoLuongGoiThau` khỏi `KeHoachLuaChonNhaThau`.
* Thêm `int? SoLuongGoiThau` trên `QuyetDinhDuyetKHLCNT`.
* Không đụng `Ten`, `LoaiKeHoach`, 4 cột mới, navigation.

### 3.2 EF Configuration

Hai file Config **không** map `SoLuongGoiThau` tường minh (convention). Sau khi chuyển property, snapshot/migration phải hiện cột trên `QuyetDinhDuyetKHLCNT`, **không** còn trên `KeHoachLuaChonNhaThau`. Không bắt buộc thêm `.Property` trừ khi review yêu cầu.

### 3.3 KHLCNT Application / API

Gỡ `SoLuongGoiThau` khỏi:

* `KeHoachLuaChonNhaThauDto` / `InsertDto` / `UpdateDto`
* `KeHoachLuaChonNhaThauMappings` (`ToEntity` / `ToDto` / `Update`)
* `KeHoachLuaChonNhaThauGetDanhSachQuery` projection

`KeHoachLuaChonNhaThauInsertCommand` / `UpdateCommand` chỉ gọi mapping — không sửa handler trừ khi còn reference.

API `api/ke-hoach-lua-chon-nha-thau` hết contract `soLuongGoiThau` (FE KHLCNT form phải bỏ field).

### 3.4 QĐ duyệt — đổi nguồn map

* `ToModel`: `SoLuongGoiThau = entity.SoLuongGoiThau` (không `KeHoachLuaChonNhaThau?.`).
* `ToEntity` / `Update`: gán `entity.SoLuongGoiThau = model.SoLuongGoiThau`.
* `QuyetDinhDuyetKHLCNTInsertOrUpdateCommandHandler` nhánh update: `dbEntity.SoLuongGoiThau = request.Entity.SoLuongGoiThau`.
* 4 field kia **vẫn** từ `KeHoachLuaChonNhaThau` (GET chi tiết vẫn `Include` kế hoạch).
* `QuyetDinhDuyetKHLCNTDto` / danh-sach-tien-do: hiện **không** có `SoLuongGoiThau` — không thêm trừ khi ticket yêu cầu list.

Không tạo Application `Service`.

---

## 4. Migration

### Không được

* `database drop` / `DropDatabase`
* Sửa tay `AppDbContextModelSnapshot.cs`
* Sửa tay / xóa migration **đã apply** `20260812044306_AddKeHoachLuaChonNhaThauSoLuongGoiThau`
* Sửa tay file `.cs` migration mới generate

### Được

1. Sửa Entity + Config + DTO/map **xong**.
2. `ef.bat QLDA add MoveSoLuongGoiThauToQuyetDinhDuyetKHLCNT`
3. **Review** `Up()`:

   * `AddColumn` `QuyetDinhDuyetKHLCNT.SoLuongGoiThau` `int` NULL
   * `DropColumn` `KeHoachLuaChonNhaThau.SoLuongGoiThau`
   * Snapshot: property chỉ còn trên entity QĐ

4. Nếu migration vừa add **vẫn** map sai (cột còn trên KHLCNT) **và chưa apply/share**: `ef.bat QLDA remove` rồi generate lại. Không patch snapshot.

### Copy dữ liệu cũ

EF thường `Drop` cột KHLCNT rồi `Add` cột QĐ → **mất số liệu**. Task cấm sửa migration tay. Nếu cần giữ data: hỏi leader (script DBA riêng / quy trình team), **không** tự `Sql()` vào file generate.

`Down()` rollback sẽ mất số trên QĐ nếu không có chiến lược copy — chấp nhận hoặc hỏi leader.

Không `update` staging/prod từ máy dev.

---

## 5. Trao đổi leader

> `SoLuongGoiThau` đang trên bảng KHLCNT (#178). Em chuyển sang `QuyetDinhDuyetKHLCNT`, KHLCNT giữ 4 cột `TongDuToan` / `DuToanThamDinh` / `NguonVonId` / `ThoiGianThucHien`. Chi tiết QĐ đọc field từ entity QĐ; form KHLCNT bỏ `soLuongGoiThau`. Migration mới drop + add. Cột cũ #178 không xóa history. Data cũ trên KHLCNT có cần copy sang QĐ không?

---

## 6. File dự kiến

| File | Việc |
| --- | --- |
| `QLDA.Domain/Entities/KeHoachLuaChonNhaThau.cs` | Xóa property |
| `QLDA.Domain/Entities/QuyetDinhDuyetKHLCNT.cs` | Thêm `int? SoLuongGoiThau` |
| `QLDA.Application/KeHoachLuaChonNhaThaus/DTOs/*.cs` | Gỡ property 3 DTO |
| `QLDA.Application/KeHoachLuaChonNhaThaus/KeHoachLuaChonNhaThauMappings.cs` | Gỡ map |
| `QLDA.Application/.../KeHoachLuaChonNhaThauGetDanhSachQuery.cs` | Gỡ Select |
| `QLDA.WebApi/.../QuyetDinhDuyetKHLCNTMappingConfiguration.cs` | ToModel/ToEntity/Update |
| `QLDA.Application/.../QuyetDinhDuyetKHLCNTInsertOrUpdateCommand.cs` | Assign khi update |
| `QLDA.Persistence/Configurations/*Configuration.cs` | Review; có thể không đổi dòng |
| Migrator — file **mới** do `ef add` | Review, không tay |

**Không sửa:** `20260812044306_*`, Init, snapshot tay, Validator (không có), `QuyetDinhDuyetKHLCNTGetDanhSachQuery` trừ khi ticket mở list.

Commit group khi được phép: Domain + Persistence/snapshot-via-ef + Migrator cùng nhóm.

---

## 7. Ticket khi xong

```
Root cause: SoLuongGoiThau persist trên KeHoachLuaChonNhaThau; nghiệp vụ thuộc QuyetDinhDuyetKHLCNT.
Files changed: (điền sau code)
Logic changed: KHLCNT 4 cột mới; QĐ có SoLuongGoiThau; chi tiết QĐ đọc entity QĐ.
Migration: Yes (mới, không sửa 20260812044306)
Build/Test: (điền)
```
