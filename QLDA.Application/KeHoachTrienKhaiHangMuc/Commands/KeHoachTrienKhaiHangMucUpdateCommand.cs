using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KeHoachTrienKhaiHangMucMappings;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Constants;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Commands;

public record KeHoachTrienKhaiHangMucUpdateCommand(KeHoachTrienKhaiHangMuc Dto) : IRequest<KeHoachTrienKhaiHangMuc>;

internal class KeHoachTrienKhaiHangMucUpdateCommandHandler : IRequestHandler<KeHoachTrienKhaiHangMucUpdateCommand, KeHoachTrienKhaiHangMuc> {
    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiHangMucUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<KeHoachTrienKhaiHangMuc> Handle(KeHoachTrienKhaiHangMucUpdateCommand request,
        CancellationToken cancellationToken = default) {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTraLai = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
                            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .Include(e => e.CanBoTrienKhais)
            .Include(e => e.TrangThai)
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Trạng thái không thể cập nhật!");
        }

        entity.DuAnId = request.Dto.DuAnId;
        entity.BuocId = request.Dto.BuocId;
        entity.So = request.Dto.So;
        entity.NgayToTrinh = request.Dto.NgayToTrinh;
        entity.TrichYeu = request.Dto.TrichYeu;
        entity.KinhPhi = request.Dto.KinhPhi;   
        entity.NgayBatDau = request.Dto.NgayBatDau; 
        entity.NgayKetThuc = request.Dto.NgayKetThuc;   
        entity.GiaiDoanId = request.Dto.GiaiDoanId; 
        entity.ThoiHan = request.Dto.ThoiHan;
        entity.CanBoChuTriId = request.Dto.CanBoChuTriId;
        entity.TenHangMuc = request.Dto.TenHangMuc;

      //  entity.SyncCanBoPhoiHop(request.Dto.CanBoTrienKhais);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
