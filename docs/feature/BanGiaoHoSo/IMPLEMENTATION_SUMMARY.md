# 📋 Tóm Tắt Thực Hiện - Feature Bàn Giao Hồ Sơ (BanGiaoHoSo)

**Status:** ✅ Thiết kế hoàn toàn, sẵn sàng code  
**Date:** 12/05/2026  
**Version:** 2.0

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
| `PhongBanChuTriId` | `long?` | FK → DanhMucDonVi |
| `TrangThai` | `ETrangThaiBanGiao` | 0: Khởi tạo, 1: Đã bàn giao |
| `NgayBanGiao` | `DateTime?` | Ngày thực hiện bàn giao |
| `UserId` | `long?` | FK → UserMaster (người tạo) |
| `IsDeleted` | `bit` | Soft delete flag |
| `CreatedAt`, `UpdatedAt` | `DateTime` | Audit fields |

**Index:** `(UserId, TrangThai)` - tối ưu query danh sách

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
| `POST` | `/api/ban-giao-ho-so/them-moi` | Thêm mới + tệp HS bàn giao | `BanGiaoHoSoModel` | `Guid` (Id) |
| `PUT` | `/api/ban-giao-ho-so/cap-nhat` | Cập nhật + tệp HS bàn giao | `BanGiaoHoSoModel` | `Guid` (Id) |
| `PUT` | `/api/ban-giao-ho-so/{id}/ban-giao` | **Thực hiện bàn giao**: đổi TrangThai 0→1, set NgayBanGiao, lưu biên bản | `BanGiaoHoSoBanGiaoModel` | `int` (1) |
| `DELETE` | `/api/ban-giao-ho-so/{id}/xoa-tam` | Xóa soft (chỉ TrangThai = 0) | `Guid id` | `int` (1) |

**Ủy quyền:** Tất cả endpoints đều `[Authorize]`

**Filter Logic:**
- Danh sách chỉ hiển thị bản giao **của chính người dùng hiện tại** (từ `IUserContext`)
- UI chỉ truyền duy nhất 1 filter param: `TrangThai` (0 hoặc 1)
- `UserId` luôn lấy từ Auth, không cho UI truyền

---

## 4. 📁 Layer-by-Layer Implementation

### 4.1 Domain Layer ✅

**File mới:**
- `QLDA.Domain/Enums/ETrangThaiBanGiao.cs` (enum)
- `QLDA.Domain/Entities/BanGiaoHoSo.cs` (aggregate root)

**Chi tiết:**
- Entity kế thừa `Entity<Guid>, IAggregateRoot`
- FK: `PhongBanChuTriId` (long?) → `DanhMucDonVi`
- FK: `UserId` (long?) → `UserMaster`
- Navigation properties: `User`, `PhongBanChuTri`

---

### 4.2 Persistence Layer ✅

**File:**
- `QLDA.Persistence/Configurations/BanGiaoHoSoConfiguration.cs` (mới)
- `QLDA.Persistence/Configurations/UserMasterConfiguration.cs` (sửa - **xoá dòng `HasNoKey()`**)

**Sửa UserMasterConfiguration:**
```csharp
// ❌ XOÁ dòng: builder.HasNoKey().ToTable("USER_MASTER");
// ✅ GIỮ: builder.HasKey(e => e.Id).HasName("...");
```
**Lý do:** Khiến UserMaster trở thành keyless entity, không cho phép FK + navigation property

**BanGiaoHoSoConfiguration:**
- Table: `BanGiaoHoSo`
- Index: `(UserId, TrangThai)`
- FK constraints: Cascade delete → SetNull
- Conversion: `TrangThai` as int

---

### 4.3 Application Layer ✅

**Folder:** `QLDA.Application/BanGiaoHoSos/`

#### DTOs (4 files)

| File | Ý nghĩa |
|------|---------|
| `BanGiaoHoSoInsertDto.cs` | Insert payload (inherit `IMayHaveTepDinhKemDto`) |
| `BanGiaoHoSoUpdateModel.cs` | Update payload (+ `Id`) |
| `BanGiaoHoSoDto.cs` | Response model (kèm cả 2 loại tệp) |
| `BanGiaoHoSoSearchDto.cs` | Filter query (chỉ `TrangThai?`) |

#### Mappings (1 file)

- `BanGiaoHoSoMappings.cs` - Entity ↔ DTO mappings

#### Commands (4 files)

| Command | Mô tả |
|---------|-------|
| `BanGiaoHoSoInsertCommand` | Thêm mới entity |
| `BanGiaoHoSoUpdateCommand` | Cập nhật entity |
| `BanGiaoHoSoBanGiaoCommand` | **Ban-giao**: TrangThai 0→1, set NgayBanGiao |
| `BanGiaoHoSoDeleteCommand` | Soft delete (chỉ TrangThai=0) |

**Xử lý Transaction:**
- Isolation Level: `ReadCommitted`
- Begin TX → Execute → SaveChanges → Commit

#### Queries (2 files)

| Query | Mô tả |
|-------|-------|
| `BanGiaoHoSoGetQuery` | Lấy 1 entity by Id (include nav properties) |
| `BanGiaoHoSoGetDanhSachQuery` | Lấy danh sách phân trang, filter theo UserId + TrangThai |

---

### 4.4 WebApi Layer ✅

**Folder:** `QLDA.WebApi/Models/BanGiaoHoSos/` + `Controllers/`

#### Models (2 files)

| Model | Ý nghĩa |
|-------|---------|
| `BanGiaoHoSoModel.cs` | Standard CRUD model (implement `IMayHaveTepDinhKemModel`) |
| `BanGiaoHoSoBanGiaoModel.cs` | Ban-giao payload (NgayBanGiao, DanhSachBienBan) |

#### Supporting Infrastructure (3 files)

| File | Ý nghĩa |
|------|---------|
| `QLDA.WebApi/Models/Common/Interfaces/IUserContext.cs` | Interface lấy UserId từ auth |
| `QLDA.WebApi/Models/Common/UserContext.cs` | Implementation IUserContext |
| `QLDA.WebApi/Models/BanGiaoHoSos/BanGiaoHoSoMappingConfiguration.cs` | Model ↔ Entity mappings + TepDinhKem helpers |

**Sửa WebApplicationExtensions.cs:**
```csharp
// Thêm vào AddCommonServices():
services.AddScoped<IUserContext>(sp => {
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    if (httpContext?.User.Identity?.IsAuthenticated != true)
        throw new UnauthorizedAccessException("User is not authenticated.");
    var userIdString = httpContext.User.FindFirst("sub")?.Value;
    if (!long.TryParse(userIdString, out var userId))
        throw new UnauthorizedAccessException("Invalid or missing 'sub' claim.");
    return new UserContext { UserId = userId };
});
```

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
✅ Cho phép: Tất cả authenticated user
❌ Không cho phép: -
✅ UserId: Auto set từ IUserContext
```

### Ban-Giao Endpoint

```
✅ Logic:
  1. Lấy entity by Id
  2. Đổi TrangThai: 0 → 1
  3. Set NgayBanGiao (default: DateTime.Now nếu null)
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
✅ Filter: UserId = người dùng hiện tại (luôn)
✅ Filter: TrangThai = ? (tuỳ chọn từ UI)
✅ No GlobalFilter: Không search full-text
```

---

## 7. ⚙️ Dependency Injection & Services

### 1. `IUserContext` Service
- **Tạo:** `/QLDA.WebApi/Models/Common/Interfaces/IUserContext.cs`
- **Implement:** `/QLDA.WebApi/Models/Common/UserContext.cs`
- **Register:** `WebApplicationExtensions.AddCommonServices()`
- **Scope:** Scoped (per-request)

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

- [x] **0. Fix UserMasterConfiguration** - Xoá `HasNoKey()`
- [x] **1. Domain - Enum ETrangThaiBanGiao**
- [x] **2. Domain - Entity BanGiaoHoSo** (PhongBanChuTriId: long?, NgayBanGiao)
- [x] **3. Persistence - BanGiaoHoSoConfiguration**
- [x] **4. Persistence - UserMasterConfiguration** (sửa)
- [x] **5. Application - DTOs** (4 files)
- [x] **6. Application - Mappings**
- [x] **7. Application - Commands** (4 files)
- [x] **8. Application - Queries** (2 files)
- [x] **9. WebApi - IUserContext + UserContext**
- [x] **10. WebApi - UserContext Registration**
- [x] **11. WebApi - Models** (BanGiaoHoSoModel, BanGiaoHoSoBanGiaoModel)
- [x] **12. WebApi - Mapping Configuration**
- [x] **13. WebApi - BanGiaoHoSoController** (6 endpoints)
- [x] **14. Enum - Add EGroupType.BanGiaoHoSo & EGroupType.BienBanBanGiao**

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
| **FK Relations** | PhongBanChuTriId (long?), UserId (long?) |
| **File Types** | 2 loại: BanGiaoHoSo, BienBanBanGiao |
| **Delete Condition** | Chỉ TrangThai = 0 |
| **Ban-Giao Logic** | 0→1, set NgayBanGiao, save biên bản |
| **User Filter** | Luôn filter theo UserId từ Auth |
| **Transaction** | ReadCommitted isolation level |

---

**Version:** 2.0  
**Last Updated:** 12/05/2026  
**Prepared By:** Design Phase  
**Status:** Ready for Implementation ✅
