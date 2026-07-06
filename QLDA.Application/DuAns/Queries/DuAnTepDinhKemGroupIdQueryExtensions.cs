using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;

namespace QLDA.Application.DuAns.Queries;

/// <summary>
/// Thu thập GroupId (string) của mọi đối tượng nghiệp vụ thuộc một dự án — dùng lọc TepDinhKem theo DuAnId.
/// </summary>
internal static class DuAnTepDinhKemGroupIdQueryExtensions
{
    internal static async Task<List<string>> ResolveGroupIdsAsync(
        Guid duAnId,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var goiThauRepo = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        var hopDongRepo = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        var nghiemThuRepo = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        var ketQuaTrungThauRepo = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        var keHoachVonRepo = serviceProvider.GetRequiredService<IRepository<KeHoachVon, Guid>>();
        var duToanRepo = serviceProvider.GetRequiredService<IRepository<DuToan, Guid>>();
        var dangTaiRepo = serviceProvider.GetRequiredService<IRepository<DangTaiKeHoachLcntLenMang, Guid>>();
        var hoSoDeXuatRepo = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        var baoCaoRepo = serviceProvider.GetRequiredService<IRepository<BaoCao, Guid>>();
        var phuLucHopDongRepo = serviceProvider.GetRequiredService<IRepository<PhuLucHopDong, Guid>>();
        var vanBanQuyetDinhRepo = serviceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();
        var thanhToanRepo = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        var tamUngRepo = serviceProvider.GetRequiredService<IRepository<TamUng, Guid>>();
        var hoSoMoiThauRepo = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        var banGiaoHoSoRepo = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        var phanKhaiKinhPhiRepo = serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
        var quyetDinhDieuChinhRepo = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        var toTrinhPheDuyetRepo = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();

        var duAnIdStr = duAnId.ToString();
        var groupIds = new List<string> { duAnIdStr };

        void AddIds(IEnumerable<string> ids) => groupIds.AddRange(ids);

        AddIds(await goiThauRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await hopDongRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await nghiemThuRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await ketQuaTrungThauRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await keHoachVonRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await duToanRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await dangTaiRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await hoSoDeXuatRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await baoCaoRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await phuLucHopDongRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await vanBanQuyetDinhRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await thanhToanRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await tamUngRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await hoSoMoiThauRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await banGiaoHoSoRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await phanKhaiKinhPhiRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await quyetDinhDieuChinhRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await toTrinhPheDuyetRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        return groupIds.Distinct().ToList();
    }
}
