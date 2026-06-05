using System.Net.Mime;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.Commands;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DeXuatNhuCauKinhPhiNams;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

[Tags("Đề xuất kế hoạch kinh phí năm")]
[Route("api/tong-hop-kinh-phi")]
public class DeXuatNhuCauKinhPhiNamController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    [ProducesResponseType<ResultApi<DeXuatNhuCauKinhPhiNamModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new DeXuatNhuCauKinhPhiNamGetQuery() {
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
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new DeXuatNhuCauKinhPhiNamDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] DeXuatNhuCauKinhPhiNamModel model) {
        var savedEntity = await Mediator.Send(new DeXuatNhuCauKinhPhiNamInsertCommand(model.ToInsertDto()));

        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(savedEntity.Id, GroupTypeConstants.NhuCauKinhPhi) ?? []];

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = savedEntity.Id.ToString(),
            Entities = files
        });

        return ResultApi.Ok(savedEntity.Id);
    }

    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<DeXuatNhuCauKinhPhiNamModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update(
        [FromBody] DeXuatNhuCauKinhPhiNamModel model,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default) {
        var insertDto = model.ToInsertDto();
        insertDto.Id = model.GetId();

        var entity = await Mediator.Send(new DeXuatNhuCauKinhPhiNamUpdateCommand(insertDto), cancellationToken);

        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.NhuCauKinhPhi) ?? []];
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

    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<DeXuatNhuCauKinhPhiNamDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] DeXuatNhuCauKinhPhiNamSearchDto req) {
        var res = await Mediator.Send(new DeXuatNhuCauKinhPhiNamQuery() {
            So = req.So,
            DenNgay = req.DenNgay,
            TuNgay = req.TuNgay,
            TrichYeu = req.TrichYeu,
            TrangThaiId = req.TrangThaiId,
            NguoiDeXuatId = req.NguoiDeXuatId,
            PhongBanDeXuatId = req.PhongBanDeXuatId,
            GlobalFilter = req.GlobalFilter,
            PageIndex = req.PageIndex ?? 1,
            PageSize = req.PageSize ?? 1000000,
            IsNoTracking = true,
        });
        return ResultApi.Ok(res);
    }
}
