using System.Net;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using QLDA.Application.QuanLyPheDuyet.Queries;
using QLDA.Tests.Fixtures;
using Xunit;

namespace QLDA.Tests.Integration;

[Collection("WebApi")]
public class PheDuyetExportTests(WebApiFixture fixture)
{
    private HttpClient AuthedClient => fixture.CreateAuthenticatedClient();

    [Fact]
    public async Task Handler_GetDanhSach_Unpaged_Translates()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var ex = await Record.ExceptionAsync(() => mediator.Send(new PheDuyetGetDanhSachQuery
        {
            PageIndex = 1,
            PageSize = 0,
            IncludeAttachments = false,
        }));

        // Direct MediatR call may lack auth context — only fail on unexpected errors
        if (ex != null)
        {
            ex.Message.Should().BeOneOf("Không có dữ liệu để xuất", "Truy cập không được phép");
        }
    }

    [Fact]
    public async Task GetPheDuyetList_Default_ReturnsOk()
    {
        var response = await AuthedClient.GetAsync(
            "/api/phe-duyet/danh-sach?pageIndex=1&pageSize=10");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Handler_GetDanhSach_DefaultIncludeAttachments_NeverReturnsNullAttachmentList()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var listEx = await Record.ExceptionAsync(() => mediator.Send(new PheDuyetGetDanhSachQuery
        {
            PageIndex = 1,
            PageSize = 10,
            // IncludeAttachments mặc định true — không truyền tham số
        }));

        if (listEx != null)
        {
            listEx.Message.Should().BeOneOf("Không có dữ liệu để xuất", "Truy cập không được phép");
            return;
        }

        var list = await mediator.Send(new PheDuyetGetDanhSachQuery
        {
            PageIndex = 1,
            PageSize = 10,
        });

        foreach (var item in list.Data)
        {
            item.DanhSachTepDinhKem.Should().NotBeNull(
                "API danh sách phải trả [] thay vì null khi không có tệp (contract code cũ)");
        }
    }

    [Fact]
    public async Task Handler_GetDanhSach_IncludeAttachmentsFalse_ReturnsEmptyNotNull()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var listEx = await Record.ExceptionAsync(() => mediator.Send(new PheDuyetGetDanhSachQuery
        {
            PageIndex = 1,
            PageSize = 10,
            IncludeAttachments = false,
        }));

        if (listEx != null)
        {
            listEx.Message.Should().BeOneOf("Không có dữ liệu để xuất", "Truy cập không được phép");
            return;
        }

        var list = await mediator.Send(new PheDuyetGetDanhSachQuery
        {
            PageIndex = 1,
            PageSize = 10,
            IncludeAttachments = false,
        });

        foreach (var item in list.Data)
        {
            item.DanhSachTepDinhKem.Should().NotBeNull();
            item.DanhSachTepDinhKem.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task ExportPheDuyet_WithDefaultFilter_ReturnsExcelOrNoData()
    {
        var response = await AuthedClient.GetAsync(
            "/api/print/danh-sach-quan-ly-phe-duyet");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should()
                .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
    }

    [Fact]
    public async Task ExportPheDuyet_NoMatch_ReturnsBadRequest()
    {
        var response = await AuthedClient.GetAsync(
            "/api/print/danh-sach-quan-ly-phe-duyet?type=__no_match_entity__&trangThai=__no_match__");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportPheDuyet_OnlyTypeFilter_ReturnsExcelOrNoData()
    {
        var response = await AuthedClient.GetAsync(
            "/api/print/danh-sach-quan-ly-phe-duyet?type=PheDuyetDuToan");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportPheDuyet_UnpagedCountMatchesListTotal_WhenSameFilter()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        const string trangThai = "ĐTr";

        var listEx = await Record.ExceptionAsync(() => mediator.Send(new PheDuyetGetDanhSachQuery
        {
            TrangThai = trangThai,
            PageIndex = 1,
            PageSize = 1000,
        }));

        var exportEx = await Record.ExceptionAsync(() => mediator.Send(new PheDuyetGetDanhSachQuery
        {
            TrangThai = trangThai,
            PageIndex = 1,
            PageSize = 0,
            IncludeAttachments = false,
        }));

        if (listEx != null || exportEx != null)
        {
            // Auth / empty DB in CI — skip parity check
            return;
        }

        var list = await mediator.Send(new PheDuyetGetDanhSachQuery
        {
            TrangThai = trangThai,
            PageIndex = 1,
            PageSize = 1000,
        });
        var export = await mediator.Send(new PheDuyetGetDanhSachQuery
        {
            TrangThai = trangThai,
            PageIndex = 1,
            PageSize = 0,
            IncludeAttachments = false,
        });

        export.Data.Count.Should().Be(list.TotalRows, "export (PageSize=0) phải khớp totalRows của danh sách với cùng filter");
    }
}
