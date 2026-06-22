using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Domain.Entities.DanhMuc;
using QLDA.Persistence;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class PhanKhaiKinhPhiImportExportTests(WebApiFixture fixture) : IAsyncLifetime
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    public async Task InitializeAsync()
    {
        await SeedNguonVonAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedNguonVonAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(fixture.GetSqliteConnection())
            .Options;
        using var db = new SqliteAppDbContext(options);

        if (!await db.Set<DanhMucNguonVon>().AnyAsync())
        {
            db.Set<DanhMucNguonVon>().Add(new DanhMucNguonVon {
                Ten = "Ngân sách nhà nước",
                Stt = 1,
                Used = true,
            });
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task ImportPhanKhaiKinhPhi_NoFile_ReturnsBadRequest()
    {
        var response = await AuthedClient.PostAsync("/api/import/phan-khai-kinh-phi", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetImportPhanKhaiKinhPhi_ReturnsFileResult()
    {
        var response = await AuthedClient.GetAsync("/api/template/import-phan-khai-kinh-phi");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Fact]
    public async Task ExportDanhSachPhanKhaiKinhPhi_ReturnsExcel()
    {
        var response = await AuthedClient.GetAsync("/api/print/danh-sach-phan-khai-kinh-phi");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        response.Content.Headers.ContentDisposition?.FileName.Should().Contain("PhanKhaiKinhPhi_");
    }

    [Fact]
    public async Task ImportPhanKhaiKinhPhi_WithRealTemplateFile_ReadsTableRows()
    {
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            "Import_PhanKhaiKinhPhi.xlsx");

        if (!File.Exists(templatePath))
            return;

        var bytes = await File.ReadAllBytesAsync(templatePath);
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", "Import_PhanKhaiKinhPhi.xlsx");

        var response = await AuthedClient.PostAsync("/api/import/phan-khai-kinh-phi", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.DataResult.Should().NotBeNull();

        var importResult = JsonSerializer.Deserialize<PhanKhaiKinhPhiImportResultDto>(
            result.DataResult!.ToString()!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        importResult.Should().NotBeNull();
        (importResult!.SuccessCount + importResult.ErrorCount).Should().BeGreaterThan(0,
            "template phải có Excel Table (PhanKhaiKinhPhiImport) để ReadDataFromExcel đọc được dòng");
    }
}
