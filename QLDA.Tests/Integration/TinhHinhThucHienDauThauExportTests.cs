using System.Net;
using FluentAssertions;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class TinhHinhThucHienDauThauExportTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task ExportTinhHinhThucHienDauThau_ValidLoai_ReturnsExcel(int loai)
    {
        var response = await AuthedClient.GetAsync(
            $"/api/print/tinh-hinh-thuc-hien-dau-thau?loai={loai}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Fact]
    public async Task ExportTinhHinhThucHienDauThau_NoLoai_ReturnsExcel()
    {
        var response = await AuthedClient.GetAsync(
            "/api/print/tinh-hinh-thuc-hien-dau-thau");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(-1)]
    public async Task ExportTinhHinhThucHienDauThau_InvalidLoai_ReturnsBadRequest(int loai)
    {
        var response = await AuthedClient.GetAsync(
            $"/api/print/tinh-hinh-thuc-hien-dau-thau?loai={loai}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
