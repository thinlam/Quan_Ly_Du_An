using Azure.Core;
using QLDA.Application.DeXuatChuyenTieps.Commands;
using QLDA.Application.DeXuatChuyenTieps.DTOs;
using QLDA.Application.DeXuatChuyenTieps.Queries;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.PhanKhaiKinhPhis.Commands;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DeXuatChuTruongMois;
using QLDA.WebApi.Models.PhanKhaiKinhPhis;
using QLDA.WebApi.Models.DeXuatChuTruongChuyenTieps;
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
    [ProducesResponseType<ResultApi<DeXuatChuyenTiepModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new DeXuatChuyenTiepGetQuery() {
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
        var res = await Mediator.Send(new DeXuatChuyenTiepDeleteCommand(id));
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
    public async Task<ResultApi> Create([FromBody] DeXuatChuyenTiepModel model) {
        //Cập nhật bước hiện tại của dự án
        
        var step = await Mediator.Send(new DuAnUpdateStepCommand(model.DuAnId, model.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(model.DuAnId, step));

        var entity = model.ToEntity();
        var savedEntity =        await Mediator.Send(new DeXuatChuyenTiepInsertCommand(entity));
        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(savedEntity.Id,
            EGroupType.DeXuatChuyenTiep) ?? []];

        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = savedEntity.Id.ToString(),
            Entities = files
        });
        
        return ResultApi.Ok(savedEntity.Id);
    }

   
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<DeXuatChuyenTiep>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update(
        [FromBody] DeXuatChuyenTiepModel model,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new DeXuatChuyenTiepUpdateCommand(
            new()
            {
                Id = model.Id,
                DuAnId = model.DuAnId,
                BuocId = model.BuocId,
                SoLieuGiaiNgan = model.SoLieuGiaiNgan,
                NhuCauKinhPhi = model.NhuCauKinhPhi,
                KhoiLuongDuKien = model.KhoiLuongDuKien,
                KhoiLuongThucTe = model.KhoiLuongThucTe,
                NamDeXuat = model.NamDeXuat,
                UocGiaiNgan = model.UocGiaiNgan
            }
        ), cancellationToken);
        List<TepDinhKem> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id,EGroupType.DeXuatChuyenTiep) ?? []];

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
    [ProducesResponseType<ResultApi<PaginatedList<DeXuatChuyenTiepDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] CommonSearchDto req, [FromQuery] int? loaiDuAnTheoNamId = null) {
        var res = await Mediator.Send(new DeXuatChuyenTiepGetDanhSachQuery() {
            DuAnId = req.DuAnId,
            BuocId = req.BuocId,
            GlobalFilter = req.GlobalFilter,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            IsNoTracking = true,
            LoaiDuAnTheoNamId = loaiDuAnTheoNamId,

        });
        return ResultApi.Ok(res);
    }
}