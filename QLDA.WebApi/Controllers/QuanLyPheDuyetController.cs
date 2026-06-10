using System.Net.Mime;
using System.Reflection;
using QLDA.Application.QuanLyPheDuyet.Commands;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Application.QuanLyPheDuyet.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.PheDuyetDuToans;
using QLDA.WebApi.Models.QuanLyPheDuyet;
using BuildingBlocks.CrossCutting.ExtensionMethods;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Quan ly phe duyet — center provider cho tat ca cac loai pheduyet
/// </summary>
[Tags("Quản lý phê duyệt")]
[Route("api/phe-duyet")]
public class QuanLyPheDuyetController : AggregateRootController {
    public QuanLyPheDuyetController(IServiceProvider serviceProvider) : base(serviceProvider) { }

    /// <summary>
    /// Danh sach entity types cho FE truyen vao tham so {type}
    /// </summary>
    [ProducesResponseType<ResultApi<List<PheDuyetTypeItemDto>>>(StatusCodes.Status200OK)]
    [HttpGet("types")]
    public ResultApi GetTypes() {
        var types = typeof(PheDuyetEntityNames)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && f.Name != nameof(PheDuyetEntityNames.Default))
            .Select(f => (string?)f.GetRawConstantValue())
            .Where(v => v != null)
            .Select(v => new PheDuyetTypeItemDto { Key = v!, Label = v!.GetDescriptionFromConstant(typeof(PheDuyetEntityNames)) })
            .ToList()!;
        return ResultApi.Ok(types);
    }

    /// <summary>
    /// Danh sach tat ca ban ghi pheduyet voi trang thai moi nhat
    /// </summary>
    [ProducesResponseType<ResultApi<PaginatedList<PheDuyetListItemDto>>>(StatusCodes.Status200OK)]
    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetDanhSach(
        [FromQuery] Guid? duAnId,
        [FromQuery] string? type,
        [FromQuery] string? globalFilter = null,
        int pageIndex = 0, int pageSize = 0) {
        var res = await Mediator.Send(new PheDuyetGetDanhSachQuery {
            DuAnId = duAnId, Type = type, GlobalFilter = globalFilter,
            PageIndex = pageIndex, PageSize = pageSize, IsNoTracking = true
        });
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Lich su phe duyet — sort moi nhat truoc
    /// </summary>
    [ProducesResponseType<ResultApi<PaginatedList<PheDuyetHistoryDto>>>(StatusCodes.Status200OK)]
    [HttpGet("lich-su")]
    public async Task<ResultApi> GetLichSu(
        [FromQuery] Guid? duAnId,
        [FromQuery] string? type,
        [FromQuery] Guid? entityId,
        int pageIndex = 0, int pageSize = 0) {
        var res = await Mediator.Send(new PheDuyetGetLichSuQuery {
            DuAnId = duAnId, Type = type, EntityId = entityId,
            PageIndex = pageIndex, PageSize = pageSize
        });
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Chi tiet ban ghi pheduyet theo type + id
    /// </summary>
    [ProducesResponseType<ResultApi<PheDuyetChiTietDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{type}/{id}/chi-tiet")]
    public async Task<ResultApi> GetChiTiet(string type, Guid id) {
        var res = await Mediator.Send(new PheDuyetGetChiTietQuery {
            Type = type, Id = id, IsNoTracking = true
        });
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Trinh phe duyet theo type
    /// </summary>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("{type}/{id}/trinh")]
    public async Task<ResultApi> Trinh(string type, Guid id, [FromBody] TrinhModel? model = null) {
        var res = await Mediator.Send(new PheDuyetDispatchTrinhCommand(type, id, model?.NoiDung));
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Duyet phe duyet theo type
    /// </summary>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("{type}/{id}/duyet")]
    public async Task<ResultApi> Duyet(string type, Guid id, string? noiDung) {
        var res = await Mediator.Send(new PheDuyetDispatchDuyetCommand(type, id, noiDung));
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Tra lai phe duyet theo type — can ly do
    /// </summary>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("{type}/{id}/tra-lai")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> TraLai(string type, Guid id, [FromBody] TraLaiModel model) {
        var res = await Mediator.Send(new PheDuyetDispatchTraLaiCommand(type, id, model.NoiDung));
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Tu choi phe duyet theo type — can ly do
    /// </summary>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("{type}/{id}/tu-choi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> TuChoi(string type, Guid id, [FromBody] TuChoiModel model) {
        var res = await Mediator.Send(new PheDuyetDispatchTuChoiCommand(type, id, model.NoiDung));
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Chuyen P.HC-TH de phat hanh so (issue #9459)
    /// </summary>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("{type}/{id}/chuyen-phat-hanh")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> ChuyenPhatHanh(string type, Guid id, [FromBody] ChuyenPhatHanhModel? model = null) {
        var res = await Mediator.Send(new PheDuyetChuyenPhatHanhCommand(type, id, model?.SoPhatHanh));
        return ResultApi.Ok(res);
    }
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("{type}/{id}/chuyen")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Chuyen(string type, Guid id, [FromBody] TuChoiModel? model = null)
    {
        var res = await Mediator.Send(new PheDuyetDispatchChuyenCommand(type, id, model?.NoiDung));
        return ResultApi.Ok(res);
    }
}