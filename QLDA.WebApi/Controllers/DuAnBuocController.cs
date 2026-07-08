using QLDA.Application.DuAnBuocs;
using QLDA.Application.DuAnBuocs.Commands;
using QLDA.Application.DuAnBuocs.DTOs;
using QLDA.Application.DuAnBuocs.Queries;
using QLDA.Application.DuAns.Commands;
using System.Data;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Tags("Dự án bước")]
public class DuAnBuocController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    // /// <summary>
    // /// Chi tiết
    // /// </summary>
    // /// <param name="id"></param>
    // /// <returns></returns>
    // [ProducesResponseType<ResultApi<DuAnBuocDto>>(StatusCodes.Status200OK)]
    // [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    // [HttpGet("{id}/chi-tiet")]
    // public async Task<ResultApi> Get(int id) {
    //     var entity = await Mediator.Send(new DuAnBuocGetQuery() {
    //         Id = id,
    //         IncludeManHinh = true,
    //         IsNoTracking = true,
    //     });

    //     return ResultApi.Ok(entity.ToDto());
    // }

    /// <summary>
    /// Danh sách bước theo dự án
    /// </summary>
    /// <remarks>
    /// Dùng cho show quy trình
    /// </remarks>
    /// <param name="duAnId"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<List<DuAnBuocStateDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("api/du-an-buoc/danh-sach/{duAnId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> GetDanhSach(Guid duAnId) {
        var res = await Mediator.Send(new DuAnBuocGetTreeListQuery() {
            DuAnId = duAnId,
        });
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Cập nhật bước
    /// </summary>
    /// <param name="updateStateDto"></param>
    /// <param name="unitOfWork"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<DuAnBuocDuAnUpdateStateDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("api/du-an-buoc/cap-nhat-trang-thai")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> UpdateState(
    [FromBody] DuAnBuocDuAnUpdateStateDto updateStateDto,
    IUnitOfWork unitOfWork,
    CancellationToken cancellationToken = default) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var entity = await Mediator.Send(new DuAnBuocDuAnUpdateStateCommand(updateStateDto), cancellationToken);

        //cập nhật bước hiện tại của dự án

        var step = await Mediator.Send(new DuAnUpdateStepCommand(entity.DuAnId, entity.Id), cancellationToken);
        await Mediator.Send(new DuAnUpdatePhaseCommand(entity.DuAnId, step), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        return ResultApi.Ok(entity.ToUpdateStateDto());
    }

    /// <summary>
    /// Danh sách
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<PaginatedList<DuAnBuocDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("api/du-an-buoc/danh-sach-tien-do")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> GetDanhSachTienDo([FromQuery] AggregateRootPagination req, string? globalFilter, int? quyTrinhId, Guid? duAnId) {
        var res = await Mediator.Send(new DuAnBuocGetDanhSachQuery() {
            QuyTrinhId = quyTrinhId,
            DuAnId = duAnId,
            PageIndex = req.PageIndex,
            GlobalFilter = globalFilter,
            PageSize = req.PageSize,
        });
        return ResultApi.Ok(res);
    }
    /// <summary>
    /// Danh sách bước theo dự án
    /// </summary>
    /// <remarks>
    /// Dùng cho show quy trình
    /// </remarks>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<DuAnBuocDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("api/du-an-buoc/quy-trinh/{id:int}/chi-tiet")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Get(int id) {
        var entity = await Mediator.Send(new DuAnBuocGetQuery() {
            Id = id,
            IncludeManHinh = true,
        });

        return ResultApi.Ok(entity.ToDto());
    }

    /// <summary>
    /// quy trình - bước - danh sách
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<PaginatedList<DuAnBuocDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("api/du-an-buoc/quy-trinh/danh-sach-day-du")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> GetAll([FromQuery] AggregateRootPagination req, string? globalFilter, int? quyTrinhId, Guid? duAnId) {
        var res = await Mediator.Send(new DuAnBuocGetDanhSachQuery() {
            QuyTrinhId = quyTrinhId,
            DuAnId = duAnId,
            PageIndex = req.PageIndex,
            GlobalFilter = globalFilter,
            PageSize = req.PageSize,
        });
        return ResultApi.Ok(res);
    }
    /// <summary>
    /// quy trình - bước
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<DuAnBuocDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("api/du-an-buoc/quy-trinh/cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> QuyTrinhCapNhat(
        [FromBody] DuAnBuocUpdateDto dto,
        CancellationToken cancellationToken
        ) {
        var entity = await Mediator.Send(new DuAnBuocUpdateCommand(dto), cancellationToken);
        return ResultApi.Ok(entity.ToDto());
    }

    // /// <summary>
    // /// Thêm mới bước
    // /// </summary>
    // /// <param name="dto"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // [ProducesResponseType<ResultApi<DuAnBuocDto>>(StatusCodes.Status200OK)]
    // [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    // [HttpPost("api/du-an-buoc/them-moi")]
    // [Consumes(MediaTypeNames.Application.Json)]
    // public async Task<ResultApi> Create(
    //     [FromBody] DuAnBuocCreateDto dto,
    //     CancellationToken cancellationToken
    //     ) {
    //     var entity = await Mediator.Send(new DuAnBuocCreateCommand(dto), cancellationToken);
    //     return ResultApi.Ok(entity.ToDto());
    // }

}
