using QLDA.Application.HoSoMoiThauDienTus.Commands;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.HoSoMoiThauDienTus.Queries;
using BuildingBlocks.Application.Attachments.Commands;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.WebApi.Models.HoSoMoiThauDienTus;
using QLDA.WebApi.Models.TepDinhKems;
using System.Data;
namespace QLDA.WebApi.Controllers;

[Tags("Hồ sơ mời thầu điện tử")]
[Route("api/ho-so-moi-thau-dien-tu")]
public class HoSoMoiThauDienTuController(IServiceProvider sp) : AggregateRootController(sp) {

    [HttpGet("{id}")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new HoSoMoiThauDienTuGetQuery { Id = id });
        var files = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTu.ToString()],
            IncludeSigned: false
        ))).ToAttachmentEntities();
        var filesToTrinh = new  List<Attachment>();
        if(entity.ToTrinh!= null)
            filesToTrinh = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.ToTrinh != null ? entity.ToTrinh.Id.ToString() : ""],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuToTrinh.ToString()],
            IncludeSigned: false
        ))).ToAttachmentEntities();
        var filesQuyetDinh = new  List<Attachment>();
        if (entity.QuyetDinh != null)
            filesQuyetDinh = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.QuyetDinh != null ? entity.QuyetDinh.Id.ToString() : ""],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString()],
            IncludeSigned: false
        ))).ToAttachmentEntities();
        var fileCamKets = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuCamKetTD.ToString()],
            IncludeSigned: false
        ))).ToAttachmentEntities();
        var fileThamDinhs = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuQuyetDinhTD.ToString()],
            IncludeSigned: false
        ))).ToAttachmentEntities();
        var fileBaoCaos = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuBaoCaoTD.ToString()],
            IncludeSigned: false
        ))).ToAttachmentEntities();
        return ResultApi.Ok(entity.ToModel(files, fileCamKets, fileThamDinhs, fileBaoCaos, filesToTrinh, filesQuyetDinh));
    }

    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetAll([FromQuery] HoSoMoiThauDienTuSearchDto dto, string? globalFilter) {
        dto.GlobalFilter = globalFilter;
        var result = await Mediator.Send(new HoSoMoiThauDienTuGetDanhSachQuery(dto));
        return ResultApi.Ok(result);
    }

    [HttpPost("them-moi")]
    public async Task<ResultApi> Create([FromBody] HoSoMoiThauDienTuModel model,
        [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var entity = await Mediator.Send(new HoSoMoiThauDienTuInsertCommand(model.ToInsertDto()));
        await SaveDanhSachTepDinhKemAsync(model, entity,null, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        return ResultApi.Ok(entity.Id);


    }

    [HttpPut("cap-nhat")]
    public async Task<ResultApi> Update([FromBody] HoSoMoiThauDienTuModel model, [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        // remove file cũ đi trong trường hợp entity cũ có Thẩm định
        var entityOld = await Mediator.Send(new HoSoMoiThauDienTuGetQuery { Id = model.GetId() });
        var entity = await Mediator.Send(new HoSoMoiThauDienTuUpdateCommand(model.ToUpdateModel()));

        await SaveDanhSachTepDinhKemAsync(model, entity, entityOld, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        return ResultApi.Ok(entity.Id);
    }

    [HttpDelete("{id}")]
    public async Task<ResultApi> Delete(Guid id) {
        await Mediator.Send(new HoSoMoiThauDienTuDeleteCommand(id));
        return ResultApi.Ok(1);
    }
    private async Task SaveDanhSachTepDinhKemAsync(HoSoMoiThauDienTuModel model, HoSoMoiThauDienTu entity, HoSoMoiThauDienTu? entityOld, CancellationToken cancellationToken) {
        var entityId = entity.Id;

        await SyncTepDinhKemAsync(
            entityId.ToString(),
            model.GetDanhSachTepDinhKem(entityId),
            EGroupType.HoSoMoiThauDienTu.ToString(),
            cancellationToken);

        if (entity.ToTrinh != null && entityOld?.ToTrinh != null) {
            var toTrinhId = entity.ToTrinh != null  ? entity.ToTrinh.Id : entityOld?.ToTrinh?.Id;
            await SyncTepDinhKemAsync(
                (toTrinhId??0).ToString(),
                model.ToTrinh?.GetDanhSachTepDinhKemToTrinh(toTrinhId??0) ?? [],
                EGroupType.HoSoMoiThauDienTuToTrinh.ToString(),
                cancellationToken);
        }
        if (entity.QuyetDinh != null && entityOld?.QuyetDinh != null) {
            var quyetDinhId = entity.QuyetDinh != null ? entity.QuyetDinh.Id : entityOld?.QuyetDinh?.Id;
            await SyncTepDinhKemAsync(
                (quyetDinhId ?? 0).ToString(),
                model.QuyetDinh?.GetDanhSachTepDinhKemQuyetDinh(quyetDinhId??0) ?? [],
                EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString(),
                cancellationToken);
        }

        await SyncTepDinhKemAsync(
                   entityId.ToString(),
                    model.HoSoMoiThauThamDinh?.GetDanhSachTepDinhKemQuyetDinhThamDinh(entityId) ?? [],
                   EGroupType.HoSoMoiThauDienTuQuyetDinhTD.ToString(),
                   cancellationToken) ;

        await SyncTepDinhKemAsync(
            entityId.ToString(),
            model.HoSoMoiThauThamDinh?.GetDanhSachTepDinhKemCamKetThamDinh(entityId)??[],
            EGroupType.HoSoMoiThauDienTuCamKetTD.ToString(),
            cancellationToken);

        await SyncTepDinhKemAsync(
            entityId.ToString(),
            model.HoSoMoiThauThamDinh?.GetDanhSachTepDinhKemBaoCaoThamDinh(entityId) ?? [],
            EGroupType.HoSoMoiThauDienTuBaoCaoTD.ToString(),
            cancellationToken);

    }

    private Task SyncTepDinhKemAsync(
        string groupId,
        List<Attachment> entities,
        string scopeGroupType,
        CancellationToken cancellationToken)
        => Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = groupId,
            GroupTypes = [scopeGroupType],
            Entities = entities,
            AutoDeleteMissing = true
        }, cancellationToken);
}
