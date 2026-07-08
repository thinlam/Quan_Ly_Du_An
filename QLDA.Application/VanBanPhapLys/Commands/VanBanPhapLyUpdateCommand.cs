using System.Data;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.VanBanPhapLys.DTOs;

namespace QLDA.Application.VanBanPhapLys.Commands;

public record VanBanPhapLyUpdateCommand(VanBanPhapLyUpdateDto Dto) : IRequest<VanBanPhapLy>;

internal class VanBanPhapLyUpdateCommandHandler : IRequestHandler<VanBanPhapLyUpdateCommand, VanBanPhapLy> {
    private readonly IRepository<VanBanPhapLy, Guid> VanBanPhapLy;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<DanhMucLoaiVanBan, int> DanhMucLoaiVanBan;
    private readonly IRepository<DanhMucChuDauTu, int> DanhMucChuDauTu;
    private readonly IRepository<DanhMucChucVu, int> DanhMucChucVu;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VanBanPhapLyUpdateCommandHandler> _logger;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public VanBanPhapLyUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<VanBanPhapLyUpdateCommandHandler> logger) {
        VanBanPhapLy = serviceProvider.GetRequiredService<IRepository<VanBanPhapLy, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        DanhMucLoaiVanBan = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiVanBan, int>>();
        DanhMucChuDauTu = serviceProvider.GetRequiredService<IRepository<DanhMucChuDauTu, int>>();
        DanhMucChucVu = serviceProvider.GetRequiredService<IRepository<DanhMucChucVu, int>>();
        _logger = logger;
        _unitOfWork = VanBanPhapLy.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<VanBanPhapLy> Handle(VanBanPhapLyUpdateCommand request, CancellationToken cancellationToken = default) {
        try {
            ManagedException.ThrowIf(request.Dto.LoaiVanBanId > 0 && !DanhMucLoaiVanBan.GetQueryableSet().Any(e => e.Id == request.Dto.LoaiVanBanId),
                "Không tồn tại loại văn bản này");
            ManagedException.ThrowIf(request.Dto.ChucVuId > 0 && !DanhMucChucVu.GetQueryableSet().Any(e => e.Id == request.Dto.ChucVuId),
                "Không tồn tại chức vụ này");

            var entity = request.Dto.ToEntity();

            await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await VanBanPhapLy.UpdateAsync(entity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }

            return entity;
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}