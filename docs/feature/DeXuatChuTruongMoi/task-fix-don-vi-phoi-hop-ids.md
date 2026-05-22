# Task – Fix lưu `donViPhoiHopIds` → `DeXuatDonViXuLy`

> Phân tích issue: `[issue-don-vi-phoi-hop-ids-khong-luu.md](./issue-don-vi-phoi-hop-ids-khong-luu.md)`

---

## 1. Phân tích yêu cầu

### 1.1 Việc cần làm


| #   | Mục                                      | Ghi chú                                                                   |
| --- | ---------------------------------------- | ------------------------------------------------------------------------- |
| 1   | **POST** `them-moi` lưu đơn vị phối hợp  | Insert `DeXuatDonViXuLy` cùng lúc với `DeXuatChuTruongMoi`                |
| 2   | **PUT** `cap-nhat` đồng bộ junction      | Clear + re-add theo `donViPhoiHopIds` (pattern `DuAnBuocPhongBanPhoiHop`) |
| 3   | **GET** `chi-tiet` trả `donViPhoiHopIds` | Include navigation + map `ToModel`                                        |
| 4   | Sửa bug map **PUT** controller           | `DonViPhuTrachChinhId` / `LanhDaoPhuTrachId` bị gán nhầm `buocId`         |


### 1.2 Field API liên quan


| Field request/response | Kiểu          | Map DB                                                        |
| ---------------------- | ------------- | ------------------------------------------------------------- |
| `donViPhoiHopIds`      | `List<long>?` | `DeXuatDonViXuLy.DonViId` (`RightId`)                         |
| —                      | —             | `DeXuatDonViXuLy.DuAnId` = `DeXuatChuTruongMoi.Id` (`LeftId`) |


> Cột DB `DuAnId` trên bảng junction = **Id đề xuất**, không phải `DuAnId` dự án.

### 1.3 Payload mẫu

```json
{
  "duAnId": "1690F8E4-3352-48D2-8D48-1D6196B2E74F6",
  "buocId": 0,
  "tomTatNoiDung": "string",
  "tongMucDauTu": 1000,
  "ngayBatDauDuKien": "2026-05-22T03:59:11.991Z",
  "hinhThucDauTuId": 3,
  "lanhDaoPhuTrachId": 4,
  "donViPhuTrachChinhId": 220,
  "nguoiXuLyChinhId": 4,
  "donViPhoiHopIds": [219],
  "danhSachTepDinhKem": []
}
```

### 1.4 API bị ảnh hưởng


| Method | Endpoint                                    | Sau fix                |
| ------ | ------------------------------------------- | ---------------------- |
| POST   | `/api/de-xuat-chu-truong-moi/them-moi`      | Parent + junction      |
| PUT    | `/api/de-xuat-chu-truong-moi/cap-nhat`      | Parent + sync junction |
| GET    | `/api/de-xuat-chu-truong-moi/{id}/chi-tiet` | Có `donViPhoiHopIds`   |

### 1.5 Kiểm tra `[Required]` / field bắt buộc (đã rà soát code)

> **Trước đây task doc chưa có mục này.** Audit dưới đây phục vụ test Swagger/FE và xác nhận `donViPhoiHopIds` không bị validation chặn.

#### Kết luận nhanh

| Nguồn | Có `[Required]` attribute? |
|-------|----------------------------|
| `DeXuatChuTruongMoiModel` | **Không** |
| `DeXuatChuTruongMoiInsertDto` / `UpdateDto` | **Không** |
| FluentValidation module DeXuatChuTruongMoi | **Không** (không có `*Validator`) |
| **`donViPhoiHopIds`** | **Không bắt buộc** — phù hợp fix (optional list) |

#### Bắt buộc thực tế khi gọi API

`QLDA.WebApi` bật `<Nullable>enable</Nullable>`. Property **non-nullable** → model binder ASP.NET coi là required (400 nếu thiếu), **không cần** gắn `[Required]`.

| Field | Kiểu (Model / InsertDto) | Bắt buộc API? | Ghi chú |
|-------|--------------------------|---------------|---------|
| `duAnId` | `Guid` | **Có** | `ITienDo`; FK `DuAn` Restrict |
| `id` | `Guid?` | Không (POST) | `GetId()` sinh GUID nếu null |
| `buocId` | `int?` | Không (attribute) | Remark controller “Quy trình id bắt buộc” → gọi `DuAnUpdateStepCommand`, không phải `[Required]` |
| **`donViPhoiHopIds`** | `List<long>?` | **Không** | `null` / `[]` / `[219]` đều pass validation |
| `tomTatNoiDung`, `tongMucDauTu`, `ngayBatDauDuKien` | nullable | Không | |
| `hinhThucDauTuId`, `trangThaiId` | `int?` | Không | Insert gán trạng thái DT server-side |
| `lanhDaoPhuTrachId`, `donViPhuTrachChinhId`, `nguoiXuLyChinhId` | `long?` | Không | |
| `danhSachTepDinhKem` | `List<...>?` | Không | `TepDinhKemModel` cũng không `[Required]` |

**File đã grep:** `QLDA.WebApi/Models/DeXuatChuTruongMois/DeXuatChuTruongMoiModel.cs`, `QLDA.Application/DeXuatChuTruongMoi/DTOs/*.cs`.

#### EF / database

| Field | `DeXuatChuTruongMoi` | Ghi chú |
|-------|----------------------|---------|
| `DuAnId` | NOT NULL | |
| `BuocId`, nội dung, số tiền, đơn vị, … | NULL allowed | |
| `TrangThaiId` | NULL | `.IsRequired(false)` trong `DeXuatChuTruongMoiConfiguration` |
| `CreatedBy`, `UpdatedBy` | NOT NULL | Audit — không từ body client |

| Field | `DeXuatDonViXuLy` | Ghi chú |
|-------|-------------------|---------|
| `DuAnId` / `DonViId` | NOT NULL (khi có row) | Chỉ tạo row khi FE gửi id trong list |

#### Ảnh hưởng tới fix này

- **Không cần** sửa `[Required]` cho task junction — không có attribute nào cản `donViPhoiHopIds` rỗng/null.
- Nếu sau này product **bắt buộc** ≥1 đơn vị phối hợp: thêm validate trong handler hoặc `[MinLength(1)]` trên list (breaking change FE).

#### Test validation (bổ sung checklist §5)

| Request | Kỳ vọng |
|---------|---------|
| POST thiếu `duAnId` | **400** (model binding) |
| POST không gửi `donViPhoiHopIds` | **200**, junction trống |
| POST `"donViPhoiHopIds": []` | **200**, junction trống |

---

## 2. Phân tích hiện trạng

### 2.1 Kết quả trước khi fix


| Bảng                 | Trạng thái                            |
| -------------------- | ------------------------------------- |
| `DeXuatChuTruongMoi` | Insert/update **OK**                  |
| `DeXuatDonViXuLy`    | **Trống** dù FE gửi `donViPhoiHopIds` |


### 2.2 Nguyên nhân gốc


| Layer                 | Vấn đề                                                                                |
| --------------------- | ------------------------------------------------------------------------------------- |
| **Insert handler**    | Tạo entity mới, không set `Id` từ request, **bỏ qua** `DeXuatDonViXuLys`              |
| **Update handler**    | Không `Include` junction, không gọi sync `DonViPhoiHopIds`                            |
| **WebApi `ToEntity`** | Chỉ map scalar, không tạo `DeXuatDonViXuLys`                                          |
| **WebApi `ToModel`**  | Không trả `DonViPhoiHopIds`; `NguoiXuLyChinhId` map sai                               |
| **Controller PUT**    | Build DTO thiếu field + gán `BuocId` vào `DonViPhuTrachChinhId` / `LanhDaoPhuTrachId` |
| **Get query**         | Không `Include(DeXuatDonViXuLys)`                                                     |


### 2.3 Đã có sẵn (không cần migration)

- Entity `DeXuatDonViXuLy`, navigation `DeXuatChuTruongMoi.DeXuatDonViXuLys`
- EF `DeXuatDonViXuLyConfiguration` (composite PK, cascade delete)
- `DeXuatChuTruongMoiModel.DonViPhoiHopIds`, `DeXuatChuTruongMoiInsertDto.DonViPhoiHopIds`
- `DeXuatChuTruongMoiMappings.ToEntity(dto)` đã map junction nhưng **không được dùng** trong luồng WebApi insert
- `DeXuatChuTruongMoiGetDanhSachQuery` đã join `DanhSachDonViPhoiHop` (read list OK nếu DB có data)

### 2.4 Pattern tham chiếu

`DuAnBuocUpdateCommand` — sync `DuAnBuocPhongBanPhoiHops`:

```csharp
if (request.Dto.DanhSachPhongBanPhoiHopIds != null) {
    entity.DuAnBuocPhongBanPhoiHops?.Clear();
    foreach (var phongBanId in request.Dto.DanhSachPhongBanPhoiHopIds) {
        entity.DuAnBuocPhongBanPhoiHops!.Add(new DuAnBuocPhongBanPhoiHop {
            LeftId = entity.Id,
            RightId = phongBanId
        });
    }
}
```

---

## 3. Thứ tự thực hiện

```
Bước 1: Application – DeXuatChuTruongMoiMappings.SyncDonViPhoiHopIds
Bước 2: Application – DeXuatChuTruongMoiInsertCommand (Id + junction)
Bước 3: Application – DeXuatChuTruongMoiUpdateCommand (Include + sync)
Bước 4: Application – DeXuatChuTruongMoiGetQuery (Include junction)
Bước 5: WebApi – DeXuatChuTruongMoiMappingConfiguration (ToEntity, ToModel, ToInsertDto)
Bước 6: WebApi – DeXuatChuTruongMoiController.Update (ToInsertDto)
```

**Không làm:** Migration, Domain entity mới, sửa snapshot.

---

## 4. Chi tiết từng bước

---

### Bước 1 – Application: `SyncDonViPhoiHopIds`

**File:** `QLDA.Application/DeXuatChuTruongMoi/DeXuatChuTruongMoiMappings.cs`

**Thêm extension:**

```csharp
public static void SyncDonViPhoiHopIds(this DeXuatChuTruongMoi entity, List<long>? donViPhoiHopIds) {
    if (donViPhoiHopIds is null) {
        return;
    }

    entity.DeXuatDonViXuLys ??= [];
    entity.DeXuatDonViXuLys.Clear();
    foreach (var donViId in donViPhoiHopIds) {
        entity.DeXuatDonViXuLys.Add(new DeXuatDonViXuLy {
            LeftId = entity.Id,
            RightId = donViId,
        });
    }
}
```


| `donViPhoiHopIds` | Hành vi                     |
| ----------------- | --------------------------- |
| `null`            | Không đổi junction (update) |
| `[]`              | Xóa hết đơn vị phối hợp     |
| `[219, 220]`      | Thay thế toàn bộ            |


---

### Bước 2 – Application: `DeXuatChuTruongMoiInsertCommand`

**File:** `QLDA.Application/DeXuatChuTruongMoi/Commands/DeXuatChuTruongMoiInsertCommand.cs`

**Trước:** Entity mới không có `Id` từ client, không có `DeXuatDonViXuLys`.

**Sau:**

```csharp
var entity = new DeXuatChuTruongMoi
{
    Id = request.Dto.Id,
    DuAnId = request.Dto.DuAnId,
    BuocId = request.Dto.BuocId,
    TongMucDauTu = request.Dto.TongMucDauTu,
    TomTatNoiDung = request.Dto.TomTatNoiDung,
    NgayBatDauDuKien = request.Dto.NgayBatDauDuKien,
    HinhThucDauTuId = request.Dto.HinhThucDauTuId,
    NguoiXuLyChinhId = request.Dto.NguoiXuLyChinhId,
    LanhDaoPhuTrachId = request.Dto.LanhDaoPhuTrachId,
    DonViPhuTrachChinhId = request.Dto.DonViPhuTrachChinhId,
    TrangThaiId = trangThaiDuThao?.Id,
};
entity.SyncDonViPhoiHopIds(
    request.Dto.DeXuatDonViXuLys?.Select(x => x.RightId).ToList() ?? []);

await _repo.AddAsync(entity, cancellationToken);
```

> `request.Dto` là `DeXuatChuTruongMoi` từ `model.ToEntity()` — `DeXuatDonViXuLys` đã map từ `donViPhoiHopIds` ở WebApi.

---

### Bước 3 – Application: `DeXuatChuTruongMoiUpdateCommand`

**File:** `QLDA.Application/DeXuatChuTruongMoi/Commands/DeXuatChuTruongMoiUpdateCommand.cs`

```csharp
var entity = await _repo.GetQueryableSet()
    .Include(e => e.DeXuatDonViXuLys)
    .Include(e => e.TrangThai)
    .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);

// ... validate trạng thái dự thảo ...

entity.TongMucDauTu = request.Dto.TongMucDauTu;
entity.TomTatNoiDung = request.Dto.TomTatNoiDung;
entity.HinhThucDauTuId = request.Dto.HinhThucDauTuId;
entity.NguoiXuLyChinhId = request.Dto.NguoiXuLyChinhId;
entity.NgayBatDauDuKien = request.Dto.NgayBatDauDuKien;
entity.DonViPhuTrachChinhId = request.Dto.DonViPhuTrachChinhId;
entity.LanhDaoPhuTrachId = request.Dto.LanhDaoPhuTrachId;
entity.DuAnId = request.Dto.DuAnId;
entity.BuocId = request.Dto.BuocId;

entity.SyncDonViPhoiHopIds(request.Dto.DonViPhoiHopIds);

await _repo.UpdateAsync(entity, cancellationToken);
```

---

### Bước 4 – Application: `DeXuatChuTruongMoiGetQuery`

**File:** `QLDA.Application/DeXuatChuTruongMoi/Queries/DeXuatChuTruongMoiGetQuery.cs`

```csharp
var queryable = DeXuatChuTruongMoi.GetOrderedSet()
    .Include(e => e.DeXuatDonViXuLys)
    .Where(e => e.Id == request.Id);
```

---

### Bước 5 – WebApi: Mapping

**File:** `QLDA.WebApi/Models/DeXuatChuTruongMois/DeXuatChuTruongMoiMappingConfiguration.cs`

#### `ToEntity` (POST)

```csharp
public static DeXuatChuTruongMoi ToEntity(this DeXuatChuTruongMoiModel model) {
    var id = model.GetId();
    return new() {
        Id = id,
        BuocId = model.BuocId,
        DuAnId = model.DuAnId,
        TongMucDauTu = model.TongMucDauTu,
        TomTatNoiDung = model.TomTatNoiDung,
        NgayBatDauDuKien = model.NgayBatDauDuKien,
        LanhDaoPhuTrachId = model.LanhDaoPhuTrachId,
        NguoiXuLyChinhId = model.NguoiXuLyChinhId,
        HinhThucDauTuId = model.HinhThucDauTuId,
        DonViPhuTrachChinhId = model.DonViPhuTrachChinhId,
        DeXuatDonViXuLys = model.DonViPhoiHopIds != null && model.DonViPhoiHopIds.Count != 0
            ? [.. model.DonViPhoiHopIds.Select(donViId => new DeXuatDonViXuLy { LeftId = id, RightId = donViId })]
            : [],
    };
}
```

#### `ToModel` (GET chi tiết)

```csharp
NguoiXuLyChinhId = entity.NguoiXuLyChinhId,  // trước: entity.LanhDaoPhuTrachId (sai)
DonViPhoiHopIds = entity.DeXuatDonViXuLys?.Select(x => x.RightId).ToList(),
```

#### `ToInsertDto` (PUT) — **mới**

```csharp
public static DeXuatChuTruongMoiInsertDto ToInsertDto(this DeXuatChuTruongMoiModel model) =>
    new() {
        Id = model.GetId(),
        DuAnId = model.DuAnId,
        BuocId = model.BuocId,
        TomTatNoiDung = model.TomTatNoiDung,
        TongMucDauTu = model.TongMucDauTu,
        NgayBatDauDuKien = model.NgayBatDauDuKien,
        HinhThucDauTuId = model.HinhThucDauTuId,
        LanhDaoPhuTrachId = model.LanhDaoPhuTrachId,
        NguoiXuLyChinhId = model.NguoiXuLyChinhId,
        DonViPhuTrachChinhId = model.DonViPhuTrachChinhId,
        TrangThaiId = model.TrangThaiId,
        DonViPhoiHopIds = model.DonViPhoiHopIds,
    };
```

---

### Bước 6 – WebApi: Controller `cap-nhat`

**File:** `QLDA.WebApi/Controllers/DeXuatChuTruongMoiController.cs`

**Trước (sai):**

```csharp
await Mediator.Send(new DeXuatChuTruongMoiUpdateCommand(new() {
    Id = model.GetId(),
    DuAnId = model.DuAnId,
    BuocId = model.BuocId,
    DonViPhuTrachChinhId = model.BuocId,  // bug
    LanhDaoPhuTrachId = model.BuocId,     // bug
    // thiếu DonViPhoiHopIds, NguoiXuLyChinhId, ...
}));
```

**Sau:**

```csharp
await Mediator.Send(new DeXuatChuTruongMoiUpdateCommand(model.ToInsertDto()), cancellationToken);
```

**POST `them-moi`:** không đổi signature — vẫn `model.ToEntity()` → `InsertCommand(entity)`.

---

## 5. Checklist hoàn thành

```
[x] 1. Thêm SyncDonViPhoiHopIds vào DeXuatChuTruongMoiMappings
[x] 2. Sửa DeXuatChuTruongMoiInsertCommand (Id + junction trước AddAsync)
[x] 3. Sửa DeXuatChuTruongMoiUpdateCommand (Include + LanhDaoPhuTrachId + sync)
[x] 4. Sửa DeXuatChuTruongMoiGetQuery (Include DeXuatDonViXuLys)
[x] 5. Sửa ToEntity / ToModel / thêm ToInsertDto (WebApi mapping)
[x] 6. Sửa DeXuatChuTruongMoiController.Update (ToInsertDto)
[x] 8. Rà soát `[Required]` / field bắt buộc (§1.5)
[ ] 7. Build + Test API (POST/PUT/GET + DB + case thiếu `duAnId`)
```

---

## 6. Lưu ý kỹ thuật

- **Không migration** — bảng `DeXuatDonViXuLy` đã có trong `Init`.
- **Insert:** `Id` phải khớp `model.GetId()` để `TepDinhKem` (`GroupId = savedEntity.Id`) và junction `LeftId` cùng một GUID.
- **Update:** `SyncDonViPhoiHopIds(null)` giữ junction cũ; PUT qua `ToInsertDto` truyền list từ model (có thể `null` nếu FE không gửi field).
- **List tiến độ** (`danh-sach-tien-do`): không đổi — vẫn join `DanhMucDonVi` → `DanhSachDonViPhoiHop`.
- **GET chi tiết:** chỉ trả `donViPhoiHopIds` (ids), chưa trả tên đơn vị; tên có ở list query.
- **`[Required]`:** xem **§1.5** — chỉ `duAnId` bắt buộc qua `Guid` non-nullable; `donViPhoiHopIds` không required.

### Test gợi ý

```powershell
dotnet build "e:\SER\QLDA.WebApi\QLDA.WebApi.csproj"
```


| Case                           | Kỳ vọng                           |
| ------------------------------ | --------------------------------- |
| POST `donViPhoiHopIds: [219]`  | 1 row `DeXuatDonViXuLy`           |
| PUT đổi `[219]` → `[220, 221]` | Junction thay thế, không trùng PK |
| PUT `donViPhoiHopIds: []`      | Junction trống                    |
| GET `chi-tiet`                 | `donViPhoiHopIds` khớp DB         |
| POST thiếu `duAnId`            | 400 (non-nullable `Guid`)         |
| POST không có `donViPhoiHopIds`| 200, không 400 validation         |


---

## 6.1 Những lỗi đã sửa trong task này

### Application Layer


| Lỗi                                      | Sửa                                                  |
| ---------------------------------------- | ---------------------------------------------------- |
| Insert handler bỏ qua `DeXuatDonViXuLys` | `SyncDonViPhoiHopIds` + `AddAsync` với collection    |
| Insert không set `Id` từ request         | `Id = request.Dto.Id`                                |
| Update không sync junction               | `.Include(DeXuatDonViXuLys)` + `SyncDonViPhoiHopIds` |
| Update thiếu `LanhDaoPhuTrachId`         | Gán từ `request.Dto`                                 |
| Get không load junction                  | `.Include(e => e.DeXuatDonViXuLys)`                  |


### WebApi Layer


| Lỗi                                               | Sửa                                           |
| ------------------------------------------------- | --------------------------------------------- |
| `ToEntity` không map `donViPhoiHopIds`            | Gán `DeXuatDonViXuLys` với `LeftId = GetId()` |
| `ToModel`: `NguoiXuLyChinhId = LanhDaoPhuTrachId` | `NguoiXuLyChinhId = entity.NguoiXuLyChinhId`  |
| `ToModel` thiếu `DonViPhoiHopIds`                 | Select `RightId` từ navigation                |
| PUT build DTO thiếu field, map `BuocId` sai       | `model.ToInsertDto()`                         |


---

## 7. Tóm tắt công việc

### Tổng quan

**Bugfix** — đồng bộ đơn vị phối hợp khi lưu đề xuất chủ trương mới:

- **0** entity / migration mới
- **6** file chỉnh sửa
- **3** API endpoint hành vi đúng (POST, PUT, GET chi tiết)

### Các file chỉnh sửa


| #   | File                                                                               | Thay đổi                             |
| --- | ---------------------------------------------------------------------------------- | ------------------------------------ |
| 1   | `QLDA.Application/DeXuatChuTruongMoi/DeXuatChuTruongMoiMappings.cs`                | ➕ `SyncDonViPhoiHopIds`              |
| 2   | `QLDA.Application/DeXuatChuTruongMoi/Commands/DeXuatChuTruongMoiInsertCommand.cs`  | `Id` + sync junction                 |
| 3   | `QLDA.Application/DeXuatChuTruongMoi/Commands/DeXuatChuTruongMoiUpdateCommand.cs`  | Include + sync + `LanhDaoPhuTrachId` |
| 4   | `QLDA.Application/DeXuatChuTruongMoi/Queries/DeXuatChuTruongMoiGetQuery.cs`        | Include `DeXuatDonViXuLys`           |
| 5   | `QLDA.WebApi/Models/DeXuatChuTruongMois/DeXuatChuTruongMoiMappingConfiguration.cs` | `ToEntity`, `ToModel`, `ToInsertDto` |
| 6   | `QLDA.WebApi/Controllers/DeXuatChuTruongMoiController.cs`                          | PUT → `ToInsertDto()`                |


### File không sửa


| File                                                           | Lý do                      |
| -------------------------------------------------------------- | -------------------------- |
| `DeXuatDonViXuLy.cs`, `DeXuatDonViXuLyConfiguration.cs`        | Schema đã đúng             |
| `DeXuatChuTruongMoiModel.cs`, `DeXuatChuTruongMoiInsertDto.cs` | Field đã có                |
| `DeXuatChuTruongMoiGetDanhSachQuery.cs`                        | Read list đã join phối hợp |
| Migrations                                                     | Không đổi DB               |


### Luồng dữ liệu sau fix

```
POST: Model.donViPhoiHopIds → ToEntity.DeXuatDonViXuLys → InsertCommand → DB
PUT:  Model.donViPhoiHopIds → ToInsertDto → UpdateCommand.Sync → DB
GET:  DB → Include DeXuatDonViXuLys → ToModel.DonViPhoiHopIds
```

### Trạng thái

- **Code:** đã implement (bước 1–6)
- **Tiếp theo:** build + test API + tick checklist mục 7

