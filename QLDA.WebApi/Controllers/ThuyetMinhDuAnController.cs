using System.Net.Mime;
using QLDA.Application.BaoCaoTienDos.Commands;
using QLDA.Application.BaoCaoTienDos.DTOs;
using QLDA.Application.BaoCaoTienDos.Queries;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.WebApi.Models.BaoCaoTienDos;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

[Tags("Thuyết minh dự án")]
[Route("api/thuyet-minh-du-an")]
public class ThuyetMinhDuAnController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    [ProducesResponseType<ResultApi<BaoCaoTienDoModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new BaoCaoTienDoGetQuery() {
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
        var res = await Mediator.Send(new BaoCaoTienDoDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    /// <remarks>du an id la bac buoc</remarks>
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] BaoCaoTienDoModel model) {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));

        var entity = model.ToEntity();
        await Mediator.Send(new BaoCaoTienDoInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
        });

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<BaoCaoTienDoModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update([FromBody] BaoCaoTienDoModel model) {
        var entity =
            await Mediator.Send(new BaoCaoTienDoGetQuery { Id = model.GetId(), ThrowIfNull = true });
        entity.Update(model);

        await Mediator.Send(new BaoCaoTienDoInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<BaoCaoTienDoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetDanhSach([FromQuery] Guid? duAnId, int? buocId, string? globalFilter = null,
        int pageIndex = 0, int pageSize = 0) {
        var res = await Mediator.Send(new BaoCaoTienDoGetDanhSachQuery() {
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
