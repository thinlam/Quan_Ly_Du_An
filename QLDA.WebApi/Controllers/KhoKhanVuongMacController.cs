using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using BuildingBlocks.Application.Attachments.Commands;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.Application.KhoKhanVuongMacs.Commands;
using QLDA.Application.KhoKhanVuongMacs.DTOs;
using QLDA.Application.KhoKhanVuongMacs.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.KhoKhanVuongMacs;

namespace QLDA.WebApi.Controllers;

[Tags("Khó khăn vướng mắc")]
[Route("api/kho-khan-vuong-mac")]
public class KhoKhanVuongMacController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/chi-tiet")]
    [ProducesResponseType<ResultApi<KhoKhanVuongMacModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new KhoKhanVuongMacGetQuery() {
            Id = id, ThrowIfNull = true, IsNoTracking = true,
        });

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            IncludeSigned: false
        ))).ToAttachmentEntities();
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }


    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    [HttpDelete("{id}/xoa")]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new KhoKhanVuongMacDeleteCommand(id));
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
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] KhoKhanVuongMacModel model) {
        //Cập nhật bước hiện tại của dự án

        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));
        var entity = model.ToEntity();
        await Mediator.Send(new KhoKhanVuongMacInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.KhoKhanVuongMac), nameof(EGroupType.KetQuaXuLyKhoKhanVuongMac)],
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
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    [HttpPut("cap-nhat")]
    [ProducesResponseType<ResultApi<KhoKhanVuongMacModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] KhoKhanVuongMacModel model) {
        var entity =
            await Mediator.Send(new KhoKhanVuongMacGetQuery
                { Id = model.GetId(), ThrowIfNull = true });
        entity.Update(model);

        await Mediator.Send(new KhoKhanVuongMacInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.KhoKhanVuongMac), nameof(EGroupType.KetQuaXuLyKhoKhanVuongMac)],
            Entities = danhSachTepDinhKem,
            AutoDeleteMissing = true
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<KhoKhanVuongMacDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] KhoKhanVuongMacSearchModel searchModel) {
        var res = await Mediator.Send(new KhoKhanVuongMacGetDanhSachQuery() {
            IsNoTracking = true,
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            PageSize = searchModel.PageSize,
            PageIndex = searchModel.PageIndex,
            GlobalFilter = searchModel.GlobalFilter,

            NoiDung = searchModel.NoiDung,
            TinhTrangId = searchModel.TinhTrangId,
            MucDoKhoKhanId = searchModel.MucDoKhoKhanId,
            LoaiDuAnId = searchModel.LoaiDuAnId,
            LoaiDuAnTheoNamId = searchModel.LoaiDuAnTheoNamId,
            TuNgay = searchModel.TuNgay,
            DenNgay = searchModel.DenNgay,
            LanhDaoPhuTrachId = searchModel.LanhDaoPhuTrachId,
            DonViPhuTrachChinhId = searchModel.DonViPhuTrachChinhId,
            DonViPhoiHopId = searchModel.DonViPhoiHopId,
        });
        return ResultApi.Ok(res);
        // hiện tại
        // kế hoạch -
        //          -
        //          -
        //          - thực tế
        //mong muốn
        // kế hoạch -
        //          - thực tế

    }
}