using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class KeHoachTrienKhaiHangMucPhieuTrinhPrintTests(WebApiFixture fixture)
{
    private const string TestKeHoachId = "30e8aa4e-4c4f-4c3d-8e0f-c419e941e44d";
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task Print_WithValidId_ReturnsDocxOrManagedError()
    {
        var response = await AuthedClient.GetAsync(
            $"/api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc?id={TestKeHoachId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

        if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            var body = await response.Content.ReadFromJsonAsync<ResultApi>();
            body.Should().NotBeNull();
            body!.Result.Should().BeFalse();
            body.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            return;
        }

        contentType.Should()
            .Be("application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
        bytes[0].Should().Be(0x50);
        bytes[1].Should().Be(0x4B);
    }

    [Fact]
    public async Task Print_WithEmptyGuid_ReturnsError()
    {
        var response = await AuthedClient.GetAsync(
            "/api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc?id=00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ResultApi>();
        body!.Result.Should().BeFalse();
    }
}
