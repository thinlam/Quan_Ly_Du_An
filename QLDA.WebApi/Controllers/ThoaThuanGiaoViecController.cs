using QLDA.Application.DuAns.Commands;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.ThoaThuanGiaoViecs;
using QLDA.Application.ThoaThuanGiaoViecs.Commands;
using QLDA.Application.ThoaThuanGiaoViecs.DTOs;
using QLDA.Application.ThoaThuanGiaoViecs.Queries;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.ThoaThuanGiaoViecs;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Tags("Thỏa thuận giao việc")]
[Route("api/thoa-thuan-giao-viec")]
public class ThoaThuanGiaoViecController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<ThoaThuanGiaoViecModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new ThoaThuanGiaoViecGetQuery()
        {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [nameof(EGroupType.ThoaThuanGiaoViec)]
        });
        
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id)
    {
        var res = await Mediator.Send(new ThoaThuanGiaoViecDeleteCommand(id));
        return ResultApi.Ok(res);
    }


    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] ThoaThuanGiaoViecDto model, [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    
    {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));

        var entity = new ThoaThuanGiaoViec()
        {
            Id = model.GetId(),
            DuAnId = model.DuAnId,
            BuocId = model.BuocId,
            PhamVi = model.PhamVi,
            NoiDung = model.NoiDung,
            GiaTri = model.GiaTri,
            ThoiGian = model.ThoiGian,
            ChatLuong = model.ChatLuong,
            GoiThauId = model.GoiThauId,

        };
        entity = await Mediator.Send(new ThoaThuanGiaoViecInsertCommand(entity));

        List<Attachment> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.ThoaThuanGiaoViec) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<ThoaThuanGiaoViecDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update([FromBody] ThoaThuanGiaoViecDto dto, [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    { 

        var entity = await Mediator.Send(new ThoaThuanGiaoViecUpdateCommand(dto));

        List<Attachment> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.ThoaThuanGiaoViec) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()]
        }, cancellationToken);

        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem.ToList())); 
    }

    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<ThoaThuanGiaoViecDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetDanhSach([FromQuery] Guid? duAnId, int? buocId, string? globalFilter = null,
        int pageIndex = 0, int pageSize = 0)
    {
        var res = await Mediator.Send(new ThoaThuanGiaoViecGetDanhSachQuery()
        {
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
