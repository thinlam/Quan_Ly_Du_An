using System.Data;
using System.Net.Mime;
using QLDA.Domain.Constants;
using QLDA.Application.PhanKhaiKinhPhis.Commands;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Application.PhanKhaiKinhPhis.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.PhanKhaiKinhPhis;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Phân khai kinh phí (UC40 - #9467)
/// </summary>
[Tags("Phân khai kinh phí")]
[Route("api/phan-khai-kinh-phi")]
public class PhanKhaiKinhPhiController : AggregateRootController {
    public PhanKhaiKinhPhiController(IServiceProvider serviceProvider) : base(serviceProvider) { }

    /// <summary>
    /// Chi tiết phân khai kinh phí
    /// </summary>
    [ProducesResponseType<ResultApi<PhanKhaiKinhPhiModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new PhanKhaiKinhPhiGetQuery {
            Id = id, ThrowIfNull = true, IsNoTracking = true,
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    /// <summary>
    /// Danh sách phân khai kinh phí
    /// </summary>
    [ProducesResponseType<ResultApi<PaginatedList<PhanKhaiKinhPhiDto>>>(StatusCodes.Status200OK)]
    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetDanhSach(
        [FromQuery] Guid? duAnId,
        [FromQuery] string? globalFilter = null,
        [FromQuery] int? trangThaiId = null,
        int pageIndex = 0, int pageSize = 0) {
        var res = await Mediator.Send(new PhanKhaiKinhPhiGetDanhSachQuery {
            DuAnId = duAnId, GlobalFilter = globalFilter,
            TrangThaiId = trangThaiId,
            PageIndex = pageIndex, PageSize = pageSize, IsNoTracking = true,
        });
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Thêm mới phân khai kinh phí
    /// </summary>
    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create(
        [FromBody] PhanKhaiKinhPhiModel model,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default) {
        var entity = await Mediator.Send(new PhanKhaiKinhPhiInsertCommand(
            new() {
                DuAnId = model.DuAnId,
               // BuocId = model.BuocId,
                SoToTrinh = model.SoToTrinh,
                NgayToTrinh = model.NgayToTrinh,
                NguonVonId = model.NguonVonId,
                KinhPhiDeXuat = model.KinhPhiDeXuat,
                KinhPhiPhanKhai = model.KinhPhiPhanKhai,
                ThuyetMinh = model.ThuyetMinh,
                TrichYeu = model.TrichYeu,
            }
        ), cancellationToken);

        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.PhanKhaiKinhPhi) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    /// <summary>
    /// Cập nhật phân khai kinh phí
    /// </summary>
    [ProducesResponseType<ResultApi<PhanKhaiKinhPhiModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(
        [FromBody] PhanKhaiKinhPhiModel model,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default) {
        var entity = await Mediator.Send(new PhanKhaiKinhPhiUpdateCommand(
            new() {
                Id = model.GetId(),
                DuAnId = model.DuAnId,
                SoToTrinh = model.SoToTrinh,
                NgayToTrinh = model.NgayToTrinh,
                NguonVonId = model.NguonVonId,
                KinhPhiDeXuat = model.KinhPhiDeXuat,
                KinhPhiPhanKhai = model.KinhPhiPhanKhai,
                ThuyetMinh = model.ThuyetMinh,
                TrichYeu = model.TrichYeu
            }
        ), cancellationToken);

        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.PhanKhaiKinhPhi) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery {
            GroupId = [entity.Id.ToString()]
        }, cancellationToken);
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    /// <summary>
    /// Xóa phân khai kinh phí
    /// </summary>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new PhanKhaiKinhPhiDeleteCommand(id));
        return ResultApi.Ok(res);
    }
}