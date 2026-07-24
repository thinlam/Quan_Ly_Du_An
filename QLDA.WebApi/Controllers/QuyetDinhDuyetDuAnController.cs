using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.QuyetDinhDuyetDuAnHangMucs.Commands;
using BuildingBlocks.Application.Attachments.Commands;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.Application.QuyetDinhDuyetDuAns.Commands;
using QLDA.Application.QuyetDinhDuyetDuAns.Queries;
using QLDA.WebApi.Models.QuyetDinhDuyetDuAns;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Quyết định kế hoạch lựa chọn nhà thầu
/// </summary>
[Tags("Quyết định duyệt dự án")]
[Route("api/quyet-dinh-duyet-du-an")]
public class QuyetDinhDuyetDuAnController : AggregateRootController {
    // GET
    public QuyetDinhDuyetDuAnController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="duAnId"></param>
    /// <returns></returns>
    [HttpGet("chi-tiet/{duAnId}")]
    [ProducesResponseType<ResultApi<QuyetDinhDuyetDuAnModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get(Guid duAnId) {
        var entity = await Mediator.Send(new QuyetDinhDuyetDuAnGetQuery() {
            DuAnId = duAnId,
            GetByDuAnId = true,
            ThrowIfNull = true, IsNoTracking = true,
            IncludeHangMuc = true, IncludeNguonVon = true
        });

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()]
        ))).ToAttachmentEntities();
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// Quy trình id là bắt buộc
    /// </remarks>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("them-moi")]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] QuyetDinhDuyetDuAnModel model) {
        //Cập nhật bước hiện tại của dự án

        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));
        var entity = model.ToEntity();
        await Mediator.Send(new QuyetDinhDuyetDuAnInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.QuyetDinhDuyetDuAn)],
            Entities = danhSachTepDinhKem,
            AutoDeleteMissing = true
        });

        return ResultApi.Ok(entity.Id);
    }

    /// <summary>
    /// Cập nhật
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("cap-nhat")]
    [ProducesResponseType<ResultApi<QuyetDinhDuyetDuAnModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] QuyetDinhDuyetDuAnModel model) {
        var entity = await Mediator.Send(new QuyetDinhDuyetDuAnGetQuery {
            Id = model.Id ?? Guid.Empty,
            ThrowIfNull = true,
            DuAnId = model.DuAnId,
            GetByDuAnId = model.Id == null,
        });
        entity.Update(model);
        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        var hangMucs = entity.QuyetDinhDuyetDuAnNguonVons?.Where(e => e.QuyetDinhDuyetDuAnHangMucs != null)
            .SelectMany(e => e.QuyetDinhDuyetDuAnHangMucs!).ToList() ?? [];
        if (hangMucs.Count == 0)
            entity.QuyetDinhDuyetDuAnNguonVons = [];
        await Mediator.Send(new QuyetDinhDuyetDuAnInsertOrUpdateCommand(entity));

        await Mediator.Send(new QuyetDinhDuyetDuAnHangMucInsertOrUpdateCommand() {
            QuyetDinhDuyetDuAnId = entity.Id,
            Entities = hangMucs
        });

        //Thêm file mới
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.QuyetDinhDuyetDuAn)],
            Entities = danhSachTepDinhKem,
            AutoDeleteMissing = true
        });
        var res = await Mediator.Send(new QuyetDinhDuyetDuAnGetQuery() {
            Id = entity.Id,
            ThrowIfNull = true, IsNoTracking = true,
            IncludeHangMuc = true, IncludeNguonVon = true
        });
        return ResultApi.Ok(res.ToModel(danhSachTepDinhKem));
    }
}