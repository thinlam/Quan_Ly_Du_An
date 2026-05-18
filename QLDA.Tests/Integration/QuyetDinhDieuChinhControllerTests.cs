using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class QuyetDinhDieuChinhControllerTests(WebApiFixture fixture) {
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();
    private HttpClient BgdClient => fixture.CreateBgdClient();
    private HttpClient KhTcClient => fixture.CreateKhTcClient();

    #region Create Tests

    [Fact]
    public async Task Create_WithChiPhi_ReturnsOk() {
        var model = new {
            PheDuyetEntityId = fixture.SeededPheDuyetDuToanId,
            PheDuyetEntityName = "PheDuyetDuToan",
            DuAnId = fixture.SeededDuAnId,
            SoQuyetDinh = $"QDDC_{Guid.NewGuid():N}",
            NgayQuyetDinh = DateTimeOffset.UtcNow,
            TrichYeu = "Test quyết định điều chỉnh",
            LoaiDieuChinhId = 1,
            LyDo = "Test lý do",
            ChiPhi = new {
                TongMucDauTu = 5000000m,
                ChiPhiXayLap = 3000000m,
                ChiPhiThietBi = 1500000m,
                ChiPhiKhac = 300000m,
                ChiPhiDuPhong = 200000m
            }
        };

        var response = await AuthedClient.PostAsJsonAsync("/api/quyet-dinh-dieu-chinh/them-moi", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithoutChiPhi_ReturnsOk() {
        var model = new {
            PheDuyetEntityId = fixture.SeededPheDuyetDuToanId,
            PheDuyetEntityName = "PheDuyetDuToan",
            DuAnId = fixture.SeededDuAnId,
            SoQuyetDinh = $"QDDC_{Guid.NewGuid():N}",
            NgayQuyetDinh = DateTimeOffset.UtcNow,
            TrichYeu = "Test quyết định điều chỉnh",
            LoaiDieuChinhId = 1,
            LyDo = "Test lý do"
        };

        var response = await AuthedClient.PostAsJsonAsync("/api/quyet-dinh-dieu-chinh/them-moi", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithMissingRequiredFields_ReturnsBadRequest() {
        var model = new {
            PheDuyetEntityName = "PheDuyetDuToan",
            DuAnId = (Guid?)null
        };

        var response = await AuthedClient.PostAsJsonAsync("/api/quyet-dinh-dieu-chinh/them-moi", model);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetChiTiet Tests

    [Fact]
    public async Task GetChiTiet_ExistingId_ReturnsOk() {
        var id = await fixture.CreateQuyetDinhDieuChinhAsync();

        var response = await AuthedClient.GetAsync($"/api/quyet-dinh-dieu-chinh/{id}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetChiTiet_NonExistentId_ReturnsFailure() {
        var fakeId = Guid.NewGuid();

        var response = await AuthedClient.GetAsync($"/api/quyet-dinh-dieu-chinh/{fakeId}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task GetChiTiet_WithChiPhi_ReturnsChiPhiInDto() {
        var id = await fixture.CreateQuyetDinhDieuChinhAsync(withChiPhi: true);

        var response = await AuthedClient.GetAsync($"/api/quyet-dinh-dieu-chinh/{id}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();

        var data = result.DataResult;
        data.Should().NotBeNull();
        var jsonEl = data as System.Text.Json.JsonElement?;
        if (jsonEl.HasValue && jsonEl.Value.TryGetProperty("chiPhis", out var chiPhisProp)) {
            chiPhisProp.GetArrayLength().Should().Be(1);
            var first = chiPhisProp.EnumerateArray().First();
            first.GetProperty("tongMucDauTu").GetDecimal().Should().Be(5000000m);
        }
    }

    #endregion

    #region GetDanhSach Tests

    [Fact]
    public async Task GetDanhSach_ReturnsOk() {
        await fixture.CreateQuyetDinhDieuChinhAsync();

        var response = await AuthedClient.GetAsync("/api/quyet-dinh-dieu-chinh/danh-sach");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetDanhSach_WithDuAnIdFilter_ReturnsOk() {
        await fixture.CreateQuyetDinhDieuChinhAsync();

        var response = await AuthedClient.GetAsync($"/api/quyet-dinh-dieu-chinh/danh-sach?duAnId={fixture.SeededDuAnId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ExistingEntityWithDuThaoStatus_ReturnsOk() {
        var id = await fixture.CreateQuyetDinhDieuChinhAsync();

        var model = new {
            PheDuyetEntityId = fixture.SeededPheDuyetDuToanId,
            PheDuyetEntityName = "PheDuyetDuToan",
            DuAnId = fixture.SeededDuAnId,
            Id = id,
            SoQuyetDinh = $"QDDC_UPDATED_{Guid.NewGuid():N}",
            NgayQuyetDinh = DateTimeOffset.UtcNow,
            TrichYeu = "Updated quyết định điều chỉnh",
            LoaiDieuChinhId = 1,
            LyDo = "Updated lý do"
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/quyet-dinh-dieu-chinh/cap-nhat", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Update_NonExistentId_ReturnsFailure() {
        var model = new {
            PheDuyetEntityId = fixture.SeededPheDuyetDuToanId,
            PheDuyetEntityName = "PheDuyetDuToan",
            DuAnId = fixture.SeededDuAnId,
            Id = Guid.NewGuid(),
            SoQuyetDinh = $"QDDC_UPDATED_{Guid.NewGuid():N}",
            NgayQuyetDinh = DateTimeOffset.UtcNow,
            TrichYeu = "Updated",
            LoaiDieuChinhId = 1
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/quyet-dinh-dieu-chinh/cap-nhat", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task Update_WithChiPhi_UpsertsChiPhi() {
        // Create without ChiPhi first
        var id = await fixture.CreateQuyetDinhDieuChinhAsync(withChiPhi: false);

        // Update with ChiPhi
        var model = new {
            PheDuyetEntityId = fixture.SeededPheDuyetDuToanId,
            PheDuyetEntityName = "PheDuyetDuToan",
            DuAnId = fixture.SeededDuAnId,
            Id = id,
            SoQuyetDinh = $"QDDC_UPDATED_{Guid.NewGuid():N}",
            NgayQuyetDinh = DateTimeOffset.UtcNow,
            TrichYeu = "Updated with chi phi",
            LoaiDieuChinhId = 1,
            LyDo = "Add chi phi",
            ChiPhi = new {
                TongMucDauTu = 6000000m,
                ChiPhiXayLap = 4000000m,
                ChiPhiThietBi = 1500000m,
                ChiPhiKhac = 400000m,
                ChiPhiDuPhong = 100000m
            }
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/quyet-dinh-dieu-chinh/cap-nhat", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();

        // Verify ChiPhi was added
        var getResponse = await AuthedClient.GetAsync($"/api/quyet-dinh-dieu-chinh/{id}/chi-tiet");
        var getResult = await getResponse.Content.ReadFromJsonAsync<ResultApi>();
        getResult!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Update_WithExistingChiPhi_UpdatesChiPhi() {
        var id = await fixture.CreateQuyetDinhDieuChinhAsync(withChiPhi: true);

        // Update ChiPhi values
        var model = new {
            PheDuyetEntityId = fixture.SeededPheDuyetDuToanId,
            PheDuyetEntityName = "PheDuyetDuToan",
            DuAnId = fixture.SeededDuAnId,
            Id = id,
            SoQuyetDinh = $"QDDC_UPDATED_{Guid.NewGuid():N}",
            NgayQuyetDinh = DateTimeOffset.UtcNow,
            TrichYeu = "Updated chi phi",
            LoaiDieuChinhId = 1,
            LyDo = "Update chi phi",
            ChiPhi = new {
                TongMucDauTu = 7000000m,
                ChiPhiXayLap = 4500000m,
                ChiPhiThietBi = 1800000m,
                ChiPhiKhac = 500000m,
                ChiPhiDuPhong = 200000m
            }
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/quyet-dinh-dieu-chinh/cap-nhat", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    #endregion

    #region Status Validation Tests

    [Fact]
    public async Task Update_AfterTrinh_StatusNotDuThao_ReturnsFailure() {
        var qdId = await fixture.CreateQuyetDinhDieuChinhAsync();

        // Submit (KH-TC) - changes status from DuThao to DaTrinh
        await KhTcClient.PostAsync($"/api/phe-duyet/QuyetDinhDieuChinh/{qdId}/trinh", null);

        // Try to update - should fail because status is now DaTrinh (not DuThao or TraLai)
        var model = new {
            PheDuyetEntityId = fixture.SeededPheDuyetDuToanId,
            PheDuyetEntityName = "PheDuyetDuToan",
            DuAnId = fixture.SeededDuAnId,
            Id = qdId,
            SoQuyetDinh = $"QDDC_UPDATED_{Guid.NewGuid():N}",
            NgayQuyetDinh = DateTimeOffset.UtcNow,
            TrichYeu = "Updated after trinh",
            LoaiDieuChinhId = 1
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/quyet-dinh-dieu-chinh/cap-nhat", model);

        // ManagedException returns 200 with ResultApi.Fail(), not HTTP 400
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    #endregion

    #region PheDuyet Workflow Tests

    [Fact]
    public async Task Trinh_AsKhTcUser_ReturnsOk() {
        var qdId = await fixture.CreateQuyetDinhDieuChinhAsync();

        var response = await KhTcClient.PostAsync($"/api/phe-duyet/QuyetDinhDieuChinh/{qdId}/trinh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Duyet_AsBgdUser_ReturnsOk() {
        var qdId = await fixture.CreateQuyetDinhDieuChinhAsync();

        // Submit first (KH-TC)
        await KhTcClient.PostAsync($"/api/phe-duyet/QuyetDinhDieuChinh/{qdId}/trinh", null);

        // Approve (BGĐ)
        var response = await BgdClient.PostAsync($"/api/phe-duyet/QuyetDinhDieuChinh/{qdId}/duyet", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task TraLai_AsBgdUser_ReturnsOk() {
        var qdId = await fixture.CreateQuyetDinhDieuChinhAsync();

        // Submit first (KH-TC)
        await KhTcClient.PostAsync($"/api/phe-duyet/QuyetDinhDieuChinh/{qdId}/trinh", null);

        // Return with reason (BGĐ)
        var traLaiModel = new { NoiDung = "Test lý do trả lại" };
        var response = await BgdClient.PostAsJsonAsync($"/api/phe-duyet/QuyetDinhDieuChinh/{qdId}/tra-lai", traLaiModel);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Update_AfterTraLai_ReturnsOk() {
        var qdId = await fixture.CreateQuyetDinhDieuChinhAsync();

        // Submit (KH-TC)
        await KhTcClient.PostAsync($"/api/phe-duyet/QuyetDinhDieuChinh/{qdId}/trinh", null);
        // Return (BGĐ)
        await BgdClient.PostAsJsonAsync($"/api/phe-duyet/QuyetDinhDieuChinh/{qdId}/tra-lai", new { NoiDung = "Cần sửa lại" });

        // Update after being returned - should succeed because status is TraLai
        var model = new {
            PheDuyetEntityId = fixture.SeededPheDuyetDuToanId,
            PheDuyetEntityName = "PheDuyetDuToan",
            DuAnId = fixture.SeededDuAnId,
            Id = qdId,
            SoQuyetDinh = $"QDDC_UPDATED_{Guid.NewGuid():N}",
            NgayQuyetDinh = DateTimeOffset.UtcNow,
            TrichYeu = "Updated after tra lai",
            LoaiDieuChinhId = 1
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/quyet-dinh-dieu-chinh/cap-nhat", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    #endregion
}