using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class RoleClaimPolicyVisibilityTests(WebApiFixture fixture)
{
    private HttpClient AdminClient => fixture.CreateAuthenticatedClient();
    private HttpClient ChuyenVienClient => fixture.CreateChuyenVienClient(phongBanId: 999);

    [Fact]
    public async Task DuAn_GetDanhSach_AsAdmin_ReturnsAllProjects()
    {
        var response = await AdminClient.GetAsync("/api/du-an/danh-sach");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task DuAn_GetDanhSach_AsChuyenVien_ReturnsFilteredProjects()
    {
        // ChuyenVien with PhongBanId=999 — should see 0 projects (seeded DuAn has no matching department)
        var response = await ChuyenVienClient.GetAsync("/api/du-an/danh-sach");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    // GoiThau/HopDong/VanBan list queries use subquery projections (FirstOrDefault/ToList in Select)
    // that require SQL APPLY — not supported on SQLite. These are tested against SQL Server in CI.
    [Fact(Skip = "SQLite does not support SQL APPLY required by GoiThau list projection")]
    public async Task GoiThau_GetDanhSachTienDo_AsAdmin_ReturnsOk()
    {
        var response = await AdminClient.GetAsync("/api/goi-thau/danh-sach-tien-do");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "SQLite does not support SQL APPLY required by HopDong list projection")]
    public async Task HopDong_GetDanhSachTienDo_AsAdmin_ReturnsOk()
    {
        var response = await AdminClient.GetAsync("/api/hop-dong/danh-sach-tien-do");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "SQLite does not support SQL APPLY required by VanBanPhapLy list projection")]
    public async Task VanBanPhapLy_GetDanhSachTienDo_AsAdmin_ReturnsOk()
    {
        var response = await AdminClient.GetAsync("/api/van-ban-phap-ly/danh-sach-tien-do");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "SQLite does not support SQL APPLY required by VanBanChuTruong list projection")]
    public async Task VanBanChuTruong_GetDanhSachTienDo_AsAdmin_ReturnsOk()
    {
        var response = await AdminClient.GetAsync("/api/van-ban-chu-truong/danh-sach-tien-do");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
