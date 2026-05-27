using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.ToTrinhPheDuyets;
using QLDA.Application.ToTrinhPheDuyets.Commands;
using QLDA.Application.ToTrinhPheDuyets.DTOs;
using QLDA.Application.ToTrinhPheDuyets.Queries;
using QLDA.Domain.Constants;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Route("api/to-trinh-phe-duyet")]
[Tags("Tờ trình phê duyệt")]
public class ToTrinhPheDuyetController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<ToTrinhPheDuyetDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new ToTrinhPheDuyetGetQuery()
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
        var res = await Mediator.Send(new ToTrinhPheDuyetDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create(
        [FromBody] ToTrinhPheDuyetInsertDto dto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step));
        dto.Loai= GroupTypeConstants.PheDuyetKhaoSat; // set loại để phân biệt với các entity khác dùng chung bảng TepDinhKem
        var entity = await Mediator.Send(new ToTrinhPheDuyetInsertCommand(dto), cancellationToken);
        // nếu dùng ToTrinhPheDuyet cho nhìu màn hình thì lấy  GroupTypeConstants.ToTrinhPheDuyet theo Loai
        //tạo contanst LoaiToTrinhPheDuyet

        List<TepDinhKem> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.PheDuyetKhaoSat) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    [ProducesResponseType<ResultApi<ToTrinhPheDuyetDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(
        [FromBody] ToTrinhPheDuyetUpdateDto dto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new ToTrinhPheDuyetUpdateCommand(dto), cancellationToken);

        List<TepDinhKem> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.PheDuyetKhaoSat) ?? []];
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

    [ProducesResponseType<ResultApi<PaginatedList<ToTrinhPheDuyetDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] ToTrinhPheDuyetSearchDto dto)
    {
        var res = await Mediator.Send(new ToTrinhPheDuyetGetPaginatedQuery()
        {
            IsNoTracking = true,
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            Loai = dto.Loai,
            PageSize = dto.PageSize,
            PageIndex = dto.PageIndex,
            GlobalFilter = dto.GlobalFilter,
            So = dto.SoQuyetDinh,
            TuNgay = dto.TuNgay,
            DenNgay = dto.DenNgay,
        });
        return ResultApi.Ok(res);
    }
}