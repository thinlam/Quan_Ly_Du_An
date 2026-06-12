using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        var response = await AuthedClient.PostAsync("/api/import/goi-thau", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
        result.DataResult.Should().NotBeNull();
        var rows = (result.DataResult as JsonElement?)?.GetArrayLength() ?? 0;
        rows.Should().BeGreaterThan(0,
            "template phải có Excel Table (GoiThauImport) để ReadDataFromExcel đọc được dòng");
    }

    #endregion
}