using Microsoft.AspNetCore.Mvc;
using QLDA.Application.HoSoMoiThauDienTus.Commands;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.HoSoMoiThauDienTus.Queries;
using QLDA.Application.KeHoachLuaChonNhaThaus.DTOs;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.HoSoMoiThauDienTus;
using QLDA.WebApi.Models.TepDinhKems;
using System.Data;
using System.Net.Mime;
namespace QLDA.WebApi.Controllers;

[Tags("Hồ sơ mời thầu điện tử")]
[Route("api/ho-so-moi-thau-dien-tu")]
public class HoSoMoiThauDienTuController(IServiceProvider sp) : AggregateRootController(sp)
{

    [HttpGet("{id}")]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new HoSoMoiThauDienTuGetQuery { Id = id });
        var files = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTu.ToString()]
        });
        var filesToTrinh = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.ToTrinh!= null ? entity.ToTrinh?.Id.ToString() :""],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTuToTrinh.ToString()]
        });
        var filesQuyetDinh = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.QuyetDinh != null ? entity.QuyetDinh?.Id.ToString() :""],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString()]
        });
        var fileCamKets = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTuCamKetTD.ToString()]
        });
        var fileThamDinhs = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTuQuyetDinhTD.ToString()]

        });
        var fileBaoCaos = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTuBaoCaoTD.ToString()]
        });
        return ResultApi.Ok(entity.ToModel(files, fileCamKets, fileThamDinhs, fileBaoCaos, filesToTrinh, filesQuyetDinh));
    }

    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetAll([FromQuery] HoSoMoiThauDienTuSearchDto dto, string? globalFilter)
    {
        dto.GlobalFilter = globalFilter;
        var result = await Mediator.Send(new HoSoMoiThauDienTuGetDanhSachQuery(dto));
        return ResultApi.Ok(result);
    }

    [HttpPost("them-moi")]
    public async Task<ResultApi> Create([FromBody] HoSoMoiThauDienTuModel model,
        [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var entity = await Mediator.Send(new HoSoMoiThauDienTuInsertCommand(model.ToInsertDto()));
        await SaveDanhSachTepDinhKemAsync(model, entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        return ResultApi.Ok(entity.Id);
      

    }

    [HttpPut("cap-nhat")]
    public async Task<ResultApi> Update([FromBody] HoSoMoiThauDienTuModel model, [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var entity = await Mediator.Send(new HoSoMoiThauDienTuUpdateCommand(model.ToUpdateModel()));
        await SaveDanhSachTepDinhKemAsync(model, entity, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        return ResultApi.Ok(entity.Id);
    }

    [HttpDelete("{id}")]
    public async Task<ResultApi> Delete(Guid id)
    {
        await Mediator.Send(new HoSoMoiThauDienTuDeleteCommand(id));
        return ResultApi.Ok(1);
    }
    private async Task SaveDanhSachTepDinhKemAsync(HoSoMoiThauDienTuModel model, HoSoMoiThauDienTu entity, CancellationToken cancellationToken)
    {
        var entityId = entity.Id;

        await SyncTepDinhKemAsync(
            entityId.ToString(),
            model.GetDanhSachTepDinhKem(entityId),
            EGroupType.HoSoMoiThauDienTu.ToString(),
            cancellationToken);

        if (entity.ToTrinh != null)
        {
            var toTrinhId = entity.ToTrinh.Id;
            await SyncTepDinhKemAsync(
                toTrinhId.ToString(),
                model.ToTrinh?.GetDanhSachTepDinhKemToTrinh(toTrinhId) ?? [],
                EGroupType.HoSoMoiThauDienTuToTrinh.ToString(),
                cancellationToken);
        }
        if (entity.QuyetDinh != null)
        {
            var quyetDinhId  = entity.QuyetDinh.Id;
            await SyncTepDinhKemAsync(
                quyetDinhId.ToString(),
                model.QuyetDinh?.GetDanhSachTepDinhKemQuyetDinh(quyetDinhId) ?? [],
                EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString(),
                cancellationToken);
        }
        if (model.HoSoMoiThauThamDinh != null)
        {
            await SyncTepDinhKemAsync(
                entityId.ToString(),
                model.HoSoMoiThauThamDinh.GetDanhSachTepDinhKemQuyetDinhThamDinh(entityId),
                EGroupType.HoSoMoiThauDienTuQuyetDinhTD.ToString(),
                cancellationToken);

            await SyncTepDinhKemAsync(
                entityId.ToString(),
                model.HoSoMoiThauThamDinh.GetDanhSachTepDinhKemCamKetThamDinh(entityId),
                EGroupType.HoSoMoiThauDienTuCamKetTD.ToString(),
                cancellationToken);

            await SyncTepDinhKemAsync(
                entityId.ToString(),
                model.HoSoMoiThauThamDinh.GetDanhSachTepDinhKemBaoCaoThamDinh(entityId),
                EGroupType.HoSoMoiThauDienTuBaoCaoTD.ToString(),
                cancellationToken);
        }
    }

    private Task SyncTepDinhKemAsync(
        string groupId,
        List<TepDinhKem> entities,
        string scopeGroupType,
        CancellationToken cancellationToken)
        => Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = groupId,
            Entities = entities,
            ScopeGroupTypes = [scopeGroupType]
        }, cancellationToken);
}
