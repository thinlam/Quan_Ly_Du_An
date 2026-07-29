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
        var groupId = entity.Id.ToString();
        var files = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [groupId],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTu.ToString()]
        ))).ToAttachmentEntities();

        // Dữ liệu mới lưu theo HoSo.Id; giữ thêm Id cũ để đọc bản ghi legacy.
        var filesToTrinh = new List<Attachment>();
        if (entity.ToTrinh != null)
            filesToTrinh = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [groupId, entity.ToTrinh.Id.ToString()],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuToTrinh.ToString()]
        ))).ToAttachmentEntities();

        var filesQuyetDinh = new List<Attachment>();
        if (entity.QuyetDinh != null)
            filesQuyetDinh = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [groupId, entity.QuyetDinh.Id.ToString()],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString()]
        ))).ToAttachmentEntities();
        var fileCamKets = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [groupId],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuCamKetTD.ToString()]
        ))).ToAttachmentEntities();
        var fileThamDinhs = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [groupId],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuQuyetDinhTD.ToString()]
        ))).ToAttachmentEntities();
        var fileBaoCaos = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [groupId],
            BaseGroupTypes: [EGroupType.HoSoMoiThauDienTuBaoCaoTD.ToString()]
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
        // Tất cả tệp HSMTĐT dùng chung GroupId = HoSoMoiThauDienTu.Id.
        var entityId = entity.Id;
        var groupId = entityId.ToString();

        await SyncTepDinhKemAsync(
            groupId,
            model.GetDanhSachTepDinhKem(entityId),
            EGroupType.HoSoMoiThauDienTu.ToString(),
            cancellationToken);

        if (entity.ToTrinh != null || entityOld?.ToTrinh != null) {
            await SyncTepDinhKemAsync(
                groupId,
                model.ToTrinh?.GetDanhSachTepDinhKemToTrinh(entityId) ?? [],
                EGroupType.HoSoMoiThauDienTuToTrinh.ToString(),
                cancellationToken);
        }
        if (entity.QuyetDinh != null || entityOld?.QuyetDinh != null) {
            await SyncTepDinhKemAsync(
                groupId,
                model.QuyetDinh?.GetDanhSachTepDinhKemQuyetDinh(entityId) ?? [],
                EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString(),
                cancellationToken);
        }

        await SyncTepDinhKemAsync(
                   groupId,
                    model.HoSoMoiThauThamDinh?.GetDanhSachTepDinhKemQuyetDinhThamDinh(entityId) ?? [],
                   EGroupType.HoSoMoiThauDienTuQuyetDinhTD.ToString(),
                   cancellationToken) ;

        await SyncTepDinhKemAsync(
            groupId,
            model.HoSoMoiThauThamDinh?.GetDanhSachTepDinhKemCamKetThamDinh(entityId)??[],
            EGroupType.HoSoMoiThauDienTuCamKetTD.ToString(),
            cancellationToken);

        await SyncTepDinhKemAsync(
            groupId,
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
