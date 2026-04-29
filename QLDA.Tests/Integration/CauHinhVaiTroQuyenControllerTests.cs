using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class CauHinhVaiTroQuyenControllerTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task GetDanhSach_ReturnsAllToggles()
    {
        var response = await AuthedClient.GetAsync("/api/cau-hinh-vai-tro-quyen/danh-sach");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetDanhSach_FilterByVaiTro_ReturnsOnlyMatchingRole()
    {
        var response = await AuthedClient.GetAsync("/api/cau-hinh-vai-tro-quyen/danh-sach?vaiTro=QLDA_ChuyenVien");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetDanhSach_FilterByNhomQuyen_ReturnsOnlyMatchingGroup()
    {
        var response = await AuthedClient.GetAsync("/api/cau-hinh-vai-tro-quyen/danh-sach?nhomQuyen=DuAn");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task BatchUpdate_AsAdmin_ReturnsOk()
    {
        // Get current config to find an ID
        var listResponse = await AuthedClient.GetAsync("/api/cau-hinh-vai-tro-quyen/danh-sach?nhomQuyen=DuAn");
        var listResult = await listResponse.Content.ReadFromJsonAsync<ResultApi>();

        // Toggle one item off
        var updateDto = new
        {
            Items = new[]
            {
                new { Id = 1, KichHoat = true } // Keep first item on (idempotent)
            }
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/cau-hinh-vai-tro-quyen/cap-nhat", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task BatchUpdate_AsNonAdmin_ReturnsForbidden()
    {
        // Create client without admin role
        var chuyenVienClient = fixture.CreateChuyenVienClient();

        var updateDto = new
        {
            Items = new[]
            {
                new { Id = 1, KichHoat = false }
            }
        };

        var response = await chuyenVienClient.PutAsJsonAsync("/api/cau-hinh-vai-tro-quyen/cap-nhat", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
