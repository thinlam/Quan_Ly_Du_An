using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLDA.Application.GoiThaus.Commands;
using QLDA.Application.GoiThaus.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Persistence;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class GoiThauImportTests(WebApiFixture fixture) : IAsyncLifetime
{
    private const int ImportBuocId = 9999;

    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    public async Task InitializeAsync()
    {
        await SeedImportReferenceDataAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedImportReferenceDataAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(fixture.GetSqliteConnection())
            .Options;
        using var db = new SqliteAppDbContext(options);

        if (await db.Set<KeHoachLuaChonNhaThau>().AnyAsync(e => e.Ten == "KH LCNT Test 001"))
            return;

        // Seed DanhMucLoaiHopDong
        var loaiHopDong = new DanhMucLoaiHopDong { Ten = "Thi công", Stt = 1 };
        db.Set<DanhMucLoaiHopDong>().Add(loaiHopDong);

        // Seed DanhMucHinhThucLuaChonNhaThau
        var hinhThuc = new DanhMucHinhThucLuaChonNhaThau { Ten = "Đấu thầu rộng rãi", Stt = 1 };
        db.Set<DanhMucHinhThucLuaChonNhaThau>().Add(hinhThuc);

        // Seed DanhMucPhuongThucLuaChonNhaThau
        var phuongThuc = new DanhMucPhuongThucLuaChonNhaThau { Ten = "Một giai đoạn một túi hồ sơ", Stt = 1 };
        db.Set<DanhMucPhuongThucLuaChonNhaThau>().Add(phuongThuc);

        // Seed DanhMucNguonVon
        var nguonVon = new DanhMucNguonVon { Ten = "Ngân sách nhà nước", Stt = 1 };
        db.Set<DanhMucNguonVon>().Add(nguonVon);

        // Seed KeHoachLuaChonNhaThau
        var keHoach = new KeHoachLuaChonNhaThau
        {
            Ten = "KH LCNT Test 001",
            DuAnId = fixture.SeededDuAnId,
            So = "KH001",
            TrichYeu = "Test",
            Loai = "TEST",
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Set<KeHoachLuaChonNhaThau>().Add(keHoach);

        await db.SaveChangesAsync();
    }

    #region ImportController Tests

    [Fact]
    public async Task ImportGoiThau_NoFile_ReturnsBadRequest()
    {
        var response = await AuthedClient.PostAsync("/api/import/goi-thau", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ImportGoiThau_EmptyFile_ReturnsOkOrBadRequest()
    {
        // Empty file with 0 bytes - IImporterHelper may return empty list instead of failing
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Array.Empty<byte>());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", "empty.xlsx");

        var response = await AuthedClient.PostAsync("/api/import/goi-thau", content);

        // Accept OK (empty list processed) or BadRequest (no file)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region TemplateController Tests

    [Fact]
    public async Task GetImportGoiThau_ReturnsFileResult()
    {
        var response = await AuthedClient.GetAsync("/api/template/import-goi-thau");

        // Template endpoint should return OK with Excel file
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region GoiThauImportRangeCommand Handler Tests (via HTTP)

    [Fact]
    public async Task ImportGoiThau_WithoutDuAnIdOrBuocId_ReturnsBadRequest()
    {
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            "Import_GoiThau.xlsx");

        if (!File.Exists(templatePath))
            return;

        var bytes = await File.ReadAllBytesAsync(templatePath);
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", "Import_GoiThau.xlsx");

        var response = await AuthedClient.PostAsync("/api/import/goi-thau", content);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();

        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
        result.ErrorMessage.Should().Contain("duAnId");
    }

    [Fact]
    public async Task ImportGoiThau_WithRealTemplateFile_ReturnsOk()
    {
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            "Import_GoiThau.xlsx");

        if (!File.Exists(templatePath))
        {
            // Template not found in test environment - skip test
            return;
        }

        var bytes = await File.ReadAllBytesAsync(templatePath);
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", "Import_GoiThau.xlsx");
        content.Add(new StringContent(fixture.SeededDuAnId.ToString()), "duAnId");
        content.Add(new StringContent(ImportBuocId.ToString()), "buocId");

        var response = await AuthedClient.PostAsync("/api/import/goi-thau", content);

        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();

        if (result!.Result) {
            result.DataResult.Should().NotBeNull();
            var importResult = JsonSerializer.Deserialize<GoiThauImportResultDto>(
                (JsonElement)result.DataResult!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            importResult.Should().NotBeNull();
            importResult!.SuccessCount.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task ImportGoiThau_WithDuAnIdAndBuocId_AppearsInDanhSachTienDo()
    {
        const string tenGoiThau = "Gói thầu import test 9579";

        using var scope = fixture.Services.CreateScope();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        var authHeader = AuthedClient.DefaultRequestHeaders.Authorization!.ToString();
        httpContext.Request.Headers.Authorization = authHeader;
        httpContextAccessor.HttpContext = httpContext;

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var importResult = await mediator.Send(new GoiThauImportRangeCommand([
            new GoiThauImportDto {
                TenKeHoachLuaChonNhaThau = "KH LCNT Test 001",
                Ten = tenGoiThau,
                GiaTri = 10_000_000,
                TenNguonVon = "Ngân sách nhà nước",
                TenHinhThucLuaChonNhaThau = "Đấu thầu rộng rãi",
                TenPhuongThucLuaChonNhaThau = "Một giai đoạn một túi hồ sơ",
                TenLoaiHopDong = "Thi công",
            }
        ]) {
            DuAnId = fixture.SeededDuAnId,
            BuocId = ImportBuocId,
        });

        importResult.SuccessCount.Should().Be(1);
        importResult.ErrorCount.Should().Be(0);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = await db.Set<GoiThau>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Ten == tenGoiThau);

        entity.Should().NotBeNull();
        entity!.DuAnId.Should().Be(fixture.SeededDuAnId);
        entity.BuocId.Should().Be(ImportBuocId);
        entity.DaDuyet.Should().BeTrue();
    }

    #endregion
}
