using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;

namespace QLDA.Application.TrienKhaiKeHoachLCNTs.Commands;

public record TrienKhaiKeHoachLCNTDeleteCommand(Guid Id) : IRequest<int>
{
}

public record TrienKhaiKeHoachLCNTDeleteCommandHandler : IRequestHandler<TrienKhaiKeHoachLCNTDeleteCommand, int>
{
    private readonly IRepository<TrienKhaiKeHoachLCNT, Guid> TrienKhaiKeHoachLCNT;
    private readonly IRepository<Attachment, Guid> TepDinhKem;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;

    public TrienKhaiKeHoachLCNTDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        TrienKhaiKeHoachLCNT = serviceProvider.GetRequiredService<IRepository<TrienKhaiKeHoachLCNT, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = TrienKhaiKeHoachLCNT.UnitOfWork;
    }

    public async Task<int> Handle(TrienKhaiKeHoachLCNTDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await TrienKhaiKeHoachLCNT.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
          .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id)
        {
            throw new ManagedException("Tờ trình đang ở trạng thái không thể xóa!");
        }

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}