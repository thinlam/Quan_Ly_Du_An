using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Trình điều chỉnh - CB.PCT gửi thẩm định
/// </summary>
public record QuyetDinhDieuChinhTrinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class QuyetDinhDieuChinhTrinhCommandHandler : IRequestHandler<QuyetDinhDieuChinhTrinhCommand, int> {
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDieuChinhTrinhCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDieuChinhTrinhCommand request, CancellationToken cancellationToken) {
        // Permission: KH-TC only (PhongBanId = 219)
        var phongBanId = _userProvider.Info.PhongBanID;
        if (phongBanId != 219) {
            throw new ManagedException("Chỉ phòng KH-TC có quyền trình điều chỉnh");
        }

        var trangThaiDDC = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DDC" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);
        var trangThaiTL = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "TL" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);
        var trangThaiCTD = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "CTD" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiCTD, "Không tìm thấy trạng thái 'Chờ thẩm định'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định điều chỉnh");

        // Validate: must be DDC or TL
        if (entity.TrangThaiId != trangThaiDDC?.Id && entity.TrangThaiId != trangThaiTL?.Id) {
            throw new ManagedException("Chỉ có thể trình khi trạng thái là Nháp điều chỉnh hoặc Trả lại");
        }

        entity.TrangThaiId = trangThaiCTD.Id;

        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.QuyetDinhDieuChinh,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiCTD.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}