# QLDA Codebase Summary

## Overview
QLDA (Quản Lý Dự Án) is a comprehensive project management system built with .NET 8.0 following Clean Architecture principles. The system manages government IT projects for Ho Chi Minh City, Vietnam, with features for project lifecycle management, tender packages, contracts, payments, and reporting.

## Solution Structure
The solution consists of 5 main projects following Clean Architecture:

- **QLDA.Domain**: Core business logic and entities
- **QLDA.Application**: CQRS pattern implementation with MediatR
- **QLDA.Infrastructure**: External service implementations (Aspose, file processing)
- **QLDA.Persistence**: Data access with EF Core and Dapper
- **QLDA.WebApi**: Web API presentation layer

## Architecture Highlights

### Domain Layer
- **Core Entities**: DuAn (Project), GoiThau (Tender Package), HopDong (Contract), ThanhToan (Payment), BaoCao (Report), VanBanQuyetDinh (Decision Documents)
- **Pattern**: MaterializedPathEntity for hierarchical data with ParentId relationships
- **Interfaces**: IRepository<TEntity,TKey>, IUnitOfWork, IAggregateRoot
- **Features**: Soft delete (IsDeleted), audit trail (CreatedAt/UpdatedAt/CreatedBy/UpdatedBy), DanhMuc master data entities

### Application Layer
- **Pattern**: CQRS with MediatR for Commands/Queries/Handlers
- **DTOs**: With ResultApi<T> pattern (Ok/Fail factory methods)
- **Validation**: FluentValidation for all commands
- **Behaviors**: Validation, Logging, Performance, UnhandledException behaviors
- **Structure**: Module-based organization (EntityName/{Commands,Queries,DTOs,Validators})

### Persistence Layer
- **Context**: AppDbContext implementing IUnitOfWork
- **Configuration**: AggregateRootConfiguration pattern with Fluent API
- **Migrations**: Single squashed migration (20260424043806_Init.cs)
- **Seed Data**: For DanhMuc tables (LoaiDuAn, TrangThaiDuAn)
- **Repository**: Generic Repository<TEntity,TKey> implementation

### WebApi Layer
- **Controllers**: DuAnController, GoiThauController, HopDongController, Auth
- **Middleware**: ExceptionMiddleware for global error handling
- **Authentication**: JWT with configurable JwtSettings
- **Features**: Response caching (12h for combobox endpoints), Swagger with Bearer token

### Infrastructure Layer
- **Integration**: Aspose.Cells (v20.11.0) for Excel processing
- **Interfaces**: IExporterHelper, IImporterHelper, IAsposeHelper for file operations
- **Templates**: Template-based Excel export/import functionality

## Technology Stack
- **Framework**: .NET 8.0, ASP.NET Core Web API
- **Database**: SQL Server + EF Core + Dapper
- **Patterns**: CQRS, Clean Architecture, Repository + Unit of Work
- **Tools**: MediatR, FluentValidation, AutoMapper, JWT
- **File Processing**: Aspose.Cells for Excel operations

## Key Features
- Hierarchical project structure with Materialized Path pattern
- Comprehensive entity relationships with junction tables
- Master data management (DanhMuc entities)
- File processing capabilities with Aspose integration
- JWT-based authentication and authorization
- CQRS implementation for scalable architecture

## Files and Statistics
- **Total Files**: 1,568 files
- **Total Tokens**: 1,297,183 tokens
- **Total Characters**: 5,359,244 chars

## Security
- **Status**: No suspicious files detected
- **Authentication**: JWT Bearer with configurable settings
- **Data Protection**: Soft delete implementation and audit trails