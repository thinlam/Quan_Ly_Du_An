# Phase 6 — Documentation (Attachment Pattern)

> **Trạng thái:** Hoàn thành  
> **Branch:** `151-refactor-consolidate-attachment-files`  
> **Ngày:** 2026-07-20  
> **Phạm vi:** Chỉ Phase 6 — cập nhật docs chuẩn dự án. **Không commit** (user tự commit).

---

## Summary

Phase 6 đồng bộ tài liệu chuẩn dự án sau refactor Phase 1–5: thay reference pattern cũ `TepDinhKem*` bằng **Attachment pattern** (entity `Attachment`, BB commands/queries, giữ API contract `TepDinhKemDto` / `DanhSachTepDinhKem`).

---

## Bước 0 — Baseline & phạm vi

### Mục tiêu (từ `plan.md` Phase 6)

| # | Task | File |
|---|------|------|
| 1 | Section "TepDinhKem pattern" → **"Attachment pattern"** | `docs/code-standards.md` |
| 2 | Cập nhật reference TepDinhKem → Attachment | `BuildingBlocks/CLAUDE.md` |
| 3 | Cập nhật tương tự cho QLDA | `CLAUDE.md` (root — xem note bên dưới) |

### Note về `QLDA/CLAUDE.md`

Repo `e:\SER` **không có** folder `QLDA/` hay file `QLDA/CLAUDE.md`. Module QLDA dùng **`CLAUDE.md` ở root** làm project rules. Phase 6 cập nhật file đó thay cho path trong plan.

### Trạng thái code trước Phase 6

Phase 1–5 đã hoàn thành (xem `phase-1-2-implementation.md` … `phase-5-implementation.md`):

- Runtime entity: `Attachment` only
- DB table QLDA: `TepDinhKem` (không đổi)
- Controllers: `GetAttachmentsQuery` + `AttachmentBulkInsertOrUpdateCommand`
- Helpers: `SignedGroupTypeHelper`, `AttachmentSubquery`, `AttachmentCollectionExtensions`

`docs/code-standards.md` **chưa có** section attachment — Phase 6 **thêm mới** §14 (plan gọi là rename nhưng section cũ chưa tồn tại trong file này).

---

## Bước 1 — `docs/code-standards.md`

**File:** `docs/code-standards.md`  
**Hành động:** Thêm **§14. Attachment Pattern (File đính kèm)** trước footer.

### Nội dung đã thêm

| Mục | Mô tả |
|-----|-------|
| Kiến trúc tổng quan | Bảng layer Domain / Application / Persistence / API contract |
| Hard rules | `IRepository<Attachment>`, không rename DTO API, không thêm field vào `AttachmentDto` chung |
| GroupId + GroupType | Giải thích liên kết không FK, convention `ParentId` / ký số |
| `SignedGroupTypeHelper` | Single source of truth cho prefix `KySo_` |
| Write side | `AttachmentBulkInsertOrUpdateCommand`, `ToEntities`, transaction, multi-GroupType |
| Read — Hydration | Controller pattern: `GetAttachmentsQuery` → `ToAttachmentEntities()` → `ToModel()` |
| Read — IQueryable subquery | `ExpandGroupTypes` + `Contains` trong `.Select()`; anti-pattern `_mediator.Send` trong projection |
| DTO mapping | `TepDinhKemDto : AttachmentDto`, `ToDto<T>()`, `ToEntities()` |
| Anti-patterns | Liệt kê entity/command cũ và sai lầm thường gặp |
| Tham chiếu code | Bảng path tới BB helper files + link tới plan/phase docs |

### Code mẫu đã document (không sửa runtime)

- Write: `BaoCaoTienDoController` pattern (`AttachmentBulkInsertOrUpdateCommand`)
- Read hydration: `GetAttachmentsQuery` + `ToAttachmentEntities()`
- Read list: `KhoKhanVuongMacGetDanhSachQuery` pattern với `ExpandGroupTypes`

---

## Bước 2 — `BuildingBlocks/CLAUDE.md`

**File:** `BuildingBlocks/CLAUDE.md`  
**Hành động:** Cập nhật 3 vị trí reference `TepDinhKem` → `Attachment`.

### 2.1 Bảng Vietnamese Domain Terms

| Trước | Sau |
|-------|-----|
| `TepDinhKem` \| File Attachment | `Attachment` \| File Attachment (runtime entity; DB table may still be `TepDinhKem`) |

### 2.2 Ví dụ GetById Query (Legacy Tables section)

| Thay đổi | Chi tiết |
|----------|----------|
| Repository inject | `IRepository<TepDinhKem, Guid>` → `IRepository<Attachment, Guid>` |
| Subquery filter | Thêm `AttachmentSubquery.ExpandGroupTypes` + `GroupType` filter (không chỉ `GroupId`) |
| Comment | "TepDinhKem uses GroupId" → "Attachment uses GroupId + GroupType" |
| Using note | Comment `Requires: BuildingBlocks.Application.Attachments.Common, ...` |

### 2.3 Bảng Key Points

| Trước | Sau |
|-------|-----|
| Subquery `TepDinhKem` … | Subquery `Attachment` … via `AttachmentSubquery` / `SignedGroupTypeHelper` |

---

## Bước 3 — `CLAUDE.md` (QLDA root)

**File:** `CLAUDE.md` (root project rules)  
**Hành động:** Thêm section **Attachment Pattern (File đính kèm)** sau Authorization Pattern.

### Nội dung section mới

- Link chi tiết: `docs/code-standards.md` §14
- Hard rules tóm tắt (6 bullet)
- 2 code snippet: Write (`AttachmentBulkInsertOrUpdateCommand`) + Read hydration (`GetAttachmentsQuery`)

**Không sửa** các rule Migration, DTO, Authorization hiện có.

---

## Bước 4 — `BuildingBlocks/docs/code-standards.md` (bonus đồng bộ)

**File:** `BuildingBlocks/docs/code-standards.md`  
**Hành động:** Cập nhật 1 dòng trong bảng Vietnamese Domain Terms (đồng bộ với BB CLAUDE.md).

| Trước | Sau |
|-------|-----|
| `TepDinhKem` | `Attachment` (runtime entity; DB table may be `TepDinhKem` per module config) |

> Plan Phase 6 không liệt kê file này — thêm để tránh mâu thuẫn giữa 2 file code-standards.

---

## Danh sách file đã sửa (Phase 6)

| File | Loại thay đổi |
|------|---------------|
| `docs/code-standards.md` | **Thêm** §14 Attachment Pattern (~120 dòng) |
| `BuildingBlocks/CLAUDE.md` | **Sửa** domain terms, GetById example, Key Points table |
| `CLAUDE.md` | **Thêm** section Attachment Pattern |
| `BuildingBlocks/docs/code-standards.md` | **Sửa** 1 dòng domain terms |
| `docs/issues/Refactor Consolidate attachment files/phase-6-implementation.md` | **Tạo mới** (file này) |

**Không sửa:** code `.cs`, migration, test, controller, handler.

---

## Checklist acceptance (Phase 6)

- [x] `docs/code-standards.md` có pattern mới (Hydration, SignedGroupTypeHelper, AttachmentSubquery, write/read)
- [x] `BuildingBlocks/CLAUDE.md` reference đồng bộ Attachment
- [x] QLDA rules (`CLAUDE.md` root) có section Attachment + link tới code-standards
- [x] File log Phase 6 chi tiết từng bước
- [ ] User commit (ngoài phạm vi agent)

---

## Gợi ý commit message (khi user commit)

```
docs: Phase 6 — document Attachment pattern (replace TepDinhKem refs)

- Add §14 Attachment Pattern to docs/code-standards.md
- Update BuildingBlocks/CLAUDE.md and root CLAUDE.md
- Add phase-6-implementation.md log
```

Hoặc tách 2 commit:

1. `docs: Add Attachment pattern to code-standards and CLAUDE files`
2. `docs: Phase 6 implementation log for attachment refactor`

---

## Liên kết các phase trước

| Phase | File log | Nội dung chính |
|-------|----------|----------------|
| 1–2 | `phase-1-2-implementation.md` | BB Attachments foundation, helpers |
| 3 | `phase-3-implementation.md` | QLDA WebApi migrate commands/queries |
| 4 | `phase-4-implementation.md` | AttachmentSubquery, list handler load |
| 5 | `phase-5-implementation.md` | EF mapping, xóa entity TepDinhKem cũ |
| 6 | `phase-6-implementation.md` | Documentation (đợt này) |

---

## Verification (optional — user chạy trước commit)

Phase 6 chỉ docs — build không bắt buộc. Nếu muốn xác nhận branch vẫn green:

```powershell
Set-Location e:\SER
dotnet build SER.sln -c Release --nologo
```

Kỳ vọng: 0 errors (không có thay đổi code).
