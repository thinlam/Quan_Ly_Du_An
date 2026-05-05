using System.Net.Mime;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.GoiThaus.Commands;
using QLDA.Application.GoiThaus.Queries;
using QLDA.Application.GoiThaus.DTOs;
using QLDA.Application.GoiThaus;
using QLDA.Domain.Constants;
using System.Data;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.WebApi.Controllers;

[Tags("Gói thầu")]
[Route("api/goi-thau")]
public class GoiThauController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/chi-tiet")]
    [ProducesResponseType<ResultApi<GoiThauDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new GoiThauGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery() {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem));
    }


    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        await Mediator.Send(new GoiThauDeleteCommand(id));
        return ResultApi.Ok(1);
    }

    /// <summary>
    /// Thêm mới gói thầu
    /// </summary>
    /// <remarks>
    /// Quy trình id là bắt buộc
    /// </remarks>
    /// <param name="insertDto"></param>
    /// <param name="unitOfWork"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] GoiThauInsertDto insertDto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default
    ) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        //Cập nhật bước hiện tại của dự án


        var step = await Mediator.Send(new DuAnUpdateStepCommand(insertDto.DuAnId, insertDto.BuocId), cancellationToken);
        await Mediator.Send(new DuAnUpdatePhaseCommand(insertDto.DuAnId, step), cancellationToken);

        var entity = await Mediator.Send(new GoiThauInsertCommand(insertDto), cancellationToken);
        List<TepDinhKem> files = [.. insertDto.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.GoiThau) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    /// <summary>
    /// Cập nhật gói thầu
    /// </summary>
    /// <param name="updateDto"></param>
    /// <param name="unitOfWork"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<GoiThauDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] GoiThauUpdateDto updateDto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default
    ) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var entity = await Mediator.Send(new GoiThauUpdateCommand(updateDto), cancellationToken);

        List<TepDinhKem> files = [.. updateDto.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.GoiThau) ?? []];
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
    /// <param name="searchDto"></param>
    /// <remarks>
    /// searchModel.Ten: Tên gói thầu
    /// </remarks>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<PaginatedList<GoiThauDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get(
        [FromQuery] GoiThauSearchDto searchDto) {
        var res = await Mediator.Send(new GoiThauGetDanhSachQuery(searchDto) {
            IsNoTracking = true,
        });
        return ResultApi.Ok(res);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="searchDto"></param>
    /// <remarks>
    /// searchModel.Ten: Tên gói thầu
    /// </remarks>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<PaginatedList<GoiThauDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [ResponseCache(CacheProfileName = "Combobox")]
    [HttpGet("combobox")]
    public async Task<ResultApi> GetCbo(
        [FromQuery] GoiThauSearchDto searchDto) {
        var res = await Mediator.Send(new GoiThauGetDanhSachQuery(searchDto) {
            IsNoTracking = true,
            IsCbo = true,
        });
        return ResultApi.Ok(res);
    }
}