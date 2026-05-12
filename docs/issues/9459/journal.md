# Journal — #9459 Quản lý phê duyệt nội dung trình duyệt

## 29/04 — Khởi tạo PheDuyetDuToan workflow (#9583)

**Commits:**
- `90f94f0` feat(PheDuyetDuToan): add approval workflow status tracking
- `7f64c07` feat(PheDuyetDuToan): implement approval workflow for issue #9583
- `e8cdb77` fix(PheDuyetDuToan): fix seeder, migration columns, role check, and add integration tests

**PR #40** merged — Nền tảng phê duyệt dự toán: entity `PheDuyetDuToan`, CRUD endpoints, role-based approval (BGĐ role), `DanhMucTrangThaiPheDuyetDuToan` seed data, integration tests.

**Note:** PheDuyetDuToan ban đầu được tạo cho #9583, sau này dùng lại làm entity đầu tiên trong unified dispatch của #9459.

---

## 05/05 — Tắt tạm thời BGĐ role check

**Commit:** `e4c76ad` chore(PheDuyetDuToan): temporarily disable BGĐ role check, update test fixture

Lý do: chưa cấu hình role thực tế trên server.

---

## 06/05 — Thêm trạng thái vào DTO + UC22 PheDuyetNoiDung ban đầu

**Commits:**
- `1fb33b1` fix(PheDuyetDuToan): add TrangThai to DTO and model responses
- `5c71571` feat(PheDuyetNoiDung): add UC22 approval module with CRUD and shared DanhMucTrangThaiPheDuyet
- `362de98` docs(issues): add #9459 UC22 implementation docs and issue references

**Thay đổi:**
- Thêm `MaTrangThai`, `TenTrangThai`, `IsSend` vào `PheDuyetDuToanDto`
- Tạo module `PheDuyetNoiDung` riêng lẻ — CRUD controller, commands (Trinh/Duyet/TraLai/TuChoi/KySo/PhatHanh/ChuyenQLVB), queries, DTOs
- Tạo shared `DanhMucTrangThaiPheDuyet` entity với `Loai` discriminator
- Tạo issue docs `docs/issues/9459/` (index.md, report.md)

---

## 07/05 — FK refactoring + merge DanhMucTrangThaiPheDuyetDuToan

**Commits:**
- `8bd9081` refactor(PheDuyet): use FK-based TrangThaiId instead of string status codes
- `3190ecd` docs(issues): update report and test-workflow for #9459 FK refactoring
- `366fe72` Merge PR #56 — DanhMucTrangThaiPheDuyetDuToan → DanhMucTrangThaiPheDuyet
- `bfe5286` refactor(PheDuyet): unify history, remove NoiDung, standardize Loai constants

**PR #55** merged — FK-based status tracking.

**Thay đổi:**
- Đổi `string TrangThai` → `int? TrangThaiId` FK → `DanhMucTrangThaiPheDuyet`
- Merge `DanhMucTrangThaiPheDuyetDuToan` vào shared `DanhMucTrangThaiPheDuyet`
- Merge `TrangThaiPheDuyetDuToanCodes` + `TrangThaiPheDuyetNoiDungCodes` → `TrangThaiPheDuyetCodes`
- Unify history: `PheDuyetDuToanHistory` → `PheDuyetHistory` (polymorphic: EntityName + EntityId)
- Xóa `PheDuyetNoiDung` entity, `PheDuyetNoiDungHistory` entity, và toàn bộ `PheDuyetNoiDungs/` application layer

---

## 08/05 — QuanLyPheDuyet unified dispatch + SQLite + tests

**Commits:**
- `e48324a` feat(PheDuyet): add QuanLyPheDuyet controller, commands, queries, DTOs
- `5727f24` fix(PheDuyet): enforce LDDV role check in Duyet/TraLai commands, update report
- `e0a911a` refactor(PheDuyet): add RefactorPheDuyet migration, standardize LEG Loai to DungChung
- `41eddc2` docs(9459): update report and test-workflow for QuanLyPheDuyet unified dispatch
- `5d14cb2` test(QuanLyPheDuyet): add 20 integration tests, fix SQLite DateTimeOffset compat
- `fb5cd4b` docs(9459): update test-workflow with QuanLyPheDuyet test matrix
- `56164f9` feat(WebApi): add SQLite provider support with CLI arg --provider sqlite
- `daf6058` chore(Migrator): update migration snapshot and config

**PR #59** merged.

**Thay đổi:**
- Tạo `QuanLyPheDuyetController` — unified dispatch pattern, 7 endpoints
- Dispatch commands theo `type` parameter → entity-specific handlers
- Bật lại role enforcement: Duyet/TraLai yêu cầu `QLDA_LD`, Trinh yêu cầu KH-TC
- Tạo migration `RefactorPheDuyet` — chuẩn LEG Loai thành DungChung
- Thêm 20 integration tests cho QuanLyPheDuyet (dispatch, role-based, flow states)
- SQLite provider support: `--provider sqlite` CLI arg
- Fix SQLite DateTimeOffset compat — client-side evaluation cho aggregate queries

---

## 11/05 — TuChoi + HoSo workflow + auto-assign Dự thảo

**Thay đổi:**

**Phase 1: PheDuyetDuToanTuChoiCommand**
- Tạo `PheDuyetDuToanTuChoiCommand` — Từ chối (Đã trình → Từ chối), role: GroupAdminOrManager (LDDV + HC_TH + QuanTri), lý do bắt buộc
- Tạo `PheDuyetDispatchTuChoiCommand` — dispatch tu-choi theo type
- Thêm endpoint `POST {type}/{id}/tu-choi` vào QuanLyPheDuyetController
- Thêm `TuChoi = "TC"` vào `TrangThaiPheDuyetCodes.DuToan`
- Seed data: Id=6, Ma="TC", Ten="Từ chối", Loai=PheDuyetDuToan

**Phase 2: HoSoDeXuatCapDoCntt workflow**
- Tạo 4 commands: `HoSoDeXuatCapDoCnttTrinhCommand`, `DuyetCommand`, `TraLaiCommand`, `TuChoiCommand`
- Cập nhật 4 dispatch commands thêm HoSoDeXuatCapDoCntt switch case
- `PheDuyetGetDanhSachQuery` + `PheDuyetGetChiTietQuery` hỗ trợ HoSoDeXuatCapDoCntt
- Thêm `TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt` constants
- Seed data: 5 rows (Id=7-11, DT/ĐTr/ĐD/TL/TC)

**Phase 3: HoSoMoiThauDienTu workflow**
- Tạo 4 commands: `HoSoMoiThauDienTuTrinhCommand`, `DuyetCommand`, `TraLaiCommand`, `TuChoiCommand`
- Cập nhật 4 dispatch commands + 2 queries
- Thêm `TrangThaiPheDuyetCodes.HoSoMoiThauDienTu` constants
- Seed data: 5 rows (Id=12-16, DT/ĐTr/ĐD/TL/TC)
- Note: `DuAnId` nullable → dùng `?? Guid.Empty` cho PheDuyetHistory

**Auto-assign Dự thảo**
- `PheDuyetDuToanInsertCommand` — tự lookup "DT" status, gán `entity.TrangThaiId`
- `HoSoDeXuatCapDoCnttInsertCommand` — tự lookup "DT" status, gán `entity.TrangThaiId`
- `HoSoMoiThauDienTuInsertCommand` — tự lookup "DT" status, gán `entity.TrangThaiId`

**Refactoring: Loai removal**
- Xóa `TrangThaiPheDuyetCodes.Loai` class — thay bằng `PheDuyetEntityNames` trực tiếp
- Thêm `PheDuyetEntityNames.Default` thay cho `Loai.DungChung`

**Migration:** `AddPheDuyetTuChoiAndHoSoSeedData` — 16 seed rows total (6 DuToan, 5 HoSoDeXuatCapDoCntt, 5 HoSoMoiThauDienTu)

---

## 12/05 — Fix NghiemThuUpdate duplicate key violation on junction table

**Issue:** `NghiemThuPhuLucHopDong` PK duplicate — `[08deace2-8e36-4f4a-687a-7b136406d2c1, 08deaa4c-2c25-4d29-687a-7b28580728af]`

**Root cause:** `NghiemThuMappings.Update()` tạo hoàn toàn mới collection `NghiemThuPhuLucHopDongs`. Khi entity được EF tracking với navigation property, `DbSet.Update()` + new junction records → tracking conflict → duplicate key.

**Fix (2 attempts):**

1. **Attempt 1 (failed):** Clear collection + recreate via `NghiemThuMappings.Update()` → still tracked by EF
2. **Attempt 2 (success):** 
   - Query entity với `.AsNoTracking()` — tránh tracking junction records
   - Update scalar properties trực tiếp (not via `entity.Update()`)
   - Sync junction table riêng trong handler — raw `DbContext.Set<>()` delete/insert

**Files changed:**
- `QLDA.Application/NghiemThus/Commands/NghiemThuUpdateCommand.cs` — handler xử lý junction table riêng
- `QLDA.Application/NghiemThus/NghiemThuMappings.cs` — revert `Update()` (không còn xử lý junction)

**Pattern cho junction table với composite key:**
```csharp
// 1. Query entity AsNoTracking để tránh tracking navigation
var entity = await repo.GetQueryableSet().AsNoTracking().FirstOrDefaultAsync(...);

// 2. Update scalar properties directly
entity.Property1 = dto.Property1;
entity.Property2 = dto.Property2;

// 3. Sync junction table via raw DbContext (tránh EF tracking conflict)
var db = repo.UnitOfWork as DbContext;
db.Set<JunctionEntity>().Where(x => x.LeftId == entity.Id).ExecuteDelete();
db.Set<JunctionEntity>().AddRangeAsync(newRecords);
```

---

## 12/05 — Refactor role constants (QLDA_LDDV → QLDA_LD, QLDA_HC_TH → PhongBanID check)

**Thay đổi:**

**QLDA_LDDV → QLDA_LD (Lãnh đạo đơn vị)**
- Xóa `QLDA_LDDV` constant khỏi `RoleConstants.cs`
- `QLDA_LD` giờ là role duy nhất cho Lãnh đạo (LDDV - Lãnh đạo đơn vị)
- Cập nhật `GroupAdminOrManager` dùng `QLDA_LD` thay vì `QLDA_LDDV`
- Cập nhật `PermissionConstants.RolePermissions[QLDA_LD]` = XemTatCa + PheDuyetActions
- Cập nhật `CauHinhVaiTroQuyenConfiguration` seed data
- Thay tất cả references trong 13 command files

**QLDA_HC_TH → PhongBanID check**
- Thay role check `QLDA_HC_TH` bằng `PhongBanID == PhongHCTHID` từ appsettings.json
- `IAppSettingsProvider.PhongHCTHID` đọc từ `appsettings.json`
- Đăng ký `TestAppSettingsProvider` trong test fixture với `PhongHCTHID = 300`
- 5 command files cập nhật: PheDuyetChuyenPhatHanh, PhanKhaiKinhPhiTuChoi, PheDuyetDuToanTuChoi, HoSoMoiThauDienTuTuChoi, HoSoDeXuatCapDoCnttTuChoi
- Xóa `QLDA_HC_TH` permission mapping trong `PermissionConstants.cs` (không còn role-based)

**Files modified:**
- `QLDA.Domain/Constants/RoleConstants.cs` — xóa QLDA_LDDV, cập nhật comment QLDA_LD
- `QLDA.Domain/Constants/PermissionConstants.cs` — cập nhật RolePermissions, xóa QLDA_HC_TH entry
- `QLDA.Persistence/Configurations/CauHinhVaiTroQuyenConfiguration.cs` — seed QLDA_LD với XemTatCa + PheDuyetActions
- `QLDA.Application/*/Commands/*TuChoiCommand.cs` — 4 files đổi sang PhongBanID check
- `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetChuyenPhatHanhCommand.cs` — đổi sang PhongBanID check
- `QLDA.WebApi/Controllers/DanhMucUserController.cs` — combobox lãnh đạo chỉ dùng QLDA_LD
- `QLDA.Tests/Fixtures/WebApiFixture.cs` — thêm TestAppSettingsProvider, CreateHcthClient() dùng PhongBanId=300
- Docs: `report.md`, `journal.md`, `test-workflow.md` — cập nhật role references

**Note:** Migrations trong `QLDA.Migrator/Migrations/` không được sửa (immutable snapshots). Database seed data sẽ dùng `QLDA_LD` từ code mới.

---

## Timeline tổng hợp

```
29/04  PheDuyetDuToan workflow (#9583) — nền tảng
  ↓
05/05  Tắt tạm BGĐ role check
  ↓
06/05  PheDuyetNoiDung UC22 + TrangThai vào DTO
  ↓
07/05  FK refactoring + unify history + merge DanhMuc
  ↓
08/05  QuanLyPheDuyet unified dispatch + SQLite + 20 tests → MERGED
  ↓
11/05  TuChoi + HoSoDeXuatCapDoCntt + HoSoMoiThauDienTu workflows + auto-assign Dự thảo
  ↓
12/05  Refactor role constants: QLDA_LDDV→QLDA_LD, QLDA_HC_TH→PhongBanID check
```

## Lessons learned

1. **PheDuyetNoiDung riêng lẻ → unified dispatch**: Thiết kế ban đầu tạo controller riêng cho từng entity → refactored sang 1 dispatch controller. Nên nghĩ về unified pattern từ đầu cho cross-cutting concerns.
2. **String status → FK status**: `TrangThaiId` FK an toàn hơn string code, DB enforce referential integrity.
3. **Polymorphic history**: 1 bảng `PheDuyetHistory` (EntityName + EntityId) linh hoạt hơn N per-entity history tables.
4. **SQLite compat**: Cần client-side evaluation cho `DateTimeOffset` aggregate queries trên SQLite.
