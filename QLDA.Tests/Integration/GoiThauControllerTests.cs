using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Tests.Fakers;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class GoiThauControllerTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task GetChiTiet_ExistingId_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync($"/api/goi-thau/{fixture.SeededGoiThauId}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsOk()
    {
        var dto = new GoiThauInsertDtoFaker(fixture.SeededDuAnId).Generate();

        var response = await AuthedClient.PostAsJsonAsync("/api/goi-thau/them-moi", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Update_ExistingGoiThau_ReturnsOk()
    {
        var dto = new GoiThauUpdateDtoFaker(fixture.SeededGoiThauId).Generate();

        var response = await AuthedClient.PutAsJsonAsync("/api/goi-thau/cap-nhat", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ExistingGoiThau_ReturnsOk()
    {
        // Create a new GoiThau to delete
        var createDto = new GoiThauInsertDtoFaker(fixture.SeededDuAnId).Generate();
        var createResponse = await AuthedClient.PostAsJsonAsync("/api/goi-thau/them-moi", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createResult = await createResponse.Content.ReadFromJsonAsync<ResultApi>();
        var idToDelete = extractGuid(createResult!);

        var response = await AuthedClient.DeleteAsync($"/api/goi-thau/{idToDelete}/xoa");

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
