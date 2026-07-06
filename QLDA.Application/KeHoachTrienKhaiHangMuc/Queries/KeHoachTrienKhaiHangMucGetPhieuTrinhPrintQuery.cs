using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.Authorization;
using QLDA.Application.KeHoachTrienKhaiHangMucs;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Queries;

public record KeHoachTrienKhaiHangMucGetPhieuTrinhPrintQuery : IRequest<KeHoachTrienKhaiHangMucPhieuTrinhPrintDto>
{
    public Guid Id { get; set; }
}

internal class KeHoachTrienKhaiHangMucGetPhieuTrinhPrintQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KeHoachTrienKhaiHangMucGetPhieuTrinhPrintQuery, KeHoachTrienKhaiHangMucPhieuTrinhPrintDto>
{
    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> _keHoachRepo =
        serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();
    private readonly IRepository<HangMucKeHoach, Guid> _hangMucRepo =
        serviceProvider.GetRequiredService<IRepository<HangMucKeHoach, Guid>>();
    private readonly IRepository<DuAn, Guid> _duAnRepo =
        serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IRepository<DanhMucGiaiDoan, int> _giaiDoanRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucGiaiDoan, int>>();
    private readonly IRepository<DmDonVi, long> _donViRepo =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    private readonly IRepository<UserMaster, long> _userRepo =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IBuocAuthorizationProvider _buocAuth =
        serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    private readonly IAuthorizationManager _authManager =
        serviceProvider.GetRequiredService<IAuthorizationManager>();
    private readonly IAuthorizationContext _authContext =
        serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<KeHoachTrienKhaiHangMucPhieuTrinhPrintDto> Handle(
        KeHoachTrienKhaiHangMucGetPhieuTrinhPrintQuery request,
        CancellationToken cancellationToken = default)
    {
        var keHoach = await LoadKeHoachForPrintAsync(request.Id, cancellationToken);

        ManagedException.ThrowIfNull(keHoach, "Không tìm thấy dữ liệu");

        await EnsureDuAnLoadedAsync(keHoach, cancellationToken);

        var hangMucs = await LoadHangMucsForKeHoachAsync(keHoach.Id, cancellationToken);

        var rows = hangMucs.Count == 0
            ? []
            : await KeHoachTrienKhaiHangMucExportRowLoader.LoadAsync(
                hangMucs,
                keHoach.DuAnId,
                _giaiDoanRepo,
                _duAnBuocRepo,
                _donViRepo,
                _userRepo,
                cancellationToken);

        var maDuAn = keHoach.DuAn?.MaDuAn?.Trim();
        var tenDuAn = keHoach.DuAn?.TenDuAn?.Trim();

        return new KeHoachTrienKhaiHangMucPhieuTrinhPrintDto
        {
            So = keHoach.So,
            NgayToTrinh = keHoach.NgayToTrinh,
            TrichYeu = keHoach.TrichYeu,
            MaDuAn = maDuAn,
            TenDuAn = tenDuAn,
            DuAnDisplay = BuildDuAnDisplay(maDuAn, tenDuAn),
            Rows = rows,
        };
    }

    private async Task<KeHoachTrienKhaiHangMuc?> LoadKeHoachForPrintAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var keHoach = await _buocAuth.FilterVisibleChildEntities(
                _keHoachRepo.GetQueryableSet(),
                _duAnBuocRepo,
                _authContext,
                e => e.BuocId)
            .AsNoTracking()
            .Include(e => e.DuAn)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (keHoach != null)
            return keHoach;

        return await _authManager.FilterVisible(
                _keHoachRepo.GetQueryableSet(),
                AuthorizationResourceKeys.DuAn)
            .AsNoTracking()
            .Include(e => e.DuAn)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    private async Task<List<HangMucKeHoach>> LoadHangMucsForKeHoachAsync(
        Guid keHoachId,
        CancellationToken cancellationToken) =>
        await _hangMucRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(h => h.KeHoachId == keHoachId)
            .OrderBy(h => h.CreatedAt)
            .ThenBy(h => h.Id)
            .ToListAsync(cancellationToken);

    private async Task EnsureDuAnLoadedAsync(
        KeHoachTrienKhaiHangMuc keHoach,
        CancellationToken cancellationToken)
    {
        if (keHoach.DuAn != null || keHoach.DuAnId == Guid.Empty)
            return;

        keHoach.DuAn = await _duAnRepo.GetQueryableSet()
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == keHoach.DuAnId, cancellationToken);
    }

    private static string BuildDuAnDisplay(string? maDuAn, string? tenDuAn)
    {
        var ma = maDuAn?.Trim();
        var ten = tenDuAn?.Trim();

        if (!string.IsNullOrEmpty(ma) && !string.IsNullOrEmpty(ten))
            return $"{ma} — {ten}";

        if (!string.IsNullOrEmpty(ma))
            return ma;

        return ten ?? string.Empty;
    }
}
