using QLDA.Application.HoSoDeXuatCapDoCntts.Commands;
using QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;
using QLDA.Application.HoSoDeXuatCapDoCntts.Queries;
using BuildingBlocks.Application.Attachments.Commands;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.WebApi.Models.HoSoDeXuatCapDoCntts;
using QLDA.WebApi.Models.QuanLyPheDuyet;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Tags("Hồ sơ đề xuất cấp độ CNTT")]
[Route("api/ho-so-de-xuat-cap-do-cntt")]
[Authorize]
public class HoSoDeXuatCapDoCnttController(IServiceProvider sp) : AggregateRootController(sp) {

    [HttpGet("{id}")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new HoSoDeXuatCapDoCnttGetQuery { Id = id });
        var files = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()]
        ))).ToAttachmentEntities();
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
            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
                GroupId = entity.Id.ToString(),
                GroupTypes = [nameof(EGroupType.HoSoDeXuatCapDoCntt)],
                Entities = model.GetDanhSachTepDinhKem(entity.Id)
,
                AutoDeleteMissing = true
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
            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
                GroupId = entity.Id.ToString(),
                GroupTypes = [nameof(EGroupType.HoSoDeXuatCapDoCntt)],
                Entities = model.GetDanhSachTepDinhKem(entity.Id)
,
                AutoDeleteMissing = true
            });
        }

        return ResultApi.Ok(entity.Id);
    }

    [HttpDelete("{id}")]
    public async Task<ResultApi> Delete(Guid id) {
        await Mediator.Send(new HoSoDeXuatCapDoCnttDeleteCommand(id));
        return ResultApi.Ok("Xóa hồ sơ thành công");
    }

    // t thêm này
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("{type}/{id}/xu-ly")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> XuLy(string type, Guid id, [FromBody] XuLyChungModel model)
    {
        //var res = await Mediator.Send(new ToTrinhCoThamDinhThaoTacCommand(id, type, model.MaTrangThaiTiepTheo, model.NoiDung));
        var res = await Mediator.Send(new HoSoDeXuatCapDoCnttPheDuyetCommand(id, type, model.MaTrangThaiTiepTheo, model.NoiDung));
        return ResultApi.Ok(res);
    }
}