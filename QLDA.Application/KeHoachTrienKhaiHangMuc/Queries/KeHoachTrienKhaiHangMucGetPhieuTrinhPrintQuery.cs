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
    private const string NoHangMucMessage =
        "Kế hoạch triển khai không có hạng mục công việc để xuất phiếu trình";

    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> _keHoachRepo =
        serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();
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
    private readonly IAuthorizationContext _authContext =
        serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<KeHoachTrienKhaiHangMucPhieuTrinhPrintDto> Handle(
        KeHoachTrienKhaiHangMucGetPhieuTrinhPrintQuery request,
        CancellationToken cancellationToken = default)
    {
        var keHoach = await _buocAuth.FilterVisibleChildEntities(
                _keHoachRepo.GetQueryableSet(),
                _duAnBuocRepo,
                _authContext,
                e => e.BuocId)
            .AsNoTracking()
            .Include(e => e.DuAn)
            .Include(e => e.DanhSachHangMuc)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(keHoach, "Không tìm thấy dữ liệu");

        var hangMucs = keHoach.DanhSachHangMuc?.ToList() ?? [];
        ManagedException.ThrowIf(hangMucs.Count == 0, NoHangMucMessage);

        var rows = await KeHoachTrienKhaiHangMucExportRowLoader.LoadAsync(
            hangMucs,
            _giaiDoanRepo,
            _donViRepo,
            _userRepo,
            cancellationToken);

        ManagedException.ThrowIf(rows.Count == 0, NoHangMucMessage);

        return new KeHoachTrienKhaiHangMucPhieuTrinhPrintDto
        {
            So = keHoach.So,
            NgayToTrinh = keHoach.NgayToTrinh,
            TrichYeu = keHoach.TrichYeu,
            MaDuAn = keHoach.DuAn?.MaDuAn,
            TenDuAn = keHoach.DuAn?.TenDuAn,
            DuAnDisplay = BuildDuAnDisplay(keHoach.DuAn?.MaDuAn, keHoach.DuAn?.TenDuAn),
            Rows = rows,
        };
    }

    private static string BuildDuAnDisplay(string? maDuAn, string? tenDuAn)
    {
        if (!string.IsNullOrWhiteSpace(maDuAn) && !string.IsNullOrWhiteSpace(tenDuAn))
            return $"{maDuAn} — {tenDuAn}";

        return maDuAn ?? tenDuAn ?? string.Empty;
    }
}
