using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using BuildingBlocks.Application.Attachments.Commands;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.Application.QuyetDinhDuyetKHLCNTs.Commands;
using QLDA.Application.QuyetDinhDuyetKHLCNTs.DTOs;
using QLDA.Application.QuyetDinhDuyetKHLCNTs.Queries;
using QLDA.WebApi.Models.QuyetDinhDuyetKHLCNTs;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

[Tags("Quyết định duyệt kế hoạch lựa chọn nhà thầu")]
[Route("api/quyet-dinh-duyet-khlcnt")]
public class QuyetDinhDuyetKHLCNTController : AggregateRootController {
    // GET
    public QuyetDinhDuyetKHLCNTController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<QuyetDinhDuyetKHLCNTModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new QuyetDinhDuyetKHLCNTGetQuery() {
            Id = id, ThrowIfNull = true, IsNoTracking = true,
        });

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            IncludeSigned: false
        ))).ToAttachmentEntities();
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }


    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new QuyetDinhDuyetKHLCNTDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// Quy trình id là bắt buộc
    /// </remarks>
    /// <param name="model"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] QuyetDinhDuyetKHLCNTModel model) {
        //Cập nhật bước hiện tại của dự án

        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.VanBanQuyetDinh!.DuAnId, model.VanBanQuyetDinh!.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.VanBanQuyetDinh!.DuAnId, step));
        var entity = model.ToEntity();
        await Mediator.Send(new QuyetDinhDuyetKHLCNTInsertOrUpdateCommand(entity));


        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.QuyetDinhDuyetKHLCNT)],
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
    [ProducesResponseType<ResultApi<QuyetDinhDuyetKHLCNTModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] QuyetDinhDuyetKHLCNTModel model) {
        var entity =
            await Mediator.Send(new QuyetDinhDuyetKHLCNTGetQuery
                { Id = model.GetId(), ThrowIfNull = true });
        entity.Update(model);

        await Mediator.Send(new QuyetDinhDuyetKHLCNTInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        //Thêm file mới
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.QuyetDinhDuyetKHLCNT)],
            Entities = danhSachTepDinhKem,
            AutoDeleteMissing = true
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
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
    [ProducesResponseType<ResultApi<PaginatedList<QuyetDinhDuyetKHLCNTDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] Guid? duAnId, int? buocId,
        string? globalFilter = null, int pageIndex = 0, int pageSize = 0, int? loaiDuAnTheoNamId = null) {
        var res = await Mediator.Send(new QuyetDinhDuyetKHLCNTGetDanhSachQuery() {
            DuAnId = duAnId, BuocId = buocId,
            GlobalFilter = globalFilter,
            PageIndex = pageIndex, PageSize = pageSize, IsNoTracking = true,
            LoaiDuAnTheoNamId = loaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}