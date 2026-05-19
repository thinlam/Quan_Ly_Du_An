using QLDA.Application.DuAns.Commands;
using QLDA.Application.ToTrinhKeHoachs.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.ToTrinhKeHoachs.Commands;
using QLDA.Application.ToTrinhKeHoachs.DTOs;
using QLDA.Application.ToTrinhKeHoachs.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.ToTrinhKeHoachs;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.ToTrinhKeHoachs;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Route("api/to-trinh-ke-hoach")]
[Tags("Tờ trình kế hoạch")]
public class ToTrinhKeHoachController(IServiceProvider serviceProvider)   : AggregateRootController(serviceProvider) {
    
  
    [ProducesResponseType<ResultApi<ToTrinhKeHoachModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new ToTrinhKeHoachGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true
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
        var res = await Mediator.Send(new ToTrinhKeHoachDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create(
        [FromBody] ToTrinhKeHoachModel model,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        //Cập nhật bước hiện tại của dự án
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));
        
        var entity = await Mediator.Send(new ToTrinhKeHoachInsertCommand(
            new()
            {
                DuAnId = model.DuAnId,
                So = model.SoToTrinh,
                NgayToTrinh = model.NgayToTrinh,
                TrichYeu = model.TrichYeu
            }
        ), cancellationToken);

        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.ToTrinhKeHoach) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    /// <summary>
    /// Cập nhật phân khai kinh phí
    /// </summary>
    [ProducesResponseType<ResultApi<ToTrinhKeHoachModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(
        [FromBody] ToTrinhKeHoachModel model,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new ToTrinhKeHoachInsertOrUpdateCommand(
            new()
            {
                Id = model.GetId(),
                DuAnId = model.DuAnId,
                So = model.SoToTrinh,
                NgayToTrinh = model.NgayToTrinh,
                TrichYeu = model.TrichYeu
            }
        ), cancellationToken);

        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.ToTrinhKeHoach) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()]
        }, cancellationToken);
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
    [ProducesResponseType<ResultApi<PaginatedList<ToTrinhKeHoachDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] ToTrinhKeHoachSearchModel searchModel) {
        var res = await Mediator.Send(new ToTrinhKeHoachGetDanhSachQuery() {
            IsNoTracking = true,
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            PageSize = searchModel.PageSize,
            PageIndex = searchModel.PageIndex,
            GlobalFilter = searchModel.GlobalFilter,

            SoQuyetDinh = searchModel.SoQuyetDinh,
            TuNgay = searchModel.TuNgay,
            DenNgay = searchModel.DenNgay,
        });
        return ResultApi.Ok(res);
    }
}