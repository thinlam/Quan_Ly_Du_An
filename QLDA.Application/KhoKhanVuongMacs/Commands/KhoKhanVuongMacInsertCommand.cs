using System.Data;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.KhoKhanVuongMacs.DTOs;

namespace QLDA.Application.KhoKhanVuongMacs.Commands;

public record KhoKhanVuongMacInsertCommand(KhoKhanVuongMacInsertDto Dto) : IRequest<BaoCaoKhoKhanVuongMac>;

internal class KhoKhanVuongMacInsertCommandHandler : IRequestHandler<KhoKhanVuongMacInsertCommand, BaoCaoKhoKhanVuongMac> {
    private readonly IRepository<BaoCaoKhoKhanVuongMac, Guid> KhoKhanVuongMac;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<DanhMucLoaiVanBan, int> DanhMucLoaiVanBan;
    private readonly IRepository<DanhMucChuDauTu, int> DanhMucChuDauTu;
    private readonly IRepository<DanhMucChucVu, int> DanhMucChucVu;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<KhoKhanVuongMacInsertCommandHandler> _logger;

    public KhoKhanVuongMacInsertCommandHandler(IServiceProvider serviceProvider,
        ILogger<KhoKhanVuongMacInsertCommandHandler> logger) {
        KhoKhanVuongMac = serviceProvider.GetRequiredService<IRepository<BaoCaoKhoKhanVuongMac, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        DanhMucLoaiVanBan = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiVanBan, int>>();
        DanhMucChuDauTu = serviceProvider.GetRequiredService<IRepository<DanhMucChuDauTu, int>>();
        DanhMucChucVu = serviceProvider.GetRequiredService<IRepository<DanhMucChucVu, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _logger = logger;
        _unitOfWork = KhoKhanVuongMac.UnitOfWork;
    }

    public async Task<BaoCaoKhoKhanVuongMac> Handle(KhoKhanVuongMacInsertCommand request, CancellationToken cancellationToken = default) {
        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Dto.DuAnId),
                "Không tồn tại dự án");

            // Phân quyền: Owner/LanhDao/KHTC/PhongBanChinh/PhongBanPhoiHop-In-Scope
            await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);

            var entity = request.Dto.ToEntity();

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await KhoKhanVuongMac.AddAsync(entity, cancellationToken);
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