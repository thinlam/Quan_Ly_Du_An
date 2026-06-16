using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.HopDongs.DTOs;

namespace QLDA.Application.HopDongs.Commands;

public record HopDongInsertCommand(HopDongInsertDto Dto) : IRequest<HopDong>;

internal class HopDongInsertCommandHandler : IRequestHandler<HopDongInsertCommand, HopDong> {
    private readonly IRepository<HopDong, Guid> HopDong;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<GoiThau, Guid> GoiThau;
    private readonly IRepository<DanhMucLoaiHopDong, int> DanhMucLoaiHopDong;
    private readonly IRepository<KetQuaTrungThau, Guid> KetQuaTrungThau;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<HopDongInsertCommandHandler>();

    public HopDongInsertCommandHandler(IServiceProvider serviceProvider) {
        HopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        GoiThau = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucLoaiHopDong = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiHopDong, int>>();
        KetQuaTrungThau = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = HopDong.UnitOfWork;
    }

    public async Task<HopDong> Handle(HopDongInsertCommand request, CancellationToken cancellationToken = default) {

        await ValidateAsync(request, cancellationToken);

        // Check step authorization
        if (request.Dto.BuocId.HasValue) {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == request.Dto.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _userProvider, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }

        var entity = request.Dto.ToEntity();

        // Calculate expected end dates from KetQuaTrungThau if not provided
        await CalculateExpectedEndDatesAsync(entity, cancellationToken);

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

    private async Task ValidateAsync(HopDongInsertCommand request, CancellationToken cancellationToken = default) {
        ManagedException.ThrowIf(
            when: !await DuAn.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.DuAnId, cancellationToken),
            message: "Không tồn tại dự án");
        ManagedException.ThrowIf(
            when: !await GoiThau.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.GoiThauId, cancellationToken),
            message: "Không tồn tại gói thầu");

        ManagedException.ThrowIf(
            when: await HopDong.GetQueryableSet().AnyAsync(e => e.GoiThauId == request.Dto.GoiThauId, cancellationToken),
            message: "Gói thầu đã có hợp đồng"
        );
        ManagedException.ThrowIf(
            when: request.Dto.LoaiHopDongId > 0 && !await DanhMucLoaiHopDong.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.LoaiHopDongId, cancellationToken),
            message: "Không tồn tại loại hợp đồng này");
    }

    private async Task InsertAsync(HopDong entity, CancellationToken cancellationToken = default) {
        await HopDong.AddAsync(entity, cancellationToken);
    }
    #endregion
}