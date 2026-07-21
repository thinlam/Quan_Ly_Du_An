using System.Data;
using BuildingBlocks.Domain.Entities;
using System.Net.Mime;
using QLDA.Application.ThanhLyHopDongs.Commands;
using QLDA.Application.ThanhLyHopDongs.DTOs;
using QLDA.Application.ThanhLyHopDongs.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;

namespace QLDA.WebApi.Controllers;

[Tags("Thanh lý hợp đồng - Phiếu trình nghiệm thu(thanh-ly-hop-dong)")]
[Route("api/thanh-ly-hop-dong")]
public class ThanhLyHopDongController : AggregateRootController {
    public ThanhLyHopDongController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    [HttpGet("{id}/chi-tiet")]
    [ProducesResponseType<ResultApi<ThanhLyHopDongDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new ThanhLyHopDongGetQuery {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem));
    }

    [HttpDelete("{id}/xoa")]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Delete(Guid id) {
        await Mediator.Send(new ThanhLyHopDongDeleteCommand(id));
        return ResultApi.Ok(1);
    }

    /// <summary>
    /// Thêm mới
    /// </summary>
    [HttpPost("them-moi")]
    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create(
        [FromBody] ThanhLyHopDongInsertDto insertDto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var entity = await Mediator.Send(new ThanhLyHopDongInsertCommand(insertDto), cancellationToken);

        List<Attachment> files = [
            .. insertDto.BienBanNghiemThus?.ToEntities(entity.Id, EGroupType.ThanhLyHopDong_BienBanNghiemThu) ?? [],
            .. insertDto.ThanhLyHopDongs?.ToEntities(entity.Id, EGroupType.ThanhLyHopDong) ?? [],
            .. insertDto.Khacs?.ToEntities(entity.Id, EGroupType.ThanhLyHopDong_Khac) ?? []
        ];
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
    [HttpPut("cap-nhat")]
    [ProducesResponseType<ResultApi<ThanhLyHopDongDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(
        [FromBody] ThanhLyHopDongUpdateDto updateDto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default) {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var entity = await Mediator.Send(new ThanhLyHopDongUpdateCommand(updateDto), cancellationToken);

        List<Attachment> files = [
            .. updateDto.BienBanNghiemThus?.ToEntities(entity.Id, EGroupType.ThanhLyHopDong_BienBanNghiemThu) ?? [],
            .. updateDto.ThanhLyHopDongs?.ToEntities(entity.Id, EGroupType.ThanhLyHopDong) ?? [],
            .. updateDto.Khacs?.ToEntities(entity.Id, EGroupType.ThanhLyHopDong_Khac) ?? []
        ];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        return ResultApi.Ok(entity.ToDto(files));
    }

    /// <summary>
    /// Danh sách tiến độ (phân trang)
    /// </summary>
    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<ThanhLyHopDongDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetDanhSachTienDo(
        [FromQuery] ThanhLyHopDongGetDanhSachTienDoQuery req) {
        var res = await Mediator.Send(req);
        return ResultApi.Ok(res);
    }

    /// <summary>
    /// Danh sách tiến độ bước (BuocId bắt buộc)
    /// </summary>
    [HttpGet("danh-sach")]
    [ProducesResponseType<ResultApi<List<ThanhLyHopDongDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetDanhSach(
        [FromQuery] ThanhLyHopDongGetDanhSachQuery req) {
        var res = await Mediator.Send(req);
        return ResultApi.Ok(res);
    }
}
