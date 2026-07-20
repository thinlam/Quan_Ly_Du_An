using System.Net.Mime;
using BuildingBlocks.Application.Attachments.Commands;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.Application.BaoCaoBanGiaoSanPhams.Commands;
using QLDA.Application.BaoCaoBanGiaoSanPhams.Queries;
using QLDA.Application.DuAns.Commands;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.BaoCaoBanGiaoSanPhams;
using QLDA.Application.BaoCaoBanGiaoSanPhams.DTOs;
namespace QLDA.WebApi.Controllers;

[Tags("Báo cáo")]
[Route("api/bao-cao-ban-giao-san-pham")]
public class BaoCaoBanGiaoSanPhamController(IServiceProvider serviceProvider)
    : AggregateRootController(serviceProvider) {
    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<BaoCaoBanGiaoSanPhamModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new BaoCaoBanGiaoSanPhamGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
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
        var res = await Mediator.Send(new BaoCaoBanGiaoSanPhamDeleteCommand(id));
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
    public async Task<ResultApi> Create([FromBody] BaoCaoBanGiaoSanPhamModel model) {
        //Cập nhật bước hiện tại của dự án
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));

        var entity = model.ToEntity();

        await Mediator.Send(new BaoCaoBanGiaoSanPhamInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.BaoCaoBanGiaoSanPham)],
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
    [ProducesResponseType<ResultApi<BaoCaoBanGiaoSanPhamModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] BaoCaoBanGiaoSanPhamModel model) {
        var entity =
            await Mediator.Send(new BaoCaoBanGiaoSanPhamGetQuery { Id = model.GetId(), ThrowIfNull = true });
        entity.Update(model);

        await Mediator.Send(new BaoCaoBanGiaoSanPhamInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        //Thêm file mới
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.BaoCaoBanGiaoSanPham)],
            Entities = danhSachTepDinhKem,
            AutoDeleteMissing = true
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// searchModel.Ten: tên báo cáo/ tiến độ
    /// searchModel.TuNgay: báo cáo từ ngày
    /// searchModel.DenNgay: báo cáo đến ngày
    /// </remarks>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<PaginatedList<BaoCaoBanGiaoSanPhamDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] BaoCaoBanGiaoSanPhamSearchModel searchModel) {
        var res = await Mediator.Send(new BaoCaoBanGiaoSanPhamGetDanhSachQuery() {
            IsNoTracking = true,
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            PageSize = searchModel.PageSize,
            PageIndex = searchModel.PageIndex,
            GlobalFilter = searchModel.GlobalFilter,

            NoiDung = searchModel.NoiDung,
            TuNgay = searchModel.TuNgay,
            DenNgay = searchModel.DenNgay,
            LoaiDuAnTheoNamId = searchModel.LoaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}