using System.Net.Mime;
using BuildingBlocks.Application.Attachments.Commands;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.Application.DangTaiKeHoachLcntLenMangs.Commands;
using QLDA.Application.DangTaiKeHoachLcntLenMangs.Queries;
using QLDA.Application.DuAns.Commands;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.DangTaiKeHoachLcntLenMangs;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers;

[Tags("Đăng tải kế hoạch lựa chọn nhà thầu lên mạng")]
[Route("api/dang-tai-ke-hoach-lcnt-len-mang")]
public class DangTaiKeHoachLcntLenMangController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    // GET

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new DangTaiKeHoachLcntLenMangGetQuery() {
            Id = id, ThrowIfNull = true, IsNoTracking = true,
        });

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()]
        ))).ToAttachmentEntities();
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }


    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new DangTaiKeHoachLcntLenMangDeleteCommand(id));
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
    [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] DangTaiKeHoachLcntLenMangModel model) {
        //Cập nhật bước hiện tại của dự án

        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));
        var entity = model.ToEntity();
        await Mediator.Send(new DangTaiKeHoachLcntLenMangInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.DangTaiKeHoachLcntLenMang)],
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
    [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] DangTaiKeHoachLcntLenMangModel model) {
        var entity =
            await Mediator.Send(new DangTaiKeHoachLcntLenMangGetQuery
                { Id = model.GetId(), ThrowIfNull = true });
        entity.Update(model);

        await Mediator.Send(new DangTaiKeHoachLcntLenMangInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        //Thêm file mới
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.DangTaiKeHoachLcntLenMang)],
            Entities = danhSachTepDinhKem,
            AutoDeleteMissing = true
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="searchModel"></param>
    /// <remarks>
    /// searchModel.Ten: Tên gói thầu
    /// </remarks>
    /// <returns></returns>
    [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get(
        [FromQuery] DangTaiKeHoachLcntLenMangSearchModel searchModel) {
        var res = await Mediator.Send(new DangTaiKeHoachLcntLenMangGetDanhSachQuery() {
            IsNoTracking = true,
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            PageSize = searchModel.PageSize,
            PageIndex = searchModel.PageIndex,
            GlobalFilter = searchModel.GlobalFilter,


            TrangThaiId = searchModel.TrangThaiId,
            KeHoachLuaChonNhaThauId = searchModel.KeHoachLuaChonNhaThauId,
            TuNgay = searchModel.TuNgay,
            DenNgay = searchModel.DenNgay,
        });
        return ResultApi.Ok(res);
    }
}