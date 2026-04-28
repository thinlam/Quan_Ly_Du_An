using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Common.DTOs;
using FluentAssertions;
using QLDA.Tests.Fakers;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class DuAnControllerTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task GetChiTiet_ExistingId_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync($"/api/du-an/{fixture.SeededDuAnId}/chi-tiet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsOk()
    {
        var dto = new DuAnInsertDtoFaker().Generate();

        var response = await AuthedClient.PostAsJsonAsync("/api/du-an/them-moi", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
        result!.DataResult.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ExistingDuAn_ReturnsOk()
    {
        var dto = new DuAnUpdateModelFaker(fixture.SeededDuAnId).Generate();

        var response = await AuthedClient.PutAsJsonAsync("/api/du-an/cap-nhat", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }

    [Fact]
    public async Task SoftDelete_ExistingDuAn_ReturnsOk()
    {
        // Create a new DuAn to delete
        var createDto = new DuAnInsertDtoFaker().Generate();
        var createResponse = await AuthedClient.PostAsJsonAsync("/api/du-an/them-moi", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createResult = await createResponse.Content.ReadFromJsonAsync<ResultApi>();
        createResult.Should().NotBeNull();

        // Extract Guid from DataResult
        var idToDelete = createResult!.DataResult switch
        {
            System.Text.Json.JsonElement el => el.GetProperty("id").GetGuid(),
            Guid g => g,
            _ => throw new InvalidOperationException($"Unexpected DataResult type: {createResult.DataResult?.GetType()}")
        };

        var response = await AuthedClient.DeleteAsync($"/api/du-an/{idToDelete}/xoa-tam");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResultApi>();
        result.Should().NotBeNull();
        result!.Result.Should().BeTrue();
    }
}
