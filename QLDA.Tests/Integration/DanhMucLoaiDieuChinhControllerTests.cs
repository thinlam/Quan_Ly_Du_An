using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Application.DanhMucLoaiDieuChinhs.DTOs;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class DanhMucLoaiDieuChinhControllerTests(WebApiFixture fixture) {
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    #region Create Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsOk() {
        var dto = new DanhMucLoaiDieuChinhInsertDto {
            Ma = $"LDDC_{Guid.NewGuid():N}".Substring(0, 20),
            Ten = "Loại điều chỉnh test",
            MoTa = "Mô tả test",
            Stt = 1,
            Used = true
        };

        var response = await AuthedClient.PostAsJsonAsync("/api/danh-muc-loai-dieu-chinh/them-moi", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithMissingRequiredFields_ReturnsOk() {
        var dto = new {
            MoTa = "Mô tả test"
        };

        var response = await AuthedClient.PostAsJsonAsync("/api/danh-muc-loai-dieu-chinh/them-moi", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_ExistingId_ReturnsOk() {
        var createDto = new DanhMucLoaiDieuChinhInsertDto {
            Ma = $"LDDC_{Guid.NewGuid():N}".Substring(0, 20),
            Ten = "Test Get",
            MoTa = "Test",
            Stt = 1,
            Used = true
        };
        await AuthedClient.PostAsJsonAsync("/api/danh-muc-loai-dieu-chinh/them-moi", createDto);

        var getAllResponse = await AuthedClient.GetAsync("/api/danh-muc-loai-dieu-chinh/danh-sach");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getAllResult = await getAllResponse.Content.ReadFromJsonAsync<ResultApi>();
        getAllResult!.DataResult.Should().NotBeNull();

        var dataArray = getAllResult.DataResult as System.Text.Json.JsonElement?;
        if (dataArray.HasValue && dataArray.Value.ValueKind == System.Text.Json.JsonValueKind.Array) {
            var firstItem = dataArray.Value.EnumerateArray().FirstOrDefault();
            if (firstItem.ValueKind != System.Text.Json.JsonValueKind.Undefined) {
                var id = firstItem.GetProperty("id").GetInt32();
                var getResponse = await AuthedClient.GetAsync($"/api/danh-muc-loai-dieu-chinh/{id}");
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await getResponse.Content.ReadFromJsonAsync<ResultApi>();
                result.Should().NotBeNull();
                result!.Result.Should().BeTrue();
            }
        }
    }

    [Fact]
    public async Task Get_NonExistentId_ReturnsFail() {
        var fakeId = 99999;

        var response = await AuthedClient.GetAsync($"/api/danh-muc-loai-dieu-chinh/{fakeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAll_ReturnsOk() {
        var response = await AuthedClient.GetAsync("/api/danh-muc-loai-dieu-chinh/danh-sach");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ExistingEntity_ReturnsOk() {
        var createDto = new DanhMucLoaiDieuChinhInsertDto {
            Ma = $"LDDC_{Guid.NewGuid():N}".Substring(0, 20),
            Ten = "Original Name",
            MoTa = "Original",
            Stt = 1,
            Used = true
        };
        var createResponse = await AuthedClient.PostAsJsonAsync("/api/danh-muc-loai-dieu-chinh/them-moi", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getAllResponse = await AuthedClient.GetAsync("/api/danh-muc-loai-dieu-chinh/danh-sach");
        var getAllResult = await getAllResponse.Content.ReadFromJsonAsync<ResultApi>();
        var dataArray = getAllResult!.DataResult as System.Text.Json.JsonElement?;
        if (dataArray.HasValue && dataArray.Value.ValueKind == System.Text.Json.JsonValueKind.Array) {
            var firstItem = dataArray.Value.EnumerateArray().FirstOrDefault();
            if (firstItem.ValueKind != System.Text.Json.JsonValueKind.Undefined) {
                var id = firstItem.GetProperty("id").GetInt32();

                var updateDto = new DanhMucLoaiDieuChinhUpdateDto {
                    Id = id,
                    Ma = $"LDDC_UPDATED_{Guid.NewGuid():N}".Substring(0, 20),
                    Ten = "Updated Name",
                    MoTa = "Updated",
                    Stt = 2,
                    Used = true
                };

                var response = await AuthedClient.PutAsJsonAsync("/api/danh-muc-loai-dieu-chinh/cap-nhat", updateDto);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadFromJsonAsync<ResultApi>();
                result.Should().NotBeNull();
                result!.Result.Should().BeTrue();
            }
        }
    }

    [Fact]
    public async Task Update_NonExistentId_ReturnsFail() {
        var updateDto = new DanhMucLoaiDieuChinhUpdateDto {
            Id = 99999,
            Ma = $"LDDC_UPDATED_{Guid.NewGuid():N}".Substring(0, 20),
            Ten = "Updated",
            MoTa = "Updated",
            Stt = 1,
            Used = true
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/danh-muc-loai-dieu-chinh/cap-nhat", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    #endregion

    #region SoftDelete Tests

    [Fact]
    public async Task SoftDelete_ExistingId_ReturnsOk() {
        var createDto = new DanhMucLoaiDieuChinhInsertDto {
            Ma = $"LDDC_{Guid.NewGuid():N}".Substring(0, 20),
            Ten = "To be deleted",
            MoTa = "Test delete",
            Stt = 1,
            Used = true
        };
        var createResponse = await AuthedClient.PostAsJsonAsync("/api/danh-muc-loai-dieu-chinh/them-moi", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getAllResponse = await AuthedClient.GetAsync("/api/danh-muc-loai-dieu-chinh/danh-sach");
        var getAllResult = await getAllResponse.Content.ReadFromJsonAsync<ResultApi>();
        var dataArray = getAllResult!.DataResult as System.Text.Json.JsonElement?;
        if (dataArray.HasValue && dataArray.Value.ValueKind == System.Text.Json.JsonValueKind.Array) {
            var firstItem = dataArray.Value.EnumerateArray().FirstOrDefault();
            if (firstItem.ValueKind != System.Text.Json.JsonValueKind.Undefined) {
                var id = firstItem.GetProperty("id").GetInt32();

                var response = await AuthedClient.DeleteAsync($"/api/danh-muc-loai-dieu-chinh/xoa-tam/{id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadFromJsonAsync<ResultApi>();
                result.Should().NotBeNull();
                result!.Result.Should().BeTrue();
            }
        }
    }

    #endregion
}