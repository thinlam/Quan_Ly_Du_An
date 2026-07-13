using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.QuyetDinhLapHoiDongThamDinhs.Commands;

public record QuyetDinhLapHoiDongThamDinhDeleteCommand(Guid Id) : IRequest<int> {
}

public record QuyetDinhLapHoiDongThamDinhDeleteCommandHandler : IRequestHandler<QuyetDinhLapHoiDongThamDinhDeleteCommand, int> {
    private readonly IRepository<QuyetDinhLapHoiDongThamDinh, Guid> QuyetDinhLapHoiDongThamDinh;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhLapHoiDongThamDinhDeleteCommandHandler(IServiceProvider serviceProvider) {
        QuyetDinhLapHoiDongThamDinh = serviceProvider.GetRequiredService<IRepository<QuyetDinhLapHoiDongThamDinh, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = QuyetDinhLapHoiDongThamDinh.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhLapHoiDongThamDinhDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await QuyetDinhLapHoiDongThamDinh.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}