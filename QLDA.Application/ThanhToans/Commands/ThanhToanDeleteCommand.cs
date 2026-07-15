using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.ThanhToans.Commands;

public record ThanhToanDeleteCommand(Guid Id) : IRequest;

public record ThanhToanDeleteCommandHandler : IRequestHandler<ThanhToanDeleteCommand>
{
    private readonly IRepository<ThanhToan, Guid> ThanhToan;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ThanhToanDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        ThanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = ThanhToan.UnitOfWork;
    }

    public async Task Handle(ThanhToanDeleteCommand request, CancellationToken cancellationToken)
    {
        ManagedException.ThrowIf(
            !_authContext.HasKhtcBypass,
            "Chỉ Phòng Kế Hoạch - Tài chính có quyền thực hiện thao tác này"
        );
        var entity = await ThanhToan.GetOrderedSet()
           .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}