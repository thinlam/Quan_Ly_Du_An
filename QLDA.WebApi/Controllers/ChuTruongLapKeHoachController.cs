using QLDA.Application.BaoCaoBanGiaoSanPhams.Commands;
using QLDA.Application.ChuTruongLapKeHoachs;
using QLDA.Application.ChuTruongLapKeHoachs.Commands;
using QLDA.Application.ChuTruongLapKeHoachs.DTOs;
using QLDA.Application.ChuTruongLapKeHoachs.Queries;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.ChuTruongLapKeHoachs;
using QLDA.WebApi.Models.TepDinhKems;
using System.Data;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Route("api/chu-truong-lap-ke-hoach")]
[Tags("Chủ trương lập kế hoạch")]
public class ChuTruongLapKeHoachController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<ChuTruongLapKeHoachDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new ChuTruongLapKeHoachGetQuery()
        {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
        {
            GroupId = [entity.Id.ToString()]
        });

        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem.ToList()));
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id)
    {
        var res = await Mediator.Send(new ChuTruongLapKeHoachDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create(
        [FromBody] ChuTruongLapKeHoachModel dto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step));


        var entity = dto.ToEntity();

        await Mediator.Send(new ChuTruongLapKeHoachInsertCommand(entity));

        var danhSachTepDinhKem = dto.GetDanhSachTep(entity.Id).ToList();
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem,
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    [ProducesResponseType<ResultApi<ChuTruongLapKeHoachDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(
        [FromBody] ChuTruongLapKeHoachModel model,
        [FromServices] IUnitOfWork _unitOfWork,
        CancellationToken cancellationToken = default)
    {
        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var entity = model.ToEntity();

        await Mediator.Send(new ChuTruongLapKeHoachUpdateCommand(entity));
        
        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.ChuTruongLapKeHoach) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var danhSachTepDinhKem = model.GetDanhSachTep(entity.Id).ToList();
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem,
        });

        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem.ToList()));
    }

    [ProducesResponseType<ResultApi<PaginatedList<ChuTruongLapKeHoachDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] ChuTruongLapKeHoachSearchDto dto)
    {
        // hien tai có 2 loai la PheDuyetKhaoSat va QuyetDinhKeHoachThue
        var res = await Mediator.Send(new ChuTruongLapKeHoachDanhSachQuery()
        {
            IsNoTracking = true,
            DuAnId = dto.DuAnId,
            PageSize = dto.PageSize,
            PageIndex = dto.PageIndex,
            GlobalFilter = dto.GlobalFilter,
            TuNgay = dto.TuNgay,
            DenNgay = dto.DenNgay,
            LoaiDuAnTheoNamId = dto.LoaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}