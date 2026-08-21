using BuildingBlocks.Application.Attachments.Common;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

namespace QLDA.Application.HoSoMoiThauDienTus.Queries;

public record HoSoMoiThauDienTuGetDanhSachQuery(HoSoMoiThauDienTuSearchDto SearchDto)
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<HoSoMoiThauDienTuDto>> {
    public string? GlobalFilter { get; set; }
}

internal class HoSoMoiThauDienTuGetDanhSachQueryHandler : IRequestHandler<HoSoMoiThauDienTuGetDanhSachQuery, PaginatedList<HoSoMoiThauDienTuDto>> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IRepository<ToTrinhQuyetDinh, long> ToTrinhQuyetDinh;

    public HoSoMoiThauDienTuGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        ToTrinhQuyetDinh = serviceProvider.GetRequiredService<IRepository<ToTrinhQuyetDinh, long>>();
    }

    public async Task<PaginatedList<HoSoMoiThauDienTuDto>> Handle(HoSoMoiThauDienTuGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = HoSoMoiThauDienTu.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.DuAn)
            .Include(e => e.Buoc)
            .Include(e => e.HinhThucLuaChonNhaThau)
            .Include(e => e.GoiThau)
            .Include(e => e.TrangThaiPheDuyet)
            .WhereGlobalFilter(
                request,
                e => e.ThoiGianThucHien
            );

        if (request.SearchDto.DuAnId.HasValue) {
            queryable = queryable.Where(e => e.DuAnId == request.SearchDto.DuAnId);
        }
        if (request.SearchDto.LoaiDuAnTheoNamId > 0) {
            queryable = queryable.Where(e => e.DuAn!.LoaiDuAnTheoNamId == request.SearchDto.LoaiDuAnTheoNamId);
        }
        if (request.SearchDto.GoiThauId.HasValue) {
            queryable = queryable.Where(e => e.GoiThauId == request.SearchDto.GoiThauId);
        }

        var groupTypesOnEntityId = AttachmentSubquery.ExpandGroupTypes(
            includeSigned: true,
            nameof(EGroupType.HoSoMoiThauDienTu),
            nameof(EGroupType.HoSoMoiThauDienTuToTrinh),
            nameof(EGroupType.HoSoMoiThauDienTuQuyetDinh),
            nameof(EGroupType.HoSoMoiThauDienTuQuyetDinhTD),
            nameof(EGroupType.HoSoMoiThauDienTuCamKetTD),
            nameof(EGroupType.HoSoMoiThauDienTuBaoCaoTD));

        var result = await queryable
             .Select(e => new HoSoMoiThauDienTuDto()
             {
                 Id = e.Id,
                 DuAnId = e.DuAnId,
                 BuocId = e.BuocId,
                 TenDuAn = e.DuAn!.TenDuAn,
                 TenBuoc = e.Buoc!.TenBuoc,
                 HinhThucLuaChonNhaThauId = e.HinhThucLuaChonNhaThauId,
                 ThamDinh = e.ThamDinh ?? false,
                 TenHinhThucLuaChonNhaThau = e.HinhThucLuaChonNhaThau!.Ten,
                 GoiThauId = e.GoiThauId,
                 TenGoiThau = e.GoiThau!.Ten,
                 GiaTri = e.GiaTri,
                 ThoiGianThucHien = e.ThoiGianThucHien,
                 TrangThaiDangTai = e.TrangThaiDangTai,
                 TrangThaiId = e.TrangThaiId,
                 TenTrangThai = e.TrangThaiId == null ? TrangThaiPheDuyetCodes.Default.TenDuThao : e.TrangThaiPheDuyet!.Ten,
                 DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString() && groupTypesOnEntityId.Contains(i.GroupType))
                    .Select(i => i.ToDto()).ToList()
             })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);

        var ids = result.Data.Select(x => x.Id).ToList();
        if (ids.Count == 0)
            return result;

        var loaiToTrinh = ToTrinhQuyetDinhLoai.HoSoMoiThauToTrinh;
        var loaiQuyetDinh = ToTrinhQuyetDinhLoai.HoSoMoiThauQuyetDinh;

        var vanBan = await ToTrinhQuyetDinh.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.EntityId != null
                && ids.Contains(e.EntityId.Value)
                && (e.Loai == loaiToTrinh || e.Loai == loaiQuyetDinh))
            .ToListAsync(cancellationToken);

        var toTrinhByHoSoId = vanBan
            .Where(e => e.Loai == loaiToTrinh && e.EntityId.HasValue)
            .GroupBy(e => e.EntityId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        var quyetDinhByHoSoId = vanBan
            .Where(e => e.Loai == loaiQuyetDinh && e.EntityId.HasValue)
            .GroupBy(e => e.EntityId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var item in result.Data) {
            item.ToTrinh = toTrinhByHoSoId.TryGetValue(item.Id, out var tt) ? tt.ToDto() : null;
            item.QuyetDinh = quyetDinhByHoSoId.TryGetValue(item.Id, out var qd) ? qd.ToDto() : null;
        }

        var legacyGroupIds = vanBan.Select(x => x.Id.ToString()).ToList();
        var groupTypesToTrinh = AttachmentSubquery.ExpandGroupTypes(
            includeSigned: true, nameof(EGroupType.HoSoMoiThauDienTuToTrinh));
        var groupTypesQuyetDinh = AttachmentSubquery.ExpandGroupTypes(
            includeSigned: true, nameof(EGroupType.HoSoMoiThauDienTuQuyetDinh));

        var legacyFiles = legacyGroupIds.Count == 0
            ? []
            : await TepDinhKem.GetQueryableSet().AsNoTracking()
                .Where(i => legacyGroupIds.Contains(i.GroupId)
                    && (groupTypesToTrinh.Contains(i.GroupType) || groupTypesQuyetDinh.Contains(i.GroupType)))
                .Select(i => i.ToDto())
                .ToListAsync(cancellationToken);

        var filesByGroupId = legacyFiles
            .Where(f => f.GroupId != null)
            .GroupBy(f => f.GroupId!)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var item in result.Data) {
            AppendLegacyFiles(item, item.ToTrinh?.Id, filesByGroupId);
            AppendLegacyFiles(item, item.QuyetDinh?.Id, filesByGroupId);
        }

        return result;
    }

    private static void AppendLegacyFiles(
        HoSoMoiThauDienTuDto item,
        long? groupId,
        Dictionary<string, List<TepDinhKemDto>> filesByGroupId) {
        if (groupId is not { } id || !filesByGroupId.TryGetValue(id.ToString(), out var extra))
            return;
        item.DanhSachTepDinhKem = (item.DanhSachTepDinhKem ?? [])
            .Concat(extra)
            .DistinctBy(f => f.Id)
            .ToList();
    }
}
