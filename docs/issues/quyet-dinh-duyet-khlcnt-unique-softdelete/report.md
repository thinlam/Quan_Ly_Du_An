# Báo cáo kỹ thuật — Unique index KHLCNT vs soft-delete

Không lấy Services làm source of truth. Task thuộc SER / QLDA.

**Ticket (điền khi merge):**

```
Root cause: Unique index IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId (WithOne) không filter IsDeleted.
Files changed: QuyetDinhDuyetKHLCNTConfiguration.cs + 20260821061550_FilterQuyetDinhDuyetKHLCNTUniqueIndexExcludeDeleted.cs (+ snapshot do ef add).
Logic changed: Unique 1-1 chỉ trên dòng IsDeleted = 0; dòng xóa mềm không chiếm slot KeHoachLuaChonNhaThauId.
Migration: Yes — đã apply catalog VI_DACDT. Staging/prod chưa.
Build/Test: Persistence build OK. SQL verify filter ([IsDeleted]=(0)). API them-moi sau update: retry trên kế hoạch 732a4c8e-...
```

---

## 1. Root cause

### 1.1 Triệu chứng

`POST /api/quyet-dinh-duyet-khlcnt/them-moi` khi kế hoạch đã từng có quyết định bị xóa mềm:

```
Khoá chính đã tồn tại: bảng [QuyetDinhDuyetKHLCNT],
chỉ mục [IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId]
```

Tên message dễ gây hiểu nhầm. Constraint fail là **unique index trên cột `KeHoachLuaChonNhaThauId`**, không phải PK `Id`.

Ví dụ reproduce: `KeHoachLuaChonNhaThauId = 732a4c8e-93af-4794-82f6-074615dadfc1`.

### 1.2 Luồng thật

1. User xóa QĐ → `QuyetDinhDuyetKHLCNTDeleteCommandHandler` set `entity.IsDeleted = true` rồi `SaveChanges`. **Không xóa dòng** khỏi bảng.
2. User thêm mới QĐ cùng kế hoạch → `QuyetDinhDuyetKHLCNTInsertOrUpdateCommandHandler` `AddAsync` + `SaveChanges`.
3. SQL Server đánh unique trên **mọi dòng** có cùng `KeHoachLuaChonNhaThauId` (kể cả đã xóa) → insert fail.

Xóa:

```26:30:QLDA.Application/QuyetDinhDuyetKHLCNTs/Commands/QuyetDinhDuyetKHLCNTDeleteCommand.cs
        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
```

Thêm mới vẫn insert dòng mới (không reuse Id cũ):

```94:98:QLDA.Application/QuyetDinhDuyetKHLCNTs/Commands/QuyetDinhDuyetKHLCNTInsertOrUpdateCommand.cs
                else
                {
                    await QuyetDinhDuyetKHLCNT.AddAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
```

Application **không** “sai logic nghiệp vụ thêm mới”. Lỗi nằm ở **schema index**.

### 1.3 Index đến từ đâu

Trước fix, `QuyetDinhDuyetKHLCNTConfiguration` **không** khai `HasIndex`. Unique vẫn tồn tại vì `HasOne/WithOne`:

```10:12:QLDA.Persistence/Configurations/QuyetDinhDuyetKHLCNTConfiguration.cs
        builder.HasOne(e => e.KeHoachLuaChonNhaThau)
            .WithOne(e => e.QuyetDinhDuyetKHLCNT)
            .HasForeignKey<QuyetDinhDuyetKHLCNT>(e => e.KeHoachLuaChonNhaThauId);
```

EF Core 1-1 tự tạo unique trên FK.

- `KeHoachLuaChonNhaThauId` là `Guid?` → filter mặc định: `[KeHoachLuaChonNhaThauId] IS NOT NULL`.
- Filter đó **không** gồm `[IsDeleted] = 0`.

Init (`20260715022910_Init.cs`, comment):

```csharp
unique: true,
filter: "[KeHoachLuaChonNhaThauId] IS NOT NULL"
```

### 1.4 Vì sao unique “cả bảng” xung đột với soft-delete

| Dòng | KeHoachLuaChonNhaThauId | IsDeleted | Unique cũ (IS NOT NULL) | Unique đúng (IsDeleted = 0) |
| --- | --- | --- | --- | --- |
| QĐ đã xóa | KH-A | 1 | **Vẫn chiếm slot** | Không chiếm |
| QĐ mới | KH-A | 0 | **Conflict** | OK |

Quy tắc: **một kế hoạch tối đa 1 QĐ đang active**. Nhiều dòng lịch sử đã xóa là hợp lệ.

### 1.5 Không sửa ở Application

Check `AnyAsync` / “nếu đã xóa thì update” không gỡ unique SQL, lệch HopDong / Đăng tải KHLCNT (đã filter DB). Phải sửa **index**.

---

## 2. Sai ở đâu (checklist)

| # | Chỗ | Kết luận |
| --- | --- | --- |
| 1 | `QuyetDinhDuyetKHLCNTConfiguration` (trước fix) | Thiếu filtered unique; chỉ `WithOne` |
| 2 | `QuyetDinhDuyetKHLCNTDeleteCommand` | Soft-delete đúng; kết hợp index cũ mới vỡ |
| 3 | Insert handler | Insert dòng mới đúng |
| 4 | Message “Khoá chính đã tồn tại” | Wrapper SQL unique; manh mối là **tên index** |
| 5 | Copy Services | Không dùng |

---

## 3. Cách sửa (đã làm)

### 3.1 Pattern

Giữ `WithOne` + khai `HasIndex` filtered — như `HopDong` / `DangTaiKeHoachLcntLenMang`. SQL Server: `[IsDeleted] = 0`.

### 3.2 Configuration

`QLDA.Persistence/Configurations/QuyetDinhDuyetKHLCNTConfiguration.cs`:

```14:17:QLDA.Persistence/Configurations/QuyetDinhDuyetKHLCNTConfiguration.cs
        // 1 kế hoạch chỉ 1 QĐ duyệt đang active; bản soft-delete không chiếm unique
        builder.HasIndex(e => e.KeHoachLuaChonNhaThauId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
```

`WithOne` giữ nguyên.

### 3.3 Migration

```bat
ef.bat QLDA add FilterQuyetDinhDuyetKHLCNTUniqueIndexExcludeDeleted
```

File: `QLDA.Migrator/Migrations/20260821061550_FilterQuyetDinhDuyetKHLCNTUniqueIndexExcludeDeleted.cs`

- Không sửa Init / snapshot tay.
- Log `Validation[10622]` / `[10400]` khi add là warn sẵn có — **không fail**. `OPERATION COMPLETED SUCCESSFULLY`.
- `ef add` chỉ tạo file. Index SQL **chỉ đổi sau** `ef.bat QLDA update`.

`Up()`:

```csharp
DropIndex("IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId");
CreateIndex(..., unique: true, filter: "[IsDeleted] = 0");
```

`Down()`: trả filter cũ `[KeHoachLuaChonNhaThauId] IS NOT NULL` (bug quay lại). Rollback unique cũ fail nếu DB đã có nhiều dòng cùng FK (active + deleted).

### 3.4 Rủi ro apply

`CREATE UNIQUE INDEX ... WHERE IsDeleted = 0` fail nếu ≥ 2 dòng **active** cùng `KeHoachLuaChonNhaThauId`. Query trước update: [test-workflow.md](./test-workflow.md) mục 2.

### 3.5 Commit group (khi được phép)

Cùng commit: Persistence.Configuration + Migrator (migration + snapshot). Chưa commit/push.

---

## 4. Trạng thái (2026-08-21)

| Hạng mục | Trạng thái |
| --- | --- |
| Configuration `HasIndex` + `HasFilter` | Xong |
| Migration `20260821061550_FilterQuyetDinhDuyetKHLCNTUniqueIndexExcludeDeleted` | Xong (`ef add`) |
| Apply DB `VI_DACDT` | Xong (`ef.bat QLDA update`) |
| Staging / production | **Chưa** apply — không update từ máy dev |
| Commit / push | Chưa |

SQL sau update (`VI_DACDT`):

| Check | Kết quả |
| --- | --- |
| `sys.indexes.filter_definition` | `([IsDeleted]=(0))`, `is_unique = 1` |
| `__EFMigrationsHistory` | Có `20260821061550_FilterQuyetDinhDuyetKHLCNTUniqueIndexExcludeDeleted` |
| `732a4c8e-93af-4794-82f6-074615dadfc1` | 1 dòng, `IsDeleted = 1` (`Id` `08DEFCD8-...`) |
| Duplicate active cùng KH | Không có |

Phạm vi: **cả bảng**, không chỉ 1 kế hoạch. Unique vẫn chặn 2 QĐ **active** cùng `KeHoachLuaChonNhaThauId`. Bảng khác unique chưa filter `IsDeleted` thì không tự hết.

---

## 5. Hướng trao đổi với leader

### Mở đầu

> Không trùng PK `Id`. Unique index `IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId`. Xóa mềm rồi tạo lại cùng kế hoạch; dòng `IsDeleted = 1` vẫn chiếm unique.

### Vì sao không sửa handler

> Soft-delete đúng convention. Check Application không gỡ unique SQL. HopDong / Đăng tải KHLCNT đã filtered `[IsDeleted] = 0`.

### Đã làm

> Giữ `WithOne`, unique filtered `WHERE IsDeleted = 0`. Migration `20260821061550_...`. Đã apply `VI_DACDT`, SQL filter `([IsDeleted]=(0))`. Staging/prod cần DBA apply riêng.

### Cần leader / DBA

1. Nghiệp vụ: 1 KHLCNT ↔ 1 QĐ active; lịch sử xóa giữ.
2. Apply staging: query duplicate active trước (test-workflow mục 2).
3. Không apply production từ máy dev.
4. Số ticket PMIS để đổi tên `docs/issues/`.

### Không nói

- “Sửa check `Any` trước insert là xong.”
- “Bỏ unique.” → mất 1-1 active.
- “Sửa migration Init.”

---

## 6. Phạm vi

**Đã làm:** Configuration + migration mới + update `VI_DACDT`.

**Không làm:** sửa Application để bypass unique; sửa migration cũ; update staging/prod; copy Services; commit/push cho đến khi được yêu cầu.
