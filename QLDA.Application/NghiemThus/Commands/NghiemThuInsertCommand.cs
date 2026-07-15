using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.NghiemThus.DTOs;

namespace QLDA.Application.NghiemThus.Commands;

public record NghiemThuInsertCommand(NghiemThuInsertDto Dto) : IRequest<NghiemThu>;

internal class NghiemThuInsertCommandHandler : IRequestHandler<NghiemThuInsertCommand, NghiemThu> {
    private readonly IRepository<NghiemThu, Guid> NghiemThu;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<HopDong, Guid> HopDong;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<NghiemThuInsertCommandHandler>();

    public NghiemThuInsertCommandHandler(IServiceProvider serviceProvider) {
        NghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        HopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        _unitOfWork = NghiemThu.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<NghiemThu> Handle(NghiemThuInsertCommand request, CancellationToken cancellationToken = default) {
        await ValidateAsync(request, cancellationToken);

        // Authorization check on BuocId from DTO
        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);

        var entity = request.Dto.ToEntity();

        if (_unitOfWork.HasTransaction) {
            await Insert(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await Insert(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }

        return entity!;
    }
    #region  Private helper methods

    private async Task ValidateAsync(NghiemThuInsertCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(
            when: !await DuAn.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.DuAnId, cancellationToken),
            message: "Không tồn tại dự án"
        );

        ManagedException.ThrowIf(
            when: !await HopDong.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.HopDongId, cancellationToken),
            message: "Không tồn tại hợp đồng"
        );
        var query = NghiemThu.GetQueryableSet()
                   .Where(e => e.HopDongId == request.Dto.HopDongId);

        ManagedException.ThrowIf(
            when: request.Dto.SoBienBan.IsNotNullOrWhitespace() && await query.AnyAsync(e => e.SoBienBan!.ToLower() == request.Dto.SoBienBan!.ToLower(), cancellationToken),
            message: "Số biên bản đã tồn tại"
        );
        ManagedException.ThrowIf(
            when: await query.AnyAsync(e => e.Dot!.ToLower() == request.Dto.Dot!.ToLower(), cancellationToken),
            message: "Đợt nghiệm thu đã tồn tại"
        );

    }
    private async Task Insert(NghiemThu entity, CancellationToken cancellationToken) {
        await NghiemThu.AddAsync(entity, cancellationToken);
    }

    #endregion
}