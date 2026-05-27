using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

public record ToTrinhPheDuyetDeleteCommand(Guid Id) : IRequest<int>
{
}

public record ToTrinhPheDuyetDeleteCommandHandler : IRequestHandler<ToTrinhPheDuyetDeleteCommand, int>
{
    private readonly IRepository<ToTrinhPheDuyet, Guid> ToTrinhPheDuyet;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhPheDuyetDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        ToTrinhPheDuyet = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = ToTrinhPheDuyet.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhPheDuyetDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await ToTrinhPheDuyet.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}