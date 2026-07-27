# Code Standards and Best Practices - QLDA

This document outlines the coding standards and best practices to be followed during the development of the QLDA (Quản Lý Dự Án) project. Adherence to these standards ensures consistency, maintainability, and quality across the codebase.

## 1. Naming Conventions

Consistency in naming is crucial for readability and understanding.

-   **General Naming:** Follow C# naming conventions (PascalCase for classes, methods, properties; camelCase for local variables).
-   **Vietnamese Property Names:** While English is preferred for general code, Vietnamese names for properties in DTOs and Entities are acceptable if they directly map to business domain terms, particularly for display purposes. Ensure these are consistently applied.
    -   Example: `TenDuAn` (Project Name), `NgayBatDau` (StartDate).
-   **Kebab-Case Routes:** API endpoints should follow kebab-case for better readability in URLs.
    -   Example: `/api/du-an/chi-tiet` instead of `/api/DuAn/ChiTiet`.
-   **Database Columns:** Database column names should generally match entity property names (PascalCase by default with EF Core). For many-to-many junction tables, use clear descriptive names combining the two entities (e.g., `DuAnNguonVon`).
-   **File Organization:** Use feature-based folder structure following `{EntityName}/{Commands,Queries,DTOs,Validators}` pattern in the Application layer.

## 2. Clean Architecture and Solution Structure

Adhere strictly to the Clean Architecture principles.

-   **Layer Responsibility:** Each project (`Domain`, `Application`, `Infrastructure`, `Persistence`, `WebApi`) has clearly defined responsibilities.
-   **Dependency Rule:** Dependencies should flow inwards, meaning inner layers should not depend on outer layers.
    -   `Domain` is independent.
    -   `Application` depends on `Domain`.
    -   `Infrastructure` and `Persistence` depend on `Application` and `Domain`.
    -   `WebApi` depends on `Application`, `Infrastructure`, and `Persistence`.
-   **Abstraction:** Use interfaces in inner layers (`Domain`, `Application`) to define contracts for outer layer implementations (`Infrastructure`, `Persistence`).

## 3. CQRS Implementation Patterns (MediatR)

The application extensively uses the CQRS pattern with MediatR.

-   **Command/Query Separation:**
    -   **Commands:** Represent actions that change state. They should return `ResultApi<T>` pattern with Ok/Fail methods.
    -   **Queries:** Represent requests for data and do not change state. They should return DTOs optimized for the consumer via `ResultApi<T>` pattern.
-   **`Features` Organization:** Organize commands, queries, handlers, DTOs, and validators within feature-specific folders in the `QLDA.Application` project.
    -   Example: `QLDA.Application/Features/DuAnFeatures/Commands/CreateDuAn/CreateDuAnCommand.cs`.
-   **MediatR Pipeline Behaviors:** Utilize MediatR pipeline behaviors for cross-cutting concerns.
    -   **Validation:** `ValidationBehavior` to automatically apply FluentValidation.
    -   **Logging:** `LoggingBehavior` to log command/query execution.
    -   **Performance:** `PerformanceBehavior` to measure execution time.
    -   **Exception Handling:** `UnhandledExceptionBehavior` for consistent error handling.

## 4. Entity Configuration Patterns (EF Core)

-   **Fluent API:** Use EF Core's Fluent API for entity configurations within the `QLDA.Persistence/Configurations` folder using `AggregateRootConfiguration<TEntity>` pattern.
    -   Each entity should have its own configuration class (e.g., `DuAnConfiguration.cs` implementing `IEntityTypeConfiguration<DuAn>`).
-   **BaseEntity:** All entities should inherit from a `BaseEntity` to include common properties like `Id`, `CreatedBy`, `CreatedAt`, `LastModifiedBy`, `LastModifiedAt`, `IsDeleted`.
-   **Soft Delete:** Implement soft delete functionality using the `IsDeleted` property. Global query filters should be applied in `AppDbContext` to exclude soft-deleted entities by default.
-   **Materialized Path:** For hierarchical entities (e.g., `DuAn` with `ParentId`), implement Materialized Path pattern for efficient querying of trees. Ensure path update logic is consistent.

## 5. DTO and Mapping Patterns

-   **DTOs (Data Transfer Objects):** Use DTOs for data exchange between application layers and API consumers.
    -   Separate DTOs for requests (e.g., `CreateDuAnRequest`, `UpdateDuAnRequest`) and responses (e.g., `DuAnResponse`, `DuAnListItem`).
    -   Avoid exposing domain entities directly through the API.
-   **ResultApi Pattern:** Use `ResultApi<T>` pattern for all API responses with `Ok()` and `Fail()` factory methods for consistent return types.
-   **AutoMapper:** Use AutoMapper for mapping between entities and DTOs. Configure mappings in `QLDA.Application/Common/Mappings`.
    -   Utilize `IAutoMapFrom<T>` and `IAutoMapTo<T>` interfaces for convention-based mapping.

## 6. Validation Patterns

-   **FluentValidation:** Use FluentValidation for all input validation in the `QLDA.Application` layer.
    -   Create a dedicated validator class for each command and DTO (e.g., `CreateDuAnCommandValidator.cs`).
    -   Ensure validation rules cover business constraints and data integrity.
    -   Integrate FluentValidation into the MediatR pipeline via `ValidationBehavior`.

## 7. Error Handling Patterns

-   **Centralized Error Handling:** Implement a global `ExceptionMiddleware` in `QLDA.WebApi` to catch unhandled exceptions and return consistent, structured error responses.
-   **Custom Exceptions:** Define custom exception types in `QLDA.Domain` for specific business rule violations.
-   **Logging:** Ensure all errors and exceptions are logged with sufficient detail for debugging and monitoring, but without exposing sensitive user data.
-   **SQL Exception Parsing:** The middleware should parse SQL exceptions to provide meaningful error messages to clients.

## 8. Testing Patterns

-   **Unit Tests:** Focus on testing individual components (e.g., domain logic, application handlers, validators) in isolation.
    -   Use mocking frameworks (e.g., Moq) for dependencies.
-   **Integration Tests:** Test the interaction between multiple components (e.g., application handlers with persistence layer, API endpoints).
    -   Use in-memory databases or test databases for persistence tests.
-   **Test-Driven Development (TDD):** Encourage TDD approach where applicable, writing tests before implementation.
-   **Naming:** Test methods should clearly indicate what is being tested and the expected outcome (e.g., `Should_ReturnDuAn_When_IdIsValid`).
-   **Mandatory Regression Run:** Sau khi triển khai bất kỳ endpoint nào (thêm mới, sửa đổi, hoặc refactor), phải chạy lại toàn bộ test suite (`test.bat` hoặc `dotnet test`) để đảm bảo không gây lỗi regression cho các tính năng hiện có. Không được commit code khi có test failing.

## 9. Security Protocols

-   **JWT Bearer Authentication:** Secure API endpoints using JWT tokens with configurable `JwtSettings`.
-   **Role-Based Access Control (RBAC):** Implement authorization checks based on user roles and permissions.
-   **Input Validation:** Prevent injection attacks and other vulnerabilities through comprehensive input validation.
-   **Sensitive Data:** Never log sensitive data (passwords, PII).

## 10. Performance Optimization

-   **Dapper for Reads:** Utilize Dapper in `QLDA.Persistence` for highly performant read operations, especially for complex queries or large data sets, to complement EF Core.
-   **Asynchronous Operations:** Use `async/await` throughout the application for I/O-bound operations to improve scalability.
-   **Caching:** Implement caching strategies (e.g., response caching for 12 hours on combobox endpoints) for frequently accessed data that does not change often.
-   **Performance Monitoring:** Use the `PerformanceBehavior` in MediatR pipeline to monitor execution times of commands and queries.

## 11. Infrastructure Integration

-   **Aspose Integration:** Use interface abstractions (`IExporterHelper`, `IImporterHelper`, `IAsposeHelper`) for Excel processing to allow flexibility and testability.
-   **Template-Based Processing:** Implement template-based Excel export/import functionality rather than hardcoded formats.

## 12. Data Access Best Practices

-   **Generic Repository Pattern:** Use the `Repository<TEntity,TKey>` generic implementation for standard CRUD operations.
-   **Unit of Work Pattern:** Implement `IUnitOfWork` interface through `AppDbContext` to coordinate changes across multiple repositories.
-   **EF Core Configuration:** Follow the `AggregateRootConfiguration<TEntity>` pattern for consistent entity configuration.

## 13. Constants and Hardcoded Strings

**Nguyên tắc:** Không dùng string literal trực tiếp trong code — phải tạo constant rồi đối chiếu qua constant.

### Tại sao
- Tránh inconsistency: nếu có lỗi chính tả, IDE không warning
- Khi cần thay đổi giá trị, chỉ sửa 1 chỗ duy nhất
- Code dễ tìm kiếm (grep) theo constant name

### Các loại constant bắt buộc dùng

| Loại | File constant | Ví dụ |
|------|-------------|-------|
| Mã trạng thái phê duyệt | `TrangThaiPheDuyetCodes.cs` | `TrangThaiPheDuyetCodes.MyEntity.DuThao` |
| Tên entity phê duyệt | `PheDuyetEntityNames.cs` | `PheDuyetEntityNames.MyEntity` |
| Role constants | `RoleConstants.cs` | `RoleConstants.QLDA_LDDV` |
| Các giá trị cố định khác | Tạo trong `Domain/Constants/` | |

### Ví dụ đúng
```csharp
// ✅ ĐÚNG — dùng constant
.FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.DuThao
    && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh)

// ✅ ĐÚNG — kiểm tra null-safe
if (entity.TrangThaiId != trangThaiDuThao?.Id)

// ✅ ĐÚNG — string.IsNullOrWhiteSpace cho NoiDung
if (string.IsNullOrWhiteSpace(request.NoiDung))
```

### Ví dụ sai
```csharp
// ❌ SAI — hardcode string "DDC", "CTD", "CPD", "DPD"
.FirstOrDefaultAsync(s => s.Ma == "DDC" && s.Loai == "QuyetDinhDieuChinh")

// ❌ SAI — dùng ManagedException.ThrowIfNull cho string
ManagedException.ThrowIfNull(request.NoiDung, "Phải nhập lý do")

// ❌ SAI — so sánh nullable không đúng cách
if (entity.TrangThaiId != trangThaiDDC.Id)  // NPE nếu null
```

### Quy tắc kiểm tra NoiDung (reason)
- **Luôn dùng:** `string.IsNullOrWhiteSpace(request.NoiDung)` — vì `NoiDung` là `string?`
- **Không dùng:** `ManagedException.ThrowIfNull(request.NoiDung, ...)` — sai vì string nullable không bao giờ là `null` theo nghĩa object reference

### Đặt tên status code constants
- `DuThao` = "DT" — khởi tạo
- `DaTrinh` = "ĐTr" — đã trình
- `DaDuyet` = "ĐD" — đã duyệt
- `TraLai` = "TL" — trả lại
- `TuChoi` = "TC" — từ chối

## 14. Attachment Pattern (File đính kèm)

> Thay thế pattern cũ dùng entity/command `TepDinhKem*`. Runtime entity là `BuildingBlocks.Domain.Entities.Attachment`; bảng DB vẫn giữ tên `TepDinhKem` (POCO ≠ TableName — chuẩn EF Core).

### Kiến trúc tổng quan

| Layer | Thành phần | Ghi chú |
|-------|-----------|---------|
| Domain | `Attachment` | Entity duy nhất cho mọi module |
| Application (BB) | `BuildingBlocks.Application.Attachments.*` | Commands, Queries, DTOs, Common helpers |
| Persistence (QLDA) | `AttachmentConfiguration` → `ToTable("TepDinhKem")` | Nơi duy nhất map bảng thật |
| API contract | `TepDinhKemDto`, `DanhSachTepDinhKem`, `TepDinhKemModel` | **Giữ nguyên tên** — Frontend không đổi |

**Hard rules:**
- Dùng `IRepository<Attachment, Guid>` — **không** inject entity `TepDinhKem`.
- Dùng chung BB commands/queries — **không** tạo command/query riêng trong module.
- **Không** rename property DTO/API (`DanhSachTepDinhKem`, `TepDinhKemDto`, …).
- **Không** thêm field vào `AttachmentDto` / `IAttachmentDto` chỉ vì 1 module — tạo class con (vd: `TepDinhKemDto : AttachmentDto`).

### GroupId + GroupType

Attachment không có FK trực tiếp tới entity cha. Liên kết qua:

| Field | Ý nghĩa |
|-------|---------|
| `GroupId` | `entity.Id.ToString()` của bản ghi cha |
| `GroupType` | Phân loại file trong cùng GroupId — dùng `nameof(EGroupType.X)` |
| `ParentId` | Id file gốc khi đây là bản ký số (child) |

Convention ký số (`SignedGroupTypeHelper` — single source of truth):

```csharp
// ParentId == null → GroupType = baseGroupType
// ParentId != null → GroupType = "KySo_" + baseGroupType
SignedGroupTypeHelper.ResolveSignedGroupType(baseGroupType, isChild: parentId != null);

// Strip prefix khi phân loại read-side
dto.BaseGroupType(); // "KySo_QLHD" → "QLHD"
```

### Write side — Controller-mediated sync

Controller (hoặc handler top-level) gọi `AttachmentBulkInsertOrUpdateCommand` **sau** khi entity cha đã có Id:

```csharp
var entity = model.ToEntity();
await Mediator.Send(new BaoCaoTienDoInsertOrUpdateCommand(entity));

var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
    GroupId = entity.Id.ToString(),
    GroupTypes = [nameof(EGroupType.BaoCaoTienDo)],  // bắt buộc — scope chặt
    Entities = danhSachTepDinhKem,
    AutoDeleteMissing = true   // true khi UI gửi full list và cần xóa file thiếu
});
```

Map DTO → entity qua `AttachmentCollectionExtensions.ToEntities`:

```csharp
// WebApi mapping helper (giữ tên GetDanhSachTepDinhKem)
model.DanhSachTepDinhKem.ToEntities(groupId, nameof(EGroupType.BaoCaoTienDo));
```

**Lưu ý transaction:** Handler `AttachmentBulkInsertOrUpdateCommand` tham gia transaction hiện có nếu `_unitOfWork.HasTransaction` — controller mở transaction 1 lần, commit 1 lần.

**Lưu ý multi-GroupType:** Mỗi loại file trên cùng entity cần `GroupTypes` riêng hoặc gọi command riêng — sync **không** xóa nhầm file của GroupType khác.

### Read side — Hydration pattern (Controller)

Load attachment **ngoài** query handler chính, rồi hydrate vào response model:

```csharp
var entity = await Mediator.Send(new BaoCaoTienDoGetQuery { Id = id, ... });

// Mặc định IncludeSigned = true → lấy cả file gốc và KySo_*
var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
    GroupIds: [entity.Id.ToString()],
    BaseGroupTypes: [nameof(EGroupType.BaoCaoTienDo)]
))).ToAttachmentEntities();      // bridge DTO → entity cho ToModel()

return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
```

`GetAttachmentsQuery` parameters:

| Param | Mặc định | Mô tả |
|-------|----------|-------|
| `GroupIds` | — | Danh sách GroupId (bắt buộc, không rỗng) |
| `BaseGroupTypes` | `null` | Filter theo base GroupType; `null` = lấy mọi GroupType |
| `IncludeSigned` | `true` | Tự thêm variant `KySo_*` khi filter theo `BaseGroupTypes` |

**Quy ước:**
- Mặc định luôn bao gồm file ký số — **không** truyền `IncludeSigned: true` ở từng caller.
- Chỉ opt-out tường minh khi nghiệp vụ không cần ký số: `IncludeSigned: false`.
- `IncludeSigned` **chỉ có hiệu lực khi có `BaseGroupTypes`**. Không truyền `BaseGroupTypes` → lấy mọi GroupType của GroupId (flag bị bỏ qua).
- Multi-GroupType trên 1 response: load 1 lần rồi split bằng `ToBaseGroupType()` / `.BaseGroupType()` — **không** so exact `GroupType == "X"` (sẽ loại nhầm `KySo_X`).

**Gộp vs tách file ký số:**
- **Gộp (mặc định):** 1 lần gọi `GetAttachmentsQuery` (IncludeSigned mặc định true) → filter bằng `.BaseGroupType()` khi gán property.
- **Tách:** 2 lần gọi — base (`IncludeSigned: false`) và signed (`BaseGroupTypes: ["KySo_X"]` hoặc `SignedOnly`).

### Read side — IQueryable subquery (List handlers)

Trong `.Select()` projection (tránh N+1), **không** gọi `_mediator.Send()` — dùng subquery trực tiếp:

```csharp
// Expand TRƯỚC Select — EF Core translate Contains → SQL IN
var types = AttachmentSubquery.ExpandGroupTypes(
    includeSigned: true,
    nameof(EGroupType.KhoKhanVuongMac));

return await queryable
    .Select(e => new KhoKhanVuongMacDto {
        Id = e.Id,
        DanhSachTepDinhKem = Attachment.GetQueryableSet()
            .Where(i => i.GroupId == e.Id.ToString() && types.Contains(i.GroupType))
            .Select(i => i.ToDto<TepDinhKemDto>())
            .ToList(),
    })
    .PaginatedListAsync(...);
```

Standalone (ngoài expression tree) dùng extension:

```csharp
_attachmentRepo.GetQueryableSet()
    .ForExactGroupType(id.ToString(), nameof(EGroupType.BanGiaoHoSo))
    .Select(a => a.ToDto<TepDinhKemDto>())
    .ToListAsync(ct);
```

### DTO mapping

| Loại | Pattern |
|------|---------|
| BB cross-module | `AttachmentDto` |
| QLDA (có audit + TenNguoiTao) | `TepDinhKemDto : AttachmentDto` — giữ file, không rename |
| Entity → DTO | `entity.ToDto<TepDinhKemDto>()` via `AttachmentMapping` |
| DTO → Entity (write) | `dtos.ToEntities(groupId, baseGroupType)` |

### Anti-patterns

```csharp
// ❌ Entity cũ đã xóa
IRepository<TepDinhKem, Guid>

// ❌ Command/query cũ
TepDinhKemBulkInsertOrUpdateCommand
GetDanhSachTepDinhKemQuery

// ❌ Side-effect trong EF projection
.Select(e => new Dto {
    DanhSachTepDinhKem = _mediator.Send(new GetAttachmentsQuery(...)) // throw runtime
})

// ❌ Sync không khai báo GroupTypes — xóa nhầm file loại khác
new AttachmentBulkInsertOrUpdateCommand { GroupId = id, Entities = files }

// ❌ Rename API contract
public List<AttachmentDto>? DanhSachAttachment { get; set; }
```

### Tham chiếu code

| Helper | File |
|--------|------|
| `SignedGroupTypeHelper` | `BuildingBlocks.Application/Attachments/Common/SignedGroupTypeHelper.cs` |
| `AttachmentSubquery` | `BuildingBlocks.Application/Attachments/Common/AttachmentSubquery.cs` |
| `AttachmentCollectionExtensions` | `BuildingBlocks.Application/Attachments/Common/AttachmentCollectionExtensions.cs` |
| `GetAttachmentsQuery` | `BuildingBlocks.Application/Attachments/Queries/GetAttachmentsQuery.cs` |
| `AttachmentBulkInsertOrUpdateCommand` | `BuildingBlocks.Application/Attachments/Commands/AttachmentBulkInsertOrUpdateCommand.cs` |

Chi tiết refactor: `docs/issues/Refactor Consolidate attachment files/plan.md` và các file `phase-*-implementation.md`.

---
*This document is a living guide and will be updated as needed.*
