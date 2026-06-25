using BuildingBlocks.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.BanGiaoHoSos.Queries;

public record BanGiaoHoSoGetDanhSachExportQuery : IRequest<List<BanGiaoHoSoDanhSachExportDto>>
{
    public BanGiaoHoSoSearchDto SearchDto { get; set; } = new();
}

internal class BanGiaoHoSoGetDanhSachExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<BanGiaoHoSoGetDanhSachExportQuery, List<BanGiaoHoSoDanhSachExportDto>>
{
    private readonly IRepository<BanGiaoHoSo, Guid> _banGiaoRepository =
        serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
    private readonly IRepository<UserMaster, long> _userMasterRepository =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IRepository<DmDonVi, long> _danhMucDonViRepository =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IBuocAuthorizationProvider _buocAuth =
        serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationContext _authContext =
        serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<List<BanGiaoHoSoDanhSachExportDto>> Handle(
        BanGiaoHoSoGetDanhSachExportQuery request,
        CancellationToken cancellationToken = default)
    {
        var users = _userMasterRepository.GetQueryableSet().AsNoTracking();
        var donVis = _danhMucDonViRepository.GetQueryableSet().AsNoTracking();

        var rows = await _banGiaoRepository.GetQueryableSet()
            .AsNoTracking()
            .ApplyDanhSachFilters(
                request.SearchDto,
                _buocAuth,
                _duAnBuocRepo,
                _authContext,
                users,
                donVis)
            .ToListAsync(cancellationToken);

        ManagedException.ThrowIf(rows.Count == 0, "Không có dữ liệu để xuất");

        return rows.Select((row, index) => new BanGiaoHoSoDanhSachExportDto
        {
            Stt = index + 1,
            Ma = row.E.Ma,
            TenHoSo = row.E.TenHoSo,
            TenPhongBan = row.DonViChuTri?.TenDonVi,
            NgayTao = row.E.CreatedAt.LocalDateTime.ToString("dd/MM/yyyy"),
            TenTrangThai = BanGiaoHoSoQueryableExtensions.GetTrangThaiText(row.E.TrangThai),
        }).ToList();
    }
}
