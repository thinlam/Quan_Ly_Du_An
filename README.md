# QLDA - Quản Lý Dự Án (Project Management System)

A .NET 8.0 Clean Architecture Web API for managing government IT projects in Ho Chi Minh City, Vietnam.

## Overview

QLDA streamlines the full lifecycle of government IT projects—from planning and bidding through execution, financial management, and reporting—ensuring transparency and compliance.

## Technology Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 8.0 |
| API | ASP.NET Core Web API |
| Database | SQL Server + EF Core + Dapper |
| CQRS | MediatR |
| Validation | FluentValidation |
| Authentication | JWT Bearer |
| Documents | Aspose (Office processing) |

## Solution Structure

```
QLDA.sln
├── QLDA.Domain/           # Core entities, interfaces, business rules
├── QLDA.Application/      # CQRS handlers, DTOs, validators
├── QLDA.Infrastructure/   # External services (Aspose, email)
├── QLDA.Persistence/      # EF Core DbContext, repositories
├── QLDA.WebApi/           # Controllers, middleware, JWT auth
├── QLDA.Migrator/         # Database migration tool
├── QLDA.FakeDataTool/     # CLI fake data generator (fake.bat)
└── QLDA.Tests/            # Unit + integration tests (test.bat)
```

## Core Modules

| Module | Description |
|--------|-------------|
| **DuAn** | Project management with hierarchical structure |
| **GoiThau** | Bid package and tender management |
| **HopDong** | Contract lifecycle management |
| **ThanhToan/TamUng** | Payment and advance processing |
| **BaoCao** | Progress reporting |
| **VanBanQuyetDinh** | Legal decision documents |
| **DanhMuc*** | Master data (categories, statuses) |

## API Endpoints

Routes follow kebab-case convention:

```
/api/{resource}/danh-sach     # List (paginated)
/api/{resource}/{id}/chi-tiet # Detail
/api/{resource}/them-moi      # Create
/api/{resource}/cap-nhat      # Update
/api/{resource}/xoa-tam       # Soft delete
```

**Authentication:**
```
POST /api/auth/login    # Get JWT token
POST /api/auth/refresh  # Refresh token
POST /api/auth/logout   # Revoke session
```

## Developer Workflows

### Workflow 1: SQLite Local (no SQL Server needed)

For individual developers working locally. Zero infrastructure dependency.

```bash
# 1. Clone
git clone <repository-url> && cd SER

# 2. Create SQLite database + schema
ef.bat update --sqlite           # → dev-data.db

# 3. Seed fake data
fake.bat all 20                  # → 60 records (DuAn + GoiThau + HopDong)

# 4. Run tests (in-memory SQLite, independent of dev-data.db)
test.bat

# 5. Create migration if schema changed
ef.bat add DescribeChange        # → generates migration file
ef.bat update --sqlite           # → recreate SQLite with new schema
fake.bat clear && fake.bat all 20

# 6. Ready for review? Push branch, CI runs same tests on server
```

**Use when:** Developing features, writing tests, iterating on schema changes. No SQL Server access required.

### Workflow 2: SQL Server Dev Schema

For shared development environment. Uses `--schema dev` to isolate data.

```bash
# 1. Clone + configure SQL Server connection in appsettings.json

# 2. Apply migrations to dev schema
ef.bat update                    # → SQL Server (default connection)

# 3. Seed data to dev schema
fake.bat all 50 --schema dev     # → isolated in [dev] schema

# 4. Run WebApi pointing to dev schema
dotnet run --project QLDA.WebApi

# 5. Verify in Swagger against real SQL Server

# 6. Run tests locally before push
test.bat
```

**Use when:** Testing against real SQL Server, shared team environment, validating SQL-specific features (Dapper queries, stored procedures).

### Workflow 3: Staging/Production Deployment

For deploying to real environments. Migrations + verified build only.

```bash
# 1. CI pipeline runs (automated)
dotnet build SER.sln
dotnet test QLDA.Tests           # All tests must pass

# 2. Apply migrations to target database
ef.bat update                    # → Production SQL Server
# Or: dotnet run --project QLDA.Migrator -- --provider sqlserver

# 3. Deploy WebApi
dotnet publish QLDA.WebApi -c Release -o ./publish
# Deploy to server / container

# 4. (Optional) Seed staging data for QA
fake.bat all 100 --schema staging
```

**Use when:** Merging to main, deploying to staging/prod. CI gate ensures tests pass before migration runs.

### Workflow Comparison

| Aspect | SQLite Local | SQL Server Dev | Staging/Prod |
|--------|-------------|----------------|--------------|
| Infrastructure | None | SQL Server | SQL Server |
| Database | `dev-data.db` (file) | `[dev]` schema | `[dbo]` schema |
| Schema method | `EnsureCreated()` | `Database.Migrate()` | `Database.Migrate()` |
| Seed data | `fake.bat all 20` | `fake.bat all 50 --schema dev` | `fake.bat all 100 --schema staging` |
| Tests | `test.bat` | `test.bat` | CI pipeline |
| Verify | Tests pass | Swagger + SQL | CI + QA |
| Risk | Zero | Low | Controlled |

Deep dive: [docs/testing-guide.md](docs/testing-guide.md)

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server 2019+ (optional — Workflow 1 uses SQLite only)
- Visual Studio 2022 / VS Code

### Setup

1. **Clone and configure:**
   ```bash
   git clone <repository-url>
   cd SER
   ```

2. **Update connection string** in `QLDA.WebApi/appsettings.json`

3. **Apply migrations:**
   ```bash
   ef.bat update              # SQL Server
   ef.bat update --sqlite     # or SQLite for local dev
   ```

4. **Start the API:**
   ```bash
   dotnet run --project QLDA.WebApi
   ```

5. **Access Swagger:** `http://localhost:{port}/swagger`

### Database Migrations

Use `ef.bat` for all migration operations:

```bash
# Add a new migration
ef.bat add <MigrationName>

# Remove last migration
ef.bat remove

# Apply to SQL Server (default)
ef.bat update

# Apply to SQLite
ef.bat update --sqlite

# List all migrations
ef.bat list
```

| Command | Description |
|---------|-------------|
| `ef.bat add <Name>` | Generate migration file (SQL Server) |
| `ef.bat remove` | Remove last migration |
| `ef.bat remove 0` | Remove ALL migrations |
| `ef.bat update` | Apply to SQL Server via Migrator |
| `ef.bat update --sqlite` | Create SQLite DB via Migrator |
| `ef.bat list` | List all migrations |

Connection strings are configured in `QLDA.Migrator/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QLDA;Trusted_Connection=True;TrustServerCertificate=True;",
    "SqliteConnection": "DataSource=dev-data.db"
  }
}
```

**How it works:**
| Provider | Method | Source |
|----------|--------|--------|
| `sqlserver` (default) | `Database.Migrate()` | Migration files with retry policy |
| `sqlite` | `Database.EnsureCreated()` | EF Core model (shared entities) |

**SQLite note:** Migration files contain SQL Server-specific SQL (`SYSDATETIMEOFFSET()`, etc.), so SQLite uses `EnsureCreated()` which creates schema from the shared EF Core model. `SqliteAppDbContext` automatically replaces SQL Server defaults with SQLite equivalents.

### Seed Fake Data

Dùng `fake.bat` để tạo dữ liệu giả cho development/testing:

```bash
# Seed 10 DuAn vào SQLite file (dev-data.db)
fake.bat da 10

# Seed 5 GoiThau (tự tạo DuAn nếu chưa đủ)
fake.bat gt 5

# Seed 3 HopDong (tự tạo DuAn + GoiThau nếu chưa đủ)
fake.bat hd 3

# Seed toàn bộ: 20 DuAn + 20 GoiThau + 20 HopDong
fake.bat all 20

# Seed vào SQL Server (dev schema)
fake.bat all 50 --schema dev

# Xóa data đã seed
fake.bat clear
```

Chi tiết: [docs/testing-guide.md](docs/testing-guide.md) → section **FakeDataTool CLI**

### Run Tests

```bash
# Build + chạy tất cả tests (unit + integration)
test.bat

# Chỉ chạy integration tests (HTTP endpoint)
test.bat int

# Chạy theo controller
test.bat duan       # DuAn: GetChiTiet, Create, Update, SoftDelete
test.bat goithau    # GoiThau: GetChiTiet, Create, Update, Delete
test.bat hopdong    # HopDong: GetChiTiet, Create, Update, Delete

# Build only
test.bat build
```

Chi tiết: [docs/testing-guide.md](docs/testing-guide.md) → section **HTTP Integration Tests**

## Architecture

The system follows Clean Architecture with strict dependency rules:

```
WebApi → Application → Domain ← Infrastructure/Persistence
```

**Key Patterns:**
- CQRS with MediatR (Commands/Queries separation)
- Repository + Unit of Work
- MediatR Pipeline Behaviors (logging, validation, exceptions)
- Soft Delete with `DeletedAt`
- Materialized Path for hierarchical data

## Documentation

| Document | Description |
|----------|-------------|
| [Project Overview & PDR](docs/project-overview-pdr.md) | Requirements, scope, NFRs |
| [Codebase Summary](docs/codebase-summary.md) | Solution structure, entry points |
| [Code Standards](docs/code-standards.md) | Naming, patterns, best practices |
| [System Architecture](docs/system-architecture.md) | Layers, flows, deployment |
| [Architecture (Vietnamese)](docs/architecture.md) | Detailed architecture diagrams |
| [Data Model](docs/data-model.md) | Database schema and ERD |
| [Features](docs/features.md) | Use cases and modules |
| [Testing Guide](docs/testing-guide.md) | Unit tests, integration tests, FakeDataTool CLI |

## Development Guidelines

- Use Vietnamese for entity property names matching business terms
- Use kebab-case for API routes
- Follow CQRS: Commands modify state, Queries read data
- Validate with FluentValidation in Application layer
- Use Dapper for complex read queries, EF Core for writes
- Soft delete via `DeletedAt` field

## Project Structure

```
QLDA.Application/
├── Auth/                 # Login, logout, refresh commands
├── Common/
│   ├── Behaviors/        # MediatR pipeline (logging, validation)
│   ├── DTOs/             # ResultApi, PaginatedList, UserInfo
│   └── Interfaces/       # IUserProvider, ICrudService
├── {Feature}/
│   ├── Commands/         # Create, Update, Delete operations
│   ├── Queries/          # List, GetById operations
│   ├── DTOs/             # Feature-specific DTOs
│   └── Validators/       # FluentValidation rules
└── DependencyInjection.cs

QLDA.WebApi/
├── Controllers/          # API endpoints
├── Middleware/           # Error handling, auth
├── ConfigurationOptions/ # AppSettings
└── Program.cs            # Entry point
```

## Contributing

1. Follow the [Developer Workflow](#developer-workflow)
2. Follow Clean Architecture principles
3. Write validators for all commands
4. Add DTOs for API boundaries (never expose entities)
5. Use async/await throughout
6. Run `test.bat` before committing — all tests must pass
7. Update documentation for significant changes

## Contact

**Phòng CNTT - UBND Thành phố Hồ Chí Minh**
(IT Department - Ho Chi Minh City People's Committee)

---
*Last updated: 2026-04-28*
