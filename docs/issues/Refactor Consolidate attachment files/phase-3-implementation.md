# Phase 3 Implementation Log — Chuyển QLDA sang `Attachment`

> **Phạm vi đợt này:** chỉ Phase 3 (QLDA migration sang BuildingBlocks.Application.Attachments).
> **Không commit** trong đợt này — chờ user tự commit.
> **Không** thực hiện Phase 4, 5, 6.

Ngày: 2026-07-20  
Branch: `151-refactor-consolidate-attachment-files`

---

## Summary

Phase 3 chuyển toàn bộ **write/read path** của QLDA WebApi từ command/query `TepDinhKem*` cũ sang hệ thống dùng chung:

| Cũ | Mới |
|---|---|
| `TepDinhKemBulkInsertOrUpdateCommand` | `AttachmentBulkInsertOrUpdateCommand` (+ `GroupTypes` bắt buộc) |
| `GetDanhSachTepDinhKemQuery` (controller callers) | `GetAttachmentsQuery` + `.ToAttachmentEntities()` |
| `TepDinhKemDto` (duplicate fields) | `TepDinhKemDto : AttachmentDto` (giữ tên UI) |
| Ký số via `SignedHelper` (WebApi mapping) | `SignedGroupTypeHelper` (BB Phase 2) |

**Giữ nguyên UI contract:** `DanhSachTepDinhKem`, `TepDinhKemDto`, `TepDinhKemInsertDto`, `TepDinhKemInsertOrUpdateDto`, `TepDinhKemModel`, `IMayHaveTepDinhKemModel`.

**Không** sửa business entity, **không** migration, **không** xóa entity/command cũ (Phase 5).

---

## Baseline trước Phase 3

```text
Branch: 151-refactor-consolidate-attachment-files
dotnet build SER.sln -c Release → Succeeded, 0 error, 0 warning

Phase 2 đã có:
  AttachmentBulkInsertOrUpdateCommand (GroupTypes bắt buộc, AutoDeleteMissing=false default)
  AttachmentCollectionExtensions / SignedGroupTypeHelper / IAttachmentDto
  GetAttachmentsQuery / AttachmentSubquery

QLDA Application handlers đã dùng IRepository<Attachment, Guid> từ trước.
Controllers vẫn gọi TepDinhKemBulkInsertOrUpdateCommand + GetDanhSachTepDinhKemQuery.
```

---

## Các bước triển khai (theo nhóm)

### Bước 0 — Survey inventory

| Pattern | Kết quả |
|---|---|
| `IRepository<TepDinhKem` trong QLDA | **0** (đã Attachment từ trước) |
| `TepDinhKemBulkInsertOrUpdateCommand` callers | ~47 controllers |
| `GetDanhSachTepDinhKemQuery` callers | ~34 controllers |
| `BuildingBlocks.Application.TepDinhKems` từ QLDA | 0 |

Phân loại: Controller (chính), DTO/Mapping (compatibility), Query/Command cũ (giữ file đến Phase 5).

---

### Nhóm 1 — DTO compatibility

**File:** `QLDA.Application/TepDinhKems/DTOs/TepDinhKemDto.cs`

```csharp
public class TepDinhKemDto : AttachmentDto, IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveCreated, IMayHaveUpdate
{
    public Guid GetId() => Id ?? SequentialGuid...;
    public string? TenNguoiTao { get; set; }   // field riêng QLDA
    // CreatedBy/At, UpdatedBy/At — JsonIgnore
}
```

- Xóa 9 core fields trùng với `AttachmentDto`.
- Tên type + JSON property giữ nguyên.

**File:** `QLDA.Application/TepDinhKems/DTOs/TepDinhKemMappingConfiguration.cs`

- `ToDto` → `entity.ToDto<TepDinhKemDto>()` (BB `AttachmentMapping`).
- Resolve ký số → `SignedGroupTypeHelper.ResolveSignedGroupType`.
- `List<TepDinhKemDto>.ToEntities` → delegate `IAttachmentDto.ToEntities` (BB).

**BB bổ sung:** `AttachmentCollectionExtensions`

- `ToEntities(IEnumerable<IAttachmentDto>, Guid, string baseGroupType)`
- `ToAttachmentEntities(IEnumerable<AttachmentDto>)` — bridge read-side cho ToModel nhận `List<Attachment>`

**Build sau Nhóm 1:** OK (sau fix nullability GroupId/GroupType).

---

### Nhóm 2 — Query (compatibility + migrate callers)

**Giữ file:** `QLDA.Application/TepDinhKems/Queries/GetDanhSachTepDinhKemQuery.cs`  
(handler vẫn `IRepository<Attachment>`; exact GroupType khi có `EGroupTypes` — chờ Phase 5 xóa)

**Migrate callers WebApi** → `GetAttachmentsQuery`:

```csharp
var files = (await Mediator.Send(new GetAttachmentsQuery(
    GroupIds: [entity.Id.ToString()],
    BaseGroupTypes: [nameof(EGroupType.Xxx)],  // null nếu cũ không filter
    IncludeSigned: false                        // khớp exact-match cũ
))).ToAttachmentEntities();
```

`IncludeSigned: false` bắt buộc để **không** đổi hành vi load (cũ không auto-merge `KySo_`).

**Không đổi:** `DuAnGetDanhSachTepDinhKemQuery` (query riêng theo DuAnId).

---

### Nhóm 3–4 — Controllers: BulkInsert → AttachmentBulkInsertOrUpdate

Pattern chuẩn:

```csharp
await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
{
    GroupId = entity.Id.ToString(),
    GroupTypes = [nameof(EGroupType.Xxx)],
    Entities = files,
    AutoDeleteMissing = true   // xác nhận: command cũ SyncCollection luôn xóa missing
}, cancellationToken);
```

- Using: `BuildingBlocks.Application.Attachments.Commands`
- Xóa `ScopeGroupTypes` → map sang `GroupTypes` (HoSoMoiThau, TrienKhaiKeHoachLCNT).
- Mọi call đều có `GroupTypes` non-empty.

**47 controllers** đã migrate (xem bảng GroupType bên dưới).

---

### Nhóm 5 — Mapping WebApi

**File:** `QLDA.WebApi/Models/TepDinhKems/TepDinhKemMappingConfigurations.cs`

- `SignedHelper.Prefix` → `SignedGroupTypeHelper.Prefix`
- Giữ logic `ResolveId` / `ResolveGroupType` đặc thù `TepDinhKemModel` (copy-link Id).
- Giữ tên helper `GetDanhSachTepDinhKem(...)`.

**File:** `QLDA.WebApi/Models/KhoKhanVuongMacs/KhoKhanVuongMacMappingConfiguration.cs`  
(using/query bridge nếu có)

---

### Bước cuối — Validation

```bash
dotnet clean SER.sln -c Release
dotnet build SER.sln -c Release --nologo
# → Succeeded, 0 Error(s), 0 Warning(s)
```

---

## Files changed

### BuildingBlocks (Phase 2 còn + Phase 3 bổ sung)

| File | Thay đổi |
|---|---|
| `Attachments/Common/AttachmentCollectionExtensions.cs` | + `IAttachmentDto.ToEntities`, + `ToAttachmentEntities` |
| `Attachments/Commands/AttachmentBulkInsertOrUpdateCommand.cs` | (Phase 2) |
| `Attachments/DTOs/*`, `Queries/GetAttachmentsQuery.cs` | (Phase 2) |
| `Attachments/Common/*`, `Validators/*` | (Phase 2, untracked) |

### QLDA.Application

| File | Thay đổi |
|---|---|
| `TepDinhKems/DTOs/TepDinhKemDto.cs` | `: AttachmentDto`, bỏ field trùng |
| `TepDinhKems/DTOs/TepDinhKemMappingConfiguration.cs` | SignedGroupTypeHelper + ToDto\<T\> |
| `TepDinhKems/Queries/GetDanhSachTepDinhKemQuery.cs` | comment compatibility; vẫn build |

### QLDA.WebApi — Controllers (modified)

BanGiaoHoSo, BaoCaoBanGiaoSanPham, BaoCaoBaoHanhSanPham, BaoCaoKetQuaKhaoSat, BaoCaoTienDo, ChuTruongLapKeHoach, DangTaiKeHoachLcntLenMang, DeXuatChuTruongChuyenTiep, DeXuatChuTruongMoi, DeXuatNhuCauKinhPhi, DeXuatNhuCauKinhPhiNam, DuAn, DuToanDauTu, GoiThau, HoSoDeXuatCapDoCntt, HoSoMoiThauDienTu, HopDong, KeHoachLuaChonNhaThau, KeHoachLuaChonNhaThauRutGon, KeHoachTrienKhaiChiTietDuAn, KeHoachTrienKhaiHangMuc, KetQuaTrungThau, KhoKhanVuongMac, NghiemThu, PhanKhaiKinhPhi, PheDuyetDuToan, PhuLucHopDong, QuyetDinhDieuChinh, QuyetDinhDuyetDuAn, QuyetDinhDuyetDuToan, QuyetDinhDuyetKHLCNT, QuyetDinhDuyetQuyetToan, QuyetDinhLapBenMoiThau, QuyetDinhLapHoiDongThamDinh, QuyetDinhThanhLapBanQlda, TamUng, ThanhLyHopDong, ThanhToan, ThoaThuanGiaoViec, ThuyetMinhDuAn, ToTrinhCoThamDinh, ToTrinhKetQuaGoiThau, ToTrinhPheDuyet, ToTrinhThamDinhNhaThau, TrienKhaiKeHoachLCNT, VanBanChuTruong, VanBanPhapLy.

### QLDA.WebApi — Mappings

| File | Thay đổi |
|---|---|
| `Models/TepDinhKems/TepDinhKemMappingConfigurations.cs` | SignedGroupTypeHelper |
| `Models/KhoKhanVuongMacs/KhoKhanVuongMacMappingConfiguration.cs` | sync usings / bridge |

### Compatibility files kept unchanged (Phase 5)

| File | Lý do |
|---|---|
| `QLDA.Application/TepDinhKems/Commands/TepDinhKemBulkInsertOrUpdateCommand.cs` | Không còn caller WebApi; xóa Phase 5 |
| `QLDA.Application/TepDinhKems/Queries/GetDanhSachTepDinhKemQuery.cs` | Không còn caller WebApi active; xóa Phase 5 |
| `BuildingBlocks/.../TepDinhKems/*` | Entity/command BB cũ — Phase 5 |
| `QLDA.Application/Common/SignedHelper.cs` | Phase 4/5 |
| `QLDA.Application/Common/SyncHelper.cs` | Phase 4/5 |
| Domain entities `DuAn`, `GoiThau`, ... | Không đụng |
| `TepDinhKemModel`, `IMayHaveTepDinhKemModel` | UI contract |

---

## GroupType mapping

| Nghiệp vụ | GroupId | GroupTypes | AutoDeleteMissing |
|---|---|---|---|
| BanGiaoHoSo Insert/Update | entity.Id | BanGiaoHoSo | true |
| BanGiaoHoSo BanGiao | entity.Id | BienBanBanGiao | true |
| BaoCaoBanGiaoSanPham | entity.Id | BaoCaoBanGiaoSanPham | true |
| BaoCaoBaoHanhSanPham | entity.Id | BaoCaoBaoHanhSanPham | true |
| BaoCaoKetQuaKhaoSat | entity.Id | BaoCaoKetQuaKhaoSat | true |
| BaoCaoTienDo | entity.Id | BaoCaoTienDo | true |
| ChuTruongLapKeHoach | entity.Id | ChuTruongLapKeHoach | true |
| DangTaiKeHoachLcntLenMang | entity.Id | DangTaiKeHoachLcntLenMang | true |
| DeXuatChuTruongChuyenTiep | entity.Id | DeXuatChuyenTiep | true |
| DeXuatChuTruongMoi | entity.Id | DeXuatChuTruongMoi | true |
| DeXuatNhuCauKinhPhi | entity.Id | DeXuatNhuCauKinhPhi | true |
| DeXuatNhuCauKinhPhiNam | entity.Id | DeXuatNhuCauKinhPhi (*) | true |
| DuAn / DuToan files | duToan.Id | DuToan | true |
| DuAn / KeHoachVon | khv.Id | KeHoachVon | true |
| DuAn / QuyetDinh | entity.Id | QuyetDinhPheDuyetNhiemVu | true |
| DuToanDauTu | entity.Id | DuToanDauTu | true |
| GoiThau | entity.Id | GoiThau | true |
| HopDong | entity.Id | KetQuaTrungThau (**) | true |
| HoSoDeXuatCapDoCntt | entity.Id | HoSoDeXuatCapDoCntt | true |
| HoSoMoiThauDienTu (6 scopes) | entity / ToTrinh / QuyetDinh Id | HoSoMoiThauDienTu, …ToTrinh, …QuyetDinh, …QuyetDinhTD, …CamKetTD, …BaoCaoTD | true |
| KeHoachLuaChonNhaThau | entity.Id | KeHoachLuaChonNhaThau | true |
| KeHoachLuaChonNhaThauRutGon | entity.Id | KeHoachLuaChonNhaThauRutGon | true |
| KeHoachTrienKhaiChiTietDuAn | entity.Id | KeHoachTrienKhaiChiTietDuAn | true |
| KeHoachTrienKhaiHangMuc | entity.Id | KeHoachTrienKhaiHangMuc | true |
| KetQuaTrungThau | entity.Id | KetQuaTrungThau | true |
| KhoKhanVuongMac | entity.Id | KhoKhanVuongMac, KetQuaXuLyKhoKhanVuongMac | true |
| NghiemThu | entity.Id | NghiemThu | true |
| PhanKhaiKinhPhi | entity.Id | PhanKhaiKinhPhi | true |
| PheDuyetDuToan | entity.Id | PheDuyetDuToan | true |
| PhuLucHopDong | entity.Id | PhuLucHopDong | true |
| QuyetDinhDieuChinh | result.Id | QuyetDinhDieuChinh | true |
| QuyetDinhDuyetDuAn | entity.Id | QuyetDinhDuyetDuAn | true |
| QuyetDinhDuyetDuToan | entity.Id | QuyetDinhDuyetDuToan | true |
| QuyetDinhDuyetDuToan (khác) | entity.Id | QuyetDinhDuyetDuToan_Khac | true |
| QuyetDinhDuyetKHLCNT | entity.Id | QuyetDinhDuyetKHLCNT | true |
| QuyetDinhDuyetQuyetToan | entity.Id | QuyetDinhDuyetQuyetToan | true |
| QuyetDinhLapBenMoiThau | entity.Id | QuyetDinhLapBenMoiThau | true |
| QuyetDinhLapHoiDongThamDinh | entity.Id | QuyetDinhLapHoiDongThamDinh | true |
| QuyetDinhThanhLapBanQlda | entity.Id | QuyetDinhLapBanQLDA | true |
| TamUng | entity.Id | TamUng | true |
| ThanhLyHopDong | entity.Id | ThanhLyHopDong_BienBanNghiemThu, ThanhLyHopDong, ThanhLyHopDong_Khac | true |
| ThanhToan | entity.Id | ThanhToan | true |
| ThoaThuanGiaoViec | entity.Id | ThoaThuanGiaoViec | true |
| ThuyetMinhDuAn | entity.Id | ThuyetMinhDuAn | true |
| ThuyetMinhDuAn ThamDinh | entity.Id | ThuyetMinhDuAnThamDinh | true |
| ToTrinhCoThamDinh | entity.Id | QuyetDinhKeHoachThue | true |
| ToTrinhCoThamDinh ThamDinh | entity.Id | QuyetDinhKeHoachThueThamDinh | true |
| ToTrinhKetQuaGoiThau | entity.Id | PheDuyetKetQuaGoiThauDuAn | true |
| ToTrinhPheDuyet | entity.Id | ToTrinhPheDuyet | true |
| ToTrinhThamDinhNhaThau | entity.Id | ToTrinhThamDinhNhaThau | true |
| ToTrinhThamDinhNhaThau nội dung | entity.Id | NoiDungToTrinhThamDinhNhaThau | true |
| ToTrinhThamDinhNhaThau kết quả | nhaThau.Id | KetQuaThamDinhNhaThau | true |
| TrienKhaiKeHoachLCNT | entity.Id | TrienKhaiKeHoachLCNT | true |
| TrienKhaiKeHoachLCNT ĐVTV | dv.Id | DonViTuVan | true |
| VanBanChuTruong | entity.Id | VanBanChuTruong | true |
| VanBanPhapLy | entity.Id | VanBanPhapLy | true |

(*) Pre-existing: controller `DeXuatNhuCauKinhPhiNam` dùng `EGroupType.DeXuatNhuCauKinhPhi` trong `ToEntities` — Phase 3 **không** đổi nghiệp vụ.  
(**) Pre-existing: `HopDongController` map file với `EGroupType.KetQuaTrungThau` — giữ nguyên.

`AutoDeleteMissing = true` vì `TepDinhKemBulkInsertOrUpdateCommand` cũ luôn `SyncCollection` (xóa missing trong scope).

---

## Remaining old references

| Reference | Phân loại | Ghi chú |
|---|---|---|
| `QLDA.Application/.../TepDinhKemBulkInsertOrUpdateCommand.cs` | Entity/command chờ Phase 5 | 0 caller WebApi |
| `QLDA.Application/.../GetDanhSachTepDinhKemQuery.cs` | Compatibility chờ Phase 5 | 0 caller WebApi active |
| `BuildingBlocks/.../TepDinhKems/*` | BB legacy chờ Phase 5 | |
| `TepDinhKemDto` / `TepDinhKemModel` / `DanhSachTepDinhKem` | **DTO compatibility — giữ** | UI contract |
| `new TepDinhKemModel { ... }` trong mapping | Compatibility — giữ | Không phải entity Domain |
| `DuAnGetDanhSachTepDinhKemQuery` | Query riêng — giữ Phase 3 | Khác GetDanhSach generic |
| `SignedHelper.cs` / `SyncHelper.cs` (QLDA) | Phase 4/5 | Application handlers vẫn có thể dùng |
| Entity `TepDinhKem` Domain + EF config | Phase 5 | |
| Comment block `GetDanhSachTepDinhKemQuery` trong ToTrinhThamDinhNhaThau | Dead comment | Có thể dọn Phase 5 |

`IRepository<TepDinhKem` trong QLDA: **0**.

---

## Validation

```text
Build command:
  dotnet clean SER.sln -c Release
  dotnet build SER.sln -c Release --nologo

Build result:
  Succeeded

Errors:
  0

Warnings:
  0
```

```text
git diff --check: không còn trailing whitespace lỗi (chỉ warning LF→CRLF autocrlf)
Không có migration mới / ModelSnapshot không sửa / không commit bin|obj
```

---

## Checklist kết thúc Phase 3

### QLDA implementation
- [x] Repository QLDA attachment → `IRepository<Attachment, Guid>` (đã sẵn + giữ)
- [x] Callers → `AttachmentBulkInsertOrUpdateCommand`
- [x] Mọi BulkInsert truyền `GroupTypes`
- [x] Controllers Get → `GetAttachmentsQuery` (+ bridge entities)
- [x] Mapping dùng `SignedGroupTypeHelper` / BB `ToDto` / `ToEntities`
- [x] Không sửa business entity

### UI contract
- [x] `DanhSachTepDinhKem` giữ tên
- [x] `TepDinhKemDto` giữ tên, kế thừa `AttachmentDto`
- [x] Insert/Update DTO tên giữ
- [x] Không đổi JSON contract

### Safety
- [x] Sync theo GroupId + GroupTypes
- [x] `AutoDeleteMissing=true` chỉ khi khớp SyncCollection cũ
- [x] Không viết `KySo_` rải rác — dùng helper
- [x] Get dùng `IncludeSigned: false` giữ exact match

### Build
- [x] Release build 0 error
- [x] Không migration / snapshot
- [x] Không commit (user tự commit)

---

## Deferred

- **Phase 4** — chưa: multi-GroupType loading sâu, transaction flow refactor, xóa/move SignedHelper/SyncHelper QLDA.
- **Phase 5** — chưa: xóa entity `TepDinhKem`, xóa command/query cũ, EF `ExcludeFromMigrations`, cleanup folder.
- **Phase 6** — chưa: docs chuẩn dự án / commit message chính thức (ngoài file log này).

---

## Gợi ý commit (khi user sẵn sàng)

Có thể tách 2 commit:

```text
1) BB: Application - Complete Attachments Phase 2 (GroupTypes, Sync, SignedHelper)
2) QLDA: WebApi/Application - Migrate TepDinhKem callers to Attachment (Phase 3)
```

Hoặc một commit gộp Phase 2+3 nếu muốn.

**Không commit** trong đợt agent này.
