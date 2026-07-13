using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Application.TrienKhaiKeHoachLCNTMappings;
using QLDA.Application.TrienKhaiKeHoachLCNTs;
using QLDA.Application.TrienKhaiKeHoachLCNTs.Commands;
using QLDA.Application.TrienKhaiKeHoachLCNTs.DTOs;
using QLDA.Application.TrienKhaiKeHoachLCNTs.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DonViTuVanKeHoachs;
using QLDA.WebApi.Models.KetQuaThamDinhNhaThaus;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.TrienKhaiKeHoachLCNTs;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Route("api/trien-khai-ke-hoach-lcnt")]
[Tags("Tờ trình triển khai kế hoạch lcnt")]
public class TrienKhaiKeHoachLCNTController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<TrienKhaiKeHoachLCNTDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new TrienKhaiKeHoachLCNTGetQuery()
        {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes= [nameof(EGroupType.TrienKhaiKeHoachLCNT)]
        });
       ////
        var dvtvModel = entity.DonViTuVans.Select(o => new DonViTuVanKeHoachModel()
        {
            Id = o.Id,
            TenDonVi = o.TenDonVi,
        }).ToList();
        foreach ( var item in dvtvModel)
        {
            var dsTep = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
            {
                GroupId = [item.Id.ToString()],
                EGroupTypes = [nameof(EGroupType.DonViTuVan)]
            });
            item.DanhSachTepDinhKem = dsTep.Select(o => o.ToModel()).ToList(); // i need ways
        }


        return ResultApi.Ok(entity.ToModel(donViTuVan: dvtvModel, danhSachTepDinhKem: danhSachTepDinhKem.ToList()
    ));
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id)
    {
        var res = await Mediator.Send(new TrienKhaiKeHoachLCNTDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create( [FromBody] TrienKhaiKeHoachLCNTModel dto, [FromServices] IUnitOfWork unitOfWork,  CancellationToken cancellationToken = default)
    {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId), cancellationToken);
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step), cancellationToken);

        var entity = await Mediator.Send(new TrienKhaiKeHoachLCNTInsertCommand(dto.ToEntity()), cancellationToken);
        var danhSachTepDinhKem = dto.GetDanhSachTep(entity.Id).ToList();

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
        }, cancellationToken);
       
        var danhSachFileKetQua = new List<TepDinhKem>();
        foreach (var dv in dto.DonViTuVans)
        {
            var id = dv.GetId();
            danhSachFileKetQua = dv.GetDanhSachTep(id).ToList();

            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
            {
                GroupId = id.ToString(),
                Entities = danhSachFileKetQua
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    [ProducesResponseType<ResultApi<TrienKhaiKeHoachLCNTDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] TrienKhaiKeHoachLCNTModel model, [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        using var tx = await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        var entity = await Mediator.Send(new TrienKhaiKeHoachLCNTUpdateCommand(model.ToEntity()), cancellationToken);

        var danhSachTepChinh = model.GetDanhSachTep(entity.Id).ToList();
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepChinh,
            ScopeGroupTypes = [nameof(EGroupType.TrienKhaiKeHoachLCNT)]
        }, cancellationToken);

        foreach (var dv in model.DonViTuVans ?? [])
        {
            var dvId = dv.Id ?? dv.GetId();
            var files = dv.GetDanhSachTep(dvId).ToList();
            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
            {
                GroupId = dvId.ToString(),
                Entities = files,
                ScopeGroupTypes = [nameof(EGroupType.DonViTuVan)]
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        return ResultApi.Ok(entity.ToDto(danhSachTepChinh));
    }

    [ProducesResponseType<ResultApi<PaginatedList<TrienKhaiKeHoachLCNTDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] TrienKhaiKeHoachLCNTSearchDto dto)
    {
        var res = await Mediator.Send(new TrienKhaiKeHoachLCNTDanhSachQuery()
        {
            IsNoTracking = true,
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            PageSize = dto.PageSize??1,
            PageIndex = dto.PageIndex??10,
            GlobalFilter = dto.GlobalFilter,
            So = dto.So,
            TuNgay = dto.TuNgay,
            DenNgay = dto.DenNgay,
        });
        return ResultApi.Ok(res);
    }
}