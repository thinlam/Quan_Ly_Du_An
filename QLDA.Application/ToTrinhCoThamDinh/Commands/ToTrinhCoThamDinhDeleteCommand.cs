using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhCoThamDinhs.Commands;

public record ToTrinhCoThamDinhDeleteCommand(Guid Id) : IRequest<int>
{
}

public record ToTrinhCoThamDinhDeleteCommandHandler : IRequestHandler<ToTrinhCoThamDinhDeleteCommand, int>
{
    private readonly IRepository<ToTrinhCoThamDinh, Guid> ToTrinhCoThamDinh;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;

    public ToTrinhCoThamDinhDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        ToTrinhCoThamDinh = serviceProvider.GetRequiredService<IRepository<ToTrinhCoThamDinh, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = ToTrinhCoThamDinh.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhCoThamDinhDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await ToTrinhCoThamDinh.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
         .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.ToTrinhCoThamDinh.DuThao && s.Loai == PheDuyetEntityNames.QuyetDinhKeHoachThue, cancellationToken);

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id)
            throw new ManagedException("Tờ trình đang ở trạng thái không thể xóa!");

        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}