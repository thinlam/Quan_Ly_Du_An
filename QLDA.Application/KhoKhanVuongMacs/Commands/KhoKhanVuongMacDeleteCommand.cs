using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.KhoKhanVuongMacs.Commands;

public record KhoKhanVuongMacDeleteCommand(Guid Id) : IRequest<int>
{
}

public record KhoKhanVuongMacDeleteCommandHandler : IRequestHandler<KhoKhanVuongMacDeleteCommand, int>
{
    private readonly IRepository<BaoCaoKhoKhanVuongMac, Guid> KhoKhanVuongMac;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public KhoKhanVuongMacDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        KhoKhanVuongMac =serviceProvider.GetRequiredService<IRepository<BaoCaoKhoKhanVuongMac, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = KhoKhanVuongMac.UnitOfWork;
    }

    public async Task<int> Handle(KhoKhanVuongMacDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await KhoKhanVuongMac.GetOrderedSet()
            // .Include(o => o.DanhSachToTrinh)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        // Phân quyền: chỉ Owner/LanhDao/KHTC mới được xóa
        await _auth.EnsureCanManageStepFieldsAsync(entity.BuocId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}