using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Commands;

public record KeHoachTrienKhaiHangMucDeleteCommand(Guid Id) : IRequest<int>
{
}

public record KeHoachTrienKhaiHangMucDeleteCommandHandler : IRequestHandler<KeHoachTrienKhaiHangMucDeleteCommand, int>
{
    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> KeHoachTrienKhaiHangMuc;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiHangMucDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        KeHoachTrienKhaiHangMuc = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _unitOfWork = KeHoachTrienKhaiHangMuc.UnitOfWork;
    }

    public async Task<int> Handle(KeHoachTrienKhaiHangMucDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await KeHoachTrienKhaiHangMuc.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}