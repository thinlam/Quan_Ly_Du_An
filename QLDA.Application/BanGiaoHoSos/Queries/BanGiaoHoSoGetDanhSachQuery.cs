using BuildingBlocks.Application.Attachments.Common;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.BanGiaoHoSos.Queries;

// CreatedBy luôn lấy từ IUserProvider (JWT token), không cho UI truyền
public record BanGiaoHoSoGetDanhSachQuery : AggregateRootPagination, IRequest<PaginatedList<BanGiaoHoSoDto>>
{
    public BanGiaoHoSoSearchDto SearchDto { get; set; } = new();
}

internal class BanGiaoHoSoGetDanhSachQueryHandler : IRequestHandler<BanGiaoHoSoGetDanhSachQuery, PaginatedList<BanGiaoHoSoDto>>
{
    private readonly IRepository<BanGiaoHoSo, Guid> _banGiaoRepository;
    private readonly IRepository<Attachment, Guid> _tepDinhKemRepository;
    private readonly IRepository<UserMaster, long> _userMasterRepository;
    private readonly IRepository<DmDonVi, long> _danhMucDonViRepository;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _buocAuth;
    private readonly IAuthorizationContext _authContext;

    public BanGiaoHoSoGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        _banGiaoRepository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _tepDinhKemRepository = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _userMasterRepository = serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
        _danhMucDonViRepository = serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _buocAuth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<PaginatedList<BanGiaoHoSoDto>> Handle(BanGiaoHoSoGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var users = _userMasterRepository.GetQueryableSet().AsNoTracking();
        var donVis = _danhMucDonViRepository.GetQueryableSet().AsNoTracking();

        var filtered = _banGiaoRepository.GetQueryableSet()
            .ApplyDanhSachFilters(
                request.SearchDto,
                _buocAuth,
                _duAnBuocRepo,
                _authContext,
                users,
                donVis);

        var groupTypesHsBanGiao = AttachmentSubquery.ExpandGroupTypes(
            includeSigned: true, nameof(EGroupType.BanGiaoHoSo));
        var groupTypesBienBan = AttachmentSubquery.ExpandGroupTypes(
            includeSigned: true, nameof(EGroupType.BienBanBanGiao));

        var queryable = filtered
            .Select(x => new BanGiaoHoSoDto
            {
                Id = x.E.Id,
                Ma = x.E.Ma,
                TenHoSo = x.E.TenHoSo,
                DuAnId = x.E.DuAnId,
                TenDuAn = x.E.DuAn!.TenDuAn,
                BuocId = x.E.BuocId,
                TenBuoc = x.E.Buoc!.TenBuoc,
                GhiChu = x.E.GhiChu,
                PhongBanChuTriId = x.E.PhongBanChuTriId,
                TenPhongBan = x.DonViChuTri != null ? x.DonViChuTri.TenDonVi : null,
                PhongBanNhanId = x.E.PhongBanNhanId,
                TenPhongBanNhan = x.DonViNhan != null ? x.DonViNhan.TenDonVi : null,
                TrangThai = (int)x.E.TrangThai,
                TenTrangThai = BanGiaoHoSoQueryableExtensions.GetTrangThaiText(x.E.TrangThai),
                NgayBanGiao = x.E.NgayBanGiao.HasValue
                    ? DateOnly.FromDateTime(x.E.NgayBanGiao.Value.LocalDateTime)
                    : null,
                TenNguoiTao = x.User != null ? x.User.HoTen : null,
                CreatedAt = x.E.CreatedAt,
                DanhSachTepHSBanGiao = _tepDinhKemRepository.GetQueryableSet()
                    .Where(f => f.GroupId == x.E.Id.ToString() && groupTypesHsBanGiao.Contains(f.GroupType))
                    .Select(f => f.ToDto()).ToList(),
                DanhSachBienBanBanGiao = _tepDinhKemRepository.GetQueryableSet()
                    .Where(f => f.GroupId == x.E.Id.ToString() && groupTypesBienBan.Contains(f.GroupType))
                    .Select(f => f.ToDto()).ToList()
            });

        return await queryable
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}
