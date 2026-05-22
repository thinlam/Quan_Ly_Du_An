using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DeXuatChuyenTieps.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatChuyenTieps.Commands;

public record DeXuatChuyenTiepUpdateCommand(DeXuatChuyenTiepInsertDto Dto) : IRequest<DeXuatChuyenTiep>;

internal class DeXuatChuyenTiepUpdateCommandHandler : IRequestHandler<DeXuatChuyenTiepUpdateCommand, DeXuatChuyenTiep>
{
    private readonly IRepository<DeXuatChuyenTiep, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatChuyenTiepUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DeXuatChuyenTiep> Handle(DeXuatChuyenTiepUpdateCommand request, CancellationToken cancellationToken = default)
    {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThai?.Ma != "LEG")
        {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là dự thảo");
        }


        entity.SoLieuGiaiNgan = request.Dto.SoLieuGiaiNgan;
        entity.NhuCauKinhPhi = request.Dto.NhuCauKinhPhi;
        entity.KhoiLuongDuKien = request.Dto.KhoiLuongDuKien;
        entity.KhoiLuongThucTe = request.Dto.KhoiLuongThucTe;
        entity.UocGiaiNgan = request.Dto.UocGiaiNgan;
        entity.DuAnId = request.Dto.DuAnId;
        entity.BuocId = request.Dto.BuocId;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}

