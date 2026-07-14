using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Tests.Fixtures;
using QLDA.WebApi.Models.PheDuyetDuToans;
using QLDA.WebApi.Models.QuanLyPheDuyet;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class PhanKhaiKinhPhiControllerTests(WebApiFixture fixture) {
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();
    private HttpClient BgdClient => fixture.CreateBgdClient();
    private HttpClient KhTcClient => fixture.CreateKhTcClient();

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

    private Guid _pkkpId;

    private const string Type = "PhanKhaiKinhPhi";

    [Fact]
    public async Task Trinh_AsKhTcUser_ReturnsOk() {
        _pkkpId = await CreatePhanKhaiKinhPhiAsync();

        var response = await KhTcClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/trinh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact(Skip = "SQLite limitation - requires SQL Server")]
    public async Task Trinh_AsNonKhTcUser_ReturnsFailure() {
        _pkkpId = await CreatePhanKhaiKinhPhiAsync();

        // Default client has PhongBanId=1 (not 219)
        var response = await AuthedClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/trinh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task Duyet_AsBgdUser_ReturnsOk() {
        _pkkpId = await CreatePhanKhaiKinhPhiAsync();

        // Submit first (KH-TC)
        await KhTcClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/trinh", null);

        // Approve (BGĐ)
        var response = await BgdClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/duyet", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Duyet_WithoutTrinh_ReturnsFailure() {
        _pkkpId = await CreatePhanKhaiKinhPhiAsync();

        // Try to approve without submitting first
        var response = await BgdClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/duyet", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task Duyet_AsNonBgdUser_ReturnsFailure() {
        _pkkpId = await CreatePhanKhaiKinhPhiAsync();

        // Submit first
        await KhTcClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/trinh", null);

        // Try to approve with non-BGĐ user
        var response = await AuthedClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/duyet", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task TraLai_AsBgdUser_ReturnsOk() {
        _pkkpId = await CreatePhanKhaiKinhPhiAsync();

        // Submit first (KH-TC)
        await KhTcClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/trinh", null);

        // Return with reason (BGĐ)
        var traLaiModel = new TraLaiModel { NoiDung = "Test lý do trả lại" };
        var response = await BgdClient.PostAsJsonAsync($"/api/phe-duyet/{Type}/{_pkkpId}/tra-lai", traLaiModel);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task TraLai_WithoutNoiDung_ReturnsFailure() {
        _pkkpId = await CreatePhanKhaiKinhPhiAsync();

        // Submit first
        await KhTcClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/trinh", null);

        // Try to return without reason
        var traLaiModel = new TraLaiModel { NoiDung = "" };
        var response = await BgdClient.PostAsJsonAsync($"/api/phe-duyet/{Type}/{_pkkpId}/tra-lai", traLaiModel);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task TuChoi_AsBgdUser_ReturnsOk() {
        _pkkpId = await CreatePhanKhaiKinhPhiAsync();

        // Submit first (KH-TC)
        await KhTcClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/trinh", null);

        // Reject with reason (BGĐ)
        var tuChoiModel = new TuChoiModel { NoiDung = "Test lý do từ chối" };
        var response = await BgdClient.PostAsJsonAsync($"/api/phe-duyet/{Type}/{_pkkpId}/tu-choi", tuChoiModel);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task TuChoi_WithoutNoiDung_ReturnsFailure() {
        _pkkpId = await CreatePhanKhaiKinhPhiAsync();

        // Submit first
        await KhTcClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/trinh", null);

        // Try to reject without reason
        var tuChoiModel = new TuChoiModel { NoiDung = "" };
        var response = await BgdClient.PostAsJsonAsync($"/api/phe-duyet/{Type}/{_pkkpId}/tu-choi", tuChoiModel);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task TuChoi_AsNonManagementRole_ReturnsFailure() {
        _pkkpId = await CreatePhanKhaiKinhPhiAsync();

        // Submit first
        await KhTcClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/trinh", null);

        // Try to reject with non-management user (ChuyenVien role)
        var chuyenVienClient = fixture.CreateChuyenVienClient();
        var tuChoiModel = new TuChoiModel { NoiDung = "Test lý do từ chối" };
        var response = await chuyenVienClient.PostAsJsonAsync($"/api/phe-duyet/{Type}/{_pkkpId}/tu-choi", tuChoiModel);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task Trinh_AfterTraLai_ReturnsOk() {
        _pkkpId = await CreatePhanKhaiKinhPhiAsync();

        // Submit (KH-TC)
        await KhTcClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/trinh", null);
        // Return (BGĐ)
        await BgdClient.PostAsJsonAsync($"/api/phe-duyet/{Type}/{_pkkpId}/tra-lai", new TraLaiModel { NoiDung = "Cần sửa lại" });

        // Resubmit after fix (KH-TC)
        var response = await KhTcClient.PostAsync($"/api/phe-duyet/{Type}/{_pkkpId}/trinh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
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
        if (result!.DataResult is System.Text.Json.JsonElement el) {
            if (el.ValueKind == System.Text.Json.JsonValueKind.Object && el.TryGetProperty("id", out var idProp)) {
                return idProp.GetGuid();
            }
            if (el.ValueKind == System.Text.Json.JsonValueKind.String) {
                return el.GetGuid();
            }
        }
        throw new InvalidOperationException($"Unexpected DataResult: {result.DataResult}");
    }
}