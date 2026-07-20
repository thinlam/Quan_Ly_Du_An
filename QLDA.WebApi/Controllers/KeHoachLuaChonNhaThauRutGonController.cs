using QLDA.Application.DuAns.Commands;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.KeHoachLuaChonNhaThauRutGons;
using QLDA.Application.KeHoachLuaChonNhaThauRutGons.Commands;
using QLDA.Application.KeHoachLuaChonNhaThauRutGons.DTOs;
using QLDA.Application.KeHoachLuaChonNhaThauRutGons.Queries;
using BuildingBlocks.Application.Attachments.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.WebApi.Models.KeHoachLuaChonNhaThauRutGons;
using QLDA.WebApi.Models.PhanKhaiKinhPhis;
using System.Data;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Quyết định kế hoạch lựa chọn nhà thầu
/// </summary>
[Tags("Kế hoạch lựa chọn nhà thầu rút gọn")]
[Route("api/ke-hoach-lua-chon-nha-thau-rut-gon")]
public class KeHoachLuaChonNhaThauRutGonController : AggregateRootController {
    // GET
    public KeHoachLuaChonNhaThauRutGonController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<KeHoachLuaChonNhaThauRutGonDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new KeHoachLuaChonNhaThauRutGonGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
        });

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            IncludeSigned: false
        ))).ToAttachmentEntities();
        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem));
    }


    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        await Mediator.Send(new KeHoachLuaChonNhaThauRutGonDeleteCommand(id));
        return ResultApi.Ok(1);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="insertDto"></param>
    /// <param name="unitOfWork"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create( [FromBody] KeHoachLuaChonNhaThauRutGonDto insertDto, [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default )
    {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        //Cập nhật bước hiện tại của dự án


        var step = await Mediator.Send(new DuAnUpdateStepCommand(insertDto.DuAnId, insertDto.BuocId), cancellationToken);
        await Mediator.Send(new DuAnUpdatePhaseCommand(insertDto.DuAnId, step), cancellationToken);

        var entity = await Mediator.Send(new KeHoachLuaChonNhaThauRutGonInsertCommand(insertDto.ToEntity()), cancellationToken);

        List<Attachment> files = [.. insertDto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.KeHoachLuaChonNhaThauRutGon) ?? []];
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.KeHoachLuaChonNhaThauRutGon)],
            Entities = files,
            AutoDeleteMissing = true
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="Dto"></param>
    /// <param name="unitOfWork"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] KeHoachLuaChonNhaThauRutGonDto Dto, [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        //Cập nhật bước hiện tại của dự án

        var step = await Mediator.Send(new DuAnUpdateStepCommand(Dto.DuAnId, Dto.BuocId), cancellationToken);
        await Mediator.Send(new DuAnUpdatePhaseCommand(Dto.DuAnId, step), cancellationToken);

        var entity = await Mediator.Send(new KeHoachLuaChonNhaThauRutGonUpdateCommand(Dto), cancellationToken);

        List<Attachment> files = [.. Dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.KeHoachLuaChonNhaThauRutGon) ?? []];
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.KeHoachLuaChonNhaThauRutGon)],
            Entities = files,
            AutoDeleteMissing = true
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }
    /// <summary>
    ///
    /// </summary>
    /// <param name="duAnId"></param>
    /// <param name="buocId"></param>
    /// <param name="globalFilter"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<PaginatedList<KeHoachLuaChonNhaThauRutGonDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] Guid? duAnId, int? buocId, string? globalFilter = null,
        int pageIndex = 0, int pageSize = 0, int? loaiDuAnTheoNamId = null) {
        var res = await Mediator.Send(new KeHoachLuaChonNhaThauRutGonGetDanhSachQuery() {
            DuAnId = duAnId,
            BuocId = buocId,
            GlobalFilter = globalFilter,
            PageIndex = pageIndex,
            PageSize = pageSize,
            IsNoTracking = true,
            LoaiDuAnTheoNamId = loaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}
