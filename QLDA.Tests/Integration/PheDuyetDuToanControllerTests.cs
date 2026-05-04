using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class PheDuyetDuToanControllerTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();
    private HttpClient BgdClient => fixture.CreateBgdClient();
    private HttpClient KhTcClient => fixture.CreateKhTcClient();

    [Fact]
    public async Task GetChiTiet_ExistingId_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync($"/api/phe-duyet-du-toan/{fixture.SeededPheDuyetDuToanId}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetChiTiet_NonExistentId_ReturnsFailure()
    {
        var fakeId = Guid.NewGuid();
        var response = await AuthedClient.GetAsync($"/api/phe-duyet-du-toan/{fakeId}/chi-tiet");

        // ManagedException returns HTTP 200 with Result=false
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task Trinh_AsKhTcUser_ReturnsOk()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        var response = await KhTcClient.PostAsync($"/api/phe-duyet-du-toan/{pddtId}/trinh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Trinh_AsNonKhTcUser_ReturnsFailure()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        // Default client has PhongBanId=1 (not 219)
        var response = await AuthedClient.PostAsync($"/api/phe-duyet-du-toan/{pddtId}/trinh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task Duyet_AsBgdUser_ReturnsOk()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        // Submit first (KH-TC)
        await KhTcClient.PostAsync($"/api/phe-duyet-du-toan/{pddtId}/trinh", null);

        // Approve (BGĐ)
        var response = await BgdClient.PostAsync($"/api/phe-duyet-du-toan/{pddtId}/duyet", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Duyet_WithoutTrinh_ReturnsFailure()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        // Try to approve without submitting first
        var response = await BgdClient.PostAsync($"/api/phe-duyet-du-toan/{pddtId}/duyet", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task Duyet_AsNonBgdUser_ReturnsFailure()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        // Submit first
        await KhTcClient.PostAsync($"/api/phe-duyet-du-toan/{pddtId}/trinh", null);

        // Try to approve with non-BGĐ user
        var response = await AuthedClient.PostAsync($"/api/phe-duyet-du-toan/{pddtId}/duyet", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task TraLai_AsBgdUser_ReturnsOk()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        // Submit first (KH-TC)
        await KhTcClient.PostAsync($"/api/phe-duyet-du-toan/{pddtId}/trinh", null);

        // Return with reason (BGĐ)
        var traLaiModel = new { NoiDung = "Test lý do trả lại" };
        var response = await BgdClient.PostAsJsonAsync($"/api/phe-duyet-du-toan/{pddtId}/tra-lai", traLaiModel);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task TraLai_WithoutNoiDung_ReturnsFailure()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        // Submit first
        await KhTcClient.PostAsync($"/api/phe-duyet-du-toan/{pddtId}/trinh", null);

        // Try to return without reason
        var traLaiModel = new { NoiDung = "" };
        var response = await BgdClient.PostAsJsonAsync($"/api/phe-duyet-du-toan/{pddtId}/tra-lai", traLaiModel);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task Trinh_AfterTraLai_ReturnsOk()
    {
        var pddtId = await fixture.CreatePheDuyetDuToanAsync();

        // Submit (KH-TC)
        await KhTcClient.PostAsync($"/api/phe-duyet-du-toan/{pddtId}/trinh", null);
        // Return (BGĐ)
        await BgdClient.PostAsJsonAsync($"/api/phe-duyet-du-toan/{pddtId}/tra-lai", new { NoiDung = "Cần sửa lại" });

        // Resubmit after fix (KH-TC)
        var response = await KhTcClient.PostAsync($"/api/phe-duyet-du-toan/{pddtId}/trinh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }
}
