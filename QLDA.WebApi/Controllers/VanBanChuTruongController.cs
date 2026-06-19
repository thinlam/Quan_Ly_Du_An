using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.VanBanChuTruongs.Commands;
using QLDA.Application.VanBanChuTruongs.DTOs;
using QLDA.Application.VanBanChuTruongs.Queries;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.VanBanChuTruongs;

namespace QLDA.WebApi.Controllers;

[Tags("Văn bản chủ trương")]
[Route("api/van-ban-chu-truong")]
public class VanBanChuTruongController : AggregateRootController {
    // GET
    public VanBanChuTruongController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<VanBanChuTruongModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new VanBanChuTruongGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery() {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }


    [HttpDelete("{id}/xoa")]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new VanBanChuTruongDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Thêm mới văn bản chủ trương
    /// </summary>
    /// <remarks>
    /// Quy trình id là bắt buộc
    /// </remarks>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] VanBanChuTruongModel model) {
        //Cập nhật bước hiện tại của dự án
        
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));
        var entity = model.ToEntity();
        await Mediator.Send(new VanBanChuTruongInsertOrUpdateCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
        });

        return ResultApi.Ok(entity.Id);
    }

    /// <summary>
    /// Cập nhật văn bản chủ trương
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<VanBanChuTruongModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update([FromBody] VanBanChuTruongModel model) {
        var entity = await Mediator.Send(new VanBanChuTruongGetQuery { Id = model.GetId(), ThrowIfNull = true });

        entity.Update(model);

        await Mediator.Send(new VanBanChuTruongInsertOrUpdateCommand(entity));

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
    /// <param name="duAnId"></param>
    /// <param name="buocId"></param>
    /// <param name="globalFilter"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet("danh-sach-tien-do")]

    [ProducesResponseType<ResultApi<PaginatedList<VanBanChuTruongDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] Guid? duAnId, int? buocId, string? globalFilter = null, int pageIndex = 0, int pageSize = 0, int? loaiDuAnTheoNamId = null) {
        var res = await Mediator.Send(new VanBanChuTruongGetDanhSachQuery() {
            DuAnId = duAnId,
            BuocId = buocId,
            GlobalFilter = globalFilter,
            PageIndex = pageIndex,
            PageSize = pageSize,
            IsNoTracking = true,
            LoaiDuAnTheoNamId = loaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}