using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuyetDinhDuyetDuToans.Commands;

public record QuyetDinhDuyetDuToanDeleteCommand(Guid Id) : IRequest<int>
{
}

public record QuyetDinhDuyetDuToanDeleteCommandHandler : IRequestHandler<QuyetDinhDuyetDuToanDeleteCommand, int>
{
    private readonly IRepository<QuyetDinhDuyetDuToan, Guid> QuyetDinhDuyetDuToan;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDuyetDuToanDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        QuyetDinhDuyetDuToan = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuToan, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = QuyetDinhDuyetDuToan.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDuyetDuToanDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await QuyetDinhDuyetDuToan.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
                .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao
                && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        ManagedException.ThrowIfNull(trangThaiDuThao, "Trạng thái không thể xóa!");

        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}