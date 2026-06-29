using QLDA.Application.TongHopVanBanQuyetDinhs.DTOs;
using QLDA.Application.TongHopVanBanQuyetDinhs.Queries;
using QLDA.WebApi.Models.TongHopVanBanQuyetDinhs;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Tổng hợp các bảng quyết định lại trong 1 controller
///
/// </summary>
[Tags("Tổng hợp văn bản quyết định")]
[Route("api/tong-hop-van-ban-quyet-dinh")]
public class TongHopVanBanQuyetDinhController(IServiceProvider serviceProvider)
    : AggregateRootController(serviceProvider) {
    [ProducesResponseType<ResultApi<PaginatedList<TongHopVanBanQuyetDinhDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-day-du")]
    public async Task<ResultApi> Get(
        [FromQuery] TongHopVanBanQuyetDinhSearchModel searchModel
    ) {
        var res = await Mediator.Send(new TongHopVanBanQuyetDinhGetListQuery {
            // IsNoTracking = true,
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            PageSize = searchModel.PageSize,
            PageIndex = searchModel.PageIndex,
            GlobalFilter = searchModel.GlobalFilter,

            CoQuanQuyetDinh = searchModel.CoQuanQuyetDinh,
            Loai = searchModel.Loai,
            TrichYeu = searchModel.TrichYeu,
            TuNgay = searchModel.TuNgay,
            DenNgay = searchModel.DenNgay,
            LoaiDuAnTheoNamId = searchModel.LoaiDuAnTheoNamId,
        });

        return ResultApi.Ok(res);
    }
}