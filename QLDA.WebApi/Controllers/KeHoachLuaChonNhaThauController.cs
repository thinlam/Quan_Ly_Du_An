using System.Data;
using BuildingBlocks.Domain.Entities;
using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.KeHoachLuaChonNhaThaus;
using QLDA.Application.KeHoachLuaChonNhaThaus.Commands;
using QLDA.Application.KeHoachLuaChonNhaThaus.DTOs;
using QLDA.Application.KeHoachLuaChonNhaThaus.Queries;
using BuildingBlocks.Application.Attachments.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Quyết định kế hoạch lựa chọn nhà thầu
/// </summary>
[Tags("Kế hoạch lựa chọn nhà thầu")]
[Route("api/ke-hoach-lua-chon-nha-thau")]
public class KeHoachLuaChonNhaThauController : AggregateRootController {
    // GET
    public KeHoachLuaChonNhaThauController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<KeHoachLuaChonNhaThauDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new KeHoachLuaChonNhaThauGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
        });

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()]
        ))).ToAttachmentEntities();
        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem));
    }


    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        await Mediator.Send(new KeHoachLuaChonNhaThauDeleteCommand(id));
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
    public async Task<ResultApi> Create(
        [FromBody] KeHoachLuaChonNhaThauInsertDto insertDto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default
    ) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        //Cập nhật bước hiện tại của dự án


        var step = await Mediator.Send(new DuAnUpdateStepCommand(insertDto.DuAnId, insertDto.BuocId), cancellationToken);
        await Mediator.Send(new DuAnUpdatePhaseCommand(insertDto.DuAnId, step), cancellationToken);

        var entity = await Mediator.Send(new KeHoachLuaChonNhaThauInsertCommand(insertDto), cancellationToken);
        List<Attachment> files = [.. insertDto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.KeHoachLuaChonNhaThau) ?? []];
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.KeHoachLuaChonNhaThau)],
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
    /// <param name="updateDto"></param>
    /// <param name="unitOfWork"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<KeHoachLuaChonNhaThauDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update(
        [FromBody] KeHoachLuaChonNhaThauUpdateDto updateDto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default
    ) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var entity = await Mediator.Send(new KeHoachLuaChonNhaThauUpdateCommand(updateDto), cancellationToken);

        List<Attachment> files = [.. updateDto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.KeHoachLuaChonNhaThau) ?? []];
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.KeHoachLuaChonNhaThau)],
            Entities = files,
            AutoDeleteMissing = true
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        return ResultApi.Ok(entity.ToDto(files));
    }

    [ProducesResponseType<ResultApi<PaginatedList<KeHoachLuaChonNhaThauDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] Guid? duAnId, int? buocId, string? globalFilter = null,
        int pageIndex = 0, int pageSize = 0, string? Loai = null) {
        var res = await Mediator.Send(new KeHoachLuaChonNhaThauGetDanhSachQuery() {
            DuAnId = duAnId,
            BuocId = buocId,
            GlobalFilter = globalFilter,
            PageIndex = pageIndex,
            PageSize = pageSize,
            IsNoTracking = true,
            LoaiKeHoach = Loai
        });
        return ResultApi.Ok(res);
    }

}
