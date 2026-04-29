using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.PheDuyetDuToans.Commands;
using QLDA.Application.PheDuyetDuToans.DTOs;
using QLDA.Application.PheDuyetDuToans.Queries;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.PheDuyetDuToans;

namespace QLDA.WebApi.Controllers;
/// <summary>
/// Vaăn bản quyết định dự toán
/// </summary>
[Tags("Phê duyệt dự toán")]
[Route("api/phe-duyet-du-toan")]
public class PheDuyetDuToanController : AggregateRootController {
    // GET
    public PheDuyetDuToanController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<PheDuyetDuToanModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new PheDuyetDuToanGetQuery() {
            Id = id, ThrowIfNull = true, IsNoTracking = true,
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
        var res = await Mediator.Send(new PheDuyetDuToanDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Thêm mới văn bản quyết định dự toán
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
    public async Task<ResultApi> Create([FromBody] PheDuyetDuToanModel model) {
        //Cập nhật bước hiện tại của dự án

        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));
        var entity = model.ToEntity();
        await Mediator.Send(new PheDuyetDuToanInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
        });

        return ResultApi.Ok(entity.Id);
    }

    /// <summary>
    /// Cập nhật văn bản quyết định dự toán
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<PheDuyetDuToanModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] PheDuyetDuToanModel model) {
        var entity = await Mediator.Send(new PheDuyetDuToanGetQuery
            { Id = model.GetId(), ThrowIfNull = true });

        entity.Update(model);

        await Mediator.Send(new PheDuyetDuToanInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        //Thêm file mới
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    /// <summary>
    /// Trình phê duyệt dự toán - chỉ phòng KH-TC
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("{id}/trinh")]
    public async Task<ResultApi> Trinh(Guid id) {
        var res = await Mediator.Send(new PheDuyetDuToanTrinhCommand(id));
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Duyệt phê duyệt dự toán - chỉ BGĐ
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("{id}/duyet")]
    public async Task<ResultApi> Duyet(Guid id) {
        var res = await Mediator.Send(new PheDuyetDuToanDuyetCommand(id));
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Trả lại phê duyệt dự toán - chỉ BGĐ, cần lý do
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("{id}/tra-lai")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> TraLai(Guid id, [FromBody] TraLaiModel model) {
        var res = await Mediator.Send(new PheDuyetDuToanTraLaiCommand(id, model.NoiDung));
        return ResultApi.Ok(res);
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
    [ProducesResponseType<ResultApi<PaginatedList<PheDuyetDuToanDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] Guid? duAnId, int? buocId,string? globalFilter = null, int pageIndex = 0, int pageSize = 0) {
        var res = await Mediator.Send(new PheDuyetDuToanGetDanhSachQuery() {
            DuAnId = duAnId, BuocId = buocId,
            GlobalFilter = globalFilter,
            PageIndex = pageIndex, PageSize = pageSize, IsNoTracking = true,
        });
        return ResultApi.Ok(res);
    }
}