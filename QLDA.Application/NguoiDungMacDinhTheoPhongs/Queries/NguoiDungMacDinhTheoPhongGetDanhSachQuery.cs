using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Queries;

public record NguoiDungMacDinhTheoPhongGetDanhSachQuery(NguoiDungMacDinhTheoPhongSearchDto SearchDto)
    : IRequest<PaginatedList<NguoiDungMacDinhTheoPhongDto>>;

internal class NguoiDungMacDinhTheoPhongGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NguoiDungMacDinhTheoPhongGetDanhSachQuery, PaginatedList<NguoiDungMacDinhTheoPhongDto>>
{
    private readonly IRepository<NguoiDungMacDinhTheoPhong, Guid> _repository =
        serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>();

    private readonly IRepository<DmDonVi, long> _dmDonVi =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();

    private readonly IRepository<UserMaster, long> _userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    public async Task<PaginatedList<NguoiDungMacDinhTheoPhongDto>> Handle(
        NguoiDungMacDinhTheoPhongGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var search = request.SearchDto;
        var keyword = search.Keyword ?? search.GlobalFilter;

        var queryable = _repository.GetQueryableSet()
            .AsNoTracking()
            .WhereIf(search.PhongBanId.HasValue, e => e.PhongBanId == search.PhongBanId)
            .WhereIf(search.NguoiDungId.HasValue, e => e.NguoiDungId == search.NguoiDungId);

        var dmDonVi = _dmDonVi.GetQueryableSet().AsNoTracking();
        var userMaster = _userMaster.GetQueryableSet().AsNoTracking();

        var query = from cfg in queryable
            join pb in dmDonVi on cfg.PhongBanId equals pb.Id into pbJoin
            from pb in pbJoin.DefaultIfEmpty()
            join nd in userMaster on cfg.NguoiDungId equals nd.UserPortalId into ndJoin
            from nd in ndJoin.DefaultIfEmpty()
            where string.IsNullOrEmpty(keyword)
                  || (pb.TenDonVi != null && pb.TenDonVi.Contains(keyword))
                  || (nd.HoTen != null && nd.HoTen.Contains(keyword))
                  || (nd.UserName != null && nd.UserName.Contains(keyword))
            orderby cfg.CreatedAt descending
            select new NguoiDungMacDinhTheoPhongDto
            {
                Id = cfg.Id,
                PhongBanId = cfg.PhongBanId,
                TenPhongBan = pb.TenDonVi,
                NguoiDungId = cfg.NguoiDungId,
                TenNguoiDung = nd.HoTen
            };

        return await query.PaginatedListAsync(search.Skip(), search.Take(), cancellationToken);
    }
}
