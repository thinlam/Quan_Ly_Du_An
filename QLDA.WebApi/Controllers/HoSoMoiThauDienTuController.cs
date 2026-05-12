using Microsoft.AspNetCore.Mvc;
using QLDA.Application.HoSoMoiThauDienTus.Commands;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.HoSoMoiThauDienTus.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.WebApi.Models.HoSoMoiThauDienTus;

namespace QLDA.WebApi.Controllers;

[Tags("Hồ sơ mời thầu điện tử")]
[Route("api/ho-so-moi-thau-dien-tu")]
public class HoSoMoiThauDienTuController(IServiceProvider sp) : AggregateRootController(sp) {

    [HttpGet("{id}")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new HoSoMoiThauDienTuGetQuery { Id = id });
        var files = await Mediator.Send(new GetDanhSachTepDinhKemQuery {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToModel(files));
    }

    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetAll([FromQuery] HoSoMoiThauDienTuSearchDto dto, string? globalFilter) {
        dto.GlobalFilter = globalFilter;
        var result = await Mediator.Send(new HoSoMoiThauDienTuGetDanhSachQuery(dto));
        return ResultApi.Ok(result);
    }

    [HttpPost("them-moi")]
    public async Task<ResultApi> Create([FromBody] HoSoMoiThauDienTuModel model) {
        var entity = await Mediator.Send(new HoSoMoiThauDienTuInsertCommand(model.ToInsertDto()));

        if (model.DanhSachTepDinhKem?.Count > 0) {
            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
                GroupId = entity.Id.ToString(),
                Entities = model.GetDanhSachTepDinhKem(entity.Id)
            });
        }

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    public async Task<ResultApi> Update([FromBody] HoSoMoiThauDienTuModel model) {
        var entity = await Mediator.Send(new HoSoMoiThauDienTuUpdateCommand(model.ToUpdateModel()));

        if (model.DanhSachTepDinhKem?.Count > 0) {
            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
                GroupId = entity.Id.ToString(),
                Entities = model.GetDanhSachTepDinhKem(entity.Id)
            });
        }

        return ResultApi.Ok(entity.Id);
    }

    [HttpDelete("{id}")]
    public async Task<ResultApi> Delete(Guid id) {
        await Mediator.Send(new HoSoMoiThauDienTuDeleteCommand(id));
        return ResultApi.Ok(1);
    }
}