using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Trả lại điều chỉnh - BGĐ trả lại
/// </summary>
public record QuyetDinhDieuChinhTraLaiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class QuyetDinhDieuChinhTraLaiCommandHandler : IRequestHandler<QuyetDinhDieuChinhTraLaiCommand, int> {
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDieuChinhTraLaiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDieuChinhTraLaiCommand request, CancellationToken cancellationToken) {
        if (!_userProvider.AuthInfo.HasRole(Domain.Constants.RoleConstants.QLDA_LDDV)) {
            throw new ManagedException("Chỉ Lãnh đạo đơn vị có quyền trả lại điều chỉnh");
        }

        ManagedException.ThrowIfNull(request.NoiDung, "Phải nhập lý do trả lại");

        var trangThaiCPD = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "CPD" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);
        var trangThaiTL = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "TL" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiTL, "Không tìm thấy trạng thái 'Trả lại'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định điều chỉnh");

        if (entity.TrangThaiId != trangThaiCPD?.Id) {
            throw new ManagedException("Chỉ có thể trả lại khi trạng thái là Chờ duyệt");
        }

        entity.TrangThaiId = trangThaiTL.Id;

        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.QuyetDinhDieuChinh,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTL.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}