using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;

namespace QLDA.Application.DuAns.Services;

/// <summary>
/// Thu thập GroupId (string) của mọi đối tượng nghiệp vụ thuộc một dự án.
/// Dùng cho lọc TepDinhKem theo DuAnId.
/// </summary>
internal sealed class DuAnTepDinhKemGroupIdResolver(IServiceProvider serviceProvider)
{
    private readonly IRepository<GoiThau, Guid> _goiThauRepo =
        serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
    private readonly IRepository<HopDong, Guid> _hopDongRepo =
        serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
    private readonly IRepository<NghiemThu, Guid> _nghiemThuRepo =
        serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
    private readonly IRepository<KetQuaTrungThau, Guid> _ketQuaTrungThauRepo =
        serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
    private readonly IRepository<KeHoachVon, Guid> _keHoachVonRepo =
        serviceProvider.GetRequiredService<IRepository<KeHoachVon, Guid>>();
    private readonly IRepository<DuToan, Guid> _duToanRepo =
        serviceProvider.GetRequiredService<IRepository<DuToan, Guid>>();
    private readonly IRepository<DangTaiKeHoachLcntLenMang, Guid> _dangTaiRepo =
        serviceProvider.GetRequiredService<IRepository<DangTaiKeHoachLcntLenMang, Guid>>();
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> _hoSoDeXuatRepo =
        serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
    private readonly IRepository<BaoCao, Guid> _baoCaoRepo =
        serviceProvider.GetRequiredService<IRepository<BaoCao, Guid>>();
    private readonly IRepository<PhuLucHopDong, Guid> _phuLucHopDongRepo =
        serviceProvider.GetRequiredService<IRepository<PhuLucHopDong, Guid>>();
    private readonly IRepository<VanBanQuyetDinh, Guid> _vanBanQuyetDinhRepo =
        serviceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();
    private readonly IRepository<ThanhToan, Guid> _thanhToanRepo =
        serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
    private readonly IRepository<TamUng, Guid> _tamUngRepo =
        serviceProvider.GetRequiredService<IRepository<TamUng, Guid>>();
    private readonly IRepository<HoSoMoiThauDienTu, Guid> _hoSoMoiThauRepo =
        serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
    private readonly IRepository<BanGiaoHoSo, Guid> _banGiaoHoSoRepo =
        serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _phanKhaiKinhPhiRepo =
        serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _quyetDinhDieuChinhRepo =
        serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
    private readonly IRepository<ToTrinhPheDuyet, Guid> _toTrinhPheDuyetRepo =
        serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();

    public async Task<List<string>> ResolveGroupIdsAsync(
        Guid duAnId,
        CancellationToken cancellationToken = default)
    {
        var duAnIdStr = duAnId.ToString();
        var groupIds = new List<string> { duAnIdStr };

        void AddIds(IEnumerable<string> ids) => groupIds.AddRange(ids);

        AddIds(await _goiThauRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _hopDongRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _nghiemThuRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _ketQuaTrungThauRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _keHoachVonRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _duToanRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _dangTaiRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _hoSoDeXuatRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _baoCaoRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _phuLucHopDongRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _vanBanQuyetDinhRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _thanhToanRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _tamUngRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _hoSoMoiThauRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _banGiaoHoSoRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _phanKhaiKinhPhiRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _quyetDinhDieuChinhRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _toTrinhPheDuyetRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        return groupIds.Distinct().ToList();
    }
}
