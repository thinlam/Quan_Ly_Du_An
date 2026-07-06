using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DeXuatChuTruongMois;
using QLDA.Application.DeXuatChuTruongMois.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatChuTruongMois.Commands;

public record DeXuatChuTruongMoiUpdateCommand(DeXuatChuTruongMoiInsertDto Dto) : IRequest<DeXuatChuTruongMoi>;

internal class DeXuatChuTruongMoiUpdateCommandHandler : IRequestHandler<DeXuatChuTruongMoiUpdateCommand, DeXuatChuTruongMoi>
{
    private readonly IRepository<DeXuatChuTruongMoi, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatChuTruongMoiUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<DeXuatChuTruongMoi, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DeXuatChuTruongMoi> Handle(DeXuatChuTruongMoiUpdateCommand request, CancellationToken cancellationToken = default)
    {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTra = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .Include(e => e.DeXuatDonViXuLys)
            .Include(e => e.TrangThai)
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);// Không tìm thấy dữ liệu
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
        if ( entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTra?.Id)
        {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là dự thảo");
        }

        entity.NamDeXuat = request.Dto.NamDeXuat;
        entity.TongMucDauTu = request.Dto.TongMucDauTu;
        entity.TomTatNoiDung = request.Dto.TomTatNoiDung;
        entity.HinhThucDauTuId = request.Dto.HinhThucDauTuId;
        entity.NguoiXuLyChinhId = request.Dto.NguoiXuLyChinhId;
        entity.NgayBatDauDuKien = request.Dto.NgayBatDauDuKien;
        entity.DonViPhuTrachChinhId = request.Dto.DonViPhuTrachChinhId;
        entity.LanhDaoPhuTrachId = request.Dto.LanhDaoPhuTrachId;

        entity.DuAnId = request.Dto.DuAnId;
        entity.BuocId = request.Dto.BuocId;

        entity.SyncDonViPhoiHopIds(request.Dto.DonViPhoiHopIds);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}

