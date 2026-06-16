using BuildingBlocks.Domain.Providers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.Common.Extensions;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;
using QLDA.Application.Common.Mapping;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.Queries;

// Không implement IMayHaveGlobalFilter - không có search full-text
// CreatedBy luôn lấy từ IUserProvider (JWT token), không cho UI truyền
public record BanGiaoHoSoGetDanhSachQuery : AggregateRootPagination, IRequest<PaginatedList<BanGiaoHoSoDto>> {
    public BanGiaoHoSoSearchDto SearchDto { get; set; } = new();
}

internal class BanGiaoHoSoGetDanhSachQueryHandler : IRequestHandler<BanGiaoHoSoGetDanhSachQuery, PaginatedList<BanGiaoHoSoDto>> {
    private readonly IRepository<BanGiaoHoSo, Guid> _banGiaoRepository;
    private readonly IRepository<QLDA.Domain.Entities.TepDinhKem, Guid> _tepDinhKemRepository;
    private readonly IRepository<BuildingBlocks.Domain.Entities.UserMaster, long> _userMasterRepository;
    private readonly IRepository<DmDonVi, long> _danhMucDonViRepository;  // ⚠️ DM_DONVI – không FK
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IUserProvider _userProvider;
    private readonly IBuocAuthorizationProvider _auth;

    public BanGiaoHoSoGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _banGiaoRepository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _tepDinhKemRepository = serviceProvider.GetRequiredService<IRepository<QLDA.Domain.Entities.TepDinhKem, Guid>>();
        _userMasterRepository = serviceProvider.GetRequiredService<IRepository<BuildingBlocks.Domain.Entities.UserMaster, long>>();
        _danhMucDonViRepository = serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    }

    public async Task<PaginatedList<BanGiaoHoSoDto>> Handle(BanGiaoHoSoGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        // LeftOuterJoin UserMaster và DanhMucDonVi (không FK – bảng đặc biệt)
        var users = _userMasterRepository.GetQueryableSet().AsNoTracking();
        var donVis = _danhMucDonViRepository.GetQueryableSet().AsNoTracking();

        var queryable = _banGiaoRepository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => e.CreatedBy == _userProvider.Id.ToString())
            .WhereFilterBuocVisibility(_duAnBuocRepo, _auth, _userProvider, e => e.BuocId)
            .WhereIf(request.SearchDto.TrangThai.HasValue, e => (int)e.TrangThai == request.SearchDto.TrangThai!.Value)
            .WhereIf(request.SearchDto.DuAnId.HasValue, e => e.DuAnId == request.SearchDto.DuAnId!.Value)
            .WhereIf(request.SearchDto.BuocId.HasValue, e => e.BuocId == request.SearchDto.BuocId!.Value)
            .LeftOuterJoin(users, e => e.CreatedBy, u => u.Id.ToString(), (e, user) => new { e, user })
            .LeftOuterJoin(donVis, x => x.e.PhongBanChuTriId, d => (long?)d.Id, (x, donViChuTri) => new { x.e, x.user, donViChuTri })
            .LeftOuterJoin(donVis, x => x.e.PhongBanNhanId, d => (long?)d.Id, (x, donViNhan) => new { x.e, x.user, x.donViChuTri, donViNhan })
            .OrderByDescending(x => x.e.CreatedAt)
            .Select(x => new BanGiaoHoSoDto {
                Id = x.e.Id,
                Ma = x.e.Ma,
                TenHoSo = x.e.TenHoSo,
                DuAnId = x.e.DuAnId,
                TenDuAn = x.e.DuAn!.TenDuAn,
                BuocId = x.e.BuocId,
                TenBuoc = x.e.Buoc!.TenBuoc,
                GhiChu = x.e.GhiChu,
                PhongBanChuTriId = x.e.PhongBanChuTriId,
                TenPhongBan = x.donViChuTri != null ? x.donViChuTri.TenDonVi : null,
                PhongBanNhanId = x.e.PhongBanNhanId,
                TenPhongBanNhan = x.donViNhan != null ? x.donViNhan.TenDonVi : null,
                TrangThai = (int)x.e.TrangThai,
                TenTrangThai = GetTrangThaiText(x.e.TrangThai),
                NgayBanGiao = x.e.NgayBanGiao.HasValue ? DateOnly.FromDateTime(x.e.NgayBanGiao.Value.LocalDateTime) : null,
                TenNguoiTao = x.user != null ? x.user.HoTen : null,
                CreatedAt = x.e.CreatedAt,
                // Tệp HS bàn giao (EGroupType.BanGiaoHoSo)
                DanhSachTepHSBanGiao = _tepDinhKemRepository.GetQueryableSet()
                    .Where(f => f.GroupId == x.e.Id.ToString() && f.GroupType == nameof(EGroupType.BanGiaoHoSo) && !f.IsDeleted)
                    .Select(f => f.ToDto()).ToList(),
                // Biên bản bàn giao (EGroupType.BienBanBanGiao)
                DanhSachBienBanBanGiao = _tepDinhKemRepository.GetQueryableSet()
                    .Where(f => f.GroupId == x.e.Id.ToString() && f.GroupType == nameof(EGroupType.BienBanBanGiao) && !f.IsDeleted)
                    .Select(f => f.ToDto()).ToList()
            });

        return await queryable
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }

    private static string GetTrangThaiText(ETrangThaiBanGiao trangThai) {
        return trangThai switch {
            ETrangThaiBanGiao.KhoiTao => "Khởi tạo",
            ETrangThaiBanGiao.DaBanGiao => "Đã bàn giao",
            _ => "Không xác định"
        };
    }
}
