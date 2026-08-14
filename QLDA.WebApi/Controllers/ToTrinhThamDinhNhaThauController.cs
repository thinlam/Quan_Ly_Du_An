using QLDA.Application.DuAns.Commands;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Application.Attachments.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.Application.ToTrinhThamDinhNhaThaus;
using QLDA.Application.ToTrinhThamDinhNhaThaus.Commands;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Application.ToTrinhThamDinhNhaThaus.Queries;
using QLDA.WebApi.Models.KetQuaThamDinhNhaThaus;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.ToTrinhThamDinhNhaThaus;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Route("api/to-trinh-tham-dinh-nha-thau")]
[Tags("Tờ trình thẩm định nhà thầu")]
public class ToTrinhThamDinhNhaThauController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<ToTrinhThamDinhNhaThauDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new ToTrinhThamDinhNhaThauGetQuery()
        {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true
        });

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.ToTrinhThamDinhNhaThau)]
        ))).ToAttachmentEntities();
        var danhSachTepThamDinh = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.NoiDungToTrinhThamDinhNhaThau)]
        ))).ToAttachmentEntities();
        var nhaThauModel = entity.NhaThaus!.Select(o => o.ToModel()).ToList();
        /*foreach (var item in nhaThauModel)
        {
            var dsTep = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
            {
                GroupId = [item.Id.ToString()],
                EGroupTypes = [EGroupType.KetQuaThamDinhNhaThau]
            });
            item.DanhSachTepDinhKem = dsTep.Select(o => o.ToModel()).ToList(); // i need ways
        }*/
        var ids = nhaThauModel.Select(x => x.Id.ToString() ?? "").ToList();

            var allFiles = (await Mediator.Send(new GetAttachmentsQuery(
                GroupIds: ids,
                BaseGroupTypes: [nameof(EGroupType.KetQuaThamDinhNhaThau)]
            ))).ToAttachmentEntities();
        var lookup = allFiles.GroupBy(x => x.GroupId)
                    .ToDictionary(g => g.Key, g => g.ToList());
        foreach (var item in nhaThauModel)
        {
            if (lookup.TryGetValue(item.Id.ToString() ?? "", out var files))
            {
                item.DanhSachTepDinhKem = files
                    .Select(x => x.ToModel())
                    .ToList();
            }
        }

        return ResultApi.Ok(entity.ToModel(nhaThauModel: nhaThauModel, // Hoặc kết quả xử lý danh sách nhà thầu của bạn
    danhSachTepDinhKem: danhSachTepDinhKem.ToList(),
    danhSachTepThamDinh: danhSachTepThamDinh.ToList()
 //   danhSachKetQuaThamDinhNhaThau: item.ToList()
    // nhaThauModel, danhSachTepDinhKem.ToList(), danhSachTepThamDinh.ToList()
    ));
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id)
    {
        var res = await Mediator.Send(new ToTrinhThamDinhNhaThauDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] ToTrinhThamDinhNhaThauModel dto, [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step), cancellationToken);

        var entity = await Mediator.Send(new ToTrinhThamDinhNhaThauInsertCommand(dto.ToEntity()), cancellationToken);
        var danhSachTepDinhKem = dto.GetDanhSachTepDinhKem(entity.Id).ToList();

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau)],
            Entities = danhSachTepDinhKem,
            AutoDeleteMissing = true
        });
        var danhSachFileThamDinh = dto.GetDanhSachTepThamDinh(entity.Id).ToList();

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.NoiDungToTrinhThamDinhNhaThau)],
            Entities = danhSachFileThamDinh,
            AutoDeleteMissing = true
        });
        var danhSachFileKetQua = new List<Attachment>();
        foreach (var nhaThaus in dto.DanhSachNhaThaus!)
        {
            var id = nhaThaus.GetId();
            danhSachFileKetQua = nhaThaus.GetDanhSachTep(id).ToList();

            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = id.ToString(),
                GroupTypes = [nameof(EGroupType.KetQuaThamDinhNhaThau)],
                Entities = danhSachFileKetQua,
                AutoDeleteMissing = true
            });
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    [ProducesResponseType<ResultApi<ToTrinhThamDinhNhaThauDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] ToTrinhThamDinhNhaThauModel model, [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new ToTrinhThamDinhNhaThauUpdateCommand(model.ToEntity()), cancellationToken);

        List<Attachment> files = [.. model.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.ToTrinhThamDinhNhaThau) ?? []];
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau)],
            Entities = files,
            AutoDeleteMissing = true
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var danhSachTepDinhKem = model.GetDanhSachTepDinhKem(entity.Id);

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau)],
            Entities = danhSachTepDinhKem,
            AutoDeleteMissing = true
        });
        var danhSachFileThamDinh = model.GetDanhSachTepThamDinh(entity.Id);

        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.NoiDungToTrinhThamDinhNhaThau)],
            Entities = danhSachFileThamDinh,
            AutoDeleteMissing = true
        });
        var danhSachFileKetQua = new List<Attachment>();
        foreach (var nhaThaus in model.DanhSachNhaThaus!)
        {
            var id = nhaThaus.GetId();
            danhSachFileKetQua = nhaThaus.GetDanhSachTep(id).ToList();

            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = id.ToString(),
                GroupTypes = [nameof(EGroupType.KetQuaThamDinhNhaThau)],
                Entities = danhSachFileKetQua,
                AutoDeleteMissing = true
            });
        }

        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem.ToList(), danhSachFileThamDinh.ToList()));
    }

    [ProducesResponseType<ResultApi<PaginatedList<ToTrinhThamDinhNhaThauDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] ToTrinhThamDinhNhaThauSearchDto dto)
    {
        var res = await Mediator.Send(new ToTrinhThamDinhNhaThauDanhSachQuery()
        {
            IsNoTracking = true,
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            PageSize = dto.PageSize ?? 1,
            PageIndex = dto.PageIndex ?? 10,
            GlobalFilter = dto.GlobalFilter,
            So = dto.So,
            TuNgay = dto.TuNgay,
            DenNgay = dto.DenNgay,
            LoaiDuAnTheoNamId = dto.LoaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}
