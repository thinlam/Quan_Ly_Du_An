using Microsoft.AspNetCore.Mvc;
using QLDA.Application.HoSoMoiThauDienTus.Commands;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.HoSoMoiThauDienTus.Queries;
using QLDA.Application.KeHoachLuaChonNhaThaus.DTOs;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.HoSoMoiThauDienTus;
using QLDA.WebApi.Models.TepDinhKems;
using System.Data;
using System.Net.Mime;
namespace QLDA.WebApi.Controllers;

[Tags("Hồ sơ mời thầu điện tử")]
[Route("api/ho-so-moi-thau-dien-tu")]
public class HoSoMoiThauDienTuController(IServiceProvider sp) : AggregateRootController(sp)
{

    [HttpGet("{id}")]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new HoSoMoiThauDienTuGetQuery { Id = id });
        var files = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTu.ToString()]
        });
        var filesToTrinh = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.ToTrinhQuyetDinh.Id.ToString()],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTuToTrinh.ToString()]
        });
        var fileCamKets = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTuCamKetTD.ToString()]
        });
        var fileThamDinhs = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTuQuyetDinhTD.ToString()]

        });
        var fileBaoCaos = await Mediator.Send(new GetDanhSachTepDinhKemQuery
        {
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [EGroupType.HoSoMoiThauDienTuBaoCaoTD.ToString()]
        });
        return ResultApi.Ok(entity.ToModel(files, fileCamKets, fileThamDinhs, fileBaoCaos, filesToTrinh));
    }

    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetAll([FromQuery] HoSoMoiThauDienTuSearchDto dto, string? globalFilter)
    {
        dto.GlobalFilter = globalFilter;
        var result = await Mediator.Send(new HoSoMoiThauDienTuGetDanhSachQuery(dto));
        return ResultApi.Ok(result);
    }

    [HttpPost("them-moi")]
    public async Task<ResultApi> Create([FromBody] HoSoMoiThauDienTuModel model,
        [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var entity = await Mediator.Send(new HoSoMoiThauDienTuInsertCommand(model.ToInsertDto()));
        await SaveDanhSachTepDinhKemAsync(model, entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        return ResultApi.Ok(entity.Id);
        //if(model.ToTrinhQuyetDinh)
        //if (model.ToTrinhQuyetDinh != null && model.ToTrinhQuyetDinh.DanhSachTepDinhKem?.Count > 0)
        //{
        //    await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        //    {
        //        GroupId = entity.Id.ToString(),
        //        Entities = model.HoSoMoiThauThamDinh.GetDanhSachTepDinhKemQuyetDinhThamDinh(entity.Id)
        //    });
        //}
        //await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        //{
        //    GroupId = entity.Id.ToString(),
        //    Entities = model.HoSoMoiThauThamDinh.GetDanhSachTepDinhKemCamKetThamDinh(entity.Id)
        //});
        //await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        //{
        //    GroupId = entity.Id.ToString(),
        //    Entities = model.HoSoMoiThauThamDinh.GetDanhSachTepDinhKemBaoCaoThamDinh(entity.Id)
        //});
        //await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        //{
        //    GroupId = entity.ToTrinhQuyetDinh.Id.ToString(),
        //    Entities = model.ToTrinhQuyetDinh.GetDanhSachTepDinhKemToTrinh(entity.ToTrinhQuyetDinh.Id)
        //});
        //await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        //{
        //    GroupId = entity.Id.ToString(),
        //    Entities = model.GetDanhSachTepDinhKem(entity.Id)
        //});

    }

    [HttpPut("cap-nhat")]
    public async Task<ResultApi> Update([FromBody] HoSoMoiThauDienTuModel model, [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default
        )
    {
        using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var entity = await Mediator.Send(new HoSoMoiThauDienTuUpdateCommand(model.ToUpdateModel()));
        // Gọi hàm dùng chung
        await SaveDanhSachTepDinhKemAsync(model, entity, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        return ResultApi.Ok(entity.Id); //await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        //{
        //    GroupId = entity.Id.ToString(),
        //    Entities = model.GetDanhSachTepDinhKem(entity.Id)
        //});

        //if (entity.ToTrinhQuyetDinh != null)
        //    await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        //    {
        //        GroupId = entity.Id.ToString(),
        //        Entities = model.ToTrinhQuyetDinh.GetDanhSachTepDinhKemToTrinh(entity.ToTrinhQuyetDinh.Id)
        //    });

    }

    [HttpDelete("{id}")]
    public async Task<ResultApi> Delete(Guid id)
    {
        await Mediator.Send(new HoSoMoiThauDienTuDeleteCommand(id));
        return ResultApi.Ok(1);
    }
    private async Task SaveDanhSachTepDinhKemAsync(HoSoMoiThauDienTuModel model, HoSoMoiThauDienTu entity, CancellationToken cancellationToken)
    {
        var toTrinhQuyetDinh = entity.ToTrinhQuyetDinh;

        // 1. Tệp đính kèm chính của hồ sơ
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            Entities = model.GetDanhSachTepDinhKem(entity.Id)
        }, cancellationToken);

        // 2. Tệp đính kèm từ Tờ trình Quyết định (nếu có)
        if (toTrinhQuyetDinh != null)
        {
            var toTrinhId = toTrinhQuyetDinh.Id;
            if (model.ToTrinhQuyetDinh?.DanhSachTepDinhKem?.Count > 0)
            {
                await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
                {
                    GroupId = toTrinhId.ToString(),
                    Entities = model.ToTrinhQuyetDinh.GetDanhSachTepDinhKemToTrinh(toTrinhId)
                }, cancellationToken);
            }
        }

        // 3. Các tệp đính kèm từ Hồ sơ Thẩm định (Chỉ chạy khi có dữ liệu thẩm định)
        if (model.HoSoMoiThauThamDinh != null)
        {
            // Quyết định thẩm định (Kiểm tra điều kiện giống hàm Create cũ của bạn)
            if (model.ToTrinhQuyetDinh?.DanhSachTepDinhKem?.Count > 0)
            {
                await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
                {
                    GroupId = entity.Id.ToString(),
                    Entities = model.HoSoMoiThauThamDinh.GetDanhSachTepDinhKemQuyetDinhThamDinh(entity.Id)
                }, cancellationToken);
            }

            // Cam kết thẩm định
            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                Entities = model.HoSoMoiThauThamDinh.GetDanhSachTepDinhKemCamKetThamDinh(entity.Id)
            }, cancellationToken);

            // Báo cáo thẩm định
            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                Entities = model.HoSoMoiThauThamDinh.GetDanhSachTepDinhKemBaoCaoThamDinh(entity.Id)
            }, cancellationToken);
        }
    }
}