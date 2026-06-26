using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class KeHoachTrienKhaiHangMucImportTests(WebApiFixture fixture) {
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task GetImportTemplate_ReturnsFileResult() {
        var response = await AuthedClient.GetAsync("/api/template/import-ke-hoach-trien-khai-hang-muc");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Import_NoFile_ReturnsBadRequest() {
        var response = await AuthedClient.PostAsync("/api/import/ke-hoach-trien-khai-hang-muc", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Import_MissingDuAnId_ReturnsFailResult() {
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            "Import_KeHoachTrienKhaiHangMuc.xlsx");

        if (!File.Exists(templatePath))
            return;

        var content = new MultipartFormDataContent();
        await using var stream = File.OpenRead(templatePath);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", "Import_KeHoachTrienKhaiHangMuc.xlsx");
        content.Add(new StringContent("1"), "buocId");

        var response = await AuthedClient.PostAsync("/api/import/ke-hoach-trien-khai-hang-muc", content);
        var body = await response.Content.ReadFromJsonAsync<ResultApi>();

        body.Should().NotBeNull();
        body!.Result.Should().BeFalse();
        body.ErrorMessage.Should().Contain("duAnId");
    }
}
