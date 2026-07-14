using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.BaoCaoTienDos.Commands;

public record BaoCaoTienDoDeleteCommand(Guid Id) : IRequest<int>
{
}

public record BaoCaoTienDoDeleteCommandHandler : IRequestHandler<BaoCaoTienDoDeleteCommand, int>
{
    private readonly IRepository<BaoCaoTienDo, Guid> BaoCaoTienDo;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoTienDoDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        BaoCaoTienDo =serviceProvider.GetRequiredService<IRepository<BaoCaoTienDo, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _unitOfWork = BaoCaoTienDo.UnitOfWork;
    }

    public async Task<int> Handle(BaoCaoTienDoDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await BaoCaoTienDo.GetOrderedSet()
            // .Include(o => o.DanhSachToTrinh)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);
        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}