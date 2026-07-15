using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.DuAnBuocs.Commands;

/// <summary>
/// Map phòng ban phụ trách chính và danh sách phòng ban phối hợp từ DuAn
/// sang tất cả các DuAnBuoc đã được clone từ quy trình.
/// </summary>
/// <remarks>
/// Được gọi sau <see cref="DuAnBuocCloneCommand"/> trong luồng thêm mới dự án.
/// </remarks>
public record DuAnBuocMapPhongBanCommand(
    Guid DuAnId,
    long? PhongBanPhuTrachChinhId,
    List<long> PhongBanPhoiHopIds
) : IRequest;

internal class DuAnBuocMapPhongBanCommandHandler : IRequestHandler<DuAnBuocMapPhongBanCommand> {
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DuAnBuocMapPhongBanCommandHandler(IServiceProvider serviceProvider) {
        _duAnBuocRepository = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _unitOfWork = _duAnBuocRepository.UnitOfWork;
    }

    public async Task Handle(DuAnBuocMapPhongBanCommand request, CancellationToken cancellationToken) {
        // Lấy tất cả DuAnBuoc hiện tại của dự án (kèm collection DuAnBuocPhongBanPhoiHops để replace)
        var buocs = await _duAnBuocRepository.GetQueryableSet()
            .Include(b => b.DuAnBuocPhongBanPhoiHops)
            .Where(b => b.DuAnId == request.DuAnId)
            .ToListAsync(cancellationToken);

        if (buocs.Count == 0) return;

        foreach (var buoc in buocs) {
            // (1) Gán phòng ban phụ trách chính cho bước
            buoc.PhongPhuTrachChinhId = request.PhongBanPhuTrachChinhId;

            // (2) Replace collection DuAnBuocPhongBanPhoiHops
            buoc.DuAnBuocPhongBanPhoiHops?.Clear();
            if (request.PhongBanPhoiHopIds != null && request.PhongBanPhoiHopIds.Count > 0) {
                foreach (var phongBanId in request.PhongBanPhoiHopIds) {
                    buoc.DuAnBuocPhongBanPhoiHops!.Add(new DuAnBuocPhongBanPhoiHop {
                        LeftId = buoc.Id,
                        RightId = phongBanId
                    });
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
