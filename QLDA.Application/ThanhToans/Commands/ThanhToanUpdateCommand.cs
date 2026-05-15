using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Providers;
using QLDA.Application.ThanhToans.DTOs;

namespace QLDA.Application.ThanhToans.Commands;

public record ThanhToanUpdateCommand(ThanhToanUpdateDto Dto) : IRequest<ThanhToan> {
    public List<Guid> NghiemThuIds { get; set; } = [];
}

internal class ThanhToanUpdateCommandHandler : IRequestHandler<ThanhToanUpdateCommand, ThanhToan> {
    private readonly IRepository<ThanhToan, Guid> ThanhToan;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProvider _userProvider;
    private readonly IAppSettingsProvider _settings;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<ThanhToanUpdateCommandHandler>();

    public ThanhToanUpdateCommandHandler(IServiceProvider serviceProvider) {
        ThanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _unitOfWork = ThanhToan.UnitOfWork;
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
    }

    public async Task<ThanhToan> Handle(ThanhToanUpdateCommand request, CancellationToken cancellationToken = default) {
        ValidatePhongKeToanPermission();
        await ValidateAsync(request, cancellationToken);

        var entity = await ThanhToan.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.Update(request.Dto);

        if (_unitOfWork.HasTransaction) {
            await UpdateAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        return entity;
    }
    #region  Private helper methods

    private async Task ValidateAsync(ThanhToanUpdateCommand request, CancellationToken cancellationToken) {
    }
    private async Task UpdateAsync(ThanhToan entity, CancellationToken cancellationToken) {
        await ThanhToan.UpdateAsync(entity, cancellationToken);
    }

    #endregion

    private void ValidatePhongKeToanPermission() {
        ManagedException.ThrowIf(
            _userProvider.Info.PhongBanID != _settings.PhongKeToanID,
            "Chỉ phòng kế toán có quyền thực hiện thao tác này"
        );
    }
}