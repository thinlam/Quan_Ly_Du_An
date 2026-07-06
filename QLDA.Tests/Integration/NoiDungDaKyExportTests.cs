using System.Net;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using QLDA.Application.KySos.DTOs;
using QLDA.Application.KySos.Queries;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class NoiDungDaKyExportTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task Handler_GetDanhSach_Translates()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var ex = await Record.ExceptionAsync(() => mediator.Send(
            new NoiDungDaKyGetDanhSachQuery(new NoiDungDaKySearchDto())
            {
                PageIndex = 1,
                PageSize = 20,
            }));

        Assert.Null(ex);
    }

    [Fact]
    public async Task GetNoiDungDaKyList_WithDayMonthOnly_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync(
            "/api/ky-so/noi-dung-da-ky/danh-sach?pageIndex=1&pageSize=20&tuNgay=30-06&denNgay=30-06");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetNoiDungDaKyList_WithTuNgayDayMonthOnly_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync(
            "/api/ky-so/noi-dung-da-ky/danh-sach?pageIndex=1&pageSize=20&tuNgay=30-06");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetNoiDungDaKyList_WithDdMmYyyyDateRange_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync(
            "/api/ky-so/noi-dung-da-ky/danh-sach?pageIndex=1&pageSize=20&tuNgay=01-07-2025&denNgay=30-06-2026");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetNoiDungDaKyList_Default_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync(
            "/api/ky-so/noi-dung-da-ky/danh-sach?pageIndex=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExportNoiDungDaKy_WithDefaultDateRange_ReturnsExcelOrNoData()
    {
        var response = await AuthedClient.GetAsync(
            "/api/print/danh-sach-noi-dung-da-ky");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should()
                .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
    }

    [Fact]
    public async Task ExportNoiDungDaKy_NoMatch_ReturnsBadRequest()
    {
        var response = await AuthedClient.GetAsync(
            "/api/print/danh-sach-noi-dung-da-ky?globalFilter=__no_match_xyz__");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
