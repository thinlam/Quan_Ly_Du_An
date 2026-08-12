using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Commands;

/// <summary>
/// Duyệt Quyết định phê duyệt (VanBanQuyetDinh) của Tờ trình thẩm định nhà thầu — Issue #179.
/// Độc lập với <c>ToTrinhThamDinhNhaThauDuyetCommand</c> (duyệt bản thân Tờ trình) vì đây là
/// 2 trạng thái khác nhau trên 2 entity khác nhau.
/// </summary>
public record ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand(Guid VanBanQuyetDinhId) : IRequest<int>;

internal class ToTrinhThamDinhNhaThauDuyetQuyetDinhCommandHandler
    : IRequestHandler<ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand, int> {
    private readonly IRepository<VanBanQuyetDinh, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhThamDinhNhaThauDuyetQuyetDinhCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhThamDinhNhaThauDuyetQuyetDinhCommand request, CancellationToken cancellationToken) {
        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.VanBanQuyetDinhId
                && e.Loai == nameof(EnumLoaiVanBanQuyetDinh.ToTrinhThamDinhNhaThau), cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy Quyết định phê duyệt của Tờ trình thẩm định nhà thầu");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        var trangThaiChoDuyet = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.ToTrinhThamDinhNhaThauQuyetDinh.ChoDuyet
                && s.Loai == PheDuyetEntityNames.ToTrinhThamDinhNhaThau, cancellationToken);
        var trangThaiDaDuyet = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.ToTrinhThamDinhNhaThauQuyetDinh.DaDuyet
                && s.Loai == PheDuyetEntityNames.ToTrinhThamDinhNhaThau, cancellationToken);
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'");

        if (entity.TrangThaiDuyetId != trangThaiChoDuyet?.Id) {
            throw new ManagedException("Chỉ có thể duyệt Quyết định khi đang ở trạng thái Chờ duyệt");
        }

        // TrangThai.Ma = "ĐD" → xuất hiện trong api/tong-hop-van-ban-quyet-dinh/danh-sach-day-du.
        entity.TrangThaiDuyetId = trangThaiDaDuyet!.Id;

        await _repo.UpdateAsync(entity, cancellationToken);
        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
