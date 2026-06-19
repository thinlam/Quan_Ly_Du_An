using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.QuyetDinhLapBanQLDAs.Commands;
using QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;
using QLDA.Application.QuyetDinhLapBanQLDAs.Queries;
using QLDA.WebApi.Models.QuyetDinhLapBanQLDAs;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

[Route("api/quyet-dinh-thanh-lap-bqlda")]
[Tags("Quyết định thành lập ban quản lý dự án")]
public class QuyetDinhThanhLapBanQldaController(IServiceProvider serviceProvider)
    : AggregateRootController(serviceProvider) {
    // GET

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<QuyetDinhLapBanQldaModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new QuyetDinhLapBanQldaGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
            IncludeThanhVien = true,
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery() {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }


    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new QuyetDinhLapBanQldaDeleteCommand(id));
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
    public async Task<ResultApi> Create([FromBody] QuyetDinhLapBanQldaModel model) {
        //Cập nhật bước hiện tại của dự án
        
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));
        var entity = model.ToEntity();
        await Mediator.Send(new QuyetDinhLapBanQldaInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
        });

        return ResultApi.Ok(entity.Id);
    }

    /// <summary>
    /// Cập nhật 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<QuyetDinhLapBanQldaModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] QuyetDinhLapBanQldaModel model) {
        var entity =
            await Mediator.Send(new QuyetDinhLapBanQldaGetQuery { Id = model.GetId(), ThrowIfNull = true });
        entity.Update(model);

        await Mediator.Send(new QuyetDinhLapBanQldaInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        //Thêm file mới
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
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
    [ProducesResponseType<ResultApi<PaginatedList<QuyetDinhLapBanQldaDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] QuyetDinhLapBanQldaSearchModel searchModel) {
        var res = await Mediator.Send(new QuyetDinhLapBanQldaGetDanhSachQuery() {
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