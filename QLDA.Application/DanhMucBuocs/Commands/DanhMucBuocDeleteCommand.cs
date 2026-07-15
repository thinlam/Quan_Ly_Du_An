using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

namespace QLDA.Application.DanhMucBuocs.Commands;

public record DanhMucBuocDeleteCommand(int Id) : IRequest<int>;

internal class DanhMucBuocDeleteCommandHandler : IRequestHandler<DanhMucBuocDeleteCommand, int>
{
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<DuAnBuoc, int> DuAnBuoc;
    private readonly IRepository<VanBanQuyetDinh, Guid> VanBanQuyetDinh;
    private readonly IRepository<GoiThau, Guid> GoiThau;
    private readonly IRepository<HopDong, Guid> HopDong;
    private readonly IRepository<BaoCaoTienDo, Guid> BaoCaoTienDo;
    private readonly IRepository<DangTaiKeHoachLcntLenMang, Guid> DangTaiKeHoachLcntLenMang;
    private readonly IRepository<KetQuaTrungThau, Guid> KetQuaTrungThau;
    private readonly IRepository<BaoCaoKhoKhanVuongMac, Guid> KhoKhanVuongMac;
    private readonly IRepository<NghiemThu, Guid> NghiemThu;
    private readonly IRepository<PheDuyetDuToan, Guid> PheDuyetDuToan;
    private readonly IRepository<PhuLucHopDong, Guid> PhuLucHopDong;
    private readonly IRepository<TamUng, Guid> TamUng;
    private readonly IRepository<ThanhToan, Guid> ThanhToan;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork UnitOfWork;

    public DanhMucBuocDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        DuAnBuoc = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        VanBanQuyetDinh = serviceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();
        GoiThau = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        HopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        BaoCaoTienDo = serviceProvider.GetRequiredService<IRepository<BaoCaoTienDo, Guid>>();
        DangTaiKeHoachLcntLenMang = serviceProvider.GetRequiredService<IRepository<DangTaiKeHoachLcntLenMang, Guid>>();
        KetQuaTrungThau = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        KhoKhanVuongMac = serviceProvider.GetRequiredService<IRepository<BaoCaoKhoKhanVuongMac, Guid>>();
        NghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        PheDuyetDuToan = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        PhuLucHopDong = serviceProvider.GetRequiredService<IRepository<PhuLucHopDong, Guid>>();
        TamUng = serviceProvider.GetRequiredService<IRepository<TamUng, Guid>>();
        ThanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        UnitOfWork = DanhMucBuoc.UnitOfWork;
    }

    public async Task<int> Handle(DanhMucBuocDeleteCommand request, CancellationToken cancellationToken)
    {
        using (await UnitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
        {
            var entity = await DanhMucBuoc.GetOriginalSet()
                .Include(e => e.DuAnBuocs)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            ManagedException.ThrowIfNull(entity);

            // bool hasData = await HasDataAsync(request, cancellationToken);
            // ManagedException.ThrowIf(hasData, "Bước đã có dữ liệu không thể xóa");

            entity.IsDeleted = true;

            var effected = await UnitOfWork.SaveChangesAsync(cancellationToken);
            await UnitOfWork.CommitTransactionAsync(cancellationToken);
            return effected;
        }
    }

    /// <summary>
    /// Mục đích kiểm tra xem bước này đã có dữ liệu chưa nếu có rồi thì không được phép xóa
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    // private async Task<bool> HasDataAsync(DanhMucBuocDeleteCommand request,
    //     CancellationToken cancellationToken) {
    //     var duAnBuocIds = await DuAnBuoc.GetQueryableSet().Where(e => e.BuocId == request.Id).Select(e => e.Id)
    //         .ToListAsync(cancellationToken);
    //     if (duAnBuocIds.Count == 0)
    //         return false;

    //     #region Văn bản quyết định

    //     /*
    //      * Với câu query dưới đã kiểm tra qua các bang sau
    //      * EnumLoaiVanBanQuyetDinh.QuyetDinhDuyetDuAn
    //      * EnumLoaiVanBanQuyetDinh.QuyetDinhDuyetKHLCNT
    //      * EnumLoaiVanBanQuyetDinh.QuyetDinhDuyetQuyetToan
    //      * EnumLoaiVanBanQuyetDinh.QuyetDinhLapBanQLDA
    //      * EnumLoaiVanBanQuyetDinh.QuyetDinhLapBenMoiThau
    //      * EnumLoaiVanBanQuyetDinh.QuyetDinhLapHoiDongThamDinh
    //      * EnumLoaiVanBanQuyetDinh.VanBanPhapLy
    //      * EnumLoaiVanBanQuyetDinh.VanBanChuTruong
    //      * EnumLoaiVanBanQuyetDinh.KeHoachLuaChonNhaThau
    //      */
    //     bool hasVanBanQuyetDinh = await VanBanQuyetDinh.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);

    //     #endregion

    //     bool hasGoiThau = await GoiThau.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);

    //     bool hasHopDong = await HopDong.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);

    //     bool hasBaoCaoTienDo = await BaoCaoTienDo.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);

    //     bool hasDangTaiKeHoachLcntLenMang = await DangTaiKeHoachLcntLenMang.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);

    //     bool hasKetQuaTrungThau = await KetQuaTrungThau.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);

    //     bool hasKhoKhanVuongMac = await KhoKhanVuongMac.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);

    //     bool hasNghiemThu = await NghiemThu.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);

    //     bool hasPheDuyetDuToan = await PheDuyetDuToan.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);

    //     bool hasPhuLucHopDong = await PhuLucHopDong.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);

    //     bool hasTamUng = await TamUng.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);

    //     bool hasThanhToan = await ThanhToan.GetQueryableSet()
    //         .AnyAsync(e => e.BuocId != null && duAnBuocIds.Contains(e.BuocId ?? 0), cancellationToken);
    //     bool hasDuAn = await DuAn.GetQueryableSet()
    //                 .AnyAsync(e => e.BuocHienTaiId != null && duAnBuocIds.Contains(e.BuocHienTaiId ?? 0), cancellationToken);
    //     return hasVanBanQuyetDinh
    //            || hasGoiThau
    //            || hasHopDong
    //            || hasBaoCaoTienDo
    //            || hasDangTaiKeHoachLcntLenMang
    //            || hasKetQuaTrungThau
    //            || hasKhoKhanVuongMac
    //            || hasNghiemThu
    //            || hasPheDuyetDuToan
    //            || hasPhuLucHopDong
    //            || hasTamUng
    //            || hasThanhToan
    //            || hasDuAn;
    // }
}