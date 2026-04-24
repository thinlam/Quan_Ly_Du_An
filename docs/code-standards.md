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

---
*This document is a living guide and will be updated as needed.*
