using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Domain.Entities;

namespace QLDA.Application.DanhMucDonVis.Queries;

public record DanhMucDonViGetDanhSachQuery : AggregateRootPagination, IRequest<PaginatedList<DmDonVi>>
{
    /// <summary>
    /// Cấp ???
    /// </summary>
    public int? Cap { get; set; }

    /// <summary>
    /// Cấp đơn vị
    /// </summary>
    public List<long?>? CapDonViIds { get; set; }

    /// <summary>
    /// Lọc theo dự án: chỉ lấy đơn vị thuộc DuAn.DonViPhuTrachChinhId
    /// và DuAn.DuAnChiuTrachNhiemXuLys (cả DonViPhoiHop + DonViTheoDoi)
    /// </summary>
    public Guid? DuAnId { get; set; }

    /// <summary>
    /// Chỉ lấy phòng ban thuộc đơn vị của user hiện tại (DonViCapChaId = DonViID).
    /// </summary>
    public bool ChiLayPhongBanThuocDonVi { get; set; }
}

public record DanhMucDonViGetDanhSachQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DanhMucDonViGetDanhSachQuery, PaginatedList<DmDonVi>>
{
    private readonly IRepository<DmDonVi, long> DanhMucDonVi =
        ServiceProvider.GetRequiredService<IRepository<DmDonVi, long>>();

    private readonly IRepository<DuAn, Guid> DuAnRepository =
        ServiceProvider.GetRequiredService<IRepository<DuAn, Guid>>();

    private readonly IUserProvider _userProvider =
        ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<DmDonVi>> Handle(DanhMucDonViGetDanhSachQuery request,
        CancellationToken cancellationToken)
    {
        // Lấy tập đơn vị liên quan tới DuAn (nếu có DuAnId):
        //   - DuAn.DonViPhuTrachChinhId
        //   - DuAn.DuAnChiuTrachNhiemXuLys (cả DonViPhoiHop + DonViTheoDoi)
        // Sau đó AND với CapDonViIds/Cap hiện có.
        HashSet<long>? duAnDonViIds = null;
        if (request.DuAnId.HasValue)
        {
            var duAn = await DuAnRepository.GetQueryableSet()
                .AsNoTracking()
                .Where(d => d.Id == request.DuAnId.Value)
                .Select(d => new
                {
                    d.DonViPhuTrachChinhId,
                    ChiuTrachNhiemXuLyIds = d.DuAnChiuTrachNhiemXuLys!.Select(x => x.RightId)
                })
                .FirstOrDefaultAsync(cancellationToken);

            duAnDonViIds = new HashSet<long>();

            if (duAn?.DonViPhuTrachChinhId.HasValue == true) duAnDonViIds.Add(duAn.DonViPhuTrachChinhId.Value);

            if (duAn?.ChiuTrachNhiemXuLyIds != null)
                foreach (var id in duAn.ChiuTrachNhiemXuLyIds)
                {
                    duAnDonViIds.Add(id);
                }
        }

        var query = DanhMucDonVi.GetQueryableSet().AsNoTracking()
            .Where(e => e.Used == true)
            .WhereIf(request.Cap > 0, e => e.Cap == request.Cap)
            .WhereIf(request.CapDonViIds != null, e => request.CapDonViIds!.Contains(e.CapDonViId))
            .WhereIf(duAnDonViIds != null, e => duAnDonViIds!.Contains(e.Id));

        if (request.ChiLayPhongBanThuocDonVi) {
            var donViId = DmDonViPhongBanScope.TryGetCurrentDonViId(_userProvider);
            query = DmDonViPhongBanScope.FilterPhongBanThuocDonVi(query, donViId);
        }


        return await query
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}