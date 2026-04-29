using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Application.DuAns.DTOs;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class BaoCaoDuAnControllerTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task GetBaoCaoDuToan_NoFilters_ReturnsOkWithPagination()
    {
        var response = await AuthedClient.GetAsync("/api/du-an/bao-cao-du-toan");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetBaoCaoDuToan_WithPagination_ReturnsPaginatedStructure()
    {
        var response = await AuthedClient.GetAsync("/api/du-an/bao-cao-du-toan?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi<PaginatedList<BaoCaoDuAnDto>>>();

        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
        result.DataResult.Should().NotBeNull();
        result.DataResult.TotalRows.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetBaoCaoDuToan_FilterByTenDuAn_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync("/api/du-an/bao-cao-du-toan?tenDuAn=Test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetBaoCaoDuToan_FilterByNam_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync("/api/du-an/bao-cao-du-toan?thoiGianKhoiCong=2024");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetBaoCaoDuToan_WithKyBaoCaoNam_ReturnsOk()
    {
        // KyBaoCao=Nam (3), NamFilter=2026
        var response = await AuthedClient.GetAsync("/api/du-an/bao-cao-du-toan?kyBaoCao=3&namFilter=2026");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetBaoCaoDuToan_WithKyBaoCaoQuy_ReturnsOk()
    {
        // KyBaoCao=Quy (2), NamFilter=2026, QuyFilter=1
        var response = await AuthedClient.GetAsync("/api/du-an/bao-cao-du-toan?kyBaoCao=2&namFilter=2026&quyFilter=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetBaoCaoDuToan_WithKyBaoCaoThang_ReturnsOk()
    {
        // KyBaoCao=Thang (1), NamFilter=2026, ThangFilter=3
        var response = await AuthedClient.GetAsync("/api/du-an/bao-cao-du-toan?kyBaoCao=1&namFilter=2026&thangFilter=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetBaoCaoDuToan_KyBaoCaoWithoutNamFilter_ReturnsOk()
    {
        // KyBaoCao without NamFilter should skip period filter (NamFilter is required for period filtering)
        var response = await AuthedClient.GetAsync("/api/du-an/bao-cao-du-toan?kyBaoCao=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetBaoCaoDuToan_CombinedFilters_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync(
            "/api/du-an/bao-cao-du-toan?tenDuAn=Test&thoiGianKhoiCong=2024&loaiDuAnId=1&pageIndex=0&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi<PaginatedList<BaoCaoDuAnDto>>>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
        result.DataResult.Should().NotBeNull();
    }
}
