using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;

namespace QLDA.Application.BanGiaoHoSos.Commands;

public record BanGiaoHoSoInsertCommand(BanGiaoHoSoInsertDto Dto) : IRequest<BanGiaoHoSo>;

internal class BanGiaoHoSoInsertCommandHandler : IRequestHandler<BanGiaoHoSoInsertCommand, BanGiaoHoSo> {
    private readonly IRepository<BanGiaoHoSo, Guid> _repository;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProvider _userProvider;

    public BanGiaoHoSoInsertCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _unitOfWork = _repository.UnitOfWork;
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    }

    public async Task<BanGiaoHoSo> Handle(BanGiaoHoSoInsertCommand request, CancellationToken cancellationToken = default) {
        var entity = request.Dto.ToEntity();
        if (request.Dto.BuocId.HasValue) {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == request.Dto.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _userProvider, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }
        // PhongBanChuTriId = phòng ban của người tạo (ưu tiên PhongBanID, fallback DonViID)
        entity.PhongBanChuTriId = _userProvider.Info.PhongBanID ?? _userProvider.Info.DonViID;
        // CreatedBy được tự động set bởi EF interceptor từ JWT token – không cần gán thủ công

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
