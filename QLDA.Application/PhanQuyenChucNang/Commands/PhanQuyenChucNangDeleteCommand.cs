using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Entities;

namespace QLDA.Application.PhanQuyenChucNangs.Commands;

public record PhanQuyenChucNangDeleteCommand(int Id) : IRequest<int> {
}

public record PhanQuyenChucNangDeleteCommandHandler : IRequestHandler<PhanQuyenChucNangDeleteCommand, int> {
    private readonly IRepository<PhanQuyenChucNang, int> PhanQuyenChucNang;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public PhanQuyenChucNangDeleteCommandHandler(IServiceProvider serviceProvider) {
        PhanQuyenChucNang = serviceProvider.GetRequiredService<IRepository<PhanQuyenChucNang, int>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = PhanQuyenChucNang.UnitOfWork;
    }

    public async Task<int> Handle(PhanQuyenChucNangDeleteCommand request, CancellationToken cancellationToken) {
        var entity = await PhanQuyenChucNang.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

      
        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}