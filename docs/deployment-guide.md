# Deployment Guide - QLDA (Quản Lý Dự Án)

## Overview
This guide provides detailed instructions for deploying the QLDA (Quản Lý Dự Án - Project Management System) application to various environments. The system follows Clean Architecture principles with a .NET 8.0 Web API backend.

## Prerequisites

### System Requirements
- **Operating System**: Windows Server 2019+, Linux (Ubuntu 20.04+), or macOS
- **Runtime**: .NET 8.0 Runtime (for deployment) or SDK (for development)
- **Database**: SQL Server 2019+ or compatible database system
- **Web Server**: IIS, Apache, Nginx, or ability to run self-hosted applications
- **Memory**: Minimum 4GB RAM (8GB recommended for production)

### Software Dependencies
- .NET 8.0 Runtime
- SQL Server or SQL Server Express (for local deployments)
- Aspose.Cells license (for Excel functionality)

## Environment Configuration

### 1. Application Settings
Configure the `appsettings.Production.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=QLDA;Integrated Security=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "Secret": "your-very-long-jwt-secret-key-here-32-characters-minimum",
    "ExpiryMinutes": 120,
    "Issuer": "QLDA-API",
    "Audience": "QLDA-Client"
  },
  "Aspose": {
    "LicensePath": "/path/to/aspose-license.lic"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 2. Environment Variables (Recommended)
Instead of appsettings files, use environment variables for sensitive data:

```bash
# Database
CONNECTIONSTRINGS__DEFAULTCONNECTION="Server=prod-server;Database=QLDA_Prod;User Id=user;Password=password;TrustServerCertificate=true;"

# JWT
JWTSETTINGS__SECRET="production-jwt-secret-key-32-characters-min"
JWTSETTINGS__EXPIRYMINUTES="120"

# Aspose License
ASPOSE__LICENSEPATH="/app/config/aspose.lic"
```

## Database Setup

### 1. Migration Execution
Before first deployment, apply database migrations:

```bash
# Navigate to the migrator project
cd QLDA.Migrator

# Apply migrations (using .NET CLI)
dotnet run

# Alternative: Using Entity Framework tools
dotnet ef database update --project ../QLDA.Persistence --startup-project .
```

### 2. Manual Migration (Alternative)
If automatic migration is not desired:

```bash
# Generate SQL script for review
dotnet ef migrations script --project ../QLDA.Persistence --startup-project . --output migration-script.sql

# Apply the script manually using SQL Server Management Studio or similar tool
```

### 3. Seed Data
The system includes seed data for master tables (DanhMuc entities). Ensure the seed process runs during deployment:

```csharp
// In Program.cs or Startup
if (environment.IsProduction())
{
    await SeedData.InitializeAsync(serviceProvider);
}
```

## Build and Publish

### 1. Local Build
```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build -c Release

# Run tests (recommended before deployment)
dotnet test -c Release
```

### 2. Publish for Deployment
```bash
# Publish to a folder
dotnet publish ./QLDA.WebApi/QLDA.WebApi.csproj -c Release -o ./publish --self-contained -r win-x64

# For Linux deployment
dotnet publish ./QLDA.WebApi/QLDA.WebApi.csproj -c Release -o ./publish --self-contained -r linux-x64

# For runtime-specific deployment (smaller size)
dotnet publish ./QLDA.WebApi/QLDA.WebApi.csproj -c Release -o ./publish --runtime win-x64
```

### 3. Container Deployment (Docker)
If using Docker, build and push the image:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_URLS=http://+:80
COPY ./publish ./

ENTRYPOINT ["dotnet", "QLDA.WebApi.dll"]
```

```bash
# Build Docker image
docker build -t ql-da:latest .

# Tag and push to registry
docker tag ql-da:latest your-registry/ql-da:v1.0.0
docker push your-registry/ql-da:v1.0.0
```

## Web Server Configuration

### IIS Deployment
1. Install ASP.NET Core Module on IIS
2. Create new Application Pool (".NET Core" managed pipeline)
3. Create new website pointing to published folder
4. Configure application pool identity with appropriate permissions

### Reverse Proxy (Nginx/Apache)
Example Nginx configuration:

```nginx
server {
    listen 80;
    server_name ql-da.yourdomain.gov.vn;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## Security Configuration

### 1. HTTPS Setup
Ensure SSL certificate is properly configured:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "path/to/certificate.pfx",
          "Password": "certificate-password"
        }
      }
    }
  }
}
```

### 2. CORS Configuration
Update CORS settings for production:

```csharp
services.AddCors(options =>
{
    options.AddPolicy("QLDAClient", policy =>
    {
        policy.WithOrigins("https://ql-da-client.yourdomain.gov.vn")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

### 3. Authentication & Authorization
Verify JWT and role-based security:

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
        };
    });
```

## Monitoring and Health Checks

### 1. Health Check Endpoints
The application includes health check endpoints:

- `/health` - Overall health status
- `/health-ui` - Health check dashboard (if configured)

### 2. Logging Configuration
Configure structured logging for production:

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "./logs/ql-da-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

## Deployment Strategies

### Blue-Green Deployment
1. Deploy new version to green environment
2. Run smoke tests on green environment
3. Switch traffic from blue to green
4. Keep blue as backup/rollback option

### Rolling Update (Kubernetes)
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ql-da-api
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 1
  template:
    spec:
      containers:
      - name: ql-da-api
        image: your-registry/ql-da:latest
        env:
        - name: CONNECTIONSTRINGS__DEFAULTCONNECTION
          valueFrom:
            secretKeyRef:
              name: ql-da-secrets
              key: db-connection-string
```

## Post-Deployment Verification

### 1. API Health Check
```bash
curl -X GET https://your-deployment-url/health
```

### 2. Database Connectivity
Verify the application can connect to the database and access all required tables.

### 3. Authentication Flow
Test the complete authentication flow with valid credentials.

### 4. Core API Endpoints
Test basic CRUD operations for key entities:
- GET `/api/du-an/danh-sach` - Project listing
- GET `/api/goi-thau/danh-sach` - Tender package listing
- GET `/api/hop-dong/danh-sach` - Contract listing

### 5. Performance Baseline
Run basic performance tests to establish baseline response times.

## Troubleshooting

### Common Issues

1. **Database Connection Errors**
   - Verify connection string format
   - Check SQL Server accessibility
   - Ensure firewall allows connections

2. **JWT Authentication Failures**
   - Verify JWT secret matches between services
   - Check token expiration settings
   - Validate issuer and audience configuration

3. **Aspose License Issues**
   - Verify license file path and permissions
   - Check license validity period
   - Ensure Aspose.Cells package is properly referenced

4. **Migration Problems**
   - Run migrations separately from application startup
   - Check migration history table for conflicts
   - Verify database user permissions

### Logs Location
- **Windows**: `%TEMP%\QLDA-Logs\` or configured file path
- **Linux**: `/var/log/ql-da/` or configured file path
- **Container**: Console logs or persistent volume mounts

## Rollback Procedure

1. **Identify the Previous Stable Version**
   - Tag or backup of previous release
   - Last known good database state

2. **Database Rollback (if needed)**
   - Use EF Core migrations to rollback
   ```bash
   dotnet ef database update PreviousMigrationName
   ```

3. **Application Rollback**
   - Revert to previous version using deployment tools
   - Restart services
   - Verify functionality

## Maintenance Tasks

### 1. Database Maintenance
- Regular backup schedules
- Index optimization
- Log file management
- Performance monitoring

### 2. Security Updates
- Regular .NET runtime updates
- Third-party package updates
- Security vulnerability scans
- Certificate renewals

### 3. Monitoring Alerts
Configure alerts for:
- Application crashes
- Database connectivity issues
- Performance degradation
- Security authentication failures

---
*This guide was last updated for the current version supporting .NET 8.0 and Clean Architecture implementation.*