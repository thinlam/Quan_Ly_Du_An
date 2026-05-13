using BuildingBlocks.Domain.Providers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;
using QLDA.Application.Common.Mapping;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.BanGiaoHoSos.Queries;

// Không implement IMayHaveGlobalFilter - không có search full-text
// UserId luôn lấy từ IUserProvider (JWT token), không cho UI truyền
public record BanGiaoHoSoGetDanhSachQuery : AggregateRootPagination, IRequest<PaginatedList<BanGiaoHoSoDto>> {
    public BanGiaoHoSoSearchDto SearchDto { get; set; } = new();
}

internal class BanGiaoHoSoGetDanhSachQueryHandler : IRequestHandler<BanGiaoHoSoGetDanhSachQuery, PaginatedList<BanGiaoHoSoDto>> {
    private readonly IRepository<BanGiaoHoSo, Guid> _banGiaoRepository;
    private readonly IRepository<TepDinhKem, Guid> _tepDinhKemRepository;
    private readonly IUserProvider _userProvider;

    public BanGiaoHoSoGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _banGiaoRepository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _tepDinhKemRepository = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    }

    public async Task<PaginatedList<BanGiaoHoSoDto>> Handle(BanGiaoHoSoGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        // Khai báo tường minh IQueryable để tránh lỗi type inference với IIncludableQueryable
        IQueryable<BanGiaoHoSo> queryable = _banGiaoRepository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => e.UserId == _userProvider.Id)  // Luôn filter theo người dùng hiện tại (từ JWT token)
            .Include(e => e.User)
            .Include(e => e.PhongBanChuTri)
            .Include(e => e.DuAn)
            .Include(e => e.Buoc);

        // Filter theo TrangThai nếu được truyền (1 param duy nhất từ UI)
        if (request.SearchDto.TrangThai.HasValue) {
            queryable = queryable.Where(e => (int)e.TrangThai == request.SearchDto.TrangThai.Value);
        }

        return await queryable
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new BanGiaoHoSoDto {
                Id = e.Id,
                Ma = e.Ma,
                TenHoSo = e.TenHoSo,
                DuAnId = e.DuAnId,
                TenDuAn = e.DuAn!.TenDuAn,
                BuocId = e.BuocId,
                TenBuoc = e.Buoc!.Ten,
                PhongBanChuTriId = e.PhongBanChuTriId,
                TenPhongBan = e.PhongBanChuTri!.TenDonVi,
                UserId = e.UserId,
                TenNguoiTao = e.User!.HoTen,
                GhiChu = e.GhiChu,
                TrangThai = (int)e.TrangThai,
                TenTrangThai = GetTrangThaiText(e.TrangThai),
                NgayBanGiao = e.NgayBanGiao,
                // Tệp HS bàn giao (EGroupType.BanGiaoHoSo)
                DanhSachTepHSBanGiao = _tepDinhKemRepository.GetQueryableSet()
                    .Where(f => f.GroupId == e.Id.ToString() && f.GroupType == nameof(EGroupType.BanGiaoHoSo) && !f.IsDeleted)
                    .Select(f => f.ToDto()).ToList(),
                // Biên bản bàn giao (EGroupType.BienBanBanGiao)
                DanhSachBienBanBanGiao = _tepDinhKemRepository.GetQueryableSet()
                    .Where(f => f.GroupId == e.Id.ToString() && f.GroupType == nameof(EGroupType.BienBanBanGiao) && !f.IsDeleted)
                    .Select(f => f.ToDto()).ToList()
            })
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
