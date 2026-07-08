using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DeXuatNhuCauKinhPhiNamMappings;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.Commands;

public record DeXuatNhuCauKinhPhiNamUpdateCommand(DeXuatNhuCauKinhPhiNamInsertDto Dto) : IRequest<DeXuatNhuCauKinhPhiNam>;

internal class DeXuatNhuCauKinhPhiNamUpdateCommandHandler : IRequestHandler<DeXuatNhuCauKinhPhiNamUpdateCommand, DeXuatNhuCauKinhPhiNam> {
    private readonly IRepository<DeXuatNhuCauKinhPhiNam, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatNhuCauKinhPhiNamUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhiNam, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DeXuatNhuCauKinhPhiNam> Handle(DeXuatNhuCauKinhPhiNamUpdateCommand request,
        CancellationToken cancellationToken = default) {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatNhuCauKinhPhiNam.DuThao
            && s.Loai == PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam, cancellationToken);
        var trangThaiTraLai = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatNhuCauKinhPhiNam.TraLai
            && s.Loai == PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .Include(e => e.DeXuats)
            .Include(e => e.TrangThai)
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Trạng thái không thể cập nhật.");
        }

        entity.GhiChu = request.Dto.GhiChu;
        entity.So = request.Dto.So;
        entity.NgayKeHoach = request.Dto.NgayKeHoach;
        entity.TrichYeu = request.Dto.TrichYeu;
        entity.TongKinhPhiDeXuat = request.Dto.TongKinhPhiDeXuat;
        entity.SyncDeXuatIds(request.Dto.DanhSachDeXuat);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
