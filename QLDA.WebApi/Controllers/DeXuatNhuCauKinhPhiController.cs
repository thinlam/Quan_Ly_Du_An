using QLDA.Application.DeXuatNhuCauKinhPhis.Commands;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Application.DeXuatNhuCauKinhPhis.Queries;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.WebApi.Models.DeXuatNhuCauKinhPhis;
using QLDA.WebApi.Models.PhanKhaiKinhPhis;
using QLDA.WebApi.Models.TepDinhKems;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Tags("Đề xuất nhu cầu kinh phí")]
[Route("api/de-xuat-nhu-cau-kinh-phi")]
public class DeXuatNhuCauKinhPhiController : AggregateRootController {
    // GET
    public DeXuatNhuCauKinhPhiController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<DeXuatNhuCauKinhPhiModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new DeXuatNhuCauKinhPhiGetQuery() {
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
        var res = await Mediator.Send(new DeXuatNhuCauKinhPhiDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] DeXuatNhuCauKinhPhiModel model) {
        //Cập nhật bước hiện tại của dự án
        
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));

        var entity = model.ToEntity();
        var savedEntity = await Mediator.Send(new DeXuatNhuCauKinhPhiInsertCommand(entity));

        List<Attachment> files = [.. model.DanhSachTepDinhKem?.ToEntities(savedEntity.Id, EGroupType.DeXuatNhuCauKinhPhi) ?? []];

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = savedEntity.Id.ToString(),
            Entities = files
        });

        return ResultApi.Ok(savedEntity.Id);
    }

   
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<DeXuatNhuCauKinhPhi>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update(
        [FromBody] DeXuatNhuCauKinhPhiModel model,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new DeXuatNhuCauKinhPhiUpdateCommand(
            new()
            {
                Id = model.GetId(),
                DuAnId = model.DuAnId,
                BuocId = model.BuocId,  
                DonViDeXuatId = model.DonViDeXuatId,
                SoPhieuChuyen = model.SoPhieuChuyen,
                NgayPhieuChuyen = model.NgayPhieuChuyen,
                TrichYeu = model.TrichYeu,
                KinhPhiDeXuat = model.KinhPhiDeXuat
            }
        ), cancellationToken);

        List<Attachment> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id,EGroupType.DeXuatNhuCauKinhPhi) ?? []];
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = files
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var danhSachTepDinhKem = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()]
        }, cancellationToken);
        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem));
    }


    /*
     Combo Đề xuất của Kế hoạch năm hiển thị các đề xuất:
    Trạng thái đề xuất = Đã duyệt.
    Không thuộc Kế hoạch năm có trạng thái:
    Chờ duyệt
    Trả lại
    Đã duyệt.
    Nếu thuộc Kế hoạch năm có trạng thái Từ chối thì vẫn được phép hiển thị để chọn lại.
    */
    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<DeXuatNhuCauKinhPhiDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] DeXuatNhuCauKinhPhiSearchDto req) {
        var res = await Mediator.Send(new DeXuatNhuCauKinhPhiQuery() {
            DuAnId = req.DuAnId,
            BuocId = req.BuocId,
            TrangThaiId = req.TrangThaiId,
            DaDuyetTongHop = req.DaDuyetTongHop,
            SoPhieuChuyen = req.SoPhieuChuyen,
            GlobalFilter = req.GlobalFilter,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            IsNoTracking = true,
            LoaiDuAnTheoNamId = req.LoaiDuAnTheoNamId,

        });
        return ResultApi.Ok(res);
    }
    [HttpGet("danh-sach-combobox")]
    [ProducesResponseType<ResultApi<PaginatedList<DeXuatNhuCauKinhPhiDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetCbo(Guid? keHoachId)
    {
        var res = await Mediator.Send(new DeXuatNhuCauKinhPhiComboboxQuery()
        {
            IsNoTracking = true,
            KeHoachId = keHoachId

        });
        return ResultApi.Ok(res);
    }
    [HttpGet("theo-doi-tinh-hinh")]
    [ProducesResponseType<ResultApi<PaginatedList<DeXuatNhuCauKinhPhiDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetTinhHinh([FromQuery] DeXuatNhuCauKinhPhiSearchDto req)
    {
        var res = await Mediator.Send(new TheoDoiDeXuatNhuCauKinhPhiQuery()
        {
            DuAnId = req.DuAnId,
            BuocId = req.BuocId,
            TrangThaiId = req.TrangThaiId,
            TrangThaiKeHoachId = req.TrangThaiKeHoachNamId,
            SoPhieuChuyen = req.SoPhieuChuyen,
            TrichYeu = req.TrichYeu,
            GlobalFilter = req.GlobalFilter,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            IsNoTracking = true,
            TuNgay = req.TuNgay,
            DenNgay = req.DenNgay,

        });
        return ResultApi.Ok(res);
    }
    
    
}
