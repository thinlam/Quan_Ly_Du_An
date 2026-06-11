using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAns.Commands;

public record KeHoachTrienKhaiChiTietDuAnDeleteCommand(Guid Id) : IRequest<int>
{
}

public record KeHoachTrienKhaiChiTietDuAnDeleteCommandHandler : IRequestHandler<KeHoachTrienKhaiChiTietDuAnDeleteCommand, int>
{
    private readonly IRepository<KeHoachTrienKhaiChiTietDuAn, Guid> KeHoachTrienKhaiChiTietDuAn;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiChiTietDuAnDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        KeHoachTrienKhaiChiTietDuAn = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiChiTietDuAn, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = KeHoachTrienKhaiChiTietDuAn.UnitOfWork;
    }

    public async Task<int> Handle(KeHoachTrienKhaiChiTietDuAnDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await KeHoachTrienKhaiChiTietDuAn.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}