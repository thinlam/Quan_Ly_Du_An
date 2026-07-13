using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.PheDuyetDuToans.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.PheDuyetDuToans.Commands;

public record PheDuyetDuToanUpdateCommand(PheDuyetDuToanUpdateDto Dto) : IRequest<PheDuyetDuToan>;

internal class PheDuyetDuToanUpdateCommandHandler : IRequestHandler<PheDuyetDuToanUpdateCommand, PheDuyetDuToan> {
    private readonly IRepository<PheDuyetDuToan, Guid> PheDuyetDuToan;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<DanhMucChucVu, int> DanhMucChucVu;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PheDuyetDuToanUpdateCommandHandler> _logger;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public PheDuyetDuToanUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<PheDuyetDuToanUpdateCommandHandler> logger) {
        PheDuyetDuToan = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        DanhMucChucVu = serviceProvider.GetRequiredService<IRepository<DanhMucChucVu, int>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _logger = logger;
        _unitOfWork = PheDuyetDuToan.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<PheDuyetDuToan> Handle(PheDuyetDuToanUpdateCommand request, CancellationToken cancellationToken = default) {
        try {
            var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
                .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DuToan.DuThao && s.Loai == PheDuyetEntityNames.PheDuyetDuToan, cancellationToken);

            var entity = await PheDuyetDuToan.GetQueryableSet()
                .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
            ManagedException.ThrowIfNull(entity, "Không tìm thấy phê duyệt dự toán");

            await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

            // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
            if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThai?.Ma != "LEG") {
                throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là Dự thảo");
            }

            ManagedException.ThrowIf(
                request.Dto.ChucVuId > 0 &&
                !DanhMucChucVu.GetQueryableSet().Any(e => e.Id == request.Dto.ChucVuId),
                "Không tồn tại chức vụ này");

            var updated = request.Dto.ToEntity();

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await PheDuyetDuToan.UpdateAsync(updated, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }

            return updated;
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}