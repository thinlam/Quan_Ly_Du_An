using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.Commands;

public record DeXuatNhuCauKinhPhiInsertCommand(DeXuatNhuCauKinhPhi Dto) : IRequest<DeXuatNhuCauKinhPhi>;

internal class DeXuatNhuCauKinhPhiInsertCommandHandler : IRequestHandler<DeXuatNhuCauKinhPhiInsertCommand, DeXuatNhuCauKinhPhi>
{
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatNhuCauKinhPhiInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DeXuatNhuCauKinhPhi> Handle(DeXuatNhuCauKinhPhiInsertCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Dto.BuocId.HasValue) {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == request.Dto.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _user, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }

        // Auto-assign Dự thảo status
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = new DeXuatNhuCauKinhPhi
        {
            DuAnId = request.Dto.DuAnId,
            BuocId = request.Dto.BuocId,
            KinhPhiDeXuat = request.Dto.KinhPhiDeXuat,
            DonViDeXuatId = request.Dto.DonViDeXuatId,
            SoPhieuChuyen = request.Dto.SoPhieuChuyen,
            NgayPhieuChuyen = request.Dto.NgayPhieuChuyen,
            TrichYeu = request.Dto.TrichYeu,
            TrangThaiId = trangThaiDuThao?.Id,
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
