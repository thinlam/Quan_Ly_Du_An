using System.Data;
using BuildingBlocks.CrossCutting.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.DuToanDauTus.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.DuToanDauTus.Commands;

public record DuToanDauTuUpdateCommand(DuToanDauTuDto Dto) : IRequest<DuToanDauTu>;

internal class DuToanDauTuUpdateCommandHandler : IRequestHandler<DuToanDauTuUpdateCommand, DuToanDauTu>
{
    private readonly IRepository<DuToanDauTu, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public DuToanDauTuUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<DuToanDauTu, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DuToanDauTu> Handle(DuToanDauTuUpdateCommand request, CancellationToken cancellationToken = default)
    {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao 
            && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTraLai = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
         .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);
        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
       
        if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Trạng thái không thể cập nhật!");
        }

        entity.SoToTrinh = request.Dto.SoToTrinh;
        entity.NgayTrinh = request.Dto.NgayTrinh;
        entity.TrichYeu = request.Dto.TrichYeu;
        entity.DuAnId = request.Dto.DuAnId;
        entity.BuocId = request.Dto.BuocId;
        entity.TongMucDauTu = request.Dto.TongMucDauTu;
        entity.TongDuToan = request.Dto.TongDuToan; 
        entity.Nam = request.Dto.Nam;   
        entity.NguonVonId   = request.Dto.NguonVonId;
        entity.PhuongAnThietKeId   = request.Dto.PhuongAnThietKeId;
        entity.NoiDungChiPhis   = request.Dto.NoiDungChiPhi;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}

