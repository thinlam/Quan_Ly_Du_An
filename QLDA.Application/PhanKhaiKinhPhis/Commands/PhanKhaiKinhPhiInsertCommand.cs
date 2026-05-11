using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.PhanKhaiKinhPhis.Commands;

public record PhanKhaiKinhPhiInsertCommand(PhanKhaiKinhPhiInsertDto Dto) : IRequest<PhanKhaiKinhPhi>;

internal class PhanKhaiKinhPhiInsertCommandHandler : IRequestHandler<PhanKhaiKinhPhiInsertCommand, PhanKhaiKinhPhi> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public PhanKhaiKinhPhiInsertCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<PhanKhaiKinhPhi> Handle(PhanKhaiKinhPhiInsertCommand request, CancellationToken cancellationToken = default) {
        // Auto-assign Dự thảo status
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.PhanKhaiKinhPhi, cancellationToken);

        var entity = new PhanKhaiKinhPhi {
            DuAnId = request.Dto.DuAnId,
            SoToTrinh = request.Dto.SoToTrinh,
            NgayToTrinh = request.Dto.NgayToTrinh,
            NguonVonId = request.Dto.NguonVonId,
            KinhPhiDeXuat = request.Dto.KinhPhiDeXuat,
            KinhPhiPhanKhai = request.Dto.KinhPhiPhanKhai,
            TrangThaiId = trangThaiDuThao?.Id,
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
