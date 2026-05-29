using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.TrienKhaiKeHoachLCNTs.Commands;

public record TrienKhaiKeHoachLCNTDeleteCommand(Guid Id) : IRequest<int>
{
}

public record TrienKhaiKeHoachLCNTDeleteCommandHandler : IRequestHandler<TrienKhaiKeHoachLCNTDeleteCommand, int>
{
    private readonly IRepository<TrienKhaiKeHoachLCNT, Guid> TrienKhaiKeHoachLCNT;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public TrienKhaiKeHoachLCNTDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        TrienKhaiKeHoachLCNT = serviceProvider.GetRequiredService<IRepository<TrienKhaiKeHoachLCNT, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = TrienKhaiKeHoachLCNT.UnitOfWork;
    }

    public async Task<int> Handle(TrienKhaiKeHoachLCNTDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await TrienKhaiKeHoachLCNT.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}