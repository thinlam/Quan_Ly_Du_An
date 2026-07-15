using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.KeHoachLuaChonNhaThaus.Commands;

public record KeHoachLuaChonNhaThauDeleteCommand(Guid Id) : IRequest {
}

public record KeHoachLuaChonNhaThauDeleteCommandHandler : IRequestHandler<KeHoachLuaChonNhaThauDeleteCommand> {
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> KeHoachLuaChonNhaThau;
    private readonly IRepository<GoiThau, Guid> GoiThau;
    private readonly IRepository<QuyetDinhDuyetKHLCNT, Guid> QuyetDinhDuyetKHLCNT;
    private readonly IRepository<DangTaiKeHoachLcntLenMang, Guid> DangTaiKeHoachLcntLenMang;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public KeHoachLuaChonNhaThauDeleteCommandHandler(IServiceProvider serviceProvider) {
        KeHoachLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
        GoiThau = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        QuyetDinhDuyetKHLCNT = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetKHLCNT, Guid>>();
        DangTaiKeHoachLcntLenMang = serviceProvider.GetRequiredService<IRepository<DangTaiKeHoachLcntLenMang, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _unitOfWork = KeHoachLuaChonNhaThau.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task Handle(KeHoachLuaChonNhaThauDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await KeHoachLuaChonNhaThau.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        await ValidateAsync(request, cancellationToken);

        await RemoveAsync(entity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    #region  Private helper methods

    private async Task ValidateAsync(KeHoachLuaChonNhaThauDeleteCommand request, CancellationToken cancellationToken) {
        var hasGoiThau = await GoiThau.GetQueryableSet().AnyAsync(e => e.KeHoachLuaChonNhaThauId == request.Id && e.KeHoachLuaChonNhaThau != null && !e.KeHoachLuaChonNhaThau.IsDeleted, cancellationToken);
        var hasQuyetDinhDuyetKHLCNT = await QuyetDinhDuyetKHLCNT.GetQueryableSet().AnyAsync(e => e.KeHoachLuaChonNhaThauId == request.Id && e.KeHoachLuaChonNhaThau != null && !e.KeHoachLuaChonNhaThau.IsDeleted, cancellationToken);
        var hasDangTaiKeHoachLcntLenMang = await DangTaiKeHoachLcntLenMang.GetQueryableSet().AnyAsync(e => e.KeHoachLuaChonNhaThauId == request.Id && e.KeHoachLuaChonNhaThau != null && !e.KeHoachLuaChonNhaThau.IsDeleted, cancellationToken);

        ManagedException.ThrowIf(
            when: hasGoiThau,
            message: "Kế hoạch lựa chọn nhà thầu đã có gói thầu không thể xoá!"
        );
        ManagedException.ThrowIf(
            when: hasQuyetDinhDuyetKHLCNT,
            message: "Kế hoạch lựa chọn nhà thầu đã duyệt không thể xoá!"
        );
        ManagedException.ThrowIf(
            when: hasDangTaiKeHoachLcntLenMang,
            message: "Kế hoạch lựa chọn nhà thầu đã đăng tải lên mạng không thể xoá!"
        );
    }

    private async Task RemoveAsync(KeHoachLuaChonNhaThau entity, CancellationToken cancellationToken) {

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);
    }

    #endregion
}