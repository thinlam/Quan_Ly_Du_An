using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;

namespace QLDA.Application.DuAnBuocs.Commands;

public record DuAnBuocDeleteCommand(int Id) : IRequest;

public record DuAnBuocDeleteCommandHandler : IRequestHandler<DuAnBuocDeleteCommand> {
    private readonly IRepository<DuAnBuoc, int> DuAnBuoc;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public DuAnBuocDeleteCommandHandler(IServiceProvider serviceProvider) {
        DuAnBuoc = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = DuAnBuoc.UnitOfWork;
    }

    public async Task Handle(DuAnBuocDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await DuAnBuoc.GetQueryableSet()
            .Include(e => e.DuAn)
            .Include(e => e.DuAnBuocPhongBanPhoiHops)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        if (!await _auth.CanExecuteStepAsync(entity, _authContext, cancellationToken))
            throw new ManagedException("Chỉ Lãnh đạo phụ trách hoặc người tạo bước mới được xóa bước");

        entity.IsDeleted = true;
        await DuAnBuoc.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
