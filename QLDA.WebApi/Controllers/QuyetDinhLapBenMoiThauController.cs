using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using BuildingBlocks.Application.Attachments.Commands;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.Application.QuyetDinhLapBenMoiThaus.Commands;
using QLDA.Application.QuyetDinhLapBenMoiThaus.DTOs;
using QLDA.Application.QuyetDinhLapBenMoiThaus.Queries;
using QLDA.WebApi.Models.QuyetDinhLapBenMoiThaus;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

[Route("api/quyet-dinh-thanh-lap-ben-moi-thau")]
[Tags("Quyết định thành lập bên mời thầu")]
public class QuyetDinhLapBenMoiThauController(IServiceProvider serviceProvider)
    : AggregateRootController(serviceProvider) {
    // GET

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<QuyetDinhLapBenMoiThauModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new QuyetDinhLapBenMoiThauGetQuery() {
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
        var res = await Mediator.Send(new QuyetDinhLapBenMoiThauDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <param name="model"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] QuyetDinhLapBenMoiThauModel model) {
        //Cập nhật bước hiện tại của dự án

        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));
        var entity = model.ToEntity();
        await Mediator.Send(new QuyetDinhLapBenMoiThauInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.QuyetDinhLapBenMoiThau)],
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
    [ProducesResponseType<ResultApi<QuyetDinhLapBenMoiThauModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] QuyetDinhLapBenMoiThauModel model) {
        var entity =
            await Mediator.Send(new QuyetDinhLapBenMoiThauGetQuery
                { Id = model.GetId(), ThrowIfNull = true });
        entity.Update(model);

        await Mediator.Send(new QuyetDinhLapBenMoiThauInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        //Thêm file mới
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.QuyetDinhLapBenMoiThau)],
            Entities = danhSachTepDinhKem,
            AutoDeleteMissing = true
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// SoQuyetDinh: Số quyết định
    /// TuNgay, DenNgay: Ngày quyết định
    /// </remarks>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<PaginatedList<QuyetDinhLapBenMoiThauDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] QuyetDinhLapBenMoiThauSearchModel searchModel) {
        var res = await Mediator.Send(new QuyetDinhLapBenMoiThauGetDanhSachQuery() {

            IsNoTracking = true,
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            PageSize = searchModel.PageSize,
            PageIndex = searchModel.PageIndex,
            GlobalFilter = searchModel.GlobalFilter,

            SoQuyetDinh = searchModel.SoQuyetDinh,
            TuNgay = searchModel.TuNgay,
            DenNgay = searchModel.DenNgay,
            LoaiDuAnTheoNamId = searchModel.LoaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}