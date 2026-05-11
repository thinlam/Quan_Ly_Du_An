using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Domain.Entities;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class PhanKhaiKinhPhiControllerTests(WebApiFixture fixture) {
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task GetChiTiet_ExistingId_ReturnsOk() {
        var id = await CreatePhanKhaiKinhPhiAsync();

        var response = await AuthedClient.GetAsync($"/api/phan-khai-kinh-phi/{id}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetChiTiet_NonExistentId_ReturnsFailure() {
        var fakeId = Guid.NewGuid();

        var response = await AuthedClient.GetAsync($"/api/phan-khai-kinh-phi/{fakeId}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task GetDanhSach_ReturnsOk() {
        await CreatePhanKhaiKinhPhiAsync();

        var response = await AuthedClient.GetAsync("/api/phan-khai-kinh-phi/danh-sach");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetDanhSach_WithDuAnIdFilter_ReturnsOk() {
        var id = await CreatePhanKhaiKinhPhiAsync();

        var response = await AuthedClient.GetAsync($"/api/phan-khai-kinh-phi/danh-sach?duAnId={fixture.SeededDuAnId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Create_ValidModel_ReturnsOk() {
        var model = new {
            DuAnId = fixture.SeededDuAnId,
            SoToTrinh = "TT_TEST_001",
            NgayToTrinh = DateTimeOffset.UtcNow,
            NguonVonId = 1,
            KinhPhiDeXuat = 1000000,
            KinhPhiPhanKhai = 800000,
        };

        var response = await AuthedClient.PostAsJsonAsync("/api/phan-khai-kinh-phi/them-moi", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithMissingRequiredFields_ReturnsBadRequest() {
        var model = new {
            DuAnId = (Guid?)null,
            SoToTrinh = "TT_TEST_001",
        };

        var response = await AuthedClient.PostAsJsonAsync("/api/phan-khai-kinh-phi/them-moi", model);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ExistingEntity_ReturnsOk() {
        var id = await CreatePhanKhaiKinhPhiAsync();

        var model = new {
            Id = id,
            DuAnId = fixture.SeededDuAnId,
            SoToTrinh = "TT_UPDATED_001",
            NgayToTrinh = DateTimeOffset.UtcNow,
            NguonVonId = 1,
            KinhPhiDeXuat = 2000000,
            KinhPhiPhanKhai = 1500000,
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/phan-khai-kinh-phi/cap-nhat", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Update_NonExistentId_ReturnsFailure() {
        var model = new {
            Id = Guid.NewGuid(),
            DuAnId = fixture.SeededDuAnId,
            SoToTrinh = "TT_UPDATED_001",
            NgayToTrinh = DateTimeOffset.UtcNow,
            NguonVonId = 1,
            KinhPhiDeXuat = 2000000,
            KinhPhiPhanKhai = 1500000,
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/phan-khai-kinh-phi/cap-nhat", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_ExistingEntity_ReturnsOk() {
        var id = await CreatePhanKhaiKinhPhiAsync();

        var response = await AuthedClient.DeleteAsync($"/api/phan-khai-kinh-phi/{id}/xoa");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_NonExistentId_ReturnsFailure() {
        var fakeId = Guid.NewGuid();

        var response = await AuthedClient.DeleteAsync($"/api/phan-khai-kinh-phi/{fakeId}/xoa");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    private async Task<Guid> CreatePhanKhaiKinhPhiAsync() {
        var model = new {
            DuAnId = fixture.SeededDuAnId,
            SoToTrinh = $"TT_{Guid.NewGuid():N}",
            NgayToTrinh = DateTimeOffset.UtcNow,
            NguonVonId = 1,
            KinhPhiDeXuat = 1000000,
            KinhPhiPhanKhai = 800000,
        };

        var response = await AuthedClient.PostAsJsonAsync("/api/phan-khai-kinh-phi/them-moi", model);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        return result!.DataResult switch {
            System.Text.Json.JsonElement el when el.ValueKind == System.Text.Json.JsonValueKind.String => el.GetGuid(),
            System.Text.Json.JsonElement el when el.ValueKind == System.Text.Json.JsonValueKind.Object => el.GetProperty("id").GetGuid(),
            Guid g => g,
            _ => throw new InvalidOperationException($"Unexpected DataResult type: {result.DataResult?.GetType()}"),
        };
    }
}