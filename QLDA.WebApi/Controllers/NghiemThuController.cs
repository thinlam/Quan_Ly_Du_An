using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.NghiemThus.Commands;
using QLDA.Application.NghiemThus.DTOs;
using QLDA.Application.NghiemThus.Queries;
using QLDA.Domain.Constants;
using QLDA.Application.NghiemThus;
using QLDA.Application.TepDinhKems.DTOs;
using System.Data;

namespace QLDA.WebApi.Controllers;

[Tags("Nghiệm thu")]
[Route("api/nghiem-thu")]
public class NghiemThuController : AggregateRootController {
    // GET
    public NghiemThuController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/chi-tiet")]
    [ProducesResponseType<ResultApi<NghiemThuDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new NghiemThuGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
            IncludePhuLucHopDong = true,
            IncludeThanhToan = true,
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery() {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem));
    }


    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    [HttpDelete("{id}/xoa")]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Delete(Guid id) {
        await Mediator.Send(new NghiemThuDeleteCommand(id));
        return ResultApi.Ok(1);
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// Quy trình id là bắt buộc
    /// </remarks>
    /// <param name="insertDto"></param>
    /// <param name="unitOfWork"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    [HttpPost("them-moi")]
    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create(
        [FromBody] NghiemThuInsertDto insertDto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        //Cập nhật bước hiện tại của dự án


        var step = await Mediator.Send(new DuAnUpdateStepCommand(insertDto.DuAnId, insertDto.BuocId), cancellationToken);
        await Mediator.Send(new DuAnUpdatePhaseCommand(insertDto.DuAnId, step), cancellationToken);

        var entity = await Mediator.Send(new NghiemThuInsertCommand(insertDto), cancellationToken);

        List<TepDinhKem> files = [.. insertDto.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.NghiemThu) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    /// <summary>
    /// Cập nhật
    /// </summary>
    /// <param name="updateDto"></param>
    /// <param name="unitOfWork"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    [HttpPut("cap-nhat")]
    [ProducesResponseType<ResultApi<NghiemThuDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(
        [FromBody] NghiemThuUpdateDto updateDto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var entity = await Mediator.Send(new NghiemThuUpdateCommand(updateDto), cancellationToken);

        List<TepDinhKem> files = [.. updateDto.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.NghiemThu) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        return ResultApi.Ok(entity.ToDto(files));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="duAnId"></param>
    /// <param name="buocId"></param>
    /// <param name="hopDongId"></param>
    /// <param name="thanhToanId"></param>
    /// <param name="globalFilter"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<NghiemThuDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] Guid? duAnId, int? buocId,
        Guid? hopDongId = null,
        Guid? thanhToanId = null,
        string? globalFilter = null,
        int pageIndex = 0,
        int pageSize = 0) {
        var res = await Mediator.Send(new NghiemThuGetDanhSachQuery() {
            DuAnId = duAnId,
            BuocId = buocId,
            HopDongId = hopDongId,
            ThanhToanId = thanhToanId,
            GlobalFilter = globalFilter,
            PageIndex = pageIndex,
            PageSize = pageSize,
            IsNoTracking = true,
        });
        return ResultApi.Ok(res);
    }
    /// <summary>
    /// Danh sách combobox
    /// </summary>
    /// <param name="duAnId"></param>
    /// <param name="buocId"></param>
    /// <param name="hopDongId"></param>
    /// <param name="thanhToanId"></param>
    /// <param name="globalFilter"></param>
    /// <returns></returns>
    [ResponseCache(CacheProfileName = "Combobox")]
    [HttpGet("combobox")]
    [ProducesResponseType<ResultApi<PaginatedList<NghiemThuDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetCbo([FromQuery] Guid? duAnId, int? buocId,
        Guid? hopDongId = null,
        Guid? thanhToanId = null,
        string? globalFilter = null) {
        var res = await Mediator.Send(new NghiemThuGetDanhSachQuery() {
            DuAnId = duAnId,
            BuocId = buocId,
            HopDongId = hopDongId,
            ThanhToanId = thanhToanId,
            GlobalFilter = globalFilter,
            PageIndex = 0,
            PageSize = 0,
            IsNoTracking = true,
            IsCbo = true
        });
        return ResultApi.Ok(res);
    }
}