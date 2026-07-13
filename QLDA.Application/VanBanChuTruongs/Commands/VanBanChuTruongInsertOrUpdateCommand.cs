using System.Data;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;

namespace QLDA.Application.VanBanChuTruongs.Commands;

public record VanBanChuTruongInsertOrUpdateCommand(VanBanChuTruong Entity) : IRequest {
}

internal class VanBanChuTruongInsertOrUpdateCommandHandler : IRequestHandler<VanBanChuTruongInsertOrUpdateCommand> {
    private readonly IRepository<VanBanChuTruong, Guid> VanBanChuTruong;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<DanhMucLoaiVanBan, int> DanhMucLoaiVanBan;
    private readonly IRepository<DanhMucChucVu, int> DanhMucChucVu;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VanBanChuTruongInsertOrUpdateCommandHandler> _logger;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public VanBanChuTruongInsertOrUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<VanBanChuTruongInsertOrUpdateCommandHandler> logger) {
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

    public async Task Handle(VanBanChuTruongInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default) {
        try {
            await _authManager.EnsureCanExecuteAsync(request.Entity.BuocId, request.Entity.DuAnId, _authContext, cancellationToken);
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.DuAnId),
                "Không tồn tại dự án");

            ManagedException.ThrowIf(request.Entity.LoaiVanBanId > 0 &&!DanhMucLoaiVanBan.GetQueryableSet().Any(e => e.Id == request.Entity.LoaiVanBanId),
                "Không tồn tại loại văn bản này");
            ManagedException.ThrowIf(request.Entity.ChucVuId > 0 &&!DanhMucChucVu.GetQueryableSet().Any(e => e.Id == request.Entity.ChucVuId),
                "Không tồn tại chức vụ này");
            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = VanBanChuTruong.GetQueryableSet().Any(o => o.Id == request.Entity.Id);
                if (isExist) {
                    await VanBanChuTruong.UpdateAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                } else {
                    //Thêm dự án trước
                    await VanBanChuTruong.AddAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}