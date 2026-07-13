using System.Data;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;

namespace QLDA.Application.KhoKhanVuongMacs.Commands;

public record KhoKhanVuongMacInsertOrUpdateCommand(BaoCaoKhoKhanVuongMac Dto) : IRequest {
}

internal class KhoKhanVuongMacInsertOrUpdateCommandHandler : IRequestHandler<KhoKhanVuongMacInsertOrUpdateCommand> {
    private readonly IRepository<BaoCaoKhoKhanVuongMac, Guid> KhoKhanVuongMac;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<DanhMucLoaiVanBan, int> DanhMucLoaiVanBan;
    private readonly IRepository<DanhMucChuDauTu, int> DanhMucChuDauTu;
    private readonly IRepository<DanhMucChucVu, int> DanhMucChucVu;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<KhoKhanVuongMacInsertOrUpdateCommandHandler> _logger;

    public KhoKhanVuongMacInsertOrUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<KhoKhanVuongMacInsertOrUpdateCommandHandler> logger) {
        KhoKhanVuongMac = serviceProvider.GetRequiredService<IRepository<BaoCaoKhoKhanVuongMac, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        DanhMucLoaiVanBan = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiVanBan, int>>();
        DanhMucChuDauTu = serviceProvider.GetRequiredService<IRepository<DanhMucChuDauTu, int>>();
        DanhMucChucVu = serviceProvider.GetRequiredService<IRepository<DanhMucChucVu, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _logger = logger;
        _unitOfWork = KhoKhanVuongMac.UnitOfWork;
    }

    public async Task Handle(KhoKhanVuongMacInsertOrUpdateCommand request, CancellationToken cancellationToken = default) {
        try {
            ManagedException.ThrowIf( !DuAn.GetQueryableSet().Any(e => e.Id == request.Dto.DuAnId),
                "Không tồn tại dự án");

            // Phân quyền: Owner/LanhDao/KHTC/PhongBanChinh/PhongBanPhoiHop-In-Scope
            await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = KhoKhanVuongMac.GetQueryableSet().Any(o => o.Id == request.Dto.Id);
                if (isExist) {
                    await KhoKhanVuongMac.UpdateAsync(request.Dto, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                } else {
                    //Thêm dự án trước
                    await KhoKhanVuongMac.AddAsync(request.Dto, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                //Cập nhật quy trình
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}