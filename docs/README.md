# Hệ thống Quản Lý Dự Án (QLDA)

## Tổng quan

Hệ thống QLDA là một ứng dụng web API được xây dựng bằng .NET Core sử dụng kiến trúc Clean Architecture, chuyên dụng cho việc quản lý các dự án xây dựng và đầu tư công nghệ thông tin của các cơ quan nhà nước, đặc biệt là Thành phố Hồ Chí Minh.

## Mục đích

Hệ thống được thiết kế để:
- Quản lý toàn bộ vòng đời của các dự án CNTT từ khâu chuẩn bị đầu tư đến nghiệm thu bàn giao
- Theo dõi tiến độ thực hiện dự án theo quy trình phê duyệt
- Quản lý gói thầu, hợp đồng, thanh toán và nghiệm thu
- Báo cáo và thống kê tình hình thực hiện dự án
- Hỗ trợ ra quyết định cho lãnh đạo các cấp

## Công nghệ sử dụng

- **Backend**: .NET 8.0, ASP.NET Core Web API
- **Database**: SQL Server với Entity Framework Core
- **Architecture**: Clean Architecture (Domain-Driven Design)
- **Authentication**: JWT Bearer Token
- **Documentation**: Swagger/OpenAPI
- **CQRS Pattern**: MediatR cho xử lý commands và queries
- **ORM**: Entity Framework Core với Code First approach

## Cấu trúc dự án

```
QLDA.Solution/
├── QLDA.Domain/           # Lớp miền (Domain Layer)
├── QLDA.Application/      # Lớp ứng dụng (Application Layer)
├── QLDA.Infrastructure/   # Lớp hạ tầng (Infrastructure Layer)
├── QLDA.Persistence/      # Lớp truy cập dữ liệu (Persistence Layer)
├── QLDA.WebApi/          # Lớp trình diễn API (Presentation Layer)
└── QLDA.Migrator/        # Công cụ migration database
```

## Các module chính

1. **Quản lý Dự án**: Tạo, cập nhật, theo dõi tiến độ dự án
2. **Quản lý Gói Thầu**: Lập kế hoạch lựa chọn nhà thầu, đấu thầu
3. **Quản lý Hợp đồng**: Ký kết và quản lý hợp đồng
4. **Quản lý Tài chính**: Thanh toán, tạm ứng, quyết toán
5. **Báo cáo và Nghiệm thu**: Báo cáo tiến độ, nghiệm thu bàn giao
6. **Quản lý Văn bản**: Quyết định phê duyệt, văn bản pháp lý

## Yêu cầu hệ thống

- .NET 8.0 SDK
- SQL Server 2019+
- Visual Studio 2022 hoặc VS Code

## Cài đặt và chạy

1. Clone repository
2. Cập nhật connection string trong `appsettings.json`
3. Chạy migration: `dotnet ef database update`
4. Chạy ứng dụng: `dotnet run`

## Tài liệu bổ sung

- [Use Cases và Tính năng](features.md)
- [Kiến trúc hệ thống](system-architecture.md)
- [Mô hình dữ liệu](data-model.md)
- [API Documentation](api.md)

## Đóng góp

Vui lòng tuân thủ các quy tắc:
- Sử dụng tiếng Việt cho comments và documentation
- Tuân thủ Clean Architecture principles
- Viết unit tests cho business logic
- Cập nhật documentation khi thay đổi code

## Liên hệ

Phòng CNTT - Ủy ban nhân dân Thành phố Hồ Chí Minh