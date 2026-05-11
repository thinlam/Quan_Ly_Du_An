using System.Net.Mime;
using QLDA.Application.PhanKhaiKinhPhis.Commands;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Application.PhanKhaiKinhPhis.Queries;
using QLDA.WebApi.Models.PhanKhaiKinhPhis;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Phân khai kinh phí (UC40 - #9467)
/// </summary>
[Tags("Phân khai kinh phí")]
[Route("api/phan-khai-kinh-phi")]
public class PhanKhaiKinhPhiController : AggregateRootController {
    public PhanKhaiKinhPhiController(IServiceProvider serviceProvider) : base(serviceProvider) { }

    /// <summary>
    /// Chi tiết phân khai kinh phí
    /// </summary>
    [ProducesResponseType<ResultApi<PhanKhaiKinhPhiModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new PhanKhaiKinhPhiGetQuery {
            Id = id, ThrowIfNull = true, IsNoTracking = true,
        });
        return ResultApi.Ok(entity.ToModel());
    }

    /// <summary>
    /// Danh sách phân khai kinh phí
    /// </summary>
    [ProducesResponseType<ResultApi<PaginatedList<PhanKhaiKinhPhiDto>>>(StatusCodes.Status200OK)]
    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetDanhSach(
        [FromQuery] Guid? duAnId,
        [FromQuery] string? globalFilter = null,
        int pageIndex = 0, int pageSize = 0) {
        var res = await Mediator.Send(new PhanKhaiKinhPhiGetDanhSachQuery {
            DuAnId = duAnId, GlobalFilter = globalFilter,
            PageIndex = pageIndex, PageSize = pageSize, IsNoTracking = true,
        });
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Thêm mới phân khai kinh phí
    /// </summary>
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] PhanKhaiKinhPhiModel model) {
        var entity = await Mediator.Send(new PhanKhaiKinhPhiInsertCommand(
            new() {
                DuAnId = model.DuAnId,
                SoToTrinh = model.SoToTrinh,
                NgayToTrinh = model.NgayToTrinh,
                NguonVonId = model.NguonVonId,
                KinhPhiDeXuat = model.KinhPhiDeXuat,
                KinhPhiPhanKhai = model.KinhPhiPhanKhai,
            }
        ));
        return ResultApi.Ok(entity.Id);
    }

    /// <summary>
    /// Cập nhật phân khai kinh phí
    /// </summary>
    [ProducesResponseType<ResultApi<PhanKhaiKinhPhiModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] PhanKhaiKinhPhiModel model) {
        var entity = await Mediator.Send(new PhanKhaiKinhPhiUpdateCommand(
            new() {
                Id = model.GetId(),
                DuAnId = model.DuAnId,
                SoToTrinh = model.SoToTrinh,
                NgayToTrinh = model.NgayToTrinh,
                NguonVonId = model.NguonVonId,
                KinhPhiDeXuat = model.KinhPhiDeXuat,
                KinhPhiPhanKhai = model.KinhPhiPhanKhai,
            }
        ));
        return ResultApi.Ok(entity.ToModel());
    }

    /// <summary>
    /// Xóa phân khai kinh phí
    /// </summary>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new PhanKhaiKinhPhiDeleteCommand(id));
        return ResultApi.Ok(res);
    }
}
