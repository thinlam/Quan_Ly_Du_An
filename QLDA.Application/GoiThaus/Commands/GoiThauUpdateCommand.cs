using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.GoiThaus.DTOs;

namespace QLDA.Application.GoiThaus.Commands;

public record GoiThauUpdateCommand(GoiThauUpdateDto Dto) : IRequest<GoiThau>;

internal class GoiThauUpdateCommandHandler : IRequestHandler<GoiThauUpdateCommand, GoiThau> {
    private readonly IRepository<GoiThau, Guid> GoiThau;
    private readonly IRepository<DanhMucLoaiHopDong, int> DanhMucLoaiHopDong;
    private readonly IRepository<DanhMucHinhThucLuaChonNhaThau, int> DanhMucHinhThucLuaChonNhaThau;
    private readonly IRepository<DanhMucPhuongThucLuaChonNhaThau, int> DanhMucPhuongThucLuaChonNhaThau;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<GoiThauUpdateCommandHandler>();

    public GoiThauUpdateCommandHandler(IServiceProvider serviceProvider) {
        GoiThau = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        DanhMucLoaiHopDong = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiHopDong, int>>();
        DanhMucHinhThucLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<DanhMucHinhThucLuaChonNhaThau, int>>();
        DanhMucPhuongThucLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<DanhMucPhuongThucLuaChonNhaThau, int>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = GoiThau.UnitOfWork;
    }

    public async Task<GoiThau> Handle(GoiThauUpdateCommand request, CancellationToken cancellationToken = default) {
        await ValidateAsync(request, cancellationToken);

        var entity = await GoiThau.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Check step authorization
        if (entity.BuocId.HasValue) {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == entity.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _userProvider, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }

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

    private async Task ValidateAsync(GoiThauUpdateCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(
            request.Dto.LoaiHopDongId > 0 && !await DanhMucLoaiHopDong.GetQueryableSet()
                .AnyAsync(e => e.Id == request.Dto.LoaiHopDongId, cancellationToken),
            "Không tồn tại loại hợp đồng này");
        ManagedException.ThrowIf(
            request.Dto.HinhThucLuaChonNhaThauId > 0 && !await DanhMucHinhThucLuaChonNhaThau.GetQueryableSet()
                .AnyAsync(e => e.Id == request.Dto.HinhThucLuaChonNhaThauId, cancellationToken),
            "Không tồn tại hình thức chọn gói thầu này");
        ManagedException.ThrowIf(
            request.Dto.PhuongThucLuaChonNhaThauId > 0 && !await DanhMucPhuongThucLuaChonNhaThau.GetQueryableSet()
                .AnyAsync(e => e.Id == request.Dto.PhuongThucLuaChonNhaThauId, cancellationToken),
            "Không tồn tại phương thức chọn gói thầu này");
    }
    private async Task UpdateAsync(GoiThau entity, CancellationToken cancellationToken) {
        await GoiThau.UpdateAsync(entity, cancellationToken);
    }

    #endregion
}