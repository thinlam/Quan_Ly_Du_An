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
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.ToTrinhThamDinhNhaThaus;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Route("api/to-trinh-tham-dinh-nha-thau")]
[Tags("Tờ trình thẩm định nhà thầu")]
public class ToTrinhThamDinhNhaThauController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<ToTrinhThamDinhNhaThauChiTietDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id)
    {
        var loaded = await Mediator.Send(new ToTrinhThamDinhNhaThauGetChiTietQuery(id));
        var entity = loaded.Entity;

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.ToTrinhThamDinhNhaThau)]
        ))).ToAttachmentEntities();
        var danhSachTepThamDinh = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.NoiDungToTrinhThamDinhNhaThau)]
        ))).ToAttachmentEntities();

        // File của 3 bước xử lý (Đối chiếu/Thương thảo/Thẩm định) — Issue #179.
        var filesDoiChieu = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.ToTrinhThamDinhNhaThau_DoiChieu)]
        ))).ToAttachmentEntities().Select(x => x.ToDto()).ToList();
        var filesThuongThao = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.ToTrinhThamDinhNhaThau_ThuongThao)]
        ))).ToAttachmentEntities().Select(x => x.ToDto()).ToList();
        var filesThamDinhBuoc = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.ToTrinhThamDinhNhaThau_ThamDinh)]
        ))).ToAttachmentEntities().Select(x => x.ToDto()).ToList();

        // File của Thông tin nhà thầu (E-HSDT / Đánh giá) — Issue #179.
        var fileEHSDT = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.ToTrinhThamDinhNhaThau_FileEHSDT)]
        ))).ToAttachmentEntities().Select(x => x.ToDto()).ToList();
        var fileDanhGia = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [nameof(EGroupType.ToTrinhThamDinhNhaThau_FileDanhGia)]
        ))).ToAttachmentEntities().Select(x => x.ToDto()).ToList();

        // File Tờ trình kết quả — GroupId là ToTrinhQuyetDinh.Id (long), chỉ khi có bản ghi.
        List<TepDinhKemDto>? filesToTrinhKetQua = null;
        if (loaded.ToTrinhKetQua != null)
        {
            filesToTrinhKetQua = (await Mediator.Send(new GetAttachmentsQuery(
                GroupIds: [loaded.ToTrinhKetQua.Id.ToString()],
                BaseGroupTypes: [nameof(EGroupType.ToTrinhQuyetDinh)]
            ))).ToAttachmentEntities().Select(x => x.ToDto()).ToList();
        }

        // File Quyết định phê duyệt — GroupId là VanBanQuyetDinh.Id (= entity.Id), chỉ khi có bản ghi.
        List<TepDinhKemDto>? filesQuyetDinh = null;
        if (loaded.QuyetDinhPheDuyet != null)
        {
            filesQuyetDinh = (await Mediator.Send(new GetAttachmentsQuery(
                GroupIds: [loaded.QuyetDinhPheDuyet.Id.ToString()],
                BaseGroupTypes: [nameof(EGroupType.ToTrinhThamDinhNhaThau_QuyetDinh)]
            ))).ToAttachmentEntities().Select(x => x.ToDto()).ToList();
        }

        return ResultApi.Ok(entity.ToChiTietDto(
            danhSachTepDinhKem: danhSachTepDinhKem.Select(x => x.ToDto()).ToList(),
            danhSachTepThamDinh: danhSachTepThamDinh.Select(x => x.ToDto()).ToList(),
            filesDoiChieu: filesDoiChieu,
            filesThuongThao: filesThuongThao,
            filesThamDinh: filesThamDinhBuoc,
            fileEHSDT: fileEHSDT,
            fileDanhGia: fileDanhGia,
            toTrinhKetQua: loaded.ToTrinhKetQua,
            filesToTrinhKetQua: filesToTrinhKetQua,
            quyetDinh: loaded.QuyetDinhPheDuyet,
            filesQuyetDinh: filesQuyetDinh
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

    /// <summary>
    /// Tạo mới Tờ trình thẩm định nhà thầu (Issue #179) — 1 gói thầu / 1 nhà thầu,
    /// gồm Đối chiếu/Thương thảo/Thẩm định (ToTrinhThamDinhBuocXuLy), Tờ trình kết quả
    /// (ToTrinhQuyetDinh) và Quyết định phê duyệt (VanBanQuyetDinh, trạng thái Chờ duyệt).
    /// </summary>
    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] ToTrinhThamDinhNhaThauThemMoiDto dto, CancellationToken cancellationToken = default)
    {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId), cancellationToken);
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step), cancellationToken);

        var result = await Mediator.Send(new ToTrinhThamDinhNhaThauThemMoiCommand(dto), cancellationToken);
        var entity = result.Entity;

        if (dto.ThongTinNhaThau?.FileEHSDT is { Count: > 0 } fileEHSDT)
        {
            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau_FileEHSDT)],
                Entities = [.. fileEHSDT.ToEntities(entity.Id, EGroupType.ToTrinhThamDinhNhaThau_FileEHSDT)],
                AutoDeleteMissing = true
            }, cancellationToken);
        }
        if (dto.ThongTinNhaThau?.FileDanhGia is { Count: > 0 } fileDanhGia)
        {
            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau_FileDanhGia)],
                Entities = [.. fileDanhGia.ToEntities(entity.Id, EGroupType.ToTrinhThamDinhNhaThau_FileDanhGia)],
                AutoDeleteMissing = true
            }, cancellationToken);
        }
        if (dto.DoiChieu?.File is { Count: > 0 } fileDoiChieu)
        {
            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau_DoiChieu)],
                Entities = [.. fileDoiChieu.ToEntities(entity.Id, EGroupType.ToTrinhThamDinhNhaThau_DoiChieu)],
                AutoDeleteMissing = true
            }, cancellationToken);
        }
        if (dto.ThuongThao?.File is { Count: > 0 } fileThuongThao)
        {
            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau_ThuongThao)],
                Entities = [.. fileThuongThao.ToEntities(entity.Id, EGroupType.ToTrinhThamDinhNhaThau_ThuongThao)],
                AutoDeleteMissing = true
            }, cancellationToken);
        }
        if (dto.ThamDinh?.File is { Count: > 0 } fileThamDinh)
        {
            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau_ThamDinh)],
                Entities = [.. fileThamDinh.ToEntities(entity.Id, EGroupType.ToTrinhThamDinhNhaThau_ThamDinh)],
                AutoDeleteMissing = true
            }, cancellationToken);
        }
        if (result.ToTrinhQuyetDinhId is { } toTrinhQuyetDinhId && dto.ToTrinhKetQua?.File is { Count: > 0 } fileToTrinhKetQua)
        {
            // ToTrinhQuyetDinh.Id là long (không phải Guid) — không dùng được overload
            // ToEntities(Guid groupId,...), map thủ công GroupId theo id dạng long.
            var files = fileToTrinhKetQua.Select(f => new Attachment {
                Id = f.Id ?? GuidExtensions.GetSequentialGuidId(),
                ParentId = f.ParentId,
                GroupId = toTrinhQuyetDinhId.ToString(),
                GroupType = nameof(EGroupType.ToTrinhQuyetDinh),
                Type = f.Type,
                FileName = f.FileName,
                OriginalName = f.OriginalName,
                Path = f.Path,
                Size = f.Size,
            }).ToList();

            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = toTrinhQuyetDinhId.ToString(),
                GroupTypes = [nameof(EGroupType.ToTrinhQuyetDinh)],
                Entities = files,
                AutoDeleteMissing = true
            }, cancellationToken);
        }
        if (result.VanBanQuyetDinhId is { } vanBanQuyetDinhId && dto.QuyetDinhPheDuyet?.File is { Count: > 0 } fileQuyetDinh)
        {
            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = vanBanQuyetDinhId.ToString(),
                GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau_QuyetDinh)],
                Entities = [.. fileQuyetDinh.ToEntities(vanBanQuyetDinhId, EGroupType.ToTrinhThamDinhNhaThau_QuyetDinh)],
                AutoDeleteMissing = true
            }, cancellationToken);
        }

        return ResultApi.Ok(new { entity.Id, ToTrinhQuyetDinhId = result.ToTrinhQuyetDinhId, VanBanQuyetDinhId = result.VanBanQuyetDinhId });
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

        // File của 3 bước xử lý (Đối chiếu/Thương thảo/Thẩm định) — Issue #179.
        List<TepDinhKemDto>? filesDoiChieu = null;
        if (model.DoiChieu?.File is { } fileDoiChieu)
        {
            var entities = fileDoiChieu.ToEntities(entity.Id, EGroupType.ToTrinhThamDinhNhaThau_DoiChieu).ToList();
            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau_DoiChieu)],
                Entities = entities,
                AutoDeleteMissing = true
            }, cancellationToken);
            filesDoiChieu = entities.Select(x => x.ToDto()).ToList();
        }
        List<TepDinhKemDto>? filesThuongThao = null;
        if (model.ThuongThao?.File is { } fileThuongThao)
        {
            var entities = fileThuongThao.ToEntities(entity.Id, EGroupType.ToTrinhThamDinhNhaThau_ThuongThao).ToList();
            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau_ThuongThao)],
                Entities = entities,
                AutoDeleteMissing = true
            }, cancellationToken);
            filesThuongThao = entities.Select(x => x.ToDto()).ToList();
        }
        List<TepDinhKemDto>? filesThamDinhBuoc = null;
        if (model.ThamDinh?.File is { } fileThamDinh)
        {
            var entities = fileThamDinh.ToEntities(entity.Id, EGroupType.ToTrinhThamDinhNhaThau_ThamDinh).ToList();
            await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                GroupTypes = [nameof(EGroupType.ToTrinhThamDinhNhaThau_ThamDinh)],
                Entities = entities,
                AutoDeleteMissing = true
            }, cancellationToken);
            filesThamDinhBuoc = entities.Select(x => x.ToDto()).ToList();
        }

        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem.ToList(), danhSachFileThamDinh.ToList(),
            filesDoiChieu, filesThuongThao, filesThamDinhBuoc));
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
