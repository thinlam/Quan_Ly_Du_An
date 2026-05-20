using QLDA.Application.DeXuatChuTruongMois.Commands;
using QLDA.Application.DeXuatChuTruongMois.DTOs;
using QLDA.Application.DeXuatChuTruongMois.Queries;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.PhanKhaiKinhPhis.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DeXuatChuTruongMois;
using QLDA.WebApi.Models.PhanKhaiKinhPhis;
using QLDA.WebApi.Models.TepDinhKems;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Tags("Đề xuất chủ trương đầu tư mới")]
[Route("api/de-xuat-chu-truong-moi")]
public class DeXuatChuTruongMoiController : AggregateRootController {
    // GET
    public DeXuatChuTruongMoiController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<DeXuatChuTruongMoiModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new DeXuatChuTruongMoiGetQuery() {
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
        var res = await Mediator.Send(new DeXuatChuTruongMoiDeleteCommand(id));
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
    public async Task<ResultApi> Create([FromBody] DeXuatChuTruongMoiModel model) {
        //Cập nhật bước hiện tại của dự án
        
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));

        var entity = model.ToEntity();
        await Mediator.Send(new DeXuatChuTruongMoiInsertCommand(entity));

        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = danhSachTepDinhKem
        });

        return ResultApi.Ok(entity.Id);
    }

   
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<DeXuatChuTruongMoi>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update(
        [FromBody] DeXuatChuTruongMoiModel model,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new DeXuatChuTruongMoiUpdateCommand(
            new()
            {
                Id = model.GetId(),
                DuAnId = model.DuAnId,
                BuocId = model.BuocId,  
                DonViPhuTrachChinhId = model.BuocId,
                LanhDaoPhuTrachId = model.BuocId,
                HinhThucDauTuId = model.HinhThucDauTuId,
                TomTatNoiDung = model.TomTatNoiDung,
                NgayBatDauDuKien = model.NgayBatDauDuKien,
                TongMucDauTu= model.TongMucDauTu,
            }
        ), cancellationToken);

        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, GroupTypeConstants.PhanKhaiKinhPhi) ?? []];
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
    [ProducesResponseType<ResultApi<PaginatedList<DeXuatChuTruongMoiDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] DeXuatChuTruongMoiSearchDto req) {
        var res = await Mediator.Send(new DeXuatChuTruongMoiQuery() {
            DuAnId = req.DuAnId,
            BuocId = req.BuocId,
            GlobalFilter = req.GlobalFilter,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            LanhDaoPhuTrachId = req.LanhDaoPhuTrachId,
            DonViPhuTrachId = req.DonViPhuTrachId,
            HinhThucDauTuId = req.HinhThucDauTuId,
            IsNoTracking = true,
            
        });
        return ResultApi.Ok(res);
    }
}