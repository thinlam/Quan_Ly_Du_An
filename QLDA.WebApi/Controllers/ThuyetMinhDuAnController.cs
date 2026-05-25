using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.ThuyetMinhDuAns.Commands;
using QLDA.Application.ThuyetMinhDuAns.DTOs;
using QLDA.Application.ThuyetMinhDuAns.Queries;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.ThuyetMinhDuAns;

namespace QLDA.WebApi.Controllers;

[Tags("Thuyết minh dự án")]
[Route("api/thuyet-minh-du-an")]
public class ThuyetMinhDuAnController : AggregateRootController {
    // GET
    public ThuyetMinhDuAnController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<ThuyetMinhDuAnModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new ThuyetMinhDuAnGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery() {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }


    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new ThuyetMinhDuAnDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///du an id la bac buoc
    /// </remarks>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] ThuyetMinhDuAnModel model) {
        //Cập nhật bước hiện tại của dự án
        
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));

        var entity = model.ToEntity();
        await Mediator.Send(new ThuyetMinhDuAnInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
        });

        return ResultApi.Ok(entity.Id);
    }

    /// <summary>
    /// Cập nhật
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<ThuyetMinhDuAnModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update([FromBody] ThuyetMinhDuAnModel model) {
        var entity =
            await Mediator.Send(new ThuyetMinhDuAnGetQuery { Id = model.GetId(), ThrowIfNull = true });
        entity.Update(model);

        await Mediator.Send(new ThuyetMinhDuAnInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        //Thêm file mới
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    /// <summary>
    /// Văn bản trong tiến độ dự án lấy theo từng bước
    /// </summary>
    /// <param name="duAnId"></param>
    /// <param name="buocId"></param>
    /// <param name="globalFilter"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<ThuyetMinhDuAnDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] Guid? duAnId, int? buocId, string? globalFilter = null, int pageIndex = 0, int pageSize = 0) {
        var res = await Mediator.Send(new ThuyetMinhDuAnGetDanhSachQuery() {
            DuAnId = duAnId,
            BuocId = buocId,
            GlobalFilter = globalFilter,
            PageIndex = pageIndex,
            PageSize = pageSize,
            IsNoTracking = true,
        });
        return ResultApi.Ok(res);
    }
}