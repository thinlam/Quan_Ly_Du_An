using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.ToTrinhKetQuaGoiThaus.Commands;

public record ToTrinhKetQuaGoiThauDeleteCommand(Guid Id) : IRequest<int>
{
}

public record ToTrinhKetQuaGoiThauDeleteCommandHandler : IRequestHandler<ToTrinhKetQuaGoiThauDeleteCommand, int>
{
    private readonly IRepository<ToTrinhKetQuaGoiThau, Guid> ToTrinhKetQuaGoiThau;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhKetQuaGoiThauDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        ToTrinhKetQuaGoiThau = serviceProvider.GetRequiredService<IRepository<ToTrinhKetQuaGoiThau, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = ToTrinhKetQuaGoiThau.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhKetQuaGoiThauDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await ToTrinhKetQuaGoiThau.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}