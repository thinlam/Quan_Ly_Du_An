using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.Commands;

public record DeXuatNhuCauKinhPhiInsertCommand(DeXuatNhuCauKinhPhi Dto) : IRequest<DeXuatNhuCauKinhPhi>;

internal class DeXuatNhuCauKinhPhiInsertCommandHandler : IRequestHandler<DeXuatNhuCauKinhPhiInsertCommand, DeXuatNhuCauKinhPhi>
{
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatNhuCauKinhPhiInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DeXuatNhuCauKinhPhi> Handle(DeXuatNhuCauKinhPhiInsertCommand request, CancellationToken cancellationToken = default)
    {
        // Auto-assign Dự thảo status
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = new DeXuatNhuCauKinhPhi
        {
            DuAnId = request.Dto.DuAnId,
            BuocId = request.Dto.BuocId,
            KinhPhiDeXuat = request.Dto.KinhPhiDeXuat,
            DonViDeXuatId = request.Dto.DonViDeXuatId,
            SoPhieuChuyen = request.Dto.SoPhieuChuyen,
            NgayPhieuChuyen = request.Dto.NgayPhieuChuyen,
            TrichYeu = request.Dto.TrichYeu,
            TrangThaiId = trangThaiDuThao?.Id,
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
