using BuildingBlocks.Domain.Entities.Abstractions;
using QLDA.Application.QuyetDinhDieuChinhs.Commands;
using QLDA.Application.QuyetDinhDieuChinhs.DTOs;
using QLDA.Application.QuyetDinhDieuChinhs.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Quyết định điều chỉnh phê duyệt (UC64 - #9483)
/// </summary>
[Tags("Quyết định điều chỉnh")]
[Route("api/quyet-dinh-dieu-chinh")]
public class QuyetDinhDieuChinhController : AggregateRootController {
    public QuyetDinhDieuChinhController(IServiceProvider serviceProvider) : base(serviceProvider) { }

    /// <summary>
    /// Danh sách quyết định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<PagedResultDto<QuyetDinhDieuChinhListItemDto>>), StatusCodes.Status200OK)]
    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetDanhSach(
        [FromQuery] Guid? duAnId,
        [FromQuery] string? globalFilter = null,
        int page = 1,
        int pageSize = 20,
        int? loaiDuAnTheoNamId = null) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhGetDanhSachQuery {
            DuAnId = duAnId,
            //PheDuyetEntityName = pheDuyetEntityName,
           // PheDuyetEntityId = pheDuyetEntityId,
            GlobalFilter = globalFilter,
            Page = page,
            PageSize = pageSize,
            LoaiDuAnTheoNamId = loaiDuAnTheoNamId,
        });
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Chi tiết quyết định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<QuyetDinhDieuChinhChiTietDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> GetChiTiet(Guid id) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhGetChiTietQuery(id));
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Thêm mới quyết định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] QuyetDinhDieuChinhInsertDto dto,
          [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhInsertCommand(dto), cancellationToken);
        List<TepDinhKem> files = [.. dto.DanhSachTepDinhKem?.ToEntities(result.Id, EGroupType.QuyetDinhDieuChinh) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = result.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Cập nhật quyết định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] QuyetDinhDieuChinhUpdateDto dto,
        [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhUpdateCommand(dto), cancellationToken);
        List<TepDinhKem> files = [.. dto.DanhSachTepDinhKem?.ToEntities(result.Id, EGroupType.QuyetDinhDieuChinh) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = result.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken); 
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Xóa quyết định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}")]
    public async Task<ResultApi> Delete(Guid id, CancellationToken cancellationToken) {
        await Mediator.Send(new QuyetDinhDieuChinhDeleteCommand(id), cancellationToken);
        return ResultApi.Ok();
    }
}