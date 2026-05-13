# 📋 Tóm Tắt Thực Hiện - Feature Bàn Giao Hồ Sơ (BanGiaoHoSo)

**Status:** ✅ Thiết kế hoàn toàn, sẵn sàng code  
**Date:** 13/05/2026  
**Version:** 8.0

---

## 1. 🎯 Tổng Quan Feature

**Feature:** Quản lý **Bàn Giao Hồ Sơ** từ người dùng → Phòng HC-TH

**Phạm vi:**
- ✅ 1 Entity mới (`BanGiaoHoSo`) - Aggregate Root với GUID key
- ✅ 1 Enum mới (`ETrangThaiBanGiao`) - 2 trạng thái: Khởi tạo (0), Đã bàn giao (1)
- ✅ 6 API Endpoints mới (CRUD + ban-giao operation)
- ✅ Hỗ trợ **2 loại tệp đính kèm** qua `TepDinhKem`:
  - **Tệp HS bàn giao** (`EGroupType.BanGiaoHoSo`) - gắn lúc Insert/Update
  - **Biên bản bàn giao** (`EGroupType.BienBanBanGiao`) - gắn lúc endpoint ban-giao

---

## 2. 📐 Model & Cấu Trúc Database

### Entity: `BanGiaoHoSo`

| Field | Kiểu | Ý nghĩa |
|-------|------|---------|
| `Id` | `Guid` | PK - sequential GUID |
| `Ma` | `string(100)` | Mã bản giao (unique) |
| `TenHoSo` | `string(500)` | Tên hồ sơ |
| `DuAnId` | `Guid?` | FK → DuAn |
| `BuocId` | `int?` | FK → DuAnBuoc |
| `GhiChu` | `string(2000)?` | Ghi chú |
| `PhongBanChuTriId` | `long?` | Ref → DanhMucDonVi (⚠️ không FK; **⚠️ V8: KHÔNG cho UI truyền** – tự động set trong `InsertCommandHandler` từ `_userProvider.Info.PhongBanID ?? _userProvider.Info.DonViID`) |
| `TrangThai` | `ETrangThaiBanGiao` | 0: Khởi tạo, 1: Đã bàn giao |
| `NgayBanGiao` | `DateTimeOffset?` | Ngày thực hiện bàn giao (Entity lưu UTC; DTO/Model trả về `DateOnly?`) |
| `CreatedBy` | `string` | Người tạo – từ base class `Entity<T>`, tự động set bởi EF interceptor |
| `IsDeleted` | `bit` | Soft delete flag |
| `CreatedAt`, `UpdatedAt` | `DateTime` | Audit fields |

**Index:** `(CreatedBy, TrangThai)` - tối ưu query danh sách

> ⚠️ **Bảng ngoại lệ (không tạo FK):**
> - `UserMaster` (DB: `USER_MASTER`) – bị force-replace bởi DB khác → dùng **LeftOuterJoin** để lấy `TenNguoiTao`
> - `DanhMucDonVi` (DB: `DM_DONVI`) – bị force-replace tương tự → dùng **LeftOuterJoin** để lấy `TenPhongBan`
> - Dùng `LinqExtensions.LeftOuterJoin()` đã implement sẵn, **KHÔNG** dùng `GroupJoin + SelectMany` thủ công

### Enum: `ETrangThaiBanGiao`

```csharp
public enum ETrangThaiBanGiao {
    KhoiTao = 0,        // Khởi tạo - chưa bàn giao
    DaBanGiao = 1       // Đã bàn giao cho phòng HC-TH
}
```

---

## 3. 🔌 API Endpoints (6 mới)

| HTTP | Route | Mô tả | Input | Output |
|------|-------|-------|-------|--------|
| `GET` | `/api/ban-giao-ho-so/{id}/chi-tiet` | Chi tiết 1 bản giao (cả 2 loại tệp) | `Guid id` | `BanGiaoHoSoModel` |
| `GET` | `/api/ban-giao-ho-so/danh-sach` | Danh sách phân trang (filter theo user) | `BanGiaoHoSoSearchDto, pagination` | `PaginatedList<BanGiaoHoSoDto>` |
| `POST` | `/api/ban-giao-ho-so/them-moi` | Thêm mới + tệp HS bàn giao | `BanGiaoHoSoInsertDto` | `Guid` (Id) |
| `PUT` | `/api/ban-giao-ho-so/cap-nhat` | Cập nhật + tệp HS bàn giao | `BanGiaoHoSoUpdateModel` | `Guid` (Id) |
| `PUT` | `/api/ban-giao-ho-so/{id}/ban-giao` | **Thực hiện bàn giao**: đổi TrangThai 0→1, set NgayBanGiao, lưu biên bản | `BanGiaoHoSoBanGiaoModel` | `int` (1) |
| `DELETE` | `/api/ban-giao-ho-so/{id}/xoa-tam` | Xóa soft (chỉ TrangThai = 0) | `Guid id` | `int` (1) |

**Ủy quyền:** Tất cả endpoints đều `[Authorize]`

**Filter Logic:**
- Danh sách chỉ hiển thị bản giao **của chính người dùng hiện tại** (từ `IUserProvider`)
- UI truyền filter params: `TrangThai`, `DuAnId`, `BuocId`
- `CreatedBy` luôn lấy từ Auth (`IUserProvider`), không cho UI truyền

---

## 4. 📁 Layer-by-Layer Implementation

### 4.1 Domain Layer ✅

**File mới:**
- `QLDA.Domain/Enums/ETrangThaiBanGiao.cs` (enum)
- `QLDA.Domain/Entities/BanGiaoHoSo.cs` (aggregate root)

**Chi tiết:**
- **Entity kế thừa** `Entity<Guid>, IAggregateRoot`
- FK: `DuAnId` (Guid?) → `DuAn`
- FK: `BuocId` (int?) → `DuAnBuoc`
- `PhongBanChuTriId` (long?) → ref DanhMucDonVi **⚠️ không FK, không navigation**
- `CreatedBy` (long?) → ref UserMaster **⚠️ không FK, không navigation** – từ base class
- Navigation properties: `DuAn`, `Buoc` (chỉ 2 cái)

---

### 4.2 Persistence Layer ✅

**File:**
- `QLDA.Persistence/Configurations/BanGiaoHoSoConfiguration.cs` (mới)

**BanGiaoHoSoConfiguration:**
- Table: `BanGiaoHoSo`
- Index: `(CreatedBy, TrangThai)`
- FK constraints: Chỉ cho `DuAn` và `DuAnBuoc` → SetNull
- ⚠️ **KHÔNG FK đến `UserMaster` và `DanhMucDonVi`** – dùng LeftOuterJoin
- Conversion: `TrangThai` as int

---

### 4.3 Application Layer ✅

**Folder:** `QLDA.Application/BanGiaoHoSos/`

#### DTOs (4 files)

| File | Ý nghĩa |
|------|---------|
| `BanGiaoHoSoInsertDto.cs` | Insert payload (inherit `IMayHaveTepDinhKemDto`) – **⚠️ V8: KHÔNG có `PhongBanChuTriId`** |
| `BanGiaoHoSoUpdateModel.cs` | Update payload (+ `Id`) |
| `BanGiaoHoSoDto.cs` | Response model (kèm cả 2 loại tệp + `CreatedAt`) |
| `BanGiaoHoSoSearchDto.cs` | Filter query (`TrangThai?`, `DuAnId?`, `BuocId?`) |

#### Mappings (1 file)

- `BanGiaoHoSoMappings.cs` - Entity ↔ DTO mappings

#### Commands (4 files)

| Command | Mô tả |
|---------|-------|
| `BanGiaoHoSoInsertCommand` | Thêm mới entity – **⚠️ V8: inject `IUserProvider`, sau `ToEntity()` gán `entity.PhongBanChuTriId = _userProvider.Info.PhongBanID ?? _userProvider.Info.DonViID`** |
| `BanGiaoHoSoUpdateCommand` | Cập nhật entity **(chỉ TrangThai=0)** |
| `BanGiaoHoSoBanGiaoCommand` | **Ban-giao**: TrangThai 0→1, set NgayBanGiao |
| `BanGiaoHoSoDeleteCommand` | Soft delete **(chỉ TrangThai=0)** |

**Xử lý Transaction:**
- Isolation Level: `ReadCommitted`
- Begin TX → Execute → SaveChanges → Commit

#### Queries (2 files)

| Query | Mô tả |
|-------|-------|
| `BanGiaoHoSoGetQuery` | Lấy 1 entity by Id (include nav properties) |
| `BanGiaoHoSoGetDanhSachQuery` | Lấy danh sách phân trang, filter theo CreatedBy + TrangThai + DuAnId + BuocId; LeftOuterJoin UserMaster + DanhMucDonVi |

---

### 4.4 WebApi Layer ✅

**Folder:** `QLDA.WebApi/Models/BanGiaoHoSos/` + `Controllers/`

#### Models (2 files)

| Model | Ý nghĩa |
|-------|---------|
| `BanGiaoHoSoModel.cs` | **Response-only** model cho endpoint `chi-tiet` – không implement request interfaces |
| `BanGiaoHoSoBanGiaoModel.cs` | Ban-giao payload (NgayBanGiao, DanhSachBienBan) |

> ⚠️ **`BanGiaoHoSoModel` là pure response model** – controller Insert nhận `BanGiaoHoSoInsertDto`, Update nhận `BanGiaoHoSoUpdateModel`; không qua model trung gian.
> ⚠️ **KHÔNG sửa `WebApplicationExtensions.cs`** – `IUserProvider` đã đăng ký sẵn trong BuildingBlocks DI.

#### Supporting Infrastructure (1 file)

| File | Ý nghĩa |
|------|---------|
| `QLDA.WebApi/Models/BanGiaoHoSos/BanGiaoHoSoMappingConfiguration.cs` | `ToModel()` + `GetDanhSachBienBanBanGiao()` |

#### Controller (1 file)

- `BanGiaoHoSoController.cs` - 6 endpoints (Get, GetList, Insert, Update, BanGiao, SoftDelete)

---

## 5. 📎 TepDinhKem Pattern (2 Loại Tệp)

**Bộ hồ sơ bàn giao gồm 2 loại tệp:**

### Cấu Trúc

```
BanGiaoHoSo (Entity)
├─ TepDinhKem (GroupId = BanGiaoHoSo.Id.ToString())
│  ├─ EGroupType.BanGiaoHoSo (Tệp HS bàn giao)
│  │   └─ Gắn khi: Insert / Update
│  │   └─ Extension: GetDanhSachTepHSBanGiao()
│  │
│  └─ EGroupType.BienBanBanGiao (Biên bản bàn giao)
│      └─ Gắn khi: Endpoint ban-giao
│      └─ Extension: GetDanhSachBienBanBanGiao()
```

### Thêm vào EGroupType Enum

```csharp
public enum EGroupType {
    // ... existing entries ...
    BanGiaoHoSo,       // Tệp HS bàn giao
    BienBanBanGiao,    // Biên bản bàn giao
}
```

### Lưu Tệp HS Bàn Giao (Insert/Update)

```csharp
await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
    GroupId = entity.Id.ToString(),
    Entities = model.GetDanhSachTepHSBanGiao(entity.Id)
});
// → EGroupType = EGroupType.BanGiaoHoSo
```

### Lưu Biên Bản Bàn Giao (Ban-Giao Endpoint)

```csharp
await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
    GroupId = entity.Id.ToString(),
    Entities = model.GetDanhSachBienBanBanGiao(entity.Id)
});
// → EGroupType = EGroupType.BienBanBanGiao
```

### Lấy Tệp (Query Danh Sách)

```csharp
// Tệp HS bàn giao
DanhSachTepHSBanGiao = TepDinhKem.GetQueryableSet()
    .Where(f => f.GroupId == e.Id.ToString() 
            && f.EGroupType == EGroupType.BanGiaoHoSo 
            && !f.IsDeleted)
    .Select(f => f.ToDto()).ToList(),

// Biên bản bàn giao
DanhSachBienBanBanGiao = TepDinhKem.GetQueryableSet()
    .Where(f => f.GroupId == e.Id.ToString() 
            && f.EGroupType == EGroupType.BienBanBanGiao 
            && !f.IsDeleted)
    .Select(f => f.ToDto()).ToList()
```

---

## 6. 🔒 Permission & Business Logic

### Insert / Update

```
✅ Cho phép Insert: Tất cả authenticated user
✅ CreatedBy: Auto set bởi EF interceptor từ JWT token – không gán thủ công
✅ PhongBanChuTriId: Auto set trong InsertCommandHandler = _userProvider.Info.PhongBanID ?? _userProvider.Info.DonViID
   (không cho UI truyền; không cập nhật khi Update – phòng ban cố định theo người tạo)
✅ Cho phép Update: Chỉ khi TrangThai = 0 (Khởi tạo)
❌ Không cho phép Update: TrangThai = 1 (đã bàn giao)
❌ Exception Update: "Chỉ có thể cập nhật bản giao hồ sơ ở trạng thái 'Khởi tạo'"
```

### Ban-Giao Endpoint

```
✅ Logic:
  1. Lấy entity by Id
  2. Đổi TrangThai: 0 → 1
  3. Set NgayBanGiao (client truyền DateOnly?, server convert sang DateTimeOffset UTC via DateOnlyExtensions. Default: ngày hiện tại nếu null)
  4. Lưu biên bản bàn giao (EGroupType.BienBanBanGiao)
  
✅ Cho phép: TrangThai = 0 (chưa bàn giao)
❌ Không cho phép: TrangThai = 1 (đã bàn giao)
```

### Delete (Soft Delete)

```
✅ Cho phép: Chỉ khi TrangThai = 0
❌ Không cho phép: TrangThai = 1
❌ Exception: "Chỉ có thể xóa bản giao hồ sơ ở trạng thái 'Khởi tạo'"
```

### List / Get

```
✅ Filter: CreatedBy = IUserProvider.Id (luôn, từ JWT)
✅ Filter: TrangThai, DuAnId, BuocId (tuỳ chọn từ UI)
✅ No GlobalFilter: Không search full-text
```

---

## 7. ⚙️ Dependency Injection & Services

### 1. `IUserProvider` Service
- **Interface:** `BuildingBlocks.Domain.Providers.IUserProvider`
- **Register:** Đã đăng ký sẵn trong DI của dự án – **không cần sửa `WebApplicationExtensions.cs`**
- **Cách dùng:**
  - Inject trong `InsertCommandHandler` để auto-set `PhongBanChuTriId` = `Info.PhongBanID ?? Info.DonViID`
  - Inject trong `GetDanhSachQueryHandler` để lọc theo `CreatedBy`
- `IUserProvider.Id` → `long` UserId từ JWT claim
- `IUserProvider.Info.PhongBanID` → `long?` (phòng ban)
- `IUserProvider.Info.DonViID` → `long?` (đơn vị – fallback khi PhongBanID null)

### 2. MediatR Commands & Queries
- Auto-register via `AddMediatR(typeof(Program).Assembly)`
- Handlers: `*CommandHandler`, `*QueryHandler` classes

### 3. Repositories
- `IRepository<BanGiaoHoSo, Guid>` - auto-injected
- `IRepository<TepDinhKem, Guid>` - lấy tệp đính kèm

### 4. UnitOfWork
- `IUnitOfWork` - transaction management

---

## 8. 📋 Checklist Hoàn Thành

### ✅ Completed

- [x] **1. Domain - Enum ETrangThaiBanGiao**
- [x] **2. Domain - Entity BanGiaoHoSo** – bỏ `UserId`, bỏ navigation `User`/`PhongBanChuTri`; chỉ nav `DuAn`, `Buoc`
- [x] **3. Persistence - BanGiaoHoSoConfiguration** – bỏ FK UserMaster + DanhMucDonVi; index `(CreatedBy, TrangThai)`
- [x] **4. Application - DTOs** – `SearchDto` thêm `DuAnId`/`BuocId`; `Dto` thêm `CreatedAt`, bỏ `UserId`
- [x] **5. Application - Mappings** – thêm `GetDanhSachTepHSBanGiao()` + `GetDanhSachBienBanBanGiao()`
- [x] **6. Application - Commands** (4 files) – Insert inject `IUserProvider`, auto-set `entity.PhongBanChuTriId` từ phòng ban người tạo (⚠️ V8)
- [x] **7. Application - Queries** – GetQuery bỏ Include User/PhongBanChuTri; GetDanhSachQuery dùng `LeftOuterJoin()` cho UserMaster + DanhMucDonVi, thêm `WhereIf` DuAnId/BuocId
- [x] **8. WebApi - Models** – `BanGiaoHoSoModel` là pure response model (bỏ IHasKey, IMustHaveId...)
- [x] **9. WebApi - MappingConfiguration** – chỉ giữ `ToModel()` + `GetDanhSachBienBanBanGiao()`; bỏ dead code
- [x] **10. WebApi - BanGiaoHoSoController** – Insert nhận `BanGiaoHoSoInsertDto`, Update nhận `BanGiaoHoSoUpdateModel`
- [x] **11. Enum - Add EGroupType.BanGiaoHoSo & EGroupType.BienBanBanGiao**

### ⏳ TODO

- [ ] **Migration** - Chạy `ef migrations add AddBanGiaoHoSo` từ QLDA.Migrator
- [ ] **Database Update** - Chạy `database update`
- [ ] **Build & Test** - Compile & test all endpoints

---

## 9. 🚀 Next Steps

1. **Migration** (trong `QLDA.Migrator/`)
   ```bash
   dotnet ef migrations add AddBanGiaoHoSo
   dotnet ef database update
   ```
   ⚠️ **KHÔNG chạy `drop-database`** - chỉ cần `add` + `update`

2. **Compile Solution**
   ```bash
   dotnet build SER.sln
   ```

3. **Test APIs**
   - POST `/api/ban-giao-ho-so/them-moi` - Create
   - PUT `/api/ban-giao-ho-so/cap-nhat` - Update
   - GET `/api/ban-giao-ho-so/danh-sach` - List
   - GET `/api/ban-giao-ho-so/{id}/chi-tiet` - Get detail
   - PUT `/api/ban-giao-ho-so/{id}/ban-giao` - Ban-giao
   - DELETE `/api/ban-giao-ho-so/{id}/xoa-tam` - Delete

---

## 10. 📊 Files Summary

| Layer | Count | Status |
|-------|-------|--------|
| **Domain** | 2 | ✅ Designed |
| **Persistence** | 2 | ✅ Designed |
| **Application** | 9 | ✅ Designed |
| **WebApi** | 7 | ✅ Designed |
| **Total** | **20** | ✅ Complete Design |

**Not Implemented Yet:**
- Migration files (auto-generated)
- Build artifacts

---

## 11. 🔖 Key Points Recap

| Aspect | Detail |
|--------|--------|
| **PK Type** | `Guid` (sequential) |
| **Soft Delete** | `IsDeleted` bit |
| **Audit** | `CreatedAt`, `UpdatedAt` |
| **Status Enum** | `ETrangThaiBanGiao` (0/1) |
| **FK Relations** | DuAnId (Guid?), BuocId (int?) – `PhongBanChuTriId` và `CreatedBy` dùng LeftOuterJoin, không FK |
| **PhongBanChuTriId** | ⚠️ V8: Không cho UI truyền; auto-set trong InsertCommandHandler = `_userProvider.Info.PhongBanID ?? _userProvider.Info.DonViID`; không thay đổi khi Update |
| **NgayBanGiao** | Entity: `DateTimeOffset?` (DB lưu UTC). Input nhận `DateOnly?`, convert qua `ToStartOfDayUtc()`. Response (`BanGiaoHoSoDto`, `BanGiaoHoSoModel`) trả về `DateOnly?` |
| **File Types** | 2 loại: BanGiaoHoSo, BienBanBanGiao |
| **Delete Condition** | Chỉ TrangThai = 0 |
| **Update Condition** | Chỉ TrangThai = 0 |
| **Ban-Giao Logic** | 0→1, set NgayBanGiao, save biên bản |
| **User Filter** | Luôn filter theo `CreatedBy == _userProvider.Id.ToString()` (`CreatedBy` là `string` trong `Entity<T>`) |
| **Transaction** | ReadCommitted isolation level |

---

**Version:** 8.0  
**Last Updated:** 13/05/2026  
**Prepared By:** Design Phase  
**Status:** Implemented ✅
