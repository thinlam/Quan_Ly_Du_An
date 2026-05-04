using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Persistence;
using Xunit;

namespace QLDA.Tests.Fixtures;

public interface IWebApiFixture
{
    HttpClient Client { get; }
    Guid SeededDuAnId { get; }
    Guid SeededGoiThauId { get; }
    Guid SeededHopDongId { get; }
    Guid SeededPheDuyetDuToanId { get; }
    HttpClient CreateAuthenticatedClient();
    HttpClient CreateBgdClient();
    HttpClient CreateKhTcClient();
    HttpClient CreateChuyenVienClient(long phongBanId = 100);
    Task<Guid> CreatePheDuyetDuToanAsync();
}

public class WebApiFixture : WebApplicationFactory<Program>, IAsyncLifetime, IWebApiFixture
{
    private SqliteConnection _connection = null!;
    private SqliteAppDbContext _seedDb = null!;

    public HttpClient Client { get; private set; } = null!;

    public Guid SeededDuAnId { get; private set; }
    public Guid SeededGoiThauId { get; private set; }
    public Guid SeededHopDongId { get; private set; }
    public Guid SeededPheDuyetDuToanId { get; private set; }

    private const string TestJwtKey = "12345678901234567890123456789012";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Remove existing AppDbContext + options + factory registrations
            foreach (var d in services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(IDbContextFactory<AppDbContext>)).ToList())
            {
                services.Remove(d);
            }
            foreach (var d in services.Where(d =>
                d.ServiceType == typeof(AppDbContext) ||
                d.ImplementationType == typeof(AppDbContext)).ToList())
            {
                services.Remove(d);
            }

            // Open SQLite in-memory connection with FK enforcement disabled (DanhMuc not fully seeded)
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA foreign_keys = OFF;";
                cmd.ExecuteNonQuery();
            }

            // Register DbContextOptions<AppDbContext> with SQLite
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            // Override AppDbContext registration to create SqliteTestDbContext (clears SQL Server defaults)
            foreach (var d in services.Where(d => d.ServiceType == typeof(AppDbContext)).ToList())
                services.Remove(d);
            services.AddScoped<AppDbContext>(sp =>
            {
                var options = sp.GetRequiredService<DbContextOptions<AppDbContext>>();
                return new SqliteAppDbContext(options);
            });

            // Override JWT validation to accept test tokens
            services.PostConfigure<JwtBearerOptions>("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RoleClaimType = ClaimConstants.Roles,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddSingleton(new ConnectionStrings { DefaultConnection = "DataSource=:memory:" });
        });
    }

    public async Task InitializeAsync()
    {
        Client = CreateClient();

        // Create SqliteTestDbContext for seeding (same model as DI-resolved instances)
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _seedDb = new SqliteAppDbContext(options);

        await _seedDb.Database.EnsureCreatedAsync();
        await SeedReferenceDataAsync();
    }

    public new async Task DisposeAsync()
    {
        await _seedDb.DisposeAsync();
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }

    private async Task SeedReferenceDataAsync()
    {
        var duAn = new DuAn
        {
            TenDuAn = "Test Dự án",
            MaDuAn = "TEST_DA_001",
            LoaiDuAnId = 1,
            TrangThaiDuAnId = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            Level = 0,
            Path = "",
            NgayBatDau = DateTime.UtcNow,
        };
        _seedDb.Set<DuAn>().Add(duAn);
        await _seedDb.SaveChangesAsync();

        var goiThau = new GoiThau
        {
            DuAnId = duAn.Id,
            Ten = "Test Gói thầu",
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
        };
        _seedDb.Set<GoiThau>().Add(goiThau);
        await _seedDb.SaveChangesAsync();

        var hopDong = new HopDong
        {
            DuAnId = duAn.Id,
            GoiThauId = goiThau.Id,
            Ten = "Test Hợp đồng",
            SoHopDong = "HD_TEST_001",
            IsBienBan = true,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
        };
        _seedDb.Set<HopDong>().Add(hopDong);
        await _seedDb.SaveChangesAsync();

        var pheDuyetDuToan = new PheDuyetDuToan
        {
            DuAnId = duAn.Id,
            TrichYeu = "Test Phê duyệt dự toán",
            So = "PDDT_TEST_001",
            TrangThaiId = 1, // Dự thảo
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
        };
        _seedDb.Set<PheDuyetDuToan>().Add(pheDuyetDuToan);
        await _seedDb.SaveChangesAsync();

        SeededDuAnId = duAn.Id;
        SeededGoiThauId = goiThau.Id;
        SeededHopDongId = hopDong.Id;
        SeededPheDuyetDuToanId = pheDuyetDuToan.Id;
    }

    private string GenerateToken(long userId = 1, long donViId = 1, long phongBanId = 1, string[]? roles = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("UserId", userId.ToString()),
            new("UnitId", donViId.ToString()),
            new("PhongBanId", phongBanId.ToString()),
        };

        if (roles != null)
        {
            foreach (var role in roles)
                claims.Add(new Claim(ClaimConstants.Roles, role));
        }
        else
        {
            // Default: admin roles
            claims.Add(new Claim(ClaimConstants.Roles, RoleConstants.QLDA_QuanTri));
            claims.Add(new Claim(ClaimConstants.Roles, RoleConstants.QLDA_TatCa));
        }

        var token = new JwtSecurityToken(
            issuer: "QLDA",
            audience: "QLDA.Client",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private HttpClient CreateClientWithToken(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public string GenerateTestToken()
    {
        return GenerateToken();
    }

    public HttpClient CreateAuthenticatedClient()
    {
        return CreateClientWithToken(GenerateToken());
    }

    /// <summary>
    /// Client with BGĐ role - can approve/reject (Duyệt/Trả lại)
    /// </summary>
    public HttpClient CreateBgdClient()
    {
        var token = GenerateToken(userId: 10, phongBanId: 1, roles: [RoleConstants.QLDA_QuanTri, "BGĐ"]);
        return CreateClientWithToken(token);
    }

    /// <summary>
    /// Client with KH-TC department (PhongBanId=219) - can submit (Trình)
    /// </summary>
    public HttpClient CreateKhTcClient()
    {
        var token = GenerateToken(userId: 20, phongBanId: 219);
        return CreateClientWithToken(token);
    }

    /// <summary>
    /// Client with ChuyenVien role - restricted to department-scoped visibility
    /// </summary>
    public HttpClient CreateChuyenVienClient(long phongBanId = 100)
    {
        var token = GenerateToken(userId: 30, phongBanId: phongBanId, roles: [RoleConstants.QLDA_ChuyenVien]);
        return CreateClientWithToken(token);
    }

    /// <summary>
    /// Creates a fresh PheDuyetDuToan in Dự thảo status for test isolation.
    /// </summary>
    public async Task<Guid> CreatePheDuyetDuToanAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        using var db = new SqliteAppDbContext(options);

        var entity = new PheDuyetDuToan
        {
            DuAnId = SeededDuAnId,
            TrichYeu = $"Test PDDT {Guid.NewGuid():N}",
            So = $"PDDT_{Guid.NewGuid():N}",
            TrangThaiId = 1, // Dự thảo
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
        };
        db.Set<PheDuyetDuToan>().Add(entity);
        await db.SaveChangesAsync();
        return entity.Id;
    }
}

[CollectionDefinition("WebApi")]
public class WebApiCollection : ICollectionFixture<WebApiFixture> { }
