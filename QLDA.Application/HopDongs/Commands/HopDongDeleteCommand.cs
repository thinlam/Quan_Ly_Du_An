using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;

namespace QLDA.Application.HopDongs.Commands;

public record HopDongDeleteCommand(Guid Id) : IRequest<int>;

public record HopDongDeleteCommandHandler : IRequestHandler<HopDongDeleteCommand, int> {
    private readonly IRepository<HopDong, Guid> HopDong;
    private readonly IRepository<NghiemThu, Guid> NghiemThu;
    private readonly IRepository<TamUng, Guid> TamUng;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public HopDongDeleteCommandHandler(IServiceProvider serviceProvider) {
        HopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        NghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        TamUng = serviceProvider.GetRequiredService<IRepository<TamUng, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = HopDong.UnitOfWork;
    }

    public async Task<int> Handle(HopDongDeleteCommand request, CancellationToken cancellationToken) {
        await ValidateAsync(request, cancellationToken);
        var entity = await HopDong.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

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

        await RemoveAsync(entity, cancellationToken);
        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    #region  Private helper methods

    private async Task ValidateAsync(HopDongDeleteCommand request, CancellationToken cancellationToken) {
        var hasNghiemThu = await NghiemThu.GetQueryableSet().AnyAsync(e => e.HopDongId == request.Id && e.HopDong != null && !e.HopDong.IsDeleted, cancellationToken);
        var hasTamUng = await TamUng.GetQueryableSet().AnyAsync(e => e.HopDongId == request.Id && e.HopDong != null && !e.HopDong.IsDeleted, cancellationToken);
        var hasPLHD = await HopDong.GetQueryableSet().AnyAsync(x => x.Id == request.Id && x.PhuLucHopDongs!.Any(pl => !pl.IsDeleted), cancellationToken);
       
        if (hasPLHD)
            ManagedException.Throw("Đã có phụ lục hợp đồng. Không thể xóa");
        ManagedException.ThrowIf(
            when: hasNghiemThu,
            message: "Hợp đồng đã nghiệm thu. Không thể xoá!"
        );
        ManagedException.ThrowIf(
            when: hasTamUng,
            message: "Hợp đồng đã tạm ứng. Không thể xoá!"
      );
    }

    private async Task RemoveAsync(HopDong entity, CancellationToken cancellationToken) {

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);
    }

    #endregion
}