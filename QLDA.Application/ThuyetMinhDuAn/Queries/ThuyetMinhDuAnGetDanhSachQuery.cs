using QLDA.Application.Authorization;

using BuildingBlocks.Application.Attachments.Common;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ThuyetMinhDuAns.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

namespace QLDA.Application.ThuyetMinhDuAns.Queries;

public record ThuyetMinhDuAnGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<ThuyetMinhDuAnDto>> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
}

internal class ThuyetMinhDuAnGetDanhSachQueryHandler(IServiceProvider ServiceProvider) : IRequestHandler<ThuyetMinhDuAnGetDanhSachQuery, PaginatedList<ThuyetMinhDuAnDto>> {
    private readonly IRepository<ThuyetMinhDuAn, Guid> _thuyetMinhDuAn = ServiceProvider.GetRequiredService<IRepository<ThuyetMinhDuAn, Guid>>();
    private readonly IRepository<Attachment, Guid> _tepDinhKem = ServiceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo = ServiceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth = ServiceProvider.GetRequiredService<IBuocAuthorizationProvider>();

    private readonly IAuthorizationContext _authContext = ServiceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<PaginatedList<ThuyetMinhDuAnDto>> Handle(ThuyetMinhDuAnGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        //   bool dieuKienThayTatCa = false;
        var queryable = _buocAuth.FilterVisibleChildEntities(_thuyetMinhDuAn.GetQueryableSet(), _duAnBuocRepo, _authContext, e => e.BuocId)
            //.WhereIf(_authContext.UserId > 0 && !dieuKienThayTatCa, e => e.CreatedBy == _authContext.UserId.ToString(), e => dieuKienThayTatCa)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId);

        var groupTypesThuyetMinh = AttachmentSubquery.ExpandGroupTypes(includeSigned: true, nameof(EGroupType.ThuyetMinhDuAn));
        var groupTypesThamDinh = AttachmentSubquery.ExpandGroupTypes(includeSigned: true, nameof(EGroupType.ThuyetMinhDuAnThamDinh));

        return await queryable
            .Select(e => new ThuyetMinhDuAnDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                So = e.So,
                NgayTrinh = e.NgayTrinh,
                TrichYeu = e.TrichYeu,
                KetQuaThamDinh = e.KetQuaThamDinh,
                TrangThaiThamDinhId = e.TrangThaiThamDinhId,
                TenTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThaiThamDinh = e.TrangThaiThamDinh!.Ten,
                DanhSachTepDinhKem = _tepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString() && groupTypesThuyetMinh.Contains(i.GroupType))
                    .Select(i => i.ToDto()).ToList(),
                DanhSachTepThamDinh = _tepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString() && groupTypesThamDinh.Contains(i.GroupType))
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}
