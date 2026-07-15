using QLDA.Application.DuAns.Commands;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.DuToanDauTus;
using QLDA.Application.DuToanDauTus.Commands;
using QLDA.Application.DuToanDauTus.DTOs;
using QLDA.Application.DuToanDauTus.Queries;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Route("api/du-toan-dau-tu")]
[Tags("Dự toán chuẩn bị đầu tư")]
public class DuToanDauTuController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<DuToanDauTuDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new DuToanDauTuGetQuery()
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
        var res = await Mediator.Send(new DuToanDauTuDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create(
        [FromBody] DuToanDauTuDto dto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step));
      
        var entity = await Mediator.Send(new DuToanDauTuInsertCommand(dto), cancellationToken);
        // nếu dùng DuToanDauTu cho nhìu màn hình thì lấy  EGroupType.DuToanDauTu theo Loai
        //tạo contanst LoaiDuToanDauTu

        List<Attachment> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.DuToanDauTu) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    [ProducesResponseType<ResultApi<DuToanDauTuDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(
        [FromBody] DuToanDauTuDto dto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new DuToanDauTuUpdateCommand(dto), cancellationToken);

        List<Attachment> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.DuToanDauTu) ?? []];
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

    [ProducesResponseType<ResultApi<PaginatedList<DuToanDauTuDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] CommonSearchDto dto)
    {
        // hien tai có 2 loai la PheDuyetKhaoSat va QuyetDinhKeHoachThue
        var res = await Mediator.Send(new DuToanDauTuGetPaginatedQuery()
        {
            IsNoTracking = true,
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
           
            PageSize = dto.PageSize,
            PageIndex = dto.PageIndex,
            GlobalFilter = dto.GlobalFilter,
            TuNgay = dto.TuNgay,
            DenNgay = dto.DenNgay,
        });
        return ResultApi.Ok(res);
    }
}
