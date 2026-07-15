using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatChuTruongMois.Commands;

public record DeXuatChuTruongMoiInsertCommand(DeXuatChuTruongMoi Dto) : IRequest<DeXuatChuTruongMoi>;

internal class DeXuatChuTruongMoiInsertCommandHandler : IRequestHandler<DeXuatChuTruongMoiInsertCommand, DeXuatChuTruongMoi>
{
    private readonly IRepository<DeXuatChuTruongMoi, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DeXuatChuTruongMoiInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<DeXuatChuTruongMoi, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DeXuatChuTruongMoi> Handle(DeXuatChuTruongMoiInsertCommand request, CancellationToken cancellationToken = default)
    {
        // Auto-assign Dự thảo status
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var donViPhoiHopIds = request.Dto.DeXuatDonViXuLys?
            .Select(x => x.RightId)
            .ToList();

        var entity = request.Dto;
        entity.TrangThaiId = trangThaiDuThao?.Id;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        entity.SyncDonViPhoiHopIds(donViPhoiHopIds);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
