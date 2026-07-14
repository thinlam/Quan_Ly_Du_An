using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;

namespace QLDA.Application.ThanhLyHopDongs.Commands;

public record ThanhLyHopDongDeleteCommand(Guid Id) : IRequest;

internal class ThanhLyHopDongDeleteCommandHandler : IRequestHandler<ThanhLyHopDongDeleteCommand> {
    private readonly IRepository<ThanhLyHopDong, Guid> _thanhLy;
    private readonly IRepository<Attachment, Guid> _tepDinhKem;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ThanhLyHopDongDeleteCommandHandler(IServiceProvider serviceProvider) {
        _thanhLy = serviceProvider.GetRequiredService<IRepository<ThanhLyHopDong, Guid>>();
        _tepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _thanhLy.UnitOfWork;
    }

    public async Task Handle(ThanhLyHopDongDeleteCommand request, CancellationToken cancellationToken) {
        // Load entity with junction navigation — EF tracks it for cascade delete
        var entity = await _thanhLy.GetQueryableSet()
            .Include(e => e.DanhSachNghiemThus)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        // Status guard: chỉ cho phép xóa khi status = Dự thảo (UC63)
        var trangThaiDuThao = (await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.ThanhLyHopDong, cancellationToken))
            .FirstOrDefault(s => s.Ma == TrangThaiPheDuyetCodes.ThanhLyHopDong.DuThao);
        if (entity.TrangThaiId != trangThaiDuThao?.Id)
        {
            throw new ManagedException("Chỉ có thể xóa khi trạng thái là Dự thảo");
        }

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(_tepDinhKem, [entity.Id.ToString()], cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
