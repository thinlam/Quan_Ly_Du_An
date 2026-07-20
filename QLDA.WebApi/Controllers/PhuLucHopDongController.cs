using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using BuildingBlocks.Application.Attachments.Commands;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.Application.PhuLucHopDongs.Commands;
using QLDA.Application.PhuLucHopDongs.DTOs;
using QLDA.Application.PhuLucHopDongs.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.PhuLucHopDongs;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

[Tags("Phụ lục hợp đồng")]
[Route("api/phu-luc-hop-dong")]
public class PhuLucHopDongController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    // GET

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/chi-tiet")]
    [ProducesResponseType<ResultApi<PhuLucHopDongModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new PhuLucHopDongGetQuery() {
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
        await Mediator.Send(new PhuLucHopDongDeleteCommand(id));
        return ResultApi.Ok(1);
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
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] PhuLucHopDongModel model) {
        //Cập nhật bước hiện tại của dự án

        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));
        var entity = model.ToEntity();
        await Mediator.Send(new PhuLucHopDongInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.PhuLucHopDong)],
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
    [ProducesResponseType<ResultApi<PhuLucHopDongModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] PhuLucHopDongModel model) {
        var entity =
            await Mediator.Send(new PhuLucHopDongGetQuery
                { Id = model.GetId(), ThrowIfNull = true });
        entity.Update(model);

        await Mediator.Send(new PhuLucHopDongInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        //Thêm file mới
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.PhuLucHopDong)],
            Entities = danhSachTepDinhKem,
            AutoDeleteMissing = true
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <returns></returns>
    ///
    /// PhuLucHopDongGetChuaThanhToanQuery
    [HttpGet("danh-sach-cbobox")]
    [ProducesResponseType<ResultApi<PaginatedList<PhuLucHopDongDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetCboThanhToan([FromQuery] PhuLucHopDongSearchModel searchModel)
    {
        var res = await Mediator.Send(new PhuLucHopDongGetChuaThanhToanQuery()
        {
            IsNoTracking = true,
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            GlobalFilter = searchModel.GlobalFilter,
            Ten = searchModel.Ten,
            SoPhuLucHopDong = searchModel.SoPhuLucHopDong,
            HopDongId = searchModel.HopDongId,
            ThanhToanId = searchModel.ThanhToanId
        });
        return ResultApi.Ok(res);
    }

    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<PhuLucHopDongDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] PhuLucHopDongSearchModel searchModel) {
        var res = await Mediator.Send(new PhuLucHopDongGetDanhSachQuery() {
            IsNoTracking = true,
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            PageSize = searchModel.PageSize,
            PageIndex = searchModel.PageIndex,
            GlobalFilter = searchModel.GlobalFilter,

            Ten = searchModel.Ten,
            SoPhuLucHopDong = searchModel.SoPhuLucHopDong,
            NoiDung = searchModel.NoiDung,
            HopDongId = searchModel.HopDongId,
            TuNgay = searchModel.TuNgay,
            DenNgay = searchModel.DenNgay,
            LoaiDuAnTheoNamId = searchModel.LoaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}