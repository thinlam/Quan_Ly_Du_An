using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class DanhMucLoaiHopDongControllerTests(WebApiFixture fixture) {
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    #region Create Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsOk() {
        var model = new {
            Ma = $"LHD_{Guid.NewGuid():N}".Substring(0, 20),
            Ten = "Loại hợp đồng test",
            MoTa = "Mô tả test",
            Stt = 1,
            Used = true
        };

        var response = await AuthedClient.PostAsJsonAsync("/api/danh-muc-loai-hop-dong/them-moi", model);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact(Skip = "SQLite limitation - requires SQL Server")]
    public async Task Create_WithMissingRequiredFields_ReturnsBadRequest() {
        var model = new {
            MoTa = "Mô tả test"
        };

        var response = await AuthedClient.PostAsJsonAsync("/api/danh-muc-loai-hop-dong/them-moi", model);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_ExistingId_ReturnsOk() {
        // Create first to get a valid id
        var createModel = new {
            Ma = $"LHD_{Guid.NewGuid():N}".Substring(0, 20),
            Ten = "Test Get",
            MoTa = "Test",
            Stt = 1,
            Used = true
        };
        await AuthedClient.PostAsJsonAsync("/api/danh-muc-loai-hop-dong/them-moi", createModel);

        // Get all and find the one we created
        var getAllResponse = await AuthedClient.GetAsync("/api/danh-muc-loai-hop-dong/danh-sach");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getAllResult = await getAllResponse.Content.ReadFromJsonAsync<ResultApi>();
        getAllResult!.DataResult.Should().NotBeNull();

        var dataArray = getAllResult.DataResult as System.Text.Json.JsonElement?;
        if (dataArray.HasValue && dataArray.Value.ValueKind == System.Text.Json.JsonValueKind.Array) {
            var firstItem = dataArray.Value.EnumerateArray().FirstOrDefault();
            if (firstItem.ValueKind != System.Text.Json.JsonValueKind.Undefined) {
                var id = firstItem.GetProperty("id").GetInt32();
                var getResponse = await AuthedClient.GetAsync($"/api/danh-muc-loai-hop-dong/{id}");
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await getResponse.Content.ReadFromJsonAsync<ResultApi>();
                result.Should().NotBeNull();
                result!.Result.Should().BeTrue();
            }
        }
    }

    [Fact]
    public async Task Get_NonExistentId_ReturnsFailure() {
        var fakeId = 99999;

        var response = await AuthedClient.GetAsync($"/api/danh-muc-loai-hop-dong/{fakeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAll_ReturnsOk() {
        var response = await AuthedClient.GetAsync("/api/danh-muc-loai-hop-dong/danh-sach-day-du");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task GetList_ReturnsOk() {
        var response = await AuthedClient.GetAsync("/api/danh-muc-loai-hop-dong/danh-sach");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ExistingEntity_ReturnsOk() {
        // Create first
        var createModel = new {
            Ma = $"LHD_{Guid.NewGuid():N}".Substring(0, 20),
            Ten = "Original Name",
            MoTa = "Original",
            Stt = 1,
            Used = true
        };
        var createResponse = await AuthedClient.PostAsJsonAsync("/api/danh-muc-loai-hop-dong/them-moi", createModel);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get the created item
        var getAllResponse = await AuthedClient.GetAsync("/api/danh-muc-loai-hop-dong/danh-sach");
        var getAllResult = await getAllResponse.Content.ReadFromJsonAsync<ResultApi>();
        var dataArray = getAllResult!.DataResult as System.Text.Json.JsonElement?;
        var firstItem = dataArray!.Value.EnumerateArray().FirstOrDefault();
        var id = firstItem.GetProperty("id").GetInt32();

        // Update
        var updateModel = new {
            Id = id,
            Ma = $"LHD_UPDATED_{Guid.NewGuid():N}".Substring(0, 20),
            Ten = "Updated Name",
            MoTa = "Updated",
            Stt = 2,
            Used = true
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/danh-muc-loai-hop-dong/cap-nhat", updateModel);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Update_NonExistentId_ReturnsFailure() {
        var updateModel = new {
            Id = 99999,
            Ma = $"LHD_UPDATED_{Guid.NewGuid():N}".Substring(0, 20),
            Ten = "Updated",
            MoTa = "Updated",
            Stt = 1,
            Used = true
        };

        var response = await AuthedClient.PutAsJsonAsync("/api/danh-muc-loai-hop-dong/cap-nhat", updateModel);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeFalse();
    }

    #endregion

    #region SoftDelete Tests

    [Fact]
    public async Task SoftDelete_ExistingId_ReturnsOk() {
        // Create first
        var createModel = new {
            Ma = $"LHD_{Guid.NewGuid():N}".Substring(0, 20),
            Ten = "To be deleted",
            MoTa = "Test delete",
            Stt = 1,
            Used = true
        };
        var createResponse = await AuthedClient.PostAsJsonAsync("/api/danh-muc-loai-hop-dong/them-moi", createModel);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get the created item
        var getAllResponse = await AuthedClient.GetAsync("/api/danh-muc-loai-hop-dong/danh-sach");
        var getAllResult = await getAllResponse.Content.ReadFromJsonAsync<ResultApi>();
        var dataArray = getAllResult!.DataResult as System.Text.Json.JsonElement?;
        var firstItem = dataArray!.Value.EnumerateArray().FirstOrDefault();
        var id = firstItem.GetProperty("id").GetInt32();

        // Soft delete
        var response = await AuthedClient.DeleteAsync($"/api/danh-muc-loai-hop-dong/xoa-tam?id={id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    #endregion
}