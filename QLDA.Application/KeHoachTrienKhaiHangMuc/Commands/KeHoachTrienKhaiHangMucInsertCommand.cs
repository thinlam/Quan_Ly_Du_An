using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Constants;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Commands;

public record KeHoachTrienKhaiHangMucInsertCommand(KeHoachTrienKhaiHangMuc entity) : IRequest<KeHoachTrienKhaiHangMuc>;

internal class KeHoachTrienKhaiHangMucInsertCommandHandler : IRequestHandler<KeHoachTrienKhaiHangMucInsertCommand, KeHoachTrienKhaiHangMuc> {
    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiHangMucInsertCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<KeHoachTrienKhaiHangMuc> Handle(KeHoachTrienKhaiHangMucInsertCommand request,
        CancellationToken cancellationToken = default) {
        await _auth.EnsureCanExecuteStepAsync(request.entity.BuocId, _authContext, cancellationToken);
        await _authManager.EnsureCanExecuteAsync(request.entity.BuocId, request.entity.DuAnId, _authContext, cancellationToken);

        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        // TẠI QUERY HANDLER: Phải Include danh sách này lên trước khi gọi Sync

        var entity = request.entity;
        entity.TrangThaiId = trangThaiDuThao?.Id;
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity!;
    }
}
