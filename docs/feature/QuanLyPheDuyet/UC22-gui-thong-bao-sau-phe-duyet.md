# UC22 — Gửi thông báo sau khi phản hồi/phê duyệt thông tin dự án

> **Issue:** [#141 — UC22 phản hồi/phê duyệt thông tin dự án](https://github.com/...)  
> **Trạng thái:** Đã triển khai + smoke test duyệt thành công (2026-07-10)  
> **Build:** `dotnet build SER.sln` — 0 error  
> **Ngày khảo sát / cập nhật:** 2026-07-10

---

## 0. Trạng thái triển khai (as-built)

### Đã xong

| Hạng mục | Chi tiết |
|----------|----------|
| Hạ tầng Dapper | `IDapperRepository.ExecuteStoredProcAsync` + implement trong `DapperRepository` |
| Helper | `PheDuyetNotificationHelper` + `PheDuyetNotificationAction` |
| Pilot | `DeXuatChuyenTiep` Duyet + TraLai (xóa stub WIP) |
| Rollout | ~51 handler: Duyet / TraLai / TuChoi / Chuyen + Phase 3 |
| Migration | **Không** sửa migration / snapshot |
| Smoke test | `POST .../DeXuatChuTruongMoi/{id}/duyet` → `result: true`, `dataResult: 2` |

### Luồng runtime thực tế

```
POST /api/phe-duyet/{type}/{entityId}/duyet
  → PheDuyetDispatchDuyetCommand (auth EnsureCanApproveDuAnAsync)
  → *DuyetCommand handler
       → validate + đổi trạng thái + PheDuyetHistory
       → SaveChangesAsync
       → nếu affected > 0:
            PheDuyetNotificationHelper.NotifyAfterSaveAsync
              → ResolveNguoiNhanId (History ĐTr → PheDuyet.NguoiTrinhId → CreatedBy)
              → BuildBody
              → EXEC dbo.CoreMessaging_CreateNotification
```

### File chính đã thêm/sửa

| File | Vai trò |
|------|---------|
| `BuildingBlocks/.../IDapperRepository.cs` | +`ExecuteStoredProcAsync` |
| `BuildingBlocks/.../DapperRepository.cs` | Implement SP execute + `CancellationToken` |
| `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetNotificationAction.cs` | Enum hành động |
| `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetNotificationHelper.cs` | Resolve + body + gọi SP |
| `QLDA.Application/**/**{Duyet,TraLai,TuChoi,Chuyen}Command.cs` | Gọi helper sau save |
| `HoSoDeXuatCapDoCnttPheDuyetCommand` / `ToTrinhCoThamDinhThaoTacCommand` | Map `TrangThaiTiepTheo` → action |

### Tham số SP (cố định)

| Param | Giá trị |
|-------|---------|
| `@NguoiNhanId` | `long` — UserPortalId người trình |
| `@NguoiGuiId` | `long` — `_userProvider.Info.UserID` |
| `@NotifyTypesId` | `24` |
| `@Subject` | `"Quản lý dự án"` |
| `@Body` | Backend build theo action |
| `@PortalId` | `0` |

---

## 1. Mục tiêu

Sau khi người có thẩm quyền xử lý phê duyệt **thành công** (đã `SaveChangesAsync`), hệ thống gọi stored procedure `dbo.CoreMessaging_CreateNotification` để gửi thông báo tới **người/đơn vị đã trình duyệt**.

| Hành động | Ai nhận thông báo | Ví dụ Body |
|-----------|-------------------|------------|
| Phê duyệt (`duyet`) | Người trình | `Thông tin dự án [Tên dự án] đã được phê duyệt.` |
| Từ chối (`tu-choi`) | Người trình | `Thông tin dự án [Tên dự án] bị từ chối. Lý do: [Lý do].` |
| Yêu cầu cập nhật (`tra-lai`) | Người trình | `Thông tin dự án [Tên dự án] cần cập nhật và gửi lại. Nội dung: [Ý kiến].` |
| Chuyển xử lý (`chuyen`) | Người liên quan bước tiếp | `Thông tin dự án [Tên dự án] đã được chuyển xử lý.` |
| Phát hành (nếu có command) | Người trình | `Thông tin dự án [Tên dự án] đã được phát hành.` |

**Không** gửi notification khi nghiệp vụ thất bại. **Không** thay đổi logic lưu hiện tại.

---

## 2. Kiến trúc hiện tại (đã khảo sát + đã code)

### 2.1 Luồng API

```
FE → QuanLyPheDuyetController
       → PheDuyetDispatch{Duyet|TraLai|TuChoi|Chuyen|Trinh}Command
            → Entity *Command handler (ví dụ DeXuatChuyenTiepDuyetCommand)
                 → validate → cập nhật entity → PheDuyetHistory → SaveChangesAsync
```

**Controller:** `QLDA.WebApi/Controllers/QuanLyPheDuyetController.cs`

| Endpoint | Dispatch command | Hành động |
|----------|----------------|-----------|
| `POST api/phe-duyet/{type}/{id}/duyet` | `PheDuyetDispatchDuyetCommand` | Phê duyệt |
| `POST api/phe-duyet/{type}/{id}/tra-lai` | `PheDuyetDispatchTraLaiCommand` | Trả lại / yêu cầu cập nhật |
| `POST api/phe-duyet/{type}/{id}/tu-choi` | `PheDuyetDispatchTuChoiCommand` | Từ chối |
| `POST api/phe-duyet/{type}/{id}/chuyen` | `PheDuyetDispatchChuyenCommand` | Chuyển xử lý |
| `POST api/phe-duyet/{type}/{id}/trinh` | `PheDuyetDispatchTrinhCommand` | Trình (ngoài scope notification UC22*) |

\* UC22 mô tả gửi thông báo **kết quả xử lý** tới người trình → **không** áp dụng cho `trinh` (trừ khi BA xác nhận thêm luồng “tiếp nhận đã cập nhật” cần báo ngược cho người duyệt).

### 2.2 Pattern handler sau UC22

```
1. Validate + auth (giữ nguyên)
2. Đổi trạng thái + PheDuyetHistory (giữ nguyên)
3. var affected = await SaveChangesAsync(...)
4. if (affected > 0) → PheDuyetNotificationHelper.NotifyAfterSaveAsync(...)
5. return affected
```

**Không** dùng explicit transaction chung với Dapper. SP chạy **sau** commit EF; lỗi SP chỉ log, không rollback nghiệp vụ.

### 2.3 Stub WIP — đã xử lý

`DeXuatChuyenTiepDuyetCommand` trước đây có stub notification sai (gọi trước save, cast `int`, nuốt exception). **Đã xóa** và thay bằng helper chuẩn.

### 2.4 Dapper / connection (as-built)

| Thành phần | File | Trạng thái |
|------------|------|------------|
| `IDapperRepository` | `BuildingBlocks.Domain/Interfaces/IDapperRepository.cs` | Đã có `ExecuteStoredProcAsync` |
| `DapperRepository` | `BuildingBlocks.Persistence/Repositories/DapperRepository.cs` | Implement bằng `CommandDefinition` + `CommandType.StoredProcedure` |
| Connection | `DefaultConnection` → `VI_DACDT` | Connection riêng, không share EF transaction |

### 2.5 Stored procedure

SP: `dbo.CoreMessaging_CreateNotification` (portal/DNN legacy).

```sql
SELECT OBJECT_ID('dbo.CoreMessaging_CreateNotification', 'P');
-- Phải khác NULL trên DB DefaultConnection
```

Code gọi SP qua `PheDuyetNotificationHelper` → `IDapperRepository.ExecuteStoredProcAsync`.

---

## 3. Phạm vi handler đã áp dụng

### 3.1 Nguyên tắc phạm vi

Chỉ sửa handler **thực sự thuộc UC22** — tức các handler được gọi từ `PheDuyetDispatch*` cho hành động **phản hồi/phê duyệt**, không refactor module khác.

### 3.1b Type không hỗ trợ `/duyet` (đã xác nhận khi test)

Các type **chỉ trình, không duyệt** — gọi `/duyet` sẽ lỗi `Loại phê duyệt '...' không hợp lệ`:

- `KeHoachLCNTChuanBiDauTu`
- `KHLCNTDuToanSanCo`
- `KHLCNTDuToanYeuCauRieng`
- `KeHoachTongTheLCNT`

→ Route qua `ToTrinhKhongDuyetCommand` ở `/trinh`, **không** có trong `PheDuyetDispatchDuyetCommand`.

### 3.1c Type khó tìm qua `danh-sach`

`DeXuatChuyenTiep` / `ThuyetMinhDuAn` bị comment khỏi `validTypes` trong `PheDuyetGetDanhSachQuery` → list thường trống. Vẫn duyệt được nếu biết `entityId`. **Test nên dùng** `DeXuatChuTruongMoi` hoặc `PheDuyetDuToan`.

### 3.2 Bảng handler theo dispatch

#### `PheDuyetDispatchDuyetCommand` → 22 handler

| Entity type | Handler file |
|-------------|--------------|
| `PheDuyetDuToan` | `PheDuyetDuToans/Commands/PheDuyetDuToanDuyetCommand.cs` |
| `HoSoMoiThauDienTu` | `HoSoMoiThauDienTus/Commands/HoSoMoiThauDienTuDuyetCommand.cs` |
| `PhanKhaiKinhPhi` | `PhanKhaiKinhPhis/Commands/PhanKhaiKinhPhiDuyetCommand.cs` |
| `BaoCaoKetQuaKhaoSat` | `BaoCaoKetQuaKhaoSats/Commands/BaoCaoKetQuaKhaoSatDuyetCommand.cs` |
| `QuyetDinhDieuChinh` | `QuyetDinhDieuChinhs/Commands/QuyetDinhDieuChinhDuyetCommand.cs` |
| `DeXuatChuTruongMoi` | `DeXuatChuTruongMois/Commands/DeXuatChuTruongMoiDuyetCommand.cs` |
| `DeXuatChuTruongChuyenTiep` | `DeXuatChuyenTiep/Commands/DeXuatChuyenTiepDuyetCommand.cs` |
| `DeXuatNhuCauKinhPhi` | `DeXuatNhuCauKinhPhi/Commands/DeXuatNhuCauKinhPhiDuyetCommand.cs` |
| `DeXuatNhuCauKinhPhiNam` | `DeXuatKinhPhiNam/Commands/DeXuatKinhPhiNamDuyetCommand.cs` |
| `ThuyetMinhDuAn` | `ThuyetMinhDuAn/Commands/ThuyetMinhDuAnDuyetCommand.cs` |
| `ToTrinhKetQuaGoiThau` | `ToTrinhKetQuaGoiThau/Commands/ToTrinhKetQuaGoiThauDuyetCommand.cs` |
| `ToTrinhThamDinhNhaThau` | `ToTrinhThamDinhNhaThau/Commands/ToTrinhThamDinhNhaThauDuyetCommand.cs` |
| `TrienKhaiKeHoachLCNT` | `TrienKhaiKeHoachLCNT/Commands/TrienKhaiKeHoachLCNTDuyetCommand.cs` |
| `KeHoachTrienKhaiHangMuc` | `KeHoachTrienKhaiHangMuc/Commands/KeHoachTrienKhaiHangMucDuyetCommand.cs` |
| `DuToanDauTu` | `DuToanDauTu/Commands/DuToanDauTuDuyetCommand.cs` |
| `PheDuyetKhaoSat`, `QuyetDinhKeHoachThue`, `ToTrinhKeHoach`, `QuyetDinhDuyetDuToan` | `ToTrinhPheDuyet/Commands/ToTrinhPheDuyetDuyetCommand.cs` |
| `ChuTruongLapKeHoach` | `ChuTruongLapKeHoach/Commands/ChuTruongLapKeHoachDuyetCommand.cs` |
| `KeHoachLuaChonNhaThauRutGon` | `KeHoachLuaChonNhaThauRutGon/Commands/KeHoachLuaChonNhaThauRutGonDuyetCommand.cs` |
| `ThoaThuanGiaoViec` | `ThoaThuanGiaoViec/Commands/ThoaThuanGiaoViecDuyetCommand.cs` |
| `QuyetDinhLapBanQLDA` | `QuyetDinhLapBanQLDAs/Commands/QuyetDinhLapBanQldaDuyetCommand.cs` |
| `ThanhLyHopDong` | `ThanhLyHopDongs/Commands/ThanhLyHopDongDuyetCommand.cs` |

#### `PheDuyetDispatchTraLaiCommand` → 20 handler

Tương tự pattern `*TraLaiCommand.cs` trong các folder entity tương ứng (danh sách đầy đủ trong `PheDuyetDispatchTraLaiCommand.cs`).

#### `PheDuyetDispatchTuChoiCommand` → 8 handler

| Entity type | Handler |
|-------------|---------|
| `PheDuyetDuToan` | `PheDuyetDuToanTuChoiCommand` |
| `HoSoMoiThauDienTu` | `HoSoMoiThauDienTuTuChoiCommand` |
| `PhanKhaiKinhPhi` | `PhanKhaiKinhPhiTuChoiCommand` |
| `QuyetDinhDieuChinh` | `QuyetDinhDieuChinhTuChoiCommand` |
| `PheDuyetKhaoSat` | `ToTrinhPheDuyetTrinhCommand` (*) |
| `DeXuatNhuCauKinhPhiNam` | `DeXuatKinhPhiNamTuChoiCommand` |
| `KeHoachLuaChonNhaThauRutGon` | `KeHoachLuaChonNhaThauRutGonTuChoiCommand` |
| `ThoaThuanGiaoViec` | `ThoaThuanGiaoViecTuChoiCommand` |

(\*) `PheDuyetKhaoSat` từ chối route sang `ToTrinhPheDuyetTrinhCommand` — cần xác nhận đúng nghiệp vụ; notification vẫn áp dụng nếu handler này đổi trạng thái thành công.

#### `PheDuyetDispatchChuyenCommand` → 2 handler

| Entity type | Handler |
|-------------|---------|
| `KeHoachLuaChonNhaThauRutGon` | `KeHoachLuaChonNhaThauRutGonChuyenCommand` |
| `ThoaThuanGiaoViec` | `ThoaThuanGiaoViecChuyenCommand` |

### 3.3 Handler ngoài dispatch (cân nhắc riêng)

| Handler | Controller | Ghi chú |
|---------|------------|---------|
| `HoSoDeXuatCapDoCnttPheDuyetCommand` | `HoSoDeXuatCapDoCnttController` | Luồng đa trạng thái (duyệt/từ chối/chuyển phòng) — **có thể thuộc UC22** nếu màn hình nằm trong “phê duyệt thông tin dự án” |
| `ToTrinhCoThamDinhThaoTacCommand` | `ToTrinhCoThamDinhController` | Tương tự |

**Khuyến nghị:** Phase 1 tập trung handler qua `QuanLyPheDuyetController`. Phase 2 mở rộng 2 handler trên nếu BA xác nhận.

→ **Đã làm Phase 3:** cả 2 handler đã map `TrangThaiTiepTheo` → `PheDuyetNotificationAction` và gọi helper sau save.

### 3.4 Không sửa

- `PheDuyetDispatch*Command` (chỉ route, không lưu)
- `*TrinhCommand` (trừ khi BA yêu cầu báo ngược cho người duyệt)
- Migration / `AppDbContextModelSnapshot.cs`
- Module ngoài phê duyệt (HopDong, GoiThau, ...)

---

## 4. Kiểu dữ liệu — đối chiếu schema thực tế

### 4.1 User ID

| Nguồn | Property | Kiểu C# | Kiểu SQL | Ghi chú |
|-------|----------|---------|----------|---------|
| `IUserProvider.Info` | `UserID` | `long` | — | **UserMaster.UserPortalId** |
| `PheDuyetHistory` | `NguoiXuLyId` | `long?` | `bigint` | Người xử lý từng bước |
| `PheDuyet` | `NguoiXuLyId` | `long?` | `bigint` | Người xử lý mới nhất |
| `PheDuyet` | `NguoiTrinhId` | `int?` | `int` | ⚠️ **Mismatch** với `UserPortalId` (long) |
| `PheDuyetListItemDto` | `NguoiTrinhId` | `long?` | — | DTO cast rộng hơn entity |

### 4.2 Tham số stored procedure (theo issue)

```sql
EXEC dbo.CoreMessaging_CreateNotification
    @NguoiNhanId = @NguoiNhanId,   -- long / bigint
    @NguoiGuiId   = @NguoiGuiId,   -- long / bigint
    @NotifyTypesId = 24,           -- int
    @Subject = N'Quản lý dự án',    -- nvarchar
    @Body = @Body,                 -- nvarchar
    @PortalId = 0;                  -- int
```

**Mapping Dapper khuyến nghị:**

| Parameter | DbType | Giá trị |
|-----------|--------|---------|
| `@NguoiNhanId` | `DbType.Int64` | `long` — UserPortalId người nhận |
| `@NguoiGuiId` | `DbType.Int64` | `long` — `_userProvider.Info.UserID` |
| `@NotifyTypesId` | `DbType.Int32` | `24` |
| `@Subject` | `DbType.String` | `"Quản lý dự án"` |
| `@Body` | `DbType.String` | Backend build (mục 6) |
| `@PortalId` | `DbType.Int32` | `0` |

**Không cast sang `int`** cho UserPortalId.

### 4.3 Cảnh báo `PheDuyet.NguoiTrinhId`

- Trong Application layer **không có** chỗ gán `NguoiTrinhId` (có thể do DB trigger trên `PheDuyet` / `PheDuyetHistory`).
- Kiểu `int?` có nguy cơ overflow nếu `UserPortalId > int.MaxValue`.
- **Cách resolve người nhận an toàn hơn:** query `PheDuyetHistory` bản ghi trình gần nhất (`TrangThai.Ma == "ĐTr"` hoặc mã `DaTrinh` tương ứng loại entity) → lấy `NguoiXuLyId` (`long?`).
- Fallback: `PheDuyet.NguoiTrinhId` (cast sang `long`), rồi `entity.CreatedBy` parse `long` + join `UserMaster`.

---

## 5. Thiết kế triển khai đề xuất

### 5.1 Không tạo `Application/Services`

Tuân thủ CQRS + rule dự án: dùng **static helper** trong Application (pattern tương tự `PheDuyetDispatchHelper`).

**File mới đề xuất:**

```
QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetNotificationHelper.cs
```

**Trách nhiệm:**

- `ResolveNguoiNhanIdAsync(...)` — tìm UserPortalId người trình
- `BuildBody(...)` — tạo nội dung theo `PheDuyetNotificationAction` + tên dự án + lý do
- `SendAsync(...)` — gọi Dapper SP sau khi save thành công

### 5.2 Mở rộng `IDapperRepository` (BuildingBlocks)

Thêm method (cần sửa cả interface + `DapperRepository`):

```csharp
Task<int> ExecuteStoredProcAsync(
    string procName,
    object? param = null,
    string? connectionName = "DefaultConnection",
    CancellationToken cancellationToken = default);
```

Implement bằng `connection.ExecuteAsync(..., commandType: CommandType.StoredProcedure)`.

> **Lý do:** Issue yêu cầu `ExecuteAsync` + `CommandType.StoredProcedure`; hiện tại repo chỉ có `QueryStoredProcAsync`.

### 5.3 Thứ tự xử lý trong handler (bắt buộc)

```csharp
// 1. Validate request (giữ nguyên)
// 2. Nghiệp vụ hiện tại (giữ nguyên)
// 3. Lưu
var affected = await _unitOfWork.SaveChangesAsync(cancellationToken);

// 4. Notification — CHỈ khi affected > 0 (hoặc không throw)
if (affected > 0) {
    await PheDuyetNotificationHelper.TrySendAsync(..., cancellationToken);
}

// 5. Return
return affected;
```

### 5.4 Xử lý lỗi notification

```csharp
try {
    await _dapper.ExecuteStoredProcAsync(...);
} catch (Exception ex) {
    _logger.Error(ex, "UC22 notification failed. Entity={EntityName}, EntityId={EntityId}, Action={Action}",
        entityName, entityId, action);
    // KHÔNG throw — nghiệp vụ đã commit
}
```

Wrapper `TrySendAsync` gom logic trên; **không nuốt im lặng**.

### 5.5 Command / request — có cần thêm property?

Issue đề xuất:

```csharp
public long NguoiGuiId { get; set; }
public long NguoiNhanId { get; set; }
public string? Body { get; set; }
```

**Khuyến nghị thực tế (bảo mật hơn):**

| Property | Nguồn | Ghi chú |
|----------|-------|---------|
| `NguoiGuiId` | `_userProvider.Info.UserID` | **Không tin client** |
| `NguoiNhanId` | Resolve server-side (mục 4.3) | **Không tin client** |
| `Body` | `PheDuyetNotificationHelper.BuildBody(...)` | Backend build; `NoiDung` từ request chỉ dùng làm **phần lý do** trong body |

→ **Không bắt buộc** thêm property vào command record nếu resolve server-side. Nếu BA/FE cần truyền `NguoiNhanId` override (ví dụ chuyển phòng chỉ định user), thêm optional vào dispatch model WebApi + validate `> 0`.

### 5.6 Validation

| Rule | Xử lý |
|------|-------|
| `NguoiNhanId <= 0` | Skip notification + log warning (không gọi SP) |
| `NguoiGuiId <= 0` | Throw `ManagedException` trước save (session invalid) |
| `Body` quá dài | Cắt theo max length SP (cần BA/DBA xác nhận, tạm `500` hoặc `2000` ký tự) |
| Save thất bại | Không gọi SP |
| Retry request | SP **không có** cơ chế chống trùng trong repo → **mỗi lần handler chạy thành công sẽ tạo 1 notification**. Idempotency: ngoài scope trừ khi DBA bổ sung; giảm rủi ro bằng validate trạng thái chặt (đã có). |

---

## 6. Template nội dung Body

```csharp
public enum PheDuyetNotificationAction {
    Duyet,
    TuChoi,
    TraLai,
    Chuyen,
    PhatHanh
}

// Ví dụ build:
// action=Duyet, tenDuAn="DA-001", tenLoai="Đề xuất chủ trương chuyển tiếp"
// → "Thông tin dự án DA-001 (Đề xuất chủ trương chuyển tiếp) đã được phê duyệt."

// action=TraLai, lyDo="Thiếu hồ sơ"
// → "Thông tin dự án DA-001 cần cập nhật và gửi lại. Nội dung: Thiếu hồ sơ."
```

Dùng `PheDuyetEntityNames.GetDescriptionFromName()` (đã có extension) cho tên loại tài liệu.

**`TenDuAn`:** Include `DuAn` khi load entity, hoặc join `DuAn.TenDuAn` từ `DuAnId`.

---

## 7. Pseudocode helper

```csharp
internal static class PheDuyetNotificationHelper
{
    private const string StoredProcedure = "dbo.CoreMessaging_CreateNotification";
    private const string Subject = "Quản lý dự án";
    private const int NotifyTypesId = 24;
    private const int PortalId = 0;

    internal static async Task TrySendAsync(
        IDapperRepository dapper,
        ILogger logger,
        long nguoiGuiId,
        long nguoiNhanId,
        string body,
        Guid entityId,
        string entityName,
        PheDuyetNotificationAction action,
        CancellationToken cancellationToken)
    {
        if (nguoiNhanId <= 0) {
            logger.Warning("Skip notification: invalid NguoiNhanId. EntityId={EntityId}", entityId);
            return;
        }

        var parameters = new DynamicParameters();
        parameters.Add("@NguoiNhanId", nguoiNhanId, DbType.Int64);
        parameters.Add("@NguoiGuiId", nguoiGuiId, DbType.Int64);
        parameters.Add("@NotifyTypesId", NotifyTypesId, DbType.Int32);
        parameters.Add("@Subject", Subject, DbType.String);
        parameters.Add("@Body", body ?? string.Empty, DbType.String);
        parameters.Add("@PortalId", PortalId, DbType.Int32);

        try {
            await dapper.ExecuteStoredProcAsync(
                StoredProcedure, parameters, cancellationToken: cancellationToken);
        } catch (Exception ex) {
            logger.Error(ex,
                "CoreMessaging_CreateNotification failed. Entity={EntityName}, EntityId={EntityId}, Action={Action}",
                entityName, entityId, action);
        }
    }
}
```

---

## 8. Hướng dẫn code từng bước (tham chiếu — đã làm xong)

> Các bước dưới đây là **as-built guide** để maintain / review. Code đã merge vào source. Mỗi bước kết thúc bằng `dotnet build` khi triển khai lần đầu.

### Bước 1 — Mở rộng `IDapperRepository`

**File:** `BuildingBlocks/src/BuildingBlocks.Domain/Interfaces/IDapperRepository.cs`

Thêm method (giữ nguyên 2 method cũ):

```csharp
Task<int> ExecuteStoredProcAsync(
    string procName,
    object? param = null,
    string? connectionName = "DefaultConnection",
    CancellationToken cancellationToken = default);
```

---

### Bước 2 — Implement `ExecuteStoredProcAsync` trong `DapperRepository`

**File:** `BuildingBlocks/src/BuildingBlocks.Persistence/Repositories/DapperRepository.cs`

Thêm method sau `QueryAsync`:

```csharp
public async Task<int> ExecuteStoredProcAsync(
    string procName,
    object? param = null,
    string? connectionName = "DefaultConnection",
    CancellationToken cancellationToken = default)
{
    using var connection = connectionFactory.CreateConnection(connectionName ?? "DefaultConnection");
    return await connection.ExecuteAsync(
        new CommandDefinition(
            procName,
            param,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
}
```

**Verify:** `dotnet build BuildingBlocks/BuildingBlocks.sln`

---

### Bước 3 — Tạo enum hành động notification

**File mới:** `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetNotificationAction.cs`

```csharp
namespace QLDA.Application.QuanLyPheDuyet.Commands;

internal enum PheDuyetNotificationAction
{
    Duyet,
    TuChoi,
    TraLai,
    Chuyen,
    PhatHanh
}
```

---

### Bước 4 — Tạo `PheDuyetNotificationHelper` (file chính)

**File mới:** `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetNotificationHelper.cs`

```csharp
using System.Data;
using BuildingBlocks.Domain.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using Serilog;

namespace QLDA.Application.QuanLyPheDuyet.Commands;

/// <summary>
/// UC22 — gọi dbo.CoreMessaging_CreateNotification sau khi lưu nghiệp vụ phê duyệt thành công.
/// </summary>
internal static class PheDuyetNotificationHelper
{
    private const string StoredProcedure = "dbo.CoreMessaging_CreateNotification";
    private const string Subject = "Quản lý dự án";
    private const int NotifyTypesId = 24;
    private const int PortalId = 0;
    private const int MaxBodyLength = 2000;

    /// <summary>
    /// Resolve UserPortalId người đã trình (người nhận thông báo).
    /// Ưu tiên: PheDuyetHistory (ĐTr) → PheDuyet.NguoiTrinhId → CreatedBy.
    /// </summary>
    internal static async Task<long> ResolveNguoiNhanIdAsync(
        IRepository<PheDuyetHistory, Guid> historyRepo,
        IRepository<PheDuyet, Guid> pheDuyetRepo,
        string entityName,
        Guid entityId,
        string? createdBy,
        CancellationToken cancellationToken)
    {
        var fromHistory = await historyRepo.GetQueryableSet()
            .AsNoTracking()
            .Include(h => h.TrangThai)
            .Where(h => h.EntityId == entityId
                        && h.EntityName == entityName
                        && h.TrangThai != null
                        && h.TrangThai.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh)
            .OrderByDescending(h => h.NgayXuLy)
            .Select(h => h.NguoiXuLyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (fromHistory is > 0)
            return fromHistory.Value;

        var pheDuyet = await pheDuyetRepo.GetQueryableSet()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.EntityId == entityId && p.EntityName == entityName, cancellationToken);

        if (pheDuyet?.NguoiTrinhId is > 0)
            return pheDuyet.NguoiTrinhId.Value;

        if (long.TryParse(createdBy, out var createdUserId) && createdUserId > 0)
            return createdUserId;

        return 0;
    }

    internal static string BuildBody(
        PheDuyetNotificationAction action,
        string? tenDuAn,
        string entityName,
        string? lyDo = null)
    {
        var ten = string.IsNullOrWhiteSpace(tenDuAn) ? "—" : tenDuAn.Trim();
        var loai = entityName.GetDescriptionFromName();

        var body = action switch
        {
            PheDuyetNotificationAction.Duyet =>
                $"Thông tin dự án {ten} ({loai}) đã được phê duyệt.",
            PheDuyetNotificationAction.TuChoi =>
                $"Thông tin dự án {ten} ({loai}) bị từ chối. Lý do: {lyDo?.Trim() ?? "—"}.",
            PheDuyetNotificationAction.TraLai =>
                $"Thông tin dự án {ten} ({loai}) cần cập nhật và gửi lại. Nội dung: {lyDo?.Trim() ?? "—"}.",
            PheDuyetNotificationAction.Chuyen =>
                $"Thông tin dự án {ten} ({loai}) đã được chuyển xử lý.",
            PheDuyetNotificationAction.PhatHanh =>
                $"Thông tin dự án {ten} ({loai}) đã được phát hành.",
            _ => $"Thông tin dự án {ten} ({loai}) đã được cập nhật."
        };

        return body.Length <= MaxBodyLength ? body : body[..MaxBodyLength];
    }

    /// <summary>
    /// Gọi SP; lỗi chỉ log, không throw (nghiệp vụ đã lưu).
    /// </summary>
    internal static async Task TrySendAsync(
        IDapperRepository dapper,
        long nguoiGuiId,
        long nguoiNhanId,
        string body,
        Guid entityId,
        string entityName,
        PheDuyetNotificationAction action,
        CancellationToken cancellationToken)
    {
        if (nguoiNhanId <= 0)
        {
            Log.Warning(
                "UC22 skip notification: NguoiNhanId invalid. Entity={EntityName}, EntityId={EntityId}",
                entityName, entityId);
            return;
        }

        if (nguoiGuiId <= 0)
        {
            Log.Warning(
                "UC22 skip notification: NguoiGuiId invalid. Entity={EntityName}, EntityId={EntityId}",
                entityName, entityId);
            return;
        }

        var parameters = new DynamicParameters();
        parameters.Add("@NguoiNhanId", nguoiNhanId, DbType.Int64);
        parameters.Add("@NguoiGuiId", nguoiGuiId, DbType.Int64);
        parameters.Add("@NotifyTypesId", NotifyTypesId, DbType.Int32);
        parameters.Add("@Subject", Subject, DbType.String);
        parameters.Add("@Body", body ?? string.Empty, DbType.String);
        parameters.Add("@PortalId", PortalId, DbType.Int32);

        try
        {
            await dapper.ExecuteStoredProcAsync(
                StoredProcedure,
                parameters,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "UC22 CoreMessaging_CreateNotification failed. Entity={EntityName}, EntityId={EntityId}, Action={Action}",
                entityName, entityId, action);
        }
    }

    /// <summary>
    /// Shortcut: resolve người nhận + build body + gửi — gọi sau SaveChangesAsync.
    /// </summary>
    internal static async Task NotifyAfterSaveAsync(
        IDapperRepository dapper,
        IRepository<PheDuyetHistory, Guid> historyRepo,
        IRepository<PheDuyet, Guid> pheDuyetRepo,
        long nguoiGuiId,
        string entityName,
        Guid entityId,
        Guid? duAnId,
        string? tenDuAn,
        string? createdBy,
        PheDuyetNotificationAction action,
        string? lyDo,
        CancellationToken cancellationToken)
    {
        var nguoiNhanId = await ResolveNguoiNhanIdAsync(
            historyRepo, pheDuyetRepo, entityName, entityId, createdBy, cancellationToken);

        var body = BuildBody(action, tenDuAn, entityName, lyDo);

        await TrySendAsync(
            dapper, nguoiGuiId, nguoiNhanId, body,
            entityId, entityName, action, cancellationToken);
    }
}
```

**Lưu ý resolve `ĐTr`:** Handler dùng `TrangThaiPheDuyetCodes` khác nhau theo loại entity (ví dụ `DuToan.DaTrinh`, `HoSoMoiThauDienTu.DaTrinh`). Nếu `ResolveNguoiNhanIdAsync` không tìm được qua `DeXuatMacDinh.DaTrinh`, bổ sung tham số `string maTrangThaiTrinh` vào helper (xem Bước 4b).

---

### Bước 4b — (Tùy chọn) Tham số hóa mã trạng thái trình

Đổi signature `ResolveNguoiNhanIdAsync` để nhận `maTrangThaiTrinh` (mặc định `"ĐTr"`):

```csharp
internal static async Task<long> ResolveNguoiNhanIdAsync(
    ...
    string maTrangThaiTrinh = TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh,
    ...)
{
    ...
    && h.TrangThai.Ma == maTrangThaiTrinh)
    ...
}
```

Khi gọi từ `PheDuyetDuToanDuyetCommand`:

```csharp
maTrangThaiTrinh: TrangThaiPheDuyetCodes.DuToan.DaTrinh
```

---

### Bước 5 — Pilot: sửa `DeXuatChuyenTiepDuyetCommand`

**File:** `QLDA.Application/DeXuatChuyenTiep/Commands/DeXuatChuyenTiepDuyetCommand.cs`

#### 5.1 Xóa code WIP (dòng 74–87) và field `_PheDuyetRepository` nếu chỉ dùng cho stub.

#### 5.2 Thêm using

```csharp
using BuildingBlocks.Domain.Interfaces;
using QLDA.Application.QuanLyPheDuyet.Commands;
```

#### 5.3 Thêm field + inject constructor

```csharp
private readonly IDapperRepository _dapper;
private readonly IRepository<PheDuyet, Guid> _pheDuyetRepo;

// trong constructor:
_dapper = serviceProvider.GetRequiredService<IDapperRepository>();
_pheDuyetRepo = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
```

#### 5.4 Load `TenDuAn` — đổi query entity

```csharp
// Trước:
var entity = await _repository.GetQueryableSet()
    .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

// Sau:
var entity = await _repository.GetQueryableSet()
    .Include(e => e.DuAn)
    .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
```

#### 5.5 Thay `return await _unitOfWork.SaveChangesAsync` bằng block sau save

```csharp
await _historyRepository.AddAsync(history, cancellationToken);

var affected = await _unitOfWork.SaveChangesAsync(cancellationToken);

if (affected > 0)
{
    await PheDuyetNotificationHelper.NotifyAfterSaveAsync(
        _dapper,
        _historyRepository,
        _pheDuyetRepo,
        _userProvider.Info.UserID,
        PheDuyetEntityNames.DeXuatChuTruongChuyenTiep,
        entity.Id,
        entity.DuAnId,
        entity.DuAn?.TenDuAn,
        entity.CreatedBy,
        PheDuyetNotificationAction.Duyet,
        lyDo: null,
        cancellationToken);
}

return affected;
```

**Handler hoàn chỉnh (phần cuối `Handle`) — tham chiếu:**

```csharp
entity.TrangThaiId = trangThaiDaDuyet.Id;

var history = new PheDuyetHistory {
    Id = Guid.NewGuid(),
    EntityName = PheDuyetEntityNames.DeXuatChuTruongChuyenTiep,
    EntityId = entity.Id,
    DuAnId = entity.DuAnId,
    NguoiXuLyId = _userProvider.Info.UserID,
    TrangThaiId = trangThaiDaDuyet.Id,
    NgayXuLy = DateTimeOffset.UtcNow
};

await _historyRepository.AddAsync(history, cancellationToken);

var affected = await _unitOfWork.SaveChangesAsync(cancellationToken);

if (affected > 0) {
    await PheDuyetNotificationHelper.NotifyAfterSaveAsync(
        _dapper, _historyRepository, _pheDuyetRepo,
        _userProvider.Info.UserID,
        PheDuyetEntityNames.DeXuatChuTruongChuyenTiep,
        entity.Id, entity.DuAnId, entity.DuAn?.TenDuAn, entity.CreatedBy,
        PheDuyetNotificationAction.Duyet, null, cancellationToken);
}

return affected;
```

**Verify:** `dotnet build QLDA.sln`

---

### Bước 6 — Pilot: sửa `DeXuatChuyenTiepTraLaiCommand`

**File:** `QLDA.Application/DeXuatChuyenTiep/Commands/DeXuatChuyenTiepTraLaiCommand.cs`

Lặp Bước 5 với khác biệt:

| Mục | Giá trị |
|-----|---------|
| `PheDuyetNotificationAction` | `TraLai` |
| `lyDo` | `request.NoiDung` |
| Include `DuAn` | Có |

```csharp
if (affected > 0) {
    await PheDuyetNotificationHelper.NotifyAfterSaveAsync(
        _dapper, _historyRepository, _pheDuyetRepo,
        _userProvider.Info.UserID,
        PheDuyetEntityNames.DeXuatChuTruongChuyenTiep,
        entity.Id, entity.DuAnId, entity.DuAn?.TenDuAn, entity.CreatedBy,
        PheDuyetNotificationAction.TraLai, request.NoiDung, cancellationToken);
}
```

---

### Bước 7 — Pattern rollout cho handler còn lại

Mỗi `*DuyetCommand` / `*TraLaiCommand` / `*TuChoiCommand` / `*ChuyenCommand` áp dụng **cùng checklist**:

```
[ ] 1. using BuildingBlocks.Domain.Interfaces
[ ] 2. using QLDA.Application.QuanLyPheDuyet.Commands
[ ] 3. Thêm field: IDapperRepository _dapper, IRepository<PheDuyet, Guid> _pheDuyetRepo
[ ] 4. Inject trong constructor (IServiceProvider)
[ ] 5. Include(e => e.DuAn) khi load entity (nếu có navigation DuAn)
[ ] 6. Đổi return SaveChanges → affected + block notification
[ ] 7. entityName = hằng PheDuyetEntityNames.* đúng với handler
[ ] 8. action = Duyet | TraLai | TuChoi | Chuyen
[ ] 9. lyDo = request.NoiDung (TraLai/TuChoi) hoặc null (Duyet/Chuyen)
[ ] 10. KHÔNG sửa logic validate / đổi trạng thái / history phía trên
```

**Snippet copy-paste** (thay `ENTITY_NAME`, `ACTION`, `lyDo`):

```csharp
var affected = await _unitOfWork.SaveChangesAsync(cancellationToken);

if (affected > 0)
{
    await PheDuyetNotificationHelper.NotifyAfterSaveAsync(
        _dapper,
        _historyRepository,
        _pheDuyetRepo,
        _userProvider.Info.UserID,
        PheDuyetEntityNames.ENTITY_NAME,   // ← đổi
        entity.Id,
        entity.DuAnId,
        entity.DuAn?.TenDuAn,              // hoặc tenDuAn đã load
        entity.CreatedBy,
        PheDuyetNotificationAction.ACTION, // ← đổi
        lyDo,                              // ← null hoặc request.NoiDung
        cancellationToken);
}

return affected;
```

**Handler đặc biệt `ToTrinhPheDuyetDuyetCommand`:** `entityName` = `request.Loai` (không phải hằng cố định).

**Handler `HoSoDeXuatCapDoCnttPheDuyetCommand`:** map `TrangThaiTiepTheo` → action:

| `TrangThaiTiepTheo` (mã) | Action |
|--------------------------|--------|
| `ĐD` | `Duyet` |
| `TC` | `TuChoi` |
| `TL` | `TraLai` |
| `ĐC` | `Chuyen` |

---

### Bước 8 — Danh sách file rollout (Phase 2)

#### `*DuyetCommand` (22 file)

```
QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanDuyetCommand.cs
QLDA.Application/HoSoMoiThauDienTus/Commands/HoSoMoiThauDienTuDuyetCommand.cs
QLDA.Application/PhanKhaiKinhPhis/Commands/PhanKhaiKinhPhiDuyetCommand.cs
QLDA.Application/BaoCaoKetQuaKhaoSats/Commands/BaoCaoKetQuaKhaoSatDuyetCommand.cs
QLDA.Application/QuyetDinhDieuChinhs/Commands/QuyetDinhDieuChinhDuyetCommand.cs
QLDA.Application/DeXuatChuTruongMois/Commands/DeXuatChuTruongMoiDuyetCommand.cs
QLDA.Application/DeXuatChuyenTiep/Commands/DeXuatChuyenTiepDuyetCommand.cs          ← pilot
QLDA.Application/DeXuatNhuCauKinhPhi/Commands/DeXuatNhuCauKinhPhiDuyetCommand.cs
QLDA.Application/DeXuatKinhPhiNam/Commands/DeXuatKinhPhiNamDuyetCommand.cs
QLDA.Application/ThuyetMinhDuAn/Commands/ThuyetMinhDuAnDuyetCommand.cs
QLDA.Application/ToTrinhKetQuaGoiThau/Commands/ToTrinhKetQuaGoiThauDuyetCommand.cs
QLDA.Application/ToTrinhThamDinhNhaThau/Commands/ToTrinhThamDinhNhaThauDuyetCommand.cs
QLDA.Application/TrienKhaiKeHoachLCNT/Commands/TrienKhaiKeHoachLCNTDuyetCommand.cs
QLDA.Application/KeHoachTrienKhaiHangMuc/Commands/KeHoachTrienKhaiHangMucDuyetCommand.cs
QLDA.Application/DuToanDauTu/Commands/DuToanDauTuDuyetCommand.cs
QLDA.Application/ToTrinhPheDuyet/Commands/ToTrinhPheDuyetDuyetCommand.cs
QLDA.Application/ChuTruongLapKeHoach/Commands/ChuTruongLapKeHoachDuyetCommand.cs
QLDA.Application/KeHoachLuaChonNhaThauRutGon/Commands/KeHoachLuaChonNhaThauRutGonDuyetCommand.cs
QLDA.Application/ThoaThuanGiaoViec/Commands/ThoaThuanGiaoViecDuyetCommand.cs
QLDA.Application/QuyetDinhLapBanQLDAs/Commands/QuyetDinhLapBanQldaDuyetCommand.cs
QLDA.Application/ThanhLyHopDongs/Commands/ThanhLyHopDongDuyetCommand.cs
```

#### `*TraLaiCommand` (20 file)

```
QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanTraLaiCommand.cs
QLDA.Application/HoSoMoiThauDienTus/Commands/HoSoMoiThauDienTuTraLaiCommand.cs
QLDA.Application/PhanKhaiKinhPhis/Commands/PhanKhaiKinhPhiTraLaiCommand.cs
QLDA.Application/BaoCaoKetQuaKhaoSats/Commands/BaoCaoKetQuaKhaoSatTraLaiCommand.cs
QLDA.Application/QuyetDinhDieuChinhs/Commands/QuyetDinhDieuChinhTraLaiCommand.cs
QLDA.Application/DeXuatChuTruongMois/Commands/DeXuatChuTruongMoiTraLaiCommand.cs
QLDA.Application/DeXuatChuyenTiep/Commands/DeXuatChuyenTiepTraLaiCommand.cs       ← pilot
QLDA.Application/DeXuatNhuCauKinhPhi/Commands/DeXuatNhuCauKinhPhiTraLaiCommand.cs
QLDA.Application/DeXuatKinhPhiNam/Commands/DeXuatKinhPhiNamTraLaiCommand.cs
QLDA.Application/ThuyetMinhDuAn/Commands/ThuyetMinhDuAnTraLaiCommand.cs
QLDA.Application/ToTrinhKetQuaGoiThau/Commands/ToTrinhKetQuaGoiThauTraLaiCommand.cs
QLDA.Application/ToTrinhThamDinhNhaThau/Commands/ToTrinhThamDinhNhaThauTraLaiCommand.cs
QLDA.Application/TrienKhaiKeHoachLCNT/Commands/TrienKhaiKeHoachLCNTTraLaiCommand.cs
QLDA.Application/KeHoachTrienKhaiHangMuc/Commands/KeHoachTrienKhaiHangMucTraLaiCommand.cs
QLDA.Application/DuToanDauTu/Commands/DuToanDauTuTraLaiCommand.cs
QLDA.Application/ToTrinhPheDuyet/Commands/ToTrinhPheDuyetTraLaiCommand.cs
QLDA.Application/ChuTruongLapKeHoach/Commands/ChuTruongLapKeHoachTraLaiCommand.cs
QLDA.Application/QuyetDinhLapBanQLDAs/Commands/QuyetDinhLapBanQldaTraLaiCommand.cs
QLDA.Application/ThanhLyHopDongs/Commands/ThanhLyHopDongTraLaiCommand.cs
```

#### `*TuChoiCommand` (8 file)

```
QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanTuChoiCommand.cs
QLDA.Application/HoSoMoiThauDienTus/Commands/HoSoMoiThauDienTuTuChoiCommand.cs
QLDA.Application/PhanKhaiKinhPhis/Commands/PhanKhaiKinhPhiTuChoiCommand.cs
QLDA.Application/QuyetDinhDieuChinhs/Commands/QuyetDinhDieuChinhTuChoiCommand.cs
QLDA.Application/ToTrinhPheDuyet/Commands/ToTrinhPheDuyetTrinhCommand.cs         ← route từ chối KhaoSat
QLDA.Application/DeXuatKinhPhiNam/Commands/DeXuatKinhPhiNamTuChoiCommand.cs
QLDA.Application/KeHoachLuaChonNhaThauRutGon/Commands/KeHoachLuaChonNhaThauRutGonTuChoiCommand.cs
QLDA.Application/ThoaThuanGiaoViec/Commands/ThoaThuanGiaoViecTuChoiCommand.cs
```

#### `*ChuyenCommand` (2 file)

```
QLDA.Application/KeHoachLuaChonNhaThauRutGon/Commands/KeHoachLuaChonNhaThauRutGonChuyenCommand.cs
QLDA.Application/ThoaThuanGiaoViec/Commands/ThoaThuanGiaoViecChuyenCommand.cs
```

---

### Bước 9 — Không cần sửa WebApi / Command record

Theo thiết kế server-side resolve:

- **Không** thêm `NguoiGuiId` / `NguoiNhanId` / `Body` vào command record
- **Không** sửa `QuanLyPheDuyetController` (API contract giữ nguyên)
- `NoiDung` hiện có trong `TraLaiModel` / `TuChoiModel` đủ làm `lyDo` cho body

Nếu sau này cần override `NguoiNhanId` (luồng chuyển phòng), thêm optional `long? NguoiNhanId` vào `TuChoiModel` hoặc model chuyển — **ngoài scope phase 1**.

---

### Bước 10 — Build & smoke test

```bash
dotnet build SER.sln
```

**Smoke test API (đã chạy thành công 2026-07-10):**

```http
POST /api/phe-duyet/DeXuatChuTruongMoi/{entityId}/duyet
Authorization: Bearer ...
```

**Kiểm tra log:** không có `UC22 CoreMessaging_CreateNotification failed` / `UC22 skip notification`.

**SQL xác nhận SP tồn tại:**

```sql
SELECT OBJECT_ID('dbo.CoreMessaging_CreateNotification', 'P');
```

> Không dùng `KeHoachLCNTChuanBiDauTu` để test `/duyet` — type này chỉ trình, không duyệt.
---

### Bước 11 — Phase 3 (handler ngoài dispatch, nếu BA confirm)

Áp dụng cùng pattern cho:

- `QLDA.Application/HoSoDeXuatCapDoCntts/Commands/HoSoDeXuatCapDoCnttPheDuyetCommand.cs`
- `QLDA.Application/ToTrinhCoThamDinh/Commands/ToTrinhCoThamDinhThaoTacCommand.cs`

Map action theo `request.TrangThaiTiepTheo` (bảng ở Bước 7).

---

## 9. Kế hoạch triển khai theo phase (đã hoàn thành)

| Phase | Nội dung | Trạng thái |
|-------|----------|------------|
| **0** | Hạ tầng Dapper + helper | Done |
| **1** | Pilot `DeXuatChuyenTiep` Duyet + TraLai | Done |
| **2** | Rollout ~50 handler | Done |
| **3** | `HoSoDeXuatCapDoCntt` + `ToTrinhCoThamDinh` | Done |

Chi tiết bước code (tham chiếu khi maintain): xem **mục 8** bên trên.

---

## 10. Hướng dẫn test (đã chạy smoke)

### 10.1 Chọn type + lấy id

```http
GET /api/phe-duyet/types
GET /api/phe-duyet/danh-sach?type=DeXuatChuTruongMoi&trangThai=ĐTr&pageIndex=0&pageSize=10
```

- Dùng field **`entityId`** (không dùng `id` của bảng `PheDuyet`)
- Ưu tiên type: `DeXuatChuTruongMoi`, `PheDuyetDuToan`
- **Không** test `/duyet` với `KeHoachLCNTChuanBiDauTu` (không hỗ trợ duyệt)

### 10.2 Duyệt

```http
POST /api/phe-duyet/DeXuatChuTruongMoi/{entityId}/duyet
Authorization: Bearer {token}
```

**Kỳ vọng API:**

```json
{ "result": true, "errorMessage": "", "dataResult": 2, "statusCode": 200 }
```

`dataResult: 2` = thường là entity + history đã ghi.

### 10.3 Cùng token trình + duyệt

- Code **không** cấm cùng user tự duyệt.
- Vẫn cần quyền: `QLDA_LDDV` **hoặc** `LanhDaoPhuTrachId == UserId` **hoặc** KHTC bypass.
- JWT claim `UserId` → `_userProvider.Info.UserID` (UserPortalId) → `@NguoiGuiId`.
- Nếu cùng user: `@NguoiGuiId == @NguoiNhanId` vẫn OK (tự nhận thông báo).

### 10.4 Đọc log API

Log file: `QLDA.WebApi/logs/service-YYYYMMDD.log` hoặc console `dotnet run`.

Sau duyệt thành công, kỳ vọng thấy:

1. `SaveChanges completed ... with 2 entities written`
2. Query resolve người nhận:

```sql
SELECT TOP(1) [p].[NguoiXuLyId]
FROM [PheDuyetHistory] ...
WHERE EntityName = 'DeXuatChuTruongMoi' AND [d].[Ma] = N'ĐTr'
```

3. `HTTP POST .../duyet responded 200`

**Log UC22:**

| Log | Ý nghĩa |
|-----|---------|
| `UC22 skip notification: NguoiNhanId invalid` | Không resolve được người nhận — SP không gọi |
| `UC22 CoreMessaging_CreateNotification failed` | SP lỗi — nghiệp vụ vẫn đã lưu |
| Không có 2 dòng trên | SP đã gọi và không throw |

> Đường thành công **không** ghi log “sent OK” (chỉ log khi skip/lỗi). Gọi Dapper cũng không hiện trong log EF.

### 10.5 Smoke test đã chạy (2026-07-10)

| Mục | Kết quả |
|-----|---------|
| `POST .../DeXuatChuTruongMoi/08DED35A-.../duyet` | `result: true`, `dataResult: 2` |
| SaveChanges | 2 entities (entity + history) |
| Resolve `NguoiNhanId` từ History `ĐTr` | Có chạy trong log |
| Log `UC22 ... failed` / `skip` | **Không có** → SP không throw |

### 10.6 Checklist case

| # | Case | Kỳ vọng |
|---|------|---------|
| 1 | Duyệt thành công | API 200, SP gọi (không log error) |
| 2 | Duyệt khi trạng thái sai | Exception trước save, không gọi SP |
| 3 | Type không hỗ trợ duyệt (`KeHoachLCNTChuanBiDauTu`) | `Loại phê duyệt '...' không hợp lệ` |
| 4 | `NguoiNhanId` không resolve | Save OK, log warning skip |
| 5 | SP throw sau save | API vẫn 200, log Error |
| 6 | Tra lại có `NoiDung` | Body chứa lý do |
| 7 | Retry sau khi đã duyệt | Validation fail, không duplicate |

### 10.7 Build

```bash
dotnet build SER.sln
```

---

## 11. Acceptance criteria (checklist)

- [x] Command nghiệp vụ lưu đúng như trước
- [x] SP gọi **sau** `SaveChangesAsync` thành công
- [x] Dùng Dapper + `CommandType.StoredProcedure`
- [x] `NotifyTypesId = 24`, `Subject = "Quản lý dự án"`, `PortalId = 0`
- [x] `NguoiGuiId` = user đăng nhập (`UserPortalId`), không đảo ID
- [x] Body phù hợp từng hành động, build backend
- [x] Không gửi khi nghiệp vụ thất bại
- [x] Log đầy đủ khi notification lỗi / skip
- [x] Không sửa migration / snapshot
- [x] Build solution thành công (`SER.sln`)
- [x] Phạm vi UC22 / QuanLyPheDuyet dispatch handlers (+ Phase 3)
- [x] Smoke test duyệt `DeXuatChuTruongMoi` thành công
- [ ] Xác nhận notification hiện trên UI portal (cần BA/DBA kiểm tra bảng DNN)

---

## 12. Rủi ro & câu hỏi còn mở

| # | Câu hỏi / rủi ro | Trạng thái |
|---|------------------|------------|
| 1 | SP nằm cùng `DefaultConnection` (`VI_DACDT`)? | Smoke: không throw — cần confirm insert portal |
| 2 | Max length `@Body`? | Tạm cắt 2000 ký tự trong helper |
| 3 | `PheDuyet.NguoiTrinhId` (int) vs UserPortalId (long) | Resolve ưu tiên History `NguoiXuLyId` (long) |
| 4 | Migration fix `NguoiTrinhId` → long? | Ngoài scope |
| 5 | Luồng `chuyen` — notify ai? | Hiện notify người trình (cùng resolve) |
| 6 | Notification khi `trinh` lại sau `tra-lai`? | Ngoài scope (không áp dụng `*TrinhCommand`) |
| 7 | Success path không log “sent OK” | Có thể bổ sung `Log.Information` nếu cần audit |

---

## 13. File đã thay đổi (tóm tắt)

| File | Thay đổi |
|------|----------|
| `BuildingBlocks.Domain/Interfaces/IDapperRepository.cs` | +`ExecuteStoredProcAsync` |
| `BuildingBlocks.Persistence/Repositories/DapperRepository.cs` | Implement execute |
| `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetNotificationAction.cs` | **Mới** |
| `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetNotificationHelper.cs` | **Mới** |
| `QLDA.Application/**/**{Duyet,TraLai,TuChoi,Chuyen}Command.cs` | Gọi helper sau save (~51 files) |
| `HoSoDeXuatCapDoCnttPheDuyetCommand` / `ToTrinhCoThamDinhThaoTacCommand` | Phase 3 |

**Không sửa:** `QLDA.Migrator/**`, `AppDbContextModelSnapshot.cs`, `QuanLyPheDuyetController` (API contract giữ nguyên)

---

## 14. Tham chiếu source

- Dispatch hub: `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatch*.cs`
- Helper: `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetNotificationHelper.cs`
- Controller: `QLDA.WebApi/Controllers/QuanLyPheDuyetController.cs`
- User context: `BuildingBlocks.Domain/DTOs/UserInfo.cs` (`UserID` = `UserPortalId` từ JWT claim `UserId`)
- Bảng tổng hợp: `QLDA.Domain/Entities/PheDuyet.cs`, `PheDuyetHistory.cs`
- Log runtime: `QLDA.WebApi/logs/service-YYYYMMDD.log`
- Journal dispatch: `docs/journals/260515-0749-quyet-dinh-dieu-chinh-dong-bo-phe-duyet-dispatch-hub.md`
