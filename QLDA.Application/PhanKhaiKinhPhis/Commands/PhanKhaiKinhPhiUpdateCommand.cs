using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.PhanKhaiKinhPhis.Commands;

public record PhanKhaiKinhPhiUpdateCommand(PhanKhaiKinhPhiUpdateDto Dto) : IRequest<PhanKhaiKinhPhi>;

internal class PhanKhaiKinhPhiUpdateCommandHandler : IRequestHandler<PhanKhaiKinhPhiUpdateCommand, PhanKhaiKinhPhi> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public PhanKhaiKinhPhiUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<PhanKhaiKinhPhi> Handle(PhanKhaiKinhPhiUpdateCommand request, CancellationToken cancellationToken = default) {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.PhanKhaiKinhPhi.DuThao && s.Loai == PheDuyetEntityNames.PhanKhaiKinhPhi, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy phân khai kinh phí");

        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThai?.Ma != "LEG") {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là Dự thảo");
        }

        entity.SoToTrinh = request.Dto.SoToTrinh;
        entity.NgayToTrinh = request.Dto.NgayToTrinh;
        entity.TrichYeu = request.Dto.TrichYeu;
        entity.NguonVonId = request.Dto.NguonVonId;
        entity.KinhPhiDeXuat = request.Dto.KinhPhiDeXuat;
        entity.KinhPhiPhanKhai = request.Dto.KinhPhiPhanKhai;
        entity.ThuyetMinh = request.Dto.ThuyetMinh;
        entity.DuAnId = request.Dto.DuAnId;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
