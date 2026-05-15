using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Trình phê duyệt điều chỉnh - P.KH-TC gửi BGĐ
/// </summary>
public record QuyetDinhDieuChinhTrinhPheDuyetCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class QuyetDinhDieuChinhTrinhPheDuyetCommandHandler : IRequestHandler<QuyetDinhDieuChinhTrinhPheDuyetCommand, int> {
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDieuChinhTrinhPheDuyetCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDieuChinhTrinhPheDuyetCommand request, CancellationToken cancellationToken) {
        // Permission: KH-TC only (PhongBanId = 219)
        var phongBanId = _userProvider.Info.PhongBanID;
        if (phongBanId != 219) {
            throw new ManagedException("Chỉ phòng KH-TC có quyền trình phê duyệt điều chỉnh");
        }

        var trangThaiDTD = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DTD" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);
        var trangThaiCPD = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "CPD" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiCPD, "Không tìm thấy trạng thái 'Chờ duyệt'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định điều chỉnh");

        if (entity.TrangThaiId != trangThaiDTD?.Id) {
            throw new ManagedException("Chỉ có thể trình phê duyệt khi trạng thái là Đã thẩm định");
        }

        entity.TrangThaiId = trangThaiCPD.Id;

        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.QuyetDinhDieuChinh,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiCPD.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}