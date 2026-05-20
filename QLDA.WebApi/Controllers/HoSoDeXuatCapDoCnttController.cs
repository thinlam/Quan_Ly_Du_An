using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLDA.Application.HoSoDeXuatCapDoCntts.Commands;
using QLDA.Application.HoSoDeXuatCapDoCntts.Queries;
using QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.WebApi.Models;
using QLDA.WebApi.Models.HoSoDeXuatCapDoCntts;

namespace QLDA.WebApi.Controllers;

[Tags("Hồ sơ đề xuất cấp độ CNTT")]
[Route("api/ho-so-de-xuat-cap-do-cntt")]
[Authorize] 
public class HoSoDeXuatCapDoCnttController(IServiceProvider sp) : AggregateRootController(sp) {

    [HttpGet("{id}")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new HoSoDeXuatCapDoCnttGetQuery { Id = id });
        var files = await Mediator.Send(new GetDanhSachTepDinhKemQuery {
            GroupId = [entity.Id.ToString()]
        });
        return ResultApi.Ok(entity.ToModel(files));
    }

    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetAll([FromQuery] HoSoDeXuatCapDoCnttSearchDto dto, string? globalFilter) {
        dto.GlobalFilter = globalFilter;
        var result = await Mediator.Send(new HoSoDeXuatCapDoCnttGetDanhSachQuery(dto));
        return ResultApi.Ok(result);
    }

    [HttpPost("them-moi")]
    public async Task<ResultApi> Create([FromBody] HoSoDeXuatCapDoCnttModel model) {
        var insertDto = model.ToInsertDto();
        var entity = await Mediator.Send(new HoSoDeXuatCapDoCnttInsertCommand(insertDto));
        
        // Lưu file đính kèm
        if (model.DanhSachTepDinhKem?.Count > 0) {
            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
                GroupId = entity.Id.ToString(),
                Entities = model.GetDanhSachTepDinhKem(entity.Id)
            });
        }
        
        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    public async Task<ResultApi> Update([FromBody] HoSoDeXuatCapDoCnttModel model) {
        var updateModel = model.ToUpdateModel();
        var entity = await Mediator.Send(new HoSoDeXuatCapDoCnttUpdateCommand(updateModel));
        
        // Cập nhật file đính kèm
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
        await Mediator.Send(new HoSoDeXuatCapDoCnttDeleteCommand(id));
        return ResultApi.Ok("Xóa hồ sơ thành công");
    }

    [HttpPut("thay-doi-trang-thai")]
    public async Task<ResultApi> ThayDoiTrangThai(
        [FromBody] HoSoDeXuatCapDoCnttThayDoiTrangThaiDto dto) {
        
        await Mediator.Send(new HoSoDeXuatCapDoCnttThayDoiTrangThaiCommand(dto));
        return ResultApi.Ok("Cập nhật trạng thái thành công");
    }
}