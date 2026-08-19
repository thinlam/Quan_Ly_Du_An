using BuildingBlocks.Application.Attachments.Common;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Queries;

public record ToTrinhThamDinhNhaThauDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<ToTrinhThamDinhNhaThauDto>> {
 
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public long? PhongBanDeXuatId { get; set; }
    public long? NguoiDeXuatId { get; set; }
    public string? So { get; set; }
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
      
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }

}

internal class    ToTrinhThamDinhNhaThauDanhSachQueryHandler(IServiceProvider ServiceProvider)    : IRequestHandler<ToTrinhThamDinhNhaThauDanhSachQuery, PaginatedList<ToTrinhThamDinhNhaThauDto>> {
    private readonly IRepository<ToTrinhThamDinhNhaThau, Guid> ToTrinhThamDinhNhaThau =  ServiceProvider.GetRequiredService<IRepository<ToTrinhThamDinhNhaThau, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem = ServiceProvider.GetRequiredService<IRepository<Attachment, Guid>>();

    private readonly IRepository<ToTrinhQuyetDinh, long> ToTrinhQuyetDinh =
        ServiceProvider.GetRequiredService<IRepository<ToTrinhQuyetDinh, long>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<ToTrinhThamDinhNhaThauDto>> Handle(ToTrinhThamDinhNhaThauDanhSachQuery request,
        CancellationToken cancellationToken = default) {

        var queryable = ToTrinhThamDinhNhaThau.GetQueryableSet().AsNoTracking()
            .Include(e => e.GoiThau)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.BuocId != null, e => e.BuocId == request.BuocId)
            .WhereIf(request.TrangThaiDangTaiId != null, e => e.TrangThaiDangTaiId == request.TrangThaiDangTaiId);
        var result = await queryable
            .Select(e => new ToTrinhThamDinhNhaThauDto() {
                Id = e.Id,
                DuAnId=e.DuAnId,
                BuocId=e.BuocId,
                GoiThauId = e.GoiThauId,
                TenGoiThau = e.GoiThau != null ? e.GoiThau.Ten ?? string.Empty : string.Empty,
                NhaThauId = e.NhaThauId,
                TrangThaiDangTaiId = e.TrangThaiDangTaiId,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ma : string.Empty,
                TenTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ten : string.Empty,
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);

        // TepDinhKem load riêng theo GroupId thay vì correlated subquery trong Select
        // (correlated subquery → SQL APPLY — SQL Server chạy được nhưng SQLite thì không).
        var groupIds = result.Data.Where(x => x.Id != null).Select(x => x.Id!.ToString()).ToList();
        var tepDinhKems = groupIds.Count == 0
            ? []
            : await TepDinhKem.GetQueryableSet().AsNoTracking()
                .Where(i => groupIds.Contains(i.GroupId))
                .Select(i => i.ToDto())
                .ToListAsync(cancellationToken);
        var tepDinhKemByGroupId = tepDinhKems.GroupBy(x => x.GroupId)
            .Where(g => g.Key is not null)
            .ToDictionary(g => g.Key!, g => g.ToList());
        foreach (var item in result.Data)
            item.DanhSachTepDinhKem = item.Id is { } id && tepDinhKemByGroupId.TryGetValue(id.ToString(), out var files)
                ? files
                : [];

        // File "Tờ trình kết quả" — GroupId là ToTrinhQuyetDinh.Id (long), không nằm trong
        // groupIds của toTrinh nên danh-sach-tien-do sót file mà chi-tiet vẫn đủ (Issue #179).
        var toTrinhIds = result.Data.Select(x => x.Id).ToList();
        var toTrinhQuyetDinhs = toTrinhIds.Count == 0
            ? []
            : await ToTrinhQuyetDinh.GetQueryableSet().AsNoTracking()
                .Where(e => toTrinhIds.Contains(e.EntityId) && e.Loai == ToTrinhQuyetDinhLoai.ToTrinhThamDinhNhaThau)
                .Select(e => new { e.Id, e.EntityId })
                .ToListAsync(cancellationToken);

        var ketQuaGroupIds = toTrinhQuyetDinhs.Select(x => x.Id.ToString()).ToList();
        // Gồm cả KySo_ToTrinhQuyetDinh (file đã ký) — khớp GetAttachmentsQuery của chi-tiet.
        var ketQuaGroupTypes = AttachmentSubquery.ExpandGroupTypes(
            [nameof(EGroupType.ToTrinhQuyetDinh)], includeSigned: true);
        var ketQuaFiles = ketQuaGroupIds.Count == 0
            ? []
            : await TepDinhKem.GetQueryableSet().AsNoTracking()
                .Where(i => ketQuaGroupIds.Contains(i.GroupId) && ketQuaGroupTypes.Contains(i.GroupType))
                .Select(i => i.ToDto())
                .ToListAsync(cancellationToken);

        var ketQuaByToTrinhId = toTrinhQuyetDinhs
            .Where(x => x.EntityId.HasValue)
            .SelectMany(x => ketQuaFiles.Where(f => f.GroupId == x.Id.ToString()), (x, f) => new { x.EntityId, f })
            .GroupBy(x => x.EntityId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.f).ToList());

        foreach (var item in result.Data)
        {
            if (item.Id is { } id && ketQuaByToTrinhId.TryGetValue(id, out var files))
                item.DanhSachTepDinhKem = item.DanhSachTepDinhKem!.Concat(files).DistinctBy(f => f.Id).ToList();
        }

        return result;
    }
}