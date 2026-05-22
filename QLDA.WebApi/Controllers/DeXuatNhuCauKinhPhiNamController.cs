using Azure.Core;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.Commands;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.Queries;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.PhanKhaiKinhPhis.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DeXuatNhuCauKinhPhiNams;
using QLDA.WebApi.Models.PhanKhaiKinhPhis;
using QLDA.WebApi.Models.TepDinhKems;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Tags("Đề xuất kế hoạch kinh phí năm")]
[Route("api/de-xuat-nhu-cau-kinh-phi-nam")]
public class DeXuatNhuCauKinhPhiNamController : AggregateRootController {
    // GET
    public DeXuatNhuCauKinhPhiNamController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<DeXuatNhuCauKinhPhiNamModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new DeXuatNhuCauKinhPhiNamGetQuery() {
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
        var res = await Mediator.Send(new DeXuatNhuCauKinhPhiNamDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] DeXuatNhuCauKinhPhiNamModel model) {
        //Cập nhật bước hiện tại của dự án
        
      
        var entity = model.ToEntity();
        var savedEntity = await Mediator.Send(new DeXuatNhuCauKinhPhiNamInsertCommand(entity));

        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(savedEntity.Id, GroupTypeConstants.NhuCauKinhPhi) ?? []];

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = savedEntity.Id.ToString(),
            Entities = files
        });

        return ResultApi.Ok(savedEntity.Id);
    }

   
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<DeXuatNhuCauKinhPhiNam>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update(
        [FromBody] DeXuatNhuCauKinhPhiNamModel model,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new DeXuatNhuCauKinhPhiNamUpdateCommand(
            new()
            {
                Id = model.GetId(),
                So = model.So,
                NgayKeHoach = model.NgayKeHoach,
                TrichYeu = model.TrichYeu,
                GhiChu = model.GhiChu,
                TongKinhPhiDeXuat = model.TongKinhPhiDeXuat
            }
        ), cancellationToken);

        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id,GroupTypeConstants.NhuCauKinhPhi) ?? []];
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
    [ProducesResponseType<ResultApi<PaginatedList<DeXuatNhuCauKinhPhiNamDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] DeXuatNhuCauKinhPhiNamSearchDto req) {
        var res = await Mediator.Send(new DeXuatNhuCauKinhPhiNamQuery() {
            So = req.So,
            DenNgay = req.DenNgay,
            TuNgay = req.TuNgay,
            NguoiDeXuatId = req.NguoiDeXuatId,
            PhongBanDeXuatId = req.PhongBanDeXuatId,
            GlobalFilter = req.GlobalFilter,
            PageIndex = req.PageIndex??1,
            PageSize = req.PageSize??1000000,
            IsNoTracking = true,
            
        });
        return ResultApi.Ok(res);
    }
}