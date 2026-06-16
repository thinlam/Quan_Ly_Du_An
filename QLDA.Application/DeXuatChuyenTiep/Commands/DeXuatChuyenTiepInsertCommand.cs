using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.DeXuatChuyenTieps.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.DeXuatChuyenTieps.Commands;

public record DeXuatChuyenTiepInsertCommand(DeXuatChuyenTiep Dto) : IRequest<DeXuatChuyenTiep>;

internal class DeXuatChuyenTiepInsertCommandHandler : IRequestHandler<DeXuatChuyenTiepInsertCommand, DeXuatChuyenTiep>
{
    private readonly IRepository<DeXuatChuyenTiep, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatChuyenTiepInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DeXuatChuyenTiep> Handle(DeXuatChuyenTiepInsertCommand request, CancellationToken cancellationToken = default)
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

        var entity = new DeXuatChuyenTiep
        {
            DuAnId = request.Dto.DuAnId,
            BuocId = request.Dto.BuocId,
            SoLieuGiaiNgan = request.Dto.SoLieuGiaiNgan,
            NhuCauKinhPhi = request.Dto.NhuCauKinhPhi,
            KhoiLuongDuKien = request.Dto.KhoiLuongDuKien,
            KhoiLuongThucTe = request.Dto.KhoiLuongThucTe,
            UocGiaiNgan = request.Dto.UocGiaiNgan,
            NamDeXuat = request.Dto.NamDeXuat,
            TrangThaiId = trangThaiDuThao?.Id,
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
