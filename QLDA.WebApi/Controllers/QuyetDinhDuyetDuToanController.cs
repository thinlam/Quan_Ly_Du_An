using QLDA.Application.DuAns.Commands;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.QuyetDinhDuyetDuToanDtos.DTOs;
using QLDA.Application.QuyetDinhDuyetDuToans;
using QLDA.Application.QuyetDinhDuyetDuToans.Commands;
using QLDA.Application.QuyetDinhDuyetDuToans.DTOs;
using QLDA.Application.QuyetDinhDuyetDuToans.Queries;
using BuildingBlocks.Application.Attachments.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.WebApi.Models.QuyetDinhDuyetDuToans;
using QLDA.WebApi.Models.TepDinhKems;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Tags("Quyết định duyệt dự toán")]
[Route("api/quyet-dinh-duyet-du-toan")]
public class QuyetDinhDuyetDuToanController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    [ProducesResponseType<ResultApi<QuyetDinhDuyetDuToanModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new QuyetDinhDuyetDuToanGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
        });

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.QuyetDinhDuyetDuToan)]
        ))).ToAttachmentEntities();
        var danhSachTepDinhKemKhac = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.QuyetDinhDuyetDuToan_Khac)]
        ))).ToAttachmentEntities();
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem, danhSachTepDinhKemKhac));
    }

    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new QuyetDinhDuyetDuToanDeleteCommand(id));
        return ResultApi.Ok(res);
    }


    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] QuyetDinhDuyetDuToanInsUpdDto dto) {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step));

        var entity = dto.ToEntity();
        entity = await Mediator.Send( new QuyetDinhDuyetDuToanInsertCommand(dto.ToEntity())   );

        List<Attachment> files = [.. dto.DanhSachTepDinhKem?.ToEntities(
            entity.Id, EGroupType.QuyetDinhDuyetDuToan) ?? []];

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.QuyetDinhDuyetDuToan)],
            Entities = files,
            AutoDeleteMissing = true
        });
        List<Attachment> fileKhacs = [.. dto.DanhSachTepDinhKemKhac?.ToEntities(
            entity.Id, EGroupType.QuyetDinhDuyetDuToan_Khac) ?? []];

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.QuyetDinhDuyetDuToan_Khac)],
            Entities = fileKhacs,
            AutoDeleteMissing = true
        });
        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<QuyetDinhDuyetDuToanModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update([FromBody] QuyetDinhDuyetDuToanInsUpdDto model)
    {
        var entity = await Mediator.Send(new QuyetDinhDuyetDuToanUpdateCommand(model));

        List<Attachment> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id,  EGroupType.QuyetDinhDuyetDuToan) ?? []];

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.QuyetDinhDuyetDuToan)],
            Entities = files,
            AutoDeleteMissing = true
        });
        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.QuyetDinhDuyetDuToan)]
        ))).ToAttachmentEntities();
        List<Attachment> fileKhacs = [.. model.DanhSachTepDinhKemKhac?.ToEntities(entity.Id,  EGroupType.QuyetDinhDuyetDuToan_Khac) ?? []];
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.QuyetDinhDuyetDuToan_Khac)],
            Entities = fileKhacs,
            AutoDeleteMissing = true
        });
        var danhSachTepDinhKemKhac = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.QuyetDinhDuyetDuToan_Khac)]
        ))).ToAttachmentEntities();
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem, danhSachTepDinhKemKhac));
    }

    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<QuyetDinhDuyetDuToanDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetDanhSach([FromQuery] Guid? duAnId, int? buocId, string? globalFilter = null,
        int pageIndex = 0, int pageSize = 0, int? loaiDuAnTheoNamId = null) {
        var res = await Mediator.Send(new QuyetDinhDuyetDuToanGetDanhSachQuery() {
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
