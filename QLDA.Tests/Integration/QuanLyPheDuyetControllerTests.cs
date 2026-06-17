using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Domain.Constants;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class QuanLyPheDuyetControllerTests(WebApiFixture fixture)
{
    private const string Type = PheDuyetEntityNames.PheDuyetDuToan;
    private const string BaseRoute = $"api/phe-duyet";

    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();
    private HttpClient BgdClient => fixture.CreateBgdClient();
    private HttpClient KhTcClient => fixture.CreateKhTcClient();
    private HttpClient HcthClient => fixture.CreateHcthClient();

    // --- GET danh-sach ---

    [Fact(Skip = "SQLite limitation - requires SQL Server")]
    public async Task GetDanhSach_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync($"/{BaseRoute}/danh-sach");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact(Skip = "SQLite limitation - requires SQL Server")]
    public async Task GetDanhSach_FilterByType_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync($"/{BaseRoute}/danh-sach?type={Type}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    // --- GET lich-su ---

    [Fact]
    public async Task GetLichSu_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync($"/{BaseRoute}/lich-su");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetLichSu_FilterByEntityId_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync($"/{BaseRoute}/lich-su?type={Type}&entityId={fixture.SeededPheDuyetDuToanId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    // --- GET chi-tiet ---

    [Fact]
    public async Task GetChiTiet_ExistingId_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync($"/{BaseRoute}/{Type}/{fixture.SeededPheDuyetDuToanId}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetChiTiet_NonExistentId_ReturnsFailure()
    {
        var fakeId = Guid.NewGuid();
        var response = await AuthedClient.GetAsync($"/{BaseRoute}/{Type}/{fakeId}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task GetChiTiet_InvalidType_ReturnsFailure()
    {
        var response = await AuthedClient.GetAsync($"/{BaseRoute}/InvalidType/{fixture.SeededPheDuyetDuToanId}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    // --- POST trinh (dispatch) ---

    [Fact]
    public async Task Trinh_AsKhTcUser_ReturnsOk()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        var response = await KhTcClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Trinh_AsNonKhTcUser_ReturnsFailure()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        var response = await AuthedClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task Trinh_InvalidType_ReturnsFailure()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        var response = await KhTcClient.PostAsync($"/{BaseRoute}/InvalidType/{pddtId}/trinh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    // --- POST duyet (dispatch) ---

    [Fact]
    public async Task Duyet_AsBgdUser_ReturnsOk()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        await KhTcClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);
        var response = await BgdClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/duyet", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Duyet_AsNonBgdUser_ReturnsFailure()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        await KhTcClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);
        var response = await AuthedClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/duyet", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task Duyet_WithoutTrinh_ReturnsFailure()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        var response = await BgdClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/duyet", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    // --- POST tra-lai (dispatch) ---

    [Fact]
    public async Task TraLai_AsBgdUser_ReturnsOk()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        await KhTcClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);
        var response = await BgdClient.PostAsJsonAsync($"/{BaseRoute}/{Type}/{pddtId}/tra-lai", new { NoiDung = "Test lý do trả lại" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task TraLai_WithoutNoiDung_ReturnsFailure()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        await KhTcClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);
        var response = await BgdClient.PostAsJsonAsync($"/{BaseRoute}/{Type}/{pddtId}/tra-lai", new { NoiDung = "" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    // --- Trinh after TraLai loop ---

    [Fact]
    public async Task Trinh_AfterTraLai_ReturnsOk()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        await KhTcClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);
        await BgdClient.PostAsJsonAsync($"/{BaseRoute}/{Type}/{pddtId}/tra-lai", new { NoiDung = "Cần sửa lại" });

        var response = await KhTcClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    // --- POST chuyen-phat-hanh ---

    [Fact]
    public async Task ChuyenPhatHanh_AsHcthUser_ReturnsOk()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        // DT → ĐTr → ĐD
        await KhTcClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);
        await BgdClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/duyet", null);

        // Chuyen phat hanh
        var response = await HcthClient.PostAsJsonAsync($"/{BaseRoute}/{Type}/{pddtId}/chuyen-phat-hanh", new { SoPhatHanh = "PH-001" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task ChuyenPhatHanh_AsBgdUser_ReturnsOk()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        await KhTcClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);
        await BgdClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/duyet", null);

        // BGD (LDDV role) can also publish
        var response = await BgdClient.PostAsJsonAsync($"/{BaseRoute}/{Type}/{pddtId}/chuyen-phat-hanh", new { SoPhatHanh = "PH-002" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task ChuyenPhatHanh_WithoutDuyet_ReturnsFailure()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        // Only trinh, not duyet yet
        await KhTcClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);

        var response = await HcthClient.PostAsJsonAsync($"/{BaseRoute}/{Type}/{pddtId}/chuyen-phat-hanh", new { SoPhatHanh = "PH-003" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task ChuyenPhatHanh_AsNonHcthNonBgdUser_ReturnsFailure()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        await KhTcClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/trinh", null);
        await BgdClient.PostAsync($"/{BaseRoute}/{Type}/{pddtId}/duyet", null);

        // AuthedClient has no HC-TH or LDDV role
        var response = await AuthedClient.PostAsJsonAsync($"/{BaseRoute}/{Type}/{pddtId}/chuyen-phat-hanh", new { SoPhatHanh = "PH-004" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }
}
