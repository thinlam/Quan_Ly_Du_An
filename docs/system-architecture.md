# System Architecture - QLDA

This document details the system architecture of the QLDA (Quản Lý Dự Án - Project Management System) application, focusing on its layered structure, request/response flow, data access patterns, authentication/authorization, cross-cutting concerns, and deployment considerations.

## 1. Layered Architecture (Clean Architecture)

The QLDA system is built upon a Clean Architecture approach, emphasizing separation of concerns and dependency inversion. This creates a highly maintainable, testable, and flexible codebase.

```mermaid
graph TD
    A[Presentation Layer<br>QLDA.WebApi] --> B[Application Layer<br>QLDA.Application]
    B --> C[Domain Layer<br>QLDA.Domain]
    B --> D[Infrastructure Layer<br>QLDA.Infrastructure]
    B --> E[Persistence Layer<br>QLDA.Persistence]
    D --> C
    E --> C
    F[Database<br>SQL Server] --> E
    G[External Services<br>Aspose, etc.] --> D
```

-   **Domain Layer (`QLDA.Domain`):** Contains enterprise-wide business rules. It is the core of the application and holds entities, value objects, domain services, and interfaces. It has no dependencies on other layers.
-   **Application Layer (`QLDA.Application`):** Contains application-specific business rules and orchestrates the domain layer to fulfill use cases. It implements the CQRS pattern with MediatR, DTOs, and validators. It depends only on the Domain layer.
-   **Infrastructure Layer (`QLDA.Infrastructure`):** Houses implementations for external concerns like file systems, email services, date/time services, and other third-party integrations (e.g., Aspose). It depends on the Application and Domain layers.
-   **Persistence Layer (`QLDA.Persistence`):** Responsible for data access, including EF Core DbContext, migrations, and repository implementations (both EF Core and Dapper). It depends on the Application and Domain layers.
-   **Presentation Layer (`QLDA.WebApi`):** The outermost layer, exposing the Web API endpoints. It handles HTTP requests, authentication, and serialization. It depends on the Application, Infrastructure, and Persistence layers.

## 2. Request/Response Flow (CQRS with MediatR)

The system leverages the CQRS (Command Query Responsibility Segregation) pattern, facilitated by MediatR, to clearly separate operations that change state (Commands) from operations that retrieve data (Queries).

```mermaid
graph TD
    A[Client Request] --> B[QLDA.WebApi Controller]
    B -- Command (Write) --> C[MediatR Dispatcher]
    C -- Pipeline Behaviors --> D[Validation Behavior]
    D --> E[Logging Behavior]
    E --> F[Performance Behavior]
    F --> G[UnhandledExceptionBehavior]
    G --> H[Command Handler<br>(QLDA.Application)]
    H --> I[Domain Entities<br>(QLDA.Domain)]
    I --> J[Repository<br>(QLDA.Persistence)]
    J -- EF Core/Dapper --> K[SQL Server]
    H -- Updates State --> K
    C -- Query (Read) --> L[Query Handler<br>(QLDA.Application)]
    L --> J
    J -- EF Core/Dapper --> K
    L --> M[DTO Mapping]
    M --> N[Client Response]
    K -- Data --> J
    J -- Data --> L
    L -- DTO --> M
    M -- DTO --> B
    B -- Response --> A
```

1.  **Client Request:** A client sends an HTTP request to the QLDA.WebApi.
2.  **Controller:** The `QLDA.WebApi` controller receives the request.
3.  **MediatR Dispatcher:** The controller dispatches either a `Command` (for state changes) or a `Query` (for data retrieval) to MediatR.
4.  **Pipeline Behaviors:** The command/query passes through a series of pipeline behaviors (e.g., Validation, Logging, Performance, Unhandled Exception) implemented in `QLDA.Application/Common/Behaviours`.
5.  **Command Handler:** For Commands, the dedicated `CommandHandler` (in `QLDA.Application`) executes the business logic, interacts with `Domain Entities`, and uses `Repositories` (from `QLDA.Persistence`) to persist changes to the `SQL Server` database.
6.  **Query Handler:** For Queries, the `QueryHandler` (in `QLDA.Application`) retrieves data using `Repositories` (from `QLDA.Persistence`), which in turn query the `SQL Server` database.
7.  **DTO Mapping:** Retrieved domain entities are mapped to appropriate DTOs by the `QueryHandler`.
8.  **Client Response:** The `QLDA.WebApi` controller returns the DTO as an HTTP response to the client.

## 3. Data Access Patterns (EF Core + Dapper)

The system employs a hybrid data access strategy, combining the benefits of Entity Framework Core (ORM) and Dapper (micro-ORM).

```mermaid
graph LR
    A[QLDA.Persistence] --> B{Data Access Layer}
    B -- Write Operations <br> (CRUD, Complex Transactions) --> C[Entity Framework Core]
    B -- Read Operations <br> (Complex Queries, Projections) --> D[Dapper]
    C --> E[SQL Server]
    D --> E
```

-   **Entity Framework Core:** Used for most CRUD operations and complex transactions where object-relational mapping capabilities are beneficial. It handles tracking changes, concurrency, and relationship management. Configured with `AggregateRootConfiguration` pattern for entity configurations.
-   **Dapper:** Employed for highly optimized read operations, especially for complex queries, reporting, or scenarios where direct SQL query execution provides significant performance advantages. This bypasses EF Core's change tracking overhead for reads.
-   **Repository Pattern:** A generic repository pattern is implemented with `Repository<TEntity,TKey>`, exposing common data access methods. Specific repositories might extend this for entity-specific operations.
-   **Unit of Work:** The `AppDbContext` acts as the Unit of Work, coordinating changes across multiple repositories and ensuring atomicity.

## 4. Authentication and Authorization Flow

The API secures its resources using JWT Bearer authentication.

```mermaid
sequenceDiagram
    participant C as Client
    participant A as QLDA.WebApi<br>(Auth Controller)
    participant B as QLDA.Application<br>(Auth Service)
    participant D as QLDA.Persistence<br>(User Repository)
    participant E as SQL Server

    C->>A: POST /api/auth/login<br>(username, password)
    A->>B: AuthenticateUser(username, password)
    B->>D: GetUserByCredentials(username)
    D->>E: Query User Table
    E-->>D: User Data
    D-->>B: User Object
    B->>B: Validate Password<br>Generate JWT
    B-->>A: JWT Token
    A-->>C: HTTP 200 OK<br>{ "token": "..." }

    C->>A: GET /api/du-an/danh-sach<br>Authorization: Bearer <token>
    A->>A: Validate JWT Token<br>Extract User Claims
    A->>A: Authorization Policy Check (e.g., [Authorize])
    alt Authorization Success
        A->>B: Dispatch Query/Command
        B-->>A: Data/Result
        A-->>C: HTTP 200 OK<br>Resource Data
    else Authorization Failure
        A-->>C: HTTP 401/403
    end
```

-   **Authentication:** Users send credentials (username/password) to the `/api/auth/login` endpoint. The `AuthService` in the `Application` layer validates these against the `UserRepository` in `Persistence` (which queries the database). Upon successful validation, a JWT token is generated and returned to the client using `JwtSettings` configuration.
-   **Authorization:** For subsequent requests, the client includes the JWT token in the `Authorization: Bearer` header. The `QLDA.WebApi` validates the token and applies ASP.NET Core Authorization Policies (`[Authorize]` attributes on controllers/actions) to enforce access control.

## 5. Entity Architecture and Relationships

The system implements a comprehensive entity model for project management:

### Core Entities
- **DuAn (Project)**: Main project entity with hierarchical structure via `ParentId` (MaterializedPathEntity pattern)
- **GoiThau (Tender Package)**: Tender package management with bid process tracking
- **HopDong (Contract)**: Contract management with terms and conditions
- **ThanhToan (Payment)**: Payment and financial transaction tracking
- **BaoCao (Report)**: Report generation and management
- **VanBanQuyetDinh (Decision Documents)**: Official decision documents
- **DanhMuc (Master Data)**: Category/reference data tables (LoaiDuAn, TrangThaiDuAn, etc.)

### Relationship Patterns
- **Hierarchical**: `DuAn` implements Materialized Path pattern for parent-child relationships
- **Many-to-Many**: Junction entities for complex relationships (e.g., `DuAnNguonVon`)
- **Soft Delete**: All entities support soft deletion via `IsDeleted` property
- **Audit Trail**: CreatedAt/UpdatedAt and CreatedBy/UpdatedBy for tracking changes

## 6. Cross-Cutting Concerns

Cross-cutting concerns are managed through various mechanisms to keep the core business logic clean.

-   **Logging:** Integrated using Serilog (or similar) across all layers, configured in `QLDA.WebApi`. MediatR pipeline behaviors provide centralized logging for command/query execution.
-   **Validation:** Handled by FluentValidation, integrated into the MediatR pipeline as a behavior in the Application layer, ensuring all incoming commands and DTOs are validated before execution.
-   **Error Handling:** A global `ExceptionMiddleware` in `QLDA.WebApi` catches unhandled exceptions and returns standardized responses to the client, preventing sensitive information leakage.
-   **Auditing:** `BaseEntity` includes `CreatedBy`, `CreatedAt`, `LastModifiedBy`, `LastModifiedAt`, `IsDeleted` fields, automatically populated to track data changes and implement soft deletes.
-   **Performance:** MediatR `PerformanceBehavior` monitors execution time of commands and queries, with configurable thresholds for performance monitoring.
-   **SQL Exception Parsing:** Custom exception handling parses SQL exceptions to provide meaningful error responses to clients.

## 7. File Processing Architecture

The system integrates Aspose.Cells (v20.11.0) for Excel processing with a flexible architecture:

```mermaid
graph TD
    A[QLDA.Infrastructure.Files] --> B{Processing Helper}
    B -- Export Operations --> C[IExporterHelper<br>IAsposeHelper]
    B -- Import Operations --> D[IImporterHelper<br>IAsposeHelper]
    C --> E[Aspose.Cells API]
    D --> E
    E --> F[Excel Templates]
    F --> G[Data Processing]
    G --> A
```

-   **Interface Abstraction**: `IExporterHelper`, `IImporterHelper`, `IAsposeHelper` interfaces provide abstraction over Aspose.Cells
-   **Template-based**: Excel export/import operations use predefined templates
-   **Flexibility**: Interface-based design allows for easy replacement of Excel processing engines

## 8. Deployment Considerations

-   **Containerization:** The `QLDA.WebApi` application can be containerized using Docker for consistent environments and easier deployment to platforms like Kubernetes or Azure App Services.
-   **CI/CD Pipeline:** Automated Continuous Integration/Continuous Deployment pipelines (e.g., Azure DevOps, GitHub Actions) for building, testing, and deploying the application and its migrations.
-   **Configuration Management:** Use environment variables and `appsettings.json` for application configuration, separating environment-specific settings including `JwtSettings`.
-   **Monitoring:** Integrate with application performance monitoring (APM) tools (e.g., Application Insights, Prometheus/Grafana) to monitor health, performance, and errors in production.
-   **Database Migrations:** Currently using squashed migration approach with single initial migration for simplified deployment.

---
*This document will be updated as the system evolves.*
