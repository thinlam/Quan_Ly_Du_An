using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DeXuatNhuCauKinhPhiNamMappings;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.Commands;

public record DeXuatNhuCauKinhPhiNamInsertCommand(DeXuatNhuCauKinhPhiNamInsertDto Dto) : IRequest<DeXuatNhuCauKinhPhiNam>;

internal class DeXuatNhuCauKinhPhiNamInsertCommandHandler : IRequestHandler<DeXuatNhuCauKinhPhiNamInsertCommand, DeXuatNhuCauKinhPhiNam> {
    private readonly IRepository<DeXuatNhuCauKinhPhiNam, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatNhuCauKinhPhiNamInsertCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhiNam, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DeXuatNhuCauKinhPhiNam> Handle(DeXuatNhuCauKinhPhiNamInsertCommand request,
        CancellationToken cancellationToken = default) {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = new DeXuatNhuCauKinhPhiNam {
            So = request.Dto.So,
            NgayKeHoach = request.Dto.NgayKeHoach,
            TrichYeu = request.Dto.TrichYeu,
            GhiChu = request.Dto.GhiChu,
            TongKinhPhiDeXuat = request.Dto.TongKinhPhiDeXuat,
            TrangThaiId = trangThaiDuThao?.Id,
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        entity.SyncDeXuatIds(request.Dto.DanhSachDeXuat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return entity;
    }
}
