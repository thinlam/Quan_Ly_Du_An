using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.Commands;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.Queries;
using QLDA.Domain.Constants;
using System.Net.Mime;
using QLDA.WebApi.Models.KeHoachTrienKhaiChiTietDuAns;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAnMappings;
using QLDA.WebApi.Models.CanBoTrienKhaiHangMucs;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

[Route("api/ke-hoach-trien-khai-chi-tiet-du-an")]
[Tags("Kế hoạch triển khai chi tiết dự án")]
public class KeHoachTrienKhaiChiTietDuAnControllerController(IServiceProvider serviceProvider) : 
    AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<KeHoachTrienKhaiChiTietDuAnDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new KeHoachTrienKhaiChiTietDuAnGetQuery()
        {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true
        });
        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [nameof(EGroupType.KeHoachTrienKhaiChiTietDuAn)]
        });

        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem));
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id)
    {
        var res = await Mediator.Send(new KeHoachTrienKhaiChiTietDuAnDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] KeHoachTrienKhaiChiTietDuAnModel dto,
        [FromServices] IUnitOfWork _unitOfWork, 
        CancellationToken cancellationToken = default)
    {
        using var tx = await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken); 
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step));
        var entity = await Mediator.Send(new KeHoachTrienKhaiChiTietDuAnInsertCommand(dto.ToEntity()), cancellationToken);
       

        List<TepDinhKem> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.KeHoachTrienKhaiChiTietDuAn) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken); 
        return ResultApi.Ok(new { entity.Id });
    }

    [ProducesResponseType<ResultApi<KeHoachTrienKhaiChiTietDuAnDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(
        [FromBody] KeHoachTrienKhaiChiTietDuAnModel dto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new KeHoachTrienKhaiChiTietDuAnUpdateCommand(dto.ToEntity()), cancellationToken);

        List<TepDinhKem> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.KeHoachTrienKhaiChiTietDuAn) ?? []];
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

    [ProducesResponseType<ResultApi<PaginatedList<KeHoachTrienKhaiChiTietDuAnDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] KeHoachTrienKhaiChiTietDuAnSearchDto dto)
    {
        var res = await Mediator.Send(new KeHoachTrienKhaiChiTietDuAnDanhSachQuery()
        {
            IsNoTracking = true,
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            Ten = dto.Ten,
            MaMoc = dto.MaMoc,
            DonViChuTriId = dto.DonViChuTriId,
            TrangThaiId = dto.TrangThaiId,
            PageSize = dto.PageSize ?? 1,
            PageIndex = dto.PageIndex ?? 10,
            GlobalFilter = dto.GlobalFilter,
            TuNgay = dto.TuNgay,
            DenNgay = dto.DenNgay,
            LoaiDuAnTheoNamId = dto.LoaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}
