using System.Data;
using System.Net.Mime;
using QLDA.Domain.Constants;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.HopDongs;
using QLDA.Application.HopDongs.DTOs;
using QLDA.Application.HopDongs.Queries;
using QLDA.Application.HopDongs.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.TepDinhKems.Commands;

namespace QLDA.WebApi.Controllers;

[Tags("Hợp đồng")]
[Route("api/hop-dong")]
public class HopDongController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/chi-tiet")]
    [ProducesResponseType<ResultApi<HopDongDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new HopDongGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery() {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem));
    }


    /// <summary>
    /// Xoá hợp đồng - xem thêm ở remarks
    /// </summary>
    /// <remarks>
    /// Nhớ bật popup xác nhận muốn xoá <br/>
    /// Khi xoá hợp đồng sẽ xoá cả: <br/>
    /// - Phụ lục hợp đồng <br/>
    /// - Thanh toán <br/>
    /// - Tạm ứng <br/>
    /// - Tệp đính kèm của các bản ghi kể trên <br/>
    /// P/s: Xoá ở đây là IsDeleted = true <br/>
    /// </remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    [HttpDelete("{id}/xoa")]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Delete(Guid id) {
        var effected = await Mediator.Send(new HopDongDeleteCommand(id));
        return ResultApi.Ok(effected);
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// IsHopDong: null/true => true | Xác định là hợp đồng hay biên bản giao nhiệm vụ
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
        [FromBody] HopDongInsertDto insertDto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default
    ) {

        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        //Cập nhật bước hiện tại của dự án


        var step = await Mediator.Send(new DuAnUpdateStepCommand(insertDto.DuAnId, insertDto.BuocId), cancellationToken);
        await Mediator.Send(new DuAnUpdatePhaseCommand(insertDto.DuAnId, step), cancellationToken);

        var entity = await Mediator.Send(new HopDongInsertCommand(insertDto), cancellationToken);
        List<TepDinhKem> files = [.. insertDto.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.KetQuaTrungThau) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// IsHopDong: null/true => true | Xác định là hợp đồng hay biên bản giao nhiệm vụ
    /// </remarks>
    /// <param name="updateDto"></param>
    /// <param name="unitOfWork"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    [HttpPut("cap-nhat")]
    [ProducesResponseType<ResultApi<HopDongDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] HopDongUpdateDto updateDto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default
    ) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var entity = await Mediator.Send(new HopDongUpdateCommand(updateDto), cancellationToken);

        List<TepDinhKem> files = [.. updateDto.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.KetQuaTrungThau) ?? []];
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
    /// <returns></returns>
    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<HopDongDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] HopDongSearchDto searchDto) {
        var res = await Mediator.Send(new HopDongGetDanhSachQuery(searchDto) {
            IsNoTracking = true
        });
        return ResultApi.Ok(res);
    }
}