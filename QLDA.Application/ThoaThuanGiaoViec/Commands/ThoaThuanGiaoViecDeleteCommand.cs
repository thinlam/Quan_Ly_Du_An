using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;

namespace QLDA.Application.ThoaThuanGiaoViecs.Commands;

public record ThoaThuanGiaoViecDeleteCommand(Guid Id) : IRequest<int>
{
}

public record ThoaThuanGiaoViecDeleteCommandHandler : IRequestHandler<ThoaThuanGiaoViecDeleteCommand, int>
{
    private readonly IRepository<ThoaThuanGiaoViec, Guid> ThoaThuanGiaoViec;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ThoaThuanGiaoViecDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        ThoaThuanGiaoViec = serviceProvider.GetRequiredService<IRepository<ThoaThuanGiaoViec, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = ThoaThuanGiaoViec.UnitOfWork;
    }

    public async Task<int> Handle(ThoaThuanGiaoViecDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await ThoaThuanGiaoViec.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
                .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        ManagedException.ThrowIfNull(trangThaiDuThao, "Trạng thái không thể xóa!");

        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}