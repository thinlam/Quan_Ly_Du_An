# UC22 — Thông báo sau phê duyệt Đề xuất chủ trương chuyển tiếp (Design)

**Ngày:** 2026-07-13  
**Phạm vi:** Chỉ luồng **phê duyệt** `DeXuatChuyenTiep` (không gồm trả lại, phát hành QLVB, migration).  
**Trạng thái:** Chờ review trước khi code.

---

## 1. Mục tiêu

Sau khi GĐ/PGĐ phê duyệt **Đề xuất chủ trương chuyển tiếp** thành công, hệ thống gửi thông báo tới **người đã trình duyệt** qua stored procedure `dbo.CoreMessaging_CreateNotification`.

Nguyên tắc cứng:

1. Lưu nghiệp vụ phê duyệt trước (`SaveChangesAsync`).
2. Chỉ gọi SP khi save thành công (`affected > 0`).
3. Lỗi notification **không** làm fail / rollback nghiệp vụ đã lưu.
4. Không migration / không đổi schema.

---

## 2. Hiện trạng source (đã khảo sát)

### 2.1. `DeXuatChuyenTiepDuyetCommand.cs` — WIP chưa đúng

File đã có skeleton thông báo nhưng **sai thứ tự và chưa hoàn thiện**:

| Vấn đề | Chi tiết |
|--------|----------|
| Gọi TB trước Save | Block `try` thông báo nằm **trước** `AddAsync(history)` / `SaveChangesAsync` |
| HTML lỗi | `của dự án {entity.DuAn?.TenDuAn}</b>` thiếu `<b>` mở |
| Cast nguy hiểm | `(int)_userProvider.Info.UserID` trong khi `UserID` là `long` |
| Catch rỗng | `catch (Exception ex) { }` |
| Chưa gọi SP | Comment `// exec thongBaoInsertCommand here` |
| Thiếu filter PheDuyet | Query chỉ `EntityId == request.Id`, chưa lọc `EntityName` |

Điểm đã tốt (giữ lại):

- `Include(x => x.DuAn).ThenInclude(x => x.BuocHienTai)` — đủ data cho body.
- Đã inject `IRepository<PheDuyet, Guid>` và `IUserProvider`.

### 2.2. Entry point API

```
QuanLyPheDuyetController
  → PheDuyetDispatchDuyetCommand
    → DeXuatChuyenTiepDuyetCommand   // khi type = DeXuatChuTruongChuyenTiep
```

### 2.3. Pattern Dapper hiện có

- Abstraction: `IDapperRepository` (`BuildingBlocks.Domain`).
- Implementation: `DapperRepository` dùng `IDbConnectionFactory` + Dapper.
- API hiện tại:
  - `QueryStoredProcAsync<T>(...)`
  - `QueryAsync<T>(...)`
- **Chưa có** `ExecuteStoredProcAsync` / `ExecuteAsync`.
- `QLDA.Application` **không** reference Persistence → **không** inject `IDbConnectionFactory` trực tiếp trong Application handler. Phải đi qua `IDapperRepository`.

### 2.4. Kiểu dữ liệu thật

| Field | Kiểu |
|-------|------|
| `_userProvider.Info.UserID` | `long` |
| `PheDuyet.NguoiTrinhId` | `int?` |
| `DeXuatChuyenTiep.SoLieuGiaiNgan` | `long?` |
| `PheDuyetEntityNames.DeXuatChuTruongChuyenTiep` | `"DeXuatChuyenTiep"` |
| Description | `"Đề xuất chủ trương chuyển tiếp"` |

### 2.5. WIP ngoài phạm vi UC22 (cảnh báo)

`ToTrinhPheDuyetDuyetCommand` / `TraLaiCommand` (uncommitted) đang gọi `PheDuyetNotificationHelper.NotifyAfterSaveAsync`, nhưng **file helper không tồn tại trên disk** → build ToTrinh sẽ fail.  
UC22 **không** bắt buộc sửa ToTrinh; khi implement `ThongBaoInsertCommand`, ToTrinh có thể chuyển sang `_mediator.Send(new ThongBaoInsertCommand(...))` ở task riêng.

---

## 3. Các hướng tiếp cận

### A. `ThongBaoInsertCommand` (CQRS) — **Khuyến nghị**

- Tạo command + handler gọi SP qua `IDapperRepository`.
- `DeXuatChuyenTiepDuyetCommandHandler` gọi `_mediator.Send(...)` **sau** Save.
- Đúng Clean Architecture + CQRS của project (không tạo Application Service).

### B. `PheDuyetNotificationHelper` static

- Giống WIP ToTrinh.
- Dễ tái sử dụng body template Duyet/TraLai.
- **Nhược điểm:** gần với Application Service; AGENTS.md cấm `Application/Services`; helper chưa tồn tại.

### C. Gọi Dapper inline trong Duyet handler

- Ít file nhất.
- **Nhược điểm:** khó tái sử dụng; trùng logic nếu nhiều module gửi TB.

**Quyết định design:** Chọn **A**. Bổ sung `ExecuteStoredProcAsync` trên `IDapperRepository` để handler không cần `IDbConnectionFactory`.

---

## 4. Kiến trúc luồng

```text
Handle(DeXuatChuyenTiepDuyetCommand)
  ├─ Validate trạng thái / quyền bước
  ├─ Update entity.TrangThaiId = Đã duyệt
  ├─ Add PheDuyetHistory
  ├─ affected = SaveChangesAsync()
  ├─ if affected <= 0 → return affected
  └─ try
       ├─ Load PheDuyet (EntityId + EntityName)
       ├─ Resolve NguoiNhanId / NguoiGuiId (validate > 0)
       ├─ Build HTML body
       └─ mediator.Send(ThongBaoInsertCommand)
     catch → LogError, return affected (không throw)
```

```text
ThongBaoInsertCommandHandler
  └─ IDapperRepository.ExecuteStoredProcAsync(
         "dbo.CoreMessaging_CreateNotification",
         { NguoiNhanId, NguoiGuiId, NotifyTypesId=24,
           Subject="Quản lý dự án", Body, PortalId=0 })
```

---

## 5. Mapping SP

| Parameter | Nguồn |
|-----------|--------|
| `@NguoiNhanId` | `pheDuyet.NguoiTrinhId` → `long` |
| `@NguoiGuiId` | `_userProvider.Info.UserID` (`long`) |
| `@NotifyTypesId` | `24` |
| `@Subject` | `"Quản lý dự án"` |
| `@Body` | HTML (xem §6) |
| `@PortalId` | `0` |

Không cast `(int)UserID`.

---

## 6. Body HTML

```html
Tờ trình/phê duyệt <b>Đề xuất chủ trương chuyển tiếp</b> giá trị giải ngân <b>{soLieu}</b> của dự án <b>{tenDuAn}</b> - <b>{tenBuoc}</b> đã được duyệt.
```

- `tenLoai` = `PheDuyetEntityNames.DeXuatChuTruongChuyenTiep.GetDescriptionFromName()`
- `soLieu` = `entity.SoLieuGiaiNgan` (giữ format hiện tại của WIP / không invent culture mới trừ khi team yêu cầu `N0`)
- Mọi thẻ `<b>` phải đóng `</b>`.

---

## 7. Error handling

| Tình huống | Hành vi |
|------------|---------|
| `affected <= 0` | Không gửi TB |
| `pheDuyet == null` | Warning log, không gọi SP |
| `NguoiTrinhId` null / ≤ 0 | Warning log, không gọi SP |
| `UserID` ≤ 0 | Warning log, không gọi SP |
| SP / Dapper exception | Error log, **không** throw lại |

---

## 8. Phạm vi file

| File | Hành động |
|------|-----------|
| `BuildingBlocks/.../IDapperRepository.cs` | Thêm `ExecuteStoredProcAsync` |
| `BuildingBlocks/.../DapperRepository.cs` | Implement bằng `connection.ExecuteAsync` |
| `QLDA.Application/ThongBaos/Commands/ThongBaoInsertCommand.cs` | **Tạo mới** |
| `QLDA.Application/DeXuatChuyenTiep/Commands/DeXuatChuyenTiepDuyetCommand.cs` | Sửa luồng TB |

**Không sửa:** migration, snapshot, SP SQL, ToTrinh (ngoài UC22), TraLai DeXuatChuyenTiep.

---

## 9. Impact analysis (thủ công — GitNexus MCP/CLI hiện không available)

### `DeXuatChuyenTiepDuyetCommand` / Handler

| Hạng mục | Chi tiết |
|----------|----------|
| Direct callers | `PheDuyetDispatchDuyetCommand` (switch `DeXuatChuTruongChuyenTiep`) |
| API surface | `QuanLyPheDuyetController` duyệt qua dispatch |
| Risk | **MEDIUM** — side-effect sau save; nếu làm đúng thì không đổi contract API (`IRequest<int>`) |
| Affected flows | UC22 phê duyệt đề xuất chuyển tiếp |

### `IDapperRepository.ExecuteStoredProcAsync` (mới)

| Hạng mục | Chi tiết |
|----------|----------|
| Callers hiện tại | Không (API mới additive) |
| Risk | **LOW** — chỉ thêm method, không đổi method cũ |
| Consumers tương lai | `ThongBaoInsertCommandHandler`, có thể ToTrinh |

### `ThongBaoInsertCommand` (mới)

| Hạng mục | Chi tiết |
|----------|----------|
| Callers dự kiến | `DeXuatChuyenTiepDuyetCommandHandler` |
| Modules dùng chung sau này | ToTrinh duyệt/trả lại, các DuyetCommand khác |
| Risk | **LOW** |

---

## 10. Acceptance Criteria (tóm tắt)

- AC01–AC10 theo task UC22: save trước, SP sau, đúng người gửi/nhận, NotifyTypesId=24, HTML đúng, lỗi TB không fail API, không schema change.

---

## 11. Out of scope

- Thông báo khi **trả lại** DeXuatChuyenTiep.
- Sửa ToTrinh / tạo `PheDuyetNotificationHelper`.
- Migration, sửa SP, API controller mới.
---

**Review gate:** Xác nhận design (đặc biệt chọn Approach A + extend `IDapperRepository`) trước khi chạy implementation plan tại `docs/superpowers/plans/2026-07-13-uc22-dexuat-chuyentiep-notification.md`.
