using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.Commands;

public record DeXuatNhuCauKinhPhiUpdateCommand(DeXuatNhuCauKinhPhiInsertDto Dto) : IRequest<DeXuatNhuCauKinhPhi>;

internal class DeXuatNhuCauKinhPhiUpdateCommandHandler : IRequestHandler<DeXuatNhuCauKinhPhiUpdateCommand, DeXuatNhuCauKinhPhi>
{
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatNhuCauKinhPhiUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DeXuatNhuCauKinhPhi> Handle(DeXuatNhuCauKinhPhiUpdateCommand request, CancellationToken cancellationToken = default)
    {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTraLai = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet().FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
        if (entity.TrangThaiId != trangThaiDuThao?.Id && trangThaiTraLai?.Id != entity.TrangThaiId)
        {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là dự thảo hoặc trả lại ");
        }
        entity.DonViDeXuatId = request.Dto.DonViDeXuatId;
        entity.SoPhieuChuyen = request.Dto.SoPhieuChuyen;
        entity.NgayPhieuChuyen = request.Dto.NgayPhieuChuyen;
        entity.TrichYeu = request.Dto.TrichYeu;
        entity.KinhPhiDeXuat = request.Dto.KinhPhiDeXuat;
        
        entity.DuAnId = request.Dto.DuAnId;
        entity.BuocId = request.Dto.BuocId;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity!;
    }
}

