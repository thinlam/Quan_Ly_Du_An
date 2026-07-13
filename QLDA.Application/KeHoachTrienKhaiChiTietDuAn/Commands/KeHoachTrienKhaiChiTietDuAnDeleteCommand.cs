using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Entities;

namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAns.Commands;

public record KeHoachTrienKhaiChiTietDuAnDeleteCommand(Guid Id) : IRequest<int>
{
}

public record KeHoachTrienKhaiChiTietDuAnDeleteCommandHandler : IRequestHandler<KeHoachTrienKhaiChiTietDuAnDeleteCommand, int>
{
    private readonly IRepository<KeHoachTrienKhaiChiTietDuAn, Guid> KeHoachTrienKhaiChiTietDuAn;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiChiTietDuAnDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        KeHoachTrienKhaiChiTietDuAn = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiChiTietDuAn, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = KeHoachTrienKhaiChiTietDuAn.UnitOfWork;
    }

    public async Task<int> Handle(KeHoachTrienKhaiChiTietDuAnDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await KeHoachTrienKhaiChiTietDuAn.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        // Check step authorization
        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);
        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
