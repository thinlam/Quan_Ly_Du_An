using BuildingBlocks.Domain.Entities.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;
using QLDA.Application.BaoCaoKetQuaKhaoSats.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models;
using QLDA.WebApi.Models.BaoCaoKetQuaKhaoSats;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

[Tags("Báo cáo kết quả khảo sát")]
[Route("api/bao-cao-ket-qua-khao-sat")]
[Authorize]
public class BaoCaoKetQuaKhaoSatController(IServiceProvider sp) : AggregateRootController(sp)
{
    [HttpGet("{id}")]
    [ProducesResponseType<ResultApi<BaoCaoKetQuaKhaoSatDto>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new BaoCaoKetQuaKhaoSatGetQuery { Id = id });
        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
        {
            GroupId = [entity.Id.ToString()]
        });

        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    [HttpGet("danh-sach")]
    [ProducesResponseType<ResultApi<PaginatedList<BaoCaoKetQuaKhaoSatDto>>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> GetAll(
        [FromQuery] BaoCaoKetQuaKhaoSatSearchDto dto,
        string? globalFilter)
    {
        dto.GlobalFilter = globalFilter;
        var result = await Mediator.Send(new BaoCaoKetQuaKhaoSatGetDanhSachQuery(dto)
        {
            PageIndex = dto.PageIndex,
            PageSize = dto.PageSize,
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            GlobalFilter = dto.GlobalFilter,
            LoaiDuAnTheoNamId = dto.LoaiDuAnTheoNamId,
        });
     
        return ResultApi.Ok(result);
    }

    [HttpPost("them-moi")]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Create([FromBody] BaoCaoKetQuaKhaoSatModel model)
    {
        var entity = await Mediator.Send(new BaoCaoKetQuaKhaoSatInsertCommand(model.ToInsertDto()));
        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.BaoCaoKetQuaKhaoSat) ?? []];

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        });


        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Update([FromBody] BaoCaoKetQuaKhaoSatModel model)
    {
        var entity = await Mediator.Send(new BaoCaoKetQuaKhaoSatUpdateCommand(model.ToUpdateModel()));
        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.BaoCaoKetQuaKhaoSat) ?? []];
        
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        });
        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Delete(Guid id)
    {
        await Mediator.Send(new BaoCaoKetQuaKhaoSatDeleteCommand(id));
        return ResultApi.Ok("Xóa báo cáo thành công");
    }
}
