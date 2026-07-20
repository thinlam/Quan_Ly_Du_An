using QLDA.Application.DuAns.Commands;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Application.Attachments.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
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

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.ToTrinhPheDuyet)],
            IncludeSigned: false
        ))).ToAttachmentEntities();

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
        [FromBody] ToTrinhPheDuyetInsUpdDto dto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step));

        var entity = await Mediator.Send(new ToTrinhPheDuyetInsertCommand(dto), cancellationToken);
        // nếu dùng ToTrinhPheDuyet cho nhìu màn hình thì lấy  EGroupType.ToTrinhPheDuyet theo Loai
        //tạo contanst LoaiToTrinhPheDuyet

        List<Attachment> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.ToTrinhPheDuyet) ?? []];
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.ToTrinhPheDuyet)],
            Entities = files,
            AutoDeleteMissing = true
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    [ProducesResponseType<ResultApi<ToTrinhPheDuyetDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(
        [FromBody] ToTrinhPheDuyetInsUpdDto dto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var entity = new ToTrinhPheDuyet();
        if(LoaiToTrinhKhongDuyetExtensions.ContainsDescription(dto.Loai))
            entity =  await Mediator.Send(new ToTrinhKhongDuyetUpdateCommand(dto), cancellationToken);
        else
            entity = await Mediator.Send(new ToTrinhPheDuyetUpdateCommand(dto), cancellationToken);

        List<Attachment> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.ToTrinhPheDuyet) ?? []];
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.ToTrinhPheDuyet)],
            Entities = files,
            AutoDeleteMissing = true
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            IncludeSigned: false
        ), cancellationToken)).ToAttachmentEntities();

        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem.ToList()));
    }

    [ProducesResponseType<ResultApi<PaginatedList<ToTrinhPheDuyetDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] ToTrinhPheDuyetSearchDto dto)
    {
        // hien tai có 6 loai trong  ToTrinhEntityNames
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
            LoaiDuAnTheoNamId = dto.LoaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}
