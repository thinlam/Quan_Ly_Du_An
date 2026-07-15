using System.Data;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.VanBanChuTruongs.DTOs;

namespace QLDA.Application.VanBanChuTruongs.Commands;

public record VanBanChuTruongInsertCommand(VanBanChuTruongInsertDto Dto) : IRequest<VanBanChuTruong>;

internal class VanBanChuTruongInsertCommandHandler : IRequestHandler<VanBanChuTruongInsertCommand, VanBanChuTruong> {
    private readonly IRepository<VanBanChuTruong, Guid> VanBanChuTruong;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<DanhMucLoaiVanBan, int> DanhMucLoaiVanBan;
    private readonly IRepository<DanhMucChucVu, int> DanhMucChucVu;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VanBanChuTruongInsertCommandHandler> _logger;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public VanBanChuTruongInsertCommandHandler(IServiceProvider serviceProvider,
        ILogger<VanBanChuTruongInsertCommandHandler> logger) {
        VanBanChuTruong = serviceProvider.GetRequiredService<IRepository<VanBanChuTruong, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        DanhMucLoaiVanBan = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiVanBan, int>>();
        DanhMucChucVu = serviceProvider.GetRequiredService<IRepository<DanhMucChucVu, int>>();
        _logger = logger;
        _unitOfWork = VanBanChuTruong.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<VanBanChuTruong> Handle(VanBanChuTruongInsertCommand request, CancellationToken cancellationToken = default) {
        try {
            await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Dto.DuAnId),
                "Không tồn tại dự án");

            ManagedException.ThrowIf(request.Dto.LoaiVanBanId > 0 && !DanhMucLoaiVanBan.GetQueryableSet().Any(e => e.Id == request.Dto.LoaiVanBanId),
                "Không tồn tại loại văn bản này");
            ManagedException.ThrowIf(request.Dto.ChucVuId > 0 && !DanhMucChucVu.GetQueryableSet().Any(e => e.Id == request.Dto.ChucVuId),
                "Không tồn tại chức vụ này");

            var entity = request.Dto.ToEntity();

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await VanBanChuTruong.AddAsync(entity, cancellationToken);
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