using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Tests.Fakers;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class HopDongControllerTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task GetChiTiet_ExistingId_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync($"/api/hop-dong/{fixture.SeededHopDongId}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsOk()
    {
        // Create a new GoiThau first (1:1 with HopDong — seeded GoiThau already has a HopDong)
        var goiThauDto = new GoiThauInsertDtoFaker(fixture.SeededDuAnId).Generate();
        var goiThauResponse = await AuthedClient.PostAsJsonAsync("/api/goi-thau/them-moi", goiThauDto);
        goiThauResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var goiThauResult = await goiThauResponse.Content.ReadFromJsonAsync<ResultApi>();
        var newGoiThauId = goiThauResult!.DataResult switch
        {
            System.Text.Json.JsonElement el => el.GetProperty("id").GetGuid(),
            Guid g => g,
            _ => throw new InvalidOperationException("Unexpected GoiThau DataResult type")
        };

        var dto = new HopDongInsertDtoFaker(fixture.SeededDuAnId, newGoiThauId).Generate();

        var response = await AuthedClient.PostAsJsonAsync("/api/hop-dong/them-moi", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Update_ExistingHopDong_ReturnsOk()
    {
        var dto = new HopDongUpdateDtoFaker(fixture.SeededHopDongId, fixture.SeededGoiThauId).Generate();

        var response = await AuthedClient.PutAsJsonAsync("/api/hop-dong/cap-nhat", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ExistingHopDong_ReturnsOk()
    {
        // Create a new GoiThau + HopDong to delete
        var goiThauDto = new GoiThauInsertDtoFaker(fixture.SeededDuAnId).Generate();
        var goiThauResp = await AuthedClient.PostAsJsonAsync("/api/goi-thau/them-moi", goiThauDto);
        var goiThauRes = await goiThauResp.Content.ReadFromJsonAsync<ResultApi>();
        var newGoiThauId = extractGuid(goiThauRes!);

        var createDto = new HopDongInsertDtoFaker(fixture.SeededDuAnId, newGoiThauId).Generate();
        var createResponse = await AuthedClient.PostAsJsonAsync("/api/hop-dong/them-moi", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createResult = await createResponse.Content.ReadFromJsonAsync<ResultApi>();
        var idToDelete = extractGuid(createResult!);

        var response = await AuthedClient.DeleteAsync($"/api/hop-dong/{idToDelete}/xoa");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    private static Guid extractGuid(ResultApi result)
    {
        return result.DataResult switch
        {
            System.Text.Json.JsonElement el => el.GetProperty("id").GetGuid(),
            Guid g => g,
            _ => throw new InvalidOperationException($"Unexpected DataResult type: {result.DataResult?.GetType()}")
        };
    }
}
