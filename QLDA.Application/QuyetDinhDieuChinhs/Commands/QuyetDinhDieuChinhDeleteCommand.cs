using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Xóa quyết định điều chỉnh - chỉ khi ở trạng thái Nháp
/// </summary>
public record QuyetDinhDieuChinhDeleteCommand(Guid Id) : IRequest<int>;

internal class QuyetDinhDieuChinhDeleteCommandHandler : IRequestHandler<QuyetDinhDieuChinhDeleteCommand, int> {
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDieuChinhDeleteCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDieuChinhDeleteCommand request, CancellationToken cancellationToken) {
        var trangThaiDDC = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DDC" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định điều chỉnh");

        // Validate: only allow delete when status is DDC
        if (entity.TrangThaiId != trangThaiDDC?.Id) {
            throw new ManagedException("Chỉ có thể xóa khi trạng thái là Nháp điều chỉnh");
        }

        entity.IsDeleted = true;

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}