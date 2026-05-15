using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Thẩm định điều chỉnh - P.KH-TC thẩm định
/// </summary>
public record QuyetDinhDieuChinhThamDinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class QuyetDinhDieuChinhThamDinhCommandHandler : IRequestHandler<QuyetDinhDieuChinhThamDinhCommand, int> {
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDieuChinhThamDinhCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDieuChinhThamDinhCommand request, CancellationToken cancellationToken) {
        // Permission: KH-TC only (PhongBanId = 219)
        var phongBanId = _userProvider.Info.PhongBanID;
        if (phongBanId != 219) {
            throw new ManagedException("Chỉ phòng KH-TC có quyền thẩm định điều chỉnh");
        }

        var trangThaiCTD = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "CTD" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);
        var trangThaiDTD = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DTD" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDTD, "Không tìm thấy trạng thái 'Đã thẩm định'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định điều chỉnh");

        if (entity.TrangThaiId != trangThaiCTD?.Id) {
            throw new ManagedException("Chỉ có thể thẩm định khi trạng thái là Chờ thẩm định");
        }

        entity.TrangThaiId = trangThaiDTD.Id;

        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.QuyetDinhDieuChinh,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDTD.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}