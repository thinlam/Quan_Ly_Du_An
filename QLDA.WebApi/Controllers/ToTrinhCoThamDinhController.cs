using Azure.Core;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.QuanLyPheDuyet.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Application.ToTrinhCoThamDinhs;
using QLDA.Application.ToTrinhCoThamDinhs.Commands;
using QLDA.Application.ToTrinhCoThamDinhs.DTOs;
using QLDA.Application.ToTrinhCoThamDinhs.Queries;
using QLDA.Application.ToTrinhPheDuyets.DTOs;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.QuanLyPheDuyet;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Route("api/to-trinh-co-tham-dinh")]
[Tags("Tờ trình có thẩm định")]
public class ToTrinhCoThamDinhController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<ToTrinhCoThamDinhDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new ToTrinhCoThamDinhGetQuery()
        {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true
        });

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes= [GroupTypeConstants.QuyetDinhKeHoachThue]
        });
        var danhSachTepThamDinh = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes= [GroupTypeConstants.QuyetDinhKeHoachThueThamDinh]
        });
        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem.ToList(), danhSachTepThamDinh.ToList()));
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id)
    {
        var res = await Mediator.Send(new ToTrinhCoThamDinhDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create(
        [FromBody] ToTrinhCoThamDinhInsUpdDto dto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step));
      
        var entity = await Mediator.Send(new ToTrinhCoThamDinhInsertCommand(dto), cancellationToken);
        // nếu dùng ToTrinhCoThamDinh cho nhìu màn hình thì lấy  GroupTypeConstants.ToTrinhCoThamDinh theo Loai
        //tạo contanst LoaiToTrinhCoThamDinh

        List<TepDinhKem> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.QuyetDinhKeHoachThue) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    [ProducesResponseType<ResultApi<ToTrinhCoThamDinhDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(
        [FromBody] ToTrinhCoThamDinhInsUpdDto dto,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new ToTrinhCoThamDinhUpdateCommand(dto), cancellationToken);

        List<TepDinhKem> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.QuyetDinhKeHoachThue) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
           
            Entities = files
        }, cancellationToken);
        List<TepDinhKem> filesThamDinh = [.. dto.DanhSachTepThamDinh?.ToEntities(entity.Id, GroupTypeConstants.QuyetDinhKeHoachThueThamDinh) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = filesThamDinh
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        //
        var danhSachTep = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [GroupTypeConstants.QuyetDinhKeHoachThue]
        }, cancellationToken);
        var danhSachTepThamDinh = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [GroupTypeConstants.QuyetDinhKeHoachThueThamDinh]
        }, cancellationToken);

        return ResultApi.Ok(entity.ToDto(danhSachTep.ToList(), danhSachTepThamDinh.ToList()));
    }
    // t thêm này
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("{type}/{id}/xu-ly")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> XuLy(string type, Guid id, [FromBody] XuLyChungModel model)
    {
        var res = await Mediator.Send(new ToTrinhCoThamDinhThaoTacCommand(id, type, model.MaTrangThaiTiepTheo, model.NoiDung ));
        return ResultApi.Ok(res);
    }
    //end  t thêm này
    [ProducesResponseType<ResultApi<PaginatedList<ToTrinhCoThamDinhDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] ToTrinhPheDuyetSearchDto dto)
    {
        // hien tai có 6 loai trong  ToTrinhEntityNames 
        var res = await Mediator.Send(new ToTrinhCoThamDinhGetPaginatedQuery()
        {
            IsNoTracking = true,
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            Loai = dto.Loai,
            PageSize = dto.PageSize,
            PageIndex = dto.PageIndex,
            GlobalFilter = dto.GlobalFilter,
            So = dto.SoQuyetDinh,
            TuNgay = dto.TuNgay,
            DenNgay = dto.DenNgay,
            LoaiDuAnTheoNamId = dto.LoaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}
