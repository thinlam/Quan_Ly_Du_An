using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

public record ToTrinhPheDuyetDeleteCommand(Guid Id) : IRequest<int>
{
}

public record ToTrinhPheDuyetDeleteCommandHandler : IRequestHandler<ToTrinhPheDuyetDeleteCommand, int>
{
    private readonly IRepository<ToTrinhPheDuyet, Guid> ToTrinhPheDuyet;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhPheDuyetDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        ToTrinhPheDuyet = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = ToTrinhPheDuyet.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhPheDuyetDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await ToTrinhPheDuyet.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        bool isKhongDuyet = LoaiToTrinhKhongDuyetExtensions.ContainsDescription(entity.Loai);

        var loaiPheDuyet = isKhongDuyet ? PheDuyetEntityNames.ToTrinhKhongDuyet : PheDuyetEntityNames.DeXuatMacDinhStt;
        var statuses = await _statusRepo.GetByLoaiAsync(loaiPheDuyet, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDaDuyet = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaDuyet);
        if(entity.TrangThaiId== trangThaiDaDuyet?.Id)
        {
            throw new ManagedException("Không thể xóa khi đã duyệt");
        }
        ManagedException.ThrowIfNull(entity);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}