using QLDA.Application.DeXuatChuTruongMois.DTOs;
using QLDA.Application.TongHopDeXuatChuTruongs.Queries;

namespace QLDA.WebApi.Controllers;

[Tags("Tổng hợp Đề xuất chủ trương")]
[Route("api/tong-hop-de-xuat-chu-truong")]
public class TongHopDeXuatChuTruongController : AggregateRootController {
    // GET
    public TongHopDeXuatChuTruongController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }


    [HttpGet("danh-sach")]
    [ProducesResponseType<ResultApi<PaginatedList<DeXuatChuTruongMoiDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] DeXuatChuTruongMoiSearchDto req) {
        var res = await Mediator.Send(new TongHopDeXuatChuTruongGetDanhSachQuery() {
            DuAnId = req.DuAnId,
            BuocId = req.BuocId,
            GlobalFilter = req.GlobalFilter,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            IsNoTracking = true,
            Loai = req.Loai,
            Nam = req.Nam,
        });
        return ResultApi.Ok(res);
    }
}