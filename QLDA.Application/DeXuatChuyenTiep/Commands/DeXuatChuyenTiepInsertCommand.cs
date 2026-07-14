using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatChuyenTieps.Commands;

public record DeXuatChuyenTiepInsertCommand(DeXuatChuyenTiep Dto) : IRequest<DeXuatChuyenTiep>;

internal class DeXuatChuyenTiepInsertCommandHandler : IRequestHandler<DeXuatChuyenTiepInsertCommand, DeXuatChuyenTiep>
{
    private readonly IRepository<DeXuatChuyenTiep, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatChuyenTiepInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DeXuatChuyenTiep> Handle(DeXuatChuyenTiepInsertCommand request, CancellationToken cancellationToken = default)
    {
        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);

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

        return entity!;
    }
}
