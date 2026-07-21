using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.HopDongs.DTOs;

namespace QLDA.Application.HopDongs.Commands;

public record HopDongUpdateCommand(HopDongUpdateDto Dto) : IRequest<HopDong>;

internal class HopDongUpdateCommandHandler : IRequestHandler<HopDongUpdateCommand, HopDong> {
    private readonly IRepository<HopDong, Guid> HopDong;
    private readonly IRepository<GoiThau, Guid> GoiThau;
    private readonly IRepository<DanhMucLoaiHopDong, int> DanhMucLoaiHopDong;
    private readonly IRepository<KetQuaTrungThau, Guid> KetQuaTrungThau;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HopDongUpdateCommandHandler> _logger;

    public HopDongUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<HopDongUpdateCommandHandler> logger) {
        HopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        GoiThau = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        DanhMucLoaiHopDong = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiHopDong, int>>();
        KetQuaTrungThau = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _logger = logger;
        _unitOfWork = HopDong.UnitOfWork;
    }

    public async Task<HopDong> Handle(HopDongUpdateCommand request, CancellationToken cancellationToken = default) {
        await ValidateAsync(request, cancellationToken);

        var entity = await HopDong.GetOrderedSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Check step authorization
        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);
        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        entity.Update(request.Dto);

        // Calculate expected end dates from KetQuaTrungThau if not provided
        await CalculateExpectedEndDatesAsync(entity, cancellationToken);

        if (_unitOfWork.HasTransaction) {
            await UpdateAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }

        return entity!;
    }
    #region Private helper methods

    private async Task CalculateExpectedEndDatesAsync(HopDong entity, CancellationToken cancellationToken) {
        if (entity.NgayHieuLuc == null) return;

        var kqtv = await KetQuaTrungThau.GetQueryableSet()
            .FirstOrDefaultAsync(x => x.GoiThauId == entity.GoiThauId, cancellationToken);

        if (kqtv == null) return;

        if (!entity.NgayDuKienKetThucHopDong.HasValue && kqtv.SoNgayThucHienHopDong.HasValue) {
            entity.NgayDuKienKetThucHopDong = entity.NgayHieuLuc.Value.AddDays(kqtv.SoNgayThucHienHopDong.Value);
        }

        if (!entity.NgayDuKienKetThucGoiThau.HasValue && kqtv.SoNgayTrienKhai.HasValue) {
            entity.NgayDuKienKetThucGoiThau = entity.NgayHieuLuc.Value.AddDays(kqtv.SoNgayTrienKhai.Value);
        }
    }

    private async Task ValidateAsync(HopDongUpdateCommand request, CancellationToken cancellationToken = default) {
        ManagedException.ThrowIf(
            when: !await GoiThau.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.GoiThauId, cancellationToken),
            message: "Không tồn tại gói thầu"
        );
        ManagedException.ThrowIf(
            when: await HopDong.GetQueryableSet().AnyAsync(e => e.GoiThauId == request.Dto.GoiThauId && e.Id != request.Dto.Id, cancellationToken),
            message: "Gói thầu đã có hợp đồng"
        );
        ManagedException.ThrowIf(
            when: request.Dto.LoaiHopDongId > 0 && !await DanhMucLoaiHopDong.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.LoaiHopDongId, cancellationToken),
            message: "Không tồn tại loại hợp đồng này");
    }

    private async Task UpdateAsync(HopDong entity, CancellationToken cancellationToken = default) {
        await HopDong.UpdateAsync(entity, cancellationToken);
    }
    #endregion
}