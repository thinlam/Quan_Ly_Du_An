using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Từ chối điều chỉnh
/// </summary>
public record QuyetDinhDieuChinhTuChoiCommand(Guid Id, string NoiDung) : IRequest<int>;

internal class QuyetDinhDieuChinhTuChoiCommandHandler : IRequestHandler<QuyetDinhDieuChinhTuChoiCommand, int> {
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDieuChinhTuChoiCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDieuChinhTuChoiCommand request, CancellationToken cancellationToken) {
        // Permission: LDDV role OR QLDA_QuanTri
        var hasPermission = _userProvider.AuthInfo.HasRole(Domain.Constants.RoleConstants.QLDA_LDDV)
                            || _userProvider.AuthInfo.HasRole(Domain.Constants.RoleConstants.QLDA_QuanTri);
        if (!hasPermission) {
            throw new ManagedException("Không có quyền từ chối điều chỉnh");
        }

        ManagedException.ThrowIfNull(request.NoiDung, "Phải nhập lý do từ chối");

        var trangThaiCTD = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "CTD" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);
        var trangThaiCPD = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "CPD" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);
        var trangThaiTC = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "TC" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiTC, "Không tìm thấy trạng thái 'Từ chối'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định điều chỉnh");

        // Can reject from CTD or CPD
        if (entity.TrangThaiId != trangThaiCTD?.Id && entity.TrangThaiId != trangThaiCPD?.Id) {
            throw new ManagedException("Chỉ có thể từ chối khi trạng thái là Chờ thẩm định hoặc Chờ duyệt");
        }

        entity.TrangThaiId = trangThaiTC.Id;

        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.QuyetDinhDieuChinh,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiTC.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}