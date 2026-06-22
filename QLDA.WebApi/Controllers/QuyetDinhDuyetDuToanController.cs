using BuildingBlocks.Domain.Entities.Abstractions;
using QLDA.Application.DanhMucDonVis.Queries;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.QuyetDinhDuyetDuToanDtos.DTOs;
using QLDA.Application.QuyetDinhDuyetDuToans;
using QLDA.Application.QuyetDinhDuyetDuToans.Commands;
using QLDA.Application.QuyetDinhDuyetDuToans.DTOs;
using QLDA.Application.QuyetDinhDuyetDuToans.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.QuyetDinhDuyetDuToans;
using QLDA.WebApi.Models.TepDinhKems;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Tags("Quyết định duyệt dự toán")]
[Route("api/quyet-dinh-duyet-du-toan")]
public class QuyetDinhDuyetDuToanController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    [ProducesResponseType<ResultApi<QuyetDinhDuyetDuToanModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new QuyetDinhDuyetDuToanGetQuery() {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery() {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [GroupTypeConstants.QuyetDinhDuyetDuToan]
        });
      
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id) {
        var res = await Mediator.Send(new QuyetDinhDuyetDuToanDeleteCommand(id));
        return ResultApi.Ok(res);
    }
    
  
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] QuyetDinhDuyetDuToanInsUpdDto dto) {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step));
      
        var entity = dto.ToEntity();
        entity = await Mediator.Send( new QuyetDinhDuyetDuToanInsertCommand(dto.ToEntity())   );
       
        List<TepDinhKem> files = [.. dto.DanhSachTepDinhKem?.ToEntities(
            entity.Id, GroupTypeConstants.QuyetDinhDuyetDuToan) ?? []];

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        });

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<QuyetDinhDuyetDuToanModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update([FromBody] QuyetDinhDuyetDuToanInsUpdDto model)
    {
        var entity = await Mediator.Send(new QuyetDinhDuyetDuToanUpdateCommand(model));

        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id,  GroupTypeConstants.QuyetDinhDuyetDuToan) ?? []];

        // 3. Lưu danh sách file
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        });
        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [GroupTypeConstants.QuyetDinhDuyetDuToan]
        });

        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }

    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<QuyetDinhDuyetDuToanDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetDanhSach([FromQuery] Guid? duAnId, int? buocId, string? globalFilter = null,
        int pageIndex = 0, int pageSize = 0, int? loaiDuAnTheoNamId = null) {
        var res = await Mediator.Send(new QuyetDinhDuyetDuToanGetDanhSachQuery() {
            DuAnId = duAnId,
            BuocId = buocId,
            GlobalFilter = globalFilter,
            PageIndex = pageIndex,
            PageSize = pageSize,
            IsNoTracking = true,
            LoaiDuAnTheoNamId = loaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}
