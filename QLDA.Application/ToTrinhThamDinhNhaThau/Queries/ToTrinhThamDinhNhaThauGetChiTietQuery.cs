using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Queries;

/// <summary>
/// Kết quả chi-tiet — kèm Tờ trình kết quả (<see cref="ToTrinhQuyetDinh"/>) và
/// Quyết định phê duyệt (<see cref="VanBanQuyetDinh"/>) liên kết với Tờ trình (Issue #179).
/// </summary>
public record ToTrinhThamDinhNhaThauChiTietResult(
    ToTrinhThamDinhNhaThau Entity,
    ToTrinhQuyetDinh? ToTrinhKetQua,
    VanBanQuyetDinh? QuyetDinhPheDuyet);

public record ToTrinhThamDinhNhaThauGetChiTietQuery(Guid Id)
    : IRequest<ToTrinhThamDinhNhaThauChiTietResult>;

internal class ToTrinhThamDinhNhaThauGetChiTietQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<ToTrinhThamDinhNhaThauGetChiTietQuery, ToTrinhThamDinhNhaThauChiTietResult> {
    private readonly IRepository<ToTrinhThamDinhNhaThau, Guid> _repo =
        serviceProvider.GetRequiredService<IRepository<ToTrinhThamDinhNhaThau, Guid>>();
    private readonly IRepository<ToTrinhQuyetDinh, long> _toTrinhQuyetDinhRepo =
        serviceProvider.GetRequiredService<IRepository<ToTrinhQuyetDinh, long>>();
    private readonly IRepository<VanBanQuyetDinh, Guid> _vanBanQuyetDinhRepo =
        serviceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();

    public async Task<ToTrinhThamDinhNhaThauChiTietResult> Handle(
        ToTrinhThamDinhNhaThauGetChiTietQuery request,
        CancellationToken cancellationToken = default) {
        var entity = await _repo.GetOrderedSet()
            .Include(e => e.BuocXuLys)
            .Where(e => e.Id == request.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu");

        var toTrinhKetQua = await _toTrinhQuyetDinhRepo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.EntityId == request.Id
                && e.Loai == ToTrinhQuyetDinhLoai.ToTrinhThamDinhNhaThau, cancellationToken);

        var quyetDinh = await _vanBanQuyetDinhRepo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id
                && e.Loai == nameof(EnumLoaiVanBanQuyetDinh.ToTrinhThamDinhNhaThau), cancellationToken);

        return new ToTrinhThamDinhNhaThauChiTietResult(entity!, toTrinhKetQua, quyetDinh);
    }
}