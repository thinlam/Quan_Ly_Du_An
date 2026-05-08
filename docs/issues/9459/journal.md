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
- Bật lại role enforcement: Duyet/TraLai yêu cầu `QLDA_LDDV`, Trinh yêu cầu KH-TC
- Tạo migration `RefactorPheDuyet` — chuẩn LEG Loai thành DungChung
- Thêm 20 integration tests cho QuanLyPheDuyet (dispatch, role-based, flow states)
- SQLite provider support: `--provider sqlite` CLI arg
- Fix SQLite DateTimeOffset compat — client-side evaluation cho aggregate queries

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
```

## Lessons learned

1. **PheDuyetNoiDung riêng lẻ → unified dispatch**: Thiết kế ban đầu tạo controller riêng cho từng entity → refactored sang 1 dispatch controller. Nên nghĩ về unified pattern từ đầu cho cross-cutting concerns.
2. **String status → FK status**: `TrangThaiId` FK an toàn hơn string code, DB enforce referential integrity.
3. **Polymorphic history**: 1 bảng `PheDuyetHistory` (EntityName + EntityId) linh hoạt hơn N per-entity history tables.
4. **SQLite compat**: Cần client-side evaluation cho `DateTimeOffset` aggregate queries trên SQLite.
