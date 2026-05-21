using QLDA.Application.DeXuatNhuCauKinhPhis.Commands;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Application.DeXuatNhuCauKinhPhis.Queries;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.PhanKhaiKinhPhis.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DeXuatNhuCauKinhPhis;
using QLDA.WebApi.Models.PhanKhaiKinhPhis;
using QLDA.WebApi.Models.TepDinhKems;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Tags("Đề xuất chủ trương đầu tư chuyển tiếp")]
[Route("api/de-xuat-chu-truong-chuyen-tiep")]
public class DeXuatChuTruongChuyenTiepController : AggregateRootController {
    // GET
    public DeXuatChuTruongChuyenTiepController(IServiceProvider serviceProvider) : base(serviceProvider) {
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

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Quy trình id là bắt buộc
    /// </remarks>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] DeXuatNhuCauKinhPhiModel model) {
        //Cập nhật bước hiện tại của dự án
        
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));

        var entity = model.ToEntity();
        await Mediator.Send(new DeXuatNhuCauKinhPhiInsertCommand(entity));

        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id,
            GroupTypeConstants.ChuTruongChuyenTiep) ?? []];

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = files
        });
        
        return ResultApi.Ok(entity.Id);
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
                KinhPhiDeXuat = model.KinhPhiDeXuat,
                TrichYeu = model.TrichYeu,
            }
        ), cancellationToken);
        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id,GroupTypeConstants.ChuTruongChuyenTiep) ?? []];

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



    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<DeXuatNhuCauKinhPhiDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] DeXuatNhuCauKinhPhiSearchDto req) {
        var res = await Mediator.Send(new DeXuatNhuCauKinhPhiQuery() {
            DuAnId = req.DuAnId,
            BuocId = req.BuocId,
            GlobalFilter = req.GlobalFilter,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            DonViDeXuatId = req.DonViDeXuatId,
            SoPhieuChuyen = req.SoPhieuChuyen,
            TuNgay = req.TuNgay,
            DenNgay = req.DenNgay,
            IsNoTracking = true,    
            
        });
        return ResultApi.Ok(res);
    }
}