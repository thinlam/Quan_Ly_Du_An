using QLDA.Application.DanhMucDonVis.Queries;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.ThuyetMinhDuAns.Commands;
using QLDA.Application.ThuyetMinhDuAns.DTOs;
using QLDA.Application.ThuyetMinhDuAns.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.ThuyetMinhDuAns;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Tags("Thuyết minh dự án")]
[Route("api/thuyet-minh-du-an")]
public class ThuyetMinhDuAnController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
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
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [GroupTypeConstants.ThuyetMinhDuAn]
        });
        var danhSachTepThamDinh = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [GroupTypeConstants.ThuyetMinhDuAnThamDinh]
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new ThuyetMinhDuAnDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    /// <remarks>du an id la bac buoc</remarks>
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] ThuyetMinhDuAnModel model) {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));
      
        var entity = model.ToEntity();
        entity = await Mediator.Send( new ThuyetMinhDuAnInsertCommand(model.ToEntity())   );
       
        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(
            entity.Id, GroupTypeConstants.ThuyetMinhDuAn) ?? []];

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [GroupTypeConstants.ThuyetMinhDuAn],
            Entities = files
        });

     
        //var danhSachFileThamDinh = model.GetDanhSachTepDinhKemThamDinh(entity.Id).ToList();
        List<TepDinhKem> filesThamDinh = [.. model.DanhSachTepThamDinh?.ToEntities(
            entity.Id, GroupTypeConstants.ThuyetMinhDuAnThamDinh) ?? []];

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [GroupTypeConstants.ThuyetMinhDuAnThamDinh],
            Entities = filesThamDinh
        });

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<ThuyetMinhDuAnModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update([FromBody] ThuyetMinhDuAnModel model) {
        var entity =
            await Mediator.Send(new ThuyetMinhDuAnGetQuery { Id = model.GetId(), ThrowIfNull = true });
        entity.Update(model);

        await Mediator.Send(new ThuyetMinhDuAnUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
        });
        var danhSachFileThamDinh = model.GetDanhSachTepDinhKemThamDinh(entity.Id);

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = danhSachFileThamDinh
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem, danhSachFileThamDinh));
    }

    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<ThuyetMinhDuAnDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetDanhSach([FromQuery] Guid? duAnId, int? buocId, string? globalFilter = null,
        int pageIndex = 0, int pageSize = 0) {
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
