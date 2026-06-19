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
public class HoSoMoiThauDienTuController(IServiceProvider sp) : AggregateRootController(sp) {

    [HttpGet("{id}")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new HoSoMoiThauDienTuGetQuery { Id = id });
        var files = await Mediator.Send(new GetDanhSachTepDinhKemQuery {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTu.ToString()]
        });
        var filesChiDinhThau = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTuChiDinh.ToString()]
        });
        return ResultApi.Ok(entity.ToModel(files, filesChiDinhThau));
    }

    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetAll([FromQuery] HoSoMoiThauDienTuSearchDto dto, string? globalFilter) {
        dto.GlobalFilter = globalFilter;
        var result = await Mediator.Send(new HoSoMoiThauDienTuGetDanhSachQuery(dto));
        return ResultApi.Ok(result);
    }

    [HttpPost("them-moi")]
    public async Task<ResultApi> Create([FromBody] HoSoMoiThauDienTuModel model,
        [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default){
       using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
      
        var entity = await Mediator.Send(new HoSoMoiThauDienTuInsertCommand(model.ToInsertDto()));

        if (model.ChiDinhThau != null && model.ChiDinhThau.DanhSachTepDinhKem?.Count > 0) {
            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
                GroupId = entity.ChiDinhThau.Id.ToString(),
                Entities = model.ChiDinhThau.GetDanhSachTepDinhKemChiDinh(entity.ChiDinhThau.Id)
            });
        }
        if (model.DanhSachTepDinhKem?.Count > 0)
        {
            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                Entities = model.GetDanhSachTepDinhKem(entity.Id)
            });
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    public async Task<ResultApi> Update([FromBody] HoSoMoiThauDienTuModel model, [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default
        ){
       using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var entity = await Mediator.Send(new HoSoMoiThauDienTuUpdateCommand(model.ToUpdateModel()));

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = model.GetDanhSachTepDinhKem(entity.Id)
        });
        if (entity.ChiDinhThau != null)
            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                Entities = model.ChiDinhThau.GetDanhSachTepDinhKemChiDinh(entity.ChiDinhThau.Id)
            });
        //List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.HoSoMoiThauDienTu) ?? []];
        //await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        //{
        //    GroupId = entity.Id.ToString(),
        //    Entities = files
        //});

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        return ResultApi.Ok(entity.Id);
    }

    [HttpDelete("{id}")]
    public async Task<ResultApi> Delete(Guid id) {
        await Mediator.Send(new HoSoMoiThauDienTuDeleteCommand(id));
        return ResultApi.Ok(1);
    }
}