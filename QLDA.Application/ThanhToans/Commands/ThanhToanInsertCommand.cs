using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Providers;
using QLDA.Application.ThanhToans.DTOs;

namespace QLDA.Application.ThanhToans.Commands;

public record ThanhToanInsertCommand(ThanhToanInsertDto Dto) : IRequest<ThanhToan>;

internal class ThanhToanInsertCommandHandler : IRequestHandler<ThanhToanInsertCommand, ThanhToan> {
    private readonly IRepository<ThanhToan, Guid> ThanhToan;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<NghiemThu, Guid> NghiemThu;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProvider _userProvider;
    private readonly IAppSettingsProvider _settings;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<ThanhToanInsertCommandHandler>();

    public ThanhToanInsertCommandHandler(IServiceProvider serviceProvider) {
        ThanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        NghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _unitOfWork = ThanhToan.UnitOfWork;
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
    }

    public async Task<ThanhToan> Handle(ThanhToanInsertCommand request, CancellationToken cancellationToken = default) {
       ValidatePhongKeToanPermission();

        if (request.Dto.BuocId.HasValue) {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == request.Dto.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _userProvider, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }

        await ValidateAsync(request, cancellationToken);

        var entity = request.Dto.ToEntity();

        if (_unitOfWork.HasTransaction) {
            await InsertAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await InsertAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }


        return entity;

    }

    #region  Private helper methods

    private async Task ValidateAsync(ThanhToanInsertCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(!await DuAn.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.DuAnId, cancellationToken: cancellationToken),
           "Không tồn tại dự án");
        ManagedException.ThrowIf(!await NghiemThu.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.NghiemThuId, cancellationToken: cancellationToken),
           "Không tồn tại đợt nghiệm thu");
    }

    private async Task InsertAsync(ThanhToan entity, CancellationToken cancellationToken) {
        await ThanhToan.AddAsync(entity, cancellationToken);
    }

    #endregion

    private void ValidatePhongKeToanPermission() {
        ManagedException.ThrowIf(
            _userProvider.Info.PhongBanID != _settings.PhongKeToanID,
            "Chỉ phòng kế toán có quyền thực hiện thao tác này"
        );
    }
}